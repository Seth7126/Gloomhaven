using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Vocabulary.Id}")]
public class VocabularyKeyword : Dictionary<SchemaVocabulary, bool>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<VocabularyKeyword>
{
	public string Name => "$vocabulary";

	public JsonSchemaVersion SupportedVersions => JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 0;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		foreach (KeyValuePair<string, JsonValue> item in json.Object)
		{
			SchemaVocabulary key = SchemaKeywordCatalog.GetVocabulary(item.Key) ?? new SchemaVocabulary(item.Key);
			base[key] = item.Value.Boolean;
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		using (Dictionary<SchemaVocabulary, bool>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<SchemaVocabulary, bool> current = enumerator.Current;
				jsonObject[current.Key.Id] = current.Value;
			}
		}
		return jsonObject;
	}

	public bool Equals(VocabularyKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return other.ContentsEqual(this);
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as VocabularyKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as VocabularyKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (!context.IsMetaSchemaValidation)
		{
			return SchemaValidationResults.Null;
		}
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		Dictionary<SchemaVocabulary, bool> dictionary = this.ToDictionary<KeyValuePair<SchemaVocabulary, bool>, SchemaVocabulary, bool>((KeyValuePair<SchemaVocabulary, bool> kvp) => kvp.Key, (KeyValuePair<SchemaVocabulary, bool> kvp) => kvp.Value);
		dictionary[SchemaVocabularies.Core] = true;
		foreach (KeyValuePair<SchemaVocabulary, bool> item in dictionary)
		{
			SchemaVocabulary key = item.Key;
			if (key.MetaSchemaId == context.Local.Id)
			{
				continue;
			}
			bool value = item.Value;
			if (key.MetaSchemaId != null)
			{
				SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
				schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name, key.Id);
				schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name, key.Id);
				SchemaValidationContext context2 = schemaValidationContext;
				JsonSchema jsonSchema = JsonSchemaRegistry.Get(key.MetaSchemaId);
				if (jsonSchema != null)
				{
					jsonSchema.Validate(context2);
				}
				else if (value)
				{
					list.Add(new SchemaValidationResults(Name, context2));
				}
			}
		}
		return new SchemaValidationResults(Name, context)
		{
			NestedResults = list,
			IsValid = list.All((SchemaValidationResults r) => r.IsValid)
		};
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return null;
	}
}
