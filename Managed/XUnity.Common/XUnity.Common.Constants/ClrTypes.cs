using System;
using System.Reflection;

namespace XUnity.Common.Constants;

public static class ClrTypes
{
	public static readonly Type AccessTools = FindTypeStrict("Harmony.AccessTools, 0Harmony") ?? FindTypeStrict("HarmonyLib.AccessTools, 0Harmony") ?? FindTypeStrict("Harmony.AccessTools, MelonLoader.ModHandler") ?? FindTypeStrict("HarmonyLib.AccessTools, MelonLoader.ModHandler");

	public static readonly Type HarmonyMethod = FindTypeStrict("Harmony.HarmonyMethod, 0Harmony") ?? FindTypeStrict("HarmonyLib.HarmonyMethod, 0Harmony") ?? FindTypeStrict("Harmony.HarmonyMethod, MelonLoader.ModHandler") ?? FindTypeStrict("HarmonyLib.HarmonyMethod, MelonLoader.ModHandler");

	public static readonly Type HarmonyInstance = FindTypeStrict("Harmony.HarmonyInstance, 0Harmony") ?? FindTypeStrict("Harmony.HarmonyInstance, MelonLoader.ModHandler");

	public static readonly Type Harmony = FindTypeStrict("HarmonyLib.Harmony, 0Harmony") ?? FindTypeStrict("HarmonyLib.Harmony, MelonLoader.ModHandler");

	public static readonly Type Hook = FindTypeStrict("MonoMod.RuntimeDetour.Hook, MonoMod.RuntimeDetour");

	public static readonly Type Detour = FindTypeStrict("MonoMod.RuntimeDetour.Detour, MonoMod.RuntimeDetour");

	public static readonly Type NativeDetour = FindTypeStrict("MonoMod.RuntimeDetour.NativeDetour, MonoMod.RuntimeDetour");

	public static readonly Type DynamicMethodDefinition = FindTypeStrict("MonoMod.Utils.DynamicMethodDefinition, MonoMod.Utils");

	public static readonly Type Imports = FindTypeStrict("MelonLoader.Imports, MelonLoader.ModHandler");

	public static readonly Type MethodBase = FindType("System.Reflection.MethodBase");

	public static readonly Type Task = FindType("System.Threading.Tasks.Task");

	private static Type FindType(string name)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				Type type = assembly.GetType(name, throwOnError: false);
				if (type != null)
				{
					return type;
				}
			}
			catch
			{
			}
		}
		return null;
	}

	private static Type FindTypeStrict(string name)
	{
		return Type.GetType(name, throwOnError: false);
	}
}
