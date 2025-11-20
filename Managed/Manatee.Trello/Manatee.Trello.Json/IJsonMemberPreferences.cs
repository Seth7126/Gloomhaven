namespace Manatee.Trello.Json;

public interface IJsonMemberPreferences
{
	[JsonDeserialize]
	int? MinutesBetweenSummaries { get; set; }

	[JsonDeserialize]
	bool? ColorBlind { get; set; }
}
