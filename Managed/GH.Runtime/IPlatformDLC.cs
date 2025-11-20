using Platforms;
using ScenarioRuleLibrary;

public interface IPlatformDLC
{
	void Initialize(IPlatform platform);

	bool UserInstalledDLC(DLCRegistry.EDLCKey dlc);

	bool UserOwnsDLC(DLCRegistry.EDLCKey dlc);

	void OpenPlatformStoreDLCOverlay(DLCRegistry.EDLCKey dlc);
}
