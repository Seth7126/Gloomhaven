namespace Manatee.Trello.Json;

public interface IJsonNotificationOldData
{
	[JsonDeserialize]
	string Desc { get; set; }

	[JsonDeserialize]
	IJsonList List { get; set; }

	[JsonDeserialize]
	double? Pos { get; set; }

	[JsonDeserialize]
	string Text { get; set; }

	[JsonDeserialize]
	bool? Closed { get; set; }
}
