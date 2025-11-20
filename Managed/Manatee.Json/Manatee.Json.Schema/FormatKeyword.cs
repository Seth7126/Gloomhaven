using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value.Key}")]
public class FormatKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<FormatKeyword>
{
	public static string ErrorTemplate { get; set; } = "{{actual}} is not in an acceptable {{format}} format.";

	public string Name => "format";

	public JsonSchemaVersion SupportedVersions => Value.SupportedBy;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Format;

	public Format Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public FormatKeyword()
	{
	}

	public FormatKeyword(Format value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context)
		{
			AnnotationValue = Value.Key
		};
		if (!JsonSchemaOptions.ValidateFormatKeyword)
		{
			Log.Schema(() => "Options indicate skipping format validation");
			return schemaValidationResults;
		}
		Format value = Value;
		if (!value.Validate(context.Instance))
		{
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance;
			schemaValidationResults.AdditionalInfo["format"] = value.Key;
			schemaValidationResults.AdditionalInfo["isKnownFormat"] = value.IsKnown;
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
		Value = Format.GetFormat(json.String);
		if (!Value.IsKnown && JsonSchemaOptions.ValidateFormatKeyword && !JsonSchemaOptions.AllowUnknownFormats)
		{
			throw new JsonSerializationException("Unknown format specifier found.  Either allow unknown formats or disable format validation in the JsonSchemaOptions.");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value.Key;
	}

	public bool Equals(FormatKeyword? other)
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
		return Equals(other as FormatKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as FormatKeyword);
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
