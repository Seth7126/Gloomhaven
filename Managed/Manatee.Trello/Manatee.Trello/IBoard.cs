using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IBoard : ICanWebhook, ICacheable, IQueryable, IRefreshable
{
	IReadOnlyActionCollection Actions { get; }

	IReadOnlyCardCollection Cards { get; }

	DateTime CreationDate { get; }

	ICustomFieldDefinitionCollection CustomFields { get; }

	string Description { get; set; }

	bool? IsClosed { get; set; }

	bool? IsSubscribed { get; set; }

	IBoardLabelCollection Labels { get; }

	IListCollection Lists { get; }

	IReadOnlyMemberCollection Members { get; }

	IBoardMembershipCollection Memberships { get; }

	string Name { get; set; }

	IOrganization Organization { get; set; }

	IPowerUpCollection PowerUps { get; }

	IReadOnlyCollection<IPowerUpData> PowerUpData { get; }

	IBoardPreferences Preferences { get; }

	IBoardPersonalPreferences PersonalPreferences { get; }

	string Url { get; }

	bool? IsPinned { get; }

	bool? IsStarred { get; }

	DateTime? LastActivity { get; }

	DateTime? LastViewed { get; }

	string ShortLink { get; }

	string ShortUrl { get; }

	IList this[string key] { get; }

	IList this[int index] { get; }

	event Action<IBoard, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
