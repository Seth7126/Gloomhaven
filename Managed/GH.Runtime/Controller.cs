using System;
using AStar;
using Chronos;
using Code.State;
using InControl;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
	private const float c_DoubleClickTime = 0.3f;

	private const float c_MaximumTranslationSquaredMagnitude = 0.0625f;

	private static Controller _instance;

	private static CInteractable s_TargetInteractableBackup;

	private static Vector3 s_FocalPointOnDown;

	public static CInteractableActor s_Actor;

	public static CInteractable s_TargetInteractable;

	public static CInteractableActor s_TargetActor;

	public static bool s_SingleClicked;

	public static bool s_DoubleClicked;

	public static bool s_StartedButtonDownInGUI;

	public static bool s_ShouldPing;

	[SerializeField]
	private string m_pingTileGamepadCombo;

	public LayerMask m_GameSelectionRaycastLayer;

	public LayerMask m_HexSelectionRaycastLayer;

	public LayerMask m_HeroSelectionRaycastLayer;

	[HideInInspector]
	public LayerMask m_ActiveSelectionRaycastLayer;

	private float m_DoubleClickStart;

	private IHotkeySession _hotkeySession;

	private bool _allowPingTile;

	private IStateFilter _stateFilter = new StateFilterByType(typeof(SelectItemState), typeof(CheckOutRoundCardsScenarioState)).InverseFilter();

	public static Controller Instance => _instance;

	[UsedImplicitly]
	public void Awake()
	{
		s_TargetInteractable = null;
		s_TargetActor = null;
		s_SingleClicked = false;
		s_DoubleClicked = false;
		s_StartedButtonDownInGUI = false;
		s_Actor = GetComponent<CInteractableActor>();
		_instance = this;
		InitializePingTileInput();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		_instance = null;
		s_Actor = null;
		s_TargetInteractable = null;
		s_TargetActor = null;
		s_TargetInteractableBackup = null;
		ReleasePingTileInput();
	}

	private void InitializePingTileInput()
	{
		int allowPingTile;
		if (InputManager.GamePadInUse)
		{
			CMapState mapState = AdventureState.MapState;
			allowPingTile = ((mapState != null && !mapState.IsPlayingTutorial) ? 1 : 0);
		}
		else
		{
			allowPingTile = 0;
		}
		_allowPingTile = (byte)allowPingTile != 0;
		if (_allowPingTile)
		{
			if (FFSNetwork.IsOnline)
			{
				OnSwitchedToMultiplayer();
			}
			else
			{
				FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
			}
		}
	}

	private void ReleasePingTileInput()
	{
		if (_allowPingTile)
		{
			FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
			FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
			UnsubscribePingTileInput();
		}
	}

	private void OnActiveDeviceChanged(InControl.InputDevice _)
	{
		if (!PlatformLayer.Instance.IsConsole)
		{
			UnsubscribePingTileInput();
			SubscribePingTileInput();
		}
	}

	private void SubscribePingTileInput()
	{
		_hotkeySession = Hotkeys.Instance.GetSession();
		Singleton<InputManager>.Instance.ButtonsComboController.AddCombo(m_pingTileGamepadCombo, OnPingTileComboPressed);
		CameraController s_CameraController = CameraController.s_CameraController;
		s_CameraController.OnInputEnableChanged = (Action<bool>)Delegate.Combine(s_CameraController.OnInputEnableChanged, new Action<bool>(OnCameraInputEnableChanged));
		OnCameraInputEnableChanged(CameraController.s_CameraController.InputEnabled);
		InControl.InputManager.OnActiveDeviceChanged += OnActiveDeviceChanged;
	}

	private void UnsubscribePingTileInput()
	{
		if (Singleton<InputManager>.Instance != null)
		{
			Singleton<InputManager>.Instance.ButtonsComboController.RemoveCombo(m_pingTileGamepadCombo);
		}
		if (CameraController.s_CameraController != null)
		{
			CameraController s_CameraController = CameraController.s_CameraController;
			s_CameraController.OnInputEnableChanged = (Action<bool>)Delegate.Remove(s_CameraController.OnInputEnableChanged, new Action<bool>(OnCameraInputEnableChanged));
		}
		InControl.InputManager.OnActiveDeviceChanged -= OnActiveDeviceChanged;
		_hotkeySession?.Dispose();
		_hotkeySession = null;
	}

	private void Start()
	{
		m_DoubleClickStart = Timekeeper.instance.m_GlobalClock.time;
		m_ActiveSelectionRaycastLayer = m_HexSelectionRaycastLayer;
	}

	private void LateUpdate()
	{
		bool flag = false;
		if (CommonLoop(isPaused: false))
		{
			bool flag2 = false;
			CInteractable cInteractable = MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, m_ActiveSelectionRaycastLayer);
			if ((bool)cInteractable)
			{
				TileBehaviour component = cInteractable.GetComponent<TileBehaviour>();
				ControllerInputPointer.CursorType = ((!(component == null) && component.m_ClientTile != null) ? ECursorType.Default : ECursorType.Invalid);
				if (component == null || component.m_ClientTile == null || InteractabilityManager.ShouldAllowSelectionForTileIndex(component.m_ClientTile.m_Tile.m_ArrayIndex))
				{
					if (s_ShouldPing)
					{
						Point arrayIndex = component.m_ClientTile.m_Tile.m_ArrayIndex;
						CClientTile tile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[arrayIndex.X, arrayIndex.Y];
						Singleton<UIScenarioMultiplayerController>.Instance.PingTile(tile);
						s_ShouldPing = false;
					}
					else if (Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
					{
						flag2 = true;
						s_TargetInteractable = SelectNewObject(s_TargetInteractable, cInteractable);
						if (component != null && component.m_ClientTile != null)
						{
							UIEventManager.LogTileSelectEvent(component.m_ClientTile.m_Tile.m_ArrayIndex);
						}
					}
					else if (s_DoubleClicked)
					{
						cInteractable.OnDoubleClicked();
					}
				}
			}
			else
			{
				ControllerInputPointer.CursorType = ECursorType.Invalid;
			}
			if (!flag2)
			{
				CameraController.s_CameraController.ResetFocalPointGameObject();
				if ((Singleton<InputManager>.Instance.PlayerControl.MouseClickLeft.WasReleased || ControllerReleaseHex()) && !CameraController.s_Translated)
				{
					flag = true;
				}
			}
		}
		if (Singleton<InputManager>.Instance.PlayerControl.MouseClickRight.WasReleased && !CameraController.s_Rotated)
		{
			flag = true;
		}
		if (flag)
		{
			ClearTarget();
		}
	}

	private bool ControllerPressedHex()
	{
		bool flag = ((Singleton<UIReadyToggle>.Instance != null && Singleton<UIReadyToggle>.Instance.IsVisible) ? Singleton<UIReadyToggle>.Instance.ShortPressed : Choreographer.s_Choreographer.readyButton.ShortPressed);
		bool flag2 = InputManager.GetWasPressed(KeyAction.UI_SUBMIT) && (Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy ? (!Choreographer.s_Choreographer.readyButton.IsVisibility) : (!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy));
		if (InputManager.GamePadInUse && _stateFilter.IsCurrentStateValid())
		{
			return flag2 || flag;
		}
		return false;
	}

	private bool ControllerReleaseHex()
	{
		if (InputManager.GamePadInUse && ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.WorldMap))
		{
			return InputManager.GetWasReleased(KeyAction.UI_SUBMIT);
		}
		return false;
	}

	private bool CommonLoop(bool isPaused)
	{
		s_SingleClicked = false;
		s_DoubleClicked = false;
		bool flag = ControllerPressedHex();
		if (Singleton<InputManager>.Instance.PlayerControl.MouseClickLeft.WasPressed || flag)
		{
			s_FocalPointOnDown = CameraController.s_CameraController.m_TargetFocalPoint;
			s_StartedButtonDownInGUI = EventSystem.current.IsPointerOverGameObject() || (DebugMenu.DebugMenuNotNull && DebugMenu.Instance.IsMenuOpen);
		}
		if (Singleton<InputManager>.Instance.PlayerControl.MouseClickLeft.WasReleased || flag)
		{
			s_SingleClicked = true;
			if (!s_StartedButtonDownInGUI)
			{
				if (Main.s_NonPausedTime - m_DoubleClickStart < 0.3f)
				{
					s_DoubleClicked = true;
					m_DoubleClickStart = Main.s_NonPausedTime - 0.3f;
				}
				else
				{
					m_DoubleClickStart = Main.s_NonPausedTime;
				}
			}
			if (!InputManager.GamePadInUse && InputManager.GetIsPressed(KeyAction.HOLD_TO_PING))
			{
				RequestPingTile();
			}
		}
		if ((s_SingleClicked || s_DoubleClicked || s_ShouldPing) && !s_StartedButtonDownInGUI)
		{
			if (isPaused)
			{
				return !TimeManager.IsPaused;
			}
			return true;
		}
		return false;
	}

	public static void ClearTarget()
	{
		if ((bool)s_TargetInteractable)
		{
			s_TargetInteractable = null;
			s_TargetActor = null;
		}
	}

	private CInteractable SelectNewObject(CInteractable old_interactable, CInteractable new_interactable)
	{
		if ((bool)new_interactable)
		{
			new_interactable.ShowNormalInterface(disabled: false);
			if (s_DoubleClicked)
			{
				new_interactable.OnDoubleClicked();
			}
			return new_interactable;
		}
		return null;
	}

	private void EnablePingTileCombo(bool value)
	{
		if (_hotkeySession != null && Singleton<InputManager>.Instance.HasGamepad())
		{
			Singleton<InputManager>.Instance.ButtonsComboController.SetEnabledCombo(m_pingTileGamepadCombo, value);
			_hotkeySession.SetHotkeyAdded("PingTile", value);
			if (!value)
			{
				s_ShouldPing = false;
			}
		}
	}

	private void RequestPingTile()
	{
		s_ShouldPing = true;
	}

	private void OnPingTileComboPressed(Gamepad gamepad)
	{
		RequestPingTile();
	}

	private void OnCameraInputEnableChanged(bool enabled)
	{
		EnablePingTileCombo(enabled);
	}

	private void OnSwitchedToMultiplayer()
	{
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
		SubscribePingTileInput();
	}

	private void OnSwitchedToSinglePlayer()
	{
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
		UnsubscribePingTileInput();
	}
}
