using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum BoardMembershipType
{
	Unknown,
	[Display(Description = "admin")]
	Admin,
	[Display(Description = "normal")]
	Normal,
	[Display(Description = "observer")]
	Observer,
	[Display(Description = "ghost")]
	Ghost
}
