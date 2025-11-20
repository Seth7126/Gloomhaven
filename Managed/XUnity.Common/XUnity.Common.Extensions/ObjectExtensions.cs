using System;

namespace XUnity.Common.Extensions;

public static class ObjectExtensions
{
	public static Type GetUnityType(this object obj)
	{
		return obj.GetType();
	}

	public static bool TryCastTo<TObject>(this object obj, out TObject castedObject)
	{
		if (obj is TObject val)
		{
			castedObject = val;
			return true;
		}
		castedObject = default(TObject);
		return false;
	}
}
