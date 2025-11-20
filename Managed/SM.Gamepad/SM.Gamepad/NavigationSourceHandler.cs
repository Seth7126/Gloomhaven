using UnityEngine;

namespace SM.Gamepad;

public class NavigationSourceHandler : MonoBehaviour, INavigationSourceHandler
{
	[SerializeField]
	private UINavigationSourceType uiNavigationSourceType;

	public bool IsAllowedToNavigate(UIActionBaseEventData baseEventData)
	{
		return uiNavigationSourceType == baseEventData.UINavigationSourceType;
	}
}
