using System.Collections.Generic;
using UnityEngine;

public class ControllerInputGO : ControllerInputElement
{
	[SerializeField]
	private bool m_SetActiveOnEnabledController = true;

	[SerializeField]
	private List<GameObject> m_ControllerObjects;

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		for (int i = 0; i < m_ControllerObjects.Count; i++)
		{
			m_ControllerObjects[i].SetActive(m_SetActiveOnEnabledController);
		}
	}

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		for (int i = 0; i < m_ControllerObjects.Count; i++)
		{
			m_ControllerObjects[i].SetActive(!m_SetActiveOnEnabledController);
		}
	}
}
