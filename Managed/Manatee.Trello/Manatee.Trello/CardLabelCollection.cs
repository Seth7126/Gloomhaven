using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CardLabelCollection : ReadOnlyCollection<ILabel>, ICardLabelCollection, IReadOnlyCollection<ILabel>, IEnumerable<ILabel>, IEnumerable, IRefreshable, IHandle<EntityDeletedEvent<IJsonLabel>>, IHandle
{
	private readonly CardContext _context;

	internal CardLabelCollection(CardContext context, TrelloAuthorization auth)
		: base((Func<string>)(() => context.Data.Id), auth)
	{
		_context = context;
		EventAggregator.Subscribe(this);
	}

	public async Task Add(ILabel label, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<ILabel>.Instance.Validate(null, label);
		if (text != null)
		{
			throw new ValidationException<ILabel>(label, new string[1] { text });
		}
		IJsonParameter jsonParameter = TrelloConfiguration.JsonFactory.Create<IJsonParameter>();
		jsonParameter.String = label.Id;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_AddLabel, new Dictionary<string, object> { { "_id", base.OwnerId } });
		await JsonRepository.Execute(base.Auth, endpoint, jsonParameter, ct);
		base.Items.Add(label);
		await _context.Synchronize(force: true, ct);
	}

	public async Task Remove(ILabel label, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<ILabel>.Instance.Validate(null, label);
		if (text != null)
		{
			throw new ValidationException<ILabel>(label, new string[1] { text });
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_RemoveLabel, new Dictionary<string, object>
		{
			{ "_id", base.OwnerId },
			{ "_labelId", label.Id }
		});
		await JsonRepository.Execute(base.Auth, endpoint, ct);
		base.Items.Remove(label);
		await _context.Synchronize(force: true, ct);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Read_Labels, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonLabel> source = await JsonRepository.Execute<List<IJsonLabel>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		base.Items.AddRange(source.Select(delegate(IJsonLabel ja)
		{
			Label fromCache = ja.GetFromCache<Label, IJsonLabel>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = ja;
			return fromCache;
		}));
	}

	void IHandle<EntityDeletedEvent<IJsonLabel>>.Handle(EntityDeletedEvent<IJsonLabel> message)
	{
		ILabel item = base.Items.FirstOrDefault((ILabel c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
