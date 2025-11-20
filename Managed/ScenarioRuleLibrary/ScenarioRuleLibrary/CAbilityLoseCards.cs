using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityLoseCards : CAbilityTargeting
{
	public CAbilityLoseCards()
		: base(EAbilityType.LoseCards)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			if (actor.Type == CActor.EType.Player && ((CPlayerActor)actor).CharacterClass.HandAbilityCards.Count > 0)
			{
				if (m_Strength == int.MaxValue)
				{
					CPlayerActor cPlayerActor = (CPlayerActor)actor;
					for (int num = cPlayerActor.CharacterClass.HandAbilityCards.Count - 1; num >= 0; num--)
					{
						cPlayerActor.CharacterClass.MoveAbilityCard(cPlayerActor.CharacterClass.HandAbilityCards[num], cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.LostAbilityCards, "HandAbilityCards", "LostAbilityCards");
					}
					CLoseCards_MessageData cLoseCards_MessageData = new CLoseCards_MessageData(base.AnimOverload, base.TargetingActor);
					cLoseCards_MessageData.m_ActorLosingCards = actor;
					cLoseCards_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cLoseCards_MessageData);
					return true;
				}
				_ = (CPlayerActor)actor;
				CSelectLoseCards_MessageData cSelectLoseCards_MessageData = new CSelectLoseCards_MessageData(base.AnimOverload, base.TargetingActor);
				cSelectLoseCards_MessageData.m_ActorLosingCards = actor;
				cSelectLoseCards_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cSelectLoseCards_MessageData);
				return true;
			}
		}
		return false;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public CAbilityLoseCards(CAbilityLoseCards state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
