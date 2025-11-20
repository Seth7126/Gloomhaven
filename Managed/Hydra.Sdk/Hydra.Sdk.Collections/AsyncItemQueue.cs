using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hydra.Sdk.Collections;

public class AsyncItemQueue<T>
{
	private readonly ConcurrentQueue<T> _queuedItems = new ConcurrentQueue<T>();

	private readonly ConcurrentQueue<TaskCompletionSource<T>> _pendingItems = new ConcurrentQueue<TaskCompletionSource<T>>();

	private long _ItemsCount = 0L;

	public async Task Enqueue(T item)
	{
		long count = Interlocked.Increment(ref _ItemsCount);
		if (count <= 0)
		{
			TaskCompletionSource<T> tcs;
			while (!_pendingItems.TryDequeue(out tcs))
			{
				await Task.Yield();
			}
			tcs.TrySetResult(item);
		}
		else
		{
			_queuedItems.Enqueue(item);
		}
	}

	public async Task<T> DequeueAsync()
	{
		long count = Interlocked.Decrement(ref _ItemsCount);
		if (count < 0)
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			_pendingItems.Enqueue(tcs);
			return (!tcs.Task.IsCompleted) ? (await tcs.Task.ConfigureAwait(continueOnCapturedContext: false)) : tcs.Task.Result;
		}
		T result;
		while (!_queuedItems.TryDequeue(out result))
		{
			await Task.Yield();
		}
		return result;
	}
}
