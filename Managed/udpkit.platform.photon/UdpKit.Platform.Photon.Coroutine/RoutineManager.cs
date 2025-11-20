using System.Collections;
using UdpKit.Platform.Photon.Utils;
using UnityEngine;

namespace UdpKit.Platform.Photon.Coroutine;

internal class RoutineManager : MonoBehaviour
{
	private static ISynchronizedQueue<IEnumerator> _tasks;

	private static ISynchronizedQueue<IEnumerator> _cancelTasks;

	public static IEnumerator EnqueueRoutine(IEnumerator asyncMethod)
	{
		if (asyncMethod == null)
		{
			return null;
		}
		_tasks.Enqueue(asyncMethod);
		return asyncMethod;
	}

	public static void CancelRoutine(params IEnumerator[] asyncMethods)
	{
		foreach (IEnumerator enumerator in asyncMethods)
		{
			if (enumerator != null)
			{
				_cancelTasks.Enqueue(enumerator);
			}
		}
	}

	public void OnEnable()
	{
		_tasks = new SynchronizedQueue<IEnumerator>();
		_cancelTasks = new SynchronizedQueue<IEnumerator>();
	}

	public void OnDestroy()
	{
		IEnumerator item;
		while (_tasks.TryDequeue(out item))
		{
			UdpLog.Info($"Stop background task: {item}");
			StopCoroutine(item);
		}
	}

	public void Update()
	{
		if (_tasks.TryDequeue(out var item))
		{
			StartCoroutine(item);
		}
		if (_cancelTasks.TryDequeue(out var item2))
		{
			StopCoroutine(item2);
		}
	}
}
