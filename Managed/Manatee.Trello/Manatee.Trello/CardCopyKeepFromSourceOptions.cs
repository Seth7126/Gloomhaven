using System;
using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

[Flags]
public enum CardCopyKeepFromSourceOptions
{
	[Display(Description = "none")]
	None = 0,
	[Display(Description = "attachments")]
	Attachments = 1,
	[Display(Description = "checklists")]
	Checklists = 2,
	[Display(Description = "comments")]
	Comments = 4,
	[Display(Description = "due")]
	Due = 8,
	[Display(Description = "labels")]
	Labels = 0x10,
	[Display(Description = "members")]
	Members = 0x20,
	[Display(Description = "stickers")]
	Stickers = 0x40,
	[Display(Description = "all")]
	All = 0x7F
}
