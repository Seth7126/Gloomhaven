using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class OneOfKeyword : List<JsonSchema>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<OneOfKeyword>
{
	public static string ErrorTemplate { get; set; } = "Expected exactly one subschema to pass validation, but found {{passed}}.";

	public string Name => "oneOf";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Applicator;

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		bool flag = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		int num = 0;
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		int validCount = 0;
		SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
		using (List<JsonSchema>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				JsonSchema current = enumerator.Current;
				SchemaValidationContext schemaValidationContext2 = new SchemaValidationContext(context);
				schemaValidationContext2.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name, num.ToString());
				schemaValidationContext2.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name, num.ToString());
				SchemaValidationContext schemaValidationContext3 = schemaValidationContext2;
				SchemaValidationResults schemaValidationResults = current.Validate(schemaValidationContext3);
				if (schemaValidationResults.IsValid)
				{
					validCount++;
				}
				Log.Schema(() => $"`{Name}` {validCount} items valid so far");
				schemaValidationContext.UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(schemaValidationContext3);
				if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
				{
					if (validCount > 1)
					{
						Log.Schema(() => "More than one subschema succeeded; halting validation early");
						break;
					}
				}
				else if (flag)
				{
					list.Add(schemaValidationResults);
				}
			}
		}
		SchemaValidationResults schemaValidationResults2 = new SchemaValidationResults(Name, context)
		{
			IsValid = (validCount == 1),
			NestedResults = list
		};
		if (!schemaValidationResults2.IsValid)
		{
			Log.Schema(() => $"{validCount} subschemas passed validation; expected only one");
			schemaValidationResults2.AdditionalInfo["passed"] = validCount;
			schemaValidationResults2.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults2.AdditionalInfo);
		}
		else
		{
			context.UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(schemaValidationContext);
		}
		return schemaValidationResults2;
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
		AddRange(json.Array.Select(serializer.Deserialize<JsonSchema>));
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonArray jsonArray = this.Select(serializer.Serialize).ToJson();
		jsonArray.EqualityStandard = ArrayEquality.ContentsEqual;
		return jsonArray;
	}

	public bool Equals(OneOfKeyword? other)
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
		return Equals(other as OneOfKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as OneOfKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
