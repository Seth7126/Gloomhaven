#define ENABLE_LOGS
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SM.Utils;

namespace ScenarioRuleLibrary;

public class SEventLogMessageHandler
{
	private static Thread _eventLogMessageHandler;

	private static volatile ConcurrentQueue<SEvent> _eventLogQueue;

	private static volatile BlockingCollection<SEvent> _blockingCollection;

	private static volatile ManualResetEventSlim _manualResetEventSlim;

	private static volatile CancellationTokenSource _cancellationTokenSource;

	private static volatile bool _blockEventLogMessageProcessing;

	public static void Initialise()
	{
		_eventLogQueue = new ConcurrentQueue<SEvent>();
		_blockingCollection = new BlockingCollection<SEvent>(_eventLogQueue);
	}

	public static void Reset()
	{
		if (_eventLogMessageHandler != null)
		{
			if (!_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}
			_eventLogMessageHandler.Join();
		}
		if (_manualResetEventSlim != null)
		{
			_manualResetEventSlim.Dispose();
		}
		_manualResetEventSlim = new ManualResetEventSlim(initialState: true);
		_cancellationTokenSource = new CancellationTokenSource();
		_eventLogMessageHandler = new Thread(ThreadEventLogMessageHandler);
		_eventLogMessageHandler.Start();
		_blockEventLogMessageProcessing = false;
	}

	public static void AddEventLogMessage(SEvent sEvent, bool processImmediately = false)
	{
		if (processImmediately)
		{
			ProcessEventLogMessage(sEvent);
		}
		else
		{
			_blockingCollection.Add(sEvent);
		}
	}

	public static List<T> GetQueuedEvents<T>() where T : SEvent
	{
		return _blockingCollection.OfType<T>().ToList();
	}

	public static List<SEventAttackModifier> GetQueuedModifiers()
	{
		return _blockingCollection.OfType<SEventAttackModifier>().ToList();
	}

	public static List<SEventAbilityCondition> GetQueuedConditions()
	{
		return _blockingCollection.OfType<SEventAbilityCondition>().ToList();
	}

	public static List<SEventAbility> GetQueuedAbilities()
	{
		return _blockingCollection.OfType<SEventAbility>().ToList();
	}

	public static void ThreadEventLogMessageHandler()
	{
		CancellationToken token = _cancellationTokenSource.Token;
		try
		{
			while (true)
			{
				token.ThrowIfCancellationRequested();
				_manualResetEventSlim.Wait(token);
				token.ThrowIfCancellationRequested();
				SEvent sEvent = null;
				sEvent = _blockingCollection.Take(token);
				token.ThrowIfCancellationRequested();
				_manualResetEventSlim.Wait(token);
				token.ThrowIfCancellationRequested();
				if (sEvent != null)
				{
					ProcessEventLogMessage(sEvent);
				}
			}
		}
		catch (OperationCanceledException)
		{
			LogUtils.Log("OperationCanceledException OperationCanceledException finished");
		}
		catch (ObjectDisposedException)
		{
			LogUtils.Log("ObjectDisposedException ThreadEventLogMessageHandler finished");
		}
	}

	private static void ProcessEventLogMessage(SEvent sEvent)
	{
		if (ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.ScenarioEventLog != null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.AddEvent(sEvent);
		}
	}

	public static void ToggleEventLogProcessing(bool process)
	{
		if (process)
		{
			if (_blockEventLogMessageProcessing)
			{
				_blockEventLogMessageProcessing = false;
				if (_manualResetEventSlim != null)
				{
					_manualResetEventSlim.Set();
				}
			}
		}
		else if (!_blockEventLogMessageProcessing)
		{
			_blockEventLogMessageProcessing = true;
			if (_manualResetEventSlim != null)
			{
				_manualResetEventSlim.Reset();
			}
		}
	}
}
