using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiSample
{
	public IntPtr self;

	public static implicit operator bool(aiSample v)
	{
		return v.self != IntPtr.Zero;
	}

	public static explicit operator aiXformSample(aiSample v)
	{
		aiXformSample result = default(aiXformSample);
		result.self = v.self;
		return result;
	}

	public static explicit operator aiCameraSample(aiSample v)
	{
		aiCameraSample result = default(aiCameraSample);
		result.self = v.self;
		return result;
	}

	public static explicit operator aiPolyMeshSample(aiSample v)
	{
		aiPolyMeshSample result = default(aiPolyMeshSample);
		result.self = v.self;
		return result;
	}

	public static explicit operator aiPointsSample(aiSample v)
	{
		aiPointsSample result = default(aiPointsSample);
		result.self = v.self;
		return result;
	}
}
