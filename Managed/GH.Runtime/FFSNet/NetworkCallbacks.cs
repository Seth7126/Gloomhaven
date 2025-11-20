#define ENABLE_LOGS
using System;
using Assets.Script.Networking.Tokens;
using Photon.Bolt;
using UdpKit;
using VoiceChat;

namespace FFSNet;

[BoltGlobalBehaviour]
public class NetworkCallbacks : GlobalEventListener
{
	public override void BoltStartBegin()
	{
		Debug.Log("Starting up FFSNetwork.");
		Debug.Log("Registering protocol tokens.");
		BoltNetwork.RegisterTokenClass<NetworkAction>();
		BoltNetwork.RegisterTokenClass<UserToken>();
		BoltNetwork.RegisterTokenClass<PlayerToken>();
		BoltNetwork.RegisterTokenClass<ControllableToken>();
		BoltNetwork.RegisterTokenClass<GameToken>();
		BoltNetwork.RegisterTokenClass<CustomDataToken>();
		BoltNetwork.RegisterTokenClass<ConnectionErrorToken>();
		BoltNetwork.RegisterTokenClass<DisconnectionErrorToken>();
	}

	public override void BoltStartDone()
	{
		PlayerRegistry.Initialize();
		if (FFSNetwork.IsHost)
		{
			InitializeGameState();
			foreach (NetworkControllable allControllable in ControllableRegistry.AllControllables)
			{
				if (allControllable.Controller == null)
				{
					PlayerRegistry.MyPlayer.AssignControllable(allControllable, releaseFirst: false, syncAssignmentToClientsIfServer: false);
				}
			}
			FFSNetwork.Manager.CreateSession();
		}
		else if (FFSNetwork.IsClient)
		{
			PlayerRegistry.MyPlayerInitialized.AddListener(InitializeGameState);
			FFSNetwork.Manager.TryJoinPendingSession();
		}
		Debug.Log("FFSNetwork started.");
		FFSNetwork.IsStartingUp = false;
	}

	protected virtual void InitializeGameState()
	{
		PlayerRegistry.MyPlayer.state.AddCallback("LatestProcessedActionID", (PropertyCallbackSimple)delegate
		{
			SaveLatestProcessedActionIDToSaveFile();
		});
	}

	protected virtual void SaveLatestProcessedActionIDToSaveFile()
	{
	}

	public override void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
	{
		Debug.LogError("Error. Failed to start FFSNetwork. Reason: " + disconnectReason);
		if (disconnectReason == UdpConnectionDisconnectReason.Authentication)
		{
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.ConnectionToBackendFailed);
		}
		else
		{
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.ConnectionToBackendFailed);
		}
		if (FFSNetwork.Manager.AutoShutdownUponJoiningFailed)
		{
			FFSNetwork.Shutdown();
		}
	}

	public override void BoltShutdownBegin(AddCallback onShutdownCompleted, UdpConnectionDisconnectReason disconnectReason)
	{
		Debug.Log("FFSNet shutdown started. Resetting variables.");
		FFSNetwork.IsShuttingDown = true;
		ActionProcessor.Shutdown();
		PlayerRegistry.Reset();
		FFSNetwork.Manager.Reset(resetHostingEndedEvent: false);
		FFSNetwork.Behaviour.Reset();
		onShutdownCompleted(delegate
		{
			FFSNetwork.Manager.HostingEndedEvent?.Invoke();
			FFSNetwork.Manager.HostingEndedEvent?.RemoveAllListeners();
			FFSNetwork.IsShuttingDown = false;
			Debug.Log("FFSNetwork is down.");
		});
	}

	public override void ConnectRequest(UdpEndPoint endPoint, IProtocolToken token)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.HasDesynchronized)
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.SessionShuttingDown));
		}
		else
		{
			BoltNetwork.Accept(endPoint);
		}
	}

	protected bool PassesBasicConnectionTests(UdpEndPoint endPoint, UserToken userToken, string correctPassword)
	{
		if (PlayerRegistry.AllPlayers.Count + PlayerRegistry.JoiningPlayers.Count + PlayerRegistry.ConnectingUsers.Count >= FFSNetwork.Manager.MaxPlayers)
		{
			Debug.Log("PassesBasicConnectionTests Failed! AllPlayers: " + PlayerRegistry.AllPlayers.Count + "  MaxPlayers: " + FFSNetwork.Manager.MaxPlayers);
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.SessionFull));
			return false;
		}
		if (!userToken.ServerPassword.Equals(correctPassword))
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.IncorrectPassword));
			return false;
		}
		if (userToken.Username.In("TEMPACCOXXX", "Bl0ckedH4xor"))
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.UserBlocked));
			return false;
		}
		return true;
	}

	public override void OnEvent(NetworkActionEvent evnt)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.HasDesynchronized)
		{
			if (!FFSNetwork.IsOnline)
			{
				Console.LogInfo("Received a NetworkActionEvent but FFSNetwork is not online.");
			}
			if (FFSNetwork.HasDesynchronized)
			{
				Console.LogInfo("Received a NetworkActionEvent but FFSNetwork has desynchronized.");
			}
		}
		else if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a NetworkActionEvent but the event returns null."));
		}
		else if (evnt.Token == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a NetworkActionEvent but the token provided returns null."));
		}
		else
		{
			Console.LogInfo("Received a NetworkAction event: " + (GameActionType)((NetworkAction)evnt.Token).ActionTypeID/*cast due to .constrained prefix*/);
			ActionProcessor.ProcessSideAction(new GameAction(evnt));
		}
	}

	public override void OnEvent(GameActionEventClassID evnt)
	{
		if (FFSNetwork.IsOnline && !FFSNetwork.HasDesynchronized)
		{
			if (evnt == null)
			{
				FFSNetwork.HandleDesync(new Exception("Received a GameActionEventClassID but the event returns null."));
				return;
			}
			Console.LogCoreInfo("Received a GameActionEventClassID event: " + (GameActionType)evnt.ActionTypeID/*cast due to .constrained prefix*/, customFlag: true);
			ActionProcessor.QueueUpAction(new GameAction(evnt));
		}
	}

	public override void OnEvent(GameActionEvent evnt)
	{
		if (FFSNetwork.IsOnline && !FFSNetwork.HasDesynchronized)
		{
			if (evnt == null)
			{
				FFSNetwork.HandleDesync(new Exception("Received a GameActionEvent but the event returns null."));
				return;
			}
			string empty = string.Empty;
			Console.LogCoreInfo("[[Received GameAction #" + evnt.ActionID + " (" + ((GameActionType)evnt.ActionTypeID/*cast due to .constrained prefix*/).ToString() + " @ " + ((ActionPhaseType)evnt.TargetPhaseID/*cast due to .constrained prefix*/).ToString() + ") initiated by " + PlayerRegistry.GetPlayer(evnt.PlayerID).Username + "(PlayerID: " + evnt.PlayerID + ")." + empty + "]]", customFlag: true);
			ActionProcessor.QueueUpAction(new GameAction(evnt));
		}
	}

	public override void OnEvent(BlockedUsersStateChangedEvent eventData)
	{
		Debug.LogWarning("[VOICE CHAT BLOCK]: New block event received!");
		if (!(eventData.Data is BlockedUsersDataToken { UserId: var userId, BlockedUsersIds: var blockedUsersIds } blockedUsersDataToken))
		{
			Debug.LogError("Blocked User data serialization failed!");
		}
		else if (global::Singleton<BoltVoiceChatService>.IsInitialized && global::Singleton<BoltVoiceChatService>.Instance.VoiceChatUserBlockerController != null)
		{
			Debug.LogWarning("[VOICE CHAT BLOCK]: OnOtherPlayerBlockedUsersReceived!");
			global::Singleton<BoltVoiceChatService>.Instance.VoiceChatUserBlockerController.OnOtherPlayerBlockedUsersReceived(userId, blockedUsersIds, blockedUsersDataToken.ResponseRequired);
		}
	}
}
