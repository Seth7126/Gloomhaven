using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aeProperty
{
	public IntPtr self;

	public aeProperty(IntPtr self)
	{
		this.self = self;
	}

	public void WriteArraySample(IntPtr data, int numData)
	{
		NativeMethods.aePropertyWriteArraySample(self, data, numData);
	}

	public void WriteScalarSample(ref float data)
	{
		NativeMethods.aePropertyWriteScalarSample(self, ref data);
	}

	public void WriteScalarSample(ref int data)
	{
		NativeMethods.aePropertyWriteScalarSample(self, ref data);
	}

	public void WriteScalarSample(ref Bool data)
	{
		NativeMethods.aePropertyWriteScalarSample(self, ref data);
	}

	public void WriteScalarSample(ref Vector2 data)
	{
		NativeMethods.aePropertyWriteScalarSample(self, ref data);
	}

	public void WriteScalarSample(ref Vector3 data)
	{
		NativeMethods.aePropertyWriteScalarSample(self, ref data);
	}

	public void WriteScalarSample(ref Vector4 data)
	{
		NativeMethods.aePropertyWriteScalarSample(self, ref data);
	}

	public void WriteScalarSample(ref Matrix4x4 data)
	{
		NativeMethods.aePropertyWriteScalarSample(self, ref data);
	}
}
