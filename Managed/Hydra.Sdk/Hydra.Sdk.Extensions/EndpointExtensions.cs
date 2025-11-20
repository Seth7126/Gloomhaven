using System;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Infrastructure;

namespace Hydra.Sdk.Extensions;

public static class EndpointExtensions
{
	public static Uri ToUri(this EndpointInfo endpoint)
	{
		string arg = ((endpoint.Scheme == EndpointScheme.Secured) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp);
		return new Uri($"{arg}://{endpoint.Ip}:{endpoint.Port}");
	}

	public static EndpointInfo ToEndpointInfo(this Uri uri, string endpointName, ServiceVersion version)
	{
		return new EndpointInfo
		{
			Name = endpointName,
			Ip = uri.Host,
			Port = uri.Port,
			Scheme = ((!(uri.Scheme == Uri.UriSchemeHttps)) ? EndpointScheme.Unsecured : EndpointScheme.Secured),
			Version = version
		};
	}
}
