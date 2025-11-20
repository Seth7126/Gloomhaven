using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum OrganizationBoardVisibility
{
	Unknown,
	[Display(Description = "none")]
	None,
	[Display(Description = "admin")]
	Admin,
	[Display(Description = "org")]
	Org
}
