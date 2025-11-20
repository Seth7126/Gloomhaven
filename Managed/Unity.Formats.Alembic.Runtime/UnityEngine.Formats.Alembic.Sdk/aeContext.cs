using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aeContext
{
	public IntPtr self;

	public aeObject topObject => NativeMethods.aeGetTopObject(self);

	public static aeContext Create()
	{
		return NativeMethods.aeCreateContext();
	}

	public void Destroy()
	{
		NativeMethods.aeDestroyContext(self);
		self = IntPtr.Zero;
	}

	public void SetConfig(AlembicExportOptions conf)
	{
		NativeMethods.aeSetConfig(self, conf);
	}

	public bool OpenArchive(string path)
	{
		return NativeMethods.aeOpenArchive(self, path);
	}

	public int AddTimeSampling(float start_time)
	{
		return NativeMethods.aeAddTimeSampling(self, start_time);
	}

	public void AddTime(float start_time)
	{
		NativeMethods.aeAddTime(self, start_time);
	}

	public void MarkFrameBegin()
	{
		NativeMethods.aeMarkFrameBegin(self);
	}

	public void MarkFrameEnd()
	{
		NativeMethods.aeMarkFrameEnd(self);
	}
}
