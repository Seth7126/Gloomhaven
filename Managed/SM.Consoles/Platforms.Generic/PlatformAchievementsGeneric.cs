#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using Platforms.PlatformAchievements;
using SM.Utils;

namespace Platforms.Generic;

public class PlatformAchievementsGeneric : IPlatformAchievements
{
	private readonly HashSet<string> _allAchievements = new HashSet<string>
	{
		"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
		"10"
	};

	private readonly HashSet<string> _unlockedAchievements = new HashSet<string>();

	private readonly Dictionary<string, uint> _progress = new Dictionary<string, uint>();

	public void GetAllAchievements(Action<OperationResult, string, HashSet<string>> resultCallback)
	{
		resultCallback(OperationResult.Success, string.Empty, _allAchievements);
	}

	public void GetUnlockedAchievements(Action<OperationResult, string, HashSet<string>> resultCallback)
	{
		resultCallback(OperationResult.Success, string.Empty, _unlockedAchievements);
	}

	public void Unlock(string achievementName, Action<OperationResult, string> resultCallback)
	{
		if (!_unlockedAchievements.Contains(achievementName))
		{
			LogUtils.Log("Achievement " + achievementName + " unlocked!");
			_unlockedAchievements.Add(achievementName);
		}
		resultCallback(OperationResult.Success, string.Empty);
	}

	public void IsUnlocked(string achievementName, Action<OperationResult, string, bool> resultCallback)
	{
		resultCallback(OperationResult.Success, string.Empty, _unlockedAchievements.Contains(achievementName));
	}

	public void SetProgress(string achievementName, uint progress, Action<OperationResult, string> resultCallback)
	{
		if (!_progress.ContainsKey(achievementName))
		{
			_progress.Add(achievementName, 0u);
		}
		_progress[achievementName] = progress;
		resultCallback(OperationResult.Success, string.Empty);
	}

	public void GetProgress(string achievementName, Action<OperationResult, string, uint> resultCallback)
	{
		if (!_progress.ContainsKey(achievementName))
		{
			_progress.Add(achievementName, 0u);
		}
		LogUtils.Log($"Achievement {achievementName} has progress {_progress[achievementName]}");
		resultCallback(OperationResult.Success, string.Empty, _progress[achievementName]);
	}
}
