using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public abstract class ReadOnlyCollection<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IRefreshable
{
	private string _ownerId;

	private readonly Func<string> _getOwnerId;

	public virtual int? Limit { get; set; }

	public T this[int index] => GetByIndex(index);

	internal string OwnerId => _ownerId ?? (_ownerId = _getOwnerId());

	internal List<T> Items { get; }

	internal TrelloAuthorization Auth { get; }

	internal Dictionary<string, object> AdditionalParameters { get; }

	internal event EventHandler Refreshed;

	protected ReadOnlyCollection(Func<string> getOwnerId, TrelloAuthorization auth)
	{
		_getOwnerId = getOwnerId;
		Auth = auth ?? TrelloAuthorization.Default;
		Items = new List<T>();
		AdditionalParameters = new Dictionary<string, object>();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return Items.GetEnumerator();
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		if (Auth == TrelloAuthorization.Null)
		{
			return Task.CompletedTask;
		}
		this.Refreshed?.Invoke(this, new EventArgs());
		return PerformRefresh(force, ct);
	}

	internal abstract Task PerformRefresh(bool force, CancellationToken ct);

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected void IncorporateLimit()
	{
		if (Limit.HasValue)
		{
			AdditionalParameters["limit"] = Limit.Value;
		}
	}

	internal void Update(IEnumerable<T> items)
	{
		Items.Clear();
		Items.AddRange(items);
	}

	private T GetByIndex(int index)
	{
		return this.ElementAt(index);
	}
}
