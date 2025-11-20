using System.Collections.Generic;
using Manatee.Trello.Internal.DataAccess;

namespace Manatee.Trello;

public static class BoardExtensions
{
	public static IReadOnlyCardCollection CardsForMember(this IBoard board, IMember member)
	{
		return new ReadOnlyCardCollection(EntityRequestType.Board_Read_CardsForMember, () => board.Id, ((Board)board).Auth, new Dictionary<string, object> { { "_idMember", member.Id } });
	}
}
