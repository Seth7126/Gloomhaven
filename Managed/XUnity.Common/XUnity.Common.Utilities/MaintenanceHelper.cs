using System;
using System.Collections.Generic;
using System.Threading;
using XUnity.Common.Logging;

namespace XUnity.Common.Utilities;

public static class MaintenanceHelper
{
	private class ActionRegistration
	{
		public Action Action { get; }

		public int Filter { get; }

		public ActionRegistration(Action action, int filter)
		{
			Action = action;
			Filter = filter;
		}
	}

	private static readonly object Sync = new object();

	private static readonly List<ActionRegistration> RegisteredActions = new List<ActionRegistration>();

	private static bool _initialized;

	public static void AddMaintenanceFunction(Action action, int filter)
	{
		lock (Sync)
		{
			if (!_initialized)
			{
				_initialized = true;
				StartMaintenance();
			}
			ActionRegistration item = new ActionRegistration(action, filter);
			RegisteredActions.Add(item);
		}
	}

	private static void StartMaintenance()
	{
		Thread thread = new Thread(MaintenanceLoop);
		thread.IsBackground = true;
		thread.Start();
	}

	private static void MaintenanceLoop(object state)
	{
		int num = 0;
		while (true)
		{
			lock (Sync)
			{
				foreach (ActionRegistration registeredAction in RegisteredActions)
				{
					if (num % registeredAction.Filter == 0)
					{
						try
						{
							registeredAction.Action();
						}
						catch (Exception e)
						{
							XuaLogger.Common.Error(e, "An unexpected error occurred during maintenance.");
						}
					}
				}
			}
			num++;
			Thread.Sleep(5000);
		}
	}
}
