using Pros.Sdk;

namespace Platforms.ProsOrHydra;

public interface IProsSettingsProvider
{
	SdkEnvironment Environment { get; }

	string ProsTitleId { get; }

	string ProsSecretKey { get; }

	string ClientVersion { get; }

	int ConnectionCheckCooldown { get; }
}
