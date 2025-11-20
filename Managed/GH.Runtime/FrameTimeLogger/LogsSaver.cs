using System;
using System.Collections.Generic;
using System.Text;
using SRDebugger.Internal;
using SRDebugger.Services;
using UnityEngine;

namespace FrameTimeLogger;

public class LogsSaver : Singleton<LogsSaver>
{
	protected override void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private static ICSVWriter GetWriter()
	{
		return new EditorCSVWriter();
	}

	public void SaveLogs()
	{
		StringBuilder stringBuilder = new StringBuilder();
		IReadOnlyList<ConsoleEntry> entries = Service.Console.Entries;
		for (int i = 0; i < entries.Count; i++)
		{
			ConsoleEntry consoleEntry = entries[i];
			if (consoleEntry.LogType == LogType.Exception || consoleEntry.LogType == LogType.Error)
			{
				stringBuilder.Append("[!!!]\n");
				stringBuilder.Append(consoleEntry.Message + "\n");
				stringBuilder.Append(consoleEntry.StackTrace + "\n");
			}
			else
			{
				stringBuilder.Append(consoleEntry.Message + "\n");
			}
		}
		GetWriter().Write($"FullLogs_{DateTime.Now:dd.MM.yyyy_HH.mm.ss}", stringBuilder.ToString());
	}
}
