using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace InControl;

[UnityEngine.Scripting.Preserve]
public class NewUnityInputDeviceManager : InputDeviceManager
{
	private protected readonly Dictionary<int, NewUnityInputDevice> internalDevices = new Dictionary<int, NewUnityInputDevice>();

	public NewUnityInputDeviceManager()
	{
		Init();
	}

	private protected virtual void Init()
	{
		foreach (UnityEngine.InputSystem.InputDevice device in InputSystem.devices)
		{
			AttachDevice(device);
		}
		InputSystem.onDeviceChange -= OnInputSystemOnDeviceChange;
		InputSystem.onDeviceChange += OnInputSystemOnDeviceChange;
	}

	private protected void OnInputSystemOnDeviceChange(UnityEngine.InputSystem.InputDevice unityDevice, InputDeviceChange inputDeviceChange)
	{
		switch (inputDeviceChange)
		{
		case InputDeviceChange.Added:
			AttachDevice(unityDevice);
			break;
		case InputDeviceChange.Removed:
			DetachDevice(unityDevice);
			break;
		}
	}

	public override void Update(ulong updateTick, float deltaTime)
	{
	}

	private protected void AttachDevice(UnityEngine.InputSystem.InputDevice unityDevice)
	{
		if (unityDevice is Gamepad unityGamepad && !internalDevices.ContainsKey(unityDevice.deviceId))
		{
			NewUnityInputDevice newUnityInputDevice = new NewUnityInputDevice(unityGamepad);
			internalDevices.Add(unityDevice.deviceId, newUnityInputDevice);
			InputManager.AttachDevice(newUnityInputDevice);
		}
	}

	private void DetachDevice(UnityEngine.InputSystem.InputDevice unityDevice)
	{
		if (internalDevices.TryGetValue(unityDevice.deviceId, out var value))
		{
			internalDevices.Remove(unityDevice.deviceId);
			InputManager.DetachDevice(value);
		}
	}

	internal static bool Enable()
	{
		InputManager.AddDeviceManager<NewUnityInputDeviceManager>();
		return true;
	}
}
