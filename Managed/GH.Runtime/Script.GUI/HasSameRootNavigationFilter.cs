using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class HasSameRootNavigationFilter : BaseNavigationFilter
{
	[SerializeField]
	private UiNavigationBase _withElement;

	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		if (navigationElement is IUiNavigationRoot || _withElement is IUiNavigationRoot)
		{
			return false;
		}
		return _withElement.Root == navigationElement.Root;
	}
}
