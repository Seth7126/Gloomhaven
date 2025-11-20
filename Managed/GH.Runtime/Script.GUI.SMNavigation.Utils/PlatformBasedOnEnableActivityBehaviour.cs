using System.Collections.Generic;
using UnityEngine;

namespace Script.GUI.SMNavigation.Utils;

public class PlatformBasedOnEnableActivityBehaviour : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _defaultObjects;

	[SerializeField]
	private List<GameObject> _gamePadObjects;

	private void OnEnable()
	{
		ToggleActivity();
	}

	private void ToggleActivity()
	{
		_defaultObjects.ForEach(delegate(GameObject obj)
		{
			obj.SetActive(!InputManager.GamePadInUse);
		});
		_gamePadObjects.ForEach(delegate(GameObject obj)
		{
			obj.SetActive(InputManager.GamePadInUse);
		});
	}
}
