using System;
using System.Collections.Generic;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public class SchemaDependency : IJsonSchemaDependency, IJsonSerializable, IEquatable<IJsonSchemaDependency>, IEquatable<SchemaDependency>
{
	private readonly JsonSchema _schema;

	public static string ErrorTemplate { get; set; } = "The schema failed validation.";

	public string PropertyName { get; }

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public SchemaDependency(string propertyName, JsonSchema schema)
	{
		if (propertyName == null)
		{
			throw new ArgumentNullException("propertyName");
		}
		if (string.IsNullOrWhiteSpace(propertyName))
		{
			throw new ArgumentException("Must provide a property name.");
		}
		_schema = schema;
		PropertyName = propertyName;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(PropertyName, context)
		{
			Keyword = string.Format("{0}/{1}", context.Misc["dependencyParent"], PropertyName)
		};
		if (context.Instance.Type != JsonValueType.Object)
		{
			Log.Schema(() => "Instance not an object; not applicable");
			return schemaValidationResults;
		}
		if (!context.Instance.Object.ContainsKey(PropertyName))
		{
			Log.Schema(() => "Property " + PropertyName + " not found; not applicable");
			return schemaValidationResults;
		}
		SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
		schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(PropertyName);
		schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(PropertyName);
		SchemaValidationContext context2 = schemaValidationContext;
		SchemaValidationResults schemaValidationResults2 = _schema.Validate(context2);
		schemaValidationResults.NestedResults = new List<SchemaValidationResults> { schemaValidationResults2 };
		if (!schemaValidationResults2.IsValid)
		{
			Log.Schema(() => "Property " + PropertyName + " found, but subschema failed");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.ErrorMessage = ErrorTemplate;
		}
		return schemaValidationResults;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		_schema.RegisterSubschemas(baseUri, localRegistry);
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return _schema.ResolveSubschema(pointer, baseUri);
	}

	public bool Equals(SchemaDependency? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (string.Equals(PropertyName, other.PropertyName))
		{
			return object.Equals(_schema, other._schema);
		}
		return false;
	}

	public bool Equals(IJsonSchemaDependency? other)
	{
		return Equals(other as SchemaDependency);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SchemaDependency);
	}

	public override int GetHashCode()
	{
		return (((_schema != null) ? _schema.GetHashCode() : 0) * 397) ^ ((PropertyName != null) ? PropertyName.GetHashCode() : 0);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return _schema.ToJson(serializer);
	}
}
