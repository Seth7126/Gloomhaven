using System;
using System.Collections.Generic;

namespace Platforms.PlatformAchievements;

public interface IPlatformAchievements
{
	void GetAllAchievements(Action<OperationResult, string, HashSet<string>> resultCallback);

	void GetUnlockedAchievements(Action<OperationResult, string, HashSet<string>> resultCallback);

	void Unlock(string achievementName, Action<OperationResult, string> resultCallback);

	void IsUnlocked(string achievementName, Action<OperationResult, string, bool> resultCallback);

	void SetProgress(string achievementName, uint progress, Action<OperationResult, string> resultCallback);

	void GetProgress(string achievementName, Action<OperationResult, string, uint> resultCallback);
}
