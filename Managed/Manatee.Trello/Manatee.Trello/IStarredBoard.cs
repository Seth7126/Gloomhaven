using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IStarredBoard : ICacheable, IRefreshable
{
	IBoard Board { get; }

	Position Position { get; set; }

	event Action<IStarredBoard, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
