using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using MonoMod.Utils.Cil;

namespace MonoMod.RuntimeDetour;

public static class HarmonyDetourBridge
{
	public enum Type
	{
		Auto,
		Basic,
		AsOriginal,
		Override
	}

	private class DetourToRDAttribute : Attribute
	{
		public string Type { get; }

		public int SkipParams { get; }

		public string Name { get; }

		public DetourToRDAttribute(string type, int skipParams = 0, string name = null)
		{
			Type = type;
			SkipParams = skipParams;
			Name = name;
		}
	}

	private class DetourToHAttribute : Attribute
	{
		public string Type { get; }

		public int SkipParams { get; }

		public string Name { get; }

		public DetourToHAttribute(string type, int skipParams = 0, string name = null)
		{
			Type = type;
			SkipParams = skipParams;
			Name = name;
		}
	}

	private class TranspileAttribute : Attribute
	{
		public string Type { get; }

		public string Name { get; }

		public TranspileAttribute(string type, string name = null)
		{
			Type = type;
			Name = name;
		}
	}

	private class CriticalAttribute : Attribute
	{
	}

	private static Type CurrentType;

	private static Assembly _HarmonyASM;

	private static readonly HashSet<IDisposable> _Detours;

	private static readonly Dictionary<System.Type, MethodInfo> _Emitters;

	[ThreadStatic]
	private static DynamicMethodDefinition _LastWrapperDMD;

	private static Assembly _SharedStateASM;

	private static DetourConfig _DetourConfig;

	public static bool Initialized { get; private set; }

	static HarmonyDetourBridge()
	{
		_Detours = new HashSet<IDisposable>();
		_Emitters = new Dictionary<System.Type, MethodInfo>();
		System.Type typeFromHandle = typeof(System.Reflection.Emit.OpCode);
		System.Type proxyType = ILGeneratorShim.GetProxyType<CecilILGenerator>();
		MethodInfo[] methods = proxyType.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.Name != "Emit")
			{
				continue;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length == 2 && parameters[0].ParameterType == typeFromHandle)
			{
				System.Type parameterType = parameters[1].ParameterType;
				if (!_Emitters.ContainsKey(parameterType) || methodInfo.DeclaringType == proxyType)
				{
					_Emitters[parameterType] = methodInfo;
				}
			}
		}
	}

	public static bool Init(bool forceLoad = true, Type type = Type.Auto)
	{
		if (_HarmonyASM == null)
		{
			_HarmonyASM = _FindHarmony();
		}
		if (_HarmonyASM == null && forceLoad)
		{
			_HarmonyASM = Assembly.Load(new AssemblyName
			{
				Name = "0Harmony"
			});
		}
		if (_HarmonyASM == null)
		{
			return false;
		}
		if (Initialized)
		{
			return true;
		}
		Initialized = true;
		if (type == Type.Auto)
		{
			type = Type.AsOriginal;
		}
		_DetourConfig = new DetourConfig
		{
			Priority = type switch
			{
				Type.Override => 536870911, 
				Type.AsOriginal => -536870912, 
				_ => 0, 
			}
		};
		CurrentType = type;
		try
		{
			MethodInfo[] methods = typeof(HarmonyDetourBridge).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				bool flag = methodInfo.GetCustomAttributes(typeof(CriticalAttribute), inherit: false).Any();
				object[] customAttributes = methodInfo.GetCustomAttributes(typeof(DetourToRDAttribute), inherit: false);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					DetourToRDAttribute detourToRDAttribute = (DetourToRDAttribute)customAttributes[j];
					foreach (MethodInfo item in GetHarmonyMethod(methodInfo, detourToRDAttribute.Type, detourToRDAttribute.SkipParams, detourToRDAttribute.Name))
					{
						flag = false;
						_Detours.Add(new Hook(item, methodInfo));
					}
				}
				customAttributes = methodInfo.GetCustomAttributes(typeof(DetourToHAttribute), inherit: false);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					DetourToHAttribute detourToHAttribute = (DetourToHAttribute)customAttributes[j];
					foreach (MethodInfo item2 in GetHarmonyMethod(methodInfo, detourToHAttribute.Type, detourToHAttribute.SkipParams, detourToHAttribute.Name))
					{
						flag = false;
						_Detours.Add(new Detour(methodInfo, item2));
					}
				}
				customAttributes = methodInfo.GetCustomAttributes(typeof(TranspileAttribute), inherit: false);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					TranspileAttribute transpileAttribute = (TranspileAttribute)customAttributes[j];
					foreach (MethodInfo item3 in GetHarmonyMethod(methodInfo, transpileAttribute.Type, -1, transpileAttribute.Name))
					{
						using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(item3);
						flag = false;
						ILContext iLContext = new ILContext(dynamicMethodDefinition.Definition)
						{
							ReferenceBag = RuntimeILReferenceBag.Instance
						};
						_Detours.Add(iLContext);
						iLContext.Invoke((ILContext.Manipulator)methodInfo.CreateDelegate<ILContext.Manipulator>());
						if (iLContext.IsReadOnly)
						{
							iLContext.Dispose();
							_Detours.Remove(iLContext);
						}
						else
						{
							_Detours.Add(new Detour(item3, dynamicMethodDefinition.Generate()));
						}
					}
				}
				if (flag)
				{
					throw new Exception("Cannot apply HarmonyDetourBridge rule " + methodInfo.Name);
				}
			}
		}
		catch
		{
			_EarlyReset();
			throw;
		}
		return true;
	}

	private static bool _EarlyReset()
	{
		foreach (IDisposable detour in _Detours)
		{
			detour.Dispose();
		}
		_Detours.Clear();
		return false;
	}

	public static void Reset()
	{
		if (Initialized)
		{
			Initialized = false;
			_EarlyReset();
		}
	}

	private static System.Type GetHarmonyType(string typeName)
	{
		return _HarmonyASM.GetType(typeName) ?? _HarmonyASM.GetType("HarmonyLib." + typeName) ?? _HarmonyASM.GetType("Harmony." + typeName) ?? _HarmonyASM.GetType("Harmony.ILCopying." + typeName);
	}

	private static IEnumerable<MethodInfo> GetHarmonyMethod(MethodInfo ctx, string typeName, int skipParams, string name)
	{
		System.Type harmonyType = GetHarmonyType(typeName);
		if (harmonyType == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(name))
		{
			name = ctx.Name;
		}
		if (skipParams < 0)
		{
			return from method in harmonyType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where method.Name == name
				select method;
		}
		return new MethodInfo[1] { harmonyType.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, (from p in ctx.GetParameters().Skip(skipParams)
			select p.ParameterType).ToArray(), null) };
	}

	private static DynamicMethodDefinition CreateDMD(MethodBase original, string suffix)
	{
		if (original == null)
		{
			throw new ArgumentNullException("original");
		}
		ParameterInfo[] parameters = original.GetParameters();
		System.Type[] array;
		if (!original.IsStatic)
		{
			array = new System.Type[parameters.Length + 1];
			array[0] = original.GetThisParamType();
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i + 1] = parameters[i].ParameterType;
			}
		}
		else
		{
			array = new System.Type[parameters.Length];
			for (int j = 0; j < parameters.Length; j++)
			{
				array[j] = parameters[j].ParameterType;
			}
		}
		return new DynamicMethodDefinition((original.Name + suffix).Replace("<>", ""), (original as MethodInfo)?.ReturnType ?? typeof(void), array);
	}

	[DetourToRD("Memory", 0, null)]
	private static long GetMethodStart(MethodBase method, out Exception exception)
	{
		exception = null;
		try
		{
			_Detours.Add(new LazyDisposable<MethodBase>(method, delegate(MethodBase m)
			{
				m.Unpin();
			}));
			return (long)method.Pin().GetNativeStart();
		}
		catch (Exception ex)
		{
			exception = ex;
			return 0L;
		}
	}

	[DetourToRD("Memory", 0, null)]
	[Critical]
	private static string WriteJump(long memory, long destination)
	{
		_Detours.Add(new NativeDetour((IntPtr)memory, (IntPtr)destination));
		return null;
	}

	[DetourToRD("Memory", 0, null)]
	[Critical]
	private static string DetourMethod(MethodBase original, MethodBase replacement)
	{
		if (replacement == null)
		{
			replacement = _LastWrapperDMD.Generate();
			_LastWrapperDMD.Dispose();
			_LastWrapperDMD = null;
		}
		_Detours.Add(new Detour(original, replacement, ref _DetourConfig));
		return null;
	}

	[DetourToRD("MethodBodyReader", 1, null)]
	private static MethodInfo EmitMethodForType(object self, System.Type type)
	{
		foreach (KeyValuePair<System.Type, MethodInfo> emitter in _Emitters)
		{
			if (emitter.Key == type)
			{
				return emitter.Value;
			}
		}
		foreach (KeyValuePair<System.Type, MethodInfo> emitter2 in _Emitters)
		{
			if (emitter2.Key.IsAssignableFrom(type))
			{
				return emitter2.Value;
			}
		}
		return null;
	}

	[DetourToRD("PatchProcessor", 2, null)]
	[Critical]
	private static List<DynamicMethod> Patch(Func<object, List<DynamicMethod>> orig, object self)
	{
		orig(self);
		return new List<DynamicMethod>();
	}

	[Transpile("PatchFunctions", null)]
	[Critical]
	private static void UpdateWrapper(ILContext il)
	{
		ILCursor iLCursor = new ILCursor(il);
		iLCursor.GotoNext((Instruction i) => i.MatchThrow());
		iLCursor.Next.OpCode = Mono.Cecil.Cil.OpCodes.Pop;
	}

	[Transpile("MethodPatcher", null)]
	[Critical]
	private static void CreatePatchedMethod(ILContext il)
	{
		ILCursor iLCursor = new ILCursor(il);
		System.Type t_DynamicTools = GetHarmonyType("DynamicTools");
		if (!iLCursor.TryGotoNext((Instruction i) => i.MatchCall(t_DynamicTools, "CreateDynamicMethod")))
		{
			il.MakeReadOnly();
			return;
		}
		iLCursor.Next.OpCode = Mono.Cecil.Cil.OpCodes.Call;
		iLCursor.Next.Operand = il.Import(typeof(HarmonyDetourBridge).GetMethod("CreateDMD", BindingFlags.Static | BindingFlags.NonPublic));
		int varDMDi = -1;
		iLCursor.GotoNext((Instruction i) => i.MatchStloc(out varDMDi));
		il.Body.Variables[varDMDi].VariableType = il.Import(typeof(DynamicMethodDefinition));
		iLCursor.GotoNext((Instruction i) => i.MatchCallvirt<DynamicMethod>("GetILGenerator"));
		iLCursor.Next.OpCode = Mono.Cecil.Cil.OpCodes.Call;
		iLCursor.Next.Operand = il.Import(typeof(DynamicMethodDefinition).GetMethod("GetILGenerator", BindingFlags.Instance | BindingFlags.Public));
		iLCursor.GotoNext((Instruction i) => i.MatchCall(t_DynamicTools, "PrepareDynamicMethod"));
		iLCursor.Next.OpCode = Mono.Cecil.Cil.OpCodes.Pop;
		iLCursor.Next.Operand = null;
		iLCursor.GotoNext((Instruction i) => i.MatchLdloc(varDMDi));
		iLCursor.Index++;
		iLCursor.EmitDelegate<Func<DynamicMethodDefinition, DynamicMethod>>(delegate(DynamicMethodDefinition dmd)
		{
			_LastWrapperDMD = dmd;
			return (DynamicMethod)null;
		});
	}

	[DetourToRD("HarmonySharedState", 1, null)]
	private static Assembly SharedStateAssembly(Func<Assembly> orig)
	{
		Assembly assembly = orig();
		if (assembly != null)
		{
			return assembly;
		}
		if (_SharedStateASM != null)
		{
			return _SharedStateASM;
		}
		string text = (string)GetHarmonyType("HarmonySharedState").GetField("name", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
		using ModuleDefinition moduleDefinition = ModuleDefinition.CreateModule("MonoMod.RuntimeDetour." + text, new ModuleParameters
		{
			Kind = ModuleKind.Dll,
			ReflectionImporterProvider = MMReflectionImporter.Provider
		});
		TypeDefinition typeDefinition = new TypeDefinition("", text, Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Abstract | Mono.Cecil.TypeAttributes.Sealed)
		{
			BaseType = moduleDefinition.TypeSystem.Object
		};
		moduleDefinition.Types.Add(typeDefinition);
		typeDefinition.Fields.Add(new FieldDefinition("state", Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static, moduleDefinition.ImportReference(typeof(Dictionary<MethodBase, byte[]>))));
		typeDefinition.Fields.Add(new FieldDefinition("version", Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static, moduleDefinition.ImportReference(typeof(int))));
		return _SharedStateASM = ReflectionHelper.Load(moduleDefinition);
	}

	private static Assembly _FindHarmony()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.GetName().Name == "0Harmony" || assembly.GetName().Name == "Harmony" || assembly.GetType("Harmony.HarmonyInstance") != null || assembly.GetType("HarmonyLib.Harmony") != null)
			{
				return assembly;
			}
		}
		object obj = System.Type.GetType("Harmony.HarmonyInstance", throwOnError: false, ignoreCase: false)?.Assembly;
		if (obj == null)
		{
			System.Type type = System.Type.GetType("HarmonyLib.Harmony", throwOnError: false, ignoreCase: false);
			if (type == null)
			{
				return null;
			}
			obj = type.Assembly;
		}
		return (Assembly)obj;
	}
}
