namespace Platforms.PS5;

public interface IGameIntentReceiver
{
	void OnGameIntentReceived(GameIntentData receivedData);
}
