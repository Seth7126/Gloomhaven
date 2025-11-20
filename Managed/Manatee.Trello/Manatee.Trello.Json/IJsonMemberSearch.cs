using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonMemberSearch
{
	[JsonDeserialize]
	List<IJsonMember> Members { get; set; }

	[JsonDeserialize]
	IJsonBoard Board { get; set; }

	[JsonSerialize]
	int? Limit { get; set; }

	[JsonSerialize]
	bool? OnlyOrgMembers { get; set; }

	[JsonSerialize]
	IJsonOrganization Organization { get; set; }

	[JsonSerialize(IsRequired = true)]
	string Query { get; set; }
}
