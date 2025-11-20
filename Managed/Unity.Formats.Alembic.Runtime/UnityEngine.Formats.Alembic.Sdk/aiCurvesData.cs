using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiCurvesData
{
	public Bool visibility;

	public IntPtr positions;

	public IntPtr numVertices;

	public IntPtr uvs;

	public IntPtr widths;

	public IntPtr velocities;

	public int count;
}
