using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace UnityEngine.InputSystem.XR;

[Preserve]
[InputControlLayout(stateType = typeof(PoseState))]
public class PoseControl : InputControl<PoseState>
{
	public ButtonControl isTracked { get; private set; }

	public IntegerControl trackingState { get; private set; }

	public Vector3Control position { get; private set; }

	public QuaternionControl rotation { get; private set; }

	public Vector3Control velocity { get; private set; }

	public Vector3Control angularVelocity { get; private set; }

	public PoseControl()
	{
		m_StateBlock.format = new FourCC('P', 'o', 's', 'e');
	}

	protected override void FinishSetup()
	{
		isTracked = GetChildControl<ButtonControl>("isTracked");
		trackingState = GetChildControl<IntegerControl>("trackingState");
		position = GetChildControl<Vector3Control>("position");
		rotation = GetChildControl<QuaternionControl>("rotation");
		velocity = GetChildControl<Vector3Control>("velocity");
		angularVelocity = GetChildControl<Vector3Control>("angularVelocity");
		base.FinishSetup();
	}

	public unsafe override PoseState ReadUnprocessedValueFromState(void* statePtr)
	{
		PoseState* ptr = (PoseState*)((byte*)statePtr + (int)m_StateBlock.byteOffset);
		return *ptr;
	}

	public unsafe override void WriteValueIntoState(PoseState value, void* statePtr)
	{
		PoseState* destination = (PoseState*)((byte*)statePtr + (int)m_StateBlock.byteOffset);
		UnsafeUtility.MemCpy(destination, UnsafeUtility.AddressOf(ref value), UnsafeUtility.SizeOf<PoseState>());
	}
}
