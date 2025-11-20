using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public sealed class DetourContext : IDisposable
{
	[ThreadStatic]
	private static List<DetourContext> _Contexts;

	[ThreadStatic]
	private static DetourContext Last;

	private MethodBase Creator;

	public int Priority;

	private readonly string _FallbackID;

	private string _ID;

	public List<string> Before = new List<string>();

	public List<string> After = new List<string>();

	private bool IsDisposed;

	private static List<DetourContext> Contexts => _Contexts ?? (_Contexts = new List<DetourContext>());

	internal static DetourContext Current
	{
		get
		{
			DetourContext last = Last;
			if (last != null && last.IsValid)
			{
				return Last;
			}
			List<DetourContext> contexts = Contexts;
			int num = contexts.Count - 1;
			while (num > -1)
			{
				DetourContext detourContext = contexts[num];
				if (!detourContext.IsValid)
				{
					contexts.RemoveAt(num);
					num--;
					continue;
				}
				return Last = detourContext;
			}
			return null;
		}
	}

	public string ID
	{
		get
		{
			return _ID ?? _FallbackID;
		}
		set
		{
			_ID = (string.IsNullOrEmpty(value) ? null : value);
		}
	}

	public DetourConfig DetourConfig => new DetourConfig
	{
		Priority = Priority,
		ID = ID,
		Before = Before,
		After = After
	};

	public HookConfig HookConfig => new HookConfig
	{
		Priority = Priority,
		ID = ID,
		Before = Before,
		After = After
	};

	public ILHookConfig ILHookConfig => new ILHookConfig
	{
		Priority = Priority,
		ID = ID,
		Before = Before,
		After = After
	};

	internal bool IsValid
	{
		get
		{
			if (IsDisposed)
			{
				return false;
			}
			if (Creator == null)
			{
				return true;
			}
			StackTrace stackTrace = new StackTrace();
			int frameCount = stackTrace.FrameCount;
			for (int i = 0; i < frameCount; i++)
			{
				if (stackTrace.GetFrame(i).GetMethod() == Creator)
				{
					return true;
				}
			}
			return false;
		}
	}

	public DetourContext(int priority, string id)
	{
		StackTrace stackTrace = new StackTrace();
		int frameCount = stackTrace.FrameCount;
		for (int i = 0; i < frameCount; i++)
		{
			MethodBase method = stackTrace.GetFrame(i).GetMethod();
			if (method?.DeclaringType != typeof(DetourContext))
			{
				Creator = method;
				break;
			}
		}
		_FallbackID = Creator?.DeclaringType?.Assembly?.GetName().Name ?? Creator?.GetID(null, null, withType: true, proxyMethod: false, simple: true);
		Last = this;
		Contexts.Add(this);
		Priority = priority;
		ID = id;
	}

	public DetourContext(string id)
		: this(0, id)
	{
	}

	public DetourContext(int priority)
		: this(priority, null)
	{
	}

	public DetourContext()
		: this(0, null)
	{
	}

	public void Dispose()
	{
		if (IsDisposed)
		{
			IsDisposed = true;
			Last = null;
			Contexts.Remove(this);
		}
	}
}
