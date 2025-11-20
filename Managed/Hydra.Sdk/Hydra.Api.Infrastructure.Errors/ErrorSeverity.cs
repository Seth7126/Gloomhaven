using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Errors;

public enum ErrorSeverity
{
	[OriginalName("ERROR_SEVERITY_UNDEFINED")]
	Undefined,
	[OriginalName("ERROR_SEVERITY_FATAL")]
	Fatal,
	[OriginalName("ERROR_SEVERITY_CRITICAL")]
	Critical,
	[OriginalName("ERROR_SEVERITY_MAJOR")]
	Major,
	[OriginalName("ERROR_SEVERITY_MINOR")]
	Minor,
	[OriginalName("ERROR_SEVERITY_EXPECTED")]
	Expected
}
