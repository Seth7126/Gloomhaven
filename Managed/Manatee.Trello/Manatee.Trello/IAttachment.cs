using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IAttachment : ICacheable, IRefreshable
{
	int? Bytes { get; }

	DateTime CreationDate { get; }

	DateTime? Date { get; }

	bool? IsUpload { get; }

	IMember Member { get; }

	string MimeType { get; }

	string Name { get; set; }

	IReadOnlyCollection<IImagePreview> Previews { get; }

	string Url { get; }

	WebColor EdgeColor { get; }

	Position Position { get; set; }

	event Action<IAttachment, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
