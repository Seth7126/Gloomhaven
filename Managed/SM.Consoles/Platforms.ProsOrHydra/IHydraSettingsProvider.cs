namespace Platforms.ProsOrHydra;

public interface IHydraSettingsProvider
{
	string HydraEndpoint { get; }

	string HydraTitleId { get; }

	string HydraSecretKey { get; }

	string ClientVersion { get; }

	int ConnectionCheckCooldown { get; }
}
