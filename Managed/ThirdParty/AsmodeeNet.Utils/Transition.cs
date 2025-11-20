using System;

namespace AsmodeeNet.Utils;

[Serializable]
public class Transition
{
	public ActionState ActionStateStart { get; set; }

	public ActionState ActionStateEnd { get; set; }

	public Func<bool> Condition { get; set; }

	public TransitionType TransitionType { get; set; }

	public float TransitionDuration { get; set; }

	public Transition(ActionState actionStateStart, ActionState actionStateEnd, Func<bool> condition, TransitionType transitionType)
	{
		ActionStateStart = actionStateStart;
		ActionStateEnd = actionStateEnd;
		Condition = condition;
		TransitionType = transitionType;
	}

	public Transition(ActionState actionStateStart, ActionState actionStateEnd, float transitionDuration, TransitionType transitionType)
	{
		ActionStateStart = actionStateStart;
		ActionStateEnd = actionStateEnd;
		TransitionDuration = transitionDuration;
		TransitionType = transitionType;
	}
}
