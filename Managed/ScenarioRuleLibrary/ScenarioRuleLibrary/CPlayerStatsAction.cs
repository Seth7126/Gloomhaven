using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsAction : CPlayerStatsBase, ISerializable
{
	public string ActingClassID { get; private set; }

	public string ActedOnClassID { get; set; }

	public string ActingType { get; set; }

	public string ActedOnType { get; set; }

	public List<ElementInfusionBoardManager.EElement> Infused { get; private set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	public List<PositiveConditionPair> ActedOnPositiveConditions { get; private set; }

	public List<NegativeConditionPair> ActedOnNegativeConditions { get; private set; }

	public string ActingGUID { get; private set; }

	public string ActedOnGUID { get; private set; }

	public int CardID { get; private set; }

	public CBaseCard.ECardType CardType { get; private set; }

	public string ActingAbilityName { get; private set; }

	public CAbility.EAbilityType AbilityType { get; private set; }

	public int AbilityStrength { get; private set; }

	public bool PerformedBySummons { get; private set; }

	public bool RolledIntoSummoner { get; private set; }

	public int Health { get; private set; }

	public int MaximumHealth { get; private set; }

	public int AllyAdjacent { get; private set; }

	public int EnemyAdjacent { get; private set; }

	public int ObstacleAdjacent { get; private set; }

	public bool WallAdjacent { get; private set; }

	public CPlayerStatsAction()
	{
	}

	public CPlayerStatsAction(CPlayerStatsAction state, ReferenceDictionary references)
		: base(state, references)
	{
		ActingClassID = state.ActingClassID;
		ActedOnClassID = state.ActedOnClassID;
		ActingType = state.ActingType;
		ActedOnType = state.ActedOnType;
		Infused = references.Get(state.Infused);
		if (Infused == null && state.Infused != null)
		{
			Infused = new List<ElementInfusionBoardManager.EElement>();
			for (int i = 0; i < state.Infused.Count; i++)
			{
				ElementInfusionBoardManager.EElement item = state.Infused[i];
				Infused.Add(item);
			}
			references.Add(state.Infused, Infused);
		}
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
			for (int j = 0; j < state.PositiveConditions.Count; j++)
			{
				PositiveConditionPair positiveConditionPair = state.PositiveConditions[j];
				PositiveConditionPair positiveConditionPair2 = references.Get(positiveConditionPair);
				if (positiveConditionPair2 == null && positiveConditionPair != null)
				{
					positiveConditionPair2 = new PositiveConditionPair(positiveConditionPair, references);
					references.Add(positiveConditionPair, positiveConditionPair2);
				}
				PositiveConditions.Add(positiveConditionPair2);
			}
			references.Add(state.PositiveConditions, PositiveConditions);
		}
		NegativeConditions = references.Get(state.NegativeConditions);
		if (NegativeConditions == null && state.NegativeConditions != null)
		{
			NegativeConditions = new List<NegativeConditionPair>();
			for (int k = 0; k < state.NegativeConditions.Count; k++)
			{
				NegativeConditionPair negativeConditionPair = state.NegativeConditions[k];
				NegativeConditionPair negativeConditionPair2 = references.Get(negativeConditionPair);
				if (negativeConditionPair2 == null && negativeConditionPair != null)
				{
					negativeConditionPair2 = new NegativeConditionPair(negativeConditionPair, references);
					references.Add(negativeConditionPair, negativeConditionPair2);
				}
				NegativeConditions.Add(negativeConditionPair2);
			}
			references.Add(state.NegativeConditions, NegativeConditions);
		}
		ActedOnPositiveConditions = references.Get(state.ActedOnPositiveConditions);
		if (ActedOnPositiveConditions == null && state.ActedOnPositiveConditions != null)
		{
			ActedOnPositiveConditions = new List<PositiveConditionPair>();
			for (int l = 0; l < state.ActedOnPositiveConditions.Count; l++)
			{
				PositiveConditionPair positiveConditionPair3 = state.ActedOnPositiveConditions[l];
				PositiveConditionPair positiveConditionPair4 = references.Get(positiveConditionPair3);
				if (positiveConditionPair4 == null && positiveConditionPair3 != null)
				{
					positiveConditionPair4 = new PositiveConditionPair(positiveConditionPair3, references);
					references.Add(positiveConditionPair3, positiveConditionPair4);
				}
				ActedOnPositiveConditions.Add(positiveConditionPair4);
			}
			references.Add(state.ActedOnPositiveConditions, ActedOnPositiveConditions);
		}
		ActedOnNegativeConditions = references.Get(state.ActedOnNegativeConditions);
		if (ActedOnNegativeConditions == null && state.ActedOnNegativeConditions != null)
		{
			ActedOnNegativeConditions = new List<NegativeConditionPair>();
			for (int m = 0; m < state.ActedOnNegativeConditions.Count; m++)
			{
				NegativeConditionPair negativeConditionPair3 = state.ActedOnNegativeConditions[m];
				NegativeConditionPair negativeConditionPair4 = references.Get(negativeConditionPair3);
				if (negativeConditionPair4 == null && negativeConditionPair3 != null)
				{
					negativeConditionPair4 = new NegativeConditionPair(negativeConditionPair3, references);
					references.Add(negativeConditionPair3, negativeConditionPair4);
				}
				ActedOnNegativeConditions.Add(negativeConditionPair4);
			}
			references.Add(state.ActedOnNegativeConditions, ActedOnNegativeConditions);
		}
		ActingGUID = state.ActingGUID;
		ActedOnGUID = state.ActedOnGUID;
		CardID = state.CardID;
		CardType = state.CardType;
		ActingAbilityName = state.ActingAbilityName;
		AbilityType = state.AbilityType;
		AbilityStrength = state.AbilityStrength;
		PerformedBySummons = state.PerformedBySummons;
		RolledIntoSummoner = state.RolledIntoSummoner;
		Health = state.Health;
		MaximumHealth = state.MaximumHealth;
		AllyAdjacent = state.AllyAdjacent;
		EnemyAdjacent = state.EnemyAdjacent;
		ObstacleAdjacent = state.ObstacleAdjacent;
		WallAdjacent = state.WallAdjacent;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActingClassID", ActingClassID);
		info.AddValue("ActedOnClassID", ActedOnClassID);
		info.AddValue("ActingType", ActingType);
		info.AddValue("ActedOnType", ActedOnType);
		info.AddValue("Infused", Infused);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("ActedOnPositiveConditions", ActedOnPositiveConditions);
		info.AddValue("ActedOnNegativeConditions", ActedOnNegativeConditions);
		info.AddValue("ActingGUID", ActingGUID);
		info.AddValue("ActedOnGUID", ActedOnGUID);
		info.AddValue("CardID", CardID);
		info.AddValue("CardType", CardType);
		info.AddValue("ActingAbilityName", ActingAbilityName);
		info.AddValue("AbilityType", AbilityType);
		info.AddValue("AbilityStrength", AbilityStrength);
		info.AddValue("PerformedBySummons", PerformedBySummons);
		info.AddValue("RolledIntoSummoner", RolledIntoSummoner);
		info.AddValue("Health", Health);
		info.AddValue("MaximumHealth", MaximumHealth);
		info.AddValue("AllyAdjacent", AllyAdjacent);
		info.AddValue("EnemyAdjacent", EnemyAdjacent);
		info.AddValue("ObstacleAdjacent", ObstacleAdjacent);
		info.AddValue("WallAdjacent", WallAdjacent);
	}

	public CPlayerStatsAction(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ActingClassID":
					ActingClassID = info.GetString("ActingClassID");
					break;
				case "ActedOnClassID":
					ActedOnClassID = info.GetString("ActedOnClassID");
					break;
				case "ActingType":
					ActingType = info.GetString("ActingType");
					break;
				case "ActedOnType":
					ActedOnType = info.GetString("ActedOnType");
					break;
				case "Infused":
					Infused = (List<ElementInfusionBoardManager.EElement>)info.GetValue("Infused", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "ActedOnPositiveConditions":
					ActedOnPositiveConditions = (List<PositiveConditionPair>)info.GetValue("ActedOnPositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "ActedOnNegativeConditions":
					ActedOnNegativeConditions = (List<NegativeConditionPair>)info.GetValue("ActedOnNegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "ActingGUID":
					ActingGUID = info.GetString("ActingGUID");
					break;
				case "ActedOnGUID":
					ActedOnGUID = info.GetString("ActedOnGUID");
					break;
				case "CardID":
					CardID = info.GetInt32("CardID");
					break;
				case "CardType":
					CardType = (CBaseCard.ECardType)info.GetValue("CardType", typeof(CBaseCard.ECardType));
					break;
				case "ActingAbilityName":
					ActingAbilityName = info.GetString("ActingAbilityName");
					break;
				case "AbilityType":
					AbilityType = (CAbility.EAbilityType)info.GetValue("AbilityType", typeof(CAbility.EAbilityType));
					break;
				case "AbilityStrength":
					AbilityStrength = info.GetInt32("AbilityStrength");
					break;
				case "AllyAdjacent":
					AllyAdjacent = info.GetInt32("AllyAdjacent");
					break;
				case "EnemyAdjacent":
					EnemyAdjacent = info.GetInt32("EnemyAdjacent");
					break;
				case "ObstacleAdjacent":
					ObstacleAdjacent = info.GetInt32("ObstacleAdjacent");
					break;
				case "WallAdjacent":
					WallAdjacent = info.GetBoolean("WallAdjacent");
					break;
				case "ActingClass":
				{
					string text2 = info.GetString("ActingClass");
					if (text2 != null)
					{
						text2 = text2.Replace(" ", string.Empty);
						ActingClassID = text2 + "ID";
					}
					break;
				}
				case "ActedOnClass":
				{
					string text = info.GetString("ActedOnClass");
					if (text != null)
					{
						text = text.Replace(" ", string.Empty);
						ActedOnClassID = text + "ID";
					}
					break;
				}
				case "PerformedBySummons":
					PerformedBySummons = info.GetBoolean("PerformedBySummons");
					break;
				case "RolledIntoSummoner":
					PerformedBySummons = info.GetBoolean("RolledIntoSummoner");
					break;
				case "Health":
					Health = info.GetInt32("Health");
					break;
				case "MaximumHealth":
					MaximumHealth = info.GetInt32("MaximumHealth");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsAction entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsAction(string advGuid, string sceGuid, string questType, int round, string actingClassID, string actedOnClassID, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false, int health = 0, int maxHealth = 0, int allyAdjacent = 0, int enemyAdjacent = 0, int obstacleAdjacent = 0, bool wallAdjacent = false)
		: base(advGuid, sceGuid, questType, round)
	{
		ActingClassID = actingClassID;
		ActedOnClassID = actedOnClassID;
		ActingType = actingType;
		ActedOnType = actedOnType;
		Infused = infused;
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
		ActedOnPositiveConditions = actedOnpositiveConditions;
		ActedOnNegativeConditions = actedOnnegativeConditions;
		ActingGUID = actingGUID;
		ActedOnGUID = actedOnGUID;
		CardID = cardID;
		CardType = cardType;
		AbilityType = abilityType;
		ActingAbilityName = actingAbilityName;
		AbilityStrength = abilityStrength;
		PerformedBySummons = performedBySummons;
		RolledIntoSummoner = rolledIntoSummoner;
		Health = health;
		MaximumHealth = maxHealth;
		AllyAdjacent = allyAdjacent;
		EnemyAdjacent = enemyAdjacent;
		ObstacleAdjacent = obstacleAdjacent;
		WallAdjacent = wallAdjacent;
	}
}
