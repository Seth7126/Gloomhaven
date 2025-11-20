using System;

namespace Platforms.Utils;

public static class CastExtension
{
	public static T TryCastTo<T>(this object data)
	{
		if (data is T)
		{
			return (T)data;
		}
		try
		{
			return (T)Convert.ChangeType(data, typeof(T));
		}
		catch (InvalidCastException)
		{
			return default(T);
		}
	}
}
