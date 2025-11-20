using UnityEngine;
using UnityEngine.InputSystem;

public class TestKeyToggle : MonoBehaviour
{
	[SerializeField]
	private Key inputKey;

	[SerializeField]
	private GameObject element;

	private void Update()
	{
		if (InputManager.GetKeyDown(inputKey))
		{
			element.SetActive(!element.activeSelf);
		}
	}
}
