using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

public static class SaveDataShared
{
	public static void ApplyEnhancementIcons(List<CEnhancement> enhancements, string ID)
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.Find((CCharacterClass s) => s.ID == ID);
		if (cCharacterClass == null)
		{
			return;
		}
		ObjectPool.ClearEnhancements(cCharacterClass.AbilityCardsPool.Select((CAbilityCard s) => s.Name).ToList());
		foreach (CEnhancement enhancement in enhancements)
		{
			try
			{
				CAbilityCard cAbilityCard = cCharacterClass.AbilityCardsPool.SingleOrDefault((CAbilityCard s) => s.ID == enhancement.AbilityCardID);
				if (cAbilityCard == null)
				{
					continue;
				}
				if (enhancement.EnhancementLine == EEnhancementLine.None)
				{
					Debug.LogError("Invalid enhancement type set on " + LocalizationManager.GetTranslation(cCharacterClass.LocKey) + " for card + " + cAbilityCard.Name);
					continue;
				}
				foreach (GameObject allCachedAbilityCard in ObjectPool.GetAllCachedAbilityCards(cAbilityCard.ID))
				{
					bool activeSelf = allCachedAbilityCard.activeSelf;
					allCachedAbilityCard.SetActive(value: true);
					try
					{
						foreach (EnhancementButtonBase item in allCachedAbilityCard.GetComponent<AbilityCardUI>().EnhancementElements.All)
						{
							if (!(item.Enhancement.AbilityName == enhancement.AbilityName) || item.Enhancement.EnhancementLine != enhancement.EnhancementLine || item.Enhancement.EnhancementSlot != enhancement.EnhancementSlot)
							{
								continue;
							}
							if (!(item is EnhancedAreaHex enhancedAreaHex))
							{
								if (item is EnhancementButton enhancementButton)
								{
									enhancementButton.UpdateEnhancement(enhancement.Enhancement);
								}
							}
							else if (enhancement.Enhancement == EEnhancement.NoEnhancement)
							{
								enhancedAreaHex.RemoveEnhancement();
							}
							else
							{
								enhancedAreaHex.ApplyEnhancement();
							}
							item.Enhancement.Enhancement = enhancement.Enhancement;
							item.Enhancement.PaidPrice = enhancement.PaidPrice;
							ObjectPool.AddEnhancedCard(allCachedAbilityCard);
							break;
						}
					}
					catch (Exception ex)
					{
						Debug.LogError("An exception occurred applying enhacements on " + LocalizationManager.GetTranslation(cCharacterClass.LocKey) + "\n: " + ex.Message + "\n" + ex.StackTrace);
					}
					allCachedAbilityCard.SetActive(activeSelf);
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError("An exception occurred applying enhacements on " + LocalizationManager.GetTranslation(cCharacterClass.LocKey) + "\n: " + ex2.Message + "\n" + ex2.StackTrace);
			}
		}
	}
}
