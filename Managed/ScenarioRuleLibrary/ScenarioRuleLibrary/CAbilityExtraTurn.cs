using System;
using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityExtraTurn : CAbilityTargeting
{
	public enum EExtraTurnType
	{
		None,
		TopAction,
		BottomAction,
		BothActions,
		BothActionsLater,
		MonsterAction,
		CurrentAction
	}

	public class PendingExtraTurnData
	{
		public int LeadingInitiative;

		public List<CAbilityCard> PendingExtraTurnCards;

		public PendingExtraTurnData(int leadingInitiative, List<CAbilityCard> pendingExtraTurnCards)
		{
			LeadingInitiative = leadingInitiative;
			PendingExtraTurnCards = pendingExtraTurnCards;
		}
	}

	public static EExtraTurnType[] ExtraTurnTypes = (EExtraTurnType[])Enum.GetValues(typeof(EExtraTurnType));

	public EExtraTurnType ExtraTurnType { get; set; }

	public CAbilityExtraTurn(EExtraTurnType extraTurnType)
		: base(EAbilityType.ExtraTurn)
	{
		ExtraTurnType = extraTurnType;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		for (int num = m_ValidActorsInRange.Count - 1; num >= 0; num--)
		{
			CActor cActor = m_ValidActorsInRange[num];
			if (cActor is CPlayerActor cPlayerActor)
			{
				if (ExtraTurnType == EExtraTurnType.BothActions || ExtraTurnType == EExtraTurnType.BothActionsLater)
				{
					if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 2)
					{
						m_ValidActorsInRange.Remove(cActor);
					}
				}
				else if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 1)
				{
					m_ValidActorsInRange.Remove(cActor);
				}
			}
		}
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.TargetingActor.Inventory.HandleUsedItems();
			if (actor is CPlayerActor cPlayerActor)
			{
				if (ExtraTurnType != EExtraTurnType.MonsterAction)
				{
					cPlayerActor.CharacterClass.ResetInitiativeBonus();
					base.AbilityHasHappened = true;
					cPlayerActor.SelectingCardsForExtraTurnOfType = ExtraTurnType;
					if (ExtraTurnType == EExtraTurnType.CurrentAction)
					{
						_ = GameState.CurrentActionSelectionSequence;
						_ = GameState.HasPlayedTopAction;
						_ = GameState.HasPlayedBottomAction;
						GameState.EActionSelectionFlag eActionSelectionFlag = GameState.EActionSelectionFlag.None;
						if (GameState.RoundAbilityCardselected.LastSelectedAction.ID == GameState.RoundAbilityCardselected.TopAction.ID || GameState.RoundAbilityCardselected.LastSelectedAction.ID == GameState.RoundAbilityCardselected.DefaultAttackAction.ID)
						{
							ExtraTurnType = EExtraTurnType.TopAction;
							eActionSelectionFlag = GameState.EActionSelectionFlag.TopActionPlayed;
						}
						else
						{
							ExtraTurnType = EExtraTurnType.BottomAction;
							eActionSelectionFlag = GameState.EActionSelectionFlag.BottomActionPlayed;
						}
						GameState.UpdateCacheActionSelectionSequence(eActionSelectionFlag);
					}
					if (ExtraTurnType == EExtraTurnType.BottomAction)
					{
						cPlayerActor.SkipTopCardAction = true;
						cPlayerActor.TakingExtraTurnOfTypeStack.Push(ExtraTurnType);
					}
					else if (ExtraTurnType == EExtraTurnType.TopAction)
					{
						cPlayerActor.SkipBottomCardAction = true;
						cPlayerActor.TakingExtraTurnOfTypeStack.Push(ExtraTurnType);
					}
					CAbilityCard extraTurnInitiatorCard = null;
					if (base.AbilityBaseCard is CAbilityCard cAbilityCard)
					{
						extraTurnInitiatorCard = cAbilityCard;
					}
					GameState.CacheExtraTurnInitiatorCard(extraTurnInitiatorCard);
					CSelectExtraTurnCards_MessageData cSelectExtraTurnCards_MessageData = new CSelectExtraTurnCards_MessageData(base.AnimOverload, base.TargetingActor);
					cSelectExtraTurnCards_MessageData.m_ActorTakingExtraTurn = actor;
					cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility = this;
					ScenarioRuleClient.MessageHandler(cSelectExtraTurnCards_MessageData);
				}
				else
				{
					DLLDebug.LogError("Unsupported ExtraTurnType for PlayerActor");
				}
			}
			else if (ExtraTurnType == EExtraTurnType.MonsterAction)
			{
				actor.PendingExtraTurnOfTypeStack.Push(ExtraTurnType);
			}
			else
			{
				DLLDebug.LogError("Unsupported ExtraTurnType for MonsterActor");
			}
		}
		return true;
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		base.TileDeselected(selectedTile, optionalTileList);
		for (int num = m_ValidActorsInRange.Count - 1; num >= 0; num--)
		{
			CActor cActor = m_ValidActorsInRange[num];
			if (cActor is CPlayerActor cPlayerActor)
			{
				if (ExtraTurnType == EExtraTurnType.BothActions || ExtraTurnType == EExtraTurnType.BothActionsLater)
				{
					if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 2)
					{
						m_ValidActorsInRange.Remove(cActor);
					}
				}
				else if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 1)
				{
					m_ValidActorsInRange.Remove(cActor);
				}
			}
		}
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityExtraTurn()
	{
	}

	public CAbilityExtraTurn(CAbilityExtraTurn state, ReferenceDictionary references)
		: base(state, references)
	{
		ExtraTurnType = state.ExtraTurnType;
	}
}
