using UnityEngine.InputSystem.LowLevel;

namespace UnityEngine.InputSystem.Controls;

public class ButtonControl : AxisControl
{
	public float pressPoint = -1f;

	internal static float s_GlobalDefaultButtonPressPoint;

	internal static float s_GlobalDefaultButtonReleaseThreshold;

	internal const float kMinButtonPressPoint = 0.0001f;

	public float pressPointOrDefault
	{
		get
		{
			if (!(pressPoint > 0f))
			{
				return s_GlobalDefaultButtonPressPoint;
			}
			return pressPoint;
		}
	}

	public bool isPressed => IsValueConsideredPressed(ReadValue());

	public bool wasPressedThisFrame
	{
		get
		{
			if (base.device.wasUpdatedThisFrame && IsValueConsideredPressed(ReadValue()))
			{
				return !IsValueConsideredPressed(ReadValueFromPreviousFrame());
			}
			return false;
		}
	}

	public bool wasReleasedThisFrame
	{
		get
		{
			if (base.device.wasUpdatedThisFrame && !IsValueConsideredPressed(ReadValue()))
			{
				return IsValueConsideredPressed(ReadValueFromPreviousFrame());
			}
			return false;
		}
	}

	public ButtonControl()
	{
		m_StateBlock.format = InputStateBlock.FormatBit;
		m_MinValue = 0f;
		m_MaxValue = 1f;
	}

	public bool IsValueConsideredPressed(float value)
	{
		return value >= pressPointOrDefault;
	}
}
