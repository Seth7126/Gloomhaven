using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiCurvesSample
{
	public IntPtr self;

	public static implicit operator bool(aiCurvesSample v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSample(aiCurvesSample v)
	{
		aiSample result = default(aiSample);
		result.self = v.self;
		return result;
	}

	public void GetSummary(ref aiCurvesSampleSummary dst)
	{
		NativeMethods.aiCurvesGetSampleSummary(self, ref dst);
	}

	public void FillData(PinnedList<aiCurvesData> dst)
	{
		NativeMethods.aiCurvesFillData(self, dst);
	}
}
