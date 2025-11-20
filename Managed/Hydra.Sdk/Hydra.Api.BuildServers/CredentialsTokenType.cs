using Google.Protobuf.Reflection;

namespace Hydra.Api.BuildServers;

public enum CredentialsTokenType
{
	[OriginalName("CREDENTIALS_TOKEN_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("CREDENTIALS_TOKEN_TYPE_SAS_URI")]
	SasUri
}
