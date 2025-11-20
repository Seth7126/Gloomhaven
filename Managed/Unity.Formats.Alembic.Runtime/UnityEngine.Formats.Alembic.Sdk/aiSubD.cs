using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

[StructLayout(LayoutKind.Explicit)]
internal struct aiSubD
{
	[FieldOffset(0)]
	public IntPtr self;

	[FieldOffset(0)]
	public aiSchema schema;

	public aiPolyMeshSample sample => NativeMethods.aiSubD.aiSchemaGetSample(self);

	public static implicit operator bool(aiSubD v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSchema(aiSubD v)
	{
		return v.schema;
	}

	public void GetSummary(ref aiMeshSummary dst)
	{
		NativeMethods.aiSubDGetSummary(self, ref dst);
	}
}
