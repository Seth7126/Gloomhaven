using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

public static class UIMultiplayerNotifications
{
	private const string NOTIFICATION_TITLE = "GUI_MULTIPLAYER";

	private const string NOTIFICATION_TAG = "MULTIPLAYER";

	private const string ID_SELECTED_OTHER_PLAYER_CARD = "SelectedOtherPlayerCard";

	private const string ID_STARTED_SESSION = "StartedSession";

	private const string ID_SHARE_CODE = "ShareCode";

	private const string ID_WAIT_PLAYERS_JOIN = "WaitingPlayersJoin";

	private const string ID_WAIT_PLAYERS_JOINING = "WaitingPlayersJoining";

	private const string ID_GENERATE_CODE = "GenerateCode";

	private const string ID_WAIT_PLAYERS_CHAR_ASSIGNED = "WaitingPlayersCharacterAssigned";

	private const string ID_SELECTED_QUEST = "SelectedQuest";

	private const string ID_CANCEL_SELECTED_CARDS = "CancelSelectedCards";

	private const string ID_WAIT_MY_SLOTS_ASSIGNED = "WaitingAllMySlotsAssigned";

	private const string ID_CITY_EVENT = "CityEvent";

	private const string ID_TOWN_RECORDS = "TownRecords";

	public static void ShowPlayerKicked()
	{
		string translation = LocalizationManager.GetTranslation("Consoles/GUI_MULTIPLAYER_CONNECTION_FAILED_KickedByHost");
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", translation, UIInfoTools.Instance.disconnectedPlayerNotificationDuration);
	}

	public static void ShowPlayerReported(string reporter, string reported)
	{
		string translation = LocalizationManager.GetTranslation("Consoles/GUI_MULTIPLAYER_CONNECTION_FAILED_Reported");
		translation = translation.Replace("{reporter}", reporter);
		translation = translation.Replace("{reported}", reported);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", translation, UIInfoTools.Instance.disconnectedPlayerNotificationDuration);
	}

	public static void ShowFailedConnectToSession()
	{
		string translation = LocalizationManager.GetTranslation("ERROR_MULTIPLAYER_00017");
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", translation, UIInfoTools.Instance.disconnectedPlayerNotificationDuration);
	}

	public static void ShowFailedHostSession()
	{
		string translation = LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HOST_DISCONNECTED_CONFIRMATION_TITLE");
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", translation, UIInfoTools.Instance.disconnectedPlayerNotificationDuration);
	}

	public static void ShowPlayerDisconnected(NetworkPlayer player, bool showEndSessionOption)
	{
		if (showEndSessionOption)
		{
			Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_DISCONNECTED_END_SESSION"), player.UserNameWithPlatformIcon()), delegate
			{
				FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
				if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
				{
					Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
				}
			}, "GUI_MULTIPLAYER_END_SESSION", UIInfoTools.Instance.disconnectedPlayerNotificationDuration, null, "MULTIPLAYER");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		List<NetworkControllable> myParticipatingControllables = player.MyParticipatingControllables;
		int count = myParticipatingControllables.Count;
		for (int num = 0; num < count; num++)
		{
			if (num > 0)
			{
				stringBuilder.Append((num == count - 1) ? LocalizationManager.GetTranslation("AND") : ", ");
			}
			if (AdventureState.MapState.IsCampaign)
			{
				CMapCharacter mapCharacterWithCharacterNameHash = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(myParticipatingControllables[num].ID);
				stringBuilder.Append(LocalizationManager.GetTranslation(mapCharacterWithCharacterNameHash.CharacterYMLData.LocKey.ToString()));
			}
			else
			{
				stringBuilder.Append(LocalizationManager.GetTranslation(((ECharacter)myParticipatingControllables[num].ID/*cast due to .constrained prefix*/).ToString()));
			}
		}
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_DISCONNECTED"), player.UserNameWithPlatformIcon(), PlayerRegistry.HostPlayer.UserNameWithPlatformIcon(), stringBuilder);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", text, UIInfoTools.Instance.disconnectedPlayerNotificationDuration, null, "MULTIPLAYER");
	}

	public static void ShowPlayerJoined(NetworkPlayer player)
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_JOINED"), player.UserNameWithPlatformIcon());
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", text, UIInfoTools.Instance.defaultNotificationDuration, null, "MULTIPLAYER");
	}

	public static void ShowSelectedOtherPlayerCard()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_WARNING_SELECT_CARDS_OTHER_PLAYER") + "</color>", UIInfoTools.Instance.selectedOtherPlayerCardNotificationDuration, "SelectedOtherPlayerCard", "MULTIPLAYER");
	}

	public static void ShowCancelReadiedCards()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_WARNING_CANCEL_SELECTED_CARDS") + "</color>", UIInfoTools.Instance.defaultNotificationDuration, "CancelSelectedCards", "MULTIPLAYER");
	}

	public static void HideCancelReadiedCards()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("CancelSelectedCards");
	}

	public static void ShowStartedSession()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_MULTIPLAYER_STARTED"), UIInfoTools.Instance.defaultNotificationDuration, "StartedSession", "MULTIPLAYER");
	}

	public static void ShowShareInviteCode()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_SHARE_INVITE_CODE"), UIInfoTools.Instance.defaultNotificationDuration, "ShareCode", "MULTIPLAYER");
	}

	public static void ShowGenerateInviteCode()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_GENERATE_INVITE_CODE"), UIInfoTools.Instance.defaultNotificationDuration, "GenerateCode", "MULTIPLAYER");
	}

	public static void ShowWaitingPlayersJoin()
	{
		HideWaitingPlayersJoin();
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("WaitingPlayersJoin", "GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_WAITING_PLAYERS_JOIN"), delegate
		{
			FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
			if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
			{
				Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
			}
		}, "GUI_MULTIPLAYER_END_SESSION", "MULTIPLAYER");
	}

	public static void HideWaitingPlayersJoin()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("WaitingPlayersJoin");
	}

	public static void ShowWaitingPlayersJoining()
	{
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("WaitingPlayersJoining", "GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_WAITING_PLAYERS_JOINING"), "MULTIPLAYER");
	}

	public static void HideWaitingPlayersJoining()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("WaitingPlayersJoining");
	}

	public static void HideAllMultiplayerNotification(bool instant = false)
	{
		Singleton<UINotificationManager>.Instance.HideNotificationsByTag("MULTIPLAYER", instant, includeTemporary: false);
	}

	public static void ShowPlayerControlsCharacter(NetworkPlayer player, string controlled)
	{
		string id = controlled + "_CONTROL";
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_WON_CONTROL"), player.UserNameWithPlatformIcon(), controlled);
		Singleton<UINotificationManager>.Instance.HideNotification(id);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", text, UIInfoTools.Instance.defaultNotificationDuration, id, "MULTIPLAYER");
	}

	public static void ShowWaitingPlayersCharacterAssigned(List<NetworkPlayer> players)
	{
		HideWaitingPlayersCharacterAssigned();
		string arg = string.Join("\n", players.Select((NetworkPlayer s) => s.UserNameWithPlatformIcon()));
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("WaitingPlayersCharacterAssigned", "GUI_MULTIPLAYER", string.Format("<color=#{1}>{0}\n{2}", LocalizationManager.GetTranslation("GUI_MULTIPLAYER_WARNING_TEXT_WaitingForCharAssignment"), UIInfoTools.Instance.warningColor.ToHex(), arg), "MULTIPLAYER");
	}

	public static void HideWaitingPlayersCharacterAssigned()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("WaitingPlayersCharacterAssigned");
	}

	public static void ShowSelectedQuest()
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_QUEST_SELECTED"), PlayerRegistry.HostPlayer.UserNameWithPlatformIcon());
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_NOTIFICATION_QUEST_SELECTED_TITLE", text, UIInfoTools.Instance.defaultNotificationDuration, "SelectedQuest", "MULTIPLAYER");
	}

	public static void ShowSelectedCityEvent()
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CITY_EVENT_SELECTED"), PlayerRegistry.HostPlayer.UserNameWithPlatformIcon());
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", text, UIInfoTools.Instance.defaultNotificationDuration, "CityEvent", "MULTIPLAYER");
	}

	public static void HideSelectedCityEvent()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("CityEvent");
	}

	public static void ShowSelectedTownRecords()
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_TOWN_RECORDS_SELECTED"), PlayerRegistry.HostPlayer.UserNameWithPlatformIcon());
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", text, UIInfoTools.Instance.defaultNotificationDuration, "TownRecords", "MULTIPLAYER");
	}

	public static void HideSelectedTownRecords()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("TownRecords");
	}

	public static void ShowWaitingAssignAllMySlots()
	{
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("WaitingAllMySlotsAssigned", "GUI_MULTIPLAYER", string.Format("<color=#{1}>{0}", string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_ASSIGN_ALL_MY_SLOTS"), PlayerRegistry.MyPlayer.UserNameWithPlatformIcon()), UIInfoTools.Instance.warningColor.ToHex()), "MULTIPLAYER");
	}

	public static void HideWaitingAssignAllMySlots()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("WaitingAllMySlotsAssigned");
	}
}
