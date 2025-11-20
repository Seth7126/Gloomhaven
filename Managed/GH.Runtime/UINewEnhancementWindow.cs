using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOO.Introduction;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.CampaignMapStates.Enhancment;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UINewEnhancementWindow : MonoBehaviour
{
	private enum ShopMode
	{
		NONE,
		BUY,
		SELL
	}

	[SerializeField]
	private UIIntroduce introduction;

	[SerializeField]
	private UINewEnhancementShopInventory enhancementShop;

	[SerializeField]
	private UIEnhancementCardHighlighter cardHolder;

	[SerializeField]
	private GameObject cardInformation;

	[SerializeField]
	private TextLocalizedListener cardInformationText;

	[SerializeField]
	private Button exitShopButton;

	[SerializeField]
	private List<UIEnhancementButtonHighlight> highlightAbilityPool = new List<UIEnhancementButtonHighlight>();

	[SerializeField]
	private UITab buyButton;

	[SerializeField]
	private UITab sellButton;

	[SerializeField]
	private UIEnchantressEffect enhanctressEffect;

	[SerializeField]
	private UIPartyCharacterEnhancementAbilityCardsDisplay cardsDisplay;

	[SerializeField]
	private string audioItemBuyEnhancement = "PlaySound_UIReceivedItem";

	[SerializeField]
	private string audioItemSellEnhancement;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private HotkeyContainer _cardAbilityHotkeys;

	private List<UIEnhancementButtonHighlight> highlightAbility = new List<UIEnhancementButtonHighlight>();

	private Transform highlightAbilityParent;

	private AbilityCardUI selectedCard;

	private AbilityCardUI previousSelectedCard;

	private AbilityCardUI lastShowedCard;

	private UIWindow shopWindow;

	private EnhancementButtonBase previewedEnhancement;

	private EnhancementLineFilter enhancementLineFilter;

	private IEnhancementShopService shopService;

	private ICharacterEnhancementService character;

	private const string FORMAT_COST = "<color=#{2}>{0} <sprite name=\"Gold_Icon_White\" color=#{2}>{1}</color>";

	private const string FORMAT_OWNER = "{0} <size=+5><font=\"MarcellusSC-Regular SDF\"><color=#F1DBAE>{1}</color></font></size> <size=+15><sprite name=\"AA_{2}\" color=#{3}></size>";

	private bool _isConfirmationBoxOpened;

	private ShopMode mode;

	private string _navigationRootName;

	private bool GamepadMode => InputManager.GamePadInUse;

	public AbilityCardUI SowingCard { get; private set; }

	public UIPartyCharacterEnhancementAbilityCardsDisplay CardsDisplay => cardsDisplay;

	public HotkeyContainer CardSelectionHotkeys => cardsDisplay.Hotkeys;

	public HotkeyContainer CardAbilityHotkeys => _cardAbilityHotkeys;

	private AbilityCardUI CheckedCard
	{
		get
		{
			if (!(selectedCard != null))
			{
				return previousSelectedCard;
			}
			return selectedCard;
		}
	}

	public UnityEvent OnExit => shopWindow.onHidden;

	public event Action<AbilityCardUI> EventOnHoveredCard;

	private void Awake()
	{
		_navigationRootName = "EnhancmentSelectOptionUpgrade";
		shopService = new MapPartyEnhancementShopService();
		shopWindow = GetComponent<UIWindow>();
		highlightAbilityParent = highlightAbilityPool[0].transform.parent;
		enhancementShop.OnHoveredSlot.AddListener(PreviewEnhancement);
		enhancementShop.OnUnhoveredSlot.AddListener(delegate
		{
			ClearPreviewEnhancement();
		});
		enhancementShop.OnSelectedSlot.AddListener(delegate(EnhancementSlot enhancement)
		{
			if (!_isConfirmationBoxOpened && (!GamepadMode || enhancement.Available))
			{
				if ((GamepadMode && enhancement.BuyMode) || (!GamepadMode && mode == ShopMode.BUY))
				{
					ConfirmBuy(enhancement);
				}
				else if (shopService.IsSellAvailable)
				{
					ConfirmSell(enhancement);
				}
			}
		});
		exitShopButton.onClick.AddListener(ExitShop);
		buyButton.onValueChanged.AddListener(delegate(bool selected)
		{
			if (selected)
			{
				ShowBuyOptions();
			}
		});
		sellButton.onValueChanged.AddListener(delegate(bool selected)
		{
			if (selected)
			{
				ShowSellOptions();
			}
		});
		enhancementLineFilter = new EnhancementLineFilter(OnSelectedEnhacementButton);
		controllerArea.OnFocusedArea.AddListener(OnControllerFocused);
		controllerArea.OnUnfocusedArea.AddListener(OnControllerUnfocused);
	}

	private void OnDestroy()
	{
		exitShopButton.onClick.RemoveAllListeners();
		buyButton.onValueChanged.RemoveAllListeners();
		sellButton.onValueChanged.RemoveAllListeners();
	}

	public void EnterShop()
	{
		((MapPartyEnhancementShopService)shopService).OnEnterShop();
		enhancementShop.Clear();
		OnSelectedCardToEnhance(null);
		if (InputManager.GamePadInUse)
		{
			SelectTabElementByShopMode(buyOrSell: true);
		}
		if (buyButton.isOn)
		{
			ShowBuyOptions();
		}
		else
		{
			buyButton.Activate();
		}
		sellButton.gameObject.SetActive(shopService.IsSellAvailable);
		buyButton.interactable = shopService.IsSellAvailable;
		controllerArea.Enable();
		NewPartyDisplayUI.PartyDisplay.EnableEnhancementMode(OnSelectedCharacter);
		shopWindow.Show();
		ShowTooltip();
		enhanctressEffect.Play();
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
	}

	public void SetEditMode(bool canEdit)
	{
		enhancementShop.SetInteractable(canEdit);
	}

	private void OnSelectedCharacter(NewPartyCharacterUI characterUI)
	{
		character = characterUI.Service;
		OnSelectedCardToEnhance(null);
		cardsDisplay.Display(character, delegate(AbilityCardUI card)
		{
			OnSelectedCardToEnhance(card);
			SelectCardFirstRow(card);
		}, OnHoveredCard, mode == ShopMode.BUY, characterUI.CardPointReference, autoselectFirst: true);
	}

	private void SelectCardFirstRow(AbilityCardUI card, UIEnhancementButtonHighlight uiEnhancementButtonHighlight = null)
	{
		controllerArea.Focus();
		if (!InputManager.GamePadInUse)
		{
			EventSystem.current.SetSelectedGameObject(highlightAbility.First()?.gameObject);
		}
		else if (card != null && highlightAbility.Count > 0)
		{
			if (uiEnhancementButtonHighlight == null)
			{
				uiEnhancementButtonHighlight = highlightAbility.First();
			}
			UiNavigationRoot root = uiEnhancementButtonHighlight.GetComponentInParent<AbilityCardButtonRootLink>().Root;
			if (!(Singleton<UINavigation>.Instance.StateMachine.CurrentState is EnhancmentSelectCardOptionState))
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.EnhancmentSelectCardOption, root);
			}
		}
	}

	public void DeselectCurrentCard()
	{
		cardsDisplay.Deselect();
	}

	public void ExitShop()
	{
		SowingCard = null;
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		ClearHighlights();
		introduction.Hide();
		enhanctressEffect.Stop();
		cardHolder.Hide();
		cardsDisplay.Hide();
		NewPartyDisplayUI.PartyDisplay.DisableEnhancementMode();
		shopWindow.Hide();
		controllerArea.Destroy();
		ClearPreviewEnhancement();
	}

	private void OnHoveredCard(bool hovered, AbilityCardUI card)
	{
		AbilityCardUI abilityCardUI = (hovered ? card : selectedCard);
		ShowCard(abilityCardUI);
		SetEnhanceFilters(abilityCardUI);
		if (cardHolder.Card != null)
		{
			HighlightButtons(!hovered || card == selectedCard);
		}
		this.EventOnHoveredCard?.Invoke(card);
	}

	private void ShowCard(AbilityCardUI card, bool animateShowCard = false)
	{
		SowingCard = card;
		ClearHighlights();
		if (card == null)
		{
			cardHolder.Hide();
			cardInformation.SetActive(value: false);
		}
		else if (EnhancementUtils.CanBeEnhanced(card))
		{
			if (!EnhancementUtils.HaveEnhancementButtons(card, mode == ShopMode.SELL) && !InputManager.GamePadInUse)
			{
				cardInformationText.SetTextKey((mode == ShopMode.BUY) ? "GUI_CARD_FULLY_ENHANCED" : "GUI_CARD_NOT_ENHANCED");
				cardInformation.SetActive(value: true);
				cardHolder.ShowNoEnhanceCard(card.AbilityCard, animateShowCard);
			}
			else
			{
				cardInformation.SetActive(value: false);
				cardHolder.ShowCard(card.AbilityCard, animateShowCard);
			}
		}
		else
		{
			cardInformationText.SetTextKey("GUI_CARD_CANNOT_BE_ENHANCED");
			cardInformation.SetActive(value: true);
			cardHolder.ShowNoEnhanceCard(card.AbilityCard);
		}
	}

	private void RefreshSelectionAfterEnhance(AbilityCardUI card)
	{
		ResetPreviewEnhancement();
		OnSelectedCardToEnhance(card, animateShowCard: true);
	}

	private void OnSelectedCardToEnhance(AbilityCardUI card, bool animateShowCard = false)
	{
		if (card == null && selectedCard != null)
		{
			previousSelectedCard = selectedCard;
		}
		selectedCard = card;
		SwitchToCard(card, animateShowCard);
	}

	private void SwitchToCard(AbilityCardUI card, bool animateShowCard)
	{
		ClearPreviewEnhancement();
		ShowCard(card, animateShowCard);
		SetEnhanceFilters(card);
		HighlightButtons();
	}

	public bool AvailableToEnhanceSelectedCard()
	{
		if (selectedCard != null)
		{
			return AvailableToEnhanceCard(selectedCard);
		}
		return false;
	}

	private bool AvailableToEnhanceCard(AbilityCardUI card)
	{
		return EnhancementUtils.HaveEnhancementButtons(card, mode == ShopMode.SELL);
	}

	public void SetFullyEnhanced(bool fullyEnhanced)
	{
		if (fullyEnhanced)
		{
			cardInformationText.SetTextKey("Consoles/GUI_ABILITY_FULLY_ENHANCED");
		}
		cardInformation.SetActive(fullyEnhanced);
	}

	private void OnSelectedEnhacementButton(EnhancementLine line)
	{
		List<EnhancementSlot> enhancements = ((line == null) ? new List<EnhancementSlot>() : (InputManager.GamePadInUse ? GetSellSlots(line).Concat(GetBuySlots(line)).ToList() : ((mode != ShopMode.BUY) ? GetSellSlots(line).ToList() : GetBuySlots(line))));
		enhancementShop.Display(character, enhancements);
		if (selectedCard != null)
		{
			HighlightButtons();
		}
	}

	private IEnumerable<EnhancementSlot> GetSellSlots(EnhancementLine line)
	{
		EnhancementSellPriceCalculator sellPriceCalculator = new EnhancementSellPriceCalculator(line, character);
		return from it in line.EnhancementSlots
			where it.EnhancedType != EEnhancement.NoEnhancement
			select new EnhancementSlot(it, it.Enhancement.Enhancement, buyMode: false, line, sellPriceCalculator);
	}

	private List<EnhancementSlot> GetBuySlots(EnhancementLine line)
	{
		if (cardHolder.Card != null)
		{
			CAbilityCard abilityCard = cardHolder.Card.AbilityCard;
			EnhancementBuyPriceCalculator buyPriceCalculator = new EnhancementBuyPriceCalculator(line.Ability, abilityCard, character);
			List<EnhancementSlot> enhancementsToBuy = shopService.GetEnhancementsToBuy(line);
			enhancementsToBuy.ForEach(delegate(EnhancementSlot slot)
			{
				slot.priceCalculator = buyPriceCalculator;
			});
			return enhancementsToBuy;
		}
		return new List<EnhancementSlot>();
	}

	private void HighlightButtons(bool isCurrentCard = true)
	{
		List<EnhancementButtonBase> obj = ((cardHolder.Card == null) ? new List<EnhancementButtonBase>() : cardHolder.Card.EnhancementElements.All);
		Dictionary<RectTransform, EnhancementButtonBase> dictionary = new Dictionary<RectTransform, EnhancementButtonBase>();
		foreach (EnhancementButtonBase item in obj)
		{
			if (item.ParentContainer == null)
			{
				string value = ((item is EnhancedAreaHex) ? "Column Container" : (item.EnhancementLine.ToString().StartsWith("Summon") ? "Summon" : "Row Container"));
				Transform parent = item.transform.parent;
				while (!parent.name.StartsWith(value))
				{
					parent = parent.parent;
					if (!(parent.parent != null))
					{
						break;
					}
				}
				item.ParentContainer = parent as RectTransform;
			}
			if (item.ParentContainer != null && (InputManager.GamePadInUse || (!InputManager.GamePadInUse && ((item.Enhancement.Enhancement == EEnhancement.NoEnhancement && mode == ShopMode.BUY) || (item.Enhancement.Enhancement != EEnhancement.NoEnhancement && mode == ShopMode.SELL)))))
			{
				dictionary[item.ParentContainer] = item;
			}
		}
		ClearHighlights(dictionary.Count);
		int num = 0;
		foreach (KeyValuePair<RectTransform, EnhancementButtonBase> item2 in dictionary)
		{
			if (num >= highlightAbilityPool.Count)
			{
				highlightAbilityPool.Add(UnityEngine.Object.Instantiate(highlightAbilityPool[0], item2.Key.transform));
			}
			else
			{
				highlightAbilityPool[num].transform.SetParent(item2.Key.transform);
			}
			highlightAbilityPool[num].transform.SetAsFirstSibling();
			highlightAbilityPool[num].gameObject.SetActive(value: true);
			if (!isCurrentCard)
			{
				highlightAbilityPool[num].HighlightPreview(item2.Key, item2.Value);
			}
			else if (item2.Value.Ability == enhancementLineFilter.CurrentFilter?.Ability && item2.Value.EnhancementLine == enhancementLineFilter.CurrentFilter?.Line)
			{
				highlightAbilityPool[num].HighlightSelected(item2.Key, item2.Value);
			}
			else
			{
				highlightAbilityPool[num].HighlightSelectable(item2.Key, item2.Value, delegate(EnhancementButtonBase enh)
				{
					enhancementLineFilter.SelectFilter(enh.Ability, enh.EnhancementLine);
				});
			}
			highlightAbilityPool[num].SetMode(mode == ShopMode.BUY);
			highlightAbility.Add(highlightAbilityPool[num]);
			num++;
		}
		if (controllerArea.IsFocused)
		{
			EnableCardHighlightsNavigation();
		}
	}

	private void SwitchShopMode()
	{
		if (mode == ShopMode.BUY)
		{
			if (shopService.IsSellAvailable)
			{
				SelectTabElementByShopMode(buyOrSell: false);
				sellButton.Activate();
			}
		}
		else
		{
			SelectTabElementByShopMode(buyOrSell: true);
			buyButton.Activate();
		}
	}

	private void ShowTooltip()
	{
		if (shopService.IsEnchantressIntroShown)
		{
			introduction.Hide();
			return;
		}
		introduction.Show();
		shopService.SetEnchantressIntroShown();
	}

	private void ConfirmBuy(EnhancementSlot enhancement)
	{
		bool flag = true;
		int num = CEnhancement.TotalCost(enhancement.enhancement, selectedCard.AbilityCard, enhancement.Ability, AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent);
		if (character.Gold < num)
		{
			flag = false;
			NewPartyDisplayUI.PartyDisplay.ShowWarningGold(show: true, (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? character.CharacterID : null);
		}
		if (!EnhancementBuyPriceCalculator.CanAffordPoints(enhancement, character))
		{
			flag = false;
			cardsDisplay.ShowWarningPoints(show: true);
		}
		EnhancementButtonBase button = enhancement.AnyEmpty;
		if (button == null)
		{
			flag = false;
		}
		if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0} <size=+5><font=\"MarcellusSC-Regular SDF\"><color=#F1DBAE>{1}</color></font></size> <size=+15><sprite name=\"AA_{2}\" color=#{3}></size>", LocalizationManager.GetTranslation("GUI_ENHANCE"), LocalizationManager.GetTranslation(selectedCard.AbilityCard.Name), character.Class.Model, UIInfoTools.Instance.GetCharacterColor(character.Class.Model, character.Class.CustomCharacterConfig).ToHex());
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("<color=#{2}>{0} <sprite name=\"Gold_Icon_White\" color=#{2}>{1}</color>", LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST"), num, UIInfoTools.Instance.goldColor.ToHex());
		_isConfirmationBoxOpened = true;
		Singleton<UIEnhancementConfirmationBox>.Instance.ShowConfirmation(LocalizationManager.GetTranslation("GUI_BUY_ENHANCEMENT_CONFIRMATION_TITLE"), stringBuilder.ToString(), enhancement.enhancement, delegate
		{
			_isConfirmationBoxOpened = false;
			if (FFSNetwork.IsOnline)
			{
				if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining) || !PlayerRegistry.ConnectingUsers.IsNullOrEmpty())
				{
					return;
				}
				if (FFSNetwork.IsHost)
				{
					SceneController.Instance.CheckingLockedContent = true;
				}
				IProtocolToken supplementaryDataToken = new EnhancementToken(button, enhancement.enhancement);
				Synchronizer.SendGameAction(GameActionType.BuyEnhancement, ActionPhaseType.MapHQ, validateOnServerBeforeExecuting: true, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
				if (FFSNetwork.IsClient)
				{
					return;
				}
			}
			AudioControllerUtils.PlaySound(audioItemBuyEnhancement);
			shopService.AddEnhancement(button, enhancement.enhancement);
			cardsDisplay.OnAddedEnhancement(CheckedCard.AbilityCard);
			RefreshSelectionAfterEnhance(CheckedCard);
		}, "GUI_BUY", null, delegate
		{
			_isConfirmationBoxOpened = false;
		});
	}

	private void ConfirmSell(EnhancementSlot enhancement)
	{
		_isConfirmationBoxOpened = true;
		if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			return;
		}
		int num = character.CalculateSellPriceEnhancement(enhancement.button);
		string information = string.Format("<color=#{2}>{0} <sprite name=\"Gold_Icon_White\" color=#{2}>{1}</color>", LocalizationManager.GetTranslation("Consoles/GUI_COST") + ":", num, UIInfoTools.Instance.goldColor.ToHex());
		Singleton<UIEnhancementConfirmationBox>.Instance.ShowConfirmation(LocalizationManager.GetTranslation("GUI_SELL_ENHANCEMENT_CONFIRMATION_TITLE"), information, enhancement.enhancement, delegate
		{
			_isConfirmationBoxOpened = false;
			if (FFSNetwork.IsOnline)
			{
				IProtocolToken supplementaryDataToken = new EnhancementToken(enhancement.button, enhancement.enhancement);
				Synchronizer.SendGameAction(GameActionType.SellEnhancement, ActionPhaseType.MapHQ, validateOnServerBeforeExecuting: true, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
				if (FFSNetwork.IsClient)
				{
					return;
				}
			}
			AudioControllerUtils.PlaySound(audioItemSellEnhancement);
			shopService.RemoveEnhancement(enhancement.button);
			cardsDisplay.OnRemovedEnhancement(CheckedCard.AbilityCard);
			RefreshSelectionAfterEnhance(CheckedCard);
			if (!GamepadMode)
			{
				controllerArea.Focus();
			}
		}, "GUI_SELL", null, delegate
		{
			_isConfirmationBoxOpened = false;
		});
	}

	private void PreviewEnhancement(EnhancementSlot enhancement)
	{
		if (previewedEnhancement != null)
		{
			ClearPreviewEnhancement();
		}
		if (mode == ShopMode.BUY)
		{
			previewedEnhancement = enhancement.button;
		}
		shopService.PreviewEnhancement(previewedEnhancement, enhancement.enhancement);
	}

	private void ClearPreviewEnhancement()
	{
		if (!(previewedEnhancement == null))
		{
			shopService.ClearPreviewEnhancement(previewedEnhancement);
			ResetPreviewEnhancement();
		}
	}

	private void ResetPreviewEnhancement()
	{
		previewedEnhancement = null;
	}

	private void SetEnhanceFilters(AbilityCardUI card)
	{
		EnhancementLine currentFilter = enhancementLineFilter.CurrentFilter;
		List<EnhancementLine> enhancementLines = EnhancementUtils.GetEnhancementLines(card, mode == ShopMode.SELL);
		EnhancementLine enhancementLine = ((currentFilter == null && enhancementLines.Count > 0) ? enhancementLines[0] : enhancementLines.FirstOrDefault((EnhancementLine line) => currentFilter != null && line.Ability == currentFilter.Ability && line.Line == currentFilter.Line));
		if (enhancementLine == null && enhancementLines.Count > 0)
		{
			enhancementLine = enhancementLines[0];
		}
		enhancementLineFilter.SetFilters(enhancementLines, enhancementLine);
	}

	private void ShowSellOptions()
	{
		if (mode != ShopMode.SELL && shopService.IsSellAvailable)
		{
			EnhancementLine currentFilter = enhancementLineFilter.CurrentFilter;
			cardsDisplay.RefreshMode(isBuy: false);
			enhanctressEffect.ShowModeEffect(isBuy: false);
			mode = ShopMode.SELL;
			if (selectedCard == null)
			{
				SwitchToCard(lastShowedCard, animateShowCard: false);
				return;
			}
			OnSelectedCardToEnhance(selectedCard);
			TrySelectLineOrDefault(currentFilter);
		}
	}

	private void ShowBuyOptions()
	{
		if (mode != ShopMode.BUY)
		{
			EnhancementLine currentFilter = enhancementLineFilter.CurrentFilter;
			mode = ShopMode.BUY;
			cardsDisplay.RefreshMode(isBuy: true);
			enhanctressEffect.ShowModeEffect(isBuy: true);
			if (selectedCard == null)
			{
				SwitchToCard(lastShowedCard, animateShowCard: false);
				return;
			}
			OnSelectedCardToEnhance(selectedCard);
			TrySelectLineOrDefault(currentFilter);
		}
	}

	private void TrySelectLineOrDefault(EnhancementLine enhancementLine)
	{
		if (Singleton<UINavigation>.Instance.StateMachine.CurrentState is EnhancmentSelectOptionUpgradeState)
		{
			return;
		}
		SelectCardFirstRow(selectedCard, highlightAbility.FirstOrDefault(delegate(UIEnhancementButtonHighlight highlight)
		{
			if (enhancementLine?.EnhancementSlots != null)
			{
				EnhancementButtonBase enhancementButtonBase = enhancementLine.EnhancementSlots.First();
				if (enhancementButtonBase != null)
				{
					return enhancementButtonBase.Enhancement == highlight.Ability.Enhancement;
				}
				return false;
			}
			return false;
		}));
	}

	private void ClearHighlights(int from = 0)
	{
		if (!CoreApplication.IsQuitting)
		{
			for (int i = from; i < highlightAbility.Count; i++)
			{
				highlightAbility[i].gameObject.SetActive(value: false);
				highlightAbility[i].transform.SetParent(highlightAbilityParent);
			}
			highlightAbility.Clear();
		}
	}

	private void OnDisable()
	{
		ClearHighlights();
	}

	private void OnControllerFocused()
	{
		EnableCardHighlightsNavigation();
		cardsDisplay.OnFocus();
	}

	private void EnableCardHighlightsNavigation()
	{
		for (int i = 0; i < highlightAbility.Count; i++)
		{
			highlightAbilityPool[i].EnableNavigation();
		}
	}

	private void OnControllerUnfocused()
	{
		cardsDisplay.OnUnfocus();
		for (int i = 0; i < highlightAbility.Count; i++)
		{
			highlightAbility[i].DisableNavigation();
		}
	}

	private void OnControllableOwnershipChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		enhancementShop.DetermineSlotInteractability();
	}

	public void ProxyBuyEnhancement(GameAction action, ref bool actionValid)
	{
		EnhancementToken enhancementToken = (EnhancementToken)action.SupplementaryDataToken;
		CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter x) => x.OwnedAbilityCardIDs.Contains(enhancementToken.CardID));
		FFSNet.Console.LogInfo("About to buy enhancement for a proxy character " + enhancementToken.GetDataString());
		if (cMapCharacter != null)
		{
			CAbilityCard cAbilityCard = cMapCharacter.OwnedAbilityCards.First((CAbilityCard x) => x.ID == enhancementToken.CardID);
			EnhancementButtonBase enhancementButton = EnhancementUtils.GetEnhancementButton(ObjectPool.GetAllCachedAbilityCards(cAbilityCard.ID)[0]?.GetComponent<AbilityCardUI>(), enhanced: false, enhancementToken.AbilityName, (EEnhancementLine)enhancementToken.EnhancementLineID, enhancementToken.EnhancementSlot);
			if (enhancementButton != null)
			{
				shopService.AddEnhancement(enhancementButton, (EEnhancement)enhancementToken.EnhancementTypeID);
				if (shopWindow.IsVisible)
				{
					if (cMapCharacter.IsUnderMyControl)
					{
						AudioControllerUtils.PlaySound(audioItemBuyEnhancement);
					}
					bool flag = selectedCard != null && selectedCard.AbilityCard.ID == cAbilityCard.ID;
					if (cMapCharacter.CharacterID == ((ICharacterService)character).CharacterID)
					{
						cardsDisplay.OnAddedEnhancement(cAbilityCard, flag);
					}
					if (flag)
					{
						RefreshSelectionAfterEnhance(selectedCard);
					}
				}
				actionValid = true;
				FFSNet.Console.LogInfo("Enhancement successfully purchased.");
			}
			else
			{
				actionValid = false;
				if (FFSNetwork.IsClient)
				{
					throw new Exception("Error buying enhancement. Cannot find the target EnhancementButton for the card " + cAbilityCard.Name + " with enhancement " + enhancementToken.GetDataString());
				}
			}
		}
		else
		{
			actionValid = false;
			if (FFSNetwork.IsClient)
			{
				throw new Exception("Error buying enhancement. Cannot find the target actor (ActorID: " + action.ActorID + ").");
			}
		}
	}

	public void ProxySellEnhancement(GameAction action, ref bool actionValid)
	{
		EnhancementToken enhancementToken = (EnhancementToken)action.SupplementaryDataToken;
		CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter x) => x.OwnedAbilityCardIDs.Contains(enhancementToken.CardID));
		FFSNet.Console.LogInfo("About to sell enhancement for a proxy character " + enhancementToken.GetDataString());
		if (cMapCharacter != null)
		{
			FFSNet.Console.LogInfo("Printing enhancements for: " + cMapCharacter.CharacterID);
			foreach (CEnhancement enhancement in cMapCharacter.Enhancements)
			{
				FFSNet.Console.LogInfo("Card Name: " + enhancement.AbilityCard.Name + ", Ability Name: " + enhancement.AbilityName + ", Enh Type: " + enhancement.Enhancement.ToString() + ", Enh Line: " + enhancement.EnhancementLine.ToString() + ", Enh Slot: " + enhancement.EnhancementSlot);
			}
			CAbilityCard cAbilityCard = cMapCharacter.OwnedAbilityCards.First((CAbilityCard x) => x.ID == enhancementToken.CardID);
			EnhancementButtonBase enhancementButton = EnhancementUtils.GetEnhancementButton(ObjectPool.GetAllCachedAbilityCards(cAbilityCard.ID)[0]?.GetComponent<AbilityCardUI>(), enhanced: true, enhancementToken.AbilityName, (EEnhancementLine)enhancementToken.EnhancementLineID, enhancementToken.EnhancementSlot);
			if (enhancementButton != null)
			{
				shopService.RemoveEnhancement(enhancementButton);
				if (shopWindow.IsVisible)
				{
					if (cMapCharacter.IsUnderMyControl)
					{
						AudioControllerUtils.PlaySound(audioItemSellEnhancement);
					}
					bool flag = selectedCard != null && selectedCard.AbilityCard.ID == cAbilityCard.ID;
					if (cMapCharacter.CharacterID == ((ICharacterService)character).CharacterID)
					{
						cardsDisplay.OnRemovedEnhancement(cAbilityCard, flag);
					}
					if (flag)
					{
						RefreshSelectionAfterEnhance(selectedCard);
					}
				}
				actionValid = true;
				FFSNet.Console.LogInfo("Enhancement successfully sold.");
			}
			else
			{
				actionValid = false;
				if (FFSNetwork.IsClient)
				{
					throw new Exception("Error selling enhancement. Cannot find the target EnhancementButton for the card " + cAbilityCard.Name + " with enhancement " + enhancementToken.GetDataString());
				}
			}
		}
		else
		{
			actionValid = false;
			if (FFSNetwork.IsClient)
			{
				throw new Exception("Error selling enhancement. Cannot find the target actor (ActorID: " + action.ActorID + ").");
			}
		}
	}

	public void SelectTabElementByShopMode(bool buyOrSell)
	{
		enhancementShop.SelectTabElementByShopMode(buyOrSell);
	}
}
