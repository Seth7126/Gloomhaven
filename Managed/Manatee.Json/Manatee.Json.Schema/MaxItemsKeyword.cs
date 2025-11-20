using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class MaxItemsKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<MaxItemsKeyword>
{
	public static string ErrorTemplate { get; set; } = "The array should contain at most {{upperBound}} items, but {{actual}} were found.";

	public string Name => "maxItems";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public uint Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public MaxItemsKeyword()
	{
	}

	public MaxItemsKeyword(uint value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Array)
		{
			Log.Schema(() => "Instance not an array; not applicable");
			return schemaValidationResults;
		}
		if (context.Instance.Array.Count > Value)
		{
			Log.Schema(() => $"Bounds check failed: {context.Instance.Array.Count} > {Value}");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["upperBound"] = Value;
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance.Array.Count;
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

	public bool Equals(MaxItemsKeyword? other)
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
		return Equals(other as MaxItemsKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as MaxItemsKeyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
