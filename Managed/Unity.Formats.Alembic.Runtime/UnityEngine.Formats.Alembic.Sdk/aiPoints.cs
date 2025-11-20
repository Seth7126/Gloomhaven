using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

[StructLayout(LayoutKind.Explicit)]
internal struct aiPoints
{
	[FieldOffset(0)]
	public IntPtr self;

	[FieldOffset(0)]
	public aiSchema schema;

	internal aiPointsSample sample => NativeMethods.aiPoints.aiSchemaGetSample(self);

	public bool sort
	{
		set
		{
			NativeMethods.aiPointsSetSort(self, value);
		}
	}

	public Vector3 sortBasePosition
	{
		set
		{
			NativeMethods.aiPointsSetSortBasePosition(self, value);
		}
	}

	public static implicit operator bool(aiPoints v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSchema(aiPoints v)
	{
		return v.schema;
	}

	public void GetSummary(ref aiPointsSummary dst)
	{
		NativeMethods.aiPointsGetSummary(self, ref dst);
	}
}
