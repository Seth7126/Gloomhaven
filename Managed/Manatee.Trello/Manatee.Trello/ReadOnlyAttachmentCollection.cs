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

public class ReadOnlyAttachmentCollection : ReadOnlyCollection<IAttachment>, IHandle<EntityDeletedEvent<IJsonAttachment>>, IHandle
{
	internal ReadOnlyAttachmentCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Dictionary<string, object> parameters = (from kvp in base.AdditionalParameters.Concat(AttachmentContext.CurrentParameters)
			where ((string)kvp.Value).Contains("attachments")
			select kvp).ToDictionary((KeyValuePair<string, object> kvp) => "attachments_" + kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Read_Attachments, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonAttachment> source = await JsonRepository.Execute<List<IJsonAttachment>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonAttachment ja)
		{
			Attachment obj = TrelloConfiguration.Cache.Find<Attachment>(ja.Id) ?? new Attachment(ja, base.OwnerId, base.Auth);
			obj.Json = ja;
			return obj;
		}));
	}

	void IHandle<EntityDeletedEvent<IJsonAttachment>>.Handle(EntityDeletedEvent<IJsonAttachment> message)
	{
		IAttachment item = base.Items.FirstOrDefault((IAttachment c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
