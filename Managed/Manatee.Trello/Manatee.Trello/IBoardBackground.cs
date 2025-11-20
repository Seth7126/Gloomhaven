using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IBoardBackground : ICacheable
{
	WebColor Color { get; }

	string Image { get; }

	bool? IsTiled { get; }

	IReadOnlyCollection<IImagePreview> ScaledImages { get; }

	WebColor BottomColor { get; }

	BoardBackgroundBrightness? Brightness { get; }

	WebColor TopColor { get; }

	BoardBackgroundType? Type { get; }

	Task Delete(CancellationToken ct = default(CancellationToken));
}
