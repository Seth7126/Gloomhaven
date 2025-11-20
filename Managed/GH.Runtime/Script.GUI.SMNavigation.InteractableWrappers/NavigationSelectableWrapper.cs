using SM.Gamepad;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation.InteractableWrappers;

[RequireComponent(typeof(UINavigationSelectable))]
public class NavigationSelectableWrapper : MonoBehaviour, IInteractable
{
	private Selectable _selectable;

	public bool IsInteractable
	{
		get
		{
			if (_selectable == null)
			{
				_selectable = GetComponent<UINavigationSelectable>().ControlledSelectable;
			}
			return _selectable.interactable;
		}
	}
}
