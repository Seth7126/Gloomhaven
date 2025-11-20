using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aePolyMeshData
{
	public IntPtr normals;

	public IntPtr uv0;

	public IntPtr uv1;

	public IntPtr colors;

	public IntPtr submeshes;

	public int submeshCount;

	public Bool visibility { get; set; }

	public IntPtr points { get; set; }

	public int pointCount { get; set; }
}
