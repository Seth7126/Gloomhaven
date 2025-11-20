using System.Collections.Generic;

namespace Platforms;

public interface IEntitlementPack
{
	HashSet<string> AvailableEntitlements { get; }
}
