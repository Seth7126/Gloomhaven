using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Photon.Bolt;

internal static class BoltStateMetas
{
	private static Dictionary<TypeId, NetworkState_Meta> boltStateMetas;

	internal static NetworkState_Meta GetStateMeta(TypeId typeId)
	{
		if (boltStateMetas == null)
		{
			FetchStateMetas();
		}
		if (boltStateMetas.TryGetValue(typeId, out var value))
		{
			return value;
		}
		return null;
	}

	private static void FetchStateMetas()
	{
		boltStateMetas = new Dictionary<TypeId, NetworkState_Meta>();
		IEnumerable<Type> enumerable = FindImplementations(typeof(NetworkState_Meta));
		foreach (Type item in enumerable)
		{
			FieldInfo field = item.GetField("Instance", BindingFlags.Static | BindingFlags.NonPublic);
			if (field != null)
			{
				NetworkState_Meta networkState_Meta = field.GetValue(null) as NetworkState_Meta;
				RuntimeHelpers.RunClassConstructor(networkState_Meta.GetType().TypeHandle);
				boltStateMetas.Add(networkState_Meta.TypeId, networkState_Meta);
			}
		}
	}

	public static IEnumerable<Type> FindImplementations(Type t)
	{
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.IsClass && t.IsAssignableFrom(type))
					{
						list.Add(type);
					}
				}
			}
			catch
			{
			}
		}
		return list;
	}
}
