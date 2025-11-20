using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiProperty
{
	public IntPtr self;

	public static implicit operator bool(aiProperty v)
	{
		return v.self != IntPtr.Zero;
	}
}
