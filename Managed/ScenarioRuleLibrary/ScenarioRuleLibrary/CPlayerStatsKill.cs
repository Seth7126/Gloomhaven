using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsKill : CPlayerStatsAction, ISerializable
{
	public CActor.ECauseOfDeath CauseOfDeath { get; private set; }

	public int MaxHealth { get; private set; }

	public int LastDamageAmount { get; private set; }

	public int PreDamageHealth { get; private set; }

	public List<string> Items { get; private set; }

	public DateTime TimeStamp { get; private set; }

	public bool Doom { get; private set; }

	public string MonsterType { get; private set; }

	public bool Disadvantaged { get; private set; }

	public bool TargetAdjacent { get; private set; }

	public bool HasFavorite { get; private set; }

	public CPlayerStatsKill()
	{
	}

	public CPlayerStatsKill(CPlayerStatsKill state, ReferenceDictionary references)
		: base(state, references)
	{
		CauseOfDeath = state.CauseOfDeath;
		MaxHealth = state.MaxHealth;
		LastDamageAmount = state.LastDamageAmount;
		PreDamageHealth = state.PreDamageHealth;
		Items = references.Get(state.Items);
		if (Items == null && state.Items != null)
		{
			Items = new List<string>();
			for (int i = 0; i < state.Items.Count; i++)
			{
				string item = state.Items[i];
				Items.Add(item);
			}
			references.Add(state.Items, Items);
		}
		TimeStamp = state.TimeStamp;
		Doom = state.Doom;
		MonsterType = state.MonsterType;
		Disadvantaged = state.Disadvantaged;
		TargetAdjacent = state.TargetAdjacent;
		HasFavorite = state.HasFavorite;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CauseOfDeath", CauseOfDeath);
		info.AddValue("MaxHealth", MaxHealth);
		info.AddValue("LastDamageAmount", LastDamageAmount);
		info.AddValue("PreDamageHealth", PreDamageHealth);
		info.AddValue("Items", Items);
		info.AddValue("TimeStamp", TimeStamp);
		info.AddValue("Doom", Doom);
		info.AddValue("MonsterType", MonsterType);
		info.AddValue("Disadvantaged", Disadvantaged);
		info.AddValue("TargetAdjacent", TargetAdjacent);
	}

	public CPlayerStatsKill(SerializationInfo info, StreamingContext context)
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
				case "CauseOfDeath":
					CauseOfDeath = (CActor.ECauseOfDeath)info.GetValue("CauseOfDeath", typeof(CActor.ECauseOfDeath));
					break;
				case "MaxHealth":
					MaxHealth = info.GetInt32("MaxHealth");
					break;
				case "LastDamageAmount":
					LastDamageAmount = info.GetInt32("LastDamageAmount");
					break;
				case "PreDamageHealth":
					PreDamageHealth = info.GetInt32("PreDamageHealth");
					break;
				case "Items":
					Items = (List<string>)info.GetValue("Items", typeof(List<string>));
					break;
				case "TimeStamp":
					TimeStamp = (DateTime)info.GetValue("TimeStamp", typeof(DateTime));
					break;
				case "Doom":
					Doom = info.GetBoolean("Doom");
					break;
				case "MonsterType":
					MonsterType = info.GetString("MonsterType");
					break;
				case "Disadvantaged":
					Disadvantaged = info.GetBoolean("Disadvantaged");
					break;
				case "TargetAdjacent":
					TargetAdjacent = info.GetBoolean("TargetAdjacent");
					break;
				case "HasFavorite":
					HasFavorite = info.GetBoolean("HasFavorite");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsKill entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsKill(CActor.ECauseOfDeath causeOfDeath, DateTime timestamp, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int maxHealth, int lastDamageAmount, int preDamageHealth, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false, List<string> items = null, bool doom = false, string monsterType = "", bool disadvantaged = false, bool targetAdjacent = false, int allyAdjacent = 0, int enemyAdjacent = 0, int obstacleAdjacent = 0, bool wallAdjacent = false, bool hasFavorite = false)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons, rolledIntoSummoner, 0, 0, allyAdjacent, enemyAdjacent, obstacleAdjacent, wallAdjacent)
	{
		CauseOfDeath = causeOfDeath;
		MaxHealth = maxHealth;
		LastDamageAmount = lastDamageAmount;
		PreDamageHealth = preDamageHealth;
		Items = items;
		TimeStamp = timestamp;
		Doom = doom;
		MonsterType = monsterType;
		Disadvantaged = disadvantaged;
		TargetAdjacent = targetAdjacent;
		HasFavorite = hasFavorite;
	}
}
