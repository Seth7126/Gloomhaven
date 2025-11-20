using ScenarioRuleLibrary;

public interface IAbilityCardOption
{
	CAbilityCard AbilityCard { get; }

	CBaseCard.ActionType ActionTypeDisabled { get; }
}
