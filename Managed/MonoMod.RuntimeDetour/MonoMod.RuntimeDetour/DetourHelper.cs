using System;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour.Platforms;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public static class DetourHelper
{
	private static readonly object _RuntimeLock = new object();

	private static IDetourRuntimePlatform _Runtime;

	private static readonly object _NativeLock = new object();

	private static IDetourNativePlatform _Native;

	private static readonly FieldInfo _f_Native = typeof(DetourHelper).GetField("_Native", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly MethodInfo _m_ToNativeDetourData = typeof(DetourHelper).GetMethod("ToNativeDetourData", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly MethodInfo _m_Copy = typeof(IDetourNativePlatform).GetMethod("Copy");

	private static readonly MethodInfo _m_Apply = typeof(IDetourNativePlatform).GetMethod("Apply");

	private static readonly ConstructorInfo _ctor_Exception = typeof(Exception).GetConstructor(new Type[1] { typeof(string) });

	public static IDetourRuntimePlatform Runtime
	{
		get
		{
			if (_Runtime != null)
			{
				return _Runtime;
			}
			lock (_RuntimeLock)
			{
				if (_Runtime != null)
				{
					return _Runtime;
				}
				if (Type.GetType("Mono.Runtime") != null)
				{
					_Runtime = new DetourRuntimeMonoPlatform();
				}
				else
				{
					_Runtime = new DetourRuntimeNETPlatform();
				}
				return _Runtime;
			}
		}
		set
		{
			_Runtime = value;
		}
	}

	public static IDetourNativePlatform Native
	{
		get
		{
			if (_Native != null)
			{
				return _Native;
			}
			lock (_NativeLock)
			{
				if (_Native != null)
				{
					return _Native;
				}
				if (PlatformHelper.Is(Platform.ARM))
				{
					_Native = new DetourNativeARMPlatform();
				}
				else
				{
					_Native = new DetourNativeX86Platform();
				}
				if (PlatformHelper.Is(Platform.Windows))
				{
					return _Native = new DetourNativeWindowsPlatform(_Native);
				}
				if (Type.GetType("Mono.Runtime") != null)
				{
					try
					{
						return _Native = new DetourNativeMonoPlatform(_Native, "libmonosgen-2.0." + PlatformHelper.LibrarySuffix);
					}
					catch
					{
					}
				}
				try
				{
					_Native = new DetourNativeMonoPosixPlatform(_Native);
				}
				catch
				{
				}
				try
				{
					_Native = new DetourNativeLibcPlatform(_Native);
				}
				catch
				{
				}
				return _Native;
			}
		}
		set
		{
			_Native = value;
		}
	}

	public static void MakeWritable(this IDetourNativePlatform plat, NativeDetourData detour)
	{
		plat.MakeWritable(detour.Method, detour.Size);
	}

	public static void MakeExecutable(this IDetourNativePlatform plat, NativeDetourData detour)
	{
		plat.MakeExecutable(detour.Method, detour.Size);
	}

	public static void FlushICache(this IDetourNativePlatform plat, NativeDetourData detour)
	{
		plat.FlushICache(detour.Method, detour.Size);
	}

	public unsafe static void Write(this IntPtr to, ref int offs, byte value)
	{
		*(byte*)((long)to + offs) = value;
		offs++;
	}

	public unsafe static void Write(this IntPtr to, ref int offs, ushort value)
	{
		*(ushort*)((long)to + offs) = value;
		offs += 2;
	}

	public unsafe static void Write(this IntPtr to, ref int offs, uint value)
	{
		*(uint*)((long)to + offs) = value;
		offs += 4;
	}

	public unsafe static void Write(this IntPtr to, ref int offs, ulong value)
	{
		*(ulong*)((long)to + offs) = value;
		offs += 8;
	}

	public static IntPtr GetNativeStart(this MethodBase method)
	{
		return Runtime.GetNativeStart(method);
	}

	public static IntPtr GetNativeStart(this Delegate method)
	{
		return method.Method.GetNativeStart();
	}

	public static IntPtr GetNativeStart(this Expression method)
	{
		return ((MethodCallExpression)method).Method.GetNativeStart();
	}

	public static MethodInfo CreateILCopy(this MethodBase method)
	{
		return Runtime.CreateCopy(method);
	}

	public static bool TryCreateILCopy(this MethodBase method, out MethodInfo dm)
	{
		return Runtime.TryCreateCopy(method, out dm);
	}

	public static T Pin<T>(this T method) where T : MethodBase
	{
		Runtime.Pin(method);
		return method;
	}

	public static T Unpin<T>(this T method) where T : MethodBase
	{
		Runtime.Unpin(method);
		return method;
	}

	public static MethodInfo GenerateNativeProxy(IntPtr target, MethodBase signature)
	{
		Type returnType = (signature as MethodInfo)?.ReturnType ?? typeof(void);
		ParameterInfo[] parameters = signature.GetParameters();
		Type[] array = new Type[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		MethodInfo methodInfo;
		using (DynamicMethodDefinition dm = new DynamicMethodDefinition("Native<" + ((long)target).ToString("X16") + ">", returnType, array))
		{
			methodInfo = dm.StubCriticalDetour().Generate().Pin();
		}
		NativeDetourData detour = Native.Create(methodInfo.GetNativeStart(), target);
		Native.MakeWritable(detour);
		Native.Apply(detour);
		Native.MakeExecutable(detour);
		Native.FlushICache(detour);
		Native.Free(detour);
		return methodInfo;
	}

	private static NativeDetourData ToNativeDetourData(IntPtr method, IntPtr target, uint size, byte type, IntPtr extra)
	{
		return new NativeDetourData
		{
			Method = method,
			Target = target,
			Size = size,
			Type = type,
			Extra = extra
		};
	}

	public static DynamicMethodDefinition StubCriticalDetour(this DynamicMethodDefinition dm)
	{
		ILProcessor iLProcessor = dm.GetILProcessor();
		ModuleDefinition module = iLProcessor.Body.Method.Module;
		for (int i = 0; i < 10; i++)
		{
			iLProcessor.Emit(OpCodes.Nop);
		}
		iLProcessor.Emit(OpCodes.Ldstr, dm.Definition.Name + " should've been detoured!");
		iLProcessor.Emit(OpCodes.Newobj, module.ImportReference(_ctor_Exception));
		iLProcessor.Emit(OpCodes.Throw);
		return dm;
	}

	public static void EmitDetourCopy(this ILProcessor il, IntPtr src, IntPtr dst, byte type)
	{
		ModuleDefinition module = il.Body.Method.Module;
		il.Emit(OpCodes.Ldsfld, module.ImportReference(_f_Native));
		il.Emit(OpCodes.Ldc_I8, (long)src);
		il.Emit(OpCodes.Conv_I);
		il.Emit(OpCodes.Ldc_I8, (long)dst);
		il.Emit(OpCodes.Conv_I);
		il.Emit(OpCodes.Ldc_I4, (int)type);
		il.Emit(OpCodes.Conv_U1);
		il.Emit(OpCodes.Callvirt, module.ImportReference(_m_Copy));
	}

	public static void EmitDetourApply(this ILProcessor il, NativeDetourData data)
	{
		ModuleDefinition module = il.Body.Method.Module;
		il.Emit(OpCodes.Ldsfld, module.ImportReference(_f_Native));
		il.Emit(OpCodes.Ldc_I8, (long)data.Method);
		il.Emit(OpCodes.Conv_I);
		il.Emit(OpCodes.Ldc_I8, (long)data.Target);
		il.Emit(OpCodes.Conv_I);
		il.Emit(OpCodes.Ldc_I4, (int)data.Size);
		il.Emit(OpCodes.Ldc_I4, (int)data.Type);
		il.Emit(OpCodes.Conv_U1);
		il.Emit(OpCodes.Ldc_I8, (long)data.Extra);
		il.Emit(OpCodes.Conv_I);
		il.Emit(OpCodes.Call, module.ImportReference(_m_ToNativeDetourData));
		il.Emit(OpCodes.Callvirt, module.ImportReference(_m_Apply));
	}
}
