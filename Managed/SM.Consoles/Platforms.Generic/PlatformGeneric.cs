using System;
using System.Threading.Tasks;
using Platforms.Activities;
using Platforms.PlatformAchievements;
using Platforms.PlatformData;
using Platforms.Profanity;
using Platforms.ProsOrHydra;
using Platforms.Social;
using Platforms.Utils;

namespace Platforms.Generic;

public class PlatformGeneric : PlatformBase
{
	private readonly UserManagementGeneric _userManagement;

	private readonly PlatformInputGeneric _platformInput;

	private readonly PlatformDataGeneric _platformData;

	private readonly PlatformAchievementsGeneric _platformAchievementsGeneric;

	private readonly PlatformEntitlementGeneric _platformEntitlementGeneric;

	private readonly HydraAnalyticsBase _hydra;

	private readonly PlatformProsGeneric _prosGeneric;

	private readonly PlatformSocialGeneric _platformSocial;

	private readonly PlatformActivities _platformActivities;

	private readonly PlatformMessageGeneric _platformMessage;

	private readonly PlatformProfanityGeneric _platformProfanity;

	public override IPlatformUserManagement UserManagement => _userManagement;

	public override IPlatformInput PlatformInput => _platformInput;

	public override IPlatformData PlatformData => _platformData;

	public override IPlatformAchievements PlatformAchievements => _platformAchievementsGeneric;

	public override IPlatformActivities PlatformActivities => _platformActivities;

	public override IPlatformStreamingInstall PlatformStreamingInstall => null;

	public override IPlatformEntitlement PlatformEntitlements => _platformEntitlementGeneric;

	public override IKsivaProvider HydraKsivaProvider => _hydra;

	public override IProsProvider ProsKsivaProvider => _prosGeneric;

	public override IPlatformSocial PlatformSocial => _platformSocial;

	public override IPlatformMessage PlatformMessage => _platformMessage;

	public override IPlatformProfanity PlatformProfanity => _platformProfanity;

	public override event Action<Action> OnApplicationResume;

	public override event Action<Action> OnApplicationSuspend;

	public override event Action OnNetworkConnectionLost;

	public override event Action OnNetworkConnectionGained;

	public PlatformGeneric(IGameProvider gameProvider, bool initHydra, bool initEntitlements, bool initPros, bool isDevicePairingIncluded)
		: base(gameProvider)
	{
		_userManagement = new UserManagementGeneric();
		_platformInput = new PlatformInputGeneric(gameProvider, _userManagement, isDevicePairingIncluded);
		_platformData = new PlatformDataGeneric();
		_platformAchievementsGeneric = new PlatformAchievementsGeneric();
		_platformSocial = new PlatformSocialGeneric();
		_platformMessage = new PlatformMessageGeneric();
		_platformProfanity = new PlatformProfanityGeneric();
		if (initHydra)
		{
			_hydra = CreateHydraAnalytics(gameProvider);
			_hydra.Initialize();
		}
		if (initPros)
		{
			_prosGeneric = new PlatformProsGeneric(base.PlatformUpdater, gameProvider.AppFlowInformer, gameProvider.ProsSettingsProvider, gameProvider.ProsHydraSettingsProviderGeneric, DebugFlags.Warning | DebugFlags.Error);
			_prosGeneric.Initialize();
		}
		if (initEntitlements)
		{
			_platformEntitlementGeneric = new PlatformEntitlementGeneric();
		}
		_platformActivities = new PlatformActivities(gameProvider.ActivitiesProvider, new ActivitiesRequestsControllerGeneric());
	}

	public override async ValueTask DisposeAsync()
	{
		_platformData.Dispose();
		_platformInput.Dispose();
		_userManagement.Dispose();
		if (_hydra != null)
		{
			await _hydra.DisposeAsync();
		}
		if (_prosGeneric != null)
		{
			await _prosGeneric.DisposeAsync();
		}
		await base.DisposeAsync();
	}

	protected virtual HydraAnalyticsBase CreateHydraAnalytics(IGameProvider gameProvider)
	{
		return new PlatformHydraAnalyticsGeneric(base.PlatformUpdater, gameProvider.AppFlowInformer, gameProvider.ProsHydraSettingsProviderGeneric, gameProvider.HydraSettingsProvider, DebugFlags.Warning | DebugFlags.Error);
	}
}
