using Platforms.PSShared;

namespace Platforms.PS4;

public interface IProjectSpecificDependenciesPS4 : IProjectSpecificDependenciesPSShared
{
	INpToolkitSettingsProvider NpToolkitSettingsProvider { get; }

	IEntitlementsProviderPS4 EntitlementsProviderPS4 { get; }

	IHydraProsSettingsProviderPS4 HydraProsSettingsProviderPS4 { get; }
}
