using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour.HookGen;

public static class HookEndpointManager
{
	private class HookEntry
	{
		public HookEntryType Type;

		public MethodBase Method;

		public Delegate Hook;

		public override bool Equals(object obj)
		{
			if (obj is HookEntry hookEntry && hookEntry.Type.Equals(Type) && hookEntry.Method.Equals(Method))
			{
				return hookEntry.Hook.Equals(Hook);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode() ^ Method.GetHashCode() ^ Hook.GetHashCode();
		}
	}

	private enum HookEntryType
	{
		Hook,
		Modification
	}

	private static readonly Dictionary<MethodBase, object> HookEndpointMap = new Dictionary<MethodBase, object>();

	private static readonly Dictionary<object, List<HookEntry>> OwnedHookLists = new Dictionary<object, List<HookEntry>>();

	public static event Func<Delegate, object> OnGetOwner;

	public static event Func<object, bool> OnRemoveAllOwnedBy;

	public static event Func<MethodBase, Delegate, bool> OnAdd;

	public static event Func<MethodBase, Delegate, bool> OnRemove;

	public static event Func<MethodBase, Delegate, bool> OnModify;

	public static event Func<MethodBase, Delegate, bool> OnUnmodify;

	public static object GetOwner(Delegate hook)
	{
		return (HookEndpointManager.OnGetOwner ?? new Func<Delegate, object>(DefaultOnGetOwner)).InvokeWhileNull<object>(new object[1] { hook });
	}

	private static object DefaultOnGetOwner(Delegate hook)
	{
		return hook.Method.DeclaringType.Assembly;
	}

	private static HookEndpoint GetEndpoint(MethodBase method)
	{
		object value;
		return (HookEndpoint)(HookEndpointMap.TryGetValue(method, out value) ? (value as HookEndpoint) : (HookEndpointMap[method] = new HookEndpoint(method)));
	}

	private static void AddEntry(HookEntryType type, MethodBase method, Delegate hook)
	{
		object owner = GetOwner(hook);
		if (owner != null)
		{
			if (!OwnedHookLists.TryGetValue(owner, out var value))
			{
				value = (OwnedHookLists[owner] = new List<HookEntry>());
			}
			value.Add(new HookEntry
			{
				Type = type,
				Method = method,
				Hook = hook
			});
		}
	}

	private static void RemoveEntry(HookEntryType type, MethodBase method, Delegate hook)
	{
		object owner = GetOwner(hook);
		if (owner != null && OwnedHookLists.TryGetValue(owner, out var value))
		{
			int num = value.FindLastIndex((HookEntry entry) => entry.Type.Equals(type) && entry.Method.Equals(method) && entry.Hook.Equals(hook));
			if (num != -1)
			{
				value.RemoveAt(num);
			}
		}
	}

	public static void RemoveAllOwnedBy(object owner)
	{
		Func<object, bool> onRemoveAllOwnedBy = HookEndpointManager.OnRemoveAllOwnedBy;
		if ((onRemoveAllOwnedBy != null && !onRemoveAllOwnedBy.InvokeWhileTrue(owner)) || owner == null || !OwnedHookLists.TryGetValue(owner, out var value) || value == null)
		{
			return;
		}
		OwnedHookLists.Remove(owner);
		foreach (HookEntry item in value)
		{
			switch (item.Type)
			{
			case HookEntryType.Hook:
				GetEndpoint(item.Method).Remove(item.Hook);
				break;
			case HookEntryType.Modification:
				GetEndpoint(item.Method).Unmodify(item.Hook);
				break;
			}
		}
	}

	public static void Add<T>(MethodBase method, Delegate hookDelegate) where T : Delegate
	{
		Add(method, hookDelegate);
	}

	public static void Add(MethodBase method, Delegate hookDelegate)
	{
		Func<MethodBase, Delegate, bool> onAdd = HookEndpointManager.OnAdd;
		if (onAdd == null || onAdd.InvokeWhileTrue(method, hookDelegate))
		{
			GetEndpoint(method).Add(hookDelegate);
			AddEntry(HookEntryType.Hook, method, hookDelegate);
		}
	}

	public static void Remove<T>(MethodBase method, Delegate hookDelegate) where T : Delegate
	{
		Remove(method, hookDelegate);
	}

	public static void Remove(MethodBase method, Delegate hookDelegate)
	{
		Func<MethodBase, Delegate, bool> onRemove = HookEndpointManager.OnRemove;
		if (onRemove == null || onRemove.InvokeWhileTrue(method, hookDelegate))
		{
			GetEndpoint(method).Remove(hookDelegate);
			RemoveEntry(HookEntryType.Hook, method, hookDelegate);
		}
	}

	public static void Modify<T>(MethodBase method, Delegate callback) where T : Delegate
	{
		Modify(method, callback);
	}

	public static void Modify(MethodBase method, Delegate callback)
	{
		Func<MethodBase, Delegate, bool> onModify = HookEndpointManager.OnModify;
		if (onModify == null || onModify.InvokeWhileTrue(method, callback))
		{
			GetEndpoint(method).Modify(callback);
			AddEntry(HookEntryType.Modification, method, callback);
		}
	}

	public static void Unmodify<T>(MethodBase method, Delegate callback)
	{
		Unmodify(method, callback);
	}

	public static void Unmodify(MethodBase method, Delegate callback)
	{
		Func<MethodBase, Delegate, bool> onUnmodify = HookEndpointManager.OnUnmodify;
		if (onUnmodify == null || onUnmodify.InvokeWhileTrue(method, callback))
		{
			GetEndpoint(method).Unmodify(callback);
			RemoveEntry(HookEntryType.Modification, method, callback);
		}
	}
}
