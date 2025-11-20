using UnityEngine;
using UnityEngine.InputSystem;

namespace GraphProgress;

public class Toucher : MonoBehaviour
{
	public void Click()
	{
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out var hitInfo, 100f) && hitInfo.transform.TryGetComponent<VertexView>(out var component))
		{
			component.Click();
		}
	}
}
