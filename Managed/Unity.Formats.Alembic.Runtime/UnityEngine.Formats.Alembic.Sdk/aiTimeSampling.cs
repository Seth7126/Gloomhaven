using System;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiTimeSampling
{
	internal IntPtr self;

	public int sampleCount => NativeMethods.aiTimeSamplingGetSampleCount(self);

	public double GetTime(int index)
	{
		return NativeMethods.aiTimeSamplingGetTime(self, index);
	}
}
