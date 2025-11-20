using System.Collections.Generic;
using FFSNet;

public static class PlatformTextSpriteProvider
{
	private static Dictionary<string, string> _platformTypes = new Dictionary<string, string>
	{
		{ "Standalone", "PC" },
		{ "GoGGalaxy", "PC" },
		{ "Steam", "PC" },
		{ "EpicGamesStore", "PC" },
		{ "GameCore", "XBOX" },
		{ "Xbox", "XBOX" },
		{ "PlayStation4", "PS" },
		{ "PlayStation5", "PS" },
		{ "NintendoSwitch", "NS" }
	};

	private static Dictionary<string, string> _platformIcons = new Dictionary<string, string>
	{
		{ "Standalone", "platform_pc" },
		{ "GoGGalaxy", "platform_pc" },
		{ "Steam", "platform_pc" },
		{ "EpicGamesStore", "platform_pc" },
		{ "GameCore", "platform_xbox" },
		{ "Xbox", "platform_xbox" },
		{ "PlayStation4", "platform_ps" },
		{ "PlayStation5", "platform_ps" },
		{ "NintendoSwitch", "platform_ns" }
	};

	public static string GetPlatformIcon(string platformName, bool onlyId = false)
	{
		string text = _platformTypes[PlatformLayer.Instance.PlatformID];
		string text2 = _platformTypes[platformName];
		string text3 = ((!(text == text2) && !(text2 == "PC")) ? (_platformIcons[platformName] + "_alt") : _platformIcons[platformName]);
		if (!onlyId)
		{
			return "<sprite name=\"" + text3 + "\">";
		}
		return text3;
	}

	public static string UserNameWithPlatformIcon(this NetworkPlayer player)
	{
		return FormatUserNameWithPlatformIcon(player.PlatformName, player.Username);
	}

	public static string FormatUserNameWithPlatformIcon(string platformName, string username)
	{
		return GetPlatformIcon(platformName) + username;
	}
}
