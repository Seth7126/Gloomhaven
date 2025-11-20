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

public class ReadOnlyBoardBackgroundCollection : ReadOnlyCollection<IBoardBackground>, IHandle<EntityDeletedEvent<IJsonBoardBackground>>, IHandle
{
	internal ReadOnlyBoardBackgroundCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	internal override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		base.AdditionalParameters["filter"] = "custom";
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Read_BoardBackgrounds, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonBoardBackground> source = await JsonRepository.Execute<List<IJsonBoardBackground>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonBoardBackground jb)
		{
			BoardBackground fromCache = jb.GetFromCache<BoardBackground, IJsonBoardBackground>(base.Auth, overwrite: true, new object[1] { base.OwnerId });
			fromCache.Json = jb;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	void IHandle<EntityDeletedEvent<IJsonBoardBackground>>.Handle(EntityDeletedEvent<IJsonBoardBackground> message)
	{
		IBoardBackground item = base.Items.FirstOrDefault((IBoardBackground c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
