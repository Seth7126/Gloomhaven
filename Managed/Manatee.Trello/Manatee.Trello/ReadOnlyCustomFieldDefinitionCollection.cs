using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyCustomFieldDefinitionCollection : ReadOnlyCollection<ICustomFieldDefinition>, IHandle<EntityDeletedEvent<IJsonCustomFieldDefinition>>, IHandle
{
	internal ReadOnlyCustomFieldDefinitionCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Read_CustomFields, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonCustomFieldDefinition> source = await JsonRepository.Execute<List<IJsonCustomFieldDefinition>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonCustomFieldDefinition ja)
		{
			CustomFieldDefinition obj = TrelloConfiguration.Cache.Find<CustomFieldDefinition>(ja.Id) ?? new CustomFieldDefinition(ja, base.Auth);
			obj.Json = ja;
			return obj;
		}));
		EventAggregator.Subscribe(this);
	}

	void IHandle<EntityDeletedEvent<IJsonCustomFieldDefinition>>.Handle(EntityDeletedEvent<IJsonCustomFieldDefinition> message)
	{
		ICustomFieldDefinition item = base.Items.FirstOrDefault((ICustomFieldDefinition c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
