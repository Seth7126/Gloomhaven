using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Platforms;
using Platforms.Social;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class NetworkHeroAssignService : MonoBehaviour, INetworkHeroAssignService, INetworkSessionService
{
	private const string UgcDisabledLocalisationKey = "Consoles/UGC_DISABLED_START_SESSION_MESSAGE";

	public PlayersChangedEvent OnPlayerJoined
	{
		get
		{
			return PlayerRegistry.OnPlayerJoined;
		}
		set
		{
			PlayerRegistry.OnPlayerJoined = value;
		}
	}

	public PlayersChangedEvent OnPlayerLeft
	{
		get
		{
			return PlayerRegistry.OnPlayerLeft;
		}
		set
		{
			PlayerRegistry.OnPlayerLeft = value;
		}
	}

	public List<NetworkPlayer> GetAllPlayers()
	{
		return PlayerRegistry.AllPlayers;
	}

	public List<Tuple<string, NetworkPlayer>> GetCharacterAssignations()
	{
		List<Tuple<string, NetworkPlayer>> list = new List<Tuple<string, NetworkPlayer>>();
		foreach (NetworkControllable participatingControllable in ControllableRegistry.ParticipatingControllables)
		{
			FFSNet.Console.LogInfo("New controllable assignment - Controllable ID: " + participatingControllable.ID + ". ControllableObject: " + ((participatingControllable.ControllableObject != null) ? participatingControllable.ControllableObject.GetName() : "NULL") + ". Player ID: " + ((participatingControllable.Controller != null) ? participatingControllable.Controller.PlayerID.ToString() : "-"));
			string item = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterWithCharacterNameHash(participatingControllable.ID).CharacterID : CharacterClassManager.GetCharacterIDFromModelInstanceID(participatingControllable.ID));
			list.Add(new Tuple<string, NetworkPlayer>(item, (participatingControllable.Controller == null) ? PlayerRegistry.HostPlayer : participatingControllable.Controller));
		}
		int i;
		for (i = list.Count; i < 4; i++)
		{
			NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer it) => it.AssignedSlots.Contains(i));
			list.Add(new Tuple<string, NetworkPlayer>(i.ToString(), (networkPlayer == null) ? PlayerRegistry.HostPlayer : networkPlayer));
		}
		return list;
	}

	public void EndSession()
	{
		if (PlayerRegistry.MyPlayer != null && !PlayerRegistry.MyPlayer.IsClient)
		{
			foreach (NetworkPlayer item in PlayerRegistry.AllPlayers.Where((NetworkPlayer w) => w.IsClient))
			{
				foreach (NetworkControllable myControllable in item.MyControllables)
				{
					PlayerRegistry.MyPlayer.AssignControllable(myControllable.ID);
				}
			}
		}
		FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
		if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
		{
			Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
		}
	}

	public void AssignHeroToPlayer(NetworkPlayer networkPlayer, string characterID, string characterName, int slot)
	{
		bool isCampaign = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.IsCampaign;
		NetworkPlayer networkPlayer2 = null;
		networkPlayer2 = ((!isCampaign) ? ControllableRegistry.GetController(CharacterClassManager.GetModelInstanceIDFromCharacterID(characterID)) : ControllableRegistry.GetController(characterName.GetHashCode()));
		if (networkPlayer2.MyParticipatingControllables.Count == 1 && networkPlayer.MyParticipatingControllables.Count > 0)
		{
			string previousCharacterID = networkPlayer.MyParticipatingControllables[0].GetCharacter;
			networkPlayer2.AssignControllable(networkPlayer.MyParticipatingControllables[0].ID, releaseFirst: true);
			if (Singleton<APartyDisplayUI>.Instance != null && Singleton<APartyDisplayUI>.Instance is NewPartyDisplayUI newPartyDisplayUI)
			{
				NewPartyCharacterUI newPartyCharacterUI = newPartyDisplayUI.CharacterSlots.SingleOrDefault((NewPartyCharacterUI s) => s.Data != null && s.Data.CharacterID == previousCharacterID);
				if (newPartyCharacterUI != null)
				{
					int num = newPartyDisplayUI.CharacterSlots.IndexOf(newPartyCharacterUI);
					Synchronizer.SendSideAction(GameActionType.AssignSlot, null, canBeUnreliable: false, sendToHostOnly: false, 0, networkPlayer2.PlayerID, num);
					AssignSlotToPlayer(networkPlayer2, num);
				}
				else
				{
					Debug.LogError("Unable to find slot UI for character " + previousCharacterID);
				}
			}
			else
			{
				int num2 = AdventureState.MapState.MapParty.SelectedCharactersArray.FindIndex((CMapCharacter it) => it != null && it.CharacterID == previousCharacterID);
				Synchronizer.SendSideAction(GameActionType.AssignSlot, null, canBeUnreliable: false, sendToHostOnly: false, 0, networkPlayer2.PlayerID, num2);
				AssignSlotToPlayer(networkPlayer2, num2);
			}
		}
		networkPlayer.AssignControllable(isCampaign ? characterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterID), releaseFirst: true);
		AssignSlotToPlayer(networkPlayer, slot);
	}

	public void AssignSlotToPlayer(GameAction action)
	{
		Task.Run(async delegate
		{
			while (PlayerRegistry.IsProfanityCheckInProcess)
			{
				await Task.Delay(50);
			}
			UnityMainThreadDispatcher.Instance().Enqueue(delegate
			{
				AssignSlotToPlayer(PlayerRegistry.AllPlayers.Single((NetworkPlayer s) => s.PlayerID == action.DataInt), action.DataInt2);
			});
		});
	}

	public void AssignSlotToPlayer(NetworkPlayer player, int slot)
	{
		foreach (NetworkPlayer item in PlayerRegistry.AllPlayers.Where((NetworkPlayer w) => w != player))
		{
			if (item.AssignedSlots.Contains(slot))
			{
				item.AssignedSlots.Remove(slot);
				Singleton<UIMapMultiplayerController>.Instance?.UpdateSlotRemovedFromPlayer(item, slot);
			}
		}
		if (!player.AssignedSlots.Contains(slot))
		{
			player.AssignedSlots.Add(slot);
			Singleton<UIMapMultiplayerController>.Instance?.UpdateSlotAssignedToPlayer(player, slot);
		}
	}

	public bool IsOnline(NetworkPlayer player)
	{
		return !PlayerRegistry.IsJoining(player);
	}

	public void StartSession(UnityAction onHostStarted, UnityAction onHostEnded)
	{
		if (!FFSNetwork.IsOnline && !FFSNetwork.IsStartingUp)
		{
			PlatformLayer.Networking.GetCurrentUserPrivilegesAsync(OnGetCurrentUserUgcPrivilege, PrivilegePlatform.Xbox, Privilege.UserGeneratedContent);
		}
		void OnGetCurrentUserUgcPrivilege(OperationResult operationResult, bool isPrivilegeValid)
		{
			FFSNetwork.IsUGCEnabled = operationResult == OperationResult.Success && isPrivilegeValid;
			if (!FFSNetwork.IsUGCEnabled)
			{
				PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, LocalizationManager.GetTranslation("Consoles/UGC_DISABLED_START_SESSION_MESSAGE"), delegate
				{
					FFSNetwork.Manager.ToggleServer(onHostStarted, onHostEnded);
				});
			}
			else
			{
				FFSNetwork.Manager.ToggleServer(onHostStarted, onHostEnded);
			}
		}
	}

	public string GetInviteCode()
	{
		return FFSNetwork.Manager.SessionID;
	}

	public void GenerateInviteCode()
	{
	}

	public void RemoveClient(NetworkPlayer client, bool sendKickMessage = false)
	{
		PlayerRegistry.RemovePlayer(client, sendKickMessage);
	}

	public void ReportClient(NetworkPlayer client, Action onReportCallback)
	{
		if (client.PlatformName == PlatformLayer.Instance.PlatformID)
		{
			PlatformLayer.Platform.PlatformSocial.AddToBlocklist(client.PlatformNetworkAccountPlayerID, delegate
			{
				onReportCallback?.Invoke();
			});
		}
		else
		{
			onReportCallback?.Invoke();
		}
	}
}
