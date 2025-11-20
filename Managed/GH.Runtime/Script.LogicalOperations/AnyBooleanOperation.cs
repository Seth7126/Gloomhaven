using System.Collections.Generic;

namespace Script.LogicalOperations;

public class AnyBooleanOperation : IBooleanOperation
{
	private readonly bool _targetValue;

	public AnyBooleanOperation(bool targetValue)
	{
		_targetValue = targetValue;
	}

	public bool Execute(IEnumerable<bool> values)
	{
		bool flag = false;
		foreach (bool value in values)
		{
			flag |= value == _targetValue;
		}
		return flag;
	}
}
