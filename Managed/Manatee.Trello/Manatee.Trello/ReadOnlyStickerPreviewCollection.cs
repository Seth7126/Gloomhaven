using System;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyStickerPreviewCollection : ReadOnlyCollection<IImagePreview>
{
	private readonly StickerContext _context;

	internal ReadOnlyStickerPreviewCollection(StickerContext context, TrelloAuthorization auth)
		: base((Func<string>)(() => context.Data.Id), auth)
	{
		_context = context;
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		await _context.Synchronize(force, ct);
		if (_context.Data.Previews == null)
		{
			return;
		}
		base.Items.Clear();
		foreach (IJsonImagePreview preview in _context.Data.Previews)
		{
			ImagePreview fromCache = preview.GetFromCache<ImagePreview>(base.Auth);
			base.Items.Add(fromCache);
		}
	}
}
