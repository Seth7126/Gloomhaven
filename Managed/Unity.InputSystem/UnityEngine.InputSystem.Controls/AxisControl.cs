using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Processors;

namespace UnityEngine.InputSystem.Controls;

public class AxisControl : InputControl<float>
{
	public enum Clamp
	{
		None,
		BeforeNormalize,
		AfterNormalize,
		ToConstantBeforeNormalize
	}

	public Clamp clamp;

	public float clampMin;

	public float clampMax;

	public float clampConstant;

	public bool invert;

	public bool normalize;

	public float normalizeMin;

	public float normalizeMax;

	public float normalizeZero;

	public bool scale;

	public float scaleFactor;

	protected float Preprocess(float value)
	{
		if (scale)
		{
			value *= scaleFactor;
		}
		if (clamp == Clamp.ToConstantBeforeNormalize)
		{
			if (value < clampMin || value > clampMax)
			{
				value = clampConstant;
			}
		}
		else if (clamp == Clamp.BeforeNormalize)
		{
			value = Mathf.Clamp(value, clampMin, clampMax);
		}
		if (normalize)
		{
			value = NormalizeProcessor.Normalize(value, normalizeMin, normalizeMax, normalizeZero);
		}
		if (clamp == Clamp.AfterNormalize)
		{
			value = Mathf.Clamp(value, clampMin, clampMax);
		}
		if (invert)
		{
			value *= -1f;
		}
		return value;
	}

	private float Unpreprocess(float value)
	{
		if (invert)
		{
			value *= -1f;
		}
		if (normalize)
		{
			value = NormalizeProcessor.Denormalize(value, normalizeMin, normalizeMax, normalizeZero);
		}
		if (scale)
		{
			value /= scaleFactor;
		}
		return value;
	}

	public AxisControl()
	{
		m_StateBlock.format = InputStateBlock.FormatFloat;
	}

	protected override void FinishSetup()
	{
		base.FinishSetup();
		if (!base.hasDefaultState && normalize && Mathf.Abs(normalizeZero) > Mathf.Epsilon)
		{
			m_DefaultState = base.stateBlock.FloatToPrimitiveValue(normalizeZero);
		}
	}

	public unsafe override float ReadUnprocessedValueFromState(void* statePtr)
	{
		float value = base.stateBlock.ReadFloat(statePtr);
		return Preprocess(value);
	}

	public unsafe override void WriteValueIntoState(float value, void* statePtr)
	{
		value = Unpreprocess(value);
		base.stateBlock.WriteFloat(statePtr, value);
	}

	public unsafe override bool CompareValue(void* firstStatePtr, void* secondStatePtr)
	{
		float a = ReadValueFromState(firstStatePtr);
		float b = ReadValueFromState(secondStatePtr);
		return !Mathf.Approximately(a, b);
	}

	public unsafe override float EvaluateMagnitude(void* statePtr)
	{
		float num = ReadValueFromState(statePtr);
		if (m_MinValue.isEmpty || m_MaxValue.isEmpty)
		{
			return Mathf.Abs(num);
		}
		float num2 = m_MinValue.ToSingle();
		float max = m_MaxValue.ToSingle();
		num = Mathf.Clamp(num, num2, max);
		if (num2 < 0f)
		{
			if (num < 0f)
			{
				return NormalizeProcessor.Normalize(Mathf.Abs(num), 0f, Mathf.Abs(num2), 0f);
			}
			return NormalizeProcessor.Normalize(num, 0f, max, 0f);
		}
		return NormalizeProcessor.Normalize(num, num2, max, 0f);
	}
}
