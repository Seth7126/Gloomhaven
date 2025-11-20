using Unity.Profiling;

namespace UnityEngine;

public sealed class StaticBatchingUtility
{
	internal static ProfilerMarker s_CombineMarker = new ProfilerMarker("StaticBatching.Combine");

	internal static ProfilerMarker s_SortMarker = new ProfilerMarker("StaticBatching.SortObjects");

	internal static ProfilerMarker s_MakeBatchMarker = new ProfilerMarker("StaticBatching.MakeBatch");

	public static void Combine(GameObject staticBatchRoot)
	{
		using (s_CombineMarker.Auto())
		{
			InternalStaticBatchingUtility.CombineRoot(staticBatchRoot, null);
		}
	}

	public static void Combine(GameObject[] gos, GameObject staticBatchRoot)
	{
		using (s_CombineMarker.Auto())
		{
			InternalStaticBatchingUtility.CombineGameObjects(gos, staticBatchRoot, isEditorPostprocessScene: false, null);
		}
	}
}
