namespace Platforms.PS5;

public abstract class GameIntentData
{
	public enum IntentTypes
	{
		LaunchActivity,
		JoinSession,
		LaunchMultiplayerActivity
	}

	public abstract IntentTypes Type { get; }

	public int UserId { get; }

	protected GameIntentData(int userId)
	{
		UserId = userId;
	}
}
