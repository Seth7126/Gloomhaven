using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum OrganizationPermissionLevel
{
	Unknown,
	[Display(Description = "private")]
	Private,
	[Display(Description = "public")]
	Public
}
