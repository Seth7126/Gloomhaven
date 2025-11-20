using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum ListFilter
{
	[Display(Description = "open")]
	Open,
	[Display(Description = "closed")]
	Closed,
	[Display(Description = "all")]
	All
}
