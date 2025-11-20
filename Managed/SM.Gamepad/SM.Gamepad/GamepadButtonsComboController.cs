#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SM.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace SM.Gamepad;

public class GamepadButtonsComboController
{
	private class ComboButton
	{
		private readonly GamepadButton _buttonType;

		private readonly ButtonControl _buttonControl;

		public GamepadButton ButtonType => _buttonType;

		public ButtonControl ButtonControl => _buttonControl;

		public ComboButton(GamepadButton buttonType, ButtonControl buttonControl)
		{
			_buttonType = buttonType;
			_buttonControl = buttonControl;
		}
	}

	private class Combo
	{
		private readonly List<ComboButton> _buttons;

		private readonly Action _onPressCallback;

		private bool _enabled;

		public bool Enabled
		{
			set
			{
				_enabled = value;
				if (value)
				{
					IsPressed = GetIsPressed();
				}
			}
		}

		public List<ComboButton> Buttons => _buttons;

		public bool IsPressed { get; private set; }

		public Combo(List<ComboButton> buttons, Action onPressCallback)
		{
			_buttons = buttons;
			_onPressCallback = onPressCallback;
		}

		public void UpdateInput()
		{
			if (!_enabled)
			{
				IsPressed = false;
			}
			else if (GetIsPressed())
			{
				if (!IsPressed)
				{
					IsPressed = true;
					_onPressCallback?.Invoke();
				}
			}
			else if (IsPressed)
			{
				IsPressed = false;
			}
		}

		private bool GetIsPressed()
		{
			foreach (ComboButton button in _buttons)
			{
				if (!button.ButtonControl.isPressed)
				{
					return false;
				}
			}
			return true;
		}
	}

	private class ComboPack : IEnumerable<KeyValuePair<string, Combo>>, IEnumerable
	{
		private readonly Dictionary<string, Combo> _combos = new Dictionary<string, Combo>();

		public bool Contains(string comboString)
		{
			return _combos.ContainsKey(comboString);
		}

		public void Add(string comboString, Combo combo)
		{
			_combos.Add(comboString, combo);
		}

		public void Remove(string comboString)
		{
			_combos.Remove(comboString);
		}

		public void SetEnabledCombo(string comboString, bool value)
		{
			if (_combos.ContainsKey(comboString))
			{
				_combos[comboString].Enabled = value;
			}
		}

		public IEnumerator<KeyValuePair<string, Combo>> GetEnumerator()
		{
			return _combos.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private Dictionary<UnityEngine.InputSystem.Gamepad, ComboPack> _gamepadComboPacks = new Dictionary<UnityEngine.InputSystem.Gamepad, ComboPack>();

	private Dictionary<string, Action<UnityEngine.InputSystem.Gamepad>> _registeredCombos = new Dictionary<string, Action<UnityEngine.InputSystem.Gamepad>>();

	private Dictionary<List<ButtonControl>, Action<UnityEngine.InputSystem.Gamepad>> _onComboCallbacks;

	private Dictionary<string, float> _triggeredTimeMap = new Dictionary<string, float>();

	private float _triggeringCooldown = 0.1f;

	private List<UnityEngine.InputSystem.Gamepad> _toRemoveBuffer = new List<UnityEngine.InputSystem.Gamepad>();

	private List<GamepadButton> _lastPressedComboButtons = new List<GamepadButton>();

	public float TriggeringCooldown
	{
		get
		{
			return _triggeringCooldown;
		}
		set
		{
			_triggeringCooldown = value;
		}
	}

	public List<GamepadButton> LastPressedComboButtons => _lastPressedComboButtons;

	public void AddCombo(string comboString, Action<UnityEngine.InputSystem.Gamepad> callback)
	{
		if (_registeredCombos.ContainsKey(comboString))
		{
			LogUtils.LogWarning("[GamepadButtonsComboController] Combo \"" + comboString + "\" already registered");
			return;
		}
		_registeredCombos.Add(comboString, callback);
		_triggeredTimeMap.Add(comboString, float.MinValue);
		foreach (KeyValuePair<UnityEngine.InputSystem.Gamepad, ComboPack> gamepadComboPack in _gamepadComboPacks)
		{
			UnityEngine.InputSystem.Gamepad key = gamepadComboPack.Key;
			ComboPack value = gamepadComboPack.Value;
			if (value.Contains(comboString))
			{
				LogUtils.LogWarning("[GamepadButtonsComboController] Some GamepadComboPack contains unregistered combo \"" + comboString + "\";");
			}
			else
			{
				value.Add(comboString, ParseCombo(key, comboString));
			}
		}
	}

	public void SetEnabledCombo(string comboString, bool value)
	{
		if (!_registeredCombos.ContainsKey(comboString))
		{
			LogUtils.LogWarning("[GamepadButtonsComboController] Combo \"" + comboString + "\" wasn't registered");
			return;
		}
		foreach (KeyValuePair<UnityEngine.InputSystem.Gamepad, ComboPack> gamepadComboPack in _gamepadComboPacks)
		{
			ComboPack value2 = gamepadComboPack.Value;
			if (!value2.Contains(comboString))
			{
				LogUtils.LogWarning("[GamepadButtonsComboController] Some GamepadComboPack doesn't contain registered combo \"" + comboString + "\";");
			}
			else
			{
				value2.SetEnabledCombo(comboString, value);
			}
		}
	}

	public void RemoveCombo(string comboString)
	{
		if (!_registeredCombos.ContainsKey(comboString))
		{
			LogUtils.LogWarning("[GamepadButtonsComboController] Combo \"" + comboString + "\" wasn't registered");
			return;
		}
		foreach (KeyValuePair<UnityEngine.InputSystem.Gamepad, ComboPack> gamepadComboPack in _gamepadComboPacks)
		{
			ComboPack value = gamepadComboPack.Value;
			if (!value.Contains(comboString))
			{
				LogUtils.LogWarning("[GamepadButtonsComboController] Some GamepadComboPack doesn't contain registered combo \"" + comboString + "\";");
			}
			else
			{
				value.Remove(comboString);
			}
		}
		_triggeredTimeMap.Remove(comboString);
		_registeredCombos.Remove(comboString);
	}

	public void Update()
	{
		ResetLastPressedCombos();
		RegisterNewGamepads();
		UnregisterInactiveGamepads();
		CheckGamepadsForCombos();
	}

	private void RegisterNewGamepads()
	{
		foreach (UnityEngine.InputSystem.Gamepad item in UnityEngine.InputSystem.Gamepad.all)
		{
			if (_gamepadComboPacks.ContainsKey(item))
			{
				continue;
			}
			ComboPack comboPack = new ComboPack();
			foreach (string key in _registeredCombos.Keys)
			{
				comboPack.Add(key, ParseCombo(item, key));
			}
			_gamepadComboPacks.Add(item, comboPack);
		}
	}

	private void UnregisterInactiveGamepads()
	{
		_toRemoveBuffer.Clear();
		foreach (KeyValuePair<UnityEngine.InputSystem.Gamepad, ComboPack> gamepadComboPack in _gamepadComboPacks)
		{
			UnityEngine.InputSystem.Gamepad key = gamepadComboPack.Key;
			if (!UnityEngine.InputSystem.Gamepad.all.Contains(key))
			{
				_toRemoveBuffer.Add(key);
			}
		}
		foreach (UnityEngine.InputSystem.Gamepad item in _toRemoveBuffer)
		{
			_gamepadComboPacks.Remove(item);
		}
		_toRemoveBuffer.Clear();
	}

	private void CheckGamepadsForCombos()
	{
		foreach (KeyValuePair<UnityEngine.InputSystem.Gamepad, ComboPack> gamepadComboPack in _gamepadComboPacks)
		{
			_ = gamepadComboPack.Key;
			foreach (KeyValuePair<string, Combo> item in gamepadComboPack.Value)
			{
				string key = item.Key;
				Combo value = item.Value;
				if (Time.time - _triggeredTimeMap[key] < _triggeringCooldown)
				{
					continue;
				}
				value.UpdateInput();
				if (!value.IsPressed)
				{
					continue;
				}
				_triggeredTimeMap[key] = Time.time;
				foreach (ComboButton button in value.Buttons)
				{
					_lastPressedComboButtons.Add(button.ButtonType);
				}
			}
		}
	}

	private void OnComboPressed(string comboString, UnityEngine.InputSystem.Gamepad gamepad)
	{
		_registeredCombos[comboString]?.Invoke(gamepad);
	}

	private Combo ParseCombo(UnityEngine.InputSystem.Gamepad forGamepad, string comboString)
	{
		List<ComboButton> list = new List<ComboButton>();
		if (forGamepad != null)
		{
			string[] array = comboString.Replace(" ", string.Empty).Split('+');
			foreach (string text in array)
			{
				if (Utility.TryGetButtonTypeByName(forGamepad, text, out var gamepadButtonType, out var button))
				{
					list.Add(new ComboButton(gamepadButtonType, button));
				}
				else
				{
					LogUtils.LogError("[GamepadButtonsComboController] Unknown button name: " + text + "; It will be skipped");
				}
			}
		}
		else
		{
			LogUtils.LogError("[GamepadButtonsComboController] There is no gamepad to parse combo");
		}
		return new Combo(list, delegate
		{
			OnComboPressed(comboString, forGamepad);
		});
	}

	private void ResetLastPressedCombos()
	{
		_lastPressedComboButtons.Clear();
	}
}
