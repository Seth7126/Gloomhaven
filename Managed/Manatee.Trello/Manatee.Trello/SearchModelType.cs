using System;
using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

[Flags]
public enum SearchModelType
{
	[Display(Description = "actions")]
	Actions = 1,
	[Display(Description = "boards")]
	Boards = 2,
	[Display(Description = "cards")]
	Cards = 4,
	[Display(Description = "members")]
	Members = 8,
	[Display(Description = "orgainzations")]
	Organizations = 0x10,
	[Display(Description = "all")]
	All = 0x1F
}
