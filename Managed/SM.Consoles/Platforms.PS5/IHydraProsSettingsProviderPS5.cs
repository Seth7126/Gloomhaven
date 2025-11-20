namespace Platforms.PS5;

public interface IHydraProsSettingsProviderPS5
{
	string SceClientIdForTokenVerification { get; }

	string SceClientSecret { get; }
}
