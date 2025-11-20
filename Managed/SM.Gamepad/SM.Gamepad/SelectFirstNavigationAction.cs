using UnityEngine;

namespace SM.Gamepad;

[CreateAssetMenu(menuName = "Data/Navigation Actions")]
public class SelectFirstNavigationAction : NavigationAction
{
	public override void HandleAction(NavigationActionArgs args)
	{
		if (!(args.NavigationElement == null))
		{
			UiNavigationGroup uiNavigationGroup = args.NavigationElement as UiNavigationGroup;
			if (!(uiNavigationGroup == null))
			{
				uiNavigationGroup.UiNavigationManager.TrySelectFirstIn(uiNavigationGroup);
			}
		}
	}
}
