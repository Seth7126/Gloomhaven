using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CommentReactionCollection : ReadOnlyCommentReactionCollection, ICommentReactionCollection, IReadOnlyCollection<ICommentReaction>, IEnumerable<ICommentReaction>, IEnumerable, IRefreshable
{
	internal CommentReactionCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(typeof(List), getOwnerId, auth)
	{
	}

	public async Task<ICommentReaction> Add(Emoji emoji, CancellationToken ct = default(CancellationToken))
	{
		IJsonCommentReaction jsonCommentReaction = TrelloConfiguration.JsonFactory.Create<IJsonCommentReaction>();
		jsonCommentReaction.Emoji = emoji;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Action_Write_AddReaction, new Dictionary<string, object> { ["_id"] = base.OwnerId });
		return new CommentReaction(await JsonRepository.Execute(base.Auth, endpoint, jsonCommentReaction, ct), base.OwnerId, base.Auth);
	}
}
