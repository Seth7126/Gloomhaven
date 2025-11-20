using System.Collections.Generic;

namespace Script.LogicalOperations;

public class AllEqualBooleanOperation : IBooleanOperation
{
	public bool Execute(IEnumerable<bool> values)
	{
		bool flag = true;
		bool flag2 = true;
		foreach (bool value in values)
		{
			flag = flag && value;
			flag2 = flag2 && !value;
		}
		return flag ^ flag2;
	}
}
