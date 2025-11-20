using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonCheckList : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	IJsonBoard Board { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonCard Card { get; set; }

	[JsonDeserialize]
	List<IJsonCheckItem> CheckItems { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonPosition Pos { get; set; }

	[JsonSerialize]
	IJsonCheckList CheckListSource { get; set; }
}
