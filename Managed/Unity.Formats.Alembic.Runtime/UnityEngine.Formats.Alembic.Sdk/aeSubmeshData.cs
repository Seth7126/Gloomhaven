using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aeSubmeshData
{
	internal IntPtr indexes { get; set; }

	public int indexCount { get; set; }

	public aeTopology topology { get; set; }
}
