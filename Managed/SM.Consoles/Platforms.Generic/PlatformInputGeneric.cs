using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Platforms.Generic;

internal class PlatformInputGeneric : IPlatformInput, IDisposable
{
	private readonly IGameProvider _gameProvider;

	private readonly UserManagementGeneric _userManagement;

	private readonly List<InputDevice> _inputDevices = new List<InputDevice>();

	private readonly bool _isDevicePairingIncluded;

	private int _i;

	public bool IsOnScreenKeyboardShown => false;

	public event Action InputDeviceChangeEvent;

	public PlatformInputGeneric(IGameProvider gameProvider, UserManagementGeneric userManagement, bool isDevicePairingIncluded)
	{
		_gameProvider = gameProvider;
		_userManagement = userManagement;
		_isDevicePairingIncluded = isDevicePairingIncluded;
		BuildInitialInputUsers();
		InputSystem.onDeviceChange += InputSystemOnDeviceChange;
	}

	public void Dispose()
	{
		InputSystem.onDeviceChange -= InputSystemOnDeviceChange;
	}

	private void InputSystemOnDeviceChange(InputDevice inputDevice, InputDeviceChange changeType)
	{
		IPlatformUserData platformUserData = FindPlatformUserForInputDevice(inputDevice);
		if ((!_isDevicePairingIncluded || platformUserData != null) && (changeType == InputDeviceChange.Removed || changeType == InputDeviceChange.Disconnected))
		{
			if (_gameProvider.IsUserActive(platformUserData))
			{
				_gameProvider.ShowJoystickDisconnectionMessage();
			}
			else
			{
				_userManagement.RemovePlatformUser(platformUserData);
			}
		}
		if (changeType == InputDeviceChange.Reconnected && (!_isDevicePairingIncluded || platformUserData != null) && _gameProvider.IsUserActive(platformUserData))
		{
			_gameProvider.HideJoystickDisconnectionMessage();
		}
		if (changeType == InputDeviceChange.Added && platformUserData == null)
		{
			_userManagement.AddPlatformUser(MakeNewPlatformUser(inputDevice));
		}
		this.InputDeviceChangeEvent?.Invoke();
	}

	private IPlatformUserData FindPlatformUserForInputDevice(InputDevice inputDevice)
	{
		foreach (IPlatformUserData currentUser in _userManagement.GetCurrentUsers())
		{
			if (currentUser.GetUnityInputDevices().Contains(inputDevice))
			{
				return currentUser;
			}
		}
		return null;
	}

	private void BuildInitialInputUsers()
	{
		DetectUnityInputDevices();
		BuildUnityUsers();
	}

	private void DetectUnityInputDevices()
	{
		_inputDevices.Clear();
		foreach (InputDevice device in InputSystem.devices)
		{
			if (device.GetType().IsSubclassOf(typeof(Gamepad)) || device.GetType().IsSubclassOf(typeof(Keyboard)))
			{
				_inputDevices.Add(device);
			}
		}
	}

	private void BuildUnityUsers()
	{
		foreach (InputDevice inputDevice in _inputDevices)
		{
			_userManagement.AddPlatformUser(MakeNewPlatformUser(inputDevice));
		}
	}

	private IPlatformUserData MakeNewPlatformUser(InputDevice inputDevice)
	{
		InputUser matchingUnityUser = (_isDevicePairingIncluded ? InputUser.PerformPairingWithDevice(inputDevice) : default(InputUser));
		UserDataGeneric result = new UserDataGeneric("GenericUser" + _i, "GenericId-" + _i, _i, matchingUnityUser);
		_i++;
		return result;
	}

	public void ShowOnScreenKeyboard(string platformUserId, string defaultText, string title, string message, uint maxTextlength, OnScreenKeyboardEndHandler onFinishedInputCallBack)
	{
	}

	public void InitializeUserDevice(int deviceId)
	{
	}
}
