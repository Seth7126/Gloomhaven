using System.Text;
using MapRuleLibrary.Party;
using Photon.Bolt;
using UdpKit;

public sealed class AchievementToken : IProtocolToken
{
	public string AchievementName { get; private set; }

	public AchievementToken(CPartyAchievement achievement)
	{
		AchievementName = achievement.ID;
	}

	public AchievementToken()
	{
		AchievementName = string.Empty;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(AchievementName, Encoding.UTF8);
	}

	public void Read(UdpPacket packet)
	{
		AchievementName = packet.ReadString(Encoding.UTF8);
	}
}
