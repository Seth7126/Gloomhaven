using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class OnRightNavigationFilter : BaseNavigationFilter
{
	[SerializeField]
	private UiNavigationBase _compareToElement;

	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		return _compareToElement.NavigationPosition.x < navigationElement.NavigationPosition.x;
	}
}
