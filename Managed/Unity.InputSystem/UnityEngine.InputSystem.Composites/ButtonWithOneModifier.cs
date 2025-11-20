using System.ComponentModel;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.Composites;

[DesignTimeVisible(false)]
[DisplayStringFormat("{modifier}+{button}")]
public class ButtonWithOneModifier : InputBindingComposite<float>
{
	[InputControl(layout = "Button")]
	public int modifier;

	[InputControl(layout = "Button")]
	public int button;

	public override float ReadValue(ref InputBindingCompositeContext context)
	{
		if (context.ReadValueAsButton(modifier))
		{
			return context.ReadValue<float>(button);
		}
		return 0f;
	}

	public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
	{
		return ReadValue(ref context);
	}
}
