using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Manatee.Json.Internal;

namespace Manatee.Json.Serialization.Internal.Serializers;

internal class AutoSerializer : IPrioritizedSerializer, ISerializer, ITypeSerializer
{
	public int Priority => int.MaxValue;

	public bool ShouldMaintainReferences => true;

	public bool Handles(SerializationContextBase context)
	{
		return true;
	}

	public JsonValue Serialize(SerializationContext context)
	{
		JsonObject jsonObject = new JsonObject();
		Type type = context.RequestedType;
		Type type2 = context.Source.GetType();
		bool flag = false;
		if (context.RootSerializer.Options.TypeNameSerializationBehavior != TypeNameSerializationBehavior.Never && (context.RootSerializer.Options.TypeNameSerializationBehavior == TypeNameSerializationBehavior.Always || type.GetTypeInfo().IsAbstract || type.GetTypeInfo().IsInterface || (type != type2 && context.RootSerializer.Options.TypeNameSerializationBehavior != TypeNameSerializationBehavior.OnlyForAbstractions)))
		{
			flag = true;
			jsonObject.Add("$type", type2.AssemblyQualifiedName);
		}
		if (flag || !context.RootSerializer.Options.OnlyExplicitProperties)
		{
			type = context.Source.GetType();
		}
		IEnumerable<SerializationInfo> members = ReflectionCache.GetMembers(type, context.RootSerializer.Options.PropertySelectionStrategy, context.RootSerializer.Options.AutoSerializeFields);
		Dictionary<SerializationInfo, JsonValue> memberMap = _SerializeValues(context, members);
		_ConstructJsonObject(jsonObject, memberMap, context.RootSerializer.Options);
		return jsonObject;
	}

	public JsonValue SerializeType<T>(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		IEnumerable<SerializationInfo> typeMembers = ReflectionCache.GetTypeMembers(typeof(T), serializer.Options.PropertySelectionStrategy, serializer.Options.AutoSerializeFields);
		Dictionary<SerializationInfo, JsonValue> memberMap = _SerializeTypeValues(serializer, typeMembers);
		_ConstructJsonObject(jsonObject, memberMap, serializer.Options);
		if (jsonObject.Count != 0)
		{
			return jsonObject;
		}
		return JsonValue.Null;
	}

	public object Deserialize(DeserializationContext context)
	{
		JsonValue localValue = context.LocalValue;
		Type type = context.RootSerializer.AbstractionMap.IdentifyTypeToResolve(context.InferredType, context.LocalValue);
		IEnumerable<SerializationInfo> members = ReflectionCache.GetMembers(type, context.RootSerializer.Options.PropertySelectionStrategy, context.RootSerializer.Options.AutoSerializeFields);
		Dictionary<SerializationInfo, object> valueMap = _DeserializeValues(context, members, !context.RootSerializer.Options.CaseSensitiveDeserialization);
		context.ValueMap = valueMap;
		if (localValue.Object.Count > 0 && context.RootSerializer.Options.InvalidPropertyKeyBehavior == InvalidPropertyKeyBehavior.ThrowException)
		{
			throw new TypeDoesNotContainPropertyException(type, localValue);
		}
		_AssignObjectProperties(out object obj, type, context);
		return obj;
	}

	public void DeserializeType<T>(JsonValue json, JsonSerializer serializer)
	{
		Type typeFromHandle = typeof(T);
		IEnumerable<SerializationInfo> typeMembers = ReflectionCache.GetTypeMembers(typeFromHandle, serializer.Options.PropertySelectionStrategy, serializer.Options.AutoSerializeFields);
		Dictionary<SerializationInfo, object> memberMap = _DeserializeTypeValues(json, serializer, typeMembers, !serializer.Options.CaseSensitiveDeserialization);
		if (json.Object.Count > 0 && serializer.Options.InvalidPropertyKeyBehavior == InvalidPropertyKeyBehavior.ThrowException)
		{
			throw new TypeDoesNotContainPropertyException(typeFromHandle, json);
		}
		object obj = null;
		_AssignObjectProperties(ref obj, memberMap);
	}

	private static Dictionary<SerializationInfo, JsonValue> _SerializeValues(SerializationContext context, IEnumerable<SerializationInfo> properties)
	{
		JsonSerializer rootSerializer = context.RootSerializer;
		object source = context.Source;
		Dictionary<SerializationInfo, JsonValue> dictionary = new Dictionary<SerializationInfo, JsonValue>();
		foreach (SerializationInfo property in properties)
		{
			object value;
			Type type;
			if (property.MemberInfo is PropertyInfo propertyInfo)
			{
				if (propertyInfo.GetIndexParameters().Length != 0)
				{
					continue;
				}
				value = propertyInfo.GetValue(source, null);
				if (value == null && !rootSerializer.Options.EncodeDefaultValues)
				{
					continue;
				}
				type = propertyInfo.PropertyType;
			}
			else
			{
				FieldInfo fieldInfo = (FieldInfo)property.MemberInfo;
				value = fieldInfo.GetValue(source);
				if (value == null && !rootSerializer.Options.EncodeDefaultValues)
				{
					continue;
				}
				type = fieldInfo.FieldType;
			}
			JsonValue jsonValue = JsonValue.Null;
			if (value != null)
			{
				context.Push(value.GetType(), type, property.SerializationName, value);
				jsonValue = rootSerializer.Serialize(context);
				context.Pop();
			}
			if (!(jsonValue == JsonValue.Null) || rootSerializer.Options.EncodeDefaultValues)
			{
				if (rootSerializer.Options.IncludeContentSample && jsonValue.Type == JsonValueType.Array)
				{
					_AddSample(type, jsonValue.Array, rootSerializer);
				}
				dictionary.Add(property, jsonValue);
			}
		}
		return dictionary;
	}

	private static Dictionary<SerializationInfo, JsonValue> _SerializeTypeValues(JsonSerializer serializer, IEnumerable<SerializationInfo> properties)
	{
		Dictionary<SerializationInfo, JsonValue> dictionary = new Dictionary<SerializationInfo, JsonValue>();
		foreach (SerializationInfo property in properties)
		{
			object value;
			Type type;
			if (property.MemberInfo is PropertyInfo propertyInfo)
			{
				if (propertyInfo.GetIndexParameters().Any())
				{
					continue;
				}
				value = propertyInfo.GetValue(null, null);
				if (value == null)
				{
					continue;
				}
				type = propertyInfo.PropertyType;
			}
			else
			{
				FieldInfo fieldInfo = (FieldInfo)property.MemberInfo;
				value = fieldInfo.GetValue(null);
				if (value == null)
				{
					continue;
				}
				type = fieldInfo.FieldType;
			}
			JsonValue jsonValue = serializer.Serialize(type, value);
			if (!(jsonValue == JsonValue.Null) || serializer.Options.EncodeDefaultValues)
			{
				dictionary.Add(property, jsonValue);
			}
		}
		return dictionary;
	}

	private static void _ConstructJsonObject(JsonObject json, IDictionary<SerializationInfo, JsonValue> memberMap, JsonSerializerOptions options)
	{
		foreach (SerializationInfo key in memberMap.Keys)
		{
			string text = key.SerializationName;
			if (key.ShouldTransform)
			{
				text = options.SerializationNameTransform(text);
			}
			json.Add(text, memberMap[key]);
		}
	}

	private static Dictionary<SerializationInfo, object?> _DeserializeValues(DeserializationContext context, IEnumerable<SerializationInfo> members, bool ignoreCase)
	{
		Dictionary<SerializationInfo, object> dictionary = new Dictionary<SerializationInfo, object>();
		foreach (SerializationInfo member in members)
		{
			string serializationName = member.SerializationName;
			Func<string, string> nameTransform = (member.ShouldTransform ? context.RootSerializer.Options.DeserializationNameTransform : ((Func<string, string>)((string s) => s)));
			HashSet<string> hashSet = new HashSet<string>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
			if (_TryGetKeyValue(context.LocalValue, serializationName, ignoreCase, nameTransform, out JsonValue value) && hashSet.Add(serializationName))
			{
				Type type = ((!(member.MemberInfo is PropertyInfo propertyInfo)) ? ((FieldInfo)member.MemberInfo).FieldType : propertyInfo.PropertyType);
				context.Push(type, member.SerializationName, value);
				object value2 = context.RootSerializer.Deserialize(context);
				context.Pop();
				dictionary.Add(member, value2);
			}
		}
		return dictionary;
	}

	private static Dictionary<SerializationInfo, object?> _DeserializeTypeValues(JsonValue json, JsonSerializer serializer, IEnumerable<SerializationInfo> members, bool ignoreCase)
	{
		Dictionary<SerializationInfo, object> dictionary = new Dictionary<SerializationInfo, object>();
		foreach (SerializationInfo member in members)
		{
			string serializationName = member.SerializationName;
			HashSet<string> hashSet = new HashSet<string>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
			if (_TryGetKeyValue(json, serializationName, ignoreCase, serializer.Options.DeserializationNameTransform, out JsonValue value) && hashSet.Add(serializationName))
			{
				Type type = ((!(member.MemberInfo is PropertyInfo propertyInfo)) ? ((FieldInfo)member.MemberInfo).FieldType : propertyInfo.PropertyType);
				object value2 = serializer.Deserialize(type, value);
				dictionary.Add(member, value2);
			}
		}
		return dictionary;
	}

	private static bool _TryGetKeyValue(JsonValue json, string name, bool ignoreCase, Func<string, string> nameTransform, out JsonValue value)
	{
		StringComparison comparison = (ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		KeyValuePair<string, JsonValue> keyValuePair = json.Object.FirstOrDefault<KeyValuePair<string, JsonValue>>((KeyValuePair<string, JsonValue> k) => string.Equals(nameTransform(k.Key), name, comparison));
		string key = keyValuePair.Key;
		value = keyValuePair.Value;
		return key != null;
	}

	private static void _AssignObjectProperties(out object? obj, Type type, DeserializationContext context)
	{
		obj = (type.GetTypeInfo().IsInterface ? TypeGenerator.Generate(type) : context.RootSerializer.Options.Resolver.Resolve(type, context.ValueMap));
		_AssignObjectProperties(ref obj, context.ValueMap);
	}

	private static void _AssignObjectProperties(ref object? obj, Dictionary<SerializationInfo, object?> memberMap)
	{
		foreach (KeyValuePair<SerializationInfo, object> item in memberMap)
		{
			SerializationInfo key = item.Key;
			if (key.MemberInfo is PropertyInfo propertyInfo)
			{
				if (propertyInfo.CanWrite)
				{
					propertyInfo.SetValue(obj, item.Value, null);
				}
				else if (typeof(IList).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
				{
					IList list = (IList)propertyInfo.GetValue(obj);
					foreach (object item2 in (IList)item.Value)
					{
						list.Add(item2);
					}
				}
				else
				{
					if (!typeof(IDictionary).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
					{
						continue;
					}
					IDictionary dictionary = (IDictionary)propertyInfo.GetValue(obj);
					foreach (DictionaryEntry item3 in (IDictionary)item.Value)
					{
						dictionary.Add(item3.Key, item3.Value);
					}
				}
			}
			else
			{
				((FieldInfo)key.MemberInfo).SetValue(obj, item.Value);
			}
		}
	}

	private static void _AddSample(Type type, JsonArray json, JsonSerializer serializer)
	{
		Type type2 = _GetElementType(type);
		object obj = TemplateGenerator.GetBuildMethod(type2).Invoke(null, new object[1] { serializer.Options });
		json.Add(serializer.Serialize(type2, obj));
	}

	private static Type _GetElementType(Type collectionType)
	{
		if (collectionType.IsArray)
		{
			return collectionType.GetElementType();
		}
		if (collectionType.GetTypeInfo().IsGenericType && collectionType.GetGenericTypeDefinition().InheritsFrom(typeof(IEnumerable<>)))
		{
			return collectionType.GetTypeArguments().First();
		}
		return typeof(object);
	}
}
