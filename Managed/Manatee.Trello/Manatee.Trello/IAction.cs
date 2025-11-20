using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IAction : ICacheable, IRefreshable
{
	DateTime CreationDate { get; }

	IMember Creator { get; }

	IActionData Data { get; }

	DateTime? Date { get; }

	ICommentReactionCollection Reactions { get; }

	ActionType? Type { get; }

	event Action<IAction, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
