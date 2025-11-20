using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum LabelColor
{
	Unknown,
	[Display(Description = "none")]
	None,
	[Display(Description = "green")]
	Green,
	[Display(Description = "yellow")]
	Yellow,
	[Display(Description = "orange")]
	Orange,
	[Display(Description = "red")]
	Red,
	[Display(Description = "purple")]
	Purple,
	[Display(Description = "blue")]
	Blue,
	[Display(Description = "pink")]
	Pink,
	[Display(Description = "sky")]
	Sky,
	[Display(Description = "lime")]
	Lime,
	[Display(Description = "black")]
	Black
}
