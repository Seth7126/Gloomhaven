using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class PhaseManager
{
	private static CPhase s_CurrentPhase;

	private static CPhase s_PreviousPhase;

	public static CPhase.PhaseType PhaseType
	{
		get
		{
			if (s_CurrentPhase != null)
			{
				return s_CurrentPhase.Type;
			}
			return CPhase.PhaseType.None;
		}
	}

	public static CPhase Phase => s_CurrentPhase;

	public static CPhase CurrentPhase => s_CurrentPhase;

	public static CPhase PreviousPhase => s_PreviousPhase;

	public static void Stop()
	{
		s_CurrentPhase = null;
	}

	public static void TileSelected(CTile tile, List<CTile> optionalTileList)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.TileSelected(tile, optionalTileList);
		}
	}

	public static void TileDeselected(CTile tile, List<CTile> optionalTileList)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.TileDeselected(tile, optionalTileList);
		}
	}

	public static void ApplySingleTarget(CActor actor)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.ApplySingleTarget(actor);
		}
	}

	public static void StepComplete(bool passingStep = false)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.StepComplete(passingStep);
		}
	}

	public static void NextStep(bool passing = false)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.NextStep(passing);
		}
	}

	public static void SetNextPhase(CPhase.PhaseType type)
	{
		SimpleLog.AddToSimpleLog("Setting Next Phase: " + type);
		bool flag = false;
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.EndPhase();
		}
		switch (type)
		{
		case CPhase.PhaseType.StartScenarioEffects:
			s_CurrentPhase = new CPhaseStartScenarioEffects();
			foreach (CScenarioModifier item2 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario))
			{
				item2.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			break;
		case CPhase.PhaseType.StartRoundEffects:
			s_CurrentPhase = new CPhaseStartRoundEffects();
			foreach (CScenarioModifier item3 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound))
			{
				item3.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				GameState.PendingActiveBonuses.AddRange(CActiveBonus.FindApplicableActiveBonuses(allAliveActor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.StartRoundAbility));
			}
			break;
		case CPhase.PhaseType.PlayerExhausted:
			s_CurrentPhase = new CPhasePlayerExhausted();
			break;
		case CPhase.PhaseType.Autosave:
			s_CurrentPhase = new CPhaseAutosave();
			break;
		case CPhase.PhaseType.SelectAbilityCardsOrLongRest:
			s_CurrentPhase = new CPhaseSelectAbilityCardsOrLongRest();
			break;
		case CPhase.PhaseType.MonsterClassesSelectAbilityCards:
			s_CurrentPhase = new CPhaseSelectMonsterClassAbilityCards();
			if (ScenarioManager.CurrentScenarioState.RoundNumber > 1)
			{
				break;
			}
			GameState.PendingCompanionSummonActors.Clear();
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (playerActor.CharacterClass.CompanionSummonData != null)
				{
					GameState.PendingCompanionSummonActors.Add(playerActor);
				}
			}
			if (GameState.PendingCompanionSummonActors.Count > 0)
			{
				StartActiveBonusOrScenarioModifierAbility();
				return;
			}
			break;
		case CPhase.PhaseType.CheckForInitiativeAdjustments:
			s_CurrentPhase = new CPhaseCheckForInitiativeAdjustments();
			break;
		case CPhase.PhaseType.CheckForForgoActionActiveBonuses:
			s_CurrentPhase = new CPhaseCheckForForgoActionActiveBonuses();
			break;
		case CPhase.PhaseType.StartTurn:
		{
			s_CurrentPhase = new CPhaseStartTurn();
			GameState.PendingOnLongRestBonuses.Clear();
			GameState.PendingActiveBonuses.Clear();
			GameState.PendingScenarioModifierAbilities.Clear();
			foreach (CActiveBonus item4 in CActiveBonus.FindAllActiveBonuses())
			{
				item4.ResetRestriction(CActiveBonus.EActiveBonusRestrictionType.OncePerTurn);
			}
			foreach (CScenarioModifier item5 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartTurn))
			{
				if (!GameState.InternalCurrentActor.IsTakingExtraTurn)
				{
					item5.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, GameState.InternalCurrentActor, ScenarioManager.CurrentScenarioState.Players.Count);
				}
			}
			CClass cClass = GameState.InternalCurrentActor.Class;
			CCharacterClass characterClass = cClass as CCharacterClass;
			if (characterClass != null)
			{
				if (GameState.InternalCurrentActor.TakingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.TopAction && GameState.InternalCurrentActor.TakingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.BottomAction)
				{
					List<CActiveBonus> list3 = (from x in characterClass.FindActiveBonuses(GameState.InternalCurrentActor)
						where x.IsSong
						select x).ToList();
					if (list3.Count > 0 && list3.Any((CActiveBonus a) => a.Caster.Class.ID == characterClass.ID))
					{
						GameState.InternalCurrentActor.GainXP(1);
					}
					List<CActiveBonus> list4 = (from x in CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility)
						where !x.IsSong
						select x).ToList();
					list4.AddRange(characterClass.FindCasterActiveBonuses(CActiveBonus.EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns, GameState.InternalCurrentActor));
					list4.AddRange(from x in CharacterClassManager.FindAllSongActiveBonuses(GameState.InternalCurrentActor)
						where x.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility
						select x);
					if (list4.Count > 0)
					{
						GameState.PendingActiveBonuses = list4;
					}
				}
			}
			else
			{
				List<CActiveBonus> list5 = (from x in CharacterClassManager.FindAllSongActiveBonuses(GameState.InternalCurrentActor)
					where x.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility && x.Ability.ActiveBonusData.Filter.IsValidTarget(GameState.InternalCurrentActor, x.Actor, isTargetedAbility: false, useTargetOriginalType: false, false)
					select x).ToList();
				if (GameState.InternalCurrentActor.Class is CMonsterClass)
				{
					list5.AddRange(from x in CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility)
						where !x.IsSong
						select x);
				}
				if (list5.Count > 0)
				{
					GameState.PendingActiveBonuses = list5;
				}
			}
			if (GameState.PendingScenarioModifierAbilities.Count > 0 || GameState.PendingActiveBonuses.Count > 0)
			{
				CStartTurn_MessageData message = new CStartTurn_MessageData(GameState.InternalCurrentActor, skipNextPhase: true);
				ScenarioRuleClient.MessageHandler(message);
				StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
				return;
			}
			break;
		}
		case CPhase.PhaseType.ActionSelection:
			if (!GameState.ActorHealthCheck(GameState.InternalCurrentActor, GameState.InternalCurrentActor) && ScenarioManager.Scenario.PlayerActors.Count > 0)
			{
				flag = true;
				GameState.EndTurnCheckNextActorAndMoveToNextPhase();
			}
			else
			{
				s_CurrentPhase = new CPhaseActionSelection();
			}
			break;
		case CPhase.PhaseType.Action:
			s_CurrentPhase = new CPhaseAction();
			break;
		case CPhase.PhaseType.EndTurnLoot:
		{
			s_CurrentPhase = new CPhaseEndTurnLoot();
			bool flag2 = false;
			if (ScenarioManager.Scenario.HasActor(GameState.InternalCurrentActor) && GameState.InternalCurrentActor.Type == CActor.EType.Player)
			{
				CTile cTile = ScenarioManager.Tiles[GameState.InternalCurrentActor.ArrayIndex.X, GameState.InternalCurrentActor.ArrayIndex.Y];
				List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor, CAbility.EAbilityType.Loot);
				List<CLootActiveBonus_LimitLoot> lootLimitBehaviours = new List<CLootActiveBonus_LimitLoot>();
				foreach (CActiveBonus item6 in list)
				{
					if (item6.BespokeBehaviour != null && item6.BespokeBehaviour is CLootActiveBonus_LimitLoot item)
					{
						lootLimitBehaviours.Add(item);
					}
				}
				List<CObjectProp> list2 = cTile.m_Props.FindAll((CObjectProp x) => x.IsLootable && !x.IgnoreEndOfTurnLooting && x.CanActorLoot(GameState.InternalCurrentActor) && lootLimitBehaviours.All((CLootActiveBonus_LimitLoot y) => y.CanLootObject(GameState.InternalCurrentActor, x.ObjectType)));
				if (list2.Count > 0 && GameState.InternalCurrentActor.LootTile(cTile, asPartOfAbility: false) && ScenarioManager.Scenario.HasActor(GameState.InternalCurrentActor))
				{
					flag2 = true;
					CLooting_MessageData cLooting_MessageData = new CLooting_MessageData("", GameState.InternalCurrentActor);
					cLooting_MessageData.m_ActorLooting = GameState.InternalCurrentActor;
					cLooting_MessageData.m_PropsLooted = list2;
					ScenarioRuleClient.MessageHandler(cLooting_MessageData);
				}
			}
			if (!flag2)
			{
				flag = true;
				GameState.NextPhase();
			}
			break;
		}
		case CPhase.PhaseType.EndTurn:
		{
			CPhaseEndTurn cPhaseEndTurn = (CPhaseEndTurn)(s_CurrentPhase = new CPhaseEndTurn());
			GameState.CheckAllPressurePlates();
			GameState.PendingActiveBonuses.Clear();
			GameState.PendingScenarioModifierAbilities.Clear();
			if (!GameState.InternalCurrentActor.IsTakingExtraTurn || GameState.InternalCurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
			{
				foreach (CScenarioModifier item7 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndTurn))
				{
					item7.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, GameState.InternalCurrentActor, ScenarioManager.CurrentScenarioState.Players.Count);
				}
				GameState.PendingActiveBonuses.AddRange(CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.EndTurnAbility));
				if (GameState.PendingScenarioModifierAbilities.Count > 0 || GameState.PendingActiveBonuses.Count > 0)
				{
					StartActiveBonusOrScenarioModifierAbility();
					return;
				}
			}
			if (s_CurrentPhase != cPhaseEndTurn)
			{
				flag = true;
			}
			break;
		}
		case CPhase.PhaseType.EndRound:
			s_CurrentPhase = new CPhaseEndRound();
			GameState.PendingActiveBonuses.Clear();
			GameState.PendingScenarioModifierAbilities.Clear();
			foreach (CScenarioModifier item8 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound))
			{
				item8.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			foreach (CActor allAliveActor2 in ScenarioManager.Scenario.AllAliveActors)
			{
				GameState.PendingActiveBonuses.AddRange(CActiveBonus.FindApplicableActiveBonuses(allAliveActor2, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.EndRoundAbility));
			}
			if (GameState.PendingScenarioModifierAbilities.Count > 0 || GameState.PendingActiveBonuses.Count > 0)
			{
				StartActiveBonusOrScenarioModifierAbility();
				return;
			}
			break;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventPhase(type, ESESubTypePhase.PhaseStart, "", doNotSerialize: true));
		DLLDebug.LogInfo("PHASE: " + type);
		if (!flag)
		{
			s_CurrentPhase.NextStep();
		}
	}

	public static void StartItemAbilities(CItem item)
	{
		GameState.CurrentActionInitiator = GameState.EActionInitiator.ItemCard;
		item.ActionHasHappened = false;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), item.YMLData.Data.Abilities.Select((CAbility s) => CAbility.CopyAbility(s, generateNewID: false)).ToList(), 0), item);
		s_PreviousPhase = s_CurrentPhase;
		SetNextPhase(CPhase.PhaseType.Action);
	}

	public static void EndItemAbilities()
	{
		if (s_PreviousPhase != null)
		{
			s_CurrentPhase = s_PreviousPhase;
			s_PreviousPhase = null;
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			if (s_CurrentPhase is CPhaseActionSelection)
			{
				GameState.InternalCurrentActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
			}
			s_CurrentPhase.NextStep();
		}
	}

	public static void StartAbilities(List<CAbility> abilities, CBaseCard baseCard, bool fullCopy = false, CActiveBonus fromActiveBonus = null)
	{
		GameState.CurrentActionInitiator = GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), abilities.Select((CAbility s) => CAbility.CopyAbility(s, generateNewID: false, fullCopy)).ToList(), 0), baseCard);
		s_PreviousPhase = s_CurrentPhase;
		SetNextPhase(CPhase.PhaseType.Action);
		if (s_CurrentPhase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.CurrentTriggeredActiveBonus = fromActiveBonus;
		}
	}

	public static void EndAbilities()
	{
		if (s_PreviousPhase != null)
		{
			s_CurrentPhase = s_PreviousPhase;
			s_PreviousPhase = null;
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			s_CurrentPhase.NextStep();
		}
	}

	public static void StartOverrideTurnAbilities(List<CAbility> abilities, CBaseCard baseCard)
	{
		GameState.CurrentActionInitiator = GameState.EActionInitiator.OverrideTurnAction;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), abilities.Select((CAbility s) => CAbility.CopyAbility(s, generateNewID: false)).ToList(), 0), baseCard);
		s_PreviousPhase = s_CurrentPhase;
		SetNextPhase(CPhase.PhaseType.Action);
	}

	public static void EndOverrideTurnAbilities()
	{
		if (s_PreviousPhase != null)
		{
			s_CurrentPhase = s_PreviousPhase;
			s_PreviousPhase = null;
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			GameState.EndTurnCheckNextActorAndMoveToNextPhase();
		}
	}

	public static void StartActiveBonusOrScenarioModifierAbility(bool skipNextPhase = false)
	{
		GameState.SkipNextPhase = skipNextPhase;
		s_PreviousPhase = s_CurrentPhase;
		if (!NextActiveBonusOrScenarioModifier())
		{
			EndActiveBonusOrScenarioModifierAbility();
		}
	}

	public static bool NextActiveBonusOrScenarioModifier()
	{
		if (GameState.PendingScenarioModifierAbilities != null && GameState.PendingScenarioModifierAbilities.Count > 0)
		{
			GameState.CurrentActionInitiator = GameState.EActionInitiator.ScenarioModifier;
			CAbility item = GameState.PendingScenarioModifierAbilities[0].Item1;
			GameState.OverrideCurrentActor(GameState.PendingScenarioModifierAbilities[0].Item2);
			GameState.PendingScenarioModifierAbilities.RemoveAt(0);
			if (item != null)
			{
				GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), new List<CAbility> { CAbility.CopyAbility(item, generateNewID: false, fullCopy: true) }, 0), null);
				GameState.CurrentAction.Action.Abilities[0].IsScenarioModifierAbility = true;
				SetNextPhase(CPhase.PhaseType.Action);
				return true;
			}
			return false;
		}
		if ((GameState.PendingCompanionSummonActors != null) & (GameState.PendingCompanionSummonActors.Count > 0))
		{
			GameState.CurrentActionInitiator = GameState.EActionInitiator.CompanionSummon;
			CPlayerActor cPlayerActor = GameState.PendingCompanionSummonActors[0];
			GameState.PendingCompanionSummonActors.RemoveAt(0);
			GameState.SetCurrentActorForCompanionSummon(cPlayerActor);
			CAbility cAbility = CAbilitySummon.CreateCompanionSummon(cPlayerActor.CharacterClass.CompanionSummonData.ID);
			List<CScenarioModifier> list = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FindAll((CScenarioModifier x) => x.ScenarioModifierType == EScenarioModifierType.OverrideCompanionSummonTiles && x.IsActiveInRound(ScenarioManager.CurrentScenarioState.RoundNumber));
			if (list.Count > 0)
			{
				CScenarioModifierOverrideCompanionSummonTiles obj = (CScenarioModifierOverrideCompanionSummonTiles)list[0];
				cAbility.IsSubAbility = true;
				cAbility.IsInlineSubAbility = true;
				cAbility.InlineSubAbilityTiles = new List<CTile>();
				foreach (TileIndex summonTileIndex in obj.SummonTileIndexes)
				{
					cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[summonTileIndex.X, summonTileIndex.Y]);
				}
			}
			GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), new List<CAbility> { cAbility }, 0), null);
			SetNextPhase(CPhase.PhaseType.Action);
			return true;
		}
		if (GameState.PendingActiveBonuses != null && GameState.PendingActiveBonuses.Count > 0)
		{
			GameState.CurrentActionInitiator = GameState.EActionInitiator.ActiveBonus;
			CActiveBonus cActiveBonus = GameState.PendingActiveBonuses[0];
			GameState.PendingActiveBonuses.RemoveAt(0);
			if (cActiveBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus)
			{
				bool flag = false;
				CAbility cAbility2 = CAbility.CopyAbility(cAbilityAddActiveBonus.AddAbility, generateNewID: false);
				if (cActiveBonus.IsAura && cActiveBonus.Caster != null && cAbility2 is CAbilityTargeting cAbilityTargeting)
				{
					cAbilityTargeting.SetOriginatingFromAura(cActiveBonus.Caster, cAbilityAddActiveBonus.ActiveBonusData.ActiveBonusAnimOverload, cAbilityAddActiveBonus.ActiveBonusData.AuraAnimTriggerAbilityType);
				}
				if (cActiveBonus is CEndTurnAbilityActiveBonus cEndTurnAbilityActiveBonus && cEndTurnAbilityActiveBonus.RequirementsMet())
				{
					flag = true;
					cEndTurnAbilityActiveBonus.AddAbility = cAbility2;
					GameState.PendingStartEndTurnAbilityBonusTriggers.Add(cEndTurnAbilityActiveBonus);
				}
				else if (cActiveBonus is CStartTurnAbilityActiveBonus cStartTurnAbilityActiveBonus && cStartTurnAbilityActiveBonus.CanTriggerAbility())
				{
					flag = true;
					cStartTurnAbilityActiveBonus.AddAbility = cAbility2;
					GameState.PendingStartEndTurnAbilityBonusTriggers.Add(cStartTurnAbilityActiveBonus);
					if (cActiveBonus.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns)
					{
						GameState.OverrideCurrentActorForOneAction(cActiveBonus.Actor);
						CUpdateCurrentActor_MessageData message = new CUpdateCurrentActor_MessageData(GameState.InternalCurrentActor);
						ScenarioRuleClient.MessageHandler(message);
					}
					CStartTurnActiveBonusTriggered_MessageData message2 = new CStartTurnActiveBonusTriggered_MessageData(GameState.InternalCurrentActor);
					ScenarioRuleClient.MessageHandler(message2);
				}
				else if (cActiveBonus is CStartRoundAbilityActiveBonus cStartRoundAbilityActiveBonus && cStartRoundAbilityActiveBonus.RequirementsMet(cActiveBonus.Actor))
				{
					flag = true;
					cStartRoundAbilityActiveBonus.AddAbility = cAbility2;
					GameState.PendingStartEndTurnAbilityBonusTriggers.Add(cStartRoundAbilityActiveBonus);
					GameState.OverrideCurrentActorForOneAction(cActiveBonus.Actor);
					CUpdateCurrentActor_MessageData message3 = new CUpdateCurrentActor_MessageData(GameState.InternalCurrentActor);
					ScenarioRuleClient.MessageHandler(message3);
				}
				else if (cActiveBonus is CEndRoundAbilityActiveBonus cEndRoundAbilityActiveBonus && cEndRoundAbilityActiveBonus.RequirementsMet(cActiveBonus.Actor))
				{
					flag = true;
					cEndRoundAbilityActiveBonus.AddAbility = cAbility2;
					GameState.PendingStartEndTurnAbilityBonusTriggers.Add(cEndRoundAbilityActiveBonus);
					GameState.OverrideCurrentActorForOneAction(cActiveBonus.Actor);
					CUpdateCurrentActor_MessageData message4 = new CUpdateCurrentActor_MessageData(GameState.InternalCurrentActor);
					ScenarioRuleClient.MessageHandler(message4);
				}
				if (flag)
				{
					GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), new List<CAbility> { cAbility2 }, 0), cActiveBonus.BaseCard);
					SetNextPhase(CPhase.PhaseType.Action);
					return true;
				}
				return NextActiveBonusOrScenarioModifier();
			}
			if (cActiveBonus.Ability is CAbilityPlaySong cAbilityPlaySong)
			{
				if (cActiveBonus is CEndTurnAbilityActiveBonus item2)
				{
					GameState.PendingStartEndTurnAbilityBonusTriggers.Add(item2);
				}
				else if (cActiveBonus is CStartTurnAbilityActiveBonus item3)
				{
					GameState.PendingStartEndTurnAbilityBonusTriggers.Add(item3);
					CStartTurnActiveBonusTriggered_MessageData message5 = new CStartTurnActiveBonusTriggered_MessageData(GameState.InternalCurrentActor);
					ScenarioRuleClient.MessageHandler(message5);
				}
				List<CAbility> list2 = new List<CAbility>();
				foreach (CSong.SongEffect songEffect in cAbilityPlaySong.Song.SongEffects)
				{
					if (songEffect.SongEffectType != CSong.ESongEffectType.Ability)
					{
						continue;
					}
					foreach (CAbility ability in songEffect.Abilities)
					{
						list2.Add(CAbility.CopyAbility(ability, generateNewID: false));
					}
				}
				GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), list2, 0), cActiveBonus.BaseCard);
				SetNextPhase(CPhase.PhaseType.Action);
				return true;
			}
			return false;
		}
		return false;
	}

	public static void EndActiveBonusOrScenarioModifierAbility()
	{
		if (s_PreviousPhase != null && !NextActiveBonusOrScenarioModifier())
		{
			s_CurrentPhase = s_PreviousPhase;
			s_PreviousPhase = null;
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			if (GameState.InternalCurrentActor != null)
			{
				ElementInfusionBoardManager.EndTurn(GameState.InternalCurrentActor.Type);
			}
			else
			{
				ElementInfusionBoardManager.EndTurn();
			}
			if (!GameState.SkipNextPhase)
			{
				s_CurrentPhase.NextStep();
			}
			else
			{
				GameState.NextPhase();
			}
		}
	}

	public static void Reset()
	{
		s_CurrentPhase = null;
		s_PreviousPhase = null;
	}
}
