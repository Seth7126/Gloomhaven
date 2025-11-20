using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class ContentSchemaKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ContentSchemaKeyword>
{
	public string Name => "contentSchema";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Content;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public ContentSchemaKeyword()
	{
	}

	public ContentSchemaKeyword(JsonSchema value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (context.Instance.Type != JsonValueType.String)
		{
			Log.Schema(() => "Instance not a string; not applicable");
			return new SchemaValidationResults(Name, context);
		}
		JsonPointer baseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
		JsonPointer relativeLocation = context.RelativeLocation.CloneAndAppend(Name);
		SchemaValidationResults schemaValidationResults = Value.Validate(new SchemaValidationContext(context)
		{
			BaseRelativeLocation = baseRelativeLocation,
			RelativeLocation = relativeLocation,
			InstanceLocation = context.InstanceLocation.CloneAndAppend(Name)
		});
		SchemaValidationResults schemaValidationResults2 = new SchemaValidationResults(Name, context)
		{
			IsValid = schemaValidationResults.IsValid
		};
		if (JsonSchemaOptions.OutputFormat != SchemaValidationOutputFormat.Flag)
		{
			schemaValidationResults2.NestedResults = new List<SchemaValidationResults> { schemaValidationResults };
		}
		return schemaValidationResults2;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		Value.RegisterSubschemas(baseUri, localRegistry);
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return Value.ResolveSubschema(pointer, baseUri);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		Value = serializer.Deserialize<JsonSchema>(json);
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return serializer.Serialize(Value);
	}

	public bool Equals(ContentSchemaKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(Value, other.Value);
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as ContentSchemaKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ContentSchemaKeyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
