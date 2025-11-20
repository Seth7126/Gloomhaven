using UnityEngine;

namespace Gloomhaven;

public class HapticFeedbackSettingsLocalization : MonoBehaviour
{
	private const int EuParam = 116;

	private const int UsParam = 115;

	[SerializeField]
	private TextLocalizedListener _hapticSpeakerText;

	[SerializeField]
	private TextLocalizedListener _hapticVibrationText;

	private string HapticSpeakerTextKey => "Consoles/GUI_OPT_AUDIO_HAPTIC_VOLUME_EU";

	private string HapticVibrationTextKey => "Consoles/GUI_OPT_AUDIO_HAPTIC_VIBRATION_EU";

	private void OnEnable()
	{
		UpdateTextHapticTexts();
	}

	private void UpdateTextHapticTexts()
	{
		_hapticSpeakerText.SetTextKey(HapticSpeakerTextKey);
		_hapticVibrationText.SetTextKey(HapticVibrationTextKey);
	}
}
