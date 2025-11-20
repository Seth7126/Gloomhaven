using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}")]
public class UnevaluatedItemsKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<UnevaluatedItemsKeyword>
{
	public static string ErrorTemplate { get; set; } = "Items at indices {{indices}} are not covered by `items` or `additionalItems` failed validation.";

	public string Name => "unevaluatedItems";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => int.MaxValue;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public UnevaluatedItemsKeyword()
	{
	}

	public UnevaluatedItemsKeyword(JsonSchema value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (context.Instance.Type != JsonValueType.Array)
		{
			Log.Schema(() => "Instance not an array; not applicable");
			return new SchemaValidationResults(Name, context);
		}
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		JsonArray array = context.Instance.Array;
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		bool flag = true;
		bool flag2 = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		List<int> indicesToEvaluate = Enumerable.Range(0, array.Count).Except(context.ValidatedIndices).ToList();
		JsonArray jsonArray = new JsonArray();
		Log.Schema(() => (!indicesToEvaluate.Any()) ? $"Indices up to {context.LastEvaluatedIndex} have been evaluated; skipping these" : "No indices have been evaluated; process all");
		if (indicesToEvaluate.Any())
		{
			if (Value == JsonSchema.False)
			{
				Log.Schema(() => "Subschema is `false`; all instances invalid");
				schemaValidationResults.IsValid = false;
				schemaValidationResults.Keyword = Name;
				schemaValidationResults.AdditionalInfo["indices"] = indicesToEvaluate.ToJson();
				schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
				return schemaValidationResults;
			}
			foreach (int item in indicesToEvaluate)
			{
				JsonValue instance = array[item];
				JsonPointer baseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
				JsonPointer relativeLocation = context.RelativeLocation.CloneAndAppend(Name);
				SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
				schemaValidationContext.Instance = instance;
				schemaValidationContext.BaseRelativeLocation = baseRelativeLocation;
				schemaValidationContext.RelativeLocation = relativeLocation;
				schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(item.ToString());
				SchemaValidationContext schemaValidationContext2 = schemaValidationContext;
				SchemaValidationResults schemaValidationResults2 = Value.Validate(schemaValidationContext2);
				if (!schemaValidationResults2.IsValid)
				{
					jsonArray.Add(item);
				}
				flag &= schemaValidationResults2.IsValid;
				if (flag)
				{
					context.UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(schemaValidationContext2);
				}
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
		else
		{
			Log.Schema(() => "All items have been validated");
		}
		schemaValidationResults.NestedResults = list;
		schemaValidationResults.IsValid = flag;
		schemaValidationResults.Keyword = Name;
		if (!flag)
		{
			schemaValidationResults.AdditionalInfo["indices"] = jsonArray;
			schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
		}
		return schemaValidationResults;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		Value.RegisterSubschemas(baseUri, localRegistry);
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return Value.ResolveSubschema(pointer, baseUri);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		Value = serializer.Deserialize<JsonSchema>(json);
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return serializer.Serialize(Value);
	}

	public bool Equals(UnevaluatedItemsKeyword? other)
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
		return Equals(other as UnevaluatedItemsKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as UnevaluatedItemsKeyword);
	}

	public override int GetHashCode()
	{
		if (!(Value != null))
		{
			return 0;
		}
		return Value.GetHashCode();
	}
}
