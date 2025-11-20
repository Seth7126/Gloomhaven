using SM.Gamepad;

namespace Script.GUI;

public class ActiveInHierarchyNavigationFilter : BaseNavigationFilter
{
	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		return navigationElement.GameObject.activeInHierarchy;
	}
}
