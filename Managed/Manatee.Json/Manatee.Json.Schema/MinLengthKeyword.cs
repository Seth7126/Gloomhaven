using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class MinLengthKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<MinLengthKeyword>
{
	public static string ErrorTemplate { get; set; } = "The string should be at least {{lowerBound}} characters long, but was {{actual}}.";

	public string Name => "minLength";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public uint Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public MinLengthKeyword()
	{
	}

	public MinLengthKeyword(uint value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.String)
		{
			Log.Schema(() => "Instance not a string; not applicable");
			return schemaValidationResults;
		}
		int length = new StringInfo(context.Instance.String).LengthInTextElements;
		if (length < Value)
		{
			Log.Schema(() => $"Bounds check failed: {length} < {Value}");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["lowerBound"] = Value;
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance.String.Length;
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
		Value = (uint)json.Number;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value;
	}

	public bool Equals(MinLengthKeyword? other)
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
		return Equals(other as MinLengthKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as MinLengthKeyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
