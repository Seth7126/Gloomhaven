using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class ExclusiveMinimumKeyword : IJsonSchemaKeywordPlus, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ExclusiveMinimumKeyword>
{
	public static string ErrorTemplate { get; set; } = "The value {{actual}} should be strictly greater than {{lowerBound}}.";

	public string Name => "exclusiveMinimum";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public double Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public ExclusiveMinimumKeyword()
	{
	}

	public ExclusiveMinimumKeyword(double value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Number)
		{
			Log.Schema(() => "Instance not a number; not applicable");
			return schemaValidationResults;
		}
		if (context.Instance.Number <= Value)
		{
			Log.Schema(() => $"Bounds check failed: {context.Instance.Number} <= {Value}");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["lowerBound"] = Value;
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance;
			schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
		}
		return schemaValidationResults;
	}

	bool IJsonSchemaKeywordPlus.Handles(JsonValue value)
	{
		return value.Type == JsonValueType.Number;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return null;
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		Value = json.Number;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value;
	}

	public bool Equals(ExclusiveMinimumKeyword? other)
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
		return Equals(other as ExclusiveMinimumKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ExclusiveMinimumKeyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
