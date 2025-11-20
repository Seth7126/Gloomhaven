using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using Code.State;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPerksInventory : MonoBehaviour
{
	[Serializable]
	public class HoverPerkSlotEvent : UnityEvent<UIPerkInventorySlot>
	{
	}

	[Serializable]
	public class SelectPerkSlotEvent : UnityEvent<CharacterPerk>
	{
	}

	[SerializeField]
	private Transform content;

	[SerializeField]
	private UIPerkInventorySlot perkPrefab;

	[SerializeField]
	private TextMeshProUGUI pointsText;

	[SerializeField]
	private Color availablePerkPointsColor;

	[SerializeField]
	private Color unavailablePerkPointsColor;

	[SerializeField]
	private Image availablePointsMask;

	[SerializeField]
	private UINewNotificationTip availablePointsNotification;

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private string audioItemEnablePerk = "PlaySound_UIReceivedItem";

	[SerializeField]
	private PanelHotkeyContainer panelHotkeyContainer;

	[Header("Perk Checks")]
	[SerializeField]
	private RectTransform perksChecksContainer;

	[SerializeField]
	private List<Toggle> perksChecks;

	public HoverPerkSlotEvent OnHoverSlot = new HoverPerkSlotEvent();

	public HoverPerkSlotEvent OnUnhoverSlot = new HoverPerkSlotEvent();

	public SelectPerkSlotEvent OnEnabledPerk = new SelectPerkSlotEvent();

	private List<UIPerkInventorySlot> perksUI = new List<UIPerkInventorySlot>();

	private ICharacterService characterData;

	private UIPerkModifiersTooltip _lastPerkTooltip;

	private UiNavigationBlocker _levelMessageNavigationBlocker = new UiNavigationBlocker("LevelMessagePerksTab");

	private INavigationOperation _toPreviousNavigationOperation = new ToNonMenuPreviousStateWithFilterOperation(new StateFilterByTag<CampaignMapStateTag>(CampaignMapStateTag.Merchant, CampaignMapStateTag.Temple, CampaignMapStateTag.Perks, CampaignMapStateTag.PerksInventory));

	public List<UIPerkInventorySlot> PerksUI => perksUI;

	private void Awake()
	{
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
	}

	public void SubscribeEvents()
	{
		NewPartyDisplayUI.PartyDisplay.PartyStats.BeforeTooltipStateChanged += PartyDisplayBeforeTooltipStateChanged;
	}

	public void HidePerkTooltip()
	{
		foreach (UIPerkInventorySlot item in PerksUI)
		{
			if (item.Tooltip.TooltipShown)
			{
				item.Tooltip.HideTooltip();
			}
		}
	}

	private void PartyDisplayBeforeTooltipStateChanged(bool isShowing)
	{
		foreach (UIPerkInventorySlot item in PerksUI)
		{
			if (item.Tooltip.TooltipShown)
			{
				_lastPerkTooltip = item.Tooltip;
			}
		}
		if (isShowing)
		{
			if (_lastPerkTooltip != null)
			{
				_lastPerkTooltip.HideTooltip();
			}
		}
		else if (_lastPerkTooltip != null)
		{
			_lastPerkTooltip.OnPointerEnter(new PointerEventData(EventSystem.current));
		}
	}

	public void Display(ICharacterService characterData, RectTransform sourceUI, bool isInteractable)
	{
		scroll.verticalNormalizedPosition = 1f;
		verticalPointer.PointAt(sourceUI);
		ReturnSlotsToObjectPool();
		this.characterData = characterData;
		UpdateDisplay(isInteractable);
		CancelAnimations();
		showAnimation?.Play();
		if (panelHotkeyContainer != null)
		{
			panelHotkeyContainer.SetActiveHotkey("Select", value: false);
		}
	}

	public void Refresh(bool forceReturnToPool = false)
	{
		if (forceReturnToPool)
		{
			ReturnSlotsToObjectPool();
		}
		UpdateDisplay(isInteractable: true);
	}

	private void RefreshPerkPoints()
	{
		if (characterData.PerkPoints == 0)
		{
			pointsText.text = string.Format(LocalizationManager.GetTranslation("GUI_PERKS_POINTS"), $"<color=#{unavailablePerkPointsColor.ToHex()}>{characterData.PerkPoints}</color>");
			availablePointsMask.enabled = false;
			availablePointsNotification.Hide();
		}
		else
		{
			pointsText.text = string.Format(LocalizationManager.GetTranslation("GUI_PERKS_POINTS"), $"<color=#{availablePerkPointsColor.ToHex()}>{characterData.PerkPoints}</color>");
			availablePointsMask.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(characterData.CharacterModel, highlighted: true, characterData.Class.CustomCharacterConfig);
			availablePointsMask.enabled = true;
			availablePointsNotification.Show();
		}
	}

	private void UpdateDisplay(bool isInteractable)
	{
		RefreshPerkPoints();
		if (characterData.PerkChecksToComplete == 0)
		{
			perksChecksContainer.gameObject.SetActive(value: false);
		}
		else
		{
			HelperTools.NormalizePool(ref perksChecks, perksChecks[0].gameObject, perksChecksContainer, characterData.PerkChecksToComplete);
			for (int i = 0; i < characterData.PerkChecksToComplete; i++)
			{
				perksChecks[i].isOn = i < characterData.PerkChecks;
			}
			perksChecksContainer.gameObject.SetActive(value: true);
		}
		bool flag = false;
		foreach (IGrouping<string, CharacterPerk> item in from it in characterData.Perks
			group it by it.PerkID)
		{
			UIPerkInventorySlot component = ObjectPool.Spawn(perkPrefab.gameObject, content).GetComponent<UIPerkInventorySlot>();
			component.Init(characterData, item.ToList(), OnHoveredPerk, OnSelectedPerk, isInteractable);
			PerksUI.Add(component);
			if (controllerArea.IsFocused)
			{
				component.EnableNavigation();
				flag |= EventSystem.current.currentSelectedGameObject == component.gameObject;
			}
		}
	}

	private void OnHoveredPerk(bool hovered, UIPerkInventorySlot perkSlot)
	{
		if (hovered)
		{
			if (InputManager.GamePadInUse)
			{
				scroll.ScrollToFit(perkSlot.transform as RectTransform);
			}
			if (perkSlot.TotalCounters == perkSlot.ActiveCounters)
			{
				if (panelHotkeyContainer != null)
				{
					panelHotkeyContainer.SetActiveHotkey("Select", value: false);
				}
				return;
			}
			CharacterPerk characterPerk = characterData.Perks.FirstOrDefault((CharacterPerk it) => !it.IsActive && it.PerkID == perkSlot.Perk.PerkID);
			if (characterPerk == null || ValidatePreconditions(characterPerk))
			{
				perkSlot.ActiveBackground(characterData.PerkPoints != 0);
				if (panelHotkeyContainer != null)
				{
					panelHotkeyContainer.SetActiveHotkey("Select", characterData.PerkPoints != 0);
				}
				OnHoverSlot.Invoke(perkSlot);
			}
		}
		else
		{
			OnUnhoverSlot.Invoke(perkSlot);
			if (panelHotkeyContainer != null)
			{
				panelHotkeyContainer.SetActiveHotkey("Select", value: false);
			}
		}
	}

	private void OnSelectedPerk(UIPerkInventorySlot perkUi)
	{
		if (characterData.PerkPoints > 0)
		{
			CharacterPerk perk = characterData.Perks.FirstOrDefault((CharacterPerk it) => !it.IsActive && it.PerkID == perkUi.Perk.PerkID);
			if (perk == null)
			{
				Debug.LogError("Should not arrived until here since UIPerksInventorySlot is not selected if there is not an inactive perk available");
				return;
			}
			if (!ValidatePreconditions(perk))
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_PERK_MISSING_REMOVE_CONDITION"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_PERK_MISSING_REMOVE_CONDITION_WARNING"), "GUI_CANCEL", delegate
				{
					if (controllerArea.IsFocused)
					{
						CoroutineHelper.RunNextFrame(delegate
						{
							Singleton<UINavigation>.Instance.NavigationManager.TrySelect(perkUi.Selectable);
						});
					}
				}, _toPreviousNavigationOperation);
				return;
			}
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.generalAudioButtonProfile.mouseDownAudioItem);
			UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
			string title = string.Format(LocalizationManager.GetTranslation("GUI_CONFIRMATION_ENABLE_PERK"), LocalizationManager.GetTranslation(perk.Perk.Name), perkUi.ActiveCounters + 1, perkUi.TotalCounters);
			string translation = LocalizationManager.GetTranslation("GUI_CONFIRMATION_ENABLE_PERK_WARNING");
			UnityAction onActionConfirmed = delegate
			{
				EnabledPerk(perk, perkUi);
			};
			UnityAction onActionCancelled = delegate
			{
				if (controllerArea.IsFocused)
				{
					CoroutineHelper.RunNextFrame(delegate
					{
						Singleton<UINavigation>.Instance.NavigationManager.TrySelect(perkUi.Selectable);
					});
				}
			};
			INavigationOperation toPreviousNavigationOperation = _toPreviousNavigationOperation;
			instance.ShowGenericConfirmation(title, translation, onActionConfirmed, onActionCancelled, null, null, null, showHeader: true, enableSoftlockReport: false, toPreviousNavigationOperation);
		}
		else
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
		}
		perkUi.ActiveBackground(characterData.PerkPoints != 0);
		if (panelHotkeyContainer != null)
		{
			panelHotkeyContainer.SetActiveHotkey("Select", characterData.PerkPoints != 0);
		}
	}

	private bool ValidatePreconditions(CharacterPerk perk)
	{
		return perk.Perk.CardsToRemove.All((AttackModifierYMLData it) => characterData.AttackModifierDeck.Exists((AttackModifierYMLData modif) => modif.Name == it.Name));
	}

	private void EnabledPerk(CharacterPerk perk, UIPerkInventorySlot perkUi)
	{
		perk.IsActive = true;
		characterData.UpdatePerkPoints(Math.Max(0, characterData.PerkPoints - 1));
		OnEnabledPerk.Invoke(perk);
		RefreshPerkPoints();
		foreach (UIPerkInventorySlot item in PerksUI)
		{
			item.RefreshCounters();
		}
		if (controllerArea.IsFocused)
		{
			CoroutineHelper.RunNextFrame(delegate
			{
				Singleton<UINavigation>.Instance.NavigationManager.TrySelect(perkUi.Selectable);
			});
		}
		AudioControllerUtils.PlaySound(audioItemEnablePerk);
		SaveData.Instance.SaveCurrentAdventureData();
		if (FFSNetwork.IsOnline)
		{
			int controllableID = (AdventureState.MapState.IsCampaign ? characterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID));
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			IProtocolToken supplementaryDataToken = new PerkPointsToken(characterData.PerkPoints);
			IProtocolToken supplementaryDataToken2 = new PerksToken(characterData.Perks);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyPerks, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken, supplementaryDataToken2);
		}
		SimpleLog.AddToSimpleLog("MapCharacter: " + characterData.CharacterID + " enabled perk: " + perk.PerkID);
		perkUi.ActiveBackground(characterData.PerkPoints != 0 && perkUi.ActiveCounters != perkUi.TotalCounters);
		if (panelHotkeyContainer != null)
		{
			panelHotkeyContainer.SetActiveHotkey("Select", characterData.PerkPoints != 0 && perkUi.ActiveCounters != perkUi.TotalCounters);
		}
	}

	private void CancelAnimations()
	{
		showAnimation?.Stop();
	}

	private void OnDisable()
	{
		CancelAnimations();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			if (NewPartyDisplayUI.PartyDisplay != null)
			{
				NewPartyDisplayUI.PartyDisplay.PartyStats.BeforeTooltipStateChanged -= PartyDisplayBeforeTooltipStateChanged;
			}
			ReturnSlotsToObjectPool();
		}
	}

	private void ReturnSlotsToObjectPool()
	{
		foreach (UIPerkInventorySlot item in PerksUI)
		{
			if (!(item == null))
			{
				ObjectPool.Recycle(item.gameObject);
			}
		}
		PerksUI.Clear();
	}

	public void EnableNavigation()
	{
		Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_levelMessageNavigationBlocker);
		scroll.verticalScrollbar.interactable = true;
		for (int i = 0; i < PerksUI.Count; i++)
		{
			PerksUI[i].EnableNavigation();
		}
		if (panelHotkeyContainer != null)
		{
			panelHotkeyContainer.SetActive(value: true);
		}
	}

	public void DisableNavigation()
	{
		if (LevelMessageUILayoutGroup.IsShown)
		{
			Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(_levelMessageNavigationBlocker);
			scroll.verticalScrollbar.interactable = false;
		}
		for (int i = 0; i < PerksUI.Count; i++)
		{
			PerksUI[i].DisableNavigation();
		}
		if (panelHotkeyContainer != null)
		{
			panelHotkeyContainer.SetActive(value: false);
		}
	}
}
