using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiPolyMeshData
{
	public unsafe void* positions;

	public unsafe void* velocities;

	public unsafe void* normals;

	public unsafe void* tangents;

	public unsafe void* uv0;

	public unsafe void* uv1;

	public unsafe void* rgba;

	public unsafe void* rgb;

	public IntPtr indices;

	public int vertexCount;

	public int indexCount;

	public Vector3 center;

	public Vector3 extents;
}
