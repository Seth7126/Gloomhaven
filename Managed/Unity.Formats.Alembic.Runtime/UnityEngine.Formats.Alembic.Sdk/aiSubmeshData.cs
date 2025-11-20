using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiSubmeshData
{
	public IntPtr indexes;

	public unsafe char* facesetNames;
}
