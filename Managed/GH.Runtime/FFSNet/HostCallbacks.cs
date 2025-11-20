using System.Linq;
using Photon.Bolt;

namespace FFSNet;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class HostCallbacks : GlobalEventListener
{
	public override void Connected(BoltConnection connection)
	{
		Console.LogInfo(((UserToken)connection.ConnectToken).Username + " connected.");
		if (!PlayerRegistry.JoiningPlayers.Contains(connection))
		{
			if (PlayerRegistry.AllPlayers.Where((NetworkPlayer w) => w.IsClient).Count() > 0)
			{
				Synchronizer.SendSideAction(GameActionType.NotifyClientsPlayersAreJoining);
			}
			UserToken userToken = (UserToken)connection.ConnectToken;
			PlayerRegistry.JoiningPlayers.Add(connection);
			PlayerRegistry.ConnectingUsers.RemoveAll((UserToken x) => x.Username == userToken.Username && x.PlatformPlayerID == userToken.PlatformPlayerID);
		}
		PlayerRegistry.OnUserConnected?.Invoke(connection);
	}

	public override void Disconnected(BoltConnection connection)
	{
		Console.LogInfo("User disconnected.");
		UserToken userToken = (UserToken)connection.ConnectToken;
		PlayerRegistry.ConnectingUsers.RemoveAll((UserToken x) => x.Username == userToken.Username && x.PlatformPlayerID == userToken.PlatformPlayerID);
		if (PlayerRegistry.JoiningPlayers.Contains(connection))
		{
			PlayerRegistry.JoiningPlayers.Remove(connection);
			PlayerRegistry.OnJoiningUserDisconnected?.Invoke(connection);
			if (PlayerRegistry.JoiningPlayers.Count == 0)
			{
				Synchronizer.SendSideAction(GameActionType.NotifyClientsPlayersAreFinishedJoining);
			}
		}
		else
		{
			PlayerRegistry.RemovePlayer((NetworkPlayer)connection.UserData);
		}
	}

	public override void OnEvent(PlayerEntityRequest request)
	{
		if (request == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00023", "Received a PlayerEntityRequest but the event returns null.");
		}
		else if (request.RaisedBy == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00024", "Received a PlayerEntityRequest but the sending player does not exist anymore.");
		}
		else
		{
			PlayerRegistry.CreatePlayer(request.RaisedBy);
		}
	}

	public override void OnEvent(PlayerEntityInitializedEvent request)
	{
		if (request == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00025", "Received a PlayerEntityInitializedEvent but the event returns null.");
			return;
		}
		if (request.RaisedBy == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00026", "Received a PlayerEntityInitializedEvent but the sending player does not exist anymore.");
			return;
		}
		if (PlayerRegistry.JoiningPlayers.Contains(request.RaisedBy))
		{
			PlayerRegistry.JoiningPlayers.Remove(request.RaisedBy);
			if (PlayerRegistry.JoiningPlayers.Count == 0)
			{
				Synchronizer.SendSideAction(GameActionType.NotifyClientsPlayersAreFinishedJoining);
			}
		}
		NetworkPlayer joinedPlayer = PlayerRegistry.GetPlayer(request.RaisedBy);
		joinedPlayer.MaskBadWordsInUsername(delegate
		{
			joinedPlayer.OnInitialized();
			foreach (NetworkPlayer item in PlayerRegistry.AllPlayers.Where((NetworkPlayer w) => w != joinedPlayer))
			{
				foreach (int assignedSlot in item.AssignedSlots)
				{
					int playerID = item.PlayerID;
					int dataInt = assignedSlot;
					Synchronizer.SendSideAction(GameActionType.AssignSlot, null, canBeUnreliable: false, sendToHostOnly: false, joinedPlayer.PlayerID, playerID, dataInt);
				}
			}
		});
	}

	public override void OnEvent(ControllableAssignmentRequest request)
	{
		if (request == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00027", "Received a ControllableAssignmentRequest but the request returns null.");
			return;
		}
		if (request.RaisedBy == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00028", "Received a ControllableAssignmentRequest but the requesting player does not exist anymore.");
			return;
		}
		NetworkControllable networkControllable = ControllableRegistry.AllControllables.FirstOrDefault((NetworkControllable x) => x.Controller == null || x.Controller == PlayerRegistry.MyPlayer);
		if (networkControllable == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00029", "Controllable was requested by the client but no controllables are left to be assigned.");
		}
		else
		{
			PlayerRegistry.GetPlayer(request.RaisedBy)?.AssignControllable(networkControllable, releaseFirst: true);
		}
	}
}
