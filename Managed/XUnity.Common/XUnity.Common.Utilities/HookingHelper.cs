using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.Common.Utilities;

public static class HookingHelper
{
	private static readonly MethodInfo PatchMethod12;

	private static readonly MethodInfo PatchMethod20;

	private static readonly object Harmony;

	private static bool _loggedHarmonyError;

	static HookingHelper()
	{
		PatchMethod12 = ClrTypes.HarmonyInstance?.GetMethod("Patch", new Type[4]
		{
			ClrTypes.MethodBase,
			ClrTypes.HarmonyMethod,
			ClrTypes.HarmonyMethod,
			ClrTypes.HarmonyMethod
		});
		PatchMethod20 = ClrTypes.Harmony?.GetMethod("Patch", new Type[5]
		{
			ClrTypes.MethodBase,
			ClrTypes.HarmonyMethod,
			ClrTypes.HarmonyMethod,
			ClrTypes.HarmonyMethod,
			ClrTypes.HarmonyMethod
		});
		_loggedHarmonyError = false;
		try
		{
			if (ClrTypes.HarmonyInstance != null)
			{
				Harmony = ClrTypes.HarmonyInstance.GetMethod("Create", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[1] { "xunity.common.hookinghelper" });
			}
			else if (ClrTypes.Harmony != null)
			{
				Harmony = ClrTypes.Harmony.GetConstructor(new Type[1] { typeof(string) }).Invoke(new object[1] { "xunity.common.hookinghelper" });
			}
			else
			{
				XuaLogger.Common.Error("An unexpected exception occurred during harmony initialization, likely caused by unknown Harmony version. Harmony hooks will be unavailable!");
			}
		}
		catch (Exception e)
		{
			XuaLogger.Common.Error(e, "An unexpected exception occurred during harmony initialization. Harmony hooks will be unavailable!");
		}
	}

	public static void PatchAll(IEnumerable<Type> types, bool forceExternHooks)
	{
		foreach (Type type in types)
		{
			PatchType(type, forceExternHooks);
		}
	}

	public static void PatchAll(IEnumerable<Type[]> types, bool forceMonoModHooks)
	{
		foreach (Type[] type in types)
		{
			for (int i = 0; i < type.Length && !PatchType(type[i], forceMonoModHooks); i++)
			{
			}
		}
	}

	public static bool PatchType(Type type, bool forceExternHooks)
	{
		MethodBase methodBase = null;
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			if (Harmony == null && !_loggedHarmonyError)
			{
				_loggedHarmonyError = true;
				XuaLogger.Common.Warn("Harmony is not loaded or could not be initialized. Using fallback hooks instead.");
			}
			BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			MethodInfo method = type.GetMethod("Prepare", bindingAttr);
			if (method == null || (bool)method.Invoke(null, new object[1] { Harmony }))
			{
				try
				{
					methodBase = (MethodBase)(type.GetMethod("TargetMethod", bindingAttr)?.Invoke(null, new object[1] { Harmony }));
				}
				catch
				{
				}
				try
				{
					intPtr = ((IntPtr?)type.GetMethod("TargetMethodPointer", bindingAttr)?.Invoke(null, null)) ?? IntPtr.Zero;
				}
				catch
				{
				}
				if (methodBase == null && intPtr == IntPtr.Zero)
				{
					if (methodBase != null)
					{
						XuaLogger.Common.Warn("Could not hook '" + methodBase.DeclaringType.FullName + "." + methodBase.Name + "'. Likely due differences between different versions of the engine or text framework.");
					}
					else
					{
						XuaLogger.Common.Warn("Could not hook '" + type.Name + "'. Likely due differences between different versions of the engine or text framework.");
					}
					return false;
				}
				MethodInfo method2 = type.GetMethod("Prefix", bindingAttr);
				MethodInfo method3 = type.GetMethod("Postfix", bindingAttr);
				MethodInfo method4 = type.GetMethod("Finalizer", bindingAttr);
				if (methodBase == null || forceExternHooks || Harmony == null || (method2 == null && method3 == null && method4 == null))
				{
					return PatchWithExternHooks(type, methodBase, intPtr, forced: true);
				}
				if (methodBase != null)
				{
					try
					{
						int? priority = type.GetCustomAttributes(typeof(HookingHelperPriorityAttribute), inherit: false).OfType<HookingHelperPriorityAttribute>().FirstOrDefault()?.priority;
						object obj3 = ((method2 != null) ? CreateHarmonyMethod(method2, priority) : null);
						object obj4 = ((method3 != null) ? CreateHarmonyMethod(method3, priority) : null);
						object obj5 = ((method4 != null) ? CreateHarmonyMethod(method4, priority) : null);
						if (PatchMethod12 != null)
						{
							PatchMethod12.Invoke(Harmony, new object[4] { methodBase, obj3, obj4, null });
						}
						else
						{
							PatchMethod20.Invoke(Harmony, new object[5] { methodBase, obj3, obj4, null, obj5 });
						}
						XuaLogger.Common.Debug("Hooked " + methodBase.DeclaringType.FullName + "." + methodBase.Name + " through Harmony hooks.");
						return true;
					}
					catch (Exception e) when (((Func<bool>)delegate
					{
						// Could not convert BlockContainer to single expression
						System.Runtime.CompilerServices.Unsafe.SkipInit(out int num);
						if (e.FirstInnerExceptionOfType<PlatformNotSupportedException>() == null)
						{
							ArgumentException ex = e.FirstInnerExceptionOfType<ArgumentException>();
							num = ((ex != null && ex.Message?.Contains("no body") == true) ? 1 : 0);
						}
						else
						{
							num = 1;
						}
						return num != 0;
					}).Invoke())
					{
						return PatchWithExternHooks(type, methodBase, intPtr, forced: false);
					}
				}
				XuaLogger.Common.Warn("Could not hook '" + type.Name + "'. Likely due differences between different versions of the engine or text framework.");
			}
		}
		catch (Exception e2)
		{
			if (methodBase != null)
			{
				XuaLogger.Common.Warn(e2, "An error occurred while patching property/method '" + methodBase.DeclaringType.FullName + "." + methodBase.Name + "'. Failing hook: '" + type.Name + "'.");
			}
			else
			{
				XuaLogger.Common.Warn(e2, "An error occurred while patching property/method. Failing hook: '" + type.Name + "'.");
			}
		}
		return false;
	}

	private static bool PatchWithExternHooks(Type type, MethodBase original, IntPtr originalPtr, bool forced)
	{
		BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		if (ClrTypes.Imports != null)
		{
			if (originalPtr == IntPtr.Zero)
			{
				XuaLogger.Common.Warn("Could not hook '" + type.Name + "'. Likely due differences between different versions of the engine or text framework.");
				return false;
			}
			IntPtr? intPtr = type.GetMethod("ML_Detour", bindingAttr)?.MethodHandle.GetFunctionPointer();
			if (intPtr.HasValue && intPtr.Value != IntPtr.Zero)
			{
				ClrTypes.Imports.GetMethod("Hook", bindingAttr).Invoke(null, new object[2] { originalPtr, intPtr.Value });
				XuaLogger.Common.Debug("Hooked " + type.Name + " through MelonMod Imports.Hook method.");
				return true;
			}
			XuaLogger.Common.Warn("Could not hook '" + type.Name + "' because no detour method was found.");
		}
		else
		{
			if (original == null)
			{
				XuaLogger.Common.Warn("Cannot hook '" + type.Name + "'. Could not locate the original method. Failing hook: '" + type.Name + "'.");
				return false;
			}
			if (ClrTypes.Hook == null || ClrTypes.NativeDetour == null)
			{
				XuaLogger.Common.Warn("Cannot hook '" + original.DeclaringType.FullName + "." + original.Name + "'. MonoMod hooks is not supported in this runtime as MonoMod is not loaded. Failing hook: '" + type.Name + "'.");
				return false;
			}
			object obj = type.GetMethod("Get_MM_Detour", bindingAttr)?.Invoke(null, null) ?? type.GetMethod("MM_Detour", bindingAttr);
			if (obj != null)
			{
				string text = "(managed)";
				object obj2;
				try
				{
					obj2 = ClrTypes.Hook.GetConstructor(new Type[2]
					{
						typeof(MethodBase),
						typeof(MethodInfo)
					}).Invoke(new object[2] { original, obj });
					obj2.GetType().GetMethod("Apply").Invoke(obj2, null);
				}
				catch (Exception e) when (((Func<bool>)delegate
				{
					// Could not convert BlockContainer to single expression
					System.Runtime.CompilerServices.Unsafe.SkipInit(out int num);
					if (e.FirstInnerExceptionOfType<NullReferenceException>() == null)
					{
						NotSupportedException ex = e.FirstInnerExceptionOfType<NotSupportedException>();
						num = ((ex != null && ex.Message?.Contains("Body-less") == true) ? 1 : 0);
					}
					else
					{
						num = 1;
					}
					return num != 0;
				}).Invoke())
				{
					text = "(native)";
					obj2 = ClrTypes.NativeDetour.GetConstructor(new Type[2]
					{
						typeof(MethodBase),
						typeof(MethodBase)
					}).Invoke(new object[2] { original, obj });
					obj2.GetType().GetMethod("Apply").Invoke(obj2, null);
				}
				type.GetMethod("MM_Init", bindingAttr)?.Invoke(null, new object[1] { obj2 });
				if (forced)
				{
					XuaLogger.Common.Debug("Hooked " + original.DeclaringType.FullName + "." + original.Name + " through forced MonoMod hooks. " + text);
				}
				else
				{
					XuaLogger.Common.Debug("Hooked " + original.DeclaringType.FullName + "." + original.Name + " through MonoMod hooks. " + text);
				}
				return true;
			}
			if (forced)
			{
				XuaLogger.Common.Warn("Cannot hook '" + original.DeclaringType.FullName + "." + original.Name + "'. Harmony is not supported in this runtime and no alternate MonoMod hook has been implemented. Failing hook: '" + type.Name + "'.");
			}
			else
			{
				XuaLogger.Common.Warn("Cannot hook '" + original.DeclaringType.FullName + "." + original.Name + "'. Harmony is not supported in this runtime and no alternate MonoMod hook has been implemented. Failing hook: '" + type.Name + "'.");
			}
		}
		return false;
	}

	private static object CreateHarmonyMethod(MethodInfo method, int? priority)
	{
		object obj = ClrTypes.HarmonyMethod.GetConstructor(new Type[1] { typeof(MethodInfo) }).Invoke(new object[1] { method });
		if (priority.HasValue)
		{
			(ClrTypes.HarmonyMethod.GetField("priority", BindingFlags.Instance | BindingFlags.Public) ?? ClrTypes.HarmonyMethod.GetField("prioritiy", BindingFlags.Instance | BindingFlags.Public)).SetValue(obj, priority.Value);
		}
		return obj;
	}
}
