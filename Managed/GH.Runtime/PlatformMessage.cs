#define ENABLE_LOGS
using System;
using Platforms;
using SM.Utils;
using UnityEngine;

public class PlatformMessage : MonoBehaviour
{
	private IPlatform _platform;

	private IPlatformMessage _platformMessage;

	public void Initialize(IPlatform platform)
	{
		_platform = platform;
		_platformMessage = _platform.PlatformMessage;
	}

	public bool IsSystemMessagesSupported()
	{
		return _platformMessage.IsSystemsMessagesSupported;
	}

	public virtual void ShowSystemMessage(IPlatformMessage.MessageType msgType, string msgText, Action<IPlatformMessage.MessageResult> callBack)
	{
		if (!IsSystemMessagesSupported())
		{
			LogUtils.LogWarning("[PlatformMessage.cs] Show system message failed, because current platform does not support system messages. callBack with 'Unknown' response called.");
			callBack?.Invoke(IPlatformMessage.MessageResult.Unknown);
		}
		else
		{
			_platformMessage.ShowSystemMessage(msgType, string.Empty, msgText, callBack);
		}
	}
}
