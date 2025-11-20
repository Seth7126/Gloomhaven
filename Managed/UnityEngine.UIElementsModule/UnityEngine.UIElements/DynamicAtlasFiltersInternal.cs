using System;

namespace UnityEngine.UIElements;

[Flags]
internal enum DynamicAtlasFiltersInternal
{
	None = 0,
	Readability = 1,
	Size = 2,
	Format = 4,
	ColorSpace = 8,
	FilterMode = 0x10
}
