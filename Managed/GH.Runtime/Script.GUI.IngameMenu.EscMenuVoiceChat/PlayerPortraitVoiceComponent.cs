using System.Linq;
using FFSNet;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.IngameMenu.EscMenuVoiceChat;

public class PlayerPortraitVoiceComponent : MonoBehaviour, IVoiceComponent
{
	[SerializeField]
	private Image _portraitIcon;

	[SerializeField]
	private Image _defaultFrame;

	[SerializeField]
	private Image _activeVoiceFrame;

	[SerializeField]
	private Image _speakingMask;

	private NetworkPlayer _associatedPlayer;

	private VoiceContext _voiceContext;

	public void Init(VoiceContext voiceContext)
	{
		_voiceContext = voiceContext;
		InitNetworkPlayer();
		if (_associatedPlayer != null)
		{
			_associatedPlayer.UpdatePlayerProfileAvatar();
		}
		_portraitIcon.sprite = _associatedPlayer.Avatar;
	}

	public void UpdateComponent()
	{
	}

	private void InitNetworkPlayer()
	{
		string accountId = _voiceContext.UserVoice.PlatformAccountID;
		_associatedPlayer = PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer x) => x.PlatformNetworkAccountPlayerID == accountId);
	}
}
