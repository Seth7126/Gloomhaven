using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class ConstKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ConstKeyword>
{
	public static string ErrorTemplate { get; set; } = "The value {{actual}} does not match the expected {{expected}}.";

	public string Name => "const";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public JsonValue Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public ConstKeyword()
	{
	}

	public ConstKeyword(JsonValue value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (context.Instance == Value)
		{
			return new SchemaValidationResults(Name, context);
		}
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context)
		{
			IsValid = false,
			Keyword = Name,
			AdditionalInfo = 
			{
				["expected"] = Value,
				["actual"] = context.Instance
			}
		};
		schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
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
		Value = json;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value;
	}

	public bool Equals(ConstKeyword? other)
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
		return Equals(other as ConstKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ConstKeyword);
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
