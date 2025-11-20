using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

namespace UnityEngine.InputSystem.XR;

public class BoneControl : InputControl<Bone>
{
	[InputControl(offset = 0u, displayName = "parentBoneIndex")]
	public IntegerControl parentBoneIndex { get; private set; }

	[InputControl(offset = 4u, displayName = "Position")]
	public Vector3Control position { get; private set; }

	[InputControl(offset = 16u, displayName = "Rotation")]
	public QuaternionControl rotation { get; private set; }

	protected override void FinishSetup()
	{
		parentBoneIndex = GetChildControl<IntegerControl>("parentBoneIndex");
		position = GetChildControl<Vector3Control>("position");
		rotation = GetChildControl<QuaternionControl>("rotation");
		base.FinishSetup();
	}

	public unsafe override Bone ReadUnprocessedValueFromState(void* statePtr)
	{
		return new Bone
		{
			parentBoneIndex = (uint)parentBoneIndex.ReadUnprocessedValueFromState(statePtr),
			position = position.ReadUnprocessedValueFromState(statePtr),
			rotation = rotation.ReadUnprocessedValueFromState(statePtr)
		};
	}

	public unsafe override void WriteValueIntoState(Bone value, void* statePtr)
	{
		parentBoneIndex.WriteValueIntoState((int)value.parentBoneIndex, statePtr);
		position.WriteValueIntoState(value.position, statePtr);
		rotation.WriteValueIntoState(value.rotation, statePtr);
	}
}
