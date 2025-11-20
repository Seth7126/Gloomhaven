using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}; Count={Count}")]
public class RequiredKeyword : List<string>, IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<RequiredKeyword>
{
	public static string ErrorTemplate { get; set; } = "The properties {{properties}} are required.";

	public string Name => "required";

	public JsonSchemaVersion SupportedVersions
	{
		get
		{
			if (!this.Any())
			{
				return JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;
			}
			return JsonSchemaVersion.All;
		}
	}

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	[DeserializationUseOnly]
	[UsedImplicitly]
	public RequiredKeyword()
	{
	}

	public RequiredKeyword(params string[] values)
		: base((IEnumerable<string>)values)
	{
	}

	public RequiredKeyword(IEnumerable<string> values)
		: base(values)
	{
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		if (context.Instance.Type != JsonValueType.Object)
		{
			Log.Schema(() => "Instance not an object; not applicable");
			return schemaValidationResults;
		}
		List<string> missingProperties = new List<string>();
		JsonObject jsonObject = context.Instance.Object;
		using (List<string>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				if (!jsonObject.ContainsKey(current))
				{
					if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
					{
						schemaValidationResults.IsValid = false;
						return schemaValidationResults;
					}
					missingProperties.Add(current);
				}
			}
		}
		if (missingProperties.Any())
		{
			Log.Schema(() => $"Properties {missingProperties.ToJson()} required but not found");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["properties"] = missingProperties.ToJson();
			schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
		}
		return schemaValidationResults;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return null;
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		AddRange(json.Array.Select((JsonValue jv) => jv.String));
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return new JsonArray(this.Select((string s) => new JsonValue(s)))
		{
			EqualityStandard = ArrayEquality.ContentsEqual
		};
	}

	public bool Equals(RequiredKeyword? other)
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
		return Equals(other as RequiredKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as RequiredKeyword);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
