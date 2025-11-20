namespace System.Threading.Tasks;

/// <summary>Provides a set of static (Shared in Visual Basic) methods for working with specific kinds of <see cref="T:System.Threading.Tasks.Task" /> instances.</summary>
public static class TaskExtensions
{
	/// <summary>Creates a proxy <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation of a <see cref="M:System.Threading.Tasks.TaskScheduler.TryExecuteTaskInline(System.Threading.Tasks.Task,System.Boolean)" />.</summary>
	/// <returns>A Task that represents the asynchronous operation of the provided System.Threading.Tasks.Task(Of Task).</returns>
	/// <param name="task">The Task&lt;Task&gt; (C#) or Task (Of Task) (Visual Basic) to unwrap.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown if the <paramref name="task" /> argument is null.</exception>
	public static Task Unwrap(this Task<Task> task)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		object task2;
		if (task.IsCompletedSuccessfully)
		{
			task2 = task.Result;
			if (task2 == null)
			{
				return Task.FromCanceled(new CancellationToken(canceled: true));
			}
		}
		else
		{
			task2 = Task.CreateUnwrapPromise<VoidTaskResult>(task, lookForOce: false);
		}
		return (Task)task2;
	}

	/// <summary>Creates a proxy <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation of a Task&lt;Task&lt;T&gt;&gt; (C#) or Task (Of Task(Of T)) (Visual Basic).</summary>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation of the provided Task&lt;Task&lt;T&gt;&gt; (C#) or Task (Of Task(Of T)) (Visual Basic).</returns>
	/// <param name="task">The Task&lt;Task&lt;T&gt;&gt; (C#) or Task (Of Task(Of T)) (Visual Basic) to unwrap.</param>
	/// <typeparam name="TResult">The type of the task's result.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown if the <paramref name="task" /> argument is null.</exception>
	public static Task<TResult> Unwrap<TResult>(this Task<Task<TResult>> task)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		Task<TResult> task2;
		if (task.IsCompletedSuccessfully)
		{
			task2 = task.Result;
			if (task2 == null)
			{
				return Task.FromCanceled<TResult>(new CancellationToken(canceled: true));
			}
		}
		else
		{
			task2 = Task.CreateUnwrapPromise<TResult>(task, lookForOce: false);
		}
		return task2;
	}
}
