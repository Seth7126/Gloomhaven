using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class RewardGroup : ISerializable
{
	public string TreasureTable;

	public string Screen;

	public string Option;

	public List<Reward> Rewards;

	public string TreasureType;

	public int GoldAmount;

	public int XPAmount;

	public int DamageAmount;

	public int ProsperityAmount;

	public int ReputationAmount;

	public int DiscardAmount;

	public int EnhancementSlots;

	public int PerkPoints;

	public int PerkChecks;

	public List<string> Items;

	public List<string> ItemStock;

	public List<RewardCondition> Conditions;

	public List<RewardCondition> EnemyConditions;

	public List<ElementInfusionBoardManager.EElement> Infusions;

	public List<EEnhancement> Enhancements;

	public List<string> CharacterIDs;

	public List<string> Locations;

	public List<string> PerkNames;

	public bool UnlockedEnhancer;

	public bool UnlockedMerchant;

	public bool UnlockedTemple;

	public bool UnlockedTrainer;

	public bool UnlockedPartyUI;

	public bool UnlockedMultiplayer;

	public bool GainedGold;

	public bool LostGold;

	public bool GainedFullGold;

	public bool LostFullGold;

	public bool GainedXP;

	public bool LostXP;

	public bool GainedDamage;

	public bool GainedProsperity;

	public bool LostProsperity;

	public bool GainedReputation;

	public bool LostReputation;

	public List<string> FromTreasureTables;

	private string m_PickedUpBy;

	public List<PerksYMLData> Perks => ScenarioRuleClient.SRLYML.Perks.FindAll((PerksYMLData x) => PerkNames.Contains(x.Name));

	public string PickedUpBy
	{
		get
		{
			return m_PickedUpBy;
		}
		set
		{
			m_PickedUpBy = value;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TreasureTable", TreasureTable);
		info.AddValue("Screen", Screen);
		info.AddValue("Option", Option);
		info.AddValue("Rewards", Rewards);
		info.AddValue("GoldAmount", GoldAmount);
		info.AddValue("XPAmount", XPAmount);
		info.AddValue("DamageAmount", DamageAmount);
		info.AddValue("ProsperityAmount", ProsperityAmount);
		info.AddValue("ReputationAmount", ReputationAmount);
		info.AddValue("DiscardAmount", DiscardAmount);
		info.AddValue("EnhancementSlots", EnhancementSlots);
		info.AddValue("PerkPoints", PerkPoints);
		info.AddValue("PerkChecks", PerkChecks);
		info.AddValue("Items", Items);
		info.AddValue("ItemStock", ItemStock);
		info.AddValue("Conditions", Conditions);
		info.AddValue("EnemyConditions", EnemyConditions);
		info.AddValue("Infusions", Infusions);
		info.AddValue("Enhancements", Enhancements);
		info.AddValue("CharacterIDs", CharacterIDs);
		info.AddValue("Locations", Locations);
		info.AddValue("PerkNames", PerkNames);
		info.AddValue("UnlockedEnhancer", UnlockedEnhancer);
		info.AddValue("UnlockedMerchant", UnlockedMerchant);
		info.AddValue("UnlockedTemple", UnlockedTemple);
		info.AddValue("UnlockedTrainer", UnlockedTrainer);
		info.AddValue("UnlockedPartyUI", UnlockedPartyUI);
		info.AddValue("UnlockedMultiplayer", UnlockedMultiplayer);
		info.AddValue("GainedGold", GainedGold);
		info.AddValue("LostGold", LostGold);
		info.AddValue("GainedFullGold", GainedFullGold);
		info.AddValue("LostFullGold", LostFullGold);
		info.AddValue("GainedXP", GainedXP);
		info.AddValue("LostXP", LostXP);
		info.AddValue("GainedDamage", GainedDamage);
		info.AddValue("GainedProsperity", GainedProsperity);
		info.AddValue("LostProsperity", LostProsperity);
		info.AddValue("GainedReputation", GainedReputation);
		info.AddValue("LostReputation", LostReputation);
		info.AddValue("FromTreasureTables", FromTreasureTables);
		info.AddValue("TreasureType", TreasureType);
	}

	protected RewardGroup(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "TreasureTable":
					TreasureTable = info.GetString("TreasureTable");
					break;
				case "Screen":
					Screen = info.GetString("Screen");
					break;
				case "Option":
					Option = info.GetString("Option");
					break;
				case "Rewards":
					Rewards = (List<Reward>)info.GetValue("Rewards", typeof(List<Reward>));
					break;
				case "GoldAmount":
					GoldAmount = info.GetInt32("GoldAmount");
					break;
				case "XPAmount":
					XPAmount = info.GetInt32("XPAmount");
					break;
				case "DamageAmount":
					DamageAmount = info.GetInt32("DamageAmount");
					break;
				case "ProsperityAmount":
					ProsperityAmount = info.GetInt32("ProsperityAmount");
					break;
				case "ReputationAmount":
					ReputationAmount = info.GetInt32("ReputationAmount");
					break;
				case "DiscardAmount":
					DiscardAmount = info.GetInt32("DiscardAmount");
					break;
				case "EnhancementSlots":
					EnhancementSlots = info.GetInt32("EnhancementSlots");
					break;
				case "PerkPoints":
					PerkPoints = info.GetInt32("PerkPoints");
					break;
				case "PerkChecks":
					PerkChecks = info.GetInt32("PerkChecks");
					break;
				case "Items":
					Items = (List<string>)info.GetValue("Items", typeof(List<string>));
					break;
				case "ItemStock":
					ItemStock = (List<string>)info.GetValue("ItemStock", typeof(List<string>));
					break;
				case "Conditions":
					Conditions = (List<RewardCondition>)info.GetValue("Conditions", typeof(List<RewardCondition>));
					break;
				case "EnemyConditions":
					EnemyConditions = (List<RewardCondition>)info.GetValue("EnemyConditions", typeof(List<RewardCondition>));
					break;
				case "Infusions":
					Infusions = (List<ElementInfusionBoardManager.EElement>)info.GetValue("Infusions", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "Enhancements":
					Enhancements = (List<EEnhancement>)info.GetValue("Enhancements", typeof(List<EEnhancement>));
					break;
				case "CharacterIDs":
					CharacterIDs = (List<string>)info.GetValue("CharacterIDs", typeof(List<string>));
					break;
				case "Locations":
					Locations = (List<string>)info.GetValue("Locations", typeof(List<string>));
					break;
				case "PerkNames":
					PerkNames = (List<string>)info.GetValue("PerkNames", typeof(List<string>));
					break;
				case "UnlockedEnhancer":
					UnlockedEnhancer = info.GetBoolean("UnlockedEnhancer");
					break;
				case "UnlockedMerchant":
					UnlockedMerchant = info.GetBoolean("UnlockedMerchant");
					break;
				case "UnlockedTemple":
					UnlockedTemple = info.GetBoolean("UnlockedTemple");
					break;
				case "UnlockedTrainer":
					UnlockedTrainer = info.GetBoolean("UnlockedTrainer");
					break;
				case "UnlockedPartyUI":
					UnlockedPartyUI = info.GetBoolean("UnlockedPartyUI");
					break;
				case "UnlockedMultiplayer":
					UnlockedMultiplayer = info.GetBoolean("UnlockedMultiplayer");
					break;
				case "GainedGold":
					GainedGold = info.GetBoolean("GainedGold");
					break;
				case "LostGold":
					LostGold = info.GetBoolean("LostGold");
					break;
				case "GainedFullGold":
					GainedFullGold = info.GetBoolean("GainedFullGold");
					break;
				case "LostFullGold":
					LostFullGold = info.GetBoolean("LostFullGold");
					break;
				case "GainedXP":
					GainedXP = info.GetBoolean("GainedXP");
					break;
				case "LostXP":
					LostXP = info.GetBoolean("LostXP");
					break;
				case "GainedDamage":
					GainedDamage = info.GetBoolean("GainedDamage");
					break;
				case "GainedProsperity":
					GainedProsperity = info.GetBoolean("GainedProsperity");
					break;
				case "LostProsperity":
					LostProsperity = info.GetBoolean("LostProsperity");
					break;
				case "GainedReputation":
					GainedReputation = info.GetBoolean("GainedReputation");
					break;
				case "LostReputation":
					LostReputation = info.GetBoolean("LostReputation");
					break;
				case "FromTreasureTables":
					FromTreasureTables = (List<string>)info.GetValue("FromTreasureTables", typeof(List<string>));
					break;
				case "TreasureType":
					TreasureType = info.GetString("TreasureType");
					break;
				case "Characters":
				{
					List<ECharacter> source = (List<ECharacter>)info.GetValue("Characters", typeof(List<ECharacter>));
					CharacterIDs = source.Select((ECharacter s) => s.ToString() + "ID").ToList();
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize RewardGroup entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public RewardGroup(List<Reward> rewards, string treasureTable = "", string screen = "", string option = "", string type = "")
	{
		TreasureTable = treasureTable;
		Screen = screen;
		Option = option;
		Rewards = rewards;
		TreasureType = type;
		RefreshRewardValues();
	}

	public RewardGroup(List<RewardGroup> rewardGroups, string treasureTable = "", string screen = "", string option = "", string type = "")
	{
		TreasureTable = treasureTable;
		Screen = screen;
		Option = option;
		Rewards = new List<Reward>();
		TreasureType = type;
		foreach (RewardGroup rewardGroup in rewardGroups)
		{
			Rewards.AddRange(rewardGroup.Rewards);
		}
		RefreshRewardValues();
	}

	public RewardGroup()
	{
		InitValues();
	}

	private void InitValues()
	{
		GoldAmount = 0;
		XPAmount = 0;
		DamageAmount = 0;
		ProsperityAmount = 0;
		ReputationAmount = 0;
		DiscardAmount = 0;
		EnhancementSlots = 0;
		PerkPoints = 0;
		PerkChecks = 0;
		Items = new List<string>();
		ItemStock = new List<string>();
		Conditions = new List<RewardCondition>();
		EnemyConditions = new List<RewardCondition>();
		Infusions = new List<ElementInfusionBoardManager.EElement>();
		Enhancements = new List<EEnhancement>();
		CharacterIDs = new List<string>();
		Locations = new List<string>();
		PerkNames = new List<string>();
		GainedGold = false;
		LostGold = false;
		GainedFullGold = false;
		LostFullGold = false;
		GainedXP = false;
		LostXP = false;
		GainedDamage = false;
		GainedProsperity = false;
		LostProsperity = false;
		GainedReputation = false;
		LostReputation = false;
		UnlockedEnhancer = false;
		UnlockedMerchant = false;
		UnlockedTemple = false;
		UnlockedTrainer = false;
		UnlockedPartyUI = false;
		UnlockedMultiplayer = false;
	}

	public bool HasValues()
	{
		if (GoldAmount == 0 && XPAmount == 0 && DamageAmount == 0 && DiscardAmount == 0 && !GainedXP && !LostXP && !GainedGold && !LostGold && !GainedFullGold && !LostFullGold && !GainedDamage && !GainedProsperity && !LostProsperity && !GainedReputation && !LostReputation && Items.Count <= 0 && Conditions.Count <= 0 && EnemyConditions.Count <= 0 && Infusions.Count <= 0 && Enhancements.Count <= 0 && CharacterIDs.Count <= 0 && Locations.Count <= 0 && Perks.Count <= 0 && !UnlockedEnhancer && !UnlockedMerchant && !UnlockedTemple && !UnlockedMultiplayer)
		{
			return UnlockedTrainer;
		}
		return true;
	}

	public void RefreshRewardValues()
	{
		InitValues();
		foreach (Reward reward in Rewards)
		{
			switch (reward.Type)
			{
			case ETreasureType.Condition:
				Conditions.AddRange(reward.Conditions);
				break;
			case ETreasureType.EnemyCondition:
				EnemyConditions.AddRange(reward.EnemyConditions);
				break;
			case ETreasureType.Damage:
				DamageAmount += reward.Amount;
				break;
			case ETreasureType.Enhancement:
				Enhancements.Add(reward.Enhancement);
				break;
			case ETreasureType.Gold:
				GoldAmount += reward.Amount;
				break;
			case ETreasureType.Infuse:
				Infusions.AddRange(reward.Infusions);
				break;
			case ETreasureType.Item:
			case ETreasureType.UnlockProsperityItem:
				if (reward.Item != null)
				{
					Items.Add(reward.Item.Name);
				}
				break;
			case ETreasureType.ItemStock:
			case ETreasureType.UnlockProsperityItemStock:
				if (reward.Item != null)
				{
					ItemStock.Add(reward.Item.Name);
				}
				break;
			case ETreasureType.Perk:
				if (reward.Perk != null)
				{
					PerkNames.Add(reward.Perk.Name);
				}
				break;
			case ETreasureType.Prosperity:
				ProsperityAmount += reward.Amount;
				break;
			case ETreasureType.Reputation:
				ReputationAmount += reward.Amount;
				break;
			case ETreasureType.Discard:
				DiscardAmount += reward.Amount;
				break;
			case ETreasureType.UnlockCharacter:
				CharacterIDs.Add(reward.CharacterID);
				break;
			case ETreasureType.UnlockEnhancer:
				UnlockedEnhancer = true;
				break;
			case ETreasureType.UnlockLocation:
				Locations.Add(reward.UnlockName);
				break;
			case ETreasureType.UnlockMerchant:
				UnlockedMerchant = true;
				break;
			case ETreasureType.UnlockTemple:
				UnlockedTemple = true;
				break;
			case ETreasureType.UnlockTrainer:
				UnlockedTrainer = true;
				break;
			case ETreasureType.XP:
				XPAmount += reward.Amount;
				break;
			case ETreasureType.EnhancementSlots:
				EnhancementSlots += reward.Amount;
				break;
			case ETreasureType.UnlockPartyUI:
				UnlockedPartyUI = true;
				break;
			case ETreasureType.PerkPoint:
				PerkPoints += reward.Amount;
				break;
			case ETreasureType.PerkCheck:
				PerkChecks += reward.Amount;
				break;
			case ETreasureType.UnlockMultiplayer:
				UnlockedMultiplayer = true;
				break;
			case ETreasureType.LoseItem:
				if (reward.Item != null)
				{
					Items.Add(reward.Item.Name);
				}
				break;
			default:
				DLLDebug.LogError("Unsupported Reward Type " + reward.Type);
				break;
			case ETreasureType.UnlockQuest:
			case ETreasureType.UnlockAchievement:
			case ETreasureType.UnlockChapter:
			case ETreasureType.CityEvent:
			case ETreasureType.RoadEvent:
			case ETreasureType.ConsumeItem:
			case ETreasureType.AddModifiers:
			case ETreasureType.LockAchievement:
				break;
			}
		}
		GainedGold = GoldAmount > 0;
		LostGold = GoldAmount < 0;
		GainedXP = XPAmount > 0;
		LostXP = XPAmount < 0;
		GainedDamage = DamageAmount > 0;
		GainedProsperity = ProsperityAmount > 0;
		LostProsperity = ProsperityAmount < 0;
		GainedReputation = ReputationAmount > 0;
		LostReputation = ReputationAmount < 0;
	}

	public string GetAbsValueAsString(string propertyName)
	{
		switch (propertyName)
		{
		case "GoldAmount":
			return Math.Abs(GoldAmount).ToString();
		case "XPAmount":
			return Math.Abs(XPAmount).ToString();
		case "Damage":
			return Math.Abs(DamageAmount).ToString();
		case "Prosperity":
			return Math.Abs(ProsperityAmount).ToString();
		case "Reputation":
			return Math.Abs(ReputationAmount).ToString();
		case "Discard":
			return Math.Abs(DiscardAmount).ToString();
		case "Item":
		case "Items":
			return string.Join(", ", Items.ToArray());
		case "Condition":
		case "Conditions":
			return string.Join(", ", Conditions.Select((RewardCondition x) => (x.Type != RewardCondition.EConditionType.Negative) ? ((x.Type != RewardCondition.EConditionType.Positive) ? string.Empty : x.PositiveCondition.ToString()) : x.NegativeCondition.ToString()).ToArray());
		case "EnemyCondition":
		case "EnemyConditions":
			return string.Join(", ", EnemyConditions.Select((RewardCondition x) => (x.Type != RewardCondition.EConditionType.Negative) ? ((x.Type != RewardCondition.EConditionType.Positive) ? string.Empty : x.PositiveCondition.ToString()) : x.NegativeCondition.ToString()).ToArray());
		case "Infuse":
		case "Infusion":
		case "Infusions":
			return string.Join(", ", Infusions.ToArray());
		case "Enhancement":
		case "Enhancements":
			return string.Join(", ", Enhancements.ToArray());
		case "Character":
		case "Characters":
			return string.Join(", ", CharacterIDs.ToArray());
		case "Perk":
		case "Perks":
			return string.Join(", ", Perks.Select((PerksYMLData x) => x.Name).ToArray());
		default:
			DLLDebug.LogError("Unsupported Reward value '" + propertyName + "' pass to GetValueAsString.");
			return null;
		}
	}

	public void AddInfusions(List<string> infusions)
	{
		foreach (string infuse in infusions)
		{
			try
			{
				Infusions.Add(ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == infuse));
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Invalid Element " + infuse + "in RewardGroup.\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
	}

	public void AddConditions(List<string> conditions, bool forEnemy)
	{
		foreach (string condition in conditions)
		{
			CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == condition);
			if (eNegativeCondition != CCondition.ENegativeCondition.NA)
			{
				if (forEnemy)
				{
					EnemyConditions.Add(new RewardCondition(RewardCondition.EConditionMapDuration.None, eNegativeCondition, 0));
				}
				else
				{
					Conditions.Add(new RewardCondition(RewardCondition.EConditionMapDuration.None, eNegativeCondition, 0));
				}
				continue;
			}
			CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == condition);
			if (ePositiveCondition != CCondition.EPositiveCondition.NA)
			{
				if (forEnemy)
				{
					EnemyConditions.Add(new RewardCondition(RewardCondition.EConditionMapDuration.None, ePositiveCondition, 0));
				}
				else
				{
					Conditions.Add(new RewardCondition(RewardCondition.EConditionMapDuration.None, ePositiveCondition, 0));
				}
			}
			else
			{
				DLLDebug.LogError("Unable to find condition " + condition);
			}
		}
	}

	public bool Equals(RewardGroup compareGroup)
	{
		if (compareGroup.Rewards.Count != Rewards.Count)
		{
			return false;
		}
		for (int i = 0; i < compareGroup.Rewards.Count; i++)
		{
			if (!compareGroup.Rewards[i].Equals(Rewards[i]))
			{
				return false;
			}
		}
		if (compareGroup.GoldAmount != GoldAmount)
		{
			return false;
		}
		if (compareGroup.XPAmount != XPAmount)
		{
			return false;
		}
		if (compareGroup.DamageAmount != DamageAmount)
		{
			return false;
		}
		if (compareGroup.ProsperityAmount != ProsperityAmount)
		{
			return false;
		}
		if (compareGroup.ReputationAmount != ReputationAmount)
		{
			return false;
		}
		if (compareGroup.DiscardAmount != DiscardAmount)
		{
			return false;
		}
		if (compareGroup.EnhancementSlots != EnhancementSlots)
		{
			return false;
		}
		if (compareGroup.PerkPoints != PerkPoints)
		{
			return false;
		}
		if (compareGroup.PerkChecks != PerkChecks)
		{
			return false;
		}
		if (compareGroup.Items.Count != Items.Count)
		{
			return false;
		}
		for (int j = 0; j < compareGroup.Items.Count; j++)
		{
			if (!compareGroup.Items[j].Equals(Items[j]))
			{
				return false;
			}
		}
		if (compareGroup.ItemStock.Count != ItemStock.Count)
		{
			return false;
		}
		for (int k = 0; k < compareGroup.ItemStock.Count; k++)
		{
			if (!compareGroup.ItemStock[k].Equals(ItemStock[k]))
			{
				return false;
			}
		}
		if (compareGroup.Conditions.Count != Conditions.Count)
		{
			return false;
		}
		for (int l = 0; l < compareGroup.Conditions.Count; l++)
		{
			if (!compareGroup.Conditions[l].Equals(Conditions[l]))
			{
				return false;
			}
		}
		if (compareGroup.EnemyConditions == null && EnemyConditions != null)
		{
			return false;
		}
		if (compareGroup.EnemyConditions != null && EnemyConditions == null)
		{
			return false;
		}
		if (compareGroup.EnemyConditions.Count != EnemyConditions.Count)
		{
			return false;
		}
		for (int m = 0; m < compareGroup.EnemyConditions.Count; m++)
		{
			if (!compareGroup.EnemyConditions[m].Equals(EnemyConditions[m]))
			{
				return false;
			}
		}
		if (compareGroup.Infusions.Count != Infusions.Count)
		{
			return false;
		}
		for (int n = 0; n < compareGroup.Infusions.Count; n++)
		{
			if (!compareGroup.Infusions[n].Equals(Infusions[n]))
			{
				return false;
			}
		}
		if (compareGroup.Enhancements.Count != Enhancements.Count)
		{
			return false;
		}
		for (int num = 0; num < compareGroup.Enhancements.Count; num++)
		{
			if (!compareGroup.Enhancements[num].Equals(Enhancements[num]))
			{
				return false;
			}
		}
		if (compareGroup.CharacterIDs.Count != CharacterIDs.Count)
		{
			return false;
		}
		for (int num2 = 0; num2 < compareGroup.CharacterIDs.Count; num2++)
		{
			if (!compareGroup.CharacterIDs[num2].Equals(CharacterIDs[num2]))
			{
				return false;
			}
		}
		if (compareGroup.Locations.Count != Locations.Count)
		{
			return false;
		}
		for (int num3 = 0; num3 < compareGroup.Locations.Count; num3++)
		{
			if (!compareGroup.Locations[num3].Equals(Locations[num3]))
			{
				return false;
			}
		}
		if (compareGroup.PerkNames.Count != PerkNames.Count)
		{
			return false;
		}
		for (int num4 = 0; num4 < compareGroup.PerkNames.Count; num4++)
		{
			if (!compareGroup.PerkNames[num4].Equals(PerkNames[num4]))
			{
				return false;
			}
		}
		if (compareGroup.UnlockedEnhancer != UnlockedEnhancer)
		{
			return false;
		}
		if (compareGroup.UnlockedMerchant != UnlockedMerchant)
		{
			return false;
		}
		if (compareGroup.UnlockedTemple != UnlockedTemple)
		{
			return false;
		}
		if (compareGroup.UnlockedTrainer != UnlockedTrainer)
		{
			return false;
		}
		if (compareGroup.UnlockedPartyUI != UnlockedPartyUI)
		{
			return false;
		}
		if (compareGroup.UnlockedMultiplayer != UnlockedMultiplayer)
		{
			return false;
		}
		return true;
	}

	public RewardGroup(RewardGroup state, ReferenceDictionary references)
	{
		TreasureTable = state.TreasureTable;
		Screen = state.Screen;
		Option = state.Option;
		Rewards = references.Get(state.Rewards);
		if (Rewards == null && state.Rewards != null)
		{
			Rewards = new List<Reward>();
			for (int i = 0; i < state.Rewards.Count; i++)
			{
				Reward reward = state.Rewards[i];
				Reward reward2 = references.Get(reward);
				if (reward2 == null && reward != null)
				{
					reward2 = new Reward(reward, references);
					references.Add(reward, reward2);
				}
				Rewards.Add(reward2);
			}
			references.Add(state.Rewards, Rewards);
		}
		TreasureType = state.TreasureType;
		GoldAmount = state.GoldAmount;
		XPAmount = state.XPAmount;
		DamageAmount = state.DamageAmount;
		ProsperityAmount = state.ProsperityAmount;
		ReputationAmount = state.ReputationAmount;
		DiscardAmount = state.DiscardAmount;
		EnhancementSlots = state.EnhancementSlots;
		PerkPoints = state.PerkPoints;
		PerkChecks = state.PerkChecks;
		Items = references.Get(state.Items);
		if (Items == null && state.Items != null)
		{
			Items = new List<string>();
			for (int j = 0; j < state.Items.Count; j++)
			{
				string item = state.Items[j];
				Items.Add(item);
			}
			references.Add(state.Items, Items);
		}
		ItemStock = references.Get(state.ItemStock);
		if (ItemStock == null && state.ItemStock != null)
		{
			ItemStock = new List<string>();
			for (int k = 0; k < state.ItemStock.Count; k++)
			{
				string item2 = state.ItemStock[k];
				ItemStock.Add(item2);
			}
			references.Add(state.ItemStock, ItemStock);
		}
		Conditions = references.Get(state.Conditions);
		if (Conditions == null && state.Conditions != null)
		{
			Conditions = new List<RewardCondition>();
			for (int l = 0; l < state.Conditions.Count; l++)
			{
				RewardCondition rewardCondition = state.Conditions[l];
				RewardCondition rewardCondition2 = references.Get(rewardCondition);
				if (rewardCondition2 == null && rewardCondition != null)
				{
					rewardCondition2 = new RewardCondition(rewardCondition, references);
					references.Add(rewardCondition, rewardCondition2);
				}
				Conditions.Add(rewardCondition2);
			}
			references.Add(state.Conditions, Conditions);
		}
		EnemyConditions = references.Get(state.EnemyConditions);
		if (EnemyConditions == null && state.EnemyConditions != null)
		{
			EnemyConditions = new List<RewardCondition>();
			for (int m = 0; m < state.EnemyConditions.Count; m++)
			{
				RewardCondition rewardCondition3 = state.EnemyConditions[m];
				RewardCondition rewardCondition4 = references.Get(rewardCondition3);
				if (rewardCondition4 == null && rewardCondition3 != null)
				{
					rewardCondition4 = new RewardCondition(rewardCondition3, references);
					references.Add(rewardCondition3, rewardCondition4);
				}
				EnemyConditions.Add(rewardCondition4);
			}
			references.Add(state.EnemyConditions, EnemyConditions);
		}
		Infusions = references.Get(state.Infusions);
		if (Infusions == null && state.Infusions != null)
		{
			Infusions = new List<ElementInfusionBoardManager.EElement>();
			for (int n = 0; n < state.Infusions.Count; n++)
			{
				ElementInfusionBoardManager.EElement item3 = state.Infusions[n];
				Infusions.Add(item3);
			}
			references.Add(state.Infusions, Infusions);
		}
		Enhancements = references.Get(state.Enhancements);
		if (Enhancements == null && state.Enhancements != null)
		{
			Enhancements = new List<EEnhancement>();
			for (int num = 0; num < state.Enhancements.Count; num++)
			{
				EEnhancement item4 = state.Enhancements[num];
				Enhancements.Add(item4);
			}
			references.Add(state.Enhancements, Enhancements);
		}
		CharacterIDs = references.Get(state.CharacterIDs);
		if (CharacterIDs == null && state.CharacterIDs != null)
		{
			CharacterIDs = new List<string>();
			for (int num2 = 0; num2 < state.CharacterIDs.Count; num2++)
			{
				string item5 = state.CharacterIDs[num2];
				CharacterIDs.Add(item5);
			}
			references.Add(state.CharacterIDs, CharacterIDs);
		}
		Locations = references.Get(state.Locations);
		if (Locations == null && state.Locations != null)
		{
			Locations = new List<string>();
			for (int num3 = 0; num3 < state.Locations.Count; num3++)
			{
				string item6 = state.Locations[num3];
				Locations.Add(item6);
			}
			references.Add(state.Locations, Locations);
		}
		PerkNames = references.Get(state.PerkNames);
		if (PerkNames == null && state.PerkNames != null)
		{
			PerkNames = new List<string>();
			for (int num4 = 0; num4 < state.PerkNames.Count; num4++)
			{
				string item7 = state.PerkNames[num4];
				PerkNames.Add(item7);
			}
			references.Add(state.PerkNames, PerkNames);
		}
		UnlockedEnhancer = state.UnlockedEnhancer;
		UnlockedMerchant = state.UnlockedMerchant;
		UnlockedTemple = state.UnlockedTemple;
		UnlockedTrainer = state.UnlockedTrainer;
		UnlockedPartyUI = state.UnlockedPartyUI;
		UnlockedMultiplayer = state.UnlockedMultiplayer;
		GainedGold = state.GainedGold;
		LostGold = state.LostGold;
		GainedFullGold = state.GainedFullGold;
		LostFullGold = state.LostFullGold;
		GainedXP = state.GainedXP;
		LostXP = state.LostXP;
		GainedDamage = state.GainedDamage;
		GainedProsperity = state.GainedProsperity;
		LostProsperity = state.LostProsperity;
		GainedReputation = state.GainedReputation;
		LostReputation = state.LostReputation;
		FromTreasureTables = references.Get(state.FromTreasureTables);
		if (FromTreasureTables == null && state.FromTreasureTables != null)
		{
			FromTreasureTables = new List<string>();
			for (int num5 = 0; num5 < state.FromTreasureTables.Count; num5++)
			{
				string item8 = state.FromTreasureTables[num5];
				FromTreasureTables.Add(item8);
			}
			references.Add(state.FromTreasureTables, FromTreasureTables);
		}
		m_PickedUpBy = state.m_PickedUpBy;
	}
}
