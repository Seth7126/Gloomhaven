using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Listens to the file system change notifications and raises events when a directory, or file in a directory, changes.</summary>
/// <filterpriority>2</filterpriority>
[IODescription("")]
[DefaultEvent("Changed")]
public class FileSystemWatcher : Component, ISupportInitialize
{
	private enum EventType
	{
		FileSystemEvent,
		ErrorEvent,
		RenameEvent
	}

	private bool inited;

	private bool start_requested;

	private bool enableRaisingEvents;

	private string filter;

	private bool includeSubdirectories;

	private int internalBufferSize;

	private NotifyFilters notifyFilter;

	private string path;

	private string fullpath;

	private ISynchronizeInvoke synchronizingObject;

	private WaitForChangedResult lastData;

	private bool waiting;

	private SearchPattern2 pattern;

	private bool disposed;

	private string mangledFilter;

	private IFileWatcher watcher;

	private object watcher_handle;

	private static object lockobj = new object();

	internal bool Waiting
	{
		get
		{
			return waiting;
		}
		set
		{
			waiting = value;
		}
	}

	internal string MangledFilter
	{
		get
		{
			if (filter != "*.*")
			{
				return filter;
			}
			if (mangledFilter != null)
			{
				return mangledFilter;
			}
			return "*.*";
		}
	}

	internal SearchPattern2 Pattern
	{
		get
		{
			if (pattern == null)
			{
				if (watcher?.GetType() == typeof(KeventWatcher))
				{
					pattern = new SearchPattern2(MangledFilter, ignore: true);
				}
				else
				{
					pattern = new SearchPattern2(MangledFilter);
				}
			}
			return pattern;
		}
	}

	internal string FullPath
	{
		get
		{
			if (fullpath == null)
			{
				if (path == null || path == "")
				{
					fullpath = Environment.CurrentDirectory;
				}
				else
				{
					fullpath = System.IO.Path.GetFullPath(path);
				}
			}
			return fullpath;
		}
	}

	/// <summary>Gets or sets a value indicating whether the component is enabled.</summary>
	/// <returns>true if the component is enabled; otherwise, false. The default is false. If you are using the component on a designer in Visual Studio 2005, the default is true.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.FileSystemWatcher" /> object has been disposed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Microsoft Windows NT or later.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The directory specified in <see cref="P:System.IO.FileSystemWatcher.Path" /> could not be found.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.IO.FileSystemWatcher.Path" /> has not been set or is invalid.</exception>
	/// <filterpriority>2</filterpriority>
	[DefaultValue(false)]
	[IODescription("Flag to indicate if this instance is active")]
	public bool EnableRaisingEvents
	{
		get
		{
			return enableRaisingEvents;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			start_requested = true;
			if (inited && value != enableRaisingEvents)
			{
				enableRaisingEvents = value;
				if (value)
				{
					Start();
					return;
				}
				Stop();
				start_requested = false;
			}
		}
	}

	/// <summary>Gets or sets the filter string used to determine what files are monitored in a directory.</summary>
	/// <returns>The filter string. The default is "*.*" (Watches all files.) </returns>
	/// <filterpriority>2</filterpriority>
	[SettingsBindable(true)]
	[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[IODescription("File name filter pattern")]
	[DefaultValue("*.*")]
	public string Filter
	{
		get
		{
			return filter;
		}
		set
		{
			if (value == null || value == "")
			{
				value = "*";
			}
			if (!string.Equals(filter, value, PathInternal.StringComparison))
			{
				filter = ((value == "*.*") ? "*" : value);
				pattern = null;
				mangledFilter = null;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether subdirectories within the specified path should be monitored.</summary>
	/// <returns>true if you want to monitor subdirectories; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[IODescription("Flag to indicate we want to watch subdirectories")]
	[DefaultValue(false)]
	public bool IncludeSubdirectories
	{
		get
		{
			return includeSubdirectories;
		}
		set
		{
			if (includeSubdirectories != value)
			{
				includeSubdirectories = value;
				if (value && enableRaisingEvents)
				{
					Stop();
					Start();
				}
			}
		}
	}

	/// <summary>Gets or sets the size (in bytes) of the internal buffer.</summary>
	/// <returns>The internal buffer size in bytes. The default is 8192 (8 KB).</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	[DefaultValue(8192)]
	public int InternalBufferSize
	{
		get
		{
			return internalBufferSize;
		}
		set
		{
			if (internalBufferSize != value)
			{
				if (value < 4096)
				{
					value = 4096;
				}
				internalBufferSize = value;
				if (enableRaisingEvents)
				{
					Stop();
					Start();
				}
			}
		}
	}

	/// <summary>Gets or sets the type of changes to watch for.</summary>
	/// <returns>One of the <see cref="T:System.IO.NotifyFilters" /> values. The default is the bitwise OR combination of LastWrite, FileName, and DirectoryName.</returns>
	/// <exception cref="T:System.ArgumentException">The value is not a valid bitwise OR combination of the <see cref="T:System.IO.NotifyFilters" /> values. </exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The value that is being set is not valid.</exception>
	/// <filterpriority>2</filterpriority>
	[DefaultValue(NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite)]
	[IODescription("Flag to indicate which change event we want to monitor")]
	public NotifyFilters NotifyFilter
	{
		get
		{
			return notifyFilter;
		}
		set
		{
			if (notifyFilter != value)
			{
				notifyFilter = value;
				if (enableRaisingEvents)
				{
					Stop();
					Start();
				}
			}
		}
	}

	/// <summary>Gets or sets the path of the directory to watch.</summary>
	/// <returns>The path to monitor. The default is an empty string ("").</returns>
	/// <exception cref="T:System.ArgumentException">The specified path does not exist or could not be found.-or- The specified path contains wildcard characters.-or- The specified path contains invalid path characters.</exception>
	/// <filterpriority>2</filterpriority>
	[DefaultValue("")]
	[IODescription("The directory to monitor")]
	[SettingsBindable(true)]
	[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[Editor("System.Diagnostics.Design.FSWPathEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public string Path
	{
		get
		{
			return path;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			value = ((value == null) ? string.Empty : value);
			if (!string.Equals(path, value, PathInternal.StringComparison))
			{
				bool flag = false;
				Exception ex = null;
				try
				{
					flag = Directory.Exists(value);
				}
				catch (Exception ex2)
				{
					ex = ex2;
				}
				if (ex != null)
				{
					throw new ArgumentException(global::SR.Format("The directory name {0} is invalid.", value), "Path");
				}
				if (!flag)
				{
					throw new ArgumentException(global::SR.Format("The directory name '{0}' does not exist.", value), "Path");
				}
				path = value;
				fullpath = null;
				if (enableRaisingEvents)
				{
					Stop();
					Start();
				}
			}
		}
	}

	/// <summary>Gets or sets an <see cref="T:System.ComponentModel.ISite" /> for the <see cref="T:System.IO.FileSystemWatcher" />.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.ISite" /> for the <see cref="T:System.IO.FileSystemWatcher" />.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public override ISite Site
	{
		get
		{
			return base.Site;
		}
		set
		{
			base.Site = value;
			if (Site != null && Site.DesignMode)
			{
				EnableRaisingEvents = true;
			}
		}
	}

	/// <summary>Gets or sets the object used to marshal the event handler calls issued as a result of a directory change.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.ISynchronizeInvoke" /> that represents the object used to marshal the event handler calls issued as a result of a directory change. The default is null.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	[DefaultValue(null)]
	[IODescription("The object used to marshal the event handler calls resulting from a directory change")]
	public ISynchronizeInvoke SynchronizingObject
	{
		get
		{
			return synchronizingObject;
		}
		set
		{
			synchronizingObject = value;
		}
	}

	/// <summary>Occurs when a file or directory in the specified <see cref="P:System.IO.FileSystemWatcher.Path" /> is changed.</summary>
	/// <filterpriority>2</filterpriority>
	[IODescription("Occurs when a file/directory change matches the filter")]
	public event FileSystemEventHandler Changed;

	/// <summary>Occurs when a file or directory in the specified <see cref="P:System.IO.FileSystemWatcher.Path" /> is created.</summary>
	/// <filterpriority>2</filterpriority>
	[IODescription("Occurs when a file/directory creation matches the filter")]
	public event FileSystemEventHandler Created;

	/// <summary>Occurs when a file or directory in the specified <see cref="P:System.IO.FileSystemWatcher.Path" /> is deleted.</summary>
	/// <filterpriority>2</filterpriority>
	[IODescription("Occurs when a file/directory deletion matches the filter")]
	public event FileSystemEventHandler Deleted;

	/// <summary>Occurs when the instance of <see cref="T:System.IO.FileSystemWatcher" /> is unable to continue monitoring changes or when the internal buffer overflows.</summary>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public event ErrorEventHandler Error;

	/// <summary>Occurs when a file or directory in the specified <see cref="P:System.IO.FileSystemWatcher.Path" /> is renamed.</summary>
	/// <filterpriority>2</filterpriority>
	[IODescription("Occurs when a file/directory rename matches the filter")]
	public event RenamedEventHandler Renamed;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileSystemWatcher" /> class.</summary>
	public FileSystemWatcher()
	{
		notifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
		enableRaisingEvents = false;
		filter = "*";
		includeSubdirectories = false;
		internalBufferSize = 8192;
		path = "";
		InitWatcher();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileSystemWatcher" /> class, given the specified directory to monitor.</summary>
	/// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter is an empty string ("").-or- The path specified through the <paramref name="path" /> parameter does not exist. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> is too long.</exception>
	public FileSystemWatcher(string path)
		: this(path, "*")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileSystemWatcher" /> class, given the specified directory and type of files to monitor.</summary>
	/// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation. </param>
	/// <param name="filter">The type of files to watch. For example, "*.txt" watches for changes to all text files. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null.-or- The <paramref name="filter" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter is an empty string ("").-or- The path specified through the <paramref name="path" /> parameter does not exist. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> is too long.</exception>
	public FileSystemWatcher(string path, string filter)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		if (path == string.Empty)
		{
			throw new ArgumentException("Empty path", "path");
		}
		if (!Directory.Exists(path))
		{
			throw new ArgumentException("Directory does not exist", "path");
		}
		inited = false;
		start_requested = false;
		enableRaisingEvents = false;
		this.filter = filter;
		if (this.filter == "*.*")
		{
			this.filter = "*";
		}
		includeSubdirectories = false;
		internalBufferSize = 8192;
		notifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
		this.path = path;
		synchronizingObject = null;
		InitWatcher();
	}

	[EnvironmentPermission(SecurityAction.Assert, Read = "MONO_MANAGED_WATCHER")]
	private void InitWatcher()
	{
		lock (lockobj)
		{
			if (watcher_handle != null)
			{
				return;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("MONO_MANAGED_WATCHER");
			int num = 0;
			bool flag = false;
			if (environmentVariable == null)
			{
				num = InternalSupportsFSW();
			}
			switch (num)
			{
			case 1:
				flag = DefaultWatcher.GetInstance(out watcher);
				watcher_handle = this;
				break;
			case 2:
				flag = FAMWatcher.GetInstance(out watcher, gamin: false);
				watcher_handle = this;
				break;
			case 3:
				flag = KeventWatcher.GetInstance(out watcher);
				watcher_handle = this;
				break;
			case 4:
				flag = FAMWatcher.GetInstance(out watcher, gamin: true);
				watcher_handle = this;
				break;
			case 6:
				flag = CoreFXFileSystemWatcherProxy.GetInstance(out watcher);
				watcher_handle = (watcher as CoreFXFileSystemWatcherProxy).NewWatcher(this);
				break;
			}
			if (num == 0 || !flag)
			{
				if (string.Compare(environmentVariable, "disabled", ignoreCase: true) == 0)
				{
					NullFileWatcher.GetInstance(out watcher);
				}
				else
				{
					DefaultWatcher.GetInstance(out watcher);
					watcher_handle = this;
				}
			}
			inited = true;
		}
	}

	[Conditional("TRACE")]
	[Conditional("DEBUG")]
	private void ShowWatcherInfo()
	{
		Console.WriteLine("Watcher implementation: {0}", (watcher != null) ? watcher.GetType().ToString() : "<none>");
	}

	/// <summary>Begins the initialization of a <see cref="T:System.IO.FileSystemWatcher" /> used on a form or used by another component. The initialization occurs at run time.</summary>
	/// <filterpriority>2</filterpriority>
	public void BeginInit()
	{
		inited = false;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.FileSystemWatcher" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			try
			{
				watcher?.StopDispatching(watcher_handle);
				watcher?.Dispose(watcher_handle);
			}
			catch (Exception)
			{
			}
			watcher_handle = null;
			watcher = null;
			disposed = true;
			base.Dispose(disposing);
			GC.SuppressFinalize(this);
		}
	}

	~FileSystemWatcher()
	{
		if (!disposed)
		{
			Dispose(disposing: false);
		}
	}

	/// <summary>Ends the initialization of a <see cref="T:System.IO.FileSystemWatcher" /> used on a form or used by another component. The initialization occurs at run time.</summary>
	/// <filterpriority>2</filterpriority>
	public void EndInit()
	{
		inited = true;
		if (start_requested)
		{
			EnableRaisingEvents = true;
		}
	}

	private void RaiseEvent(Delegate ev, EventArgs arg, EventType evtype)
	{
		if (disposed || (object)ev == null)
		{
			return;
		}
		if (synchronizingObject == null)
		{
			Delegate[] invocationList = ev.GetInvocationList();
			foreach (Delegate obj in invocationList)
			{
				switch (evtype)
				{
				case EventType.RenameEvent:
					((RenamedEventHandler)obj)(this, (RenamedEventArgs)arg);
					break;
				case EventType.ErrorEvent:
					((ErrorEventHandler)obj)(this, (ErrorEventArgs)arg);
					break;
				case EventType.FileSystemEvent:
					((FileSystemEventHandler)obj)(this, (FileSystemEventArgs)arg);
					break;
				}
			}
		}
		else
		{
			synchronizingObject.BeginInvoke(ev, new object[2] { this, arg });
		}
	}

	/// <summary>Raises the <see cref="E:System.IO.FileSystemWatcher.Changed" /> event.</summary>
	/// <param name="e">A <see cref="T:System.IO.FileSystemEventArgs" /> that contains the event data. </param>
	protected void OnChanged(FileSystemEventArgs e)
	{
		RaiseEvent(this.Changed, e, EventType.FileSystemEvent);
	}

	/// <summary>Raises the <see cref="E:System.IO.FileSystemWatcher.Created" /> event.</summary>
	/// <param name="e">A <see cref="T:System.IO.FileSystemEventArgs" /> that contains the event data. </param>
	protected void OnCreated(FileSystemEventArgs e)
	{
		RaiseEvent(this.Created, e, EventType.FileSystemEvent);
	}

	/// <summary>Raises the <see cref="E:System.IO.FileSystemWatcher.Deleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.IO.FileSystemEventArgs" /> that contains the event data. </param>
	protected void OnDeleted(FileSystemEventArgs e)
	{
		RaiseEvent(this.Deleted, e, EventType.FileSystemEvent);
	}

	/// <summary>Raises the <see cref="E:System.IO.FileSystemWatcher.Error" /> event.</summary>
	/// <param name="e">An <see cref="T:System.IO.ErrorEventArgs" /> that contains the event data. </param>
	protected void OnError(ErrorEventArgs e)
	{
		RaiseEvent(this.Error, e, EventType.ErrorEvent);
	}

	/// <summary>Raises the <see cref="E:System.IO.FileSystemWatcher.Renamed" /> event.</summary>
	/// <param name="e">A <see cref="T:System.IO.RenamedEventArgs" /> that contains the event data. </param>
	protected void OnRenamed(RenamedEventArgs e)
	{
		RaiseEvent(this.Renamed, e, EventType.RenameEvent);
	}

	/// <summary>A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor.</summary>
	/// <returns>A <see cref="T:System.IO.WaitForChangedResult" /> that contains specific information on the change that occurred.</returns>
	/// <param name="changeType">The <see cref="T:System.IO.WatcherChangeTypes" /> to watch for. </param>
	/// <filterpriority>2</filterpriority>
	public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
	{
		return WaitForChanged(changeType, -1);
	}

	/// <summary>A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor and the time (in milliseconds) to wait before timing out.</summary>
	/// <returns>A <see cref="T:System.IO.WaitForChangedResult" /> that contains specific information on the change that occurred.</returns>
	/// <param name="changeType">The <see cref="T:System.IO.WatcherChangeTypes" /> to watch for. </param>
	/// <param name="timeout">The time (in milliseconds) to wait before timing out. </param>
	/// <filterpriority>2</filterpriority>
	public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
	{
		WaitForChangedResult result = default(WaitForChangedResult);
		bool flag = EnableRaisingEvents;
		if (!flag)
		{
			EnableRaisingEvents = true;
		}
		bool flag2;
		lock (this)
		{
			waiting = true;
			flag2 = Monitor.Wait(this, timeout);
			if (flag2)
			{
				result = lastData;
			}
		}
		EnableRaisingEvents = flag;
		if (!flag2)
		{
			result.TimedOut = true;
		}
		return result;
	}

	internal void DispatchErrorEvents(ErrorEventArgs args)
	{
		if (!disposed)
		{
			OnError(args);
		}
	}

	internal void DispatchEvents(FileAction act, string filename, ref RenamedEventArgs renamed)
	{
		if (disposed)
		{
			return;
		}
		if (waiting)
		{
			lastData = default(WaitForChangedResult);
		}
		switch (act)
		{
		case FileAction.Added:
			lastData.Name = filename;
			lastData.ChangeType = WatcherChangeTypes.Created;
			Task.Run(delegate
			{
				OnCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, path, filename));
			});
			break;
		case FileAction.Removed:
			lastData.Name = filename;
			lastData.ChangeType = WatcherChangeTypes.Deleted;
			Task.Run(delegate
			{
				OnDeleted(new FileSystemEventArgs(WatcherChangeTypes.Deleted, path, filename));
			});
			break;
		case FileAction.Modified:
			lastData.Name = filename;
			lastData.ChangeType = WatcherChangeTypes.Changed;
			Task.Run(delegate
			{
				OnChanged(new FileSystemEventArgs(WatcherChangeTypes.Changed, path, filename));
			});
			break;
		case FileAction.RenamedOldName:
			if (renamed != null)
			{
				OnRenamed(renamed);
			}
			lastData.OldName = filename;
			lastData.ChangeType = WatcherChangeTypes.Renamed;
			renamed = new RenamedEventArgs(WatcherChangeTypes.Renamed, path, filename, "");
			break;
		case FileAction.RenamedNewName:
		{
			lastData.Name = filename;
			lastData.ChangeType = WatcherChangeTypes.Renamed;
			if (renamed == null)
			{
				renamed = new RenamedEventArgs(WatcherChangeTypes.Renamed, path, "", filename);
			}
			RenamedEventArgs renamed_ref = renamed;
			Task.Run(delegate
			{
				OnRenamed(renamed_ref);
			});
			renamed = null;
			break;
		}
		}
	}

	private void Start()
	{
		if (!disposed && watcher_handle != null)
		{
			watcher?.StartDispatching(watcher_handle);
		}
	}

	private void Stop()
	{
		if (!disposed && watcher_handle != null)
		{
			watcher?.StopDispatching(watcher_handle);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int InternalSupportsFSW();
}
