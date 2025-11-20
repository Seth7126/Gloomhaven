using Platforms.PS5;

namespace Script.PlatformLayer;

public class SamplePsnSettingsProvider : IPsnSettingsProvider
{
	public ulong UdsPoolSize => 262144uL;
}
