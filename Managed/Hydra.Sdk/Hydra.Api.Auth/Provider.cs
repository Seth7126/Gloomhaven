using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public enum Provider
{
	[OriginalName("PROVIDER_STEAM")]
	Steam = 0,
	[OriginalName("PROVIDER_EOS")]
	Eos = 1,
	[OriginalName("PROVIDER_STADIA")]
	Stadia = 2,
	[OriginalName("PROVIDER_XBOX")]
	Xbox = 3,
	[OriginalName("PROVIDER_PSN")]
	Psn = 4,
	[OriginalName("PROVIDER_NINTENDO")]
	Nintendo = 5,
	[OriginalName("PROVIDER_MS_STORE")]
	MsStore = 6,
	[OriginalName("PROVIDER_OCULUS")]
	Oculus = 7,
	[OriginalName("PROVIDER_HYDRA")]
	Hydra = 100,
	[OriginalName("PROVIDER_TOOL")]
	Tool = 101,
	[OriginalName("PROVIDER_STANDALONE")]
	Standalone = 102
}
