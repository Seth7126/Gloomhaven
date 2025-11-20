using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public class NativeDetour : IDetour, IDisposable
{
	public static Func<NativeDetour, MethodBase, IntPtr, IntPtr, bool> OnDetour;

	public static Func<NativeDetour, bool> OnUndo;

	public static Func<NativeDetour, MethodBase, MethodBase> OnGenerateTrampoline;

	public readonly NativeDetourData Data;

	public readonly MethodBase Method;

	private readonly MethodInfo _BackupMethod;

	private readonly IntPtr _BackupNative;

	private HashSet<MethodBase> _Pinned = new HashSet<MethodBase>();

	public bool IsValid { get; private set; }

	public bool IsApplied { get; private set; }

	public NativeDetour(MethodBase method, IntPtr from, IntPtr to, ref NativeDetourConfig config)
	{
		Method = method;
		Func<NativeDetour, MethodBase, IntPtr, IntPtr, bool> onDetour = OnDetour;
		if (onDetour == null || onDetour.InvokeWhileTrue(this, method, from, to))
		{
			IsValid = true;
			Data = DetourHelper.Native.Create(from, to);
			if (!config.SkipILCopy)
			{
				method?.TryCreateILCopy(out _BackupMethod);
			}
			_BackupNative = DetourHelper.Native.MemAlloc(Data.Size);
			if (!config.ManualApply)
			{
				Apply();
			}
		}
	}

	public NativeDetour(MethodBase method, IntPtr from, IntPtr to, NativeDetourConfig config)
		: this(method, from, to, ref config)
	{
	}

	public NativeDetour(MethodBase method, IntPtr from, IntPtr to)
		: this(method, from, to, default(NativeDetourConfig))
	{
	}

	public NativeDetour(IntPtr from, IntPtr to, ref NativeDetourConfig config)
		: this(null, from, to, ref config)
	{
	}

	public NativeDetour(IntPtr from, IntPtr to, NativeDetourConfig config)
		: this(null, from, to, ref config)
	{
	}

	public NativeDetour(IntPtr from, IntPtr to)
		: this(null, from, to)
	{
	}

	public NativeDetour(MethodBase from, IntPtr to, ref NativeDetourConfig config)
		: this(from, from.Pin().GetNativeStart(), to, ref config)
	{
		_Pinned.Add(from);
	}

	public NativeDetour(MethodBase from, IntPtr to, NativeDetourConfig config)
		: this(from, from.Pin().GetNativeStart(), to, ref config)
	{
		_Pinned.Add(from);
	}

	public NativeDetour(MethodBase from, IntPtr to)
		: this(from, from.Pin().GetNativeStart(), to)
	{
		_Pinned.Add(from);
	}

	public NativeDetour(IntPtr from, MethodBase to, ref NativeDetourConfig config)
		: this(from, to.Pin().GetNativeStart(), ref config)
	{
		_Pinned.Add(to);
	}

	public NativeDetour(IntPtr from, MethodBase to, NativeDetourConfig config)
		: this(from, to.Pin().GetNativeStart(), ref config)
	{
		_Pinned.Add(to);
	}

	public NativeDetour(IntPtr from, MethodBase to)
		: this(from, to.Pin().GetNativeStart())
	{
		_Pinned.Add(to);
	}

	public NativeDetour(MethodBase from, MethodBase to, ref NativeDetourConfig config)
		: this(from, DetourHelper.Runtime.GetDetourTarget(from, to).Pin().GetNativeStart(), ref config)
	{
		_Pinned.Add(to);
	}

	public NativeDetour(MethodBase from, MethodBase to, NativeDetourConfig config)
		: this(from, DetourHelper.Runtime.GetDetourTarget(from, to).Pin().GetNativeStart(), ref config)
	{
		_Pinned.Add(to);
	}

	public NativeDetour(MethodBase from, MethodBase to)
		: this(from, DetourHelper.Runtime.GetDetourTarget(from, to).Pin().GetNativeStart())
	{
		_Pinned.Add(to);
	}

	public NativeDetour(Delegate from, IntPtr to, ref NativeDetourConfig config)
		: this(from.Method, to, ref config)
	{
	}

	public NativeDetour(Delegate from, IntPtr to, NativeDetourConfig config)
		: this(from.Method, to, ref config)
	{
	}

	public NativeDetour(Delegate from, IntPtr to)
		: this(from.Method, to)
	{
	}

	public NativeDetour(IntPtr from, Delegate to, ref NativeDetourConfig config)
		: this(from, to.Method, ref config)
	{
	}

	public NativeDetour(IntPtr from, Delegate to, NativeDetourConfig config)
		: this(from, to.Method, ref config)
	{
	}

	public NativeDetour(IntPtr from, Delegate to)
		: this(from, to.Method)
	{
	}

	public NativeDetour(Delegate from, Delegate to, ref NativeDetourConfig config)
		: this(from.Method, to.Method, ref config)
	{
	}

	public NativeDetour(Delegate from, Delegate to, NativeDetourConfig config)
		: this(from.Method, to.Method, ref config)
	{
	}

	public NativeDetour(Delegate from, Delegate to)
		: this(from.Method, to.Method)
	{
	}

	public void Apply()
	{
		if (!IsValid)
		{
			throw new ObjectDisposedException("NativeDetour");
		}
		if (!IsApplied)
		{
			IsApplied = true;
			DetourHelper.Native.Copy(Data.Method, _BackupNative, Data.Type);
			DetourHelper.Native.MakeWritable(Data);
			DetourHelper.Native.Apply(Data);
			DetourHelper.Native.MakeExecutable(Data);
			DetourHelper.Native.FlushICache(Data);
		}
	}

	public void Undo()
	{
		if (!IsValid)
		{
			throw new ObjectDisposedException("NativeDetour");
		}
		Func<NativeDetour, bool> onUndo = OnUndo;
		if ((onUndo == null || onUndo.InvokeWhileTrue(this)) && IsApplied)
		{
			IsApplied = false;
			DetourHelper.Native.MakeWritable(Data);
			DetourHelper.Native.Copy(_BackupNative, Data.Method, Data.Type);
			DetourHelper.Native.MakeExecutable(Data);
			DetourHelper.Native.FlushICache(Data);
		}
	}

	public void Free()
	{
		if (!IsValid)
		{
			return;
		}
		IsValid = false;
		DetourHelper.Native.MemFree(_BackupNative);
		DetourHelper.Native.Free(Data);
		if (IsApplied)
		{
			return;
		}
		foreach (MethodBase item in _Pinned)
		{
			item.Unpin();
		}
		_Pinned.Clear();
	}

	public void Dispose()
	{
		if (IsValid)
		{
			Undo();
			Free();
		}
	}

	public MethodBase GenerateTrampoline(MethodBase signature = null)
	{
		MethodBase methodBase = OnGenerateTrampoline?.InvokeWhileNull<MethodBase>(new object[2] { this, signature });
		if (methodBase != null)
		{
			return methodBase;
		}
		if (!IsValid)
		{
			throw new ObjectDisposedException("NativeDetour");
		}
		if (_BackupMethod != null)
		{
			return _BackupMethod;
		}
		if (signature == null)
		{
			throw new ArgumentNullException("A signature must be given if the NativeDetour doesn't hold a reference to a managed method.");
		}
		MethodBase methodBase2 = Method;
		if (methodBase2 == null)
		{
			methodBase2 = DetourHelper.GenerateNativeProxy(Data.Method, signature);
		}
		Type type = (signature as MethodInfo)?.ReturnType ?? typeof(void);
		ParameterInfo[] parameters = signature.GetParameters();
		Type[] array = new Type[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(string.Format("Trampoline:Native<{0}>?{1}", Method?.GetID(null, null, withType: true, proxyMethod: false, simple: true) ?? ((long)Data.Method).ToString("X16"), GetHashCode()), type, array);
		ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
		ExceptionHandler exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally);
		iLProcessor.Body.ExceptionHandlers.Add(exceptionHandler);
		iLProcessor.EmitDetourCopy(_BackupNative, Data.Method, Data.Type);
		VariableDefinition variableDefinition = null;
		if (type != typeof(void))
		{
			iLProcessor.Body.Variables.Add(variableDefinition = new VariableDefinition(iLProcessor.Import(type)));
		}
		int count = iLProcessor.Body.Instructions.Count;
		for (int j = 0; j < array.Length; j++)
		{
			iLProcessor.Emit(OpCodes.Ldarg, j);
		}
		if (methodBase2 is MethodInfo)
		{
			iLProcessor.Emit(OpCodes.Call, (MethodInfo)methodBase2);
		}
		else
		{
			if (!(methodBase2 is ConstructorInfo))
			{
				throw new NotSupportedException("Method type " + methodBase2.GetType().FullName + " not supported.");
			}
			iLProcessor.Emit(OpCodes.Call, (ConstructorInfo)methodBase2);
		}
		if (variableDefinition != null)
		{
			iLProcessor.Emit(OpCodes.Stloc, variableDefinition);
		}
		iLProcessor.Emit(OpCodes.Leave, (object)null);
		Instruction instruction = iLProcessor.Body.Instructions[iLProcessor.Body.Instructions.Count - 1];
		int count2 = iLProcessor.Body.Instructions.Count;
		_ = iLProcessor.Body.Instructions.Count;
		iLProcessor.EmitDetourApply(Data);
		int count3 = iLProcessor.Body.Instructions.Count;
		Instruction instruction2 = null;
		if (variableDefinition != null)
		{
			iLProcessor.Emit(OpCodes.Ldloc, variableDefinition);
			instruction2 = iLProcessor.Body.Instructions[iLProcessor.Body.Instructions.Count - 1];
		}
		iLProcessor.Emit(OpCodes.Ret);
		instruction2 = instruction2 ?? iLProcessor.Body.Instructions[iLProcessor.Body.Instructions.Count - 1];
		instruction.Operand = instruction2;
		exceptionHandler.TryStart = iLProcessor.Body.Instructions[count];
		exceptionHandler.TryEnd = iLProcessor.Body.Instructions[count2];
		exceptionHandler.HandlerStart = iLProcessor.Body.Instructions[count2];
		exceptionHandler.HandlerEnd = iLProcessor.Body.Instructions[count3];
		return dynamicMethodDefinition.Generate();
	}

	public T GenerateTrampoline<T>() where T : Delegate
	{
		if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
		{
			throw new InvalidOperationException($"Type {typeof(T)} not a delegate type.");
		}
		return GenerateTrampoline(typeof(T).GetMethod("Invoke")).CreateDelegate(typeof(T)) as T;
	}
}
