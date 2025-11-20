using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectTrap : CObjectProp, ISerializable
{
	public bool Damage { get; private set; }

	public bool HasCustomDamageSet { get; private set; }

	public int DamageValue { get; private set; }

	public List<CCondition.ENegativeCondition> Conditions { get; private set; }

	public int AdjacentRange { get; set; }

	public int AdjacentDamageValue { get; set; }

	public List<CCondition.ENegativeCondition> AdjacentConditions { get; private set; }

	public CAbilityFilterContainer AdjacentFilter { get; private set; }

	public int TriggeredXP { get; private set; }

	public bool TargetAdjacentActors => AdjacentRange > 0;

	public CObjectTrap()
	{
	}

	public CObjectTrap(CObjectTrap state, ReferenceDictionary references)
		: base(state, references)
	{
		Damage = state.Damage;
		HasCustomDamageSet = state.HasCustomDamageSet;
		DamageValue = state.DamageValue;
		Conditions = references.Get(state.Conditions);
		if (Conditions == null && state.Conditions != null)
		{
			Conditions = new List<CCondition.ENegativeCondition>();
			for (int i = 0; i < state.Conditions.Count; i++)
			{
				CCondition.ENegativeCondition item = state.Conditions[i];
				Conditions.Add(item);
			}
			references.Add(state.Conditions, Conditions);
		}
		AdjacentRange = state.AdjacentRange;
		AdjacentDamageValue = state.AdjacentDamageValue;
		AdjacentConditions = references.Get(state.AdjacentConditions);
		if (AdjacentConditions == null && state.AdjacentConditions != null)
		{
			AdjacentConditions = new List<CCondition.ENegativeCondition>();
			for (int j = 0; j < state.AdjacentConditions.Count; j++)
			{
				CCondition.ENegativeCondition item2 = state.AdjacentConditions[j];
				AdjacentConditions.Add(item2);
			}
			references.Add(state.AdjacentConditions, AdjacentConditions);
		}
		AdjacentFilter = references.Get(state.AdjacentFilter);
		if (AdjacentFilter == null && state.AdjacentFilter != null)
		{
			AdjacentFilter = new CAbilityFilterContainer(state.AdjacentFilter, references);
			references.Add(state.AdjacentFilter, AdjacentFilter);
		}
		TriggeredXP = state.TriggeredXP;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Conditions", Conditions);
		info.AddValue("Damage", Damage);
		info.AddValue("HasCustomDamageSet", HasCustomDamageSet);
		info.AddValue("DamageValue", DamageValue);
		info.AddValue("AdjacentRange", AdjacentRange);
		info.AddValue("AdjacentDamageValue", AdjacentDamageValue);
		info.AddValue("AdjacentConditions", AdjacentConditions);
		info.AddValue("AdjacentFilter", AdjacentFilter);
		info.AddValue("TriggeredXP", TriggeredXP);
	}

	public CObjectTrap(SerializationInfo info, StreamingContext context)
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
				case "Conditions":
					Conditions = (List<CCondition.ENegativeCondition>)info.GetValue("Conditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "Damage":
					Damage = info.GetBoolean("Damage");
					break;
				case "HasCustomDamageSet":
					HasCustomDamageSet = info.GetBoolean("HasCustomDamageSet");
					break;
				case "DamageValue":
					DamageValue = info.GetInt32("DamageValue");
					break;
				case "AdjacentRange":
					AdjacentRange = info.GetInt32("AdjacentRange");
					break;
				case "AdjacentDamageValue":
					AdjacentDamageValue = info.GetInt32("AdjacentDamageValue");
					break;
				case "AdjacentConditions":
					AdjacentConditions = (List<CCondition.ENegativeCondition>)info.GetValue("AdjacentConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "AdjacentFilter":
					AdjacentFilter = (CAbilityFilterContainer)info.GetValue("AdjacentFilter", typeof(CAbilityFilterContainer));
					break;
				case "TriggeredXP":
					TriggeredXP = info.GetInt32("TriggeredXP");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectTrap entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectTrap(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid, List<CCondition.ENegativeCondition> conditions, bool damage, int damageValue, int adjacentRange, int adjacentDamageValue, List<CCondition.ENegativeCondition> adjacentConditions, CAbilityFilterContainer adjacentFilter, int triggeredXp)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		Damage = damage;
		HasCustomDamageSet = false;
		DamageValue = damageValue;
		Conditions = conditions;
		AdjacentRange = adjacentRange;
		AdjacentDamageValue = adjacentDamageValue;
		AdjacentConditions = adjacentConditions;
		AdjacentFilter = adjacentFilter;
		TriggeredXP = triggeredXp;
		if (AdjacentFilter == null)
		{
			AdjacentFilter = CAbilityFilterContainer.CreateDefaultFilter();
		}
	}

	public CObjectTrap(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid, AbilityData.TrapData trapData)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		Damage = trapData.Damage > 0 || (trapData.AdjacentRange > 0 && trapData.AdjacentDamage > 0);
		HasCustomDamageSet = trapData.Damage > 0 || (trapData.AdjacentRange > 0 && trapData.AdjacentDamage > 0);
		DamageValue = trapData.Damage;
		Conditions = trapData.Conditions;
		AdjacentRange = trapData.AdjacentRange;
		AdjacentDamageValue = trapData.AdjacentDamage;
		AdjacentConditions = trapData.AdjacentConditions;
		AdjacentFilter = trapData.AdjacentFilter;
		TriggeredXP = trapData.TriggeredXP;
	}

	public CObjectTrap(string name, ScenarioManager.ObjectImportType type, string mapGuid, List<CCondition.ENegativeCondition> conditions, bool damage, int damageValue)
		: base(name, type, null, null, null, null, mapGuid)
	{
		Damage = damage;
		DamageValue = damageValue;
		Conditions = conditions;
		TriggeredXP = 0;
	}

	public CObjectTrap(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid, List<CCondition.ENegativeCondition> conditions, bool damage, int damageValue)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		Damage = damage;
		DamageValue = damageValue;
		Conditions = conditions;
		TriggeredXP = 0;
	}

	public int GetTrapDamage()
	{
		int result = ((base.Owner != null || HasCustomDamageSet) ? DamageValue : ScenarioManager.Scenario.SLTE.TrapDamage);
		SimpleLog.AddToSimpleLog("[CObjectTrap] Trap damage is: " + result + ((base.Owner != null || HasCustomDamageSet) ? " and is using a custom set value" : " and is using Scenario Level trap damage value"));
		SimpleLog.AddToSimpleLog("[Scenario Level]: (Trap Activated) Current Scenario level is " + ScenarioManager.CurrentScenarioState.Level);
		return result;
	}

	public override bool Activate(CActor actor, CActor creditActor = null)
	{
		if (!base.Activated)
		{
			base.ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			int num = 0;
			if (Damage)
			{
				num = GetTrapDamage();
			}
			string text = "";
			string overrideInternalActor = "";
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.CurrentPhaseAbility?.m_BaseCard is CAbilityCard cAbilityCard && !string.IsNullOrEmpty(cAbilityCard?.ClassID))
			{
				text = cAbilityCard.ClassID;
				overrideInternalActor = cAbilityCard.ClassID;
			}
			if (actor != null)
			{
				if (string.IsNullOrEmpty(text))
				{
					if (actor is CPlayerActor cPlayerActor)
					{
						text = cPlayerActor.CharacterClass.ID;
					}
					if (actor is CHeroSummonActor cHeroSummonActor)
					{
						text = cHeroSummonActor.Summoner?.CharacterClass.ID;
					}
					if ((actor.OriginalType == CActor.EType.Enemy || actor.OriginalType == CActor.EType.Enemy2) && GameState.TurnActor.Type == CActor.EType.Player)
					{
						text = GameState.TurnActor.Class.ID;
						overrideInternalActor = GameState.TurnActor.Class.ID;
					}
				}
				SEventLogMessageHandler.AddEventLogMessage(new SEventObjectPropTrap(num, text, "Activate", ESESubTypeObjectProp.Activated, base.ObjectType, base.PrefabName, m_OwnerGuid, "", overrideInternalActor));
			}
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var _, ref strength);
			if (fly)
			{
				return false;
			}
			if ((actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon || actor.Type == CActor.EType.Ally) && ScenarioRuleClient.s_WorkThread != Thread.CurrentThread)
			{
				ScenarioRuleClient.AddSRLQueueMessage(new CSRLTrapMessage(this, actor), processImmediately: false);
				return false;
			}
			if (ScenarioManager.Scenario.HasActor(actor))
			{
				base.Activate(actor);
				if (base.Owner != null && TriggeredXP > 0 && !CActor.AreActorsAllied(actor.Type, base.Owner.Type))
				{
					base.Owner.GainXP(TriggeredXP);
				}
				if (GameState.ActorHealthCheck(actor, actor) && Conditions.Count > 0)
				{
					CActor cActor = ((base.Owner == null) ? actor : base.Owner);
					foreach (CCondition.ENegativeCondition condition in Conditions)
					{
						CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == condition.ToString());
						if (condition != CCondition.ENegativeCondition.NA && eAbilityType != CAbility.EAbilityType.None)
						{
							CAbility cAbility = CAbility.CreateAbility(eAbilityType, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: false);
							cAbility.Start(cActor, cActor);
							((CAbilityTargeting)cAbility).ApplyToActor(actor);
							if (eAbilityType == CAbility.EAbilityType.Stun || eAbilityType == CAbility.EAbilityType.Immobilize || eAbilityType == CAbility.EAbilityType.Sleep)
							{
								CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
								ScenarioRuleClient.MessageHandler(message);
							}
						}
						else
						{
							DLLDebug.LogError("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
						}
					}
				}
				int health = actor.Health;
				bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				if (Damage)
				{
					CActor trapCredit = GetTrapCredit(actor);
					GameState.ActorBeenDamaged(actor, num, checkIfPlayerCanAvoidDamage: true, trapCredit, null, CAbility.EAbilityType.Trap, 0, isTrapDamage: true);
				}
				if ((!(actor.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None || !Damage) && GameState.ActorHealthCheck(actor, actor, isTrap: true, isTerrain: false, actorWasAsleep))
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
			if (TargetAdjacentActors)
			{
				CActor cActor2 = ((base.Owner == null) ? actor : base.Owner);
				foreach (CActor item in GameState.GetActorsInRange(new Point(base.ArrayIndex.X, base.ArrayIndex.Y), cActor2, AdjacentRange, new List<CActor> { actor }, AdjacentFilter, null, null, isTargetedAbility: false, null, true))
				{
					if (GameState.ActorHealthCheck(item, item) && AdjacentConditions.Count > 0)
					{
						foreach (CCondition.ENegativeCondition condition2 in AdjacentConditions)
						{
							CAbility.EAbilityType eAbilityType2 = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == condition2.ToString());
							if (condition2 != CCondition.ENegativeCondition.NA && eAbilityType2 != CAbility.EAbilityType.None)
							{
								CAbility cAbility2 = CAbility.CreateAbility(eAbilityType2, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: false);
								cAbility2.Start(cActor2, cActor2);
								((CAbilityTargeting)cAbility2).ApplyToActor(item);
							}
							else
							{
								DLLDebug.LogError("Condition " + condition2.ToString() + " could not be found in EAbilityType enum.");
							}
						}
					}
					int health2 = item.Health;
					bool actorWasAsleep2 = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					if (Damage)
					{
						int strength2 = ((AdjacentDamageValue > 0) ? AdjacentDamageValue : ScenarioManager.Scenario.SLTE.TrapDamage);
						CActor trapCredit2 = GetTrapCredit(actor);
						GameState.ActorBeenDamaged(item, strength2, checkIfPlayerCanAvoidDamage: true, trapCredit2, null, CAbility.EAbilityType.Trap, 0, isTrapDamage: true);
					}
					if ((!(item.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None || !Damage) && GameState.ActorHealthCheck(item, item, isTrap: true, isTerrain: false, actorWasAsleep2))
					{
						CActorBeenDamaged_MessageData message3 = new CActorBeenDamaged_MessageData(item)
						{
							m_ActorBeingDamaged = item,
							m_DamageAbility = null,
							m_ActorOriginalHealth = health2,
							m_ActorWasAsleep = actorWasAsleep2
						};
						ScenarioRuleClient.MessageHandler(message3);
					}
				}
			}
		}
		return false;
	}

	public CActor GetTrapCredit(CActor actor)
	{
		CActor cActor = base.Owner;
		if (cActor == null || ((actor.OriginalType == CActor.EType.Enemy || actor.OriginalType == CActor.EType.Enemy2) && GameState.InternalCurrentActor.Type == CActor.EType.Player))
		{
			cActor = GameState.InternalCurrentActor;
		}
		return cActor;
	}

	public override bool WillActivationDamageActor(CActor actor)
	{
		if (base.Activated)
		{
			return false;
		}
		int num = 0;
		if (Damage)
		{
			num = ((base.Owner != null || HasCustomDamageSet) ? DamageValue : ScenarioManager.Scenario.SLTE.TrapDamage);
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var _, ref strength);
			if (fly)
			{
				num = 0;
			}
		}
		return num > 0;
	}

	public override bool WillActivationKillActor(CActor actor)
	{
		if (base.Activated)
		{
			return false;
		}
		int incomingDamage = 0;
		if (Damage)
		{
			incomingDamage = ((base.Owner != null || HasCustomDamageSet) ? DamageValue : ScenarioManager.Scenario.SLTE.TrapDamage);
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var _, ref strength);
			if (fly)
			{
				incomingDamage = 0;
			}
		}
		return actor.WillActorDie(incomingDamage, 0);
	}

	public void SetCustomDamageValue(int damageValueToSet)
	{
		HasCustomDamageSet = true;
		DamageValue = damageValueToSet;
	}

	public void ClearCustomDamage()
	{
		HasCustomDamageSet = false;
	}

	public static List<Tuple<int, string>> Compare(CObjectTrap trap1, CObjectTrap trap2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(trap1, trap2, isMPCompare));
			if (trap1.Damage != trap2.Damage)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2101, "CObjectTrap Damage does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"Damage",
						trap1.Damage.ToString(),
						trap2.Damage.ToString()
					}
				});
			}
			if (trap1.HasCustomDamageSet != trap2.HasCustomDamageSet)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2102, "CObjectTrap HasCustomDamageSet does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"HasCustomDamageSet",
						trap1.HasCustomDamageSet.ToString(),
						trap2.HasCustomDamageSet.ToString()
					}
				});
			}
			if (trap1.DamageValue != trap2.DamageValue)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2103, "CObjectTrap DamageValue does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"DamageValue",
						trap1.DamageValue.ToString(),
						trap2.DamageValue.ToString()
					}
				});
			}
			if (trap1.TriggeredXP != trap2.TriggeredXP)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2104, "CObjectTrap TriggeredXP does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"TriggeredXP",
						trap1.TriggeredXP.ToString(),
						trap2.TriggeredXP.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(trap1.Conditions, trap2.Conditions))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2105, "CObjectTrap Conditions null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"Conditions",
						(trap1.Conditions == null) ? "is null" : "is not null",
						(trap2.Conditions == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (trap1.Conditions.Count != trap2.Conditions.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2106, "CObjectTrap total Conditions Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
						new string[3]
						{
							"ObjectType",
							trap1.ObjectType.ToString(),
							trap2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
						new string[3]
						{
							"Conditions Count",
							trap1.Conditions.Count.ToString(),
							trap2.Conditions.Count.ToString()
						}
					});
					break;
				}
				foreach (CCondition.ENegativeCondition condition in trap1.Conditions)
				{
					if (!trap2.Conditions.Contains(condition))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2107, "CObjectTrap Conditions does not match.", new List<string[]>
						{
							new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
							new string[3]
							{
								"ObjectType",
								trap1.ObjectType.ToString(),
								trap2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
							new string[3]
							{
								"NegativeCondition",
								condition.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (CCondition.ENegativeCondition condition2 in trap2.Conditions)
				{
					if (!trap1.Conditions.Contains(condition2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2107, "CObjectTrap Conditions does not match.", new List<string[]>
						{
							new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
							new string[3]
							{
								"ObjectType",
								trap1.ObjectType.ToString(),
								trap2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
							new string[3]
							{
								"NegativeCondition",
								"Missing",
								condition2.ToString()
							}
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2199, "Exception during CObjectTrap compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
