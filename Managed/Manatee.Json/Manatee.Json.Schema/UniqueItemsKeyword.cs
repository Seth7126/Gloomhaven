using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class UniqueItemsKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<UniqueItemsKeyword>
{
	public static string ErrorTemplate { get; set; } = "Array contains multiple instances of at least one value.";

	public string Name => "uniqueItems";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public bool Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public UniqueItemsKeyword()
	{
	}

	public UniqueItemsKeyword(bool value = true)
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
		if (context.Instance.Array.Distinct().Count() != context.Instance.Array.Count)
		{
			Log.Schema(() => "Instance contains duplicate items");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["value"] = context.Instance;
			schemaValidationResults.ErrorMessage = ErrorTemplate;
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
		Value = json.Boolean;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value;
	}

	public bool Equals(UniqueItemsKeyword? other)
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
		return Equals(other as UniqueItemsKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as UniqueItemsKeyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
