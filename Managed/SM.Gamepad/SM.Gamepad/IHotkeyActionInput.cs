using System;

namespace SM.Gamepad;

public interface IHotkeyActionInput
{
	InputDisplayData.InputDeviceType CurrentDeviceType { get; set; }

	event Action<string> OnInputEvent;

	event Action<InputDisplayData.InputDeviceType> OnInputDeviceTypeChanged;
}
