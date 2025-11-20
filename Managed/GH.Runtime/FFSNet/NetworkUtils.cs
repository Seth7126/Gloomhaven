using System;
using Photon.Bolt;

namespace FFSNet;

public static class NetworkUtils
{
	public const PlatformType PlatformType = PlatformType.PC;

	public static bool HasSession(PlatformType platformType)
	{
		switch (platformType)
		{
		case PlatformType.PS4:
		case PlatformType.PS5:
			return true;
		case PlatformType.PC:
		case PlatformType.GameCore:
		case PlatformType.Switch:
			return false;
		default:
			throw new Exception($"Invalid {platformType}");
		}
	}

	public static void SendSessionCommunicationEvent(SessionMessageType messageType, string data, BoltConnection boltConnection)
	{
		SessionNegotiationEvent sessionNegotiationEvent = SessionNegotiationEvent.Create(boltConnection, ReliabilityModes.ReliableOrdered);
		sessionNegotiationEvent.MessageType = (int)messageType;
		sessionNegotiationEvent.Data = data;
		sessionNegotiationEvent.Platform = 1;
		sessionNegotiationEvent.Send();
	}
}
