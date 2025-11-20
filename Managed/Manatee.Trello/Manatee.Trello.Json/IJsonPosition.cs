namespace Manatee.Trello.Json;

public interface IJsonPosition
{
	[JsonDeserialize]
	[JsonSerialize]
	double? Explicit { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Named { get; set; }
}
