using System.Diagnostics;

namespace UdpKit.Utils;

internal static class PrecisionTimer
{
	private static readonly long start = Stopwatch.GetTimestamp();

	private static readonly double freq = 1f / (float)Stopwatch.Frequency;

	internal static uint GetCurrentTime()
	{
		long num = Stopwatch.GetTimestamp() - start;
		double num2 = (double)num * freq;
		return (uint)(num2 * 1000.0);
	}
}
