using System;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public class MinContainsKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<MinContainsKeyword>
{
	public static string ErrorTemplate { get; set; } = "The array should contain at least {{lowerBound}} items that match the schema, but {{actual}} were found.";

	public string Name => "minContains";

	public JsonSchemaVersion SupportedVersions => JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 2;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public uint Value { get; set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public MinContainsKeyword()
	{
	}

	public MinContainsKeyword(uint value)
	{
		Value = value;
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		Value = (uint)json.Number;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value;
	}

	public bool Equals(MinContainsKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return Value == other.Value;
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as MinContainsKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as MinContainsKeyword);
	}

	public override int GetHashCode()
	{
		return (int)Value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Array)
		{
			Log.Schema(() => "Instance not an array; not applicable");
			return schemaValidationResults;
		}
		if (!context.Misc.TryGetValue("containsCount", out object value))
		{
			Log.Schema(() => "`contains` keyword not present; not applicable");
			return schemaValidationResults;
		}
		int containsCount = (int)value;
		if (containsCount < Value)
		{
			Log.Schema(() => $"Required at least {Value} matching items, but only {containsCount} found");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["actual"] = containsCount;
			schemaValidationResults.AdditionalInfo["lowerBound"] = Value;
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
}
