using InControl;
using UnityEngine;

namespace TouchExample;

public class CubeController : MonoBehaviour
{
	private Renderer cachedRenderer;

	private void Start()
	{
		cachedRenderer = GetComponent<Renderer>();
	}

	private void Update()
	{
		InputDevice activeDevice = InputManager.ActiveDevice;
		if (activeDevice != InputDevice.Null && activeDevice != TouchManager.Device)
		{
			TouchManager.ControlsEnabled = false;
		}
		cachedRenderer.material.color = GetColorFromActionButtons(activeDevice);
		base.transform.Rotate(Vector3.down, 500f * Time.deltaTime * activeDevice.Direction.X, Space.World);
		base.transform.Rotate(Vector3.right, 500f * Time.deltaTime * activeDevice.Direction.Y, Space.World);
	}

	private static Color GetColorFromActionButtons(InputDevice inputDevice)
	{
		if ((bool)inputDevice.Action1)
		{
			return Color.green;
		}
		if ((bool)inputDevice.Action2)
		{
			return Color.red;
		}
		if ((bool)inputDevice.Action3)
		{
			return Color.blue;
		}
		if ((bool)inputDevice.Action4)
		{
			return Color.yellow;
		}
		return Color.white;
	}

	private void OnGUI()
	{
		float num = 10f;
		int touchCount = TouchManager.TouchCount;
		for (int i = 0; i < touchCount; i++)
		{
			InControl.Touch touch = TouchManager.GetTouch(i);
			string text = i + ": fingerId = " + touch.fingerId;
			text = text + ", phase = " + touch.phase;
			string text2 = text;
			Vector2 startPosition = touch.startPosition;
			text = text2 + ", startPosition = " + startPosition.ToString();
			string text3 = text;
			startPosition = touch.position;
			text = text3 + ", position = " + startPosition.ToString();
			if (touch.IsMouse)
			{
				text = text + ", mouseButton = " + touch.mouseButton;
			}
			GUI.Label(new Rect(10f, num, Screen.width, num + 15f), text);
			num += 20f;
		}
	}
}
