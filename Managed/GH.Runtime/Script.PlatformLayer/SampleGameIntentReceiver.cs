#define ENABLE_LOGS
using Platforms.PS5;

namespace Script.PlatformLayer;

public class SampleGameIntentReceiver : IGameIntentReceiver
{
	private GameIntentData _latestReceivedData;

	public GameIntentData LatestReceivedData => _latestReceivedData;

	public void OnGameIntentReceived(GameIntentData receivedData)
	{
		Debug.Log($"GameIntent received: type - {receivedData.Type}; UserID - {receivedData.UserId};");
		if (receivedData is LaunchActivityData launchActivityData)
		{
			Debug.Log("    Launching activity: " + launchActivityData.ActivityId + ";");
		}
		_latestReceivedData = receivedData;
	}

	public void ResetLatestReceivedData()
	{
		_latestReceivedData = null;
	}
}
