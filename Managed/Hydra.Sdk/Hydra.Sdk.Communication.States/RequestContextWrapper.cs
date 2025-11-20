using Hydra.Api.Auth;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class RequestContextWrapper : IHydraSdkStateWrapper
{
	public HydraRequestContext Context { get; private set; }

	public RequestContextWrapper(HydraRequestContext requestContext)
	{
		Context = requestContext;
	}
}
