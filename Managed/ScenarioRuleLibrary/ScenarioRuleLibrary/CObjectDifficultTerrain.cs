using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using SharedLibrary;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectDifficultTerrain : CObjectProp, ISerializable
{
	public bool TreatAsTrap { get; set; }

	public CObjectiveFilter TreatAsTrapFilter { get; set; }

	public CObjectDifficultTerrain()
	{
	}

	public CObjectDifficultTerrain(CObjectDifficultTerrain state, ReferenceDictionary references)
		: base(state, references)
	{
		TreatAsTrap = state.TreatAsTrap;
		TreatAsTrapFilter = references.Get(state.TreatAsTrapFilter);
		if (TreatAsTrapFilter == null && state.TreatAsTrapFilter != null)
		{
			TreatAsTrapFilter = new CObjectiveFilter(state.TreatAsTrapFilter, references);
			references.Add(state.TreatAsTrapFilter, TreatAsTrapFilter);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TreatAsTrap", TreatAsTrap);
		info.AddValue("TreatAsTrapFilter", TreatAsTrapFilter);
	}

	public CObjectDifficultTerrain(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "TreatAsTrap"))
				{
					if (name == "TreatAsTrapFilter")
					{
						TreatAsTrapFilter = (CObjectiveFilter)info.GetValue("TreatAsTrapFilter", typeof(CObjectiveFilter));
					}
				}
				else
				{
					TreatAsTrap = info.GetBoolean("TreatAsTrap");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectDifficultTerrain entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectDifficultTerrain(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid, bool treatAsTrap = false, CObjectiveFilter treatAsTrapFilter = null)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		TreatAsTrap = treatAsTrap;
		TreatAsTrapFilter = treatAsTrapFilter;
		InitDifficultTerrain();
	}

	public CObjectDifficultTerrain(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
	}

	public CObjectDifficultTerrain(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
	}

	public override bool Activate(CActor actor, CActor creditActor = null)
	{
		if (!base.Activated)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
			if (TreatAsTrap && (TreatAsTrapFilter == null || TreatAsTrapFilter.IsValidTarget(actorState)))
			{
				int trapDamage = ScenarioManager.Scenario.SLTE.TrapDamage;
				if ((actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon || actor.Type == CActor.EType.Ally) && ScenarioRuleClient.s_WorkThread != Thread.CurrentThread)
				{
					ScenarioRuleClient.AddSRLQueueMessage(new CSRLHazardousTerrainMessage(this, actor), processImmediately: false);
					return false;
				}
				if (ScenarioManager.Scenario.HasActor(actor))
				{
					int health = actor.Health;
					bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					GameState.ActorBeenDamaged(actor, trapDamage, checkIfPlayerCanAvoidDamage: true, null, null, CAbility.EAbilityType.None, 0, isTrapDamage: false, isTerrainDamage: true);
					if ((!(actor.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actor, actor, isTrap: false, isTerrain: true, actorWasAsleep))
					{
						CActorBeenDamaged_MessageData message = new CActorBeenDamaged_MessageData(actor)
						{
							m_ActorBeingDamaged = actor,
							m_DamageAbility = null,
							m_ActorOriginalHealth = health,
							m_ActorWasAsleep = actorWasAsleep
						};
						ScenarioRuleClient.MessageHandler(message);
					}
				}
			}
			base.ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			CActivateProp_MessageData message2 = new CActivateProp_MessageData(actor)
			{
				m_Prop = this,
				m_InitialLoad = false
			};
			ScenarioRuleClient.MessageHandler(message2);
			base.Activated = false;
		}
		return false;
	}

	public override bool WillActivationDamageActor(CActor actor)
	{
		int num = ((ScenarioManager.Scenario.SLTE == null) ? 1 : ScenarioManager.Scenario.SLTE.TrapDamage);
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
		if (TreatAsTrap && (TreatAsTrapFilter == null || TreatAsTrapFilter.IsValidTarget(actorState)))
		{
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
			if (fly || ignoreHazardousTerrain)
			{
				num = 0;
			}
		}
		else
		{
			num = 0;
		}
		return num > 0;
	}

	public override bool WillActivationKillActor(CActor actor)
	{
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
		if (TreatAsTrap && (TreatAsTrapFilter == null || TreatAsTrapFilter.IsValidTarget(actorState)))
		{
			int incomingDamage = ((ScenarioManager.Scenario.SLTE == null) ? 1 : ScenarioManager.Scenario.SLTE.TrapDamage);
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
			if (fly || ignoreHazardousTerrain)
			{
				incomingDamage = 0;
			}
			return actor.WillActorDie(incomingDamage, 0);
		}
		return false;
	}

	public void InitDifficultTerrain()
	{
		ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].IsDifficultTerrain = true;
	}

	public void RemoveDifficultTerrain()
	{
		ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].IsDifficultTerrain = false;
		ScenarioManager.Tiles[base.ArrayIndex.X, base.ArrayIndex.Y].m_Props.RemoveAll((CObjectProp p) => p.PropGuid == base.PropGuid);
		ScenarioManager.CurrentScenarioState.Props.RemoveAll((CObjectProp p) => p.PropGuid == base.PropGuid);
		CDifficultTerrainRemoved_MessageData message = new CDifficultTerrainRemoved_MessageData
		{
			m_DifficultTerrainRemoved = this
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static List<Tuple<int, string>> Compare(CObjectDifficultTerrain difficult1, CObjectDifficultTerrain difficult2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(difficult1, difficult2, isMPCompare));
		return list;
	}
}
