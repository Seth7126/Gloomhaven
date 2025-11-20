#define ENABLE_LOGS
using System;
using FFSNet;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMultiplayerPlayerOption : UIMenuOptionButton
{
	public class PlayerEvent : UnityEvent<NetworkPlayer>
	{
	}

	public class PlayerHoveredEvent : UnityEvent<bool, NetworkPlayer>
	{
	}

	[SerializeField]
	private Image userAvatar;

	[SerializeField]
	private TextMeshProUGUI userName;

	[SerializeField]
	private string formatHost = "<size=130%><sprite name=\"Host_Icon\" ></size> <color=#EACF8C>$GUI_MULTIPLAYER_HOST$</color> {0}";

	[SerializeField]
	private Button removeButton;

	[SerializeField]
	private Image _selectedImage;

	[SerializeField]
	private Hotkey _kickPlayerHotkey;

	[SerializeField]
	private Hotkey _viewProfileHotkey;

	[SerializeField]
	private Image _hotkeysBackground;

	[SerializeField]
	private Image _playerPlatformImage;

	[SerializeField]
	private GameObject _playerPlatformImageRoot;

	public int SlotIndex;

	public PlayerEvent OnSelect = new PlayerEvent();

	public PlayerEvent OnRemove = new PlayerEvent();

	public PlayerHoveredEvent OnHoveredPlayer = new PlayerHoveredEvent();

	private NetworkPlayer player;

	private KeyActionHandler _keyActionHandler;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	private IHotkeySession _hotkeySession;

	public static bool DisableViewingProfile
	{
		get
		{
			DeviceType currentPlatform = PlatformLayer.Instance.GetCurrentPlatform();
			if ((uint)(currentPlatform - 1) <= 4u || currentPlatform == DeviceType.Switch)
			{
				return true;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		removeButton.onClick.AddListener(KickCurrentPlayer);
		InitInput();
		HideHotkey();
		_hotkeySession = Singleton<UIMultiplayerSelectPlayerScreen>.Instance.HotkeySession;
	}

	private void ShowHotkey()
	{
		if (!DisableViewingProfile && PlatformLayer.Instance.IsConsole && player != null && player.PlatformName == PlatformLayer.Instance.PlatformID)
		{
			if (_keyActionHandler == null || !_keyActionHandler.HasBlockers)
			{
				InitInput();
			}
			_viewProfileHotkey.DisplayHotkey(active: true);
			_viewProfileHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_viewProfileHotkey.gameObject.SetActive(value: true);
			_simpleKeyActionHandlerBlocker.SetBlock(value: false);
			LayoutRebuilder.ForceRebuildLayoutImmediate(_viewProfileHotkey.transform.parent as RectTransform);
		}
		else
		{
			_viewProfileHotkey.gameObject.SetActive(value: false);
		}
		_kickPlayerHotkey.DisplayHotkey(active: true);
		_kickPlayerHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		_hotkeysBackground.enabled = true;
	}

	private void HideHotkey()
	{
		if (!DisableViewingProfile && PlatformLayer.Instance.IsConsole && player != null && player.PlatformName == PlatformLayer.Instance.PlatformID)
		{
			_viewProfileHotkey.DisplayHotkey(active: false);
			_viewProfileHotkey.Deinitialize();
			_simpleKeyActionHandlerBlocker.SetBlock(value: true);
		}
		_viewProfileHotkey.gameObject.SetActive(value: false);
		_kickPlayerHotkey.DisplayHotkey(active: false);
		_kickPlayerHotkey.Deinitialize();
		_hotkeysBackground.enabled = false;
	}

	private void InitInput()
	{
		_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this, defaultIsBlock: false);
		_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		_keyActionHandler = new KeyActionHandler(KeyAction.VIEW_PROFILE, ViewProfile).AddBlocker(_simpleKeyActionHandlerBlocker).AddBlocker(_skipFrameKeyActionHandlerBlocker);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(_keyActionHandler);
	}

	private void OnRemoveClicked()
	{
		if (isHovered && player.IsClient)
		{
			KickCurrentPlayer();
		}
	}

	protected override void OnDestroy()
	{
		OnSelect.RemoveAllListeners();
		OnRemove.RemoveAllListeners();
		removeButton.onClick.RemoveAllListeners();
		base.OnDestroy();
	}

	private void KickCurrentPlayer()
	{
		OnRemove.Invoke(player);
	}

	public void SetPlayer(NetworkPlayer player, bool isSelected = false)
	{
		this.player = player;
		userAvatar.sprite = player.Avatar;
		userAvatar.color = UIInfoTools.GetPlaceholderPlayerColor(player);
		string platformName = this.player.PlatformName;
		if (_playerPlatformImageRoot != null && _playerPlatformImage != null)
		{
			_playerPlatformImageRoot.SetActive(value: true);
			_playerPlatformImage.sprite = PlatformLayer.Instance.PlayerPlatformImageController.GetPlayerPlatformImage(platformName);
		}
		userName.text = (player.IsClient ? player.Username : string.Format(formatHost, player.Username));
		Init(delegate
		{
			OnSelect.Invoke(player);
		}, null, delegate(bool b)
		{
			OnHoveredPlayer.Invoke(b, player);
		}, isSelected);
	}

	public override void SetSelected(bool selected)
	{
		base.SetSelected(selected);
		if (InputManager.GamePadInUse)
		{
			_hotkeySession.RemoveAllHotkeys();
			if (selected)
			{
				UpdateHotkeys(isSelected);
			}
		}
		else
		{
			removeButton.gameObject.SetActive(isSelected && player.IsClient);
		}
	}

	protected override void RefreshHighlight(bool isHighlighted)
	{
		if (InputManager.GamePadInUse)
		{
			HandleGamepadHighlight();
		}
		else if (isHighlighted)
		{
			CancelHighlightAnimations();
			highlightImage.color = highlightColor;
		}
		else if (unhighlightAnimation == null)
		{
			CancelHighlightAnimations();
			unhighlightAnimation = LeanTween.alpha(highlightImage.transform as RectTransform, 0f, unhighlightDuration).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
			{
				unhighlightAnimation = null;
			});
		}
		_selectedImage.enabled = isSelected;
	}

	private void HandleGamepadHighlight()
	{
		if (isHovered)
		{
			CancelHighlightAnimations();
			highlightImage.color = highlightColor;
		}
		else if (unhighlightAnimation == null)
		{
			CancelHighlightAnimations();
			unhighlightAnimation = LeanTween.alpha(highlightImage.transform as RectTransform, 0f, unhighlightDuration).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
			{
				unhighlightAnimation = null;
			});
		}
	}

	protected override void OnHovered(bool hovered)
	{
		base.OnHovered(hovered);
		if (InputManager.GamePadInUse)
		{
			_hotkeySession.RemoveAllHotkeys();
			if (hovered)
			{
				UpdateHotkeys(isSelected);
			}
			if (hovered && player.IsClient)
			{
				ShowHotkey();
			}
			else
			{
				HideHotkey();
			}
		}
	}

	private void UpdateHotkeys(bool isSelected)
	{
		if (isSelected)
		{
			_hotkeySession.AddOrReplaceHotkeys("Back");
		}
		else
		{
			_hotkeySession.AddOrReplaceHotkeys("Back", "Select");
		}
	}

	private void ViewProfile()
	{
		Debug.Log("ViewProfile... User with id " + player.PlatformPlayerId + "!");
		if (DisableViewingProfile)
		{
			return;
		}
		_skipFrameKeyActionHandlerBlocker.Run();
		if (player.PlatformName == PlatformLayer.Instance.PlatformID)
		{
			if (PlatformLayer.Instance.GetCurrentPlatform() == DeviceType.Switch)
			{
				PlatformLayer.Platform.PlatformSocial.ViewUserProfile(Convert.ToUInt64(player.PlatformNetworkAccountPlayerID, 16), player.Username);
			}
			else
			{
				PlatformLayer.Platform.PlatformSocial.ViewUserProfile(ulong.Parse(player.PlatformNetworkAccountPlayerID), player.Username);
			}
		}
	}
}
