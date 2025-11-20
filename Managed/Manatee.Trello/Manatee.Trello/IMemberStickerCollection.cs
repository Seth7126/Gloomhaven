using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IMemberStickerCollection
{
	Task<ISticker> Add(byte[] data, string name, CancellationToken ct = default(CancellationToken));
}
