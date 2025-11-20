using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class PatternPropertiesKeyword : Dictionary<string, JsonSchema>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<PatternPropertiesKeyword>
{
	public static string ErrorTemplate { get; set; } = "At least one subschema failed validation.";

	public string Name => "patternProperties";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 2;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Object)
		{
			Log.Schema(() => "Instance not an object; not applicable");
			return schemaValidationResults;
		}
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		JsonObject jsonObject = context.Instance.Object;
		bool flag = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		bool flag2 = true;
		using (Dictionary<string, JsonSchema>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JsonSchema> current = enumerator.Current;
				Regex pattern = new Regex(current.Key);
				JsonSchema value = current.Value;
				IEnumerable<string> matches = jsonObject.Keys.Where((string k) => pattern.IsMatch(k));
				if (matches.Any())
				{
					Log.Schema(() => $"Properties {matches.ToJson()} are matches for regular expression \"{pattern}\"");
					JsonPointer baseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name, current.Key);
					JsonPointer relativeLocation = context.RelativeLocation.CloneAndAppend(Name, current.Key);
					foreach (string item in matches)
					{
						context.EvaluatedPropertyNames.Add(item);
						context.LocallyEvaluatedPropertyNames.Add(item);
						SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
						schemaValidationContext.Instance = jsonObject[item];
						schemaValidationContext.BaseRelativeLocation = baseRelativeLocation;
						schemaValidationContext.RelativeLocation = relativeLocation;
						schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(item);
						SchemaValidationContext schemaValidationContext2 = schemaValidationContext;
						SchemaValidationResults schemaValidationResults2 = value.Validate(schemaValidationContext2);
						flag2 &= schemaValidationResults2.IsValid;
						if (flag2)
						{
							context.UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(schemaValidationContext2);
						}
						if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
						{
							if (!flag2)
							{
								Log.Schema(() => "Subschema failed; halting validation early");
								break;
							}
						}
						else if (flag)
						{
							list.Add(schemaValidationResults2);
						}
					}
				}
				else
				{
					Log.Schema(() => $"No properties found that match regular expression \"{pattern}\"");
				}
			}
		}
		schemaValidationResults.IsValid = flag2;
		if (flag)
		{
			schemaValidationResults.NestedResults = list;
		}
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

	public bool Equals(PatternPropertiesKeyword? other)
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
		return Equals(other as PatternPropertiesKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as PatternPropertiesKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
