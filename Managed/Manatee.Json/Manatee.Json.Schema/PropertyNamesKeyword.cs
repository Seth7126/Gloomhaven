using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}")]
public class PropertyNamesKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<PropertyNamesKeyword>
{
	public static string ErrorTemplate { get; set; } = "Properties {{properties}} have names that failed validation.";

	public string Name => "propertyNames";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public PropertyNamesKeyword()
	{
	}

	public PropertyNamesKeyword(JsonSchema value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Object)
		{
			Log.Schema(() => "Instance not an object; not applicable");
			return schemaValidationResults;
		}
		JsonPointer baseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
		JsonPointer relativeLocation = context.RelativeLocation.CloneAndAppend(Name);
		bool flag = true;
		bool flag2 = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		JsonArray invalidPropertyNames = new JsonArray();
		foreach (string key in context.Instance.Object.Keys)
		{
			SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
			schemaValidationContext.Instance = key;
			schemaValidationContext.BaseRelativeLocation = baseRelativeLocation;
			schemaValidationContext.RelativeLocation = relativeLocation;
			schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(key);
			SchemaValidationContext context2 = schemaValidationContext;
			SchemaValidationResults schemaValidationResults2 = Value.Validate(context2);
			flag &= schemaValidationResults2.IsValid;
			if (!flag)
			{
				invalidPropertyNames.Add(key);
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
		schemaValidationResults.IsValid = flag;
		schemaValidationResults.NestedResults = list;
		if (!schemaValidationResults.IsValid)
		{
			Log.Schema(() => $"Property names {invalidPropertyNames.ToJson()} failed");
			schemaValidationResults.AdditionalInfo["properties"] = invalidPropertyNames;
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

	public bool Equals(PropertyNamesKeyword? other)
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
		return Equals(other as PropertyNamesKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as PropertyNamesKeyword);
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
