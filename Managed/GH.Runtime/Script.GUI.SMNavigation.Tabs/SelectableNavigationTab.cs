using SM.Gamepad;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Script.GUI.SMNavigation.Tabs;

public class SelectableNavigationTab : MonoBehaviour, INavigationTab
{
	public void Select()
	{
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}

	public void Deselect()
	{
		EventSystem.current.SetSelectedGameObject(null);
	}
}
