using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}")]
public class ContainsKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ContainsKeyword>
{
	public static string ErrorTemplate { get; set; } = "Expected an item that matched the given schema but no such items were found.";

	public string Name => "contains";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Applicator;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public ContainsKeyword()
	{
	}

	public ContainsKeyword(JsonSchema value)
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
		JsonPointer baseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
		JsonPointer relativeLocation = context.RelativeLocation.CloneAndAppend(Name);
		bool flag = false;
		bool flag2 = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		int num = 0;
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		JsonArray matchedIndices = new JsonArray();
		bool flag3 = context.Local.Get<MinContainsKeyword>() != null || context.Local.Get<MaxContainsKeyword>() != null;
		foreach (JsonValue item in context.Instance.Array)
		{
			SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
			schemaValidationContext.Instance = item;
			schemaValidationContext.BaseRelativeLocation = baseRelativeLocation;
			schemaValidationContext.RelativeLocation = relativeLocation;
			schemaValidationContext.InstanceLocation = context.InstanceLocation.CloneAndAppend(num.ToString());
			SchemaValidationContext context2 = schemaValidationContext;
			SchemaValidationResults schemaValidationResults = Value.Validate(context2);
			flag |= schemaValidationResults.IsValid;
			if (schemaValidationResults.IsValid)
			{
				context.LocallyValidatedIndices.Add(num);
				matchedIndices.Add(num);
			}
			if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
			{
				if (flag && !flag3)
				{
					Log.Schema(() => "Match found and no min/max constraints; halting validation early");
					break;
				}
			}
			else if (flag2)
			{
				list.Add(schemaValidationResults);
			}
			num++;
		}
		Log.Schema(() => $"Found {matchedIndices.Count} instances that match; saving for later");
		context.Misc["containsCount"] = matchedIndices.Count;
		SchemaValidationResults schemaValidationResults2 = new SchemaValidationResults
		{
			NestedResults = list,
			IsValid = flag,
			Keyword = Name,
			AdditionalInfo = { ["matchedIndices"] = matchedIndices }
		};
		if (!flag)
		{
			schemaValidationResults2.ErrorMessage = ErrorTemplate;
		}
		return schemaValidationResults2;
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

	public bool Equals(ContainsKeyword? other)
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
		return Equals(other as ContainsKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ContainsKeyword);
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
