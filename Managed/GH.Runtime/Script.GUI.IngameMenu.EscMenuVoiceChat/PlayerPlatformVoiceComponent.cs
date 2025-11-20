using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.IngameMenu.EscMenuVoiceChat;

public class PlayerPlatformVoiceComponent : MonoBehaviour, IVoiceComponent
{
	[SerializeField]
	private Image _platformIcon;

	[SerializeField]
	private Color _defaultColor;

	[SerializeField]
	private Color _speakingColor;

	private VoiceContext _voiceContext;

	public void Init(VoiceContext voiceContext)
	{
		_voiceContext = voiceContext;
		string platformName = _voiceContext.UserVoice.PlatformName;
		_platformIcon.enabled = true;
		_platformIcon.sprite = global::PlatformLayer.Instance.PlayerPlatformImageController.GetPlayerPlatformImage(platformName);
	}

	public void UpdateComponent()
	{
	}
}
