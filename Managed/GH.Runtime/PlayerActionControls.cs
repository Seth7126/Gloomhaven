using System;
using System.Collections.Generic;
using InControl;
using SM.Gamepad;
using UnityEngine.InputSystem.LowLevel;

public class PlayerActionControls : IDisposable
{
	private readonly GamepadButtonsComboController _gamepadButtonsComboController;

	private Dictionary<PlayerAction, PlayerActionBindingSources> _actionBindings = new Dictionary<PlayerAction, PlayerActionBindingSources>();

	private DevicePlayerActionControlsProvider _deviceControlsProvider;

	private float _gamepadComboHandleDuration = 0.2f;

	public DevicePlayerActionControlsProvider DeviceControlsProvider => _deviceControlsProvider;

	public PlayerActionControls(PlayerActionSet actionSet, GamepadButtonsComboController gamepadButtonsComboController)
	{
		_gamepadButtonsComboController = gamepadButtonsComboController;
		foreach (PlayerAction action in actionSet.Actions)
		{
			_actionBindings.TryAdd(action, new PlayerActionBindingSources(action));
		}
		_deviceControlsProvider = new DevicePlayerActionControlsProvider(this);
	}

	public void Dispose()
	{
		_actionBindings.Clear();
		_actionBindings = null;
	}

	public PlayerActionBindingSources GetPlayerActionBindings(PlayerAction action)
	{
		if (!_actionBindings.ContainsKey(action))
		{
			return null;
		}
		return _actionBindings[action];
	}

	public bool TryGetPlayerActionBindings(PlayerAction action, out PlayerActionBindingSources bindings)
	{
		return _actionBindings.TryGetValue(action, out bindings);
	}

	public void UpdateHandledComboControls()
	{
		foreach (GamepadButton lastPressedComboButton in _gamepadButtonsComboController.LastPressedComboButtons)
		{
			_deviceControlsProvider.MarkControlAsHandled(lastPressedComboButton.ToInputControlType(), _gamepadComboHandleDuration);
		}
	}

	public bool ControlIsHandled(PlayerAction action)
	{
		if (_actionBindings.ContainsKey(action) && InputManager.GamePadInUse)
		{
			return _deviceControlsProvider.ControlIsHandled(_actionBindings[action]);
		}
		return false;
	}
}
