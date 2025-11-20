namespace Manatee.Trello.Json;

public interface IJsonImagePreview : IJsonCacheable
{
	[JsonDeserialize]
	int? Width { get; set; }

	[JsonDeserialize]
	int? Height { get; set; }

	[JsonDeserialize]
	string Url { get; set; }

	[JsonDeserialize]
	bool? Scaled { get; set; }
}
