using System;
using Platforms.Activities;
using Platforms.PlatformAchievements;
using Platforms.PlatformData;
using Platforms.Profanity;
using Platforms.ProsOrHydra;
using Platforms.Social;

namespace Platforms;

public interface IPlatform : IAsyncDisposable
{
	IPlatformUserManagement UserManagement { get; }

	IPlatformInput PlatformInput { get; }

	IPlatformData PlatformData { get; }

	IPlatformAchievements PlatformAchievements { get; }

	bool IsSupportActivities { get; }

	IPlatformActivities PlatformActivities { get; }

	bool IsSupportStreamingInstall { get; }

	IPlatformStreamingInstall PlatformStreamingInstall { get; }

	IPlatformEntitlement PlatformEntitlements { get; }

	IKsivaProvider HydraKsivaProvider { get; }

	IProsProvider ProsKsivaProvider { get; }

	IPlatformSocial PlatformSocial { get; }

	IPlatformMessage PlatformMessage { get; }

	IPlatformProfanity PlatformProfanity { get; }

	bool SwitchSouthAndEastGamepadButtons { get; }

	event Action<Action> OnApplicationResume;

	event Action<Action> OnApplicationSuspend;

	event Action OnApplicationFocus;

	event Action OnApplicationUnfocus;

	event Action OnNetworkConnectionLost;

	event Action OnNetworkConnectionGained;

	string GetSystemLanguage();

	bool IsNetworkConnectionAvailable();
}
