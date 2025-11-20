using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using Photon.Bolt;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIResetLevelUpWindow : Singleton<UIResetLevelUpWindow>, IGoldEventListener, IEscapable
{
	private ICharacterService characterData;

	[SerializeField]
	private Toggle displayToggle;

	[SerializeField]
	private UILevelUpCardInventory inventory;

	[SerializeField]
	private RectTransform fullCardHolder;

	[Header("Controller")]
	[SerializeField]
	private ControllerInputArea controllerArea;

	[Header("Reset panel")]
	[SerializeField]
	private TextMeshProUGUI resetCostText;

	[SerializeField]
	private GameObject resetPanel;

	[SerializeField]
	private ExtendedButton resetButton;

	[SerializeField]
	private GameObject resetGoldInfo;

	[Header("Notification")]
	[SerializeField]
	private UIAdventureFirstTimeTooltip notificationUI;

	[SerializeField]
	private List<NewCardNotification> newCardNotifications;

	[SerializeField]
	private BackgroundToggleElement _darkenPanel;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	private readonly List<Component> requestDisableShow = new List<Component>();

	private UILevelUpCardInventory _UILevelUpCardInventory;

	private bool isOpenConfirmationBox;

	public ControllerInputArea ControllerArea => controllerArea;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public UILevelUpCardInventory UILevelUpCardInventory
	{
		get
		{
			if (_UILevelUpCardInventory == null)
			{
				_UILevelUpCardInventory = GetComponent<UILevelUpCardInventory>();
			}
			return _UILevelUpCardInventory;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		displayToggle.onValueChanged.AddListener(ToggleDisplay);
		resetButton.onClick.AddListener(ResetAbilities);
		controllerArea.OnFocused.AddListener(OnControllerFocused);
		controllerArea.OnUnfocused.AddListener(OnControllerUnfocused);
	}

	protected override void OnDestroy()
	{
		displayToggle.onValueChanged.RemoveListener(ToggleDisplay);
		resetButton.onClick.RemoveListener(ResetAbilities);
		UIWindowManager.UnregisterEscapable(this);
		base.OnDestroy();
	}

	public void Show(ICharacterService character)
	{
		if (requestDisableShow.Count <= 0)
		{
			characterData = character;
			controllerArea.enabled = true;
			if (InputManager.GamePadInUse)
			{
				UILevelUpCardInventory.SetShowResetMercenaryHotkey(character.Level > 1 && !AdventureState.MapState.IsCampaign);
			}
			else
			{
				resetPanel.SetActive(characterData.Level > 1 && !AdventureState.MapState.IsCampaign);
			}
			displayToggle.gameObject.SetActive(value: true);
			ToggleDisplay(displayToggle.isOn);
			ShowNewCardsNotification();
		}
	}

	public void TryActiveResetMercenaryHotkey()
	{
		UILevelUpCardInventory.SetShowResetMercenaryHotkey(characterData.Level > 1 && !AdventureState.MapState.IsCampaign);
	}

	public void ResetAbilities()
	{
		if ((FFSNetwork.IsOnline && FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			return;
		}
		bool useFreeTicket = characterData.HasFreeLevelReset();
		int price = ((!useFreeTicket) ? characterData.CalculatePriceResetLevel() : 0);
		if (characterData.Gold < price)
		{
			return;
		}
		if (price == 0)
		{
			isOpenConfirmationBox = true;
			UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
			string translation = LocalizationManager.GetTranslation("GUI_RESET_CHARACTER_LEVEL_CONFIRMATION");
			UnityAction onActionConfirmed = delegate
			{
				isOpenConfirmationBox = false;
				if (useFreeTicket)
				{
					characterData.UseFreeResetLevel();
				}
				OnResetLevel();
				if (FFSNetwork.IsOnline)
				{
					AdventureState.MapState.MapParty.CheckCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterData.CharacterID && x.CharacterName == characterData.CharacterName);
					int actorID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? characterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID));
					Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.ResetCharacter, ActionProcessor.CurrentPhase, disableAutoReplication: false, actorID, 0, 0, PlayerRegistry.MyPlayer.PlayerID);
				}
			};
			UnityAction onActionCancelled = delegate
			{
				isOpenConfirmationBox = false;
				if (controllerArea.IsFocused)
				{
					EventSystem.current.SetSelectedGameObject(resetButton.gameObject);
				}
			};
			INavigationOperation toPreviousNonMenuState = NavigationOperation.ToPreviousNonMenuState;
			instance.ShowGenericConfirmation(translation, null, onActionConfirmed, onActionCancelled, null, null, null, showHeader: true, enableSoftlockReport: false, toPreviousNonMenuState);
			return;
		}
		isOpenConfirmationBox = true;
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericSpendConfirmation(LocalizationManager.GetTranslation("GUI_RESET_CHARACTER_LEVEL_CONFIRMATION"), price, delegate
		{
			characterData.ModifyGold(-price);
			OnResetLevel();
			if (FFSNetwork.IsOnline)
			{
				AdventureState.MapState.MapParty.CheckCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterData.CharacterID && x.CharacterName == characterData.CharacterName);
				int actorID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? characterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID));
				Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.ResetCharacter, ActionProcessor.CurrentPhase, disableAutoReplication: false, actorID, 0, 0, PlayerRegistry.MyPlayer.PlayerID);
			}
		}, delegate
		{
			isOpenConfirmationBox = false;
		}, NavigationOperation.ToPreviousNonMenuState);
	}

	private void OnResetLevel()
	{
		characterData.ResetLevels();
		Hide();
		Singleton<APartyDisplayUI>.Instance.CloseWindows();
	}

	public void Hide()
	{
		if (isOpenConfirmationBox)
		{
			Singleton<UIConfirmationBoxManager>.Instance.Hide();
			isOpenConfirmationBox = false;
		}
		displayToggle.gameObject.SetActive(value: false);
		resetPanel.SetActive(value: false);
		UILevelUpCardInventory.ResetHotkeys();
		ToggleDisplay(display: false);
		controllerArea.enabled = false;
	}

	private void ToggleDisplay(bool display)
	{
		if (display)
		{
			UIWindowManager.RegisterEscapable(this);
			if (characterData != null)
			{
				characterData.RemoveCallbackOnGoldChanged(this);
				characterData.AddCallbackOnGoldChanged(this);
				UpdatePriceReset(characterData.Gold);
			}
			inventory.Show(characterData, 0f, locked: true, isOpenByToggle: true);
			inventory.EnableInteraction(enabled: true);
			controllerArea.Focus();
		}
		else
		{
			UIWindowManager.UnregisterEscapable(this);
			controllerArea.Unfocus();
			characterData?.RemoveCallbackOnGoldChanged(this);
			inventory.Hide();
		}
	}

	public void EnableShow(bool enableShow, Component locker)
	{
		if (enableShow)
		{
			requestDisableShow.Remove(locker);
		}
		else
		{
			requestDisableShow.Add(locker);
		}
	}

	private void ShowNewCardsNotification()
	{
		NewCardNotification newCardNotification = (from it in newCardNotifications
			where it.characterID == characterData.CharacterID
			orderby it.order descending
			select it).FirstOrDefault();
		if (!(newCardNotification == null) && characterData.AddNewFreeResetLevel(newCardNotification.id, newCardNotification.order))
		{
			notificationUI.Show("GUI_NEW_CARD_NOTIFICATION_TITLE", "GUI_NEW_CARD_NOTIFICATION");
		}
	}

	private void UpdatePriceReset(int money)
	{
		int num = characterData.CalculatePriceResetLevel();
		resetButton.interactable = money >= num && (!FFSNetwork.IsOnline || characterData.IsUnderMyControl);
		resetGoldInfo.SetActive(num > 0);
		resetCostText.text = num.ToString();
	}

	public void OnUpdatedGold(int gold)
	{
		UpdatePriceReset(gold);
	}

	private void OnControllerFocused()
	{
		displayToggle.isOn = true;
		inventory.EnableNavigation(resetButton);
		resetButton.SetNavigation(new NavigationCalculator
		{
			up = inventory.GetLastSelectable
		});
		if (EventSystem.current.currentSelectedGameObject == null && resetButton.gameObject.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(resetButton.gameObject);
		}
	}

	private void OnControllerUnfocused()
	{
		if (!isOpenConfirmationBox)
		{
			displayToggle.isOn = false;
		}
		inventory.DisableNavigation();
		resetButton.SetNavigation(null);
	}

	public void MPResetCharacter(GameAction action, ref bool actionValid)
	{
		actionValid = MPResetCharacter(action.ActorID);
	}

	public bool MPResetCharacter(int controllableID)
	{
		CMapCharacter cMapCharacter = null;
		cMapCharacter = ((!AdventureState.MapState.IsCampaign) ? AdventureState.MapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter x) => CharacterClassManager.GetModelInstanceIDFromCharacterID(x.CharacterID) == controllableID) : AdventureState.MapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterName.GetHashCode() == controllableID));
		if (cMapCharacter != null)
		{
			if (cMapCharacter.LastFreeLevelResetTicket != null && !cMapCharacter.LastFreeLevelResetTicket.IsUsed)
			{
				cMapCharacter.LastFreeLevelResetTicket.IsUsed = true;
			}
			else
			{
				int num = cMapCharacter.CalculatePriceResetLevel();
				switch (AdventureState.MapState.GoldMode)
				{
				case EGoldMode.PartyGold:
					if (AdventureState.MapState.MapParty.PartyGold < num)
					{
						if (FFSNetwork.IsClient)
						{
							throw new Exception("Error resetting proxy character. Not enough gold (ControllableID: " + controllableID + ").");
						}
						return false;
					}
					AdventureState.MapState.MapParty.ModifyPartyGold(-num, useGoldModifier: false);
					break;
				case EGoldMode.CharacterGold:
					if (cMapCharacter.CharacterGold < num)
					{
						if (FFSNetwork.IsClient)
						{
							throw new Exception("Error resetting proxy character. Not enough gold (ControllableID: " + controllableID + ").");
						}
						return false;
					}
					cMapCharacter.ModifyGold(-num, useGoldModifier: false);
					break;
				default:
					throw new Exception("Error resetting character. Gold mode not determined.");
				}
			}
			List<CEnhancement> list = cMapCharacter.Enhancements.Select((CEnhancement s) => s.Copy()).ToList();
			foreach (CEnhancement item in list)
			{
				item.Enhancement = EEnhancement.NoEnhancement;
				item.PaidPrice = 0;
			}
			cMapCharacter.ResetLevels();
			SaveDataShared.ApplyEnhancementIcons(list, cMapCharacter.CharacterID);
			SaveData.Instance.SaveCurrentAdventureData();
			if (cMapCharacter.IsUnderMyControl && inventory.IsVisible)
			{
				Hide();
				Singleton<APartyDisplayUI>.Instance.CloseWindows();
			}
			if (FFSNetwork.IsHost)
			{
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				int controllableID2 = controllableID;
				IProtocolToken supplementaryDataToken = new CardInventoryToken(cMapCharacter.HandAbilityCardIDs, cMapCharacter.OwnedAbilityCardIDs);
				IProtocolToken supplementaryDataToken2 = new LevelToken(cMapCharacter.Level);
				IProtocolToken supplementaryDataToken3 = new PerkPointsToken(cMapCharacter.PerkPoints);
				IProtocolToken supplementaryDataToken4 = new ItemInventoryToken(cMapCharacter, AdventureState.MapState.MapParty, AdventureState.MapState.GoldMode);
				Synchronizer.ReplicateControllableStateChange(GameActionType.ResetCharacter, currentPhase, controllableID2, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4);
			}
			return true;
		}
		if (FFSNetwork.IsClient)
		{
			throw new Exception("Error resetting proxy character. Character returns null (ControllableID: " + controllableID + ").");
		}
		return false;
	}

	public bool Escape()
	{
		if (!displayToggle.isOn)
		{
			return false;
		}
		displayToggle.isOn = false;
		return true;
	}

	public void ChangeToggleValue()
	{
		displayToggle.isOn = !displayToggle.isOn;
	}

	public int Order()
	{
		return 0;
	}

	public void SetShowDarkenPanel(bool shown)
	{
		_darkenPanel.Toggle(shown);
	}
}
