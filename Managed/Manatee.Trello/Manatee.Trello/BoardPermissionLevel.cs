using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum BoardPermissionLevel
{
	Unknown,
	[Display(Description = "private")]
	Private,
	[Display(Description = "org")]
	Org,
	[Display(Description = "enterprise")]
	Enterprise,
	[Display(Description = "public")]
	Public
}
