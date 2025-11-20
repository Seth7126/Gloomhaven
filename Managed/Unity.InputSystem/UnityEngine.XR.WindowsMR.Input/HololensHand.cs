using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;

namespace UnityEngine.XR.WindowsMR.Input;

[InputControlLayout(displayName = "HoloLens Hand", commonUsages = new string[] { "LeftHand", "RightHand" })]
public class HololensHand : XRController
{
	[InputControl(noisy = true, aliases = new string[] { "gripVelocity" })]
	public Vector3Control deviceVelocity { get; private set; }

	[InputControl(aliases = new string[] { "triggerbutton" })]
	public ButtonControl airTap { get; private set; }

	[InputControl(noisy = true)]
	public AxisControl sourceLossRisk { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control sourceLossMitigationDirection { get; private set; }

	protected override void FinishSetup()
	{
		base.FinishSetup();
		airTap = GetChildControl<ButtonControl>("airTap");
		deviceVelocity = GetChildControl<Vector3Control>("deviceVelocity");
		sourceLossRisk = GetChildControl<AxisControl>("sourceLossRisk");
		sourceLossMitigationDirection = GetChildControl<Vector3Control>("sourceLossMitigationDirection");
	}
}
