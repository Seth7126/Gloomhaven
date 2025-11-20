using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using UnityEngine;

public class MapPartyEnhancementShopService : IEnhancementShopService
{
	private CMapParty party;

	public bool IsEnchantressIntroShown => party.HasIntroduced(EIntroductionConcept.Enchantress.ToString());

	public bool IsSellAvailable => ScenarioRuleClient.SRLYML.Enhancements.AllowSell(AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent);

	public MapPartyEnhancementShopService()
	{
		party = AdventureState.MapState.MapParty;
	}

	public void OnEnterShop()
	{
		if (AdventureState.MapState.HeadquartersState.EnhancerHasNewStock)
		{
			AdventureState.MapState.HeadquartersState.EnhancerHasNewStock = false;
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	public void AddEnhancement(EnhancementButtonBase button, EEnhancement enhancement)
	{
		bool flag = enhancement == EEnhancement.NoEnhancement;
		List<string> list = new List<string>();
		CMapCharacter cMapCharacter = party.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.OwnedAbilityCardIDs.Contains(button.Enhancement.AbilityCardID));
		if (cMapCharacter != null)
		{
			foreach (CEnhancement item in cMapCharacter.Enhancements.Where((CEnhancement a) => a.AbilityCardID == button.Enhancement.AbilityCardID && a.AbilityName == button.Enhancement.AbilityName && a.EnhancementLine == button.Enhancement.EnhancementLine && a.EnhancementSlot == button.Enhancement.EnhancementSlot).ToList())
			{
				int amount = Mathf.RoundToInt((float)item.PaidPrice * ScenarioRuleClient.SRLYML.Enhancements.SellPricePercentage(AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent));
				ModifyGold(cMapCharacter, amount);
				list.Add(item.Enhancement.ToString());
				cMapCharacter.Enhancements.Remove(item);
			}
			int num = ((!flag) ? CEnhancement.TotalCost(enhancement, button.Enhancement.AbilityCard, button.Enhancement.Ability, AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent) : 0);
			button.Enhancement.BuyEnhancement(enhancement, num);
			cMapCharacter.Enhancements.Add(button.Enhancement.Copy());
			AdventureState.MapState.CheckTrophyAchievements(new CEnhancementAdded_AchievementTrigger());
			ModifyGold(cMapCharacter, -num);
			cMapCharacter.EnhancementsBought++;
			cMapCharacter.ApplyEnhancements(new List<CEnhancement> { button.Enhancement });
			SaveDataShared.ApplyEnhancementIcons(cMapCharacter.Enhancements, cMapCharacter.CharacterID);
			if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.Enchantress)
			{
				PreviewEnhancement(button, enhancement);
			}
			SaveData.Instance.SaveCurrentAdventureData();
			MapRuleLibraryClient.Instance.AddQueueMessage(new CMapDLLMessage(EMapDLLMessageType.CheckLockedContent), processImmediately: false);
			string text = string.Empty;
			if (FFSNetwork.IsOnline)
			{
				NetworkPlayer controller = ControllableRegistry.GetController(AdventureState.MapState.IsCampaign ? cMapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter.CharacterID));
				if (controller != null)
				{
					text = " (Controller: " + controller.Username + ")";
				}
			}
			Console.LogInfo(cMapCharacter.CharacterID + text + (flag ? " removed an enhancement (Card: " : " bought a new enhancement (Card: ") + button.Enhancement.AbilityCard.StrictName + ", Ability: " + button.Enhancement.Ability?.ToString() + ", Enhancement: " + (flag ? list.ToStringPretty() : enhancement.ToString()) + ") for " + num + " gold.", customFlag: true);
		}
		else
		{
			Debug.LogError("Invalid Ability Card ID " + button.Enhancement.AbilityCardID + ".  Could not find card.");
		}
	}

	private void ModifyGold(CMapCharacter character, int amount)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			party.ModifyPartyGold(amount, useGoldModifier: false);
		}
		else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			character.ModifyGold(amount, useGoldModifier: false);
		}
	}

	public void PreviewEnhancement(EnhancementButtonBase button, EEnhancement enhancement)
	{
		if (button is EnhancementButton enhancementButton)
		{
			enhancementButton.UpdateEnhancement(enhancement);
		}
		else if (button is EnhancedAreaHex enhancedAreaHex)
		{
			switch (enhancement)
			{
			case EEnhancement.NoEnhancement:
				enhancedAreaHex.RemoveEnhancement();
				break;
			case EEnhancement.Area:
				enhancedAreaHex.ApplyEnhancement();
				break;
			default:
				Debug.LogError("Invalid area enhancement");
				break;
			}
		}
	}

	public void SetEnchantressIntroShown()
	{
		party.MarkIntroDone(EIntroductionConcept.Enchantress.ToString());
		SaveData.Instance.SaveCurrentAdventureData();
	}

	public void ClearPreviewEnhancement(EnhancementButtonBase enhancement)
	{
		PreviewEnhancement(enhancement, enhancement.EnhancedType);
	}

	public List<EnhancementSlot> GetEnhancementsToBuy(EnhancementLine line)
	{
		List<EEnhancement> unlockedEnhancements = AdventureState.MapState.HeadquartersState.EnhancerStock;
		return line.GetEnhancementsCompatibleWithEnhancementButtonLine().FindAll((EnhancementSlot it) => unlockedEnhancements.Contains(it.enhancement));
	}

	public void RemoveEnhancement(EnhancementButtonBase button)
	{
		AddEnhancement(button, EEnhancement.NoEnhancement);
	}
}
