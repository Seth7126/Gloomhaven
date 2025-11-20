using SM.Gamepad;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation.InteractableWrappers;

[RequireComponent(typeof(Selectable))]
public class SelectableInteractionWrapper : MonoBehaviour, IInteractable
{
	private Selectable _selectable;

	public bool IsInteractable
	{
		get
		{
			if (_selectable == null)
			{
				_selectable = GetComponent<Selectable>();
			}
			return _selectable.interactable;
		}
	}
}
