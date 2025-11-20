using System;
using UnityEngine;

[RequireComponent(typeof(AbilityCardUI))]
public class CardHighlight : MonoBehaviour
{
	[SerializeField]
	private float duration = 1f;

	[SerializeField]
	private Vector3 scale = new Vector3(1.5f, 1.5f, 1f);

	[SerializeField]
	private bool autostart = true;

	private MiniAbilityCard highlight;

	private LTDescr scaleAnimation;

	private LTDescr fadeAnimation;

	private bool isHighlighted;

	private void Start()
	{
		if (autostart)
		{
			Highlight();
		}
	}

	[ContextMenu("Highlight")]
	private void Highlight()
	{
		isHighlighted = true;
		AbilityCardUI component = GetComponent<AbilityCardUI>();
		highlight = component.MiniAbilityCard.GenerateCopy(base.transform);
		highlight.SetDisplayMode(CardPileType.Round, null, isHighlighted: true);
		RectTransform obj = highlight.transform as RectTransform;
		Vector2 vector = (obj.pivot = new Vector2(0.5f, 0.5f));
		Vector2 anchorMax = (obj.anchorMin = vector);
		obj.anchorMax = anchorMax;
		obj.anchoredPosition = Vector2.up;
		Canvas canvas = highlight.gameObject.AddComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingOrder = 1;
		highlight.transform.localScale = Vector3.one;
		scaleAnimation = LeanTween.scale(highlight.gameObject, scale, duration).setOnComplete((Action)delegate
		{
			scaleAnimation = null;
		});
		CanvasGroup component2 = highlight.GetComponent<CanvasGroup>();
		component2.alpha = 1f;
		fadeAnimation = LeanTween.alphaCanvas(component2, 0f, duration).setOnComplete(Unhighlight);
	}

	[ContextMenu("Unhighlight")]
	private void Unhighlight()
	{
		if (isHighlighted)
		{
			isHighlighted = false;
			CancelLeanTween();
			UnityEngine.Object.Destroy(this);
		}
	}

	private void CancelLeanTween()
	{
		if (fadeAnimation != null)
		{
			LeanTween.cancel(fadeAnimation.id);
			fadeAnimation = null;
		}
		if (scaleAnimation != null)
		{
			LeanTween.cancel(scaleAnimation.id);
			scaleAnimation = null;
		}
	}

	private void OnDisable()
	{
		Unhighlight();
	}

	private void OnDestroy()
	{
		CancelLeanTween();
		if (highlight != null)
		{
			UnityEngine.Object.Destroy(highlight.gameObject);
		}
	}
}
