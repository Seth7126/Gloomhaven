using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICardStickerCollection : IReadOnlyCollection<ISticker>, IEnumerable<ISticker>, IEnumerable, IRefreshable
{
	Task<ISticker> Add(string name, double left, double top, int zIndex = 0, int rotation = 0, CancellationToken ct = default(CancellationToken));

	Task Remove(Sticker sticker, CancellationToken ct = default(CancellationToken));
}
