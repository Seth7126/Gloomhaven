using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

namespace Unity.XR.OpenVR;

[InputControlLayout(displayName = "Handed Vive Tracker", commonUsages = new string[] { "LeftHand", "RightHand" })]
public class HandedViveTracker : ViveTracker
{
	[InputControl]
	public AxisControl grip { get; private set; }

	[InputControl]
	public ButtonControl gripPressed { get; private set; }

	[InputControl]
	public ButtonControl primary { get; private set; }

	[InputControl(aliases = new string[] { "JoystickOrPadPressed" })]
	public ButtonControl trackpadPressed { get; private set; }

	[InputControl]
	public ButtonControl triggerPressed { get; private set; }

	protected override void FinishSetup()
	{
		grip = GetChildControl<AxisControl>("grip");
		primary = GetChildControl<ButtonControl>("primary");
		gripPressed = GetChildControl<ButtonControl>("gripPressed");
		trackpadPressed = GetChildControl<ButtonControl>("trackpadPressed");
		triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
		base.FinishSetup();
	}
}
