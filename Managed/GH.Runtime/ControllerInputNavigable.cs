using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerInputNavigable : ControllerInputElement
{
	[SerializeField]
	private List<Selectable> m_Selectables;

	[SerializeField]
	private Navigation.Mode m_FocusNavigation = Navigation.Mode.Automatic;

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		foreach (Selectable selectable in m_Selectables)
		{
			Navigation navigation = selectable.navigation;
			navigation.mode = Navigation.Mode.None;
			selectable.navigation = navigation;
		}
	}

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		foreach (Selectable selectable in m_Selectables)
		{
			Navigation navigation = selectable.navigation;
			navigation.mode = m_FocusNavigation;
			selectable.navigation = navigation;
		}
	}
}
