namespace Manatee.Trello.Json;

public interface IJsonCacheable
{
	[JsonDeserialize]
	[JsonSerialize(IsRequired = true)]
	string Id { get; set; }
}
