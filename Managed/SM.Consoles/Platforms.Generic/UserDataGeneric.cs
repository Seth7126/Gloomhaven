using System;
using System.Collections.Generic;
using Platforms.Social;
using SM.Utils;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Platforms.Generic;

public class UserDataGeneric : IPlatformUserData
{
	private readonly string _platformUserName;

	private readonly string _platformUserId;

	private readonly int _slot;

	private readonly InputUser _matchingUnityUser;

	private string _getInGameNickName;

	public UserDataGeneric(string platformUserName, string platformUserId, int slot, InputUser matchingUnityUser)
	{
		_platformUserName = platformUserName;
		_platformUserId = platformUserId;
		_slot = slot;
		_matchingUnityUser = matchingUnityUser;
	}

	public string GetPlatformDisplayName()
	{
		return _platformUserName;
	}

	public string GetPlatformUniqueUserID()
	{
		return _platformUserId;
	}

	public string GetPlatformNetworkAccountUserID()
	{
		return _platformUserId;
	}

	public bool IsSignedInOnline()
	{
		return false;
	}

	public string GetInGameNickName()
	{
		return _getInGameNickName;
	}

	public void SetInGameNickName(string nickName)
	{
		_getInGameNickName = nickName;
	}

	public int GetInputDevicePlatformSerialNumber()
	{
		return 0;
	}

	public int GetInputDeviceSlot()
	{
		return _slot;
	}

	public InputUser GetUnityInputUser()
	{
		return _matchingUnityUser;
	}

	public void GetAvatar(Action<OperationResult, byte[]> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError, null);
	}

	public List<InputDevice> GetUnityInputDevices()
	{
		List<InputDevice> list = new List<InputDevice>();
		if (!_matchingUnityUser.valid)
		{
			return list;
		}
		foreach (InputDevice pairedDevice in _matchingUnityUser.pairedDevices)
		{
			list.Add(pairedDevice);
		}
		foreach (InputDevice lostDevice in _matchingUnityUser.lostDevices)
		{
			list.Add(lostDevice);
		}
		if (list.Count == 0)
		{
			LogUtils.LogError("[UserDataGeneric] GetUnityInputDevices() Was not able to match user " + _platformUserName + " to any input device. Empty list returned.");
		}
		return list;
	}

	public List<int> GetUnityInputDevicesIds()
	{
		List<int> list = new List<int>();
		foreach (InputDevice unityInputDevice in GetUnityInputDevices())
		{
			list.Add(unityInputDevice.deviceId);
		}
		if (list.Count == 0)
		{
			LogUtils.LogError("[UserDataGeneric] GetUnityInputDevicesIds() Was not able to detect any suitable input devices. Null returned.");
			return null;
		}
		return list;
	}
}
