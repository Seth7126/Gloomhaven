using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICheckList : ICacheable, IRefreshable
{
	IBoard Board { get; }

	ICard Card { get; }

	ICheckItemCollection CheckItems { get; }

	DateTime CreationDate { get; }

	string Name { get; set; }

	Position Position { get; set; }

	ICheckItem this[string key] { get; }

	ICheckItem this[int index] { get; }

	event Action<ICheckList, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
