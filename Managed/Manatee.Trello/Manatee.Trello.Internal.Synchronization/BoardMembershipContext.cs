using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class BoardMembershipContext : SynchronizationContext<IJsonBoardMembership>
{
	private readonly string _ownerId;

	static BoardMembershipContext()
	{
		SynchronizationContext<IJsonBoardMembership>.Properties = new Dictionary<string, Property<IJsonBoardMembership>>
		{
			{
				"Id",
				new Property<IJsonBoardMembership, string>((IJsonBoardMembership d, TrelloAuthorization a) => d.Id, delegate(IJsonBoardMembership d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsDeactivated",
				new Property<IJsonBoardMembership, bool?>((IJsonBoardMembership d, TrelloAuthorization a) => d.Deactivated, delegate(IJsonBoardMembership d, bool? o)
				{
					d.Deactivated = o;
				})
			},
			{
				"Member",
				new Property<IJsonBoardMembership, Member>((IJsonBoardMembership d, TrelloAuthorization a) => d.Member.GetFromCache<Member, IJsonMember>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonBoardMembership d, Member o)
				{
					d.Member = o?.Json;
				})
			},
			{
				"MemberType",
				new Property<IJsonBoardMembership, BoardMembershipType?>((IJsonBoardMembership d, TrelloAuthorization a) => d.MemberType, delegate(IJsonBoardMembership d, BoardMembershipType? o)
				{
					d.MemberType = o;
				})
			}
		};
	}

	public BoardMembershipContext(string id, string ownerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_ownerId = ownerId;
		base.Data.Id = id;
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.BoardMembership_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task<IJsonBoardMembership> GetData(CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.BoardMembership_Read_Refresh, new Dictionary<string, object>
		{
			{ "_boardId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
		return await JsonRepository.Execute<IJsonBoardMembership>(base.Auth, endpoint, ct);
	}

	protected override async Task SubmitData(IJsonBoardMembership json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.BoardMembership_Write_Update, new Dictionary<string, object>
		{
			{ "_boardId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}
}
