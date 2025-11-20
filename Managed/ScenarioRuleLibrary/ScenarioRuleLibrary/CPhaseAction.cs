using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class CPhaseAction : CPhase
{
	public class CPhaseAbility
	{
		public CAbility m_Ability;

		public CActor TargetingActor;

		public Guid? ActionID;

		public int? ItemID;

		public CBaseCard m_BaseCard;

		public Action m_EndAction;

		public bool KillAfterAction;

		public bool IsCostAbility;

		public CPhaseAbility(CAbility ability, CActor targetingActor, CBaseCard card, Guid? actionID, int? item = null, Action endAction = null, bool killAfter = false, bool isCostAbility = false)
		{
			m_Ability = ability;
			TargetingActor = targetingActor;
			ActionID = actionID;
			ItemID = item;
			m_BaseCard = card;
			m_EndAction = endAction;
			KillAfterAction = killAfter;
			IsCostAbility = isCostAbility;
		}
	}

	private List<CPhaseAbility> m_PhaseAbilities = new List<CPhaseAbility>();

	private List<CPhaseAbility> m_CurrentPhaseAbilities;

	private List<CPhaseAbility> m_PreviousPhaseAbilities = new List<CPhaseAbility>();

	private CPhaseAbility m_FirstPhaseAbility;

	private int m_ActionXP;

	private int m_ActionConsumeXP;

	private bool m_EndAbilitySynced;

	private List<CPhaseAbility> m_SavedAbilities = new List<CPhaseAbility>();

	private int? m_CurrentItemID;

	private bool m_ApplyActionXP;

	private Guid? m_CurrentActionID;

	private List<ElementInfusionBoardManager.EElement> m_ElementsToInfuse = new List<ElementInfusionBoardManager.EElement>();

	private bool m_DontMoveAbilityState;

	private GameState.CurrentActorAction m_CurrentAction;

	private bool m_HasAnyAbilityHappened;

	private bool m_HasAnyNonItemAbilityHappened;

	private List<CItem> m_ItemsLoggedAsUsedThisPhase = new List<CItem>();

	private List<CActionAugmentation> m_ActionAugmentationsToConsume = new List<CActionAugmentation>();

	private List<CActionAugmentation> m_ActionAugmentationsConsumed = new List<CActionAugmentation>();

	private CActiveBonus m_CurrentTriggeredActiveBonus;

	private bool m_ShownEndOfActionActiveBonuses;

	private bool m_CheckedForStartActionSongs;

	private bool m_StartActionNonToggleActivesAdded;

	private bool m_EndActionNonToggleActivesAdded;

	private static int s_DamageInflictedThisAction;

	private static int s_HexesMovedThisAction;

	private static int s_ObstaclesDestroyedThisAction;

	private static int s_TargetsDamagedInPrevAttackThisAction;

	private static Guid s_TargetsDamagedInPrevAttackThisActionAbilityID;

	private static int s_TargetsActuallyDamagedInPrevAttackThisAction;

	private static Guid s_TargetsActuallyDamagedInPrevAttackThisActionAbilityID;

	private static int s_TargetsDamagedInPrevDamageAbilityThisAction;

	private static Guid s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID;

	private static List<CActor> s_ActorsKilledThisAction = new List<CActor>();

	public List<CPhaseAbility> RemainingPhaseAbilities => m_PhaseAbilities;

	public CActiveBonus CurrentTriggeredActiveBonus
	{
		get
		{
			return m_CurrentTriggeredActiveBonus;
		}
		set
		{
			m_CurrentTriggeredActiveBonus = value;
		}
	}

	public List<CActionAugmentation> ActionAugmentationsConsumed => m_ActionAugmentationsConsumed;

	public CPhaseAbility CurrentPhaseAbility
	{
		get
		{
			if (m_CurrentPhaseAbilities == null || m_CurrentPhaseAbilities.Count <= 0)
			{
				return null;
			}
			return m_CurrentPhaseAbilities[m_CurrentPhaseAbilities.Count - 1];
		}
	}

	public List<CPhaseAbility> CurrentPhaseAbilities
	{
		get
		{
			if (m_CurrentPhaseAbilities == null || m_CurrentPhaseAbilities.Count <= 0)
			{
				return null;
			}
			return m_CurrentPhaseAbilities;
		}
	}

	public List<CPhaseAbility> PreviousPhaseAbilities => m_PreviousPhaseAbilities;

	public List<CItem> ItemsUsedThisPhase => m_ItemsLoggedAsUsedThisPhase;

	public bool IsNextAbilitySharedAnim
	{
		get
		{
			if (m_CurrentPhaseAbilities.Count > 1)
			{
				return m_CurrentPhaseAbilities[1].m_Ability.SkipAnim;
			}
			if (m_PhaseAbilities.Count > 0)
			{
				return m_PhaseAbilities[0].m_Ability.SkipAnim;
			}
			return false;
		}
	}

	public bool HasConsumedElements => m_ActionAugmentationsConsumed.Any((CActionAugmentation x) => x.Elements.Count > 0);

	public static int DamageInflictedThisAction => s_DamageInflictedThisAction;

	public static int HexesMovedThisAction => s_HexesMovedThisAction;

	public static int ObstaclesDestroyedThisAction => s_ObstaclesDestroyedThisAction;

	public static int TargetsDamagedInPrevAttackThisAction => s_TargetsDamagedInPrevAttackThisAction;

	public static int TargetsDamagedInPrevDamageAbilityThisAction => s_TargetsDamagedInPrevDamageAbilityThisAction;

	public static List<CActor> ActorsKilledThisAction => s_ActorsKilledThisAction;

	public List<ElementInfusionBoardManager.EElement> ElementsToInfuse => m_ElementsToInfuse;

	public CPhaseAction()
	{
		m_PhaseType = PhaseType.Action;
		m_ActionXP = 0;
		m_ActionConsumeXP = 0;
		s_DamageInflictedThisAction = 0;
		s_HexesMovedThisAction = 0;
		s_ObstaclesDestroyedThisAction = 0;
		s_TargetsDamagedInPrevAttackThisAction = 0;
		s_TargetsActuallyDamagedInPrevAttackThisAction = 0;
		s_TargetsDamagedInPrevAttackThisActionAbilityID = default(Guid);
		s_TargetsDamagedInPrevDamageAbilityThisAction = 0;
		s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID = default(Guid);
		s_ActorsKilledThisAction = new List<CActor>();
		m_ActionAugmentationsToConsume = new List<CActionAugmentation>();
		m_ActionAugmentationsConsumed = new List<CActionAugmentation>();
		foreach (CAbility item in GameState.CurrentAction.Action.Abilities.Where((CAbility x) => !x.OnDeath))
		{
			m_PhaseAbilities.Add(new CPhaseAbility(item, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
			if (item.SubAbilities == null)
			{
				continue;
			}
			foreach (CAbility subAbility in item.SubAbilities)
			{
				if (!subAbility.IsInlineSubAbility)
				{
					subAbility.ParentAbility = item;
					m_PhaseAbilities.Add(new CPhaseAbility(subAbility, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
				}
			}
		}
		m_FirstPhaseAbility = ((m_PhaseAbilities.Count > 0) ? m_PhaseAbilities[0] : null);
		m_ActionXP += GameState.CurrentAction.Action.ActionXP;
		foreach (CPhaseAbility phaseAbility in m_PhaseAbilities)
		{
			if (phaseAbility.m_Ability is CAbilityMerged cAbilityMerged)
			{
				cAbilityMerged.CopyMergedAbilities();
			}
		}
		AddMonsterConsumes();
		if (GameState.CurrentAction.Action.Infusions != null)
		{
			foreach (ElementInfusionBoardManager.EElement infusion in GameState.CurrentAction.Action.Infusions)
			{
				m_ElementsToInfuse.Add(infusion);
			}
		}
		m_CurrentPhaseAbilities = new List<CPhaseAbility>();
		m_EndAbilitySynced = true;
		m_CurrentAction = GameState.CurrentAction;
		GameState.CurrentAction = null;
		foreach (CActiveBonus item2 in CActiveBonus.FindAllActiveBonuses())
		{
			item2.ResetRestriction(CActiveBonus.EActiveBonusRestrictionType.OncePerAction);
			item2.ApplyToAction = false;
		}
		if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
		{
			CShowAugmentationBar_MessageData message = new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
			{
				m_Action = m_CurrentAction.Action,
				m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionInitialized, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
	}

	public void ResetActionAugmentationsToConsume()
	{
		m_ActionAugmentationsToConsume.Clear();
	}

	public void AddMonsterConsumes()
	{
		if (!GameState.InternalCurrentActor.IsMonsterType)
		{
			return;
		}
		m_ActionAugmentationsToConsume.AddRange(GameState.CurrentActionSelectedAugmentations);
		if (GameState.CurrentActionSelectedAugmentations.Count <= 0)
		{
			return;
		}
		foreach (CActionAugmentation consume in GameState.CurrentActionSelectedAugmentations)
		{
			foreach (CActionAugmentationOp consumeOp in consume.AugmentationOps)
			{
				if (consumeOp.Ability != null && consumeOp.Ability.ActiveBonusYML != null)
				{
					consumeOp.Ability.ActiveBonusYML.UseAlt = true;
				}
				if (!(consumeOp.ParentAbilityName != string.Empty))
				{
					continue;
				}
				CPhaseAbility cPhaseAbility = null;
				cPhaseAbility = m_PhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
				{
					if (x.m_Ability.Name == consumeOp.ParentAbilityName)
					{
						Guid? actionID = x.ActionID;
						Guid actionID2 = consume.ActionID;
						if (!actionID.HasValue)
						{
							return false;
						}
						if (!actionID.HasValue)
						{
							return true;
						}
						return actionID.GetValueOrDefault() == actionID2;
					}
					return false;
				});
				if (cPhaseAbility == null)
				{
					foreach (CPhaseAbility phaseAbility in m_PhaseAbilities)
					{
						if (!(phaseAbility.m_Ability is CAbilityMerged cAbilityMerged))
						{
							continue;
						}
						foreach (CAbility mergedAbility in cAbilityMerged.MergedAbilities)
						{
							if (mergedAbility.Name == consumeOp.ParentAbilityName && phaseAbility.ActionID == consume.ActionID)
							{
								cPhaseAbility = phaseAbility;
								break;
							}
						}
					}
				}
				if (cPhaseAbility == null)
				{
					continue;
				}
				if (cPhaseAbility.m_Ability.ActiveBonusYML != null)
				{
					cPhaseAbility.m_Ability.ActiveBonusYML.UseAlt = true;
				}
				if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
				{
					cPhaseAbility.m_Ability.OverrideAbilityValues(consumeOp.AbilityOverride, perform: false);
					if (consumeOp.AbilityOverride.AbilityType.HasValue && consumeOp.AbilityOverride.AbilityType != consumeOp.AbilityOverride.OriginalAbility.AbilityType)
					{
						CAbility ability = CAbility.CopyAbility(cPhaseAbility.m_Ability, generateNewID: false, fullCopy: true);
						m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(cPhaseAbility), new CPhaseAbility(ability, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
						m_PhaseAbilities.Remove(cPhaseAbility);
						m_FirstPhaseAbility = ((m_PhaseAbilities.Count > 0) ? m_PhaseAbilities[0] : null);
					}
					if (consumeOp.AbilityOverride.SubAbilities == null || consumeOp.AbilityOverride.SubAbilities.Count <= 0)
					{
						continue;
					}
					foreach (CAbility subAbility in consumeOp.AbilityOverride.SubAbilities)
					{
						if (!subAbility.IsInlineSubAbility)
						{
							CAbility cAbility = CAbility.CopyAbility(subAbility, generateNewID: false);
							AbilityData.MiscAbilityData miscAbilityData = cAbility.MiscAbilityData;
							if ((miscAbilityData == null || miscAbilityData.FilterSpecified != true) && cPhaseAbility.m_Ability.AbilityFilter != null)
							{
								cAbility.AbilityFilter = cPhaseAbility.m_Ability.AbilityFilter.Copy();
								cAbility.MiscAbilityData.FilterSpecified = true;
							}
							cAbility.ParentAbility = cPhaseAbility.m_Ability;
							if (m_PhaseAbilities.IndexOf(cPhaseAbility) == m_PhaseAbilities.Count - 1)
							{
								m_PhaseAbilities.Add(new CPhaseAbility(cAbility, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
							}
							else
							{
								m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(cPhaseAbility) + 1, new CPhaseAbility(cAbility, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
							}
						}
					}
				}
				else if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
				{
					CAbility ability2 = CAbility.CopyAbility(consumeOp.Ability, generateNewID: false);
					CPhaseAbility cPhaseAbility2 = null;
					cPhaseAbility2 = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) => x.m_Ability.Name == consumeOp.Ability.Name);
					if (cPhaseAbility2 != null)
					{
						m_PhaseAbilities.Remove(cPhaseAbility2);
					}
					m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(cPhaseAbility), new CPhaseAbility(ability2, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
				}
			}
			foreach (ElementInfusionBoardManager.EElement infusion in consume.Infusions)
			{
				m_ElementsToInfuse.Add(infusion);
			}
		}
	}

	public void ToggleActionAugmentation(CActionAugmentation actionAugmentation, bool passing = false)
	{
		if (!m_ActionAugmentationsConsumed.Contains(actionAugmentation))
		{
			if (!m_ActionAugmentationsToConsume.Contains(actionAugmentation))
			{
				foreach (CActionAugmentationOp consumeOp in actionAugmentation.AugmentationOps)
				{
					if (consumeOp.Ability != null && consumeOp.Ability.ActiveBonusYML != null)
					{
						consumeOp.Ability.ActiveBonusYML.UseAlt = true;
					}
					if (consumeOp.ParentAbilityName == string.Empty)
					{
						if (!GameState.InternalCurrentActor.IsMonsterType)
						{
							if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
							{
								m_PhaseAbilities.Add(new CPhaseAbility(CAbility.CopyAbility(consumeOp.Ability, generateNewID: false), GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
							}
							else
							{
								DLLDebug.LogWarning("No parent specified for ability override");
							}
						}
						continue;
					}
					CPhaseAbility phaseAbility;
					bool isCurrentPhaseAbility;
					bool isActiveMergedAbility;
					CAbility cAbility = FindAbilityFromPhaseAbilityWithActionAugmentationParentName(actionAugmentation, consumeOp, out phaseAbility, out isCurrentPhaseAbility, out isActiveMergedAbility);
					if (cAbility == null)
					{
						continue;
					}
					if (cAbility.ActiveBonusYML != null)
					{
						cAbility.ActiveBonusYML.UseAlt = true;
					}
					if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
					{
						if (cAbility.AbilityType == CAbility.EAbilityType.Null)
						{
							CAbility cAbility2 = CAbility.CopyAbility(consumeOp.Ability, generateNewID: false);
							if (phaseAbility == CurrentPhaseAbility)
							{
								StackInlineSubAbilities(new List<CAbility> { cAbility2 }, null, performNow: true, stopPlayerSkipping: false, false);
								cAbility.AbilityHasHappened = cAbility2.AbilityHasHappened;
								ScenarioRuleClient.MessageHandler(new CStackedAugmentToNullAbility_MessageData(GameState.InternalCurrentActor));
							}
							else
							{
								m_PhaseAbilities.Add(new CPhaseAbility(cAbility2, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
							}
							continue;
						}
						CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == consumeOp.Ability.AbilityType.ToString());
						if (eNegativeCondition != CCondition.ENegativeCondition.NA)
						{
							cAbility.NegativeConditions.Add(eNegativeCondition, CAbility.CopyAbility(consumeOp.Ability, generateNewID: false));
							continue;
						}
						CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == consumeOp.Ability.AbilityType.ToString());
						if (ePositiveCondition != CCondition.EPositiveCondition.NA)
						{
							cAbility.PositiveConditions.Add(ePositiveCondition, CAbility.CopyAbility(consumeOp.Ability, generateNewID: false));
							continue;
						}
						CAbility ability = CAbility.CopyAbility(consumeOp.Ability, generateNewID: false);
						m_PhaseAbilities.Add(new CPhaseAbility(ability, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
					}
					else if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
					{
						bool flag = consumeOp.AbilityOverride.AbilityType.HasValue && consumeOp.AbilityOverride.AbilityType != cAbility.AbilityType;
						bool flag2 = CurrentPhaseAbility == phaseAbility && cAbility.TargetingActor != null && (!(cAbility is CAbilityMerged) || isActiveMergedAbility) && actionAugmentation.CostAbility == null && !(cAbility is CAbilityTargeting);
						if (flag)
						{
							foreach (CItem item in cAbility.ActiveSingleTargetItems.Concat(cAbility.ActiveOverrideItems))
							{
								cAbility.TargetingActor.Inventory.DeselectItem(item);
							}
							for (int num = cAbility.CurrentOverrides.Count - 1; num >= 0; num--)
							{
								CAbilityOverride abilityOverride = cAbility.CurrentOverrides[num];
								cAbility.UndoOverride(abilityOverride, perform: false);
							}
						}
						if (flag2)
						{
							ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
						}
						cAbility.OverrideAbilityValues(consumeOp.AbilityOverride, flag2);
						if (flag)
						{
							FinalizeOverrideAbilityType(cAbility, consumeOp.AbilityOverride);
						}
						if (consumeOp.AbilityOverride.SubAbilities == null || consumeOp.AbilityOverride.SubAbilities.Count <= 0)
						{
							continue;
						}
						foreach (CAbility subAbility in consumeOp.AbilityOverride.SubAbilities)
						{
							if (!subAbility.IsInlineSubAbility)
							{
								CAbility cAbility3 = CAbility.CopyAbility(subAbility, generateNewID: false);
								AbilityData.MiscAbilityData miscAbilityData = cAbility3.MiscAbilityData;
								if ((miscAbilityData == null || miscAbilityData.FilterSpecified != true) && cAbility.AbilityFilter != null)
								{
									cAbility3.AbilityFilter = cAbility.AbilityFilter.Copy();
									cAbility3.MiscAbilityData.FilterSpecified = true;
								}
								cAbility3.ParentAbility = cAbility;
								if (isCurrentPhaseAbility)
								{
									m_PhaseAbilities.Insert(0, new CPhaseAbility(cAbility3, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
								}
								else if (m_PhaseAbilities.IndexOf(phaseAbility) == m_PhaseAbilities.Count - 1)
								{
									m_PhaseAbilities.Add(new CPhaseAbility(cAbility3, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
								}
								else
								{
									m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(phaseAbility) + 1, new CPhaseAbility(cAbility3, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
								}
							}
						}
					}
					else
					{
						DLLDebug.LogWarning("Unable to find matching parent ability '" + consumeOp.ParentAbilityName + "' for consume.");
					}
				}
				if (actionAugmentation.CostAbility != null)
				{
					if (!string.IsNullOrEmpty(actionAugmentation.CostAbility.ParentName))
					{
						CPhaseAbility cPhaseAbility = m_CurrentPhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
						{
							if (x.m_Ability.Name == actionAugmentation.CostAbility.ParentName)
							{
								Guid? actionID = x.ActionID;
								Guid actionID2 = actionAugmentation.ActionID;
								if (!actionID.HasValue)
								{
									return false;
								}
								if (!actionID.HasValue)
								{
									return true;
								}
								return actionID.GetValueOrDefault() == actionID2;
							}
							return false;
						});
						if (cPhaseAbility != null)
						{
							CPhaseAbility currentPhaseAbility = CurrentPhaseAbility;
							StackInlineSubAbilities(new List<CAbility> { actionAugmentation.CostAbility }, null, performNow: true, stopPlayerSkipping: false, false);
							foreach (CItem activeOverrideItem in currentPhaseAbility.m_Ability.ActiveOverrideItems)
							{
								foreach (CAbilityOverride @override in activeOverrideItem.YMLData.Data.Overrides)
								{
									currentPhaseAbility.m_Ability.UndoOverride(@override, perform: false, activeOverrideItem);
								}
								GameState.InternalCurrentActor.Inventory.DeselectItem(activeOverrideItem);
							}
						}
						else
						{
							cPhaseAbility = m_PhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
							{
								if (x.m_Ability.Name == actionAugmentation.CostAbility.ParentName)
								{
									Guid? actionID = x.ActionID;
									Guid actionID2 = actionAugmentation.ActionID;
									if (!actionID.HasValue)
									{
										return false;
									}
									if (!actionID.HasValue)
									{
										return true;
									}
									return actionID.GetValueOrDefault() == actionID2;
								}
								return false;
							});
							if (cPhaseAbility != null)
							{
								m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(cPhaseAbility), new CPhaseAbility(CAbility.CopyAbility(actionAugmentation.CostAbility, generateNewID: false), GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID, null, null, killAfter: false, isCostAbility: true));
							}
							else
							{
								DLLDebug.LogWarning("Unable to find matching parent ability '" + actionAugmentation.CostAbility.ParentName + "' for consume.");
							}
						}
					}
					else
					{
						m_PhaseAbilities.Add(new CPhaseAbility(CAbility.CopyAbility(actionAugmentation.CostAbility, generateNewID: false), GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
					}
				}
				foreach (ElementInfusionBoardManager.EElement infusion in actionAugmentation.Infusions)
				{
					m_ElementsToInfuse.Add(infusion);
				}
				m_ActionAugmentationsToConsume.Add(actionAugmentation);
				m_ActionConsumeXP += actionAugmentation.XP;
				SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionAddConsumeAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", m_ActionAugmentationsToConsume));
			}
			else
			{
				foreach (ElementInfusionBoardManager.EElement infusion2 in actionAugmentation.Infusions)
				{
					m_ElementsToInfuse.Remove(infusion2);
				}
				m_ActionAugmentationsToConsume.Remove(actionAugmentation);
				m_ActionConsumeXP -= actionAugmentation.XP;
				if (actionAugmentation.CostAbility != null)
				{
					CPhaseAbility cPhaseAbility2 = m_PhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
					{
						if (x.m_Ability.Name == actionAugmentation.CostAbility.Name)
						{
							Guid? actionID = x.ActionID;
							Guid actionID2 = actionAugmentation.ActionID;
							if (!actionID.HasValue)
							{
								return false;
							}
							if (!actionID.HasValue)
							{
								return true;
							}
							return actionID.GetValueOrDefault() == actionID2;
						}
						return false;
					});
					if (cPhaseAbility2 != null)
					{
						m_PhaseAbilities.Remove(cPhaseAbility2);
					}
					else
					{
						UnstackAbility(actionAugmentation.CostAbility);
					}
				}
				foreach (CActionAugmentationOp consumeOp2 in actionAugmentation.AugmentationOps)
				{
					if (consumeOp2.ParentAbilityName == string.Empty)
					{
						if (consumeOp2.Ability == null)
						{
							continue;
						}
						CPhaseAbility cPhaseAbility3 = m_PhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
						{
							if (x.m_Ability.Name == consumeOp2.Ability.Name)
							{
								Guid? actionID = x.ActionID;
								Guid actionID2 = actionAugmentation.ActionID;
								if (!actionID.HasValue)
								{
									return false;
								}
								if (!actionID.HasValue)
								{
									return true;
								}
								return actionID.GetValueOrDefault() == actionID2;
							}
							return false;
						});
						if (cPhaseAbility3 != null)
						{
							m_PhaseAbilities.Remove(cPhaseAbility3);
						}
						else
						{
							UnstackAbility(consumeOp2.Ability);
						}
						continue;
					}
					CPhaseAbility phaseAbility2;
					bool isCurrentPhaseAbility2;
					bool isActiveMergedAbility2;
					CAbility cAbility4 = FindAbilityFromPhaseAbilityWithActionAugmentationParentName(actionAugmentation, consumeOp2, out phaseAbility2, out isCurrentPhaseAbility2, out isActiveMergedAbility2);
					if (consumeOp2.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
					{
						if (cAbility4 == null)
						{
							continue;
						}
						if (cAbility4.AbilityType == CAbility.EAbilityType.Null)
						{
							if (phaseAbility2 == null || isCurrentPhaseAbility2)
							{
								UnstackAbility(consumeOp2.Ability);
							}
							else
							{
								m_PhaseAbilities.Remove(phaseAbility2);
							}
							continue;
						}
						CCondition.ENegativeCondition eNegativeCondition2 = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == consumeOp2.Ability.AbilityType.ToString());
						if (eNegativeCondition2 != CCondition.ENegativeCondition.NA)
						{
							cAbility4.NegativeConditions.Remove(eNegativeCondition2);
							continue;
						}
						CCondition.EPositiveCondition ePositiveCondition2 = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == consumeOp2.Ability.AbilityType.ToString());
						if (ePositiveCondition2 != CCondition.EPositiveCondition.NA)
						{
							cAbility4.PositiveConditions.Remove(ePositiveCondition2);
						}
						else if (phaseAbility2 == null || isCurrentPhaseAbility2)
						{
							UnstackAbility(consumeOp2.Ability);
						}
						else
						{
							m_PhaseAbilities.Remove(phaseAbility2);
						}
					}
					else
					{
						if (consumeOp2.Type != CActionAugmentationOp.EActionAugmentationType.AbilityOverride || cAbility4 == null)
						{
							continue;
						}
						bool flag3 = consumeOp2.AbilityOverride.AbilityType.HasValue && consumeOp2.AbilityOverride.AbilityType != consumeOp2.AbilityOverride.OriginalAbility.AbilityType;
						bool flag4 = CurrentPhaseAbility == phaseAbility2 && cAbility4.TargetingActor != null && !passing;
						if (flag4)
						{
							ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
						}
						if (!passing)
						{
							cAbility4.UndoOverride(consumeOp2.AbilityOverride, flag4 && !flag3);
						}
						if (flag3)
						{
							foreach (CItem item2 in cAbility4.ActiveSingleTargetItems.Concat(cAbility4.ActiveOverrideItems))
							{
								cAbility4.TargetingActor.Inventory.DeselectItem(item2);
							}
							for (int num2 = cAbility4.CurrentOverrides.Count - 1; num2 >= 0; num2--)
							{
								CAbilityOverride abilityOverride2 = cAbility4.CurrentOverrides[num2];
								cAbility4.UndoOverride(abilityOverride2, perform: false);
							}
							ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
							FinalizeOverrideAbilityType(cAbility4);
						}
						if (consumeOp2.AbilityOverride.SubAbilities == null || consumeOp2.AbilityOverride.SubAbilities.Count <= 0)
						{
							continue;
						}
						foreach (CAbility subAbility2 in consumeOp2.AbilityOverride.SubAbilities)
						{
							if (!subAbility2.IsInlineSubAbility)
							{
								if (phaseAbility2 != null)
								{
									m_PhaseAbilities.Remove(phaseAbility2);
								}
								else
								{
									UnstackAbility(cAbility4);
								}
							}
						}
					}
				}
				SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionRemoveConsumeAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", m_ActionAugmentationsToConsume));
			}
			if (CurrentPhaseAbility != null)
			{
				foreach (CItem item3 in GameState.InternalCurrentActor.Inventory.SelectedItems.FindAll((CItem a) => a.YMLData.ItemType == CItem.EItemType.Override))
				{
					if (item3.YMLData.Data.CompareAbility == null || item3.YMLData.Data.CompareAbility.CompareAbility(CurrentPhaseAbility.m_Ability) || item3.YMLData.Data.ItemRequirements == null || item3.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(CurrentPhaseAbility.m_Ability.TargetingActor, CurrentPhaseAbility.m_Ability))
					{
						continue;
					}
					foreach (CAbilityOverride override2 in item3.YMLData.Data.Overrides)
					{
						CurrentPhaseAbility.m_Ability.UndoOverride(override2, perform: false, item3);
					}
					GameState.InternalCurrentActor.Inventory.DeselectItem(item3);
				}
			}
			if (!passing)
			{
				CToggledActionAugmentation_MessageData cToggledActionAugmentation_MessageData = new CToggledActionAugmentation_MessageData();
				cToggledActionAugmentation_MessageData.m_ActorSpawningMessage = GameState.InternalCurrentActor;
				cToggledActionAugmentation_MessageData.m_CurrentAbilityAfterToggling = CurrentPhaseAbility.m_Ability;
				ScenarioRuleClient.MessageHandler(cToggledActionAugmentation_MessageData);
			}
		}
		if (GameState.InternalCurrentActor is CPlayerActor cPlayerActor)
		{
			cPlayerActor.Inventory.HighlightUsableItems(CurrentPhaseAbility.m_Ability, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
		}
	}

	private CAbility FindAbilityFromPhaseAbilityWithActionAugmentationParentName(CActionAugmentation actionAugmentation, CActionAugmentationOp consumeOp, out CPhaseAbility phaseAbility, out bool isCurrentPhaseAbility, out bool isActiveMergedAbility)
	{
		CAbility result = null;
		isCurrentPhaseAbility = false;
		isActiveMergedAbility = false;
		phaseAbility = m_CurrentPhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
		{
			if (x.m_Ability.Name == consumeOp.ParentAbilityName)
			{
				Guid? actionID = x.ActionID;
				Guid actionID2 = actionAugmentation.ActionID;
				if (!actionID.HasValue)
				{
					return false;
				}
				if (!actionID.HasValue)
				{
					return true;
				}
				return actionID.GetValueOrDefault() == actionID2;
			}
			return false;
		});
		if (phaseAbility != null)
		{
			isCurrentPhaseAbility = true;
			return phaseAbility.m_Ability;
		}
		if (phaseAbility == null)
		{
			foreach (CPhaseAbility currentPhaseAbility in m_CurrentPhaseAbilities)
			{
				if (currentPhaseAbility.m_Ability is CAbilityMerged cAbilityMerged)
				{
					foreach (CAbility copiedMergedAbility in cAbilityMerged.CopiedMergedAbilities)
					{
						if (copiedMergedAbility.Name == consumeOp.ParentAbilityName && currentPhaseAbility.ActionID == actionAugmentation.ActionID)
						{
							phaseAbility = currentPhaseAbility;
							result = copiedMergedAbility;
							isCurrentPhaseAbility = true;
							if (cAbilityMerged.ActiveAbility.Name == copiedMergedAbility.Name)
							{
								isActiveMergedAbility = true;
							}
							break;
						}
					}
					continue;
				}
				if (currentPhaseAbility.m_Ability is CAbilityControlActor cAbilityControlActor)
				{
					foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
					{
						if (controlAbility.Name == consumeOp.ParentAbilityName && currentPhaseAbility.ActionID == actionAugmentation.ActionID)
						{
							phaseAbility = currentPhaseAbility;
							result = controlAbility;
							isCurrentPhaseAbility = true;
							break;
						}
					}
					continue;
				}
				foreach (CAbility subAbility in currentPhaseAbility.m_Ability.SubAbilities)
				{
					if (subAbility.IsInlineSubAbility && subAbility.Name == consumeOp.ParentAbilityName && currentPhaseAbility.ActionID == actionAugmentation.ActionID)
					{
						phaseAbility = currentPhaseAbility;
						result = subAbility;
						isCurrentPhaseAbility = true;
						break;
					}
				}
			}
		}
		if (phaseAbility == null)
		{
			phaseAbility = m_PhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
			{
				if (x.m_Ability.Name == consumeOp.ParentAbilityName)
				{
					Guid? actionID = x.ActionID;
					Guid actionID2 = actionAugmentation.ActionID;
					if (!actionID.HasValue)
					{
						return false;
					}
					if (!actionID.HasValue)
					{
						return true;
					}
					return actionID.GetValueOrDefault() == actionID2;
				}
				return false;
			});
			if (phaseAbility != null)
			{
				return phaseAbility.m_Ability;
			}
			if (phaseAbility == null)
			{
				foreach (CPhaseAbility phaseAbility2 in m_PhaseAbilities)
				{
					if (phaseAbility2.m_Ability is CAbilityMerged cAbilityMerged2)
					{
						foreach (CAbility copiedMergedAbility2 in cAbilityMerged2.CopiedMergedAbilities)
						{
							if (copiedMergedAbility2.Name == consumeOp.ParentAbilityName && phaseAbility2.ActionID == actionAugmentation.ActionID)
							{
								phaseAbility = phaseAbility2;
								result = copiedMergedAbility2;
								break;
							}
						}
					}
					else
					{
						if (!(phaseAbility2.m_Ability is CAbilityControlActor cAbilityControlActor2))
						{
							continue;
						}
						foreach (CAbility controlAbility2 in cAbilityControlActor2.ControlActorData.ControlAbilities)
						{
							if (controlAbility2.Name == consumeOp.ParentAbilityName && phaseAbility2.ActionID == actionAugmentation.ActionID)
							{
								phaseAbility = phaseAbility2;
								result = controlAbility2;
								break;
							}
						}
					}
				}
			}
		}
		return result;
	}

	public void HandleActiveBonusCostAbilityToggling(CActiveBonus fromActiveBonus, CAbility costAbility, bool toggleOn)
	{
		if (costAbility == null)
		{
			return;
		}
		if (toggleOn)
		{
			StackInlineSubAbilities(new List<CAbility> { costAbility }, null, performNow: true, stopPlayerSkipping: true, false, stopPlayerUndo: false, fromActiveBonus);
			return;
		}
		CPhaseAbility cPhaseAbility = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) => x.m_Ability.ID == costAbility.ID);
		if (cPhaseAbility != null)
		{
			m_PhaseAbilities.Remove(cPhaseAbility);
		}
		else
		{
			UnstackAbility(costAbility);
		}
	}

	public bool HasAnyUntoggleableActionAugments()
	{
		foreach (CActionAugmentation item in m_ActionAugmentationsToConsume)
		{
			if (!CanUntoggleActionAugment(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyActionAugmentationsConsumed()
	{
		return m_ActionAugmentationsConsumed.Count > 0;
	}

	public List<CActionAugmentation> ActionAugmentationsAvailableForCurrentAbility()
	{
		List<CActionAugmentation> list = m_ActionAugmentationsToConsume.ToList();
		GameState.CurrentActorAction currentAction = m_CurrentAction;
		CPhaseAbility currentPhaseAbility = CurrentPhaseAbility;
		if (currentAction.BaseCard is CAbilityCard && currentAction.Action.Augmentations != null && currentAction.Action.Augmentations.Count > 0)
		{
			foreach (CActionAugmentation actionAugmentation in currentAction.Action.Augmentations)
			{
				if (m_ActionAugmentationsConsumed.Any((CActionAugmentation a) => a.ActionID == actionAugmentation.ActionID && a.Name == actionAugmentation.Name))
				{
					continue;
				}
				List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements();
				bool flag = true;
				foreach (ElementInfusionBoardManager.EElement item in actionAugmentation.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
				{
					flag = availableElements.Remove(item);
					if (!flag)
					{
						break;
					}
				}
				if (actionAugmentation.Elements.Count((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any) > availableElements.Count)
				{
					flag = false;
				}
				if (!flag)
				{
					continue;
				}
				if (actionAugmentation.CostAbility != null)
				{
					if (!string.IsNullOrEmpty(actionAugmentation.CostAbility.ParentName))
					{
						if (currentPhaseAbility != null && currentPhaseAbility.m_Ability.Name == actionAugmentation.CostAbility.ParentName && currentPhaseAbility.ActionID == actionAugmentation.ActionID)
						{
							list.Add(actionAugmentation);
						}
					}
					else
					{
						list.Add(actionAugmentation);
					}
					continue;
				}
				foreach (CActionAugmentationOp consumeOp in actionAugmentation.AugmentationOps)
				{
					if (consumeOp.ParentAbilityName == string.Empty)
					{
						list.Add(actionAugmentation);
						continue;
					}
					CPhaseAbility cPhaseAbility = null;
					cPhaseAbility = m_CurrentPhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
					{
						if (x.m_Ability.Name == consumeOp.ParentAbilityName)
						{
							Guid? actionID = x.ActionID;
							Guid actionID2 = actionAugmentation.ActionID;
							if (actionID.HasValue && (!actionID.HasValue || actionID.GetValueOrDefault() == actionID2) && !x.m_Ability.IsInlineSubAbility)
							{
								if (x.m_Ability.AbilityType != CAbility.EAbilityType.Null)
								{
									return !x.m_Ability.AbilityHasHappened;
								}
								return true;
							}
						}
						return false;
					});
					if (cPhaseAbility == null)
					{
						foreach (CPhaseAbility currentPhaseAbility2 in m_CurrentPhaseAbilities)
						{
							if (currentPhaseAbility2.m_Ability is CAbilityMerged cAbilityMerged && cAbilityMerged.ActiveAbility?.Name == consumeOp.ParentAbilityName)
							{
								cPhaseAbility = currentPhaseAbility2;
								break;
							}
						}
					}
					if (cPhaseAbility != null)
					{
						list.Add(actionAugmentation);
					}
				}
			}
		}
		return list;
	}

	public bool CanUntoggleActionAugment(CActionAugmentation actionAugmentation)
	{
		if (m_ActionAugmentationsToConsume.Contains(actionAugmentation))
		{
			if (actionAugmentation.CostAbility != null && m_PreviousPhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
			{
				if (x.m_Ability.Name == actionAugmentation.CostAbility.Name)
				{
					Guid? actionID = x.ActionID;
					Guid actionID2 = actionAugmentation.ActionID;
					if (!actionID.HasValue)
					{
						return false;
					}
					if (!actionID.HasValue)
					{
						return true;
					}
					return actionID.GetValueOrDefault() == actionID2;
				}
				return false;
			}) != null)
			{
				return false;
			}
			foreach (CActionAugmentationOp consumeOp in actionAugmentation.AugmentationOps)
			{
				if (consumeOp.Ability != null && m_PreviousPhaseAbilities.SingleOrDefault(delegate(CPhaseAbility x)
				{
					if (x.m_Ability.Name == consumeOp.Ability.Name)
					{
						Guid? actionID = x.ActionID;
						Guid actionID2 = actionAugmentation.ActionID;
						if (!actionID.HasValue)
						{
							return false;
						}
						if (!actionID.HasValue)
						{
							return true;
						}
						return actionID.GetValueOrDefault() == actionID2;
					}
					return false;
				}) != null)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool ShouldUntoggleActionAugmentAfterPassingAbility(CActionAugmentation actionAugmentation)
	{
		foreach (CActionAugmentationOp augmentationOp in actionAugmentation.AugmentationOps)
		{
			if (CurrentPhaseAbility != null && CurrentPhaseAbility.m_Ability != null && augmentationOp.ParentAbilityType != CurrentPhaseAbility.m_Ability.AbilityType && CurrentPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Push)
			{
				return false;
			}
			if (augmentationOp.AbilityOverride != null && augmentationOp.AbilityOverride.ControlActorData?.ControlAbilities != null && augmentationOp.AbilityOverride.ControlActorData.ControlAbilities.Count > 0)
			{
				return false;
			}
			if (augmentationOp.Ability != null)
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnUpdate()
	{
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.Update();
		}
	}

	private void Perform(bool fullAbilityRestart)
	{
		m_CurrentActionID = CurrentPhaseAbility.ActionID;
		if (fullAbilityRestart)
		{
			if (PreviousPhaseAbilities.Count > 0 && ((PreviousPhaseAbilities.Last() == m_FirstPhaseAbility && !m_HasAnyAbilityHappened) || (PreviousPhaseAbilities.Count == 1 && PreviousPhaseAbilities.First().m_Ability.AbilityType == CAbility.EAbilityType.Null)))
			{
				m_FirstPhaseAbility = CurrentPhaseAbility;
			}
			CStartActorAbility_MessageData cStartActorAbility_MessageData = new CStartActorAbility_MessageData(GameState.InternalCurrentActor);
			cStartActorAbility_MessageData.m_IsFirstAbility = m_FirstPhaseAbility == CurrentPhaseAbility;
			cStartActorAbility_MessageData.m_Ability = CurrentPhaseAbility.m_Ability;
			cStartActorAbility_MessageData.merged = false;
			if (cStartActorAbility_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.MergedMoveAttack)
			{
				cStartActorAbility_MessageData.merged = true;
			}
			ScenarioRuleClient.MessageHandler(cStartActorAbility_MessageData);
			if (GameState.InternalCurrentActor is CPlayerActor cPlayerActor)
			{
				cPlayerActor.Inventory.HighlightUsableItems(CurrentPhaseAbility.m_Ability, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
			}
		}
		CurrentPhaseAbility.m_Ability.Perform();
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionPerform, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
	}

	public void UndoItemAbility()
	{
		m_CurrentPhaseAbilities.Remove(CurrentPhaseAbility);
		RemainingPhaseAbilities.RemoveAll((CPhaseAbility x) => x.m_Ability.IsItemAbility);
		GameState.InternalCurrentActor.Inventory.Undo();
		EndAbilitySynchronise(undoItemAbility: true);
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionUndoItemAbility, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
	}

	public void UndoChooseAbility()
	{
		CurrentPhaseAbility.m_Ability.ParentAbility.Undo();
		m_CurrentPhaseAbilities.Remove(CurrentPhaseAbility);
		GameState.InternalCurrentActor.Inventory.Undo();
		EndAbilitySynchronise();
	}

	public void OnFirstAbilityStarted()
	{
		if ((!m_HasAnyNonItemAbilityHappened || CurrentPhaseAbility.m_Ability.IsItemAbility) && !CurrentPhaseAbility.m_Ability.IsScenarioModifierAbility)
		{
			string defaultAbility = "";
			if (CurrentPhaseAbility.m_Ability is CAbilityAttack cAbilityAttack)
			{
				defaultAbility = (cAbilityAttack.IsDefaultAttack ? "GUI_DEFAULT_ATTACK" : "");
			}
			if (CurrentPhaseAbility.m_Ability is CAbilityMove cAbilityMove)
			{
				defaultAbility = (cAbilityMove.IsDefaultMove ? "GUI_DEFAULT_MOVE" : "");
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnFirstAbilityStarted, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, "", defaultAbility));
			CAbilityStartUpdateCombatLog_MessageData cAbilityStartUpdateCombatLog_MessageData = new CAbilityStartUpdateCombatLog_MessageData(GameState.InternalCurrentActor);
			cAbilityStartUpdateCombatLog_MessageData.m_AbilityType = CurrentPhaseAbility.m_Ability.AbilityType;
			cAbilityStartUpdateCombatLog_MessageData.m_Ability = CurrentPhaseAbility.m_Ability;
			ScenarioRuleClient.MessageHandler(cAbilityStartUpdateCombatLog_MessageData);
		}
		UpdateItemsUsedInPhase();
	}

	public void UpdateItemsUsedInPhase()
	{
		COverrideItemUsedUpdateCombatLog_MessageData cOverrideItemUsedUpdateCombatLog_MessageData = new COverrideItemUsedUpdateCombatLog_MessageData(GameState.InternalCurrentActor);
		foreach (CItem activeOverrideItem in CurrentPhaseAbility.m_Ability.ActiveOverrideItems)
		{
			if (!m_ItemsLoggedAsUsedThisPhase.Contains(activeOverrideItem))
			{
				m_ItemsLoggedAsUsedThisPhase.Add(activeOverrideItem);
				cOverrideItemUsedUpdateCombatLog_MessageData.m_ItemsToUpdate.Add(activeOverrideItem);
			}
		}
		foreach (CItem activeSingleTargetItem in CurrentPhaseAbility.m_Ability.ActiveSingleTargetItems)
		{
			if (!m_ItemsLoggedAsUsedThisPhase.Contains(activeSingleTargetItem))
			{
				m_ItemsLoggedAsUsedThisPhase.Add(activeSingleTargetItem);
				cOverrideItemUsedUpdateCombatLog_MessageData.m_ItemsToUpdate.Add(activeSingleTargetItem);
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionUpdateItemsUsedInPhase, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
		ScenarioRuleClient.MessageHandler(cOverrideItemUsedUpdateCombatLog_MessageData);
	}

	protected override void OnNextStep(bool passing = false)
	{
		bool overridingCurrentActor = GameState.OverridingCurrentActor;
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionNextStep, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
		if (!m_EndAbilitySynced && (CurrentPhaseAbility == null || CurrentPhaseAbility.m_Ability == null || !CurrentPhaseAbility.m_Ability.IsInlineSubAbility))
		{
			return;
		}
		if (CurrentPhaseAbility == null)
		{
			EndAbilitySynchronise();
			return;
		}
		bool flag = CurrentPhaseAbility.m_Ability.AbilityHasHappened || CurrentPhaseAbility.m_Ability.AugmentsAdded;
		if (!m_HasAnyAbilityHappened && (flag || passing))
		{
			GameState.InternalCurrentActor.AbilityTypesPerformedThisAction.Clear();
		}
		m_HasAnyAbilityHappened |= flag;
		m_HasAnyNonItemAbilityHappened = m_HasAnyNonItemAbilityHappened || (flag && !CurrentPhaseAbility.m_Ability.IsItemAbility);
		if (passing && !flag)
		{
			for (int num = m_ActionAugmentationsToConsume.Count - 1; num >= 0; num--)
			{
				CActionAugmentation actionAugmentation = m_ActionAugmentationsToConsume[num];
				if (ShouldUntoggleActionAugmentAfterPassingAbility(actionAugmentation))
				{
					ToggleActionAugmentation(actionAugmentation, passing: true);
				}
			}
		}
		if (CurrentPhaseAbility == null)
		{
			EndAbilitySynchronise();
			return;
		}
		CPhaseAbility cPhaseAbility = CurrentPhaseAbility;
		if (m_PhaseAbilities.Count > 0 && m_PhaseAbilities[0].m_Ability.IsSubAbility && CurrentPhaseAbility.m_Ability != m_PhaseAbilities[0].m_Ability.ParentAbility)
		{
			CPhaseAbility cPhaseAbility2 = PreviousPhaseAbilities.SingleOrDefault((CPhaseAbility x) => x.m_Ability == m_PhaseAbilities[0].m_Ability.ParentAbility);
			if (cPhaseAbility2 != null && cPhaseAbility2.m_Ability.AbilityType == CAbility.EAbilityType.Move)
			{
				cPhaseAbility = cPhaseAbility2;
			}
		}
		while (m_PhaseAbilities.Count > 0 && m_PhaseAbilities[0].m_Ability.IsSubAbility && (cPhaseAbility.m_Ability.AbilityType != CAbility.EAbilityType.Move || !m_HasAnyAbilityHappened || (m_PhaseAbilities[0].m_Ability.MiscAbilityData != null && m_PhaseAbilities[0].m_Ability.MiscAbilityData.TreatAsNonSubAbility == true)) && (cPhaseAbility.m_Ability.AbilityType != CAbility.EAbilityType.Move || !cPhaseAbility.m_Ability.AbilityHasHappened || m_PhaseAbilities[0].m_Ability.MiscAbilityData == null || m_PhaseAbilities[0].m_Ability.MiscAbilityData.TreatAsNonSubAbility != true) && (!m_PhaseAbilities[0].m_Ability.IsInlineSubAbility || m_PhaseAbilities[0].m_Ability.InlineSubAbilityTiles.Count <= 0) && m_CurrentPhaseAbilities.Find((CPhaseAbility w) => w.m_Ability == m_PhaseAbilities[0].m_Ability.ParentAbility && w != CurrentPhaseAbility) == null && (passing || m_PhaseAbilities[0].m_Ability.ParentAbility == null || (!m_PhaseAbilities[0].m_Ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && (m_PhaseAbilities[0].m_Ability.ParentAbility.TilesSelected == null || m_PhaseAbilities[0].m_Ability.ParentAbility.TilesSelected.Count == 0))))
		{
			m_PhaseAbilities.Remove(m_PhaseAbilities[0]);
		}
		if (!CurrentPhaseAbility.ItemID.HasValue)
		{
			GameState.InternalCurrentActor.ActorActionHasHappened |= flag;
			if (CurrentPhaseAbility.m_BaseCard != null)
			{
				CurrentPhaseAbility.m_BaseCard.ActionHasHappened |= flag;
			}
		}
		else
		{
			GameState.InternalCurrentActor.Inventory.HandleUsedItem(CurrentPhaseAbility.ItemID.Value);
			GameState.InternalCurrentActor.Inventory.OnAbilityEnd();
		}
		string animOverload = CurrentPhaseAbility.m_Ability.AnimOverload;
		CurrentPhaseAbility.m_Ability.AbilityEnded();
		if (CurrentPhaseAbility.m_Ability.AbilityHasHappened || CurrentPhaseAbility.m_Ability.AugmentsAdded)
		{
			GameState.InternalCurrentActor.ProcessConditionTokens(EConditionDecTrigger.Abilities);
			if (!CurrentPhaseAbility.m_Ability.IsItemAbility)
			{
				m_ApplyActionXP = true;
			}
			if (m_CurrentActionID.HasValue)
			{
				if (GameState.InternalCurrentActor.Class is CCharacterClass)
				{
					foreach (CActionAugmentation item in m_ActionAugmentationsToConsume)
					{
						item.ConsumeElements(GameState.InternalCurrentActor);
					}
					m_ActionAugmentationsConsumed.AddRange(m_ActionAugmentationsToConsume);
					m_ActionAugmentationsToConsume.Clear();
				}
				else
				{
					for (int num2 = m_ActionAugmentationsToConsume.Count - 1; num2 >= 0; num2--)
					{
						CActionAugmentation cActionAugmentation = m_ActionAugmentationsToConsume[num2];
						foreach (CActionAugmentationOp augmentationOp in cActionAugmentation.AugmentationOps)
						{
							if (CurrentPhaseAbility.m_Ability.Name == augmentationOp.ParentAbilityName || (augmentationOp.Ability != null && CurrentPhaseAbility.m_Ability.Name == augmentationOp.Ability.Name))
							{
								cActionAugmentation.ConsumeElements(GameState.InternalCurrentActor);
								m_ActionAugmentationsConsumed.Add(cActionAugmentation);
								m_ActionAugmentationsToConsume.Remove(cActionAugmentation);
								break;
							}
						}
						if (cActionAugmentation.AugmentationOps.Count == 0)
						{
							cActionAugmentation.ConsumeElements(GameState.InternalCurrentActor);
							m_ActionAugmentationsConsumed.Add(cActionAugmentation);
							m_ActionAugmentationsToConsume.Remove(cActionAugmentation);
						}
					}
				}
			}
			CMessageData message = new CActionAbilityHasHappened_MessageData(GameState.InternalCurrentActor)
			{
				PendingElementsToInfuse = (m_ElementsToInfuse.Count > 0 && !overridingCurrentActor),
				m_Ability = CurrentPhaseAbility.m_Ability
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		CurrentPhaseAbility.m_EndAction?.Invoke();
		m_PreviousPhaseAbilities.Add(CurrentPhaseAbility);
		m_CurrentPhaseAbilities.Remove(CurrentPhaseAbility);
		if (m_CurrentTriggeredActiveBonus != null)
		{
			if (m_CurrentTriggeredActiveBonus.Ability.ActiveBonusData.CostAbility != null && m_CurrentTriggeredActiveBonus.Ability.ActiveBonusData.CostAbility.ID == PreviousPhaseAbilities.Last().m_Ability.ID)
			{
				m_CurrentTriggeredActiveBonus.ToggleLocked = true;
			}
			if (m_CurrentTriggeredActiveBonus is CDuringActionAbilityActiveBonus cDuringActionAbilityActiveBonus)
			{
				cDuringActionAbilityActiveBonus.TriggerAbility();
			}
			else if (m_CurrentTriggeredActiveBonus is CDuringTurnAbilityActiveBonus cDuringTurnAbilityActiveBonus)
			{
				cDuringTurnAbilityActiveBonus.TriggerAbility();
			}
			m_CurrentTriggeredActiveBonus = null;
		}
		bool flag2 = false;
		if (CurrentPhaseAbility != null && overridingCurrentActor && CurrentPhaseAbility.TargetingActor != GameState.InternalCurrentActor)
		{
			for (int num3 = GameState.OverridenActionActorStack.Count - 1; num3 >= 0; num3--)
			{
				GameState.EndOverrideCurrentActorForOneAction();
				flag2 = true;
				if (CurrentPhaseAbility.TargetingActor == GameState.InternalCurrentActor)
				{
					break;
				}
			}
		}
		else if (CurrentPhaseAbility == null && overridingCurrentActor)
		{
			for (int num4 = GameState.OverridenActionActorStack.Count - 1; num4 >= 0; num4--)
			{
				GameState.EndOverrideCurrentActorForOneAction();
				flag2 = true;
			}
		}
		if (m_CurrentPhaseAbilities.Count > 0 && CurrentPhaseAbility.m_Ability.TargetingActor != null && (ScenarioManager.Scenario.HasActor(CurrentPhaseAbility.m_Ability.TargetingActor) || CurrentPhaseAbility.m_Ability.ProcessIfDead))
		{
			if (m_CurrentItemID.HasValue && m_CurrentItemID != CurrentPhaseAbility?.ItemID)
			{
				m_CurrentItemID = null;
			}
			bool fullAbilityRestart = false;
			if (PreviousPhaseAbilities.Count > 0 && (!PreviousPhaseAbilities.Last().m_Ability.OnDeath || !CurrentPhaseAbility.m_Ability.OnDeath) && !PreviousPhaseAbilities.Last().m_Ability.StackedAttackEffectAbility && !CurrentPhaseAbility.m_Ability.AbilityHasHappened)
			{
				if (m_FirstPhaseAbility != PreviousPhaseAbilities[0])
				{
					m_FirstPhaseAbility = PreviousPhaseAbilities[0];
				}
				CurrentPhaseAbility.m_Ability.Restart();
				fullAbilityRestart = true;
			}
			if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
			{
				CShowAugmentationBar_MessageData message2 = new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
				{
					m_Action = m_CurrentAction.Action,
					m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			Perform(fullAbilityRestart);
			return;
		}
		CPhaseAbility currentPhaseAbility = CurrentPhaseAbility;
		if (currentPhaseAbility != null && currentPhaseAbility.m_Ability?.SkipAnim == true && flag)
		{
			CurrentPhaseAbility.m_Ability.AnimOverload = string.Empty;
			m_DontMoveAbilityState = true;
			CurrentPhaseAbility.m_Ability.Start(GameState.InternalCurrentActor, GameState.InternalCurrentActor);
			CWaitForProgressChoreographer_MessageData message3 = new CWaitForProgressChoreographer_MessageData(GameState.InternalCurrentActor)
			{
				WaitTickFrame = 10000,
				WaitActor = GameState.InternalCurrentActor,
				ClearEvents = false
			};
			ScenarioRuleClient.MessageHandler(message3);
			return;
		}
		if (m_PhaseAbilities.Count > 0 && m_PhaseAbilities[0].m_Ability.SkipAnim && (ScenarioManager.Scenario.HasActor(m_PhaseAbilities[0].TargetingActor) || m_PhaseAbilities[0].m_Ability.ProcessIfDead))
		{
			m_DontMoveAbilityState = true;
			m_CurrentPhaseAbilities.Add(m_PhaseAbilities[0]);
			m_PhaseAbilities.Remove(m_PhaseAbilities[0]);
			if (CurrentPhaseAbility.TargetingActor != null && CurrentPhaseAbility.TargetingActor != GameState.InternalCurrentActor)
			{
				CActor targetingActor = CurrentPhaseAbility.TargetingActor;
				bool killAfterAction = CurrentPhaseAbility.KillAfterAction;
				GameState.OverrideCurrentActorForOneAction(targetingActor, null, killAfterAction);
			}
			if (flag)
			{
				if (CurrentPhaseAbility.m_Ability.AnimOverload.Contains("+Skip"))
				{
					CurrentPhaseAbility.m_Ability.AnimOverload = "+Skip";
				}
				else
				{
					CurrentPhaseAbility.m_Ability.AnimOverload = string.Empty;
				}
			}
			else if (CurrentPhaseAbility.m_Ability.AnimOverload == string.Empty)
			{
				CurrentPhaseAbility.m_Ability.AnimOverload = animOverload;
			}
			else if (CurrentPhaseAbility.m_Ability.AnimOverload.Contains("+Skip"))
			{
				CurrentPhaseAbility.m_Ability.AnimOverload = CurrentPhaseAbility.m_Ability.AnimOverload.Replace("+Skip", "");
			}
			CItem.EItemTrigger[] triggers = new CItem.EItemTrigger[2]
			{
				CItem.EItemTrigger.SingleTarget,
				CItem.EItemTrigger.SingleAbility
			};
			GameState.InternalCurrentActor.Inventory.HandleUsedItems(triggers);
			CurrentPhaseAbility.m_Ability.Start(CurrentPhaseAbility.TargetingActor, CurrentPhaseAbility.TargetingActor);
			CStartActorAbility_MessageData cStartActorAbility_MessageData = new CStartActorAbility_MessageData(GameState.InternalCurrentActor);
			cStartActorAbility_MessageData.m_IsFirstAbility = m_FirstPhaseAbility == CurrentPhaseAbility;
			cStartActorAbility_MessageData.m_Ability = CurrentPhaseAbility.m_Ability;
			cStartActorAbility_MessageData.merged = false;
			if (cStartActorAbility_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.MergedMoveAttack)
			{
				cStartActorAbility_MessageData.merged = true;
			}
			ScenarioRuleClient.MessageHandler(cStartActorAbility_MessageData);
			CWaitForProgressChoreographer_MessageData message4 = new CWaitForProgressChoreographer_MessageData(GameState.InternalCurrentActor)
			{
				WaitTickFrame = 10000,
				WaitActor = GameState.InternalCurrentActor,
				ClearEvents = false
			};
			ScenarioRuleClient.MessageHandler(message4);
			return;
		}
		if (!flag2)
		{
			m_EndAbilitySynced = false;
		}
		else
		{
			m_DontMoveAbilityState = false;
		}
		for (int num5 = m_PhaseAbilities.Count - 1; num5 >= 0; num5--)
		{
			if ((m_PhaseAbilities[num5].TargetingActor == null && GameState.InternalCurrentActor.IsDead) || (m_PhaseAbilities[num5].TargetingActor != null && m_PhaseAbilities[num5].TargetingActor.IsDead))
			{
				m_PhaseAbilities.RemoveAt(num5);
			}
		}
		for (int num6 = m_CurrentPhaseAbilities.Count - 1; num6 >= 0; num6--)
		{
			if ((m_CurrentPhaseAbilities[num6].TargetingActor == null && GameState.InternalCurrentActor.IsDead) || (m_CurrentPhaseAbilities[num6].TargetingActor != null && m_CurrentPhaseAbilities[num6].TargetingActor.IsDead))
			{
				m_CurrentPhaseAbilities.RemoveAt(num6);
			}
		}
		CEndActorAbilityAnimSync_MessageData cEndActorAbilityAnimSync_MessageData = new CEndActorAbilityAnimSync_MessageData(GameState.InternalCurrentActor);
		cEndActorAbilityAnimSync_MessageData.m_IsLastAbility = CurrentPhaseAbility == null && m_PhaseAbilities.Count == 0;
		ScenarioRuleClient.MessageHandler(cEndActorAbilityAnimSync_MessageData);
	}

	public void EnemyConsume()
	{
		for (int num = m_ActionAugmentationsToConsume.Count - 1; num >= 0; num--)
		{
			CActionAugmentation cActionAugmentation = m_ActionAugmentationsToConsume[num];
			cActionAugmentation.ConsumeElements(GameState.InternalCurrentActor);
			m_ActionAugmentationsConsumed.Add(cActionAugmentation);
			m_ActionAugmentationsToConsume.Remove(cActionAugmentation);
		}
	}

	public List<ElementInfusionBoardManager.EElement> GetReservedElements()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		if (m_CurrentActionID.HasValue && m_ActionAugmentationsToConsume != null && m_ActionAugmentationsToConsume.Count > 0)
		{
			foreach (CActionAugmentation item in m_ActionAugmentationsToConsume)
			{
				foreach (ElementInfusionBoardManager.EElement item2 in item.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
				{
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public void EndAbilitySynchronise(bool undoItemAbility = false)
	{
		m_EndAbilitySynced = true;
		ScenarioRuleClient.MessageHandler(new CClearAllActorEvents_MessageData());
		bool flag = false;
		if (m_CurrentPhaseAbilities.Count > 0 && (ScenarioManager.Scenario.HasActor(CurrentPhaseAbility.TargetingActor) || CurrentPhaseAbility.m_Ability.ProcessIfDead))
		{
			if (m_CurrentItemID.HasValue && m_CurrentItemID != CurrentPhaseAbility?.ItemID)
			{
				GameState.InternalCurrentActor.Inventory.SelectedItems.SingleOrDefault((CItem s) => s.ID == m_CurrentItemID.Value);
				if (!undoItemAbility)
				{
					_ = new CItem.EItemTrigger[2]
					{
						CItem.EItemTrigger.SingleTarget,
						CItem.EItemTrigger.SingleAbility
					};
				}
				m_CurrentItemID = null;
			}
			bool fullAbilityRestart = false;
			if (PreviousPhaseAbilities.Count > 0 && !PreviousPhaseAbilities.Last().m_Ability.OnDeath && !PreviousPhaseAbilities.Last().m_Ability.StackedAttackEffectAbility && !CurrentPhaseAbility.m_Ability.AbilityHasHappened)
			{
				if (m_FirstPhaseAbility != PreviousPhaseAbilities[0])
				{
					m_FirstPhaseAbility = PreviousPhaseAbilities[0];
				}
				CurrentPhaseAbility.m_Ability.Restart();
				fullAbilityRestart = true;
			}
			if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
			{
				CShowAugmentationBar_MessageData message = new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
				{
					m_Action = m_CurrentAction.Action,
					m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			Perform(fullAbilityRestart);
		}
		else if (m_PhaseAbilities.Count > 0 && (ScenarioManager.Scenario.HasActor(m_PhaseAbilities[0].TargetingActor) || m_PhaseAbilities[0].m_Ability.ProcessIfDead))
		{
			AbilityData.MiscAbilityData miscAbilityData = m_PhaseAbilities[0].m_Ability.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.PerformXTimesBasedOn.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = m_PhaseAbilities[0].m_Ability.MiscAbilityData;
				if (miscAbilityData2 == null || miscAbilityData2.PerformXTimesBasedOn.Value != CAbility.EStatIsBasedOnXType.None)
				{
					List<CPhaseAbility> list = new List<CPhaseAbility>();
					CAbility.EStatIsBasedOnXType basedOnOverride = m_PhaseAbilities[0].m_Ability.MiscAbilityData?.PerformXTimesBasedOn.Value ?? CAbility.EStatIsBasedOnXType.None;
					int statIsBasedOnXValue = m_PhaseAbilities[0].m_Ability.GetStatIsBasedOnXValue(m_PhaseAbilities[0].TargetingActor, null, m_PhaseAbilities[0].m_Ability.AbilityFilter, basedOnOverride);
					if (statIsBasedOnXValue > 0)
					{
						for (int num = 0; num < statIsBasedOnXValue; num++)
						{
							CPhaseAbility cPhaseAbility = new CPhaseAbility(CAbility.CopyAbility(m_PhaseAbilities[0].m_Ability, generateNewID: false, fullCopy: true), m_PhaseAbilities[0].TargetingActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID);
							cPhaseAbility.m_Ability.MiscAbilityData.PerformXTimesBasedOn = null;
							cPhaseAbility.m_Ability.AnimOverload = "None";
							list.Add(cPhaseAbility);
						}
						m_PhaseAbilities.Remove(m_PhaseAbilities[0]);
						m_PhaseAbilities.InsertRange(0, list);
					}
					else
					{
						m_PhaseAbilities[0].m_Ability.SetCancelAbilityFlag(cancel: true);
						m_PhaseAbilities[0].m_Ability.MiscAbilityData.PerformXTimesBasedOn = null;
					}
				}
			}
			m_CurrentPhaseAbilities.Add(m_PhaseAbilities[0]);
			m_PhaseAbilities.Remove(m_PhaseAbilities[0]);
			if (!flag)
			{
				HandleRemainingPhaseAbilitiesFromEndAbilitySync();
			}
		}
		else
		{
			flag = true;
			if (!m_EndActionNonToggleActivesAdded && GameState.InternalCurrentActor.Class is CCharacterClass cCharacterClass)
			{
				foreach (CEndActionAbilityActiveBonus endActionActiveBonus in from x in cCharacterClass.FindActiveBonuses(CActiveBonus.EActiveBonusBehaviourType.EndActionAbility, GameState.InternalCurrentActor)
					where x is CEndActionAbilityActiveBonus
					select x)
				{
					if (!endActionActiveBonus.IsRestricted(GameState.InternalCurrentActor) && endActionActiveBonus.RequirementsMet() && endActionActiveBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus && (endActionActiveBonus.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || m_PreviousPhaseAbilities.Any((CPhaseAbility x) => endActionActiveBonus.Ability.ActiveBonusData.ValidAbilityTypes.Any((CAbility.EAbilityType y) => x.m_Ability.IsValidAbilityForActiveBonusOfType(y)))))
					{
						endActionActiveBonus.AddAbility = CAbility.CopyAbility(cAbilityAddActiveBonus.AddAbility, generateNewID: true, fullCopy: true);
						CPhaseAbility item = new CPhaseAbility(endActionActiveBonus.AddAbility, GameState.InternalCurrentActor, endActionActiveBonus.BaseCard, m_CurrentAction.Action.ID);
						m_CurrentPhaseAbilities.Add(item);
						endActionActiveBonus.TriggerAbility();
						m_HasAnyAbilityHappened = true;
						flag = false;
					}
				}
				m_EndActionNonToggleActivesAdded = true;
				if (!flag)
				{
					HandleRemainingPhaseAbilitiesFromEndAbilitySync();
				}
			}
		}
		if (flag)
		{
			if (!undoItemAbility)
			{
				GameState.InternalCurrentActor.Inventory.HandleUsedItems();
			}
			else
			{
				GameState.InternalCurrentActor.Inventory.RefreshItems(CItem.EItemSlotState.Locked);
			}
			foreach (CActiveBonus pendingStartEndTurnAbilityBonusTrigger in GameState.PendingStartEndTurnAbilityBonusTriggers)
			{
				if (pendingStartEndTurnAbilityBonusTrigger is CStartRoundAbilityActiveBonus cStartRoundAbilityActiveBonus)
				{
					cStartRoundAbilityActiveBonus.TriggerAbility();
				}
				else if (pendingStartEndTurnAbilityBonusTrigger is CStartTurnAbilityActiveBonus cStartTurnAbilityActiveBonus)
				{
					cStartTurnAbilityActiveBonus.TriggerAbility();
				}
				else if (pendingStartEndTurnAbilityBonusTrigger is CEndTurnAbilityActiveBonus cEndTurnAbilityActiveBonus)
				{
					cEndTurnAbilityActiveBonus.TriggerAbility();
				}
				else if (pendingStartEndTurnAbilityBonusTrigger is CEndRoundAbilityActiveBonus cEndRoundAbilityActiveBonus)
				{
					cEndRoundAbilityActiveBonus.TriggerAbility();
				}
				else if (pendingStartEndTurnAbilityBonusTrigger is CDuringActionAbilityActiveBonus cDuringActionAbilityActiveBonus)
				{
					cDuringActionAbilityActiveBonus.TriggerAbility();
				}
				if (pendingStartEndTurnAbilityBonusTrigger.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns && GameState.OverridingCurrentActor)
				{
					GameState.EndOverrideCurrentActorForOneAction();
				}
			}
			GameState.PendingStartEndTurnAbilityBonusTriggers.Clear();
			foreach (CActiveBonus item2 in CActiveBonus.FindAllActiveBonuses())
			{
				item2.OnActionEnded(GameState.InternalCurrentActor);
			}
			if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ItemCard)
			{
				PhaseManager.EndItemAbilities();
			}
			else if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase)
			{
				PhaseManager.EndAbilities();
			}
			else if (GameState.CurrentActionInitiator == GameState.EActionInitiator.OverrideTurnAction)
			{
				PhaseManager.EndOverrideTurnAbilities();
			}
			else if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ActiveBonus || GameState.CurrentActionInitiator == GameState.EActionInitiator.ScenarioModifier || GameState.CurrentActionInitiator == GameState.EActionInitiator.CompanionSummon)
			{
				PhaseManager.EndActiveBonusOrScenarioModifierAbility();
			}
			else
			{
				if (!m_ShownEndOfActionActiveBonuses && GameState.InternalCurrentActor.Type == CActor.EType.Player)
				{
					foreach (CActiveBonus item3 in CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor))
					{
						if (item3.BespokeBehaviour != null && item3.Ability.ActiveBonusData.IsToggleBonus && item3.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.EndActionAbility && !item3.IsRestricted(GameState.InternalCurrentActor) && item3.RequirementsMet() && item3.CanToggleActiveBonus(GameState.InternalCurrentActor))
						{
							m_ShownEndOfActionActiveBonuses = true;
							CShowEndOfActionToggleBonuses_MessageData message2 = new CShowEndOfActionToggleBonuses_MessageData(GameState.InternalCurrentActor);
							ScenarioRuleClient.MessageHandler(message2);
							return;
						}
					}
				}
				CEndAction_MessageData cEndAction_MessageData = new CEndAction_MessageData(GameState.InternalCurrentActor);
				cEndAction_MessageData.m_ActionHappened = m_HasAnyNonItemAbilityHappened;
				cEndAction_MessageData.m_ActionName = m_CurrentAction.BaseCard.Name;
				ScenarioRuleClient.MessageHandler(cEndAction_MessageData);
				if (m_HasAnyNonItemAbilityHappened)
				{
					GameState.InternalCurrentActor.GainXP(m_ActionConsumeXP);
					if (m_ApplyActionXP)
					{
						GameState.InternalCurrentActor.GainXP(m_ActionXP);
					}
					foreach (ElementInfusionBoardManager.EElement item4 in m_ElementsToInfuse)
					{
						ElementInfusionBoardManager.Infuse(item4, GameState.InternalCurrentActor);
					}
					GameState.InternalCurrentActor.ProcessConditionTokens(EConditionDecTrigger.Actions);
					if (GameState.InternalCurrentActor.ExhaustAfterAction)
					{
						bool actorWasAsleep = GameState.InternalCurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
						GameState.KillActor(GameState.InternalCurrentActor, GameState.InternalCurrentActor, CActor.ECauseOfDeath.Suicide, out var _, null, actorWasAsleep);
					}
				}
				GameState.NextPhase();
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionEndAbilitySynchronise, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
	}

	private void HandleRemainingPhaseAbilitiesFromEndAbilitySync(bool FullAbilityRestart = true)
	{
		if (CurrentPhaseAbility.TargetingActor != null && CurrentPhaseAbility.TargetingActor != GameState.InternalCurrentActor)
		{
			CActor targetingActor = CurrentPhaseAbility.TargetingActor;
			bool killAfterAction = CurrentPhaseAbility.KillAfterAction;
			GameState.OverrideCurrentActorForOneAction(targetingActor, null, killAfterAction);
		}
		CItem.EItemTrigger[] triggers = new CItem.EItemTrigger[2]
		{
			CItem.EItemTrigger.SingleTarget,
			CItem.EItemTrigger.SingleAbility
		};
		GameState.InternalCurrentActor.Inventory.HandleUsedItems(triggers);
		_ = CurrentPhaseAbility.m_Ability;
		CurrentPhaseAbility.m_Ability.Start(CurrentPhaseAbility.TargetingActor, CurrentPhaseAbility.TargetingActor);
		List<CPhaseAbility> list = m_PhaseAbilities.Concat(m_CurrentPhaseAbilities).ToList();
		if (!m_CheckedForStartActionSongs)
		{
			foreach (CPhaseAbility item in list)
			{
				item.m_Ability.ProcessSongOverridesAndAbilities(GameState.InternalCurrentActor, CSong.ESongActivationType.ActionStart);
			}
			m_CheckedForStartActionSongs = true;
		}
		if (!m_StartActionNonToggleActivesAdded && GameState.InternalCurrentActor.Class is CCharacterClass cCharacterClass)
		{
			List<CActiveBonus> source = cCharacterClass.FindActiveBonuses(CActiveBonus.EActiveBonusBehaviourType.StartActionAbility, GameState.InternalCurrentActor);
			List<CAbility> list2 = new List<CAbility>();
			foreach (CStartActionAbilityActiveBonus startActionActiveBonus in source.Where((CActiveBonus x) => x is CStartActionAbilityActiveBonus))
			{
				if (!startActionActiveBonus.IsRestricted(GameState.InternalCurrentActor) && startActionActiveBonus.RequirementsMet() && startActionActiveBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus && (startActionActiveBonus.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || list.Any((CPhaseAbility x) => startActionActiveBonus.Ability.ActiveBonusData.ValidAbilityTypes.Any((CAbility.EAbilityType y) => x.m_Ability.IsValidAbilityForActiveBonusOfType(y)))))
				{
					startActionActiveBonus.AddAbility = CAbility.CopyAbility(cAbilityAddActiveBonus.AddAbility, generateNewID: true, fullCopy: true);
					list2.Add(startActionActiveBonus.AddAbility);
					startActionActiveBonus.TriggerAbility();
				}
			}
			if (list2.Count > 0)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list2, null, performNow: false, stopPlayerSkipping: false, null, stopPlayerUndo: false, null, ignorePerformNow: true);
			}
			m_StartActionNonToggleActivesAdded = true;
		}
		if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ActiveBonus || GameState.CurrentActionInitiator == GameState.EActionInitiator.ScenarioModifier || GameState.CurrentActionInitiator == GameState.EActionInitiator.CompanionSummon)
		{
			CurrentPhaseAbility.m_Ability.SetCanUndo(canUndo: false);
		}
		if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
		{
			CShowAugmentationBar_MessageData message = new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
			{
				m_Action = m_CurrentAction.Action,
				m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		Perform(FullAbilityRestart);
	}

	public bool AbilityHappened()
	{
		if (m_CurrentActionID.HasValue && m_HasAnyAbilityHappened)
		{
			return true;
		}
		return false;
	}

	protected override void OnTileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnTileSelected, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.TileSelected(selectedTile, optionalTileList);
		}
		ScenarioRuleClient.MessageHandler(new CTileSelectionFinished_MessageData());
	}

	protected override void OnTileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnTileDeselected, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.TileDeselected(selectedTile, optionalTileList);
			if (CurrentPhaseAbility.m_Ability.AreaEffect != null)
			{
				ScenarioRuleClient.ClearTargets();
			}
		}
		ScenarioRuleClient.MessageHandler(new CTileSelectionFinished_MessageData());
	}

	protected override void OnApplySingleTarget(CActor actor)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnApplySingleTarget, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
		if (CurrentPhaseAbility != null)
		{
			CAbility cAbility = CurrentPhaseAbility.m_Ability;
			if (cAbility is CAbilityMerged cAbilityMerged)
			{
				cAbility = cAbilityMerged.ActiveAbility;
			}
			if (cAbility.IsWaitingForSingleTargetItem())
			{
				cAbility.ApplySingleTargetItem(actor);
			}
			if (cAbility.IsWaitingForSingleTargetActiveBonus())
			{
				cAbility.ApplySingleTargetActiveBonus(actor);
			}
			CurrentPhaseAbility.m_Ability.Perform();
		}
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnStepComplete, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
		if (passingStep && CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.AbilityPassStep();
		}
		bool fullAbilityRestart;
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			GameState.WaitingForMercenarySpecialMechanicSlotChoice = false;
			Perform(fullAbilityRestart: false);
		}
		else if (CurrentPhaseAbility == null || CurrentPhaseAbility.m_Ability.AbilityComplete(m_DontMoveAbilityState, out fullAbilityRestart))
		{
			m_DontMoveAbilityState = false;
			NextStep();
		}
		else
		{
			m_DontMoveAbilityState = false;
			Perform(fullAbilityRestart);
		}
	}

	public void InjectAbility(CAbility ability, CActor targetingActor, CBaseAbilityCard baseAbilityCard, Action endAction = null)
	{
		m_PhaseAbilities.Insert(0, new CPhaseAbility(ability, targetingActor, baseAbilityCard, null, null, endAction));
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionInjectAbility, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
	}

	public void StackItemAbilities(CItem item)
	{
		if (item.YMLData.Data.Abilities != null && item.YMLData.Data.Abilities.Count > 0)
		{
			for (int num = item.YMLData.Data.Abilities.Count - 1; num >= 0; num--)
			{
				CurrentPhaseAbility.m_Ability.InterruptAbility();
				m_CurrentPhaseAbilities.Add(new CPhaseAbility(CAbility.CopyAbility(item.YMLData.Data.Abilities[num], generateNewID: false), GameState.InternalCurrentActor, item, null, item.ID));
			}
			m_CurrentItemID = item.ID;
			for (int i = 0; i < item.YMLData.Data.Abilities.Count; i++)
			{
				m_CurrentPhaseAbilities[m_CurrentPhaseAbilities.Count - 1 - i].m_Ability.Start(GameState.InternalCurrentActor, GameState.InternalCurrentActor);
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionStackItemAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
			Perform(fullAbilityRestart: true);
		}
		else
		{
			DLLDebug.LogError("Attempting to stack a null ability");
		}
	}

	public void StackInlineSubAbilities(List<CAbility> inlineSubAbilities, CActor controllingActor = null, bool performNow = false, bool stopPlayerSkipping = false, bool? dontMoveAbilityState = null, bool stopPlayerUndo = false, CActiveBonus fromActiveBonus = null, bool ignorePerformNow = false, CActor overrideActor = null)
	{
		CActor cActor = ((overrideActor == null) ? GameState.InternalCurrentActor : overrideActor);
		if (!m_EndAbilitySynced)
		{
			ignorePerformNow = true;
		}
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.InterruptAbility();
		}
		int num = 0;
		for (int num2 = inlineSubAbilities.Count - 1; num2 >= 0; num2--)
		{
			CAbility cAbility = CAbility.CopyAbility(inlineSubAbilities[num2], generateNewID: false, fullCopy: true);
			if (cAbility.SubAbilities != null)
			{
				foreach (CAbility subAbility in cAbility.SubAbilities)
				{
					if (!subAbility.IsInlineSubAbility)
					{
						subAbility.ParentAbility = cAbility;
						if (cAbility.StartAbilityRequirements == null || cAbility.StartAbilityRequirements.MeetsAbilityRequirements(GameState.InternalCurrentActor, subAbility))
						{
							m_CurrentPhaseAbilities.Add(new CPhaseAbility(subAbility, cActor, (m_CurrentPhaseAbilities.Count > 0) ? m_CurrentPhaseAbilities[0].m_BaseCard : m_PreviousPhaseAbilities.Last().m_BaseCard, (CurrentPhaseAbility != null) ? CurrentPhaseAbility.ActionID : m_PreviousPhaseAbilities.Last().ActionID));
							num++;
						}
					}
				}
			}
			CPhaseAbility cPhaseAbility = null;
			if (m_CurrentPhaseAbilities.Count > 0)
			{
				cPhaseAbility = m_CurrentPhaseAbilities[0];
			}
			else if (m_PreviousPhaseAbilities.Count > 0)
			{
				cPhaseAbility = m_PreviousPhaseAbilities[0];
			}
			else if (m_PhaseAbilities.Count > 0)
			{
				cPhaseAbility = m_PhaseAbilities[0];
			}
			m_CurrentPhaseAbilities.Add(new CPhaseAbility(cAbility, cActor, cPhaseAbility.m_BaseCard, cPhaseAbility.ActionID));
			num++;
		}
		if (dontMoveAbilityState.HasValue)
		{
			m_DontMoveAbilityState = dontMoveAbilityState.Value;
		}
		CAbility ability = m_CurrentPhaseAbilities[m_CurrentPhaseAbilities.Count - 1].m_Ability;
		List<CAbility> list = new List<CAbility>();
		for (int i = 0; i < num; i++)
		{
			CAbility ability2 = m_CurrentPhaseAbilities[m_CurrentPhaseAbilities.Count - 1 - i].m_Ability;
			list.Add(ability2);
		}
		if (fromActiveBonus != null)
		{
			m_CurrentTriggeredActiveBonus = fromActiveBonus;
		}
		foreach (CAbility item in list)
		{
			item.Start(cActor, cActor, controllingActor);
			if (stopPlayerSkipping)
			{
				item.SetCanSkip(canSkip: false);
			}
			if (stopPlayerUndo)
			{
				item.SetCanUndo(canUndo: false);
			}
		}
		if (!ignorePerformNow && CurrentPhaseAbility?.m_Ability == ability)
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			if (performNow)
			{
				CurrentPhaseAbility.m_Ability.Perform();
			}
			else
			{
				CWaitForProgressChoreographer_MessageData message = new CWaitForProgressChoreographer_MessageData(cActor)
				{
					WaitTickFrame = 10000,
					WaitActor = cActor,
					ClearEvents = false
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		CClearWaypointsAndTargets_MessageData message2 = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message2);
		if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
		{
			CShowAugmentationBar_MessageData message3 = new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
			{
				m_Action = m_CurrentAction.Action,
				m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
			};
			ScenarioRuleClient.MessageHandler(message3);
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionStackInlineSubAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
	}

	public void StackNextAbilities(List<CAbility> nextAbilities, CActor targetingActor, bool killAfter = false, bool stackToNextCurrent = false, bool copyCurrentActionID = false)
	{
		foreach (CAbility nextAbility in nextAbilities)
		{
			if (stackToNextCurrent && CurrentPhaseAbility != null)
			{
				int index = m_CurrentPhaseAbilities.IndexOf(CurrentPhaseAbility);
				CAbility ability = CAbility.CopyAbility(nextAbility, generateNewID: false, fullCopy: true);
				CBaseCard abilityBaseCard = nextAbility.AbilityBaseCard;
				bool killAfter2 = killAfter;
				CPhaseAbility cPhaseAbility = new CPhaseAbility(ability, targetingActor, abilityBaseCard, null, null, null, killAfter2);
				if (copyCurrentActionID)
				{
					cPhaseAbility.ActionID = CurrentPhaseAbility.ActionID;
				}
				m_CurrentPhaseAbilities.Insert(index, cPhaseAbility);
				cPhaseAbility.m_Ability.Start(targetingActor, targetingActor);
			}
			else
			{
				CAbility ability2 = CAbility.CopyAbility(nextAbility, generateNewID: false, fullCopy: true);
				CBaseCard abilityBaseCard2 = nextAbility.AbilityBaseCard;
				bool killAfter2 = killAfter;
				CPhaseAbility cPhaseAbility2 = new CPhaseAbility(ability2, targetingActor, abilityBaseCard2, null, null, null, killAfter2);
				if (copyCurrentActionID)
				{
					cPhaseAbility2.ActionID = CurrentPhaseAbility.ActionID;
				}
				m_PhaseAbilities.Add(cPhaseAbility2);
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionStackNextAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
	}

	public void UnstackItemAbilities(CItem item)
	{
		if (item.YMLData.Data.Abilities != null && item.YMLData.Data.Abilities.Count > 0)
		{
			foreach (CPhaseAbility item2 in m_CurrentPhaseAbilities.Where((CPhaseAbility w) => w.ItemID == item.ID).ToList())
			{
				COnItemAbilityUnstacked_MessageData message = new COnItemAbilityUnstacked_MessageData(GameState.InternalCurrentActor)
				{
					m_ItemAbility = item2.m_Ability,
					m_Item = item
				};
				ScenarioRuleClient.MessageHandler(message);
				m_CurrentPhaseAbilities.Remove(item2);
			}
			m_CurrentItemID = CurrentPhaseAbility?.ItemID;
			SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionUnstackItemAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations));
			Perform(fullAbilityRestart: false);
		}
		else
		{
			DLLDebug.LogError("Attempting to unstack a null ability");
		}
	}

	public void UnstackAbility(CAbility ability, CActiveBonus fromActiveBonus = null)
	{
		CurrentPhaseAbility.m_Ability.AbilityEnded();
		foreach (CPhaseAbility item in m_CurrentPhaseAbilities.Where((CPhaseAbility w) => w.m_Ability.ID == ability.ID).ToList())
		{
			m_CurrentPhaseAbilities.Remove(item);
		}
		if (fromActiveBonus != null && m_CurrentTriggeredActiveBonus != null)
		{
			m_CurrentTriggeredActiveBonus = null;
		}
		if (CurrentPhaseAbility != null)
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			CurrentPhaseAbility.m_Ability.Restart();
			CurrentPhaseAbility.m_Ability.Perform();
		}
		else
		{
			EndAbilitySynchronise();
		}
	}

	public void FinalizeOverrideAbilityType(CAbility overridingAbility, CAbilityOverride abilityOverride = null)
	{
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		CAbility cAbility = CAbility.CopyAbility(overridingAbility, generateNewID: false, fullCopy: true);
		CPhaseAbility cPhaseAbility = m_CurrentPhaseAbilities.Find((CPhaseAbility x) => x.m_Ability.ID == overridingAbility.ID);
		int index = m_CurrentPhaseAbilities.IndexOf(cPhaseAbility);
		m_CurrentPhaseAbilities.Insert(index, new CPhaseAbility(cAbility, GameState.InternalCurrentActor, cPhaseAbility.m_BaseCard, cPhaseAbility.ActionID));
		m_CurrentPhaseAbilities.Remove(cPhaseAbility);
		if (m_FirstPhaseAbility == cPhaseAbility)
		{
			m_FirstPhaseAbility = m_CurrentPhaseAbilities[index];
		}
		if (abilityOverride != null)
		{
			CurrentPhaseAbility.m_Ability.CurrentOverrides.Add(abilityOverride);
		}
		cAbility.Start(CurrentPhaseAbility.TargetingActor, CurrentPhaseAbility.TargetingActor);
		Perform(fullAbilityRestart: false);
		if (GameState.InternalCurrentActor is CPlayerActor cPlayerActor)
		{
			cPlayerActor.Inventory.HighlightUsableItems(CurrentPhaseAbility.m_Ability, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
		}
		CFinalizedOverrideAbilityType cFinalizedOverrideAbilityType = new CFinalizedOverrideAbilityType(CurrentPhaseAbility.TargetingActor);
		cFinalizedOverrideAbilityType.m_Ability = CurrentPhaseAbility.m_Ability;
		ScenarioRuleClient.MessageHandler(cFinalizedOverrideAbilityType);
	}

	public void SwapAbility(CAbility abilityToRemove, CAbility abilityToAdd, bool canUndo = true)
	{
		CPhaseAbility cPhaseAbility = m_CurrentPhaseAbilities.Find((CPhaseAbility x) => x.m_Ability == abilityToRemove);
		int num = m_CurrentPhaseAbilities.IndexOf(cPhaseAbility);
		m_CurrentPhaseAbilities.Insert(num, new CPhaseAbility(abilityToAdd, GameState.InternalCurrentActor, cPhaseAbility.m_BaseCard, cPhaseAbility.ActionID));
		m_CurrentPhaseAbilities.Remove(cPhaseAbility);
		if (m_FirstPhaseAbility == cPhaseAbility)
		{
			m_FirstPhaseAbility = m_CurrentPhaseAbilities[num];
		}
		CurrentPhaseAbility.m_Ability.SetCanUndo(canUndo);
		if (num == m_CurrentPhaseAbilities.Count - 1)
		{
			CurrentPhaseAbility.m_Ability.Start(GameState.InternalCurrentActor, GameState.InternalCurrentActor);
			Perform(fullAbilityRestart: true);
		}
	}

	public bool HasConsumedActionAugmentation(CActionAugmentation augment)
	{
		return m_ActionAugmentationsConsumed.Exists((CActionAugmentation it) => it.ActionID == augment.ActionID);
	}

	public static void UpdateDamageInflictedThisAction(int damageInflictedThisAction)
	{
		s_DamageInflictedThisAction += damageInflictedThisAction;
	}

	public static void UpdateHexesMovedThisAction(int hexesMovedThisAction)
	{
		s_HexesMovedThisAction += hexesMovedThisAction;
	}

	public static void UpdateObstaclesDestroyedThisAction(int obstaclesDestroyedThisAction)
	{
		s_ObstaclesDestroyedThisAction += obstaclesDestroyedThisAction;
	}

	public static void UpdateTargetsDamagedInPrevAttackThisAction(CAbilityAttack abilityAttack)
	{
		if (s_TargetsDamagedInPrevAttackThisActionAbilityID != abilityAttack.ID)
		{
			s_TargetsDamagedInPrevAttackThisActionAbilityID = abilityAttack.ID;
			s_TargetsDamagedInPrevAttackThisAction = 0;
		}
		s_TargetsDamagedInPrevAttackThisAction++;
	}

	public static void UpdateTargetsActuallyDamagedInPrevAttackThisAction(CAbilityAttack abilityAttack)
	{
		if (s_TargetsActuallyDamagedInPrevAttackThisActionAbilityID != abilityAttack.ID)
		{
			s_TargetsActuallyDamagedInPrevAttackThisActionAbilityID = abilityAttack.ID;
			s_TargetsActuallyDamagedInPrevAttackThisAction = 0;
		}
		s_TargetsActuallyDamagedInPrevAttackThisAction++;
	}

	public static void UpdateTargetsDamagedInPrevDamageAbilityThisAction(CAbilityDamage abilityDamage)
	{
		if (s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID != abilityDamage.ID)
		{
			s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID = abilityDamage.ID;
			s_TargetsDamagedInPrevDamageAbilityThisAction = 0;
		}
		s_TargetsDamagedInPrevDamageAbilityThisAction++;
	}

	public static void UpdateActorsKilledThisAction(CActor actorKilled)
	{
		s_ActorsKilledThisAction.Add(actorKilled);
	}

	public static void Reset()
	{
		s_DamageInflictedThisAction = 0;
		s_HexesMovedThisAction = 0;
		s_ObstaclesDestroyedThisAction = 0;
		s_TargetsDamagedInPrevAttackThisAction = 0;
		s_TargetsActuallyDamagedInPrevAttackThisAction = 0;
		s_TargetsDamagedInPrevAttackThisActionAbilityID = Guid.Empty;
		s_TargetsDamagedInPrevDamageAbilityThisAction = 0;
		s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID = Guid.Empty;
		s_ActorsKilledThisAction = new List<CActor>();
	}
}
