using GLOOM;

public static class UIMapNotifications
{
	private const string NOTIFICATION_TAG = "MAP";

	public static void ShowTownUnlocked()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification(new UINotificationManager.NotificationData
		{
			titleLoc = "GLOOMHAVEN",
			message = LocalizationManager.GetTranslation("GUI_TOWN_UNLOCKED_NOTIFICATION"),
			icon = UIInfoTools.Instance.GetGuildmasterModeSprite(EGuildmasterMode.MercenaryLog)
		}, UIInfoTools.Instance.defaultNotificationDuration, null, "MAP");
	}

	public static void HideAllNotifications()
	{
		Singleton<UINotificationManager>.Instance.HideNotificationsByTag("MAP");
	}
}
