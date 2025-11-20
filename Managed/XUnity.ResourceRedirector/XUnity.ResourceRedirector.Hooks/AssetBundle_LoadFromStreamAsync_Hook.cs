using System.IO;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_LoadFromStreamAsync_Hook
{
	private delegate AssetBundleCreateRequest OriginalMethod(Stream stream, uint crc, uint managedReadBufferSize);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundle != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromStreamAsyncInternal", typeof(Stream), typeof(uint), typeof(uint)) ?? AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromStreamAsync", typeof(Stream), typeof(uint), typeof(uint));
	}

	private static bool Prefix(ref Stream stream, ref uint crc, ref uint managedReadBufferSize, ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state)
	{
		AssetBundleLoadingParameters parameters = new AssetBundleLoadingParameters(null, null, crc, 0uL, stream, managedReadBufferSize, AssetBundleLoadType.LoadFromMemory);
		__state = ResourceRedirection.Hook_AssetBundleLoading_Prefix(parameters, out __result);
		AssetBundleLoadingParameters parameters2 = __state.Parameters;
		stream = parameters2.Stream;
		crc = parameters2.Crc;
		managedReadBufferSize = parameters2.ManagedReadBufferSize;
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

	private static AssetBundleCreateRequest MM_Detour(Stream stream, uint crc, uint managedReadBufferSize)
	{
		AssetBundleCreateRequest __result = null;
		AsyncAssetBundleLoadingContext __state = null;
		if (Prefix(ref stream, ref crc, ref managedReadBufferSize, ref __result, ref __state))
		{
			__result = _original(stream, crc, managedReadBufferSize);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
