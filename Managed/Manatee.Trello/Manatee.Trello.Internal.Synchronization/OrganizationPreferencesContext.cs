using System.Collections.Generic;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class OrganizationPreferencesContext : LinkedSynchronizationContext<IJsonOrganizationPreferences>
{
	static OrganizationPreferencesContext()
	{
		SynchronizationContext<IJsonOrganizationPreferences>.Properties = new Dictionary<string, Property<IJsonOrganizationPreferences>>
		{
			{
				"PermissionLevel",
				new Property<IJsonOrganizationPreferences, OrganizationPermissionLevel?>((IJsonOrganizationPreferences d, TrelloAuthorization a) => d.PermissionLevel, delegate(IJsonOrganizationPreferences d, OrganizationPermissionLevel? o)
				{
					d.PermissionLevel = o;
				})
			},
			{
				"ExternalMembersDisabled",
				new Property<IJsonOrganizationPreferences, bool?>((IJsonOrganizationPreferences d, TrelloAuthorization a) => d.ExternalMembersDisabled, delegate(IJsonOrganizationPreferences d, bool? o)
				{
					d.ExternalMembersDisabled = o;
				})
			},
			{
				"AssociatedDomain",
				new Property<IJsonOrganizationPreferences, string>((IJsonOrganizationPreferences d, TrelloAuthorization a) => d.AssociatedDomain, delegate(IJsonOrganizationPreferences d, string o)
				{
					d.AssociatedDomain = o;
				})
			},
			{
				"PublicBoardVisibility",
				new Property<IJsonOrganizationPreferences, OrganizationBoardVisibility?>((IJsonOrganizationPreferences d, TrelloAuthorization a) => d.BoardVisibilityRestrict.Public, delegate(IJsonOrganizationPreferences d, OrganizationBoardVisibility? o)
				{
					d.BoardVisibilityRestrict.Public = o;
				})
			},
			{
				"OrganizationBoardVisibility",
				new Property<IJsonOrganizationPreferences, OrganizationBoardVisibility?>((IJsonOrganizationPreferences d, TrelloAuthorization a) => d.BoardVisibilityRestrict.Org, delegate(IJsonOrganizationPreferences d, OrganizationBoardVisibility? o)
				{
					d.BoardVisibilityRestrict.Org = o;
				})
			},
			{
				"PrivateBoardVisibility",
				new Property<IJsonOrganizationPreferences, OrganizationBoardVisibility?>((IJsonOrganizationPreferences d, TrelloAuthorization a) => d.BoardVisibilityRestrict.Private, delegate(IJsonOrganizationPreferences d, OrganizationBoardVisibility? o)
				{
					d.BoardVisibilityRestrict.Private = o;
				})
			}
		};
	}

	internal OrganizationPreferencesContext(TrelloAuthorization auth)
		: base(auth)
	{
		base.Data.BoardVisibilityRestrict = TrelloConfiguration.JsonFactory.Create<IJsonBoardVisibilityRestrict>();
	}
}
