using System.Collections.Generic;
using GLOOM;
using UnityEngine;

public class UICampaignPartyCharacterStats : UICustomPartyCharacterStats
{
	[SerializeField]
	private RectTransform conditionsContainer;

	[SerializeField]
	private List<UICharacterGainConditionSlot> conditionsPool;

	[SerializeField]
	private GameObject levelUpIndicator;

	[SerializeField]
	private UITextTooltipTarget levelUpTooltip;

	public override void Setup(CMapCharacterService character)
	{
		base.Setup(character);
		Singleton<MapChoreographer>.Instance?.EventBuss?.RegisterToOnCharacterConditionGained(RefreshConditions);
		RefreshConditions();
	}

	private void RefreshConditions(string characterId, string characterName)
	{
		if (characterId == character.CharacterID && characterName == character.CharacterName)
		{
			RefreshConditions();
		}
	}

	public override void Show()
	{
		base.Show();
		conditionsContainer.gameObject.SetActive(value: true);
	}

	public override void Hide()
	{
		base.Show();
		conditionsContainer.gameObject.SetActive(value: false);
		levelUpIndicator.SetActive(value: false);
		levelUpTooltip.enabled = false;
	}

	private void RefreshConditions()
	{
		HelperTools.NormalizePool(ref conditionsPool, conditionsPool[0].gameObject, conditionsContainer, character.Data.TempNegativeConditions.Count + character.Data.TempPositiveConditions.Count + character.Data.NextScenarioNegativeConditions.Count + character.Data.NextScenarioPositiveConditions.Count);
		int index = 0;
		for (int i = 0; i < character.Data.TempNegativeConditions.Count; i++)
		{
			conditionsPool[index++].SetCondition(character.Data.TempNegativeConditions[i].NegativeCondition);
		}
		for (int j = 0; j < character.Data.TempPositiveConditions.Count; j++)
		{
			conditionsPool[index].SetCondition(character.Data.TempPositiveConditions[j].PositiveCondition);
		}
		for (int k = 0; k < character.Data.NextScenarioNegativeConditions.Count; k++)
		{
			conditionsPool[index].SetCondition(character.Data.NextScenarioNegativeConditions[k].NegativeCondition);
		}
		for (int l = 0; l < character.Data.NextScenarioPositiveConditions.Count; l++)
		{
			conditionsPool[index].SetCondition(character.Data.NextScenarioPositiveConditions[l].PositiveCondition);
		}
	}

	protected override void ClearEvents()
	{
		base.ClearEvents();
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterConditionGained(RefreshConditions);
		}
	}

	public override void EnableUIForLevelingUp()
	{
		base.EnableUIForLevelingUp();
		levelUpIndicator.SetActive(value: true);
		NewPartyDisplayUI.PartyDisplay.RefreshAbilityCardsLevelUp(character);
		if (character.HasXpToLevelUp())
		{
			levelUpTooltip.enabled = false;
			return;
		}
		levelUpTooltip.SetText(LocalizationManager.GetTranslation("GUI_LEVELUP_BUTTON"), refreshTooltip: false, LocalizationManager.GetTranslation("GUI_LEVELUP_WEALTH_BUTTON_TOOLTIP"));
		levelUpTooltip.enabled = true;
	}

	public override void DisableUIForLevelingUp()
	{
		NewPartyDisplayUI.PartyDisplay.RefreshAbilityCardsLevelUp(character);
		base.DisableUIForLevelingUp();
		levelUpIndicator.SetActive(value: false);
	}
}
