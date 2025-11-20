using System;

namespace Platforms.Utils;

[Flags]
public enum DebugFlags
{
	None = 0,
	Log = 1,
	Warning = 2,
	Error = 4
}
