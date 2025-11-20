using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICommentReaction : ICacheable
{
	Action Comment { get; }

	Emoji Emoji { get; }

	Member Member { get; }

	Task Delete(CancellationToken ct = default(CancellationToken));
}
