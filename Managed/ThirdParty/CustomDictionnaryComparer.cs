using System.Collections.Generic;

public class CustomDictionnaryComparer : IEqualityComparer<KeyValuePair<string, string>>
{
	public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
	{
		return x.Key.Equals(y.Key);
	}

	public int GetHashCode(KeyValuePair<string, string> obj)
	{
		return obj.Key.GetHashCode();
	}
}
