#define ENABLE_LOGS
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation.Utils;

public class ScrollRectFxBehaviour : MonoBehaviour
{
	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private CardsHandUI _hand;

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private VerticalLayoutGroup _layout;

	[UsedImplicitly]
	private void Awake()
	{
		_scrollRect.onValueChanged.AddListener(UpdateFx);
	}

	private void UpdateFx(Vector2 vector)
	{
		Debug.Log($"[ScrollRectFxBehaviour] ScrollRect Vector Changed to {vector}");
		foreach (AbilityCardUI selectedCard in _hand.SelectedCards)
		{
			selectedCard.PlayEffect(GetEffectBasedOnVisibility(selectedCard));
		}
	}

	private bool IsVisible(AbilityCardUI card)
	{
		float num = 0f - card.RectTransform.anchoredPosition.y - card.RectTransform.pivot.y * card.RectTransform.sizeDelta.y + (float)_layout.padding.top + 20f;
		float y = _container.anchoredPosition.y;
		Debug.Log($"[ScrollRectFxBehaviour] Card Position {num} Container Position {y}");
		return num >= y;
	}

	private InitiativeTrackActorAvatar.InitiativeEffects GetEffectBasedOnVisibility(AbilityCardUI card)
	{
		if (IsVisible(card) && _hand.PlayerActor.CharacterClass.RoundAbilityCards.Count > 0)
		{
			CAbilityCard cAbilityCard = _hand.PlayerActor.CharacterClass.RoundAbilityCards[0];
			if (cAbilityCard != null && cAbilityCard == card.AbilityCard)
			{
				return InitiativeTrackActorAvatar.InitiativeEffects.Active;
			}
		}
		return InitiativeTrackActorAvatar.InitiativeEffects.None;
	}
}
