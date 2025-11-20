namespace Platforms.PS5;

public class JoinSessionData : GameIntentData
{
	public enum MemberTypes
	{
		Unknown,
		Player,
		Spectator
	}

	public override IntentTypes Type => IntentTypes.JoinSession;

	public string PlayerSessionId { get; }

	public MemberTypes MemberType { get; }

	public JoinSessionData(int userId, string playerSessionId, MemberTypes memberType)
		: base(userId)
	{
		PlayerSessionId = playerSessionId;
		MemberType = memberType;
	}
}
