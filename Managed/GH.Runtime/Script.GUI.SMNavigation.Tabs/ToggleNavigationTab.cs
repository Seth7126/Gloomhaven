using SM.Gamepad;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Script.GUI.SMNavigation.Tabs;

public class ToggleNavigationTab : MonoBehaviour, INavigationTab
{
	public void Select()
	{
		ExecuteEvents.Execute(base.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
	}

	public void Deselect()
	{
		ExecuteEvents.Execute(base.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
	}
}
