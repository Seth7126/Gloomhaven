using System;
using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRecoverLostCards : CAbilityTargeting
{
	public List<EAbilityType> RecoverCardsWithAbilityOfTypeFilter;

	public int StartLostCards;

	public CPlayerActor playerActor;

	public CAbilityRecoverLostCards(List<EAbilityType> recoverCardsWithAbilityOfTypeFilter)
		: base(EAbilityType.RecoverLostCards)
	{
		RecoverCardsWithAbilityOfTypeFilter = recoverCardsWithAbilityOfTypeFilter;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (base.Strength != int.MaxValue)
		{
			m_OneTargetAtATime = true;
		}
		base.Start(targetActor, filterActor, controllingActor);
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		playerActor = null;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			if (actor.Type == CActor.EType.Player && HasCardsToRecover(((CPlayerActor)actor).CharacterClass.LostAbilityCards))
			{
				playerActor = (CPlayerActor)actor;
				StartLostCards = playerActor.CharacterClass.LostAbilityCards.Count;
				if (m_Strength == int.MaxValue)
				{
					foreach (CAbilityCard card in playerActor.CharacterClass.LostAbilityCards.ToList())
					{
						bool flag = false;
						if (RecoverCardsWithAbilityOfTypeFilter.Count > 0)
						{
							if (RecoverCardsWithAbilityOfTypeFilter.Any((EAbilityType a) => card.HasAbilityOfType(a)))
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
						if (flag)
						{
							playerActor.CharacterClass.MoveAbilityCard(card, playerActor.CharacterClass.LostAbilityCards, playerActor.CharacterClass.HandAbilityCards, "LostAbilityCards", "HandAbilityCards");
						}
					}
					CRecoverLostCards_MessageData cRecoverLostCards_MessageData = new CRecoverLostCards_MessageData(base.AnimOverload, base.TargetingActor);
					cRecoverLostCards_MessageData.m_ActorRecoveringLostCards = actor;
					cRecoverLostCards_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cRecoverLostCards_MessageData);
					return true;
				}
				CSelectRecoverCards_MessageData cSelectRecoverCards_MessageData = new CSelectRecoverCards_MessageData(base.AnimOverload, base.TargetingActor);
				cSelectRecoverCards_MessageData.m_ActorRecoveringLostCards = actor;
				cSelectRecoverCards_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cSelectRecoverCards_MessageData);
				return true;
			}
		}
		return false;
	}

	private bool HasCardsToRecover(List<CAbilityCard> cards)
	{
		if (RecoverCardsWithAbilityOfTypeFilter == null || RecoverCardsWithAbilityOfTypeFilter.Count == 0)
		{
			return cards.Count > 0;
		}
		return cards.Exists((CAbilityCard card) => RecoverCardsWithAbilityOfTypeFilter.Any(card.HasAbilityOfType));
	}

	public override void AbilityEnded()
	{
		if (playerActor != null)
		{
			StartLostCards = Math.Max(0, StartLostCards - playerActor.CharacterClass.LostAbilityCards.Count);
		}
		LogEvent(ESESubTypeAbility.AbilityEnded);
		StartLostCards = -1;
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		EAbilityType abilityType = base.AbilityType;
		if (base.ActiveBonusData.OverrideAsSong)
		{
			abilityType = EAbilityType.PlaySong;
		}
		bool isSummon = false;
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		string actedOnClass = "";
		CActor.EType actedOnType = CActor.EType.Unknown;
		bool actedOnIsSummon = false;
		CActor actor = null;
		if (base.ActorsTargeted.Count > 0)
		{
			actor = base.ActorsTargeted[base.ActorsTargeted.Count - 1];
			actedOnClass = actor.Class.ID;
			actedOnType = actor.Type;
			if (actor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
				if (cEnemyActor2 != null)
				{
					actedOnIsSummon = cEnemyActor2.IsSummon;
				}
			}
		}
		if (StartLostCards >= 0)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTargeting(abilityType, subTypeAbility, m_State, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, actedOnClass, actedOnType, actedOnIsSummon, actor?.Tokens.CheckPositiveTokens, actor?.Tokens.CheckNegativeTokens, "", StartLostCards));
		}
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityRecoverLostCards()
	{
	}

	public CAbilityRecoverLostCards(CAbilityRecoverLostCards state, ReferenceDictionary references)
		: base(state, references)
	{
		RecoverCardsWithAbilityOfTypeFilter = references.Get(state.RecoverCardsWithAbilityOfTypeFilter);
		if (RecoverCardsWithAbilityOfTypeFilter == null && state.RecoverCardsWithAbilityOfTypeFilter != null)
		{
			RecoverCardsWithAbilityOfTypeFilter = new List<EAbilityType>();
			for (int i = 0; i < state.RecoverCardsWithAbilityOfTypeFilter.Count; i++)
			{
				EAbilityType item = state.RecoverCardsWithAbilityOfTypeFilter[i];
				RecoverCardsWithAbilityOfTypeFilter.Add(item);
			}
			references.Add(state.RecoverCardsWithAbilityOfTypeFilter, RecoverCardsWithAbilityOfTypeFilter);
		}
		StartLostCards = state.StartLostCards;
	}
}
