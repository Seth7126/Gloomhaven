using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiXformSample
{
	public IntPtr self;

	public static implicit operator bool(aiXformSample v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSample(aiXformSample v)
	{
		aiSample result = default(aiSample);
		result.self = v.self;
		return result;
	}

	public void GetData(ref aiXformData dst)
	{
		NativeMethods.aiXformGetData(self, ref dst);
	}
}
