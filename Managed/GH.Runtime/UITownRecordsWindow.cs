using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Message;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UITownRecordsWindow : MonoBehaviour
{
	private enum ETownRecordsMode
	{
		None,
		Narrative,
		Log
	}

	[SerializeField]
	private List<UITownRecordsCharacter> townRecordsSlots;

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private UIWindow window;

	private readonly ITownRecordsService service = new TownRecordsService();

	private CanvasGroup canvasGroup;

	private ETownRecordsMode mode;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		canvasGroup = window.GetComponent<CanvasGroup>();
		window.onHidden.AddListener(delegate
		{
			canvasGroup.ignoreParentGroups = false;
			controllerArea.Destroy();
		});
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
	}

	public ICallbackPromise OpenLog(bool force = false)
	{
		if (window.IsOpen)
		{
			if (mode != ETownRecordsMode.Narrative)
			{
				return CallbackPromise.Cancelled();
			}
			return CallbackPromise.Resolved();
		}
		mode = ETownRecordsMode.Log;
		canvasGroup.ignoreParentGroups = force;
		CreateRecords(service.GetCharacters());
		scroll.gameObject.SetActive(value: true);
		window.Show();
		controllerArea.Enable();
		return ShowStory(service.GetStory(), EMapMessageTrigger.TownRecords);
	}

	public ICallbackPromise OpenRecords()
	{
		if (window.IsOpen)
		{
			if (mode != ETownRecordsMode.Narrative)
			{
				return CallbackPromise.Cancelled();
			}
			return CallbackPromise.Resolved();
		}
		mode = ETownRecordsMode.Narrative;
		canvasGroup.ignoreParentGroups = false;
		scroll.gameObject.SetActive(value: false);
		window.Show();
		controllerArea.Enable();
		return ShowStory(service.GetStory(), EMapMessageTrigger.TownRecords).Then(() => ClaimAchievements(service.GetAchievementsToClaim()));
	}

	public void Exit()
	{
		mode = ETownRecordsMode.None;
		window.Hide();
	}

	private ICallbackPromise ShowStory(List<DialogLineDTO> storyLines, EMapMessageTrigger trigger)
	{
		if (storyLines.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise promise = new CallbackPromise();
		scroll.gameObject.SetActive(value: false);
		Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: false);
		Singleton<MapStoryController>.Instance.Show(trigger, new MapStoryController.MapDialogInfo(storyLines, delegate
		{
			Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: true);
			scroll.gameObject.SetActive(mode == ETownRecordsMode.Log);
			promise.Resolve();
		}, hideOtherUI: false));
		return promise;
	}

	private ICallbackPromise ClaimAchievements(List<CPartyAchievement> achievements)
	{
		ICallbackPromise callbackPromise = CallbackPromise.Resolved();
		foreach (CPartyAchievement achievement in achievements)
		{
			callbackPromise = callbackPromise.Then(() => ClaimAchievement(achievement));
		}
		return callbackPromise;
	}

	private ICallbackPromise ClaimAchievement(CPartyAchievement achievement)
	{
		return ShowStory(achievement.Achievement.CompleteDialogueLines.ConvertAll((MapDialogueLine it) => new DialogLineDTO(it)), EMapMessageTrigger.AchievementClaimed).Then(delegate
		{
			CallbackPromise promise = new CallbackPromise();
			List<Reward> rewards = achievement.Rewards.SelectMany((RewardGroup it) => it.Rewards).ToList();
			Singleton<UIAdventureRewardsManager>.Instance.ShowRewards(rewards.FindAll((Reward reward) => reward.Type == ETreasureType.UnlockQuest || reward.IsVisibleInUI()), LocalizationManager.GetTranslation("GUI_ACHIEVEMENT_REWARDS"), "GUI_ACHIEVEMENT_REWARDS_CLOSE", delegate
			{
				List<Reward> rewards2 = rewards.Where((Reward it) => it.Type == ETreasureType.UnlockQuest).ToList();
				rewards.RemoveAll((Reward it) => it.Type == ETreasureType.UnlockQuest);
				ApplyUnlockedLocations(rewards2).Done(delegate
				{
					Singleton<UIDistributeRewardManager>.Instance.Process(rewards, promise.Resolve);
				});
			});
			return promise.Then(delegate
			{
				service.ClaimAchievement(achievement);
			});
		});
	}

	private ICallbackPromise ApplyUnlockedLocations(List<Reward> rewards)
	{
		if (rewards.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise promise = new CallbackPromise();
		AdventureState.MapState.ApplyRewards(rewards, "party");
		Singleton<MapChoreographer>.Instance.RefreshAllMapLocations(delegate
		{
			Singleton<MapChoreographer>.Instance.ShowUnlockedQuests(rewards.Select((Reward it) => it.UnlockName).ToList(), promise.Resolve);
		}, save: false);
		return promise;
	}

	private void CreateRecords(List<ITownRecordCharacter> characters)
	{
		HelperTools.NormalizePool(ref townRecordsSlots, townRecordsSlots[0].gameObject, scroll.content, characters.Count);
		for (int i = 0; i < characters.Count; i++)
		{
			townRecordsSlots[i].Display(characters[i], OnHovered);
		}
	}

	private void OnHovered(UITownRecordsCharacter slot, bool hovered)
	{
		if (hovered && InputManager.GamePadInUse)
		{
			scroll.ScrollToFit(slot.transform as RectTransform);
		}
	}

	private void DisableNavigation()
	{
		for (int i = 0; i < townRecordsSlots.Count && townRecordsSlots[i].gameObject.activeSelf; i++)
		{
			townRecordsSlots[i].DisableNavigation();
		}
	}

	private void EnableNavigation()
	{
		for (int i = 0; i < townRecordsSlots.Count && townRecordsSlots[i].gameObject.activeSelf; i++)
		{
			townRecordsSlots[i].EnableNavigation();
		}
		EventSystem.current.SetSelectedGameObject(townRecordsSlots.FirstOrDefault()?.gameObject);
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.TownRecords);
	}
}
