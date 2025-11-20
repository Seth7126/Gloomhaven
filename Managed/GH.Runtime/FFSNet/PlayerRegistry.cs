#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Bolt;
using UnityEngine.Events;

namespace FFSNet;

public static class PlayerRegistry
{
	public static List<NetworkPlayer> AllPlayers = new List<NetworkPlayer>();

	private static bool m_WaitForOtherPlayers;

	public static bool WaitForOtherPlayersFullLoaded;

	public static List<NetworkPlayer> PlayersFinishedLoading = new List<NetworkPlayer>();

	public static HashSet<NetworkPlayer> PlayersFinishedFullLoading = new HashSet<NetworkPlayer>();

	private static bool m_IsSwitchingCharacter;

	public static int UsersUnderProfanityCheck = 0;

	public static int SwitchingPlayerID = int.MaxValue;

	public static int SwitchingCharacterSlot = int.MaxValue;

	public static UnityEvent MyPlayerInitialized = new UnityEvent();

	private static NetworkPlayer myPlayer;

	private static NetworkPlayer hostPlayer;

	public static readonly int HostPlayerID = 1;

	public static List<NetworkPlayer> Participants => AllPlayers.FindAll((NetworkPlayer x) => x.IsParticipant);

	public static List<BoltConnection> JoiningPlayers { get; private set; }

	public static List<UserToken> ConnectingUsers { get; private set; }

	public static bool OtherClientsAreJoining { get; private set; }

	public static bool WaitForOtherPlayers
	{
		get
		{
			return m_WaitForOtherPlayers;
		}
		set
		{
			m_WaitForOtherPlayers = value;
			Console.Log("WaitForOtherPlayers set to: " + m_WaitForOtherPlayers);
		}
	}

	public static bool LoadingInFromJoiningClient { get; set; }

	public static bool IsSwitchingCharacter
	{
		get
		{
			return m_IsSwitchingCharacter;
		}
		set
		{
			if (m_IsSwitchingCharacter != value)
			{
				m_IsSwitchingCharacter = value;
				if (!value)
				{
					SwitchingPlayerID = int.MaxValue;
				}
				OnDetermineRosterSlotInteractability?.Invoke();
			}
		}
	}

	public static bool IsProfanityCheckInProcess => UsersUnderProfanityCheck > 0;

	public static BoltConnection MyConnection { get; set; }

	public static NetworkPlayer MyPlayer
	{
		get
		{
			return myPlayer;
		}
		set
		{
			myPlayer = value;
			if (value != null && MyPlayerInitialized != null)
			{
				Console.LogInfo("Invoking MyPlayerInitilized. Listeners: " + MyPlayerInitialized.GetPersistentEventCount());
				MyPlayerInitialized.Invoke();
				MyPlayerInitialized.RemoveAllListeners();
				if (FFSNetwork.IsClient)
				{
					PlayerEntityInitializedEvent.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered).Send();
				}
			}
		}
	}

	public static NetworkPlayer HostPlayer
	{
		get
		{
			if (hostPlayer == null)
			{
				hostPlayer = AllPlayers.FirstOrDefault((NetworkPlayer x) => !x.IsClient);
			}
			return hostPlayer;
		}
	}

	public static ConnectionEstablishedEvent OnUserConnected { get; set; }

	public static ConnectionEstablishedEvent OnJoiningUserDisconnected { get; set; }

	public static DetermineRosterSlotInteractability OnDetermineRosterSlotInteractability { get; set; }

	public static PlayersChangedEvent OnPlayerJoined { get; set; }

	public static PlayersChangedEvent OnPlayerLeft { get; set; }

	public static UserEnterEvent OnUserEnterRoom { get; set; }

	public static void Initialize()
	{
		if (FFSNetwork.IsHost)
		{
			JoiningPlayers = new List<BoltConnection>();
			ConnectingUsers = new List<UserToken>();
			CreatePlayer(null);
		}
		OtherClientsAreJoining = false;
		LoadingInFromJoiningClient = false;
		WaitForOtherPlayers = false;
		WaitForOtherPlayersFullLoaded = false;
		m_IsSwitchingCharacter = false;
		SwitchingPlayerID = int.MaxValue;
		SwitchingCharacterSlot = int.MaxValue;
		PlayersFinishedLoading.Clear();
		PlatformLayer.Platform.PlatformSocial.OnUserLeftPlayerSession += RemovePlayerFromBoltSession;
	}

	public static string GetUserName(BoltConnection connection)
	{
		Debug.Log("GetUserName() called");
		return PlatformLayer.UserData.GetUserNameForConnection(connection);
	}

	private static string GetPlatformName(BoltConnection connection)
	{
		if (connection != null)
		{
			return ((UserToken)connection.ConnectToken).PlatformName;
		}
		return PlatformLayer.Instance.PlatformID;
	}

	private static string GetPlatformNetworkAccountPlayerID(BoltConnection connection)
	{
		if (connection != null)
		{
			return ((UserToken)connection.ConnectToken).PlatformNetworkAccountPlayerID;
		}
		return PlatformLayer.UserData.PlatformNetworkAccountPlayerID;
	}

	private static byte[] GetPlatformRecentPlayerKey(BoltConnection connection)
	{
		if (connection != null)
		{
			return ((UserToken)connection.ConnectToken).RecentPlayerKey;
		}
		return PlatformLayer.Networking.GetRecentPlayerKey();
	}

	public static void CreatePlayer(BoltConnection connection)
	{
		int playerID = (int)(connection?.ConnectionId ?? 1);
		string platformIDForConnection = PlatformLayer.UserData.GetPlatformIDForConnection(connection);
		string userName = GetUserName(connection);
		string platformName = GetPlatformName(connection);
		string platformNetworkAccountPlayerID = GetPlatformNetworkAccountPlayerID(connection);
		byte[] platformRecentPlayerKey = GetPlatformRecentPlayerKey(connection);
		NetworkPlayer component = BoltNetwork.Instantiate(BoltPrefabs.Network_Player, new PlayerToken(playerID, platformIDForConnection, userName, platformName, platformNetworkAccountPlayerID, platformRecentPlayerKey)).GetComponent<NetworkPlayer>();
		Console.LogInfo("Created a NetworkPlayer with PlayerID: " + playerID + ", Entity NetworkID: " + component.entity.NetworkId.ToString());
		if (connection == null)
		{
			component.NetStats = new NetworkStatsServer(null);
			component.entity.TakeControl();
		}
		else
		{
			component.NetStats = new NetworkStatsClient(connection);
			component.NetStats.Connection.UserData = component;
			component.entity.AssignControl(connection);
		}
	}

	private static void RemovePlayerFromBoltSession(string playerNetworkId)
	{
		Console.LogInfo("[PlayerRegistry] RemovePlayerFromBoltSession(" + playerNetworkId + ") It looks like the player has been removed / left Player Session on PS platform. Removing him from Bolt Session...");
		RemovePlayer(GetPlayer(playerNetworkId), sendKickMessage: true);
	}

	public static void RemovePlayer(NetworkPlayer player, bool sendKickMessage = false)
	{
		try
		{
			if (player != null)
			{
				if (!player.entity.IsAttached)
				{
					return;
				}
				Console.LogInfo("Removing NetworkPlayer: " + player);
				if (sendKickMessage)
				{
					if (FFSNetwork.IsHost)
					{
						BoltNetwork.Destroy(player.entity, new ConnectionErrorToken(ConnectionErrorCode.MultiplayerValidationFail));
					}
					else
					{
						BoltNetwork.Destroy(player.entity, new ConnectionErrorToken(ConnectionErrorCode.KickedByHost));
					}
				}
				else
				{
					BoltNetwork.Destroy(player.entity);
				}
			}
			else
			{
				Console.LogWarning("Tried removing a player but the NetworkPlayer returns null.");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception during RemovePlayer.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static NetworkPlayer GetPlayer(BoltConnection connection)
	{
		if (connection == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00035", "No such player found.");
			return null;
		}
		return (NetworkPlayer)connection.UserData;
	}

	public static NetworkPlayer GetPlayer(int playerID)
	{
		return AllPlayers.FirstOrDefault((NetworkPlayer x) => x.PlayerID == playerID);
	}

	public static NetworkPlayer GetPlayer(string platformNetworkAccountPlayerID)
	{
		Console.Log("[PlayerRegistry.cs] GetPlayer(" + platformNetworkAccountPlayerID + ") called...");
		NetworkPlayer networkPlayer = AllPlayers.FirstOrDefault((NetworkPlayer x) => x.PlatformNetworkAccountPlayerID == platformNetworkAccountPlayerID);
		if (networkPlayer != null)
		{
			Console.Log("[PlayerRegistry.cs] GetPlayer about to return player named: " + networkPlayer.Username + " with playerID: " + networkPlayer.PlayerID);
		}
		else
		{
			Console.LogError("ERROR_MULTIPLAYER_00035", "[PlayerRegistry.cs] GetPlayer: No such player found!");
		}
		return networkPlayer;
	}

	public static void ProxyClientDesync(GameAction action)
	{
		NetworkPlayer player = GetPlayer(action.DataInt);
		if (player != null)
		{
			player.HasDesynched = true;
		}
	}

	public static void Reset()
	{
		MyPlayerInitialized.RemoveAllListeners();
		OnUserConnected = null;
		OnJoiningUserDisconnected = null;
		OnDetermineRosterSlotInteractability = null;
		OnPlayerJoined = null;
		OnPlayerLeft = null;
		OnUserEnterRoom = null;
		PlatformLayer.Platform.PlatformSocial.OnUserLeftPlayerSession -= RemovePlayerFromBoltSession;
		hostPlayer = null;
		myPlayer = null;
		MyConnection = null;
		JoiningPlayers = null;
		ConnectingUsers = null;
		OtherClientsAreJoining = false;
		LoadingInFromJoiningClient = false;
		AllPlayers.Clear();
		Console.LogInfo("Cleared Network Player Registry!");
	}

	public static void PrintAllPlayerControllables()
	{
		Console.LogDebug("Printing all players' controllables:");
		foreach (NetworkPlayer allPlayer in AllPlayers)
		{
			if (allPlayer != null)
			{
				allPlayer.PrintControllables();
			}
			else
			{
				Console.LogWarning("Null player detected when printing all player controllables.");
			}
		}
	}

	public static bool CheckForReceivedAvatarUpdate()
	{
		bool result = false;
		foreach (NetworkPlayer allPlayer in AllPlayers)
		{
			if (allPlayer.Avatar == null && allPlayer.AvatarUpdated)
			{
				result = true;
				allPlayer.AvatarUpdated = false;
			}
		}
		return result;
	}

	public static bool IsJoining(NetworkPlayer controller)
	{
		if (JoiningPlayers != null)
		{
			return JoiningPlayers.Exists((BoltConnection it) => (NetworkPlayer)it.UserData == controller);
		}
		return false;
	}

	public static string GetPlayerIdentifierString(int playerID)
	{
		return GetPlayerIdentifierString(GetPlayer(playerID));
	}

	public static string GetPlayerIdentifierString(NetworkPlayer player)
	{
		if (!(player != null))
		{
			return " (NULL PLAYER)";
		}
		return "(Player: " + player.Username + ", PlayerID: " + player.PlayerID + ")";
	}

	public static void ToggleOtherClientsAreJoining(bool value)
	{
		OtherClientsAreJoining = value;
		if (global::Singleton<UIMapMultiplayerController>.Instance != null)
		{
			global::Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
		}
		if (global::Singleton<UIScenarioMultiplayerController>.Instance != null)
		{
			global::Singleton<UIScenarioMultiplayerController>.Instance.RefreshWaitingNotifications();
		}
	}

	public static void StartWaitingForPlayers()
	{
		if (MyPlayer != null)
		{
			MyPlayer.SentPlayerReadyForAssignment = false;
		}
		if (FFSNetwork.IsOnline && MyPlayer.IsParticipant)
		{
			WaitForOtherPlayers = true;
		}
		else
		{
			WaitForOtherPlayers = false;
		}
		PlayersFinishedLoading.Clear();
	}

	public static void ProxyNotifyLoadingFinished(GameAction action)
	{
		NotifyLoadingFinished(AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID == action.PlayerID));
	}

	public static void NotifyLoadingFinished(NetworkPlayer player)
	{
		if (player != null && !PlayersFinishedLoading.Contains(player))
		{
			PlayersFinishedLoading.Add(player);
		}
		CheckLoadingFinished();
	}

	public static void CheckLoadingFinished()
	{
		if (WaitForOtherPlayers && Participants.Count <= PlayersFinishedLoading.Count)
		{
			WaitForOtherPlayers = false;
			PlayersFinishedLoading.Clear();
		}
	}

	public static void ProxyNotifyFullLoadingFinished(GameAction action)
	{
		NotifyFullLoadingFinished(AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID == action.PlayerID));
	}

	public static void NotifyFullLoadingFinished(NetworkPlayer player)
	{
		if (player != null && !PlayersFinishedFullLoading.Contains(player))
		{
			PlayersFinishedFullLoading.Add(player);
		}
		CheckFullLoadingFinished();
	}

	public static void CheckFullLoadingFinished()
	{
		if (WaitForOtherPlayersFullLoaded && AllPlayers.Count == PlayersFinishedFullLoading.Count)
		{
			WaitForOtherPlayersFullLoaded = false;
			PlayersFinishedFullLoading.Clear();
		}
	}

	public static IEnumerator WaitForAllPlayersFullLoaded()
	{
		while (WaitForOtherPlayersFullLoaded)
		{
			yield return null;
		}
	}

	public static void ProxyToggleIsSwitchingCharacter(GameAction action)
	{
		if (action.SupplementaryDataBoolean)
		{
			if (!IsSwitchingCharacter)
			{
				Debug.Log("[IsSwitchingCharacter] False at ProxyToggleIsSwitchingCharacter");
				IsSwitchingCharacter = true;
				SwitchingPlayerID = action.DataInt;
				SwitchingCharacterSlot = action.DataInt2;
			}
			else
			{
				Debug.Log("Call to ProxyToggleIsSwitchingCharacter from player ID " + action.PlayerID + " is being ignored as player ID " + SwitchingPlayerID + " is already switching characters");
			}
		}
		else if (IsSwitchingCharacter)
		{
			IsSwitchingCharacter = false;
			Debug.Log("[IsSwitchingCharacter] True at ProxyToggleIsSwitchingCharacter");
		}
		else
		{
			Debug.Log("Call to ProxyToggleIsSwitchingCharacter from player ID " + action.PlayerID + " is being ignored as we are already not switching characters");
		}
	}

	public static bool HasPlayerControllables()
	{
		if (MyPlayer != null && MyPlayer.IsParticipant)
		{
			return MyPlayer.MyControllables.Any((NetworkControllable x) => x.IsAlive);
		}
		return false;
	}
}
