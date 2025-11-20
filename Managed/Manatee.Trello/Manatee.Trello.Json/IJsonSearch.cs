using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonSearch
{
	[JsonDeserialize]
	List<IJsonAction> Actions { get; set; }

	[JsonDeserialize]
	List<IJsonBoard> Boards { get; set; }

	[JsonDeserialize]
	List<IJsonCard> Cards { get; set; }

	[JsonDeserialize]
	List<IJsonMember> Members { get; set; }

	[JsonDeserialize]
	List<IJsonOrganization> Organizations { get; set; }

	[JsonSerialize(IsRequired = true)]
	string Query { get; set; }

	[JsonSerialize]
	List<IJsonCacheable> Context { get; set; }

	[JsonSerialize]
	SearchModelType? Types { get; set; }

	[JsonSerialize]
	int? Limit { get; set; }

	[JsonSerialize]
	bool Partial { get; set; }
}
