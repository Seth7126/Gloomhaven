using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRecoverDiscardedCards : CAbilityTargeting
{
	public List<EAbilityType> RecoverCardsWithAbilityOfTypeFilter;

	public CAbilityRecoverDiscardedCards(List<EAbilityType> recoverCardsWithAbilityOfTypeFilter)
		: base(EAbilityType.RecoverDiscardedCards)
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
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			if (actor.Type == CActor.EType.Player && HasCardsToRecover(((CPlayerActor)actor).CharacterClass.DiscardedAbilityCards))
			{
				if (m_Strength == int.MaxValue)
				{
					CPlayerActor cPlayerActor = (CPlayerActor)actor;
					foreach (CAbilityCard card in cPlayerActor.CharacterClass.DiscardedAbilityCards.ToList())
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
							cPlayerActor.CharacterClass.MoveAbilityCard(card, cPlayerActor.CharacterClass.DiscardedAbilityCards, cPlayerActor.CharacterClass.HandAbilityCards, "DiscardedAbilityCards", "HandAbilityCards");
						}
					}
					CRecoverDiscardedCards_MessageData cRecoverDiscardedCards_MessageData = new CRecoverDiscardedCards_MessageData(base.AnimOverload, base.TargetingActor);
					cRecoverDiscardedCards_MessageData.m_ActorRecoveringLostCards = actor;
					cRecoverDiscardedCards_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cRecoverDiscardedCards_MessageData);
					return true;
				}
				_ = (CPlayerActor)actor;
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

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityRecoverDiscardedCards()
	{
	}

	public CAbilityRecoverDiscardedCards(CAbilityRecoverDiscardedCards state, ReferenceDictionary references)
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
	}
}
