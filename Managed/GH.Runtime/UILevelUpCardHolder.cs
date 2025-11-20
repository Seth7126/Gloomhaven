using System;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.Utils;
using UnityEngine;

public class UILevelUpCardHolder : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private float fadeDuration = 0.5f;

	private AbilityCardUI card;

	private LTDescr animationFade;

	public AbilityCardUI Card => card;

	private void OnDestroy()
	{
		CancelAnimation();
	}

	private void OnEnable()
	{
		canvasGroup.alpha = 0f;
	}

	public void PlaceNewCard(CAbilityCard cardToHold, bool immediately = false, Action onShown = null)
	{
		_ = card;
		if (card != null)
		{
			ObjectPool.RecycleCard(card.CardID, ObjectPool.ECardType.Ability, card.gameObject);
			card = null;
		}
		card = ObjectPool.SpawnCard(cardToHold.ID, ObjectPool.ECardType.Ability, base.transform, resetLocalScale: true, resetToMiddle: true).GetComponent<AbilityCardUI>();
		card.Init(cardToHold, disableEventDetection: true);
		card.fullAbilityCard.UpdateView(UISettings.DefaultFullCardViewSettings);
		card.Show();
		CancelAnimation();
		if (immediately)
		{
			canvasGroup.alpha = 1f;
			onShown?.Invoke();
			return;
		}
		animationFade = LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration).setOnComplete((Action)delegate
		{
			animationFade = null;
			onShown?.Invoke();
		});
	}

	public void Empty(Action onHide = null)
	{
		CancelAnimation();
		animationFade = LeanTween.alphaCanvas(canvasGroup, 0f, fadeDuration).setOnComplete((Action)delegate
		{
			animationFade = null;
			Hide();
			onHide?.Invoke();
		});
	}

	public void Hide()
	{
		CancelAnimation();
		canvasGroup.alpha = 0f;
		if (card != null)
		{
			ObjectPool.RecycleCard(card.CardID, ObjectPool.ECardType.Ability, card.gameObject);
			card = null;
		}
	}

	private void CancelAnimation()
	{
		if (animationFade != null)
		{
			LeanTween.cancel(animationFade.id);
			animationFade = null;
		}
	}
}
