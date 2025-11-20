using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using FFSNet;
using JetBrains.Annotations;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardsHandPreviewUI : MonoBehaviour
{
	[SerializeField]
	private UICharacterStats characterStats;

	[SerializeField]
	private Transform abilityCardsHolder;

	[SerializeField]
	private CardHandHeader burntHeader;

	[SerializeField]
	private CardHandHeader discardedHeader;

	[SerializeField]
	private ItemsUI itemsUI;

	[SerializeField]
	private ModifiersDisplayController modifiersController;

	[SerializeField]
	private int fullCardCanvasOrder = 2;

	[SerializeField]
	private int fullCardPositionX = -270;

	[SerializeField]
	private UiNavigationGroup navGroup;

	[SerializeField]
	private Image characterPortrait;

	private Action onNextCallback;

	private Action onPreviousCallback;

	private List<AbilityCardUI> cardsUI = new List<AbilityCardUI>();

	private bool cardsSpawned;

	private CardsHandUI cardHand;

	private bool cardsSelectable;

	private ExtendedButton modifiersButton;

	private AbilityCardUI hoveredCard;

	public CPlayerActor PlayerActor => cardHand.PlayerActor;

	public CardsHandUI CardsHandUI => cardHand;

	[UsedImplicitly]
	private void Awake()
	{
		modifiersButton = GetComponentInChildren<ExtendedButton>();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Release();
	}

	public void ToggleAttackModifiers(bool active)
	{
		try
		{
			if (active)
			{
				modifiersController.Display(PlayerActor.CharacterClass, !FFSNetwork.IsOnline || PlayerActor.IsUnderMyControl);
			}
			else
			{
				modifiersController.Hide();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.ToggleAttackModifiers().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Release()
	{
		if (CoreApplication.IsQuitting)
		{
			return;
		}
		foreach (AbilityCardUI item in cardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		cardsUI.Clear();
		cardHand = null;
	}

	public void UpdateCharText()
	{
		characterStats.UpdateCharText();
	}

	public void Init(CardsHandUI cardHand, Action onNextCallback = null, Action onPreviousCallback = null)
	{
		try
		{
			this.onNextCallback = onNextCallback;
			this.onPreviousCallback = onPreviousCallback;
			this.cardHand = cardHand;
			if (!cardsSpawned)
			{
				SpawnCards(PlayerActor.CharacterClass.SelectedAbilityCards);
			}
			characterStats.Init(PlayerActor);
			characterStats.Show(show: true);
			itemsUI.SetActor(PlayerActor, subscribeToEvents: false);
			characterPortrait.sprite = UIInfoTools.Instance.GetCharacterHeroPortrait(PlayerActor.CharacterClass.CharacterID);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.Init().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public IEnumerator InitCoroutine(CardsHandUI cardHand, Action onNextCallback = null, Action onPreviousCallback = null)
	{
		this.onNextCallback = onNextCallback;
		this.onPreviousCallback = onPreviousCallback;
		this.cardHand = cardHand;
		if (!cardsSpawned)
		{
			yield return SpawnCardsCoroutine(PlayerActor.CharacterClass.SelectedAbilityCards);
		}
		characterStats.Init(PlayerActor);
		characterStats.Show(show: true);
		itemsUI.SetActor(PlayerActor, subscribeToEvents: false);
		characterPortrait.sprite = UIInfoTools.Instance.GetCharacterHeroPortrait(PlayerActor.CharacterClass.CharacterID);
	}

	public void Hide()
	{
		try
		{
			base.gameObject.SetActive(value: false);
			ResetView();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.Hide().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ResetView()
	{
		modifiersController.Hide(immendiately: true);
		if (hoveredCard != null)
		{
			hoveredCard.OnTransitNavigationUnmarked();
			hoveredCard.OnPointerExit();
			hoveredCard = null;
		}
	}

	public void Show(bool cardsSelectable)
	{
		try
		{
			characterStats.UpdateCharText();
			this.cardsSelectable = cardsSelectable && cardHand.IsInteractable;
			UpdateView();
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.Show().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void UpdateView()
	{
		try
		{
			bool flag = !FFSNetwork.IsOnline || PlayerActor == null || (PlayerActor.IsUnderMyControl && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar);
			foreach (AbilityCardUI item in cardsUI)
			{
				CopyState(item);
				item.fullAbilityCard.UpdateView(UISettings.DefaultFullCardViewSettings);
				item.fullAbilityCard.IsActionSelection = false;
				item.SetUnfocused(FFSNetwork.IsOnline && PlayerActor != null && !PlayerActor.IsUnderMyControl);
				item.SetMode(CardHandMode.CardsSelection, cardsSelectable ? new List<CardPileType>
				{
					CardPileType.Hand,
					CardPileType.Round
				} : null);
				if (flag)
				{
					item.SetValid(isValid: true);
					if (!cardsSelectable)
					{
						item.fullAbilityCard.SetValid(isValid: false);
					}
				}
				else if (PlayerActor.IsUnderMyControl)
				{
					item.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
				}
				else
				{
					item.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
				}
			}
			SortCards();
			UpdateEffects();
			itemsUI.SetUnfocused(FFSNetwork.IsOnline && PlayerActor != null && !PlayerActor.IsUnderMyControl);
			modifiersController.SetUnfocused(FFSNetwork.IsOnline && PlayerActor != null && !PlayerActor.IsUnderMyControl);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.UpdateView().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void SetCardsSelectable(bool selectable)
	{
		bool flag = selectable && cardHand.IsInteractable;
		if (!base.gameObject.activeSelf || cardsSelectable == flag)
		{
			return;
		}
		cardsSelectable = flag;
		bool flag2 = !FFSNetwork.IsOnline || PlayerActor == null || (PlayerActor.IsUnderMyControl && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar);
		foreach (AbilityCardUI item in cardsUI)
		{
			item.SetMode(CardHandMode.CardsSelection, flag ? new List<CardPileType>
			{
				CardPileType.Hand,
				CardPileType.Round
			} : null);
			if (flag2)
			{
				item.SetValid(isValid: true);
				if (!cardsSelectable)
				{
					item.fullAbilityCard.SetValid(isValid: false);
				}
			}
			else if (PlayerActor.IsUnderMyControl)
			{
				item.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
			}
			else
			{
				item.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			}
			item.SetUnfocused(FFSNetwork.IsOnline && PlayerActor != null && !PlayerActor.IsUnderMyControl);
		}
	}

	private void CopyState(AbilityCardUI cardUI)
	{
		AbilityCardUI card = cardHand.GetCard(cardUI.AbilityCard.ID);
		if (PlayerActor.CharacterClass.DiscardedAbilityCards.Contains(cardUI.AbilityCard))
		{
			cardUI.SetType(CardPileType.Discarded);
		}
		else if (PlayerActor.CharacterClass.LostAbilityCards.Contains(cardUI.AbilityCard))
		{
			cardUI.SetType(CardPileType.Lost);
		}
		else if (PlayerActor.CharacterClass.PermanentlyLostAbilityCards.Contains(cardUI.AbilityCard))
		{
			cardUI.SetType(CardPileType.Permalost);
		}
		else
		{
			cardUI.SetType(card.CardType);
		}
		cardUI.UpdateCard();
	}

	private void UpdateEffects()
	{
		try
		{
			bool flag = !FFSNetwork.IsOnline || PlayerActor.IsUnderMyControl || PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && PlayerActor.CharacterClass.RoundAbilityCards.Count >= 1 && flag)
			{
				foreach (AbilityCardUI item in cardsUI)
				{
					item.PlayEffect((PlayerActor.CharacterClass.RoundAbilityCards[0] == item.AbilityCard) ? InitiativeTrackActorAvatar.InitiativeEffects.Active : InitiativeTrackActorAvatar.InitiativeEffects.None);
				}
				return;
			}
			if (PlayerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && flag)
			{
				foreach (AbilityCardUI item2 in cardsUI)
				{
					item2.PlayEffect((PlayerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Count > 0 && PlayerActor.CharacterClass.ExtraTurnInitiativeAbilityCard == item2.AbilityCard) ? InitiativeTrackActorAvatar.InitiativeEffects.Active : InitiativeTrackActorAvatar.InitiativeEffects.None);
				}
				return;
			}
			foreach (AbilityCardUI item3 in cardsUI)
			{
				item3.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.UpdateEffects().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void SortCards()
	{
		try
		{
			cardsUI.Sort();
			discardedHeader.Hide();
			burntHeader.Hide();
			List<CardHandHeader> list = new List<CardHandHeader>();
			foreach (AbilityCardUI item in cardsUI)
			{
				if (item.CardType == CardPileType.Unselected && !list.Contains(burntHeader))
				{
					list.Add(burntHeader);
					burntHeader.transform.SetAsLastSibling();
				}
				CardHandHeader header = GetHeader(item.CardType);
				if (header != null)
				{
					if (!list.Contains(header))
					{
						list.Add(header);
						header.transform.SetAsLastSibling();
					}
					if (item.gameObject.activeSelf)
					{
						header.Show();
					}
				}
				item.transform.SetAsLastSibling();
			}
			if (!list.Contains(burntHeader))
			{
				burntHeader.transform.SetAsLastSibling();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.SortCards().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private CardHandHeader GetHeader(CardPileType cardType)
	{
		switch (cardType)
		{
		case CardPileType.Active:
			return null;
		case CardPileType.Discarded:
			return discardedHeader;
		case CardPileType.Lost:
		case CardPileType.Permalost:
			return burntHeader;
		default:
			return null;
		}
	}

	private void SpawnCards(List<CAbilityCard> abilityCards)
	{
		cardsSpawned = true;
		cardsUI.Clear();
		foreach (CAbilityCard abilityCard in abilityCards)
		{
			if (abilityCard.GetAbilityCardYML == null)
			{
				Debug.LogError("Ability card data is null! Check YML file for " + abilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
				continue;
			}
			try
			{
				AbilityCardUI component = ObjectPool.SpawnCard(abilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>();
				component.Init(abilityCard, PlayerActor, CardPileType.Hand, OnSelectedCard, OnDeselectedCard, null, OnHovered, null, null, null, isLongRest: false, (RectTransform rect) => new Vector2(fullCardPositionX, rect.anchoredPosition.y), CanToggleCard);
				component.transform.localPosition = Vector3.zero;
				cardsUI.Add(component);
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to initialise ability card " + abilityCard.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
	}

	private IEnumerator SpawnCardsCoroutine(List<CAbilityCard> abilityCards)
	{
		cardsSpawned = true;
		cardsUI.Clear();
		foreach (CAbilityCard abilityCard in abilityCards)
		{
			if (abilityCard.GetAbilityCardYML == null)
			{
				Debug.LogError("Ability card data is null! Check YML file for " + abilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
				continue;
			}
			try
			{
				AbilityCardUI component = ObjectPool.SpawnCard(abilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>();
				component.Init(abilityCard, PlayerActor, CardPileType.Hand, OnSelectedCard, OnDeselectedCard, null, OnHovered, null, null, null, isLongRest: false, (RectTransform rect) => new Vector2(fullCardPositionX, rect.anchoredPosition.y), CanToggleCard);
				component.transform.localPosition = Vector3.zero;
				cardsUI.Add(component);
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to initialise ability card " + abilityCard.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
			yield return null;
		}
	}

	private bool CanToggleCard()
	{
		return PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest;
	}

	private void OnDeselectedCard(AbilityCardUI card, bool arg2)
	{
		cardHand.UnselectCard(card.AbilityCard);
		UpdateEffects();
	}

	private void OnSelectedCard(AbilityCardUI card, bool arg2)
	{
		cardHand.SelectCard(card.AbilityCard);
		foreach (AbilityCardUI item in cardsUI)
		{
			if (item.CardType == CardPileType.Round)
			{
				CopyState(item);
			}
		}
		UpdateEffects();
	}

	private void OnHovered(bool hovered, AbilityCardUI cardUI)
	{
		if (hovered)
		{
			hoveredCard = cardUI;
			cardUI.fullAbilityCard.HighlightInCanvas(fullCardCanvasOrder);
		}
		else if (hoveredCard == cardUI)
		{
			hoveredCard = null;
		}
	}

	public void MoveNextDeckTab()
	{
		onNextCallback?.Invoke();
	}

	public void MovePreviousDeckTab()
	{
		onPreviousCallback?.Invoke();
	}

	public void EnableNavigation()
	{
		List<Selectable> buttons = (from it in cardsUI
			where it.gameObject.activeSelf && it.IsHoverable
			select it.GetComponent<Selectable>()).ToList();
		for (int num = 0; num < buttons.Count; num++)
		{
			buttons[num].SetNavigation(null, modifiersButton, down: buttons[(num + 1) % buttons.Count], up: buttons[(num == 0) ? (buttons.Count - 1) : (num - 1)]);
		}
		modifiersButton.SetNavigation(new NavigationCalculator
		{
			right = () => buttons[0],
			down = itemsUI.GetFirstEquippedItem
		});
		itemsUI.EnableNavigation(modifiersButton, buttons[0]);
	}

	public void DisableNavigation()
	{
		cardsUI.ForEach(delegate(AbilityCardUI it)
		{
			it.DisableNavigation();
		});
		itemsUI.DisableNavigation();
		modifiersButton.DisableNavigation();
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void Select()
	{
		if (navGroup != null)
		{
			Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(navGroup);
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(cardsUI[0].gameObject);
		}
	}
}
