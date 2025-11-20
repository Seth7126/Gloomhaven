namespace System.Runtime.Serialization;

public static class SerializationInfoExtensions
{
	public static bool Exists(this SerializationInfo info, string name)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (name == enumerator.Current.Name)
			{
				return true;
			}
		}
		return false;
	}
}
