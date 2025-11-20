using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platforms.Utils;

internal static class AsyncHelper
{
	private static readonly TaskFactory _taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

	public static TResult RunSync<TResult>(Func<Task<TResult>> func)
	{
		return _taskFactory.StartNew(func).Unwrap().GetAwaiter()
			.GetResult();
	}

	public static void RunSync(Func<Task> func)
	{
		_taskFactory.StartNew(func).Unwrap().GetAwaiter()
			.GetResult();
	}

	public static async Task WaitUntil(Func<bool> condition, int frequency = 100, int timeout = -1)
	{
		Task task = Task.Run(async delegate
		{
			while (!condition())
			{
				await Task.Delay(frequency);
			}
		});
		if (timeout >= 0)
		{
			object obj = task;
			if (obj != await Task.WhenAny(task, Task.Delay(timeout)))
			{
				throw new TimeoutException();
			}
		}
		else
		{
			await task;
		}
	}
}
