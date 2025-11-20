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

public class ReadOnlyCheckListCollection : ReadOnlyCollection<ICheckList>, IReadOnlyCheckListCollection, IReadOnlyCollection<ICheckList>, IEnumerable<ICheckList>, IEnumerable, IRefreshable, IHandle<EntityDeletedEvent<IJsonCheckList>>, IHandle
{
	public ICheckList this[string key] => GetByKey(key);

	internal ReadOnlyCheckListCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(CheckListContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Read_CheckLists, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonCheckList> source = await JsonRepository.Execute<List<IJsonCheckList>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonCheckList jc)
		{
			CheckList fromCache = jc.GetFromCache<CheckList, IJsonCheckList>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jc;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	private ICheckList GetByKey(string key)
	{
		return this.FirstOrDefault((ICheckList cl) => key.In(cl.Id, cl.Name));
	}

	void IHandle<EntityDeletedEvent<IJsonCheckList>>.Handle(EntityDeletedEvent<IJsonCheckList> message)
	{
		ICheckList item = base.Items.FirstOrDefault((ICheckList c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
