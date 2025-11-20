using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_LoadFromMemoryAsync_Hook
{
	private delegate AssetBundleCreateRequest OriginalMethod(byte[] binary, uint crc);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundle != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromMemoryAsync_Internal", typeof(byte[]), typeof(uint)) ?? AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromMemoryAsync", typeof(byte[]), typeof(uint));
	}

	private static bool Prefix(ref byte[] binary, ref uint crc, ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state)
	{
		AssetBundleLoadingParameters parameters = new AssetBundleLoadingParameters(binary, null, crc, 0uL, null, 0u, AssetBundleLoadType.LoadFromMemory);
		__state = ResourceRedirection.Hook_AssetBundleLoading_Prefix(parameters, out __result);
		AssetBundleLoadingParameters parameters2 = __state.Parameters;
		binary = parameters2.Binary;
		crc = parameters2.Crc;
		return !__state.SkipOriginalCall;
	}

	private static void Postfix(ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state)
	{
		ResourceRedirection.Hook_AssetBundleLoading_Postfix(__state, __result);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static AssetBundleCreateRequest MM_Detour(byte[] binary, uint crc)
	{
		AssetBundleCreateRequest __result = null;
		AsyncAssetBundleLoadingContext __state = null;
		if (Prefix(ref binary, ref crc, ref __result, ref __state))
		{
			__result = _original(binary, crc);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
