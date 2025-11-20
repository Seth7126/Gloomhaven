using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_LoadFromMemory_Hook
{
	private delegate AssetBundle OriginalMethod(byte[] binary, uint crc);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundle != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromMemory_Internal", typeof(byte[]), typeof(uint)) ?? AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromMemory", typeof(byte[]), typeof(uint));
	}

	private static bool Prefix(ref byte[] binary, ref uint crc, ref AssetBundle __result, ref AssetBundleLoadingContext __state)
	{
		AssetBundleLoadingParameters parameters = new AssetBundleLoadingParameters(binary, null, crc, 0uL, null, 0u, AssetBundleLoadType.LoadFromMemory);
		__state = ResourceRedirection.Hook_AssetBundleLoading_Prefix(parameters, out __result);
		AssetBundleLoadingParameters parameters2 = __state.Parameters;
		binary = parameters2.Binary;
		crc = parameters2.Crc;
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

	private static AssetBundle MM_Detour(byte[] binary, uint crc)
	{
		AssetBundle __result = null;
		AssetBundleLoadingContext __state = null;
		if (Prefix(ref binary, ref crc, ref __result, ref __state))
		{
			__result = _original(binary, crc);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
