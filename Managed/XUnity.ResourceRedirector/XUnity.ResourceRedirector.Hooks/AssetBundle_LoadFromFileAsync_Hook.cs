using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_LoadFromFileAsync_Hook
{
	private delegate AssetBundleCreateRequest OriginalMethod(string path, uint crc, ulong offset);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundle != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromFileAsync_Internal", typeof(string), typeof(uint), typeof(ulong)) ?? AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromFileAsync", typeof(string), typeof(uint), typeof(ulong));
	}

	private static bool Prefix(ref string path, ref uint crc, ref ulong offset, ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state)
	{
		AssetBundleLoadingParameters parameters = new AssetBundleLoadingParameters(null, path, crc, offset, null, 0u, AssetBundleLoadType.LoadFromFile);
		__state = ResourceRedirection.Hook_AssetBundleLoading_Prefix(parameters, out __result);
		AssetBundleLoadingParameters parameters2 = __state.Parameters;
		path = parameters2.Path;
		crc = parameters2.Crc;
		offset = parameters2.Offset;
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

	private static AssetBundleCreateRequest MM_Detour(string path, uint crc, ulong offset)
	{
		AssetBundleCreateRequest __result = null;
		AsyncAssetBundleLoadingContext __state = null;
		if (Prefix(ref path, ref crc, ref offset, ref __result, ref __state))
		{
			__result = _original(path, crc, offset);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
