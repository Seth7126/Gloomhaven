using System.Collections.Generic;

namespace Manatee.Trello;

public interface ISearch : IRefreshable
{
	IEnumerable<IAction> Actions { get; }

	IEnumerable<IBoard> Boards { get; }

	IEnumerable<ICard> Cards { get; }

	IEnumerable<IMember> Members { get; }

	IEnumerable<IOrganization> Organizations { get; }

	string Query { get; }
}
