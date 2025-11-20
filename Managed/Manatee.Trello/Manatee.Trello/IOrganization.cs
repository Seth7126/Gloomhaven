using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IOrganization : ICanWebhook, ICacheable, IQueryable, IRefreshable
{
	IReadOnlyActionCollection Actions { get; }

	IBoardCollection Boards { get; }

	DateTime CreationDate { get; }

	string Description { get; set; }

	string DisplayName { get; set; }

	bool IsBusinessClass { get; }

	IReadOnlyMemberCollection Members { get; }

	IOrganizationMembershipCollection Memberships { get; }

	string Name { get; set; }

	IReadOnlyCollection<IPowerUpData> PowerUpData { get; }

	IOrganizationPreferences Preferences { get; }

	string Url { get; }

	string Website { get; set; }

	event Action<IOrganization, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
