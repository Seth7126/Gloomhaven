using System;
using System.Collections.Generic;
using SRF.Service;
using UnityEngine;

namespace SRDebugger.Services.Implementation;

[Service(typeof(IConsoleService))]
public class StandardConsoleService : IConsoleService, IDisposable
{
	private readonly bool _collapseEnabled;

	private bool _hasCleared;

	private readonly CircularBuffer<ConsoleEntry> _allConsoleEntries;

	private CircularBuffer<ConsoleEntry> _consoleEntries;

	private readonly object _threadLock = new object();

	private ILogHandler _expectedLogHandler;

	public int ErrorCount { get; private set; }

	public int WarningCount { get; private set; }

	public int InfoCount { get; private set; }

	public bool LoggingEnabled
	{
		get
		{
			return Debug.unityLogger.logEnabled;
		}
		set
		{
			Debug.unityLogger.logEnabled = value;
		}
	}

	public bool LogHandlerIsOverriden => Debug.unityLogger.logHandler != _expectedLogHandler;

	public IReadOnlyList<ConsoleEntry> Entries
	{
		get
		{
			if (!_hasCleared)
			{
				return _allConsoleEntries;
			}
			return _consoleEntries;
		}
	}

	public IReadOnlyList<ConsoleEntry> AllEntries => _allConsoleEntries;

	public event ConsoleUpdatedEventHandler Updated;

	public event ConsoleUpdatedEventHandler Error;

	public StandardConsoleService()
	{
		Application.logMessageReceivedThreaded += UnityLogCallback;
		_expectedLogHandler = Debug.unityLogger.logHandler;
		SRServiceManager.RegisterService<IConsoleService>(this);
		_collapseEnabled = Settings.Instance.CollapseDuplicateLogEntries;
		_allConsoleEntries = new CircularBuffer<ConsoleEntry>(Settings.Instance.MaximumConsoleEntries);
	}

	public void Dispose()
	{
		Application.logMessageReceivedThreaded -= UnityLogCallback;
		if (_consoleEntries != null)
		{
			_consoleEntries.Clear();
		}
		_allConsoleEntries.Clear();
	}

	public void Clear()
	{
		lock (_threadLock)
		{
			_hasCleared = true;
			if (_consoleEntries == null)
			{
				_consoleEntries = new CircularBuffer<ConsoleEntry>(Settings.Instance.MaximumConsoleEntries);
			}
			else
			{
				_consoleEntries.Clear();
			}
			int num = (InfoCount = 0);
			int errorCount = (WarningCount = num);
			ErrorCount = errorCount;
		}
		OnUpdated();
	}

	protected void OnEntryAdded(ConsoleEntry entry)
	{
		if (_hasCleared)
		{
			if (_consoleEntries.IsFull)
			{
				AdjustCounter(_consoleEntries.Front().LogType, -1);
				_consoleEntries.PopFront();
			}
			_consoleEntries.PushBack(entry);
		}
		else if (_allConsoleEntries.IsFull)
		{
			AdjustCounter(_allConsoleEntries.Front().LogType, -1);
			_allConsoleEntries.PopFront();
		}
		_allConsoleEntries.PushBack(entry);
		OnUpdated();
	}

	protected void OnEntryDuplicated(ConsoleEntry entry)
	{
		entry.Count++;
		OnUpdated();
		if (_hasCleared && _consoleEntries.Count == 0)
		{
			OnEntryAdded(new ConsoleEntry(entry)
			{
				Count = 1
			});
		}
	}

	private void OnUpdated()
	{
		if (this.Updated != null)
		{
			try
			{
				this.Updated(this);
			}
			catch
			{
			}
		}
	}

	private void UnityLogCallback(string condition, string stackTrace, LogType type)
	{
		lock (_threadLock)
		{
			ConsoleEntry consoleEntry = ((_collapseEnabled && _allConsoleEntries.Count > 0) ? _allConsoleEntries[_allConsoleEntries.Count - 1] : null);
			AdjustCounter(type, 1);
			if (consoleEntry != null && consoleEntry.LogType == type && consoleEntry.Message == condition && consoleEntry.StackTrace == stackTrace)
			{
				OnEntryDuplicated(consoleEntry);
				return;
			}
			ConsoleEntry entry = new ConsoleEntry
			{
				LogType = type,
				StackTrace = stackTrace,
				Message = condition
			};
			OnEntryAdded(entry);
		}
	}

	private void AdjustCounter(LogType type, int amount)
	{
		switch (type)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			ErrorCount += amount;
			if (this.Error != null)
			{
				this.Error(this);
			}
			break;
		case LogType.Warning:
			WarningCount += amount;
			break;
		case LogType.Log:
			InfoCount += amount;
			break;
		}
	}
}
