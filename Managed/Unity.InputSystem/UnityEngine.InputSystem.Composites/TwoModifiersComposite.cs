using System;
using System.ComponentModel;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.Composites;

[DisplayStringFormat("{modifier1}+{modifier2}+{binding}")]
[DisplayName("Binding With Two Modifiers")]
public class TwoModifiersComposite : InputBindingComposite
{
	[InputControl(layout = "Button")]
	public int modifier1;

	[InputControl(layout = "Button")]
	public int modifier2;

	[InputControl(layout = "Button")]
	public int binding;

	private int m_ValueSizeInBytes;

	private Type m_ValueType;

	public override Type valueType => m_ValueType;

	public override int valueSizeInBytes => m_ValueSizeInBytes;

	public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
	{
		if (context.ReadValueAsButton(modifier1) && context.ReadValueAsButton(modifier2))
		{
			return context.EvaluateMagnitude(binding);
		}
		return 0f;
	}

	public unsafe override void ReadValue(ref InputBindingCompositeContext context, void* buffer, int bufferSize)
	{
		if (context.ReadValueAsButton(modifier1) && context.ReadValueAsButton(modifier2))
		{
			context.ReadValue(binding, buffer, bufferSize);
		}
		else
		{
			UnsafeUtility.MemClear(buffer, m_ValueSizeInBytes);
		}
	}

	protected override void FinishSetup(ref InputBindingCompositeContext context)
	{
		OneModifierComposite.DetermineValueTypeAndSize(ref context, binding, out m_ValueType, out m_ValueSizeInBytes);
	}

	public override object ReadValueAsObject(ref InputBindingCompositeContext context)
	{
		if (context.ReadValueAsButton(modifier1) && context.ReadValueAsButton(modifier2))
		{
			return context.ReadValueAsObject(binding);
		}
		return null;
	}
}
