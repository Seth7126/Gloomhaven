using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum CardAgingStyle
{
	Unknown,
	[Display(Description = "regular")]
	Regular,
	[Display(Description = "pirate")]
	Pirate
}
