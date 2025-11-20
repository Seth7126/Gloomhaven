using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UILevelUpCardInventory : MonoBehaviour
{
	[Serializable]
	public class LevelUpCardEvent : UnityEvent<CAbilityCard, bool>
	{
	}

	[SerializeField]
	private TextMeshProUGUI informationText;

	[SerializeField]
	private RectTransform cardsContainer;

	[SerializeField]
	private UILevelUpCard cardPrefab;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private UiNavigationRoot _cardsRoot;

	[SerializeField]
	private LocalHotkeys _panelHotkeyContainer;

	private LTDescr panelAnimation;

	private ICharacterService characterData;

	private List<UILevelUpCard> unownedAbilityCardsUI = new List<UILevelUpCard>();

	private List<UILevelUpCard> newAbilityCardsUI = new List<UILevelUpCard>();

	private List<UILevelUpCard> abilityCardsUI = new List<UILevelUpCard>();

	public LevelUpCardEvent OnCardHovered = new LevelUpCardEvent();

	public UnityEvent OnCardUnhovered = new UnityEvent();

	public LevelUpCardEvent OnCardSelected = new LevelUpCardEvent();

	private UIWindow window;

	private RectTransform windowRectTransform;

	private bool isLocked;

	private bool isNavigationEnabled;

	private bool interactionsEnabled;

	private IHotkeySession _panelHotkeySession;

	private SessionHotkey _backHotkey;

	private SessionHotkey _selectHotkey;

	private SessionHotkey _switchRightHotkey;

	private SessionHotkey _resetMercenaryHotkey;

	private CAbilityCard _currentHoveredCard;

	public bool IsVisible => window.IsVisible;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		windowRectTransform = window.GetComponent<RectTransform>();
		windowRectTransform.anchoredPosition = new Vector2(windowRectTransform.rect.width, windowRectTransform.anchoredPosition.y);
		showAnimation.OnAnimationFinished.AddListener(delegate
		{
			if (isNavigationEnabled && !Singleton<UILevelUpWindow>.Instance.IsShowing)
			{
				ControllerSelectFirstCard();
			}
		});
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		OnCardHovered.RemoveAllListeners();
		OnCardUnhovered.RemoveAllListeners();
		OnCardSelected.RemoveAllListeners();
		CancelAnimation();
		panelAnimation = null;
	}

	public void Show(ICharacterService characterData, float duration = 0f, bool locked = false, bool isOpenByToggle = false)
	{
		characterData?.RemoveCallbackOnPerkPointsChanged(RefreshAvailablePoints);
		this.characterData = characterData;
		isLocked = locked;
		RefreshUnownedCards();
		newAbilityCardsUI.Clear();
		RefreshAvailablePoints();
		DisplayPanel(duration);
		_panelHotkeySession = _panelHotkeyContainer.GetSessionOrEmpty().GetHotkey(out _backHotkey, "Back").GetHotkey(out _selectHotkey, "Select")
			.GetHotkey(out _switchRightHotkey, "Switch_Right")
			.GetHotkey(out _resetMercenaryHotkey, "ResetMercenary", Singleton<UIResetLevelUpWindow>.Instance.ResetAbilities);
		if (isOpenByToggle)
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.FurtherAbilityCards);
		}
	}

	private void DisplayPanel(float duration = 0f)
	{
		if (window.IsOpen)
		{
			return;
		}
		characterData.AddCallbackOnPerkPointsChanged(RefreshAvailablePoints);
		window.Show();
		CancelAnimation();
		if (showAnimation != null)
		{
			showAnimation.Play();
			return;
		}
		if (duration <= 0f)
		{
			windowRectTransform.anchoredPosition = new Vector2(0f, windowRectTransform.anchoredPosition.y);
			ControllerSelectFirstCard();
			return;
		}
		panelAnimation = LeanTween.move(windowRectTransform, new Vector3(0f, windowRectTransform.anchoredPosition.y), duration).setOnComplete((Action)delegate
		{
			panelAnimation = null;
			ControllerSelectFirstCard();
		});
	}

	private void RefreshUnownedCards()
	{
		List<CAbilityCard> unownedAbilityCards = characterData.GetUnownedAbilityCards(1, characterData.Level);
		unownedAbilityCardsUI.Clear();
		HelperTools.NormalizePool(ref abilityCardsUI, cardPrefab.gameObject, cardsContainer, unownedAbilityCards.Count);
		for (int i = 0; i < unownedAbilityCards.Count; i++)
		{
			DecorateCard(unownedAbilityCards[i], abilityCardsUI[i], isNew: false);
			unownedAbilityCardsUI.Add(abilityCardsUI[i]);
		}
		if (isNavigationEnabled)
		{
			EnableNavigation();
		}
	}

	private void DecorateCard(CAbilityCard abilityCard, UILevelUpCard cardUI, bool isNew)
	{
		cardUI.Init(abilityCard, isNew, characterData.CharacterID);
		cardUI.FullAbilityCard.UpdateView(UISettings.DefaultFullCardViewSettings);
		cardUI.SetLocked(isLocked);
		cardUI.OnMouseEnter.RemoveAllListeners();
		cardUI.OnMouseEnter.AddListener(delegate
		{
			OnHoverCard(hovered: true, abilityCard, isNew);
		});
		cardUI.OnMouseExit.RemoveAllListeners();
		cardUI.OnMouseExit.AddListener(delegate
		{
			OnHoverCard(hovered: false, abilityCard, isNew);
		});
		cardUI.OnClick.RemoveAllListeners();
		cardUI.OnClick.AddListener(delegate
		{
			OnCardSelected.Invoke(abilityCard, isNew);
		});
	}

	private void OnHoverCard(bool hovered, CAbilityCard abilityCard, bool isNew)
	{
		if (hovered)
		{
			_currentHoveredCard = abilityCard;
			OnCardHovered.Invoke(abilityCard, isNew);
		}
		else
		{
			OnCardUnhovered.Invoke();
		}
	}

	public void AddNewCard(CAbilityCard abilityCard)
	{
		UILevelUpCard uILevelUpCard;
		if (abilityCardsUI.Count <= unownedAbilityCardsUI.Count + newAbilityCardsUI.Count)
		{
			uILevelUpCard = UnityEngine.Object.Instantiate(cardPrefab, cardsContainer);
			abilityCardsUI.Add(uILevelUpCard);
		}
		else
		{
			uILevelUpCard = abilityCardsUI[unownedAbilityCardsUI.Count + newAbilityCardsUI.Count];
		}
		DecorateCard(abilityCard, uILevelUpCard, isNew: true);
		newAbilityCardsUI.Add(uILevelUpCard);
		if (FFSNetwork.IsOnline && characterData.IsUnderMyControl)
		{
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? characterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID));
			IProtocolToken supplementaryDataToken = new CardInventoryToken(characterData.HandAbilityCardIDs, characterData.OwnedAbilityCardIDs);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyCardInventory, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		if (isNavigationEnabled)
		{
			EnableNavigation();
		}
	}

	public void RemoveCard(CAbilityCard card)
	{
		UILevelUpCard uILevelUpCard = newAbilityCardsUI.FirstOrDefault((UILevelUpCard it) => it.AbilityCard == card);
		if (uILevelUpCard == null)
		{
			uILevelUpCard = unownedAbilityCardsUI.First((UILevelUpCard it) => it.AbilityCard == card);
			unownedAbilityCardsUI.Remove(uILevelUpCard);
		}
		else
		{
			newAbilityCardsUI.Remove(uILevelUpCard);
		}
		uILevelUpCard.Hide();
	}

	public void Hide(float duration = -1f)
	{
		if (InputManager.GamePadInUse)
		{
			foreach (UILevelUpCard item in newAbilityCardsUI)
			{
				item.MouseExit();
			}
		}
		characterData?.RemoveCallbackOnPerkPointsChanged(RefreshAvailablePoints);
		DisableNavigation();
		CancelAnimation();
		window.Hide();
		if (duration <= 0f)
		{
			windowRectTransform.anchoredPosition = new Vector2(windowRectTransform.rect.width, windowRectTransform.anchoredPosition.y);
			ClearContent();
			ResetHotkeys();
			_currentHoveredCard = null;
			return;
		}
		panelAnimation = LeanTween.move(windowRectTransform, new Vector3(windowRectTransform.rect.width, windowRectTransform.anchoredPosition.y), duration).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
		{
			panelAnimation = null;
			ClearContent();
		});
		if (InputManager.GamePadInUse && !LevelMessageUILayoutGroup.IsShown)
		{
			Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
		}
		ResetHotkeys();
		_currentHoveredCard = null;
	}

	private void ClearContent()
	{
		foreach (UILevelUpCard item in abilityCardsUI)
		{
			item.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			CancelAnimation();
			ClearContent();
		}
	}

	private void CancelAnimation()
	{
		if (panelAnimation != null)
		{
			LeanTween.cancel(panelAnimation.id);
			panelAnimation = null;
		}
		if (showAnimation != null)
		{
			showAnimation.Stop();
		}
	}

	public void EnableInteraction(bool enabled)
	{
		interactionsEnabled = enabled;
		canvasGroup.interactable = interactionsEnabled;
		canvasGroup.blocksRaycasts = interactionsEnabled;
		if (isNavigationEnabled)
		{
			if (interactionsEnabled)
			{
				ControllerSelectFirstCard();
			}
			else if (EventSystem.current.currentSelectedGameObject != null && (unownedAbilityCardsUI.Exists((UILevelUpCard it) => it.gameObject == EventSystem.current.currentSelectedGameObject) || newAbilityCardsUI.Exists((UILevelUpCard it) => it.gameObject == EventSystem.current.currentSelectedGameObject)))
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
	}

	private void RefreshAvailablePoints()
	{
		informationText.text = string.Format(LocalizationManager.GetTranslation("GUI_LEVELUP_AVAILABLE_CARDS"), characterData.GetLevelsToLevelUp());
	}

	public void SetShowBackHotkey(bool shown)
	{
		if (_panelHotkeySession != null)
		{
			_backHotkey.SetShown(shown);
		}
	}

	public void SetShowSelectHotkey(bool shown)
	{
		if (_panelHotkeySession != null)
		{
			_selectHotkey.SetShown(shown);
		}
	}

	public void SetShowSwitchRightHotkey(bool shown)
	{
		if (_panelHotkeySession != null)
		{
			_switchRightHotkey.SetShown(shown);
		}
	}

	public void SetShowResetMercenaryHotkey(bool shown)
	{
		if (!_resetMercenaryHotkey.IsValid || !_resetMercenaryHotkey.Session.IsDispose)
		{
			_resetMercenaryHotkey.TrySetShown(shown);
		}
	}

	public void ResetHotkeys()
	{
		_panelHotkeySession?.Dispose();
		_panelHotkeySession = null;
	}

	public void EnableNavigation(Selectable down = null)
	{
		isNavigationEnabled = true;
		if (!InputManager.GamePadInUse)
		{
			List<UILevelUpCard> list = unownedAbilityCardsUI.Concat(newAbilityCardsUI).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Selectable.SetNavigation(null, null, (i == 0) ? null : list[i - 1].Selectable, (i == list.Count - 1) ? down : list[i + 1].Selectable);
			}
		}
		if (InputManager.GamePadInUse)
		{
			if (interactionsEnabled)
			{
				HandleGamePadNavigation();
			}
		}
		else
		{
			HandleGamePadNavigation();
		}
	}

	private void HandleGamePadNavigation()
	{
		if (_currentHoveredCard != null)
		{
			NavigateToGamepad(_currentHoveredCard);
		}
		else
		{
			ControllerSelectFirstCard();
		}
	}

	private void ControllerSelectFirstCard()
	{
		if (!isNavigationEnabled || (showAnimation != null && showAnimation.IsPlaying))
		{
			return;
		}
		if (InputManager.GamePadInUse)
		{
			Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot(_cardsRoot);
			return;
		}
		UILevelUpCard uILevelUpCard = unownedAbilityCardsUI.FirstOrDefault();
		if (uILevelUpCard == null)
		{
			uILevelUpCard = newAbilityCardsUI.FirstOrDefault();
		}
		if (uILevelUpCard != null && canvasGroup.blocksRaycasts)
		{
			EventSystem.current.SetSelectedGameObject(uILevelUpCard.gameObject);
		}
	}

	public void DisableNavigation()
	{
		isNavigationEnabled = false;
	}

	public void NavigateToGamepad(CAbilityCard card)
	{
		if (!isNavigationEnabled || !InputManager.GamePadInUse)
		{
			return;
		}
		Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot(_cardsRoot);
		UILevelUpCard uILevelUpCard = unownedAbilityCardsUI.FirstOrDefault((UILevelUpCard it) => it.AbilityCard == card);
		if (uILevelUpCard == null)
		{
			uILevelUpCard = newAbilityCardsUI.FirstOrDefault((UILevelUpCard it) => it.AbilityCard == card);
		}
		if (uILevelUpCard != null)
		{
			IUiNavigationSelectable miniCardNavigationSelectable = uILevelUpCard.GetMiniCardNavigationSelectable();
			if (miniCardNavigationSelectable != null)
			{
				Singleton<UINavigation>.Instance.NavigationManager.TrySelect(miniCardNavigationSelectable);
			}
		}
	}

	public void NavigateTo(CAbilityCard card)
	{
		if (!isNavigationEnabled)
		{
			return;
		}
		UILevelUpCard uILevelUpCard = unownedAbilityCardsUI.FirstOrDefault((UILevelUpCard it) => it.AbilityCard == card);
		if (uILevelUpCard == null)
		{
			uILevelUpCard = newAbilityCardsUI.FirstOrDefault((UILevelUpCard it) => it.AbilityCard == card);
		}
		EventSystem.current.SetSelectedGameObject(uILevelUpCard?.gameObject);
	}

	public Selectable GetLastSelectable()
	{
		if (newAbilityCardsUI.Count > 0)
		{
			return newAbilityCardsUI.Last().Selectable;
		}
		return unownedAbilityCardsUI.LastOrDefault()?.Selectable;
	}
}
