using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

[StructLayout(LayoutKind.Explicit)]
internal struct aiCamera
{
	[FieldOffset(0)]
	public IntPtr self;

	[FieldOffset(0)]
	public aiSchema schema;

	public aiCameraSample sample => NativeMethods.aiCamera.aiSchemaGetSample(self);

	public static implicit operator bool(aiCamera v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSchema(aiCamera v)
	{
		return v.schema;
	}
}
