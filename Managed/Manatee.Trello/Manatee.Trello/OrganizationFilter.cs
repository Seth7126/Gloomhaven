using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum OrganizationFilter
{
	[Display(Description = "members")]
	Members,
	[Display(Description = "public")]
	Public
}
