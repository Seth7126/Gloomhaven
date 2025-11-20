using Platforms.Epic;
using Platforms.GameCore;
using Platforms.Generic;
using Platforms.PS4;
using Platforms.PS5;
using Platforms.PSShared;
using Platforms.ProsOrHydra;
using Platforms.Steam;

namespace Platforms;

public interface IGameProvider : IProjectSpecificDependenciesGeneric, IProjectSpecificDependenciesPS4, IProjectSpecificDependenciesPSShared, IProjectSpecificDependenciesPS5, IProjectSpecificDependenciesGameCore, IProjectSpecificDependenciesSteam, IProjectSpecificDependenciesEpic
{
	IAppFlowInformer AppFlowInformer { get; }

	IProsSettingsProvider ProsSettingsProvider { get; }

	IHydraSettingsProvider HydraSettingsProvider { get; }

	bool PlayerOnline { get; }

	bool CrossPlayTurnedOn { get; }

	bool IsSignInUIRequired { get; }

	bool IsUserActive(IPlatformUserData userData);

	void ReturnToInitialInteractiveScreen(bool isSignInUIRequired = false);

	void ShowJoystickDisconnectionMessage();

	void HideJoystickDisconnectionMessage();
}
