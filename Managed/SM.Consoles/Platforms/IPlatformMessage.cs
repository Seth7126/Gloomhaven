using System;

namespace Platforms;

public interface IPlatformMessage : IDisposable
{
	public enum MessageType
	{
		Ok,
		YesNo,
		None,
		OkCancel,
		Cancel
	}

	public enum MessageResult
	{
		Yes,
		No,
		Unknown
	}

	bool IsSystemsMessagesSupported { get; }

	void ShowSystemMessage(MessageType msgType, string titleText, string msgText, Action<MessageResult> callBack);
}
