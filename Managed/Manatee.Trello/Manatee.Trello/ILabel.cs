using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ILabel : ICacheable, IRefreshable
{
	IBoard Board { get; }

	LabelColor? Color { get; set; }

	DateTime CreationDate { get; }

	string Name { get; set; }

	int? Uses { get; }

	Task Delete(CancellationToken ct = default(CancellationToken));
}
