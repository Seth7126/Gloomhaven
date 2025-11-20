using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CommentCollection : ReadOnlyActionCollection, ICommentCollection, IReadOnlyActionCollection, IReadOnlyCollection<IAction>, IEnumerable<IAction>, IEnumerable, IRefreshable
{
	internal CommentCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(typeof(Card), getOwnerId, auth)
	{
		Filter(ActionType.CommentCard | ActionType.CopyCommentCard);
	}

	public async Task<IAction> Add(string text, CancellationToken ct = default(CancellationToken))
	{
		string text2 = NotNullOrWhiteSpaceRule.Instance.Validate(null, text);
		if (text2 != null)
		{
			throw new ValidationException<string>(text, new string[1] { text2 });
		}
		IJsonAction jsonAction = TrelloConfiguration.JsonFactory.Create<IJsonAction>();
		jsonAction.Text = text;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_AddComment, new Dictionary<string, object> { { "_id", base.OwnerId } });
		return new Action(await JsonRepository.Execute(base.Auth, endpoint, jsonAction, ct), base.Auth);
	}
}
