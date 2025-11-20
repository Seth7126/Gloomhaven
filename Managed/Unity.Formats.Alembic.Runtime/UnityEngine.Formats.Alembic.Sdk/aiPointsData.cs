using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiPointsData
{
	public Bool visibility;

	public IntPtr points;

	public IntPtr velocities;

	public IntPtr ids;

	public int count;

	public Vector3 boundsCenter;

	public Vector3 boundsExtents;
}
