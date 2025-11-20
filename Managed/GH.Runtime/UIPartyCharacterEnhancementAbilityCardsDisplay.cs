using System;
using System.Collections.Generic;
using MapRuleLibrary.Adventure;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.CampaignMapStates.Enhancment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPartyCharacterEnhancementAbilityCardsDisplay : MonoBehaviour
{
	[SerializeField]
	private HotkeyContainer hotkeyContainer;

	[SerializeField]
	private TextMeshProUGUI enhancementPointsText;

	[SerializeField]
	private TextMeshProUGUI warningEnhancementPointsText;

	[SerializeField]
	private HeaderHighlight highlight;

	[SerializeField]
	private ScrollRect abilityCardsPanel;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private Image headerBackground;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private string audioItemShow = "PlaySound_UICardTabSelect";

	[SerializeField]
	private UIEnhanceCardSlot slotPrefab;

	[SerializeField]
	private UiNavigationRoot _navigationRoot;

	[SerializeField]
	private ControllerInputScroll _controllerInputScroll;

	private List<UIEnhanceCardSlot> slotsPool = new List<UIEnhanceCardSlot>();

	private Dictionary<CAbilityCard, UIEnhanceCardSlot> assignedSlots = new Dictionary<CAbilityCard, UIEnhanceCardSlot>();

	private LTDescr animationDisplay;

	private ICharacterEnhancementService characterData;

	private UIEnhanceCardSlot selectedCard;

	private Action<AbilityCardUI> onAbilityCardSelected;

	private int currentPoints;

	private bool isOpen;

	public HotkeyContainer Hotkeys => hotkeyContainer;

	private void Awake()
	{
		showAnimation.OnAnimationFinished.AddListener(delegate
		{
			if (!isOpen)
			{
				isOpen = true;
				OnFocus();
			}
		});
	}

	public void Display(ICharacterEnhancementService characterData, Action<AbilityCardUI> onAbilityCardSelected, Action<bool, AbilityCardUI> onAbilityCardHover, bool isBuy, RectTransform sourceUI, bool autoselectFirst)
	{
		verticalPointer.PointAt(sourceUI);
		this.onAbilityCardSelected = onAbilityCardSelected;
		this.characterData = characterData;
		abilityCardsPanel.verticalNormalizedPosition = 1f;
		ShowWarningPoints(show: false);
		selectedCard = null;
		assignedSlots.Clear();
		List<CAbilityCard> list;
		if (characterData == null)
		{
			list = new List<CAbilityCard>();
			headerBackground.enabled = false;
		}
		else
		{
			list = characterData.GetOwnedAbilityCards();
			headerBackground.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(characterData.Class.Model, highlighted: true, characterData.Class.CustomCharacterConfig);
			headerBackground.enabled = true;
		}
		HelperTools.NormalizePool(ref slotsPool, slotPrefab.gameObject, abilityCardsPanel.content, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			CAbilityCard cAbilityCard = list[i];
			UIEnhanceCardSlot uIEnhanceCardSlot = slotsPool[i];
			uIEnhanceCardSlot.Init(cAbilityCard, characterData, OnSelectedCard, delegate(bool hovered, AbilityCardUI card)
			{
				onAbilityCardHover?.Invoke(hovered, card);
				if (hovered && InputManager.GamePadInUse)
				{
					abilityCardsPanel.ScrollToFit(card.transform as RectTransform);
				}
			}, isBuy);
			assignedSlots[cAbilityCard] = uIEnhanceCardSlot;
		}
		UpdateEnhancementPoints();
		if (!isOpen)
		{
			Show();
		}
		foreach (KeyValuePair<CAbilityCard, UIEnhanceCardSlot> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.AbilityCard.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
		}
		if (_navigationRoot != null)
		{
			Singleton<UINavigation>.Instance.NavigationManager.DeselectCurrentSelectable();
			Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(_navigationRoot);
		}
	}

	private void OnSelectedCard(AbilityCardUI cardUI)
	{
		UIEnhanceCardSlot uIEnhanceCardSlot = assignedSlots[cardUI.AbilityCard];
		if (selectedCard != null)
		{
			selectedCard.Deselect();
		}
		onAbilityCardSelected(cardUI);
		selectedCard = uIEnhanceCardSlot;
	}

	public void Deselect()
	{
		if (selectedCard != null)
		{
			selectedCard.Deselect();
			selectedCard = null;
		}
	}

	public void OnAddedEnhancement(CAbilityCard card, bool showAnimation = true)
	{
		int num = currentPoints;
		assignedSlots[card].ShowEnhanced(show: true);
		UpdateEnhancementPoints();
		if (num != currentPoints && showAnimation)
		{
			highlight.ShowWarning();
		}
	}

	public void OnRemovedEnhancement(CAbilityCard card, bool showAnimation = true)
	{
		int num = currentPoints;
		assignedSlots[card].ShowEnhanced(show: false);
		UpdateEnhancementPoints();
		if (num != currentPoints && showAnimation)
		{
			highlight.ShowHighlight();
		}
	}

	private void CancelAnimations()
	{
		if (animationDisplay != null)
		{
			LeanTween.cancel(animationDisplay.id);
		}
		animationDisplay = null;
		ShowWarningPoints(show: false);
	}

	private void Show()
	{
		CancelAnimations();
		base.gameObject.SetActive(value: true);
		showAnimation.Play();
		AudioControllerUtils.PlaySound(audioItemShow);
	}

	public void UpdateEnhancementPoints()
	{
		currentPoints = characterData?.GetFreeEnhancementSlots() ?? 0;
		TextMeshProUGUI textMeshProUGUI = enhancementPointsText;
		string text = (warningEnhancementPointsText.text = (AdventureState.MapState.IsCampaign ? currentPoints.ToString() : $"{currentPoints}/{AdventureState.MapState.HeadquartersState.EnhancementSlots}"));
		textMeshProUGUI.text = text;
		headerBackground.SetAlpha((characterData != null && characterData.HasFreeEnhancementSlots()) ? 1 : 0);
	}

	public void ShowWarningPoints(bool show)
	{
		if (show)
		{
			highlight.ShowWarning();
		}
		else
		{
			highlight.Hide();
		}
	}

	public void Hide()
	{
		isOpen = false;
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		CancelAnimations();
	}

	public void RefreshMode(bool isBuy)
	{
		foreach (KeyValuePair<CAbilityCard, UIEnhanceCardSlot> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.UpdateMode(isBuy);
		}
	}

	public void OnFocus()
	{
		if (isOpen && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<EnhancmentSelectCardOptionState>() && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<EnhancmentConfirmationState>() && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<EnhancmentSelectOptionUpgradeState>())
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.EnhancmentCardSelect);
		}
	}

	public void OnUnfocus()
	{
	}

	public void SetActiveControllerInputScroll(bool active)
	{
		if (InputManager.GamePadInUse && _controllerInputScroll != null)
		{
			_controllerInputScroll.enabled = active;
		}
	}
}
