using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;

namespace MonoMod.RuntimeDetour;

public sealed class DetourModManager : IDisposable
{
	private readonly Dictionary<IDetour, Assembly> DetourOwners = new Dictionary<IDetour, Assembly>();

	private readonly Dictionary<Assembly, List<IDetour>> OwnedDetourLists = new Dictionary<Assembly, List<IDetour>>();

	public HashSet<Assembly> Ignored = new HashSet<Assembly>();

	private bool Disposed;

	private static readonly string[] HookTypeNames = new string[4] { "MonoMod.RuntimeDetour.NativeDetour", "MonoMod.RuntimeDetour.Detour", "MonoMod.RuntimeDetour.Hook", "MonoMod.RuntimeDetour.ILHook" };

	public event Action<Assembly, MethodBase, ILContext.Manipulator> OnILHook;

	public event Action<Assembly, MethodBase, MethodBase, object> OnHook;

	public event Action<Assembly, MethodBase, MethodBase> OnDetour;

	public event Action<Assembly, MethodBase, IntPtr, IntPtr> OnNativeDetour;

	public DetourModManager()
	{
		Ignored.Add(typeof(DetourModManager).Assembly);
		ILHook.OnDetour = (Func<ILHook, MethodBase, ILContext.Manipulator, bool>)Delegate.Combine(ILHook.OnDetour, new Func<ILHook, MethodBase, ILContext.Manipulator, bool>(RegisterILHook));
		ILHook.OnUndo = (Func<ILHook, bool>)Delegate.Combine(ILHook.OnUndo, new Func<ILHook, bool>(UnregisterDetour));
		Hook.OnDetour = (Func<Hook, MethodBase, MethodBase, object, bool>)Delegate.Combine(Hook.OnDetour, new Func<Hook, MethodBase, MethodBase, object, bool>(RegisterHook));
		Hook.OnUndo = (Func<Hook, bool>)Delegate.Combine(Hook.OnUndo, new Func<Hook, bool>(UnregisterDetour));
		Detour.OnDetour = (Func<Detour, MethodBase, MethodBase, bool>)Delegate.Combine(Detour.OnDetour, new Func<Detour, MethodBase, MethodBase, bool>(RegisterDetour));
		Detour.OnUndo = (Func<Detour, bool>)Delegate.Combine(Detour.OnUndo, new Func<Detour, bool>(UnregisterDetour));
		NativeDetour.OnDetour = (Func<NativeDetour, MethodBase, IntPtr, IntPtr, bool>)Delegate.Combine(NativeDetour.OnDetour, new Func<NativeDetour, MethodBase, IntPtr, IntPtr, bool>(RegisterNativeDetour));
		NativeDetour.OnUndo = (Func<NativeDetour, bool>)Delegate.Combine(NativeDetour.OnUndo, new Func<NativeDetour, bool>(UnregisterDetour));
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			OwnedDetourLists.Clear();
			Hook.OnDetour = (Func<Hook, MethodBase, MethodBase, object, bool>)Delegate.Remove(Hook.OnDetour, new Func<Hook, MethodBase, MethodBase, object, bool>(RegisterHook));
			Hook.OnUndo = (Func<Hook, bool>)Delegate.Remove(Hook.OnUndo, new Func<Hook, bool>(UnregisterDetour));
			Detour.OnDetour = (Func<Detour, MethodBase, MethodBase, bool>)Delegate.Remove(Detour.OnDetour, new Func<Detour, MethodBase, MethodBase, bool>(RegisterDetour));
			Detour.OnUndo = (Func<Detour, bool>)Delegate.Remove(Detour.OnUndo, new Func<Detour, bool>(UnregisterDetour));
			NativeDetour.OnDetour = (Func<NativeDetour, MethodBase, IntPtr, IntPtr, bool>)Delegate.Remove(NativeDetour.OnDetour, new Func<NativeDetour, MethodBase, IntPtr, IntPtr, bool>(RegisterNativeDetour));
			NativeDetour.OnUndo = (Func<NativeDetour, bool>)Delegate.Remove(NativeDetour.OnUndo, new Func<NativeDetour, bool>(UnregisterDetour));
		}
	}

	public void Unload(Assembly asm)
	{
		if (asm == null || Ignored.Contains(asm))
		{
			return;
		}
		HookEndpointManager.RemoveAllOwnedBy(asm);
		if (OwnedDetourLists.TryGetValue(asm, out var value))
		{
			IDetour[] array = value.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
			if (value.Count > 0)
			{
				throw new Exception("Some detours failed to unregister in " + asm.FullName);
			}
			OwnedDetourLists.Remove(asm);
		}
	}

	internal Assembly GetHookOwner(StackTrace stack = null)
	{
		if (stack == null)
		{
			stack = new StackTrace();
		}
		Assembly assembly = null;
		int frameCount = stack.FrameCount;
		string text = null;
		for (int i = 0; i < frameCount; i++)
		{
			MethodBase method = stack.GetFrame(i).GetMethod();
			if (method?.DeclaringType == null)
			{
				continue;
			}
			string fullName = method.DeclaringType.FullName;
			if (text == null)
			{
				if (HookTypeNames.Contains(fullName))
				{
					text = method.DeclaringType.FullName;
				}
			}
			else if (!(fullName == text))
			{
				assembly = method?.DeclaringType.Assembly;
				break;
			}
		}
		if (Ignored.Contains(assembly))
		{
			return null;
		}
		return assembly;
	}

	internal void TrackDetour(Assembly owner, IDetour detour)
	{
		if (!OwnedDetourLists.TryGetValue(owner, out var value))
		{
			value = (OwnedDetourLists[owner] = new List<IDetour>());
		}
		value.Add(detour);
		DetourOwners[detour] = owner;
	}

	internal bool RegisterILHook(ILHook _detour, MethodBase from, ILContext.Manipulator manipulator)
	{
		Assembly hookOwner = GetHookOwner();
		if (hookOwner == null)
		{
			return true;
		}
		this.OnILHook?.Invoke(hookOwner, from, manipulator);
		TrackDetour(hookOwner, _detour);
		return true;
	}

	internal bool RegisterHook(Hook _detour, MethodBase from, MethodBase to, object target)
	{
		Assembly hookOwner = GetHookOwner();
		if (hookOwner == null)
		{
			return true;
		}
		this.OnHook?.Invoke(hookOwner, from, to, target);
		TrackDetour(hookOwner, _detour);
		return true;
	}

	internal bool RegisterDetour(Detour _detour, MethodBase from, MethodBase to)
	{
		Assembly hookOwner = GetHookOwner();
		if (hookOwner == null)
		{
			return true;
		}
		this.OnDetour?.Invoke(hookOwner, from, to);
		TrackDetour(hookOwner, _detour);
		return true;
	}

	internal bool RegisterNativeDetour(NativeDetour _detour, MethodBase method, IntPtr from, IntPtr to)
	{
		Assembly hookOwner = GetHookOwner();
		if (hookOwner == null)
		{
			return true;
		}
		this.OnNativeDetour?.Invoke(hookOwner, method, from, to);
		TrackDetour(hookOwner, _detour);
		return true;
	}

	internal bool UnregisterDetour(IDetour _detour)
	{
		if (DetourOwners.TryGetValue(_detour, out var value))
		{
			DetourOwners.Remove(_detour);
			OwnedDetourLists[value].Remove(_detour);
		}
		return true;
	}
}
