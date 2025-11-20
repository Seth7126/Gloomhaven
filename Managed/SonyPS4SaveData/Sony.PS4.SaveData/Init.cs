using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

internal class Init
{
	[DllImport("SaveData")]
	private static extern int PrxSaveDataSetThreadAffinity(ThreadSettingsNative settings, out APIResult result);

	internal static void SetThreadAffinity(ThreadSettingsNative settings)
	{
		PrxSaveDataSetThreadAffinity(settings, out var _);
	}
}
