using System;
using ScenarioRuleLibrary;
using UnityEngine;

public class UILevelUpCardHighlighter : MonoBehaviour
{
	[SerializeField]
	private UILevelUpCardHolder cardHolder;

	[SerializeField]
	private GameObject cardHighlightEffect;

	[SerializeField]
	private GUIAnimator animator;

	private void Awake()
	{
		cardHighlightEffect.SetActive(value: false);
	}

	public void HighlightWonCard(CAbilityCard card, Action onFinishAnimation)
	{
		cardHighlightEffect.SetActive(value: true);
		animator.Play();
		cardHolder.PlaceNewCard(card, immediately: false, onFinishAnimation);
	}

	public void HighlightCard(CAbilityCard card, bool showEffect)
	{
		cardHighlightEffect.SetActive(showEffect);
		cardHolder.PlaceNewCard(card, immediately: true);
	}

	public void UnhighlightCard()
	{
		cardHolder.Hide();
		cardHighlightEffect.gameObject.SetActive(value: false);
	}

	public void UnhighlightWonCard(Action onFinishedAnimation = null)
	{
		cardHolder.Empty(onFinishedAnimation);
		cardHighlightEffect.SetActive(value: false);
	}

	private void OnDisable()
	{
		cardHolder.Hide();
	}
}
