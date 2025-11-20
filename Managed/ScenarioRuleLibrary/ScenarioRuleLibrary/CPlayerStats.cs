using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStats : ISerializable
{
	public string CharacterID { get; private set; }

	public List<CPlayerStatsKill> Kills { get; private set; }

	public List<CPlayerStatsKill> Deaths { get; private set; }

	public List<CPlayerStatsDamage> DamageDealt { get; private set; }

	public List<CPlayerStatsDamage> DamageReceived { get; private set; }

	public List<CPlayerStatsDamage> Actor { get; private set; }

	public List<CPlayerStatsDestroyObstacle> DestroyedObstacles { get; private set; }

	public List<CPlayerStatsElement> Infusions { get; private set; }

	public List<CPlayerStatsElement> Consumed { get; private set; }

	public List<CPlayerStatsAbilities> Abilities { get; private set; }

	public List<CPlayerStatsModifiers> Modifiers { get; private set; }

	public List<CPlayerStatsHeal> Heals { get; private set; }

	public List<CPlayerStatsItem> Items { get; private set; }

	public List<CPlayerStatsLoot> Loot { get; private set; }

	public List<CPlayerStatsMonsters> Monsters { get; private set; }

	public List<CPlayerStatsDoor> Door { get; private set; }

	public List<CPlayerStatsTrap> Trap { get; private set; }

	public List<CPlayerStatsHand> Hand { get; private set; }

	public List<CPlayerStatsXP> XP { get; private set; }

	public List<CPlayerStatsBattlePerks> BattleGoalPerks { get; private set; }

	public List<CPlayerStatsDonations> Donations { get; private set; }

	public List<CPlayerStatsPersonalQuests> PersonalQuests { get; private set; }

	public List<CPlayerStatsEnhancements> Enhancements { get; private set; }

	public List<CPlayerStatsLoseCard> LoseCards { get; private set; }

	public List<CPlayerStatsDiscardCard> DiscardCard { get; private set; }

	public List<CPlayerStatsEndTurn> EndTurn { get; private set; }

	public List<CPlayerStatsLostAdjacency> LostAdjacency { get; private set; }

	public CPlayerStats()
	{
	}

	public CPlayerStats(CPlayerStats state, ReferenceDictionary references)
	{
		CharacterID = state.CharacterID;
		Kills = references.Get(state.Kills);
		if (Kills == null && state.Kills != null)
		{
			Kills = new List<CPlayerStatsKill>();
			for (int i = 0; i < state.Kills.Count; i++)
			{
				CPlayerStatsKill cPlayerStatsKill = state.Kills[i];
				CPlayerStatsKill cPlayerStatsKill2 = references.Get(cPlayerStatsKill);
				if (cPlayerStatsKill2 == null && cPlayerStatsKill != null)
				{
					cPlayerStatsKill2 = new CPlayerStatsKill(cPlayerStatsKill, references);
					references.Add(cPlayerStatsKill, cPlayerStatsKill2);
				}
				Kills.Add(cPlayerStatsKill2);
			}
			references.Add(state.Kills, Kills);
		}
		Deaths = references.Get(state.Deaths);
		if (Deaths == null && state.Deaths != null)
		{
			Deaths = new List<CPlayerStatsKill>();
			for (int j = 0; j < state.Deaths.Count; j++)
			{
				CPlayerStatsKill cPlayerStatsKill3 = state.Deaths[j];
				CPlayerStatsKill cPlayerStatsKill4 = references.Get(cPlayerStatsKill3);
				if (cPlayerStatsKill4 == null && cPlayerStatsKill3 != null)
				{
					cPlayerStatsKill4 = new CPlayerStatsKill(cPlayerStatsKill3, references);
					references.Add(cPlayerStatsKill3, cPlayerStatsKill4);
				}
				Deaths.Add(cPlayerStatsKill4);
			}
			references.Add(state.Deaths, Deaths);
		}
		DamageDealt = references.Get(state.DamageDealt);
		if (DamageDealt == null && state.DamageDealt != null)
		{
			DamageDealt = new List<CPlayerStatsDamage>();
			for (int k = 0; k < state.DamageDealt.Count; k++)
			{
				CPlayerStatsDamage cPlayerStatsDamage = state.DamageDealt[k];
				CPlayerStatsDamage cPlayerStatsDamage2 = references.Get(cPlayerStatsDamage);
				if (cPlayerStatsDamage2 == null && cPlayerStatsDamage != null)
				{
					cPlayerStatsDamage2 = new CPlayerStatsDamage(cPlayerStatsDamage, references);
					references.Add(cPlayerStatsDamage, cPlayerStatsDamage2);
				}
				DamageDealt.Add(cPlayerStatsDamage2);
			}
			references.Add(state.DamageDealt, DamageDealt);
		}
		DamageReceived = references.Get(state.DamageReceived);
		if (DamageReceived == null && state.DamageReceived != null)
		{
			DamageReceived = new List<CPlayerStatsDamage>();
			for (int l = 0; l < state.DamageReceived.Count; l++)
			{
				CPlayerStatsDamage cPlayerStatsDamage3 = state.DamageReceived[l];
				CPlayerStatsDamage cPlayerStatsDamage4 = references.Get(cPlayerStatsDamage3);
				if (cPlayerStatsDamage4 == null && cPlayerStatsDamage3 != null)
				{
					cPlayerStatsDamage4 = new CPlayerStatsDamage(cPlayerStatsDamage3, references);
					references.Add(cPlayerStatsDamage3, cPlayerStatsDamage4);
				}
				DamageReceived.Add(cPlayerStatsDamage4);
			}
			references.Add(state.DamageReceived, DamageReceived);
		}
		Actor = references.Get(state.Actor);
		if (Actor == null && state.Actor != null)
		{
			Actor = new List<CPlayerStatsDamage>();
			for (int m = 0; m < state.Actor.Count; m++)
			{
				CPlayerStatsDamage cPlayerStatsDamage5 = state.Actor[m];
				CPlayerStatsDamage cPlayerStatsDamage6 = references.Get(cPlayerStatsDamage5);
				if (cPlayerStatsDamage6 == null && cPlayerStatsDamage5 != null)
				{
					cPlayerStatsDamage6 = new CPlayerStatsDamage(cPlayerStatsDamage5, references);
					references.Add(cPlayerStatsDamage5, cPlayerStatsDamage6);
				}
				Actor.Add(cPlayerStatsDamage6);
			}
			references.Add(state.Actor, Actor);
		}
		DestroyedObstacles = references.Get(state.DestroyedObstacles);
		if (DestroyedObstacles == null && state.DestroyedObstacles != null)
		{
			DestroyedObstacles = new List<CPlayerStatsDestroyObstacle>();
			for (int n = 0; n < state.DestroyedObstacles.Count; n++)
			{
				CPlayerStatsDestroyObstacle cPlayerStatsDestroyObstacle = state.DestroyedObstacles[n];
				CPlayerStatsDestroyObstacle cPlayerStatsDestroyObstacle2 = references.Get(cPlayerStatsDestroyObstacle);
				if (cPlayerStatsDestroyObstacle2 == null && cPlayerStatsDestroyObstacle != null)
				{
					cPlayerStatsDestroyObstacle2 = new CPlayerStatsDestroyObstacle(cPlayerStatsDestroyObstacle, references);
					references.Add(cPlayerStatsDestroyObstacle, cPlayerStatsDestroyObstacle2);
				}
				DestroyedObstacles.Add(cPlayerStatsDestroyObstacle2);
			}
			references.Add(state.DestroyedObstacles, DestroyedObstacles);
		}
		Infusions = references.Get(state.Infusions);
		if (Infusions == null && state.Infusions != null)
		{
			Infusions = new List<CPlayerStatsElement>();
			for (int num = 0; num < state.Infusions.Count; num++)
			{
				CPlayerStatsElement cPlayerStatsElement = state.Infusions[num];
				CPlayerStatsElement cPlayerStatsElement2 = references.Get(cPlayerStatsElement);
				if (cPlayerStatsElement2 == null && cPlayerStatsElement != null)
				{
					cPlayerStatsElement2 = new CPlayerStatsElement(cPlayerStatsElement, references);
					references.Add(cPlayerStatsElement, cPlayerStatsElement2);
				}
				Infusions.Add(cPlayerStatsElement2);
			}
			references.Add(state.Infusions, Infusions);
		}
		Consumed = references.Get(state.Consumed);
		if (Consumed == null && state.Consumed != null)
		{
			Consumed = new List<CPlayerStatsElement>();
			for (int num2 = 0; num2 < state.Consumed.Count; num2++)
			{
				CPlayerStatsElement cPlayerStatsElement3 = state.Consumed[num2];
				CPlayerStatsElement cPlayerStatsElement4 = references.Get(cPlayerStatsElement3);
				if (cPlayerStatsElement4 == null && cPlayerStatsElement3 != null)
				{
					cPlayerStatsElement4 = new CPlayerStatsElement(cPlayerStatsElement3, references);
					references.Add(cPlayerStatsElement3, cPlayerStatsElement4);
				}
				Consumed.Add(cPlayerStatsElement4);
			}
			references.Add(state.Consumed, Consumed);
		}
		Abilities = references.Get(state.Abilities);
		if (Abilities == null && state.Abilities != null)
		{
			Abilities = new List<CPlayerStatsAbilities>();
			for (int num3 = 0; num3 < state.Abilities.Count; num3++)
			{
				CPlayerStatsAbilities cPlayerStatsAbilities = state.Abilities[num3];
				CPlayerStatsAbilities cPlayerStatsAbilities2 = references.Get(cPlayerStatsAbilities);
				if (cPlayerStatsAbilities2 == null && cPlayerStatsAbilities != null)
				{
					cPlayerStatsAbilities2 = new CPlayerStatsAbilities(cPlayerStatsAbilities, references);
					references.Add(cPlayerStatsAbilities, cPlayerStatsAbilities2);
				}
				Abilities.Add(cPlayerStatsAbilities2);
			}
			references.Add(state.Abilities, Abilities);
		}
		Modifiers = references.Get(state.Modifiers);
		if (Modifiers == null && state.Modifiers != null)
		{
			Modifiers = new List<CPlayerStatsModifiers>();
			for (int num4 = 0; num4 < state.Modifiers.Count; num4++)
			{
				CPlayerStatsModifiers cPlayerStatsModifiers = state.Modifiers[num4];
				CPlayerStatsModifiers cPlayerStatsModifiers2 = references.Get(cPlayerStatsModifiers);
				if (cPlayerStatsModifiers2 == null && cPlayerStatsModifiers != null)
				{
					cPlayerStatsModifiers2 = new CPlayerStatsModifiers(cPlayerStatsModifiers, references);
					references.Add(cPlayerStatsModifiers, cPlayerStatsModifiers2);
				}
				Modifiers.Add(cPlayerStatsModifiers2);
			}
			references.Add(state.Modifiers, Modifiers);
		}
		Heals = references.Get(state.Heals);
		if (Heals == null && state.Heals != null)
		{
			Heals = new List<CPlayerStatsHeal>();
			for (int num5 = 0; num5 < state.Heals.Count; num5++)
			{
				CPlayerStatsHeal cPlayerStatsHeal = state.Heals[num5];
				CPlayerStatsHeal cPlayerStatsHeal2 = references.Get(cPlayerStatsHeal);
				if (cPlayerStatsHeal2 == null && cPlayerStatsHeal != null)
				{
					cPlayerStatsHeal2 = new CPlayerStatsHeal(cPlayerStatsHeal, references);
					references.Add(cPlayerStatsHeal, cPlayerStatsHeal2);
				}
				Heals.Add(cPlayerStatsHeal2);
			}
			references.Add(state.Heals, Heals);
		}
		Items = references.Get(state.Items);
		if (Items == null && state.Items != null)
		{
			Items = new List<CPlayerStatsItem>();
			for (int num6 = 0; num6 < state.Items.Count; num6++)
			{
				CPlayerStatsItem cPlayerStatsItem = state.Items[num6];
				CPlayerStatsItem cPlayerStatsItem2 = references.Get(cPlayerStatsItem);
				if (cPlayerStatsItem2 == null && cPlayerStatsItem != null)
				{
					cPlayerStatsItem2 = new CPlayerStatsItem(cPlayerStatsItem, references);
					references.Add(cPlayerStatsItem, cPlayerStatsItem2);
				}
				Items.Add(cPlayerStatsItem2);
			}
			references.Add(state.Items, Items);
		}
		Loot = references.Get(state.Loot);
		if (Loot == null && state.Loot != null)
		{
			Loot = new List<CPlayerStatsLoot>();
			for (int num7 = 0; num7 < state.Loot.Count; num7++)
			{
				CPlayerStatsLoot cPlayerStatsLoot = state.Loot[num7];
				CPlayerStatsLoot cPlayerStatsLoot2 = references.Get(cPlayerStatsLoot);
				if (cPlayerStatsLoot2 == null && cPlayerStatsLoot != null)
				{
					cPlayerStatsLoot2 = new CPlayerStatsLoot(cPlayerStatsLoot, references);
					references.Add(cPlayerStatsLoot, cPlayerStatsLoot2);
				}
				Loot.Add(cPlayerStatsLoot2);
			}
			references.Add(state.Loot, Loot);
		}
		Monsters = references.Get(state.Monsters);
		if (Monsters == null && state.Monsters != null)
		{
			Monsters = new List<CPlayerStatsMonsters>();
			for (int num8 = 0; num8 < state.Monsters.Count; num8++)
			{
				CPlayerStatsMonsters cPlayerStatsMonsters = state.Monsters[num8];
				CPlayerStatsMonsters cPlayerStatsMonsters2 = references.Get(cPlayerStatsMonsters);
				if (cPlayerStatsMonsters2 == null && cPlayerStatsMonsters != null)
				{
					cPlayerStatsMonsters2 = new CPlayerStatsMonsters(cPlayerStatsMonsters, references);
					references.Add(cPlayerStatsMonsters, cPlayerStatsMonsters2);
				}
				Monsters.Add(cPlayerStatsMonsters2);
			}
			references.Add(state.Monsters, Monsters);
		}
		Door = references.Get(state.Door);
		if (Door == null && state.Door != null)
		{
			Door = new List<CPlayerStatsDoor>();
			for (int num9 = 0; num9 < state.Door.Count; num9++)
			{
				CPlayerStatsDoor cPlayerStatsDoor = state.Door[num9];
				CPlayerStatsDoor cPlayerStatsDoor2 = references.Get(cPlayerStatsDoor);
				if (cPlayerStatsDoor2 == null && cPlayerStatsDoor != null)
				{
					cPlayerStatsDoor2 = new CPlayerStatsDoor(cPlayerStatsDoor, references);
					references.Add(cPlayerStatsDoor, cPlayerStatsDoor2);
				}
				Door.Add(cPlayerStatsDoor2);
			}
			references.Add(state.Door, Door);
		}
		Trap = references.Get(state.Trap);
		if (Trap == null && state.Trap != null)
		{
			Trap = new List<CPlayerStatsTrap>();
			for (int num10 = 0; num10 < state.Trap.Count; num10++)
			{
				CPlayerStatsTrap cPlayerStatsTrap = state.Trap[num10];
				CPlayerStatsTrap cPlayerStatsTrap2 = references.Get(cPlayerStatsTrap);
				if (cPlayerStatsTrap2 == null && cPlayerStatsTrap != null)
				{
					cPlayerStatsTrap2 = new CPlayerStatsTrap(cPlayerStatsTrap, references);
					references.Add(cPlayerStatsTrap, cPlayerStatsTrap2);
				}
				Trap.Add(cPlayerStatsTrap2);
			}
			references.Add(state.Trap, Trap);
		}
		Hand = references.Get(state.Hand);
		if (Hand == null && state.Hand != null)
		{
			Hand = new List<CPlayerStatsHand>();
			for (int num11 = 0; num11 < state.Hand.Count; num11++)
			{
				CPlayerStatsHand cPlayerStatsHand = state.Hand[num11];
				CPlayerStatsHand cPlayerStatsHand2 = references.Get(cPlayerStatsHand);
				if (cPlayerStatsHand2 == null && cPlayerStatsHand != null)
				{
					cPlayerStatsHand2 = new CPlayerStatsHand(cPlayerStatsHand, references);
					references.Add(cPlayerStatsHand, cPlayerStatsHand2);
				}
				Hand.Add(cPlayerStatsHand2);
			}
			references.Add(state.Hand, Hand);
		}
		XP = references.Get(state.XP);
		if (XP == null && state.XP != null)
		{
			XP = new List<CPlayerStatsXP>();
			for (int num12 = 0; num12 < state.XP.Count; num12++)
			{
				CPlayerStatsXP cPlayerStatsXP = state.XP[num12];
				CPlayerStatsXP cPlayerStatsXP2 = references.Get(cPlayerStatsXP);
				if (cPlayerStatsXP2 == null && cPlayerStatsXP != null)
				{
					cPlayerStatsXP2 = new CPlayerStatsXP(cPlayerStatsXP, references);
					references.Add(cPlayerStatsXP, cPlayerStatsXP2);
				}
				XP.Add(cPlayerStatsXP2);
			}
			references.Add(state.XP, XP);
		}
		BattleGoalPerks = references.Get(state.BattleGoalPerks);
		if (BattleGoalPerks == null && state.BattleGoalPerks != null)
		{
			BattleGoalPerks = new List<CPlayerStatsBattlePerks>();
			for (int num13 = 0; num13 < state.BattleGoalPerks.Count; num13++)
			{
				CPlayerStatsBattlePerks cPlayerStatsBattlePerks = state.BattleGoalPerks[num13];
				CPlayerStatsBattlePerks cPlayerStatsBattlePerks2 = references.Get(cPlayerStatsBattlePerks);
				if (cPlayerStatsBattlePerks2 == null && cPlayerStatsBattlePerks != null)
				{
					cPlayerStatsBattlePerks2 = new CPlayerStatsBattlePerks(cPlayerStatsBattlePerks, references);
					references.Add(cPlayerStatsBattlePerks, cPlayerStatsBattlePerks2);
				}
				BattleGoalPerks.Add(cPlayerStatsBattlePerks2);
			}
			references.Add(state.BattleGoalPerks, BattleGoalPerks);
		}
		Donations = references.Get(state.Donations);
		if (Donations == null && state.Donations != null)
		{
			Donations = new List<CPlayerStatsDonations>();
			for (int num14 = 0; num14 < state.Donations.Count; num14++)
			{
				CPlayerStatsDonations cPlayerStatsDonations = state.Donations[num14];
				CPlayerStatsDonations cPlayerStatsDonations2 = references.Get(cPlayerStatsDonations);
				if (cPlayerStatsDonations2 == null && cPlayerStatsDonations != null)
				{
					cPlayerStatsDonations2 = new CPlayerStatsDonations(cPlayerStatsDonations, references);
					references.Add(cPlayerStatsDonations, cPlayerStatsDonations2);
				}
				Donations.Add(cPlayerStatsDonations2);
			}
			references.Add(state.Donations, Donations);
		}
		PersonalQuests = references.Get(state.PersonalQuests);
		if (PersonalQuests == null && state.PersonalQuests != null)
		{
			PersonalQuests = new List<CPlayerStatsPersonalQuests>();
			for (int num15 = 0; num15 < state.PersonalQuests.Count; num15++)
			{
				CPlayerStatsPersonalQuests cPlayerStatsPersonalQuests = state.PersonalQuests[num15];
				CPlayerStatsPersonalQuests cPlayerStatsPersonalQuests2 = references.Get(cPlayerStatsPersonalQuests);
				if (cPlayerStatsPersonalQuests2 == null && cPlayerStatsPersonalQuests != null)
				{
					cPlayerStatsPersonalQuests2 = new CPlayerStatsPersonalQuests(cPlayerStatsPersonalQuests, references);
					references.Add(cPlayerStatsPersonalQuests, cPlayerStatsPersonalQuests2);
				}
				PersonalQuests.Add(cPlayerStatsPersonalQuests2);
			}
			references.Add(state.PersonalQuests, PersonalQuests);
		}
		Enhancements = references.Get(state.Enhancements);
		if (Enhancements == null && state.Enhancements != null)
		{
			Enhancements = new List<CPlayerStatsEnhancements>();
			for (int num16 = 0; num16 < state.Enhancements.Count; num16++)
			{
				CPlayerStatsEnhancements cPlayerStatsEnhancements = state.Enhancements[num16];
				CPlayerStatsEnhancements cPlayerStatsEnhancements2 = references.Get(cPlayerStatsEnhancements);
				if (cPlayerStatsEnhancements2 == null && cPlayerStatsEnhancements != null)
				{
					cPlayerStatsEnhancements2 = new CPlayerStatsEnhancements(cPlayerStatsEnhancements, references);
					references.Add(cPlayerStatsEnhancements, cPlayerStatsEnhancements2);
				}
				Enhancements.Add(cPlayerStatsEnhancements2);
			}
			references.Add(state.Enhancements, Enhancements);
		}
		LoseCards = references.Get(state.LoseCards);
		if (LoseCards == null && state.LoseCards != null)
		{
			LoseCards = new List<CPlayerStatsLoseCard>();
			for (int num17 = 0; num17 < state.LoseCards.Count; num17++)
			{
				CPlayerStatsLoseCard cPlayerStatsLoseCard = state.LoseCards[num17];
				CPlayerStatsLoseCard cPlayerStatsLoseCard2 = references.Get(cPlayerStatsLoseCard);
				if (cPlayerStatsLoseCard2 == null && cPlayerStatsLoseCard != null)
				{
					cPlayerStatsLoseCard2 = new CPlayerStatsLoseCard(cPlayerStatsLoseCard, references);
					references.Add(cPlayerStatsLoseCard, cPlayerStatsLoseCard2);
				}
				LoseCards.Add(cPlayerStatsLoseCard2);
			}
			references.Add(state.LoseCards, LoseCards);
		}
		DiscardCard = references.Get(state.DiscardCard);
		if (DiscardCard == null && state.DiscardCard != null)
		{
			DiscardCard = new List<CPlayerStatsDiscardCard>();
			for (int num18 = 0; num18 < state.DiscardCard.Count; num18++)
			{
				CPlayerStatsDiscardCard cPlayerStatsDiscardCard = state.DiscardCard[num18];
				CPlayerStatsDiscardCard cPlayerStatsDiscardCard2 = references.Get(cPlayerStatsDiscardCard);
				if (cPlayerStatsDiscardCard2 == null && cPlayerStatsDiscardCard != null)
				{
					cPlayerStatsDiscardCard2 = new CPlayerStatsDiscardCard(cPlayerStatsDiscardCard, references);
					references.Add(cPlayerStatsDiscardCard, cPlayerStatsDiscardCard2);
				}
				DiscardCard.Add(cPlayerStatsDiscardCard2);
			}
			references.Add(state.DiscardCard, DiscardCard);
		}
		EndTurn = references.Get(state.EndTurn);
		if (EndTurn == null && state.EndTurn != null)
		{
			EndTurn = new List<CPlayerStatsEndTurn>();
			for (int num19 = 0; num19 < state.EndTurn.Count; num19++)
			{
				CPlayerStatsEndTurn cPlayerStatsEndTurn = state.EndTurn[num19];
				CPlayerStatsEndTurn cPlayerStatsEndTurn2 = references.Get(cPlayerStatsEndTurn);
				if (cPlayerStatsEndTurn2 == null && cPlayerStatsEndTurn != null)
				{
					cPlayerStatsEndTurn2 = new CPlayerStatsEndTurn(cPlayerStatsEndTurn, references);
					references.Add(cPlayerStatsEndTurn, cPlayerStatsEndTurn2);
				}
				EndTurn.Add(cPlayerStatsEndTurn2);
			}
			references.Add(state.EndTurn, EndTurn);
		}
		LostAdjacency = references.Get(state.LostAdjacency);
		if (LostAdjacency != null || state.LostAdjacency == null)
		{
			return;
		}
		LostAdjacency = new List<CPlayerStatsLostAdjacency>();
		for (int num20 = 0; num20 < state.LostAdjacency.Count; num20++)
		{
			CPlayerStatsLostAdjacency cPlayerStatsLostAdjacency = state.LostAdjacency[num20];
			CPlayerStatsLostAdjacency cPlayerStatsLostAdjacency2 = references.Get(cPlayerStatsLostAdjacency);
			if (cPlayerStatsLostAdjacency2 == null && cPlayerStatsLostAdjacency != null)
			{
				cPlayerStatsLostAdjacency2 = new CPlayerStatsLostAdjacency(cPlayerStatsLostAdjacency, references);
				references.Add(cPlayerStatsLostAdjacency, cPlayerStatsLostAdjacency2);
			}
			LostAdjacency.Add(cPlayerStatsLostAdjacency2);
		}
		references.Add(state.LostAdjacency, LostAdjacency);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("Kills", Kills);
		info.AddValue("Deaths", Deaths);
		info.AddValue("DamageDealt", DamageDealt);
		info.AddValue("DamageReceived", DamageReceived);
		info.AddValue("Actor", Actor);
		info.AddValue("DestroyedObstacles", DestroyedObstacles);
		info.AddValue("Infusions", Infusions);
		info.AddValue("Consumed", Consumed);
		info.AddValue("Abilities", Abilities);
		info.AddValue("Modifiers", Modifiers);
		info.AddValue("Heals", Heals);
		info.AddValue("Items", Items);
		info.AddValue("Loot", Loot);
		info.AddValue("Monsters", Monsters);
		info.AddValue("Door", Door);
		info.AddValue("Trap", Trap);
		info.AddValue("Hand", Hand);
		info.AddValue("XP", XP);
		info.AddValue("BattleGoalPerks", BattleGoalPerks);
		info.AddValue("Donations", Donations);
		info.AddValue("PersonalQuests", PersonalQuests);
		info.AddValue("Enhancements", Enhancements);
		info.AddValue("LoseCards", LoseCards);
		info.AddValue("DiscardCard", DiscardCard);
		info.AddValue("EndTurn", EndTurn);
		info.AddValue("LostAdjacency", LostAdjacency);
	}

	public CPlayerStats(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "CharacterID":
					CharacterID = info.GetString("CharacterID");
					break;
				case "Kills":
					Kills = (List<CPlayerStatsKill>)info.GetValue("Kills", typeof(List<CPlayerStatsKill>));
					break;
				case "Deaths":
					Deaths = (List<CPlayerStatsKill>)info.GetValue("Deaths", typeof(List<CPlayerStatsKill>));
					break;
				case "DamageDealt":
					DamageDealt = (List<CPlayerStatsDamage>)info.GetValue("DamageDealt", typeof(List<CPlayerStatsDamage>));
					break;
				case "DamageReceived":
					DamageReceived = (List<CPlayerStatsDamage>)info.GetValue("DamageReceived", typeof(List<CPlayerStatsDamage>));
					break;
				case "Actor":
					Actor = (List<CPlayerStatsDamage>)info.GetValue("Actor", typeof(List<CPlayerStatsDamage>));
					break;
				case "DestroyedObstacles":
					DestroyedObstacles = (List<CPlayerStatsDestroyObstacle>)info.GetValue("DestroyedObstacles", typeof(List<CPlayerStatsDestroyObstacle>));
					break;
				case "Infusions":
					Infusions = (List<CPlayerStatsElement>)info.GetValue("Infusions", typeof(List<CPlayerStatsElement>));
					break;
				case "Consumed":
					Consumed = (List<CPlayerStatsElement>)info.GetValue("Consumed", typeof(List<CPlayerStatsElement>));
					break;
				case "Abilities":
					Abilities = (List<CPlayerStatsAbilities>)info.GetValue("Abilities", typeof(List<CPlayerStatsAbilities>));
					break;
				case "Modifiers":
					Modifiers = (List<CPlayerStatsModifiers>)info.GetValue("Modifiers", typeof(List<CPlayerStatsModifiers>));
					break;
				case "Heals":
					Heals = (List<CPlayerStatsHeal>)info.GetValue("Heals", typeof(List<CPlayerStatsHeal>));
					break;
				case "Items":
					Items = (List<CPlayerStatsItem>)info.GetValue("Items", typeof(List<CPlayerStatsItem>));
					break;
				case "Loot":
					Loot = (List<CPlayerStatsLoot>)info.GetValue("Loot", typeof(List<CPlayerStatsLoot>));
					break;
				case "Monsters":
					Monsters = (List<CPlayerStatsMonsters>)info.GetValue("Monsters", typeof(List<CPlayerStatsMonsters>));
					break;
				case "Door":
					Door = (List<CPlayerStatsDoor>)info.GetValue("Door", typeof(List<CPlayerStatsDoor>));
					break;
				case "Trap":
					Trap = (List<CPlayerStatsTrap>)info.GetValue("Trap", typeof(List<CPlayerStatsTrap>));
					break;
				case "Hand":
					Hand = (List<CPlayerStatsHand>)info.GetValue("Hand", typeof(List<CPlayerStatsHand>));
					break;
				case "XP":
					XP = (List<CPlayerStatsXP>)info.GetValue("XP", typeof(List<CPlayerStatsXP>));
					break;
				case "BattleGoalPerks":
					BattleGoalPerks = (List<CPlayerStatsBattlePerks>)info.GetValue("BattleGoalPerks", typeof(List<CPlayerStatsBattlePerks>));
					break;
				case "Donations":
					Donations = (List<CPlayerStatsDonations>)info.GetValue("Donations", typeof(List<CPlayerStatsDonations>));
					break;
				case "PersonalQuests":
					PersonalQuests = (List<CPlayerStatsPersonalQuests>)info.GetValue("PersonalQuests", typeof(List<CPlayerStatsPersonalQuests>));
					break;
				case "Enhancements":
					Enhancements = (List<CPlayerStatsEnhancements>)info.GetValue("Enhancements", typeof(List<CPlayerStatsEnhancements>));
					break;
				case "LoseCards":
					LoseCards = (List<CPlayerStatsLoseCard>)info.GetValue("LoseCards", typeof(List<CPlayerStatsLoseCard>));
					break;
				case "DiscardCard":
					DiscardCard = (List<CPlayerStatsDiscardCard>)info.GetValue("DiscardCard", typeof(List<CPlayerStatsDiscardCard>));
					break;
				case "EndTurn":
					EndTurn = (List<CPlayerStatsEndTurn>)info.GetValue("EndTurn", typeof(List<CPlayerStatsEndTurn>));
					break;
				case "LostAdjacency":
					LostAdjacency = (List<CPlayerStatsLostAdjacency>)info.GetValue("LostAdjacency", typeof(List<CPlayerStatsLostAdjacency>));
					break;
				case "Character":
					CharacterID = ((ECharacter)info.GetValue("Character", typeof(ECharacter))/*cast due to .constrained prefix*/).ToString() + "ID";
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStats entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (Kills == null)
		{
			Kills = new List<CPlayerStatsKill>();
		}
		if (Deaths == null)
		{
			Deaths = new List<CPlayerStatsKill>();
		}
		if (DamageDealt == null)
		{
			DamageDealt = new List<CPlayerStatsDamage>();
		}
		if (DamageReceived == null)
		{
			DamageReceived = new List<CPlayerStatsDamage>();
		}
		if (Actor == null)
		{
			Actor = new List<CPlayerStatsDamage>();
		}
		if (DestroyedObstacles == null)
		{
			DestroyedObstacles = new List<CPlayerStatsDestroyObstacle>();
		}
		if (Infusions == null)
		{
			Infusions = new List<CPlayerStatsElement>();
		}
		if (Consumed == null)
		{
			Consumed = new List<CPlayerStatsElement>();
		}
		if (Abilities == null)
		{
			Abilities = new List<CPlayerStatsAbilities>();
		}
		if (Modifiers == null)
		{
			Modifiers = new List<CPlayerStatsModifiers>();
		}
		if (Heals == null)
		{
			Heals = new List<CPlayerStatsHeal>();
		}
		if (Items == null)
		{
			Items = new List<CPlayerStatsItem>();
		}
		if (Loot == null)
		{
			Loot = new List<CPlayerStatsLoot>();
		}
		if (Monsters == null)
		{
			Monsters = new List<CPlayerStatsMonsters>();
		}
		if (Door == null)
		{
			Door = new List<CPlayerStatsDoor>();
		}
		if (Trap == null)
		{
			Trap = new List<CPlayerStatsTrap>();
		}
		if (Hand == null)
		{
			Hand = new List<CPlayerStatsHand>();
		}
		if (XP == null)
		{
			XP = new List<CPlayerStatsXP>();
		}
		if (BattleGoalPerks == null)
		{
			BattleGoalPerks = new List<CPlayerStatsBattlePerks>();
		}
		if (Donations == null)
		{
			Donations = new List<CPlayerStatsDonations>();
		}
		if (PersonalQuests == null)
		{
			PersonalQuests = new List<CPlayerStatsPersonalQuests>();
		}
		if (Enhancements == null)
		{
			Enhancements = new List<CPlayerStatsEnhancements>();
		}
		if (LoseCards == null)
		{
			LoseCards = new List<CPlayerStatsLoseCard>();
		}
		if (DiscardCard == null)
		{
			DiscardCard = new List<CPlayerStatsDiscardCard>();
		}
		if (EndTurn == null)
		{
			EndTurn = new List<CPlayerStatsEndTurn>();
		}
		if (LostAdjacency == null)
		{
			LostAdjacency = new List<CPlayerStatsLostAdjacency>();
		}
	}

	public CPlayerStats(string characterID)
	{
		CharacterID = characterID;
		Kills = new List<CPlayerStatsKill>();
		Deaths = new List<CPlayerStatsKill>();
		DamageDealt = new List<CPlayerStatsDamage>();
		DamageReceived = new List<CPlayerStatsDamage>();
		Actor = new List<CPlayerStatsDamage>();
		DestroyedObstacles = new List<CPlayerStatsDestroyObstacle>();
		Infusions = new List<CPlayerStatsElement>();
		Consumed = new List<CPlayerStatsElement>();
		Abilities = new List<CPlayerStatsAbilities>();
		Modifiers = new List<CPlayerStatsModifiers>();
		Heals = new List<CPlayerStatsHeal>();
		Items = new List<CPlayerStatsItem>();
		Loot = new List<CPlayerStatsLoot>();
		Monsters = new List<CPlayerStatsMonsters>();
		Door = new List<CPlayerStatsDoor>();
		Trap = new List<CPlayerStatsTrap>();
		Hand = new List<CPlayerStatsHand>();
		XP = new List<CPlayerStatsXP>();
		BattleGoalPerks = new List<CPlayerStatsBattlePerks>();
		Donations = new List<CPlayerStatsDonations>();
		PersonalQuests = new List<CPlayerStatsPersonalQuests>();
		Enhancements = new List<CPlayerStatsEnhancements>();
		LoseCards = new List<CPlayerStatsLoseCard>();
		DiscardCard = new List<CPlayerStatsDiscardCard>();
		EndTurn = new List<CPlayerStatsEndTurn>();
		LostAdjacency = new List<CPlayerStatsLostAdjacency>();
	}

	public void UpdatePlayerStats(CPlayerStats scenarioStats)
	{
		Kills.AddRange(scenarioStats.Kills);
		Deaths.AddRange(scenarioStats.Deaths);
		DamageDealt.AddRange(scenarioStats.DamageDealt);
		DamageReceived.AddRange(scenarioStats.DamageReceived);
		Actor.AddRange(scenarioStats.Actor);
		DestroyedObstacles.AddRange(scenarioStats.DestroyedObstacles);
		Infusions.AddRange(scenarioStats.Infusions);
		Consumed.AddRange(scenarioStats.Consumed);
		Abilities.AddRange(scenarioStats.Abilities);
		Modifiers.AddRange(scenarioStats.Modifiers);
		Heals.AddRange(scenarioStats.Heals);
		Items.AddRange(scenarioStats.Items);
		Loot.AddRange(scenarioStats.Loot);
		Monsters.AddRange(scenarioStats.Monsters);
		Door.AddRange(scenarioStats.Door);
		Trap.AddRange(scenarioStats.Trap);
		Hand.AddRange(scenarioStats.Hand);
		XP.AddRange(scenarioStats.XP);
		BattleGoalPerks.AddRange(scenarioStats.BattleGoalPerks);
		Donations.AddRange(scenarioStats.Donations);
		PersonalQuests.AddRange(scenarioStats.PersonalQuests);
		Enhancements.AddRange(scenarioStats.Enhancements);
		LoseCards.AddRange(scenarioStats.LoseCards);
		DiscardCard.AddRange(scenarioStats.DiscardCard);
		EndTurn.AddRange(scenarioStats.EndTurn);
		LostAdjacency.AddRange(scenarioStats.LostAdjacency);
	}

	public void Clear()
	{
		Kills.Clear();
		Deaths.Clear();
		DamageDealt.Clear();
		DamageReceived.Clear();
		Actor.Clear();
		DestroyedObstacles.Clear();
		Infusions.Clear();
		Consumed.Clear();
		Abilities.Clear();
		Modifiers.Clear();
		Heals.Clear();
		Items.Clear();
		Loot.Clear();
		Monsters.Clear();
		Door.Clear();
		Trap.Clear();
		Hand.Clear();
		XP.Clear();
		BattleGoalPerks.Clear();
		Donations.Clear();
		PersonalQuests.Clear();
		Enhancements.Clear();
		LoseCards.Clear();
		DiscardCard.Clear();
		EndTurn.Clear();
		LostAdjacency.Clear();
	}

	public bool IsClear()
	{
		return Kills.Count() + Deaths.Count() + DamageDealt.Count() + DamageReceived.Count() + Actor.Count() + DestroyedObstacles.Count() + Infusions.Count() + Consumed.Count() + Abilities.Count() + Modifiers.Count() + Heals.Count() + Items.Count() + Loot.Count() + Monsters.Count() + Door.Count() + Trap.Count() + Hand.Count() + XP.Count() + BattleGoalPerks.Count() + Donations.Count() + PersonalQuests.Count() + Enhancements.Count() + LoseCards.Count() + DiscardCard.Count() + EndTurn.Count() + LostAdjacency.Count() == 0;
	}
}
