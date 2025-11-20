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

public class ReadOnlyBoardCollection : ReadOnlyCollection<IBoard>, IReadOnlyBoardCollection, IReadOnlyCollection<IBoard>, IEnumerable<IBoard>, IEnumerable, IRefreshable, IHandle<EntityUpdatedEvent<IJsonBoard>>, IHandle, IHandle<EntityDeletedEvent<IJsonBoard>>
{
	private readonly EntityRequestType _updateRequestType;

	public IBoard this[string key] => GetByKey(key);

	internal ReadOnlyBoardCollection(Type type, Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		_updateRequestType = ((type == typeof(Organization)) ? EntityRequestType.Organization_Read_Boards : EntityRequestType.Member_Read_Boards);
		EventAggregator.Subscribe(this);
	}

	public void Filter(BoardFilter filter)
	{
		base.AdditionalParameters["filter"] = filter.GetDescription();
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(BoardContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(_updateRequestType, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonBoard> source = await JsonRepository.Execute<List<IJsonBoard>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonBoard jb)
		{
			Board fromCache = jb.GetFromCache<Board, IJsonBoard>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jb;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	private IBoard GetByKey(string key)
	{
		return this.FirstOrDefault((IBoard b) => key.In(b.Id, b.Name));
	}

	void IHandle<EntityUpdatedEvent<IJsonBoard>>.Handle(EntityUpdatedEvent<IJsonBoard> message)
	{
		switch (_updateRequestType)
		{
		case EntityRequestType.Organization_Read_Boards:
			if (message.Properties.Contains("Organization"))
			{
				IBoard board = base.Items.FirstOrDefault((IBoard b) => b.Id == message.Data.Id);
				if (message.Data.Organization?.Id != base.OwnerId && board != null)
				{
					base.Items.Remove(board);
				}
				else if (message.Data.Organization?.Id == base.OwnerId && board == null)
				{
					base.Items.Add(message.Data.GetFromCache<Board>(base.Auth));
				}
			}
			break;
		case EntityRequestType.Member_Read_Boards:
			if (message.Properties.Contains("Members") && message.Data.Members != null)
			{
				IBoard board = base.Items.FirstOrDefault((IBoard b) => b.Id == message.Data.Id);
				List<string> list = message.Data.Members.Select((IJsonMember m) => m.Id).ToList();
				if (!list.Contains(base.OwnerId) && board != null)
				{
					base.Items.Remove(board);
				}
				else if (list.Contains(base.OwnerId) && board == null)
				{
					base.Items.Add(message.Data.GetFromCache<Board>(base.Auth));
				}
			}
			break;
		}
	}

	void IHandle<EntityDeletedEvent<IJsonBoard>>.Handle(EntityDeletedEvent<IJsonBoard> message)
	{
		IBoard item = base.Items.FirstOrDefault((IBoard c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
