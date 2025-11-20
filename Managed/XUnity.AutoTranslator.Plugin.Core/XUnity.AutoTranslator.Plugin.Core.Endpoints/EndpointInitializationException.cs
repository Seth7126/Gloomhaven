using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints;

public class EndpointInitializationException : Exception
{
	public EndpointInitializationException()
	{
	}

	public EndpointInitializationException(string message)
		: base(message)
	{
	}

	public EndpointInitializationException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
