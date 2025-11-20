using System;
using System.Collections.Generic;
using System.Text;
using AsmodeeNet.Utils.Extensions;
using Code.State;
using GLOOM;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmationBox : MonoBehaviour
{
	[SerializeField]
	private RectTransform pictureHolder;

	[SerializeField]
	private UIButtonExtended confirmButton;

	[SerializeField]
	private Hotkey congirmHotkey;

	[SerializeField]
	private Hotkey _reportHotkey;

	[SerializeField]
	private bool isPCVersion;

	[SerializeField]
	private TextLocalizedListener confirmButtonText;

	[SerializeField]
	private UIButtonExtended cancelButton;

	[SerializeField]
	private Hotkey cancelHotkey;

	[SerializeField]
	private TextLocalizedListener cancelButtonText;

	[SerializeField]
	private TextLocalizedListener headerText;

	[SerializeField]
	private GameObject header;

	[Header("PRIMARY CONTENT")]
	[SerializeField]
	private GameObject primaryContent;

	[SerializeField]
	private TMP_Text primaryContentText;

	[Header("SECONDARY CONTENT", order = 1)]
	[Header("Price content", order = 2)]
	[SerializeField]
	private GameObject secondaryContent;

	[SerializeField]
	private TMP_Text secondaryContentFirstText;

	[SerializeField]
	private TMP_Text secondaryContentSecondText;

	[Header("General secondary")]
	[SerializeField]
	private GameObject secondaryGeneralContent;

	[SerializeField]
	private TMP_Text secondaryGeneralContentText;

	[Header("Reporty")]
	[SerializeField]
	private GameObject reportContent;

	[SerializeField]
	private UIButtonExtended reportButton;

	private UIWindow confirmationBox;

	private bool usingAdventureStartConfirmation;

	private ControllerInputAreaLocal controllerArea;

	private INavigationOperation _onHideNavigation;

	private IState _uiState;

	private bool _resetAfterAction;

	private List<Action> _onConfirmActions = new List<Action>();

	private List<Action> _onCancelActions = new List<Action>();

	private List<Action> _onReportActions = new List<Action>();

	private Color HeaderTextColor => UIInfoTools.Instance.basicTextColor;

	private Color MessageTextColor => UIInfoTools.Instance.neutralActionTextColor;

	public bool IsOpen
	{
		get
		{
			if (confirmationBox != null)
			{
				return confirmationBox.IsOpen;
			}
			return false;
		}
	}

	public bool IsRequested { get; private set; }

	public event Action WindowShown;

	public event Action WindowHidden;

	private event Action onHidden;

	[UsedImplicitly]
	private void Start()
	{
		confirmationBox = GetComponent<UIWindow>();
		controllerArea = GetComponent<ControllerInputAreaLocal>();
		confirmationBox.onShown.AddListener(OnWindowShown);
		confirmationBox.onHidden.AddListener(OnWindowHidden);
		Action action = delegate
		{
			HideInternal();
		};
		if (InputManager.GamePadInUse)
		{
			_onReportActions.Add(action);
		}
		else
		{
			reportButton.onClick.AddListener(action.Invoke);
		}
		confirmationBox.IsPopUp = true;
		ResetConfirmationBox();
	}

	private void OnWindowShown()
	{
		this.WindowShown?.Invoke();
	}

	private void OnWindowHidden()
	{
		this.WindowHidden?.Invoke();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (InputManager.GamePadInUse && !isPCVersion)
		{
			DisableGamepadInput();
			if (congirmHotkey != null)
			{
				congirmHotkey.Deinitialize();
			}
			if (cancelHotkey != null)
			{
				cancelHotkey.Deinitialize();
			}
			if (_reportHotkey != null)
			{
				_reportHotkey.Deinitialize();
			}
		}
	}

	private void SetupReport(UnityAction onClicked)
	{
		reportContent.SetActive(value: true);
		Action action = delegate
		{
			ShowSendErrorWrapper("USER_REPORTS", delegate(bool hasSendReport)
			{
				reportButton.ClearSelectedState();
				if (hasSendReport)
				{
					onClicked?.Invoke();
					HideInternal();
				}
			});
		};
		if (InputManager.GamePadInUse)
		{
			_onReportActions.Add(action);
		}
		else
		{
			reportButton.onClick.AddListener(action.Invoke);
		}
	}

	public void ShowSendErrorWrapper(string errorMessageKey, UnityAction<bool> onFinished = null)
	{
		confirmationBox.Hide(instant: true);
		SceneController.Instance.GlobalErrorMessage.ShowSendErrorReport(errorMessageKey, onFinished);
	}

	public void ShowGenericCancelConfirmation(string title, string explanation, string buttonKey = "GUI_CANCEL", UnityAction onFinished = null, Color? titleColor = null, INavigationOperation onHideNavigation = null)
	{
		ShowGenericConfirmation(title, explanation, null, onFinished, titleColor, null, enableSoftlockReport: false, onHideNavigation);
		confirmButton.gameObject.SetActive(value: false);
		cancelButtonText.SetTextKey(buttonKey);
		header.SetActive(value: false);
	}

	public void ShowGenericConfirmation(string title, string explanation, UnityAction onActionConfirmed, Color? titleColor = null, Color? explanationColor = null, bool enableSoftlockReport = false, INavigationOperation onHideNavigation = null, bool resetAfterAction = false)
	{
		IsRequested = true;
		Singleton<UINavigation>.Instance.StateMachine.SwitchToState(_uiState);
		ResetConfirmationBox();
		_onHideNavigation = onHideNavigation;
		_resetAfterAction = resetAfterAction;
		if (InputManager.GamePadInUse && !isPCVersion)
		{
			congirmHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			cancelHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_reportHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		primaryContentText.text = title;
		primaryContentText.color = titleColor ?? HeaderTextColor;
		secondaryGeneralContentText.text = explanation;
		secondaryGeneralContentText.color = explanationColor ?? MessageTextColor;
		secondaryGeneralContent.SetActive(explanation != null);
		Action action = delegate
		{
			InputManager.SkipNextSubmitAction();
			HideInternal();
			onActionConfirmed?.Invoke();
		};
		Action action2 = delegate
		{
			HideInternal();
		};
		if (InputManager.GamePadInUse)
		{
			_onConfirmActions.Add(action);
			InitGamepadInput();
		}
		else
		{
			confirmButton.onClick.AddListener(action.Invoke);
			cancelButton.onClick.AddListener(action2.Invoke);
		}
		confirmationBox.Show();
	}

	private void InitGamepadInput()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnSubmit, null, null, isPersistent: true).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)).AddBlocker(new UIButtonExtendedActiveKeyActionHandlerBlocker(confirmButton)));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancel, null, null, isPersistent: true).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)).AddBlocker(new UIButtonExtendedActiveKeyActionHandlerBlocker(cancelButton)));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_MENU_ALT1, OnReport, null, null, isPersistent: true).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)).AddBlocker(new UIButtonExtendedActiveKeyActionHandlerBlocker(reportButton)));
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnSubmit);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancel);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_MENU_ALT1, OnReport);
		}
	}

	private void OnSubmit()
	{
		if (InputManager.GamePadInUse)
		{
			DisableGamepadInput();
		}
		foreach (Action item in new List<Action>(_onConfirmActions))
		{
			item?.Invoke();
		}
	}

	private void OnCancel()
	{
		if (InputManager.GamePadInUse)
		{
			DisableGamepadInput();
		}
		foreach (Action item in new List<Action>(_onCancelActions))
		{
			item?.Invoke();
		}
	}

	private void OnReport()
	{
		if (InputManager.GamePadInUse)
		{
			DisableGamepadInput();
		}
		foreach (Action item in new List<Action>(_onReportActions))
		{
			item?.Invoke();
		}
	}

	public void ShowGenericConfirmation(string title, string explanation, UnityAction onActionConfirmed, UnityAction onActionCancelled, string confirmButtonKey = null, string cancelButtonKey = null, Color? titleColor = null, bool showHeader = true, bool enableSoftlockReport = false, INavigationOperation onHideNavigation = null, bool resetAfterAction = false, string headerKey = null)
	{
		ShowGenericConfirmation(title, explanation, onActionConfirmed, onActionCancelled, titleColor, null, enableSoftlockReport, onHideNavigation, resetAfterAction);
		if (confirmButtonKey.IsNOTNullOrEmpty())
		{
			confirmButtonText.SetTextKey(confirmButtonKey);
		}
		if (cancelButtonKey.IsNOTNullOrEmpty())
		{
			cancelButtonText.SetTextKey(cancelButtonKey);
		}
		header.SetActive(showHeader);
		if (headerKey.IsNOTNullOrEmpty())
		{
			headerText.SetTextKey(headerKey);
		}
	}

	private void ShowGenericConfirmation(string title, string explanation, UnityAction onActionConfirmed, UnityAction onActionCancelled, Color? titleColor = null, Color? explanationColor = null, bool enableSoftlockReport = false, INavigationOperation onHideNavigation = null, bool resetAfterAction = false)
	{
		ShowGenericConfirmation(title, explanation, delegate
		{
			this.onHidden = null;
			onActionConfirmed?.Invoke();
		}, titleColor, explanationColor, enableSoftlockReport, onHideNavigation, resetAfterAction);
		if (onActionCancelled != null)
		{
			Action action = delegate
			{
				this.onHidden = null;
				onActionCancelled();
			};
			if (InputManager.GamePadInUse)
			{
				_onCancelActions.Add(action);
			}
			else
			{
				cancelButton.onClick.AddListener(action.Invoke);
			}
			this.onHidden = onActionCancelled.Invoke;
		}
	}

	public void ShowGenericSpendConfirmation(string title, int price, UnityAction onActionConfirmed, Color? titleColor = null, UnityAction onActionCancelled = null, INavigationOperation onHideNavigation = null, bool resetAfterAction = false)
	{
		IsRequested = true;
		Singleton<UINavigation>.Instance.StateMachine.SwitchToState(_uiState);
		ResetConfirmationBox();
		_onHideNavigation = onHideNavigation;
		_resetAfterAction = resetAfterAction;
		primaryContentText.text = title;
		primaryContentText.color = titleColor ?? HeaderTextColor;
		secondaryContentFirstText.text = LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST");
		secondaryContentSecondText.text = price.ToString();
		secondaryContent.SetActive(value: true);
		Action action = delegate
		{
			this.onHidden = null;
			HideInternal();
			onActionConfirmed?.Invoke();
		};
		Action action2 = delegate
		{
			HideInternal();
		};
		if (InputManager.GamePadInUse)
		{
			_onConfirmActions.Add(action);
			_onCancelActions.Add(action2);
		}
		else
		{
			confirmButton.onClick.AddListener(action.Invoke);
			cancelButton.onClick.AddListener(action2.Invoke);
		}
		if (onActionCancelled != null)
		{
			this.onHidden = onActionCancelled.Invoke;
		}
		confirmationBox.Show();
		if (InputManager.GamePadInUse)
		{
			InitGamepadInput();
		}
	}

	private void HideInternal()
	{
		confirmationBox.Hide(instant: true);
		if (_resetAfterAction)
		{
			ResetConfirmationBox();
		}
	}

	public void ShowCancelActiveAbility(string abilityName, bool lost, UnityAction onActionConfirmed, string explanation = null, UnityAction onCancelled = null, Color? explanationColor = null, INavigationOperation onHideNavigation = null)
	{
		StringBuilder stringBuilder = new StringBuilder(string.Format(LocalizationManager.GetTranslation(lost ? "GUI_CONFIRMATION_CANCEL_ABILITY" : "GUI_CONFIRMATION_CANCEL_DISCARD_ABILITY"), abilityName));
		if (explanation.IsNOTNullOrEmpty())
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("<color=#{0}>{1}</color>", MessageTextColor.ToHex(), explanation);
		}
		ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CANCEL") + " " + abilityName, stringBuilder.ToString(), onActionConfirmed, onCancelled, null, explanationColor, enableSoftlockReport: false, onHideNavigation);
	}

	public void ResetConfirmationBox()
	{
		cancelButtonText.SetTextKey("NO");
		confirmButtonText.SetTextKey("YES");
		headerText.SetTextKey("GUI_CONFIRMATION");
		primaryContent.SetActive(value: true);
		secondaryGeneralContent.SetActive(value: false);
		secondaryContent.SetActive(value: false);
		reportContent.SetActive(value: false);
		if (InputManager.GamePadInUse)
		{
			_onConfirmActions.Clear();
			_onCancelActions.Clear();
			_onReportActions.Clear();
		}
		else
		{
			confirmButton.onClick.RemoveAllListeners();
			cancelButton.onClick.RemoveAllListeners();
			reportButton.onClick.RemoveAllListeners();
		}
		confirmationBox.onHidden.RemoveAllListeners();
		confirmationBox.onHidden.AddListener(OnHidden);
		confirmationBox.onShown.RemoveAllListeners();
		confirmationBox.onShown.AddListener(OnShown);
		confirmButton.gameObject.SetActive(value: true);
		header.SetActive(value: true);
		this.onHidden = null;
		_onHideNavigation = null;
		if (InputManager.GamePadInUse && !isPCVersion)
		{
			congirmHotkey.Deinitialize();
			cancelHotkey.Deinitialize();
			_reportHotkey.Deinitialize();
		}
	}

	private void OnShown()
	{
		controllerArea.Enable();
	}

	public void OnHidden()
	{
		IsRequested = false;
		controllerArea.Destroy();
		if (usingAdventureStartConfirmation)
		{
			usingAdventureStartConfirmation = false;
		}
		this.onHidden?.Invoke();
		_onHideNavigation?.ExecuteForDefaultMachine();
		Singleton<UINavigation>.Instance.StateMachine.RemoveLast(_uiState);
	}

	public void Hide()
	{
		HideInternal();
	}

	public void SetUiState(IState uiState)
	{
		_uiState = uiState;
	}
}
