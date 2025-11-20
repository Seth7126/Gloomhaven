using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class ExclusiveMaximumDraft04Keyword : IJsonSchemaKeywordPlus, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ExclusiveMaximumDraft04Keyword>
{
	public static string ErrorTemplate { get; set; } = "The value {{actual}} should be strictly less than {{upperBound}}.";

	public string Name => "exclusiveMaximum";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft04;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.None;

	public bool Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public ExclusiveMaximumDraft04Keyword()
	{
	}

	public ExclusiveMaximumDraft04Keyword(bool value)
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
		MaximumKeyword keyword = context.Local.Get<MaximumKeyword>();
		if (keyword == null)
		{
			Log.Schema(() => "`maximum` keyword not defined; not applicable");
			return schemaValidationResults;
		}
		if (!Value)
		{
			Log.Schema(() => "Not exclusive; see `maximum` results");
			return schemaValidationResults;
		}
		if (context.Instance.Number >= keyword.Value)
		{
			Log.Schema(() => $"Bounds check failed: {context.Instance.Number} >= {keyword.Value}");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["upperBound"] = keyword.Value;
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance;
			schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
		}
		return schemaValidationResults;
	}

	bool IJsonSchemaKeywordPlus.Handles(JsonValue value)
	{
		return value.Type == JsonValueType.Boolean;
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
		Value = json.Boolean;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value;
	}

	public bool Equals(ExclusiveMaximumDraft04Keyword? other)
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
		return Equals(other as ExclusiveMaximumDraft04Keyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ExclusiveMaximumDraft04Keyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
