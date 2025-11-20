using System;

namespace UnityEngine.Formats.Alembic.Sdk;

[Serializable]
internal struct Bool
{
	[SerializeField]
	private byte v;

	public static implicit operator bool(Bool v)
	{
		return v.v != 0;
	}

	public static bool ToBool(Bool v)
	{
		return v;
	}

	public static implicit operator Bool(bool v)
	{
		Bool result = default(Bool);
		result.v = (byte)(v ? 1 : 0);
		return result;
	}

	public static Bool ToBool(bool v)
	{
		return v;
	}
}
