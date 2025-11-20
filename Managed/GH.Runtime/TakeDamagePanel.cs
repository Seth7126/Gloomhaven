#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFSNet;
using GLOOM;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class TakeDamagePanel : Singleton<TakeDamagePanel>
{
	public class ActionButtonsState
	{
		public float Alpha;

		public bool TakeDamageButtonInteractable;

		public bool BurnAvailableCardsButtonInteractable;

		public bool BurnDiscardedCardsButtonInteractable;
	}

	private enum PreviewOptions
	{
		None,
		PreviewFullHand,
		PreviewAvailableCards,
		PreviewDiscardedCards,
		PreviewDamage
	}

	[SerializeField]
	private TMP_Text takeDamageText;

	[SerializeField]
	private TMP_Text damageAmount;

	[SerializeField]
	private Image damageObject;

	[SerializeField]
	private GameObject fatalDamageObject;

	[SerializeField]
	private TrackedToggle burnAvailableCardsToggle;

	[SerializeField]
	private TrackedToggle burnDiscardedCardsToggle;

	[SerializeField]
	private TrackedButton takeDamageButton;

	[SerializeField]
	private GameObject mandatoryTakeDamageHighlight;

	[SerializeField]
	private UIControllerKeyTip burnAvailableCardsControllerTip;

	[SerializeField]
	private UIControllerKeyTip burnDiscardedCardsControllerTip;

	[SerializeField]
	private UIControllerKeyTip takeDamageControllerTip;

	[SerializeField]
	private CanvasGroup burnAvailableCardsCanvasGroup;

	[SerializeField]
	private CanvasGroup burnDiscardedCardsCanvasGroup;

	[SerializeField]
	private Color shieldAppliedColor;

	[SerializeField]
	private CanvasGroup canvasGroupVisbility;

	[SerializeField]
	private ControllerInputArea m_ControllerInputArea;

	private Color defaultColor;

	private const float UnactiveButtonTransparency = 0.7f;

	private const float ActiveButtonTransparency = 1f;

	private UIWindow myWindow;

	private CActor actorBeingAttacked;

	private CPlayerActor actorToShowCardsFor;

	private CActor actorAttacking;

	private CAbility damageAbility;

	private WorldspacePanelUIController actorHUDController;

	private int damageToTake;

	private int pierce;

	private int addedShield;

	private PreviewOptions currentlyPreviewing;

	private Toggle currentlyToggled;

	private bool hasEnoughAvailableCards;

	private bool hasEnoughDiscardedCards;

	private bool isDirectDamage;

	private List<CItem> toggledShieldItems;

	private bool preventAllDamage;

	private List<CActiveBonus> toggledActiveBonuses;

	private bool m_TakeDamageInvoked;

	public ControllerInputArea InputArea => m_ControllerInputArea;

	public bool ThisPlayerHasTakeDamageControl
	{
		get
		{
			if (!FFSNetwork.IsOnline)
			{
				return true;
			}
			CActor cActor = ((actorToShowCardsFor != null) ? actorToShowCardsFor : actorBeingAttacked);
			bool result = false;
			if (!(cActor is CPlayerActor cPlayerActor))
			{
				if (!(cActor is CHeroSummonActor cHeroSummonActor))
				{
					if (cActor is CEnemyActor)
					{
						result = FFSNetwork.IsHost;
					}
				}
				else
				{
					result = cHeroSummonActor.Summoner.IsUnderMyControl;
				}
			}
			else
			{
				result = cPlayerActor.IsUnderMyControl;
			}
			return result;
		}
	}

	public bool IsOpen => myWindow.IsOpen;

	private bool IsLethalDamage
	{
		get
		{
			if (actorBeingAttacked.Health <= 0 && !preventAllDamage)
			{
				return actorBeingAttacked.Health + (damageToTake - CalculateCurrentDamage()) <= 0;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		defaultColor = damageAmount.color;
		myWindow = GetComponent<UIWindow>();
		myWindow.onHidden.AddListener(ClearButtonsSelected);
		toggledShieldItems = new List<CItem>();
		toggledActiveBonuses = new List<CActiveBonus>();
		m_ControllerInputArea.OnFocused.AddListener(delegate
		{
			ClearPreviouslyPreviewed();
			burnAvailableCardsToggle.group.SetAllTogglesOff();
		});
	}

	private void ClearButtonsSelected()
	{
		takeDamageButton.ClearSelectedState();
	}

	public void DisplayButtons(bool visibility = true)
	{
		burnAvailableCardsToggle.gameObject.SetActive(visibility);
		burnDiscardedCardsToggle.gameObject.SetActive(visibility);
		takeDamageButton.gameObject.SetActive(visibility);
	}

	public bool IsTakingDamage(CActor actor)
	{
		if (actorBeingAttacked == actor)
		{
			return IsOpen;
		}
		return false;
	}

	public void Show(CActor actorBeingAttacked, CPlayerActor actorToShowCardsFor, int damageToTake, int pierce, bool isDirectDamage, CAbility damageAbility)
	{
		try
		{
			this.actorBeingAttacked = actorBeingAttacked;
			this.actorToShowCardsFor = actorToShowCardsFor;
			actorAttacking = ((damageAbility != null) ? damageAbility.TargetingActor : null);
			this.damageAbility = damageAbility;
			this.damageToTake = damageToTake;
			this.pierce = pierce;
			this.isDirectDamage = isDirectDamage;
			addedShield = 0;
			damageAmount.color = defaultColor;
			damageObject.color = defaultColor;
			mandatoryTakeDamageHighlight.SetActive(value: false);
			canvasGroupVisbility.alpha = 1f;
			WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
			for (int i = 0; i < array?.Length; i++)
			{
				if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorBeingAttacked))
				{
					actorHUDController = array[i];
					break;
				}
			}
			damageAmount.text = damageToTake.ToString();
			UpdateTakeDamageOptionVisuals();
			UpdateCardRemovalOptionVisuals();
			ClearPreviouslyPreviewed();
			ClearTakeDamageInvoked();
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.DamageChoiceDialogAppeared));
			myWindow.Show();
			if (!isDirectDamage)
			{
				Singleton<UIUseItemsBar>.Instance.ShowItems(actorBeingAttacked, delegate(CItem item)
				{
					if (item.YMLData.Trigger.Equals(CItem.EItemTrigger.OnAttacked) && !actorBeingAttacked.Inventory.IsItemUsedOrActive(item))
					{
						if (item.YMLData.Data.CompareAbility == null)
						{
							return true;
						}
						if (damageAbility == null)
						{
							return false;
						}
						return item.YMLData.Data.CompareAbility.CompareAbility(damageAbility, actorBeingAttacked);
					}
					return false;
				}, ToggleShieldItem, clear: true);
				Singleton<UIActiveBonusBar>.Instance.ShowReduceDamageActiveBonuses(actorBeingAttacked, damageAbility, IsLethalDamage, ToggleActiveBonus);
			}
			else
			{
				Singleton<UIActiveBonusBar>.Instance.ShowReduceDamageActiveBonuses(actorBeingAttacked, CAbility.EAbilityType.Damage, IsLethalDamage, ToggleActiveBonus);
			}
			ClearSelectedToggle();
			ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.Damage);
			if (Singleton<StoryController>.Instance.DialogBox.IsStoryBoxFocused)
			{
				Singleton<StoryController>.Instance.DialogBox.AddFinishedCallback(StoryBoxFinished);
			}
			else
			{
				m_ControllerInputArea.Focus();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.Show().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00034", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void StoryBoxFinished()
	{
		m_ControllerInputArea.Focus();
	}

	public void UnToggleBurnAvailableCard()
	{
		if (burnAvailableCardsToggle.isOn)
		{
			burnAvailableCardsToggle.isOn = false;
		}
	}

	public void UnToggleBurnDiscardedCards()
	{
		if (burnDiscardedCardsToggle.isOn)
		{
			burnDiscardedCardsToggle.isOn = false;
		}
	}

	public void ClearSelectedToggle()
	{
		currentlyToggled = null;
		RefreshPreviewing();
		ShowDamageTooltip();
		Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: true);
		Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: true);
	}

	private void ShowDamageTooltip()
	{
		bool flag = ((CalculateCurrentDamage() > 0 && IsLethalDamage) ? (Singleton<UIActiveBonusBar>.Instance.GetNonSelectedActiveBonus().Count((CActiveBonus it) => !it.Ability.ActiveBonusData.ToggleIsOptional) > 0) : (Singleton<UIActiveBonusBar>.Instance.GetNonSelectedActiveBonus().Count((CActiveBonus it) => !it.Ability.ActiveBonusData.ToggleIsOptional && (!(it is CPreventDamageActiveBonus cPreventDamageActiveBonus) || !cPreventDamageActiveBonus.PreventOnlyIfLethal)) > 0));
		mandatoryTakeDamageHighlight.SetActive(flag);
		if (currentlyToggled == null && flag)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (CActiveBonus nonSelectedActiveBonu in Singleton<UIActiveBonusBar>.Instance.GetNonSelectedActiveBonus())
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendFormat("{0} ", LocalizationManager.GetTranslation("AND"));
				}
				stringBuilder.AppendFormat("<color=\"red\"><font=\"MarcellusSC-Regular SDF\">{0}</font></color> ", LocalizationNameConverter.MultiLookupLocalization(nonSelectedActiveBonu.BaseCard.Name, out var _));
			}
			stringBuilder.Append(LocalizationManager.GetTranslation("GUI_TOOLTIP_DEAL_DAMAGE_MANDATORY_USE"));
			Singleton<HelpBox>.Instance.ShowTranslated(stringBuilder.ToString());
		}
		else if ((damageAbility != null && damageAbility.AbilityType == CAbility.EAbilityType.Wound) || (damageAbility == null && GameState.CurrentDamageData.DamageSourceAbilityType == CAbility.EAbilityType.Wound))
		{
			Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_WOUNDED"), LocalizationManager.GetTranslation(actorBeingAttacked.ActorLocKey())));
		}
		else if (actorBeingAttacked is CHeroSummonActor cHeroSummonActor)
		{
			if (cHeroSummonActor.IsCompanionSummon)
			{
				InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_DEAL_DAMAGE_COMPANION"), LocalizationManager.GetTranslation(cHeroSummonActor.ActorLocKey()), LocalizationManager.GetTranslation(cHeroSummonActor.Summoner.ActorLocKey())), LocalizationManager.GetTranslation("GUI_TOOLTIP_TITLE_DEAL_DAMAGE"), HelpBox.FormatTarget.TITLE);
			}
			else
			{
				InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_DEAL_DAMAGE_SUMMON", "GUI_TOOLTIP_DEAL_DAMAGE_SUMMON_TITLE", null, HelpBox.FormatTarget.TITLE);
			}
		}
		else
		{
			InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_DEAL_DAMAGE", "GUI_TOOLTIP_TITLE_DEAL_DAMAGE", null, HelpBox.FormatTarget.TITLE);
		}
	}

	public void RefreshDamageTooltip()
	{
		if (myWindow.IsOpen)
		{
			ShowDamageTooltip();
		}
	}

	public void ClearPreviouslyPreviewed()
	{
		currentlyPreviewing = PreviewOptions.None;
	}

	public void ClearTakeDamageInvoked()
	{
		m_TakeDamageInvoked = false;
	}

	private void UpdateTakeDamageOptionVisuals()
	{
		try
		{
			if (!IsLethalDamage)
			{
				fatalDamageObject.SetActive(value: false);
				damageObject.gameObject.SetActive(value: true);
				TMP_Text tMP_Text = damageAmount;
				Color color = (takeDamageText.color = UIInfoTools.Instance.basicTextColor);
				tMP_Text.color = color;
			}
			else
			{
				damageObject.gameObject.SetActive(value: false);
				fatalDamageObject.SetActive(value: true);
				TMP_Text tMP_Text2 = damageAmount;
				Color color = (takeDamageText.color = UIInfoTools.Instance.negativeTextColor);
				tMP_Text2.color = color;
			}
			takeDamageButton.interactable = ThisPlayerHasTakeDamageControl;
			takeDamageControllerTip.ShowInteractable(takeDamageButton.interactable);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.UpdateTakeDamageOptionVisuals().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00035", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void UpdateCardRemovalOptionVisuals()
	{
		try
		{
			ResetToggles();
			bool thisPlayerHasTakeDamageControl = ThisPlayerHasTakeDamageControl;
			hasEnoughAvailableCards = actorToShowCardsFor != null && actorToShowCardsFor.CharacterClass.HandAbilityCards.Count > 0;
			burnAvailableCardsToggle.interactable = hasEnoughAvailableCards && thisPlayerHasTakeDamageControl;
			burnAvailableCardsCanvasGroup.alpha = (hasEnoughAvailableCards ? 1f : 0.7f);
			burnAvailableCardsControllerTip.ShowInteractable(burnAvailableCardsToggle.interactable);
			hasEnoughDiscardedCards = actorToShowCardsFor != null && actorToShowCardsFor.CharacterClass.DiscardedAbilityCards.Count > 1;
			burnDiscardedCardsToggle.interactable = hasEnoughDiscardedCards && thisPlayerHasTakeDamageControl;
			burnDiscardedCardsCanvasGroup.alpha = (hasEnoughDiscardedCards ? 1f : 0.7f);
			burnDiscardedCardsControllerTip.ShowInteractable(burnDiscardedCardsToggle.interactable);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.UpdateCardRemovalOptionVisuals().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00036", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ResetToggles()
	{
		burnAvailableCardsToggle.SetValue(value: false);
		burnDiscardedCardsToggle.SetValue(value: false);
		currentlyToggled = null;
		burnDiscardedCardsToggle.group.SetAllTogglesOff(sendCallback: false);
		Singleton<UIUseItemsBar>.Instance.Hide();
		Singleton<UIActiveBonusBar>.Instance.Hide();
		toggledShieldItems.Clear();
		toggledActiveBonuses.Clear();
		addedShield = 0;
		preventAllDamage = false;
	}

	public void OnMouseEnterTakeDamage()
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedButton(takeDamageButton) && myWindow.IsVisible)
		{
			Debug.Log("Mouse Enter for button " + takeDamageButton.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			PreviewDamage();
		}
	}

	public void OnMouseExitTakeDamage()
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedButton(takeDamageButton) && myWindow.IsVisible)
		{
			Debug.Log("Mouse Exit for button " + takeDamageButton.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			ResetPreviewing();
		}
	}

	public void OnMouseEnterBurnOne()
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedToggle(burnAvailableCardsToggle) && myWindow.IsVisible)
		{
			Debug.Log("Mouse Enter for button " + burnAvailableCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			PreviewAvailableCards();
		}
	}

	public void OnMouseExitBurnOne()
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedToggle(burnAvailableCardsToggle) && myWindow.IsVisible)
		{
			Debug.Log("Mouse Exit for button " + burnAvailableCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			ResetPreviewing();
		}
	}

	public void OnMouseEnterBurnTwo()
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedToggle(burnDiscardedCardsToggle) && myWindow.IsVisible)
		{
			Debug.Log("Mouse Enter for button " + burnDiscardedCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			PreviewDiscardedCards();
		}
	}

	public void OnMouseExitBurnTwo()
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedToggle(burnDiscardedCardsToggle) && myWindow.IsVisible)
		{
			Debug.Log("Mouse Exit for button " + burnDiscardedCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			ResetPreviewing();
		}
	}

	public void PreviewFullHand()
	{
		try
		{
			currentlyPreviewing = PreviewOptions.PreviewFullHand;
			CardsHandManager.Instance.EnableCancelActiveAbilities = false;
			CardsHandManager.Instance.Show(actorToShowCardsFor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Any, 0, fadeUnselectableCards: false, highlightSelectableCards: false, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: true);
			ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.CharacterActions);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.PreviewFullHand().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00037", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void PreviewAvailableCards()
	{
		try
		{
			if (hasEnoughAvailableCards)
			{
				currentlyPreviewing = PreviewOptions.PreviewAvailableCards;
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				CardsHandManager.Instance.Show(actorToShowCardsFor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Hand, 1, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: true);
				ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.CharacterActions);
				Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_BURN_1_ABILITY_CARD"), Singleton<InputManager>.Instance.GetGamepadActionIcon(KeyAction.UI_SUBMIT)), LocalizationManager.GetTranslation("GUI_TAKE_DAMAGE_BURN"));
				if (InputManager.GamePadInUse)
				{
					UiNavigationManager navigationManager = Singleton<UINavigation>.Instance.NavigationManager;
					navigationManager.SetCurrentRoot("CardHands");
					IUiNavigationSelectable uiNavigationSelectable = UiNavigationUtils.FindFirstSelectableFiltered(navigationManager.CurrentNavigationRoot.Elements, FilterFunction);
					navigationManager.TrySelect(uiNavigationSelectable);
				}
				Singleton<InputManager>.Instance.PlayerControl.MarkActionAsHandled(Singleton<InputManager>.Instance.PlayerControl.UICancel, "TakeDamagePanel");
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.ChooseCards);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.PreviewAvailableCards().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00038", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		static bool FilterFunction(IUiNavigationSelectable selectable)
		{
			if (selectable.GameObject.activeInHierarchy)
			{
				return selectable.ControlledSelectable.IsInteractable();
			}
			return false;
		}
	}

	public void PreviewDiscardedCards()
	{
		try
		{
			if (hasEnoughDiscardedCards)
			{
				currentlyPreviewing = PreviewOptions.PreviewDiscardedCards;
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				CardsHandManager.Instance.Show(actorToShowCardsFor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Discarded, 2, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: true);
				ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.CharacterActions);
				Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_BURN_2_DISCARDED_ABILITY_CARDS"), Singleton<InputManager>.Instance.GetGamepadActionIcon(KeyAction.UI_SUBMIT)), LocalizationManager.GetTranslation("GUI_TAKE_DAMAGE_DISCARD"));
				Singleton<InputManager>.Instance.PlayerControl.MarkActionAsHandled(Singleton<InputManager>.Instance.PlayerControl.UICancel, "TakeDamagePanel");
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.ChooseCards);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.PreviewDiscardedCards().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00039", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void PreviewDamage()
	{
		try
		{
			currentlyPreviewing = PreviewOptions.PreviewDamage;
			Singleton<HelpBox>.Instance.ClearOverrideController();
			HideCardPreview();
			RefreshDamageInformation();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.PreviewDamage().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00040", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private int CalculateCurrentHealth()
	{
		return Mathf.Min(actorBeingAttacked.Health + (preventAllDamage ? damageToTake : addedShield), actorBeingAttacked.MaxHealth);
	}

	private void RefreshDamageInformation(Action<int> damageUpdated = null)
	{
		int num = CalculateCurrentDamage();
		damageAmount.text = num.ToString();
		damageAmount.color = ((preventAllDamage || addedShield > 0) ? shieldAppliedColor : defaultColor);
		damageObject.color = ((preventAllDamage || addedShield > 0) ? shieldAppliedColor : defaultColor);
		takeDamageButton.interactable = ThisPlayerHasTakeDamageControl;
		takeDamageControllerTip.ShowInteractable(takeDamageButton.interactable);
		RefreshDamageTooltip();
		UpdateTakeDamageOptionVisuals();
		if (currentlyToggled == null || currentlyPreviewing == PreviewOptions.PreviewDamage)
		{
			actorHUDController?.PreviewSimpleDamage(num, CalculateCurrentHealth());
			if (num == 0)
			{
				Singleton<UIActiveBonusBar>.Instance.RefreshLethalReduceDamageActiveBonuses(actorBeingAttacked as CPlayerActor, IsLethalDamage);
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false, onlyChangeUnselected: true);
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false, toggledShieldItems);
			}
			else
			{
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: true);
				Singleton<UIActiveBonusBar>.Instance.RefreshLethalReduceDamageActiveBonuses(actorBeingAttacked as CPlayerActor, IsLethalDamage);
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: true);
			}
			damageUpdated?.Invoke(num);
		}
	}

	private int CalculateCurrentDamage()
	{
		if (!preventAllDamage)
		{
			return Mathf.Max(0, damageToTake - Mathf.Max(0, addedShield - pierce));
		}
		return 0;
	}

	public void ResetPreviewing()
	{
		try
		{
			currentlyPreviewing = PreviewOptions.None;
			if (burnAvailableCardsToggle.Equals(currentlyToggled))
			{
				BurnAvailableCard();
				return;
			}
			if (burnDiscardedCardsToggle.Equals(currentlyToggled))
			{
				BurnDiscardedCards();
				return;
			}
			if (myWindow.IsOpen)
			{
				actorHUDController?.PreviewSimpleDamage(CalculateCurrentDamage(), CalculateCurrentHealth());
			}
			HideCardPreview();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.ResetPreviewing().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00041", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RefreshDamagePreview()
	{
		if (myWindow.IsOpen)
		{
			if (currentlyToggled == null || currentlyPreviewing == PreviewOptions.PreviewDamage)
			{
				actorHUDController?.PreviewSimpleDamage(CalculateCurrentDamage(), CalculateCurrentHealth());
			}
			else
			{
				actorHUDController?.ResetDamagePreview(damageToTake);
			}
		}
	}

	private void RefreshPreviewing()
	{
		if (currentlyToggled == null)
		{
			actorHUDController?.PreviewSimpleDamage(CalculateCurrentDamage(), CalculateCurrentHealth());
		}
		else
		{
			actorHUDController?.ResetDamagePreview(damageToTake);
		}
		switch (currentlyPreviewing)
		{
		case PreviewOptions.None:
			HideCardPreview();
			break;
		case PreviewOptions.PreviewFullHand:
			PreviewFullHand();
			break;
		case PreviewOptions.PreviewAvailableCards:
			PreviewAvailableCards();
			break;
		case PreviewOptions.PreviewDiscardedCards:
			PreviewDiscardedCards();
			break;
		case PreviewOptions.PreviewDamage:
			PreviewDamage();
			break;
		default:
			Debug.LogError("Unknown preview option encountered for the Take Damage Panel.");
			break;
		}
	}

	public void BurnAvailableCard(Toggle toggle)
	{
		BurnAvailableCard(toggle.isOn);
	}

	public void BurnAvailableCard(bool toggledOn = true)
	{
		try
		{
			if (toggledOn)
			{
				UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.PlayerChoseBurnAvailable));
				OnSelectedToggle(burnAvailableCardsToggle);
				PreviewAvailableCards();
			}
			else if (currentlyToggled == burnAvailableCardsToggle)
			{
				ClearSelectedToggle();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.BurnAvailableCard().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00042", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void HideCardPreview()
	{
		Singleton<InputManager>.Instance.PlayerControl.MarkActionAsHandled(Singleton<InputManager>.Instance.PlayerControl.UICancel, "TakeDamagePanel");
		CardsHandManager.Instance.Hide();
	}

	public void TakeDamage()
	{
		try
		{
			burnDiscardedCardsToggle.group.SetAllTogglesOff();
			currentlyToggled = null;
			ClearPreviouslyPreviewed();
			if (!CanTakeDamage() || m_TakeDamageInvoked)
			{
				return;
			}
			m_TakeDamageInvoked = true;
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.PlayerChoseDamage));
			if (FFSNetwork.IsOnline && ThisPlayerHasTakeDamageControl)
			{
				IProtocolToken supplementaryDataToken = new ItemsToken(toggledShieldItems);
				IProtocolToken supplementaryDataToken2 = new ActiveBonusesToken(toggledActiveBonuses);
				Synchronizer.SendGameAction(GameActionType.TakeDamage, ActionPhaseType.TakeDamageConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken, supplementaryDataToken2);
			}
			actorBeingAttacked.Inventory.LockInSelectedItemsAndResetUnselected();
			if (toggledShieldItems.Count > 0)
			{
				foreach (CItem toggledShieldItem in toggledShieldItems)
				{
					if (toggledShieldItem.YMLData.Data.Abilities != null && toggledShieldItem.YMLData.Data.Abilities.Count > 0)
					{
						foreach (CAbility ability in toggledShieldItem.YMLData.Data.Abilities)
						{
							if (!(ability is CAbilityTargeting cAbilityTargeting))
							{
								continue;
							}
							cAbilityTargeting.Start(actorBeingAttacked, actorBeingAttacked);
							cAbilityTargeting.ActorIsApplying(actorBeingAttacked, new List<CActor> { actorBeingAttacked });
							cAbilityTargeting.ApplyToActor(actorBeingAttacked);
							CBaseCard baseCard = actorBeingAttacked.FindCardWithAbility(cAbilityTargeting);
							if (baseCard == null || !((actorToShowCardsFor?.CharacterClass.ActivatedCards.FindAll((CBaseCard x) => x is CItem)).SingleOrDefault((CBaseCard s) => s.ID.Equals(baseCard.ID)) is CItem cItem))
							{
								continue;
							}
							List<CActiveBonus> activeBonuses = cItem.ActiveBonuses;
							if (activeBonuses == null || activeBonuses.Count <= 0)
							{
								continue;
							}
							foreach (CActiveBonus activeBonuse in cItem.ActiveBonuses)
							{
								if (activeBonuse.Ability != null && activeBonuse.Ability.Equals(cAbilityTargeting))
								{
									activeBonuse.BespokeBehaviour?.OnBehaviourTriggered();
									activeBonuse.UpdateXPTracker();
								}
							}
						}
					}
					if (toggledShieldItem.YMLData.Data.RetaliateValue > 0 && actorAttacking != null)
					{
						CAbilityRetaliate.SingleShotRetaliate(toggledShieldItem.YMLData.Data.RetaliateValue, toggledShieldItem.YMLData.Data.RetaliateRange, actorAttacking, actorBeingAttacked, damageAbility);
					}
					if (toggledShieldItem.YMLData.Data.ShieldValue > 0)
					{
						CPreventDamageTriggered_MessageData message = new CPreventDamageTriggered_MessageData("", actorBeingAttacked)
						{
							m_PreventDamageBaseCard = toggledShieldItem
						};
						ScenarioRuleClient.MessageHandler(message);
					}
					actorBeingAttacked.Inventory.HandleUsedItems();
				}
			}
			int num = damageToTake - Mathf.Max(0, addedShield - pierce);
			int num2 = (preventAllDamage ? damageToTake : Mathf.Max(0, Mathf.Min(damageToTake, addedShield - pierce)));
			if (actorBeingAttacked.m_OnTakenDamageListeners != null && !preventAllDamage && damageToTake > 0)
			{
				actorBeingAttacked.m_OnTakenDamageListeners?.Invoke(damageToTake, damageAbility, num2, num);
			}
			GameState.PlayerNotAvoidingDamage(num2, toggledActiveBonuses, num);
			if (actorToShowCardsFor != null && ThisPlayerHasTakeDamageControl)
			{
				CardsHandManager.Instance.Hide(actorToShowCardsFor);
			}
			ResetAndHide(stopActiveBonus: false, switchBackToSavedState: true, Lockin: false);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.TakeDamage().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00043", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private bool CanTakeDamage()
	{
		if (CalculateCurrentDamage() == 0)
		{
			return true;
		}
		List<CActiveBonus> list = Singleton<UIActiveBonusBar>.Instance.ShowingActiveBonuses.Where((CActiveBonus a) => !a.Ability.ActiveBonusData.ToggleIsOptional).ToList();
		if (list.Count == 0)
		{
			return true;
		}
		if (actorBeingAttacked.Health + addedShield > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				CActiveBonus cActiveBonus = list[num];
				if (cActiveBonus is CPreventDamageActiveBonus { PreventOnlyIfLethal: not false })
				{
					list.Remove(cActiveBonus);
				}
			}
		}
		if (preventAllDamage || (toggledActiveBonuses.Count == list.Count && !list.Except(toggledActiveBonuses).Any()))
		{
			return true;
		}
		Singleton<HelpBox>.Instance.HighlightWarning();
		Singleton<UIActiveBonusBar>.Instance.HighlightMandatoryActiveBonus();
		return false;
	}

	public void ToggleShieldItem(CItem item, bool networkActionIfOnline = true)
	{
		try
		{
			if (!toggledShieldItems.Contains(item))
			{
				if (item.YMLData.Data.ShieldValue > 0 || item.YMLData.Data.RetaliateValue > 0)
				{
					toggledShieldItems.Add(item);
					actorBeingAttacked.Inventory.SelectItem(item);
				}
			}
			else
			{
				DeselectShieldItem(item);
			}
			CalculateAddedShield();
			RefreshDamageInformation(InputManager.GamePadInUse ? new Action<int>(GamepadDamageUpdated) : null);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.ToggleShieldItem().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00044", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void GamepadDamageUpdated(int damage)
	{
		if (damage == 0 || (Singleton<UIActiveBonusBar>.Instance.ShowingActiveBonuses.Count == toggledActiveBonuses.Count && Singleton<UIUseItemsBar>.Instance.ItemSlots.Count == toggledShieldItems.Count))
		{
			CoroutineHelper.RunNextFrame(delegate
			{
				Singleton<UIWindowManager>.Instance.Escape();
			});
		}
	}

	private void DeselectAllShieldItems(bool stopActiveBonus = false)
	{
		if (stopActiveBonus)
		{
			if (Singleton<UIActiveBonusBar>.Instance.IsShowing)
			{
				Singleton<UIActiveBonusBar>.Instance.UndoSelection();
			}
			toggledActiveBonuses.Clear();
			preventAllDamage = false;
		}
		if (toggledShieldItems.Count > 0)
		{
			for (int num = toggledShieldItems.Count - 1; num >= 0; num--)
			{
				CItem item = toggledShieldItems[num];
				DeselectShieldItem(item);
			}
		}
		CalculateAddedShield();
	}

	private void DeselectShieldItem(CItem item)
	{
		if (item != null && (item.YMLData.Data.ShieldValue > 0 || item.YMLData.Data.RetaliateValue > 0))
		{
			toggledShieldItems.Remove(item);
			actorBeingAttacked.Inventory.DeselectItem(item);
		}
	}

	public void ToggleActiveBonus(CActiveBonus activeBonus, CActor actor)
	{
		try
		{
			if (toggledActiveBonuses.Contains(activeBonus))
			{
				toggledActiveBonuses.Remove(activeBonus);
				if (activeBonus is CPreventDamageActiveBonus)
				{
					preventAllDamage = false;
				}
			}
			else
			{
				toggledActiveBonuses.Add(activeBonus);
				if (activeBonus is CPreventDamageActiveBonus)
				{
					preventAllDamage = true;
				}
			}
			CalculateAddedShield();
			RefreshDamageInformation(delegate(int dmg)
			{
				if (InputManager.GamePadInUse && (dmg == 0 || (Singleton<UIActiveBonusBar>.Instance.ShowingActiveBonuses.Count == toggledActiveBonuses.Count && Singleton<UIUseItemsBar>.Instance.ItemSlots.Count == toggledShieldItems.Count)))
				{
					CoroutineHelper.RunNextFrame(delegate
					{
						Singleton<UIWindowManager>.Instance.Escape();
					});
				}
			});
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.TogglePreventDamageActiveBonus().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00045", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void CalculateAddedShield()
	{
		addedShield = 0;
		foreach (CActiveBonus toggledActiveBonuse in toggledActiveBonuses)
		{
			if (toggledActiveBonuse is CShieldActiveBonus)
			{
				addedShield += toggledActiveBonuse.ReferenceStrength(damageAbility, actorBeingAttacked);
			}
		}
		foreach (CItem toggledShieldItem in toggledShieldItems)
		{
			if (toggledShieldItem.YMLData.Data.ShieldValue > 0)
			{
				addedShield += toggledShieldItem.YMLData.Data.ShieldValue;
			}
		}
	}

	private void OnSelectedToggle(TrackedToggle toggle)
	{
		currentlyToggled = toggle;
		DeselectAllShieldItems(stopActiveBonus: true);
		RefreshDamageInformation();
		Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
		actorHUDController?.ResetDamagePreview(damageToTake);
		Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false, onlyChangeUnselected: true);
		Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false);
	}

	public void BurnDiscardedCards(Toggle toggle)
	{
		BurnDiscardedCards(toggle.isOn);
	}

	public void BurnDiscardedCards(bool toggledOn = true)
	{
		try
		{
			if (toggledOn)
			{
				UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.PlayerChoseBurnDiscarded));
				OnSelectedToggle(burnDiscardedCardsToggle);
				PreviewDiscardedCards();
			}
			else if (currentlyToggled == burnDiscardedCardsToggle)
			{
				ClearSelectedToggle();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the TakeDamagePanel.BurnDiscardedCards().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00046", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ResetAndHide(bool stopActiveBonus = false, bool switchBackToSavedState = false, bool Lockin = true)
	{
		Singleton<HelpBox>.Instance.Hide();
		DeselectAllShieldItems(stopActiveBonus);
		ResetToggles();
		myWindow.Hide();
		if (Lockin)
		{
			actorBeingAttacked?.Inventory.LockInSelectedItemsAndResetUnselected();
		}
		if (FFSNetwork.IsOnline && ActionProcessor.HasSavedState && switchBackToSavedState)
		{
			ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
		}
		ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.WorldMap);
		m_ControllerInputArea.Unfocus();
		actorBeingAttacked = null;
	}

	public void ToggleVisibility(bool visible)
	{
		if (myWindow.IsOpen)
		{
			if (visible)
			{
				canvasGroupVisbility.alpha = 1f;
				ShowDamageTooltip();
			}
			else
			{
				canvasGroupVisbility.alpha = 0f;
				Singleton<HelpBox>.Instance.Hide();
			}
		}
	}

	public ActionButtonsState SetDisableVisualState()
	{
		ActionButtonsState result = new ActionButtonsState
		{
			Alpha = canvasGroupVisbility.alpha,
			TakeDamageButtonInteractable = takeDamageButton.interactable,
			BurnAvailableCardsButtonInteractable = burnAvailableCardsToggle.interactable,
			BurnDiscardedCardsButtonInteractable = burnDiscardedCardsToggle.interactable
		};
		canvasGroupVisbility.alpha = 0f;
		takeDamageButton.interactable = false;
		burnAvailableCardsToggle.interactable = false;
		burnDiscardedCardsToggle.interactable = false;
		return result;
	}

	public void SetActionButtonsState(ActionButtonsState state)
	{
		canvasGroupVisbility.alpha = state.Alpha;
		takeDamageButton.interactable = state.TakeDamageButtonInteractable;
		burnAvailableCardsToggle.interactable = state.BurnAvailableCardsButtonInteractable;
		burnDiscardedCardsToggle.interactable = state.BurnDiscardedCardsButtonInteractable;
	}

	public void ShowOtherPlayer(CActor actorBeingAttacked, CPlayerActor actorToShowCardsFor, int damageToTake, int pierce, bool isDirectDamage, CAbility damageAbility)
	{
		this.actorBeingAttacked = actorBeingAttacked;
		this.actorToShowCardsFor = actorToShowCardsFor;
		actorAttacking = damageAbility?.TargetingActor;
		this.damageAbility = damageAbility;
		this.damageToTake = damageToTake;
		this.pierce = pierce;
		this.isDirectDamage = isDirectDamage;
		addedShield = 0;
		preventAllDamage = false;
		WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
		for (int i = 0; i < array?.Length; i++)
		{
			if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorBeingAttacked))
			{
				actorHUDController = array[i];
				break;
			}
		}
		ResetToggles();
		if (actorToShowCardsFor == null)
		{
			actorToShowCardsFor = ((!(actorBeingAttacked is CHeroSummonActor cHeroSummonActor)) ? ScenarioManager.Scenario.PlayerActors[0] : cHeroSummonActor.Summoner);
		}
		int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? actorToShowCardsFor.CharacterName.GetHashCode() : actorToShowCardsFor.CharacterClass.ModelInstanceID);
		InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_DEAL_DAMAGE_MP"), ControllableRegistry.GetController(controllableID).UserNameWithPlatformIcon()), LocalizationManager.GetTranslation("GUI_TOOLTIP_TITLE_DEAL_DAMAGE"), HelpBox.FormatTarget.TITLE);
		if (Choreographer.s_Choreographer.FindClientActorGameObject(actorBeingAttacked) != null)
		{
			ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(actorBeingAttacked)).m_WorldspacePanelUI.PreviewSimpleDamage(CalculateCurrentDamage());
		}
		myWindow.Hide(instant: true);
	}

	public void ProxyTakeDamage(GameAction gameAction)
	{
		List<CItem> allItems = actorBeingAttacked.Inventory.AllItems;
		uint[] activatedItemNetworkIDs = ((ItemsToken)gameAction.SupplementaryDataToken).ItemNetworkIDs;
		int i;
		for (i = 0; i < activatedItemNetworkIDs.Length; i++)
		{
			CItem cItem = allItems.FirstOrDefault((CItem x) => x.NetworkID == activatedItemNetworkIDs[i]);
			if (cItem != null)
			{
				ToggleShieldItem(cItem);
				continue;
			}
			throw new Exception("Error applying shield item to mitigate damage. Item returns null (NetworkID: " + activatedItemNetworkIDs[i] + ").");
		}
		CAbility.EAbilityType abilityType = ((damageAbility != null) ? damageAbility.AbilityType : CAbility.EAbilityType.None);
		List<CActiveBonus> preventDamageActiveBonuses = Singleton<UIActiveBonusBar>.Instance.GetPreventDamageActiveBonuses(actorBeingAttacked, abilityType, IsLethalDamage);
		preventDamageActiveBonuses.AddRange(Singleton<UIActiveBonusBar>.Instance.GetShieldIncomingAttackActiveBonuses(actorBeingAttacked, abilityType));
		ActiveBonusesToken.ActiveBonusData[] appliedActiveBonusesData = ((ActiveBonusesToken)gameAction.SupplementaryDataToken2).ActiveBonuses;
		int i2;
		for (i2 = 0; i2 < appliedActiveBonusesData.Length; i2++)
		{
			CActiveBonus cActiveBonus = preventDamageActiveBonuses.FirstOrDefault((CActiveBonus x) => x.Ability.Name == appliedActiveBonusesData[i2].AbilityName && x.BaseCard.ID == appliedActiveBonusesData[i2].BaseCardID);
			if (cActiveBonus != null)
			{
				ToggleActiveBonus(cActiveBonus, actorBeingAttacked);
				ScenarioRuleClient.ToggleActiveBonus(cActiveBonus, actorBeingAttacked, null, 0, null, processImmediately: true);
				CalculateAddedShield();
				RefreshDamageInformation();
				continue;
			}
			throw new Exception("Error applying active bonus to mitigate damage. Active bonus returns null (AbilityName: " + appliedActiveBonusesData[i2].AbilityName + ", BaseCardID: " + appliedActiveBonusesData[i2].BaseCardID + ").");
		}
		Singleton<TakeDamagePanel>.Instance.ClearTakeDamageInvoked();
		TakeDamage();
	}

	public void ProxyBurnAvailableCard(GameAction action)
	{
		CardsHandUI hand = CardsHandManager.Instance.GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyBurnOneAvailableCard(((CardsToken)action.SupplementaryDataToken).CardIDs[0]);
			ResetAndHide(stopActiveBonus: false, switchBackToSavedState: true);
			return;
		}
		throw new Exception("Error burning an available card to avoid damage. Card Hand returns null for the actor (ID: " + action.ActorID + ").");
	}

	public void ProxyBurnDiscardedCards(GameAction action)
	{
		CardsHandUI hand = CardsHandManager.Instance.GetHand(action.ActorID);
		if (hand != null)
		{
			CardsToken cardsToken = (CardsToken)action.SupplementaryDataToken;
			hand.ProxyBurnTwoDiscardedCards(cardsToken.CardIDs[0], cardsToken.CardIDs[1]);
			ResetAndHide(stopActiveBonus: false, switchBackToSavedState: true);
			return;
		}
		throw new Exception("Error burning an available card to avoid damage. Card Hand returns null for the actor (ID: " + action.ActorID + ").");
	}
}
