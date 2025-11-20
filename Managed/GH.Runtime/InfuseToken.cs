using System.Collections.Generic;
using System.Text;
using Photon.Bolt;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class InfuseToken : IProtocolToken
{
	private int elementsCount;

	public string AbilityID { get; set; }

	public int[] Elements { get; set; }

	public InfuseToken(string abilityID, List<ElementInfusionBoardManager.EElement> elements)
	{
		AbilityID = abilityID;
		elementsCount = elements?.Count ?? 0;
		Elements = new int[elementsCount];
		for (int i = 0; i < elementsCount; i++)
		{
			Elements[i] = (int)elements[i];
		}
	}

	public InfuseToken()
	{
		AbilityID = string.Empty;
		elementsCount = 0;
		Elements = new int[elementsCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(AbilityID, Encoding.ASCII);
		packet.WriteInt(elementsCount);
		for (int i = 0; i < elementsCount; i++)
		{
			packet.WriteInt(Elements[i]);
		}
	}

	public void Read(UdpPacket packet)
	{
		AbilityID = packet.ReadString(Encoding.ASCII);
		elementsCount = packet.ReadInt();
		Elements = new int[elementsCount];
		for (int i = 0; i < elementsCount; i++)
		{
			Elements[i] = packet.ReadInt();
		}
	}
}
