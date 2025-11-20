using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICheckItem : ICacheable, IRefreshable
{
	ICheckList CheckList { get; set; }

	DateTime CreationDate { get; }

	string Name { get; set; }

	Position Position { get; set; }

	CheckItemState? State { get; set; }

	event Action<ICheckItem, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
