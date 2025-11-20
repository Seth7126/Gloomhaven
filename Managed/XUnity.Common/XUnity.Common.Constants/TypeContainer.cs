using System;

namespace XUnity.Common.Constants;

public class TypeContainer
{
	public Type ClrType { get; }

	public Type UnityType { get; }

	public TypeContainer(Type type)
	{
		UnityType = type;
		ClrType = type;
	}

	public bool IsAssignableFrom(Type unityType)
	{
		if (UnityType != null)
		{
			return UnityType.IsAssignableFrom(unityType);
		}
		return false;
	}
}
