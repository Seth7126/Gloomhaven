using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum BoardBackgroundType
{
	Unknown,
	[Display(Description = "default")]
	Default,
	[Display(Description = "premium")]
	Premium,
	[Display(Description = "custom")]
	Custom
}
