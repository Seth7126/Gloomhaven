using System;
using UnityEngine;

[Serializable]
public class LeanTweenGuiAnimationSettingScale : LeanTweenGuiAnimationSetting<Vector3, RectTransform>
{
	protected override LTDescr BuildTweenAction()
	{
		return LeanTween.scale(Target, ToValue, Duration);
	}

	protected override void SetValue(Vector3 value)
	{
		Target.localScale = value;
	}

	public LeanTweenGuiAnimationSettingScale(RectTransform target, float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(target, duration, delay, easing)
	{
	}
}
