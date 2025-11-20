using System;

namespace Manatee.Json.Serialization;

[Flags]
public enum PropertySelectionStrategy
{
	ReadWriteOnly = 1,
	ReadOnly = 2,
	ReadAndWrite = 3
}
