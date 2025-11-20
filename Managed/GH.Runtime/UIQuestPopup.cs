using System.Collections.Generic;
using System.Linq;
using Assets.Script.GUI.Quest;
using GLOOM;
using JetBrains.Annotations;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.PartyDisplay;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIQuestPopup : MonoBehaviour
{
	private const string TravelLocalizationKey = "Consoles/GUI_HOTKEY_CONFIRM_TRAVEL";

	[SerializeField]
	private CanvasGroup navigationKeyCanvasGroup;

	[SerializeField]
	private UIQuestDescription information;

	[SerializeField]
	private Hotkey hotkey;

	[SerializeField]
	private ControllerInputAreaLocal controllerInputAreaLocal;

	[SerializeField]
	private DimmerUIElements _dimmerUIElements;

	[Header("Enemies")]
	[SerializeField]
	private RectTransform enemiesSection;

	[SerializeField]
	private ExtendedScrollRect enemiesScroll;

	[SerializeField]
	private UIQuestEnemy enemyPrefab;

	[SerializeField]
	private List<UIQuestEnemy> enemiesUI;

	[SerializeField]
	private List<GameObject> masksToCompleteRow;

	[SerializeField]
	private UIQuestEnemyStatsPopup enemyStatsTooltip;

	[Header("Rewards")]
	[SerializeField]
	private RectTransform rewardsSection;

	[SerializeField]
	private ExtendedScrollRect rewardsScroll;

	[SerializeField]
	private UIQuestReward rewardPrefab;

	[SerializeField]
	private List<UIQuestReward> rewardsUI;

	[SerializeField]
	private UIItemLocalTooltip itemTooltip;

	[SerializeField]
	private TextLocalizedListener completedRewardsText;

	[Header("Treasures")]
	[SerializeField]
	private GameObject treasuresSection;

	[SerializeField]
	private UIQuestReward treasureReward;

	[SerializeField]
	private Sprite treasureIcon;

	private UIWindow window;

	private IQuest quest;

	private bool isUnlocked;

	private Component currentElementFocused;

	private bool _isDarken;

	private readonly SimpleKeyActionHandlerBlocker _requireNavigationHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);

	private readonly SimpleKeyActionHandlerBlocker _focusHandlerBlocker = new SimpleKeyActionHandlerBlocker();

	private readonly SimpleKeyActionHandlerBlocker _canSwitchBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		if (InputManager.GamePadInUse)
		{
			_dimmerUIElements.Initialize();
		}
		if (InputManager.GamePadInUse)
		{
			hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		else
		{
			hotkey.gameObject.SetActive(value: false);
		}
		SubscribeOnInputAreaEvents();
		SubscribeOnInputHandlers();
		window.onShown.AddListener(OnWindowShown);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_R_RIGHT, OnRStickRightMoved);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_R_LEFT, OnRStickLeftMoved);
		}
		if (InputManager.GamePadInUse)
		{
			hotkey.Deinitialize();
		}
	}

	private void OnWindowShown()
	{
		if (InputManager.GamePadInUse)
		{
			_dimmerUIElements.DarkenElements();
			if (FFSNetwork.IsHost || FFSNetwork.IsClient)
			{
				string translation = LocalizationManager.GetTranslation("Consoles/GUI_HOTKEY_CONFIRM_TRAVEL");
				Singleton<UIReadyToggle>.Instance.InitializeLabelGamepad(translation);
			}
		}
	}

	public void ShowQuest(IQuest quest, bool autoFocus = false)
	{
		SetQuest(quest);
		Show(autoFocus);
	}

	public void SetQuest(IQuest quest, bool forceRefresh = false)
	{
		if (forceRefresh || !object.Equals(this.quest, quest))
		{
			this.quest = quest;
			information.Setup(quest);
			if (_isDarken)
			{
				UpdateInformationText();
			}
			SetupRewards(quest.Rewards, quest.IsSoloQuest ? "GUI_SOLO_QUEST_COMPLETED_UNAVAILABLE_REWARDS" : "GUI_QUEST_COMPLETED_UNAVAILABLE_REWARDS");
			SetupEnemies(quest.Enemies);
			SetupTreasures(quest.Treasures, quest.TreasuresTextLoc);
			if (_dimmerUIElements != null)
			{
				_dimmerUIElements.Initialize();
			}
		}
		rewardsScroll.ScrollToTop();
		enemiesScroll.ScrollToTop();
		RefreshLocked();
	}

	public void Show(bool autoFocus = false)
	{
		enemyStatsTooltip.Hide();
		itemTooltip.Hide();
		window.ShowOrUpdateStartingState();
		if (autoFocus)
		{
			Focus();
		}
	}

	public void Focus()
	{
		if (Singleton<UINavigation>.Instance.StateMachine.CurrentState is CharacterAbilityCardsState && Singleton<UINavigation>.Instance.NavigationManager.CurrentlySelectedElement.Parent.NavigationTags.Contains("AbilityCardMainButton"))
		{
			NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.HideFullCardsPreview();
		}
		controllerInputAreaLocal.Enable();
	}

	public void Unfocus()
	{
		controllerInputAreaLocal.Destroy();
	}

	private void SubscribeOnInputAreaEvents()
	{
		if (controllerInputAreaLocal != null)
		{
			controllerInputAreaLocal.OnFocusedArea.AddListener(OnInputAreaFocused);
			controllerInputAreaLocal.OnUnfocusedArea.AddListener(OnInputAreaUnfocused);
		}
	}

	public void SetSwitchNavigation(bool canSwitch)
	{
		_requireNavigationHandlerBlocker.SetBlock(!canSwitch);
	}

	public void SetFocusHandlerBlocker(bool block)
	{
		_focusHandlerBlocker.SetBlock(block);
	}

	private void SetHotKeyNavigationVisibility(bool visibility)
	{
		navigationKeyCanvasGroup.alpha = (visibility ? 1 : 0);
	}

	private void OnInputAreaFocused()
	{
		SetFocusHandlerBlocker(block: true);
		SetFirstElementFocused();
		LightenElements();
		if (InputManager.GamePadInUse && Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<CharacterAbilityCardsState>())
		{
			NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchLeftHotkey(shown: true);
			NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetDimmer(isDimmer: true);
			NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: false);
		}
		_canSwitchBlocker.SetBlock(value: false);
	}

	private void OnInputAreaUnfocused()
	{
		SetEmptyFocusedElement();
		SetFocusHandlerBlocker(block: false);
		DarkenElements();
		if (InputManager.GamePadInUse && Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<CharacterAbilityCardsState>())
		{
			NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchLeftHotkey(shown: false);
			NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetDimmer(isDimmer: false);
			NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: true);
		}
		enemyStatsTooltip.Hide();
		_canSwitchBlocker.SetBlock(value: true);
	}

	private void LightenElements()
	{
		if (InputManager.GamePadInUse)
		{
			_isDarken = false;
			UpdateInformationText();
			_dimmerUIElements.LightenToStandard();
		}
	}

	private void DarkenElements()
	{
		if (InputManager.GamePadInUse && !SceneController.Instance.IsLoading)
		{
			_isDarken = true;
			UpdateInformationText();
			_dimmerUIElements.DarkenElements();
		}
	}

	private void UpdateInformationText()
	{
		information.UpdateText(_isDarken);
	}

	private void SubscribeOnInputHandlers()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_R_RIGHT, OnRStickRightMoved, delegate
		{
			SetHotKeyNavigationVisibility(visibility: true);
		}, delegate
		{
			SetHotKeyNavigationVisibility(visibility: false);
		}).AddBlocker(new UIWindowOpenKeyActionBlocker(window)).AddBlocker(_requireNavigationHandlerBlocker).AddBlocker(_focusHandlerBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_R_LEFT, OnRStickLeftMoved).AddBlocker(new UIWindowOpenKeyActionBlocker(window)).AddBlocker(_canSwitchBlocker));
	}

	private void OnRStickRightMoved()
	{
		Focus();
	}

	private void OnRStickLeftMoved()
	{
		if (NewPartyDisplayUI.PartyDisplay.BattleGoalWindow.IsOpen || NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.IsOpen)
		{
			Unfocus();
		}
	}

	private void SetFirstElementFocused()
	{
		currentElementFocused = null;
		UIQuestEnemy elementFocused = enemiesUI.First();
		SetElementFocused(elementFocused);
	}

	private void SetEmptyFocusedElement()
	{
		SetElementFocused(null);
	}

	public void RefreshLocked()
	{
		IRequirementCheckResult requirementCheckResult = quest.CheckRequirements();
		isUnlocked = requirementCheckResult.IsUnlocked() || Singleton<MapChoreographer>.Instance.ShowAllScenariosMode;
		information.SetRequirements(requirementCheckResult);
		if (enemiesSection.gameObject.activeSelf && enemiesUI != null)
		{
			for (int i = 0; i < enemiesUI.Count && enemiesUI[i].gameObject.activeSelf; i++)
			{
				enemiesUI[i].ShowUnlocked(isUnlocked);
			}
			enemyStatsTooltip.SetFocused(isUnlocked);
		}
		if (rewardsSection.gameObject.activeSelf && rewardsUI != null)
		{
			for (int j = 0; j < rewardsUI.Count && rewardsUI[j].gameObject.activeSelf; j++)
			{
				rewardsUI[j].ShowUnlocked(isUnlocked);
			}
		}
		if (treasuresSection.gameObject.activeSelf)
		{
			treasureReward.ShowUnlocked(isUnlocked);
		}
	}

	public void Hide(bool instant = false)
	{
		if (base.gameObject.activeSelf)
		{
			window.Hide(instant);
		}
		enemyStatsTooltip.Hide();
		itemTooltip.Hide();
		SetElementFocused(null);
		StopAllCoroutines();
	}

	private void SetupRewards(List<Reward> rewards, string emptyRewardLocKey)
	{
		if (rewards == null)
		{
			rewardsSection.gameObject.SetActive(value: false);
		}
		else
		{
			List<Reward> list = rewards.FindAll((Reward x) => x.IsVisibleInUI());
			HelperTools.NormalizePool(ref rewardsUI, rewardPrefab.gameObject, rewardsScroll.content, list.Count);
			for (int num = 0; num < list.Count; num++)
			{
				UIQuestReward slot = rewardsUI[num];
				slot.ShowReward(list[num], delegate(Reward reward, bool hovered)
				{
					OnHoveredRewardSlot(reward, hovered, slot);
				}, !quest.IsCompleted());
			}
			rewardsSection.gameObject.SetActive(value: true);
		}
		completedRewardsText.SetTextKey(emptyRewardLocKey);
	}

	private void SetupTreasures(int treasures, string textLoc)
	{
		if (treasures <= 0)
		{
			treasuresSection.SetActive(value: false);
			return;
		}
		treasureReward.ShowReward(treasureIcon, string.Format(LocalizationManager.GetTranslation(textLoc), treasures), null);
		treasuresSection.SetActive(value: true);
	}

	private void SetupEnemies(List<string> enemyClasses)
	{
		if (enemyClasses == null || enemyClasses.Count == 0)
		{
			enemiesSection.gameObject.SetActive(value: false);
			return;
		}
		bool prefabsWasInstantiated = false;
		HelperTools.NormalizePool(ref enemiesUI, enemyPrefab.gameObject, enemiesScroll.content, enemyClasses.Count, delegate
		{
			prefabsWasInstantiated = true;
		});
		if (prefabsWasInstantiated && _dimmerUIElements != null)
		{
			_dimmerUIElements.Initialize();
		}
		for (int num = 0; num < enemyClasses.Count; num++)
		{
			UIQuestEnemy slot = enemiesUI[num];
			bool eliteMaskActivated = slot.EliteMaskActivated;
			slot.ShowEnemy(enemyClasses[num], quest.Level, delegate(CMonsterClass monster, bool hovered)
			{
				OnHoveredEnemySlot(monster, hovered, slot);
			});
			if (eliteMaskActivated && _dimmerUIElements != null)
			{
				_dimmerUIElements.Initialize();
			}
		}
		int num2 = ((enemyClasses.Count % 5 != 0) ? (5 - enemyClasses.Count % 5) : 0);
		for (int num3 = masksToCompleteRow.Count - 1; num3 >= 0; num3--)
		{
			if (num2 > 0)
			{
				num2--;
				masksToCompleteRow[num3].transform.SetAsLastSibling();
				masksToCompleteRow[num3].SetActive(value: true);
			}
			else
			{
				masksToCompleteRow[num3].SetActive(value: false);
			}
		}
		enemiesSection.gameObject.SetActive(value: true);
	}

	private void OnHoveredEnemySlot(CMonsterClass monster, bool hovered, UIQuestEnemy slot)
	{
		if (!hovered)
		{
			enemyStatsTooltip.Hide();
		}
		else
		{
			enemyStatsTooltip.Show(monster, quest.Level, slot.transform as RectTransform, isUnlocked);
		}
	}

	private void OnHoveredRewardSlot(Reward reward, bool hovered, UIQuestReward slot)
	{
		if (hovered && reward.Item != null)
		{
			itemTooltip.Show(reward.Item, slot.transform as RectTransform);
		}
		else
		{
			itemTooltip.Hide();
		}
	}

	private void SetElementFocused(Component element)
	{
		if (!(currentElementFocused == element))
		{
			if (currentElementFocused != null)
			{
				ExecuteEvents.Execute(currentElementFocused.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
				currentElementFocused = null;
			}
			currentElementFocused = element;
			if (element != null)
			{
				ExecuteEvents.Execute(currentElementFocused.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
			}
		}
	}

	private void OnDisable()
	{
	}
}
