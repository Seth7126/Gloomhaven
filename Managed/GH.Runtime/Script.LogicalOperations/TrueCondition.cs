using System;

namespace Script.LogicalOperations;

public class TrueCondition : ICondition
{
	public bool Value => true;

	public event Action<bool> OnValueChanged;
}
