using System;
using System.Collections;
using System.Threading;
using AsmodeeNet.Utils;
using UnityEngine.Profiling;

namespace AsmodeeNet.Foundation;

public class AsyncCoroutine
{
	public delegate void Task();

	private const string _kModuleName = "AsyncCoroutine";

	public static IEnumerator Start(Task t, string name)
	{
		bool done = false;
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				t();
			}
			catch (Exception ex)
			{
				AsmoLogger.LogException(ex, name, AsmoLogger.Severity.Fatal);
			}
			finally
			{
				Profiler.EndThreadProfiling();
				done = true;
			}
		});
		while (!done)
		{
			yield return null;
		}
	}

	public static void RecursivelyMoveNext(IEnumerator enumerator)
	{
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is IEnumerator enumerator2)
			{
				RecursivelyMoveNext(enumerator2);
			}
		}
	}
}
