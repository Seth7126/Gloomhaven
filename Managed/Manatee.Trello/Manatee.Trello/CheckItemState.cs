using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum CheckItemState
{
	Unknown,
	[Display(Description = "incomplete")]
	Incomplete,
	[Display(Description = "complete")]
	Complete
}
