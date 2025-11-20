#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Photon.Bolt.Utils;

public static class BoltLog
{
	public interface IWriter : IDisposable
	{
		void Info(string message);

		void Debug(string message);

		void Warn(string message);

		void Error(string message);
	}

	internal class File : IWriter, IDisposable
	{
		private volatile bool running = true;

		private readonly bool isServer;

		private readonly Thread thread;

		private readonly AutoResetEvent threadEvent;

		private readonly Queue<string> threadQueue;

		public File(bool server)
		{
			isServer = server;
			threadEvent = new AutoResetEvent(initialState: false);
			threadQueue = new Queue<string>(1024);
			if (!Application.isEditor)
			{
				thread = new Thread(WriteLoop);
				thread.IsBackground = true;
				thread.Start();
			}
		}

		private void Queue(string message)
		{
			lock (threadQueue)
			{
				threadQueue.Enqueue(message);
				threadEvent.Set();
			}
		}

		void IWriter.Info(string message)
		{
			Queue(message);
		}

		void IWriter.Debug(string message)
		{
			Queue(message);
		}

		void IWriter.Warn(string message)
		{
			Queue(message);
		}

		void IWriter.Error(string message)
		{
			Queue(message);
		}

		public void Dispose()
		{
			running = false;
		}

		private void WriteLoop()
		{
			try
			{
				DateTime now = DateTime.Now;
				string format = "Bolt_Log_{7}_{0}Y-{1}M-{2}D_{3}H{4}M{5}S_{6}MS.txt";
				format = string.Format(format, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, isServer ? "SERVER" : "CLIENT");
				FileStream fileStream = System.IO.File.Open(format, FileMode.Create);
				StreamWriter streamWriter = new StreamWriter(fileStream);
				while (running)
				{
					if (threadEvent.WaitOne(100))
					{
						lock (threadQueue)
						{
							while (threadQueue.Count > 0)
							{
								streamWriter.WriteLine(threadQueue.Dequeue());
							}
						}
					}
					streamWriter.Flush();
					fileStream.Flush();
				}
				streamWriter.Flush();
				streamWriter.Close();
				streamWriter.Dispose();
				threadEvent.Close();
			}
			catch (Exception exception)
			{
				Exception(exception);
			}
		}

		public override bool Equals(object obj)
		{
			return obj.GetType().FullName == GetType().FullName;
		}

		public override int GetHashCode()
		{
			return GetType().FullName.GetHashCode();
		}
	}

	internal class ConsoleWriter : IWriter, IDisposable
	{
		void IWriter.Info(string message)
		{
			BoltConsole.Write(message, BoltGUI.Info);
		}

		void IWriter.Debug(string message)
		{
			BoltConsole.Write(message, BoltGUI.Debug);
		}

		void IWriter.Warn(string message)
		{
			BoltConsole.Write(message, BoltGUI.Warn);
		}

		void IWriter.Error(string message)
		{
			BoltConsole.Write(message, BoltGUI.Error);
		}

		public void Dispose()
		{
		}

		public override bool Equals(object obj)
		{
			return obj.GetType().FullName == GetType().FullName;
		}

		public override int GetHashCode()
		{
			return GetType().FullName.GetHashCode();
		}
	}

	internal class SystemOutWriter : IWriter, IDisposable
	{
		void IWriter.Info(string message)
		{
			Console.Out.WriteLine(message);
		}

		void IWriter.Debug(string message)
		{
			Console.Out.WriteLine(message);
		}

		void IWriter.Warn(string message)
		{
			Console.Out.WriteLine(message);
		}

		void IWriter.Error(string message)
		{
			Console.Error.WriteLine(message);
		}

		public void Dispose()
		{
		}

		public override bool Equals(object obj)
		{
			return obj.GetType().FullName == GetType().FullName;
		}

		public override int GetHashCode()
		{
			return GetType().FullName.GetHashCode();
		}
	}

	internal class UnityWriter : IWriter, IDisposable
	{
		void IWriter.Info(string message)
		{
			UnityEngine.Debug.Log(message);
		}

		void IWriter.Debug(string message)
		{
			UnityEngine.Debug.Log(message);
		}

		void IWriter.Warn(string message)
		{
			UnityEngine.Debug.LogWarning(message);
		}

		void IWriter.Error(string message)
		{
			UnityEngine.Debug.LogError(message);
		}

		public void Dispose()
		{
		}

		public override bool Equals(object obj)
		{
			return obj.GetType().FullName == GetType().FullName;
		}

		public override int GetHashCode()
		{
			return GetType().FullName.GetHashCode();
		}
	}

	private static readonly object _lock = new object();

	private static List<IWriter> _writers = new List<IWriter>();

	public static void RemoveAll()
	{
		lock (_lock)
		{
			for (int i = 0; i < _writers.Count; i++)
			{
				_writers[i].Dispose();
			}
			_writers = new List<IWriter>();
		}
	}

	public static void Add<T>(T instance) where T : class, IWriter
	{
		lock (_lock)
		{
			if (!_writers.Contains(instance))
			{
				_writers.Add(instance);
			}
		}
	}

	[Conditional("DEBUG")]
	public static void Info(string message)
	{
		lock (_lock)
		{
			for (int i = 0; i < _writers.Count; i++)
			{
				_writers[i].Info(message);
			}
		}
	}

	[Conditional("DEBUG")]
	public static void Info(object message)
	{
		Info(Format(message));
	}

	[Conditional("DEBUG")]
	public static void Info(string message, object arg0)
	{
		Info(Format(message, arg0));
	}

	[Conditional("DEBUG")]
	public static void Info(string message, object arg0, object arg1)
	{
		Info(Format(message, arg0, arg1));
	}

	[Conditional("DEBUG")]
	public static void Info(string message, object arg0, object arg1, object arg2)
	{
		Info(Format(message, arg0, arg1, arg2));
	}

	[Conditional("DEBUG")]
	public static void Info(string message, params object[] args)
	{
		Info(Format(message, args));
	}

	[Conditional("DEBUG")]
	internal static void Debug(string message)
	{
		lock (_lock)
		{
			for (int i = 0; i < _writers.Count; i++)
			{
				_writers[i].Debug(message);
			}
		}
	}

	[Conditional("DEBUG")]
	internal static void Debug(object message)
	{
		Debug(Format(message));
	}

	[Conditional("DEBUG")]
	internal static void Debug(string message, object arg0)
	{
		Debug(Format(message, arg0));
	}

	[Conditional("DEBUG")]
	internal static void Debug(string message, object arg0, object arg1)
	{
		Debug(Format(message, arg0, arg1));
	}

	[Conditional("DEBUG")]
	internal static void Debug(string message, object arg0, object arg1, object arg2)
	{
		Debug(Format(message, arg0, arg1, arg2));
	}

	[Conditional("DEBUG")]
	internal static void Debug(string message, params object[] args)
	{
		Debug(Format(message, args));
	}

	[Conditional("DEBUG")]
	public static void Warn(string message)
	{
		lock (_lock)
		{
			for (int i = 0; i < _writers.Count; i++)
			{
				_writers[i].Warn(message);
			}
		}
	}

	[Conditional("DEBUG")]
	public static void Warn(object message)
	{
		Warn(Format(message));
	}

	[Conditional("DEBUG")]
	public static void Warn(string message, object arg0)
	{
		Warn(Format(message, arg0));
	}

	[Conditional("DEBUG")]
	public static void Warn(string message, object arg0, object arg1)
	{
		Warn(Format(message, arg0, arg1));
	}

	[Conditional("DEBUG")]
	public static void Warn(string message, object arg0, object arg1, object arg2)
	{
		Warn(Format(message, arg0, arg1, arg2));
	}

	[Conditional("DEBUG")]
	public static void Warn(string message, params object[] args)
	{
		Warn(Format(message, FixNulls(args)));
	}

	private static object[] FixNulls(object[] args)
	{
		if (args == null)
		{
			args = new object[0];
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == null)
			{
				args[i] = "NULL";
			}
		}
		return args;
	}

	[Conditional("DEBUG")]
	public static void Error(string message)
	{
		lock (_lock)
		{
			for (int i = 0; i < _writers.Count; i++)
			{
				_writers[i].Error(message);
			}
		}
	}

	[Conditional("DEBUG")]
	public static void Error(object message)
	{
		Error(Format(message));
	}

	[Conditional("DEBUG")]
	public static void Error(string message, object arg0)
	{
		Error(Format(message, arg0));
	}

	[Conditional("DEBUG")]
	public static void Error(string message, object arg0, object arg1)
	{
		Error(Format(message, arg0, arg1));
	}

	[Conditional("DEBUG")]
	public static void Error(string message, object arg0, object arg1, object arg2)
	{
		Error(Format(message, arg0, arg1, arg2));
	}

	[Conditional("DEBUG")]
	public static void Error(string message, params object[] args)
	{
		Error(Format(message, args));
	}

	public static void Exception(Exception exception)
	{
		lock (_lock)
		{
			UnityEngine.Debug.LogException(exception);
			for (int i = 0; i < _writers.Count; i++)
			{
				if (!(_writers[i] is UnityWriter))
				{
					_writers[i].Error(exception.GetType()?.ToString() + ": " + exception.Message);
					_writers[i].Error(exception.StackTrace);
				}
			}
		}
	}

	private static string Format(object message)
	{
		return (message == null) ? "NULL" : message.ToString();
	}

	private static string Format(string message, object arg0)
	{
		return string.Format(Format(message), Format(arg0));
	}

	private static string Format(string message, object arg0, object arg1)
	{
		return string.Format(Format(message), Format(arg0), Format(arg1));
	}

	private static string Format(string message, object arg0, object arg1, object arg2)
	{
		return string.Format(Format(message), Format(arg0), Format(arg1), Format(arg2));
	}

	private static string Format(string message, object[] args)
	{
		if (args == null)
		{
			return Format(message);
		}
		for (int i = 0; i < args.Length; i++)
		{
			args[i] = Format(args[i]);
		}
		return string.Format(Format(message), args);
	}

	internal static void Setup(BoltNetworkModes mode, BoltConfigLogTargets logTargets)
	{
		bool flag = (logTargets & BoltConfigLogTargets.File) == BoltConfigLogTargets.File;
		bool flag2 = (logTargets & BoltConfigLogTargets.Unity) == BoltConfigLogTargets.Unity;
		bool flag3 = (logTargets & BoltConfigLogTargets.Console) == BoltConfigLogTargets.Console;
		bool flag4 = (logTargets & BoltConfigLogTargets.SystemOut) == BoltConfigLogTargets.SystemOut;
		if (flag2)
		{
			Add(new UnityWriter());
		}
		if (flag3)
		{
			Add(new ConsoleWriter());
		}
		if (flag4)
		{
			Add(new SystemOutWriter());
		}
		if (flag)
		{
			RuntimePlatform platform = Application.platform;
			RuntimePlatform runtimePlatform = platform;
			if ((uint)runtimePlatform <= 2u || runtimePlatform == RuntimePlatform.WindowsEditor)
			{
				Add(new File(mode == BoltNetworkModes.Server));
			}
		}
	}
}
