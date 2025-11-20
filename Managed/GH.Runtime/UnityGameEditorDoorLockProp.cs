using ScenarioRuleLibrary;
using UnityEngine;

public class UnityGameEditorDoorLockProp : MonoBehaviour
{
	private UnityGameEditorObject m_ParentDoor;

	private void Start()
	{
		m_ParentDoor = GetComponentInParent<UnityGameEditorObject>();
		if (m_ParentDoor != null && m_ParentDoor.PropObject != null && m_ParentDoor.PropObject is CObjectDoor { Activated: not false })
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
