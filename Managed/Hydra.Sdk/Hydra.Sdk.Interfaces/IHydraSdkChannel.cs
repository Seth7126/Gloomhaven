using System.Threading.Tasks;
using Hydra.Sdk.Communication.Channels;
using Hydra.Sdk.Generated;

namespace Hydra.Sdk.Interfaces;

public interface IHydraSdkChannel
{
	ICaller GetInvoker();

	ChannelInfo GetInfo();

	void UpdateToken(string token);

	Task Shutdown();
}
