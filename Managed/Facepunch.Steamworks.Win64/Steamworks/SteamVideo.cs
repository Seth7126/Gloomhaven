namespace Steamworks;

public class SteamVideo : SteamClientClass<SteamVideo>
{
	internal static ISteamVideo Internal => SteamClientClass<SteamVideo>.Interface as ISteamVideo;

	public static bool IsBroadcasting
	{
		get
		{
			int pnNumViewers = 0;
			return Internal.IsBroadcasting(ref pnNumViewers);
		}
	}

	public static int NumViewers
	{
		get
		{
			int pnNumViewers = 0;
			if (!Internal.IsBroadcasting(ref pnNumViewers))
			{
				return 0;
			}
			return pnNumViewers;
		}
	}

	internal override void InitializeInterface(bool server)
	{
		SetInterface(server, new ISteamVideo(server));
		InstallEvents();
	}

	internal static void InstallEvents()
	{
	}
}
