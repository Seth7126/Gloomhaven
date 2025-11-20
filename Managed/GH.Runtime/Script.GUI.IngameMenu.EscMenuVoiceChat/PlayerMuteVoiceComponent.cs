using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.IngameMenu.EscMenuVoiceChat;

public class PlayerMuteVoiceComponent : MonoBehaviour, IVoiceComponent
{
	[SerializeField]
	private Image _muteIcon;

	private VoiceContext _voiceContext;

	public void Init(VoiceContext voiceContext)
	{
		_voiceContext = voiceContext;
	}

	public void UpdateComponent()
	{
		_muteIcon.enabled = _voiceContext.UserVoice.IsMuted;
	}
}
