using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Manatee.Json.Internal;

namespace Manatee.Json.Serialization.Internal;

internal static class TemplateGenerator
{
	private static readonly MethodInfo _buildMethod;

	private static readonly Dictionary<Type, MethodInfo> _buildMethods;

	private static readonly Dictionary<Type, object> _defaultInstances;

	[ThreadStatic]
	private static List<Type> _generatedTypes;

	static TemplateGenerator()
	{
		_buildMethod = typeof(TemplateGenerator).GetTypeInfo().GetDeclaredMethod("_BuildInstance");
		_buildMethods = new Dictionary<Type, MethodInfo>();
		_defaultInstances = new Dictionary<Type, object>
		{
			{
				typeof(string),
				string.Empty
			},
			{
				typeof(Guid),
				Guid.Empty
			}
		};
	}

	public static JsonValue FromType<T>(JsonSerializer serializer)
	{
		bool encodeDefaultValues = serializer.Options.EncodeDefaultValues;
		serializer.Options.EncodeDefaultValues = true;
		serializer.Options.IncludeContentSample = true;
		_generatedTypes = new List<Type>();
		T obj = _BuildInstance<T>(serializer);
		JsonValue result = serializer.Serialize(obj);
		serializer.Options.IncludeContentSample = false;
		serializer.Options.EncodeDefaultValues = encodeDefaultValues;
		return result;
	}

	private static T _BuildInstance<T>(JsonSerializer serializer)
	{
		Type typeFromHandle = typeof(T);
		JsonSerializerOptions options = serializer.Options;
		if (_defaultInstances.ContainsKey(typeFromHandle))
		{
			return (T)_defaultInstances[typeFromHandle];
		}
		if (_generatedTypes.Contains(typeFromHandle))
		{
			return default(T);
		}
		_generatedTypes.Add(typeFromHandle);
		T val;
		if (typeFromHandle.GetTypeInfo().IsGenericType && typeFromHandle.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			val = (T)GetBuildMethod(typeFromHandle.GetTypeArguments().First()).Invoke(null, new object[1] { options });
		}
		else
		{
			val = (T)serializer.AbstractionMap.CreateInstance(typeof(T), null, options.Resolver);
			_FillProperties(val, options);
			if (options.AutoSerializeFields)
			{
				_FillFields(val, options);
			}
		}
		_defaultInstances[typeFromHandle] = val;
		return val;
	}

	private static void _FillProperties<T>(T instance, JsonSerializerOptions options)
	{
		foreach (PropertyInfo item in from p in typeof(T).GetTypeInfo().DeclaredProperties
			where p.SetMethod != null
			where p.GetMethod != null
			where !p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
			select p)
		{
			Type propertyType = item.PropertyType;
			if (!item.GetIndexParameters().ToList().Any())
			{
				object value = _GetValue(options, propertyType);
				item.SetValue(instance, value, null);
			}
		}
	}

	private static void _FillFields<T>(T instance, JsonSerializerOptions options)
	{
		foreach (FieldInfo item in from p in typeof(T).GetTypeInfo().DeclaredFields
			where !p.IsInitOnly
			where !p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
			select p)
		{
			object value = GetBuildMethod(item.FieldType).Invoke(null, new object[1] { options });
			item.SetValue(instance, value);
		}
	}

	private static object? _GetValue(JsonSerializerOptions options, Type propertyType)
	{
		return GetBuildMethod(propertyType).Invoke(null, new object[1] { options });
	}

	internal static MethodInfo GetBuildMethod(Type type)
	{
		if (!_buildMethods.TryGetValue(type, out MethodInfo value))
		{
			value = _buildMethod.MakeGenericMethod(type);
			_buildMethods[type] = value;
		}
		return value;
	}
}
