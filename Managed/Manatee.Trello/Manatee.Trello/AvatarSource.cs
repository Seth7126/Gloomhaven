using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum AvatarSource
{
	Unknown,
	[Display(Description = "none")]
	None,
	[Display(Description = "upload")]
	Upload,
	[Display(Description = "gravatar")]
	Gravatar
}
