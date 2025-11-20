using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorLooted : SEventActor
{
	public ScenarioManager.ObjectImportType LootedType { get; private set; }

	public string ActorDroppingLoot { get; private set; }

	public SEventActorLooted()
	{
	}

	public SEventActorLooted(SEventActorLooted state, ReferenceDictionary references)
		: base(state, references)
	{
		LootedType = state.LootedType;
		ActorDroppingLoot = state.ActorDroppingLoot;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("LootedType", LootedType);
		info.AddValue("ActorDroppingLoot", ActorDroppingLoot);
	}

	public SEventActorLooted(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "LootedType"))
				{
					if (name == "ActorDroppingLoot")
					{
						ActorDroppingLoot = info.GetString("ActorDroppingLoot");
					}
				}
				else
				{
					LootedType = (ScenarioManager.ObjectImportType)info.GetValue("LootedType", typeof(ScenarioManager.ObjectImportType));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventActorLooted entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorLooted(ScenarioManager.ObjectImportType lootedType, CActor.EType actorType, string actorGuid, string actorClass, int health, int gold, int xp, int level, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, bool playedThisRound, bool isDead, CActor.ECauseOfDeath causeOfDeath, bool IsSummon, string actorActivated, string actedOnByGUID = "", string actedOnByClass = "", CActor.EType? actedOnType = null, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, bool actedOnSummon = false, List<PositiveConditionPair> actedOnPositiveConditions = null, List<NegativeConditionPair> actedOnNegativeConditions = null, string text = "", int maxHealth = 0, int allyAdjacent = 0, int enemyAdjacent = 0, int obstacleAdjacent = 0, bool wallAdjacent = false)
		: base(ESESubTypeActor.ActorLooted, actorType, actorGuid, actorClass, health, gold, xp, level, positiveConditions, negativeConditions, playedThisRound, isDead, causeOfDeath, IsSummon, actedOnByGUID, actedOnByClass, actedOnType, cardID, cardType, abilityType, actingAbilityName, abilityStrength, actedOnSummon, actedOnPositiveConditions, actedOnNegativeConditions, text, doNotSerialize: false, maxHealth, 0, 0, null, doom: false, "", attackerDisadvantage: false, targetAdjacent: false, allyAdjacent, enemyAdjacent, obstacleAdjacent, wallAdjacent)
	{
		LootedType = lootedType;
		ActorDroppingLoot = actorActivated;
	}
}
