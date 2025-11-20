using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IWebhook<T> : ICacheable, IRefreshable where T : class, ICanWebhook
{
	string CallBackUrl { get; set; }

	DateTime CreationDate { get; }

	string Description { get; set; }

	bool? IsActive { get; set; }

	T Target { get; set; }

	event Action<IWebhook<T>, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
