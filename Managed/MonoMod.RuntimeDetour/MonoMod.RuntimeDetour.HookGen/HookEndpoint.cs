using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour.HookGen;

internal sealed class HookEndpoint
{
	internal readonly MethodBase Method;

	private readonly Dictionary<Delegate, Stack<IDetour>> HookMap = new Dictionary<Delegate, Stack<IDetour>>();

	private readonly List<IDetour> HookList = new List<IDetour>();

	internal HookEndpoint(MethodBase method)
	{
		Method = method;
	}

	private static IDetour _NewHook(MethodBase from, Delegate to)
	{
		return new Hook(from, to);
	}

	private static IDetour _NewILHook(MethodBase from, ILContext.Manipulator to)
	{
		return new ILHook(from, to);
	}

	private void _Add<TDelegate>(Func<MethodBase, TDelegate, IDetour> gen, TDelegate hookDelegate) where TDelegate : Delegate
	{
		if (!(hookDelegate == null))
		{
			if (!HookMap.TryGetValue(hookDelegate, out var value))
			{
				value = (HookMap[hookDelegate] = new Stack<IDetour>());
			}
			IDetour item = gen(Method, hookDelegate);
			value.Push(item);
			HookList.Add(item);
		}
	}

	public void _Remove(Delegate hookDelegate)
	{
		if ((object)hookDelegate != null && HookMap.TryGetValue(hookDelegate, out var value))
		{
			IDetour detour = value.Pop();
			detour.Dispose();
			if (value.Count == 0)
			{
				HookMap.Remove(hookDelegate);
			}
			HookList.Remove(detour);
		}
	}

	public void Add(Delegate hookDelegate)
	{
		_Add(_NewHook, hookDelegate);
	}

	public void Remove(Delegate hookDelegate)
	{
		_Remove(hookDelegate);
	}

	public void Modify(Delegate hookDelegate)
	{
		_Add(_NewILHook, hookDelegate.CastDelegate<ILContext.Manipulator>());
	}

	public void Unmodify(Delegate hookDelegate)
	{
		_Remove(hookDelegate);
	}
}
