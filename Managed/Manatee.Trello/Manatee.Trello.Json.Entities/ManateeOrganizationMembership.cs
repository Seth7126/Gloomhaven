using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeOrganizationMembership : IJsonOrganizationMembership, IJsonCacheable, IJsonSerializable
{
	public string Id { get; set; }

	public IJsonMember Member { get; set; }

	public OrganizationMembershipType? MemberType { get; set; }

	public bool? Unconfirmed { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Member = obj.Deserialize<IJsonMember>(serializer, "member") ?? obj.Deserialize<IJsonMember>(serializer, "idMember");
			MemberType = obj.Deserialize<OrganizationMembershipType?>(serializer, "memberType");
			Unconfirmed = obj.TryGetBoolean("unconfirmed");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		Id.Serialize(jsonObject, serializer, "id");
		Member.SerializeId(jsonObject, "idMember");
		MemberType.Serialize(jsonObject, serializer, "type");
		return jsonObject;
	}
}
