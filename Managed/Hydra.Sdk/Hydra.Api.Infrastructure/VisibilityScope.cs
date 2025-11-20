using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure;

public enum VisibilityScope
{
	[OriginalName("NONE")]
	None,
	[OriginalName("BASE")]
	Base,
	[OriginalName("TITLE")]
	Title
}
