using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class ExamplesKeyword : List<JsonValue>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ExamplesKeyword>
{
	public string Name => "examples";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.MetaData;

	[DeserializationUseOnly]
	[UsedImplicitly]
	public ExamplesKeyword()
	{
	}

	public ExamplesKeyword(params JsonValue[] values)
		: base((IEnumerable<JsonValue>)values)
	{
	}

	public ExamplesKeyword(IEnumerable<JsonValue> values)
		: base(values)
	{
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		return new SchemaValidationResults(Name, context)
		{
			AnnotationValue = this.ToJson()
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
		AddRange(json.Array);
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return new JsonArray(this)
		{
			EqualityStandard = ArrayEquality.ContentsEqual
		};
	}

	public bool Equals(ExamplesKeyword? other)
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
		return Equals(other as ExamplesKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ExamplesKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
