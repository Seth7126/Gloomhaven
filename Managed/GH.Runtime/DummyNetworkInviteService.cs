#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using Assets.Script.Misc;
using Epic.OnlineServices;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.Sessions;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

public class DummyNetworkInviteService : INetworkInviteService
{
	private List<IInvitePlayer> players;

	private EOSFriendsManager FriendsManager;

	private EOSSessionsManager SessionsManager;

	private static CallbackPromise sendInvitePromise;

	private IInvitePlayer InvitedPlayer;

	public DummyNetworkInviteService()
	{
		players = new List<IInvitePlayer>
		{
			new DummyInvitePlayer("Paula", null, isInvited: false, isOnline: false),
			new DummyInvitePlayer("Name", null, isInvited: true, isOnline: true)
		};
	}

	public void UpdatePlayers()
	{
		FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
		FriendsManager.GetCachedFriends(out var Friends);
		players = new List<IInvitePlayer>();
		foreach (FriendData value in Friends.Values)
		{
			players.Add(new DummyInvitePlayer(value.Name, null, isInvited: false, value.Presence.Status != Status.Offline, value.UserProductUserId, value.Presence.Application));
		}
	}

	public List<IInvitePlayer> GetPlayersToInvite()
	{
		UpdatePlayers();
		return players;
	}

	public ICallbackPromise SendInvite(IInvitePlayer player)
	{
		if (sendInvitePromise != null && sendInvitePromise.IsPending)
		{
			return sendInvitePromise;
		}
		InvitedPlayer = player;
		sendInvitePromise = new CallbackPromise();
		SessionsManager = EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>();
		Session session = new Session
		{
			Name = "Gloomhaven",
			MaxPlayers = 4u,
			PermissionLevel = OnlineSessionPermissionLevel.InviteOnly
		};
		SessionAttribute sessionAttribute = new SessionAttribute();
		sessionAttribute.Key = "PhotonKey";
		sessionAttribute.AsString = PlatformLayer.Networking.EpicCurrentSessionService.GetInviteCode();
		sessionAttribute.ValueType = AttributeType.String;
		sessionAttribute.Advertisement = SessionAttributeAdvertisementType.Advertise;
		session.Attributes.Add(sessionAttribute);
		SessionsManager.ModifySession(session, OnModifySessionCompleted);
		return sendInvitePromise;
	}

	private void OnModifySessionCompleted()
	{
		Action callback = OnSendInviteCallback;
		((DummyInvitePlayer)InvitedPlayer).IsInvited = true;
		Debug.Log("Invite send");
		SessionsManager.InviteToSession("Gloomhaven", InvitedPlayer.ProductUserID, callback);
	}

	public static void OnSendInviteCallback()
	{
		if (sendInvitePromise.IsPending)
		{
			sendInvitePromise.Resolve();
			Debug.Log("Session Matchmaking: invite to session sent successfully.");
		}
	}
}
