using System.Linq;
using FFSNet;
using TMPro;
using UnityEngine;

namespace Script.GUI.IngameMenu.EscMenuVoiceChat;

public class PlayerNameVoiceComponent : MonoBehaviour, IVoiceComponent
{
	[SerializeField]
	private TMP_Text _userNameText;

	private string _hostFormat = "<color=#EACF8C>{0}</color> <size=130%><sprite name=\"Host_Icon\" ></size>";

	private string _hostFormatTalking = "<color=#FFFFFF>{0}</color> <size=130%><sprite name=\"Host_Icon_ActiveVoice\" color=#FFFFFF></size>";

	private string _clientFormat = "<color=#969696FF>{0}</color>";

	private string _clientFormatTalking = "<color=#FFFFFF>{0}</color>";

	private VoiceContext _voiceContext;

	private string _userName = string.Empty;

	private NetworkPlayer _associatedPlayer;

	public void Init(VoiceContext voiceContext)
	{
		_voiceContext = voiceContext;
		InitNetworkPlayer();
		string formatString = (_voiceContext.UserVoice.IsHost ? _hostFormat : _clientFormat);
		if (_voiceContext.UserVoice.IsPCUser)
		{
			if (_voiceContext.UserVoice.Name != null)
			{
				_voiceContext.UserVoice.Name.GetCensoredStringAsync(delegate(string censoredName)
				{
					_userName = censoredName;
					_userNameText.text = string.Format(formatString, _userName);
				});
			}
			else
			{
				global::PlatformLayer.UserData.UserName.GetCensoredStringAsync(delegate(string censoredName)
				{
					_userName = censoredName;
					_userNameText.text = string.Format(formatString, _userName);
				});
			}
		}
		else if (_voiceContext.UserVoice.Name != null)
		{
			_userName = _voiceContext.UserVoice.Name;
			_userNameText.text = string.Format(formatString, _userName);
		}
		else
		{
			_userName = global::PlatformLayer.UserData.UserName;
			_userNameText.text = string.Format(formatString, _userName);
		}
		if (_voiceContext.UserVoice.Name != null)
		{
			_userName = _voiceContext.UserVoice.Name;
			_userNameText.text = string.Format(formatString, _userName);
			return;
		}
		global::PlatformLayer.UserData.UserName.GetCensoredStringAsync(delegate(string x)
		{
			_userName = x;
			_userNameText.text = string.Format(formatString, _userName);
		});
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
