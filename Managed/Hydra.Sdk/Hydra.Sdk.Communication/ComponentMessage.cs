using Hydra.Sdk.Enums;

namespace Hydra.Sdk.Communication;

public class ComponentMessage
{
	public MessageType Message;

	public MessageReason Reason;

	public object[] Args;
}
