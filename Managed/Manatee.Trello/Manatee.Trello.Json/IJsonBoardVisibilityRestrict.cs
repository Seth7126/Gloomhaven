namespace Manatee.Trello.Json;

public interface IJsonBoardVisibilityRestrict
{
	[JsonDeserialize]
	OrganizationBoardVisibility? Public { get; set; }

	[JsonDeserialize]
	OrganizationBoardVisibility? Org { get; set; }

	[JsonDeserialize]
	OrganizationBoardVisibility? Private { get; set; }
}
