using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class ContentMediaTypeKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ContentMediaTypeKeyword>
{
	public string Name => "contentMediaType";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Content;

	public string Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public ContentMediaTypeKeyword()
	{
	}

	public ContentMediaTypeKeyword(string value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		return new SchemaValidationResults(Name, context)
		{
			AnnotationValue = Value
		};
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

	public bool Equals(ContentMediaTypeKeyword? other)
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
		return Equals(other as ContentMediaTypeKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ContentMediaTypeKeyword);
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
