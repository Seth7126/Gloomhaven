using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAdjustInitiativeActiveBonus_FocusInitiative : CBespokeBehaviour
{
	public CAdjustInitiativeActiveBonus_FocusInitiative(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public int SetFocusInitiative()
	{
		return m_Strength;
	}

	public CAdjustInitiativeActiveBonus_FocusInitiative()
	{
	}

	public CAdjustInitiativeActiveBonus_FocusInitiative(CAdjustInitiativeActiveBonus_FocusInitiative state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
