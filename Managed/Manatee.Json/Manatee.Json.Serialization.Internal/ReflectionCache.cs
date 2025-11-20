using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Manatee.Json.Internal;

namespace Manatee.Json.Serialization.Internal;

internal static class ReflectionCache
{
	private class ReflectionInfo
	{
		public IEnumerable<SerializationInfo> ReadOnlyProperties { get; }

		public IEnumerable<SerializationInfo> ReadWriteProperties { get; }

		public IEnumerable<SerializationInfo> Fields { get; }

		public ReflectionInfo(IEnumerable<SerializationInfo> readOnlyProperties, IEnumerable<SerializationInfo> readWriteProperties, IEnumerable<SerializationInfo> fields)
		{
			ReadOnlyProperties = readOnlyProperties.ToList();
			ReadWriteProperties = readWriteProperties.ToList();
			Fields = fields.ToList();
		}
	}

	private static readonly ConcurrentDictionary<Type, ReflectionInfo> _instanceCache;

	private static readonly ConcurrentDictionary<Type, ReflectionInfo> _staticCache;

	static ReflectionCache()
	{
		_instanceCache = new ConcurrentDictionary<Type, ReflectionInfo>();
		_staticCache = new ConcurrentDictionary<Type, ReflectionInfo>();
	}

	public static IEnumerable<SerializationInfo> GetMembers(Type type, PropertySelectionStrategy propertyTypes, bool includeFields)
	{
		ReflectionInfo info = _InitializeInstanceCache(type);
		List<SerializationInfo> list = new List<SerializationInfo>();
		_GetProperties(info, propertyTypes, list);
		if (includeFields)
		{
			_GetFields(info, list);
		}
		return list;
	}

	public static IEnumerable<SerializationInfo> GetTypeMembers(Type type, PropertySelectionStrategy propertyTypes, bool includeFields)
	{
		ReflectionInfo info = _InitializeStaticCache(type);
		List<SerializationInfo> list = new List<SerializationInfo>();
		_GetProperties(info, propertyTypes, list);
		if (includeFields)
		{
			_GetFields(info, list);
		}
		return list;
	}

	private static void _GetProperties(ReflectionInfo info, PropertySelectionStrategy propertyTypes, List<SerializationInfo> properties)
	{
		if (propertyTypes.HasFlag(PropertySelectionStrategy.ReadWriteOnly))
		{
			properties.AddRange(info.ReadWriteProperties);
		}
		if (propertyTypes.HasFlag(PropertySelectionStrategy.ReadOnly))
		{
			properties.AddRange(info.ReadOnlyProperties);
		}
	}

	private static void _GetFields(ReflectionInfo info, List<SerializationInfo> fields)
	{
		fields.AddRange(info.Fields);
	}

	private static ReflectionInfo _InitializeInstanceCache(Type type)
	{
		if (!_instanceCache.TryGetValue(type, out ReflectionInfo value))
		{
			IEnumerable<SerializationInfo> readOnlyProperties = (from p in type._GetInstanceProperties().Where(delegate(PropertyInfo p)
				{
					MethodInfo setMethod = p.SetMethod;
					return (object)setMethod == null || !setMethod.IsPublic;
				})
				where p.GetMethod?.IsPublic ?? false
				where !p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
				select p).Select(_BuildSerializationInfo);
			IEnumerable<SerializationInfo> readWriteProperties = (from p in type._GetInstanceProperties()
				where p.SetMethod?.IsPublic ?? false
				where p.GetMethod?.IsPublic ?? false
				where !p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
				select p).Select(_BuildSerializationInfo);
			IEnumerable<SerializationInfo> fields = (from p in type._GetInstanceFields()
				where !p.IsInitOnly
				where !p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
				select p).Select(_BuildSerializationInfo);
			value = (_instanceCache[type] = new ReflectionInfo(readOnlyProperties, readWriteProperties, fields));
		}
		return value;
	}

	private static ReflectionInfo _InitializeStaticCache(Type type)
	{
		if (!_staticCache.TryGetValue(type, out ReflectionInfo value))
		{
			IEnumerable<SerializationInfo> readOnlyProperties = (from p in type._GetStaticProperties().Where(delegate(PropertyInfo p)
				{
					MethodInfo setMethod = p.SetMethod;
					return (object)setMethod == null || !setMethod.IsPublic;
				})
				where p.GetMethod?.IsPublic ?? false
				where !p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
				select p).Select(_BuildSerializationInfo);
			IEnumerable<SerializationInfo> readWriteProperties = (from p in type._GetStaticProperties()
				where p.SetMethod?.IsPublic ?? false
				where p.GetMethod?.IsPublic ?? false
				where !p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
				select p).Select(_BuildSerializationInfo);
			IEnumerable<SerializationInfo> fields = (from p in type._GetStaticFields()
				where !p.IsInitOnly
				where !p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
				select p).Select(_BuildSerializationInfo);
			value = (_staticCache[type] = new ReflectionInfo(readOnlyProperties, readWriteProperties, fields));
		}
		return value;
	}

	private static SerializationInfo _BuildSerializationInfo(MemberInfo info)
	{
		JsonMapToAttribute jsonMapToAttribute = (JsonMapToAttribute)info.GetCustomAttributes(typeof(JsonMapToAttribute), inherit: false).FirstOrDefault();
		string serializationName = ((jsonMapToAttribute == null) ? info.Name : jsonMapToAttribute.MapToKey);
		return new SerializationInfo(info, serializationName, jsonMapToAttribute == null);
	}

	private static IEnumerable<PropertyInfo> _GetInstanceProperties(this Type type)
	{
		return type.GetTypeInfo().GetAllProperties().Where(delegate(PropertyInfo p)
		{
			MethodInfo getMethod = p.GetMethod;
			return (object)getMethod != null && !getMethod.IsStatic && (p.GetMethod?.IsPublic ?? false);
		});
	}

	private static IEnumerable<PropertyInfo> _GetStaticProperties(this Type type)
	{
		return type.GetTypeInfo().GetAllProperties().Where(delegate(PropertyInfo p)
		{
			MethodInfo getMethod = p.GetMethod;
			return (object)getMethod != null && getMethod.IsStatic && (p.GetMethod?.IsPublic ?? false);
		});
	}

	private static IEnumerable<FieldInfo> _GetInstanceFields(this Type type)
	{
		return from f in type.GetTypeInfo()._GetAllFields()
			where !f.IsStatic && f.IsPublic
			select f;
	}

	private static IEnumerable<FieldInfo> _GetStaticFields(this Type type)
	{
		return from f in type.GetTypeInfo()._GetAllFields()
			where f.IsStatic && f.IsPublic
			select f;
	}

	private static IEnumerable<FieldInfo> _GetAllFields(this TypeInfo? type)
	{
		List<FieldInfo> list = new List<FieldInfo>();
		while (type != null)
		{
			list.AddRange(type.DeclaredFields);
			type = type.BaseType?.GetTypeInfo();
		}
		return list;
	}
}
