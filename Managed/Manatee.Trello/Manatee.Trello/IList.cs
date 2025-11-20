using System;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IList : ICanWebhook, ICacheable, IRefreshable
{
	IReadOnlyActionCollection Actions { get; }

	IBoard Board { get; set; }

	ICardCollection Cards { get; }

	DateTime CreationDate { get; }

	bool? IsArchived { get; set; }

	bool? IsSubscribed { get; set; }

	string Name { get; set; }

	Position Position { get; set; }

	ICard this[string key] { get; }

	ICard this[int index] { get; }

	event Action<IList, IEnumerable<string>> Updated;
}
