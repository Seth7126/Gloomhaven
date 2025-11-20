using System;
using System.Collections.Generic;
using System.Linq;
using SM.Gamepad;
using Script.GUI;
using UnityEngine;

public class ShopInventoryPanelHotkeyContainerProxy : MonoBehaviour
{
	private PanelHotkeyContainer _currentHotkeyContainer;

	private PanelHotkeyContainer _defaultHotkeyContainer;

	public void ResetToDefaultContainer()
	{
		SetCurrentHotkeyContainer(_defaultHotkeyContainer);
	}

	public void SetCurrentHotkeyContainer(PanelHotkeyContainer newHotkeyContainer)
	{
		if (_currentHotkeyContainer == newHotkeyContainer)
		{
			return;
		}
		if (_currentHotkeyContainer != null)
		{
			IDictionary<Hotkey, Action> hotkeysActions = _currentHotkeyContainer.GetHotkeysActions();
			newHotkeyContainer.ToggleActiveAllHotkeys(value: false, ignoreActiveInHierarchy: true);
			newHotkeyContainer.SetHotkeysAction(hotkeysActions.Select((KeyValuePair<Hotkey, Action> x) => new HotkeyAction(x.Key.ExpectedEvent, x.Value)));
			foreach (KeyValuePair<Hotkey, Action> item in hotkeysActions)
			{
				newHotkeyContainer.SetActiveHotkey(item.Key.ExpectedEvent, item.Key.gameObject.activeSelf);
			}
			_currentHotkeyContainer.ToggleActiveAllHotkeys(value: false, ignoreActiveInHierarchy: true);
		}
		_currentHotkeyContainer = newHotkeyContainer;
	}

	public void SetHotkeysAction(IEnumerable<HotkeyAction> hotkeyActions)
	{
		if (_currentHotkeyContainer != null)
		{
			_currentHotkeyContainer.SetHotkeysAction(hotkeyActions);
		}
	}

	public void ToggleActiveAllHotkeys(bool isActive)
	{
		_currentHotkeyContainer.ToggleActiveAllHotkeys(isActive);
	}

	public void SetActiveHotkey(string keyName, bool value)
	{
		_currentHotkeyContainer.SetActiveHotkey(keyName, value, ignoreActiveInHierarchy: true);
	}

	public void SetDefaultHotkeyContainer(PanelHotkeyContainer shopInventoryPanelHotkeyContainer)
	{
		_defaultHotkeyContainer = shopInventoryPanelHotkeyContainer;
	}
}
