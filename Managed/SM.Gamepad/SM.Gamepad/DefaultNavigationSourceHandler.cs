namespace SM.Gamepad;

public class DefaultNavigationSourceHandler : INavigationSourceHandler
{
	public bool IsAllowedToNavigate(UIActionBaseEventData baseEventData)
	{
		return baseEventData.UINavigationSourceType == UINavigationSourceType.Stick;
	}
}
