using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class DependentSchemasKeyword : List<SchemaDependency>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<DependentSchemasKeyword>
{
	public static string ErrorTemplate { get; set; } = "{{failed}} of {{total}} dependencies failed validation.";

	public string Name => "dependentSchemas";

	public JsonSchemaVersion SupportedVersions => JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		JsonPointer baseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
		JsonPointer relativeLocation = context.RelativeLocation.CloneAndAppend(Name);
		bool flag = true;
		bool flag2 = JsonSchemaOptions.ShouldReportChildErrors(this, context);
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		int num = 0;
		using (List<SchemaDependency>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				SchemaDependency current = enumerator.Current;
				SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
				schemaValidationContext.BaseRelativeLocation = baseRelativeLocation;
				schemaValidationContext.RelativeLocation = relativeLocation;
				schemaValidationContext.Misc["dependencyParent"] = Name;
				SchemaValidationContext context2 = schemaValidationContext;
				SchemaValidationResults schemaValidationResults = current.Validate(context2);
				flag &= schemaValidationResults.IsValid;
				if (!flag)
				{
					num++;
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
					list.Add(schemaValidationResults);
				}
			}
		}
		SchemaValidationResults schemaValidationResults2 = new SchemaValidationResults(Name, context)
		{
			IsValid = flag
		};
		if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
		{
			schemaValidationResults2.NestedResults = list;
		}
		else if (!schemaValidationResults2.IsValid)
		{
			schemaValidationResults2.AdditionalInfo["failed"] = num;
			schemaValidationResults2.AdditionalInfo["total"] = base.Count;
			schemaValidationResults2.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults2.AdditionalInfo);
		}
		return schemaValidationResults2;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		using List<SchemaDependency>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.RegisterSubschemas(baseUri, localRegistry);
		}
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		string first = pointer.FirstOrDefault();
		if (first == null)
		{
			return null;
		}
		return this.FirstOrDefault((SchemaDependency k) => k.PropertyName == first)?.ResolveSubschema(new JsonPointer(pointer.Skip(1)), baseUri);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		AddRange(json.Object.Select<KeyValuePair<string, JsonValue>, SchemaDependency>((KeyValuePair<string, JsonValue> kvp) => new SchemaDependency(kvp.Key, serializer.Deserialize<JsonSchema>(kvp.Value))));
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return this.ToDictionary((SchemaDependency d) => d.PropertyName, (SchemaDependency d) => d.ToJson(serializer)).ToJson();
	}

	public bool Equals(DependentSchemasKeyword? other)
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
		return Equals(other as DependentSchemasKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as DependentSchemasKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
