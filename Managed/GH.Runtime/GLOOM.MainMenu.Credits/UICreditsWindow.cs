using System;
using System.Collections.Generic;
using System.Linq;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu.Credits;

public class UICreditsWindow : UISubmenuGOWindow
{
	[SerializeField]
	private RectTransform scrollContainer;

	[SerializeField]
	private float scrollDuration = 10f;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private Vector2 backgroundMaxDisplacement;

	[SerializeField]
	private Image backgroundHolder;

	[SerializeField]
	private AnimationCurve backgroundAnimCurve;

	[SerializeField]
	private float backgroundAnimDuration;

	[SerializeField]
	private List<ReferenceToSprite> _referencesToBackgrounds;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	private LTSeq animationSeq;

	private LTDescr scrollAnim;

	protected override void Awake()
	{
		base.Awake();
		showAnimator.OnAnimationFinished.AddListener(PlayCredits);
		exitButton.onClick.AddListener(base.Hide);
		window.onHidden.AddListener(StopAnimation);
	}

	protected override void OnDestroy()
	{
		exitButton.onClick.RemoveListener(base.Hide);
		StopAnimation();
		base.OnDestroy();
	}

	[ContextMenu("Play")]
	public override void Show()
	{
		StopAnimation();
		backgroundHolder.SetAlpha(0f);
		scrollContainer.anchoredPosition = new Vector2(scrollContainer.anchoredPosition.x, 0f);
		base.Show();
	}

	private void PlayCredits()
	{
		ScrollCredits(base.Hide);
		ShowBackgroundImages();
	}

	private void ScrollCredits(Action onFinished)
	{
		float num = ((RectTransform)scrollContainer.parent.transform).rect.height + 10f;
		scrollAnim = LeanTween.value(scrollContainer.gameObject, delegate(float val)
		{
			scrollContainer.anchoredPosition = new Vector2(0f, val);
		}, 0f, scrollContainer.sizeDelta.y + num, scrollDuration);
		scrollAnim.setOnComplete((Action)delegate
		{
			scrollAnim = null;
			onFinished?.Invoke();
		});
	}

	private void ShowBackgroundImages()
	{
		animationSeq = LeanTween.sequence();
		System.Random random = new System.Random();
		foreach (ReferenceToSprite reference in _referencesToBackgrounds.OrderBy((ReferenceToSprite it) => random.Next()))
		{
			animationSeq.append(delegate
			{
				_imageSpriteLoader.AddReferenceToSpriteForImage(backgroundHolder, reference);
				backgroundHolder.rectTransform.anchoredPosition = Vector2.zero;
			});
			animationSeq.append(BuildShowImageTween(new Vector2(LimitRandomDirection(random.NextDouble()), LimitRandomDirection(random.NextDouble()))));
		}
		animationSeq.append(ShowBackgroundImages);
	}

	private float LimitRandomDirection(double num)
	{
		float num2 = (float)num * 2f - 1f;
		if (Math.Abs(num2) < 0.3f)
		{
			return num2 + 0.3f * (float)Math.Sign(num2);
		}
		return num2;
	}

	private LTDescr BuildShowImageTween(Vector2 moveDirection)
	{
		Vector2 displacement = moveDirection * backgroundMaxDisplacement;
		return LeanTween.value(backgroundHolder.gameObject, delegate(float progress)
		{
			backgroundHolder.SetAlpha(backgroundAnimCurve.Evaluate(progress));
			backgroundHolder.rectTransform.anchoredPosition = displacement * progress;
		}, 0f, 1f, backgroundAnimDuration);
	}

	private void StopAnimation()
	{
		StopAllCoroutines();
		if (animationSeq != null)
		{
			LeanTween.cancel(animationSeq.id);
		}
		animationSeq = null;
		if (scrollAnim != null)
		{
			LeanTween.cancel(scrollAnim.id);
		}
		scrollAnim = null;
	}

	protected override void OnCompleteHidden()
	{
		base.OnCompleteHidden();
		StopAnimation();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		StopAnimation();
	}
}
