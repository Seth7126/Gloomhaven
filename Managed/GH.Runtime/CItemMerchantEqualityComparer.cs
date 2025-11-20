using System.Collections.Generic;
using ScenarioRuleLibrary;

internal class CItemMerchantEqualityComparer : IEqualityComparer<CItem>
{
	public bool Equals(CItem x, CItem y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null)
		{
			return false;
		}
		if (y == null)
		{
			return false;
		}
		if (x.GetType() != y.GetType())
		{
			return false;
		}
		return x.ID == y.ID;
	}

	public int GetHashCode(CItem obj)
	{
		return obj.ID.GetHashCode();
	}
}
