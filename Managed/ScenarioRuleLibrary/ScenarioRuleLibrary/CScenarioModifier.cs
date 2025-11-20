using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifier : CBaseCard, ISerializable
{
	public static EScenarioModifierType[] ScenarioModifierTypes = (EScenarioModifierType[])Enum.GetValues(typeof(EScenarioModifierType));

	public static EScenarioModifierTriggerPhase[] ScenarioModifierTriggerPhases = (EScenarioModifierTriggerPhase[])Enum.GetValues(typeof(EScenarioModifierTriggerPhase));

	public static EScenarioModifierActivationType[] ScenarioModifierActivationTypes = (EScenarioModifierActivationType[])Enum.GetValues(typeof(EScenarioModifierActivationType));

	public static EScenarioModifierRoomOpenBehaviour[] ScenarioModifierRoomOpenTypes = (EScenarioModifierRoomOpenBehaviour[])Enum.GetValues(typeof(EScenarioModifierRoomOpenBehaviour));

	public int ActivationRound { get; protected set; }

	public EScenarioModifierType ScenarioModifierType { get; private set; }

	public EScenarioModifierTriggerPhase ScenarioModifierTriggerPhase { get; private set; }

	public EScenarioModifierActivationType ScenarioModifierActivationType { get; private set; }

	public string ScenarioModifierActivationID { get; private set; }

	public bool ApplyToEachActorOnce { get; private set; }

	public bool IsPositive { get; private set; }

	public bool ApplyOnceTotal { get; private set; }

	public bool HasBeenAppliedOnce { get; protected set; }

	public List<string> AppliedToActorGUIDs { get; private set; }

	public List<string> AppliedToPropGUIDs { get; private set; }

	public List<string> TriggerAndActivationMapGUIDs { get; private set; }

	public CObjectiveFilter ScenarioModifierFilter { get; private set; }

	public string ScenarioAbilityID { get; private set; }

	public string CustomLocKey { get; private set; }

	public string CustomTriggerLocKey { get; private set; }

	public string EventIdentifier { get; private set; }

	public bool Deactivated { get; private set; }

	public bool IsHidden { get; private set; }

	public EScenarioModifierRoomOpenBehaviour RoomOpenBehaviour { get; private set; }

	public List<CAbility.EAbilityType> AfterAbilityTypes { get; private set; }

	public List<CAbility.EAttackType> AfterAttackTypes { get; private set; }

	public List<ActiveBonusState> ActiveBonusStates { get; private set; }

	public bool CancelAllActiveBonusesOnDeactivation { get; private set; }

	public string LocKey
	{
		get
		{
			if (!string.IsNullOrEmpty(CustomLocKey))
			{
				return CustomLocKey;
			}
			switch (ScenarioModifierType)
			{
			case EScenarioModifierType.SetElements:
			{
				CScenarioModifierSetElements cScenarioModifierSetElements = this as CScenarioModifierSetElements;
				if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.WaningElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					return "GUI_SCENARIO_MOD_SET_ELEMENTS_ALL";
				}
				if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.WaningElements.Count > 0)
				{
					return "GUI_SCENARIO_MOD_SET_ELEMENTS_STRONG_WANING";
				}
				if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					return "GUI_SCENARIO_MOD_SET_ELEMENTS_STRONG_INERT";
				}
				if (cScenarioModifierSetElements.WaningElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					return "GUI_SCENARIO_MOD_SET_ELEMENTS_WANING_INERT";
				}
				if (cScenarioModifierSetElements.StrongElements.Count > 0)
				{
					return "GUI_SCENARIO_MOD_SET_ELEMENTS_STRONG";
				}
				if (cScenarioModifierSetElements.WaningElements.Count > 0)
				{
					return "GUI_SCENARIO_MOD_SET_ELEMENTS_WANING";
				}
				if (cScenarioModifierSetElements.InertElements.Count > 0)
				{
					return "GUI_SCENARIO_MOD_SET_ELEMENTS_INERT";
				}
				return string.Empty;
			}
			case EScenarioModifierType.TriggerAbility:
				return "GUI_SCENARIO_MOD_TRIGGER_ABILITY";
			case EScenarioModifierType.AddConditionsToAbilities:
				if (ScenarioModifierFilter != null && ScenarioModifierFilter.FilterAbilityType != CAbility.EAbilityType.None)
				{
					if (ScenarioModifierFilter.FilterActorType != CAbilityFilter.EFilterActorType.None)
					{
						return "GUI_SCENARIO_MOD_ADD_CONDITIONS_TO_ABILITIES_FILTERED_ABILITY_TYPE_ACTOR";
					}
					return "GUI_SCENARIO_MOD_ADD_CONDITIONS_TO_ABILITIES_FILTERED_ABILITY_TYPE";
				}
				return "GUI_SCENARIO_MOD_ADD_CONDITIONS_TO_ABILITIES";
			default:
				return string.Empty;
			}
		}
	}

	public string TriggerLocKey
	{
		get
		{
			if (!string.IsNullOrEmpty(CustomTriggerLocKey))
			{
				return CustomTriggerLocKey;
			}
			return ScenarioModifierTriggerPhase switch
			{
				EScenarioModifierTriggerPhase.StartTurn => "GUI_SCENARIO_MOD_TRIGGER_START_TURN", 
				EScenarioModifierTriggerPhase.EndTurn => "GUI_SCENARIO_MOD_TRIGGER_END_TURN", 
				EScenarioModifierTriggerPhase.StartRound => "GUI_SCENARIO_MOD_TRIGGER_START_ROUND", 
				EScenarioModifierTriggerPhase.EndRound => "GUI_SCENARIO_MOD_TRIGGER_END_ROUND", 
				_ => string.Empty, 
			};
		}
	}

	public List<CMap> Maps
	{
		get
		{
			if (TriggerAndActivationMapGUIDs != null)
			{
				return ScenarioManager.Scenario.Maps.FindAll((CMap x) => TriggerAndActivationMapGUIDs.Contains(x.MapGuid));
			}
			return null;
		}
	}

	public CScenarioModifier()
	{
	}

	public CScenarioModifier(CScenarioModifier state, ReferenceDictionary references)
		: base(state, references)
	{
		ActivationRound = state.ActivationRound;
		ScenarioModifierType = state.ScenarioModifierType;
		ScenarioModifierTriggerPhase = state.ScenarioModifierTriggerPhase;
		ScenarioModifierActivationType = state.ScenarioModifierActivationType;
		ScenarioModifierActivationID = state.ScenarioModifierActivationID;
		ApplyToEachActorOnce = state.ApplyToEachActorOnce;
		IsPositive = state.IsPositive;
		ApplyOnceTotal = state.ApplyOnceTotal;
		HasBeenAppliedOnce = state.HasBeenAppliedOnce;
		AppliedToActorGUIDs = references.Get(state.AppliedToActorGUIDs);
		if (AppliedToActorGUIDs == null && state.AppliedToActorGUIDs != null)
		{
			AppliedToActorGUIDs = new List<string>();
			for (int i = 0; i < state.AppliedToActorGUIDs.Count; i++)
			{
				string item = state.AppliedToActorGUIDs[i];
				AppliedToActorGUIDs.Add(item);
			}
			references.Add(state.AppliedToActorGUIDs, AppliedToActorGUIDs);
		}
		AppliedToPropGUIDs = references.Get(state.AppliedToPropGUIDs);
		if (AppliedToPropGUIDs == null && state.AppliedToPropGUIDs != null)
		{
			AppliedToPropGUIDs = new List<string>();
			for (int j = 0; j < state.AppliedToPropGUIDs.Count; j++)
			{
				string item2 = state.AppliedToPropGUIDs[j];
				AppliedToPropGUIDs.Add(item2);
			}
			references.Add(state.AppliedToPropGUIDs, AppliedToPropGUIDs);
		}
		TriggerAndActivationMapGUIDs = references.Get(state.TriggerAndActivationMapGUIDs);
		if (TriggerAndActivationMapGUIDs == null && state.TriggerAndActivationMapGUIDs != null)
		{
			TriggerAndActivationMapGUIDs = new List<string>();
			for (int k = 0; k < state.TriggerAndActivationMapGUIDs.Count; k++)
			{
				string item3 = state.TriggerAndActivationMapGUIDs[k];
				TriggerAndActivationMapGUIDs.Add(item3);
			}
			references.Add(state.TriggerAndActivationMapGUIDs, TriggerAndActivationMapGUIDs);
		}
		ScenarioModifierFilter = references.Get(state.ScenarioModifierFilter);
		if (ScenarioModifierFilter == null && state.ScenarioModifierFilter != null)
		{
			ScenarioModifierFilter = new CObjectiveFilter(state.ScenarioModifierFilter, references);
			references.Add(state.ScenarioModifierFilter, ScenarioModifierFilter);
		}
		ScenarioAbilityID = state.ScenarioAbilityID;
		CustomLocKey = state.CustomLocKey;
		CustomTriggerLocKey = state.CustomTriggerLocKey;
		EventIdentifier = state.EventIdentifier;
		Deactivated = state.Deactivated;
		IsHidden = state.IsHidden;
		RoomOpenBehaviour = state.RoomOpenBehaviour;
		AfterAbilityTypes = references.Get(state.AfterAbilityTypes);
		if (AfterAbilityTypes == null && state.AfterAbilityTypes != null)
		{
			AfterAbilityTypes = new List<CAbility.EAbilityType>();
			for (int l = 0; l < state.AfterAbilityTypes.Count; l++)
			{
				CAbility.EAbilityType item4 = state.AfterAbilityTypes[l];
				AfterAbilityTypes.Add(item4);
			}
			references.Add(state.AfterAbilityTypes, AfterAbilityTypes);
		}
		AfterAttackTypes = references.Get(state.AfterAttackTypes);
		if (AfterAttackTypes == null && state.AfterAttackTypes != null)
		{
			AfterAttackTypes = new List<CAbility.EAttackType>();
			for (int m = 0; m < state.AfterAttackTypes.Count; m++)
			{
				CAbility.EAttackType item5 = state.AfterAttackTypes[m];
				AfterAttackTypes.Add(item5);
			}
			references.Add(state.AfterAttackTypes, AfterAttackTypes);
		}
		ActiveBonusStates = references.Get(state.ActiveBonusStates);
		if (ActiveBonusStates == null && state.ActiveBonusStates != null)
		{
			ActiveBonusStates = new List<ActiveBonusState>();
			for (int n = 0; n < state.ActiveBonusStates.Count; n++)
			{
				ActiveBonusState activeBonusState = state.ActiveBonusStates[n];
				ActiveBonusState activeBonusState2 = references.Get(activeBonusState);
				if (activeBonusState2 == null && activeBonusState != null)
				{
					activeBonusState2 = new ActiveBonusState(activeBonusState, references);
					references.Add(activeBonusState, activeBonusState2);
				}
				ActiveBonusStates.Add(activeBonusState2);
			}
			references.Add(state.ActiveBonusStates, ActiveBonusStates);
		}
		CancelAllActiveBonusesOnDeactivation = state.CancelAllActiveBonusesOnDeactivation;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActivationRound", ActivationRound);
		info.AddValue("ScenarioModifierType", ScenarioModifierType);
		info.AddValue("ScenarioModifierTriggerPhase", ScenarioModifierTriggerPhase);
		info.AddValue("ScenarioModifierActivationType", ScenarioModifierActivationType);
		info.AddValue("ScenarioModifierActivationID", ScenarioModifierActivationID);
		info.AddValue("ScenarioModifierFilter", ScenarioModifierFilter);
		info.AddValue("ScenarioAbilityID", ScenarioAbilityID);
		info.AddValue("ApplyToEachActorOnce", ApplyToEachActorOnce);
		info.AddValue("ApplyOnceTotal", ApplyOnceTotal);
		info.AddValue("HasBeenAppliedOnce", HasBeenAppliedOnce);
		info.AddValue("AppliedToActorGUIDs", AppliedToActorGUIDs);
		info.AddValue("AppliedToPropGUIDs", AppliedToPropGUIDs);
		info.AddValue("TriggerAndActivationMapGUIDs", TriggerAndActivationMapGUIDs);
		info.AddValue("ActiveBonusStates", ActiveBonusStates);
		info.AddValue("IsPositive", IsPositive);
		info.AddValue("CustomLocKey", CustomLocKey);
		info.AddValue("CustomTriggerLocKey", CustomTriggerLocKey);
		info.AddValue("EventIdentifier", EventIdentifier);
		info.AddValue("Deactivated", Deactivated);
		info.AddValue("IsHidden", IsHidden);
		info.AddValue("RoomOpenBehaviour", RoomOpenBehaviour);
		info.AddValue("AfterAbilityTypes", AfterAbilityTypes);
		info.AddValue("AfterAttackTypes", AfterAttackTypes);
		info.AddValue("CancelAllActiveBonusesOnDeactivation", CancelAllActiveBonusesOnDeactivation);
	}

	protected CScenarioModifier(SerializationInfo info, StreamingContext context)
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
				case "ActivationRound":
					ActivationRound = info.GetInt32("ActivationRound");
					break;
				case "ScenarioModifierType":
					ScenarioModifierType = (EScenarioModifierType)info.GetValue("ScenarioModifierType", typeof(EScenarioModifierType));
					break;
				case "ScenarioModifierTriggerPhase":
					ScenarioModifierTriggerPhase = (EScenarioModifierTriggerPhase)info.GetValue("ScenarioModifierTriggerPhase", typeof(EScenarioModifierTriggerPhase));
					break;
				case "ScenarioModifierActivationType":
					ScenarioModifierActivationType = (EScenarioModifierActivationType)info.GetValue("ScenarioModifierActivationType", typeof(EScenarioModifierActivationType));
					break;
				case "ScenarioModifierActivationID":
					ScenarioModifierActivationID = info.GetString("ScenarioModifierActivationID");
					break;
				case "ScenarioModifierFilter":
					ScenarioModifierFilter = (CObjectiveFilter)info.GetValue("ScenarioModifierFilter", typeof(CObjectiveFilter));
					break;
				case "ScenarioAbilityID":
					ScenarioAbilityID = info.GetString("ScenarioAbilityID");
					break;
				case "ApplyToEachActorOnce":
					ApplyToEachActorOnce = info.GetBoolean("ApplyToEachActorOnce");
					break;
				case "ApplyOnceTotal":
					ApplyOnceTotal = info.GetBoolean("ApplyOnceTotal");
					break;
				case "HasBeenAppliedOnce":
					HasBeenAppliedOnce = info.GetBoolean("HasBeenAppliedOnce");
					break;
				case "AppliedToActorGUIDs":
					AppliedToActorGUIDs = (List<string>)info.GetValue("AppliedToActorGUIDs", typeof(List<string>));
					break;
				case "AppliedToPropGUIDs":
					AppliedToPropGUIDs = (List<string>)info.GetValue("AppliedToPropGUIDs", typeof(List<string>));
					break;
				case "TriggerAndActivationMapGUIDs":
					TriggerAndActivationMapGUIDs = (List<string>)info.GetValue("TriggerAndActivationMapGUIDs", typeof(List<string>));
					break;
				case "ActiveBonusStates":
					ActiveBonusStates = (List<ActiveBonusState>)info.GetValue("ActiveBonusStates", typeof(List<ActiveBonusState>));
					break;
				case "IsPositive":
					IsPositive = info.GetBoolean("IsPositive");
					break;
				case "CustomLocKey":
					CustomLocKey = info.GetString("CustomLocKey");
					break;
				case "CustomTriggerLocKey":
					CustomTriggerLocKey = info.GetString("CustomTriggerLocKey");
					break;
				case "EventIdentifier":
					EventIdentifier = info.GetString("EventIdentifier");
					break;
				case "Deactivated":
					Deactivated = info.GetBoolean("Deactivated");
					break;
				case "IsHidden":
					IsHidden = info.GetBoolean("IsHidden");
					break;
				case "RoomOpenBehaviour":
					RoomOpenBehaviour = (EScenarioModifierRoomOpenBehaviour)info.GetValue("RoomOpenBehaviour", typeof(EScenarioModifierRoomOpenBehaviour));
					break;
				case "AfterAbilityTypes":
					AfterAbilityTypes = (List<CAbility.EAbilityType>)info.GetValue("AfterAbilityTypes", typeof(List<CAbility.EAbilityType>));
					break;
				case "AfterAttackTypes":
					AfterAttackTypes = (List<CAbility.EAttackType>)info.GetValue("AfterAttackTypes", typeof(List<CAbility.EAttackType>));
					break;
				case "CancelAllActiveBonusesOnDeactivation":
					CancelAllActiveBonusesOnDeactivation = info.GetBoolean("CancelAllActiveBonusesOnDeactivation");
					break;
				case "RoomMapGUIDs":
					TriggerAndActivationMapGUIDs = (List<string>)info.GetValue("RoomMapGUIDs", typeof(List<string>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifier entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (AppliedToActorGUIDs == null)
		{
			AppliedToActorGUIDs = new List<string>();
		}
		if (AppliedToPropGUIDs == null)
		{
			AppliedToPropGUIDs = new List<string>();
		}
		if (ActiveBonusStates == null)
		{
			ActiveBonusStates = new List<ActiveBonusState>();
		}
	}

	public CScenarioModifier(string name, int id, int activationRound, bool applyToEachActorOnce, EScenarioModifierType scenarioModifierType = EScenarioModifierType.None, EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = EScenarioModifierTriggerPhase.None, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, CObjectiveFilter scenarioModifierFilter = null, bool isPositive = false, string scenarioAbilityID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(id, ECardType.ScenarioModifier, name)
	{
		ActivationRound = activationRound;
		ApplyToEachActorOnce = applyToEachActorOnce;
		ApplyOnceTotal = applyOnceTotal;
		ScenarioModifierType = scenarioModifierType;
		ScenarioModifierTriggerPhase = scenarioModifierTriggerPhase;
		ScenarioModifierActivationType = scenarioModifierActivationType;
		ScenarioModifierActivationID = scenarioModifierActivationID;
		ScenarioModifierFilter = scenarioModifierFilter;
		ScenarioAbilityID = scenarioAbilityID;
		IsPositive = isPositive;
		ActiveBonusStates = new List<ActiveBonusState>();
		AppliedToActorGUIDs = new List<string>();
		AppliedToPropGUIDs = new List<string>();
		CustomLocKey = customLocKey;
		CustomTriggerLocKey = customTriggerLocKey;
		EventIdentifier = eventId;
		AfterAbilityTypes = afterAbilityTypes?.ToList();
		AfterAttackTypes = afterAttackTypes?.ToList();
		IsHidden = isHidden;
		Deactivated = isDeactivated;
		CancelAllActiveBonusesOnDeactivation = cancelAllActiveBonusesOnDeactivation;
		RoomOpenBehaviour = roomOpenBehaviour;
		TriggerAndActivationMapGUIDs = roomMapGuids;
		if (ScenarioModifierActivationType != EScenarioModifierActivationType.None && ScenarioModifierActivationID != null)
		{
			Deactivated = true;
		}
	}

	public void SetDeactivated(bool deactivate)
	{
		Deactivated = deactivate;
		if (Deactivated && CancelAllActiveBonusesOnDeactivation)
		{
			for (int num = base.ActiveBonuses.Count - 1; num >= 0; num--)
			{
				base.ActiveBonuses[num].Finish();
			}
		}
		if (ScenarioRuleClient.MessageHandler != null)
		{
			CScenarioModifierHiddenStateUpdated_MessageData message = new CScenarioModifierHiddenStateUpdated_MessageData
			{
				m_UpdatedModifier = this
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public bool IsActiveInRound(int roundCount, bool forceActivate = false)
	{
		if (!forceActivate)
		{
			if (!Deactivated)
			{
				return roundCount >= ActivationRound;
			}
			return false;
		}
		return true;
	}

	public void SetHiddenState(bool shouldSetHidden)
	{
		IsHidden = shouldSetHidden;
		CScenarioModifierHiddenStateUpdated_MessageData message = new CScenarioModifierHiddenStateUpdated_MessageData
		{
			m_UpdatedModifier = this
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public virtual void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
	}

	public virtual void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		HasBeenAppliedOnce = true;
	}

	public virtual void PerformScenarioModifier(int roundCount, CObjectProp currentProp = null, int partySize = 2, bool forceActivate = false)
	{
		HasBeenAppliedOnce = true;
	}

	public static bool IgnoreNegativeScenarioEffects(CActor currentActor)
	{
		bool result = false;
		if (currentActor is CPlayerActor cPlayerActor)
		{
			result = cPlayerActor.CharacterClass.Perks.Exists((PerksYMLData it) => it.IgnoreNegativeScenarioEffects);
		}
		return result;
	}

	public void SaveScenarioModifier()
	{
		ActiveBonusStates.Clear();
		foreach (CActiveBonus activeBonuse in base.ActiveBonuses)
		{
			ActiveBonusStates.Add(new ActiveBonusState(activeBonuse.ID, base.ID, base.Name, activeBonuse.Ability.Name, activeBonuse.Actor.ActorGuid, activeBonuse.Caster?.ActorGuid, isTopAction: false, activeBonuse.Remaining, activeBonuse.ActiveBonusStartRound, isDoom: false, activeBonuse.BespokeBehaviourStrength()));
		}
	}

	public void LoadScenarioModifier()
	{
		if (ActiveBonusStates.Count <= 0)
		{
			return;
		}
		foreach (ActiveBonusState activeBonusState in ActiveBonusStates)
		{
			CActor cActor = ScenarioManager.FindActor(activeBonusState.ActorGuid);
			CActor caster = ScenarioManager.FindActor(activeBonusState.CasterGuid);
			if (cActor == null)
			{
				DLLDebug.LogError("Unable to find actor attached to active bonus");
				continue;
			}
			CAbility ability = AllListedTriggerAbilities().SingleOrDefault((CAbility a) => a.Name == activeBonusState.AbilityName);
			int? iD = activeBonusState.ID;
			int? remaining = activeBonusState.Remaining;
			int? activeBonusStartRound = activeBonusState.ActiveBonusStartRound;
			AddActiveBonus(ability, cActor, caster, iD, remaining, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, null, activeBonusStartRound);
		}
	}

	public bool ShouldTriggerWhenOpeningRoom(string roomGuid)
	{
		if (RoomOpenBehaviour.HasFlag(EScenarioModifierRoomOpenBehaviour.TriggerOnOpen))
		{
			if (TriggerAndActivationMapGUIDs == null || TriggerAndActivationMapGUIDs.Count <= 0)
			{
				return true;
			}
			return TriggerAndActivationMapGUIDs.Contains(roomGuid);
		}
		return false;
	}

	public bool ShouldActivateWhenOpeningRoom(string roomGuid)
	{
		if (RoomOpenBehaviour.HasFlag(EScenarioModifierRoomOpenBehaviour.ActivateOnOpen))
		{
			if (TriggerAndActivationMapGUIDs == null || TriggerAndActivationMapGUIDs.Count <= 0)
			{
				return true;
			}
			return TriggerAndActivationMapGUIDs.Contains(roomGuid);
		}
		return false;
	}

	public virtual CAbility TriggerAbility(CActor actorPerformingAbility = null, int partySize = 2)
	{
		return ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == ScenarioAbilityID)?.ScenarioAbilities[0];
	}

	public virtual List<CAbility> TriggerAbilities(CActor actorPerformingAbility = null, int partySize = 2)
	{
		return ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == ScenarioAbilityID)?.ScenarioAbilities.ToList();
	}

	public virtual List<CAbility> AllListedTriggerAbilities()
	{
		return ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == ScenarioAbilityID)?.ScenarioAbilities.ToList() ?? new List<CAbility>();
	}

	public static List<Tuple<int, string>> Compare(CScenarioModifier mod1, CScenarioModifier mod2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CBaseCard.Compare(mod1, mod2, isMPCompare));
			if (mod1.ActivationRound != mod2.ActivationRound)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2213, "CScenarioModifier ActivationRound does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ActivationRound",
						mod1.ActivationRound.ToString(),
						mod2.ActivationRound.ToString()
					}
				});
			}
			if (mod1.ScenarioModifierType != mod2.ScenarioModifierType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2201, "CScenarioModifier ScenarioModifierType does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					}
				});
			}
			if (mod1.ScenarioModifierTriggerPhase != mod2.ScenarioModifierTriggerPhase)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2202, "CScenarioModifier ScenarioModifierTriggerPhase does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(mod1.ScenarioModifierFilter, mod2.ScenarioModifierFilter))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2203, "ScenarioModifierFilter null state does not match.", new List<string[]> { new string[3]
				{
					"ScenarioModifierFilter",
					(mod1.ScenarioModifierFilter == null) ? "is null" : "is not null",
					(mod2.ScenarioModifierFilter == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (!mod1.ScenarioModifierFilter.Compare(mod2.ScenarioModifierFilter))
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2203, "CScenarioModifier ScenarioModifierFilter does not match.", new List<string[]>
					{
						new string[3]
						{
							"ScenarioModifierType",
							mod1.ScenarioModifierType.ToString(),
							mod2.ScenarioModifierType.ToString()
						},
						new string[3]
						{
							"ScenarioModifierTriggerPhase",
							mod1.ScenarioModifierTriggerPhase.ToString(),
							mod2.ScenarioModifierTriggerPhase.ToString()
						},
						new string[3]
						{
							"ScenarioModifierFilter",
							mod1.ScenarioModifierFilter.ToString(),
							mod2.ScenarioModifierFilter.ToString()
						}
					});
				}
				break;
			}
			if (mod1.ScenarioAbilityID != mod2.ScenarioAbilityID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2204, "CScenarioModifier ScenarioAbilityID does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3] { "ScenarioAbilityID", mod1.ScenarioAbilityID, mod2.ScenarioAbilityID }
				});
			}
			if (mod1.ApplyToEachActorOnce != mod2.ApplyToEachActorOnce)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2205, "CScenarioModifier ApplyToEachActorOnce does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3]
					{
						"ApplyToEachActorOnce",
						mod1.ApplyToEachActorOnce.ToString(),
						mod2.ApplyToEachActorOnce.ToString()
					}
				});
			}
			if (mod1.IsPositive != mod2.IsPositive)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2206, "CScenarioModifier IsPositive does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3]
					{
						"IsPositive",
						mod1.IsPositive.ToString(),
						mod2.IsPositive.ToString()
					}
				});
			}
			if (mod1.CustomLocKey != mod2.CustomLocKey)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2213, "CScenarioModifier CustomLocKey does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3] { "CustomLocKey", mod1.CustomLocKey, mod2.CustomLocKey }
				});
			}
			if (mod1.CustomTriggerLocKey != mod2.CustomTriggerLocKey)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2214, "CScenarioModifier CustomTriggerLocKey does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3] { "CustomTriggerLocKey", mod1.CustomTriggerLocKey, mod2.CustomTriggerLocKey }
				});
			}
			if (mod1.EventIdentifier != mod2.EventIdentifier)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2215, "CScenarioModifier EventIdentifier does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3] { "EventIdentifier", mod1.EventIdentifier, mod2.EventIdentifier }
				});
			}
			if (mod1.Deactivated != mod2.Deactivated)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2216, "CScenarioModifier Deactivated does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3]
					{
						"Deactivated",
						mod1.Deactivated.ToString(),
						mod2.Deactivated.ToString()
					}
				});
			}
			if (mod1.IsHidden != mod2.IsHidden)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2217, "CScenarioModifier IsHidden does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3]
					{
						"IsHidden",
						mod1.IsHidden.ToString(),
						mod2.IsHidden.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(mod1.AppliedToActorGUIDs, mod2.AppliedToActorGUIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2207, "CScenarioModifier AppliedToActorGUIDs Null state does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3]
					{
						"AppliedToActorGUIDs",
						(mod1.AppliedToActorGUIDs == null) ? "is null" : "is not null",
						(mod2.AppliedToActorGUIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (mod1.AppliedToActorGUIDs.Count != mod2.AppliedToActorGUIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2208, "CScenarioModifier AppliedToActorGUIDs Count does not match.", new List<string[]>
					{
						new string[3]
						{
							"ScenarioModifierType",
							mod1.ScenarioModifierType.ToString(),
							mod2.ScenarioModifierType.ToString()
						},
						new string[3]
						{
							"ScenarioModifierTriggerPhase",
							mod1.ScenarioModifierTriggerPhase.ToString(),
							mod2.ScenarioModifierTriggerPhase.ToString()
						},
						new string[3]
						{
							"AppliedToActorGUIDs Count",
							mod1.AppliedToActorGUIDs.Count.ToString(),
							mod2.AppliedToActorGUIDs.Count.ToString()
						}
					});
					break;
				}
				foreach (string appliedToActorGUID in mod1.AppliedToActorGUIDs)
				{
					if (!mod2.AppliedToActorGUIDs.Contains(appliedToActorGUID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2209, "CScenarioModifier AppliedToActorGUIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3]
							{
								"ScenarioModifierType",
								mod1.ScenarioModifierType.ToString(),
								mod2.ScenarioModifierType.ToString()
							},
							new string[3]
							{
								"ScenarioModifierTriggerPhase",
								mod1.ScenarioModifierTriggerPhase.ToString(),
								mod2.ScenarioModifierTriggerPhase.ToString()
							},
							new string[3] { "AppliedToActorGUID", appliedToActorGUID, "Missing" }
						});
					}
				}
				foreach (string appliedToActorGUID2 in mod2.AppliedToActorGUIDs)
				{
					if (!mod1.AppliedToActorGUIDs.Contains(appliedToActorGUID2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2209, "CScenarioModifier AppliedToActorGUIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3]
							{
								"ScenarioModifierType",
								mod1.ScenarioModifierType.ToString(),
								mod2.ScenarioModifierType.ToString()
							},
							new string[3]
							{
								"ScenarioModifierTriggerPhase",
								mod1.ScenarioModifierTriggerPhase.ToString(),
								mod2.ScenarioModifierTriggerPhase.ToString()
							},
							new string[3] { "AppliedToActorGUID", "Missing", appliedToActorGUID2 }
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(mod1.ActiveBonusStates, mod2.ActiveBonusStates))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2210, "CScenarioModifier ActiveBonusStates Null state does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3]
					{
						"ActiveBonuses",
						(mod1.ActiveBonusStates == null) ? "is null" : "is not null",
						(mod2.ActiveBonusStates == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (mod1.ActiveBonusStates.Count != mod2.ActiveBonusStates.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2211, "CScenarioModifier ActiveBonusStates Count does not match.", new List<string[]>
					{
						new string[3]
						{
							"ScenarioModifierType",
							mod1.ScenarioModifierType.ToString(),
							mod2.ScenarioModifierType.ToString()
						},
						new string[3]
						{
							"ScenarioModifierTriggerPhase",
							mod1.ScenarioModifierTriggerPhase.ToString(),
							mod2.ScenarioModifierTriggerPhase.ToString()
						},
						new string[3]
						{
							"ActiveBonusStates Count",
							mod1.ActiveBonusStates.Count.ToString(),
							mod2.ActiveBonusStates.Count.ToString()
						}
					});
					break;
				}
				List<ActiveBonusState> list2 = mod1.ActiveBonusStates.ToList();
				List<ActiveBonusState> list3 = mod2.ActiveBonusStates.ToList();
				for (int num = list2.Count - 1; num >= 0; num--)
				{
					ActiveBonusState abState1 = list2[num];
					ActiveBonusState activeBonusState = list3.Find((ActiveBonusState s) => s.CardID == abState1.CardID && abState1.CardName.Contains(s.CardName) && s.AbilityName == abState1.AbilityName && abState1.ActorGuid == s.ActorGuid);
					if (activeBonusState == null)
					{
						activeBonusState = list3.Find((ActiveBonusState s) => s.CardID == abState1.CardID && abState1.CardName.Contains(s.CardName.Replace(" ", string.Empty).Replace("'", string.Empty)) && s.AbilityName == abState1.AbilityName && abState1.ActorGuid == s.ActorGuid);
					}
					if (activeBonusState == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2212, "CScenarioModifier ActiveBonusState in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3]
							{
								"ScenarioModifierType",
								mod1.ScenarioModifierType.ToString(),
								mod2.ScenarioModifierType.ToString()
							},
							new string[3]
							{
								"ScenarioModifierTriggerPhase",
								mod1.ScenarioModifierTriggerPhase.ToString(),
								mod2.ScenarioModifierTriggerPhase.ToString()
							},
							new string[3]
							{
								"ActiveBonus CardID",
								abState1.CardID.ToString(),
								"Missing"
							},
							new string[3] { "ActiveBonus CardName", abState1.CardName, "Missing" },
							new string[3] { "ActiveBonus AbilityName", abState1.AbilityName, "Missing" }
						});
					}
					else
					{
						list.AddRange(ActiveBonusState.Compare(abState1, activeBonusState, "ScenarioModifier", mod1.ScenarioModifierType.ToString(), isMPCompare));
						list3.Remove(activeBonusState);
					}
					list2.Remove(abState1);
				}
				if (list3.Count <= 0)
				{
					break;
				}
				foreach (ActiveBonusState item in list3)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2212, "CScenarioModifier ActiveBonusState in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
					{
						new string[3]
						{
							"ScenarioModifierType",
							mod1.ScenarioModifierType.ToString(),
							mod2.ScenarioModifierType.ToString()
						},
						new string[3]
						{
							"ScenarioModifierTriggerPhase",
							mod1.ScenarioModifierTriggerPhase.ToString(),
							mod2.ScenarioModifierTriggerPhase.ToString()
						},
						new string[3]
						{
							"ActiveBonus CardID",
							"Missing",
							item.CardID.ToString()
						},
						new string[3] { "ActiveBonus CardName", "Missing", item.CardName },
						new string[3] { "ActiveBonus AbilityName", "Missing", item.AbilityName }
					});
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(mod1.AppliedToPropGUIDs, mod2.AppliedToPropGUIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2213, "CScenarioModifier AppliedToPropGUIDs Null state does not match.", new List<string[]>
				{
					new string[3]
					{
						"ScenarioModifierType",
						mod1.ScenarioModifierType.ToString(),
						mod2.ScenarioModifierType.ToString()
					},
					new string[3]
					{
						"ScenarioModifierTriggerPhase",
						mod1.ScenarioModifierTriggerPhase.ToString(),
						mod2.ScenarioModifierTriggerPhase.ToString()
					},
					new string[3]
					{
						"AppliedToPropGUIDs",
						(mod1.AppliedToPropGUIDs == null) ? "is null" : "is not null",
						(mod2.AppliedToPropGUIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (mod1.AppliedToPropGUIDs.Count != mod2.AppliedToPropGUIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2214, "CScenarioModifier AppliedToPropGUIDs Count does not match.", new List<string[]>
					{
						new string[3]
						{
							"ScenarioModifierType",
							mod1.ScenarioModifierType.ToString(),
							mod2.ScenarioModifierType.ToString()
						},
						new string[3]
						{
							"ScenarioModifierTriggerPhase",
							mod1.ScenarioModifierTriggerPhase.ToString(),
							mod2.ScenarioModifierTriggerPhase.ToString()
						},
						new string[3]
						{
							"AppliedToPropGUIDs Count",
							mod1.AppliedToPropGUIDs.Count.ToString(),
							mod2.AppliedToPropGUIDs.Count.ToString()
						}
					});
					break;
				}
				foreach (string appliedToPropGUID in mod1.AppliedToPropGUIDs)
				{
					if (!mod2.AppliedToPropGUIDs.Contains(appliedToPropGUID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2215, "CScenarioModifier AppliedToPropGUIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3]
							{
								"ScenarioModifierType",
								mod1.ScenarioModifierType.ToString(),
								mod2.ScenarioModifierType.ToString()
							},
							new string[3]
							{
								"ScenarioModifierTriggerPhase",
								mod1.ScenarioModifierTriggerPhase.ToString(),
								mod2.ScenarioModifierTriggerPhase.ToString()
							},
							new string[3] { "AppliedToPropGUID", appliedToPropGUID, "Missing" }
						});
					}
				}
				foreach (string appliedToPropGUID2 in mod2.AppliedToPropGUIDs)
				{
					if (!mod1.AppliedToPropGUIDs.Contains(appliedToPropGUID2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2216, "CScenarioModifier AppliedToPropGUIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3]
							{
								"ScenarioModifierType",
								mod1.ScenarioModifierType.ToString(),
								mod2.ScenarioModifierType.ToString()
							},
							new string[3]
							{
								"ScenarioModifierTriggerPhase",
								mod1.ScenarioModifierTriggerPhase.ToString(),
								mod2.ScenarioModifierTriggerPhase.ToString()
							},
							new string[3] { "AppliedToPropGUID", "Missing", appliedToPropGUID2 }
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2299, "Exception during CScenarioModifier compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
