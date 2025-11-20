using Hydra.Api.Push;

namespace Hydra.Sdk.Communication.WebSockets;

public struct WebSocketMessage
{
	public PushMessageType Type;

	public byte[] Data;

	public int Offset;

	public int Lenght;
}
