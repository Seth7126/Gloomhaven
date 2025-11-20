using System;
using System.Collections.Generic;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeBatch : IJsonBatch, IJsonSerializable
{
	public List<IJsonBatchItem> Items { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Array)
		{
			JsonArray array = json.Array;
			Items = serializer.Deserialize<List<IJsonBatchItem>>(array);
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
