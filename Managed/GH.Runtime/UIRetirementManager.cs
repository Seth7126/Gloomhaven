#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Message;
using Photon.Bolt;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class UIRetirementManager : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	protected UICompletedPersonalQuestWindow completedPopup;

	[SerializeField]
	protected UIRetireCharacterConfirmationBox retireConfirmationBox;

	private void Awake()
	{
		window.onHidden.AddListener(ResetState);
	}

	public ICallbackPromise ProcessRetirement(CMapCharacter character, PersonalQuestDTO completedStepState)
	{
		ResetState();
		return completedPopup.Show(character, completedStepState).Then(delegate
		{
			if (FFSNetwork.IsOnline)
			{
				foreach (NetworkPlayer item in Singleton<UIReadyToggle>.Instance.PlayersReady)
				{
					if (item != PlayerRegistry.MyPlayer)
					{
						ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
						int playerID = item.PlayerID;
						IProtocolToken supplementaryDataToken = new ReadyUpToken(EReadyUpToggleStates.Retirement.ToString());
						Synchronizer.SendGameAction(GameActionType.ForceUnreadyUp, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, playerID, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
					}
				}
				CallbackPromise promise = new CallbackPromise();
				StartCoroutine(WaitAllPlayersUnready(delegate
				{
					Hide();
					int actorID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
					Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.RetireCharacter, ActionProcessor.CurrentPhase, disableAutoReplication: false, actorID);
					promise.Resolve();
				}));
				return promise;
			}
			return CommitRetirement(character, completedStepState);
		});
	}

	private IEnumerator WaitAllPlayersUnready(Action callback)
	{
		yield return new WaitUntil(() => !Singleton<UIReadyToggle>.Instance.PlayerReadiedState.Any((KeyValuePair<NetworkPlayer, EReadyUpToggleStates> it) => it.Value != EReadyUpToggleStates.Retirement));
		callback?.Invoke();
	}

	public ICallbackPromise MPConfirmRetire(CMapCharacter character, PersonalQuestDTO completedStepState)
	{
		CallbackPromise promise = new CallbackPromise();
		Singleton<UIReadyToggle>.Instance.Initialize(show: false, null, null, delegate
		{
			Singleton<UIMapMultiplayerController>.Instance.HideRetirementMultiplayer();
			Singleton<UIReadyToggle>.Instance.Reset();
			CommitRetirement(character, completedStepState).Done(promise.Resolve);
		}, delegate(NetworkPlayer player, bool isReady)
		{
			Singleton<UIMapMultiplayerController>.Instance.UpdateReadyTracker(player, isReady);
		}, null, null, "GUI_READY", "GUI_UNREADY", "PlaySound_UIMultiPlayerReady", bringToFront: false, null, null, validateReadyUpOnPlayerLeft: true, UIReadyToggle.EReadyUpType.Player, EReadyUpToggleStates.Retirement);
		Singleton<UIMapMultiplayerController>.Instance.ShowRetirementMultiplayer();
		Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
		return promise;
	}

	private ICallbackPromise CommitRetirement(CMapCharacter character, PersonalQuestDTO completedStepState)
	{
		Debug.LogGUI("CommitRetirement " + character.CharacterID);
		AdventureState.MapState.MapParty.RetireCharacter(character.CharacterID);
		int id = ((!AdventureState.MapState.IsCampaign) ? CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID) : character.CharacterName.GetHashCode());
		ControllableRegistry.DestroyControllable(id);
		return ShowOnRetired(character, completedStepState);
	}

	private ICallbackPromise ShowOnRetired(CMapCharacter character, PersonalQuestDTO completedStepState)
	{
		if (FFSNetwork.IsOnline && !character.IsUnderMyControl)
		{
			return ShowStory(character, character.PersonalQuest.CurrentPersonalQuestStepData.LocalisedCompletedStory, character.PersonalQuest.CurrentPersonalQuestStepData.AudioIdCompletedStory).Then(delegate
			{
				window.Show();
				Singleton<Character3DDisplayManager>.Instance.Display(this, character.CharacterYMLData.Model, "", "PowerUp");
				return ShowRewards(character, character.PersonalQuest.FinalRewards.SelectMany((RewardGroup it) => it.Rewards).ToList(), "GUI_CHARACTER_RETIRED");
			}).Then(Hide, null);
		}
		return ShowStory(character, character.PersonalQuest.CurrentPersonalQuestStepData.LocalisedCompletedStory, character.PersonalQuest.CurrentPersonalQuestStepData.AudioIdCompletedStory).Then(() => RetireCharacter(character)).Then(() => ShowRewards(character, completedStepState.Rewards, "GUI_CHARACTER_RETIRED")).Then(delegate
		{
			Hide();
		})
			.Then(() => ShowUnlockedLocations(completedStepState.Rewards));
	}

	public ICallbackPromise ProcessProgressPersonalQuest(CMapCharacter character, PersonalQuestDTO completedStepState)
	{
		ResetState();
		if (FFSNetwork.IsOnline && !character.IsUnderMyControl)
		{
			return ShowStory(character, completedStepState.LocalizationProgressStory, completedStepState.AudioIdProgressStory).Then(() => ShowUnlockedLocations(completedStepState.Rewards));
		}
		return completedPopup.Show(character, completedStepState).Then(delegate
		{
			if (FFSNetwork.IsOnline && character.IsUnderMyControl)
			{
				int actorID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
				Synchronizer.SendGameAction(GameActionType.ProgressPersonalQuest, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID);
			}
		}).Then(() => ShowStory(character, completedStepState.LocalizationProgressStory, completedStepState.AudioIdProgressStory))
			.Then(() => ShowUnlockedLocations(completedStepState.Rewards))
			.Then(delegate
			{
				Hide();
			});
	}

	private void ResetState()
	{
		Singleton<Character3DDisplayManager>.Instance.Hide(this);
		retireConfirmationBox.Hide();
	}

	private void Hide()
	{
		window.Hide(instant: true);
	}

	private ICallbackPromise RetireCharacter(CMapCharacter character)
	{
		window.Show();
		Singleton<Character3DDisplayManager>.Instance.Display(this, character.CharacterYMLData.Model);
		CallbackPromise callbackPromise = new CallbackPromise();
		retireConfirmationBox.ShowConfirmationBox(character, delegate
		{
			Singleton<Character3DDisplayManager>.Instance.Display(this, character.CharacterYMLData.Model, "", "PowerUp");
		}, callbackPromise.Resolve);
		return callbackPromise;
	}

	private ICallbackPromise ShowRewards(CMapCharacter character, List<Reward> rewards, string titleLocKey)
	{
		CallbackPromise callbackPromise = new CallbackPromise();
		Singleton<UIAdventureRewardsManager>.Instance.ShowRewards(rewards, string.Format(LocalizationManager.GetTranslation(titleLocKey), character.CharacterName.IsNOTNullOrEmpty() ? character.CharacterName : LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey)), "GUI_QUEST_COMPLETED_REWARDS_CLOSE", callbackPromise.Resolve);
		return callbackPromise;
	}

	private ICallbackPromise ShowStory(CMapCharacter character, string storyLineLocKey, string storyAudioId)
	{
		if (storyLineLocKey.IsNullOrEmpty())
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise callbackPromise = new CallbackPromise();
		DialogLineDTO item = new DialogLineDTO(storyLineLocKey, "Narrator", EExpression.Default, null, character.CharacterName.IsNOTNullOrEmpty() ? character.CharacterName : LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey), storyAudioId);
		Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.Retirement, new MapStoryController.MapDialogInfo(new List<DialogLineDTO> { item }, callbackPromise.Resolve));
		return callbackPromise;
	}

	private ICallbackPromise ShowUnlockedLocations(List<Reward> rewards)
	{
		List<string> list = (from it in rewards
			where it.Type == ETreasureType.UnlockQuest
			select it.UnlockName).ToList();
		if (list.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise callbackPromise = new CallbackPromise();
		Singleton<MapChoreographer>.Instance.ShowUnlockedQuests(list, callbackPromise.Resolve);
		return callbackPromise;
	}
}
