using Hydra.Api.Infrastructure.Context;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class UserContextWrapper : IHydraSdkStateWrapper
{
	public UserContext Context { get; private set; }

	public UserContextWrapper(UserContext userContext)
	{
		Context = userContext;
	}
}
