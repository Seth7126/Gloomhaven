using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

namespace Script.GUI;

public class PanelHotkeyContainer : ControllerInputElement
{
	[SerializeField]
	private List<Hotkey> _hotkeys = new List<Hotkey>();

	[SerializeField]
	private bool _useEnableDisableCallbacks = true;

	private Dictionary<Hotkey, Action> _hotkeyActions = new Dictionary<Hotkey, Action>();

	[UsedImplicitly]
	private void OnDestroy()
	{
		foreach (Hotkey hotkey in _hotkeys)
		{
			hotkey.Deinitialize();
		}
		_hotkeyActions.Clear();
	}

	protected override void OnEnabledControllerControl()
	{
		if (!_useEnableDisableCallbacks)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		foreach (Hotkey hotkey in _hotkeys)
		{
			hotkey.Initialize(Singleton<UINavigation>.Instance.Input, null, _hotkeyActions.ContainsKey(hotkey) ? _hotkeyActions[hotkey] : null);
		}
	}

	protected override void OnDisabledControllerControl()
	{
		if (!_useEnableDisableCallbacks)
		{
			return;
		}
		foreach (Hotkey hotkey in _hotkeys)
		{
			hotkey.Deinitialize();
		}
		base.gameObject.SetActive(value: false);
	}

	public IDictionary<Hotkey, Action> GetHotkeysActions()
	{
		return new Dictionary<Hotkey, Action>(_hotkeyActions);
	}

	public void SetHotkeyAction(string keyName, Action action)
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		Hotkey hotkey = _hotkeys.Find((Hotkey item) => item.ExpectedEvent == keyName);
		if (hotkey != null)
		{
			if (hotkey.gameObject.activeInHierarchy)
			{
				hotkey.Deinitialize();
				hotkey.Initialize(Singleton<UINavigation>.Instance.Input, null, action);
			}
			_hotkeyActions[hotkey] = action;
		}
	}

	public void SetHotkeysAction(IEnumerable<HotkeyAction> hotkeyActions)
	{
		if (hotkeyActions == null)
		{
			return;
		}
		foreach (HotkeyAction hotkeyAction in hotkeyActions)
		{
			SetHotkeyAction(hotkeyAction.KeyName, hotkeyAction.Action);
		}
	}

	public void ToggleActiveAllHotkeys(bool value, bool ignoreActiveInHierarchy = false)
	{
		foreach (Hotkey hotkey in _hotkeys)
		{
			SetActiveHotkey(hotkey.ExpectedEvent, value, ignoreActiveInHierarchy);
		}
	}

	public void SetActiveHotkey(string keyName, bool value, bool ignoreActiveInHierarchy = false)
	{
		if (!InputManager.GamePadInUse || !(base.gameObject.activeInHierarchy || ignoreActiveInHierarchy))
		{
			return;
		}
		Hotkey hotkey = _hotkeys.Find((Hotkey item) => item.ExpectedEvent == keyName);
		if (hotkey != null && hotkey.gameObject.activeSelf != value)
		{
			if (value)
			{
				hotkey.gameObject.SetActive(value: true);
				hotkey.Initialize(Singleton<UINavigation>.Instance.Input, null, _hotkeyActions.ContainsKey(hotkey) ? _hotkeyActions[hotkey] : null);
			}
			else
			{
				hotkey.Deinitialize();
				hotkey.gameObject.SetActive(value: false);
			}
		}
	}

	public void SetActive(bool value)
	{
		if (InputManager.GamePadInUse && base.gameObject.activeInHierarchy != value)
		{
			if (value)
			{
				OnEnabledControllerControl();
			}
			else
			{
				OnDisabledControllerControl();
			}
		}
	}
}
