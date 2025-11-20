using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonOrganizationPreferences
{
	[JsonDeserialize]
	OrganizationPermissionLevel? PermissionLevel { get; set; }

	[JsonDeserialize]
	List<object> OrgInviteRestrict { get; set; }

	[JsonDeserialize]
	bool? ExternalMembersDisabled { get; set; }

	[JsonDeserialize]
	string AssociatedDomain { get; set; }

	[JsonDeserialize]
	IJsonBoardVisibilityRestrict BoardVisibilityRestrict { get; set; }
}
