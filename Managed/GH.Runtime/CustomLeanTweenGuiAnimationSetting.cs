using System;
using UnityEngine;

[Serializable]
public class CustomLeanTweenGuiAnimationSetting : LeanTweenGUIAnimationSetting
{
	[SerializeField]
	private CustomLeanTweenAnimator animation;

	public CustomLeanTweenGuiAnimationSetting(float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(duration, delay, easing)
	{
	}

	protected override LTDescr BuildTweenAction()
	{
		return animation.BuildTweenAction(Duration);
	}

	public override void RestorOriginalValue()
	{
		animation.RestorOriginalValue();
	}

	public override void SetFinalValue()
	{
		animation.SetFinalValue();
	}
}
