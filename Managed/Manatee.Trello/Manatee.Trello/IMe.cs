namespace Manatee.Trello;

public interface IMe : IMember, ICanWebhook, ICacheable, IRefreshable
{
	new AvatarSource? AvatarSource { get; set; }

	new string Bio { get; set; }

	new IBoardCollection Boards { get; }

	new IBoardBackgroundCollection BoardBackgrounds { get; }

	string Email { get; set; }

	new string FullName { get; set; }

	new string Initials { get; set; }

	IReadOnlyNotificationCollection Notifications { get; }

	new IOrganizationCollection Organizations { get; }

	IMemberPreferences Preferences { get; }

	new IStarredBoardCollection StarredBoards { get; }

	new string UserName { get; set; }
}
