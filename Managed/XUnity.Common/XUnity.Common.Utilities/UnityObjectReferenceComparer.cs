using System.Collections.Generic;

namespace XUnity.Common.Utilities;

public class UnityObjectReferenceComparer : IEqualityComparer<object>
{
	public static readonly UnityObjectReferenceComparer Default = new UnityObjectReferenceComparer();

	public new bool Equals(object x, object y)
	{
		return x == y;
	}

	public int GetHashCode(object obj)
	{
		return obj.GetHashCode();
	}
}
