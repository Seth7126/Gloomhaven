using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIGuildmasterAdventureRewardsManager : UIAdventureRewardsManager
{
	[SerializeField]
	private HeaderHighlightText header;

	[SerializeField]
	private ExtendedButton closeButton;

	[SerializeField]
	private CanvasGroup rewardsPopupCanvasGroup;

	[SerializeField]
	private RectTransform rewardsSection;

	[SerializeField]
	private UIQuestReward rewardPrefab;

	[SerializeField]
	private int initialPool = 5;

	[SerializeField]
	private UIIntroductionRewardsProcess rewardIntroduction;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private Hotkey hotkey;

	[SerializeField]
	private Image _backgroundHotkey;

	private List<UIQuestReward> rewardsUI = new List<UIQuestReward>();

	private UIWindow window;

	private Action onClosed;

	private SimpleKeyActionHandlerBlocker animationBlocker;

	private List<string> m_CharacterIDsUnlocked = new List<string>();

	public override bool IsShowingRewards => window.IsOpen;

	protected override void Awake()
	{
		base.Awake();
		window = GetComponent<UIWindow>();
		if (!InputManager.GamePadInUse)
		{
			closeButton.onClick.AddListener(Hide);
		}
		window.onTransitionComplete.AddListener(delegate(UIWindow w, UIWindow.VisualState stat)
		{
			if (stat == UIWindow.VisualState.Hidden)
			{
				OnHidden();
			}
		});
		HelperTools.NormalizePool(ref rewardsUI, rewardPrefab.gameObject, rewardsSection, initialPool, delegate(UIQuestReward obj)
		{
			obj.gameObject.SetActive(value: false);
		});
		controllerArea.OnFocusedArea.AddListener(OnFocused);
		animationBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Hide, ShowHotkey, HideHotkey).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)).AddBlocker(animationBlocker));
	}

	protected override void OnDestroy()
	{
		closeButton.onClick.RemoveAllListeners();
		HideHotkey();
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, Hide);
		}
		base.OnDestroy();
	}

	private void OnFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(PopupStateTag.GuildmasterRewards);
	}

	public override void ShowRewards(List<Reward> rewards, string title, string closeKey, Action onClosed = null)
	{
		this.onClosed = onClosed;
		closeButton.TextLanguageKey = closeKey;
		header.SetHighlgihtText(title);
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.rewards_screen);
		SetupRewards(rewards, closeKey == "GUI_QUEST_COMPLETED_REWARDS_CLOSE");
		if (Singleton<UIConfirmationBoxManager>.Instance.IsOpen)
		{
			Singleton<UIConfirmationBoxManager>.Instance.Hide();
			Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		}
		rewardsPopupCanvasGroup.alpha = 0f;
		CanvasGroup canvasGroup = rewardsPopupCanvasGroup;
		bool blocksRaycasts = (rewardsPopupCanvasGroup.interactable = false);
		canvasGroup.blocksRaycasts = blocksRaycasts;
		window.Show();
		controllerArea.Enable();
		animationBlocker.SetBlock(value: true);
		header.Show(delegate
		{
			OnFinishHeaderAnimation();
			rewardIntroduction.Process(rewards);
			animationBlocker.SetBlock(value: false);
		});
	}

	private void OnFinishHeaderAnimation()
	{
		rewardsPopupCanvasGroup.alpha = 1f;
		CanvasGroup canvasGroup = rewardsPopupCanvasGroup;
		bool blocksRaycasts = (rewardsPopupCanvasGroup.interactable = true);
		canvasGroup.blocksRaycasts = blocksRaycasts;
	}

	private void ShowHotkey()
	{
		hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		if (_backgroundHotkey != null)
		{
			_backgroundHotkey.enabled = true;
		}
	}

	private void HideHotkey()
	{
		hotkey.DisplayHotkey(active: false);
		hotkey.Deinitialize();
		if (_backgroundHotkey != null)
		{
			_backgroundHotkey.enabled = false;
		}
	}

	public void Hide()
	{
		if (m_CharacterIDsUnlocked.Count > 0)
		{
			string text = CharacterClassManager.Find(m_CharacterIDsUnlocked[0]).CharacterModel.ToString();
			m_CharacterIDsUnlocked.RemoveAt(0);
			if (!VideoCamera.s_This.PlayFullscreenVideo("Heroes/" + text, Hide, m_CharacterIDsUnlocked.Count == 0))
			{
				window.Hide();
			}
			else
			{
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
			}
		}
		else
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			window.Hide();
		}
	}

	private void OnDisable()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		header.Hide();
	}

	private void OnHidden()
	{
		header.Hide();
		controllerArea.Destroy();
		onClosed?.Invoke();
		if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<GuildmasterRewardsState>())
		{
			IStateFilter filter = new StateFilterByType(typeof(MapStoryState), typeof(ConfirmationBoxState), typeof(GuildmasterRewardsState)).InverseFilter();
			Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(filter);
		}
	}

	private void SetupRewards(List<Reward> rewards, bool storeCharacterUnlocks)
	{
		m_CharacterIDsUnlocked.Clear();
		HelperTools.NormalizePool(ref rewardsUI, rewardPrefab.gameObject, rewardsSection, rewards.Count);
		for (int i = 0; i < rewards.Count; i++)
		{
			rewardsUI[i].ShowReward(rewards[i]);
			if (storeCharacterUnlocks && rewards[i].Type == ETreasureType.UnlockCharacter)
			{
				m_CharacterIDsUnlocked.Add(rewards[i].CharacterID);
			}
		}
	}
}
