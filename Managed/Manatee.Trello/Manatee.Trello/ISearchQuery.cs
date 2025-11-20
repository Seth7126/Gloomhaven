namespace Manatee.Trello;

public interface ISearchQuery
{
	ISearchQuery Text(string text);

	ISearchQuery TextInName(string text);

	ISearchQuery TextInDescription(string text);

	ISearchQuery TextInComments(string text);

	ISearchQuery TextInCheckLists(string text);

	ISearchQuery Member(IMember member);

	ISearchQuery Label(ILabel label);

	ISearchQuery Label(LabelColor labelColor);

	ISearchQuery IsArchived();

	ISearchQuery IsOpen();

	ISearchQuery IsStarred();

	ISearchQuery DueWithinDay();

	ISearchQuery DueWithinWeek();

	ISearchQuery DueWithinMonth();

	ISearchQuery DueWithinDays(int days);

	ISearchQuery Overdue();

	ISearchQuery CreatedWithinDay();

	ISearchQuery CreatedWithinWeek();

	ISearchQuery CreatedWithinMonth();

	ISearchQuery CreatedWithinDays(int days);

	ISearchQuery EditedWithinDay();

	ISearchQuery EditedWithinWeek();

	ISearchQuery EditedWithinMonth();

	ISearchQuery EditedWithinDays(int days);
}
