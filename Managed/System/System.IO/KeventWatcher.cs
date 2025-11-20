using System.Collections;
using System.Runtime.InteropServices;

namespace System.IO;

internal class KeventWatcher : IFileWatcher
{
	private static bool failed;

	private static KeventWatcher instance;

	private static Hashtable watches;

	private KeventWatcher()
	{
	}

	public static bool GetInstance(out IFileWatcher watcher)
	{
		if (failed)
		{
			watcher = null;
			return false;
		}
		if (instance != null)
		{
			watcher = instance;
			return true;
		}
		watches = Hashtable.Synchronized(new Hashtable());
		int num = kqueue();
		if (num == -1)
		{
			failed = true;
			watcher = null;
			return false;
		}
		close(num);
		instance = new KeventWatcher();
		watcher = instance;
		return true;
	}

	public void StartDispatching(object handle)
	{
		FileSystemWatcher fileSystemWatcher = handle as FileSystemWatcher;
		KqueueMonitor kqueueMonitor;
		if (watches.ContainsKey(fileSystemWatcher))
		{
			kqueueMonitor = (KqueueMonitor)watches[fileSystemWatcher];
		}
		else
		{
			kqueueMonitor = new KqueueMonitor(fileSystemWatcher);
			watches.Add(fileSystemWatcher, kqueueMonitor);
		}
		kqueueMonitor.Start();
	}

	public void StopDispatching(object handle)
	{
		FileSystemWatcher key = handle as FileSystemWatcher;
		((KqueueMonitor)watches[key])?.Stop();
	}

	public void Dispose(object handle)
	{
	}

	[DllImport("libc")]
	private static extern int close(int fd);

	[DllImport("libc")]
	private static extern int kqueue();
}
