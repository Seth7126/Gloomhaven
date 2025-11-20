using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActor : SEvent
{
	public ESESubTypeActor ActorSubType { get; private set; }

	public CActor.EType ActorType { get; private set; }

	public string ActorGuid { get; private set; }

	public string ActorClassID { get; private set; }

	public string ActorSummonerID { get; private set; }

	public int Health { get; private set; }

	public int Gold { get; private set; }

	public int XP { get; private set; }

	public int Level { get; private set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	public bool PlayedThisRound { get; private set; }

	public bool IsDead { get; private set; }

	public CActor.ECauseOfDeath CauseOfDeath { get; private set; }

	public string ActedOnByGUID { get; private set; }

	public string ActedOnByClassID { get; private set; }

	public CActor.EType? ActedOnType { get; private set; }

	public string ActedOnSummonerID { get; private set; }

	public int CardID { get; private set; }

	public CBaseCard.ECardType CardType { get; private set; }

	public CAbility.EAbilityType AbilityType { get; private set; }

	public string ActingAbilityName { get; private set; }

	public int AbilityStrength { get; private set; }

	public bool ActorEnemySummon { get; private set; }

	public bool ActedOnEnemySummon { get; private set; }

	public bool Doom { get; private set; }

	public string MonsterType { get; private set; }

	public List<PositiveConditionPair> ActedOnPositiveConditions { get; private set; }

	public List<NegativeConditionPair> ActedOnNegativeConditions { get; private set; }

	public int MaxHealth { get; private set; }

	public int LastDamageAmount { get; private set; }

	public int PreDamageHealth { get; private set; }

	public DateTime TimeStamp { get; private set; }

	public List<string> Items { get; private set; }

	public bool AttackerAtDisadvantage { get; private set; }

	public bool TargetAdjacent { get; private set; }

	public int AllyAdjacent { get; private set; }

	public int EnemyAdjacent { get; private set; }

	public int ObstacleAdjacent { get; private set; }

	public bool WallAdjacent { get; private set; }

	public bool HasFavorite { get; private set; }

	public SEventActor()
	{
	}

	public SEventActor(SEventActor state, ReferenceDictionary references)
		: base(state, references)
	{
		ActorSubType = state.ActorSubType;
		ActorType = state.ActorType;
		ActorGuid = state.ActorGuid;
		ActorClassID = state.ActorClassID;
		ActorSummonerID = state.ActorSummonerID;
		Health = state.Health;
		Gold = state.Gold;
		XP = state.XP;
		Level = state.Level;
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
			for (int i = 0; i < state.PositiveConditions.Count; i++)
			{
				PositiveConditionPair positiveConditionPair = state.PositiveConditions[i];
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
			for (int j = 0; j < state.NegativeConditions.Count; j++)
			{
				NegativeConditionPair negativeConditionPair = state.NegativeConditions[j];
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
		PlayedThisRound = state.PlayedThisRound;
		IsDead = state.IsDead;
		CauseOfDeath = state.CauseOfDeath;
		ActedOnByGUID = state.ActedOnByGUID;
		ActedOnByClassID = state.ActedOnByClassID;
		ActedOnType = state.ActedOnType;
		ActedOnSummonerID = state.ActedOnSummonerID;
		CardID = state.CardID;
		CardType = state.CardType;
		AbilityType = state.AbilityType;
		ActingAbilityName = state.ActingAbilityName;
		AbilityStrength = state.AbilityStrength;
		ActorEnemySummon = state.ActorEnemySummon;
		ActedOnEnemySummon = state.ActedOnEnemySummon;
		Doom = state.Doom;
		MonsterType = state.MonsterType;
		ActedOnPositiveConditions = references.Get(state.ActedOnPositiveConditions);
		if (ActedOnPositiveConditions == null && state.ActedOnPositiveConditions != null)
		{
			ActedOnPositiveConditions = new List<PositiveConditionPair>();
			for (int k = 0; k < state.ActedOnPositiveConditions.Count; k++)
			{
				PositiveConditionPair positiveConditionPair3 = state.ActedOnPositiveConditions[k];
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
			for (int l = 0; l < state.ActedOnNegativeConditions.Count; l++)
			{
				NegativeConditionPair negativeConditionPair3 = state.ActedOnNegativeConditions[l];
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
		MaxHealth = state.MaxHealth;
		LastDamageAmount = state.LastDamageAmount;
		PreDamageHealth = state.PreDamageHealth;
		TimeStamp = state.TimeStamp;
		Items = references.Get(state.Items);
		if (Items == null && state.Items != null)
		{
			Items = new List<string>();
			for (int m = 0; m < state.Items.Count; m++)
			{
				string item = state.Items[m];
				Items.Add(item);
			}
			references.Add(state.Items, Items);
		}
		AttackerAtDisadvantage = state.AttackerAtDisadvantage;
		TargetAdjacent = state.TargetAdjacent;
		AllyAdjacent = state.AllyAdjacent;
		EnemyAdjacent = state.EnemyAdjacent;
		ObstacleAdjacent = state.ObstacleAdjacent;
		WallAdjacent = state.WallAdjacent;
		HasFavorite = state.HasFavorite;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActorSubType", ActorSubType);
		info.AddValue("ActorType", ActorType);
		info.AddValue("ActorGuid", ActorGuid);
		info.AddValue("ActorClassID", ActorClassID);
		info.AddValue("ActorSummonerID", ActorSummonerID);
		info.AddValue("Health", Health);
		info.AddValue("Gold", Gold);
		info.AddValue("XP", XP);
		info.AddValue("Level", Level);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("PlayedThisRound", PlayedThisRound);
		info.AddValue("IsDead", IsDead);
		info.AddValue("CauseOfDeath", CauseOfDeath);
		info.AddValue("ActedOnByGUID", ActedOnByGUID);
		info.AddValue("ActedOnByClassID", ActedOnByClassID);
		info.AddValue("ActedOnType", ActedOnType);
		info.AddValue("ActedOnSummonerID", ActorSummonerID);
		info.AddValue("CardID", CardID);
		info.AddValue("CardType", CardType);
		info.AddValue("AbilityType", AbilityType);
		info.AddValue("ActingAbilityName", ActingAbilityName);
		info.AddValue("AbilityStrength", AbilityStrength);
		info.AddValue("ActorEnemySummon", ActorEnemySummon);
		info.AddValue("ActedOnEnemySummon", ActorEnemySummon);
		info.AddValue("Doom", Doom);
		info.AddValue("MonsterType", MonsterType);
		info.AddValue("ActedOnPositiveConditions", ActedOnPositiveConditions);
		info.AddValue("ActedOnNegativeConditions", ActedOnNegativeConditions);
		info.AddValue("MaxHealth", MaxHealth);
		info.AddValue("LastDamageAmount", LastDamageAmount);
		info.AddValue("PreDamageHealth", PreDamageHealth);
		info.AddValue("TimeStamp", TimeStamp);
		info.AddValue("AttackerAtDisadvantage", AttackerAtDisadvantage);
		info.AddValue("TargetAdjacent", TargetAdjacent);
		info.AddValue("AllyAdjacent", AllyAdjacent);
		info.AddValue("EnemyAdjacent", EnemyAdjacent);
		info.AddValue("ObstacleAdjacent", ObstacleAdjacent);
		info.AddValue("WallAdjacent", WallAdjacent);
		info.AddValue("HasFavorite", HasFavorite);
	}

	public SEventActor(SerializationInfo info, StreamingContext context)
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
				case "ActorSubType":
					ActorSubType = (ESESubTypeActor)info.GetValue("ActorSubType", typeof(ESESubTypeActor));
					break;
				case "ActorType":
					ActorType = (CActor.EType)info.GetValue("ActorType", typeof(CActor.EType));
					break;
				case "ActorGuid":
					ActorGuid = info.GetString("ActorGuid");
					break;
				case "ActorClassID":
					ActorClassID = info.GetString("ActorClassID");
					break;
				case "ActorSummonerID":
					ActorSummonerID = info.GetString("ActorSummonerID");
					break;
				case "Health":
					Health = info.GetInt32("Health");
					break;
				case "Gold":
					Gold = info.GetInt32("Gold");
					break;
				case "XP":
					XP = info.GetInt32("XP");
					break;
				case "Level":
					Level = info.GetInt32("Level");
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "PlayedThisRound":
					PlayedThisRound = info.GetBoolean("PlayedThisRound");
					break;
				case "IsDead":
					IsDead = info.GetBoolean("IsDead");
					break;
				case "CauseOfDeath":
					CauseOfDeath = (CActor.ECauseOfDeath)info.GetValue("CauseOfDeath", typeof(CActor.ECauseOfDeath));
					break;
				case "ActedOnByGUID":
					ActedOnByGUID = info.GetString("ActedOnByGUID");
					break;
				case "ActedOnByClassID":
					ActedOnByClassID = info.GetString("ActedOnByClassID");
					break;
				case "ActedOnType":
					ActedOnType = (CActor.EType?)info.GetValue("ActedOnType", typeof(CActor.EType?));
					break;
				case "ActedOnSummonerID":
					ActedOnSummonerID = info.GetString("ActedOnSummonerID");
					break;
				case "CardID":
					CardID = info.GetInt32("CardID");
					break;
				case "CardType":
					CardType = (CBaseCard.ECardType)info.GetValue("CardType", typeof(CBaseCard.ECardType));
					break;
				case "AbilityType":
					AbilityType = (CAbility.EAbilityType)info.GetValue("AbilityType", typeof(CAbility.EAbilityType));
					break;
				case "ActingAbilityName":
					ActingAbilityName = info.GetString("ActingAbilityName");
					break;
				case "AbilityStrength":
					AbilityStrength = info.GetInt32("AbilityStrength");
					break;
				case "ActorEnemySummon":
					ActorEnemySummon = info.GetBoolean("ActorEnemySummon");
					break;
				case "ActedOnEnemySummon":
					ActedOnEnemySummon = info.GetBoolean("ActedOnEnemySummon");
					break;
				case "Doom":
					Doom = info.GetBoolean("Doom");
					break;
				case "MonsterType":
					MonsterType = info.GetString("MonsterType");
					break;
				case "ActedOnPositiveConditions":
					ActedOnPositiveConditions = (List<PositiveConditionPair>)info.GetValue("ActedOnPositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "ActedOnNegativeConditions":
					ActedOnNegativeConditions = (List<NegativeConditionPair>)info.GetValue("ActedOnNegativeConditions", typeof(List<NegativeConditionPair>));
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
				case "TimeStamp":
					TimeStamp = (DateTime)info.GetValue("TimeStamp", typeof(DateTime));
					break;
				case "AttackerAtDisadvantage":
					AttackerAtDisadvantage = info.GetBoolean("AttackerAtDisadvantage");
					break;
				case "TargetAdjacent":
					TargetAdjacent = info.GetBoolean("TargetAdjacent");
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
				case "HasFavorite":
					HasFavorite = info.GetBoolean("HasFavorite");
					break;
				case "ActorClass":
				{
					string text2 = info.GetString("ActorClass");
					if (text2 != null)
					{
						text2 = text2.Replace(" ", string.Empty);
						ActorClassID = text2 + "ID";
					}
					break;
				}
				case "ActedOnByClass":
				{
					string text = info.GetString("ActedOnByClass");
					if (text != null)
					{
						text = text.Replace(" ", string.Empty);
						ActedOnByClassID = text + "ID";
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventActor entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActor(ESESubTypeActor actorEventSubType, CActor.EType actorType, string actorGuid, string actorClassID, int health, int gold, int xp, int level, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, bool playedThisRound, bool isDead, CActor.ECauseOfDeath causeOfDeath, bool IsSummon, string actedOnByGUID = "", string actedOnByClassID = "", CActor.EType? actedOnType = null, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, bool actedOnEnemySummon = false, List<PositiveConditionPair> actedOnPositiveConditions = null, List<NegativeConditionPair> actedOnNegativeConditions = null, string text = "", bool doNotSerialize = false, int maxHealth = 0, int lastDamageAmount = 0, int preDamageHealth = 0, List<string> items = null, bool doom = false, string monsterType = "", bool attackerDisadvantage = false, bool targetAdjacent = false, int allyAdjacent = 0, int enemyAdjacent = 0, int obstacleAdjacent = 0, bool wallAdjacent = false, bool hasFavorite = false)
		: base(ESEType.Actor, text, doNotSerialize)
	{
		SEventActor sEventActor = this;
		ActorSubType = actorEventSubType;
		ActorType = actorType;
		ActorGuid = actorGuid;
		ActorClassID = actorClassID;
		Health = health;
		Gold = gold;
		XP = xp;
		Level = level;
		PositiveConditions = positiveConditions?.ToList();
		NegativeConditions = negativeConditions?.ToList();
		PlayedThisRound = playedThisRound;
		IsDead = isDead;
		CauseOfDeath = causeOfDeath;
		ActorEnemySummon = IsSummon;
		ActedOnByGUID = actedOnByGUID;
		ActedOnByClassID = actedOnByClassID;
		ActedOnType = actedOnType;
		CardID = cardID;
		CardType = cardType;
		AbilityType = abilityType;
		ActingAbilityName = actingAbilityName;
		AbilityStrength = abilityStrength;
		ActedOnEnemySummon = actedOnEnemySummon;
		ActedOnPositiveConditions = actedOnPositiveConditions?.ToList();
		ActedOnNegativeConditions = actedOnNegativeConditions?.ToList();
		MaxHealth = maxHealth;
		LastDamageAmount = lastDamageAmount;
		PreDamageHealth = preDamageHealth;
		TimeStamp = DateTime.Now;
		Items = items;
		Doom = doom;
		MonsterType = monsterType;
		AttackerAtDisadvantage = attackerDisadvantage;
		TargetAdjacent = targetAdjacent;
		AllyAdjacent = allyAdjacent;
		EnemyAdjacent = enemyAdjacent;
		ObstacleAdjacent = obstacleAdjacent;
		WallAdjacent = wallAdjacent;
		HasFavorite = hasFavorite;
		if (ActorType == CActor.EType.HeroSummon)
		{
			CHeroSummonActor summons = ScenarioManager.Scenario.HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == actorGuid);
			if (summons != null)
			{
				CActor cActor = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == summons.Summoner.ActorGuid);
				if (cActor != null)
				{
					ActorSummonerID = cActor.Class.ID;
				}
			}
		}
		if (ActedOnType != CActor.EType.HeroSummon)
		{
			return;
		}
		CHeroSummonActor summons2 = ScenarioManager.Scenario.HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == sEventActor.ActedOnByGUID);
		if (summons2 != null)
		{
			CActor cActor2 = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == summons2.Summoner.ActorGuid);
			if (cActor2 != null)
			{
				ActedOnSummonerID = cActor2.Class.ID;
			}
		}
	}
}
