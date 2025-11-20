namespace Platforms.PS5;

public class LaunchActivityData : GameIntentData
{
	public override IntentTypes Type => IntentTypes.LaunchActivity;

	public string ActivityId { get; }

	public LaunchActivityData(int userId, string activityId)
		: base(userId)
	{
		ActivityId = activityId;
	}
}
