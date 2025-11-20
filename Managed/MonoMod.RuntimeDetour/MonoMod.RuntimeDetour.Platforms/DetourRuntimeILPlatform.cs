using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour.Platforms;

public abstract class DetourRuntimeILPlatform : IDetourRuntimePlatform
{
	private struct _SelftestStruct
	{
		private readonly byte A;

		private readonly byte B;

		private readonly byte C;
	}

	protected class MethodPin
	{
		public int Count;

		public RuntimeMethodHandle Handle;
	}

	private enum GlueThiscallStructRetPtrOrder
	{
		Original,
		ThisRetArgs,
		RetThisArgs
	}

	protected Dictionary<MethodBase, MethodPin> PinnedMethods = new Dictionary<MethodBase, MethodPin>();

	private readonly GlueThiscallStructRetPtrOrder GlueThiscallStructRetPtr;

	protected abstract RuntimeMethodHandle GetMethodHandle(MethodBase method);

	public unsafe DetourRuntimeILPlatform()
	{
		MethodInfo method = typeof(DetourRuntimeILPlatform).GetMethod("_SelftestGetRefPtr", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method2 = typeof(DetourRuntimeILPlatform).GetMethod("_SelftestGetRefPtrHook", BindingFlags.Static | BindingFlags.NonPublic);
		_HookSelftest(method, method2);
		IntPtr arg = ((Func<IntPtr>)Delegate.CreateDelegate(typeof(Func<IntPtr>), this, method))();
		MethodInfo method3 = typeof(DetourRuntimeILPlatform).GetMethod("_SelftestGetStruct", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method4 = typeof(DetourRuntimeILPlatform).GetMethod("_SelftestGetStructHook", BindingFlags.Static | BindingFlags.NonPublic);
		_HookSelftest(method3, method4);
		fixed (GlueThiscallStructRetPtrOrder* glueThiscallStructRetPtr = &GlueThiscallStructRetPtr)
		{
			((Func<IntPtr, IntPtr, IntPtr, _SelftestStruct>)Delegate.CreateDelegate(typeof(Func<IntPtr, IntPtr, IntPtr, _SelftestStruct>), this, method3))((IntPtr)glueThiscallStructRetPtr, (IntPtr)glueThiscallStructRetPtr, arg);
		}
	}

	private void _HookSelftest(MethodInfo from, MethodInfo to)
	{
		Pin(from);
		Pin(to);
		NativeDetourData detour = DetourHelper.Native.Create(GetNativeStart(from), GetNativeStart(to));
		DetourHelper.Native.MakeWritable(detour);
		DetourHelper.Native.Apply(detour);
		DetourHelper.Native.MakeExecutable(detour);
		DetourHelper.Native.FlushICache(detour);
		DetourHelper.Native.Free(detour);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private IntPtr _SelftestGetRefPtr()
	{
		Console.Error.WriteLine("If you're reading this, the MonoMod.RuntimeDetour selftest failed.");
		throw new Exception("This method should've been detoured!");
	}

	private static IntPtr _SelftestGetRefPtrHook(IntPtr self)
	{
		return self;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private _SelftestStruct _SelftestGetStruct(IntPtr x, IntPtr y, IntPtr thisPtr)
	{
		Console.Error.WriteLine("If you're reading this, the MonoMod.RuntimeDetour selftest failed.");
		throw new Exception("This method should've been detoured!");
	}

	private unsafe static void _SelftestGetStructHook(IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr e)
	{
		if (b == c)
		{
			*(int*)(void*)b = 0;
		}
		else if (b == e)
		{
			*(int*)(void*)c = 2;
		}
		else
		{
			*(int*)(void*)c = 1;
		}
	}

	protected virtual IntPtr GetFunctionPointer(RuntimeMethodHandle handle)
	{
		return handle.GetFunctionPointer();
	}

	protected virtual void PrepareMethod(RuntimeMethodHandle handle)
	{
		RuntimeHelpers.PrepareMethod(handle);
	}

	public IntPtr GetNativeStart(MethodBase method)
	{
		bool flag;
		MethodPin value;
		lock (PinnedMethods)
		{
			flag = PinnedMethods.TryGetValue(method, out value);
		}
		if (flag)
		{
			return GetFunctionPointer(value.Handle);
		}
		return GetFunctionPointer(GetMethodHandle(method));
	}

	public void Pin(MethodBase method)
	{
		lock (PinnedMethods)
		{
			if (PinnedMethods.TryGetValue(method, out var value))
			{
				value.Count++;
				return;
			}
			value = new MethodPin();
			value.Count = 1;
			PrepareMethod(value.Handle = GetMethodHandle(method));
			PinnedMethods[method] = value;
		}
	}

	public void Unpin(MethodBase method)
	{
		lock (PinnedMethods)
		{
			if (PinnedMethods.TryGetValue(method, out var value))
			{
				if (value.Count <= 1)
				{
					PinnedMethods.Remove(method);
				}
				else
				{
					value.Count--;
				}
			}
		}
	}

	public MethodInfo CreateCopy(MethodBase method)
	{
		if (method == null || (method.GetMethodImplementationFlags() & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL)
		{
			throw new InvalidOperationException("Uncopyable method: " + (method?.ToString() ?? "NULL"));
		}
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(method);
		return dynamicMethodDefinition.Generate();
	}

	public bool TryCreateCopy(MethodBase method, out MethodInfo dm)
	{
		if (method == null || (method.GetMethodImplementationFlags() & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL)
		{
			dm = null;
			return false;
		}
		try
		{
			dm = CreateCopy(method);
			return true;
		}
		catch
		{
			dm = null;
			return false;
		}
	}

	public MethodBase GetDetourTarget(MethodBase from, MethodBase to)
	{
		MethodInfo methodInfo = from as MethodInfo;
		MethodInfo methodInfo2 = to as MethodInfo;
		_ = to.DeclaringType;
		MethodInfo methodInfo3 = null;
		if (GlueThiscallStructRetPtr != GlueThiscallStructRetPtrOrder.Original && methodInfo != null && !from.IsStatic && methodInfo2 != null && to.IsStatic && methodInfo.ReturnType == methodInfo2.ReturnType && methodInfo.ReturnType.IsValueType)
		{
			int managedSize = methodInfo.ReturnType.GetManagedSize();
			if (managedSize == 3 || managedSize == 5 || managedSize == 6 || managedSize == 7 || managedSize >= 9)
			{
				Type thisParamType = from.GetThisParamType();
				Type item = methodInfo.ReturnType.MakeByRefType();
				int value = 0;
				int num = 1;
				if (GlueThiscallStructRetPtr == GlueThiscallStructRetPtrOrder.RetThisArgs)
				{
					value = 1;
					num = 0;
				}
				List<Type> list = new List<Type> { thisParamType };
				list.Insert(num, item);
				list.AddRange(from p in @from.GetParameters()
					select p.ParameterType);
				using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("Glue:ThiscallStructRetPtr<" + from.GetID(null, null, withType: true, proxyMethod: false, simple: true) + "," + to.GetID(null, null, withType: true, proxyMethod: false, simple: true) + ">", typeof(void), list.ToArray());
				ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
				iLProcessor.Emit(OpCodes.Ldarg, num);
				iLProcessor.Emit(OpCodes.Ldarg, value);
				for (int num2 = 2; num2 < list.Count; num2++)
				{
					iLProcessor.Emit(OpCodes.Ldarg, num2);
				}
				iLProcessor.Emit(OpCodes.Call, iLProcessor.Body.Method.Module.ImportReference(to));
				iLProcessor.Emit(OpCodes.Stobj, iLProcessor.Body.Method.Module.ImportReference(methodInfo.ReturnType));
				iLProcessor.Emit(OpCodes.Ret);
				methodInfo3 = dynamicMethodDefinition.Generate();
			}
		}
		return methodInfo3 ?? to;
	}
}
