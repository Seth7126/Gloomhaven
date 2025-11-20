using Script.LogicalOperations;

namespace Script.GUI.SMNavigation;

public abstract class ConditionalSession : Session, IConditionHandler
{
	private readonly ICondition _condition;

	protected bool ConditionValue => _condition.Value;

	protected ConditionalSession(ICondition condition)
	{
		_condition = condition;
	}

	public override void Enter()
	{
		base.Enter();
		_condition.OnValueChanged += OnConditionValueChanged;
	}

	public override void Exit()
	{
		base.Exit();
		_condition.OnValueChanged -= OnConditionValueChanged;
	}

	public abstract void OnConditionValueChanged(bool value);
}
