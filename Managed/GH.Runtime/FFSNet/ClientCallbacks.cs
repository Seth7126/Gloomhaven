using System;
using Photon.Bolt;
using UdpKit;

namespace FFSNet;

[BoltGlobalBehaviour(BoltNetworkModes.Client)]
public class ClientCallbacks : GlobalEventListener
{
	public override void Connected(BoltConnection connection)
	{
		Console.LogInfo("Connected. My Connection: " + connection);
		PlayerRegistry.MyConnection = connection;
	}

	public override void Disconnected(BoltConnection connection)
	{
		string empty = string.Empty;
		DisconnectionErrorCode disconnectionErrorCode;
		if (connection.DisconnectToken is DisconnectionErrorToken disconnectionErrorToken)
		{
			disconnectionErrorCode = (DisconnectionErrorCode)disconnectionErrorToken.ErrorCode;
			empty = ((DisconnectionErrorCode)disconnectionErrorToken.ErrorCode/*cast due to .constrained prefix*/).ToString();
		}
		else
		{
			disconnectionErrorCode = DisconnectionErrorCode.None;
			empty = connection.DisconnectReason.ToString();
		}
		switch (disconnectionErrorCode)
		{
		case DisconnectionErrorCode.HostEndedSession:
			Console.LogInfo("Host ended the session.");
			break;
		case DisconnectionErrorCode.Desynchronization:
			Console.LogInfo("Disconnected by the host. Host desynchronized.");
			break;
		default:
			if (empty == UdpConnectionDisconnectReason.Disconnected.ToString())
			{
				Console.LogInfo("Session ended.");
			}
			else
			{
				Console.LogError("ERROR_MULTIPLAYER_00038", "Disconnected from the session. Reason: " + empty);
			}
			break;
		}
		FFSNetwork.Manager.OnDisconnected?.Invoke(disconnectionErrorCode);
		FFSNetwork.Shutdown();
	}

	public override void ConnectRefused(UdpEndPoint endPoint, IProtocolToken token)
	{
		ConnectionErrorCode errorCode = (ConnectionErrorCode)((ConnectionErrorToken)token).ErrorCode;
		Console.LogError("ERROR_MULTIPLAYER_00016", "Connection refused: " + errorCode.GetAttribute<NameAttribute>().Description);
		FFSNetwork.Manager.OnConnectionFailed?.Invoke(errorCode);
		if (FFSNetwork.Manager.AutoShutdownUponJoiningFailed)
		{
			FFSNetwork.Shutdown();
		}
	}

	public override void SessionConnected(UdpSession session, IProtocolToken token)
	{
		Console.LogInfo("Connected to session: " + session);
		FFSNetwork.Manager.ConnectingToSession = false;
	}

	public override void SessionConnectFailed(UdpSession session, IProtocolToken token, UdpSessionError errorReason)
	{
		Console.LogError("ERROR_MULTIPLAYER_00017", "Failed to connect to session.", "Session:" + session);
		FFSNetwork.Manager.ConnectingToSession = false;
		FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.ConnectionToSessionFailed);
		if (FFSNetwork.Manager.AutoShutdownUponJoiningFailed)
		{
			FFSNetwork.Shutdown();
		}
	}

	public override void OnEvent(ControllableAssignmentEvent evnt)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a ControllableAssignmentEvent but the event returns null."));
			return;
		}
		NetworkPlayer player = PlayerRegistry.GetPlayer(evnt.PlayerID);
		if (player != null)
		{
			player.AssignControllable(evnt.ControllableID, evnt.ReleaseFirst, syncAssignmentToClientsIfServer: false);
		}
		else
		{
			FFSNetwork.HandleDesync(new Exception("Received a ControllableAssignmentEvent but the target player does not (ID: " + evnt.PlayerID + ") exist."));
		}
	}

	public override void OnEvent(ControllableReleaseEvent evnt)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a ControllableReleaseEvent but the event returns null."));
			return;
		}
		NetworkPlayer player = PlayerRegistry.GetPlayer(evnt.PlayerID);
		if (player != null)
		{
			player.ReleaseControllable(evnt.ControllableID);
		}
		else
		{
			FFSNetwork.HandleDesync(new Exception("Received a ControllableReleaseEvent but the target player (ID: " + evnt.PlayerID + ") does not exist."));
		}
	}

	public override void OnEvent(ControllableDestructionEvent evnt)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a ControllableReleaseEvent but the event returns null."));
		}
		else
		{
			ControllableRegistry.DestroyControllable(evnt.ControllableID);
		}
	}
}
