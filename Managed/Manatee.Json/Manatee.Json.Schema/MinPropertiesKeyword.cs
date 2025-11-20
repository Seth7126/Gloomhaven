using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class MinPropertiesKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<MinPropertiesKeyword>
{
	public static string ErrorTemplate { get; set; } = "The array should contain at least {{lowerBound}} properties, but {{actual}} were found.";

	public string Name => "minProperties";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public uint Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public MinPropertiesKeyword()
	{
	}

	public MinPropertiesKeyword(uint value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Object)
		{
			Log.Schema(() => "Instance not a string; not applicable");
			return schemaValidationResults;
		}
		if (context.Instance.Object.Count < Value)
		{
			Log.Schema(() => $"Bounds check failed: {context.Instance.Object.Count} < {Value}");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["lowerBound"] = Value;
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance.Object.Count;
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

	public bool Equals(MinPropertiesKeyword? other)
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
		return Equals(other as MinPropertiesKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as MinPropertiesKeyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
