namespace Manatee.Trello.Json;

public interface IJsonOrganizationMembership : IJsonCacheable
{
	[JsonDeserialize]
	IJsonMember Member { get; set; }

	[JsonDeserialize]
	[JsonSerialize(IsRequired = true)]
	OrganizationMembershipType? MemberType { get; set; }

	[JsonDeserialize]
	bool? Unconfirmed { get; set; }
}
