using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonList : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Closed { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonBoard Board { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonPosition Pos { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Subscribed { get; set; }

	[JsonDeserialize]
	List<IJsonAction> Actions { get; set; }

	[JsonDeserialize]
	List<IJsonCard> Cards { get; set; }
}
