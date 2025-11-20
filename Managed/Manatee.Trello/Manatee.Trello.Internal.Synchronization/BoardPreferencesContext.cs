using System.Collections.Generic;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class BoardPreferencesContext : LinkedSynchronizationContext<IJsonBoardPreferences>
{
	static BoardPreferencesContext()
	{
		SynchronizationContext<IJsonBoardPreferences>.Properties = new Dictionary<string, Property<IJsonBoardPreferences>>
		{
			{
				"PermissionLevel",
				new Property<IJsonBoardPreferences, BoardPermissionLevel?>((IJsonBoardPreferences d, TrelloAuthorization a) => d.PermissionLevel, delegate(IJsonBoardPreferences d, BoardPermissionLevel? o)
				{
					d.PermissionLevel = o;
				})
			},
			{
				"Voting",
				new Property<IJsonBoardPreferences, BoardVotingPermission?>((IJsonBoardPreferences d, TrelloAuthorization a) => d.Voting, delegate(IJsonBoardPreferences d, BoardVotingPermission? o)
				{
					d.Voting = o;
				})
			},
			{
				"Commenting",
				new Property<IJsonBoardPreferences, BoardCommentPermission?>((IJsonBoardPreferences d, TrelloAuthorization a) => d.Comments, delegate(IJsonBoardPreferences d, BoardCommentPermission? o)
				{
					d.Comments = o;
				})
			},
			{
				"Invitations",
				new Property<IJsonBoardPreferences, BoardInvitationPermission?>((IJsonBoardPreferences d, TrelloAuthorization a) => d.Invitations, delegate(IJsonBoardPreferences d, BoardInvitationPermission? o)
				{
					d.Invitations = o;
				})
			},
			{
				"AllowSelfJoin",
				new Property<IJsonBoardPreferences, bool?>((IJsonBoardPreferences d, TrelloAuthorization a) => d.SelfJoin, delegate(IJsonBoardPreferences d, bool? o)
				{
					d.SelfJoin = o;
				})
			},
			{
				"ShowCardCovers",
				new Property<IJsonBoardPreferences, bool?>((IJsonBoardPreferences d, TrelloAuthorization a) => d.CardCovers, delegate(IJsonBoardPreferences d, bool? o)
				{
					d.CardCovers = o;
				})
			},
			{
				"IsCalendarFeedEnabled",
				new Property<IJsonBoardPreferences, bool?>((IJsonBoardPreferences d, TrelloAuthorization a) => d.CalendarFeed, delegate(IJsonBoardPreferences d, bool? o)
				{
					d.CalendarFeed = o;
				})
			},
			{
				"CardAgingStyle",
				new Property<IJsonBoardPreferences, CardAgingStyle?>((IJsonBoardPreferences d, TrelloAuthorization a) => d.CardAging, delegate(IJsonBoardPreferences d, CardAgingStyle? o)
				{
					d.CardAging = o;
				})
			},
			{
				"Background",
				new Property<IJsonBoardPreferences, BoardBackground>((IJsonBoardPreferences d, TrelloAuthorization a) => d.Background?.GetFromCache<BoardBackground>(a), delegate(IJsonBoardPreferences d, BoardBackground o)
				{
					d.Background = o?.Json;
				})
			}
		};
	}

	public BoardPreferencesContext(TrelloAuthorization auth)
		: base(auth)
	{
	}
}
