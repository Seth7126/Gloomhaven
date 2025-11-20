using SM.Gamepad;

namespace Script.GUI;

public class NotPreviewHandFilter : BaseNavigationFilter
{
	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		return !navigationElement.GameObject.transform.parent.parent.name.Contains("Preview");
	}
}
