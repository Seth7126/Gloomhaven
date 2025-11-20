namespace Platforms.PS4;

public interface IHydraProsSettingsProviderPS4
{
	string SceClientIdForTokenVerification { get; }

	string SceClientSecret { get; }
}
