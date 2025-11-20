using System;
using System.Runtime.InteropServices;

namespace Apparance.Net;

internal static class Interop
{
	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern int ApparanceInitialise(string procedure_storage_path, LogMessageFn log_message, int num_synths, int buffer_size_mb, int live_editing);

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern int ApparanceIsRunning();

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern void ApparanceUpdate(float dt, Vector3 view_position);

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern void ApparanceSave();

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern void ApparanceShutdown();

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern int ApparanceCreateEntity(int old_handle);

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern void ApparanceDestroyEntity(int entity_id);

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern void ApparanceEntityBuild(int entity_handle, uint procedure_id, int data_size, IntPtr data_bytes, bool dynamic_detail);

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern int ApparancePopEntityTask(out int data_size, out IntPtr data_bytes);

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern int ApparancePopEngineTask(out int data_size, out IntPtr data_bytes);

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern void ApparanceUpdateAsset(int entity_context, string asset_descriptor, int asset_id, int bounds_available, float[] asset_frame, int variant_count);

	[DllImport("ApparanceEngine", CharSet = CharSet.Ansi)]
	public static extern string ApparanceGetNextAssetRequest(out int entity_context, out int asset_id);
}
