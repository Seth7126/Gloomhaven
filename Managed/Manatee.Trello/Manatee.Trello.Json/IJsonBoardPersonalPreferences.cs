namespace Manatee.Trello.Json;

public interface IJsonBoardPersonalPreferences
{
	[JsonDeserialize]
	bool? ShowSidebar { get; set; }

	[JsonDeserialize]
	bool? ShowSidebarMembers { get; set; }

	[JsonDeserialize]
	bool? ShowSidebarBoardActions { get; set; }

	[JsonDeserialize]
	bool? ShowSidebarActivity { get; set; }

	[JsonDeserialize]
	bool? ShowListGuide { get; set; }

	[JsonDeserialize]
	IJsonPosition EmailPosition { get; set; }

	[JsonDeserialize]
	IJsonList EmailList { get; set; }
}
