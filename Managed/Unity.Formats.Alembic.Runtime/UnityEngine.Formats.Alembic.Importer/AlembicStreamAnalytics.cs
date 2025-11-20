using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Importer;

internal static class AlembicStreamAnalytics
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct AlembicChangeStreamEvent
	{
	}

	private const string VendorKey = "unity.alembic";

	private const string EventName = "alembic_change_stream";

	private const int MAXEventsPerHour = 1000;

	private const int MAXNumberOfElements = 1000;

	internal static void SendAnalytics()
	{
	}
}
