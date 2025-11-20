namespace Manatee.Trello;

public interface IMemberPreferences
{
	bool? EnableColorBlindMode { get; set; }

	int? MinutesBetweenSummaries { get; set; }
}
