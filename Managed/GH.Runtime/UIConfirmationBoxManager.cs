using System;
using Code.State;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.Events;

public class UIConfirmationBoxManager : Singleton<UIConfirmationBoxManager>
{
	[SerializeField]
	private ConfirmationBox pcConfirmationBox;

	[SerializeField]
	private ConfirmationBox gamepadConfirmationBox;

	[SerializeField]
	private bool _mainMenuManager;

	public bool IsOpen => CurrentBox.IsOpen;

	public bool IsRequested => CurrentBox.IsRequested;

	public static UIConfirmationBoxManager MainMenuInstance { get; private set; }

	private ConfirmationBox CurrentBox
	{
		get
		{
			if (!InputManager.GamePadInUse)
			{
				return pcConfirmationBox;
			}
			return gamepadConfirmationBox;
		}
	}

	private Color WarningTextColor => UIInfoTools.Instance.negativeTextColor;

	public event Action BoxWindowShown;

	public event Action BoxWindowHidden;

	private void Start()
	{
		IState state;
		if (_mainMenuManager)
		{
			MainMenuInstance = this;
			state = Singleton<UINavigation>.Instance.StateMachine.GetState(MainStateTag.MessageConfirmationEscMenu);
		}
		else
		{
			SetInstance(this);
			state = Singleton<UINavigation>.Instance.StateMachine.GetState(PopupStateTag.ConfirmationBox);
		}
		pcConfirmationBox.SetUiState(state);
		gamepadConfirmationBox.SetUiState(state);
		SubscribeOnBoxesEvents();
	}

	private void SubscribeOnBoxesEvents()
	{
		pcConfirmationBox.WindowHidden += OnWindowBoxHidden;
		gamepadConfirmationBox.WindowHidden += OnWindowBoxHidden;
		pcConfirmationBox.WindowShown += OnWindowBoxShown;
		gamepadConfirmationBox.WindowShown += OnWindowBoxShown;
	}

	public void ShowGenericCancelConfirmation(string title, string explanation, string buttonKey = "GUI_CANCEL", UnityAction onFinished = null, INavigationOperation onHideNavigation = null)
	{
		CurrentBox.ShowGenericCancelConfirmation(title, explanation, buttonKey, onFinished, WarningTextColor, onHideNavigation);
	}

	public void ShowGenericWarningConfirmation(string title, string explanation, UnityAction onActionConfirmed, UnityAction onActionCancelled = null, string confirmButtonKey = null, string cancelButtonKey = null, bool showHeader = true, bool enableSoftlockReport = false, INavigationOperation onHideNavigation = null, bool resetAfterAction = false)
	{
		ShowGenericConfirmation(title, explanation, onActionConfirmed, onActionCancelled, confirmButtonKey, cancelButtonKey, WarningTextColor, showHeader, enableSoftlockReport, onHideNavigation, resetAfterAction);
	}

	public void ShowGenericConfirmation(string title, string explanation, UnityAction onActionConfirmed, UnityAction onActionCancelled = null, string confirmButtonKey = null, string cancelButtonKey = null, Color? titleColor = null, bool showHeader = true, bool enableSoftlockReport = false, INavigationOperation onHideNavigation = null, bool resetAfterAction = false, string headerKey = null)
	{
		CurrentBox.ShowGenericConfirmation(title, explanation, onActionConfirmed, onActionCancelled, confirmButtonKey, cancelButtonKey, titleColor, showHeader, enableSoftlockReport, onHideNavigation, resetAfterAction, headerKey);
	}

	public void ShowGenericSpendConfirmation(string title, int price, UnityAction onActionConfirmed, UnityAction onActionCancelled = null, INavigationOperation onHideNavigation = null)
	{
		CurrentBox.ShowGenericSpendConfirmation(title, price, onActionConfirmed, null, onActionCancelled, onHideNavigation);
	}

	public void ShowCancelActiveAbility(string abilityName, bool lost, UnityAction onActionConfirmed, string explanation = null, UnityAction onCancelled = null, INavigationOperation onHideNavigation = null)
	{
		CurrentBox.ShowCancelActiveAbility(abilityName, lost, onActionConfirmed, explanation, onCancelled, WarningTextColor, onHideNavigation);
	}

	public void Hide()
	{
		CurrentBox.Hide();
	}

	public void ResetConfirmationBox()
	{
		CurrentBox.ResetConfirmationBox();
	}

	private void OnWindowBoxShown()
	{
		this.BoxWindowShown?.Invoke();
	}

	private void OnWindowBoxHidden()
	{
		this.BoxWindowHidden?.Invoke();
	}
}
