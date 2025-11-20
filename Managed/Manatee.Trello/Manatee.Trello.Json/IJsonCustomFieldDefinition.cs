using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonCustomFieldDefinition : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	[JsonSerialize]
	IJsonBoard Board { get; set; }

	[JsonDeserialize]
	string FieldGroup { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonPosition Pos { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	CustomFieldType? Type { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	List<IJsonCustomDropDownOption> Options { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonCustomFieldDisplayInfo Display { get; set; }
}
