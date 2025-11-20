using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class MultipleOfKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<MultipleOfKeyword>
{
	public static string ErrorTemplate { get; set; } = "{{actual}} should be a multiple of {{divisor}}.";

	public string Name => "multipleOf";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public double Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public MultipleOfKeyword()
	{
	}

	public MultipleOfKeyword(double value)
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
		if ((decimal)context.Instance.Number % (decimal)Value != default(decimal))
		{
			Log.Schema(() => $"{context.Instance.Number} is not a multiple of {Value}");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["divisor"] = Value;
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance.Number % Value;
			schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
		}
		return schemaValidationResults;
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

	public bool Equals(MultipleOfKeyword? other)
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
		return Equals(other as MultipleOfKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as MultipleOfKeyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
