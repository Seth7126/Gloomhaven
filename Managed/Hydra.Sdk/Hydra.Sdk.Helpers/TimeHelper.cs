using System;
using System.Diagnostics;
using Hydra.Sdk.Extensions;

namespace Hydra.Sdk.Helpers;

public static class TimeHelper
{
	private static Stopwatch _stopwatch;

	private static long _startTime;

	internal static void Initialize(long startTime)
	{
		_startTime = startTime;
		_stopwatch = Stopwatch.StartNew();
	}

	public static (bool IsLocal, long Time) GetBackendTime()
	{
		return (IsLocal: _stopwatch == null, Time: (_stopwatch != null) ? (_startTime + _stopwatch.ElapsedMilliseconds) : DateTime.UtcNow.ToUnixMilliseconds());
	}
}
