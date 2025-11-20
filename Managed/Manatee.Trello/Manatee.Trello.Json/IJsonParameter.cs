namespace Manatee.Trello.Json;

public interface IJsonParameter
{
	[JsonSerialize]
	string String { get; set; }

	[JsonSerialize]
	bool? Boolean { get; set; }

	[JsonSerialize]
	object Object { get; set; }
}
