namespace Manatee.Trello.Json;

public interface IJsonPowerUpData : IJsonCacheable
{
	[JsonDeserialize]
	string PluginId { get; set; }

	[JsonDeserialize]
	string Value { get; set; }
}
