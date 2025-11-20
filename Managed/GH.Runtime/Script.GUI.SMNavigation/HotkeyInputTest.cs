#define ENABLE_LOGS
using SM.Gamepad;

namespace Script.GUI.SMNavigation;

public class HotkeyInputTest
{
	private readonly IHotkeyActionInput _input;

	public HotkeyInputTest(IHotkeyActionInput input)
	{
		_input = input;
		AddListeners();
		LogDeviceType(input.CurrentDeviceType);
	}

	public void Destroy()
	{
		RemoveListeners();
	}

	private void AddListeners()
	{
		_input.OnInputDeviceTypeChanged += LogDeviceType;
		_input.OnInputEvent += LogEvent;
	}

	private void RemoveListeners()
	{
		_input.OnInputDeviceTypeChanged -= LogDeviceType;
		_input.OnInputEvent -= LogEvent;
	}

	private void LogDeviceType(InputDisplayData.InputDeviceType deviceType)
	{
		Log($"Current Device type {deviceType}");
	}

	private void LogEvent(string evt)
	{
		Log("Hotkey Event '" + evt + "'");
	}

	private void Log(string text)
	{
		Debug.Log("[HotkeyInputTest] " + text);
	}
}
