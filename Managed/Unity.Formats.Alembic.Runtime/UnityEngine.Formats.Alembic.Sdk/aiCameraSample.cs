using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiCameraSample
{
	public IntPtr self;

	public static implicit operator bool(aiCameraSample v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSample(aiCameraSample v)
	{
		aiSample result = default(aiSample);
		result.self = v.self;
		return result;
	}

	public void GetData(ref CameraData dst)
	{
		NativeMethods.aiCameraGetData(self, ref dst);
	}
}
