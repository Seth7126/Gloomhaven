using System.Collections.Generic;

namespace Photon.Bolt;

public static class BoltScenes
{
	internal static readonly Dictionary<string, int> nameLookup;

	internal static readonly Dictionary<int, string> indexLookup;

	public const string Bootstrap = "Bootstrap";

	public const string Gloomhaven_unified = "Gloomhaven_unified";

	public const string MainMenu = "MainMenu";

	public const string MainMenu_gamepad = "MainMenu_gamepad";

	public const string NewAdventureMap = "NewAdventureMap";

	public const string NewAdventureMap_gamepad = "NewAdventureMap_gamepad";

	public const string Game = "Game";

	public const string Game_gamepad = "Game_gamepad";

	public const string ProcGen = "ProcGen";

	public const string CampaignMap = "CampaignMap";

	public const string CampaignMap_gamepad = "CampaignMap_gamepad";

	public const string EmptyScene = "EmptyScene";

	public const string Intro = "Intro";

	public static IEnumerable<string> AllScenes => nameLookup.Keys;

	static BoltScenes()
	{
		nameLookup = new Dictionary<string, int>();
		indexLookup = new Dictionary<int, string>();
		AddScene(0, 0, "Bootstrap");
		AddScene(0, 1, "Gloomhaven_unified");
		AddScene(0, 2, "MainMenu");
		AddScene(0, 3, "MainMenu_gamepad");
		AddScene(0, 4, "NewAdventureMap");
		AddScene(0, 5, "NewAdventureMap_gamepad");
		AddScene(0, 6, "Game");
		AddScene(0, 7, "Game_gamepad");
		AddScene(0, 8, "ProcGen");
		AddScene(0, 9, "CampaignMap");
		AddScene(0, 10, "CampaignMap_gamepad");
		AddScene(0, 11, "EmptyScene");
		AddScene(0, 12, "Intro");
	}

	public static void AddScene(short prefix, short id, string name)
	{
		int num = (prefix << 16) | id;
		nameLookup.Add(name, num);
		indexLookup.Add(num, name);
	}
}
