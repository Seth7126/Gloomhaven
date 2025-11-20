using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonOrganization : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string DisplayName { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Desc { get; set; }

	[JsonDeserialize]
	string Url { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Website { get; set; }

	[JsonDeserialize]
	string LogoHash { get; set; }

	[JsonDeserialize]
	List<int> PowerUps { get; set; }

	[JsonDeserialize]
	bool? PaidAccount { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	IJsonOrganizationPreferences Prefs { get; set; }

	[JsonDeserialize]
	List<string> PremiumFeatures { get; set; }

	[JsonDeserialize]
	List<IJsonAction> Actions { get; set; }

	[JsonDeserialize]
	List<IJsonBoard> Boards { get; set; }

	[JsonDeserialize]
	List<IJsonMember> Members { get; set; }

	[JsonDeserialize]
	List<IJsonOrganizationMembership> Memberships { get; set; }

	[JsonDeserialize]
	List<IJsonPowerUpData> PowerUpData { get; set; }
}
