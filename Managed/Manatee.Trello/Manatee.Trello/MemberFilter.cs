using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum MemberFilter
{
	[Display(Description = "normal")]
	Normal,
	[Display(Description = "admins")]
	Admins,
	[Display(Description = "owners")]
	Owners
}
