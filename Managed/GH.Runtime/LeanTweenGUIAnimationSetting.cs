using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class LeanTweenGUIAnimationSetting
{
	[TextArea]
	[SerializeField]
	public string Information;

	[Header("Easing")]
	public LeanTweenType AnimationType;

	[ConditionalField("AnimationType", LeanTweenType.animationCurve, true)]
	[SerializeField]
	private AnimationCurve animationCurve;

	[Header("Times")]
	public float Duration;

	public float Delay;

	[Space]
	public UnityEvent OnTweenStart;

	public LTDescr BuildTween()
	{
		LTDescr lTDescr = BuildTweenAction()?.setDelay(Delay);
		lTDescr?.setOnStart(OnTweenStart.Invoke);
		if (AnimationType == LeanTweenType.animationCurve)
		{
			return lTDescr?.setEase(animationCurve);
		}
		return lTDescr?.setEase(AnimationType);
	}

	protected abstract LTDescr BuildTweenAction();

	public abstract void RestorOriginalValue();

	public abstract void SetFinalValue();

	public virtual void Initialize()
	{
	}

	protected LeanTweenGUIAnimationSetting(float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
	{
		Duration = duration;
		Delay = delay;
		AnimationType = easing;
	}
}
[Serializable]
public abstract class LeanTweenGuiAnimationSetting<T, T2> : LeanTweenGUIAnimationSetting where T2 : Component
{
	[Header("Object to animate")]
	public T2 Target;

	[Header("Values")]
	public T OriginalValue;

	public T ToValue;

	public override void RestorOriginalValue()
	{
		SetValue(OriginalValue);
	}

	public override void SetFinalValue()
	{
		SetValue(ToValue);
	}

	protected abstract void SetValue(T value);

	protected LeanTweenGuiAnimationSetting(T2 target, float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(duration, delay, easing)
	{
		Target = target;
	}

	public override string ToString()
	{
		return $"{Target.name} ({base.ToString()} from {OriginalValue} to {ToValue})";
	}
}
[Serializable]
public abstract class LeanTweenGuiAnimationSetting<T, T2, T3> : LeanTweenGuiAnimationSetting<T, T3> where T3 : Component
{
	[Header("Animation type")]
	public T2 Animation;

	protected LeanTweenGuiAnimationSetting(T3 target, T2 type, float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(target, duration, delay, easing)
	{
		Animation = type;
	}

	public override string ToString()
	{
		return $"{Target.name} ({Animation} from {OriginalValue} to {ToValue})";
	}
}
