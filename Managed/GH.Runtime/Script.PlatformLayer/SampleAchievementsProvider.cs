using System;
using System.Collections.Generic;
using Platforms;
using Platforms.PS4;
using Platforms.PS5;

namespace Script.PlatformLayer;

public class SampleAchievementsProvider : IAchievementsProviderPS4, IAchievementPack, IAchievementsProviderPS5
{
	public enum SampleTrophies
	{
		Platinum = 0,
		BasicGold = 1,
		BasicSilver = 2,
		BasicBronze = 3,
		Hidden = 4,
		ProggreeStatThree = 5,
		ProgressStatTwenty = 6,
		BasicProgress = 7,
		Reward = 8,
		LastIndex = 8,
		TrophyCount = 9
	}

	private readonly HashSet<string> _achievements = new HashSet<string>
	{
		SampleTrophies.BasicGold.ToString(),
		SampleTrophies.BasicSilver.ToString(),
		SampleTrophies.BasicBronze.ToString(),
		SampleTrophies.Hidden.ToString(),
		SampleTrophies.BasicProgress.ToString(),
		SampleTrophies.Reward.ToString()
	};

	public HashSet<string> GetAllAchievements()
	{
		return _achievements;
	}

	public int NameToId(string name)
	{
		if (Enum.TryParse<SampleTrophies>(name, out var result))
		{
			return (int)result;
		}
		throw new Exception("Invalid achievement name - " + name + ";");
	}

	public string IdToName(int id)
	{
		SampleTrophies sampleTrophies = (SampleTrophies)id;
		return sampleTrophies.ToString();
	}
}
