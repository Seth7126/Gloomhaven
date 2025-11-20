namespace Manatee.Trello;

public interface IBoardPersonalPreferences
{
	IList EmailList { get; set; }

	Position EmailPosition { get; set; }

	bool? ShowListGuide { get; set; }

	bool? ShowSidebar { get; set; }

	bool? ShowSidebarActivity { get; set; }

	bool? ShowSidebarBoardActions { get; set; }

	bool? ShowSidebarMembers { get; set; }
}
