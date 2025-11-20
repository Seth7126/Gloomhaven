using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using FFSNet;
using GLOO.Introduction;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.HotkeysBehaviour;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.Utils;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPartyCharacterAbilityCardsDisplay : MonoBehaviour, IEscapable
{
	[SerializeField]
	private TextMeshProUGUI infoText;

	[SerializeField]
	private Image infoIcon;

	[SerializeField]
	private TextMeshProUGUI warningText;

	[SerializeField]
	private GUIAnimator errorAnimator;

	[SerializeField]
	private Color regularTitleColor;

	[SerializeField]
	private Color errorTitleColor;

	[SerializeField]
	private string audioItemShow;

	[SerializeField]
	private string audioItemHide;

	[SerializeField]
	private ScrollRect abilityCardsPanel;

	[SerializeField]
	private RectTransform fullCardAreaToFit;

	[SerializeField]
	private float cardHighlightScale = 1.08f;

	[SerializeField]
	private Vector3 cardHighlightDisplacement = new Vector3(14f, 0f, 0f);

	[SerializeField]
	private float fullCardSpacing = 33f;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private GameObject levelUpHeader;

	[SerializeField]
	private ExtendedButton levelUpButton;

	[SerializeField]
	private GameObject levelUpWarning;

	[SerializeField]
	private GameObject levelUpHighlight;

	[SerializeField]
	private UIIntroduce introduction;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private UiNavigationRoot _cardsRoot;

	[SerializeField]
	private UiNavigationRoot _levelUpButtonRoot;

	[SerializeField]
	private LocalHotkeys _panelHotkeyContainer;

	[SerializeField]
	private LocalHotkeys hotkeyContainer;

	[SerializeField]
	private PanelHotkeyContainer _shopInventoryPanelHotkeyContainer;

	[SerializeField]
	private Hotkey _switchRightHotkey;

	[SerializeField]
	private GameObject _dimmer;

	[SerializeField]
	private BackgroundToggleElement _darkenPanel;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	private bool isLevelUpEnabled;

	private bool isLevelUpValid;

	private bool displaySelectHotkeys = true;

	private AbilityCardUI hoveredCard;

	private Action onLevelup;

	private List<AbilityCardUI> abilityCardsUI = new List<AbilityCardUI>();

	private CMapCharacter characterData;

	private Action<bool> onAbilityCardSelected;

	private Action<bool, AbilityCardUI> onAbilityCardHovered;

	private Color? defaultLevelColor;

	private Action onClosed;

	private Coroutine selectElementCoroutine;

	private ControllerInputScroll _controllerInputScroll;

	private IHotkeySession _panelHotkeySession;

	private SessionHotkey _backHotkey;

	private SessionHotkey _selectHotkey;

	private SessionHotkey _unselectHotkey;

	private SessionHotkey _switchLeftHotkey;

	private IHotkeySession _hotkeySession;

	private SessionHotkey _furtherAbilityCardHotkey;

	private SessionHotkey _tipsHotkey;

	private LTDescr animationDisplay;

	private bool _isLevelUpWindowShowing;

	public bool IsVisible => base.gameObject.activeSelf;

	public ControllerInputAreaLocal ControllerInputAreaLocal => controllerArea;

	public bool IsOpen => base.gameObject.activeInHierarchy;

	public IHotkeyContainer PanelHotkeyContainer => _panelHotkeyContainer;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public bool ShowIntro
	{
		get
		{
			if (!AdventureState.MapState.MapParty.HasIntroduced(EIntroductionConcept.AbilityCardPanel.ToString()))
			{
				return !LevelMessageUILayoutGroup.IsShown;
			}
			return false;
		}
	}

	private void Awake()
	{
		levelUpButton.onClick.AddListener(delegate
		{
			if (!isLevelUpValid)
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
				levelUpWarning.SetActive(value: true);
			}
			else
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.generalAudioButtonProfile.mouseDownAudioItem);
				onLevelup?.Invoke();
			}
		});
		if (!defaultLevelColor.HasValue)
		{
			defaultLevelColor = levelUpButton.buttonText.color;
		}
		controllerArea.OnFocusedArea.AddListener(OnControllerAreaFocused);
		controllerArea.OnUnfocusedArea.AddListener(OnControllerAreaUnfocused);
		showAnimation.OnAnimationFinished.AddListener(delegate
		{
			if (controllerArea.IsFocused)
			{
				ControllerFocusFirstElement();
			}
		});
		if (_shopInventoryPanelHotkeyContainer != null)
		{
			_shopInventoryPanelHotkeyContainer.ToggleActiveAllHotkeys(value: false);
		}
		SetShowSwitchRightHotkey(shown: false);
		_controllerInputScroll = abilityCardsPanel.GetComponent<ControllerInputScroll>();
	}

	private void OnDestroy()
	{
		if (CoreApplication.IsQuitting)
		{
			return;
		}
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		TooltipsVisibilityHelper.Instance.RemoveTooltipRequest(this);
		levelUpButton.onClick.RemoveAllListeners();
		CancelAnimations();
	}

	public void Display(CMapCharacter characterData, Action<bool> onAbilityCardSelected, RectTransform sourceUI, bool isEditAllowed, bool animate = true, Action onLevelup = null, Action onClosed = null, bool enableLevelUp = false, bool isLevelUpValid = false)
	{
		verticalPointer.PointAt(sourceUI);
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.card_selection);
		this.characterData = characterData;
		this.onAbilityCardSelected = onAbilityCardSelected;
		onAbilityCardHovered = null;
		this.onLevelup = onLevelup;
		this.onClosed = onClosed;
		EnableLevelUp(enableLevelUp, isLevelUpValid);
		if (_panelHotkeySession == null)
		{
			_panelHotkeySession = _panelHotkeyContainer.GetSessionOrEmpty().GetHotkey(out _backHotkey, "Back").GetHotkey(out _selectHotkey, "Select")
				.GetHotkey(out _unselectHotkey, "Unselect")
				.GetHotkey(out _switchLeftHotkey, "Switch_Left");
			SetShowSwitchLeftHotkey(shown: false);
		}
		if (_hotkeySession == null)
		{
			_hotkeySession = hotkeyContainer.GetSessionOrEmpty().GetHotkey(out _tipsHotkey, "Tips", delegate
			{
				TooltipsVisibilityHelper.Instance.ToggleTooltips(this);
			}).GetHotkey(out _furtherAbilityCardHotkey, "FurtherAbilityCard");
		}
		if (InputManager.GamePadInUse)
		{
			NewPartyDisplayUI.PartyDisplay.ShopInventoryPanelHotkeyContainerProxy.SetCurrentHotkeyContainer(_shopInventoryPanelHotkeyContainer);
			_shopInventoryPanelHotkeyContainer.gameObject.SetActive(value: true);
		}
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		abilityCardsUI.Clear();
		foreach (CAbilityCard ownedAbilityCard in characterData.GetOwnedAbilityCards())
		{
			if (ownedAbilityCard.GetAbilityCardYML == null)
			{
				Debug.LogError("Ability card data is null! Check YML file for " + ownedAbilityCard.Name);
				continue;
			}
			try
			{
				AbilityCardUI component = ObjectPool.SpawnCard(ownedAbilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsPanel.content, resetLocalScale: true, resetToMiddle: true, resetLocalRotation: false, activate: false).GetComponent<AbilityCardUI>();
				component.Init(ownedAbilityCard, null, characterData.CharacterID, OnAbilityCardSelect, OnAbilityCardDeselect, OnUnableToSelectAbilityCard, OnAbilityCardHover, cardHighlightScale, cardHighlightDisplacement, fullCardSpacing);
				if (InputManager.GamePadInUse)
				{
					component.SetCanToggleFunctor(CanBeAbilityCardToggled);
				}
				component.fullAbilityCard.UpdateView(UISettings.DefaultFullCardViewSettings);
				abilityCardsUI.Add(component);
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to initialise ability card " + ownedAbilityCard.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		if (!IsOpen)
		{
			Show(!animate);
		}
		foreach (AbilityCardUI item2 in abilityCardsUI)
		{
			if (InputManager.GamePadInUse)
			{
				item2.SetSelectable(isOn: true);
			}
			else
			{
				item2.SetSelectable(FFSNetwork.IsOnline ? (characterData.IsUnderMyControl && isEditAllowed) : isEditAllowed);
			}
			item2.gameObject.SetActive(value: true);
		}
		UpdateDisplay();
		ShowIntroduction();
	}

	private bool CanBeAbilityCardToggled()
	{
		if (!FFSNetwork.IsOnline)
		{
			return true;
		}
		return characterData?.IsUnderMyControl ?? false;
	}

	public void SetSelectable(bool isEditAllowed)
	{
		if (!IsVisible)
		{
			return;
		}
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			item.SetSelectable(FFSNetwork.IsOnline ? (characterData.IsUnderMyControl && isEditAllowed) : isEditAllowed);
		}
	}

	private void ShowIntroduction()
	{
		if (!ShowIntro)
		{
			return;
		}
		bool isNotGuildmaster = AdventureState.MapState.DLLMode != ScenarioManager.EDLLMode.Guildmaster;
		introduction.Show(delegate
		{
			if (!AdventureState.MapState.IsCampaign && isNotGuildmaster)
			{
				ControllerInputAreaManager.Instance.UnfocusArea(EControllerInputAreaType.CharacterAbilityCards);
			}
		});
		AdventureState.MapState.MapParty.MarkIntroDone(EIntroductionConcept.AbilityCardPanel.ToString());
	}

	public void HideFullCards()
	{
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			item.ToggleFullCard(active: false);
		}
	}

	public void HideFullCardsPreview()
	{
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			item.ToggleFullCardPreview(isHighlighted: false);
		}
	}

	public void EnableLevelUp(bool enable, bool isValid = true)
	{
		isLevelUpEnabled = enable;
		isLevelUpValid = isValid;
		levelUpWarning.SetActive(value: false);
		levelUpButton.targetGraphic.material = ((!isValid) ? UIInfoTools.Instance.disabledGrayscaleMaterial : null);
		if (!defaultLevelColor.HasValue)
		{
			defaultLevelColor = levelUpButton.buttonText.color;
		}
		levelUpButton.buttonText.color = (isValid ? defaultLevelColor.Value : UIInfoTools.Instance.greyedOutTextColor);
		levelUpHighlight.SetActive(isValid);
		bool flag = levelUpHeader.gameObject.activeSelf != (onLevelup != null && enable);
		levelUpHeader.gameObject.SetActive(onLevelup != null && enable);
		if (controllerArea.IsFocused && flag)
		{
			EnableNavigation(!levelUpButton.gameObject.activeSelf && EventSystem.current.currentSelectedGameObject == levelUpButton.gameObject);
		}
	}

	public void DisplayCardSelection(CMapCharacter characterData, Action<AbilityCardUI, bool> onAbilityCardSelected, Action<bool, AbilityCardUI> onAbilityCardHover, RectTransform sourceUI, Func<AbilityCardUI, bool> filter, bool autoselectFirst)
	{
		onLevelup = null;
		EnableLevelUp(enable: false);
		verticalPointer.PointAt(sourceUI);
		this.characterData = characterData;
		this.onAbilityCardSelected = null;
		onAbilityCardHovered = onAbilityCardHover;
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		abilityCardsUI.Clear();
		AbilityCardUI selectedCard = null;
		foreach (CAbilityCard ownedAbilityCard in characterData.GetOwnedAbilityCards())
		{
			if (ownedAbilityCard.GetAbilityCardYML == null)
			{
				Debug.LogError("Ability card data is null! Check YML file for " + ownedAbilityCard.Name);
				continue;
			}
			try
			{
				AbilityCardUI component = ObjectPool.SpawnCard(ownedAbilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsPanel.content, resetLocalScale: false, resetToMiddle: true).GetComponent<AbilityCardUI>();
				component.transform.localScale = Vector3.one;
				component.Init(ownedAbilityCard, null, characterData.CharacterID, delegate(AbilityCardUI cardUI, bool networkAction)
				{
					if (selectedCard != cardUI)
					{
						onAbilityCardSelected(cardUI, networkAction);
						AbilityCardUI abilityCardUI = selectedCard;
						selectedCard = cardUI;
						abilityCardUI?.ToggleSelect(active: false, highlight: false, networkAction);
					}
				}, delegate(AbilityCardUI cardUI, bool networkAction)
				{
					if (selectedCard != null && selectedCard == cardUI)
					{
						cardUI.OnClick();
					}
				}, OnUnableToSelectAbilityCard, OnAbilityCardHover, cardHighlightScale, cardHighlightDisplacement, fullCardSpacing, disableFullCard: true);
				abilityCardsUI.Add(component);
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to initialise ability card " + ownedAbilityCard.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		foreach (AbilityCardUI item2 in abilityCardsUI)
		{
			bool flag = (filter != null && !filter(item2)) || !FFSNetwork.IsOnline || characterData.IsUnderMyControl;
			item2.SetSelectable(flag);
			if (!flag)
			{
				item2.SetType(CardPileType.Discarded);
				item2.UpdateCard();
			}
			else if (autoselectFirst)
			{
				autoselectFirst = false;
				item2.OnClick();
			}
		}
		Show();
		UpdateDisplay();
		abilityCardsPanel.verticalNormalizedPosition = 1f;
		if (autoselectFirst)
		{
			onAbilityCardSelected(null, arg2: true);
		}
	}

	public void HighlightCard(CAbilityCard abilityCard)
	{
		AbilityCardUI abilityCardUI = abilityCardsUI.FirstOrDefault((AbilityCardUI it) => it.AbilityCard == abilityCard);
		if (abilityCardUI == null)
		{
			Debug.LogError("Ability card UI is not int the card list" + abilityCard.Name);
			return;
		}
		abilityCardUI.gameObject.AddComponent<CardHighlight>();
		abilityCardsPanel.ScrollToFit(abilityCardUI.transform as RectTransform);
	}

	private void CancelAnimations()
	{
		if (animationDisplay != null)
		{
			LeanTween.cancel(animationDisplay.id);
		}
		animationDisplay = null;
		errorAnimator.Stop();
	}

	public void Show(bool instant = false, Action callback = null)
	{
		CancelAnimations();
		base.gameObject.SetActive(value: true);
		UILevelUpWindow instance = Singleton<UILevelUpWindow>.Instance;
		if ((object)instance != null && instance.IsShowing)
		{
			Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		}
		AudioControllerUtils.PlaySound(audioItemShow);
		if (instant)
		{
			showAnimation.GoToFinishState();
			if (controllerArea.IsFocused)
			{
				EnableNavigation();
			}
		}
		else
		{
			showAnimation.Play();
		}
		if (!controllerArea.IsEnabled || !controllerArea.IsFocused)
		{
			controllerArea.Enable();
		}
	}

	private void UpdateDisplay()
	{
		TextMeshProUGUI textMeshProUGUI = infoText;
		string text = (warningText.text = characterData.HandAbilityCardIDs.Count + " / " + characterData.MaxCards + " " + LocalizationManager.GetTranslation("GUI_EQUIPPED"));
		textMeshProUGUI.text = text;
		TextMeshProUGUI textMeshProUGUI2 = infoText;
		Color color = (infoIcon.color = ((characterData.HandAbilityCardIDs.Count == characterData.MaxCards) ? regularTitleColor : errorTitleColor));
		textMeshProUGUI2.color = color;
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			item.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
		}
	}

	private void OnUnableToSelectAbilityCard(AbilityCardUI cardUI)
	{
		TextMeshProUGUI textMeshProUGUI = infoText;
		Color color = (infoIcon.color = regularTitleColor);
		textMeshProUGUI.color = color;
		errorAnimator.Play();
		cardUI.MiniAbilityCard.ShowWarning(show: true);
	}

	private void OnAbilityCardSelect(AbilityCardUI cardUI, bool networkAction = true)
	{
		if (!characterData.HandAbilityCardIDs.Contains(cardUI.AbilityCard.ID))
		{
			characterData.HandAbilityCardIDs.Add(cardUI.AbilityCard.ID);
		}
		UpdateSelectHotkeys();
		UpdateDisplay();
		onAbilityCardSelected(obj: true);
		if (FFSNetwork.IsOnline && characterData.IsUnderMyControl && networkAction)
		{
			int controllableID = (AdventureState.MapState.IsCampaign ? characterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID));
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			IProtocolToken supplementaryDataToken = new CardInventoryToken(characterData.HandAbilityCardIDs, characterData.OwnedAbilityCardIDs);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyCardInventory, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		SimpleLog.AddToSimpleLog("MapCharacter: " + characterData.CharacterID + " selected card: " + cardUI.CardName);
	}

	private void OnAbilityCardDeselect(AbilityCardUI cardUI, bool networkAction = true)
	{
		if (characterData.HandAbilityCardIDs.Contains(cardUI.AbilityCard.ID))
		{
			TextMeshProUGUI textMeshProUGUI = infoText;
			Color color = (infoIcon.color = regularTitleColor);
			textMeshProUGUI.color = color;
			characterData.HandAbilityCardIDs.Remove(cardUI.AbilityCard.ID);
			errorAnimator.Stop();
			cardUI.MiniAbilityCard.ShowWarning(show: false);
		}
		UpdateSelectHotkeys();
		UpdateDisplay();
		onAbilityCardSelected(obj: false);
		if (FFSNetwork.IsOnline && characterData.IsUnderMyControl && networkAction)
		{
			int controllableID = (AdventureState.MapState.IsCampaign ? characterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID));
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			IProtocolToken supplementaryDataToken = new CardInventoryToken(characterData.HandAbilityCardIDs, characterData.OwnedAbilityCardIDs);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyCardInventory, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		SimpleLog.AddToSimpleLog("MapCharacter: " + characterData.CharacterID + " deselected card: " + cardUI.CardName);
	}

	private void OnAbilityCardHover(bool isActive, AbilityCardUI cardUI)
	{
		if (isActive)
		{
			hoveredCard = cardUI;
			if (InputManager.GamePadInUse)
			{
				abilityCardsPanel.ScrollToFit(cardUI.MiniAbilityCard.transform as RectTransform);
			}
			cardUI.fullAbilityCard.transform.position += cardUI.fullAbilityCard.GetComponent<RectTransform>().DeltaWorldPositionToFitRectTransform(UIManager.Instance.UICamera, fullCardAreaToFit);
		}
		else
		{
			hoveredCard = null;
		}
		UpdateSelectHotkeys();
		onAbilityCardHovered?.Invoke(isActive, cardUI);
	}

	public void Hide(bool playHideAudio = true)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		DisableNavigation();
		base.gameObject.SetActive(value: false);
		controllerArea.Destroy();
		CancelAnimations();
		if (playHideAudio)
		{
			AudioControllerUtils.PlaySound(audioItemHide);
		}
		introduction.Hide();
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		abilityCardsUI.Clear();
		if (InputManager.GamePadInUse)
		{
			NewPartyDisplayUI.PartyDisplay.ShopInventoryPanelHotkeyContainerProxy.ResetToDefaultContainer();
		}
		onClosed?.Invoke();
		_panelHotkeySession?.Dispose();
		_panelHotkeySession = null;
		_hotkeySession?.Dispose();
		_hotkeySession = null;
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			controllerArea.Destroy();
			CancelAnimations();
			if (InputManager.GamePadInUse)
			{
				NewPartyDisplayUI.PartyDisplay.ShopInventoryPanelHotkeyContainerProxy.ResetToDefaultContainer();
			}
		}
	}

	public bool Escape()
	{
		Hide();
		return true;
	}

	public void SetShowTipHotkey(bool shown)
	{
		if (_hotkeySession != null)
		{
			_tipsHotkey.TrySetShown(shown);
		}
	}

	public void SetShowSwitchLeftHotkey(bool shown)
	{
		if (!_switchLeftHotkey.Session.IsDispose)
		{
			_switchLeftHotkey.TrySetShown(shown);
		}
	}

	public void SetShowSwitchRightHotkey(bool shown)
	{
		_switchRightHotkey.TrySetEnabledHotkey(shown);
	}

	public void DisplaySelectHotkeys(bool value)
	{
		displaySelectHotkeys = value;
		UpdateSelectHotkeys();
	}

	public void SetDimmer(bool isDimmer)
	{
		_dimmer.SetActive(isDimmer);
	}

	public void UpdateSelectHotkeys()
	{
		if (!displaySelectHotkeys || hoveredCard == null)
		{
			if (_panelHotkeySession != null)
			{
				_selectHotkey.Hide();
				_unselectHotkey.Hide();
			}
		}
		else if (InputManager.GamePadInUse)
		{
			bool flag = !characterData.HandAbilityCardIDs.Contains(hoveredCard.AbilityCard.ID);
			bool flag2 = true;
			if (FFSNetwork.IsOnline)
			{
				flag2 = characterData?.IsUnderMyControl ?? false;
			}
			_selectHotkey.SetShown(flag2 && flag);
			_unselectHotkey.SetShown(flag2 && !flag);
		}
	}

	public void SetShowFurtherAbilityCardHotkey(bool shown)
	{
		if (_panelHotkeySession != null)
		{
			_furtherAbilityCardHotkey.SetShown(shown);
		}
	}

	public void SetActiveControllerInputScroll(bool active)
	{
		if (InputManager.GamePadInUse && _controllerInputScroll != null)
		{
			_controllerInputScroll.enabled = active;
		}
	}

	public void SetShowDarkenPanel(bool shown)
	{
		_darkenPanel.Toggle(shown);
		SetDimmer(shown);
	}

	public int Order()
	{
		if (!Singleton<UILevelUpWindow>.Instance.IsLevelingUp(characterData.CharacterID))
		{
			return 0;
		}
		return 1;
	}

	private void FocusOnControllerArea()
	{
		ControllerInputAreaManager.Instance.FocusArea(controllerArea);
	}

	private void OnControllerAreaFocused()
	{
		_isLevelUpWindowShowing = Singleton<UILevelUpWindow>.Instance.IsShowing;
		if (_isLevelUpWindowShowing)
		{
			Singleton<UILevelUpWindow>.Instance.Focus();
			return;
		}
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.CharacterAbilityCards);
		_backHotkey.Show();
		DisplaySelectHotkeys(value: true);
		if (!NewPartyDisplayUI.PartyDisplay.IsDisableFurtherAbilityPanel)
		{
			_furtherAbilityCardHotkey.Show();
		}
		if (selectElementCoroutine != null)
		{
			StopCoroutine(selectElementCoroutine);
			selectElementCoroutine = null;
		}
		EnableNavigation(autoselectFirst: false);
		if (!showAnimation.IsPlaying)
		{
			selectElementCoroutine = StartCoroutine(WaitControllerSelectFirst());
		}
		UIWindowManager.RegisterEscapable(this);
	}

	private void OnControllerAreaUnfocused()
	{
		if (_isLevelUpWindowShowing)
		{
			_isLevelUpWindowShowing = false;
			return;
		}
		hoveredCard = null;
		_backHotkey.Hide();
		_selectHotkey.Hide();
		_unselectHotkey.Hide();
		DisableNavigation();
		UIWindowManager.UnregisterEscapable(this);
	}

	public void CloseLevelUpWindow()
	{
		_isLevelUpWindowShowing = false;
		Singleton<UILevelUpWindow>.Instance.ForceClose();
	}

	private IEnumerator WaitControllerSelectFirst()
	{
		yield return null;
		if (!LevelMessageUILayoutGroup.IsShown)
		{
			ControllerFocusFirstElement();
		}
	}

	public void EnableNavigation(bool autoselectFirst = true)
	{
		if (InputManager.GamePadInUse)
		{
			if (autoselectFirst && !showAnimation.IsPlaying)
			{
				ControllerFocusFirstElement();
			}
			return;
		}
		for (int i = 0; i < abilityCardsUI.Count; i++)
		{
			if (i == 0)
			{
				abilityCardsUI[i].Selectable.SetNavigation(null, null, levelUpButton.gameObject.activeInHierarchy ? levelUpButton : null, abilityCardsUI[i + 1].Selectable);
			}
			else
			{
				abilityCardsUI[i].Selectable.SetNavigation(null, null, abilityCardsUI[i - 1].Selectable, (i == abilityCardsUI.Count - 1) ? null : abilityCardsUI[i + 1].Selectable);
			}
		}
		levelUpButton?.SetNavigation(Navigation.Mode.Vertical);
		if (autoselectFirst && !showAnimation.IsPlaying)
		{
			ControllerFocusFirstElement();
		}
	}

	private void DisableNavigation()
	{
		if (InputManager.GamePadInUse)
		{
			return;
		}
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			item.DisableNavigation();
		}
		levelUpButton?.DisableNavigation();
		if (selectElementCoroutine != null)
		{
			StopCoroutine(selectElementCoroutine);
			selectElementCoroutine = null;
		}
	}

	private void ControllerFocusFirstElement()
	{
		if (InputManager.GamePadInUse)
		{
			if (levelUpButton != null && levelUpButton.gameObject.activeInHierarchy)
			{
				Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot(_levelUpButtonRoot);
			}
			else
			{
				Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot(_cardsRoot);
			}
			return;
		}
		if (selectElementCoroutine != null)
		{
			StopCoroutine(selectElementCoroutine);
			selectElementCoroutine = null;
		}
		if (levelUpButton != null && levelUpButton.gameObject.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(levelUpButton.gameObject);
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(abilityCardsUI[0].gameObject);
		}
	}
}
