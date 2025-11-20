namespace Manatee.Trello.Json;

public interface IJsonCheckItem : IJsonCacheable
{
	[JsonDeserialize]
	[JsonSerialize]
	IJsonCheckList CheckList { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	CheckItemState? State { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonPosition Pos { get; set; }
}
