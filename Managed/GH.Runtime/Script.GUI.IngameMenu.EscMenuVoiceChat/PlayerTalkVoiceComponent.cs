using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.IngameMenu.EscMenuVoiceChat;

public class PlayerTalkVoiceComponent : MonoBehaviour, IVoiceComponent
{
	[SerializeField]
	private Image _talkIcon;

	private VoiceContext _voiceContext;

	public void Init(VoiceContext voiceContext)
	{
		_voiceContext = voiceContext;
	}

	public void UpdateComponent()
	{
		_talkIcon.enabled = _voiceContext.UserVoice.IsSpeaking;
	}
}
