using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Locations;

public class DifficultyYMLData
{
	public string GameMode { get; set; }

	public List<CAdventureDifficulty> DifficultySettings { get; set; }

	public string FileName { get; private set; }

	public DifficultyYMLData(string fileName)
	{
		GameMode = null;
		DifficultySettings = new List<CAdventureDifficulty>();
		FileName = fileName;
	}

	public bool Validate()
	{
		bool result = true;
		if (GameMode == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Game Mode set for Difficulty.  File " + FileName);
			result = false;
		}
		if (DifficultySettings.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Difficulty settings set for game mode " + GameMode + ".  File: " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(DifficultyYMLData data)
	{
		if (data.DifficultySettings == null)
		{
			return;
		}
		if (DifficultySettings == null)
		{
			DifficultySettings = data.DifficultySettings.Where((CAdventureDifficulty w) => !w.LoadAsNewDifficulty).ToList();
			return;
		}
		if (data.DifficultySettings.Any((CAdventureDifficulty a) => a.LoadAsNewDifficulty))
		{
			DifficultySettings.Clear();
		}
		DifficultySettings.AddRange(data.DifficultySettings.Where((CAdventureDifficulty w) => !w.LoadAsNewDifficulty));
	}
}
