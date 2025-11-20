using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeBoardMembership : IJsonBoardMembership, IJsonCacheable, IJsonSerializable
{
	public string Id { get; set; }

	public IJsonMember Member { get; set; }

	public BoardMembershipType? MemberType { get; set; }

	public bool? Deactivated { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Member = obj.Deserialize<IJsonMember>(serializer, "member") ?? obj.Deserialize<IJsonMember>(serializer, "idMember");
			MemberType = obj.Deserialize<BoardMembershipType?>(serializer, "memberType");
			Deactivated = obj.TryGetBoolean("deactivated");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		Id.Serialize(jsonObject, serializer, "id");
		MemberType.Serialize(jsonObject, serializer, "type");
		Member.SerializeId(jsonObject, "idMember");
		return jsonObject;
	}
}
