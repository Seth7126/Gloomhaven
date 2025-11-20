namespace Platforms.PS4;

public interface IAchievementsProviderPS4 : IAchievementPack
{
	int NameToId(string name);

	string IdToName(int id);
}
