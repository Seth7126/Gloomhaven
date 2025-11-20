using System.Collections.Generic;

namespace Script.LogicalOperations;

public class AllBooleanOperation : IBooleanOperation
{
	private readonly bool _targetValue;

	public AllBooleanOperation(bool targetValue)
	{
		_targetValue = targetValue;
	}

	public bool Execute(IEnumerable<bool> values)
	{
		bool flag = true;
		foreach (bool value in values)
		{
			flag &= value == _targetValue;
		}
		return flag;
	}
}
