using System;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Party;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerMerchantSlot : UIMenuOptionButton
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private MultiplayerUserState owner;

	[SerializeField]
	private Image focusMask;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	private UIMultiplayerMerchantSlot twinSlot;

	[SerializeField]
	private bool isClientSlot;

	[SerializeField]
	private Hotkey reportHotkey;

	[SerializeField]
	private Hotkey profileHotkey;

	[SerializeField]
	private GameObject hotkeysBackground;

	[SerializeField]
	private NetworkHeroAssignService networkHeroAssignService;

	[SerializeField]
	private UIMultiplayerConfirmationBox confirmationBox;

	public int SlotIndex;

	private NetworkPlayer player;

	private bool isUnfocused;

	private IUiNavigationSelectable previousSelectable;

	private KeyActionHandler reportActionHandler;

	private KeyActionHandler profileActionHandler;

	private SimpleKeyActionHandlerBlocker reportHotkeyBlocker;

	private SimpleKeyActionHandlerBlocker simpleHotkeyBlocker;

	public string CharacterID { get; set; }

	public string CharacterName { get; set; }

	public string Title => title.text;

	public NetworkPlayer AssignedPlayer => player;

	private bool isGamepadInputReinitRequired
	{
		get
		{
			if (reportActionHandler != null)
			{
				return !reportActionHandler.HasBlockers;
			}
			return true;
		}
	}

	public override void Init(Action onSelected, Action onDeselected = null, Action<bool> onHovered = null, bool isSelected = false)
	{
		base.Init(onSelected, onDeselected, onHovered, isSelected);
		twinSlot?.Init(onSelected, onDeselected, onHovered, isSelected);
	}

	private void Init(string title, string characterID, string characterName, bool isSelected, bool interactable)
	{
		this.title.text = title;
		CharacterID = characterID;
		CharacterName = characterName;
		player = null;
		if (isClientSlot)
		{
			hotkeysBackground.gameObject.SetActive(value: false);
			reportHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			reportHotkey.DisplayHotkey(active: false);
			profileHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			profileHotkey.DisplayHotkey(active: false);
			profileHotkey.gameObject.SetActive(value: false);
		}
		SetSelected(isSelected);
		SetInteractable(interactable);
		twinSlot?.Init(title, characterID, characterName, isSelected, interactable);
	}

	private void InitGamepadInput()
	{
		reportHotkeyBlocker = new SimpleKeyActionHandlerBlocker();
		simpleHotkeyBlocker = new SimpleKeyActionHandlerBlocker();
		reportActionHandler = new KeyActionHandler(KeyAction.UI_REPORT_PLAYER, OnReportHotkeyClicked).AddBlocker(reportHotkeyBlocker).AddBlocker(simpleHotkeyBlocker);
		profileActionHandler = new KeyActionHandler(KeyAction.VIEW_PROFILE, OnProfileHotkeyClicked).AddBlocker(simpleHotkeyBlocker);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(reportActionHandler);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(profileActionHandler);
	}

	public void SetEmpty(string title, bool isSelected = false, bool interactable = true)
	{
		Init(title, null, null, isSelected, interactable);
		twinSlot?.SetEmpty(title, isSelected, interactable);
	}

	public void SetCharacter(CMapCharacter character, bool isSelected = false, bool interactable = true)
	{
		Init(LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey), character.CharacterID, character.CharacterName, isSelected, interactable);
		twinSlot?.SetCharacter(character, isSelected, interactable);
	}

	public void AssignTo(NetworkPlayer player, bool isOnline)
	{
		if (player != null && this.player == player)
		{
			UpdateConnectionStatus(isOnline);
		}
		else
		{
			this.player = player;
			if (player != null)
			{
				owner.Show(player, isOnline);
			}
			else
			{
				owner.Hide();
			}
		}
		twinSlot?.AssignTo(player, isOnline);
	}

	public void UpdateConnectionStatus(bool isOnline)
	{
		owner.RefreshOnline(isOnline);
		twinSlot?.UpdateConnectionStatus(isOnline);
	}

	public void Focus(bool focus, bool interactable = true)
	{
		isUnfocused = !focus;
		focusMask.enabled = isUnfocused && !isHovered;
		focusMask.raycastTarget = !interactable;
		twinSlot?.Focus(focus);
	}

	protected override void OnHovered(bool hovered)
	{
		base.OnHovered(hovered);
		focusMask.enabled = isUnfocused && !hovered;
		if (InputManager.GamePadInUse && isClientSlot && !(player == null))
		{
			bool flag = hovered && isClientSlot && PlatformLayer.Instance.GetCurrentPlatform() != DeviceType.Standalone && !player.Username.Equals(PlayerRegistry.MyPlayer.Username);
			bool flag2 = !UIMultiplayerPlayerOption.DisableViewingProfile && player.PlatformName.Equals(PlatformLayer.Instance.PlatformID);
			hotkeysBackground.gameObject.SetActive(flag);
			reportHotkey.DisplayHotkey(flag);
			profileHotkey.DisplayHotkey(flag2 && flag);
			profileHotkey.gameObject.SetActive(flag2 && flag);
			if (isGamepadInputReinitRequired)
			{
				InitGamepadInput();
			}
			if (flag)
			{
				simpleHotkeyBlocker.SetBlock(value: false);
			}
			else
			{
				simpleHotkeyBlocker.SetBlock(value: true);
			}
		}
	}

	public void SetInteractable(bool isInteractable)
	{
		button.interactable = isInteractable;
		tooltip.enabled = !isInteractable;
		twinSlot?.SetInteractable(isInteractable);
	}

	private void OnReportHotkeyClicked()
	{
		previousSelectable = Singleton<UINavigation>.Instance.NavigationManager.CurrentlySelectedElement;
		Singleton<UINavigation>.Instance.StateMachine.RemoveFilter();
		reportHotkeyBlocker.SetBlock(value: true);
		confirmationBox.ShowConfirmation(player, null, SendReport, CancelReport);
	}

	private void OnProfileHotkeyClicked()
	{
		if (!UIMultiplayerPlayerOption.DisableViewingProfile && player.PlatformName == PlatformLayer.Instance.PlatformID)
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

	private void SendReport()
	{
		reportHotkeyBlocker.SetBlock(value: false);
		networkHeroAssignService?.ReportClient(player, delegate
		{
			OnReportCallback(PlayerRegistry.MyPlayer, player);
		});
	}

	private void CancelReport()
	{
		reportHotkeyBlocker.SetBlock(value: false);
	}

	private void OnReportCallback(NetworkPlayer reporter, NetworkPlayer reported)
	{
		Singleton<UINavigation>.Instance.NavigationManager.TrySelect(previousSelectable);
		UIMultiplayerNotifications.ShowPlayerReported(reporter.Username, reported.Username);
	}
}
