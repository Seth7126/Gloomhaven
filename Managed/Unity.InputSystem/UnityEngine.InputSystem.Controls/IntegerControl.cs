using UnityEngine.InputSystem.LowLevel;

namespace UnityEngine.InputSystem.Controls;

public class IntegerControl : InputControl<int>
{
	public IntegerControl()
	{
		m_StateBlock.format = InputStateBlock.FormatInt;
	}

	public unsafe override int ReadUnprocessedValueFromState(void* statePtr)
	{
		return m_StateBlock.ReadInt(statePtr);
	}

	public unsafe override void WriteValueIntoState(int value, void* statePtr)
	{
		m_StateBlock.WriteInt(statePtr, value);
	}
}
