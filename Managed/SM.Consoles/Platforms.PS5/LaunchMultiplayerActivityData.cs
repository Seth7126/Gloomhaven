namespace Platforms.PS5;

public class LaunchMultiplayerActivityData : GameIntentData
{
	public override IntentTypes Type => IntentTypes.LaunchMultiplayerActivity;

	public string ActivityId { get; }

	public string PlayerSessionId { get; }

	public LaunchMultiplayerActivityData(int userId, string activityId, string playerSessionId)
		: base(userId)
	{
		ActivityId = activityId;
		PlayerSessionId = playerSessionId;
	}
}
