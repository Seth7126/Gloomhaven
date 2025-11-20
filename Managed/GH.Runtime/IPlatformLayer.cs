using Platforms;

internal interface IPlatformLayer
{
	bool IsConsole { get; }

	string PlatformID { get; }

	void Initialize(IPlatform platform);

	void Shutdown();

	bool IsPlatformPartnerIDCorrect(int partnerID);

	DeviceType GetCurrentPlatform();

	string GetSystemLanguage();
}
