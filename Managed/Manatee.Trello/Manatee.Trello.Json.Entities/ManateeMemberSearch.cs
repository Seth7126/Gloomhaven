using System.Collections.Generic;
using System.Linq;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeMemberSearch : IJsonMemberSearch, IJsonSerializable
{
	public List<IJsonMember> Members { get; set; }

	public IJsonBoard Board { get; set; }

	public int? Limit { get; set; }

	public bool? OnlyOrgMembers { get; set; }

	public IJsonOrganization Organization { get; set; }

	public string Query { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Array)
		{
			JsonArray array = json.Array;
			Members = array.Select(serializer.Deserialize<IJsonMember>).ToList();
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject { { "query", Query } };
		Limit.Serialize(jsonObject, serializer, "limit");
		Board.SerializeId(jsonObject, "idBoard");
		if (Organization != null)
		{
			Organization.SerializeId(jsonObject, "idOrganization");
			OnlyOrgMembers.Serialize(jsonObject, serializer, "onlyOrgMembers");
		}
		return jsonObject;
	}
}
