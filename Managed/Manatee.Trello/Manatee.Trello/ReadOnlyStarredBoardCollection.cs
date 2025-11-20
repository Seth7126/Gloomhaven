using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyStarredBoardCollection : ReadOnlyCollection<IStarredBoard>, IHandle<EntityDeletedEvent<IJsonStarredBoard>>, IHandle
{
	internal ReadOnlyStarredBoardCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Read_StarredBoards, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonStarredBoard> source = await JsonRepository.Execute<List<IJsonStarredBoard>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonStarredBoard jb)
		{
			StarredBoard fromCache = jb.GetFromCache<StarredBoard, IJsonStarredBoard>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jb;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	void IHandle<EntityDeletedEvent<IJsonStarredBoard>>.Handle(EntityDeletedEvent<IJsonStarredBoard> message)
	{
		IStarredBoard item = base.Items.FirstOrDefault((IStarredBoard c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
