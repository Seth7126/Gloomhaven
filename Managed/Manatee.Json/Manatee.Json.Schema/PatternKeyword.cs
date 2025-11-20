using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class PatternKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<PatternKeyword>
{
	public static string ErrorTemplate { get; set; } = "{{actual}} should match the Regular Expression `{{pattern}}`.";

	public string Name => "pattern";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public Regex Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public PatternKeyword()
	{
	}

	public PatternKeyword(Regex value)
	{
		Value = value;
	}

	public PatternKeyword(string value)
	{
		Value = new Regex(value, RegexOptions.Compiled);
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.String)
		{
			Log.Schema(() => "Instance not a string; not applicable");
			return schemaValidationResults;
		}
		if (!Value.IsMatch(context.Instance.String))
		{
			Log.Schema(() => "Value does not match regular expression");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance;
			schemaValidationResults.AdditionalInfo["pattern"] = Value.ToString();
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
		Value = new Regex(json.String, RegexOptions.Compiled);
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value.ToString();
	}

	public bool Equals(PatternKeyword? other)
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
		return Equals(other as PatternKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as PatternKeyword);
	}

	public override int GetHashCode()
	{
		if (Value == null)
		{
			return 0;
		}
		return Value.GetHashCode();
	}
}
