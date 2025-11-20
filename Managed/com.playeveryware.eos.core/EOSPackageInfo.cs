public static class EOSPackageInfo
{
	public static readonly string ConfigFileName = "EpicOnlineServicesConfig.json";

	public const string UnknownVersion = "?.?.?";

	public static string GetPackageName()
	{
		return "com.playeveryware.eos";
	}

	public static string GetPackageVersion()
	{
		return "2.3.3";
	}
}
