using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CChangeCharacterModelActiveBonus : CActiveBonus
{
	private CPlayerActor m_PlayerActor;

	public CAbilityChangeCharacterModel ChangeCharacterModelAbility { get; set; }

	public CChangeCharacterModelActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		ChangeCharacterModelAbility = (CAbilityChangeCharacterModel)ability;
		if (actor is CPlayerActor cPlayerActor)
		{
			m_PlayerActor = cPlayerActor;
			cPlayerActor.OverrideCharacterModel = ChangeCharacterModelAbility.CharacterModel.ToString();
		}
	}

	public override void Finish()
	{
		m_PlayerActor.OverrideCharacterModel = null;
		CChangeCharacterModel_MessageData cChangeCharacterModel_MessageData = new CChangeCharacterModel_MessageData(m_PlayerActor);
		cChangeCharacterModel_MessageData.m_ChangeCharacterAbility = ChangeCharacterModelAbility;
		cChangeCharacterModel_MessageData.AnimOverload = base.Ability.ActiveBonusData.ActiveBonusAnimOverload;
		ScenarioRuleClient.MessageHandler(cChangeCharacterModel_MessageData);
		base.Finish();
	}

	public CChangeCharacterModelActiveBonus()
	{
	}

	public CChangeCharacterModelActiveBonus(CChangeCharacterModelActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
