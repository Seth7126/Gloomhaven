using System.Threading.Tasks;

namespace Hydra.Sdk.Communication;

public class ComponentMessager
{
	public delegate Task MessageReceived(ComponentMessage msg);

	public MessageReceived OnMessageReceived;

	public Task BroadcastMessage(ComponentMessage msg)
	{
		return OnMessageReceived(msg);
	}
}
