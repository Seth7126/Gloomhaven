using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using Script.GUI.Popups;
using Script.GUI.SMNavigation.HotkeysBehaviour;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogPopup : MonoBehaviour, IEscapable
{
	private struct PreviousState
	{
		private bool wasActive;

		private Transform contentHolder;

		public GameObject GameObject;

		private Vector3 position;

		private Vector2 minAnchor;

		private Vector2 maxAnchor;

		public PreviousState(GameObject obj)
		{
			GameObject = obj;
			wasActive = obj.activeSelf;
			contentHolder = obj.transform.parent;
			position = obj.transform.localPosition;
			RectTransform component = obj.GetComponent<RectTransform>();
			minAnchor = component.anchorMin;
			maxAnchor = component.anchorMax;
		}

		public void ApplyPreviousState()
		{
			GameObject.SetActive(wasActive);
			GameObject.transform.SetParent(contentHolder);
			RectTransform component = GameObject.GetComponent<RectTransform>();
			component.anchorMin = minAnchor;
			component.anchorMax = maxAnchor;
			GameObject.transform.localPosition = position;
		}
	}

	[SerializeField]
	private Transform contentHolder;

	[SerializeField]
	private Transform horizontalOptionsHolder;

	[SerializeField]
	private Transform verticalOptionsHolder;

	[SerializeField]
	private TextMeshProUGUI contentText;

	[SerializeField]
	private GameObject captionHolder;

	[SerializeField]
	private TextMeshProUGUI captionText;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private GameObject optionButtonPrefab;

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	private List<InputButton> optionButtons = new List<InputButton>();

	private bool allowHide;

	private List<PreviousState> contentState = new List<PreviousState>();

	private int cancelOption = -1;

	private UnityAction cancelAction;

	public Action OnHiding;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	public UIWindow Window => window;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	private void Awake()
	{
		_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this);
		controllerArea.OnFocusedArea.AddListener(OnControllerFocused);
		controllerArea.OnUnfocusedArea.AddListener(OnControllerUnfocused);
		controllerArea.OnEnabledArea.AddListener(delegate
		{
			InputManager.RequestDisableInput(this, EKeyActionTag.AreaShortcuts);
		});
		controllerArea.OnDisabledArea.AddListener(delegate
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.AreaShortcuts);
		});
	}

	private void OnDestroy()
	{
		OnHiding = null;
		if (CoreApplication.IsQuitting || !IsOpen())
		{
			return;
		}
		UIWindowManager.UnregisterEscapable(this);
		foreach (PreviousState item in contentState)
		{
			item.ApplyPreviousState();
		}
		contentState.Clear();
		controllerArea.Destroy();
		InputManager.RequestEnableInput(this, EKeyActionTag.AreaShortcuts);
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnBackButtonPressed);
		}
	}

	private void OnControllerUnfocused()
	{
		for (int i = 0; i < optionButtons.Count && optionButtons[i].gameObject.activeSelf; i++)
		{
			optionButtons[i].GetComponent<ControllerInputElement>().enabled = false;
		}
	}

	private void OnControllerFocused()
	{
		for (int i = 0; i < optionButtons.Count && optionButtons[i].gameObject.activeSelf; i++)
		{
			optionButtons[i].GetComponent<ControllerInputElement>().enabled = true;
		}
	}

	public void Show(GameObject content, DialogOption[] options, bool allowHide = false, int cancelOption = -1, UnityAction cancelAction = null)
	{
		Show(new List<GameObject> { content }, options, allowHide, cancelOption, cancelAction);
	}

	public void Show(List<GameObject> content, DialogOption[] options, bool allowHide = false, int cancelOption = -1, UnityAction cancelAction = null)
	{
		contentState.Clear();
		contentText.gameObject.SetActive(value: false);
		this.allowHide = allowHide;
		this.cancelOption = cancelOption;
		this.cancelAction = cancelAction;
		foreach (GameObject item in content)
		{
			contentState.Add(new PreviousState(item));
			item.SetActive(value: true);
			item.transform.SetParent(contentHolder);
		}
		backgroundImage.enabled = false;
		captionHolder.SetActive(value: false);
		horizontalOptionsHolder.gameObject.SetActive(value: true);
		verticalOptionsHolder.gameObject.SetActive(value: false);
		HelperTools.NormalizePool(ref optionButtons, optionButtonPrefab, horizontalOptionsHolder, options.Length);
		int num = 0;
		foreach (DialogOption option in options)
		{
			optionButtons[num].ExtendedButton.buttonText.text = option.text;
			optionButtons[num].ExtendedButton.onClick.RemoveAllListeners();
			optionButtons[num].ExtendedButton.onMouseEnter.RemoveAllListeners();
			optionButtons[num].ExtendedButton.onMouseExit.RemoveAllListeners();
			optionButtons[num].ExtendedButton.onClick.AddListener(delegate
			{
				Hide();
				option.onMouseClickAction?.Invoke();
			});
			InputButton inputButton = optionButtons[num];
			IKeyActionHandlerBlocker[] blockers = new IKeyActionHandlerBlocker[2]
			{
				_skipFrameKeyActionHandlerBlocker,
				new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)
			};
			bool num2 = option.keyAction != KeyAction.UI_CANCEL;
			KeyAction keyAction = option.keyAction;
			if (num2)
			{
				inputButton.Deinitialize();
				inputButton.InitializeInputGamepad(keyAction, blockers);
				inputButton.ButtonPressed += delegate
				{
					OnOptionButtonPressed(option);
				};
			}
			if (InputManager.GamePadInUse)
			{
				optionButtons[num].gameObject.TrySetHotkeyEvent(option.keyAction);
			}
			else
			{
				InitiativeTrack.Instance.ToggleSortingOrder(value: false);
			}
			if (option.onMouseEnterAction != null)
			{
				optionButtons[num].ExtendedButton.onMouseEnter.AddListener(option.onMouseEnterAction);
			}
			if (option.onMouseExitAction != null)
			{
				optionButtons[num].ExtendedButton.onMouseExit.AddListener(option.onMouseExitAction);
			}
			InteractabilityIsolatedUIControl component = optionButtons[num].GetComponent<InteractabilityIsolatedUIControl>();
			if (component != null)
			{
				component.ControlIdentifier = num.ToString();
				InteractabilityManager.RegisterControlForInteractionLimiting(component);
			}
			num++;
		}
		_skipFrameKeyActionHandlerBlocker.Run();
		if (window == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			window.escapeKeyAction = (allowHide ? UIWindow.EscapeKeyAction.Hide : UIWindow.EscapeKeyAction.None);
			window.Show();
		}
		if (InputManager.GamePadInUse && !allowHide)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnBackButtonPressed).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
		}
		else if (!allowHide)
		{
			UIWindowManager.RegisterEscapable(this);
		}
		controllerArea.Enable();
	}

	private void OnBackButtonPressed()
	{
		Escape();
	}

	private void OnOptionButtonPressed(DialogOption option)
	{
		Hide();
		option.onMouseClickAction?.Invoke();
	}

	public void Show(string descriptionString, string captionString, DialogOption[] options, bool allowHide = false, int cancelOption = -1)
	{
		this.cancelOption = cancelOption;
		contentText.gameObject.SetActive(value: true);
		backgroundImage.enabled = true;
		if (string.IsNullOrEmpty(captionString))
		{
			captionHolder.SetActive(value: false);
		}
		else
		{
			captionHolder.SetActive(value: true);
			captionText.text = captionString;
		}
		this.allowHide = allowHide;
		horizontalOptionsHolder.gameObject.SetActive(value: false);
		verticalOptionsHolder.gameObject.SetActive(value: true);
		HelperTools.NormalizePool(ref optionButtons, optionButtonPrefab, verticalOptionsHolder, options.Length);
		contentText.text = descriptionString;
		int num = 0;
		foreach (DialogOption option in options)
		{
			optionButtons[num].ExtendedButton.buttonText.text = option.text;
			optionButtons[num].ExtendedButton.onClick.RemoveAllListeners();
			optionButtons[num].ExtendedButton.onMouseEnter.RemoveAllListeners();
			optionButtons[num].ExtendedButton.onMouseExit.RemoveAllListeners();
			optionButtons[num].ExtendedButton.onClick.AddListener(delegate
			{
				Hide();
				option.onMouseClickAction?.Invoke();
			});
			ControllerInputClickeable component = optionButtons[num].GetComponent<ControllerInputClickeable>();
			component.SetKeyAction(option.keyAction, option.keyAction != KeyAction.UI_CANCEL);
			component.enabled = controllerArea.IsFocused;
			if (option.onMouseEnterAction != null)
			{
				optionButtons[num].ExtendedButton.onMouseEnter.AddListener(option.onMouseEnterAction);
			}
			if (option.onMouseExitAction != null)
			{
				optionButtons[num].ExtendedButton.onMouseExit.AddListener(option.onMouseExitAction);
			}
			InteractabilityIsolatedUIControl component2 = optionButtons[num].GetComponent<InteractabilityIsolatedUIControl>();
			if (component2 != null)
			{
				component2.ControlIdentifier = num.ToString();
				InteractabilityManager.RegisterControlForInteractionLimiting(component2);
			}
			num++;
		}
		if (window == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			window.escapeKeyAction = (allowHide ? UIWindow.EscapeKeyAction.Hide : UIWindow.EscapeKeyAction.None);
			window.Show();
		}
		if (InputManager.GamePadInUse && !allowHide)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnBackButtonPressed).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
		}
		else if (!allowHide)
		{
			UIWindowManager.RegisterEscapable(this);
		}
		controllerArea.Enable();
	}

	public void Show(string descriptionText, DialogOption[] options, bool allowHide = false, int cancelOption = -1)
	{
		Show(descriptionText, null, options, allowHide, cancelOption);
	}

	public void Hide(bool cleanContent = true)
	{
		OnHiding?.Invoke();
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnBackButtonPressed);
		}
		else
		{
			UIWindowManager.UnregisterEscapable(this);
			InitiativeTrack.Instance.ToggleSortingOrder(value: true);
		}
		OnControllerUnfocused();
		if (window == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			HelperTools.NormalizePool(ref optionButtons, optionButtonPrefab, verticalOptionsHolder, 0);
			window.Hide();
		}
		foreach (PreviousState item in contentState)
		{
			item.ApplyPreviousState();
		}
		if (cleanContent)
		{
			contentState.Clear();
		}
		foreach (InputButton optionButton in optionButtons)
		{
			if (!(optionButton == null))
			{
				InteractabilityIsolatedUIControl component = optionButton.GetComponent<InteractabilityIsolatedUIControl>();
				if (component != null)
				{
					component.ControlIdentifier = string.Empty;
					InteractabilityManager.DeregisterControlForInteractionLimiting(component);
				}
			}
		}
		cancelOption = -1;
		controllerArea.Destroy();
	}

	public void TryHide()
	{
		if (allowHide)
		{
			Hide();
		}
	}

	public void Cancel()
	{
		Singleton<InputManager>.Instance.PlayerControl.MarkActionAsHandled(Singleton<InputManager>.Instance.PlayerControl.UICancel, "DialogPopup");
		if (cancelOption >= 0 && cancelOption < optionButtons.Count)
		{
			ExtendedButton extendedButton = optionButtons[cancelOption].ExtendedButton;
			if (extendedButton.gameObject.activeSelf)
			{
				extendedButton.onClick.Invoke();
			}
		}
		else if (cancelAction != null)
		{
			Hide();
			cancelAction();
		}
	}

	public void ShowLoadedContent()
	{
		foreach (PreviousState item in contentState)
		{
			item.GameObject.transform.SetParent(contentHolder);
			item.GameObject.SetActive(value: true);
		}
		if (window == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			window.Show();
		}
		HelperTools.NormalizePool(ref optionButtons, optionButtonPrefab, verticalOptionsHolder, optionButtons.Count);
		controllerArea.Enable();
	}

	public bool IsOpen()
	{
		if (window == null)
		{
			return base.gameObject.activeSelf;
		}
		return window.IsOpen;
	}

	public bool Escape()
	{
		if (allowHide)
		{
			return false;
		}
		Cancel();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
