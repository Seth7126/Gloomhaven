using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Formats.Alembic.Util;

namespace UnityEngine.Formats.Alembic.Exporter;

internal static class AlembicExporterAnalytics
{
	[Serializable]
	internal struct AlembicExporterAnalyticsEvent
	{
		public bool capture_mesh;

		public bool skinned_mesh;

		public bool camera;

		public bool static_mesh_renderers;
	}

	private const string VendorKey = "unity.alembic";

	private const string EventName = "alembic_exporter";

	private const int MAXEventsPerHour = 1000;

	private const int MAXNumberOfElements = 1000;

	internal static void SendAnalytics(AlembicRecorderSettings settings)
	{
	}

	internal static AlembicExporterAnalyticsEvent CreateEvent(AlembicRecorderSettings settings)
	{
		return new AlembicExporterAnalyticsEvent
		{
			capture_mesh = (settings.CaptureMeshRenderer && settings.GetTargets<MeshFilter>().Any()),
			skinned_mesh = (settings.CaptureSkinnedMeshRenderer && settings.GetTargets<SkinnedMeshRenderer>().Any()),
			camera = (settings.CaptureCamera && settings.GetTargets<Camera>().Any()),
			static_mesh_renderers = settings.AssumeNonSkinnedMeshesAreConstant
		};
	}

	private static IEnumerable<T> GetTargets<T>(this AlembicRecorderSettings settings) where T : Component
	{
		if (settings.Scope == ExportScope.TargetBranch)
		{
			if (!(settings.TargetBranch != null))
			{
				return Enumerable.Empty<T>();
			}
			return settings.TargetBranch.GetComponentsInChildren<T>();
		}
		return Object.FindObjectsOfType<T>();
	}
}
