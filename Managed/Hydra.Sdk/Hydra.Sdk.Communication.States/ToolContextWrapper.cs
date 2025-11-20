using Hydra.Api.Infrastructure.Context;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class ToolContextWrapper : IHydraSdkStateWrapper
{
	public ToolContext Context { get; private set; }

	public ToolContextWrapper(ToolContext context)
	{
		Context = context;
	}
}
