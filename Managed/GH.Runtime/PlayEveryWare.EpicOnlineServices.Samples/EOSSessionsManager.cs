#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.UI;

namespace PlayEveryWare.EpicOnlineServices.Samples;

public class EOSSessionsManager : IEOSSubManager
{
	private Dictionary<string, Session> CurrentSessions;

	private SessionSearch CurrentSearch;

	private string JoinPresenceSessionId = string.Empty;

	private ulong JoinUiEvent;

	private string KnownPresenceSessionId = string.Empty;

	private Dictionary<Session, SessionDetails> Invites;

	private Session CurrentInvite;

	private SessionDetails JoiningSessionDetails;

	private ulong JoinedSessionIndex;

	private const string JOINED_SESSION_NAME = "Session#";

	private const uint JOINED_SESSION_NAME_ROTATION_NUM = 9u;

	private const int JOINED_SESSION_NAME_ROTATION = 9;

	private const string BUCKET_ID = "SessionSample:Region";

	private const string EOS_SESSIONS_SEARCH_BUCKET_ID = "bucket";

	private const string EOS_SESSIONS_SEARCH_EMPTY_SERVERS_ONLY = "emptyonly";

	private const string EOS_SESSIONS_SEARCH_NONEMPTY_SERVERS_ONLY = "nonemptyonly";

	private const string EOS_SESSIONS_SEARCH_MINSLOTSAVAILABLE = "minslotsavailable";

	private Queue<Action> UIOnSessionCreated;

	private Queue<Action> UIOnSessionModified;

	private Queue<Action> UIOnJoinSession;

	private Queue<Action> UIOnInviteSession;

	private Queue<Action> UIOnLeaveSession;

	private Queue<Action> UIOnSessionSearchCompleted;

	private Action UIOnInviteReceived;

	public const ulong INVALID_NOTIFICATIONID = 0uL;

	public ulong SessionInviteNotificationHandle;

	public ulong SessionInviteAcceptedNotificationHandle;

	public ulong JoinGameNotificationHandle;

	public ulong SessionJoinGameNotificationHandle;

	private bool subscribtedToGameInvites;

	private bool userLoggedIn;

	public EOSSessionsManager()
	{
		UIOnSessionCreated = new Queue<Action>();
		UIOnSessionModified = new Queue<Action>();
		UIOnJoinSession = new Queue<Action>();
		UIOnLeaveSession = new Queue<Action>();
		UIOnInviteSession = new Queue<Action>();
		UIOnSessionSearchCompleted = new Queue<Action>();
		CurrentSessions = new Dictionary<string, Session>();
		CurrentSearch = new SessionSearch();
		Invites = new Dictionary<Session, SessionDetails>();
		CurrentInvite = null;
	}

	public Dictionary<Session, SessionDetails> GetInvites()
	{
		return Invites;
	}

	public Session GetCurrentInvite()
	{
		return CurrentInvite;
	}

	public SessionSearch GetCurrentSearch()
	{
		return CurrentSearch;
	}

	public Dictionary<string, Session> GetCurrentSessions()
	{
		return CurrentSessions;
	}

	public void SubscribteToGameInvites(Action callback = null)
	{
		if (subscribtedToGameInvites)
		{
			Debug.LogWarning("Session Matchmaking (SubscribteToGameInvites): Already subscribed.");
			return;
		}
		SessionsInterface eOSSessionsInterface = EOSManager.Instance.GetEOSSessionsInterface();
		PresenceInterface eOSPresenceInterface = EOSManager.Instance.GetEOSPresenceInterface();
		AddNotifySessionInviteReceivedOptions options = default(AddNotifySessionInviteReceivedOptions);
		AddNotifySessionInviteAcceptedOptions options2 = default(AddNotifySessionInviteAcceptedOptions);
		AddNotifyJoinSessionAcceptedOptions options3 = default(AddNotifyJoinSessionAcceptedOptions);
		AddNotifyJoinGameAcceptedOptions options4 = default(AddNotifyJoinGameAcceptedOptions);
		if (callback != null)
		{
			UIOnInviteReceived = callback;
		}
		SessionInviteNotificationHandle = eOSSessionsInterface.AddNotifySessionInviteReceived(ref options, null, OnSessionInviteReceivedListener);
		SessionInviteAcceptedNotificationHandle = eOSSessionsInterface.AddNotifySessionInviteAccepted(ref options2, null, OnSessionInviteAcceptedListener);
		JoinGameNotificationHandle = eOSPresenceInterface.AddNotifyJoinGameAccepted(ref options4, null, OnJoinGameAcceptedListener);
		SessionJoinGameNotificationHandle = eOSSessionsInterface.AddNotifyJoinSessionAccepted(ref options3, null, OnJoinSessionAcceptedListener);
		subscribtedToGameInvites = true;
	}

	public void UnsubscribeFromGameInvites()
	{
		if (!subscribtedToGameInvites)
		{
			Debug.LogWarning("Session Matchmaking (UnsubscribeFromGameInvites): Not subscribed yet.");
			return;
		}
		SessionsInterface sessionsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
		PresenceInterface presenceInterface = EOSManager.Instance.GetEOSPlatformInterface().GetPresenceInterface();
		if (SessionInviteNotificationHandle != 0L)
		{
			sessionsInterface.RemoveNotifySessionInviteReceived(SessionInviteNotificationHandle);
			SessionInviteNotificationHandle = 0uL;
		}
		if (SessionInviteAcceptedNotificationHandle != 0L)
		{
			sessionsInterface.RemoveNotifySessionInviteAccepted(SessionInviteAcceptedNotificationHandle);
			SessionInviteAcceptedNotificationHandle = 0uL;
		}
		if (JoinGameNotificationHandle != 0L)
		{
			presenceInterface.RemoveNotifyJoinGameAccepted(JoinGameNotificationHandle);
			JoinGameNotificationHandle = 0uL;
		}
		if (SessionJoinGameNotificationHandle != 0L)
		{
			sessionsInterface.RemoveNotifyJoinSessionAccepted(SessionJoinGameNotificationHandle);
			SessionJoinGameNotificationHandle = 0uL;
		}
		subscribtedToGameInvites = false;
	}

	private void OnShutDown()
	{
		DestroyAllSessions();
		UnsubscribeFromGameInvites();
	}

	public bool Update()
	{
		bool result = false;
		foreach (KeyValuePair<string, Session> currentSession in CurrentSessions)
		{
			if (string.IsNullOrEmpty(currentSession.Key) || !currentSession.Value.IsValid())
			{
				continue;
			}
			Session value = currentSession.Value;
			if (value.UpdateInProgress || !(value.ActiveSession != null))
			{
				continue;
			}
			EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
			ActiveSessionCopyInfoOptions options = default(ActiveSessionCopyInfoOptions);
			ActiveSessionInfo? outActiveSessionInfo;
			Result result2 = value.ActiveSession.CopyInfo(ref options, out outActiveSessionInfo);
			if (result2 == Result.Success)
			{
				if (outActiveSessionInfo.HasValue && value.SessionState != outActiveSessionInfo?.State)
				{
					value.SessionState = (outActiveSessionInfo?.State).Value;
					result = true;
				}
			}
			else
			{
				Debug.LogErrorFormat("Session Matchmaking: ActiveSessionCopyInfo failed. Errors code: {0}", result2);
			}
		}
		return result;
	}

	public void OnLoggedIn()
	{
		if (userLoggedIn)
		{
			Debug.LogWarning("Session Matchmaking (OnLoggedIn): Already logged in.");
			return;
		}
		SubscribteToGameInvites();
		CurrentInvite = null;
		SetJoininfo("");
		userLoggedIn = true;
	}

	public void OnLoggedOut()
	{
		if (!userLoggedIn)
		{
			Debug.LogWarning("Session Matchmaking (OnLoggedOut): Not logged in.");
			return;
		}
		UnsubscribeFromGameInvites();
		LeaveAllSessions();
		SetJoininfo("", onLoggingOut: true);
		CurrentSearch.Release();
		CurrentSessions.Clear();
		Invites.Clear();
		CurrentInvite = null;
		JoiningSessionDetails = null;
		userLoggedIn = false;
	}

	private void LeaveAllSessions()
	{
		foreach (KeyValuePair<string, Session> currentSession in GetCurrentSessions())
		{
			DestroySession(currentSession.Key);
		}
	}

	public bool CreateSession(Session session, Action callback = null)
	{
		if (session == null)
		{
			Debug.LogErrorFormat("Session Matchmaking: parameter 'session' cannot be null!");
			return false;
		}
		SessionsInterface sessionsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
		if (sessionsInterface == null)
		{
			Debug.LogErrorFormat("Session Matchmaking: can't get sessions interface.");
			return false;
		}
		CreateSessionModificationOptions options = new CreateSessionModificationOptions
		{
			BucketId = "SessionSample:Region",
			MaxPlayers = session.MaxPlayers,
			SessionName = session.Name,
			LocalUserId = EOSManager.Instance.GetProductUserId(),
			PresenceEnabled = session.PresenceSession
		};
		Result result = sessionsInterface.CreateSessionModification(ref options, out var outSessionModificationHandle);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking: could not create session modification. Error code: {0}", result);
			return false;
		}
		SessionModificationSetPermissionLevelOptions options2 = new SessionModificationSetPermissionLevelOptions
		{
			PermissionLevel = session.PermissionLevel
		};
		result = outSessionModificationHandle.SetPermissionLevel(ref options2);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking: failed to set permissions. Error code: {0}", result);
			outSessionModificationHandle.Release();
			return false;
		}
		SessionModificationSetJoinInProgressAllowedOptions options3 = new SessionModificationSetJoinInProgressAllowedOptions
		{
			AllowJoinInProgress = session.AllowJoinInProgress
		};
		result = outSessionModificationHandle.SetJoinInProgressAllowed(ref options3);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking: failed to set 'join in progress allowed' flag. Error code: {0}", result);
			outSessionModificationHandle.Release();
			return false;
		}
		SessionModificationSetInvitesAllowedOptions options4 = new SessionModificationSetInvitesAllowedOptions
		{
			InvitesAllowed = session.InvitesAllowed
		};
		result = outSessionModificationHandle.SetInvitesAllowed(ref options4);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking: failed to set invites allowed. Error code: {0}", result);
			outSessionModificationHandle.Release();
			return false;
		}
		AttributeData value = new AttributeData
		{
			Key = "bucket",
			Value = new AttributeDataValue
			{
				AsUtf8 = "SessionSample:Region"
			}
		};
		SessionModificationAddAttributeOptions options5 = new SessionModificationAddAttributeOptions
		{
			SessionAttribute = value
		};
		result = outSessionModificationHandle.AddAttribute(ref options5);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking: failed to set a bucket id attribute. Error code: {0}", result);
			outSessionModificationHandle.Release();
			return false;
		}
		foreach (SessionAttribute attribute in session.Attributes)
		{
			value.Key = attribute.Key;
			switch (attribute.ValueType)
			{
			case AttributeType.Boolean:
				value.Value = attribute.AsBool.Value;
				break;
			case AttributeType.Double:
				value.Value = attribute.AsDouble.Value;
				break;
			case AttributeType.Int64:
				value.Value = attribute.AsInt64.Value;
				break;
			case AttributeType.String:
				value.Value = attribute.AsString;
				break;
			}
			options5.AdvertisementType = attribute.Advertisement;
			options5.SessionAttribute = value;
			result = outSessionModificationHandle.AddAttribute(ref options5);
			if (result != Result.Success)
			{
				Debug.LogErrorFormat("Session Matchmaking: failed to set an attribute: {0}. Error code: {1}", attribute.Key, result);
				outSessionModificationHandle.Release();
				return false;
			}
		}
		if (callback != null)
		{
			UIOnSessionCreated.Enqueue(callback);
		}
		UpdateSessionOptions options6 = new UpdateSessionOptions
		{
			SessionModificationHandle = outSessionModificationHandle
		};
		sessionsInterface.UpdateSession(ref options6, null, OnUpdateSessionCompleteCallback_ForCreate);
		outSessionModificationHandle.Release();
		if (CurrentSessions.ContainsKey(session.Name))
		{
			CurrentSessions[session.Name] = session;
		}
		else
		{
			CurrentSessions.Add(session.Name, session);
		}
		CurrentSessions[session.Name].UpdateInProgress = true;
		return true;
	}

	public void DestroySession(string name)
	{
		SessionsInterface sessionsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
		DestroySessionOptions options = new DestroySessionOptions
		{
			SessionName = name
		};
		sessionsInterface.DestroySession(ref options, name, OnDestroySessionCompleteCallback);
	}

	public void DestroyAllSessions()
	{
		SetJoininfo("");
		foreach (KeyValuePair<string, Session> currentSession in CurrentSessions)
		{
			if (!currentSession.Key.Contains("Session#"))
			{
				DestroySession(currentSession.Key);
			}
		}
	}

	public bool HasActiveLocalSessions()
	{
		foreach (KeyValuePair<string, Session> currentSession in CurrentSessions)
		{
			if (currentSession.Key.Contains("Session#"))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPresenceSession()
	{
		if (KnownPresenceSessionId.Length > 0)
		{
			if (CurrentSessions.ContainsKey(KnownPresenceSessionId))
			{
				return true;
			}
			KnownPresenceSessionId = string.Empty;
		}
		ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
		if (!productUserId.IsValid())
		{
			return false;
		}
		CopySessionHandleForPresenceOptions options = new CopySessionHandleForPresenceOptions
		{
			LocalUserId = productUserId
		};
		if (EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().CopySessionHandleForPresence(ref options, out var outSessionHandle) != Result.Success)
		{
			return false;
		}
		if (outSessionHandle == null)
		{
			return false;
		}
		SessionDetailsCopyInfoOptions options2 = default(SessionDetailsCopyInfoOptions);
		if (outSessionHandle.CopyInfo(ref options2, out var outSessionInfo) != Result.Success)
		{
			return false;
		}
		KnownPresenceSessionId = outSessionInfo?.SessionId;
		return true;
	}

	public bool IsPresenceSession(string id)
	{
		if (HasPresenceSession())
		{
			return id.Equals(KnownPresenceSessionId, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public Session GetSession(string name)
	{
		if (CurrentSessions.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public void StartSession(string name)
	{
		if (CurrentSessions.TryGetValue(name, out var _))
		{
			StartSessionOptions options = new StartSessionOptions
			{
				SessionName = name
			};
			EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().StartSession(ref options, name, OnStartSessionCompleteCallBack);
		}
		else
		{
			Debug.LogErrorFormat("Session Matchmaking: can't start session: no active session with specified name.");
		}
	}

	public void EndSession(string name)
	{
		if (!CurrentSessions.TryGetValue(name, out var _))
		{
			Debug.LogErrorFormat("Session Matchmaking: can't end session: no active session with specified name: {0}", name);
		}
		else
		{
			EndSessionOptions options = new EndSessionOptions
			{
				SessionName = name
			};
			EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().EndSession(ref options, name, OnEndSessionCompleteCallback);
		}
	}

	public void Register(string sessionName, ProductUserId friendId)
	{
		RegisterPlayersOptions options = new RegisterPlayersOptions
		{
			SessionName = sessionName,
			PlayersToRegister = new ProductUserId[1] { friendId }
		};
		EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().RegisterPlayers(ref options, null, OnRegisterCompleteCallback);
	}

	public void UnRegister(string sessionName, ProductUserId friendId)
	{
		UnregisterPlayersOptions options = new UnregisterPlayersOptions
		{
			SessionName = sessionName,
			PlayersToUnregister = new ProductUserId[1] { friendId }
		};
		EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().UnregisterPlayers(ref options, null, OnUnregisterCompleteCallback);
	}

	public void InviteToSession(string sessionName, ProductUserId friendId, Action callback = null)
	{
		if (!friendId.IsValid())
		{
			Debug.LogError("Session Matchmaking - InviteToSession: friend's product user id is invalid!");
			return;
		}
		ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
		if (!productUserId.IsValid())
		{
			Debug.LogError("Session Matchmaking - InviteToSession: current user's product user id is invalid!");
			return;
		}
		SendInviteOptions options = new SendInviteOptions
		{
			LocalUserId = productUserId,
			TargetUserId = friendId,
			SessionName = sessionName
		};
		if (callback != null)
		{
			UIOnInviteSession.Enqueue(callback);
		}
		EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().SendInvite(ref options, null, OnSendInviteCompleteCallback);
	}

	public void SetInviteSession(Session session, SessionDetails sessionDetails)
	{
		Invites.Add(session, sessionDetails);
		if (CurrentInvite != null)
		{
			PopLobbyInvite();
		}
		else
		{
			CurrentInvite = session;
		}
	}

	public void Search(List<SessionAttribute> attributes)
	{
		CurrentSearch.Release();
		CreateSessionSearchOptions options = new CreateSessionSearchOptions
		{
			MaxSearchResults = 10u
		};
		Result result = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().CreateSessionSearch(ref options, out var outSessionSearchHandle);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking: failed to create session search. Error code: {0}", result);
			return;
		}
		CurrentSearch.SetNewSearch(outSessionSearchHandle);
		AttributeData value = new AttributeData
		{
			Key = "bucket",
			Value = new AttributeDataValue
			{
				AsUtf8 = "SessionSample:Region"
			}
		};
		SessionSearchSetParameterOptions options2 = new SessionSearchSetParameterOptions
		{
			ComparisonOp = ComparisonOp.Equal,
			Parameter = value
		};
		result = outSessionSearchHandle.SetParameter(ref options2);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking: failed to update session search with bucket id parameter. Error code: {0}", result);
			return;
		}
		foreach (SessionAttribute attribute in attributes)
		{
			value.Key = attribute.Key;
			switch (attribute.ValueType)
			{
			case AttributeType.Boolean:
				value.Value = attribute.AsBool.Value;
				break;
			case AttributeType.Int64:
				value.Value = attribute.AsInt64.Value;
				break;
			case AttributeType.Double:
				value.Value = attribute.AsDouble.Value;
				break;
			case AttributeType.String:
				value.Value = attribute.AsString;
				break;
			}
			options2.Parameter = value;
			result = outSessionSearchHandle.SetParameter(ref options2);
			if (result != Result.Success)
			{
				Debug.LogErrorFormat("Session Matchmaking: failed to update session search with parameter. Error code: {0}", result);
				return;
			}
		}
		SessionSearchFindOptions options3 = new SessionSearchFindOptions
		{
			LocalUserId = EOSManager.Instance.GetProductUserId()
		};
		outSessionSearchHandle.Find(ref options3, null, OnFindSessionsCompleteCallback);
	}

	public void SearchById(string sessionId)
	{
		CurrentSearch.Release();
		CreateSessionSearchOptions options = new CreateSessionSearchOptions
		{
			MaxSearchResults = 10u
		};
		Result result = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().CreateSessionSearch(ref options, out var outSessionSearchHandle);
		if (result != Result.Success)
		{
			AcknowledgeEventId(result);
			Debug.LogErrorFormat("Session Matchmaking: failed create session search. Error code: {0}", result);
			return;
		}
		CurrentSearch.SetNewSearch(outSessionSearchHandle);
		SessionSearchSetSessionIdOptions options2 = new SessionSearchSetSessionIdOptions
		{
			SessionId = sessionId
		};
		result = outSessionSearchHandle.SetSessionId(ref options2);
		if (result != Result.Success)
		{
			AcknowledgeEventId(result);
			Debug.LogErrorFormat("Session Matchmaking: failed to update session search with session ID. Error code: {0}", result);
		}
		else
		{
			SessionSearchFindOptions options3 = new SessionSearchFindOptions
			{
				LocalUserId = EOSManager.Instance.GetProductUserId()
			};
			outSessionSearchHandle.Find(ref options3, null, OnFindSessionsCompleteCallback);
		}
	}

	public SessionDetails MakeSessionHandleByInviteId(string inviteId)
	{
		CopySessionHandleByInviteIdOptions options = new CopySessionHandleByInviteIdOptions
		{
			InviteId = inviteId
		};
		if (EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().CopySessionHandleByInviteId(ref options, out var outSessionHandle) == Result.Success)
		{
			return outSessionHandle;
		}
		return null;
	}

	public SessionDetails MakeSessionHandleByEventId(ulong uiEventId)
	{
		CopySessionHandleByUiEventIdOptions options = new CopySessionHandleByUiEventIdOptions
		{
			UiEventId = uiEventId
		};
		if (EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().CopySessionHandleByUiEventId(ref options, out var outSessionHandle) == Result.Success && outSessionHandle != null)
		{
			return outSessionHandle;
		}
		return null;
	}

	public SessionDetails MakeSessionHandleFromSearch(string sessionId)
	{
		return null;
	}

	public void JoinSession(SessionDetails sessionHandle, bool presenceSession, Action callback = null)
	{
		JoinSessionOptions options = new JoinSessionOptions
		{
			SessionHandle = sessionHandle,
			SessionName = GenerateJoinedSessionName(),
			LocalUserId = EOSManager.Instance.GetProductUserId(),
			PresenceEnabled = presenceSession
		};
		if (callback != null)
		{
			UIOnJoinSession.Enqueue(callback);
		}
		EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().JoinSession(ref options, null, OnJoinSessionListener);
		JoiningSessionDetails = sessionHandle;
	}

	public bool ModifySession(Session session, Action callback = null)
	{
		if (session == null)
		{
			Debug.LogError("Session Matchmaking: pamater session is null.");
			return false;
		}
		SessionsInterface sessionsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface();
		if (sessionsInterface == null)
		{
			Debug.LogError("Session Matchmaking: can't get sessions interface.");
			return false;
		}
		if (!CurrentSessions.TryGetValue(session.Name, out var value))
		{
			Debug.LogError("Session Matchmaking: can't modify session: no active session with specified name.");
			return false;
		}
		UpdateSessionModificationOptions options = new UpdateSessionModificationOptions
		{
			SessionName = session.Name
		};
		Result result = sessionsInterface.UpdateSessionModification(ref options, out var outSessionModificationHandle);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking: failed create session modification. Error code: {0}", result);
			return false;
		}
		if (session.MaxPlayers != value.MaxPlayers)
		{
			SessionModificationSetMaxPlayersOptions options2 = new SessionModificationSetMaxPlayersOptions
			{
				MaxPlayers = session.MaxPlayers
			};
			result = outSessionModificationHandle.SetMaxPlayers(ref options2);
			if (result != Result.Success)
			{
				Debug.LogErrorFormat("Session Matchmaking: failed to set maxp layers. Error code:: {0}", result);
				outSessionModificationHandle.Release();
				return false;
			}
			value.MaxPlayers = session.MaxPlayers;
		}
		if (session.PermissionLevel != value.PermissionLevel)
		{
			SessionModificationSetPermissionLevelOptions options3 = new SessionModificationSetPermissionLevelOptions
			{
				PermissionLevel = session.PermissionLevel
			};
			result = outSessionModificationHandle.SetPermissionLevel(ref options3);
			if (result != Result.Success)
			{
				Debug.LogErrorFormat("Session Matchmaking: failed to set permission level. Error code:: {0}", result);
				outSessionModificationHandle.Release();
				return false;
			}
			value.PermissionLevel = session.PermissionLevel;
		}
		if (session.AllowJoinInProgress != value.AllowJoinInProgress)
		{
			SessionModificationSetJoinInProgressAllowedOptions options4 = new SessionModificationSetJoinInProgressAllowedOptions
			{
				AllowJoinInProgress = session.AllowJoinInProgress
			};
			result = outSessionModificationHandle.SetJoinInProgressAllowed(ref options4);
			if (result != Result.Success)
			{
				Debug.LogErrorFormat("Session Matchmaking: failed to set 'join in progress allowed' flag. Error code:: {0}", result);
				outSessionModificationHandle.Release();
				return false;
			}
			value.AllowJoinInProgress = session.AllowJoinInProgress;
		}
		foreach (SessionAttribute nextAttribute in session.Attributes)
		{
			SessionAttribute sessionAttribute = value.Attributes.Find((SessionAttribute x) => string.Equals(x.Key, nextAttribute.Key, StringComparison.OrdinalIgnoreCase));
			if (sessionAttribute == null || sessionAttribute != nextAttribute)
			{
				AttributeData value2 = new AttributeData
				{
					Key = nextAttribute.Key
				};
				switch (nextAttribute.ValueType)
				{
				case AttributeType.Boolean:
					value2.Value = new AttributeDataValue
					{
						AsBool = nextAttribute.AsBool
					};
					break;
				case AttributeType.Double:
					value2.Value = new AttributeDataValue
					{
						AsDouble = nextAttribute.AsDouble
					};
					break;
				case AttributeType.Int64:
					value2.Value = new AttributeDataValue
					{
						AsInt64 = nextAttribute.AsInt64
					};
					break;
				case AttributeType.String:
					value2.Value = new AttributeDataValue
					{
						AsUtf8 = nextAttribute.AsString
					};
					break;
				}
				SessionModificationAddAttributeOptions options5 = new SessionModificationAddAttributeOptions
				{
					SessionAttribute = value2,
					AdvertisementType = nextAttribute.Advertisement
				};
				result = outSessionModificationHandle.AddAttribute(ref options5);
				if (result != Result.Success)
				{
					Debug.LogErrorFormat("Session Matchmaking: failed to set an attribute: {0}. Error code: {1}", nextAttribute.Key, result);
					outSessionModificationHandle.Release();
					return false;
				}
				if (sessionAttribute == null)
				{
					value.Attributes.Add(nextAttribute);
					continue;
				}
				sessionAttribute.AsString = nextAttribute.AsString;
				sessionAttribute.Advertisement = nextAttribute.Advertisement;
			}
		}
		if (callback != null)
		{
			UIOnSessionModified.Enqueue(callback);
		}
		UpdateSessionOptions options6 = new UpdateSessionOptions
		{
			SessionModificationHandle = outSessionModificationHandle
		};
		sessionsInterface.UpdateSession(ref options6, null, OnUpdateSessionCompleteCallback);
		value = session;
		value.UpdateInProgress = true;
		outSessionModificationHandle.Release();
		return true;
	}

	private void OnSessionDestroyed(string sessionName)
	{
		if (!string.IsNullOrEmpty(sessionName) && CurrentSessions.TryGetValue(sessionName, out var value))
		{
			if (value != null && value.PresenceSession)
			{
				SetJoininfo("");
			}
			CurrentSessions.Remove(sessionName);
		}
	}

	private void OnSessionUpdateFinished(bool success, string sessionName, string sessionId, bool removeSessionOnFailure = false)
	{
		if (!CurrentSessions.TryGetValue(sessionName, out var value))
		{
			return;
		}
		value.Name = sessionName;
		value.InitActiveSession();
		value.UpdateInProgress = false;
		if (success)
		{
			value.Id = sessionId;
			if (value.PresenceSession)
			{
				SetJoininfo(sessionId);
			}
		}
		else if (removeSessionOnFailure)
		{
			CurrentSessions.Remove(sessionName);
		}
	}

	private void OnSearchResultsReceived()
	{
		if (CurrentSearch == null)
		{
			Debug.LogError("Session Matchmaking (OnSearchResultsReceived): CurrentSearch is null");
			return;
		}
		Epic.OnlineServices.Sessions.SessionSearch searchHandle = CurrentSearch.GetSearchHandle();
		if (searchHandle == null)
		{
			Debug.LogError("Session Matchmaking (OnSearchResultsReceived): searchHandle is null");
			return;
		}
		SessionSearchGetSearchResultCountOptions options = default(SessionSearchGetSearchResultCountOptions);
		uint searchResultCount = searchHandle.GetSearchResultCount(ref options);
		Dictionary<Session, SessionDetails> dictionary = new Dictionary<Session, SessionDetails>();
		SessionSearchCopySearchResultByIndexOptions options2 = default(SessionSearchCopySearchResultByIndexOptions);
		for (uint num = 0u; num < searchResultCount; num++)
		{
			options2.SessionIndex = num;
			if (searchHandle.CopySearchResultByIndex(ref options2, out var outSessionHandle) != Result.Success || !(outSessionHandle != null))
			{
				continue;
			}
			SessionDetailsCopyInfoOptions options3 = default(SessionDetailsCopyInfoOptions);
			SessionDetailsInfo? outSessionInfo;
			Result num2 = outSessionHandle.CopyInfo(ref options3, out outSessionInfo);
			Session session = new Session();
			if (num2 == Result.Success)
			{
				session.InitFromSessionInfo(outSessionHandle, outSessionInfo);
			}
			session.SearchResults = true;
			dictionary.Add(session, outSessionHandle);
			foreach (KeyValuePair<string, Session> currentSession in CurrentSessions)
			{
				if (currentSession.Value.Id == session.Id)
				{
					session.Name = currentSession.Key;
					break;
				}
			}
		}
		CurrentSearch.OnSearchResultReceived(dictionary);
		if (JoinPresenceSessionId.Length > 0)
		{
			SessionDetails sessionHandleById = CurrentSearch.GetSessionHandleById(JoinPresenceSessionId);
			if (sessionHandleById != null)
			{
				JoinPresenceSessionId = string.Empty;
				JoinSession(sessionHandleById, presenceSession: true);
			}
			else
			{
				AcknowledgeEventId(Result.NotFound);
			}
		}
		else
		{
			AcknowledgeEventId(Result.NotFound);
		}
	}

	private void OnJoinSessionFinished()
	{
		if (!(JoiningSessionDetails != null))
		{
			return;
		}
		SessionDetailsCopyInfoOptions options = default(SessionDetailsCopyInfoOptions);
		if (JoiningSessionDetails.CopyInfo(ref options, out var outSessionInfo) != Result.Success)
		{
			return;
		}
		Session session = new Session();
		session.Name = GenerateJoinedSessionName(noIncrement: true);
		session.InitFromSessionInfo(JoiningSessionDetails, outSessionInfo);
		bool flag = false;
		foreach (Session value in CurrentSessions.Values)
		{
			if (value.Id == session.Id)
			{
				flag = true;
				if (session.PresenceSession)
				{
					SetJoininfo(session.Id);
				}
				break;
			}
		}
		if (!flag)
		{
			CurrentSessions[session.Name] = session;
			if (session.PresenceSession)
			{
				SetJoininfo(session.Id);
			}
		}
		if (UIOnJoinSession.Count > 0)
		{
			UIOnJoinSession.Dequeue()();
		}
	}

	private void SetJoininfo(string sessionId, bool onLoggingOut = false)
	{
		if (EOSManager.Instance.GetProductUserId() == null)
		{
			Debug.LogError("Session Matchmaking (SetJoinInfo): Current player is invalid");
			return;
		}
		PresenceInterface presenceInterface = EOSManager.Instance.GetEOSPlatformInterface().GetPresenceInterface();
		CreatePresenceModificationOptions options = new CreatePresenceModificationOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId()
		};
		Result result = presenceInterface.CreatePresenceModification(ref options, out var outPresenceModificationHandle);
		if (result != Result.Success)
		{
			if (onLoggingOut)
			{
				Debug.LogWarning("Session Matchmaking (SetJoinInfo): Create presence modification during logOut, ignore.");
				return;
			}
			Debug.LogErrorFormat("Session Matchmaking (SetJoinInfo): Create presence modification failed: {0}", result);
			return;
		}
		PresenceModificationSetJoinInfoOptions options2 = default(PresenceModificationSetJoinInfoOptions);
		if (string.IsNullOrEmpty(sessionId))
		{
			options2.JoinInfo = null;
		}
		else
		{
			options2.JoinInfo = sessionId;
		}
		result = outPresenceModificationHandle.SetJoinInfo(ref options2);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking (SetJoinInfo): SetJoinInfo failed: {0}", result);
			return;
		}
		SetPresenceOptions options3 = new SetPresenceOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId(),
			PresenceModificationHandle = outPresenceModificationHandle
		};
		presenceInterface.SetPresence(ref options3, null, OnSetPresenceCompleteCallback);
		outPresenceModificationHandle.Release();
	}

	private void OnJoinGameAcceptedByJoinInfo(string joinInfo, ulong uiEventId)
	{
		JoinUiEvent = uiEventId;
		if (joinInfo.Contains("SessionId") && joinInfo.Length == 2)
		{
			JoinPresenceSessionById(joinInfo.Substring(1, 1));
			return;
		}
		AcknowledgeEventId(Result.UnexpectedError);
		Debug.LogErrorFormat("Session Matchmaking (OnJoinGameAccepted): unable to parse location string: {0}", joinInfo);
	}

	private void OnJoinGameAcceptedByEventId(ulong uiEventId)
	{
		SessionDetails sessionDetails = MakeSessionHandleByEventId(uiEventId);
		if (sessionDetails != null)
		{
			JoinSession(sessionDetails, presenceSession: true);
			return;
		}
		JoinUiEvent = uiEventId;
		AcknowledgeEventId(Result.UnexpectedError);
		Debug.LogErrorFormat("Session Matchmaking (OnJoinGameAcceptedByEventId): unable to get details for event ID: {0}", uiEventId);
	}

	private void JoinPresenceSessionById(string sessionId)
	{
		JoinPresenceSessionId = sessionId;
		Debug.LogFormat("Session Matchmaking (JoinPresenceSessionById): looking for session ID: {0}", JoinPresenceSessionId);
		SearchById(JoinPresenceSessionId);
	}

	private void AcknowledgeEventId(Result result)
	{
		if (JoinUiEvent != 0L)
		{
			AcknowledgeEventIdOptions options = new AcknowledgeEventIdOptions
			{
				UiEventId = JoinUiEvent,
				Result = result
			};
			EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().AcknowledgeEventId(ref options);
			JoinUiEvent = 0uL;
		}
	}

	private string GenerateJoinedSessionName(bool noIncrement = false)
	{
		if (!noIncrement)
		{
			JoinedSessionIndex = (JoinedSessionIndex + 1) & 9;
		}
		return string.Format("{0}{1}", "Session#", JoinedSessionIndex);
	}

	private void OnUpdateSessionCompleteCallback(ref UpdateSessionCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			OnSessionUpdateFinished(success: false, data.SessionName, data.SessionId);
			Debug.LogErrorFormat("Session Matchmaking (OnUpdateSessionCompleteCallback): error code: {0}", data.ResultCode);
			return;
		}
		OnSessionUpdateFinished(success: true, data.SessionName, data.SessionId);
		Debug.Log("Session Matchmaking: game session updated successfully.");
		if (UIOnSessionModified.Count > 0)
		{
			UIOnSessionModified.Dequeue()();
		}
	}

	private void OnUpdateSessionCompleteCallback_ForCreate(ref UpdateSessionCallbackInfo data)
	{
		bool removeSessionOnFailure = true;
		bool flag = data.ResultCode == Result.Success;
		if (flag)
		{
			ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
			if (productUserId != null)
			{
				Register(data.SessionName, productUserId);
				removeSessionOnFailure = false;
			}
			else
			{
				Debug.LogError("Session Matchmaking (OnUpdateSessionCompleteCallback_ForCreate): player is null, can't register yourself in created session.");
			}
		}
		else
		{
			Debug.LogErrorFormat("Session Matchmaking (OnUpdateSessionCompleteCallback): error code: {0}", data.ResultCode);
		}
		if (UIOnSessionCreated.Count > 0)
		{
			UIOnSessionCreated.Dequeue()();
		}
		OnSessionUpdateFinished(flag, data.SessionName, data.SessionId, removeSessionOnFailure);
	}

	private void OnStartSessionCompleteCallBack(ref StartSessionCallbackInfo data)
	{
		if (data.ClientData == null)
		{
			Debug.LogError("Session Matchmaking (OnStartSessionCompleteCallback): data.ClientData is null");
			return;
		}
		string text = (string)data.ClientData;
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking (OnStartSessionCompleteCallback): session name: '{0}' error code: {1}", text, data.ResultCode);
		}
		else
		{
			Debug.LogFormat("Session Matchmaking(OnStartSessionCompleteCallback): Started session: {0}", text);
		}
	}

	private void OnEndSessionCompleteCallback(ref EndSessionCallbackInfo data)
	{
		string text = (string)data.ClientData;
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking (OnEndSessionCompleteCallback): session name: '{0}' error code: {1}", text, data.ResultCode);
		}
		else
		{
			Debug.LogFormat("Session Matchmaking(OnEndSessionCompleteCallback): Ended session: {0}", text);
		}
	}

	private void OnDestroySessionCompleteCallback(ref DestroySessionCallbackInfo data)
	{
		if (data.ClientData == null)
		{
			Debug.LogError("Session Matchmaking (OnDestroySessionCompleteCallback): data.ClientData is null!");
			return;
		}
		string text = (string)data.ClientData;
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking (OnDestroySessionCompleteCallback): error code: {0}", data.ResultCode);
		}
		else if (!string.IsNullOrEmpty(text))
		{
			OnSessionDestroyed(text);
		}
	}

	private void OnRegisterCompleteCallback(ref RegisterPlayersCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking (OnRegisterCompleteCallback): error code: {0}", data.ResultCode);
		}
	}

	private void OnUnregisterCompleteCallback(ref UnregisterPlayersCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking (OnUnregisterCompleteCallback): error code: {0}", data.ResultCode);
		}
	}

	private void OnFindSessionsCompleteCallback(ref SessionSearchFindCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			AcknowledgeEventId(data.ResultCode);
			Debug.LogErrorFormat("Session Matchmaking (OnFindSessionsCompleteCallback): error code: {0}", data.ResultCode);
		}
		else
		{
			OnSearchResultsReceived();
		}
	}

	private void OnSendInviteCompleteCallback(ref SendInviteCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking (OnSendInviteCompleteCallback): error code: {0}", data.ResultCode);
			return;
		}
		Debug.Log("Session Matchmaking: invite to session sent successfully.");
		if (UIOnInviteSession.Count > 0)
		{
			UIOnInviteSession.Dequeue()();
		}
	}

	public void OnSessionInviteReceivedListener(ref SessionInviteReceivedCallbackInfo data)
	{
		Debug.LogFormat("Session Matchmaking: invite to session received. Invite id: {0}", data.InviteId);
		SessionDetails sessionDetails = MakeSessionHandleByInviteId(data.InviteId);
		if (sessionDetails == null)
		{
			Debug.LogErrorFormat("Session Matchmaking (OnSessionInviteReceivedListener): Could not copy session information for invite id {0}", data.InviteId);
			return;
		}
		Session session = new Session();
		if (session.InitFromInfoOfSessionDetails(sessionDetails))
		{
			SetInviteSession(session, sessionDetails);
			Debug.LogFormat("Session Matchmaking (OnSessionInviteReceivedListener): Invite received id = {0}", data.InviteId);
		}
		else
		{
			Debug.LogErrorFormat("Session Matchmaking (OnSessionInviteReceivedListener): Could not copy session information for invite id {0}", data.InviteId);
		}
		if (UIOnInviteReceived != null)
		{
			UIOnInviteReceived();
		}
	}

	private void PopLobbyInvite()
	{
		if (CurrentInvite != null)
		{
			Invites.Remove(CurrentInvite);
			CurrentInvite = null;
		}
		if (Invites.Count > 0)
		{
			Dictionary<Session, SessionDetails>.Enumerator enumerator = Invites.GetEnumerator();
			enumerator.MoveNext();
			CurrentInvite = enumerator.Current.Key;
		}
	}

	public void OnSessionInviteAcceptedListener(ref SessionInviteAcceptedCallbackInfo data)
	{
		Debug.Log("Session Matchmaking: joined session successfully.");
		OnJoinSessionFinished();
	}

	private void OnJoinSessionListener(ref JoinSessionCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			AcknowledgeEventId(data.ResultCode);
			Debug.LogErrorFormat("Session Matchmaking (OnJoinSessionListener): error code: {0}", data.ResultCode);
		}
		else
		{
			Debug.Log("Session Matchmaking: joined session successfully.");
			OnJoinSessionFinished();
			AcknowledgeEventId(data.ResultCode);
		}
	}

	public void OnJoinGameAcceptedListener(ref JoinGameAcceptedCallbackInfo data)
	{
		Debug.Log("Session Matchmaking: join game accepted successfully.");
		OnJoinGameAcceptedByJoinInfo(data.JoinInfo, data.UiEventId);
	}

	public void OnJoinSessionAcceptedListener(ref JoinSessionAcceptedCallbackInfo data)
	{
		Debug.Log("Session Matchmaking: join game accepted successfully.");
		OnJoinGameAcceptedByEventId(data.UiEventId);
	}

	private void OnSetPresenceCompleteCallback(ref SetPresenceCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Session Matchmaking (OnSetPresenceCallback): error code: {0}", data.ResultCode);
		}
		else
		{
			Debug.Log("Session Matchmaking: set presence successfully.");
		}
	}

	public void AcceptLobbyInvite(bool invitePresenceToggled)
	{
		if (CurrentInvite != null && Invites.TryGetValue(CurrentInvite, out var value))
		{
			JoinSession(value, invitePresenceToggled, OnJoinSessionFinished);
			PopLobbyInvite();
		}
		else
		{
			Debug.LogError("Session Matchmaking (AcceptLobbyInvite): CurrentInvite not found.");
		}
	}

	public void DeclineLobbyInvite()
	{
		PopLobbyInvite();
	}
}
