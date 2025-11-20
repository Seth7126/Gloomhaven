using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_LoadFromFile_Hook
{
	private delegate AssetBundle OriginalMethod(string path, uint crc, ulong offset);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundle != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromFile_Internal", typeof(string), typeof(uint), typeof(ulong)) ?? AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromFile", typeof(string), typeof(uint), typeof(ulong));
	}

	private static bool Prefix(ref string path, ref uint crc, ref ulong offset, ref AssetBundle __result, ref AssetBundleLoadingContext __state)
	{
		AssetBundleLoadingParameters parameters = new AssetBundleLoadingParameters(null, path, crc, offset, null, 0u, AssetBundleLoadType.LoadFromFile);
		__state = ResourceRedirection.Hook_AssetBundleLoading_Prefix(parameters, out __result);
		AssetBundleLoadingParameters parameters2 = __state.Parameters;
		path = parameters2.Path;
		crc = parameters2.Crc;
		offset = parameters2.Offset;
		return !__state.SkipOriginalCall;
	}

	private static void Postfix(ref AssetBundle __result, ref AssetBundleLoadingContext __state)
	{
		if (!__state.SkipAllPostfixes)
		{
			ResourceRedirection.Hook_AssetBundleLoaded_Postfix(__state.Parameters, ref __result);
		}
		if ((Object)(object)__result != (Object)null && __state.Parameters.Path != null)
		{
			__result.GetOrCreateExtensionData<AssetBundleExtensionData>().Path = __state.Parameters.Path;
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static AssetBundle MM_Detour(string path, uint crc, ulong offset)
	{
		AssetBundle __result = null;
		AssetBundleLoadingContext __state = null;
		if (Prefix(ref path, ref crc, ref offset, ref __result, ref __state))
		{
			__result = _original(path, crc, offset);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
