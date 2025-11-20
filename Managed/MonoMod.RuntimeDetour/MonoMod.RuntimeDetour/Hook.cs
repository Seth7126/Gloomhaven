using System;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public class Hook : IDetour, IDisposable
{
	public static Func<Hook, MethodBase, MethodBase, object, bool> OnDetour;

	public static Func<Hook, bool> OnUndo;

	public static Func<Hook, MethodBase, MethodBase> OnGenerateTrampoline;

	public readonly MethodBase Method;

	public readonly MethodBase Target;

	public readonly MethodBase TargetReal;

	public readonly object DelegateTarget;

	private Detour _Detour;

	private readonly Type _OrigDelegateType;

	private readonly MethodInfo _OrigDelegateInvoke;

	private int? _RefTarget;

	private int? _RefTrampoline;

	private int? _RefTrampolineTmp;

	public bool IsValid => _Detour.IsValid;

	public bool IsApplied => _Detour.IsApplied;

	public Detour Detour => _Detour;

	public Hook(MethodBase from, MethodInfo to, object target, ref HookConfig config)
	{
		Method = from;
		Target = to;
		DelegateTarget = target;
		Type type = (from as MethodInfo)?.ReturnType ?? typeof(void);
		if (to.ReturnType != type && !to.ReturnType.IsCompatible(type))
		{
			throw new InvalidOperationException($"Return type of hook for {from} doesn't match, must be {((from as MethodInfo)?.ReturnType ?? typeof(void)).FullName}");
		}
		if (target == null && !to.IsStatic)
		{
			throw new InvalidOperationException($"Hook for method {from} must be static, or you must pass a target instance.");
		}
		ParameterInfo[] parameters = Target.GetParameters();
		ParameterInfo[] parameters2 = Method.GetParameters();
		Type[] array;
		if (!Method.IsStatic)
		{
			array = new Type[parameters2.Length + 1];
			array[0] = Method.GetThisParamType();
			for (int i = 0; i < parameters2.Length; i++)
			{
				array[i + 1] = parameters2[i].ParameterType;
			}
		}
		else
		{
			array = new Type[parameters2.Length];
			for (int j = 0; j < parameters2.Length; j++)
			{
				array[j] = parameters2[j].ParameterType;
			}
		}
		Type type2 = null;
		if (parameters.Length == array.Length + 1 && typeof(Delegate).IsAssignableFrom(parameters[0].ParameterType))
		{
			type2 = (_OrigDelegateType = parameters[0].ParameterType);
		}
		else if (parameters.Length != array.Length)
		{
			throw new InvalidOperationException($"Parameter count of hook for {from} doesn't match, must be {array.Length}");
		}
		for (int k = 0; k < array.Length; k++)
		{
			Type type3 = array[k];
			Type parameterType = parameters[k + ((type2 != null) ? 1 : 0)].ParameterType;
			if (!type3.IsCompatible(parameterType))
			{
				throw new InvalidOperationException($"Parameter #{k} of hook for {from} doesn't match, must be {type3.FullName} or related");
			}
		}
		MethodInfo methodInfo = (_OrigDelegateInvoke = type2?.GetMethod("Invoke"));
		DynamicMethodDefinition dynamicMethodDefinition;
		using (dynamicMethodDefinition = new DynamicMethodDefinition($"Hook<{Method.GetID(null, null, withType: true, proxyMethod: false, simple: true)}>?{GetHashCode()}", (Method as MethodInfo)?.ReturnType ?? typeof(void), array))
		{
			ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
			if (target != null)
			{
				_RefTarget = iLProcessor.EmitReference(target);
			}
			if (type2 != null)
			{
				_RefTrampoline = iLProcessor.EmitReference<Delegate>(null);
			}
			for (int l = 0; l < array.Length; l++)
			{
				iLProcessor.Emit(OpCodes.Ldarg, l);
			}
			iLProcessor.Emit(OpCodes.Call, Target);
			iLProcessor.Emit(OpCodes.Ret);
			TargetReal = dynamicMethodDefinition.Generate().Pin();
		}
		if (type2 != null)
		{
			ParameterInfo[] parameters3 = methodInfo.GetParameters();
			Type[] array2 = new Type[parameters3.Length];
			for (int m = 0; m < parameters3.Length; m++)
			{
				array2[m] = parameters3[m].ParameterType;
			}
			using (dynamicMethodDefinition = new DynamicMethodDefinition($"Chain:TMP<{Method.GetID(null, null, withType: true, proxyMethod: false, simple: true)}>?{GetHashCode()}", methodInfo?.ReturnType ?? typeof(void), array2))
			{
				ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
				_RefTrampolineTmp = iLProcessor.EmitReference<Delegate>(null);
				iLProcessor.Emit(OpCodes.Brfalse, iLProcessor.Body.Instructions[0]);
				iLProcessor.EmitGetReference<Delegate>(_RefTrampolineTmp.Value);
				for (int n = 0; n < array.Length; n++)
				{
					iLProcessor.Emit(OpCodes.Ldarg, n);
				}
				iLProcessor.Emit(OpCodes.Callvirt, methodInfo);
				iLProcessor.Emit(OpCodes.Ret);
				DynamicMethodHelper.SetReference(_RefTrampoline.Value, dynamicMethodDefinition.Generate().CreateDelegate(type2));
			}
		}
		_Detour = new Detour(Method, TargetReal, new DetourConfig
		{
			ManualApply = true,
			Priority = config.Priority,
			ID = config.ID,
			Before = config.Before,
			After = config.After
		});
		_UpdateOrig(null);
		if (!config.ManualApply)
		{
			Apply();
		}
	}

	public Hook(MethodBase from, MethodInfo to, object target, HookConfig config)
		: this(from, to, target, ref config)
	{
	}

	public Hook(MethodBase from, MethodInfo to, object target)
		: this(from, to, target, DetourContext.Current?.HookConfig ?? default(HookConfig))
	{
	}

	public Hook(MethodBase from, MethodInfo to, ref HookConfig config)
		: this(from, to, null, ref config)
	{
	}

	public Hook(MethodBase from, MethodInfo to, HookConfig config)
		: this(from, to, null, ref config)
	{
	}

	public Hook(MethodBase from, MethodInfo to)
		: this(from, to, null)
	{
	}

	public Hook(MethodBase method, IntPtr to, ref HookConfig config)
		: this(method, DetourHelper.GenerateNativeProxy(to, method), null, ref config)
	{
	}

	public Hook(MethodBase method, IntPtr to, HookConfig config)
		: this(method, DetourHelper.GenerateNativeProxy(to, method), null, ref config)
	{
	}

	public Hook(MethodBase method, IntPtr to)
		: this(method, DetourHelper.GenerateNativeProxy(to, method), null)
	{
	}

	public Hook(MethodBase method, Delegate to, ref HookConfig config)
		: this(method, to.Method, to.Target, ref config)
	{
	}

	public Hook(MethodBase method, Delegate to, HookConfig config)
		: this(method, to.Method, to.Target, ref config)
	{
	}

	public Hook(MethodBase method, Delegate to)
		: this(method, to.Method, to.Target)
	{
	}

	public Hook(Delegate from, IntPtr to, ref HookConfig config)
		: this(from.Method, to, ref config)
	{
	}

	public Hook(Delegate from, IntPtr to, HookConfig config)
		: this(from.Method, to, ref config)
	{
	}

	public Hook(Delegate from, IntPtr to)
		: this(from.Method, to)
	{
	}

	public Hook(Delegate from, Delegate to, ref HookConfig config)
		: this(from.Method, to, ref config)
	{
	}

	public Hook(Delegate from, Delegate to, HookConfig config)
		: this(from.Method, to, ref config)
	{
	}

	public Hook(Delegate from, Delegate to)
		: this(from.Method, to)
	{
	}

	public Hook(Expression from, IntPtr to, ref HookConfig config)
		: this(((MethodCallExpression)from).Method, to, ref config)
	{
	}

	public Hook(Expression from, IntPtr to, HookConfig config)
		: this(((MethodCallExpression)from).Method, to, ref config)
	{
	}

	public Hook(Expression from, IntPtr to)
		: this(((MethodCallExpression)from).Method, to)
	{
	}

	public Hook(Expression from, Delegate to, ref HookConfig config)
		: this(((MethodCallExpression)from).Method, to, ref config)
	{
	}

	public Hook(Expression from, Delegate to, HookConfig config)
		: this(((MethodCallExpression)from).Method, to, ref config)
	{
	}

	public Hook(Expression from, Delegate to)
		: this(((MethodCallExpression)from).Method, to)
	{
	}

	public Hook(Expression<Action> from, IntPtr to, ref HookConfig config)
		: this(from.Body, to, ref config)
	{
	}

	public Hook(Expression<Action> from, IntPtr to, HookConfig config)
		: this(from.Body, to, ref config)
	{
	}

	public Hook(Expression<Action> from, IntPtr to)
		: this(from.Body, to)
	{
	}

	public Hook(Expression<Action> from, Delegate to, ref HookConfig config)
		: this(from.Body, to, ref config)
	{
	}

	public Hook(Expression<Action> from, Delegate to, HookConfig config)
		: this(from.Body, to, ref config)
	{
	}

	public Hook(Expression<Action> from, Delegate to)
		: this(from.Body, to)
	{
	}

	public void Apply()
	{
		if (!IsValid)
		{
			throw new ObjectDisposedException("Hook");
		}
		if (!IsApplied)
		{
			Func<Hook, MethodBase, MethodBase, object, bool> onDetour = OnDetour;
			if (onDetour != null && !onDetour.InvokeWhileTrue(this, Method, Target, DelegateTarget))
			{
				return;
			}
		}
		_Detour.Apply();
	}

	public void Undo()
	{
		if (!IsValid)
		{
			throw new ObjectDisposedException("Hook");
		}
		if (IsApplied)
		{
			Func<Hook, bool> onUndo = OnUndo;
			if (onUndo != null && !onUndo.InvokeWhileTrue(this))
			{
				return;
			}
		}
		_Detour.Undo();
		if (!IsValid)
		{
			_Free();
		}
	}

	public void Free()
	{
		if (IsValid)
		{
			_Detour.Free();
			_Free();
		}
	}

	public void Dispose()
	{
		if (IsValid)
		{
			Undo();
			Free();
		}
	}

	private void _Free()
	{
		if (_RefTarget.HasValue)
		{
			DynamicMethodHelper.FreeReference(_RefTarget.Value);
		}
		if (_RefTrampoline.HasValue)
		{
			DynamicMethodHelper.FreeReference(_RefTrampoline.Value);
		}
		if (_RefTrampolineTmp.HasValue)
		{
			DynamicMethodHelper.FreeReference(_RefTrampolineTmp.Value);
		}
		TargetReal.Unpin();
	}

	public MethodBase GenerateTrampoline(MethodBase signature = null)
	{
		MethodBase methodBase = OnGenerateTrampoline?.InvokeWhileNull<MethodBase>(new object[2] { this, signature });
		if (methodBase != null)
		{
			return methodBase;
		}
		return _Detour.GenerateTrampoline(signature);
	}

	public T GenerateTrampoline<T>() where T : Delegate
	{
		if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
		{
			throw new InvalidOperationException($"Type {typeof(T)} not a delegate type.");
		}
		return GenerateTrampoline(typeof(T).GetMethod("Invoke")).CreateDelegate(typeof(T)) as T;
	}

	internal void _UpdateOrig(MethodBase invoke)
	{
		if (_OrigDelegateType != null)
		{
			Delegate obj = (invoke ?? GenerateTrampoline(_OrigDelegateInvoke)).CreateDelegate(_OrigDelegateType);
			DynamicMethodHelper.SetReference(_RefTrampoline.Value, obj);
			DynamicMethodHelper.SetReference(_RefTrampolineTmp.Value, obj);
		}
	}
}
public class Hook<T> : Hook
{
	public Hook(Expression<Action> from, T to, ref HookConfig config)
		: base(from.Body, to as Delegate, ref config)
	{
	}

	public Hook(Expression<Action> from, T to, HookConfig config)
		: base(from.Body, to as Delegate, ref config)
	{
	}

	public Hook(Expression<Action> from, T to)
		: base(from.Body, to as Delegate)
	{
	}

	public Hook(Expression<Func<T>> from, IntPtr to, ref HookConfig config)
		: base(from.Body, to, ref config)
	{
	}

	public Hook(Expression<Func<T>> from, IntPtr to, HookConfig config)
		: base(from.Body, to, ref config)
	{
	}

	public Hook(Expression<Func<T>> from, IntPtr to)
		: base(from.Body, to)
	{
	}

	public Hook(Expression<Func<T>> from, Delegate to, ref HookConfig config)
		: base(from.Body, to, ref config)
	{
	}

	public Hook(Expression<Func<T>> from, Delegate to, HookConfig config)
		: base(from.Body, to, ref config)
	{
	}

	public Hook(Expression<Func<T>> from, Delegate to)
		: base(from.Body, to)
	{
	}

	public Hook(T from, IntPtr to, ref HookConfig config)
		: base(from as Delegate, to, ref config)
	{
	}

	public Hook(T from, IntPtr to, HookConfig config)
		: base(from as Delegate, to, ref config)
	{
	}

	public Hook(T from, IntPtr to)
		: base(from as Delegate, to)
	{
	}

	public Hook(T from, T to, ref HookConfig config)
		: base(from as Delegate, to as Delegate, ref config)
	{
	}

	public Hook(T from, T to, HookConfig config)
		: base(from as Delegate, to as Delegate, ref config)
	{
	}

	public Hook(T from, T to)
		: base(from as Delegate, to as Delegate)
	{
	}
}
public class Hook<TFrom, TTo> : Hook
{
	public Hook(Expression<Func<TFrom>> from, TTo to, ref HookConfig config)
		: base(from.Body, to as Delegate)
	{
	}

	public Hook(Expression<Func<TFrom>> from, TTo to, HookConfig config)
		: base(from.Body, to as Delegate)
	{
	}

	public Hook(Expression<Func<TFrom>> from, TTo to)
		: base(from.Body, to as Delegate)
	{
	}

	public Hook(TFrom from, TTo to, ref HookConfig config)
		: base(from as Delegate, to as Delegate)
	{
	}

	public Hook(TFrom from, TTo to, HookConfig config)
		: base(from as Delegate, to as Delegate)
	{
	}

	public Hook(TFrom from, TTo to)
		: base(from as Delegate, to as Delegate)
	{
	}
}
