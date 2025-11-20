using System;
using FFSNet;
using Script.GUI.Multiplayer.Hero_Assign_Slot;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMultiplayerSaveOwner : MonoBehaviour, IDisposable
{
	public class PlayerEvent : UnityEvent<NetworkPlayer>
	{
	}

	[SerializeField]
	private Image userAvatar;

	[SerializeField]
	private TextMeshProUGUI userName;

	[SerializeField]
	private Sprite _defaultUserAvatarSprite;

	public PlayerEvent OnSelect = new PlayerEvent();

	public PlayerEvent OnDeselect = new PlayerEvent();

	private ITempUserAvatarData _tempUserAvatarData;

	public void SetPlayer(SaveOwner owner)
	{
		userName.text = owner.Username;
		InitAvatarImage(_defaultUserAvatarSprite);
		if (PlatformLayer.Instance.IsConsole)
		{
			if (IsCurrentPlatformValid() && owner.PlatformName == PlatformLayer.Instance.PlatformID && owner.ActualAvatarURL != null)
			{
				_tempUserAvatarData = TempAvatarStorage.CreateTempAvatar(owner.ActualAvatarURL);
				_tempUserAvatarData.OnAvatarDownloadedEvent += InitAvatarImage;
			}
		}
		else
		{
			InitAvatarImage(owner.Avatar?.Avatar);
		}
		static bool IsCurrentPlatformValid()
		{
			string platformID = PlatformLayer.Instance.PlatformID;
			return platformID == "GameCore" || platformID == "PlayStation4" || platformID == "PlayStation5";
		}
	}

	private void InitAvatarImage(Sprite avatarImage)
	{
		userAvatar.sprite = avatarImage ?? _defaultUserAvatarSprite;
		userAvatar.color = Color.white;
	}

	public void Dispose()
	{
		if (_tempUserAvatarData != null)
		{
			_tempUserAvatarData.OnAvatarDownloadedEvent -= InitAvatarImage;
			TempAvatarStorage.RemoveTempAvatar(_tempUserAvatarData.AvatarUrl);
			_tempUserAvatarData = null;
		}
	}
}
