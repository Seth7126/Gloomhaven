using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICommentReactionCollection : IReadOnlyCollection<ICommentReaction>, IEnumerable<ICommentReaction>, IEnumerable, IRefreshable
{
	Task<ICommentReaction> Add(Emoji emoji, CancellationToken ct = default(CancellationToken));
}
