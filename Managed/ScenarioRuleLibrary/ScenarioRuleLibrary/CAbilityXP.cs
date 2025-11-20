using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityXP : CAbility
{
	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		targetActor.GainXP(m_Strength);
	}

	public override bool Perform()
	{
		PhaseManager.NextStep();
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}

	public CAbilityXP()
	{
	}

	public CAbilityXP(CAbilityXP state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
