using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityChangeCharacterModel : CAbilityTargeting
{
	public ECharacter CharacterModel { get; private set; }

	public CAbilityChangeCharacterModel(ECharacter characterModel)
		: base(EAbilityType.ChangeCharacterModel)
	{
		CharacterModel = characterModel;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		return true;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
			if (base.ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, base.TargetingActor);
			}
			else if (cBaseCard != null)
			{
				cBaseCard.AddActiveBonus(this, actor, base.TargetingActor);
			}
			else
			{
				DLLDebug.LogError("Unable to find base ability card for ability " + base.Name);
			}
			CChangeCharacterModel_MessageData cChangeCharacterModel_MessageData = new CChangeCharacterModel_MessageData(actor);
			cChangeCharacterModel_MessageData.m_ChangeCharacterAbility = this;
			ScenarioRuleClient.MessageHandler(cChangeCharacterModel_MessageData);
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool CanApplyActiveBonusTogglesTo()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityChangeCharacterModel()
	{
	}

	public CAbilityChangeCharacterModel(CAbilityChangeCharacterModel state, ReferenceDictionary references)
		: base(state, references)
	{
		CharacterModel = state.CharacterModel;
	}
}
