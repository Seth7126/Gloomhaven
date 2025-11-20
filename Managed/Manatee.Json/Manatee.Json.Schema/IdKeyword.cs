using System;
using System.Diagnostics;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class IdKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<IdKeyword>
{
	public virtual string Name => "$id";

	public virtual JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public virtual SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

	public string Value { get; private set; }

	[DeserializationUseOnly]
	public IdKeyword()
	{
	}

	public IdKeyword(string value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		return SchemaValidationResults.Null;
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
		Value = json.String;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value;
	}

	public bool Equals(IdKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Name == other.Name)
		{
			return object.Equals(Value, other.Value);
		}
		return false;
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as IdKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as IdKeyword);
	}

	public override int GetHashCode()
	{
		return Value?.GetHashCode() ?? 0;
	}
}
