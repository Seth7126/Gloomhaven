#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;

namespace UdpKit.Async;

internal class ThreadManager : Singleton<ThreadManager>
{
	private readonly List<Task> _threads;

	public ThreadManager()
	{
		_threads = new List<Task>();
	}

	public static void Clear()
	{
		for (int i = 0; i < Singleton<ThreadManager>.Instance._threads.Count; i++)
		{
			Singleton<ThreadManager>.Instance._threads[i].Abort();
		}
		Singleton<ThreadManager>.Instance._threads.Clear();
	}

	public static string GetInfo()
	{
		return $"Total: {Singleton<ThreadManager>.Instance._threads.Count} :: Alive {Singleton<ThreadManager>.Instance._threads.Count((Task thr) => thr.IsRunning)}";
	}

	public static void Start(Action run)
	{
		FindTask().Run(run);
	}

	private static Task FindTask()
	{
		Task task;
		for (int i = 0; i < Singleton<ThreadManager>.Instance._threads.Count; i++)
		{
			task = Singleton<ThreadManager>.Instance._threads[i];
			if (!task.IsRunning)
			{
				UdpLog.Debug("Reusing UdpKit Task with ID: {0}", task.Name);
				return task;
			}
		}
		task = new Task();
		Singleton<ThreadManager>.Instance._threads.Add(task);
		return task;
	}
}
