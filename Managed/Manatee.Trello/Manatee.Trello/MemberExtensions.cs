using Manatee.Trello.Internal.DataAccess;

namespace Manatee.Trello;

public static class MemberExtensions
{
	public static IReadOnlyCollection<ICard> Cards(this IMember member)
	{
		return new ReadOnlyCardCollection(EntityRequestType.Member_Read_Cards, () => member.Id, ((Member)member).Auth);
	}
}
