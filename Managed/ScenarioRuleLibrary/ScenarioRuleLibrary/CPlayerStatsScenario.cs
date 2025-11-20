using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsScenario : CPlayerStats, ISerializable
{
	public bool PlayerSurvivedScenario { get; set; }

	public int RoundsPlayed { get; set; }

	public CPlayerStatsScenario()
	{
	}

	public CPlayerStatsScenario(CPlayerStatsScenario state, ReferenceDictionary references)
		: base(state, references)
	{
		PlayerSurvivedScenario = state.PlayerSurvivedScenario;
		RoundsPlayed = state.RoundsPlayed;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PlayerSurvivedScenario", PlayerSurvivedScenario);
		info.AddValue("RoundsPlayed", RoundsPlayed);
	}

	public CPlayerStatsScenario(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "PlayerSurvivedScenario"))
				{
					if (name == "RoundsPlayed")
					{
						RoundsPlayed = info.GetInt32("RoundsPlayed");
					}
				}
				else
				{
					PlayerSurvivedScenario = info.GetBoolean("PlayerSurvivedScenario");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsScenario entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsScenario(string characterID)
		: base(characterID)
	{
		PlayerSurvivedScenario = false;
	}

	public CPlayerStatsScenario GetPlayerStatsForRound(int requiredRound)
	{
		CPlayerStatsScenario cPlayerStatsScenario = new CPlayerStatsScenario(base.CharacterID);
		cPlayerStatsScenario.PlayerSurvivedScenario = PlayerSurvivedScenario;
		cPlayerStatsScenario.RoundsPlayed = RoundsPlayed;
		cPlayerStatsScenario.Kills.AddRange(base.Kills.Where((CPlayerStatsKill x) => x.Round == requiredRound));
		cPlayerStatsScenario.Deaths.AddRange(base.Deaths.Where((CPlayerStatsKill x) => x.Round == requiredRound));
		cPlayerStatsScenario.DamageDealt.AddRange(base.DamageDealt.Where((CPlayerStatsDamage x) => x.Round == requiredRound));
		cPlayerStatsScenario.Actor.AddRange(base.Actor.Where((CPlayerStatsDamage x) => x.Round == requiredRound));
		cPlayerStatsScenario.DamageReceived.AddRange(base.DamageReceived.Where((CPlayerStatsDamage x) => x.Round == requiredRound));
		cPlayerStatsScenario.DestroyedObstacles.AddRange(base.DestroyedObstacles.Where((CPlayerStatsDestroyObstacle x) => x.Round == requiredRound));
		cPlayerStatsScenario.Infusions.AddRange(base.Infusions.Where((CPlayerStatsElement x) => x.Round == requiredRound));
		cPlayerStatsScenario.Consumed.AddRange(base.Consumed.Where((CPlayerStatsElement x) => x.Round == requiredRound));
		cPlayerStatsScenario.Abilities.AddRange(base.Abilities.Where((CPlayerStatsAbilities x) => x.Round == requiredRound));
		cPlayerStatsScenario.Modifiers.AddRange(base.Modifiers.Where((CPlayerStatsModifiers x) => x.Round == requiredRound));
		cPlayerStatsScenario.Heals.AddRange(base.Heals.Where((CPlayerStatsHeal x) => x.Round == requiredRound));
		cPlayerStatsScenario.Items.AddRange(base.Items.Where((CPlayerStatsItem x) => x.Round == requiredRound));
		cPlayerStatsScenario.Loot.AddRange(base.Loot.Where((CPlayerStatsLoot x) => x.Round == requiredRound));
		cPlayerStatsScenario.Monsters.AddRange(base.Monsters.Where((CPlayerStatsMonsters x) => x.Round == requiredRound));
		cPlayerStatsScenario.Door.AddRange(base.Door.Where((CPlayerStatsDoor x) => x.Round == requiredRound));
		cPlayerStatsScenario.Trap.AddRange(base.Trap.Where((CPlayerStatsTrap x) => x.Round == requiredRound));
		cPlayerStatsScenario.Hand.AddRange(base.Hand.Where((CPlayerStatsHand x) => x.Round == requiredRound));
		cPlayerStatsScenario.XP.AddRange(base.XP.Where((CPlayerStatsXP x) => x.Round == requiredRound));
		cPlayerStatsScenario.BattleGoalPerks.AddRange(base.BattleGoalPerks.Where((CPlayerStatsBattlePerks x) => x.Round == requiredRound));
		cPlayerStatsScenario.Donations.AddRange(base.Donations.Where((CPlayerStatsDonations x) => x.Round == requiredRound));
		cPlayerStatsScenario.PersonalQuests.AddRange(base.PersonalQuests.Where((CPlayerStatsPersonalQuests x) => x.Round == requiredRound));
		cPlayerStatsScenario.Enhancements.AddRange(base.Enhancements.Where((CPlayerStatsEnhancements x) => x.Round == requiredRound));
		cPlayerStatsScenario.LoseCards.AddRange(base.LoseCards.Where((CPlayerStatsLoseCard x) => x.Round == requiredRound));
		cPlayerStatsScenario.DiscardCard.AddRange(base.DiscardCard.Where((CPlayerStatsDiscardCard x) => x.Round == requiredRound));
		cPlayerStatsScenario.EndTurn.AddRange(base.EndTurn.Where((CPlayerStatsEndTurn x) => x.Round == requiredRound));
		cPlayerStatsScenario.LostAdjacency.AddRange(base.LostAdjacency.Where((CPlayerStatsLostAdjacency x) => x.Round == requiredRound));
		return cPlayerStatsScenario;
	}
}
