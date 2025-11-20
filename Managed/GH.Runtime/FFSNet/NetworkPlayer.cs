using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using Platforms;
using Platforms.Social;
using UnityEngine;

namespace FFSNet;

[DebuggerDisplay("PlayerID: {PlayerID}  IsClient: {IsClient}")]
public class NetworkPlayer : EntityBehaviour<IPlayerState>
{
	private CreatingCharacter m_CreatingCharacter;

	private bool m_PlayerReadyForAssignment;

	private List<int> m_AssignedSlots = new List<int>();

	private bool m_IsCreatingCharacter;

	public ObservableCollection<NetworkControllable> MyControllables = new ObservableCollection<NetworkControllable>();

	public int PlayerID { get; private set; }

	public bool IsClient => PlayerID > PlayerRegistry.HostPlayerID;

	public bool HasDesynched { get; set; }

	public bool IsParticipant => MyControllables.FirstOrDefault((NetworkControllable x) => x.IsParticipant) != null;

	public bool IsSlotAssigned => m_AssignedSlots.Count > 0;

	public bool IsActive => MyControllables.FirstOrDefault((NetworkControllable x) => x.IsAlive) != null;

	public NetworkStats NetStats { get; set; }

	public string Username { get; private set; }

	public string PlatformName { get; private set; }

	public string PlatformPlayerId { get; private set; }

	public string PlatformNetworkAccountPlayerID { get; private set; }

	public byte[] RecentPlayerKey { get; private set; }

	public CreatingCharacter CreatingCharacter
	{
		get
		{
			return m_CreatingCharacter;
		}
		set
		{
			m_CreatingCharacter = value;
			Console.Log("CreatingCharacter set to: " + ((m_CreatingCharacter == null) ? "NULL" : ("(PlayerID: " + m_CreatingCharacter.PlayerID + " CharacterName: " + m_CreatingCharacter.CharacterName.ToString() + ")")));
		}
	}

	public bool PlayerReadyForAssignment
	{
		get
		{
			return m_PlayerReadyForAssignment;
		}
		set
		{
			m_PlayerReadyForAssignment = value;
			Console.Log("Player of ID: " + PlayerID + "PlayerReadyForAssignment set to: " + m_PlayerReadyForAssignment);
		}
	}

	public bool SentPlayerReadyForAssignment { get; set; }

	public List<int> AssignedSlots => m_AssignedSlots;

	public Sprite Avatar { get; set; }

	public bool AvatarUpdated { get; set; }

	public bool IsCreatingCharacter => m_IsCreatingCharacter;

	public List<NetworkControllable> MyParticipatingControllables => MyControllables.ToList().FindAll((NetworkControllable x) => x.IsParticipant);

	public List<NetworkPlayer> PlayersACKedMyLatestControllableState { get; set; }

	public void ToggleCharacterCreationScreen(bool value)
	{
		m_IsCreatingCharacter = value;
		bool supplementaryDataBoolean = value;
		Synchronizer.SendGameAction(GameActionType.ToggleCharacterCreationUI, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, PlayerID, 0, 0, supplementaryDataBoolean);
		if (!value)
		{
			CreatingCharacter = null;
			if (FFSNetwork.IsHost && !PlayerRegistry.AllPlayers.Any((NetworkPlayer a) => a.IsCreatingCharacter))
			{
				Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
			}
		}
	}

	public static void ProxyToggleCharacterCreationUI(GameAction action)
	{
		NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID == action.SupplementaryDataIDMin);
		if (!(networkPlayer != null))
		{
			return;
		}
		networkPlayer.m_IsCreatingCharacter = action.SupplementaryDataBoolean;
		if (!action.SupplementaryDataBoolean)
		{
			networkPlayer.CreatingCharacter = null;
			if (FFSNetwork.IsHost && !PlayerRegistry.AllPlayers.Any((NetworkPlayer a) => a.IsCreatingCharacter))
			{
				Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
			}
		}
	}

	public void UpdatePlayerProfileAvatar()
	{
		PlayerToken playerToken = (PlayerToken)base.entity.AttachToken;
		PlayerID = playerToken.PlayerID;
		if (!PlatformLayer.Instance.IsDelayedInit && PlatformLayer.Instance.IsConsole)
		{
			GetProfileAvatar(playerToken.PlatformNetworkAccountPlayerID, this);
		}
		else
		{
			GetProfileAvatar(playerToken.PlatformPlayerID, this);
		}
	}

	public override void Attached()
	{
		PlayerToken playerToken = (PlayerToken)base.entity.AttachToken;
		PlayerID = playerToken.PlayerID;
		PlatformName = playerToken.PlatformName;
		PlatformPlayerId = playerToken.PlatformPlayerID;
		PlatformNetworkAccountPlayerID = playerToken.PlatformNetworkAccountPlayerID;
		HasDesynched = false;
		Username = playerToken.Username;
		RecentPlayerKey = playerToken.RecentPlayerKey;
		GetProfileAvatar(PlatformNetworkAccountPlayerID, this);
		Console.LogCoreInfo(Username + " (PlayerID: " + PlayerID + ") JOINED the session. PlatformName: " + PlatformName, customFlag: true);
		base.state.AddCallback("LatestProcessedActionID", OnLatestProcessedActionIDChanged);
		if (FFSNetwork.IsClient && !base.entity.IsControllerOrOwner)
		{
			MaskBadWordsInUsername(OnInitialized);
		}
		else if (FFSNetwork.IsHost)
		{
			PlayersACKedMyLatestControllableState = new List<NetworkPlayer>();
		}
	}

	public override void Detached()
	{
		Console.LogCoreInfo(Username + " (PlayerID: " + PlayerID + ") LEFT the session.", customFlag: true);
		if (base.entity.DetachToken is ConnectionErrorToken connectionErrorToken)
		{
			ConnectionErrorCode errorCode = (ConnectionErrorCode)connectionErrorToken.ErrorCode;
			if (errorCode == ConnectionErrorCode.KickedByHost && !(PlayerRegistry.MyPlayer != this))
			{
				FFSNetwork.IsKickedState = true;
				if (PlatformLayer.Message.IsSystemMessagesSupported())
				{
					string translation = LocalizationManager.GetTranslation($"Consoles/GUI_MULTIPLAYER_CONNECTION_FAILED_{errorCode}");
					PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, translation, null);
				}
				else
				{
					UIMultiplayerNotifications.ShowPlayerKicked();
				}
			}
		}
		if (PlayerRegistry.MyPlayer == this && FFSNetwork.Manager.AutoShutdownUponMyPlayerRemoved)
		{
			FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
			if (global::Singleton<UIResultsManager>.Instance != null && global::Singleton<UIResultsManager>.Instance.IsShown)
			{
				global::Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
			}
		}
		if (FFSNetwork.IsOnline)
		{
			foreach (NetworkControllable item in MyControllables.ToList())
			{
				if (FFSNetwork.Manager.AutoAssignControllablesToHost)
				{
					PlayerRegistry.HostPlayer.AssignControllable(item, releaseFirst: true, syncAssignmentToClientsIfServer: false);
				}
				else
				{
					ReleaseControllable(item);
				}
			}
			if (FFSNetwork.Manager.AutoAssignControllablesToHost)
			{
				PlayerRegistry.HostPlayer.AssignedSlots.AddRange(AssignedSlots);
				AssignedSlots.Clear();
				if (global::Singleton<UIMapMultiplayerController>.IsInitialized)
				{
					global::Singleton<UIMapMultiplayerController>.Instance.ValidateCharactersAssignedToSlots();
				}
			}
			if (CreatingCharacter != null)
			{
				CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.CheckCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterName == CreatingCharacter.CharacterName);
				if (cMapCharacter != null && cMapCharacter.PersonalQuest == null)
				{
					AdventureState.MapState.MapParty.RemoveCharacterFromCharactersList(cMapCharacter);
					if (AdventureState.MapState.MapParty.SelectedCharactersArray.Contains(cMapCharacter))
					{
						AdventureState.MapState.MapParty.RemoveSelectedCharacter(cMapCharacter);
					}
				}
				CreatingCharacter = null;
				if (IsCreatingCharacter)
				{
					ToggleCharacterCreationScreen(value: false);
				}
			}
			PlayerRegistry.AllPlayers.Remove(this);
		}
		PlayerRegistry.OnPlayerLeft?.Invoke(this);
	}

	public override void ControlGained()
	{
		Console.LogInfo("Received control of NetworkPlayer (PlayerID: " + PlayerID + ").");
		if (FFSNetwork.IsClient)
		{
			NetStats = new NetworkStatsClient(PlayerRegistry.MyConnection);
		}
		PlayerRegistry.MyPlayer = this;
		OnInitialized();
	}

	public bool AssignControllable(int controllableID, bool releaseFirst = false, bool syncAssignmentToClientsIfServer = true)
	{
		return AssignControllable(ControllableRegistry.GetControllable(controllableID), releaseFirst, syncAssignmentToClientsIfServer);
	}

	public bool AssignControllable(NetworkControllable controllable, bool releaseFirst = false, bool syncAssignmentToClientsIfServer = true)
	{
		if (controllable != null)
		{
			NetworkPlayer controller = controllable.Controller;
			if (this == controller)
			{
				Console.LogWarning("Trying to ASSIGN the same controllable (ID: " + controllable.ID + ") to " + Username + "(ID: " + PlayerID + ").");
				return false;
			}
			if (releaseFirst)
			{
				controllable.Release();
			}
			controllable.OnControlAssigned(this);
			MyControllables.Add(controllable);
			ControllableRegistry.OnControllerChanged?.Invoke(controllable, controller, this);
			if (global::Singleton<UIMapMultiplayerController>.Instance != null)
			{
				global::Singleton<UIMapMultiplayerController>.Instance.ValidateCharactersAssignedToSlots();
			}
			Console.LogInfo("Controllable (ID: " + controllable.ID + ") ASSIGNED to " + Username + " (ID: " + PlayerID + ").");
			if (FFSNetwork.IsHost)
			{
				if (IsClient)
				{
					controllable.NetworkEntity?.AssignControl(NetStats.Connection);
				}
				else
				{
					controllable.NetworkEntity?.TakeControl();
				}
				if (syncAssignmentToClientsIfServer)
				{
					ControllableAssignmentEvent controllableAssignmentEvent = ControllableAssignmentEvent.Create(GlobalTargets.AllClients, ReliabilityModes.ReliableOrdered);
					controllableAssignmentEvent.PlayerID = PlayerID;
					controllableAssignmentEvent.ControllableID = controllable.ID;
					controllableAssignmentEvent.ReleaseFirst = releaseFirst;
					controllableAssignmentEvent.Send();
				}
			}
			return true;
		}
		Console.LogWarning("Error assigning controllable to " + Username + " (ID: " + PlayerID + "). The controllable returns null.");
		return false;
	}

	public void ReleaseControllable(int controllableID, bool syncToClientsIfServer = false)
	{
		ReleaseControllable(ControllableRegistry.GetControllable(controllableID), syncToClientsIfServer);
	}

	public void ReleaseControllable(NetworkControllable controllable, bool syncToClientsIfServer = false)
	{
		if (controllable != null)
		{
			if (MyControllables.Contains(controllable))
			{
				controllable.OnControlReleased();
				MyControllables.Remove(controllable);
				Console.LogInfo("Controllable (ID: " + controllable.ID + ") RELEASED from " + Username + " (ID: " + PlayerID + ").");
				if (FFSNetwork.IsHost && syncToClientsIfServer)
				{
					ControllableReleaseEvent controllableReleaseEvent = ControllableReleaseEvent.Create(GlobalTargets.AllClients, ReliabilityModes.ReliableOrdered);
					controllableReleaseEvent.PlayerID = PlayerID;
					controllableReleaseEvent.ControllableID = controllable.ID;
					controllableReleaseEvent.Send();
				}
			}
			else
			{
				Console.LogError("ERROR_MULTIPLAYER_00033", "Error releasing a controllable (ID: " + controllable.ID + ") from " + Username + " (ID: " + PlayerID + "). The player did not have control over it.");
			}
		}
		else
		{
			Console.LogError("ERROR_MULTIPLAYER_00034", "Error releasing a controllable from user.", "User:" + Username + " (ID: " + PlayerID + "). The controllable returns null.");
		}
	}

	public bool HasControlOver<TControllableEnumType>(string controllableEnumMemberName) where TControllableEnumType : struct, IConvertible
	{
		if (Enum.TryParse<TControllableEnumType>(controllableEnumMemberName, out var result) && HasControlOver(Convert.ToInt32(result)))
		{
			return true;
		}
		return false;
	}

	public bool HasControlOver(int controllableID)
	{
		return MyControllables.FirstOrDefault((NetworkControllable x) => x.ID == controllableID) != null;
	}

	public void PrintControllables()
	{
		foreach (NetworkControllable myControllable in MyControllables)
		{
			if (myControllable != null)
			{
				Console.LogDebug(Username + " (PlayerID: " + PlayerID + "): " + ((myControllable.ControllableObject != null) ? myControllable.ControllableObject.GetName() : "Uninitialized controllable") + " - " + (myControllable.IsParticipant ? "Participant" : "Benched") + " - " + (myControllable.IsAlive ? "Alive" : "Dead"));
			}
			else
			{
				Console.LogDebug(Username + " (PlayerID: " + PlayerID + "): Null controllable");
			}
		}
	}

	public void MaskBadWordsInUsername(Action callback)
	{
		switch (PlatformName)
		{
		case "Standalone":
		case "Steam":
		case "GoGGalaxy":
		case "EpicGamesStore":
			PlayerRegistry.UsersUnderProfanityCheck++;
			Username.GetCensoredStringAsync(delegate(string censoredUsername)
			{
				Username = censoredUsername;
				PlayerRegistry.UsersUnderProfanityCheck--;
				callback?.Invoke();
			});
			break;
		default:
			callback?.Invoke();
			break;
		}
	}

	public void OnInitialized()
	{
		PlayerRegistry.AllPlayers.Add(this);
		PlayerRegistry.OnPlayerJoined?.Invoke(this);
		if (!IsClient)
		{
			m_AssignedSlots.Add(0);
			m_AssignedSlots.Add(1);
			m_AssignedSlots.Add(2);
			m_AssignedSlots.Add(3);
		}
		if (PlatformName == PlatformLayer.Instance.PlatformID && PlatformPlayerId != PlatformLayer.UserData.PlatformPlayerID)
		{
			RecentPlayer item = new RecentPlayer(PlatformPlayerId, RecentPlayerKey, Username);
			PlatformLayer.Platform.PlatformSocial.UpdateRecentPlayers(new List<RecentPlayer> { item });
		}
		Console.LogInfo($"NetworkPlayer INITIALIZED (Username: {Username}, PlayerID: {PlayerID}, PlatformName: {PlatformName}, PlatformPlayerId: {PlatformPlayerId}).");
	}

	private void OnLatestProcessedActionIDChanged()
	{
		if (!(base.entity == null) && base.entity.IsAttached)
		{
			if (PlayerRegistry.MyPlayer == this)
			{
				Console.LogInfo("LatestProcessedActionID changed to: " + base.state.LatestProcessedActionID);
			}
			else if (!IsClient)
			{
				Console.LogInfo("HOST: LatestProcessedActionID changed to: " + base.state.LatestProcessedActionID);
			}
		}
	}

	private static void GetProfileAvatar(string platformPlayerID, NetworkPlayer player)
	{
		PlatformLayer.UserData.GetAvatarForNetworkPlayer(player, platformPlayerID);
	}
}
