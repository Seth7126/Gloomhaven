using System;
using System.Threading.Tasks;
using Platforms.Activities;
using Platforms.PlatformAchievements;
using Platforms.PlatformData;
using Platforms.Profanity;
using Platforms.ProsOrHydra;
using Platforms.Social;
using Platforms.Utils;
using UnityEngine;

namespace Platforms;

public abstract class PlatformBase : IPlatform, IAsyncDisposable
{
	protected GameObject PlatformDaemon { get; }

	protected IPlatformCoroutineRunner PlatformCoroutineRunner { get; }

	protected IUpdater PlatformUpdater { get; }

	public abstract IPlatformUserManagement UserManagement { get; }

	public abstract IPlatformInput PlatformInput { get; }

	public abstract IPlatformData PlatformData { get; }

	public abstract IPlatformAchievements PlatformAchievements { get; }

	public bool IsSupportActivities => PlatformActivities != null;

	public abstract IPlatformActivities PlatformActivities { get; }

	public bool IsSupportStreamingInstall => PlatformStreamingInstall != null;

	public abstract IPlatformStreamingInstall PlatformStreamingInstall { get; }

	public abstract IPlatformEntitlement PlatformEntitlements { get; }

	public abstract IKsivaProvider HydraKsivaProvider { get; }

	public abstract IProsProvider ProsKsivaProvider { get; }

	public abstract IPlatformSocial PlatformSocial { get; }

	public abstract IPlatformMessage PlatformMessage { get; }

	public abstract IPlatformProfanity PlatformProfanity { get; }

	public virtual bool SwitchSouthAndEastGamepadButtons { get; }

	public virtual event Action<Action> OnApplicationResume;

	public virtual event Action<Action> OnApplicationSuspend;

	public virtual event Action OnApplicationFocus;

	public virtual event Action OnApplicationUnfocus;

	public virtual event Action OnNetworkConnectionLost;

	public virtual event Action OnNetworkConnectionGained;

	protected PlatformBase(IGameProvider gameProvider)
	{
		PlatformDaemon = new GameObject("PlaftormDaemon");
		UnityEngine.Object.DontDestroyOnLoad(PlatformDaemon);
		PlatformCoroutineRunner = PlatformDaemon.AddComponent<PlatformCoroutineRunner>();
		PlatformUpdater = PlatformDaemon.AddComponent<Updater>();
	}

	public virtual string GetSystemLanguage()
	{
		return "English";
	}

	public virtual bool IsNetworkConnectionAvailable()
	{
		return true;
	}

	public virtual ValueTask DisposeAsync()
	{
		if (PlatformDaemon != null)
		{
			UnityEngine.Object.Destroy(PlatformDaemon);
		}
		return default(ValueTask);
	}
}
