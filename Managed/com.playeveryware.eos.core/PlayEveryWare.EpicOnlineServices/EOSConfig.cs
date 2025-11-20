using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.IntegratedPlatform;
using Epic.OnlineServices.Platform;

namespace PlayEveryWare.EpicOnlineServices;

[Serializable]
public class EOSConfig : ICloneableGeneric<EOSConfig>, IEmpty
{
	public string productName;

	public string productVersion;

	public string productID;

	public string sandboxID;

	public string deploymentID;

	public List<SandboxDeploymentOverride> sandboxDeploymentOverrides;

	public string clientSecret;

	public string clientID;

	public string encryptionKey;

	public List<string> platformOptionsFlags;

	public List<string> authScopeOptionsFlags;

	public uint tickBudgetInMilliseconds;

	public string ThreadAffinity_networkWork;

	public string ThreadAffinity_storageIO;

	public string ThreadAffinity_webSocketIO;

	public string ThreadAffinity_P2PIO;

	public string ThreadAffinity_HTTPRequestIO;

	public string ThreadAffinity_RTCIO;

	public bool alwaysSendInputToOverlay;

	public string initialButtonDelayForOverlay;

	public string repeatButtonDelayForOverlay;

	public bool hackForceSendInputDirectlyToSDK;

	public static Regex InvalidEncryptionKeyRegex;

	static EOSConfig()
	{
		InvalidEncryptionKeyRegex = new Regex("[^0-9a-fA-F]");
	}

	public static bool IsEncryptionKeyValid(string key)
	{
		if (key != null && key.Length == 64)
		{
			return !InvalidEncryptionKeyRegex.Match(key).Success;
		}
		return false;
	}

	public static bool StringIsEqualToAny(string flagAsCString, params string[] parameters)
	{
		foreach (string text in parameters)
		{
			if (flagAsCString == text)
			{
				return true;
			}
		}
		return false;
	}

	public static T EnumCast<T, V>(V value)
	{
		return (T)Enum.ToObject(typeof(T), value);
	}

	public static IntegratedPlatformManagementFlags flagsAsIntegratedPlatformManagementFlags(List<string> flags)
	{
		int num = 0;
		foreach (string flag in flags)
		{
			if (StringIsEqualToAny(flag, "EOS_IPMF_Disabled", "Disabled"))
			{
				num |= 1;
			}
			else if (StringIsEqualToAny(flag, "EOS_IPMF_ManagedByApplication", "ManagedByApplication", "EOS_IPMF_LibraryManagedByApplication"))
			{
				num |= 2;
			}
			else if (StringIsEqualToAny(flag, "EOS_IPMF_ManagedBySDK", "ManagedBySDK", "EOS_IPMF_LibraryManagedBySDK"))
			{
				num |= 4;
			}
			else if (StringIsEqualToAny(flag, "EOS_IPMF_DisableSharedPresence", "DisableSharedPresence", "EOS_IPMF_DisablePresenceMirroring"))
			{
				num |= 8;
			}
			else if (StringIsEqualToAny(flag, "EOS_IPMF_DisableSessions", "DisableSessions", "EOS_IPMF_DisableSDKManagedSessions"))
			{
				num |= 0x10;
			}
			else if (StringIsEqualToAny(flag, "EOS_IPMF_PreferEOS", "PreferEOS", "EOS_IPMF_PreferEOSIdentity"))
			{
				num |= 0x20;
			}
			else if (StringIsEqualToAny(flag, "EOS_IPMF_PreferIntegrated", "PreferIntegrated", "EOS_IPMF_PreferIntegratedIdentity"))
			{
				num |= 0x40;
			}
		}
		return EnumCast<IntegratedPlatformManagementFlags, int>(num);
	}

	public static PlatformFlags platformOptionsFlagsAsPlatformFlags(List<string> platformOptionsFlags)
	{
		PlatformFlags platformFlags = PlatformFlags.None;
		using List<string>.Enumerator enumerator = platformOptionsFlags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case "LoadingInEditor":
			case "EOS_PF_LOADING_IN_EDITOR":
				platformFlags |= PlatformFlags.LoadingInEditor;
				break;
			case "DisableOverlay":
			case "EOS_PF_DISABLE_OVERLAY":
				platformFlags |= PlatformFlags.DisableOverlay;
				break;
			case "DisableSocialOverlay":
			case "EOS_PF_DISABLE_SOCIAL_OVERLAY":
				platformFlags |= PlatformFlags.DisableSocialOverlay;
				break;
			case "Reserved1":
			case "EOS_PF_RESERVED1":
				platformFlags |= PlatformFlags.Reserved1;
				break;
			case "WindowsEnabledOverlayD3D9":
			case "EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9":
				platformFlags |= PlatformFlags.WindowsEnableOverlayD3D9;
				break;
			case "WindowsEnabledOverlayD3D10":
			case "EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10":
				platformFlags |= PlatformFlags.WindowsEnableOverlayD3D10;
				break;
			case "WindowsEnabledOverlayOpengl":
			case "EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL":
				platformFlags |= PlatformFlags.WindowsEnableOverlayOpengl;
				break;
			}
		}
		return platformFlags;
	}

	public PlatformFlags platformOptionsFlagsAsPlatformFlags()
	{
		return platformOptionsFlagsAsPlatformFlags(platformOptionsFlags);
	}

	public static AuthScopeFlags authScopeOptionsFlagsAsAuthScopeFlags(List<string> authScopeOptionsFlags)
	{
		AuthScopeFlags authScopeFlags = AuthScopeFlags.NoFlags;
		using List<string>.Enumerator enumerator = authScopeOptionsFlags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case "BasicProfile":
			case "EOS_AS_BasicProfile":
				authScopeFlags |= AuthScopeFlags.BasicProfile;
				break;
			case "FriendsList":
			case "EOS_AS_FriendsList":
				authScopeFlags |= AuthScopeFlags.FriendsList;
				break;
			case "Presence":
			case "EOS_AS_Presence":
				authScopeFlags |= AuthScopeFlags.Presence;
				break;
			case "FriendsManagement":
			case "EOS_AS_FriendsManagement":
				authScopeFlags |= AuthScopeFlags.FriendsManagement;
				break;
			case "Email":
			case "EOS_AS_Email":
				authScopeFlags |= AuthScopeFlags.Email;
				break;
			case "Country":
			case "EOS_AS_Country":
				authScopeFlags |= AuthScopeFlags.Country;
				break;
			}
		}
		return authScopeFlags;
	}

	public AuthScopeFlags authScopeOptionsFlagsAsAuthScopeFlags()
	{
		return authScopeOptionsFlagsAsAuthScopeFlags(authScopeOptionsFlags);
	}

	public EOSConfig Clone()
	{
		return (EOSConfig)MemberwiseClone();
	}

	public bool IsEmpty()
	{
		if (EmptyPredicates.IsEmptyOrNull(productName) && EmptyPredicates.IsEmptyOrNull(productVersion) && EmptyPredicates.IsEmptyOrNull(productID) && EmptyPredicates.IsEmptyOrNull(sandboxID) && EmptyPredicates.IsEmptyOrNull(deploymentID) && (sandboxDeploymentOverrides == null || sandboxDeploymentOverrides.Count == 0) && EmptyPredicates.IsEmptyOrNull(clientSecret) && EmptyPredicates.IsEmptyOrNull(clientID) && EmptyPredicates.IsEmptyOrNull(encryptionKey) && EmptyPredicates.IsEmptyOrNull(platformOptionsFlags) && EmptyPredicates.IsEmptyOrNull(authScopeOptionsFlags) && EmptyPredicates.IsEmptyOrNull(repeatButtonDelayForOverlay))
		{
			return EmptyPredicates.IsEmptyOrNull(initialButtonDelayForOverlay);
		}
		return false;
	}

	public float GetInitialButtonDelayForOverlayAsFloat()
	{
		return float.Parse(initialButtonDelayForOverlay);
	}

	public void SetInitialButtonDelayForOverlayFromFloat(float f)
	{
		initialButtonDelayForOverlay = f.ToString();
	}

	public float GetRepeatButtonDelayForOverlayAsFloat()
	{
		return float.Parse(repeatButtonDelayForOverlay);
	}

	public void SetRepeatButtonDelayForOverlayFromFloat(float f)
	{
		repeatButtonDelayForOverlay = f.ToString();
	}

	public ulong GetThreadAffinityNetworkWork(ulong defaultValue = 0uL)
	{
		if (!string.IsNullOrEmpty(ThreadAffinity_networkWork))
		{
			return ulong.Parse(ThreadAffinity_networkWork);
		}
		return defaultValue;
	}

	public ulong GetThreadAffinityStorageIO(ulong defaultValue = 0uL)
	{
		if (!string.IsNullOrEmpty(ThreadAffinity_storageIO))
		{
			return ulong.Parse(ThreadAffinity_storageIO);
		}
		return defaultValue;
	}

	public ulong GetThreadAffinityWebSocketIO(ulong defaultValue = 0uL)
	{
		if (!string.IsNullOrEmpty(ThreadAffinity_webSocketIO))
		{
			return ulong.Parse(ThreadAffinity_webSocketIO);
		}
		return defaultValue;
	}

	public ulong GetThreadAffinityP2PIO(ulong defaultValue = 0uL)
	{
		if (!string.IsNullOrEmpty(ThreadAffinity_P2PIO))
		{
			return ulong.Parse(ThreadAffinity_P2PIO);
		}
		return defaultValue;
	}

	public ulong GetThreadAffinityHTTPRequestIO(ulong defaultValue = 0uL)
	{
		if (!string.IsNullOrEmpty(ThreadAffinity_HTTPRequestIO))
		{
			return ulong.Parse(ThreadAffinity_HTTPRequestIO);
		}
		return defaultValue;
	}

	public ulong GetThreadAffinityRTCIO(ulong defaultValue = 0uL)
	{
		if (!string.IsNullOrEmpty(ThreadAffinity_RTCIO))
		{
			return ulong.Parse(ThreadAffinity_RTCIO);
		}
		return defaultValue;
	}

	public bool IsEncryptionKeyValid()
	{
		return IsEncryptionKeyValid(encryptionKey);
	}
}
