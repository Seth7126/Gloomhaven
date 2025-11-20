using System.Collections.Generic;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeOrganizationPreferences : IJsonOrganizationPreferences, IJsonSerializable
{
	public OrganizationPermissionLevel? PermissionLevel { get; set; }

	public List<object> OrgInviteRestrict { get; set; }

	public bool? ExternalMembersDisabled { get; set; }

	public string AssociatedDomain { get; set; }

	public IJsonBoardVisibilityRestrict BoardVisibilityRestrict { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject jsonObject = json.Object;
			if (jsonObject.ContainsKey("permissionLevel") && jsonObject["permissionLevel"].Type == JsonValueType.Object)
			{
				jsonObject = jsonObject["permissionLevel"].Object["prefs"].Object;
			}
			PermissionLevel = jsonObject.Deserialize<OrganizationPermissionLevel?>(serializer, "permissionLevel");
			ExternalMembersDisabled = jsonObject.TryGetBoolean("externalMembersDisabled");
			AssociatedDomain = jsonObject.TryGetString("associatedDomain");
			BoardVisibilityRestrict = jsonObject.Deserialize<IJsonBoardVisibilityRestrict>(serializer, "boardVisibilityRestrict") ?? new ManateeBoardVisibilityRestrict();
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return null;
	}
}
