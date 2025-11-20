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
public class AdditionalItemsKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<AdditionalItemsKeyword>
{
	public static string ErrorTemplate { get; set; } = "Items at indices {{indices}} are not covered by `items` and failed validation of the local subschema.";

	public static string ErrorTemplate_False { get; set; } = "Items at indices {{indices}} are not covered by `items` and so are not allowed.";

	public string Name => "additionalItems";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 2;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public AdditionalItemsKeyword()
	{
	}

	public AdditionalItemsKeyword(JsonSchema value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (context.Instance.Type != JsonValueType.Array)
		{
			return new SchemaValidationResults(Name, context);
		}
		ItemsKeyword itemsKeyword = context.Local.Get<ItemsKeyword>();
		if (itemsKeyword == null || !itemsKeyword.IsArray)
		{
			return new SchemaValidationResults(Name, context);
		}
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		JsonArray array = context.Instance.Array;
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		bool flag = true;
		bool flag2 = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		int startIndex = context.LocalTierLastEvaluatedIndex + 1;
		JsonArray jsonArray = new JsonArray();
		Log.Schema(() => (startIndex != 0) ? $"Indices up to {context.LastEvaluatedIndex} have been evaluated; skipping these" : "No indices have been evaluated; process all");
		if (startIndex < array.Count)
		{
			if (Value == JsonSchema.False)
			{
				Log.Schema(() => $"Subschema is `false`; all instances after index {startIndex} are invalid");
				schemaValidationResults.IsValid = false;
				schemaValidationResults.Keyword = Name;
				schemaValidationResults.AdditionalInfo["indices"] = Enumerable.Range(startIndex, array.Count - startIndex).ToJson();
				schemaValidationResults.ErrorMessage = ErrorTemplate_False.ResolveTokens(schemaValidationResults.AdditionalInfo);
				return schemaValidationResults;
			}
			IEnumerable<JsonValue> enumerable = array.Skip(startIndex);
			int num = startIndex;
			foreach (JsonValue item in enumerable)
			{
				JsonPointer baseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
				JsonPointer relativeLocation = context.RelativeLocation.CloneAndAppend(Name);
				SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
				schemaValidationContext.Instance = item;
				schemaValidationContext.BaseRelativeLocation = baseRelativeLocation;
				schemaValidationContext.RelativeLocation = relativeLocation;
				schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(num.ToString());
				SchemaValidationContext context2 = schemaValidationContext;
				SchemaValidationResults schemaValidationResults2 = Value.Validate(context2);
				if (!schemaValidationResults2.IsValid)
				{
					jsonArray.Add(num);
				}
				flag &= schemaValidationResults2.IsValid;
				context.LastEvaluatedIndex = Math.Max(context.LastEvaluatedIndex, num);
				context.LocalTierLastEvaluatedIndex = Math.Max(context.LastEvaluatedIndex, num);
				context.LocallyValidatedIndices.Add(num);
				num++;
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

	public bool Equals(AdditionalItemsKeyword? other)
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
		return Equals(other as AdditionalItemsKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as AdditionalItemsKeyword);
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
