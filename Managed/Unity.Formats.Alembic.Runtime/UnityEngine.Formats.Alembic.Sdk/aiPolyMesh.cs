using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

[StructLayout(LayoutKind.Explicit)]
internal struct aiPolyMesh
{
	[FieldOffset(0)]
	public IntPtr self;

	[FieldOffset(0)]
	public aiSchema schema;

	public aiPolyMeshSample sample => NativeMethods.aiPolyMesh.aiSchemaGetSample(self);

	public static implicit operator bool(aiPolyMesh v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSchema(aiPolyMesh v)
	{
		return v.schema;
	}

	public void GetSummary(ref aiMeshSummary dst)
	{
		NativeMethods.aiPolyMeshGetSummary(self, ref dst);
	}
}
