using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public class SchemaValidationResults : IJsonSerializable, IEquatable<SchemaValidationResults>
{
	private List<SchemaValidationResults> _nestedResults;

	private JsonObject _additionalInfo;

	public static SchemaValidationResults Null { get; } = new SchemaValidationResults();

	public bool IsValid { get; set; } = true;

	public JsonPointer RelativeLocation { get; set; }

	public Uri? AbsoluteLocation { get; set; }

	public JsonPointer InstanceLocation { get; set; }

	public JsonValue? AnnotationValue { get; set; }

	public string? Keyword { get; set; }

	public string? ErrorMessage { get; set; }

	public JsonObject AdditionalInfo
	{
		get
		{
			return _additionalInfo ?? (_additionalInfo = new JsonObject());
		}
		[param: AllowNull]
		set
		{
			_additionalInfo = value;
		}
	}

	public List<SchemaValidationResults> NestedResults
	{
		get
		{
			return _nestedResults ?? (_nestedResults = new List<SchemaValidationResults>());
		}
		[param: AllowNull]
		set
		{
			_nestedResults = value;
		}
	}

	internal static bool IncludeAdditionalInfo { get; set; }

	internal bool RecursionDetected { get; set; }

	[DeserializationUseOnly]
	public SchemaValidationResults()
	{
	}

	internal SchemaValidationResults(SchemaValidationContext context)
	{
		InstanceLocation = context.InstanceLocation.Clone();
		if (context.BaseUri != null && context.BaseRelativeLocation != null)
		{
			AbsoluteLocation = new Uri(context.BaseUri?.ToString() + context.BaseRelativeLocation, UriKind.RelativeOrAbsolute);
		}
		RelativeLocation = context.RelativeLocation;
	}

	public SchemaValidationResults(string keyword, SchemaValidationContext context)
	{
		InstanceLocation = context.InstanceLocation.Clone();
		if (context.BaseUri != null && context.BaseRelativeLocation != null)
		{
			AbsoluteLocation = new Uri(context.BaseUri?.ToString() + context.BaseRelativeLocation?.CloneAndAppend(keyword).ToString(), UriKind.RelativeOrAbsolute);
		}
		RelativeLocation = context.RelativeLocation.CloneAndAppend(keyword);
		Keyword = keyword;
	}

	public SchemaValidationResults Condense()
	{
		List<SchemaValidationResults> nestedResults = (from r in NestedResults
			where r.RelativeLocation != null && !r.RecursionDetected
			select r.Condense() into r
			where (!IsValid && !r.IsValid) || (IsValid && r.AnnotationValue != null) || r.NestedResults.Any()
			select r).Distinct().ToList();
		SchemaValidationResults copy = new SchemaValidationResults();
		copy._CopyDataFrom(this);
		copy.NestedResults = nestedResults;
		if (copy.AnnotationValue != null)
		{
			return copy;
		}
		copy.NestedResults = copy.NestedResults.Where((SchemaValidationResults r) => r.IsValid == copy.IsValid).ToList();
		if (copy.NestedResults.Count != 1)
		{
			return copy;
		}
		copy._CopyDataFrom(copy.NestedResults[0]);
		return copy;
	}

	public SchemaValidationResults Flatten()
	{
		SchemaValidationResults schemaValidationResults = Condense();
		List<SchemaValidationResults> list = schemaValidationResults._GetAllChildren().ToList();
		schemaValidationResults.NestedResults = new List<SchemaValidationResults>();
		if (!list.Any())
		{
			return schemaValidationResults;
		}
		SchemaValidationResults schemaValidationResults2 = new SchemaValidationResults();
		schemaValidationResults2.IsValid = IsValid;
		schemaValidationResults2.NestedResults.Add(schemaValidationResults);
		schemaValidationResults2.NestedResults.AddRange(list);
		return schemaValidationResults2;
	}

	private IEnumerable<SchemaValidationResults> _GetAllChildren()
	{
		return NestedResults.Union<SchemaValidationResults>(NestedResults.SelectMany((SchemaValidationResults r) => r._GetAllChildren()));
	}

	private void _CopyDataFrom(SchemaValidationResults other)
	{
		IsValid = other.IsValid;
		RelativeLocation = other.RelativeLocation;
		AbsoluteLocation = other.AbsoluteLocation;
		InstanceLocation = other.InstanceLocation;
		AnnotationValue = other.AnnotationValue;
		Keyword = other.Keyword;
		ErrorMessage = other.ErrorMessage;
		AdditionalInfo = other.AdditionalInfo;
		NestedResults = other.NestedResults;
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		JsonObject jsonObject = json.Object;
		IsValid = jsonObject["valid"].Boolean;
		string text = jsonObject.TryGetString("keywordLocation");
		if (text != null)
		{
			RelativeLocation = JsonPointer.Parse(text);
		}
		string text2 = jsonObject.TryGetString("absoluteKeywordLocation");
		if (text2 != null)
		{
			AbsoluteLocation = new Uri(text2);
		}
		string text3 = jsonObject.TryGetString("instanceLocation");
		if (text3 != null)
		{
			InstanceLocation = JsonPointer.Parse(text3);
		}
		Keyword = jsonObject.TryGetString("keyword");
		AnnotationValue = jsonObject.TryGetString("annotation");
		JsonArray jsonArray = jsonObject.TryGetArray("annotations") ?? jsonObject.TryGetArray("errors");
		if (jsonArray != null)
		{
			NestedResults = jsonArray.Select(serializer.Deserialize<SchemaValidationResults>).ToList();
		}
		ErrorMessage = jsonObject.TryGetString("error");
		AdditionalInfo = jsonObject.TryGetObject("additionalInfo");
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		jsonObject["valid"] = IsValid;
		if (RelativeLocation != null)
		{
			string text = RelativeLocation.ToString();
			jsonObject["keywordLocation"] = text;
			if (AbsoluteLocation != null && (AbsoluteLocation.Fragment != text || RelativeLocation.Contains("$ref") || RelativeLocation.Contains("$recursiveRef")))
			{
				jsonObject["absoluteKeywordLocation"] = AbsoluteLocation.OriginalString;
			}
		}
		if (InstanceLocation != null)
		{
			jsonObject["instanceLocation"] = InstanceLocation.ToString();
		}
		List<SchemaValidationResults> source = NestedResults.Where((SchemaValidationResults r) => r != Null).ToList();
		if (!JsonSchemaOptions.ConfigureForTestOutput && Keyword != null)
		{
			jsonObject["keyword"] = Keyword;
		}
		if (IsValid)
		{
			if (AnnotationValue != null)
			{
				jsonObject["annotation"] = AnnotationValue;
			}
			if (source.Any())
			{
				jsonObject["annotations"] = source.Select((SchemaValidationResults r) => r.ToJson(serializer)).ToJson();
			}
		}
		else
		{
			if (!JsonSchemaOptions.ConfigureForTestOutput && !string.IsNullOrWhiteSpace(ErrorMessage))
			{
				jsonObject["error"] = ErrorMessage;
			}
			if (source.Any())
			{
				jsonObject["errors"] = source.Select((SchemaValidationResults r) => r.ToJson(serializer)).ToJson();
			}
		}
		if (!JsonSchemaOptions.ConfigureForTestOutput && IncludeAdditionalInfo && AdditionalInfo != null && AdditionalInfo.Any())
		{
			jsonObject["additionalInfo"] = AdditionalInfo;
		}
		return jsonObject;
	}

	public bool Equals(SchemaValidationResults? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (IsValid == other.IsValid && object.Equals(RelativeLocation, other.RelativeLocation) && object.Equals(AbsoluteLocation, other.AbsoluteLocation) && object.Equals(InstanceLocation, other.InstanceLocation) && object.Equals(AnnotationValue, other.AnnotationValue) && object.Equals(ErrorMessage, other.ErrorMessage) && string.Equals(Keyword, other.Keyword) && object.Equals(AdditionalInfo, other.AdditionalInfo))
		{
			return NestedResults.ContentsEqual(other.NestedResults);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SchemaValidationResults);
	}

	public override int GetHashCode()
	{
		return (((((((((((((((IsValid.GetHashCode() * 397) ^ (RelativeLocation?.GetHashCode() ?? 0)) * 397) ^ (AbsoluteLocation?.GetHashCode() ?? 0)) * 397) ^ (InstanceLocation?.GetHashCode() ?? 0)) * 397) ^ (AnnotationValue?.GetHashCode() ?? 0)) * 397) ^ (ErrorMessage?.GetHashCode() ?? 0)) * 397) ^ (Keyword?.GetHashCode() ?? 0)) * 397) ^ (AdditionalInfo?.GetHashCode() ?? 0)) * 397) ^ (NestedResults?.GetCollectionHashCode() ?? 0);
	}
}
