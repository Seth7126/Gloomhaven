using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class DefsKeyword : Dictionary<string, JsonSchema>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<DefsKeyword>
{
	public virtual string Name => "$defs";

	public virtual JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public virtual SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		return new SchemaValidationResults(Name, context);
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		foreach (JsonSchema value in base.Values)
		{
			value.RegisterSubschemas(baseUri, localRegistry);
		}
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		string text = pointer.FirstOrDefault();
		if (text == null)
		{
			return null;
		}
		if (!TryGetValue(text, out JsonSchema value))
		{
			return null;
		}
		return value.ResolveSubschema(new JsonPointer(pointer.Skip(1)), baseUri);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		foreach (KeyValuePair<string, JsonValue> item in json.Object)
		{
			base[item.Key] = serializer.Deserialize<JsonSchema>(item.Value);
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return this.ToDictionary<KeyValuePair<string, JsonSchema>, string, JsonValue>((KeyValuePair<string, JsonSchema> kvp) => kvp.Key, (KeyValuePair<string, JsonSchema> kvp) => serializer.Serialize(kvp.Value)).ToJson();
	}

	public bool Equals(DefsKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return Manatee.Json.Internal.LinqExtensions.FullOuterJoin(this, other, (KeyValuePair<string, JsonSchema> tk) => tk.Key, (KeyValuePair<string, JsonSchema> ok) => ok.Key, (KeyValuePair<string, JsonSchema> tk, KeyValuePair<string, JsonSchema> ok) => new
		{
			ThisDefinition = tk.Value,
			OtherDefinition = ok.Value
		}).ToList().All(k => object.Equals(k.ThisDefinition, k.OtherDefinition));
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as DefsKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as DefsKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
