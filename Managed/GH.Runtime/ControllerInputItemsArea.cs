using System;
using Code.State;
using GLOOM;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using Script.GUI.SMNavigation.Tabs;
using UnityEngine;

public class ControllerInputItemsArea : ControllerInputEscapableArea
{
	[SerializeField]
	private Hotkey _previousHotkey;

	[SerializeField]
	private Hotkey _nextHotkey;

	[SerializeField]
	private UINavigationTabComponent _tab;

	[SerializeField]
	private TabComponentInputListener _tabInput;

	private string previousTipTitle;

	private Func<string> previousTipText;

	private IState _enterState;

	private readonly IStateFilter _stateFilter = new StateFilterByType(typeof(AnimationScenarioState)).InverseFilter();

	public IState EnterState => _enterState;

	private void Awake()
	{
		if (InputManager.GamePadInUse)
		{
			_tabInput.OnNext += OnTabChanged;
			_tabInput.OnPrevious += OnTabChanged;
			_tabInput.SetTabChangeConditions(CanChangeTab, CanChangeTab);
		}
	}

	protected override void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			_tabInput.OnNext -= OnTabChanged;
			_tabInput.OnPrevious -= OnTabChanged;
			SetActiveTabInput(value: false);
		}
		base.OnDestroy();
	}

	private void OnTabChanged()
	{
		if (!base.IsFocused)
		{
			Focus();
		}
	}

	public override void EnableGroup(bool isFocused)
	{
		base.EnableGroup(isFocused);
		RefreshAvailable();
	}

	public void RefreshAvailable()
	{
		if (isEnabled)
		{
			RefreshElements();
			if (!_tab.ContainsElements)
			{
				Unfocus();
				DisableAvailable();
			}
			else
			{
				EnableAvailableElement();
			}
		}
	}

	private void EnableAvailableElement()
	{
		SetActiveTabInput(value: true);
	}

	private void DisableAvailable()
	{
		SetActiveTabInput(value: false);
	}

	public override void DisableGroup()
	{
		base.DisableGroup();
		DisableAvailable();
		if (ControllerInputAreaManager.IsFocusedArea(base.Id))
		{
			ControllerInputAreaManager.Instance.ResetFocusedArea();
		}
	}

	private bool CanChangeTab()
	{
		if (!base.IsFocused && !ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.WorldMap))
		{
			return ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.Damage);
		}
		return true;
	}

	private void RefreshElements()
	{
		_tab.Initialize();
	}

	public void SetActiveTabInput(bool value)
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		if (value)
		{
			if (_tab.ContainsElements)
			{
				_tabInput.Register();
				_previousHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
				_nextHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			}
		}
		else
		{
			_tabInput.UnRegister();
			_previousHotkey.Deinitialize();
			_previousHotkey.DisplayHotkey(active: false);
			_nextHotkey.Deinitialize();
			_nextHotkey.DisplayHotkey(active: false);
		}
	}

	public override void Focus()
	{
		if (base.IsFocused)
		{
			return;
		}
		if (!_tab.ContainsElements)
		{
			RefreshElements();
		}
		if (!_tab.ContainsElements)
		{
			return;
		}
		if (!Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<SelectElementItemState>() && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<ItemActionsScenarioState>() && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<SelectItemState>())
		{
			_enterState = Singleton<UINavigation>.Instance.StateMachine.CurrentState;
		}
		if (Singleton<UIUseItemsBar>.Instance.IsCurrentItemReadyToUse())
		{
			previousTipText = null;
			previousTipTitle = null;
			Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.ItemActions);
			return;
		}
		base.Focus();
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectItem);
		_tab.First();
		previousTipText = Singleton<HelpBox>.Instance.ControllerText;
		previousTipTitle = Singleton<HelpBox>.Instance.ControllerTitle;
		InitiativeTrack.Instance.helpBox.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_SELECT_ITEMS"), Singleton<InputManager>.Instance.GetGamepadActionIcon(KeyAction.UI_SUBMIT)));
	}

	public override void Unfocus()
	{
		if (base.IsFocused)
		{
			if (previousTipText != null)
			{
				Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(previousTipText, previousTipTitle);
			}
			else
			{
				InitiativeTrack.Instance.helpBox.ClearOverrideController();
			}
			_tab.RemoveCurrent();
		}
		previousTipText = null;
		previousTipTitle = null;
		base.Unfocus();
	}

	public override bool Escape()
	{
		if (base.IsFocused)
		{
			Unfocus();
			if (_enterState != null)
			{
				if (_enterState.GetType() != typeof(AnimationScenarioState))
				{
					Singleton<UINavigation>.Instance.StateMachine.SwitchToState(_enterState);
				}
				else
				{
					Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(_stateFilter);
				}
			}
			OnEscape?.Invoke();
			return true;
		}
		return false;
	}
}
