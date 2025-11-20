using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation;

[CreateAssetMenu(menuName = "Data/UnHighlightNavAction")]
public class UnHighlightNavAction : NavigationAction
{
	public override void HandleAction(NavigationActionArgs args)
	{
		args.NavigationElement.GetComponent<UIMainMenuOption>()?.Highlight(isHighlighted: false);
	}
}
