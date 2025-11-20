namespace Manatee.Trello.Json;

public interface IJsonPowerUp : IJsonCacheable
{
	[JsonDeserialize]
	string Name { get; set; }

	[JsonDeserialize]
	bool? Public { get; set; }

	[JsonDeserialize]
	string Url { get; set; }
}
