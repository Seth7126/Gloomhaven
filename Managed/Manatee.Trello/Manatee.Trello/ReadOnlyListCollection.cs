using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyListCollection : ReadOnlyCollection<IList>, IReadOnlyListCollection, IReadOnlyCollection<IList>, IEnumerable<IList>, IEnumerable, IRefreshable, IHandle<EntityUpdatedEvent<IJsonList>>, IHandle
{
	public IList this[string key] => GetByKey(key);

	internal ReadOnlyListCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	public void Filter(ListFilter filter)
	{
		base.AdditionalParameters["filter"] = filter.GetDescription();
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(ListContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Read_Lists, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonList> source = await JsonRepository.Execute<List<IJsonList>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonList jl)
		{
			List fromCache = jl.GetFromCache<List, IJsonList>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jl;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	private IList GetByKey(string key)
	{
		return this.FirstOrDefault((IList l) => key.In(l.Id, l.Name));
	}

	void IHandle<EntityUpdatedEvent<IJsonList>>.Handle(EntityUpdatedEvent<IJsonList> message)
	{
		if (message.Properties.Contains("Board"))
		{
			IList list = base.Items.FirstOrDefault((IList l) => l.Id == message.Data.Id);
			if (message.Data.Board?.Id != base.OwnerId && list != null)
			{
				base.Items.Remove(list);
			}
			else if (message.Data.Board?.Id == base.OwnerId && list == null)
			{
				base.Items.Add(message.Data.GetFromCache<List>(base.Auth));
			}
		}
	}
}
