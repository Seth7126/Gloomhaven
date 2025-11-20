using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum CardFilter
{
	[Display(Description = "visible")]
	Visible,
	[Display(Description = "open")]
	Open,
	[Display(Description = "closed")]
	Closed,
	[Display(Description = "all")]
	All
}
