using UnityEngine;

public class UICampaignRewardRevealAnimator : CustomLeanTweenAnimator
{
	[SerializeField]
	private RectTransform revealRect;

	[SerializeField]
	private float widthToExpand;

	public override void SetFinalValue()
	{
		revealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (revealRect.parent as RectTransform).rect.width);
	}

	public override void RestorOriginalValue()
	{
		revealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (revealRect.parent as RectTransform).rect.width - widthToExpand);
	}

	public override LTDescr BuildTweenAction(float duration)
	{
		return LeanTween.value(revealRect.gameObject, delegate(float w)
		{
			revealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
		}, (revealRect.parent as RectTransform).rect.width - widthToExpand, (revealRect.parent as RectTransform).rect.width, duration);
	}
}
