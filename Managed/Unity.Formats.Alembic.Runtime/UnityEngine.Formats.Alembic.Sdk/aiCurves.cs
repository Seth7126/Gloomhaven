using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

[StructLayout(LayoutKind.Explicit)]
internal struct aiCurves
{
	[FieldOffset(0)]
	public IntPtr self;

	[FieldOffset(0)]
	public aiSchema schema;

	internal aiCurvesSample sample => NativeMethods.aiCurves.aiSchemaGetSample(self);

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

	public static implicit operator bool(aiCurves v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSchema(aiCurves v)
	{
		return v.schema;
	}

	public void GetSummary(ref aiCurvesSummary dst)
	{
		NativeMethods.aiCurvesGetSummary(self, ref dst);
	}
}
