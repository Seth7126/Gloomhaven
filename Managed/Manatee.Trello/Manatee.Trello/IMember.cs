using System;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IMember : ICanWebhook, ICacheable, IRefreshable
{
	IReadOnlyActionCollection Actions { get; }

	AvatarSource? AvatarSource { get; }

	string AvatarUrl { get; }

	string Bio { get; }

	IReadOnlyBoardCollection Boards { get; }

	IReadOnlyCollection<IBoardBackground> BoardBackgrounds { get; }

	IReadOnlyCardCollection Cards { get; }

	DateTime CreationDate { get; }

	string FullName { get; }

	string Initials { get; }

	bool? IsConfirmed { get; }

	string Mention { get; }

	IReadOnlyOrganizationCollection Organizations { get; }

	IReadOnlyCollection<IStarredBoard> StarredBoards { get; }

	MemberStatus? Status { get; }

	IEnumerable<string> Trophies { get; }

	string Url { get; }

	string UserName { get; }

	event Action<IMember, IEnumerable<string>> Updated;
}
