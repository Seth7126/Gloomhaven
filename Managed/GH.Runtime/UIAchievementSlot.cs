using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using FFSNet;
using I2.Loc;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAchievementSlot : UIAchievement, IMoveHandler, IEventSystemHandler
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private List<Graphic> interactionMasks;

	[SerializeField]
	private Color interactionDisabledColor;

	[SerializeField]
	private UINewNotificationTip newNotification;

	[SerializeField]
	private LoopAnimator highlightAnimator;

	[Header("Completed")]
	[SerializeField]
	private GameObject completedPanel;

	[SerializeField]
	private TextMeshProUGUI completedDate;

	[SerializeField]
	private string formatDate = "dd/MM/y, HH:mm";

	[Header("Rewards")]
	[SerializeField]
	private List<UIQuestReward> rewardsUI;

	[SerializeField]
	private ExtendedButton rewardButton;

	[SerializeField]
	private CanvasGroup rewardPanel;

	[SerializeField]
	private float opacityRewardPanelClaimed = 0.5f;

	[SerializeField]
	private GameObject availableRewardText;

	private CPartyAchievement _partyAchievement;

	private CanvasGroup canvasGroup;

	private LayoutElement layout;

	private Action<UIAchievementSlot, MoveDirection> onMoved;

	private Action<UIAchievementSlot> onClaimReward;

	private Action<UIAchievementSlot, bool> onHovered;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	private List<string> m_CharacterIDsUnlocked = new List<string>();

	public EAchievementType Type => achievementYML.AchievementType;

	public string OrderId => achievementYML.AchievementOrderId ?? base.Achievement.ID;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		layout = GetComponent<LayoutElement>();
		rewardButton.onClick.AddListener(ClaimReward);
		if (InputManager.GamePadInUse)
		{
			_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this, defaultIsBlock: false);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, ClaimReward).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)).AddBlocker(_skipFrameKeyActionHandlerBlocker));
		}
	}

	private void OnDestroy()
	{
		rewardButton.onClick.RemoveAllListeners();
		button.onClick.RemoveAllListeners();
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, ClaimReward);
		}
	}

	public void SetVisibility(bool value)
	{
		canvasGroup.alpha = (value ? 1f : 0f);
		canvasGroup.interactable = value;
		canvasGroup.blocksRaycasts = value;
		layout.ignoreLayout = !value;
		if (!value)
		{
			OnDisable();
		}
	}

	public void SetAchievement(CPartyAchievement partyAchievement, Action<UIAchievementSlot> onClaimReward, Action<UIAchievementSlot, bool> onHovered, Action<UIAchievementSlot, MoveDirection> onMoved)
	{
		_partyAchievement = partyAchievement;
		this.onClaimReward = onClaimReward;
		this.onHovered = onHovered;
		this.onMoved = onMoved;
		StopAnimations();
		base.SetAchievement(partyAchievement);
		if (partyAchievement.IsNew)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
		List<Reward> list = partyAchievement.Rewards.SelectMany((RewardGroup it) => it.Rewards.Where((Reward reward) => reward.IsVisibleInUI() || (reward.Type == ETreasureType.UnlockQuest && EAchievementType.Relics == Type))).ToList();
		HelperTools.NormalizePool(ref rewardsUI, rewardsUI[0].gameObject, rewardsUI[0].transform.parent, list.Count);
		m_CharacterIDsUnlocked.Clear();
		for (int num = 0; num < list.Count; num++)
		{
			rewardsUI[num].ShowReward(list[num]);
			if (list[num].Type == ETreasureType.UnlockCharacter)
			{
				m_CharacterIDsUnlocked.Add(list[num].CharacterID);
			}
		}
	}

	public override void SetAchievement(CPartyAchievement partyAchievement)
	{
		SetAchievement(partyAchievement, null, null, null);
	}

	public void ClaimReward()
	{
		if (partyAchievement.State == EAchievementState.Completed)
		{
			if (FFSNetwork.IsClient || (FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0))
			{
				return;
			}
			if (m_CharacterIDsUnlocked.Count > 0)
			{
				string text = CharacterClassManager.Find(m_CharacterIDsUnlocked[0]).CharacterModel.ToString();
				m_CharacterIDsUnlocked.RemoveAt(0);
				if (!VideoCamera.s_This.PlayFullscreenVideo("Heroes/" + text, delegate
				{
					InputManager.RequestEnableInput(this, EKeyActionTag.All);
					ClaimReward();
				}, m_CharacterIDsUnlocked.Count == 0))
				{
					onClaimReward?.Invoke(this);
				}
				else
				{
					InputManager.RequestDisableInput(this, EKeyActionTag.All);
				}
			}
			else
			{
				onClaimReward?.Invoke(this);
			}
		}
		_skipFrameKeyActionHandlerBlocker?.Run();
	}

	public override void RefresState()
	{
		base.RefresState();
		if (partyAchievement.State == EAchievementState.RewardsClaimed)
		{
			completedDate.text = partyAchievement.CompletedTimeStamp.ToString(formatDate);
			completedPanel.SetActive(value: true);
		}
		else
		{
			completedPanel.SetActive(value: false);
		}
		if (partyAchievement.State == EAchievementState.Completed)
		{
			highlightAnimator.StartLoop();
		}
		else
		{
			highlightAnimator.StopLoop();
		}
		if ((bool)availableRewardText)
		{
			availableRewardText?.SetActive(partyAchievement.State == EAchievementState.Completed);
		}
		rewardButton.gameObject.SetActive(partyAchievement.State == EAchievementState.Completed);
		rewardPanel.alpha = ((partyAchievement.State == EAchievementState.RewardsClaimed) ? opacityRewardPanelClaimed : 1f);
		rewardButton.interactable = !FFSNetwork.IsOnline || FFSNetwork.IsHost;
	}

	public void SetInteractable(bool interactable)
	{
		for (int i = 0; i < interactionMasks.Count; i++)
		{
			interactionMasks[i].gameObject.SetActive(!interactable);
		}
		icon.CrossFadeColor(interactable ? Color.white : interactionDisabledColor, 0f, ignoreTimeScale: true, useAlpha: false);
		rewardButton.interactable = !FFSNetwork.IsOnline || FFSNetwork.IsHost;
	}

	public void OnHovered(bool hovered)
	{
		onHovered?.Invoke(this, hovered);
		if (hovered)
		{
			newNotification.Hide();
		}
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
		if (!CoreApplication.IsQuitting)
		{
			if (VideoCamera.IsPlaying)
			{
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
			}
			StopAnimations();
			DisableNavigation();
		}
	}

	public void StopAnimations()
	{
		highlightAnimator.StopLoop();
	}

	private void OnLanguageChanged()
	{
		if (_partyAchievement != null)
		{
			base.SetAchievement(_partyAchievement);
		}
	}

	public void EnableNavigation()
	{
		button.interactable = true;
		button.SetNavigation(Navigation.Mode.Vertical);
	}

	public void DisableNavigation()
	{
		button.interactable = false;
		button.DisableNavigation();
	}

	public void OnMove(AxisEventData eventData)
	{
		onMoved?.Invoke(this, eventData.moveDir);
	}
}
