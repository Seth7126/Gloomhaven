using System.Collections;
using System.Collections.Generic;
using Platforms;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class PlatformStats : MonoBehaviour, IPlatformStats
{
	public bool AchievementsSupported => false;

	private IEnumerator Start()
	{
		while (!PlatformLayer.Initialised)
		{
			yield return null;
		}
		Initialize(PlatformLayer.Platform);
	}

	public void Initialize(IPlatform platform)
	{
		if (SteamClient.IsValid)
		{
			SteamUserStats.RequestCurrentStats();
		}
	}

	public List<string> GetAllPlatformAchievements()
	{
		List<string> list = new List<string>();
		if (SteamClient.IsValid)
		{
			foreach (Achievement achievement in SteamUserStats.Achievements)
			{
				list.Add(achievement.Name);
			}
		}
		if (list.Count == 0)
		{
			Debug.LogError("[PlatformStats_Steam] GetAllPlatformAchievements() was not able to load any Achievements on Steam platform. Empty list returned.");
		}
		return list;
	}

	public void SetAchievementCompleted(List<string> achievementName)
	{
		if (!SteamClient.IsValid || achievementName == null || achievementName.Count <= 0)
		{
			return;
		}
		foreach (string item in achievementName)
		{
			Achievement achievement = new Achievement(item);
			achievement.Trigger();
			_ = achievement.GlobalUnlocked;
		}
		SteamUserStats.StoreStats();
	}

	public void ClearAllAchievements()
	{
		if (!SteamClient.IsValid)
		{
			return;
		}
		foreach (Achievement achievement in SteamUserStats.Achievements)
		{
			achievement.Clear();
		}
	}
}
