using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiContext
{
	[NativeDisableUnsafePtrRestriction]
	internal IntPtr self;

	internal aiObject topObject => NativeMethods.aiContextGetTopObject(self);

	public int timeSamplingCount => NativeMethods.aiContextGetTimeSamplingCount(self);

	public static implicit operator bool(aiContext v)
	{
		return v.self != IntPtr.Zero;
	}

	public static bool ToBool(aiContext v)
	{
		return v;
	}

	public static aiContext Create(int uid)
	{
		return NativeMethods.aiContextCreate(uid);
	}

	public static void DestroyByPath(string path)
	{
		NativeMethods.aiClearContextsWithPath(Path.GetFullPath(path));
	}

	public void Destroy()
	{
		NativeMethods.aiContextDestroy(self);
		self = IntPtr.Zero;
	}

	public bool Load(string path)
	{
		string fullPath = Path.GetFullPath(path);
		return NativeMethods.aiContextLoad(self, fullPath);
	}

	public bool IsHDF5()
	{
		return NativeMethods.aiContextGetIsHDF5(self);
	}

	public string GetApplication()
	{
		return Marshal.PtrToStringAnsi(NativeMethods.aiContextGetApplication(self));
	}

	internal void SetConfig(ref aiConfig conf)
	{
		NativeMethods.aiContextSetConfig(self, ref conf);
	}

	public void UpdateSamples(double time)
	{
		NativeMethods.aiContextUpdateSamples(self, time);
	}

	public aiTimeSampling GetTimeSampling(int i)
	{
		return NativeMethods.aiContextGetTimeSampling(self, i);
	}

	internal void GetTimeRange(out double begin, out double end)
	{
		NativeMethods.aiContextGetTimeRange(self, out begin, out end);
	}
}
