using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyStickerCollection : ReadOnlyCollection<ISticker>, IHandle<EntityDeletedEvent<IJsonSticker>>, IHandle
{
	internal ReadOnlyStickerCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(StickerContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Read_Stickers, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonSticker> source = await JsonRepository.Execute<List<IJsonSticker>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonSticker ja)
		{
			Sticker obj = TrelloConfiguration.Cache.Find<Sticker>(ja.Id) ?? new Sticker(ja, base.OwnerId, base.Auth);
			obj.Json = ja;
			return obj;
		}));
		EventAggregator.Subscribe(this);
	}

	void IHandle<EntityDeletedEvent<IJsonSticker>>.Handle(EntityDeletedEvent<IJsonSticker> message)
	{
		ISticker item = base.Items.FirstOrDefault((ISticker c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
