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

public class ReadOnlyActionCollection : ReadOnlyCollection<IAction>, IReadOnlyActionCollection, IReadOnlyCollection<IAction>, IEnumerable<IAction>, IEnumerable, IRefreshable, IHandle<EntityDeletedEvent<IJsonAction>>, IHandle
{
	private static readonly Dictionary<Type, EntityRequestType> RequestTypes;

	private readonly EntityRequestType _updateRequestType;

	static ReadOnlyActionCollection()
	{
		RequestTypes = new Dictionary<Type, EntityRequestType>
		{
			{
				typeof(Board),
				EntityRequestType.Board_Read_Actions
			},
			{
				typeof(Card),
				EntityRequestType.Card_Read_Actions
			},
			{
				typeof(List),
				EntityRequestType.List_Read_Actions
			},
			{
				typeof(Member),
				EntityRequestType.Member_Read_Actions
			},
			{
				typeof(Organization),
				EntityRequestType.Organization_Read_Actions
			}
		};
	}

	internal ReadOnlyActionCollection(Type type, Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		_updateRequestType = RequestTypes[type];
		EventAggregator.Subscribe(this);
	}

	public void Filter(ActionType actionType)
	{
		IEnumerable<ActionType> flags = actionType.GetFlags();
		Filter(flags);
	}

	public void Filter(IEnumerable<ActionType> actionTypes)
	{
		string text = (base.AdditionalParameters.ContainsKey("filter") ? ((string)base.AdditionalParameters["filter"]) : string.Empty);
		if (!text.IsNullOrWhiteSpace())
		{
			text += ",";
		}
		text += actionTypes.Aggregate(ActionType.Unknown, (ActionType c, ActionType a) => c | a).ToString();
		base.AdditionalParameters["filter"] = text;
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
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(ActionContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(_updateRequestType, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonAction> source = await JsonRepository.Execute<List<IJsonAction>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonAction ja)
		{
			Action fromCache = ja.GetFromCache<Action, IJsonAction>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = ja;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	void IHandle<EntityDeletedEvent<IJsonAction>>.Handle(EntityDeletedEvent<IJsonAction> message)
	{
		IAction item = base.Items.FirstOrDefault((IAction c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
