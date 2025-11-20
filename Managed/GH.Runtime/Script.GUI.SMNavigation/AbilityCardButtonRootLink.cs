using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation;

public class AbilityCardButtonRootLink : MonoBehaviour
{
	[SerializeField]
	private UiNavigationRoot _root;

	[SerializeField]
	private UiNavigationRoot _secondRoot;

	public UiNavigationRoot Root
	{
		get
		{
			if (_root.Elements.Count != 0)
			{
				return _root;
			}
			return _secondRoot;
		}
	}
}
