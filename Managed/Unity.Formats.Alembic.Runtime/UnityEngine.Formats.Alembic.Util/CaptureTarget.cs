using System;

namespace UnityEngine.Formats.Alembic.Util;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class CaptureTarget : Attribute
{
	public Type componentType { get; set; }

	public CaptureTarget(Type t)
	{
		componentType = t;
	}
}
