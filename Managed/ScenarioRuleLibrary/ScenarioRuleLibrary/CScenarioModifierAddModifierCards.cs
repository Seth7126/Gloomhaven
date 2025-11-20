using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierAddModifierCards : CScenarioModifier
{
	public List<string> ModifierCardNames { get; private set; }

	public CScenarioModifierAddModifierCards()
	{
	}

	public CScenarioModifierAddModifierCards(CScenarioModifierAddModifierCards state, ReferenceDictionary references)
		: base(state, references)
	{
		ModifierCardNames = references.Get(state.ModifierCardNames);
		if (ModifierCardNames == null && state.ModifierCardNames != null)
		{
			ModifierCardNames = new List<string>();
			for (int i = 0; i < state.ModifierCardNames.Count; i++)
			{
				string item = state.ModifierCardNames[i];
				ModifierCardNames.Add(item);
			}
			references.Add(state.ModifierCardNames, ModifierCardNames);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ModifierCardNames", ModifierCardNames);
	}

	public CScenarioModifierAddModifierCards(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "ModifierCardNames")
				{
					ModifierCardNames = (List<string>)info.GetValue("ModifierCardNames", typeof(List<string>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierAddModifierCards entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierAddModifierCards(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<string> modifierCardNames, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.AddModifierCards, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		ModifierCardNames = modifierCardNames;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate))
		{
			return;
		}
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			PerformScenarioModifier(roundCount, allActor, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
	}

	public override void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate) || (!base.IsPositive && CScenarioModifier.IgnoreNegativeScenarioEffects(currentActor)) || (base.ApplyOnceTotal && base.HasBeenAppliedOnce) || base.AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = false;
		if (base.ScenarioModifierFilter != null && currentActor != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null && base.ScenarioModifierFilter.IsValidTarget(actorState))
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (base.ApplyToEachActorOnce)
			{
				base.AppliedToActorGUIDs.Add(currentActor.ActorGuid);
			}
			base.HasBeenAppliedOnce = true;
			if (!(currentActor is CEnemyActor enemyActor))
			{
				CCharacterClass cCharacterClass = null;
				cCharacterClass = ((!(currentActor is CHeroSummonActor cHeroSummonActor)) ? (currentActor.Class as CCharacterClass) : (cHeroSummonActor.Summoner.Class as CCharacterClass));
				cCharacterClass.AddAdditionalModifierCards(ModifierCardNames);
			}
			else
			{
				MonsterClassManager.AddAdditionalModifierCards(enemyActor, ModifierCardNames);
			}
		}
	}
}
