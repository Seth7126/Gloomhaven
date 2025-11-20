namespace Manatee.Trello.Json;

public interface IJsonBoardMembership : IJsonCacheable
{
	[JsonDeserialize]
	IJsonMember Member { get; set; }

	[JsonDeserialize]
	[JsonSerialize(IsRequired = true)]
	BoardMembershipType? MemberType { get; set; }

	[JsonDeserialize]
	bool? Deactivated { get; set; }
}
