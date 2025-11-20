using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiSchema
{
	public IntPtr self;

	public bool isDataUpdated
	{
		get
		{
			NativeMethods.aiSchemaSync(self);
			return NativeMethods.aiSchemaIsDataUpdated(self);
		}
	}

	public static implicit operator bool(aiSchema v)
	{
		return v.self != IntPtr.Zero;
	}

	public static explicit operator aiXform(aiSchema v)
	{
		return new aiXform
		{
			self = v.self
		};
	}

	public static explicit operator aiCamera(aiSchema v)
	{
		return new aiCamera
		{
			self = v.self
		};
	}

	public static explicit operator aiPolyMesh(aiSchema v)
	{
		return new aiPolyMesh
		{
			self = v.self
		};
	}

	public static explicit operator aiSubD(aiSchema v)
	{
		return new aiSubD
		{
			self = v.self
		};
	}

	public static explicit operator aiPoints(aiSchema v)
	{
		return new aiPoints
		{
			self = v.self
		};
	}

	public static explicit operator aiCurves(aiSchema v)
	{
		return new aiCurves
		{
			self = v.self
		};
	}

	public void UpdateSample(ref aiSampleSelector ss)
	{
		NativeMethods.aiSchemaUpdateSample(self, ref ss);
	}
}
