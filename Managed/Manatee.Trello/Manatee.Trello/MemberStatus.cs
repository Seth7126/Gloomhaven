using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

public enum MemberStatus
{
	Unknown,
	[Display(Description = "disconnected")]
	Disconnected,
	[Display(Description = "idle")]
	Idle,
	[Display(Description = "active")]
	Active
}
