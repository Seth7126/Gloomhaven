namespace GLOOM.MainMenu;

public interface ICreateGameService
{
	void CreateGame(IGameData data);

	bool ValidateExistsGame(string partyName);
}
