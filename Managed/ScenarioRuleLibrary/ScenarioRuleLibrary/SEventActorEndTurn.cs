using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorEndTurn : SEventActor
{
	public int TileX { get; private set; }

	public int TileY { get; private set; }

	public SEventActorEndTurn()
	{
	}

	public SEventActorEndTurn(SEventActorEndTurn state, ReferenceDictionary references)
		: base(state, references)
	{
		TileX = state.TileX;
		TileY = state.TileY;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TileX", TileX);
		info.AddValue("TileY", TileY);
	}

	public SEventActorEndTurn(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "TileX"))
				{
					if (name == "TileY")
					{
						TileY = info.GetInt32("TileY");
					}
				}
				else
				{
					TileX = info.GetInt32("TileX");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventActorEndTurn entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorEndTurn(int x, int y, CActor.EType actorType, string actorGuid, string actorClass, int health, int gold, int xp, int level, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, bool playedThisRound, bool isDead, CActor.ECauseOfDeath causeOfDeath, bool IsSummon, string actedOnByGUID = "", string actedOnByClass = "", CActor.EType? actedOnType = null, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, bool actedOnSummon = false, List<PositiveConditionPair> actedOnPositiveConditions = null, List<NegativeConditionPair> actedOnNegativeConditions = null, string text = "", int maxHealth = 0, int allyAdjacent = 0, int enemyAdjacent = 0, int obstacleAdjacent = 0, bool wallAdjacent = false)
		: base(ESESubTypeActor.ActorHealed, actorType, actorGuid, actorClass, health, gold, xp, level, positiveConditions, negativeConditions, playedThisRound, isDead, causeOfDeath, IsSummon, actedOnByGUID, actedOnByClass, actedOnType, cardID, cardType, abilityType, actingAbilityName, abilityStrength, actedOnSummon, actedOnPositiveConditions, actedOnNegativeConditions, text, doNotSerialize: false, maxHealth, 0, 0, null, doom: false, "", attackerDisadvantage: false, targetAdjacent: false, allyAdjacent, enemyAdjacent, obstacleAdjacent, wallAdjacent)
	{
		TileX = x;
		TileY = y;
	}
}
