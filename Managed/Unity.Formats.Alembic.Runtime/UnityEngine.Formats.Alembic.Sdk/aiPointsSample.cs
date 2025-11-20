using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiPointsSample
{
	public IntPtr self;

	public static implicit operator bool(aiPointsSample v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSample(aiPointsSample v)
	{
		aiSample result = default(aiSample);
		result.self = v.self;
		return result;
	}

	public void GetSummary(ref aiPointsSampleSummary dst)
	{
		NativeMethods.aiPointsGetSampleSummary(self, ref dst);
	}

	public void FillData(PinnedList<aiPointsData> dst)
	{
		NativeMethods.aiPointsFillData(self, dst);
	}
}
