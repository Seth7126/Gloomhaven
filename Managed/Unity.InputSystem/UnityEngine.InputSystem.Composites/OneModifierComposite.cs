using System;
using System.ComponentModel;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.Composites;

[DisplayStringFormat("{modifier}+{binding}")]
[DisplayName("Binding With One Modifier")]
public class OneModifierComposite : InputBindingComposite
{
	[InputControl(layout = "Button")]
	public int modifier;

	[InputControl]
	public int binding;

	private int m_ValueSizeInBytes;

	private Type m_ValueType;

	public override Type valueType => m_ValueType;

	public override int valueSizeInBytes => m_ValueSizeInBytes;

	public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
	{
		if (context.ReadValueAsButton(modifier))
		{
			return context.EvaluateMagnitude(binding);
		}
		return 0f;
	}

	public unsafe override void ReadValue(ref InputBindingCompositeContext context, void* buffer, int bufferSize)
	{
		if (context.ReadValueAsButton(modifier))
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
		DetermineValueTypeAndSize(ref context, binding, out m_ValueType, out m_ValueSizeInBytes);
	}

	public override object ReadValueAsObject(ref InputBindingCompositeContext context)
	{
		if (context.ReadValueAsButton(modifier))
		{
			return context.ReadValueAsObject(binding);
		}
		return null;
	}

	internal static void DetermineValueTypeAndSize(ref InputBindingCompositeContext context, int part, out Type valueType, out int valueSizeInBytes)
	{
		valueSizeInBytes = 0;
		Type type = null;
		foreach (InputBindingCompositeContext.PartBinding control in context.controls)
		{
			if (control.part == part)
			{
				Type type2 = control.control.valueType;
				if (type == null || type2.IsAssignableFrom(type))
				{
					type = type2;
				}
				else if (!type.IsAssignableFrom(type2))
				{
					type = typeof(Object);
				}
				valueSizeInBytes = Math.Max(control.control.valueSizeInBytes, valueSizeInBytes);
			}
		}
		valueType = type;
	}
}
