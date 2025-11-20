using System;
using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

[Flags]
public enum MembershipFilter
{
	[Display(Description = "me")]
	Me = 1,
	[Display(Description = "normal")]
	Normal = 2,
	[Display(Description = "admin")]
	Admin = 4,
	[Display(Description = "active")]
	Active = 8,
	[Display(Description = "deactivated")]
	Deactivated = 0x10,
	[Display(Description = "all")]
	All = int.MinValue
}
