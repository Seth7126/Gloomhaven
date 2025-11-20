using System.Collections.Generic;
using System.Linq;
using Hydra.Api.Errors;
using Hydra.Api.Nullable;
using Hydra.Api.SessionControl;
using Hydra.Sdk.Components.SessionControl.Core.ServerBrowsing;
using Hydra.Sdk.Errors;

namespace Hydra.Sdk.Extensions;

public static class ServerBrowsingExtensions
{
	public static ServerBrowsingSessionData Copy(this ServerBrowsingSessionData dataToCopyFrom)
	{
		return new ServerBrowsingSessionData
		{
			GameMap = new NullableString
			{
				Data = dataToCopyFrom.GameMap.Data
			},
			GameMode = new NullableString
			{
				Data = dataToCopyFrom.GameMode.Data
			},
			ServerName = new NullableString
			{
				Data = dataToCopyFrom.ServerName.Data
			},
			MaxPlayerCount = new NullableInt
			{
				Data = dataToCopyFrom.MaxPlayerCount.Data
			},
			PasswordProtected = new NullableBool
			{
				Data = dataToCopyFrom.PasswordProtected.Data
			},
			KeyValues = new HeartbeatServerKeyValue
			{
				List = { dataToCopyFrom.KeyValues.List.Select((SessionKeyValue kv) => new SessionKeyValue
				{
					Key = kv.Key,
					Value = kv.Value
				}) }
			},
			Members = new HeartbeatServerMembers
			{
				List = { dataToCopyFrom.Members.List.Select((string m) => m) }
			},
			Tags = new HeartbeatServerTags
			{
				List = { dataToCopyFrom.Tags.List.Select((string t) => t) }
			}
		};
	}

	public static void Validate(this ServerBrowsingSessionData serverData)
	{
		if (serverData.GameMode?.Data == null)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "GameMode cannot be null!");
		}
		if (serverData.GameMap?.Data == null)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "GameMap cannot be null!");
		}
		if (serverData.ServerName?.Data == null)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "ServerName cannot be null!");
		}
		NullableInt maxPlayerCount = serverData.MaxPlayerCount;
		if (maxPlayerCount != null)
		{
			_ = maxPlayerCount.Data;
			if (0 == 0)
			{
				if (serverData.MaxPlayerCount.Data <= 0)
				{
					throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "MaxPlayerCount must be greater than 0!");
				}
				if (serverData.Members?.List == null)
				{
					throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Members cannot be null!");
				}
				if (serverData.Tags?.List == null)
				{
					throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Tags cannot be null!");
				}
				if (serverData.KeyValues?.List == null)
				{
					throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "KeyValues cannot be null!");
				}
				return;
			}
		}
		throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "MaxPlayerCount cannot be null!");
	}

	public static ServerBrowsingSessionData GetDifference(this ServerBrowsingSessionData actual, ServerBrowsingSessionData cached)
	{
		ServerBrowsingSessionData serverBrowsingSessionData = new ServerBrowsingSessionData();
		bool flag = false;
		if (actual.GameMap != null && actual.GameMap.Data != cached.GameMap.Data)
		{
			serverBrowsingSessionData.GameMap = actual.GameMap;
			flag = true;
		}
		if (actual.GameMode != null && actual.GameMode.Data != cached.GameMode.Data)
		{
			serverBrowsingSessionData.GameMode = actual.GameMode;
			flag = true;
		}
		if (actual.ServerName != null && actual.ServerName.Data != cached.ServerName.Data)
		{
			serverBrowsingSessionData.ServerName = actual.ServerName;
			flag = true;
		}
		if (actual.PasswordProtected != null && actual.PasswordProtected.Data != cached.PasswordProtected.Data)
		{
			serverBrowsingSessionData.PasswordProtected = actual.PasswordProtected;
			flag = true;
		}
		if (actual.MaxPlayerCount != null && actual.MaxPlayerCount.Data != cached.MaxPlayerCount.Data)
		{
			serverBrowsingSessionData.MaxPlayerCount = actual.MaxPlayerCount;
			flag = true;
		}
		if (actual.KeyValues != null && (actual.KeyValues.List.Except(cached.KeyValues.List).Any() || actual.KeyValues.List.Count != cached.KeyValues.List.Count))
		{
			serverBrowsingSessionData.KeyValues = actual.KeyValues;
			flag = true;
		}
		if (actual.Members != null && !actual.Members.List.All(cached.Members.List.Contains))
		{
			serverBrowsingSessionData.Members = actual.Members;
			flag = true;
		}
		if (actual.Tags != null && !actual.Tags.List.All(cached.Tags.List.Contains))
		{
			serverBrowsingSessionData.Tags = actual.Tags;
			flag = true;
		}
		return flag ? serverBrowsingSessionData : null;
	}

	public static ServerParameters ToServerParameters(this ServerBrowsingSessionData sessionData)
	{
		return new ServerParameters
		{
			GameMap = sessionData.GameMap.Data,
			GameMode = sessionData.GameMode.Data,
			ServerName = sessionData.ServerName.Data,
			MaxPlayerCount = sessionData.MaxPlayerCount.Data,
			PasswordProtected = sessionData.PasswordProtected.Data,
			KeyValues = sessionData.KeyValues.List.ToDictionary((SessionKeyValue k) => k.Key, (SessionKeyValue v) => v.Value),
			Members = sessionData.Members.List.Select((string m) => m).ToList(),
			Tags = sessionData.Tags.List.Select((string t) => t).ToList()
		};
	}

	public static ServerBrowsingSessionData ToServerBrowsingSessionData(this ServerParameters parameters)
	{
		ServerBrowsingSessionData serverBrowsingSessionData = new ServerBrowsingSessionData();
		if (parameters.GameMap != null)
		{
			serverBrowsingSessionData.GameMap = new NullableString
			{
				Data = parameters.GameMap
			};
		}
		if (parameters.GameMode != null)
		{
			serverBrowsingSessionData.GameMode = new NullableString
			{
				Data = parameters.GameMode
			};
		}
		if (parameters.ServerName != null)
		{
			serverBrowsingSessionData.ServerName = new NullableString
			{
				Data = parameters.ServerName
			};
		}
		if (parameters.MaxPlayerCount.HasValue)
		{
			serverBrowsingSessionData.MaxPlayerCount = new NullableInt
			{
				Data = parameters.MaxPlayerCount.Value
			};
		}
		if (parameters.PasswordProtected.HasValue)
		{
			serverBrowsingSessionData.PasswordProtected = new NullableBool
			{
				Data = parameters.PasswordProtected.Value
			};
		}
		if (parameters.KeyValues != null)
		{
			serverBrowsingSessionData.KeyValues = new HeartbeatServerKeyValue
			{
				List = { parameters.KeyValues.Select((KeyValuePair<string, string> kv) => new SessionKeyValue
				{
					Key = kv.Key,
					Value = kv.Value
				}) }
			};
		}
		if (parameters.Tags != null)
		{
			serverBrowsingSessionData.Tags = new HeartbeatServerTags
			{
				List = { (IEnumerable<string>)parameters.Tags }
			};
		}
		if (parameters.Members != null)
		{
			serverBrowsingSessionData.Members = new HeartbeatServerMembers
			{
				List = { (IEnumerable<string>)parameters.Members }
			};
		}
		return serverBrowsingSessionData;
	}

	public static ServerParameters Copy(this ServerParameters copyFrom)
	{
		return new ServerParameters
		{
			GameMap = copyFrom.GameMap,
			GameMode = copyFrom.GameMode,
			ServerName = copyFrom.ServerName,
			MaxPlayerCount = copyFrom.MaxPlayerCount,
			PasswordProtected = copyFrom.PasswordProtected,
			Tags = new List<string>(copyFrom.Tags),
			Members = new List<string>(copyFrom.Members),
			KeyValues = new Dictionary<string, string>(copyFrom.KeyValues)
		};
	}

	public static ServerParameters GetDifference(this ServerParameters actual, ServerParameters cached)
	{
		ServerParameters serverParameters = new ServerParameters
		{
			GameMap = null,
			GameMode = null,
			ServerName = null,
			MaxPlayerCount = null,
			PasswordProtected = null,
			Tags = null,
			Members = null,
			KeyValues = null
		};
		bool flag = false;
		if (actual.GameMap != cached.GameMap)
		{
			serverParameters.GameMap = actual.GameMap;
			flag = true;
		}
		if (actual.GameMode != cached.GameMode)
		{
			serverParameters.GameMode = actual.GameMode;
			flag = true;
		}
		if (actual.ServerName != cached.ServerName)
		{
			serverParameters.ServerName = actual.ServerName;
			flag = true;
		}
		if (actual.PasswordProtected != cached.PasswordProtected)
		{
			serverParameters.PasswordProtected = actual.PasswordProtected;
			flag = true;
		}
		if (actual.MaxPlayerCount != cached.MaxPlayerCount)
		{
			serverParameters.MaxPlayerCount = actual.MaxPlayerCount;
			flag = true;
		}
		if (actual.KeyValues != null && (actual.KeyValues.Except(cached.KeyValues).Any() || actual.KeyValues.Count != cached.KeyValues.Count))
		{
			serverParameters.KeyValues = actual.KeyValues;
			flag = true;
		}
		if (actual.Members != null && !actual.Members.All(cached.Members.Contains))
		{
			serverParameters.Members = actual.Members;
			flag = true;
		}
		if (actual.Tags != null && !actual.Tags.All(cached.Tags.Contains))
		{
			serverParameters.Tags = actual.Tags;
			flag = true;
		}
		return flag ? serverParameters : null;
	}
}
