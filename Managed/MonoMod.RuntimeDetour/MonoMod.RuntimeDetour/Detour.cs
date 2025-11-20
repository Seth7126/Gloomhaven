using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public class Detour : ISortableDetour, IDetour, IDisposable
{
	private static Dictionary<MethodBase, List<Detour>> _DetourMap = new Dictionary<MethodBase, List<Detour>>();

	private static Dictionary<MethodBase, MethodInfo> _BackupMethods = new Dictionary<MethodBase, MethodInfo>();

	private static uint _GlobalIndexNext = 0u;

	public static Func<Detour, MethodBase, MethodBase, bool> OnDetour;

	public static Func<Detour, bool> OnUndo;

	public static Func<Detour, MethodBase, MethodBase> OnGenerateTrampoline;

	private readonly uint _GlobalIndex;

	private int _Priority;

	private string _ID;

	private List<string> _Before = new List<string>();

	private ReadOnlyCollection<string> _BeforeRO;

	private List<string> _After = new List<string>();

	private ReadOnlyCollection<string> _AfterRO;

	public readonly MethodBase Method;

	public readonly MethodBase Target;

	public readonly MethodBase TargetReal;

	private NativeDetour _TopDetour;

	private MethodInfo _ChainedTrampoline;

	private List<Detour> _DetourChain
	{
		get
		{
			if (!_DetourMap.TryGetValue(Method, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public bool IsValid => Index != -1;

	public bool IsApplied { get; private set; }

	private bool IsTop => _TopDetour != null;

	public int Index => _DetourChain?.IndexOf(this) ?? (-1);

	public int MaxIndex => _DetourChain?.Count ?? (-1);

	public uint GlobalIndex => _GlobalIndex;

	public int Priority
	{
		get
		{
			return _Priority;
		}
		set
		{
			if (_Priority != value)
			{
				_Priority = value;
				_RefreshChain(Method);
			}
		}
	}

	public string ID
	{
		get
		{
			return _ID;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = Target.GetID(null, null, withType: true, proxyMethod: false, simple: true);
			}
			if (!(_ID == value))
			{
				_ID = value;
				_RefreshChain(Method);
			}
		}
	}

	public IEnumerable<string> Before
	{
		get
		{
			return _BeforeRO ?? (_BeforeRO = _Before.AsReadOnly());
		}
		set
		{
			lock (_Before)
			{
				_Before.Clear();
				if (value != null)
				{
					foreach (string item in value)
					{
						_Before.Add(item);
					}
				}
				_RefreshChain(Method);
			}
		}
	}

	public IEnumerable<string> After
	{
		get
		{
			return _AfterRO ?? (_AfterRO = _After.AsReadOnly());
		}
		set
		{
			lock (_After)
			{
				_After.Clear();
				if (value != null)
				{
					foreach (string item in value)
					{
						_After.Add(item);
					}
				}
				_RefreshChain(Method);
			}
		}
	}

	public Detour(MethodBase from, MethodBase to, ref DetourConfig config)
	{
		Method = from.Pin();
		Target = to.Pin();
		TargetReal = DetourHelper.Runtime.GetDetourTarget(from, to).Pin();
		_GlobalIndex = _GlobalIndexNext++;
		_Priority = config.Priority;
		_ID = config.ID;
		if (config.Before != null)
		{
			foreach (string item in config.Before)
			{
				_Before.Add(item);
			}
		}
		if (config.After != null)
		{
			foreach (string item2 in config.After)
			{
				_After.Add(item2);
			}
		}
		lock (_BackupMethods)
		{
			if ((!_BackupMethods.TryGetValue(Method, out var value) || value == null) && (value = Method.CreateILCopy()) != null)
			{
				_BackupMethods[Method] = value.Pin();
			}
		}
		ParameterInfo[] parameters = Method.GetParameters();
		Type[] array;
		if (!Method.IsStatic)
		{
			array = new Type[parameters.Length + 1];
			array[0] = Method.GetThisParamType();
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i + 1] = parameters[i].ParameterType;
			}
		}
		else
		{
			array = new Type[parameters.Length];
			for (int j = 0; j < parameters.Length; j++)
			{
				array[j] = parameters[j].ParameterType;
			}
		}
		using (DynamicMethodDefinition dm = new DynamicMethodDefinition($"Chain<{Method.GetID(null, null, withType: true, proxyMethod: false, simple: true)}>?{GetHashCode()}", (Method as MethodInfo)?.ReturnType ?? typeof(void), array))
		{
			_ChainedTrampoline = dm.StubCriticalDetour().Generate().Pin();
		}
		List<Detour> value2;
		lock (_DetourMap)
		{
			if (!_DetourMap.TryGetValue(Method, out value2))
			{
				value2 = (_DetourMap[Method] = new List<Detour>());
			}
		}
		lock (value2)
		{
			value2.Add(this);
		}
		if (!config.ManualApply)
		{
			Apply();
		}
	}

	public Detour(MethodBase from, MethodBase to, DetourConfig config)
		: this(from, to, ref config)
	{
	}

	public Detour(MethodBase from, MethodBase to)
		: this(from, to, DetourContext.Current?.DetourConfig ?? default(DetourConfig))
	{
	}

	public Detour(MethodBase method, IntPtr to, ref DetourConfig config)
		: this(method, DetourHelper.GenerateNativeProxy(to, method), ref config)
	{
	}

	public Detour(MethodBase method, IntPtr to, DetourConfig config)
		: this(method, DetourHelper.GenerateNativeProxy(to, method), ref config)
	{
	}

	public Detour(MethodBase method, IntPtr to)
		: this(method, DetourHelper.GenerateNativeProxy(to, method))
	{
	}

	public Detour(Delegate from, IntPtr to, ref DetourConfig config)
		: this(from.Method, to, ref config)
	{
	}

	public Detour(Delegate from, IntPtr to, DetourConfig config)
		: this(from.Method, to, ref config)
	{
	}

	public Detour(Delegate from, IntPtr to)
		: this(from.Method, to)
	{
	}

	public Detour(Delegate from, Delegate to, ref DetourConfig config)
		: this(from.Method, to.Method, ref config)
	{
	}

	public Detour(Delegate from, Delegate to, DetourConfig config)
		: this(from.Method, to.Method, ref config)
	{
	}

	public Detour(Delegate from, Delegate to)
		: this(from.Method, to.Method)
	{
	}

	public Detour(Expression from, IntPtr to, ref DetourConfig config)
		: this(((MethodCallExpression)from).Method, to, ref config)
	{
	}

	public Detour(Expression from, IntPtr to, DetourConfig config)
		: this(((MethodCallExpression)from).Method, to, ref config)
	{
	}

	public Detour(Expression from, IntPtr to)
		: this(((MethodCallExpression)from).Method, to)
	{
	}

	public Detour(Expression from, Expression to, ref DetourConfig config)
		: this(((MethodCallExpression)from).Method, ((MethodCallExpression)to).Method, ref config)
	{
	}

	public Detour(Expression from, Expression to, DetourConfig config)
		: this(((MethodCallExpression)from).Method, ((MethodCallExpression)to).Method, ref config)
	{
	}

	public Detour(Expression from, Expression to)
		: this(((MethodCallExpression)from).Method, ((MethodCallExpression)to).Method)
	{
	}

	public Detour(Expression<Action> from, IntPtr to, ref DetourConfig config)
		: this(from.Body, to, ref config)
	{
	}

	public Detour(Expression<Action> from, IntPtr to, DetourConfig config)
		: this(from.Body, to, ref config)
	{
	}

	public Detour(Expression<Action> from, IntPtr to)
		: this(from.Body, to)
	{
	}

	public Detour(Expression<Action> from, Expression<Action> to, ref DetourConfig config)
		: this(from.Body, to.Body, ref config)
	{
	}

	public Detour(Expression<Action> from, Expression<Action> to, DetourConfig config)
		: this(from.Body, to.Body, ref config)
	{
	}

	public Detour(Expression<Action> from, Expression<Action> to)
		: this(from.Body, to.Body)
	{
	}

	public void Apply()
	{
		if (!IsValid)
		{
			throw new ObjectDisposedException("Detour");
		}
		if (!IsApplied)
		{
			Func<Detour, MethodBase, MethodBase, bool> onDetour = OnDetour;
			if (onDetour == null || onDetour.InvokeWhileTrue(this, Method, Target))
			{
				IsApplied = true;
				_RefreshChain(Method);
			}
		}
	}

	public void Undo()
	{
		if (!IsValid)
		{
			throw new ObjectDisposedException("Detour");
		}
		if (IsApplied)
		{
			Func<Detour, bool> onUndo = OnUndo;
			if (onUndo == null || onUndo.InvokeWhileTrue(this))
			{
				IsApplied = false;
				_RefreshChain(Method);
			}
		}
	}

	public void Free()
	{
		if (!IsValid)
		{
			return;
		}
		Undo();
		List<Detour> detourChain = _DetourChain;
		lock (detourChain)
		{
			detourChain.Remove(this);
			if (detourChain.Count == 0)
			{
				lock (_BackupMethods)
				{
					if (_BackupMethods.TryGetValue(Method, out var value))
					{
						value.Unpin();
						_BackupMethods.Remove(Method);
					}
				}
				lock (_DetourMap)
				{
					_DetourMap.Remove(Method);
				}
			}
		}
		_ChainedTrampoline.Unpin();
		Method.Unpin();
		Target.Unpin();
		TargetReal.Unpin();
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
		if (signature == null)
		{
			signature = Target;
		}
		Type returnType = (signature as MethodInfo)?.ReturnType ?? typeof(void);
		ParameterInfo[] parameters = signature.GetParameters();
		Type[] array = new Type[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition($"Trampoline<{Method.GetID(null, null, withType: true, proxyMethod: false, simple: true)}>?{GetHashCode()}", returnType, array);
		ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
		for (int j = 0; j < 10; j++)
		{
			iLProcessor.Emit(OpCodes.Nop);
		}
		iLProcessor.Emit(OpCodes.Jmp, _ChainedTrampoline);
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

	private void _TopUndo()
	{
		if (_TopDetour != null)
		{
			_TopDetour.Undo();
			_TopDetour.Free();
			_TopDetour = null;
		}
	}

	private void _TopApply()
	{
		if (_TopDetour == null)
		{
			_TopDetour = new NativeDetour(Method.GetNativeStart(), TargetReal.GetNativeStart());
		}
	}

	private static void _RefreshChain(MethodBase method)
	{
		List<Detour> list = _DetourMap[method];
		lock (list)
		{
			DetourSorter<Detour>.Sort(list);
			Detour detour = list.FindLast((Detour d) => d.IsTop);
			Detour detour2 = list.FindLast((Detour d) => d.IsApplied);
			if (detour != detour2)
			{
				detour?._TopUndo();
				detour2?._TopApply();
			}
			if (list.Count == 0)
			{
				return;
			}
			MethodBase method2 = _BackupMethods[method];
			foreach (Detour item in list)
			{
				if (item.IsApplied)
				{
					_ = item._ChainedTrampoline;
					using (NativeDetour nativeDetour = new NativeDetour(item._ChainedTrampoline.GetNativeStart(), method2.GetNativeStart()))
					{
						nativeDetour.Free();
					}
					method2 = item.Target;
				}
			}
		}
	}
}
public class Detour<T> : Detour where T : Delegate
{
	public Detour(T from, IntPtr to, ref DetourConfig config)
		: base(from, to, ref config)
	{
	}

	public Detour(T from, IntPtr to, DetourConfig config)
		: base(from, to, ref config)
	{
	}

	public Detour(T from, IntPtr to)
		: base(from, to)
	{
	}

	public Detour(T from, T to, ref DetourConfig config)
		: base(from, to, ref config)
	{
	}

	public Detour(T from, T to, DetourConfig config)
		: base(from, to, ref config)
	{
	}

	public Detour(T from, T to)
		: base(from, to)
	{
	}
}
