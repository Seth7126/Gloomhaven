using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class RootTransformLinker : BaseTransformLinker
{
	[SerializeField]
	private UiNavigationBase _navigationElement;

	public override Transform LinkedTransform
	{
		get
		{
			Transform transform = null;
			if (_navigationElement is IUiNavigationRoot)
			{
				return _navigationElement.GameObject.transform;
			}
			return _navigationElement.Root.GameObject.transform;
		}
	}
}
