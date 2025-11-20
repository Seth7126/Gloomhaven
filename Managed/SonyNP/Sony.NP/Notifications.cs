namespace Sony.NP;

internal class Notifications
{
	internal static ResponseBase CreateNotificationResponse(FunctionTypes notificationType)
	{
		ResponseBase result = null;
		switch (notificationType)
		{
		case FunctionTypes.NotificationDialogOpened:
		case FunctionTypes.NotificationDialogClosed:
		case FunctionTypes.NotificationAborted:
			result = new Core.EmptyResponse();
			break;
		case FunctionTypes.NotificationRefreshRoom:
			result = new Matching.RefreshRoomResponse();
			break;
		case FunctionTypes.NotificationNewRoomMessage:
			result = new Matching.NewRoomMessageResponse();
			break;
		case FunctionTypes.NotificationNewInGameMessage:
			result = new Messaging.NewInGameMessageResponse();
			break;
		case FunctionTypes.NotificationNewGameDataMessage:
			result = new Messaging.NewGameDataMessageResponse();
			break;
		case FunctionTypes.NotificationUserStateChange:
			result = new NpUtils.UserStateChangeResponse();
			break;
		case FunctionTypes.NotificationNetStateChange:
			result = new NetworkUtils.NetStateChangeResponse();
			break;
		case FunctionTypes.NotificationUpdateFriendsList:
			result = new Friends.FriendListUpdateResponse();
			break;
		case FunctionTypes.NotificationUpdateFriendPresence:
			result = new Presence.PresenceUpdateResponse();
			break;
		case FunctionTypes.NotificationUpdateBlockedUsersList:
			result = new Friends.BlocklistUpdateResponse();
			break;
		case FunctionTypes.NotificationNewInvitation:
			result = new Matching.InvitationReceivedResponse();
			break;
		case FunctionTypes.NotificationSessionInvitationEvent:
			result = new Matching.SessionInvitationEventResponse();
			break;
		case FunctionTypes.NotificationPlayTogetherHostEvent:
			result = new Matching.PlayTogetherHostEventResponse();
			break;
		case FunctionTypes.NotificationGameCustomDataEvent:
			result = new Messaging.GameCustomDataEventResponse();
			break;
		case FunctionTypes.NotificationLaunchAppEvent:
			result = new Main.LaunchAppEventResponse();
			break;
		}
		return result;
	}
}
