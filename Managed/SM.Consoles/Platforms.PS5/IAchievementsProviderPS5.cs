namespace Platforms.PS5;

public interface IAchievementsProviderPS5 : IAchievementPack
{
	int NameToId(string name);

	string IdToName(int id);
}
