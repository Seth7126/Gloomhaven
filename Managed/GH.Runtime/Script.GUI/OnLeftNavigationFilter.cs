using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class OnLeftNavigationFilter : BaseNavigationFilter
{
	[SerializeField]
	private UiNavigationBase _compareToElement;

	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		return navigationElement.NavigationPosition.x < _compareToElement.NavigationPosition.x;
	}
}
