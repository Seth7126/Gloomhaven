namespace GLOOM.MainMenu;

public interface IGameModeService : ICreateGameService, ILoadGameService
{
	bool CanResume();

	bool CanLoadGames();

	void ResumeLastGame();

	void RegisterToDeletedGame(BasicEventHandler action);
}
