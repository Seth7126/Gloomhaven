using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class EnumKeyword : List<JsonValue>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<EnumKeyword>
{
	public static string ErrorTemplate { get; set; } = "{{value}} does not match any of the required values.";

	public string Name => "enum";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	[DeserializationUseOnly]
	[UsedImplicitly]
	public EnumKeyword()
	{
	}

	public EnumKeyword(params JsonValue[] values)
		: base((IEnumerable<JsonValue>)values)
	{
	}

	public EnumKeyword(IEnumerable<JsonValue> values)
		: base(values)
	{
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		Log.Schema(() => "Checking defined values for instance");
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context)
		{
			IsValid = Contains(context.Instance)
		};
		if (!schemaValidationResults.IsValid)
		{
			Log.Schema(() => "Instance does not match any of the defined values");
			schemaValidationResults.AdditionalInfo["value"] = context.Instance;
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
		AddRange(json.Array);
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return new JsonArray(this)
		{
			EqualityStandard = ArrayEquality.ContentsEqual
		};
	}

	public bool Equals(EnumKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return this.ContentsEqual(other);
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as EnumKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as EnumKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
