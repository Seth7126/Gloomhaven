using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class EnumNameSerializer : IPrioritizedSerializer, ISerializer
{
	private class Description
	{
		public object Value { get; }

		public string Name { get; }

		public Description(object value, string name)
		{
			Value = value;
			Name = name;
		}
	}

	private static readonly Dictionary<Type, List<Description>> _descriptions = new Dictionary<Type, List<Description>>();

	public int Priority => 2;

	public bool ShouldMaintainReferences => false;

	public bool Handles(SerializationContextBase context)
	{
		DeserializationContext deserializationContext = context as DeserializationContext;
		if (context.InferredType.GetTypeInfo().IsEnum)
		{
			if (deserializationContext != null || context.RootSerializer.Options.EnumSerializationFormat != EnumSerializationFormat.AsName)
			{
				if (deserializationContext != null)
				{
					JsonValue localValue = deserializationContext.LocalValue;
					if ((object)localValue == null)
					{
						return false;
					}
					return localValue.Type == JsonValueType.String;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public JsonValue Serialize(SerializationContext context)
	{
		Type type = _GetType(context.InferredType);
		_EnsureDescriptions(type);
		if (type.GetTypeInfo().GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Any())
		{
			return _BuildFlagsValues(context.InferredType, context.Source, context.RootSerializer.Options);
		}
		Description description = _descriptions[type].FirstOrDefault((Description d) => object.Equals(d.Value, context.Source));
		if (description != null)
		{
			return description.Name;
		}
		return context.RootSerializer.Options.SerializationNameTransform(context.Source.ToString());
	}

	public object Deserialize(DeserializationContext context)
	{
		Type type = _GetType(context.InferredType);
		_EnsureDescriptions(type);
		StringComparison options = (context.RootSerializer.Options.CaseSensitiveDeserialization ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		Description description = _descriptions[type].FirstOrDefault((Description d) => string.Equals(d.Name, context.LocalValue.String, options));
		if (description == null)
		{
			string value = context.RootSerializer.Options.DeserializationNameTransform(context.LocalValue.String);
			return Enum.Parse(type, value, !context.RootSerializer.Options.CaseSensitiveDeserialization);
		}
		return description.Value;
	}

	private static Type _GetType(Type type)
	{
		TypeInfo typeInfo = type.GetTypeInfo();
		if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			type = typeInfo.GenericTypeArguments[0];
		}
		return type;
	}

	private static void _EnsureDescriptions(Type type)
	{
		lock (_descriptions)
		{
			if (!_descriptions.ContainsKey(type))
			{
				List<Description> value = (from object n in Enum.GetValues(type)
					select new Description(n, _GetDescription(type, n.ToString()))).ToList();
				_descriptions.Add(type, value);
			}
		}
	}

	private static string _GetDescription(Type type, string name)
	{
		object[] customAttributes = type.GetTypeInfo().GetDeclaredField(name).GetCustomAttributes(typeof(DisplayAttribute), inherit: false);
		if (!customAttributes.Any())
		{
			return name;
		}
		return ((DisplayAttribute)customAttributes.First()).Description;
	}

	private static string _BuildFlagsValues(Type type, object obj, JsonSerializerOptions options)
	{
		List<Description> list = _descriptions[type];
		long num = Convert.ToInt64(obj);
		int num2 = list.Count - 1;
		List<string> list2 = new List<string>();
		while (num > 0 && num2 > 0)
		{
			long num3 = Convert.ToInt64(list[num2].Value);
			if (num >= num3)
			{
				list2.Insert(0, list[num2].Name);
				num -= num3;
			}
			num2--;
		}
		return string.Join(options.FlagsEnumSeparator, list2.Select<string, string>(options.SerializationNameTransform));
	}
}
