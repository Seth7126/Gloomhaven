using UnityEngine;
using UnityEngine.UI;

public class GUIImageFillAnimator : CustomLeanTweenAnimator
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private float OriginalValue;

	[SerializeField]
	private float ToValue;

	public override void SetFinalValue()
	{
		image.fillAmount = ToValue;
	}

	public override void RestorOriginalValue()
	{
		image.fillAmount = OriginalValue;
	}

	public override LTDescr BuildTweenAction(float duration)
	{
		return LeanTween.value(image.gameObject, delegate(float val)
		{
			image.fillAmount = val;
		}, image.fillAmount, ToValue, duration);
	}
}
