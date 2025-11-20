namespace Manatee.Trello;

public interface IBoardPreferences
{
	BoardPermissionLevel? PermissionLevel { get; set; }

	BoardVotingPermission? Voting { get; set; }

	BoardCommentPermission? Commenting { get; set; }

	BoardInvitationPermission? Invitations { get; set; }

	bool? AllowSelfJoin { get; set; }

	bool? ShowCardCovers { get; set; }

	bool? IsCalendarFeedEnabled { get; set; }

	CardAgingStyle? CardAgingStyle { get; set; }

	IBoardBackground Background { get; set; }
}
