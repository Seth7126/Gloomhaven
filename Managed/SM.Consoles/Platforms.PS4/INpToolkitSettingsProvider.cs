using System.Collections.Generic;

namespace Platforms.PS4;

public interface INpToolkitSettingsProvider
{
	Dictionary<string, int> AgeRestrictions { get; }

	Dictionary<MemoryPoolType, uint> MemoryPoolsSizes { get; }

	NotificationFlags NotificationFlags { get; }
}
