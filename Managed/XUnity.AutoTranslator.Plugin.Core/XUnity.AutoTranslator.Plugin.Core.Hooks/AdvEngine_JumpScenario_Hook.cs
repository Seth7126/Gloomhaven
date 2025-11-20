using System;
using System.Reflection;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class AdvEngine_JumpScenario_Hook
{
	private static Action<object, string> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AdvEngine != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AdvEngine?.ClrType, "JumpScenario", typeof(string));
	}

	private static void Prefix(ref string label)
	{
		UtageHelper.FixLabel(ref label);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<object, string>>();
	}

	private static void MM_Detour(object __instance, string value)
	{
		Prefix(ref value);
		_original(__instance, value);
	}
}
