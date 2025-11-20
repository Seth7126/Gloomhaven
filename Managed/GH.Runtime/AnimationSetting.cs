using System;
using UnityEngine;

[Serializable]
public struct AnimationSetting
{
	public RectTransform Transform;

	public TweenAction Animation;

	public Vector3 ToValue;

	public float ToUniqueValue;

	[ConditionalField("UseSpecificFromValue", "true", true)]
	public float FromUniqueValue;

	public bool UseSpecificFromValue;

	public float Duration;

	public LeanTweenType AnimationType;

	public AnimationSetting(RectTransform transform, TweenAction animation, Vector3 toValue, float duration, LeanTweenType animationType)
	{
		Transform = transform;
		ToValue = toValue;
		Duration = duration;
		AnimationType = animationType;
		Animation = animation;
		ToUniqueValue = 0f;
		FromUniqueValue = 0f;
		UseSpecificFromValue = false;
	}

	private AnimationSetting(RectTransform transform, TweenAction animation, float toUniqueValue, float fromUniqueValue, bool useSpecificFromValue, float duration, LeanTweenType animationType)
	{
		Transform = transform;
		ToValue = Vector3.zero;
		Duration = duration;
		AnimationType = animationType;
		Animation = animation;
		ToUniqueValue = toUniqueValue;
		FromUniqueValue = fromUniqueValue;
		UseSpecificFromValue = useSpecificFromValue;
	}

	public AnimationSetting(RectTransform transform, TweenAction animation, float toUniqueValue, float duration, LeanTweenType animationType)
		: this(transform, animation, toUniqueValue, 0f, useSpecificFromValue: false, duration, animationType)
	{
	}

	public AnimationSetting(RectTransform transform, TweenAction animation, float toUniqueValue, float fromUniqueValue, float duration, LeanTweenType animationType)
		: this(transform, animation, toUniqueValue, fromUniqueValue, useSpecificFromValue: true, duration, animationType)
	{
	}
}
