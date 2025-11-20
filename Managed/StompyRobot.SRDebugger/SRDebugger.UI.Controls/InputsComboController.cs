using System.Collections.Generic;
using SRDebugger.Gamepad;
using UnityEngine;
using UnityEngine.Events;

namespace SRDebugger.UI.Controls;

public class InputsComboController : MonoBehaviour
{
	private string _combo;

	private float _maxInputDelay;

	private List<IGamepadButton> _buttonsCombo;

	private int _nextComboButtonIndex;

	private float _lastComboButtonPressedTime;

	public UnityEvent ComboActivatedEvent;

	private bool _comboParsed;

	private bool _comboIsValid;

	private IGamepadButton NextComboButton => _buttonsCombo[_nextComboButtonIndex];

	private bool TryParseCombo(out bool comboIsValid)
	{
		comboIsValid = false;
		_buttonsCombo = ParseCombo(_combo);
		if (_buttonsCombo.Count > 1)
		{
			comboIsValid = true;
		}
		return true;
	}

	private void Update()
	{
		IGamepad gamepad = SRDebug.Instance.Gamepad;
		if (gamepad == null || !gamepad.IsValid())
		{
			return;
		}
		if (!_comboParsed)
		{
			_combo = Settings.Instance.InputCombo;
			_maxInputDelay = Settings.Instance.ComboWindow;
			_comboParsed = TryParseCombo(out _comboIsValid);
			_nextComboButtonIndex = 0;
		}
		else
		{
			if (!_comboIsValid)
			{
				return;
			}
			if (Time.time - _lastComboButtonPressedTime >= _maxInputDelay)
			{
				_nextComboButtonIndex = 0;
			}
			if (NextComboButton.IsWasPressed())
			{
				_lastComboButtonPressedTime = Time.time;
				_nextComboButtonIndex++;
				if (_nextComboButtonIndex == _buttonsCombo.Count)
				{
					_nextComboButtonIndex = 0;
					ComboActivatedEvent.Invoke();
				}
			}
		}
	}

	private List<IGamepadButton> ParseCombo(string combo)
	{
		string[] array = combo.Replace(" ", string.Empty).Split(',', '/');
		List<IGamepadButton> list = new List<IGamepadButton>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (NameToButtonControl(text, out var buttonControl))
			{
				list.Add(buttonControl);
			}
			else
			{
				Debug.LogError("[InputsComboController] Invalid button name: " + text + "; It will be skipped");
			}
		}
		return list;
	}

	private bool NameToButtonControl(string buttonName, out IGamepadButton buttonControl)
	{
		buttonControl = null;
		IGamepad gamepad = SRDebug.Instance.Gamepad;
		if (gamepad == null || !gamepad.IsValid())
		{
			Debug.LogError("There is no Gamepad");
			return false;
		}
		buttonControl = gamepad.GetGamepadButton(buttonName);
		if (buttonControl != null)
		{
			return true;
		}
		return false;
	}
}
