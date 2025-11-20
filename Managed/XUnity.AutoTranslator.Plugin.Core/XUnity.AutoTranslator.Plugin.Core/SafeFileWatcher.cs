using System;
using System.IO;
using System.Threading;

namespace XUnity.AutoTranslator.Plugin.Core;

internal sealed class SafeFileWatcher : IDisposable
{
	private int _counter;

	private FileSystemWatcher _watcher;

	private bool _disposed;

	private object _sync = new object();

	private Timer _timer;

	private readonly string _directory;

	public event Action DirectoryUpdated;

	public SafeFileWatcher(string directory)
	{
		_directory = directory;
		_timer = new Timer(RaiseEvent, null, -1, -1);
		EnableWatcher();
	}

	public void EnableWatcher()
	{
		if (_watcher == null)
		{
			_watcher = new FileSystemWatcher(_directory);
			_watcher.Changed += Watcher_Changed;
			_watcher.Created += Watcher_Created;
			_watcher.Deleted += Watcher_Deleted;
			_watcher.EnableRaisingEvents = true;
		}
	}

	public void DisableWatcher()
	{
		if (_watcher != null)
		{
			_watcher.EnableRaisingEvents = false;
			_watcher.Dispose();
			_watcher = null;
		}
	}

	public void RaiseEvent(object state)
	{
		this.DirectoryUpdated?.Invoke();
	}

	public void Disable()
	{
		int num = Interlocked.Increment(ref _counter);
		UpdateRaisingEvents(num == 0);
	}

	public void Enable()
	{
		int num = Interlocked.Decrement(ref _counter);
		UpdateRaisingEvents(num == 0);
	}

	private void Watcher_Deleted(object sender, FileSystemEventArgs e)
	{
		_timer.Change(1000, -1);
	}

	private void Watcher_Created(object sender, FileSystemEventArgs e)
	{
		FileInfo file = new FileInfo(e.FullPath);
		WaitForFile(file);
		_timer.Change(1000, -1);
	}

	private void Watcher_Changed(object sender, FileSystemEventArgs e)
	{
		_timer.Change(1000, -1);
	}

	private void UpdateRaisingEvents(bool enabled)
	{
		lock (_sync)
		{
			if (enabled)
			{
				EnableWatcher();
			}
			else
			{
				DisableWatcher();
			}
		}
	}

	private void WaitForFile(FileInfo file)
	{
		while (IsFileLocked(file))
		{
			Thread.Sleep(100);
		}
	}

	private bool IsFileLocked(FileInfo file)
	{
		try
		{
			using (file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			{
			}
		}
		catch (IOException)
		{
			return true;
		}
		return false;
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_watcher?.Dispose();
				_watcher = null;
				_timer.Dispose();
			}
			_disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
