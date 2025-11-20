using SM.Gamepad;

namespace Script.GUI;

public static class HotkeyContainerUtils
{
	public static IHotkeySession GetSessionOrEmpty(this IHotkeyContainer hotkeyContainer, IHotkeySession session)
	{
		if (InputManager.GamePadInUse && hotkeyContainer != null && session != null)
		{
			return hotkeyContainer.GetSession(session);
		}
		return new EmptyHotkeySession();
	}

	public static IHotkeySession GetSessionOrEmpty(this IHotkeyContainer hotkeyContainer)
	{
		if (InputManager.GamePadInUse && hotkeyContainer != null)
		{
			return hotkeyContainer.GetSession();
		}
		return new EmptyHotkeySession();
	}
}
