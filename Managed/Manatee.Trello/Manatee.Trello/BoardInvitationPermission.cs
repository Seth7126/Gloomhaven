using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum BoardInvitationPermission
{
	Unknown,
	[Display(Description = "members")]
	Members,
	[Display(Description = "admins")]
	Admins
}
