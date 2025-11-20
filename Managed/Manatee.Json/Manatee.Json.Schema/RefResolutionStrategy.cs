using System;

namespace Manatee.Json.Schema;

public enum RefResolutionStrategy
{
	[Obsolete("This will be replaced by IgnoreSiblingKeywords")]
	IgnoreSiblingId = 0,
	IgnoreSiblingKeywords = 0,
	[Obsolete("This will be replaced by ProcessSiblingKeywords")]
	ProcessSiblingId = 1,
	ProcessSiblingKeywords = 1
}
