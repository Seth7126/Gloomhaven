using AsmodeeNet.Foundation;
using UnityEngine;

public class SpeedUpButton : MonoBehaviour
{
	private void Awake()
	{
		InputManager.RegisterToOnPressed(KeyAction.TOGGLE_SPEED, SwitchSpeed);
	}

	private void SwitchSpeed()
	{
		if (CanToggle())
		{
			SaveData.Instance.Global.SpeedUpToggle = !SaveData.Instance.Global.SpeedUpToggle;
		}
	}

	private bool CanToggle()
	{
		if (FFSNetwork.IsOnline)
		{
			return FFSNetwork.IsHost;
		}
		return true;
	}

	private void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			InputManager.UnregisterToOnPressed(KeyAction.TOGGLE_SPEED, SwitchSpeed);
		}
	}
}
