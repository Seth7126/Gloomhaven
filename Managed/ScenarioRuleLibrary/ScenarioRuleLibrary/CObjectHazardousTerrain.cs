using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using SharedLibrary;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectHazardousTerrain : CObjectProp, ISerializable
{
	public CObjectHazardousTerrain()
	{
	}

	public CObjectHazardousTerrain(CObjectHazardousTerrain state, ReferenceDictionary references)
		: base(state, references)
	{
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjectHazardousTerrain(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjectHazardousTerrain(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
	}

	public CObjectHazardousTerrain(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
	}

	public CObjectHazardousTerrain(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
	}

	public override bool Activate(CActor actor, CActor creditActor = null)
	{
		if (!base.Activated)
		{
			base.ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			int num = 0;
			num = ((ScenarioManager.Scenario.SLTE == null) ? 1 : ScenarioManager.Scenario.SLTE.HazardousTerrainDamage);
			string characterID = "";
			string overrideInternalActor = "";
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.CurrentPhaseAbility?.m_BaseCard is CAbilityCard cAbilityCard && !string.IsNullOrEmpty(cAbilityCard?.ClassID))
			{
				characterID = cAbilityCard.ClassID;
				overrideInternalActor = cAbilityCard.ClassID;
			}
			if (actor != null)
			{
				if (actor is CPlayerActor cPlayerActor)
				{
					characterID = cPlayerActor.CharacterClass.ID;
				}
				if (actor is CHeroSummonActor cHeroSummonActor)
				{
					characterID = cHeroSummonActor.Summoner?.CharacterClass.ID;
				}
				if ((actor.OriginalType == CActor.EType.Enemy || actor.OriginalType == CActor.EType.Enemy2) && GameState.TurnActor.Type == CActor.EType.Player)
				{
					characterID = GameState.TurnActor.Class.ID;
					overrideInternalActor = GameState.TurnActor.Class.ID;
				}
				SEventLogMessageHandler.AddEventLogMessage(new SEventObjectPropTrap(num, characterID, "Activate", ESESubTypeObjectProp.Activated, base.ObjectType, base.PrefabName, m_OwnerGuid, "", overrideInternalActor));
			}
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
			if (fly || ignoreHazardousTerrain)
			{
				return false;
			}
			if ((actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon || actor.Type == CActor.EType.Ally) && ScenarioRuleClient.s_WorkThread != Thread.CurrentThread)
			{
				ScenarioRuleClient.AddSRLQueueMessage(new CSRLHazardousTerrainMessage(this, actor), processImmediately: false);
				return false;
			}
			if (ScenarioManager.Scenario.HasActor(actor))
			{
				CActivateProp_MessageData message = new CActivateProp_MessageData(actor)
				{
					m_Prop = this,
					m_InitialLoad = false
				};
				ScenarioRuleClient.MessageHandler(message);
				int health = actor.Health;
				bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(actor, num, checkIfPlayerCanAvoidDamage: true, null, null, CAbility.EAbilityType.None, 0, isTrapDamage: false, isTerrainDamage: true);
				if ((!(actor.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actor, actor, isTrap: false, isTerrain: true, actorWasAsleep))
				{
					CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(actor)
					{
						m_ActorBeingDamaged = actor,
						m_DamageAbility = null,
						m_ActorOriginalHealth = health,
						m_ActorWasAsleep = actorWasAsleep
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
			}
			base.Activated = false;
		}
		return false;
	}

	public override bool WillActivationDamageActor(CActor actor)
	{
		int num = ((ScenarioManager.Scenario.SLTE == null) ? 1 : ScenarioManager.Scenario.SLTE.HazardousTerrainDamage);
		int strength = 1;
		CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
		if (fly || ignoreHazardousTerrain)
		{
			num = 0;
		}
		return num > 0;
	}

	public override bool WillActivationKillActor(CActor actor)
	{
		int incomingDamage = ((ScenarioManager.Scenario.SLTE == null) ? 1 : ScenarioManager.Scenario.SLTE.HazardousTerrainDamage);
		int strength = 1;
		CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
		if (fly || ignoreHazardousTerrain)
		{
			incomingDamage = 0;
		}
		return actor.WillActorDie(incomingDamage, 0);
	}

	public static List<Tuple<int, string>> Compare(CObjectHazardousTerrain hazard1, CObjectHazardousTerrain hazard2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(hazard1, hazard2, isMPCompare));
		return list;
	}
}
