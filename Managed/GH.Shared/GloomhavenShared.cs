#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using SM.Utils;
using UnityEngine;

public class GloomhavenShared : MonoBehaviour
{
	public const string GloomhavenSceneName = "Gloomhaven_unified";

	public static bool OverrideGameBootedForAutoTests;

	public const string OverrideAutoTestFolder = "../AutotestsPerformance";

	public const string OverrideAutoTestResultsLog = "./Autotest_Runner_Results.xml";

	private static bool _canStart;

	private static bool _autoTestRunnerStepFinished;

	public static int AutoTestIndex;

	public static bool CanStart
	{
		get
		{
			return _canStart;
		}
		set
		{
			if (value != _canStart)
			{
				_canStart = value;
			}
		}
	}

	public static bool AutoTestRunnerStepFinished
	{
		get
		{
			return _autoTestRunnerStepFinished;
		}
		set
		{
			if (value != _autoTestRunnerStepFinished)
			{
				_autoTestRunnerStepFinished = value;
			}
		}
	}

	public static void LogBuildInfo()
	{
		List<string> list = new List<string>();
		list.Add("ENABLE_MONO");
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);
		LogUtils.Log("[INFO] Important Defines:" + string.Join(",", list));
		LogUtils.Log("[INFO] Application.dataPath:" + Application.dataPath);
		LogUtils.Log("[INFO] Application.persistentDataPath:" + PathsManager.PersistionDataPath);
		LogUtils.Log("[INFO] ParseCommandLineArgs:" + string.Join(",", Environment.GetCommandLineArgs()));
	}
}
