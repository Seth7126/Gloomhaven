using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ISticker : ICacheable, IRefreshable
{
	double? Left { get; set; }

	string Name { get; }

	IReadOnlyCollection<IImagePreview> Previews { get; }

	int? Rotation { get; set; }

	double? Top { get; set; }

	string ImageUrl { get; }

	int? ZIndex { get; set; }

	event Action<ISticker, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
