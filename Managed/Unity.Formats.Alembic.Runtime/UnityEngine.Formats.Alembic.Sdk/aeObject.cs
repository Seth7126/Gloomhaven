using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aeObject
{
	public IntPtr self;

	public aeObject(IntPtr self)
	{
		this.self = self;
	}

	public aeObject NewXform(string name, int tsi)
	{
		return NativeMethods.aeNewXform(self, SanitizeName(name), tsi);
	}

	public aeObject NewCamera(string name, int tsi)
	{
		return NativeMethods.aeNewCamera(self, SanitizeName(name), tsi);
	}

	public aeObject NewPoints(string name, int tsi)
	{
		return NativeMethods.aeNewPoints(self, SanitizeName(name), tsi);
	}

	public aeObject NewPolyMesh(string name, int tsi)
	{
		return NativeMethods.aeNewPolyMesh(self, SanitizeName(name), tsi);
	}

	public void WriteSample(ref aeXformData data)
	{
		NativeMethods.aeXformWriteSample(self, ref data);
	}

	public void WriteSample(ref CameraData data)
	{
		NativeMethods.aeCameraWriteSample(self, ref data);
	}

	public void WriteSample(ref aePolyMeshData data)
	{
		NativeMethods.aePolyMeshWriteSample(self, ref data);
	}

	public void AddFaceSet(string name)
	{
		NativeMethods.aePolyMeshAddFaceSet(self, name);
	}

	public void WriteSample(ref aePointsData data)
	{
		NativeMethods.aePointsWriteSample(self, ref data);
	}

	public aeProperty NewProperty(string name, aePropertyType type)
	{
		return NativeMethods.aeNewProperty(self, name, type);
	}

	public void MarkForceInvisible()
	{
		NativeMethods.aeMarkForceInvisible(self);
	}

	private static string SanitizeName(string name)
	{
		if (!name.Contains("/"))
		{
			return name;
		}
		string text = name.Replace('/', '_');
		Debug.LogWarning("AlembicExporter: Illegal character '/' in Alembic object name '" + name + "'. Replaced with " + text);
		return text;
	}
}
