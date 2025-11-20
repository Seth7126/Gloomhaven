using System.Collections.Generic;

namespace Platforms;

public interface IAchievementPack
{
	HashSet<string> GetAllAchievements();
}
