using System;

namespace Manatee.Json.Schema;

[Flags]
public enum JsonSchemaVersion
{
	None = 0,
	Draft04 = 1,
	Draft06 = 2,
	Draft07 = 4,
	Draft2019_09 = 8,
	All = 0xF
}
