using Platforms.Activities;
using Platforms.PSShared;

namespace Platforms.PS5;

public interface IProjectSpecificDependenciesPS5 : IProjectSpecificDependenciesPSShared
{
	IPsnSettingsProvider PsnSettingsProvider { get; }

	IActivitiesProvider ActivitiesProvider { get; }

	IGameIntentReceiver GameIntentReceiver { get; }

	IEntitlementsProviderPS5 EntitlementsProviderPS5 { get; }

	IHydraProsSettingsProviderPS5 HydraProsSettingsProviderPS5 { get; }
}
