using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICardCollection : IReadOnlyCardCollection, IReadOnlyCollection<ICard>, IEnumerable<ICard>, IEnumerable, IRefreshable
{
	Task<ICard> Add(string name, string description = null, Position position = null, DateTime? dueDate = null, bool? isComplete = null, IEnumerable<IMember> members = null, IEnumerable<ILabel> labels = null, CancellationToken ct = default(CancellationToken));

	Task<ICard> Add(ICard source, CardCopyKeepFromSourceOptions keep = CardCopyKeepFromSourceOptions.None, CancellationToken ct = default(CancellationToken));

	Task<ICard> Add(string name, string sourceUrl, CancellationToken ct = default(CancellationToken));
}
