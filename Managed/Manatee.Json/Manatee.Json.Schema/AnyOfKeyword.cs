using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class AnyOfKeyword : List<JsonSchema>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<AnyOfKeyword>
{
	public static string ErrorTemplate { get; set; } = "All subschemas failed validation.";

	public string Name => "anyOf";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Applicator;

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		bool valid = false;
		bool flag = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		int num = 0;
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		using (List<JsonSchema>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				JsonSchema current = enumerator.Current;
				SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
				schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name, num.ToString());
				schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name, num.ToString());
				SchemaValidationContext schemaValidationContext2 = schemaValidationContext;
				SchemaValidationResults schemaValidationResults = current.Validate(schemaValidationContext2);
				valid |= schemaValidationResults.IsValid;
				Log.Schema(() => "`" + Name + "` " + (valid ? "valid" : "invalid") + " so far");
				if (valid)
				{
					context.UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(schemaValidationContext2);
				}
				if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
				{
					if (valid)
					{
						Log.Schema(() => "Subschema passed; halting validation early");
						break;
					}
				}
				else if (flag)
				{
					list.Add(schemaValidationResults);
				}
				num++;
			}
		}
		List<SchemaValidationResults> nestedResults = list.ToList();
		SchemaValidationResults schemaValidationResults2 = new SchemaValidationResults(Name, context)
		{
			NestedResults = nestedResults,
			IsValid = valid
		};
		if (!schemaValidationResults2.IsValid)
		{
			schemaValidationResults2.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults2.AdditionalInfo);
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

	public bool Equals(AnyOfKeyword? other)
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
		return Equals(other as AnyOfKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as AnyOfKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
