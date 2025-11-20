using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class NotChildNavigationFilter : BaseNavigationFilter
{
	[SerializeField]
	private Transform _parent;

	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		return !navigationElement.GameObject.transform.IsChildOf(_parent);
	}
}
