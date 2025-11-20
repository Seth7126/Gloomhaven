using UnityEngine;

namespace SRDebugger;

public class GamepadPointerControllerActivator : MonoBehaviour
{
	[SerializeField]
	private GamepadPointerController _gamepadPointerController;

	private void Start()
	{
		if (SRDebug.Instance.IsDebugPanelVisible)
		{
			Activate();
		}
		SRDebug.Instance.PanelVisibilityChanged += DebugPanelVisibilityChanged;
	}

	private void OnDestroy()
	{
		if (SRDebug.Instance != null)
		{
			SRDebug.Instance.PanelVisibilityChanged -= DebugPanelVisibilityChanged;
		}
	}

	private void DebugPanelVisibilityChanged(bool isVisible)
	{
		if (isVisible)
		{
			Activate();
		}
		else
		{
			Deactivate();
		}
	}

	private void Activate()
	{
		_gamepadPointerController.enabled = true;
	}

	private void Deactivate()
	{
		_gamepadPointerController.enabled = false;
	}
}
