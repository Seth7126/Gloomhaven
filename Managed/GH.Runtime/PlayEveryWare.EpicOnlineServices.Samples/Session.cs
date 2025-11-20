using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;

namespace PlayEveryWare.EpicOnlineServices.Samples;

public class Session
{
	public string Name = string.Empty;

	public string Id = string.Empty;

	public string BucketId = string.Empty;

	public uint MaxPlayers;

	public uint NumConnections = 1u;

	public bool AllowJoinInProgress;

	public bool PresenceSession;

	public bool InvitesAllowed = true;

	public OnlineSessionPermissionLevel PermissionLevel;

	public ActiveSession ActiveSession;

	public List<SessionAttribute> Attributes = new List<SessionAttribute>();

	public bool SearchResults;

	public bool UpdateInProgress = true;

	public OnlineSessionState SessionState;

	public bool InitFromInfoOfSessionDetails(SessionDetails session)
	{
		SessionDetailsCopyInfoOptions options = default(SessionDetailsCopyInfoOptions);
		if (session.CopyInfo(ref options, out var outSessionInfo) != Result.Success)
		{
			return false;
		}
		InitFromSessionInfo(session, outSessionInfo);
		return true;
	}

	public void InitFromSessionInfo(SessionDetails session, SessionDetailsInfo? sessionDetailsInfo)
	{
		if (sessionDetailsInfo.HasValue && sessionDetailsInfo.HasValue && sessionDetailsInfo.GetValueOrDefault().Settings.HasValue)
		{
			AllowJoinInProgress = (sessionDetailsInfo?.Settings?.AllowJoinInProgress).Value;
			BucketId = sessionDetailsInfo?.Settings?.BucketId;
			PermissionLevel = (sessionDetailsInfo?.Settings?.PermissionLevel).Value;
			MaxPlayers = (sessionDetailsInfo?.Settings?.NumPublicConnections).Value;
			Id = sessionDetailsInfo?.SessionId;
		}
		Attributes.Clear();
		SessionDetailsGetSessionAttributeCountOptions options = default(SessionDetailsGetSessionAttributeCountOptions);
		uint sessionAttributeCount = session.GetSessionAttributeCount(ref options);
		for (uint num = 0u; num < sessionAttributeCount; num++)
		{
			SessionDetailsCopySessionAttributeByIndexOptions options2 = new SessionDetailsCopySessionAttributeByIndexOptions
			{
				AttrIndex = num
			};
			if (session.CopySessionAttributeByIndex(ref options2, out var outSessionAttribute) == Result.Success && outSessionAttribute.HasValue && outSessionAttribute.HasValue && outSessionAttribute.GetValueOrDefault().Data.HasValue)
			{
				SessionAttribute sessionAttribute = new SessionAttribute();
				sessionAttribute.Advertisement = (outSessionAttribute?.AdvertisementType).Value;
				sessionAttribute.Key = outSessionAttribute?.Data?.Key;
				AttributeDataValue value = (outSessionAttribute?.Data?.Value).Value;
				switch (value.ValueType)
				{
				case AttributeType.Boolean:
					sessionAttribute.ValueType = AttributeType.Boolean;
					sessionAttribute.AsBool = value.AsBool;
					break;
				case AttributeType.Int64:
					sessionAttribute.ValueType = AttributeType.Int64;
					sessionAttribute.AsInt64 = value.AsInt64;
					break;
				case AttributeType.Double:
					sessionAttribute.ValueType = AttributeType.Double;
					sessionAttribute.AsDouble = value.AsDouble;
					break;
				case AttributeType.String:
					sessionAttribute.ValueType = AttributeType.String;
					sessionAttribute.AsString = value.AsUtf8;
					break;
				}
				Attributes.Add(sessionAttribute);
			}
		}
		InitActiveSession();
		UpdateInProgress = false;
	}

	public void InitActiveSession()
	{
		if (!string.IsNullOrEmpty(Name))
		{
			CopyActiveSessionHandleOptions options = new CopyActiveSessionHandleOptions
			{
				SessionName = Name
			};
			if (EOSManager.Instance.GetEOSPlatformInterface().GetSessionsInterface().CopyActiveSessionHandle(ref options, out var outSessionHandle) != Result.Success)
			{
				Debug.LogErrorFormat("Session Matchmaking: could not get ActiveSession for name: {0}", Name);
			}
			else
			{
				ActiveSession = outSessionHandle;
			}
		}
	}

	public bool IsValid()
	{
		if (string.IsNullOrEmpty(Name))
		{
			return !string.IsNullOrEmpty(Id);
		}
		return true;
	}

	public override bool Equals(object other)
	{
		Session session = (Session)other;
		if (Name.Equals(session.Name, StringComparison.OrdinalIgnoreCase) && Id.Equals(session.Id, StringComparison.OrdinalIgnoreCase) && BucketId.Equals(session.BucketId, StringComparison.OrdinalIgnoreCase) && MaxPlayers == session.MaxPlayers && NumConnections == session.NumConnections && AllowJoinInProgress == session.AllowJoinInProgress && PresenceSession == session.PresenceSession && InvitesAllowed == session.InvitesAllowed && PermissionLevel == session.PermissionLevel)
		{
			return Attributes == session.Attributes;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
