using System.Collections.Generic;

namespace Script.LogicalOperations;

public interface IBooleanOperation
{
	bool Execute(IEnumerable<bool> values);
}
