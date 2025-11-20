using System;
using System.Collections.Generic;
using UnityEngine;

public class UIRetirementShowAnimation : LeanTweenGUIAnimator
{
	[SerializeField]
	private List<CanvasGroup> rows;

	[SerializeField]
	private float duration;

	[SerializeField]
	private float moveInXDistance;

	[SerializeField]
	private float delayBetweenRows;

	protected override void Animate()
	{
		for (int i = 0; i < rows.Count; i++)
		{
			CanvasGroup row = rows[i];
			LTDescr tween = LeanTween.value(row.gameObject, delegate(float val)
			{
				row.alpha = val;
				RectTransform rectTransform = row.transform as RectTransform;
				rectTransform.anchoredPosition = new Vector2((1f - val) * moveInXDistance, rectTransform.anchoredPosition.y);
			}, 0f, 1f, duration).setDelay(delayBetweenRows * (float)i).setIgnoreTimeScale(ignoreScaleTime);
			if (i == rows.Count - 1)
			{
				tween.setOnComplete((Action)delegate
				{
					animationsInProcess.Remove(tween);
					base.Animate();
				});
			}
			else
			{
				tween.setOnComplete((Action)delegate
				{
					animationsInProcess.Remove(tween);
				});
			}
			animationsInProcess.Add(tween);
		}
	}

	protected override void ResetFinishState()
	{
		foreach (CanvasGroup row in rows)
		{
			row.alpha = 1f;
			((RectTransform)row.transform).anchoredPosition = new Vector2(0f, ((RectTransform)row.transform).anchoredPosition.y);
		}
		base.ResetFinishState();
	}

	protected override void ResetInitialState()
	{
		foreach (CanvasGroup row in rows)
		{
			row.alpha = 0f;
			((RectTransform)row.transform).anchoredPosition = new Vector2(moveInXDistance, ((RectTransform)row.transform).anchoredPosition.y);
		}
		base.ResetInitialState();
	}

	private void OnDisable()
	{
		Stop();
	}
}
