using System;
using System.Collections.Generic;
using Platforms.Social;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Platforms;

public interface IPlatformUserData
{
	string GetPlatformDisplayName();

	string GetPlatformUniqueUserID();

	string GetPlatformNetworkAccountUserID();

	bool IsSignedInOnline();

	int GetInputDevicePlatformSerialNumber();

	List<InputDevice> GetUnityInputDevices();

	List<int> GetUnityInputDevicesIds();

	int GetInputDeviceSlot();

	InputUser GetUnityInputUser();

	void GetAvatar(Action<OperationResult, byte[]> resultCallback);
}
