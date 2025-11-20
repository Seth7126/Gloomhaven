using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;

namespace Unity.XR.Oculus.Input;

[InputControlLayout(displayName = "Oculus Headset")]
public class OculusHMD : XRHMD
{
	[InputControl]
	[InputControl(name = "trackingState", layout = "Integer", aliases = new string[] { "devicetrackingstate" })]
	[InputControl(name = "isTracked", layout = "Button", aliases = new string[] { "deviceistracked" })]
	public ButtonControl userPresence { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control deviceAngularVelocity { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control deviceAcceleration { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control deviceAngularAcceleration { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control leftEyeAngularVelocity { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control leftEyeAcceleration { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control leftEyeAngularAcceleration { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control rightEyeAngularVelocity { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control rightEyeAcceleration { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control rightEyeAngularAcceleration { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control centerEyeAngularVelocity { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control centerEyeAcceleration { get; private set; }

	[InputControl(noisy = true)]
	public Vector3Control centerEyeAngularAcceleration { get; private set; }

	protected override void FinishSetup()
	{
		base.FinishSetup();
		userPresence = GetChildControl<ButtonControl>("userPresence");
		deviceAngularVelocity = GetChildControl<Vector3Control>("deviceAngularVelocity");
		deviceAcceleration = GetChildControl<Vector3Control>("deviceAcceleration");
		deviceAngularAcceleration = GetChildControl<Vector3Control>("deviceAngularAcceleration");
		leftEyeAngularVelocity = GetChildControl<Vector3Control>("leftEyeAngularVelocity");
		leftEyeAcceleration = GetChildControl<Vector3Control>("leftEyeAcceleration");
		leftEyeAngularAcceleration = GetChildControl<Vector3Control>("leftEyeAngularAcceleration");
		rightEyeAngularVelocity = GetChildControl<Vector3Control>("rightEyeAngularVelocity");
		rightEyeAcceleration = GetChildControl<Vector3Control>("rightEyeAcceleration");
		rightEyeAngularAcceleration = GetChildControl<Vector3Control>("rightEyeAngularAcceleration");
		centerEyeAngularVelocity = GetChildControl<Vector3Control>("centerEyeAngularVelocity");
		centerEyeAcceleration = GetChildControl<Vector3Control>("centerEyeAcceleration");
		centerEyeAngularAcceleration = GetChildControl<Vector3Control>("centerEyeAngularAcceleration");
	}
}
