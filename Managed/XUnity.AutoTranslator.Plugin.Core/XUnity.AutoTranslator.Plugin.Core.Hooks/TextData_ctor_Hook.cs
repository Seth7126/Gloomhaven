using System;
using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

[HookingHelperPriority(0)]
internal static class TextData_ctor_Hook
{
	private static Action<object, string> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TextData != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return UnityTypes.TextData.ClrType.GetConstructor(new Type[1] { typeof(string) });
	}

	private static void Postfix(object __instance, string text)
	{
		__instance.SetExtensionData(text);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<object, string>>();
	}

	private static void MM_Detour(object __instance, string text)
	{
		_original(__instance, text);
		Postfix(__instance, text);
	}
}
