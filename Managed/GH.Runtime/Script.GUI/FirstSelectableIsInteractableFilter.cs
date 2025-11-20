using SM.Gamepad;

namespace Script.GUI;

public class FirstSelectableIsInteractableFilter : BaseNavigationFilter
{
	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		if (navigationElement is IUiNavigationSelectable uiNavigationSelectable)
		{
			return uiNavigationSelectable.ControlledSelectable.IsInteractable();
		}
		if (navigationElement is IUiNavigationNode uiNavigationNode)
		{
			return UiNavigationUtils.FindFirstSelectable(uiNavigationNode.Elements).ControlledSelectable.IsInteractable();
		}
		return false;
	}
}
