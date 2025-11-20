using SM.Gamepad;

namespace Script.GUI.SMNavigation.Utils;

public static class UiNavigationBlockerExtension
{
	private static UiNavigationManager _navigationManager = Singleton<UINavigation>.Instance.NavigationManager;

	public static void Block(this UiNavigationBlocker blocker, UiNavigationManager navigationManager = null)
	{
		((navigationManager == null) ? _navigationManager : navigationManager).BlockNavigation(blocker);
	}

	public static void Unblock(this UiNavigationBlocker blocker, UiNavigationManager navigationManager = null)
	{
		((navigationManager == null) ? _navigationManager : navigationManager).UnblockNavigation(blocker);
	}
}
