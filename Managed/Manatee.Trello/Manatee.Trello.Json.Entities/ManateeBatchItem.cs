using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeBatchItem : IJsonBatchItem, IJsonSerializable
{
	public HttpStatusCode StatusCode { get; set; }

	public string EntityId { get; set; }

	public string Content { get; set; }

	public string Error { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		switch (json.Type)
		{
		case JsonValueType.Object:
			if (json.Object.Count == 1)
			{
				KeyValuePair<string, JsonValue> keyValuePair = json.Object.Single();
				StatusCode = (HttpStatusCode)int.Parse(keyValuePair.Key);
				EntityId = keyValuePair.Value.Object["id"].String;
				Content = keyValuePair.Value.ToString();
			}
			break;
		case JsonValueType.String:
			Error = json.String;
			break;
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
