using Photon.Bolt;
using UdpKit;

namespace FFSNet;

public class NetworkAction : IProtocolToken
{
	public int ActionTypeID { get; private set; }

	public int PlayerID { get; private set; }

	public IProtocolToken DataToken { get; private set; }

	public int TargetPlayerID { get; private set; }

	public int DataInt { get; private set; }

	public int DataInt2 { get; private set; }

	public bool DataBoolean { get; private set; }

	public NetworkAction(GameActionType actionType, NetworkPlayer actingPlayer, IProtocolToken actionDataToken, int targetPlayerID = 0, int dataInt = 0, int dataInt2 = 0, bool dataBoolean = false)
	{
		ActionTypeID = (int)actionType;
		PlayerID = actingPlayer.PlayerID;
		DataToken = actionDataToken;
		TargetPlayerID = targetPlayerID;
		DataInt = dataInt;
		DataInt2 = dataInt2;
		DataBoolean = dataBoolean;
	}

	public NetworkAction()
	{
		ActionTypeID = 0;
		PlayerID = 0;
		DataToken = null;
		TargetPlayerID = 0;
		DataInt = 0;
		DataInt2 = 0;
		DataBoolean = false;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(ActionTypeID);
		packet.WriteInt(PlayerID);
		packet.WriteInt(TargetPlayerID);
		packet.WriteInt(DataInt);
		packet.WriteInt(DataInt2);
		packet.WriteBool(DataBoolean);
		packet.WriteToken(DataToken);
	}

	public void Read(UdpPacket packet)
	{
		ActionTypeID = packet.ReadInt();
		PlayerID = packet.ReadInt();
		TargetPlayerID = packet.ReadInt();
		DataInt = packet.ReadInt();
		DataInt2 = packet.ReadInt();
		DataBoolean = packet.ReadBool();
		DataToken = packet.ReadToken();
	}
}
