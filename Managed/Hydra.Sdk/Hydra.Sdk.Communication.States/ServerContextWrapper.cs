using Hydra.Api.Infrastructure.Context;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class ServerContextWrapper : IHydraSdkStateWrapper
{
	public ServerContext Context { get; private set; }

	public ServerContextWrapper(ServerContext context)
	{
		Context = context;
	}
}
