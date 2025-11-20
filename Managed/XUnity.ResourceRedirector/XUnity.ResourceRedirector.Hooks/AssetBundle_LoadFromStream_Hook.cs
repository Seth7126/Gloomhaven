using System.IO;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_LoadFromStream_Hook
{
	private delegate AssetBundle OriginalMethod(Stream stream, uint crc, uint managedReadBufferSize);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundle != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromStreamInternal", typeof(Stream), typeof(uint), typeof(uint)) ?? AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadFromStream", typeof(Stream), typeof(uint), typeof(uint));
	}

	private static bool Prefix(ref Stream stream, ref uint crc, ref uint managedReadBufferSize, ref AssetBundle __result, ref AssetBundleLoadingContext __state)
	{
		AssetBundleLoadingParameters parameters = new AssetBundleLoadingParameters(null, null, crc, 0uL, stream, managedReadBufferSize, AssetBundleLoadType.LoadFromMemory);
		__state = ResourceRedirection.Hook_AssetBundleLoading_Prefix(parameters, out __result);
		AssetBundleLoadingParameters parameters2 = __state.Parameters;
		stream = parameters2.Stream;
		crc = parameters2.Crc;
		managedReadBufferSize = parameters2.ManagedReadBufferSize;
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

	private static AssetBundle MM_Detour(Stream stream, uint crc, uint managedReadBufferSize)
	{
		AssetBundle __result = null;
		AssetBundleLoadingContext __state = null;
		if (Prefix(ref stream, ref crc, ref managedReadBufferSize, ref __result, ref __state))
		{
			__result = _original(stream, crc, managedReadBufferSize);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
