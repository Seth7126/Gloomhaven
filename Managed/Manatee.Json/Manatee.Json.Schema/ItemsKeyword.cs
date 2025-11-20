using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class ItemsKeyword : List<JsonSchema>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ItemsKeyword>
{
	public static string ErrorTemplate { get; set; } = "Items at indices {{indices}} failed validation.";

	public string Name => "items";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public bool IsArray { get; set; }

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Array)
		{
			Log.Schema(() => "Instance not an array; not applicable");
			return schemaValidationResults;
		}
		bool flag = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		JsonArray array = context.Instance.Array;
		JsonArray jsonArray = new JsonArray();
		if (IsArray)
		{
			Log.Schema(() => "items is an array; process elements index-aligned");
			for (int num = 0; num < array.Count && num < base.Count; num++)
			{
				SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
				schemaValidationContext.Instance = array[num];
				schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name, num.ToString());
				schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name, num.ToString());
				schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(num.ToString());
				SchemaValidationContext schemaValidationContext2 = schemaValidationContext;
				SchemaValidationResults schemaValidationResults2 = base[num].Validate(schemaValidationContext2);
				if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag && !schemaValidationResults2.IsValid)
				{
					Log.Schema(() => "Subschema failed; halting validation early");
					schemaValidationResults.IsValid = false;
					break;
				}
				if (!schemaValidationResults2.IsValid)
				{
					jsonArray.Add(num);
				}
				else
				{
					context.LocallyValidatedIndices.Add(num);
				}
				if (flag)
				{
					list.Add(base[num].Validate(schemaValidationContext2));
				}
				context.LastEvaluatedIndex = Math.Max(context.LastEvaluatedIndex, num);
				context.LocalTierLastEvaluatedIndex = Math.Max(context.LocalTierLastEvaluatedIndex, num);
				context.UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(schemaValidationContext2);
			}
			schemaValidationResults.IsValid = list.All((SchemaValidationResults r) => r.IsValid);
			schemaValidationResults.NestedResults = list;
		}
		else
		{
			Log.Schema(() => "items is an single subschema; process all elements");
			JsonPointer baseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
			JsonPointer relativeLocation = context.RelativeLocation.CloneAndAppend(Name);
			bool flag2 = true;
			int num2 = 0;
			foreach (JsonValue item in array)
			{
				SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
				schemaValidationContext.Instance = item;
				schemaValidationContext.BaseRelativeLocation = baseRelativeLocation;
				schemaValidationContext.RelativeLocation = relativeLocation;
				schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(num2.ToString());
				SchemaValidationContext schemaValidationContext3 = schemaValidationContext;
				SchemaValidationResults schemaValidationResults3 = base[0].Validate(schemaValidationContext3);
				flag2 &= schemaValidationResults3.IsValid;
				if (!schemaValidationResults3.IsValid)
				{
					jsonArray.Add(num2);
				}
				else
				{
					context.LocallyValidatedIndices.Add(num2);
				}
				context.LastEvaluatedIndex = Math.Max(context.LastEvaluatedIndex, num2);
				context.LocalTierLastEvaluatedIndex = Math.Max(context.LocalTierLastEvaluatedIndex, num2);
				context.UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(schemaValidationContext3);
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
					list.Add(schemaValidationResults3);
				}
				num2++;
			}
			schemaValidationResults.IsValid = flag2;
			schemaValidationResults.NestedResults = list;
		}
		if (!schemaValidationResults.IsValid)
		{
			schemaValidationResults.AdditionalInfo["indices"] = jsonArray;
			schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
		}
		return schemaValidationResults;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		using List<JsonSchema>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.RegisterSubschemas(baseUri, localRegistry);
		}
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		string text = pointer.FirstOrDefault();
		if (text == null)
		{
			return null;
		}
		if (!int.TryParse(text, out var result) || result < 0 || result >= base.Count)
		{
			return null;
		}
		return base[result].ResolveSubschema(new JsonPointer(pointer.Skip(1)), baseUri);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Array)
		{
			AddRange(serializer.Deserialize<List<JsonSchema>>(json));
			IsArray = true;
		}
		else
		{
			Add(serializer.Deserialize<JsonSchema>(json));
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		if (IsArray)
		{
			JsonArray jsonArray = this.Select(serializer.Serialize).ToJson();
			jsonArray.EqualityStandard = ArrayEquality.SequenceEqual;
			return jsonArray;
		}
		return serializer.Serialize(base[0]);
	}

	public bool Equals(ItemsKeyword? other)
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
		return Equals(other as ItemsKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ItemsKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
