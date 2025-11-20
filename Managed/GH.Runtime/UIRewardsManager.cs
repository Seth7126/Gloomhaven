#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MEC;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIRewardsManager : Singleton<UIRewardsManager>
{
	[SerializeField]
	private Transform rewardHolder;

	[SerializeField]
	private GameObject positiveBackground;

	[SerializeField]
	private GameObject negativeBackground;

	[SerializeField]
	private Image rewardImage;

	[SerializeField]
	private TMP_Text rewardAnnouncementText;

	[SerializeField]
	private Image lineImage;

	[SerializeField]
	private Image subicon;

	[SerializeField]
	private Sprite winGoldSprite;

	[SerializeField]
	private Sprite loseGoldSprite;

	[SerializeField]
	private Sprite winXPSprite;

	[SerializeField]
	private UIReward questReward;

	[SerializeField]
	private string positiveRewardAudioItem = "PlaySound_UIPingRewardPositive";

	[SerializeField]
	private string negativeRewardAudioItem = "PlaySound_UIPingRewardNegative";

	[SerializeField]
	private UIIntroductionRewardsProcess rewardIntroduction;

	[SerializeField]
	private LongConfirmHandler _longConfirmHandler;

	private ControllerInputAreaLocal controllerArea;

	private SimpleKeyActionHandlerBlocker blocker;

	private UIWindow myWindow;

	private Func<bool> interactionChecker;

	private bool hideBlackOverlayInstantly;

	private bool networkProcessIfServer;

	private Action onProcessEnded;

	private bool processingRewards;

	private GameObject pooledRewardObj;

	private int pooledRewardID;

	private bool nextRewardOverride;

	private bool isConfirmPressed;

	private CoroutineHandle _currentProcessRewardCoroutine;

	public bool IsShown => myWindow.IsOpen;

	public bool ProcessingRewards => processingRewards;

	private bool IsGuildmasterMode => SaveData.Instance.Global.GameMode == EGameMode.Guildmaster;

	private void Start()
	{
		SetInstance(this);
		myWindow = GetComponent<UIWindow>();
		controllerArea = GetComponent<ControllerInputAreaLocal>();
		if (controllerArea != null)
		{
			controllerArea.OnFocusedArea.AddListener(OnFocused);
		}
	}

	private void OnFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Reward);
	}

	private void HandleOnBeforeMainMenuLoadingStarted()
	{
		Timing.KillCoroutines(_currentProcessRewardCoroutine);
		EndProcess();
	}

	public void StartRewardsShowcase(List<Reward> rewards, bool hideBlackOverlayInstantly = false, bool networkProcessIfServer = false, Action onProcessEnded = null, Func<bool> interactionChecker = null)
	{
		StartRewardsShowcase(new List<RewardGroup>
		{
			new RewardGroup(rewards)
		}, hideBlackOverlayInstantly, networkProcessIfServer, onProcessEnded, interactionChecker);
	}

	public void StartRewardsShowcase(List<RewardGroup> rewardGroups, bool hideBlackOverlayInstantly = false, bool networkProcessIfServer = false, Action onProcessEnded = null, Func<bool> interactionChecker = null)
	{
		if (rewardGroups.IsNullOrEmpty() || rewardGroups.FirstOrDefault().Rewards.IsNullOrEmpty())
		{
			Debug.Log("About to start the rewards showcase but there are no rewards to show.");
			onProcessEnded?.Invoke();
			return;
		}
		Debug.Log("Starting the rewards showcase");
		this.hideBlackOverlayInstantly = hideBlackOverlayInstantly;
		this.networkProcessIfServer = networkProcessIfServer;
		this.onProcessEnded = onProcessEnded;
		this.interactionChecker = interactionChecker;
		_currentProcessRewardCoroutine = Timing.RunCoroutine(ProcessRewards(rewardGroups));
		myWindow.Show();
		if (Singleton<UIBlackOverlay>.Instance == null)
		{
			Debug.LogError("UIBlackOverlay.Instance is null in StartRewardsShowcase");
		}
		else
		{
			Singleton<UIBlackOverlay>.Instance.ShowControlled(myWindow, 0.8f, 1f);
		}
	}

	public void MoveToNextReward()
	{
		nextRewardOverride = true;
	}

	private void ConfirmPressed()
	{
		if (IsGuildmasterMode)
		{
			isConfirmPressed = true;
			return;
		}
		_longConfirmHandler?.Pressed(delegate
		{
			isConfirmPressed = true;
		});
	}

	private IEnumerator<float> ProcessRewards(List<RewardGroup> rewardGroups)
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<ESCMenu>.Instance.BeforeMainMenuLoadingStarted += HandleOnBeforeMainMenuLoadingStarted;
			_longConfirmHandler.SetActiveLongConfirmButton(!IsGuildmasterMode);
			blocker = new SimpleKeyActionHandlerBlocker(Singleton<ESCMenu>.Instance.IsOpen);
			Singleton<ESCMenu>.Instance.EscMenuStateChanged += InstanceOnEscMenuStateChanged;
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.CONFIRM_ACTION_BUTTON, ConfirmPressed).AddBlocker(blocker));
		}
		processingRewards = true;
		int rewardGroupIndex = 0;
		int rewardIndex = 0;
		bool movingToNextGroup = true;
		if ((interactionChecker == null) ? FFSNetwork.IsClient : (!interactionChecker()))
		{
			ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndHalt);
		}
		while (processingRewards)
		{
			bool flag = (!((interactionChecker == null) ? FFSNetwork.IsClient : (!interactionChecker())) || !networkProcessIfServer) && (Singleton<InputManager>.Instance.PlayerControl.MouseClickLeft.WasPressed || isConfirmPressed);
			if (flag && FFSNetwork.IsOnline && ((interactionChecker == null) ? FFSNetwork.IsHost : interactionChecker()) && networkProcessIfServer)
			{
				Synchronizer.SendGameAction(GameActionType.ProcessNextReward);
			}
			if (flag || movingToNextGroup || nextRewardOverride)
			{
				nextRewardOverride = false;
				if (flag && AutoTestController.s_ShouldRecordUIActionsForAutoTest)
				{
					AutoTestController.s_Instance.LogNextRewardClicked();
				}
				if (rewardGroupIndex < rewardGroups.Count)
				{
					if (rewardIndex < rewardGroups[rewardGroupIndex].Rewards.Count)
					{
						InitializeBackground();
						ProcessReward(rewardGroups[rewardGroupIndex].Rewards[rewardIndex]);
						rewardIndex++;
						movingToNextGroup = false;
						if (FFSNetwork.IsOnline && ((interactionChecker == null) ? FFSNetwork.IsClient : (!interactionChecker())))
						{
							ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndHalt);
						}
						isConfirmPressed = false;
					}
					else
					{
						rewardGroupIndex++;
						rewardIndex = 0;
						movingToNextGroup = true;
					}
				}
				else
				{
					EndProcess();
				}
			}
			yield return 0f;
		}
	}

	private void InstanceOnEscMenuStateChanged(bool open)
	{
		blocker.SetBlock(open);
	}

	private void InitializeBackground()
	{
		if (pooledRewardObj != null)
		{
			ObjectPool.RecycleCard(pooledRewardID, ObjectPool.ECardType.Item, pooledRewardObj);
		}
		positiveBackground.SetActive(value: false);
		negativeBackground.SetActive(value: false);
	}

	private void ProcessReward(Reward reward)
	{
		Debug.Log("Showing reward: " + reward.Type);
		subicon.gameObject.SetActive(value: false);
		switch (reward.Type)
		{
		case ETreasureType.None:
			positiveBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: false);
			lineImage.color = Color.white;
			rewardAnnouncementText.text = LocalizationManager.GetTranslation("GUI_GIFT_NONE");
			rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			break;
		case ETreasureType.Gold:
		{
			string text = "";
			if (AdventureState.MapState.DLLMode == ScenarioManager.EDLLMode.Guildmaster && reward.TreasureDistributionType != ETreasureDistributionType.Combined)
			{
				text = "EACH_";
			}
			if (reward.Amount < 0)
			{
				rewardImage.gameObject.SetActive(value: true);
				negativeBackground.SetActive(value: true);
				rewardImage.sprite = loseGoldSprite;
				lineImage.color = UIInfoTools.Instance.negativeTextColor;
				rewardAnnouncementText.text = string.Format(LocalizationManager.GetTranslation("GUI_GIFT_" + text + "LOST_GOLD"), reward.Amount);
				rewardAnnouncementText.color = UIInfoTools.Instance.negativeTextColor;
			}
			else if (reward.Amount > 0)
			{
				rewardImage.gameObject.SetActive(value: true);
				positiveBackground.SetActive(value: true);
				rewardImage.sprite = winGoldSprite;
				lineImage.color = Color.white;
				rewardAnnouncementText.text = string.Format(LocalizationManager.GetTranslation("GUI_GIFT_" + text + "WIN_GOLD"), reward.Amount);
				rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			}
			break;
		}
		case ETreasureType.XP:
			positiveBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: true);
			rewardImage.sprite = winXPSprite;
			lineImage.color = Color.white;
			rewardAnnouncementText.text = string.Format(LocalizationManager.GetTranslation("GUI_GIFT_XP"), reward.Amount);
			rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			break;
		case ETreasureType.Damage:
			negativeBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: true);
			rewardImage.sprite = UIInfoTools.Instance.DamageSprite;
			lineImage.color = UIInfoTools.Instance.negativeTextColor;
			rewardAnnouncementText.text = string.Format(LocalizationManager.GetTranslation("GUI_GIFT_DAMAGE"), reward.Amount);
			rewardAnnouncementText.color = UIInfoTools.Instance.negativeTextColor;
			break;
		case ETreasureType.Item:
		case ETreasureType.UnlockProsperityItem:
		{
			positiveBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: false);
			lineImage.color = Color.white;
			pooledRewardID = reward.Item.ID;
			pooledRewardObj = ObjectPool.SpawnCard(pooledRewardID, ObjectPool.ECardType.Item, rewardHolder, resetLocalScale: true, resetToMiddle: true);
			ItemCardUI component = pooledRewardObj.GetComponent<ItemCardUI>();
			component.item = reward.Item;
			Canvas component2 = component.GetComponent<Canvas>();
			if ((bool)component2)
			{
				component2.overrideSorting = false;
			}
			rewardAnnouncementText.text = LocalizationManager.GetTranslation("GUI_GIFT_ITEM");
			rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			break;
		}
		case ETreasureType.ItemStock:
		case ETreasureType.UnlockProsperityItemStock:
			positiveBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: false);
			lineImage.color = Color.white;
			pooledRewardID = reward.Item.ID;
			pooledRewardObj = ObjectPool.SpawnCard(pooledRewardID, ObjectPool.ECardType.Item, rewardHolder, resetLocalScale: true, resetToMiddle: true);
			pooledRewardObj.GetComponent<ItemCardUI>().item = reward.Item;
			rewardAnnouncementText.text = LocalizationManager.GetTranslation("GUI_GIFT_ITEM_DESIGN");
			rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			break;
		case ETreasureType.Condition:
		{
			RewardCondition rewardCondition2 = reward.Conditions.SingleOrDefault();
			if (rewardCondition2.PositiveCondition != CCondition.EPositiveCondition.NA)
			{
				positiveBackground.SetActive(value: true);
				rewardImage.gameObject.SetActive(value: true);
				lineImage.color = Color.white;
				UpdateConditionSpriteAndText(rewardCondition2, positiveCondition: true, forEnemy: false);
				rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			}
			else
			{
				negativeBackground.SetActive(value: true);
				rewardImage.gameObject.SetActive(value: true);
				lineImage.color = UIInfoTools.Instance.negativeTextColor;
				UpdateConditionSpriteAndText(rewardCondition2, positiveCondition: false, forEnemy: false);
				rewardAnnouncementText.color = UIInfoTools.Instance.negativeTextColor;
			}
			break;
		}
		case ETreasureType.EnemyCondition:
		{
			RewardCondition rewardCondition = reward.EnemyConditions.SingleOrDefault();
			if (rewardCondition.NegativeCondition != CCondition.ENegativeCondition.NA)
			{
				positiveBackground.SetActive(value: true);
				rewardImage.gameObject.SetActive(value: true);
				lineImage.color = Color.white;
				UpdateConditionSpriteAndText(rewardCondition, positiveCondition: false, forEnemy: true);
				rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			}
			else
			{
				negativeBackground.SetActive(value: true);
				rewardImage.gameObject.SetActive(value: true);
				lineImage.color = UIInfoTools.Instance.negativeTextColor;
				UpdateConditionSpriteAndText(rewardCondition, positiveCondition: true, forEnemy: true);
				rewardAnnouncementText.color = UIInfoTools.Instance.negativeTextColor;
			}
			break;
		}
		case ETreasureType.Infuse:
			positiveBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: true);
			lineImage.color = Color.white;
			UpdateInfusionSpriteAndText(reward.Infusions.SingleOrDefault());
			rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			break;
		case ETreasureType.Enhancement:
			positiveBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: false);
			lineImage.color = Color.white;
			rewardAnnouncementText.text = LocalizationManager.GetTranslation("GUI_GIFT_ENHANCEMENT");
			rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			break;
		case ETreasureType.Perk:
			positiveBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: false);
			lineImage.color = Color.white;
			rewardAnnouncementText.text = LocalizationManager.GetTranslation("GUI_GIFT_PERK");
			rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			break;
		default:
			positiveBackground.SetActive(value: true);
			rewardImage.gameObject.SetActive(value: true);
			lineImage.color = Color.white;
			rewardAnnouncementText.color = UIInfoTools.Instance.basicTextColor;
			questReward.ShowReward(reward);
			break;
		}
		AudioControllerUtils.PlaySound(reward.IsNegative() ? negativeRewardAudioItem : positiveRewardAudioItem);
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			rewardIntroduction?.Process(reward);
		}
	}

	private void UpdateInfusionSpriteAndText(ElementInfusionBoardManager.EElement element)
	{
		rewardAnnouncementText.text = string.Format(LocalizationManager.GetTranslation("GUI_GIFT_INFUSION"), element);
		rewardImage.sprite = UIInfoTools.Instance.GetRewardElementIcon(element);
	}

	private void UpdateConditionSpriteAndText(RewardCondition condition, bool positiveCondition, bool forEnemy)
	{
		if (positiveCondition)
		{
			switch (condition.PositiveCondition)
			{
			case CCondition.EPositiveCondition.Strengthen:
				rewardImage.sprite = UIInfoTools.Instance.GetIconPositiveCondition(CCondition.EPositiveCondition.Strengthen, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_STRENGTHEN" : "GUI_GIFT_STRENGTHEN");
				break;
			case CCondition.EPositiveCondition.Bless:
				rewardImage.sprite = UIInfoTools.Instance.GetIconPositiveCondition(CCondition.EPositiveCondition.Bless, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_BLESS" : "GUI_GIFT_BLESS");
				break;
			case CCondition.EPositiveCondition.Invisible:
				rewardImage.sprite = UIInfoTools.Instance.GetIconPositiveCondition(CCondition.EPositiveCondition.Invisible, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_INVISIBLE" : "GUI_GIFT_INVISIBLE");
				break;
			case CCondition.EPositiveCondition.Advantage:
				rewardImage.sprite = UIInfoTools.Instance.GetIconPositiveCondition(CCondition.EPositiveCondition.Advantage, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_ADVANTAGE" : "GUI_GIFT_ADVANTAGE");
				break;
			default:
				Debug.LogWarning($"[{typeof(UIRewardsManager)}] Type: {condition.Type} Id: {condition.ID} is handled as default");
				rewardAnnouncementText.text = LocalizationManager.GetTranslation("GUI_GIFT_POSITIVE_CONDITION");
				break;
			}
		}
		else
		{
			switch (condition.NegativeCondition)
			{
			case CCondition.ENegativeCondition.Poison:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Poison, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_POISON" : "GUI_GIFT_POISON");
				break;
			case CCondition.ENegativeCondition.Curse:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Curse, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_CURSE" : "GUI_GIFT_CURSE");
				break;
			case CCondition.ENegativeCondition.Disadvantage:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Disadvantage, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_DISADVANTAGE" : "GUI_GIFT_DISADVANTAGE");
				break;
			case CCondition.ENegativeCondition.Disarm:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Disarm, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_DISARM" : "GUI_GIFT_DISARM");
				break;
			case CCondition.ENegativeCondition.Immobilize:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Immobilize, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_IMMOBILIZE" : "GUI_GIFT_IMMOBILIZE");
				break;
			case CCondition.ENegativeCondition.Stun:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Stun, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_STUN" : "GUI_GIFT_STUN");
				break;
			case CCondition.ENegativeCondition.Muddle:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Muddle, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_MUDDLE" : "GUI_GIFT_MUDDLE");
				break;
			case CCondition.ENegativeCondition.Wound:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Wound, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_WOUND" : "GUI_GIFT_WOUND");
				break;
			case CCondition.ENegativeCondition.Sleep:
				rewardImage.sprite = UIInfoTools.Instance.GetIconNegativeCondition(CCondition.ENegativeCondition.Sleep, big: true);
				rewardAnnouncementText.text = LocalizationManager.GetTranslation(forEnemy ? "GUI_GIFT_ENEMY_SLEEP" : "GUI_GIFT_SLEEP");
				break;
			default:
				Debug.LogWarning($"[{typeof(UIRewardsManager)}] Type: {condition.Type} Id: {condition.ID} is handled as default");
				rewardAnnouncementText.text = LocalizationManager.GetTranslation("GUI_GIFT_NEGATIVE_CONDITION");
				break;
			}
		}
		if (condition.RoundDuration > 1)
		{
			rewardAnnouncementText.text += string.Format(LocalizationManager.GetTranslation("GUI_GIFT_SEVERAL_ROUNDS_DURATION"), condition.RoundDuration);
		}
		else if (condition.RoundDuration == 0)
		{
			rewardAnnouncementText.text += LocalizationManager.GetTranslation("GUI_GIFT_ROUNDS_DURATION");
		}
	}

	private void EndProcess()
	{
		Debug.Log("Reward showcase has ended.");
		if (InputManager.GamePadInUse && Singleton<ESCMenu>.Instance != null)
		{
			Singleton<ESCMenu>.Instance.BeforeMainMenuLoadingStarted -= HandleOnBeforeMainMenuLoadingStarted;
		}
		Singleton<ESCMenu>.Instance.EscMenuStateChanged -= InstanceOnEscMenuStateChanged;
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.CONFIRM_ACTION_BUTTON, ConfirmPressed);
		processingRewards = false;
		InitializeBackground();
		myWindow.Hide(instant: true);
		if (hideBlackOverlayInstantly)
		{
			Singleton<UIBlackOverlay>.Instance.HideInstant();
		}
		else
		{
			Singleton<UIBlackOverlay>.Instance.DoTimedTransition(myWindow, UIWindow.VisualState.Hidden);
		}
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.RewardsWindowDismissed));
		onProcessEnded?.Invoke();
	}

	public void ClientProcessNextReward(GameAction action)
	{
		MoveToNextReward();
	}
}
