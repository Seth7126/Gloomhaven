using AsmodeeNet.Foundation;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIEnhancementCardHighlighter : MonoBehaviour
{
	[SerializeField]
	private UILevelUpCardHolder cardHolder;

	[SerializeField]
	private GameObject cardHighlightEffect;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private float alphaInvalidCard = 0.5f;

	[SerializeField]
	private GUIAnimator animator;

	public AbilityCardUI Card => cardHolder.Card;

	private void Awake()
	{
		cardHighlightEffect.SetActive(value: false);
	}

	public void ShowCard(CAbilityCard card, bool animateShow = false)
	{
		cardHighlightEffect.SetActive(animateShow);
		cardHolder.PlaceNewCard(card, !animateShow);
		canvasGroup.alpha = 1f;
		if (animateShow)
		{
			animator.Play();
		}
		else
		{
			animator.Stop();
		}
	}

	public void ShowNoEnhanceCard(CAbilityCard card, bool animateShow = false)
	{
		cardHighlightEffect.SetActive(animateShow);
		cardHolder.PlaceNewCard(card, !animateShow);
		canvasGroup.alpha = alphaInvalidCard;
		if (animateShow)
		{
			animator.Play();
		}
		else
		{
			animator.Stop();
		}
	}

	public void Hide()
	{
		cardHolder.Hide();
		cardHighlightEffect.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			cardHolder.Hide();
		}
	}
}
