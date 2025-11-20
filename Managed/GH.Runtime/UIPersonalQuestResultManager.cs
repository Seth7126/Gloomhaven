using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Assets.Script.Misc;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using UnityEngine;

public class UIPersonalQuestResultManager : Singleton<UIPersonalQuestResultManager>
{
	private class StackInfo
	{
		public bool isCompleted;

		private Func<ICallbackPromise> process;

		public StackInfo(bool isCompleted, Func<ICallbackPromise> process)
		{
			this.isCompleted = isCompleted;
			this.process = process;
		}

		public ICallbackPromise Process()
		{
			return process();
		}
	}

	private const string NOTIFICATION_TAG = "PERSONAL_QUEST";

	[SerializeField]
	private UIPersonalQuestProgress personalQuestPrefab;

	[SerializeField]
	private UIRetirementManager retirementFlow;

	[SerializeField]
	private float otherPlayerCompletedPersonalQuestNotificationDuration = 20f;

	private bool hasHiddenUI;

	private List<StackInfo> queue = new List<StackInfo>();

	private StackInfo current;

	public ICallbackPromise ShowPersonalQuestsProgress(params CMapCharacter[] characters)
	{
		try
		{
			List<CMapCharacter> list = characters.Where((CMapCharacter it) => it.PersonalQuest.State != EPersonalQuestState.Completed && HasProgressed(it.PersonalQuest)).ToList();
			List<CMapCharacter> list2 = characters.Where((CMapCharacter it) => it.PersonalQuest.State == EPersonalQuestState.Completed && (!FFSNetwork.IsOnline || it.IsUnderMyControl) && HasProgressed(it.PersonalQuest)).ToList();
			if (list.Count + list2.Count == 0)
			{
				return CallbackPromise.Resolved();
			}
			CallbackPromise promise = new CallbackPromise();
			for (int num = 0; num < list.Count; num++)
			{
				CMapCharacter character = list[num];
				if (num == list.Count - 1 && list2.Count == 0)
				{
					queue.Add(new StackInfo(isCompleted: false, () => ProcessPersonalQuestProgressed(character).Then(delegate
					{
						promise.Resolve();
					})));
				}
				else
				{
					queue.Add(new StackInfo(isCompleted: false, () => ProcessPersonalQuestProgressed(character)));
				}
			}
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				CMapCharacter character2 = list2[num2];
				PersonalQuestDTO personalQuest = new PersonalQuestDTO(character2.PersonalQuest);
				character2.PersonalQuest.NextPersonalQuestStep();
				if (num2 == list2.Count - 1)
				{
					queue.Add(new StackInfo(isCompleted: true, () => ProcessPersonalQuestCompleted(character2, personalQuest).Then(delegate
					{
						promise.Resolve();
					})));
				}
				else
				{
					queue.Add(new StackInfo(isCompleted: true, () => ProcessPersonalQuestCompleted(character2, personalQuest)));
				}
			}
			ProcessNextAction();
			return promise;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing ShowPersonalQuestsProgress.\n Characters with null PQ: " + string.Join(", ", from it in characters
				where it.PersonalQuest == null
				select it.CharacterName));
			throw ex;
		}
	}

	private void ProcessNextAction()
	{
		if (current == null && queue.Count != 0)
		{
			current = queue[0];
			current.Process().Done(delegate
			{
				queue.Remove(current);
				current = null;
				ProcessNextAction();
			});
		}
	}

	private bool HasProgressed(CPersonalQuestState personalQuest)
	{
		if (personalQuest != null && (personalQuest.LastProgressShown != personalQuest.PersonalQuestConditionState.CurrentProgress || personalQuest.State == EPersonalQuestState.Completed))
		{
			return !personalQuest.PersonalQuestConditionState.Failed;
		}
		return false;
	}

	private ICallbackPromise ProcessPersonalQuestProgressed(CMapCharacter character)
	{
		if (character.PersonalQuest.IsConcealed && FFSNetwork.IsOnline && !character.IsUnderMyControl)
		{
			return CallbackPromise.Resolved();
		}
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		CallbackPromise callbackPromise = new CallbackPromise();
		UIPersonalQuestProgress personalQuestUI = ObjectPool.Spawn(personalQuestPrefab, base.transform);
		UINotificationManager.INotificationControl notificationControl = Singleton<UINotificationManager>.Instance.ShowNotification(new UINotificationManager.NotificationData
		{
			titleLoc = character.PersonalQuest.CurrentPersonalQuestStepData.LocalisedName,
			content = personalQuestUI.gameObject,
			icon = UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData)
		}, UIInfoTools.Instance.defaultNotificationDuration, null, "PERSONAL_QUEST");
		personalQuestUI.SetPersonalQuest(character.PersonalQuest, character.PersonalQuest.LastProgressShown);
		personalQuestUI.transform.localScale = Vector3.one;
		character.PersonalQuest.LastProgressShown = character.PersonalQuest.PersonalQuestConditionState.CurrentProgress;
		notificationControl.RegisterToOnHidden(callbackPromise.Resolve);
		return callbackPromise.Then(delegate
		{
			ObjectPool.Recycle(personalQuestUI.gameObject, personalQuestPrefab.gameObject);
			CameraController.s_CameraController?.FreeDisableCameraInput(this);
		});
	}

	public void ShowOtherPlayerCompletedQuestNotification(CMapCharacter character)
	{
		UIPersonalQuestProgress personalQuestUI = ObjectPool.Spawn(personalQuestPrefab, base.transform);
		UINotificationManager.INotificationControl notificationControl = Singleton<UINotificationManager>.Instance.ShowNotification(new UINotificationManager.NotificationData
		{
			titleLoc = character.PersonalQuest.CurrentPersonalQuestStepData.LocalisedName,
			content = personalQuestUI.gameObject,
			icon = UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData)
		}, UIInfoTools.Instance.defaultNotificationDuration, null, "PERSONAL_QUEST");
		personalQuestUI.SetPersonalQuest(character.PersonalQuest, character.PersonalQuest.LastProgressShown);
		character.PersonalQuest.LastProgressShown = character.PersonalQuest.PersonalQuestConditionState.CurrentProgress;
		notificationControl.RegisterToOnHidden(delegate
		{
			ObjectPool.Recycle(personalQuestUI.gameObject, personalQuestPrefab.gameObject);
		});
	}

	public ICallbackPromise ShowPersonalQuestCompleted(CMapCharacter character, PersonalQuestDTO personalQuest)
	{
		CallbackPromise promise = new CallbackPromise();
		queue.Add(new StackInfo(isCompleted: true, () => ProcessPersonalQuestCompleted(character, personalQuest).Then(delegate
		{
			promise.Resolve();
		})));
		ProcessNextAction();
		return promise;
	}

	private ICallbackPromise ProcessPersonalQuestCompleted(CMapCharacter character, PersonalQuestDTO personalQuest)
	{
		if (!hasHiddenUI)
		{
			hasHiddenUI = true;
			Singleton<QuestManager>.Instance.HideLogScreen(this, instant: false);
			Singleton<MapChoreographer>.Instance.OpenWorldMap(transition: false);
			Singleton<UIGuildmasterHUD>.Instance.Hide(this, instant: false);
			NewPartyDisplayUI.PartyDisplay.Hide(this, instant: false);
			Singleton<UINotificationManager>.Instance.Hide(this);
		}
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		return ProcessPersonalQuestStepCompleted(character, personalQuest).Then(delegate
		{
			CameraController.s_CameraController?.FreeDisableCameraInput(this);
			if (queue.Count < 2 || !queue[1].isCompleted)
			{
				hasHiddenUI = false;
				Singleton<QuestManager>.Instance.ShowLogScreen(this, instant: false);
				Singleton<UIGuildmasterHUD>.Instance.Show(this, instant: false);
				NewPartyDisplayUI.PartyDisplay.Show(this);
				Singleton<UINotificationManager>.Instance.Show(this);
			}
			return (queue.Count < 2 || !queue.Skip(1).Any((StackInfo it) => it.isCompleted)) ? ShowUnlockTownRecords() : CallbackPromise.Resolved();
		});
	}

	private ICallbackPromise ProcessPersonalQuestStepCompleted(CMapCharacter character, PersonalQuestDTO personalQuest)
	{
		if (personalQuest.Step < character.PersonalQuest.PersonalQuestSteps - 1)
		{
			return retirementFlow.ProcessProgressPersonalQuest(character, personalQuest);
		}
		return retirementFlow.ProcessRetirement(character, personalQuest);
	}

	private ICallbackPromise ShowUnlockTownRecords()
	{
		if (AdventureState.MapState.HeadquartersState.HasShownIntroTownRecords || AdventureState.MapState.MapParty.RetiredCharacterRecords.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		return Singleton<UIGuildmasterHUD>.Instance.ShowUnlockTownRecords();
	}

	public ICallbackPromise PlayerConfirmRetirement(CMapCharacter character, PersonalQuestDTO personalQuestState)
	{
		CallbackPromise promise = new CallbackPromise();
		if (character.IsUnderMyControl)
		{
			queue.Add(new StackInfo(isCompleted: false, () => retirementFlow.MPConfirmRetire(character, personalQuestState).Then(delegate
			{
				promise.Resolve();
			})));
		}
		else
		{
			queue.Add(new StackInfo(isCompleted: false, () => retirementFlow.MPConfirmRetire(character, personalQuestState).Then((Func<ICallbackPromise>)ShowUnlockTownRecords).Then(delegate
			{
				promise.Resolve();
			})));
		}
		ProcessNextAction();
		return promise;
	}

	public ICallbackPromise CreateOtherPlayerProgressedPersonalQuest(CMapCharacter character, PersonalQuestDTO personalQuestDto)
	{
		CallbackPromise showPromise = new CallbackPromise();
		queue.Add(new StackInfo(isCompleted: false, delegate
		{
			ICallbackPromise result = ShowOtherPlayerProgressedPersonalQuest(character, personalQuestDto);
			showPromise.Resolve();
			return result;
		}));
		ProcessNextAction();
		return showPromise;
	}

	private ICallbackPromise ShowOtherPlayerProgressedPersonalQuest(CMapCharacter character, PersonalQuestDTO personalQuestDto)
	{
		CallbackPromise promise = new CallbackPromise();
		UIPersonalQuestProgress personalQuestUI = ObjectPool.Spawn(personalQuestPrefab, base.transform);
		UINotificationManager.INotificationControl notificationControl = Singleton<UINotificationManager>.Instance.ShowNotification(new UINotificationManager.NotificationData
		{
			titleLoc = character.PersonalQuest.CurrentPersonalQuestStepData.LocalisedName,
			content = personalQuestUI.gameObject,
			icon = UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData),
			dataButton1 = new UINotificationManager.NotificationDataButton(delegate
			{
				ProcessPersonalQuestCompleted(character, personalQuestDto).Done(promise.Resolve);
			}, "GUI_WATCH_PERSONAL_QUEST_PROGRESS"),
			dataButton2 = new UINotificationManager.NotificationDataButton(promise.Resolve, "GUI_SKIP")
		}, otherPlayerCompletedPersonalQuestNotificationDuration, character.PersonalQuest.ID, "PERSONAL_QUEST");
		personalQuestUI.SetPersonalQuest(personalQuestDto.Data, character.PersonalQuest.LastProgressShown, personalQuestDto.CurrentProgress, personalQuestDto.TotalProgress);
		notificationControl.RegisterToOnHidden(delegate
		{
			ObjectPool.Recycle(personalQuestUI.gameObject, personalQuestPrefab.gameObject);
			if (promise.IsPending)
			{
				promise.Resolve();
			}
		});
		return promise;
	}

	public void HideAllPersonalQuestNotifications(bool instant = false)
	{
		queue.Clear();
		Singleton<UINotificationManager>.Instance.HideNotificationsByTag("PERSONAL_QUEST", instant);
	}

	protected override void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			Singleton<UINotificationManager>.Instance.Show(this);
		}
		base.OnDestroy();
	}
}
