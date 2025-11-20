using SM.Gamepad;

namespace Script.GUI;

public class SelectableIsInteractableNavigationFilter : BaseNavigationFilter
{
	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		if (navigationElement is IUiNavigationSelectable uiNavigationSelectable)
		{
			return uiNavigationSelectable.ControlledSelectable.IsInteractable();
		}
		return false;
	}
}
