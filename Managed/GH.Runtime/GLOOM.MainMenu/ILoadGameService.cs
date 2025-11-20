using System.Collections.Generic;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public interface ILoadGameService
{
	List<IGameSaveData> GetSaves();

	void DeleteGame(IGameSaveData data);

	void LoadGame(IGameSaveData data);

	void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd);
}
