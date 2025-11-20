using UnityEngine;

namespace InControl;

public readonly struct InputVectorEventData
{
	public Vector2 InputVector { get; }

	public MoveActionSourceType SourceType { get; }

	public InputVectorEventData(MoveActionSourceType sourceType, Vector2 inputVector)
	{
		SourceType = sourceType;
		InputVector = inputVector;
	}
}
