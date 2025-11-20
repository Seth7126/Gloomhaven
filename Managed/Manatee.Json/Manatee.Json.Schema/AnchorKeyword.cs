using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class AnchorKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<AnchorKeyword>
{
	public virtual string Name => "$anchor";

	public virtual JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public virtual SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

	public string Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public AnchorKeyword()
	{
	}

	public AnchorKeyword(string value)
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

	public bool Equals(AnchorKeyword? other)
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
		return Equals(other as AnchorKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as AnchorKeyword);
	}

	public override int GetHashCode()
	{
		return Value?.GetHashCode() ?? 0;
	}
}
