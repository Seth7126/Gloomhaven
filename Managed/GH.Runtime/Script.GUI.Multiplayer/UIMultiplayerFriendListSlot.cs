#define ENABLE_LOGS
using System;
using System.Collections;
using DynamicScroll;
using Platforms.Social;
using SM.Gamepad;
using SM.Utils;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Script.GUI.Multiplayer;

public class UIMultiplayerFriendListSlot : MonoBehaviour, IDynamicScrollViewItem
{
	[SerializeField]
	private TextMeshProUGUI _nameText;

	[SerializeField]
	private Graphic _statusGraphic;

	[SerializeField]
	private TextLocalizedListener _statusText;

	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private Image _platformIconImage;

	[SerializeField]
	private GameObject _hotkeyParent;

	[SerializeField]
	private Hotkey _hotkey;

	[SerializeField]
	private UINavigationSelectable _selectable;

	private string _userId;

	private Sprite _defaultAvatarSprite;

	private Texture2D _loadedTexture;

	private Sprite _createdSprite;

	private UnityWebRequest _unityWebRequest;

	private KeyActionHandler _keyActionHandler;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	private void Awake()
	{
		_defaultAvatarSprite = _avatarImage.sprite;
		_platformIconImage.sprite = global::PlatformLayer.Instance.PlayerPlatformImageController.GetCurrentPlatformImage();
	}

	private void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(_keyActionHandler);
		}
	}

	public void ActivateHotkey()
	{
		if (InputManager.GamePadInUse && global::PlatformLayer.Instance.IsDelayedInit)
		{
			if (_keyActionHandler == null || !_keyActionHandler.HasBlockers)
			{
				InitInput();
			}
			_hotkeyParent.SetActive(value: true);
			_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_hotkey.DisplayHotkey(active: true);
			_simpleKeyActionHandlerBlocker?.SetBlock(value: false);
		}
	}

	public void DeactivateHotkey()
	{
		if (InputManager.GamePadInUse && global::PlatformLayer.Instance.IsDelayedInit)
		{
			_hotkeyParent.SetActive(value: false);
			_hotkey.Deinitialize();
			_hotkey.DisplayHotkey(active: false);
			_simpleKeyActionHandlerBlocker?.SetBlock(value: true);
		}
	}

	private void InitInput()
	{
		_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this, defaultIsBlock: false);
		_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		_keyActionHandler = new KeyActionHandler(KeyAction.VIEW_PROFILE, ViewProfile).AddBlocker(_simpleKeyActionHandlerBlocker).AddBlocker(_skipFrameKeyActionHandlerBlocker);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(_keyActionHandler);
	}

	public void OnUpdateItem(int index, bool isSelected)
	{
		if (isSelected)
		{
			_selectable.OnPointerEnter(new PointerEventData(EventSystem.current));
		}
		else if (_selectable.gameObject == EventSystem.current.currentSelectedGameObject)
		{
			_selectable.OnPointerExit(null);
		}
		User user = Singleton<MultiplayerFriendList>.Instance.CurrentFriends[index];
		_userId = user.UserId;
		_nameText.text = user.UserName;
		InitUserStatus(user.UserStatus);
		_avatarImage.sprite = _defaultAvatarSprite;
		StopAllCoroutines();
		if (_unityWebRequest != null)
		{
			_unityWebRequest?.Abort();
			_unityWebRequest = null;
		}
		if (_loadedTexture != null)
		{
			UnityEngine.Object.Destroy(_loadedTexture);
			UnityEngine.Object.Destroy(_createdSprite);
			_loadedTexture = null;
			_createdSprite = null;
		}
		StartCoroutine(GetTexture(user.PictureUri));
		LogUtils.Log($"Update element with {index} index");
	}

	private IEnumerator GetTexture(string uri)
	{
		_unityWebRequest = UnityWebRequestTexture.GetTexture(uri);
		yield return _unityWebRequest.SendWebRequest();
		if (_unityWebRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(_unityWebRequest.error);
			yield break;
		}
		_loadedTexture = ((DownloadHandlerTexture)_unityWebRequest.downloadHandler).texture;
		_loadedTexture.hideFlags = HideFlags.HideAndDontSave;
		_createdSprite = Sprite.Create(_loadedTexture, new Rect(0f, 0f, _loadedTexture.width, _loadedTexture.height), new Vector2(0.5f, 0.5f));
		_avatarImage.sprite = _createdSprite;
	}

	private void InitUserStatus(UserStatus userStatus)
	{
		switch (userStatus)
		{
		case UserStatus.InGame:
			_statusGraphic.color = UIInfoTools.Instance.playerOnlineColor;
			_statusText.Text.color = UIInfoTools.Instance.playerOnlineColor;
			_statusText.SetTextKey("Consoles/GUI_MULTIPLAYER_USER_INGAME");
			break;
		case UserStatus.Online:
			_statusGraphic.color = UIInfoTools.Instance.playerOnlineColor;
			_statusText.Text.color = UIInfoTools.Instance.playerOnlineColor;
			_statusText.SetTextKey("Consoles/GUI_MULTIPLAYER_USER_ONLINE");
			break;
		case UserStatus.Away:
			_statusGraphic.color = UIInfoTools.Instance.playerAwayColor;
			_statusText.Text.color = UIInfoTools.Instance.playerAwayColor;
			_statusText.SetTextKey("Consoles/GUI_MULTIPLAYER_USER_AWAY");
			break;
		case UserStatus.Offline:
			_statusGraphic.color = UIInfoTools.Instance.playerOfflineColor;
			_statusText.Text.color = UIInfoTools.Instance.playerOfflineColor;
			_statusText.SetTextKey("Consoles/GUI_MULTIPLAYER_USER_OFFLINE");
			break;
		default:
			_statusGraphic.color = UIInfoTools.Instance.playerOfflineColor;
			_statusText.Text.color = UIInfoTools.Instance.playerOfflineColor;
			_statusText.SetTextKey("Consoles/GUI_MULTIPLAYER_USER_OFFLINE");
			break;
		}
	}

	public void SendInvite()
	{
		global::PlatformLayer.Networking.InviteUser(_userId, delegate
		{
			Debug.Log("User with id " + _userId + " invited!");
		});
	}

	private void ViewProfile()
	{
		Debug.Log("ViewProfile... User with id " + _userId + "!");
		_skipFrameKeyActionHandlerBlocker.Run();
		global::PlatformLayer.Platform.PlatformSocial.ViewUserProfile(Convert.ToUInt64(_userId), null);
	}
}
