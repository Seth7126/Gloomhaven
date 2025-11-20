using System;
using System.Collections.Generic;
using AsmodeeNet.Utils;
using UnityEngine;

namespace AsmodeeNet.Foundation;

public class EventManager : MonoBehaviour
{
	private const string _debugModuleName = "EventManager";

	public static EventManager Instance;

	private object _queueLock = new object();

	private List<AsmodeeNet.Utils.Tuple<Delegate, object>> _queuedEvents = new List<AsmodeeNet.Utils.Tuple<Delegate, object>>();

	private List<AsmodeeNet.Utils.Tuple<Delegate, object>> _executingEvents = new List<AsmodeeNet.Utils.Tuple<Delegate, object>>();

	private void OnEnable()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void QueueEvent(Action action)
	{
		if (action == null)
		{
			return;
		}
		lock (_queueLock)
		{
			_queuedEvents.Add(new AsmodeeNet.Utils.Tuple<Delegate, object>(action, null));
		}
	}

	public void QueueEvent<T>(Action<T> action, T parameter)
	{
		if (action == null)
		{
			return;
		}
		lock (_queueLock)
		{
			_queuedEvents.Add(new AsmodeeNet.Utils.Tuple<Delegate, object>(action, parameter));
		}
	}

	public void QueueEvent<T1, T2>(Action<T1, T2> action, T1 parameter1, T2 parameter2)
	{
		if (action == null)
		{
			return;
		}
		lock (_queueLock)
		{
			_queuedEvents.Add(new AsmodeeNet.Utils.Tuple<Delegate, object>(action, new object[2] { parameter1, parameter2 }));
		}
	}

	private void Update()
	{
		MoveQueuedEventsToExecuting();
		while (_executingEvents.Count > 0)
		{
			AsmodeeNet.Utils.Tuple<Delegate, object> tuple = _executingEvents[0];
			_executingEvents.RemoveAt(0);
			try
			{
				if (tuple.Item2 != null && tuple.Item2 is Array)
				{
					Array array = tuple.Item2 as Array;
					if (array.Length == 2)
					{
						tuple.Item1.DynamicInvoke(array.GetValue(0), array.GetValue(1));
					}
					else if (array.Length == 3)
					{
						tuple.Item1.DynamicInvoke(array.GetValue(0), array.GetValue(1), array.GetValue(2));
					}
				}
				else if (tuple.Item2 != null)
				{
					tuple.Item1.DynamicInvoke(tuple.Item2);
				}
				else
				{
					tuple.Item1.DynamicInvoke();
				}
			}
			catch
			{
				AsmoLogger.Error("EventManager", "There was an error when calling the function " + tuple.Item1.Method.Name);
				throw;
			}
		}
	}

	private void MoveQueuedEventsToExecuting()
	{
		lock (_queueLock)
		{
			while (_queuedEvents.Count > 0)
			{
				AsmodeeNet.Utils.Tuple<Delegate, object> item = _queuedEvents[0];
				_executingEvents.Add(item);
				_queuedEvents.RemoveAt(0);
			}
		}
	}
}
