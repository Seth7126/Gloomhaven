using System;

namespace Manatee.Trello.Json;

public interface IJsonCustomField : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	IJsonCustomFieldDefinition Definition { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	string Text { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	double? Number { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	DateTime? Date { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	bool? Checked { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	IJsonCustomDropDownOption Selected { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	CustomFieldType Type { get; set; }
}
