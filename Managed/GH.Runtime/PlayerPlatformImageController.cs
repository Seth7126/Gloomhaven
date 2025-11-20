using System.Collections.Generic;
using Script.GUI;
using UnityEngine;

public class PlayerPlatformImageController : MonoBehaviour
{
	private const string PCPlatformName = "PC";

	private const string XboxPlatformName = "XBOX";

	private const string PSPlatformName = "PS";

	private const string NsPlatformName = "NS";

	[SerializeField]
	private PlayerPlatformImageProvider _platformImageProvider;

	private Dictionary<string, string> _platformTypes;

	private void Awake()
	{
		_platformTypes = new Dictionary<string, string>
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
	}

	public Sprite GetPlayerPlatformImage(string platformName)
	{
		bool isAlternative = false;
		if (_platformTypes.TryGetValue(platformName, out var value))
		{
			string platformID = PlatformLayer.Instance.PlatformID;
			isAlternative = _platformTypes[platformID] != value;
		}
		return _platformImageProvider.GetPlayerPlatformImage(platformName, isAlternative);
	}

	public Sprite GetCurrentPlatformImage()
	{
		return _platformImageProvider.GetPlayerPlatformImage(PlatformLayer.Instance.PlatformID, isAlternative: false);
	}
}
