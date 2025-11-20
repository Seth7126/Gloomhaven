namespace Manatee.Trello;

public interface IOrganizationPreferences
{
	OrganizationPermissionLevel? PermissionLevel { get; set; }

	bool? ExternalMembersDisabled { get; set; }

	string AssociatedDomain { get; set; }

	OrganizationBoardVisibility? PublicBoardVisibility { get; set; }

	OrganizationBoardVisibility? OrganizationBoardVisibility { get; set; }

	OrganizationBoardVisibility? PrivateBoardVisibility { get; set; }
}
