using FFSNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerUser : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI username;

	[SerializeField]
	protected Image userAvatar;

	[SerializeField]
	protected GameObject hostMask;

	[SerializeField]
	protected string hostFormat;

	[SerializeField]
	protected Image _playerPlatformImage;

	[SerializeField]
	protected GameObject _playerPlatformImageRoot;

	[SerializeField]
	protected Sprite _defaultUserAvatarSprite;

	private string _hostFormat = "<color=#EACF8C>{0}</color><sprite name=\"Host_Icon\" >";

	private string _hostFormatInitiativeTrack = "<color=#EACF8C>{0}</color>";

	protected NetworkPlayer player;

	public virtual void Show(NetworkPlayer player)
	{
		if (player != this.player)
		{
			this.player = player;
			SetupUserName(player);
			SetupPlayerPlatformIcon(player.PlatformName);
			if (hostMask != null)
			{
				hostMask.SetActive(!player.IsClient);
			}
			if (userAvatar != null)
			{
				player.UpdatePlayerProfileAvatar();
				if (player.Avatar == null)
				{
					PlayerRegistry.CheckForReceivedAvatarUpdate();
				}
				userAvatar.sprite = player.Avatar ?? _defaultUserAvatarSprite;
				userAvatar.color = UIInfoTools.GetPlaceholderPlayerColor(player);
			}
		}
		base.gameObject.SetActive(value: true);
	}

	protected virtual void SetupUserName(NetworkPlayer networkPlayer)
	{
		if (networkPlayer.IsClient || hostFormat.IsNullOrEmpty())
		{
			username.text = networkPlayer.Username;
		}
		else
		{
			username.text = string.Format(CreateLayout.LocaliseText(hostFormat), networkPlayer.Username);
		}
	}

	protected virtual void SetupPlayerPlatformIcon(string platformName)
	{
		if (_playerPlatformImageRoot != null && _playerPlatformImage != null)
		{
			_playerPlatformImageRoot.SetActive(value: true);
			_playerPlatformImage.sprite = PlatformLayer.Instance.PlayerPlatformImageController.GetPlayerPlatformImage(platformName);
		}
	}

	private void Update()
	{
		if (player != null && player.Avatar != null && userAvatar != null)
		{
			userAvatar.sprite = player.Avatar;
			userAvatar.color = UIInfoTools.GetPlaceholderPlayerColor(player);
		}
	}

	public virtual void Hide()
	{
		player = null;
		base.gameObject.SetActive(value: false);
	}
}
