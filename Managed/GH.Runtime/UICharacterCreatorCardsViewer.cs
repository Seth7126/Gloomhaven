using System.Collections.Generic;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UICharacterCreatorCardsViewer : MonoBehaviour
{
	[SerializeField]
	private ScrollRect cardContainer;

	[SerializeField]
	private PanelHotkeyContainer _hotkeyContainer;

	private UIWindow window;

	private ICharacterCreatorClass character;

	private List<AbilityCardUI> cards = new List<AbilityCardUI>();

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHidden);
		if (InputManager.GamePadInUse)
		{
			_hotkeyContainer.SetHotkeyAction("Tips", delegate
			{
				TooltipsVisibilityHelper.Instance.ToggleTooltips(this);
			});
		}
	}

	private void AnyFullCardHighlightChanged(bool isHighlight)
	{
		_hotkeyContainer.SetActiveHotkey("Tips", isHighlight);
	}

	public void Show(ICharacterCreatorClass character)
	{
		if (this.character != character)
		{
			this.character = character;
			ClearCards();
			foreach (CAbilityCard ownedAbilityCard in character.OwnedAbilityCards)
			{
				AbilityCardUI component = ObjectPool.SpawnCard(ownedAbilityCard.ID, ObjectPool.ECardType.Ability, cardContainer.content, resetLocalScale: true, resetToMiddle: true).GetComponent<AbilityCardUI>();
				component.Init(ownedAbilityCard);
				component.ToggleFullCard(active: true);
				cards.Add(component);
			}
		}
		foreach (AbilityCardUI card in cards)
		{
			card.Show();
		}
		cardContainer.verticalNormalizedPosition = 1f;
		window.Show();
		FullAbilityCard.FullCardHoveringStateChanged += AnyFullCardHighlightChanged;
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.StartingAbilityCards);
	}

	public void Hide()
	{
		window.Hide();
	}

	private void OnHidden()
	{
		FullAbilityCard.FullCardHoveringStateChanged -= AnyFullCardHighlightChanged;
	}

	private void ClearCards()
	{
		foreach (AbilityCardUI card in cards)
		{
			ObjectPool.RecycleCard(card.CardID, ObjectPool.ECardType.Ability, card.gameObject);
		}
		cards.Clear();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (TooltipsVisibilityHelper.Instance != null)
		{
			TooltipsVisibilityHelper.Instance.RemoveTooltipRequest(this);
		}
		FullAbilityCard.FullCardHoveringStateChanged -= AnyFullCardHighlightChanged;
		ClearCards();
	}
}
