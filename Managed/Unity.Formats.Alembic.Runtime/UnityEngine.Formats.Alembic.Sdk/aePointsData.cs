using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aePointsData
{
	public Bool visibility { get; set; }

	public IntPtr positions { get; set; }

	public IntPtr velocities { get; set; }

	public IntPtr ids { get; set; }

	public int count { get; set; }
}
