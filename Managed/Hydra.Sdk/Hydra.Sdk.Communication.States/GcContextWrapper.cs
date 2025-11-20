using Hydra.Api.Infrastructure.Context;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class GcContextWrapper : IHydraSdkStateWrapper
{
	public ConfigurationContext Context { get; private set; }

	public GcContextWrapper(ConfigurationContext context)
	{
		Context = context;
	}
}
