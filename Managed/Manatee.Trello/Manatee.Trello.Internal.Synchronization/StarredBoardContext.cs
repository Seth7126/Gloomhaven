using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class StarredBoardContext : DeletableSynchronizationContext<IJsonStarredBoard>
{
	private readonly string _ownerId;

	static StarredBoardContext()
	{
		SynchronizationContext<IJsonStarredBoard>.Properties = new Dictionary<string, Property<IJsonStarredBoard>>
		{
			{
				"Board",
				new Property<IJsonStarredBoard, Board>((IJsonStarredBoard d, TrelloAuthorization a) => d.Board?.GetFromCache<Board, IJsonBoard>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonStarredBoard d, Board o)
				{
					d.Board = o?.Json;
				})
			},
			{
				"Position",
				new Property<IJsonStarredBoard, Position>((IJsonStarredBoard d, TrelloAuthorization a) => Position.GetPosition(d.Pos), delegate(IJsonStarredBoard d, Position o)
				{
					d.Pos = Position.GetJson(o);
				})
			}
		};
	}

	public StarredBoardContext(string id, string ownerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_ownerId = ownerId;
		base.Data.Id = id;
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.StarredBoard_Read_Refresh, new Dictionary<string, object>
		{
			{ "_idMember", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.StarredBoard_Write_Delete, new Dictionary<string, object>
		{
			{ "_idMember", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override async Task SubmitData(IJsonStarredBoard json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.StarredBoard_Write_Update, new Dictionary<string, object>
		{
			{ "_idMember", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}
}
