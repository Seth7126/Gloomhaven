using System;

namespace Platforms;

public interface IPlatformInput : IDisposable
{
	bool IsOnScreenKeyboardShown { get; }

	event Action InputDeviceChangeEvent;

	void ShowOnScreenKeyboard(string platformUserId, string defaultText, string title, string message, uint maxTextLength, OnScreenKeyboardEndHandler onFinishedInputCallBack);
}
