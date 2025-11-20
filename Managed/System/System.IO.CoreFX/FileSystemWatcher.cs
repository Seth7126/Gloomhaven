using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Enumeration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.CoreFX;

public class FileSystemWatcher : Component, ISupportInitialize
{
	private sealed class AsyncReadState
	{
		internal int Session { get; private set; }

		internal byte[] Buffer { get; private set; }

		internal SafeFileHandle DirectoryHandle { get; private set; }

		internal ThreadPoolBoundHandle ThreadPoolBinding { get; private set; }

		internal PreAllocatedOverlapped PreAllocatedOverlapped { get; set; }

		internal AsyncReadState(int session, byte[] buffer, SafeFileHandle handle, ThreadPoolBoundHandle binding)
		{
			Session = session;
			Buffer = buffer;
			DirectoryHandle = handle;
			ThreadPoolBinding = binding;
		}
	}

	private sealed class NormalizedFilterCollection : Collection<string>
	{
		private sealed class ImmutableStringList : IList<string>, ICollection<string>, IEnumerable<string>, IEnumerable
		{
			public string[] Items = Array.Empty<string>();

			public string this[int index]
			{
				get
				{
					string[] items = Items;
					if ((uint)index >= (uint)items.Length)
					{
						throw new ArgumentOutOfRangeException("index");
					}
					return items[index];
				}
				set
				{
					string[] array = (string[])Items.Clone();
					array[index] = value;
					Items = array;
				}
			}

			public int Count => Items.Length;

			public bool IsReadOnly => false;

			public void Add(string item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				Items = Array.Empty<string>();
			}

			public bool Contains(string item)
			{
				return Array.IndexOf(Items, item) != -1;
			}

			public void CopyTo(string[] array, int arrayIndex)
			{
				Items.CopyTo(array, arrayIndex);
			}

			public IEnumerator<string> GetEnumerator()
			{
				return ((IEnumerable<string>)Items).GetEnumerator();
			}

			public int IndexOf(string item)
			{
				return Array.IndexOf(Items, item);
			}

			public void Insert(int index, string item)
			{
				string[] items = Items;
				string[] array = new string[items.Length + 1];
				items.AsSpan(0, index).CopyTo(array);
				items.AsSpan(index).CopyTo(array.AsSpan(index + 1));
				array[index] = item;
				Items = array;
			}

			public bool Remove(string item)
			{
				throw new NotSupportedException();
			}

			public void RemoveAt(int index)
			{
				string[] items = Items;
				string[] array = new string[items.Length - 1];
				items.AsSpan(0, index).CopyTo(array);
				items.AsSpan(index + 1).CopyTo(array.AsSpan(index));
				Items = array;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		internal NormalizedFilterCollection()
			: base((IList<string>)new ImmutableStringList())
		{
		}

		protected override void InsertItem(int index, string item)
		{
			base.InsertItem(index, (string.IsNullOrEmpty(item) || item == "*.*") ? "*" : item);
		}

		protected override void SetItem(int index, string item)
		{
			base.SetItem(index, (string.IsNullOrEmpty(item) || item == "*.*") ? "*" : item);
		}

		internal string[] GetFilters()
		{
			return ((ImmutableStringList)base.Items).Items;
		}
	}

	private int _currentSession;

	private SafeFileHandle _directoryHandle;

	private readonly NormalizedFilterCollection _filters = new NormalizedFilterCollection();

	private string _directory;

	private const NotifyFilters c_defaultNotifyFilters = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;

	private NotifyFilters _notifyFilters = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;

	private bool _includeSubdirectories;

	private bool _enabled;

	private bool _initializing;

	private uint _internalBufferSize = 8192u;

	private bool _disposed;

	private FileSystemEventHandler _onChangedHandler;

	private FileSystemEventHandler _onCreatedHandler;

	private FileSystemEventHandler _onDeletedHandler;

	private RenamedEventHandler _onRenamedHandler;

	private ErrorEventHandler _onErrorHandler;

	private static readonly char[] s_wildcards = new char[2] { '?', '*' };

	private const int c_notifyFiltersValidMask = 383;

	public NotifyFilters NotifyFilter
	{
		get
		{
			return _notifyFilters;
		}
		set
		{
			if ((value & ~(NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size)) != 0)
			{
				throw new ArgumentException(global::SR.Format("The value of argument '{0}' ({1}) is invalid for Enum type '{2}'.", "value", (int)value, "NotifyFilters"));
			}
			if (_notifyFilters != value)
			{
				_notifyFilters = value;
				Restart();
			}
		}
	}

	public Collection<string> Filters => _filters;

	public bool EnableRaisingEvents
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (_enabled != value)
			{
				if (IsSuspended())
				{
					_enabled = value;
				}
				else if (value)
				{
					StartRaisingEventsIfNotDisposed();
				}
				else
				{
					StopRaisingEvents();
				}
			}
		}
	}

	public string Filter
	{
		get
		{
			if (Filters.Count != 0)
			{
				return Filters[0];
			}
			return "*";
		}
		set
		{
			Filters.Clear();
			Filters.Add(value);
		}
	}

	public bool IncludeSubdirectories
	{
		get
		{
			return _includeSubdirectories;
		}
		set
		{
			if (_includeSubdirectories != value)
			{
				_includeSubdirectories = value;
				Restart();
			}
		}
	}

	public int InternalBufferSize
	{
		get
		{
			return (int)_internalBufferSize;
		}
		set
		{
			if (_internalBufferSize != value)
			{
				if (value < 4096)
				{
					_internalBufferSize = 4096u;
				}
				else
				{
					_internalBufferSize = (uint)value;
				}
				Restart();
			}
		}
	}

	public string Path
	{
		get
		{
			return _directory;
		}
		set
		{
			value = ((value == null) ? string.Empty : value);
			if (!string.Equals(_directory, value, PathInternal.StringComparison))
			{
				if (value.Length == 0)
				{
					throw new ArgumentException(global::SR.Format("The directory name {0} is invalid.", value), "Path");
				}
				if (!Directory.Exists(value))
				{
					throw new ArgumentException(global::SR.Format("The directory name '{0}' does not exist.", value), "Path");
				}
				_directory = value;
				Restart();
			}
		}
	}

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

	public ISynchronizeInvoke SynchronizingObject { get; set; }

	public event FileSystemEventHandler Changed
	{
		add
		{
			_onChangedHandler = (FileSystemEventHandler)Delegate.Combine(_onChangedHandler, value);
		}
		remove
		{
			_onChangedHandler = (FileSystemEventHandler)Delegate.Remove(_onChangedHandler, value);
		}
	}

	public event FileSystemEventHandler Created
	{
		add
		{
			_onCreatedHandler = (FileSystemEventHandler)Delegate.Combine(_onCreatedHandler, value);
		}
		remove
		{
			_onCreatedHandler = (FileSystemEventHandler)Delegate.Remove(_onCreatedHandler, value);
		}
	}

	public event FileSystemEventHandler Deleted
	{
		add
		{
			_onDeletedHandler = (FileSystemEventHandler)Delegate.Combine(_onDeletedHandler, value);
		}
		remove
		{
			_onDeletedHandler = (FileSystemEventHandler)Delegate.Remove(_onDeletedHandler, value);
		}
	}

	public event ErrorEventHandler Error
	{
		add
		{
			_onErrorHandler = (ErrorEventHandler)Delegate.Combine(_onErrorHandler, value);
		}
		remove
		{
			_onErrorHandler = (ErrorEventHandler)Delegate.Remove(_onErrorHandler, value);
		}
	}

	public event RenamedEventHandler Renamed
	{
		add
		{
			_onRenamedHandler = (RenamedEventHandler)Delegate.Combine(_onRenamedHandler, value);
		}
		remove
		{
			_onRenamedHandler = (RenamedEventHandler)Delegate.Remove(_onRenamedHandler, value);
		}
	}

	private unsafe void StartRaisingEvents()
	{
		if (IsSuspended())
		{
			_enabled = true;
		}
		else if (IsHandleInvalid(_directoryHandle))
		{
			_directoryHandle = global::Interop.Kernel32.CreateFile(_directory, 1, FileShare.ReadWrite | FileShare.Delete, FileMode.Open, 1107296256);
			if (IsHandleInvalid(_directoryHandle))
			{
				_directoryHandle = null;
				throw new FileNotFoundException(global::SR.Format("Error reading the {0} directory.", _directory));
			}
			AsyncReadState asyncReadState;
			try
			{
				int session = Interlocked.Increment(ref _currentSession);
				byte[] array = AllocateBuffer();
				asyncReadState = new AsyncReadState(session, array, _directoryHandle, ThreadPoolBoundHandle.BindHandle(_directoryHandle));
				asyncReadState.PreAllocatedOverlapped = new PreAllocatedOverlapped(ReadDirectoryChangesCallback, asyncReadState, array);
			}
			catch
			{
				_directoryHandle.Dispose();
				_directoryHandle = null;
				throw;
			}
			_enabled = true;
			Monitor(asyncReadState);
		}
	}

	private void StopRaisingEvents()
	{
		_enabled = false;
		if (!IsSuspended() && !IsHandleInvalid(_directoryHandle))
		{
			Interlocked.Increment(ref _currentSession);
			_directoryHandle.Dispose();
			_directoryHandle = null;
		}
	}

	private void FinalizeDispose()
	{
		if (!IsHandleInvalid(_directoryHandle))
		{
			_directoryHandle.Dispose();
		}
	}

	private static bool IsHandleInvalid(SafeFileHandle handle)
	{
		if (handle != null && !handle.IsInvalid)
		{
			return handle.IsClosed;
		}
		return true;
	}

	private unsafe void Monitor(AsyncReadState state)
	{
		NativeOverlapped* ptr = null;
		bool flag = false;
		try
		{
			if (_enabled && !IsHandleInvalid(state.DirectoryHandle))
			{
				ptr = state.ThreadPoolBinding.AllocateNativeOverlapped(state.PreAllocatedOverlapped);
				flag = global::Interop.Kernel32.ReadDirectoryChangesW(state.DirectoryHandle, state.Buffer, _internalBufferSize, _includeSubdirectories, (int)_notifyFilters, out var _, ptr, IntPtr.Zero);
			}
		}
		catch (ObjectDisposedException)
		{
		}
		catch (ArgumentNullException)
		{
		}
		finally
		{
			if (!flag)
			{
				if (ptr != null)
				{
					state.ThreadPoolBinding.FreeNativeOverlapped(ptr);
				}
				state.PreAllocatedOverlapped.Dispose();
				state.ThreadPoolBinding.Dispose();
				if (!IsHandleInvalid(state.DirectoryHandle))
				{
					OnError(new ErrorEventArgs(new Win32Exception()));
				}
			}
		}
	}

	private unsafe void ReadDirectoryChangesCallback(uint errorCode, uint numBytes, NativeOverlapped* overlappedPointer)
	{
		AsyncReadState asyncReadState = (AsyncReadState)ThreadPoolBoundHandle.GetNativeOverlappedState(overlappedPointer);
		try
		{
			if (IsHandleInvalid(asyncReadState.DirectoryHandle))
			{
				return;
			}
			switch (errorCode)
			{
			default:
				OnError(new ErrorEventArgs(new Win32Exception((int)errorCode)));
				EnableRaisingEvents = false;
				break;
			case 995u:
				break;
			case 0u:
				if (asyncReadState.Session == Volatile.Read(ref _currentSession))
				{
					if (numBytes == 0)
					{
						NotifyInternalBufferOverflowEvent();
					}
					else
					{
						ParseEventBufferAndNotifyForEach(asyncReadState.Buffer);
					}
				}
				break;
			}
		}
		finally
		{
			asyncReadState.ThreadPoolBinding.FreeNativeOverlapped(overlappedPointer);
			Monitor(asyncReadState);
		}
	}

	private unsafe void ParseEventBufferAndNotifyForEach(byte[] buffer)
	{
		int num = 0;
		string text = null;
		string text2 = null;
		int num2;
		do
		{
			int num3;
			fixed (byte* ptr = &buffer[0])
			{
				num2 = *(int*)(ptr + num);
				num3 = ((int*)(ptr + num))[1];
				int num4 = ((int*)(ptr + num))[2];
				text2 = new string((char*)(ptr + num) + 6, 0, num4 / 2);
			}
			switch (num3)
			{
			case 4:
				text = text2;
				break;
			case 5:
				NotifyRenameEventArgs(WatcherChangeTypes.Renamed, text2, text);
				text = null;
				break;
			default:
				if (text != null)
				{
					NotifyRenameEventArgs(WatcherChangeTypes.Renamed, null, text);
					text = null;
				}
				switch (num3)
				{
				case 1:
					NotifyFileSystemEventArgs(WatcherChangeTypes.Created, text2);
					break;
				case 2:
					NotifyFileSystemEventArgs(WatcherChangeTypes.Deleted, text2);
					break;
				case 3:
					NotifyFileSystemEventArgs(WatcherChangeTypes.Changed, text2);
					break;
				}
				break;
			}
			num += num2;
		}
		while (num2 != 0);
		if (text != null)
		{
			NotifyRenameEventArgs(WatcherChangeTypes.Renamed, null, text);
			text = null;
		}
	}

	public FileSystemWatcher()
	{
		_directory = string.Empty;
	}

	public FileSystemWatcher(string path)
	{
		CheckPathValidity(path);
		_directory = path;
	}

	public FileSystemWatcher(string path, string filter)
	{
		CheckPathValidity(path);
		_directory = path;
		Filter = filter ?? throw new ArgumentNullException("filter");
	}

	private byte[] AllocateBuffer()
	{
		try
		{
			return new byte[_internalBufferSize];
		}
		catch (OutOfMemoryException)
		{
			throw new OutOfMemoryException(global::SR.Format("The specified buffer size is too large. FileSystemWatcher cannot allocate {0} bytes for the internal buffer.", _internalBufferSize));
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				StopRaisingEvents();
				_onChangedHandler = null;
				_onCreatedHandler = null;
				_onDeletedHandler = null;
				_onRenamedHandler = null;
				_onErrorHandler = null;
			}
			else
			{
				FinalizeDispose();
			}
		}
		finally
		{
			_disposed = true;
			base.Dispose(disposing);
		}
	}

	private static void CheckPathValidity(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException(global::SR.Format("The directory name {0} is invalid.", path), "path");
		}
		if (!Directory.Exists(path))
		{
			throw new ArgumentException(global::SR.Format("The directory name '{0}' does not exist.", path), "path");
		}
	}

	private bool MatchPattern(ReadOnlySpan<char> relativePath)
	{
		if (relativePath.IsWhiteSpace())
		{
			return false;
		}
		ReadOnlySpan<char> fileName = System.IO.Path.GetFileName(relativePath);
		if (fileName.Length == 0)
		{
			return false;
		}
		string[] filters = _filters.GetFilters();
		if (filters.Length == 0)
		{
			return true;
		}
		string[] array = filters;
		for (int i = 0; i < array.Length; i++)
		{
			if (FileSystemName.MatchesSimpleExpression(array[i], fileName, !PathInternal.IsCaseSensitive))
			{
				return true;
			}
		}
		return false;
	}

	private void NotifyInternalBufferOverflowEvent()
	{
		_onErrorHandler?.Invoke(this, new ErrorEventArgs(new InternalBufferOverflowException(global::SR.Format("Too many changes at once in directory:{0}.", _directory))));
	}

	private void NotifyRenameEventArgs(WatcherChangeTypes action, ReadOnlySpan<char> name, ReadOnlySpan<char> oldName)
	{
		RenamedEventHandler onRenamedHandler = _onRenamedHandler;
		if (onRenamedHandler != null && (MatchPattern(name) || MatchPattern(oldName)))
		{
			onRenamedHandler(this, new RenamedEventArgs(action, _directory, name.IsEmpty ? null : name.ToString(), oldName.IsEmpty ? null : oldName.ToString()));
		}
	}

	private FileSystemEventHandler GetHandler(WatcherChangeTypes changeType)
	{
		return changeType switch
		{
			WatcherChangeTypes.Created => _onCreatedHandler, 
			WatcherChangeTypes.Deleted => _onDeletedHandler, 
			WatcherChangeTypes.Changed => _onChangedHandler, 
			_ => null, 
		};
	}

	private void NotifyFileSystemEventArgs(WatcherChangeTypes changeType, ReadOnlySpan<char> name)
	{
		FileSystemEventHandler handler = GetHandler(changeType);
		if (handler != null && MatchPattern(name.IsEmpty ? ((ReadOnlySpan<char>)_directory) : name))
		{
			handler(this, new FileSystemEventArgs(changeType, _directory, name.IsEmpty ? null : name.ToString()));
		}
	}

	private void NotifyFileSystemEventArgs(WatcherChangeTypes changeType, string name)
	{
		FileSystemEventHandler handler = GetHandler(changeType);
		if (handler != null && MatchPattern(string.IsNullOrEmpty(name) ? _directory : name))
		{
			handler(this, new FileSystemEventArgs(changeType, _directory, name));
		}
	}

	protected void OnChanged(FileSystemEventArgs e)
	{
		InvokeOn(e, _onChangedHandler);
	}

	protected void OnCreated(FileSystemEventArgs e)
	{
		InvokeOn(e, _onCreatedHandler);
	}

	protected void OnDeleted(FileSystemEventArgs e)
	{
		InvokeOn(e, _onDeletedHandler);
	}

	private void InvokeOn(FileSystemEventArgs e, FileSystemEventHandler handler)
	{
		if (handler != null)
		{
			ISynchronizeInvoke synchronizingObject = SynchronizingObject;
			if (synchronizingObject != null && synchronizingObject.InvokeRequired)
			{
				synchronizingObject.BeginInvoke(handler, new object[2] { this, e });
			}
			else
			{
				handler(this, e);
			}
		}
	}

	protected void OnError(ErrorEventArgs e)
	{
		ErrorEventHandler onErrorHandler = _onErrorHandler;
		if (onErrorHandler != null)
		{
			ISynchronizeInvoke synchronizingObject = SynchronizingObject;
			if (synchronizingObject != null && synchronizingObject.InvokeRequired)
			{
				synchronizingObject.BeginInvoke(onErrorHandler, new object[2] { this, e });
			}
			else
			{
				onErrorHandler(this, e);
			}
		}
	}

	protected void OnRenamed(RenamedEventArgs e)
	{
		RenamedEventHandler onRenamedHandler = _onRenamedHandler;
		if (onRenamedHandler != null)
		{
			ISynchronizeInvoke synchronizingObject = SynchronizingObject;
			if (synchronizingObject != null && synchronizingObject.InvokeRequired)
			{
				synchronizingObject.BeginInvoke(onRenamedHandler, new object[2] { this, e });
			}
			else
			{
				onRenamedHandler(this, e);
			}
		}
	}

	public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
	{
		return WaitForChanged(changeType, -1);
	}

	public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
	{
		TaskCompletionSource<WaitForChangedResult> tcs = new TaskCompletionSource<WaitForChangedResult>();
		FileSystemEventHandler fileSystemEventHandler = null;
		RenamedEventHandler renamedEventHandler = null;
		if ((changeType & (WatcherChangeTypes.Changed | WatcherChangeTypes.Created | WatcherChangeTypes.Deleted)) != 0)
		{
			fileSystemEventHandler = delegate(object s, FileSystemEventArgs e)
			{
				if ((e.ChangeType & changeType) != 0)
				{
					tcs.TrySetResult(new WaitForChangedResult(e.ChangeType, e.Name, null, timedOut: false));
				}
			};
			if ((changeType & WatcherChangeTypes.Created) != 0)
			{
				Created += fileSystemEventHandler;
			}
			if ((changeType & WatcherChangeTypes.Deleted) != 0)
			{
				Deleted += fileSystemEventHandler;
			}
			if ((changeType & WatcherChangeTypes.Changed) != 0)
			{
				Changed += fileSystemEventHandler;
			}
		}
		if ((changeType & WatcherChangeTypes.Renamed) != 0)
		{
			renamedEventHandler = delegate(object s, RenamedEventArgs e)
			{
				if ((e.ChangeType & changeType) != 0)
				{
					tcs.TrySetResult(new WaitForChangedResult(e.ChangeType, e.Name, e.OldName, timedOut: false));
				}
			};
			Renamed += renamedEventHandler;
		}
		try
		{
			bool enableRaisingEvents = EnableRaisingEvents;
			if (!enableRaisingEvents)
			{
				EnableRaisingEvents = true;
			}
			tcs.Task.Wait(timeout);
			EnableRaisingEvents = enableRaisingEvents;
		}
		finally
		{
			if (renamedEventHandler != null)
			{
				Renamed -= renamedEventHandler;
			}
			if (fileSystemEventHandler != null)
			{
				if ((changeType & WatcherChangeTypes.Changed) != 0)
				{
					Changed -= fileSystemEventHandler;
				}
				if ((changeType & WatcherChangeTypes.Deleted) != 0)
				{
					Deleted -= fileSystemEventHandler;
				}
				if ((changeType & WatcherChangeTypes.Created) != 0)
				{
					Created -= fileSystemEventHandler;
				}
			}
		}
		if (tcs.Task.Status != TaskStatus.RanToCompletion)
		{
			return WaitForChangedResult.TimedOutResult;
		}
		return tcs.Task.Result;
	}

	private void Restart()
	{
		if (!IsSuspended() && _enabled)
		{
			StopRaisingEvents();
			StartRaisingEventsIfNotDisposed();
		}
	}

	private void StartRaisingEventsIfNotDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		StartRaisingEvents();
	}

	public void BeginInit()
	{
		bool enabled = _enabled;
		StopRaisingEvents();
		_enabled = enabled;
		_initializing = true;
	}

	public void EndInit()
	{
		_initializing = false;
		if (_directory.Length != 0 && _enabled)
		{
			StartRaisingEvents();
		}
	}

	private bool IsSuspended()
	{
		if (!_initializing)
		{
			return base.DesignMode;
		}
		return true;
	}
}
