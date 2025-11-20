using System;

namespace Manatee.Json;

[Flags]
public enum LogCategory
{
	None = 0,
	General = 1,
	Serialization = 2,
	Schema = 4,
	All = 7
}
