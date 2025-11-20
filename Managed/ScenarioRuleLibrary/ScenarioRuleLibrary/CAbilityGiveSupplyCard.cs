using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityGiveSupplyCard : CAbilityTargeting
{
	public List<int> SupplyCardIDs;

	private int m_SupplyCardsGiven;

	private CPlayerActor m_PlayerActor;

	public CAbilityGiveSupplyCard(List<int> supplyCardIDs)
		: base(EAbilityType.GiveSupplyCard)
	{
		SupplyCardIDs = supplyCardIDs.ToList();
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		m_SupplyCardsGiven = 0;
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(base.AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			base.TargetingActor.FindCardWithAbility(this);
			CPlayerActor obj = (CPlayerActor)base.TargetingActor;
			if (actor is CPlayerActor playerActor)
			{
				m_PlayerActor = playerActor;
			}
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (CAbilityCard supplyCard in obj.CharacterClass.SupplyCards)
			{
				if (!SupplyCardIDs.Contains(supplyCard.ID))
				{
					continue;
				}
				if (m_PlayerActor == null)
				{
					foreach (CAbility topActionAbility in supplyCard.Copy().GetTopActionAbilities())
					{
						if (topActionAbility is CAbilityTargeting)
						{
							topActionAbility.TargetingActor = base.TargetingActor;
							topActionAbility.ApplyToActor(actor);
						}
					}
				}
				else
				{
					CAbilityCard item = supplyCard.Copy();
					list.Add(item);
					m_PlayerActor.CharacterClass.HandAbilityCards.Add(item);
					CharacterClassManager.AllAbilityCardInstances.Add(item);
					m_SupplyCardsGiven++;
				}
			}
			if (m_PlayerActor != null)
			{
				CSupplyCardsGiven_MessageData cSupplyCardsGiven_MessageData = new CSupplyCardsGiven_MessageData(base.TargetingActor);
				cSupplyCardsGiven_MessageData.m_ActorGivenCards = m_PlayerActor;
				cSupplyCardsGiven_MessageData.m_SupplyCardsGiven = list;
				ScenarioRuleClient.MessageHandler(cSupplyCardsGiven_MessageData);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public static List<string> GetSupplyCardNames(CAbilityGiveSupplyCard ability)
	{
		List<string> list = new List<string>();
		if (ability.TargetingActor.Class is CCharacterClass cCharacterClass)
		{
			foreach (CAbilityCard supplyCard in cCharacterClass.SupplyCards)
			{
				if (ability.SupplyCardIDs.Contains(supplyCard.ID))
				{
					list.Add(supplyCard.Name);
				}
			}
		}
		return list;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		m_SupplyCardsGiven = -1;
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
		if (m_SupplyCardsGiven >= 0)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTargeting(abilityType, subTypeAbility, m_State, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, actedOnClass, actedOnType, actedOnIsSummon, actor?.Tokens.CheckPositiveTokens, actor?.Tokens.CheckNegativeTokens, "", m_SupplyCardsGiven));
		}
	}

	public CAbilityGiveSupplyCard()
	{
	}

	public CAbilityGiveSupplyCard(CAbilityGiveSupplyCard state, ReferenceDictionary references)
		: base(state, references)
	{
		SupplyCardIDs = references.Get(state.SupplyCardIDs);
		if (SupplyCardIDs == null && state.SupplyCardIDs != null)
		{
			SupplyCardIDs = new List<int>();
			for (int i = 0; i < state.SupplyCardIDs.Count; i++)
			{
				int item = state.SupplyCardIDs[i];
				SupplyCardIDs.Add(item);
			}
			references.Add(state.SupplyCardIDs, SupplyCardIDs);
		}
		m_SupplyCardsGiven = state.m_SupplyCardsGiven;
	}
}
