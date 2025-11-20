using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IAttachmentCollection : IReadOnlyCollection<IAttachment>, IEnumerable<IAttachment>, IEnumerable, IRefreshable
{
	Task<IAttachment> Add(string url, string name = null, CancellationToken ct = default(CancellationToken));

	Task<IAttachment> Add(byte[] data, string name, CancellationToken ct = default(CancellationToken));
}
