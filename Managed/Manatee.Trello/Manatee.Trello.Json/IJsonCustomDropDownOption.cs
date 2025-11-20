namespace Manatee.Trello.Json;

public interface IJsonCustomDropDownOption : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	IJsonCustomFieldDefinition Field { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Text { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	LabelColor? Color { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonPosition Pos { get; set; }
}
