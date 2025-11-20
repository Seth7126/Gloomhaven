using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class NotSameGameObjectFilter : BaseNavigationFilter
{
	[SerializeField]
	private GameObject _objectToCompare;

	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		return _objectToCompare != navigationElement.GameObject;
	}
}
