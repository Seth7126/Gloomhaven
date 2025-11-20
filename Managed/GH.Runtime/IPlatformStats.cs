using System.Collections.Generic;
using Platforms;

public interface IPlatformStats
{
	bool AchievementsSupported { get; }

	void Initialize(IPlatform platform);

	List<string> GetAllPlatformAchievements();

	void SetAchievementCompleted(List<string> achievementName);

	void ClearAllAchievements();
}
