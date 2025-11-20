using System.Collections.Generic;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeOrganization : IJsonOrganization, IJsonCacheable, IAcceptId, IJsonSerializable
{
	public string Id { get; set; }

	public string Name { get; set; }

	public string DisplayName { get; set; }

	public string Desc { get; set; }

	public string Url { get; set; }

	public string Website { get; set; }

	public string LogoHash { get; set; }

	public List<int> PowerUps { get; set; }

	public bool? PaidAccount { get; set; }

	public List<string> PremiumFeatures { get; set; }

	public List<IJsonAction> Actions { get; set; }

	public List<IJsonBoard> Boards { get; set; }

	public List<IJsonMember> Members { get; set; }

	public List<IJsonOrganizationMembership> Memberships { get; set; }

	public List<IJsonPowerUpData> PowerUpData { get; set; }

	public IJsonOrganizationPreferences Prefs { get; set; }

	public bool ValidForMerge { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		switch (json.Type)
		{
		case JsonValueType.Object:
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Name = obj.TryGetString("name");
			DisplayName = obj.TryGetString("displayName");
			Desc = obj.TryGetString("desc");
			Url = obj.TryGetString("url");
			Website = obj.TryGetString("website");
			LogoHash = obj.TryGetString("logoHash");
			PowerUps = obj.Deserialize<List<int>>(serializer, "powerUps");
			PaidAccount = obj.TryGetBoolean("paid_account");
			PremiumFeatures = obj.Deserialize<List<string>>(serializer, "premiumFeatures");
			Prefs = obj.Deserialize<IJsonOrganizationPreferences>(serializer, "prefs");
			Actions = obj.Deserialize<List<IJsonAction>>(serializer, "actions");
			Boards = obj.Deserialize<List<IJsonBoard>>(serializer, "boards");
			Members = obj.Deserialize<List<IJsonMember>>(serializer, "members");
			Memberships = obj.Deserialize<List<IJsonOrganizationMembership>>(serializer, "memberships");
			PowerUpData = obj.Deserialize<List<IJsonPowerUpData>>(serializer, "pluginData");
			ValidForMerge = true;
			break;
		}
		case JsonValueType.String:
			Id = json.String;
			break;
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		Id.Serialize(jsonObject, serializer, "id");
		Name.Serialize(jsonObject, serializer, "name");
		DisplayName.Serialize(jsonObject, serializer, "displayName");
		Desc.Serialize(jsonObject, serializer, "desc");
		Website.Serialize(jsonObject, serializer, "website");
		if (Prefs != null)
		{
			Prefs.PermissionLevel.Serialize(jsonObject, serializer, "prefs/permissionLevel");
			Prefs.OrgInviteRestrict.Serialize(jsonObject, serializer, "prefs/orgInviteRestrict");
			if (string.IsNullOrWhiteSpace(Prefs.AssociatedDomain))
			{
				jsonObject.Add("prefs/associatedDomain", JsonValue.Null);
			}
			else
			{
				Prefs.AssociatedDomain.Serialize(jsonObject, serializer, "prefs/associatedDomain");
			}
			if (Prefs.BoardVisibilityRestrict != null)
			{
				Prefs.BoardVisibilityRestrict.Private.Serialize(jsonObject, serializer, "prefs/boardVisibilityRestrict/private");
				Prefs.BoardVisibilityRestrict.Org.Serialize(jsonObject, serializer, "prefs/boardVisibilityRestrict/org");
				Prefs.BoardVisibilityRestrict.Public.Serialize(jsonObject, serializer, "prefs/boardVisibilityRestrict/public");
			}
		}
		return jsonObject;
	}
}
