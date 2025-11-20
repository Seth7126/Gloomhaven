using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITextMeshProMaterialAnimator : CustomLeanTweenAnimator
{
	[SerializeField]
	private List<TextMeshProUGUI> texts;

	[SerializeField]
	private Material originalMaterial;

	[SerializeField]
	private Material finalMaterial;

	private Material mat;

	private void Awake()
	{
		mat = new Material(originalMaterial);
	}

	public override void SetFinalValue()
	{
		foreach (TextMeshProUGUI text in texts)
		{
			text.fontMaterial = finalMaterial;
		}
	}

	public override void RestorOriginalValue()
	{
		foreach (TextMeshProUGUI text in texts)
		{
			text.fontMaterial = originalMaterial;
		}
	}

	public override LTDescr BuildTweenAction(float duration)
	{
		return LeanTween.value(texts[0].gameObject, delegate(float val)
		{
			mat.Lerp(originalMaterial, finalMaterial, val);
			foreach (TextMeshProUGUI text in texts)
			{
				text.fontMaterial = mat;
			}
		}, 0f, 1f, duration);
	}
}
