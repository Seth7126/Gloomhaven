using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class RecursiveAnchorKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<RecursiveAnchorKeyword>
{
	public string Name => "$recursiveAnchor";

	public JsonSchemaVersion SupportedVersions => JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => int.MinValue;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

	public bool Value { get; set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public RecursiveAnchorKeyword()
	{
	}

	public RecursiveAnchorKeyword(bool value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (context.RecursiveAnchor == null)
		{
			Log.Schema(() => $"Marking recursive anchor at {context.RelativeLocation}");
			context.RecursiveAnchor = context.Local;
		}
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
		Value = json.Boolean;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value;
	}

	public bool Equals(RecursiveAnchorKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return string.Equals(Name, other.Name);
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as RecursiveAnchorKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as RecursiveAnchorKeyword);
	}

	public override int GetHashCode()
	{
		if (Name == null)
		{
			return 0;
		}
		return Name.GetHashCode();
	}
}
