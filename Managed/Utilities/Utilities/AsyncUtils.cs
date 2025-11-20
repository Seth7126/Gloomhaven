using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities;

public class AsyncUtils
{
	public static async Task WaitUntil(Func<bool> condition, CancellationToken cancellationToken, int frequency = 100, int timeout = -1)
	{
		Task task = Task.Run(async delegate
		{
			while (!condition())
			{
				await Task.Delay(frequency, cancellationToken);
			}
		});
		object obj = task;
		if (obj != await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)))
		{
			throw new TimeoutException();
		}
	}
}
