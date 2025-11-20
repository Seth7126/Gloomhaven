using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class HasSameTransformsLinked : BaseNavigationFilter
{
	[SerializeField]
	private BaseTransformLinker _withLinker;

	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		BaseTransformLinker component = navigationElement.GameObject.GetComponent<BaseTransformLinker>();
		return _withLinker.LinkedTransform == component.LinkedTransform;
	}
}
