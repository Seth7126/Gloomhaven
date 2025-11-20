using System;

namespace Script.LogicalOperations;

public interface ICondition
{
	bool Value { get; }

	event Action<bool> OnValueChanged;
}
