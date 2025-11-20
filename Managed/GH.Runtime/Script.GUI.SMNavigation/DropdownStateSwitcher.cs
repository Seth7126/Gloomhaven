using System;
using Code.State;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;

namespace Script.GUI.SMNavigation;

[RequireComponent(typeof(ExtendedDropdown))]
public class DropdownStateSwitcher : MonoBehaviour
{
	private UiNavigationRoot _navigationRoot;

	private ExtendedDropdown _dropdown;

	private UiNavigationRoot _root;

	private MainStateData _stateData;

	private StateMachine _stateMachine;

	[UsedImplicitly]
	private void Awake()
	{
		_dropdown = GetComponent<ExtendedDropdown>();
		_navigationRoot = GetComponent<UiNavigationRoot>();
		_stateData = new MainStateData(_navigationRoot);
		ExtendedDropdown dropdown = _dropdown;
		dropdown.OnShow = (Action)Delegate.Combine(dropdown.OnShow, new Action(OnShow));
		ExtendedDropdown dropdown2 = _dropdown;
		dropdown2.OnHide = (Action)Delegate.Combine(dropdown2.OnHide, new Action(OnHide));
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		ExtendedDropdown dropdown = _dropdown;
		dropdown.OnShow = (Action)Delegate.Remove(dropdown.OnShow, new Action(OnShow));
		ExtendedDropdown dropdown2 = _dropdown;
		dropdown2.OnHide = (Action)Delegate.Remove(dropdown2.OnHide, new Action(OnHide));
	}

	private void OnShow()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.DropdownOptionSelect, _stateData);
		UINavigationSelectable component = _dropdown.CurrentSelectable.GetComponent<UINavigationSelectable>();
		Singleton<UINavigation>.Instance.NavigationManager.TrySelect(component);
	}

	private void OnHide()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.DisplaySettingsWithSelected);
	}
}
