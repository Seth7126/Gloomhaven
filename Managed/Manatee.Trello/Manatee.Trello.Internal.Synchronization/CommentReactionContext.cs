using System;
using System.Collections.Generic;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class CommentReactionContext : DeletableSynchronizationContext<IJsonCommentReaction>
{
	private readonly string _ownerId;

	static CommentReactionContext()
	{
		SynchronizationContext<IJsonCommentReaction>.Properties = new Dictionary<string, Property<IJsonCommentReaction>>
		{
			{
				"Comment",
				new Property<IJsonCommentReaction, Action>((IJsonCommentReaction d, TrelloAuthorization a) => d.Comment?.GetFromCache<Action, IJsonAction>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCommentReaction d, Action o)
				{
					d.Comment = o?.Json;
				})
			},
			{
				"Emoji",
				new Property<IJsonCommentReaction, Emoji>((IJsonCommentReaction d, TrelloAuthorization a) => d.Emoji, delegate(IJsonCommentReaction d, Emoji o)
				{
					d.Emoji = o;
				})
			},
			{
				"Id",
				new Property<IJsonCommentReaction, string>((IJsonCommentReaction d, TrelloAuthorization a) => d.Id, delegate(IJsonCommentReaction d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Member",
				new Property<IJsonCommentReaction, Member>((IJsonCommentReaction d, TrelloAuthorization a) => d.Member?.GetFromCache<Member, IJsonMember>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCommentReaction d, Member o)
				{
					d.Member = o?.Json;
				})
			}
		};
	}

	public CommentReactionContext(string id, string ownerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_ownerId = ownerId;
		base.Data.Id = id;
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CommentReaction_Read_Refresh, new Dictionary<string, object>
		{
			{ "_actionId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CommentReaction_Write_Delete, new Dictionary<string, object>
		{
			{ "_actionId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}
}
