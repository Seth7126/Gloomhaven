using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Schema;
using Manatee.Json.Serialization;

namespace Manatee.Json.Patch;

public class JsonPatch : List<JsonPatchAction>, IJsonSerializable
{
	public static readonly JsonSchema Schema = new JsonSchema().Title("JSON schema for JSONPatch files").Id("http://json.schemastore.org/json-patch#").Schema(MetaSchemas.Draft04.Id)
		.Type(JsonSchemaType.Array)
		.Items(new JsonSchema().Ref("#/definitions/operation"))
		.Definition("operation", new JsonSchema().Type(JsonSchemaType.Object).Required("op", "path").AllOf(new JsonSchema().Ref("#/definitions/path"))
			.OneOf(new JsonSchema().Property("op", new JsonSchema().Description("The operation to perform").Type(JsonSchemaType.String).Enum("add", "replace", "test")).Property("value", new JsonSchema().Description("The value to add, replace or test.")).Required("value"), new JsonSchema().Property("op", new JsonSchema().Description("The operation to perform").Type(JsonSchemaType.String).Enum("remove")), new JsonSchema().Property("op", new JsonSchema().Description("The operation to perform").Type(JsonSchemaType.String).Enum("move", "copy")).Property("from", new JsonSchema().Description("A JSON Pointer path pointing to the location to move/copy from.")).Required("from")))
		.Definition("path", new JsonSchema().Property("path", new JsonSchema().Description("A JSON Pointer path.").Type(JsonSchemaType.String)));

	public JsonPatchResult TryApply(JsonValue json)
	{
		JsonValue json2 = new JsonValue(json);
		JsonPatchResult jsonPatchResult = new JsonPatchResult(json);
		using (List<JsonPatchAction>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				jsonPatchResult = enumerator.Current.TryApply(json2);
				if (jsonPatchResult.Success)
				{
					json2 = jsonPatchResult.Patched;
					continue;
				}
				break;
			}
		}
		return jsonPatchResult;
	}

	void IJsonSerializable.FromJson(JsonValue json, JsonSerializer serializer)
	{
		AddRange(json.Array.Select(serializer.Deserialize<JsonPatchAction>));
	}

	JsonValue IJsonSerializable.ToJson(JsonSerializer serializer)
	{
		return new JsonArray(this.Select((JsonPatchAction pa) => ((IJsonSerializable)pa).ToJson(serializer)));
	}
}
