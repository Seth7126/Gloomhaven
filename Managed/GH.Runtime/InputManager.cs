#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.Extensions;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using Utilities;

public class InputManager : Singleton<InputManager>
{
	public static string FreeCursorInputRequest = "CursorMovement";

	private const string VirtualMouseName = "ConsoleVirtualMouse";

	private InControlInputModule m_CurrentInputModule;

	[SerializeField]
	private ControlBindings DefaultBindingsObject;

	[SerializeField]
	private List<UnityEngine.InputSystem.Key> excludedKeys;

	private Dictionary<KeyAction, KeyCode> defaultKeys;

	private Dictionary<KeyAction, KeyCode> keybinds;

	private static Dictionary<EKeyActionTag, IEnumerable<KeyAction>> actionTagToKeyActions = new Dictionary<EKeyActionTag, IEnumerable<KeyAction>>();

	private static bool isUseGamepadInPc;

	private List<UnityEngine.InputSystem.Key> allowedKeycodes;

	private bool initialized;

	private UnityEngine.InputSystem.Mouse _virtualMouse;

	private GamepadButtonsComboController _buttonsComboController;

	private PlayerActionControls _playerActionControls;

	private bool _mouseDisabled;

	private CursorLockMode _originalLockState;

	public Action<UINavigationDirection> PreUnityMoveSelectionEvent;

	public static Action<UIActionBaseEventData> MoveSelectionEvent;

	public static Action<ControllerType> OnControllerTypeChangedEvent;

	public static Action OnKeybindingsInitialized;

	public static Action DeviceChangedEvent;

	private static Dictionary<KeyAction, HashSet<object>> disableInputRequests = new Dictionary<KeyAction, HashSet<object>>();

	private static readonly KeyAction[] KeyActions = ((KeyAction[])Enum.GetValues(typeof(KeyAction))).Where((KeyAction it) => it != KeyAction.None).ToArray();

	private const string TAG_FORMAT = "<color=#DCFF00>[INPUT MANAGER]</color> {0}";

	private bool _switchLeftStickAndDPad = true;

	private UiNavigationBlocker _navBlocker = new UiNavigationBlocker("AppFocusBlocker");

	public GHControls PlayerControl { get; private set; }

	public ControllerType CurrentControllerType { get; private set; }

	public bool IsPlayerSelectInputDevice { get; set; }

	public PlayerActionControls PlayerActionControls => _playerActionControls;

	public GamepadButtonsComboController ButtonsComboController => _buttonsComboController;

	public UnityEngine.InputSystem.Mouse VirtualMouse => _virtualMouse;

	public bool HasCustomKeybinds
	{
		get
		{
			if (keybinds != null)
			{
				return keybinds.Any((KeyValuePair<KeyAction, KeyCode> it) => defaultKeys.ContainsKey(it.Key) && it.Value != defaultKeys[it.Key]);
			}
			return false;
		}
	}

	public bool LastInputWasMouseOrKeyboard
	{
		get
		{
			if (PlatformLayer.Instance.IsConsole)
			{
				return false;
			}
			if (PlayerControl.LastInputType != BindingSourceType.KeyBindingSource)
			{
				return PlayerControl.LastInputType == BindingSourceType.MouseBindingSource;
			}
			return true;
		}
	}

	public static bool GamePadInUse
	{
		get
		{
			if (PlatformLayer.Instance.IsConsole)
			{
				return true;
			}
			return isUseGamepadInPc;
		}
	}

	public static Vector2 CursorPosition
	{
		get
		{
			if (GamePadInUse)
			{
				if (Singleton<UINavigation>.Instance.NavigationManager.CurrentlySelectedElement != null)
				{
					Transform transform = Singleton<UINavigation>.Instance.NavigationManager.CurrentlySelectedElement.GameObject.transform;
					Camera camera = null;
					Camera[] allCameras = Camera.allCameras;
					foreach (Camera camera2 in allCameras)
					{
						if (camera2.CompareTag("UICamera"))
						{
							camera = camera2;
						}
					}
					if (camera != null)
					{
						return camera.WorldToScreenPoint(transform.position);
					}
					return new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
				}
				return new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
			}
			return InputSystemUtilities.GetMousePosition();
		}
	}

	public static bool IsControllerModeDisabled { get; set; }

	public event BasicEventHandler onInitialized;

	protected override void Awake()
	{
		base.Awake();
		if (GamePadInUse)
		{
			CreateVirtualMouse();
		}
		_buttonsComboController = new GamepadButtonsComboController();
		SetInstance(this);
		allowedKeycodes = (from x in Enum.GetValues(typeof(UnityEngine.InputSystem.Key)).OfType<UnityEngine.InputSystem.Key>()
			where !excludedKeys.Contains(x) && x != UnityEngine.InputSystem.Key.None
			select x).ToList();
		defaultKeys = new Dictionary<KeyAction, KeyCode>();
		OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Combine(OnControllerTypeChangedEvent, new Action<ControllerType>(ControllerTypeChanged));
		InitialiseInControlGHControls();
		foreach (KeyValuePair<KeyAction, HashSet<object>> item in disableInputRequests.Where((KeyValuePair<KeyAction, HashSet<object>> it) => it.Value.Count > 0))
		{
			PlayerControl.GetPlayerActionForKeyAction(item.Key).Enabled = false;
		}
		OnKeybindingsInitialized?.Invoke();
		initialized = true;
		this.onInitialized?.Invoke();
		this.onInitialized = null;
		PollLastInputForControllerChanges();
		InControl.InputManager.OnDeviceAttached += OnDeviceAttached;
		InControl.InputManager.OnActiveDeviceChanged += OnDeviceAttached;
	}

	public IEnumerator InitKeyBindings()
	{
		while (SaveData.Instance.Global == null)
		{
			yield return null;
		}
		InitializeKeybindings();
		if (IsPCVersion())
		{
			InitializeGamepadSettings();
		}
	}

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		EnableAllMouses();
		_playerActionControls = null;
		OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(OnControllerTypeChangedEvent, new Action<ControllerType>(ControllerTypeChanged));
		if (m_CurrentInputModule != null)
		{
			m_CurrentInputModule.InputVectorEvent -= OnVectorEvent;
			m_CurrentInputModule.PreUnityInputVectorEvent -= OnPreUnityVectorEvent;
		}
		InControl.InputManager.OnDeviceAttached -= OnDeviceAttached;
		InControl.InputManager.OnActiveDeviceChanged -= OnDeviceAttached;
		PreUnityMoveSelectionEvent = null;
		MoveSelectionEvent = null;
		OnControllerTypeChangedEvent = null;
		OnKeybindingsInitialized = null;
		disableInputRequests.Clear();
		base.OnDestroy();
	}

	private void Update()
	{
		if (GamePadInUse)
		{
			_buttonsComboController.Update();
		}
		_playerActionControls?.UpdateHandledComboControls();
		if (PlayerControl != null)
		{
			PlayerControl.CheckClicks();
			PlayerControl.ResetHandledActions();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (DebugMenu.disableInputOnAppUnfocus)
		{
			if (hasFocus)
			{
				Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navBlocker);
				EnableInput();
			}
			else
			{
				Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(_navBlocker);
				DisableInput();
			}
		}
	}

	public static IEnumerable<KeyAction> GetKeyActionsAssociated(EKeyActionTag actionTag)
	{
		if (actionTag == EKeyActionTag.All)
		{
			return KeyActions;
		}
		if (!actionTagToKeyActions.TryGetValue(actionTag, out var value))
		{
			value = KeyActions.Where((KeyAction it) => it.IsAssociated(actionTag)).ToArray();
			actionTagToKeyActions.Add(actionTag, value);
		}
		return value;
	}

	public void CreateVirtualMouse()
	{
		Debug.Log("[InputManager] Creating virtual mouse");
		if (_virtualMouse == null)
		{
			_virtualMouse = InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>("ConsoleVirtualMouse");
		}
		InputUser.PerformPairingWithDevice(_virtualMouse);
		_virtualMouse.MakeCurrent();
		Vector2 vector = new Vector2(Screen.width / 2, Screen.height / 2);
		_virtualMouse.WarpCursorPosition(vector);
		InputState.Change(_virtualMouse.position, vector);
		if (PlatformLayer.Instance.IsConsole)
		{
			_originalLockState = Cursor.lockState;
			Cursor.lockState = CursorLockMode.Locked;
		}
	}

	public void DisableAllMouses()
	{
		if (_mouseDisabled)
		{
			return;
		}
		_mouseDisabled = true;
		foreach (UnityEngine.InputSystem.InputDevice device2 in InputSystem.devices)
		{
			if (device2 is UnityEngine.InputSystem.Mouse device)
			{
				InputSystem.DisableDevice(device);
			}
		}
		_originalLockState = Cursor.lockState;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void EnableAllMouses()
	{
		if (!_mouseDisabled)
		{
			return;
		}
		_mouseDisabled = false;
		foreach (UnityEngine.InputSystem.InputDevice device2 in InputSystem.devices)
		{
			if (device2 is UnityEngine.InputSystem.Mouse device)
			{
				InputSystem.EnableDevice(device);
			}
		}
		Cursor.lockState = _originalLockState;
	}

	public bool IsPCAndGamepadVersion()
	{
		if (HasGamepad())
		{
			return IsPCVersion();
		}
		return false;
	}

	public bool HasGamepad()
	{
		bool result = false;
		for (int i = 0; i < InputSystem.devices.Count; i++)
		{
			InputSystem.devices[i].GetType();
			if (InputSystem.devices[i] is Gamepad)
			{
				result = true;
			}
		}
		return result;
	}

	public bool IsPCVersion()
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < InputSystem.devices.Count; i++)
		{
			InputSystem.devices[i].GetType();
			if (InputSystem.devices[i] is UnityEngine.InputSystem.Mouse)
			{
				flag = true;
			}
			else if (InputSystem.devices[i] is Keyboard)
			{
				flag2 = true;
			}
		}
		if (flag2 && flag)
		{
			return !PlatformLayer.Instance.IsConsole;
		}
		return false;
	}

	public void SetGamepadInputDevice(bool isUseGamepad)
	{
		bool num = isUseGamepadInPc != isUseGamepad;
		if (isUseGamepad && _virtualMouse == null)
		{
			CreateVirtualMouse();
		}
		if (m_CurrentInputModule != null)
		{
			m_CurrentInputModule.allowMouseInput = true;
		}
		if (num)
		{
			if (isUseGamepad)
			{
				AssignGamepadBindingsToPlayerActions(withRemoveCurrentBindings: true);
			}
			else
			{
				AssignMouseKeyboardBindingsToPlayerActions(withRemoveCurrentBindings: true);
			}
		}
		PollLastInputForControllerChanges();
		DeviceChangedEvent?.Invoke();
	}

	private void InitializeGamepadSettings()
	{
		InControl.InputManager.InvertYAxis = SaveData.Instance.Global.InvertYAxis;
		InControl.InputManager.Sensitivity = SaveData.Instance.Global.StickSensitivity;
		InControl.InputManager.HasVibration = SaveData.Instance.Global.HasVibration;
	}

	public void SetInvertYAxis(bool invertYAxis)
	{
		InControl.InputManager.InvertYAxis = invertYAxis;
		SaveData.Instance.Global.InvertYAxis = invertYAxis;
		SaveData.Instance.SaveGlobalData();
	}

	public void SetVibration(bool hasVibration)
	{
		InControl.InputManager.HasVibration = hasVibration;
		SaveData.Instance.Global.HasVibration = hasVibration;
		SaveData.Instance.SaveGlobalData();
	}

	public void SetStickSensitivity(float sensitivity)
	{
		float stickSensitivity = (InControl.InputManager.Sensitivity = sensitivity / 100f);
		SaveData.Instance.Global.StickSensitivity = stickSensitivity;
		SaveData.Instance.SaveGlobalData();
	}

	public static void RequestDisableInput(object request, EKeyActionTag actionTag = EKeyActionTag.All, params KeyAction[] excludedKeyActions)
	{
		if (actionTag != EKeyActionTag.None)
		{
			IEnumerable<KeyAction> keyActionsAssociated = GetKeyActionsAssociated(actionTag);
			RequestDisableInput(request, (excludedKeyActions.IsNullOrEmpty() ? keyActionsAssociated : keyActionsAssociated.Except(excludedKeyActions)).ToArray());
		}
	}

	public static void RequestDisableInput(object request, KeyAction keyAction)
	{
		RequestDisableInputKeyActions(request, keyAction);
	}

	public static void RequestDisableInput(object request, KeyAction[] keyActions)
	{
		RequestDisableInputKeyActions(request, keyActions);
	}

	private static void RequestDisableInputKeyActions(object request, params KeyAction[] keyActions)
	{
		if (keyActions.IsNullOrEmpty())
		{
			return;
		}
		foreach (KeyAction keyAction in keyActions)
		{
			if (!disableInputRequests.ContainsKey(keyAction))
			{
				disableInputRequests[keyAction] = new HashSet<object> { request };
			}
			else
			{
				disableInputRequests[keyAction].Add(request);
			}
			if (Singleton<InputManager>.Instance?.PlayerControl != null)
			{
				Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction).Enabled = false;
			}
		}
	}

	public static void RequestEnableInput(object request, EKeyActionTag actionTag = EKeyActionTag.All, params KeyAction[] excludedKeyActions)
	{
		if (actionTag != EKeyActionTag.None)
		{
			IEnumerable<KeyAction> keyActionsAssociated = GetKeyActionsAssociated(actionTag);
			RequestEnableInput(request, (excludedKeyActions.IsNullOrEmpty() ? keyActionsAssociated : keyActionsAssociated.Except(excludedKeyActions)).ToArray());
		}
	}

	public static void RequestEnableInput(object request, KeyAction[] keyActions)
	{
		RequestEnableInputKeyActions(request, keyActions);
	}

	public static void RequestEnableInput(object request, KeyAction keyAction)
	{
		RequestEnableInputKeyActions(request, keyAction);
	}

	private static void RequestEnableInputKeyActions(object request, params KeyAction[] keyActions)
	{
		if (keyActions.IsNullOrEmpty())
		{
			return;
		}
		foreach (KeyAction keyAction in keyActions)
		{
			if (!disableInputRequests.ContainsKey(keyAction))
			{
				continue;
			}
			bool flag = disableInputRequests[keyAction].Remove(request);
			bool flag2 = true;
			foreach (object item in disableInputRequests[keyAction])
			{
				flag2 = InputRequestIsNull(item);
				if (!flag2)
				{
					break;
				}
			}
			if (flag && flag2 && Singleton<InputManager>.Instance != null && Singleton<InputManager>.Instance.PlayerControl != null)
			{
				Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction).Enabled = true;
			}
		}
	}

	public static void EnableInput(EKeyActionTag actionTag)
	{
		if (actionTag == EKeyActionTag.None)
		{
			return;
		}
		KeyAction[] keyActions = KeyActions;
		foreach (KeyAction keyAction in keyActions)
		{
			if (keyAction.IsAssociated(actionTag))
			{
				EnableInput(keyAction);
			}
		}
	}

	public static void EnableInput(KeyAction keyAction)
	{
		if (disableInputRequests.ContainsKey(keyAction) && !disableInputRequests[keyAction].IsNullOrEmpty())
		{
			disableInputRequests[keyAction].Clear();
			if (Singleton<InputManager>.Instance?.PlayerControl != null)
			{
				Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction).Enabled = true;
			}
		}
	}

	public static void EnableInput()
	{
		if (disableInputRequests.IsNullOrEmpty())
		{
			return;
		}
		Debug.LogFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", "Enable all key actions");
		if (Singleton<InputManager>.Instance != null && Singleton<InputManager>.Instance.PlayerControl != null)
		{
			foreach (KeyAction key in disableInputRequests.Keys)
			{
				Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(key).Enabled = true;
			}
		}
		disableInputRequests.Clear();
	}

	public void DisableInput()
	{
		RequestDisableInput(typeof(InputManager), EKeyActionTag.All);
	}

	private static bool InputRequestIsNull(object inputRequest)
	{
		if (inputRequest is MonoBehaviour monoBehaviour)
		{
			if (monoBehaviour != null)
			{
				return false;
			}
		}
		else if (inputRequest != null)
		{
			return false;
		}
		return true;
	}

	private void InitializeKeybindings()
	{
		Debug.LogFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", "Fetching Keybindings from save data");
		if (SaveData.Instance.Global.Keybinds == null)
		{
			keybinds = new Dictionary<KeyAction, KeyCode>();
		}
		else
		{
			keybinds = SaveData.Instance.Global.KeyBindings.Where((GlobalData.KeyBinding it) => UnityKeyboardProvider.OldToNewCodes.ContainsKey(it.Code) && !excludedKeys.Contains(UnityKeyboardProvider.OldToNewCodes[it.Code])).ToDictionary((GlobalData.KeyBinding it) => it.Action, (GlobalData.KeyBinding it) => it.Code);
		}
		KeyAction[] keyActions = KeyActions;
		for (int num = 0; num < keyActions.Length; num++)
		{
			KeyAction value = keyActions[num];
			try
			{
				defaultKeys[value] = GetKeyCodeForBinding(DefaultBindingsObject.MouseKeyboardDefaultBindings.GetBindingForKeyAction(value));
			}
			catch (Exception)
			{
				Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", "Missing binding for " + value);
				throw;
			}
			if (!keybinds.ContainsKey(value))
			{
				if (!keybinds.ContainsValue(defaultKeys[value]) || !defaultKeys.Any((KeyValuePair<KeyAction, KeyCode> it) => it.Key != value && it.Value == defaultKeys[value] && (string.IsNullOrEmpty(it.Key.GetGroupId()) || it.Key.GetGroupId() == value.GetGroupId())))
				{
					keybinds[value] = defaultKeys[value];
				}
				else
				{
					keybinds.Remove(value);
				}
			}
		}
		AssignMouseKeyboardBindingsToPlayerActions();
	}

	private KeyCode GetKeyCodeForBinding(BindingDetails binding)
	{
		if (binding.BindingType == ControlBindingType.Mouse)
		{
			return UnityKeyboardProvider.GetKeyCodeForMouse(binding.MouseControl);
		}
		return UnityKeyboardProvider.KeyMappings.FirstOrDefault((UnityKeyboardProvider.KeyMapping m) => m.source == binding.KeyControl).OldTarget0;
	}

	public Tuple<string, Sprite> GetGamepadActionInfo(KeyAction keyAction, bool shortName = true)
	{
		return GetGamepadActionInfo(PlayerControl.GetPlayerActionForKeyAction(keyAction), shortName);
	}

	public string GetGamepadActionIcon(KeyAction keyAction, bool useSize = true)
	{
		return GetGamepadActionIcon(PlayerControl.GetPlayerActionForKeyAction(keyAction), useSize);
	}

	public string GetGamepadActionIconByName(string name)
	{
		GamepadDeviceMappingConfig deviceMapping = UIInfoTools.Instance.GetDeviceMapping(CurrentControllerType);
		if (deviceMapping == null)
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing mapping for {CurrentControllerType}");
			return null;
		}
		return deviceMapping.GetActionIconByName(name);
	}

	public Tuple<string, Sprite> GetGamepadActionInfo(PlayerAction playerAction, bool shortName = true)
	{
		GamepadDeviceMappingConfig deviceMapping = UIInfoTools.Instance.GetDeviceMapping(CurrentControllerType);
		if (deviceMapping == null)
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing mapping for {CurrentControllerType}");
			return null;
		}
		if (!_playerActionControls.DeviceControlsProvider.TryGetControl(playerAction, out var control))
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing binding for player action {playerAction.Name} controller {CurrentControllerType}");
			return null;
		}
		return new Tuple<string, Sprite>(shortName ? deviceMapping.GetShortActionName(control) : deviceMapping.GetActionName(control), deviceMapping.GetActionTypeIcon(control));
	}

	public string GetGamepadActionIcon(PlayerAction playerAction, bool useSize = true)
	{
		GamepadDeviceMappingConfig deviceMapping = UIInfoTools.Instance.GetDeviceMapping(CurrentControllerType);
		if (deviceMapping == null)
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing mapping for {CurrentControllerType}");
			return null;
		}
		if (!_playerActionControls.DeviceControlsProvider.TryGetControl(playerAction, out var control))
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing binding for player action {playerAction.Name} controller {CurrentControllerType}");
			return null;
		}
		return deviceMapping.GetActionIcon(control, useSize);
	}

	public string LocalizeControls(string text)
	{
		string text2 = text.SubstringFromXToY(text.IndexOf("{") + 1, text.IndexOf("}"));
		while (text2.Length > 0)
		{
			KeyAction keyAction = (KeyAction)Enum.Parse(typeof(KeyAction), text2);
			text = text.Replace("{" + text2 + "}", GetGamepadActionIcon(keyAction, useSize: false));
			text2 = text.SubstringFromXToY(text.IndexOf("{") + 1, text.IndexOf("}"));
		}
		return text;
	}

	public void RegisterCallbackOnInitialized(Action callback)
	{
		if (initialized)
		{
			callback();
		}
		else
		{
			onInitialized += callback.Invoke;
		}
	}

	public List<UnityEngine.InputSystem.Key> GetAllowedKeyCodes()
	{
		return allowedKeycodes;
	}

	public KeyCode GetKeyCode(KeyAction action)
	{
		if (keybinds != null && keybinds.ContainsKey(action))
		{
			return keybinds[action];
		}
		if (defaultKeys.ContainsKey(action))
		{
			return defaultKeys[action];
		}
		return KeyCode.None;
	}

	public bool SetKey(KeyAction keyAction, UnityEngine.InputSystem.Key newKey)
	{
		KeyCode newKeyCode = UnityKeyboardProvider.NewToOldCodes[newKey];
		if (newKeyCode == keybinds[keyAction])
		{
			return true;
		}
		if (!IsKeyAssignable(newKey))
		{
			return false;
		}
		foreach (KeyAction item in (from it in keybinds
			where it.Key != keyAction && it.Value == newKeyCode && (string.IsNullOrEmpty(it.Key.GetGroupId()) || it.Key.GetGroupId() == keyAction.GetGroupId())
			select it.Key).ToList())
		{
			ReplaceKeybind(item, KeyCode.None);
		}
		ReplaceKeybind(keyAction, newKeyCode);
		KeyAction[] relatedActions = keyAction.GetRelatedActions();
		if (relatedActions != null)
		{
			KeyAction[] array = relatedActions;
			foreach (KeyAction keyAction2 in array)
			{
				ReplaceKeybind(keyAction2, newKeyCode);
			}
		}
		SaveData.Instance.Global.Keybinds = keybinds;
		SaveData.Instance.SaveGlobalData();
		return true;
	}

	private void ReplaceKeybind(KeyAction keyAction, KeyCode newKeyCode)
	{
		PlayerAction playerActionForKeyAction = PlayerControl.GetPlayerActionForKeyAction(keyAction);
		if (!keybinds.ContainsKey(keyAction))
		{
			keybinds.Add(keyAction, KeyCode.None);
		}
		InControl.Key keyForKeyCode = UnityKeyboardProvider.GetKeyForKeyCode(keybinds[keyAction]);
		playerActionForKeyAction.RemoveBinding(keyForKeyCode);
		if (newKeyCode != KeyCode.None)
		{
			playerActionForKeyAction.AddBinding(new KeyBindingSource(UnityKeyboardProvider.GetKeyForKeyCode(newKeyCode)));
		}
		keybinds[keyAction] = newKeyCode;
	}

	private bool IsKeyAssignable(UnityEngine.InputSystem.Key keyCode)
	{
		if (!allowedKeycodes.Contains(keyCode))
		{
			return false;
		}
		return true;
	}

	public void RestoreDefaultValues()
	{
		SaveData.Instance.Global.KeyBindings.Clear();
		SaveData.Instance.SaveGlobalData();
		keybinds = defaultKeys.ToDictionary((KeyValuePair<KeyAction, KeyCode> it) => it.Key, (KeyValuePair<KeyAction, KeyCode> it) => it.Value);
		PlayerControl.ResetBindings();
		SetBindings(DefaultBindingsObject.MouseKeyboardDefaultBindings, keybinds);
	}

	public static void RegisterToOnBindingChanged(KeyAction keyAction, Action onChanged)
	{
		if (!(Singleton<InputManager>.Instance == null))
		{
			PlayerAction playerActionForKeyAction = Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction);
			playerActionForKeyAction.OnBindingsChanged -= onChanged;
			playerActionForKeyAction.OnBindingsChanged += onChanged;
		}
	}

	public static void UnregisterToOnBindingChanged(KeyAction keyAction, Action onChanged)
	{
		if (!(Singleton<InputManager>.Instance == null) && Singleton<InputManager>.Instance.PlayerControl != null)
		{
			Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction).OnBindingsChanged -= onChanged;
		}
	}

	public void PollLastInputForControllerChanges()
	{
		if (!GamePadInUse || PlayerControl.LastInputType == BindingSourceType.None)
		{
			if (PlayerControl.LastInputType == BindingSourceType.None)
			{
				if (InControl.InputManager.Devices.Count == 0)
				{
					if (PlatformLayer.Instance.IsConsole)
					{
						CurrentControllerType = PlatformLayer.Instance.DefaultControllerType;
						RequestEnableInput(this, EKeyActionTag.ControllerExclusive);
						OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
					}
					else if (CurrentControllerType != ControllerType.MouseKeyboard)
					{
						CurrentControllerType = ControllerType.MouseKeyboard;
						RequestEnableInput(this, EKeyActionTag.All);
						RequestDisableInput(this, EKeyActionTag.ControllerExclusive);
					}
				}
				else
				{
					ControllerType gamePadControllerTypeForDeviceStyle = GetGamePadControllerTypeForDeviceStyle(InControl.InputManager.Devices[0].DeviceStyle);
					if (CurrentControllerType != gamePadControllerTypeForDeviceStyle)
					{
						CurrentControllerType = gamePadControllerTypeForDeviceStyle;
						RequestEnableInput(this, EKeyActionTag.ControllerExclusive);
						OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
					}
				}
			}
			else if (!LastInputWasMouseOrKeyboard && !IsControllerModeDisabled)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(PlayerControl.LastDeviceStyle);
				RequestEnableInput(this, EKeyActionTag.ControllerExclusive);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
		}
		else
		{
			if (!GamePadInUse)
			{
				return;
			}
			if (isUseGamepadInPc)
			{
				InputDeviceStyle deviceStyle = InControl.InputManager.ActiveDevice.DeviceStyle;
				if (deviceStyle == InputDeviceStyle.Unknown && InControl.InputManager.Devices.Count > 0)
				{
					deviceStyle = InControl.InputManager.Devices[0].DeviceStyle;
				}
				ChangeControllerTypeForDeviceStyle(deviceStyle);
			}
			else if (PlayerControl.LastInputType == BindingSourceType.DeviceBindingSource)
			{
				ChangeControllerTypeForDeviceStyle(PlayerControl.LastDeviceStyle);
			}
		}
	}

	private void ChangeControllerTypeForDeviceStyle(InputDeviceStyle style)
	{
		switch (CurrentControllerType)
		{
		case ControllerType.XboxOne:
			if (style != InputDeviceStyle.XboxOne)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(style);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
			break;
		case ControllerType.Xbox360:
			if (style != InputDeviceStyle.Xbox360)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(style);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
			break;
		case ControllerType.PS4:
			if (style != InputDeviceStyle.PlayStation4)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(style);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
			break;
		case ControllerType.PS5:
			if (style != InputDeviceStyle.PlayStation5)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(style);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
			break;
		case ControllerType.PS3:
			if (style != InputDeviceStyle.PlayStation3)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(style);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
			break;
		case ControllerType.Switch:
			if (style != InputDeviceStyle.NintendoSwitch)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(style);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
			break;
		case ControllerType.Generic:
			if (style == InputDeviceStyle.XboxOne || style == InputDeviceStyle.Xbox360 || style == InputDeviceStyle.PlayStation4 || style == InputDeviceStyle.PlayStation3)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(style);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
			break;
		default:
			if (style != InputDeviceStyle.Xbox360)
			{
				CurrentControllerType = GetGamePadControllerTypeForDeviceStyle(style);
				OnControllerTypeChangedEvent?.Invoke(CurrentControllerType);
			}
			break;
		}
	}

	private void ControllerTypeChanged(ControllerType typeChangedTo)
	{
		Debug.LogFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", "Controller type changed to " + typeChangedTo);
		if (!GamePadInUse)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		else if (EventSystem.current.currentInputModule is IInputModulePointer inputModulePointer && inputModulePointer.GameObjectUnderPointer() != null)
		{
			ExecuteEvents.ExecuteHierarchy(inputModulePointer.GameObjectUnderPointer(), new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
		}
	}

	public ControllerType GetGamePadControllerTypeForDeviceStyle(InputDeviceStyle style)
	{
		return style switch
		{
			InputDeviceStyle.XboxOne => ControllerType.XboxOne, 
			InputDeviceStyle.Xbox360 => ControllerType.Xbox360, 
			InputDeviceStyle.PlayStation4 => ControllerType.PS4, 
			InputDeviceStyle.PlayStation3 => ControllerType.PS3, 
			InputDeviceStyle.PlayStation5 => ControllerType.PS5, 
			InputDeviceStyle.NintendoSwitch => ControllerType.Switch, 
			_ => ControllerType.XboxOne, 
		};
	}

	public static bool GetIsPressed(KeyAction action)
	{
		if (Singleton<InputManager>.Instance == null)
		{
			return false;
		}
		return Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(action)?.IsPressed ?? false;
	}

	public static float GetValue(KeyAction action)
	{
		if (Singleton<InputManager>.Instance == null)
		{
			return 0f;
		}
		return Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(action)?.Value ?? 0f;
	}

	public static bool GetWasPressed(KeyAction action)
	{
		if (Singleton<InputManager>.Instance == null)
		{
			return false;
		}
		return Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(action)?.WasPressed ?? false;
	}

	public static bool GetWasReleased(KeyAction action)
	{
		if (Singleton<InputManager>.Instance == null)
		{
			return false;
		}
		return Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(action)?.WasReleased ?? false;
	}

	public static bool GetKey(UnityEngine.InputSystem.Key keyboardInput)
	{
		return InputSystemUtilities.GetKey(keyboardInput);
	}

	public static bool GetKeyUp(UnityEngine.InputSystem.Key keyboardInput)
	{
		return InputSystemUtilities.GetKeyUp(keyboardInput);
	}

	public static bool GetKeyDown(UnityEngine.InputSystem.Key keyboardInput)
	{
		return InputSystemUtilities.GetKeyDown(keyboardInput);
	}

	public static void RegisterToOnPressed(KeyAction keyAction, Action onPressed)
	{
		if (keyAction == KeyAction.None)
		{
			return;
		}
		try
		{
			GHControls.GHAction gHAction = (GHControls.GHAction)Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction);
			if (gHAction == null)
			{
				Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing PlayerAction for {keyAction} action");
				return;
			}
			gHAction.OnPressed -= onPressed;
			gHAction.OnPressed += onPressed;
			Debug.LogFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Register to Pressed {keyAction}");
		}
		catch (InvalidCastException ex)
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"KeyAction {keyAction} is not a GHAction");
			throw ex;
		}
	}

	public static void UnregisterToOnPressed(KeyAction keyAction, Action onPressed)
	{
		try
		{
			if (keyAction != KeyAction.None && !(Singleton<InputManager>.Instance == null) && Singleton<InputManager>.Instance.PlayerControl != null)
			{
				GHControls.GHAction gHAction = (GHControls.GHAction)Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction);
				if (gHAction == null)
				{
					Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing PlayerAction for {keyAction} action");
				}
				else
				{
					gHAction.OnPressed -= onPressed;
				}
			}
		}
		catch (InvalidCastException)
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"KeyAction {keyAction} is not a GHAction");
			throw;
		}
	}

	public static void RegisterToOnReleased(KeyAction keyAction, Action onReleased)
	{
		if (keyAction == KeyAction.None)
		{
			return;
		}
		try
		{
			GHControls.GHAction gHAction = (GHControls.GHAction)Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction);
			if (gHAction == null)
			{
				Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing PlayerAction for {keyAction} action");
				return;
			}
			gHAction.OnReleased -= onReleased;
			gHAction.OnReleased += onReleased;
			Debug.LogFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Register to Released {keyAction}");
		}
		catch (InvalidCastException ex)
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"KeyAction {keyAction} is not a GHAction");
			throw ex;
		}
	}

	public static void UnregisterToOnReleased(KeyAction keyAction, Action onReleased)
	{
		try
		{
			if (keyAction != KeyAction.None && !(Singleton<InputManager>.Instance == null) && Singleton<InputManager>.Instance.PlayerControl != null)
			{
				GHControls.GHAction gHAction = (GHControls.GHAction)Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(keyAction);
				if (gHAction == null)
				{
					Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"Missing PlayerAction for {keyAction} action");
				}
				else
				{
					gHAction.OnReleased -= onReleased;
				}
			}
		}
		catch (InvalidCastException)
		{
			Debug.LogErrorFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", $"KeyAction {keyAction} is not a GHAction");
			throw;
		}
	}

	public static void SkipNextSubmitAction()
	{
		if (Singleton<InputManager>.Instance != null && Singleton<InputManager>.Instance.m_CurrentInputModule != null)
		{
			Singleton<InputManager>.Instance.m_CurrentInputModule.SkipNextSubmitAction();
		}
	}

	public static void SkipNextCancelAction()
	{
		if (Singleton<InputManager>.Instance != null && Singleton<InputManager>.Instance.m_CurrentInputModule != null)
		{
			Singleton<InputManager>.Instance.m_CurrentInputModule.SkipNextCancelAction();
		}
	}

	public static void DisableMoveEvents()
	{
		Singleton<InputManager>.Instance.m_CurrentInputModule.DisableMoveEvents();
	}

	public static void EnableMoveEvents()
	{
		Singleton<InputManager>.Instance.m_CurrentInputModule.EnableMoveEvents();
	}

	private void InitialiseInControlGHControls()
	{
		PlayerControl = new GHControls();
		Debug.LogFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", "Initialising GHControls");
		PlayerControl.MouseUp.AddDefaultBinding(InControl.Mouse.PositiveY);
		PlayerControl.MouseDown.AddDefaultBinding(InControl.Mouse.NegativeY);
		PlayerControl.MouseLeft.AddDefaultBinding(InControl.Mouse.NegativeX);
		PlayerControl.MouseRight.AddDefaultBinding(InControl.Mouse.PositiveX);
		PlayerControl.MouseWheelDown.AddDefaultBinding(InControl.Mouse.NegativeScrollWheel);
		PlayerControl.MouseWheelUp.AddDefaultBinding(InControl.Mouse.PositiveScrollWheel);
		PlayerControl.MouseClickLeft.AddDefaultBinding(InControl.Mouse.LeftButton);
		PlayerControl.MouseClickRight.AddDefaultBinding(InControl.Mouse.RightButton);
		PlayerControl.MouseClickMiddle.AddDefaultBinding(InControl.Mouse.MiddleButton);
		PlayerControl.UIAlt1.AddDefaultBinding(InputControlType.Action3);
		PlayerControl.UIAlt2.AddDefaultBinding(InputControlType.Action4);
		PlayerControl.DebugMenuKey.AddDefaultBinding(InControl.Key.F1);
		PlayerControl.FreeCamRotateUp.AddDefaultBinding(InControl.Mouse.PositiveY);
		PlayerControl.FreeCamRotateUp.AddDefaultBinding(InputControlType.RightStickUp);
		PlayerControl.FreeCamRotateDown.AddDefaultBinding(InControl.Mouse.NegativeY);
		PlayerControl.FreeCamRotateDown.AddDefaultBinding(InputControlType.RightStickDown);
		PlayerControl.FreeCamRotateLeft.AddDefaultBinding(InControl.Mouse.NegativeX);
		PlayerControl.FreeCamRotateLeft.AddDefaultBinding(InputControlType.RightStickLeft);
		PlayerControl.FreeCamRotateRight.AddDefaultBinding(InControl.Mouse.PositiveX);
		PlayerControl.FreeCamRotateRight.AddDefaultBinding(InputControlType.RightStickRight);
		PlayerControl.FreeCamMoveDown.AddDefaultBinding(InputControlType.RightTrigger);
		PlayerControl.FreeCamMoveUp.AddDefaultBinding(InputControlType.RightBumper);
		PlayerControl.UIDownAlt.AddDefaultBinding(InputControlType.RightStickDown);
		PlayerControl.UIUpAlt.AddDefaultBinding(InputControlType.RightStickUp);
		PlayerControl.UILeftAlt.AddDefaultBinding(InputControlType.RightStickLeft);
		PlayerControl.UIRightAlt.AddDefaultBinding(InputControlType.RightStickRight);
		PlayerControl.DebugLeftStick.AddDefaultBinding(InputControlType.LeftStickButton);
		PlayerControl.DebugRightStick.AddDefaultBinding(InputControlType.RightStickButton);
		PlayerControl.DebugButtonEast.AddDefaultBinding(InputControlType.Action2);
		PlayerControl.DebugButtonSouth.AddDefaultBinding(InputControlType.Action1);
		PlayerControl.MenuEnter.AddDefaultBinding(InControl.Key.Return);
		if (!IsPCVersion())
		{
			AssignGamepadBindingsToPlayerActions();
		}
		m_CurrentInputModule = EventSystem.current.GetComponent<InControlInputModule>();
		if (m_CurrentInputModule != null)
		{
			m_CurrentInputModule.SubmitAction = PlayerControl.UISubmit;
			m_CurrentInputModule.CancelAction = PlayerControl.UICancel;
			m_CurrentInputModule.AddMoveAction(PlayerControl.UIMove, MoveActionSourceType.Stick);
			m_CurrentInputModule.AddMoveAction(PlayerControl.UIMoveDPad, MoveActionSourceType.DPad);
			m_CurrentInputModule.InputVectorEvent += OnVectorEvent;
			m_CurrentInputModule.PreUnityInputVectorEvent += OnPreUnityVectorEvent;
			m_CurrentInputModule.allowMouseInput = !isUseGamepadInPc;
		}
	}

	private void OnVectorEvent(InputVectorEventData inputVectorEventData)
	{
		UINavigationDirection uINavigationDirection = inputVectorEventData.InputVector.ToNavigationDirection();
		MoveSelectionEvent?.Invoke(new UIActionBaseEventData
		{
			UINavigationDirection = uINavigationDirection,
			UINavigationSourceType = InputSourceTypeToNavigationSourceType(inputVectorEventData.SourceType)
		});
	}

	private UINavigationSourceType InputSourceTypeToNavigationSourceType(MoveActionSourceType moveActionSourceType)
	{
		return moveActionSourceType switch
		{
			MoveActionSourceType.None => UINavigationSourceType.None, 
			MoveActionSourceType.Stick => UINavigationSourceType.Stick, 
			MoveActionSourceType.DPad => UINavigationSourceType.DPad, 
			_ => throw new ArgumentOutOfRangeException("moveActionSourceType", moveActionSourceType, null), 
		};
	}

	private void OnPreUnityVectorEvent(Vector2 vector)
	{
		UINavigationDirection obj = vector.ToNavigationDirection();
		PreUnityMoveSelectionEvent?.Invoke(obj);
	}

	private void AssignGamepadBindingsToPlayerActions(bool withRemoveCurrentBindings = false)
	{
		if (withRemoveCurrentBindings)
		{
			RemoveBindings(DefaultBindingsObject.MouseKeyboardDefaultBindings, keybinds);
			RemoveDefaultMouseKeyboardBindings();
		}
		isUseGamepadInPc = true;
		SetBindings(DefaultBindingsObject.XboxGamepadDefaultBindings);
		AssignDefaultGamepadBindings();
		Debug.LogFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", "Assigning Gamepad bindings to KeyActions");
	}

	private void AssignMouseKeyboardBindingsToPlayerActions(bool withRemoveCurrentBindings = false)
	{
		if (withRemoveCurrentBindings)
		{
			RemoveBindings(DefaultBindingsObject.XboxGamepadDefaultBindings);
			RemoveDefaultGamepadBindings();
		}
		isUseGamepadInPc = false;
		SetBindings(DefaultBindingsObject.MouseKeyboardDefaultBindings, keybinds);
		AssignDefaultMouseKeyboardBindings();
		Debug.LogFormat("<color=#DCFF00>[INPUT MANAGER]</color> {0}", "Assigning MouseKeyboard bindings to KeyActions");
	}

	private void AssignDefaultGamepadBindings()
	{
		PlayerControl.UIUp.AddDefaultBinding(InputControlType.LeftStickUp);
		PlayerControl.UIDown.AddDefaultBinding(InputControlType.LeftStickDown);
		PlayerControl.UILeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		PlayerControl.UIRight.AddDefaultBinding(InputControlType.LeftStickRight);
		PlayerControl.UIUpDPad.AddDefaultBinding(InputControlType.DPadUp);
		PlayerControl.UIDownDPad.AddDefaultBinding(InputControlType.DPadDown);
		PlayerControl.UILeftDPad.AddDefaultBinding(InputControlType.DPadLeft);
		PlayerControl.UIRightDPad.AddDefaultBinding(InputControlType.DPadRight);
	}

	private void RemoveDefaultGamepadBindings()
	{
		PlayerControl.UIUp.RemoveBinding(InputControlType.LeftStickUp);
		PlayerControl.UIDown.RemoveBinding(InputControlType.LeftStickDown);
		PlayerControl.UILeft.RemoveBinding(InputControlType.LeftStickLeft);
		PlayerControl.UIRight.RemoveBinding(InputControlType.LeftStickRight);
		PlayerControl.UIUpDPad.RemoveBinding(InputControlType.DPadUp);
		PlayerControl.UIDownDPad.RemoveBinding(InputControlType.DPadDown);
		PlayerControl.UILeftDPad.RemoveBinding(InputControlType.DPadLeft);
		PlayerControl.UIRightDPad.RemoveBinding(InputControlType.DPadRight);
	}

	private void AssignDefaultMouseKeyboardBindings()
	{
		PlayerControl.PanCameraUp.AddDefaultBinding(InControl.Key.UpArrow);
		PlayerControl.PanCameraRight.AddDefaultBinding(InControl.Key.RightArrow);
		PlayerControl.PanCameraLeft.AddDefaultBinding(InControl.Key.LeftArrow);
		PlayerControl.PanCameraDown.AddDefaultBinding(InControl.Key.DownArrow);
		PlayerControl.PanCameraUpMap.AddDefaultBinding(InControl.Key.UpArrow);
		PlayerControl.PanCameraRightMap.AddDefaultBinding(InControl.Key.RightArrow);
		PlayerControl.PanCameraLeftMap.AddDefaultBinding(InControl.Key.LeftArrow);
		PlayerControl.PanCameraDownMap.AddDefaultBinding(InControl.Key.DownArrow);
		PlayerControl.UINextTab.AddDefaultBinding(InControl.Key.Tab);
		PlayerControl.UIPreviousTab.AddDefaultBinding(new KeyCombo(InControl.Key.LeftShift, InControl.Key.Tab));
	}

	private void RemoveDefaultMouseKeyboardBindings()
	{
		PlayerControl.PanCameraUp.RemoveBinding(InControl.Key.UpArrow);
		PlayerControl.PanCameraRight.RemoveBinding(InControl.Key.RightArrow);
		PlayerControl.PanCameraLeft.RemoveBinding(InControl.Key.LeftArrow);
		PlayerControl.PanCameraDown.RemoveBinding(InControl.Key.DownArrow);
		PlayerControl.PanCameraUpMap.RemoveBinding(InControl.Key.UpArrow);
		PlayerControl.PanCameraRightMap.RemoveBinding(InControl.Key.RightArrow);
		PlayerControl.PanCameraLeftMap.RemoveBinding(InControl.Key.LeftArrow);
		PlayerControl.PanCameraDownMap.RemoveBinding(InControl.Key.DownArrow);
		PlayerControl.UINextTab.RemoveBinding(InControl.Key.Tab);
		PlayerControl.UIPreviousTab.RemoveBinding(new KeyCombo(InControl.Key.LeftShift, InControl.Key.Tab));
	}

	private void SetBindings(ControlBindings.ControlsMapping controlBindings, Dictionary<KeyAction, KeyCode> savedKeyBindingsToUse = null)
	{
		controlBindings.MapBindingsToPlayerControls(PlayerControl, savedKeyBindingsToUse);
		_playerActionControls = new PlayerActionControls(PlayerControl, _buttonsComboController);
	}

	private void RemoveBindings(ControlBindings.ControlsMapping controlBindings, Dictionary<KeyAction, KeyCode> savedKeyBindingsToUse = null)
	{
		controlBindings.MapRemovingFromPlayerControls(PlayerControl, savedKeyBindingsToUse);
		_playerActionControls.Dispose();
	}

	public Vector2 GetVirtualMousePosition()
	{
		if (_virtualMouse == null)
		{
			Debug.LogError("Virtual mouse is null;");
			return new Vector2(Screen.width / 2, Screen.height / 2);
		}
		return _virtualMouse.position.ReadValue();
	}

	[ContextMenu("Print Disabled Controls")]
	private void PrintDisabledControls()
	{
		Debug.Log("------ Disabled controls -----");
		foreach (KeyValuePair<KeyAction, HashSet<object>> disableInputRequest in disableInputRequests)
		{
			if (disableInputRequest.Value.Count > 0)
			{
				Debug.Log(string.Format("{0} disabled by {1}", disableInputRequest.Key, string.Join(", ", disableInputRequest.Value)));
			}
		}
	}

	public static void UpdateMouseInputEnabled(bool value)
	{
		if (Singleton<InputManager>.Instance != null && Singleton<InputManager>.Instance.m_CurrentInputModule != null)
		{
			if (value)
			{
				Singleton<InputManager>.Instance.m_CurrentInputModule.allowMouseInput = true;
			}
			else
			{
				Singleton<InputManager>.Instance.m_CurrentInputModule.allowMouseInput = !isUseGamepadInPc;
			}
		}
	}

	private void OnDeviceAttached(InControl.InputDevice inputDevice)
	{
		PollLastInputForControllerChanges();
	}
}
