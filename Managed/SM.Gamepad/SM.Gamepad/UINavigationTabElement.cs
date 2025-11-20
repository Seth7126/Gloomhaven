using UnityEngine;

namespace SM.Gamepad;

public class UINavigationTabElement : MonoBehaviour
{
	[SerializeField]
	private bool includeInactive;

	[SerializeField]
	private bool forceRefresh;

	private IInteractable _interactable;

	private INavigationTab _navigationTab;

	public bool IsInteractable
	{
		get
		{
			if (_interactable == null)
			{
				_interactable = GetComponentInChildren<IInteractable>();
			}
			if (_interactable != null)
			{
				return _interactable.IsInteractable;
			}
			return false;
		}
	}

	public INavigationTab Element
	{
		get
		{
			if (forceRefresh)
			{
				_navigationTab = GetComponentInChildren<INavigationTab>(includeInactive);
			}
			else if (_navigationTab == null)
			{
				_navigationTab = GetComponentInChildren<INavigationTab>(includeInactive);
			}
			return _navigationTab;
		}
	}
}
