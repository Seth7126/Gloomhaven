using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation;

[CreateAssetMenu(menuName = "Data/HighlightNavAction")]
public class HighlightNavAction : NavigationAction
{
	public override void HandleAction(NavigationActionArgs args)
	{
		args.NavigationElement.GetComponent<UIMainMenuOption>()?.Highlight(isHighlighted: true);
	}
}
