using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Errors;

public enum ErrorCategory
{
	[OriginalName("ERROR_CATEGORY_FATAL")]
	Fatal,
	[OriginalName("ERROR_CATEGORY_CRITICAL")]
	Critical,
	[OriginalName("ERROR_CATEGORY_TERMINAL")]
	Terminal,
	[OriginalName("ERROR_CATEGORY_NONE")]
	None
}
