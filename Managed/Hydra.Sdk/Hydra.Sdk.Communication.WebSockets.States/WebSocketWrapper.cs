using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.WebSockets.States;

public class WebSocketWrapper : IHydraSdkStateWrapper
{
	public WebSocketClient Client { get; }

	public WebSocketWrapper(WebSocketClient client)
	{
		Client = client;
	}
}
