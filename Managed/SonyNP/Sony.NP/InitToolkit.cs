using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

[StructLayout(LayoutKind.Sequential)]
public class InitToolkit
{
	public ContentRestriction contentRestrictions;

	[Obsolete("serverPushNotifications is deprecated, please use SetPushNotificationsFlags instead.")]
	public ServerPushNotifications serverPushNotifications;

	private PushNotificationsFlags serverPushNotificationsFlags;

	[MarshalAs(UnmanagedType.I1)]
	private bool notificationsFlagsSet;

	public ThreadSettings threadSettings;

	public MemoryPools memoryPools;

	public InitToolkit()
	{
		contentRestrictions.Init();
		serverPushNotifications.Init();
		threadSettings.Init();
		memoryPools.Init();
		serverPushNotificationsFlags = PushNotificationsFlags.None;
		notificationsFlagsSet = false;
	}

	public void SetPushNotificationsFlags(PushNotificationsFlags pushNotifications)
	{
		serverPushNotificationsFlags = pushNotifications;
		notificationsFlagsSet = true;
	}

	public void CheckValid()
	{
		if ((threadSettings.affinity & (Affinity)3) != 0)
		{
			throw new NpToolkitException("Can't set thread affinity to Core 0 or Core 1 as this will interfere with the main loop and gfx threads.");
		}
	}
}
