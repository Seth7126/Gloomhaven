using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class OrganizationMembershipContext : SynchronizationContext<IJsonOrganizationMembership>
{
	private readonly string _ownerId;

	static OrganizationMembershipContext()
	{
		SynchronizationContext<IJsonOrganizationMembership>.Properties = new Dictionary<string, Property<IJsonOrganizationMembership>>
		{
			{
				"Id",
				new Property<IJsonOrganizationMembership, string>((IJsonOrganizationMembership d, TrelloAuthorization a) => d.Id, delegate(IJsonOrganizationMembership d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsUnconfirmed",
				new Property<IJsonOrganizationMembership, bool?>((IJsonOrganizationMembership d, TrelloAuthorization a) => d.Unconfirmed, delegate(IJsonOrganizationMembership d, bool? o)
				{
					d.Unconfirmed = o;
				})
			},
			{
				"Member",
				new Property<IJsonOrganizationMembership, Member>((IJsonOrganizationMembership d, TrelloAuthorization a) => d.Member.GetFromCache<Member, IJsonMember>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonOrganizationMembership d, Member o)
				{
					d.Member = o?.Json;
				})
			},
			{
				"MemberType",
				new Property<IJsonOrganizationMembership, OrganizationMembershipType?>((IJsonOrganizationMembership d, TrelloAuthorization a) => d.MemberType, delegate(IJsonOrganizationMembership d, OrganizationMembershipType? o)
				{
					d.MemberType = o;
				})
			}
		};
	}

	public OrganizationMembershipContext(string id, string ownerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_ownerId = ownerId;
		base.Data.Id = id;
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.OrganizationMembership_Read_Refresh, new Dictionary<string, object>
		{
			{ "_organizationId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override async Task<IJsonOrganizationMembership> GetData(CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.OrganizationMembership_Read_Refresh, new Dictionary<string, object>
		{
			{ "_organizationId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
		return await JsonRepository.Execute<IJsonOrganizationMembership>(base.Auth, endpoint, ct);
	}

	protected override async Task SubmitData(IJsonOrganizationMembership json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.OrganizationMembership_Write_Update, new Dictionary<string, object>
		{
			{ "_organizationId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}
}
