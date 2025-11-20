using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Switch.LowLevel;

namespace UnityEngine.InputSystem.Switch;

[InputControlLayout(stateType = typeof(SwitchProControllerHIDInputState), displayName = "Switch Pro Controller")]
public class SwitchProControllerHID : Gamepad
{
}
