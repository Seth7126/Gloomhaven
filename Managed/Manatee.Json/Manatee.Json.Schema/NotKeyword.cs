using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}")]
public class NotKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<NotKeyword>
{
	public static string ErrorTemplate { get; set; } = "Value should not validate against the schema.";

	public string Name => "not";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Applicator;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public NotKeyword()
	{
	}

	public NotKeyword(JsonSchema value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
		schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
		schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name);
		SchemaValidationContext context2 = schemaValidationContext;
		SchemaValidationResults schemaValidationResults2 = Value.Validate(context2);
		schemaValidationResults.IsValid = !schemaValidationResults2.IsValid;
		if (!schemaValidationResults.IsValid)
		{
			Log.Schema(() => "Subschema succeeded; inverting result");
			schemaValidationResults.ErrorMessage = ErrorTemplate;
		}
		if (JsonSchemaOptions.OutputFormat != SchemaValidationOutputFormat.Flag && JsonSchemaOptions.ShouldReportChildErrors(this, context))
		{
			schemaValidationResults.NestedResults = new List<SchemaValidationResults> { schemaValidationResults2 };
		}
		return schemaValidationResults;
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

	public bool Equals(NotKeyword? other)
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
		return Equals(other as NotKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as NotKeyword);
	}

	public override int GetHashCode()
	{
		if (!(Value != null))
		{
			return 0;
		}
		return Value.GetHashCode();
	}
}
