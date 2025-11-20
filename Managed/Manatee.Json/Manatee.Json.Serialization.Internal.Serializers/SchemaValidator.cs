using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Manatee.Json.Internal;
using Manatee.Json.Schema;

namespace Manatee.Json.Serialization.Internal.Serializers;

internal class SchemaValidator : IChainedSerializer
{
	private static readonly ConcurrentDictionary<TypeInfo, JsonSchema> _schemas = new ConcurrentDictionary<TypeInfo, JsonSchema>();

	public static SchemaValidator Instance { get; } = new SchemaValidator();

	private SchemaValidator()
	{
	}

	public JsonValue TrySerialize(ISerializer serializer, SerializationContext context)
	{
		throw new NotImplementedException("SchemaValidator is only used for deserialization");
	}

	public object? TryDeserialize(ISerializer serializer, DeserializationContext context)
	{
		JsonSchema jsonSchema = _GetSchema(context.InferredType.GetTypeInfo());
		if (jsonSchema != null)
		{
			SchemaValidationResults schemaValidationResults = jsonSchema.Validate(context.LocalValue);
			if (!schemaValidationResults.IsValid)
			{
				throw new JsonSerializationException($"JSON did not pass schema defined by type '{context.InferredType}'.\n" + "Errors:\n" + context.RootSerializer.Serialize(schemaValidationResults));
			}
		}
		return DefaultValueSerializer.Instance.TryDeserialize(serializer, context);
	}

	private static JsonSchema? _GetSchema(TypeInfo typeInfo)
	{
		return _schemas.GetOrAdd(typeInfo, _GetSchemaSlow);
	}

	private static JsonSchema? _GetSchemaSlow(TypeInfo typeInfo)
	{
		SchemaAttribute customAttribute = typeInfo.GetCustomAttribute<SchemaAttribute>();
		if (customAttribute == null)
		{
			return null;
		}
		Exception innerException = null;
		JsonSchema jsonSchema = null;
		try
		{
			jsonSchema = _GetPropertySchema(typeInfo, customAttribute) ?? _GetFileSchema(customAttribute);
		}
		catch (FileNotFoundException ex)
		{
			innerException = ex;
		}
		catch (UriFormatException ex2)
		{
			innerException = ex2;
		}
		if (jsonSchema == null)
		{
			throw new JsonSerializationException("The value '" + customAttribute.Source + "' could not be translated into a valid schema. This value should represent either a public static property on the " + typeInfo.Name + " type or a file with this name should exist at the execution path.", innerException);
		}
		return jsonSchema;
	}

	private static JsonSchema? _GetPropertySchema(TypeInfo typeInfo, SchemaAttribute attribute)
	{
		string propertyName = attribute.Source;
		return (JsonSchema)(typeInfo.GetAllProperties().FirstOrDefault((PropertyInfo p) => typeof(JsonSchema).GetTypeInfo().IsAssignableFrom(p.PropertyType.GetTypeInfo()) && p.GetMethod != null && p.GetMethod.IsStatic && p.Name == propertyName)?.GetMethod?.Invoke(null, new object[0]));
	}

	private static JsonSchema? _GetFileSchema(SchemaAttribute attribute)
	{
		string text = attribute.Source;
		if (!Uri.TryCreate(text, UriKind.Absolute, out var _))
		{
			text = System.IO.Path.Combine(Directory.GetCurrentDirectory(), text);
		}
		return JsonSchemaRegistry.Get(text);
	}
}
