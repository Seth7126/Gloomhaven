using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.Cards_Hand;

[CreateAssetMenu(menuName = "Navigation Action/FullCardFocusAction", fileName = "FullCardFocusAction")]
public class FullCardFocusAction : NavigationAction
{
	public override void HandleAction(NavigationActionArgs args)
	{
		FullCardHandViewer instance = Singleton<FullCardHandViewer>.Instance;
		if (!(instance == null) && instance.IsActive && args.NavigationElement.TryGetComponent<RectTransform>(out var component))
		{
			instance.CardNavigationTranslator.Enter(component);
		}
	}
}
