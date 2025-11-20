using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

namespace Unity.XR.Oculus.Input;

[InputControlLayout(displayName = "Oculus Remote")]
public class OculusRemote : InputDevice
{
	[InputControl]
	public ButtonControl back { get; private set; }

	[InputControl]
	public ButtonControl start { get; private set; }

	[InputControl]
	public Vector2Control touchpad { get; private set; }

	protected override void FinishSetup()
	{
		base.FinishSetup();
		back = GetChildControl<ButtonControl>("back");
		start = GetChildControl<ButtonControl>("start");
		touchpad = GetChildControl<Vector2Control>("touchpad");
	}
}
