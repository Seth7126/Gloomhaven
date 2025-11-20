using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyCommentReactionCollection : ReadOnlyCollection<ICommentReaction>, IHandle<EntityDeletedEvent<IJsonCommentReaction>>, IHandle
{
	private readonly Dictionary<string, object> _requestParameters;

	internal ReadOnlyCommentReactionCollection(Type type, Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		_requestParameters = new Dictionary<string, object>();
		EventAggregator.Subscribe(this);
	}

	internal ReadOnlyCommentReactionCollection(Func<string> getOwnerId, TrelloAuthorization auth, Dictionary<string, object> requestParameters = null)
		: base(getOwnerId, auth)
	{
		_requestParameters = requestParameters ?? new Dictionary<string, object>();
		EventAggregator.Subscribe(this);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		_requestParameters["_id"] = base.OwnerId;
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(CardContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Action_Read_Reactions, _requestParameters);
		List<IJsonCommentReaction> source = await JsonRepository.Execute<List<IJsonCommentReaction>>(base.Auth, endpoint, ct, parameters);
		List<ICommentReaction> first = new List<ICommentReaction>(base.Items);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonCommentReaction jc)
		{
			CommentReaction fromCache = jc.GetFromCache<CommentReaction, IJsonCommentReaction>(base.Auth, overwrite: true, new object[1] { base.OwnerId });
			fromCache.Json = jc;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
		foreach (CommentReaction item in first.Except(base.Items, CacheableComparer.Get<ICommentReaction>()).OfType<CommentReaction>().ToList())
		{
			item.Json.Comment = null;
		}
	}

	void IHandle<EntityDeletedEvent<IJsonCommentReaction>>.Handle(EntityDeletedEvent<IJsonCommentReaction> message)
	{
		ICommentReaction item = base.Items.FirstOrDefault((ICommentReaction c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
