namespace Manatee.Trello.Json;

public interface IJsonWebhook : IJsonCacheable
{
	[JsonDeserialize]
	[JsonSerialize]
	string Description { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string IdModel { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string CallbackUrl { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Active { get; set; }
}
