using System;
using SM.Utils;

namespace Platforms.Generic;

public class PlatformMessageGeneric : IPlatformMessage, IDisposable
{
	public bool IsSystemsMessagesSupported => false;

	public void ShowSystemMessage(IPlatformMessage.MessageType msgType, string titleText, string msgText, Action<IPlatformMessage.MessageResult> callBack = null)
	{
		if (!IsSystemsMessagesSupported)
		{
			LogUtils.LogError("[SM.Consoles PlatformMessageGeneric] Something tried to display system message. \n  \"" + msgText + "\", but system messages are not supported on this platform!");
			callBack?.Invoke(IPlatformMessage.MessageResult.Unknown);
			return;
		}
		throw new NotImplementedException();
	}

	public void Dispose()
	{
	}
}
