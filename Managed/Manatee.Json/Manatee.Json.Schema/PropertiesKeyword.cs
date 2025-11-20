using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class PropertiesKeyword : Dictionary<string, JsonSchema>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<PropertiesKeyword>
{
	public static string ErrorTemplate { get; set; } = "At least one subschema failed validation.";

	public string Name => "properties";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Object)
		{
			Log.Schema(() => "Instance not an object; not applicable");
			return schemaValidationResults;
		}
		bool flag = true;
		bool flag2 = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		JsonObject jsonObject = context.Instance.Object;
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		using (Dictionary<string, JsonSchema>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JsonSchema> property = enumerator.Current;
				if (!jsonObject.ContainsKey(property.Key))
				{
					Log.Schema(() => "Property " + property.Key + " not found; skipping");
					continue;
				}
				context.EvaluatedPropertyNames.Add(property.Key);
				context.LocallyEvaluatedPropertyNames.Add(property.Key);
				SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
				schemaValidationContext.Instance = jsonObject[property.Key];
				schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name, property.Key);
				schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name, property.Key);
				schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(property.Key);
				SchemaValidationContext schemaValidationContext2 = schemaValidationContext;
				SchemaValidationResults schemaValidationResults2 = property.Value.Validate(schemaValidationContext2);
				context.EvaluatedPropertyNames.UnionWith(schemaValidationContext2.EvaluatedPropertyNames);
				context.EvaluatedPropertyNames.UnionWith(schemaValidationContext2.LocallyEvaluatedPropertyNames);
				flag &= schemaValidationResults2.IsValid;
				if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
				{
					if (!flag)
					{
						Log.Schema(() => "Subschema failed; halting validation early");
						break;
					}
				}
				else if (flag2)
				{
					list.Add(schemaValidationResults2);
				}
			}
		}
		schemaValidationResults.IsValid = flag;
		schemaValidationResults.NestedResults = list;
		if (!schemaValidationResults.IsValid)
		{
			schemaValidationResults.ErrorMessage = ErrorTemplate;
		}
		return schemaValidationResults;
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

	public bool Equals(PropertiesKeyword? other)
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
			ThisProperty = tk.Value,
			OtherProperty = ok.Value
		}).ToList().All(k => object.Equals(k.ThisProperty, k.OtherProperty));
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as PropertiesKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as PropertiesKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
