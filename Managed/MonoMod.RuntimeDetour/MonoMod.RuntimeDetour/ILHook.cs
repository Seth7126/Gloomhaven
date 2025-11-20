using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public class ILHook : ISortableDetour, IDetour, IDisposable
{
	private class Context
	{
		public List<ILHook> Chain = new List<ILHook>();

		public HashSet<ILContext> Active = new HashSet<ILContext>();

		public MethodBase Method;

		public Detour Detour;

		public Context(MethodBase method)
		{
			Method = method;
		}

		public void Add(ILHook hook)
		{
			List<ILHook> chain = Chain;
			lock (chain)
			{
				chain.Add(hook);
			}
		}

		public void Remove(ILHook hook)
		{
			List<ILHook> chain = Chain;
			lock (chain)
			{
				chain.Remove(hook);
				if (chain.Count == 0)
				{
					Refresh();
					lock (_Map)
					{
						_Map.Remove(Method);
						return;
					}
				}
			}
		}

		public void Refresh()
		{
			List<ILHook> chain = Chain;
			lock (chain)
			{
				foreach (ILContext item in Active)
				{
					item.Dispose();
				}
				Active.Clear();
				Detour?.Dispose();
				Detour = null;
				if (chain.Count == 0)
				{
					return;
				}
				bool flag = false;
				foreach (ILHook item2 in chain)
				{
					if (item2.IsApplied)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
				DetourSorter<ILHook>.Sort(chain);
				MethodBase to;
				using (DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(Method))
				{
					MethodDefinition definition = dynamicMethodDefinition.Definition;
					foreach (ILHook item3 in chain)
					{
						if (item3.IsApplied)
						{
							InvokeManipulator(definition, item3.Manipulator);
						}
					}
					to = dynamicMethodDefinition.Generate();
				}
				Detour = new Detour(Method, to, ref ILDetourConfig);
			}
		}

		private void InvokeManipulator(MethodDefinition def, ILContext.Manipulator cb)
		{
			ILContext iLContext = new ILContext(def);
			iLContext.ReferenceBag = RuntimeILReferenceBag.Instance;
			iLContext.Invoke(cb);
			if (iLContext.IsReadOnly)
			{
				iLContext.Dispose();
				return;
			}
			iLContext.MakeReadOnly();
			Active.Add(iLContext);
		}
	}

	public static Func<ILHook, MethodBase, ILContext.Manipulator, bool> OnDetour;

	public static Func<ILHook, bool> OnUndo;

	private static DetourConfig ILDetourConfig = new DetourConfig
	{
		Priority = -268435456,
		Before = new string[1] { "*" }
	};

	private static Dictionary<MethodBase, Context> _Map = new Dictionary<MethodBase, Context>();

	private static uint _GlobalIndexNext = 0u;

	private readonly uint _GlobalIndex;

	private int _Priority;

	private string _ID;

	private List<string> _Before = new List<string>();

	private ReadOnlyCollection<string> _BeforeRO;

	private List<string> _After = new List<string>();

	private ReadOnlyCollection<string> _AfterRO;

	public readonly MethodBase Method;

	public readonly ILContext.Manipulator Manipulator;

	private Context _Ctx
	{
		get
		{
			if (!_Map.TryGetValue(Method, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public bool IsValid => Index != -1;

	public bool IsApplied { get; private set; }

	public int Index => _Ctx?.Chain.IndexOf(this) ?? (-1);

	public int MaxIndex => _Ctx?.Chain.Count ?? (-1);

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
				_Ctx.Refresh();
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
				value = Manipulator.Method?.GetID(null, null, withType: true, proxyMethod: false, simple: true) ?? GetHashCode().ToString();
			}
			if (!(_ID == value))
			{
				_ID = value;
				_Ctx.Refresh();
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
				_Ctx.Refresh();
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
				_Ctx.Refresh();
			}
		}
	}

	public ILHook(MethodBase from, ILContext.Manipulator manipulator, ref ILHookConfig config)
	{
		Method = from.Pin();
		Manipulator = manipulator;
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
		Context value;
		lock (_Map)
		{
			if (!_Map.TryGetValue(Method, out value))
			{
				value = (_Map[Method] = new Context(Method));
			}
		}
		lock (value)
		{
			value.Add(this);
		}
		if (!config.ManualApply)
		{
			Apply();
		}
	}

	public ILHook(MethodBase from, ILContext.Manipulator manipulator, ILHookConfig config)
		: this(from, manipulator, ref config)
	{
	}

	public ILHook(MethodBase from, ILContext.Manipulator manipulator)
		: this(from, manipulator, DetourContext.Current?.ILHookConfig ?? default(ILHookConfig))
	{
	}

	public void Apply()
	{
		if (!IsValid)
		{
			throw new ObjectDisposedException("ILHook");
		}
		if (!IsApplied)
		{
			Func<ILHook, MethodBase, ILContext.Manipulator, bool> onDetour = OnDetour;
			if (onDetour == null || onDetour.InvokeWhileTrue(this, Method, Manipulator))
			{
				IsApplied = true;
				_Ctx.Refresh();
			}
		}
	}

	public void Undo()
	{
		if (!IsValid)
		{
			throw new ObjectDisposedException("ILHook");
		}
		if (IsApplied)
		{
			Func<ILHook, bool> onUndo = OnUndo;
			if (onUndo == null || onUndo.InvokeWhileTrue(this))
			{
				IsApplied = false;
				_Ctx.Refresh();
			}
		}
	}

	public void Free()
	{
		if (IsValid)
		{
			Undo();
			_Ctx.Remove(this);
			Method.Unpin();
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

	public MethodBase GenerateTrampoline(MethodBase signature = null)
	{
		throw new NotSupportedException();
	}

	public T GenerateTrampoline<T>() where T : Delegate
	{
		throw new NotSupportedException();
	}
}
