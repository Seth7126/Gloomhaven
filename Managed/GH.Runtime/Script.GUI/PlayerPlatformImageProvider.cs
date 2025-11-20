using SM;
using UnityEngine;

namespace Script.GUI;

[CreateAssetMenu(menuName = "Data/Multiplayer/Player Platform Image Provider")]
public class PlayerPlatformImageProvider : ScriptableObject
{
	[SerializeField]
	private SerializableDictionary<string, PlatformImageData> _platformImageData;

	[SerializeField]
	private Sprite _fallbackImage;

	public Sprite GetPlayerPlatformImage(string platformName, bool isAlternative)
	{
		if (_platformImageData.TryGetValue(platformName, out var value))
		{
			if (!isAlternative)
			{
				return value._platformImage;
			}
			return value._alternativeImage;
		}
		Debug.LogError("Platform image for " + platformName + " is missing! Returning fallback");
		return _fallbackImage;
	}
}
