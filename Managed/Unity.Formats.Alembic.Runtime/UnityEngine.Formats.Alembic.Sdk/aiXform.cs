using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

[StructLayout(LayoutKind.Explicit)]
internal struct aiXform
{
	[FieldOffset(0)]
	public IntPtr self;

	[FieldOffset(0)]
	public aiSchema schema;

	public aiXformSample sample => NativeMethods.aiXform.aiSchemaGetSample(self);

	public static implicit operator bool(aiXform v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSchema(aiXform v)
	{
		return v.schema;
	}
}
