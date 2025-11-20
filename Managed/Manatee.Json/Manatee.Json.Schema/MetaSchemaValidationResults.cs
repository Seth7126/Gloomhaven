using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public class MetaSchemaValidationResults : IJsonSerializable
{
	public JsonSchemaVersion SupportedVersions { get; set; }

	public Dictionary<string, SchemaValidationResults> MetaSchemaValidations { get; } = new Dictionary<string, SchemaValidationResults>();

	public List<string> OtherErrors { get; } = new List<string>();

	public bool IsValid
	{
		get
		{
			if (MetaSchemaValidations.All<KeyValuePair<string, SchemaValidationResults>>((KeyValuePair<string, SchemaValidationResults> v) => v.Value.IsValid))
			{
				return !OtherErrors.Any();
			}
			return false;
		}
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return new JsonObject
		{
			["supportedVersions"] = serializer.Serialize(SupportedVersions),
			["valid"] = IsValid,
			["validations"] = serializer.Serialize(MetaSchemaValidations),
			["otherErrors"] = OtherErrors.ToJson()
		};
	}
}
