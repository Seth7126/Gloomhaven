using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsmodeeNet.Foundation;
using MapRuleLibrary.State;

public static class AnalyticsWrapper
{
	private static StatsDataStorage m_StatsDataStorage;

	public static AWContentType[] AWContentTypes = (AWContentType[])Enum.GetValues(typeof(AWContentType));

	public static bool IsAppBootSent = false;

	public static bool IsReady => CoreApplication.HasInstance;

	public static bool IsAnalyticsDisabled()
	{
		return true;
	}

	public static void InitHeader()
	{
	}

	public static Dictionary<string, object> MergeEventProperties(Dictionary<string, object> specificEventProperties)
	{
		return null;
	}

	public static void LogAppBoot()
	{
	}

	public static void LogRunStart(AWGameMode gameMode, AWLaunchMethod launchMethod, CMapState mapState, bool isOnline = false, bool isPrivate = false)
	{
	}

	public static void LogRunStop(AWGameMode gameMode, AWSessionEndReason sessionEndReason, CMapState mapState, bool isOnline = false)
	{
	}

	public static void LogMatchStart(MatchStartConfig matchStartConfig)
	{
	}

	public static void LogMatchStop(MatchStopConfig matchStopConfig)
	{
	}

	public static void LogContentUnlocked(EventContentUnlockedConfig eventContentUnlockedConfig)
	{
	}

	private static Task WaitForAppBootEvent()
	{
		return Task.CompletedTask;
	}

	public static async void LogScreenDisplay(AWScreenName screenName)
	{
	}

	public static void LogEventStop(EventStopConfig eventStopConfig)
	{
	}

	public static void LogAchievementUnlocked(string achievementID)
	{
	}

	public static void LogTutorialStep(EventTutorialStepConfig eventTutorialStepConfig)
	{
	}

	public static void LogDebug(EventDebugStepConfig eventDebugStepConfig)
	{
	}

	public static void LogInventoryUpdate(EventInventoryUpdateConfig eventInventoryUpdateConfig)
	{
	}
}
