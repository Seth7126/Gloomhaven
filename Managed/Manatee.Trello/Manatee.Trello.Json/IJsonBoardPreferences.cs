namespace Manatee.Trello.Json;

public interface IJsonBoardPreferences
{
	[JsonDeserialize]
	BoardPermissionLevel? PermissionLevel { get; set; }

	[JsonDeserialize]
	BoardVotingPermission? Voting { get; set; }

	[JsonDeserialize]
	BoardCommentPermission? Comments { get; set; }

	[JsonDeserialize]
	BoardInvitationPermission? Invitations { get; set; }

	[JsonDeserialize]
	bool? SelfJoin { get; set; }

	[JsonDeserialize]
	bool? CardCovers { get; set; }

	[JsonDeserialize]
	bool? CalendarFeed { get; set; }

	[JsonDeserialize]
	CardAgingStyle? CardAging { get; set; }

	[JsonDeserialize]
	IJsonBoardBackground Background { get; set; }
}
