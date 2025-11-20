using Google.Protobuf.Reflection;

namespace Hydra.Api.EndpointDispatcher;

public enum EndpointScheme
{
	[OriginalName("ENDPOINT_SCHEME_SECURED")]
	Secured,
	[OriginalName("ENDPOINT_SCHEME_UNSECURED")]
	Unsecured,
	[OriginalName("ENDPOINT_SCHEME_UDP")]
	Udp
}
