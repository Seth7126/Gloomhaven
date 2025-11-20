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

public class ReadOnlyDropDownOptionCollection : ReadOnlyCollection<IDropDownOption>, IHandle<EntityDeletedEvent<IJsonCustomDropDownOption>>, IHandle
{
	internal ReadOnlyDropDownOptionCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CustomFieldDefinition_Read_Options, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonCustomDropDownOption> source = await JsonRepository.Execute<List<IJsonCustomDropDownOption>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonCustomDropDownOption jb)
		{
			DropDownOption fromCache = jb.GetFromCache<DropDownOption, IJsonCustomDropDownOption>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jb;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	void IHandle<EntityDeletedEvent<IJsonCustomDropDownOption>>.Handle(EntityDeletedEvent<IJsonCustomDropDownOption> message)
	{
		IDropDownOption item = base.Items.FirstOrDefault((IDropDownOption c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
