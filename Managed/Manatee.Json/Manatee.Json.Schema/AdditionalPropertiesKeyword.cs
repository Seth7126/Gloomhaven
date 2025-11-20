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
public class AdditionalPropertiesKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<AdditionalPropertiesKeyword>
{
	public static string ErrorTemplate { get; set; } = "Properties {{properties}} were not covered by either `properties` or `patternProperties` and failed validation of the local subschema.";

	public static string ErrorTemplate_False { get; set; } = "Properties {{properties}} are covered by neither `properties` nor `patternProperties` and so are not allowed.";

	public string Name => "additionalProperties";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 3;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public AdditionalPropertiesKeyword()
	{
	}

	public AdditionalPropertiesKeyword(JsonSchema value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (context.Instance.Type != JsonValueType.Object)
		{
			Log.Schema(() => "Instance not an object; not applicable");
			return new SchemaValidationResults(Name, context);
		}
		JsonObject source = context.Instance.Object;
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		JsonObject jsonObject = source.Where<KeyValuePair<string, JsonValue>>((KeyValuePair<string, JsonValue> kvp) => !context.LocallyEvaluatedPropertyNames.Contains(kvp.Key)).ToJson();
		if (jsonObject.Count == 0)
		{
			Log.Schema(() => "All properties have been evaluated");
			return schemaValidationResults;
		}
		Log.Schema(() => (context.LocallyEvaluatedPropertyNames.Count != 0) ? $"Properties {context.LocallyEvaluatedPropertyNames.ToJson()} have been evaluated; skipping these" : "No properties have been evaluated; process all");
		if (Value == JsonSchema.False && jsonObject.Any())
		{
			Log.Schema(() => "Subschema is `false`; all instances invalid");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.Keyword = Name;
			schemaValidationResults.AdditionalInfo["properties"] = jsonObject.Keys.ToJson();
			schemaValidationResults.ErrorMessage = ErrorTemplate_False.ResolveTokens(schemaValidationResults.AdditionalInfo);
			return schemaValidationResults;
		}
		bool flag = true;
		bool flag2 = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		JsonArray jsonArray = new JsonArray();
		foreach (KeyValuePair<string, JsonValue> item in jsonObject)
		{
			context.EvaluatedPropertyNames.Add(item.Key);
			context.LocallyEvaluatedPropertyNames.Add(item.Key);
			SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
			schemaValidationContext.Instance = item.Value;
			schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
			schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name);
			schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(item.Key);
			SchemaValidationContext schemaValidationContext2 = schemaValidationContext;
			SchemaValidationResults schemaValidationResults2 = Value.Validate(schemaValidationContext2);
			if (!schemaValidationResults2.IsValid)
			{
				jsonArray.Add(item.Key);
			}
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
		schemaValidationResults.NestedResults = list;
		if (list.Any((SchemaValidationResults r) => !r.IsValid))
		{
			schemaValidationResults.IsValid = false;
			schemaValidationResults.Keyword = Name;
			schemaValidationResults.AdditionalInfo["properties"] = jsonArray;
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
		return Value.ToJson(serializer);
	}

	public bool Equals(AdditionalPropertiesKeyword? other)
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
		return Equals(other as AdditionalPropertiesKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as AdditionalPropertiesKeyword);
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
