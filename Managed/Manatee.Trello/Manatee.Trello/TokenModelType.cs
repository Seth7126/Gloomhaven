using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum TokenModelType
{
	Unknown,
	[Display(Description = "Member")]
	Member,
	[Display(Description = "Board")]
	Board,
	[Display(Description = "Organization")]
	Organization
}
