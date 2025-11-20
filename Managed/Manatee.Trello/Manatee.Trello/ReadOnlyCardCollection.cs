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

public class ReadOnlyCardCollection : ReadOnlyCollection<ICard>, IReadOnlyCardCollection, IReadOnlyCollection<ICard>, IEnumerable<ICard>, IEnumerable, IRefreshable, IHandle<EntityUpdatedEvent<IJsonCard>>, IHandle, IHandle<EntityDeletedEvent<IJsonCard>>
{
	private readonly EntityRequestType _updateRequestType;

	private readonly Dictionary<string, object> _requestParameters;

	public ICard this[string key] => GetByKey(key);

	internal ReadOnlyCardCollection(Type type, Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		_updateRequestType = ((type == typeof(List)) ? EntityRequestType.List_Read_Cards : EntityRequestType.Board_Read_Cards);
		_requestParameters = new Dictionary<string, object>();
		EventAggregator.Subscribe(this);
	}

	internal ReadOnlyCardCollection(EntityRequestType requestType, Func<string> getOwnerId, TrelloAuthorization auth, Dictionary<string, object> requestParameters = null)
		: base(getOwnerId, auth)
	{
		_updateRequestType = requestType;
		_requestParameters = requestParameters ?? new Dictionary<string, object>();
		EventAggregator.Subscribe(this);
	}

	public void Filter(CardFilter filter)
	{
		if (_updateRequestType == EntityRequestType.List_Read_Cards && filter == CardFilter.Visible)
		{
			base.AdditionalParameters.Remove("filter");
		}
		else
		{
			base.AdditionalParameters["filter"] = filter.GetDescription();
		}
	}

	public void Filter(DateTime? start, DateTime? end)
	{
		if (start.HasValue)
		{
			base.AdditionalParameters["since"] = start.Value.ToUniversalTime().ToString("O");
		}
		if (end.HasValue)
		{
			base.AdditionalParameters["before"] = end.Value.ToUniversalTime().ToString("O");
		}
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		_requestParameters["_id"] = base.OwnerId;
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(CardContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(_updateRequestType, _requestParameters);
		List<IJsonCard> source = await JsonRepository.Execute<List<IJsonCard>>(base.Auth, endpoint, ct, parameters);
		List<ICard> first = new List<ICard>(base.Items);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonCard jc)
		{
			Card fromCache = jc.GetFromCache<Card, IJsonCard>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jc;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
		foreach (Card item in first.Except(base.Items, CacheableComparer.Get<ICard>()).OfType<Card>().ToList())
		{
			item.Json.List = null;
		}
	}

	private ICard GetByKey(string key)
	{
		return this.FirstOrDefault((ICard c) => key.In(c.Id, c.Name));
	}

	void IHandle<EntityUpdatedEvent<IJsonCard>>.Handle(EntityUpdatedEvent<IJsonCard> message)
	{
		switch (_updateRequestType)
		{
		case EntityRequestType.Board_Read_Cards:
			if (message.Properties.Contains("Board"))
			{
				ICard card = base.Items.FirstOrDefault((ICard c) => c.Id == message.Data.Id);
				if (message.Data.Board?.Id != base.OwnerId && card != null)
				{
					base.Items.Remove(card);
				}
				else if (message.Data.Board?.Id == base.OwnerId && card == null)
				{
					base.Items.Add(message.Data.GetFromCache<Card>(base.Auth));
				}
			}
			break;
		case EntityRequestType.List_Read_Cards:
			if (message.Properties.Contains("List"))
			{
				ICard card = base.Items.FirstOrDefault((ICard c) => c.Id == message.Data.Id);
				if (message.Data.List?.Id != base.OwnerId && card != null)
				{
					base.Items.Remove(card);
				}
				else if (message.Data.List?.Id == base.OwnerId && card == null)
				{
					base.Items.Add(message.Data.GetFromCache<Card>(base.Auth));
				}
			}
			break;
		case EntityRequestType.Member_Read_Cards:
			if (message.Properties.Contains("Members"))
			{
				ICard card = base.Items.FirstOrDefault((ICard c) => c.Id == message.Data.Id);
				List<string> list = message.Data.Members.Select((IJsonMember m) => m.Id).ToList();
				if (!list.Contains(base.OwnerId) && card != null)
				{
					base.Items.Remove(card);
				}
				else if (list.Contains(base.OwnerId) && card == null)
				{
					base.Items.Add(message.Data.GetFromCache<Card>(base.Auth));
				}
			}
			break;
		}
	}

	void IHandle<EntityDeletedEvent<IJsonCard>>.Handle(EntityDeletedEvent<IJsonCard> message)
	{
		ICard item = base.Items.FirstOrDefault((ICard c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
