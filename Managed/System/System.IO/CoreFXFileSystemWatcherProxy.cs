using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.CoreFX;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

internal class CoreFXFileSystemWatcherProxy : IFileWatcher
{
	private static IFileWatcher instance;

	private static IDictionary<object, System.IO.CoreFX.FileSystemWatcher> internal_map;

	private static ConditionalWeakTable<object, FileSystemWatcher> external_map;

	private static IDictionary<object, object> event_map;

	private const int INTERRUPT_MS = 300;

	protected void Operation(Action<IDictionary<object, System.IO.CoreFX.FileSystemWatcher>, ConditionalWeakTable<object, FileSystemWatcher>, IDictionary<object, object>, object> map_op = null, Action<System.IO.CoreFX.FileSystemWatcher, FileSystemWatcher> object_op = null, object handle = null, Action<System.IO.CoreFX.FileSystemWatcher, FileSystemWatcher> cancel_op = null)
	{
		System.IO.CoreFX.FileSystemWatcher internal_fsw = null;
		FileSystemWatcher fsw = null;
		bool flag2;
		if (cancel_op != null)
		{
			bool flag = Monitor.TryEnter(instance, 300);
			flag2 = handle != null && (internal_map.TryGetValue(handle, out internal_fsw) || external_map.TryGetValue(handle, out fsw));
			if (flag2 && flag)
			{
				try
				{
					cancel_op(internal_fsw, fsw);
				}
				catch (Exception)
				{
				}
			}
			if (flag)
			{
				Monitor.Exit(instance);
			}
			if (!flag2 || flag)
			{
				return;
			}
			try
			{
				Task.Run(delegate
				{
					cancel_op(internal_fsw, fsw);
					return true;
				}).Wait(300);
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
		if (map_op != null && handle == null)
		{
			lock (instance)
			{
				try
				{
					map_op(internal_map, external_map, event_map, null);
					return;
				}
				catch (Exception innerException)
				{
					throw new InvalidOperationException("map_op", innerException);
				}
			}
		}
		if (handle == null)
		{
			return;
		}
		lock (instance)
		{
			flag2 = internal_map.TryGetValue(handle, out internal_fsw) && external_map.TryGetValue(handle, out fsw);
			if (flag2 && map_op != null)
			{
				try
				{
					map_op(internal_map, external_map, event_map, handle);
				}
				catch (Exception innerException2)
				{
					throw new InvalidOperationException("map_op", innerException2);
				}
			}
		}
		if (!flag2 || object_op == null)
		{
			return;
		}
		try
		{
			object_op(internal_fsw, fsw);
		}
		catch (Exception innerException3)
		{
			throw new InvalidOperationException("object_op", innerException3);
		}
	}

	protected void ProxyDispatch(object sender, FileAction action, FileSystemEventArgs args)
	{
		RenamedEventArgs renamed = ((action == FileAction.RenamedNewName) ? ((RenamedEventArgs)args) : null);
		object handle = null;
		Operation(delegate(IDictionary<object, System.IO.CoreFX.FileSystemWatcher> in_map, ConditionalWeakTable<object, FileSystemWatcher> out_map, IDictionary<object, object> event_map, object h)
		{
			event_map.TryGetValue(sender, out handle);
		});
		Operation(null, delegate(System.IO.CoreFX.FileSystemWatcher _, FileSystemWatcher fsw)
		{
			if (fsw.EnableRaisingEvents)
			{
				fsw.DispatchEvents(action, args.Name, ref renamed);
				if (fsw.Waiting)
				{
					fsw.Waiting = false;
					Monitor.PulseAll(fsw);
				}
			}
		}, handle);
	}

	protected void ProxyDispatchError(object sender, ErrorEventArgs args)
	{
		object handle = null;
		Operation(delegate(IDictionary<object, System.IO.CoreFX.FileSystemWatcher> in_map, ConditionalWeakTable<object, FileSystemWatcher> out_map, IDictionary<object, object> event_map, object _)
		{
			event_map.TryGetValue(sender, out handle);
		});
		Operation(null, delegate(System.IO.CoreFX.FileSystemWatcher _, FileSystemWatcher fsw)
		{
			fsw.DispatchErrorEvents(args);
		}, handle);
	}

	public object NewWatcher(FileSystemWatcher fsw)
	{
		object handle = new object();
		System.IO.CoreFX.FileSystemWatcher result = new System.IO.CoreFX.FileSystemWatcher();
		result.Changed += delegate(object o, FileSystemEventArgs args)
		{
			Task.Run(delegate
			{
				ProxyDispatch(o, FileAction.Modified, args);
			});
		};
		result.Created += delegate(object o, FileSystemEventArgs args)
		{
			Task.Run(delegate
			{
				ProxyDispatch(o, FileAction.Added, args);
			});
		};
		result.Deleted += delegate(object o, FileSystemEventArgs args)
		{
			Task.Run(delegate
			{
				ProxyDispatch(o, FileAction.Removed, args);
			});
		};
		result.Renamed += delegate(object o, RenamedEventArgs args)
		{
			Task.Run(delegate
			{
				ProxyDispatch(o, FileAction.RenamedNewName, args);
			});
		};
		result.Error += delegate(object o, ErrorEventArgs args)
		{
			Task.Run(delegate
			{
				ProxyDispatchError(handle, args);
			});
		};
		Operation(delegate(IDictionary<object, System.IO.CoreFX.FileSystemWatcher> in_map, ConditionalWeakTable<object, FileSystemWatcher> out_map, IDictionary<object, object> event_map, object _)
		{
			in_map.Add(handle, result);
			out_map.Add(handle, fsw);
			event_map.Add(result, handle);
		});
		return handle;
	}

	public void StartDispatching(object handle)
	{
		if (handle != null)
		{
			Operation(null, delegate(System.IO.CoreFX.FileSystemWatcher internal_fsw, FileSystemWatcher fsw)
			{
				internal_fsw.Path = fsw.Path;
				internal_fsw.Filter = fsw.Filter;
				internal_fsw.IncludeSubdirectories = fsw.IncludeSubdirectories;
				internal_fsw.InternalBufferSize = fsw.InternalBufferSize;
				internal_fsw.NotifyFilter = fsw.NotifyFilter;
				internal_fsw.Site = fsw.Site;
				internal_fsw.EnableRaisingEvents = true;
			}, handle);
		}
	}

	public void StopDispatching(object handle)
	{
		if (handle == null)
		{
			return;
		}
		Operation(null, null, handle, delegate(System.IO.CoreFX.FileSystemWatcher internal_fsw, FileSystemWatcher fsw)
		{
			if (internal_fsw != null)
			{
				internal_fsw.EnableRaisingEvents = false;
			}
		});
	}

	public void Dispose(object handle)
	{
		if (handle != null)
		{
			Operation(null, null, handle, delegate(System.IO.CoreFX.FileSystemWatcher internal_fsw, FileSystemWatcher fsw)
			{
				internal_fsw?.Dispose();
				System.IO.CoreFX.FileSystemWatcher key = internal_map[handle];
				internal_map.Remove(handle);
				external_map.Remove(handle);
				event_map.Remove(key);
				handle = null;
			});
		}
	}

	public static bool GetInstance(out IFileWatcher watcher)
	{
		if (instance != null)
		{
			watcher = instance;
			return true;
		}
		internal_map = new ConcurrentDictionary<object, System.IO.CoreFX.FileSystemWatcher>();
		external_map = new ConditionalWeakTable<object, FileSystemWatcher>();
		event_map = new ConcurrentDictionary<object, object>();
		instance = (watcher = new CoreFXFileSystemWatcherProxy());
		return true;
	}
}
