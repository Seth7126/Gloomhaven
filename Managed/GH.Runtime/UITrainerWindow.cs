using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Message;
using Photon.Bolt;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UITrainerWindow : MonoBehaviour
{
	[SerializeField]
	private UIAchievementInventory inventory;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private List<GameObject> interactionMasks;

	[SerializeField]
	private List<Graphic> interactionGraphics;

	[SerializeField]
	private Color disabledInteractionColor;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private float disabledInteractionAlpha = 0.5f;

	private bool hasClaimedRewards;

	private UIWindow window;

	private ITrainerService service = new PartyAchievementService();

	private Action<bool> onExit;

	public BasicEventHandler OnNewNotificationShown;

	public UIAchievementInventory UIAchievementInventory => inventory;

	public event Action<bool> OnScreenStateChanged;

	[UsedImplicitly]
	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHidden);
		exitButton.onClick.AddListener(Exit);
		inventory.OnClaimedAchievement += OnAchievementClaimed;
		inventory.OnHoveredAchievement += OnHovered;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		window.onHidden.RemoveListener(OnHidden);
		exitButton.onClick.RemoveListener(Exit);
		inventory.OnClaimedAchievement -= OnAchievementClaimed;
		inventory.OnHoveredAchievement -= OnHovered;
	}

	private void OnHovered(CPartyAchievement achievement)
	{
		service.UncheckNewAchievement(achievement);
		OnNewNotificationShown?.Invoke();
	}

	public void Show(Action<bool> onExit)
	{
		this.onExit = onExit;
		hasClaimedRewards = false;
		window.Show();
		inventory.Show(service.GetAchievements());
		if (service.HasShownTrainerTooltip)
		{
			SetInteractable(interactable: true);
		}
		else
		{
			ShowIntroduction();
		}
		this.OnScreenStateChanged?.Invoke(obj: true);
	}

	private void ShowIntroduction()
	{
		SetInteractable(interactable: false);
		Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.Trainer, new MapStoryController.MapDialogInfo(new List<DialogLineDTO>
		{
			new DialogLineDTO("GUI_TRAINER_INTRODUCTION", "Trainer")
		}, delegate
		{
			SetInteractable(interactable: true);
		}, hideOtherUI: false), null, navigateToPreviousStateWhenHidden: false);
		service.HasShownTrainerTooltip = true;
	}

	private void SetInteractable(bool interactable)
	{
		inventory.SetInteractable(interactable);
		Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(!interactable);
		for (int i = 0; i < interactionMasks.Count; i++)
		{
			interactionMasks[i].gameObject.SetActive(!interactable);
		}
		for (int j = 0; j < interactionGraphics.Count; j++)
		{
			interactionGraphics[j].CrossFadeColor(interactable ? Color.white : disabledInteractionColor, 0f, ignoreTimeScale: true, useAlpha: true);
		}
		canvasGroup.alpha = (interactable ? 1f : disabledInteractionAlpha);
		CanvasGroup obj = canvasGroup;
		bool interactable2 = (canvasGroup.blocksRaycasts = interactable);
		obj.interactable = interactable2;
	}

	private void OnAchievementClaimed(CPartyAchievement achievement)
	{
		hasClaimedRewards = true;
		service.ClaimReward(achievement);
		List<Reward> list = (from it in achievement.Rewards.SelectMany((RewardGroup it) => it.Rewards)
			where it.IsVisibleInUI()
			select it).ToList();
		if (list.Count == 0 || (FFSNetwork.IsOnline && Singleton<UIAdventureRewardsManager>.Instance.IsShowingRewards))
		{
			OnShownRewards(achievement);
		}
		else
		{
			Singleton<UIAdventureRewardsManager>.Instance.ShowRewards(list, LocalizationManager.GetTranslation("GUI_ACHIEVEMENT_REWARDS"), "GUI_ACHIEVEMENT_REWARDS_CLOSE", delegate
			{
				OnShownRewards(achievement);
			});
		}
		if (FFSNetwork.IsHost)
		{
			IProtocolToken supplementaryDataToken = new AchievementToken(achievement);
			Synchronizer.SendGameAction(GameActionType.ClaimAchievementReward, ActionPhaseType.MapHQ, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		if (FFSNetwork.IsOnline)
		{
			hasClaimedRewards = false;
			Singleton<MapChoreographer>.Instance.RefreshAllMapLocations();
		}
	}

	private void OnShownRewards(CPartyAchievement achievement)
	{
		if (achievement.Achievement.CompleteDialogueLines.Count > 0)
		{
			Singleton<MapChoreographer>.Instance.ShowAchievementCompletionMessage(achievement);
		}
		else
		{
			Singleton<MapChoreographer>.Instance.FinishedShowingAchievementCompletionMessage(EMapMessageTrigger.AchievementClaimed);
		}
		if (!FFSNetwork.IsClient || window.IsVisible)
		{
			inventory.Show(service.GetAchievements(), selectDefaultFilter: false);
		}
	}

	public void Exit()
	{
		inventory.Hide();
		window.Hide();
	}

	private void OnHidden()
	{
		onExit?.Invoke(hasClaimedRewards);
		this.OnScreenStateChanged?.Invoke(obj: false);
	}

	public void ClientClaimAchievementReward(GameAction action)
	{
		CPartyAchievement cPartyAchievement = service.GetAchievements().FirstOrDefault((CPartyAchievement x) => x.ID == ((AchievementToken)action.SupplementaryDataToken).AchievementName);
		if (cPartyAchievement != null)
		{
			OnAchievementClaimed(cPartyAchievement);
			return;
		}
		throw new Exception("Error claiming achievement reward. Cannot find achievement called " + ((AchievementToken)action.SupplementaryDataToken).AchievementName + ".");
	}
}
