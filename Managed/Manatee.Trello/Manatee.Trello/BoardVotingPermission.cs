using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum BoardVotingPermission
{
	Unknown,
	[Display(Description = "members")]
	Members,
	[Display(Description = "org")]
	Org,
	[Display(Description = "public")]
	Public,
	[Display(Description = "disabled")]
	Disabled
}
