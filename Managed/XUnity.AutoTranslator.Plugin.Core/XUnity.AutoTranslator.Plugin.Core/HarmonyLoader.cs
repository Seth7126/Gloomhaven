namespace XUnity.AutoTranslator.Plugin.Core;

internal static class HarmonyLoader
{
	public static void Load()
	{
		try
		{
			Harmony12Loader.Load();
		}
		catch
		{
		}
	}
}
