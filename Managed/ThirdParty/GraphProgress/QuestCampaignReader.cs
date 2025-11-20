#define ENABLE_LOGS
using System.Collections.Generic;
using System.IO;
using SM.Utils;
using UnityEngine;

namespace GraphProgress;

public class QuestCampaignReader
{
	private static string rulebaseSource = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Rulebase\\Campaign\\Quests\\Story"));

	private static List<string[]> fileStorage = new List<string[]>();

	private void ReadFromFile()
	{
		string[] files = Directory.GetFiles(rulebaseSource, "*.yml");
		for (int i = 0; i < files.Length; i++)
		{
			string[] item = File.ReadAllLines(files[i]);
			fileStorage.Add(item);
		}
		for (int j = 0; j < fileStorage.Count; j++)
		{
			LogUtils.Log("________" + j + "________");
			for (int k = 0; k < fileStorage[j].Length; k++)
			{
				LogUtils.Log(fileStorage[j][k]);
			}
		}
	}
}
