using System.Threading.Tasks;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States.Core;

namespace Hydra.Sdk.Interfaces;

public interface IHydraSdkComponent
{
	Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger);

	int GetDisposePriority();

	Task Unregister();
}
