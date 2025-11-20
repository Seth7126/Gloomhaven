namespace SM.Gamepad;

public interface INavigationSourceHandler
{
	bool IsAllowedToNavigate(UIActionBaseEventData baseEventData);
}
