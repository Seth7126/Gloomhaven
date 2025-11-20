using System;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyBoardBackgroundScalesCollection : ReadOnlyCollection<IImagePreview>
{
	private readonly BoardBackgroundContext _context;

	internal ReadOnlyBoardBackgroundScalesCollection(BoardBackgroundContext context, TrelloAuthorization auth)
		: base((Func<string>)(() => context.Data.Id), auth)
	{
		_context = context;
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		await _context.Synchronize(force, ct);
		if (_context.Data.ImageScaled == null)
		{
			return;
		}
		base.Items.Clear();
		foreach (IJsonImagePreview item in _context.Data.ImageScaled)
		{
			ImagePreview fromCache = item.GetFromCache<ImagePreview>(base.Auth);
			base.Items.Add(fromCache);
		}
	}
}
