#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Assets.Script.Misc;
using GLOO.Introduction;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapFTUEManager : Singleton<MapFTUEManager>
{
	[Serializable]
	public class MapFTUREStepEvent : UnityEvent<EMapFTUEStep>
	{
	}

	private bool isPlaying;

	public UnityEvent OnStarted = new UnityEvent();

	public UnityEvent OnFinished = new UnityEvent();

	public MapFTUREStepEvent OnStartedStep = new MapFTUREStepEvent();

	public MapFTUREStepEvent OnFinishedStep = new MapFTUREStepEvent();

	[SerializeField]
	private UIMapFTUEStep initialStep;

	private IMapFTUEStep currentStep;

	private List<EMapFTUEStep> stepsCompleted = new List<EMapFTUEStep>();

	private List<string> tags = new List<string>();

	public bool HasToShowFTUEOnNoneOrInitialStep
	{
		get
		{
			if (HasToShowFTUE)
			{
				EMapFTUEStep eMapFTUEStep = CurrentStep;
				return eMapFTUEStep == EMapFTUEStep.None || eMapFTUEStep == EMapFTUEStep.Initial;
			}
			return false;
		}
	}

	public static bool IsPlaying
	{
		get
		{
			if (Singleton<MapFTUEManager>.Instance != null)
			{
				return Singleton<MapFTUEManager>.Instance.isPlaying;
			}
			return false;
		}
	}

	public EMapFTUEStep CurrentStep
	{
		get
		{
			if (!IsPlaying || currentStep == null)
			{
				return EMapFTUEStep.None;
			}
			return currentStep.Step;
		}
	}

	public bool HasToShowFTUE
	{
		get
		{
			if (!AdventureState.MapState.IsCampaign || AdventureState.MapState.MapFTUECompleted)
			{
				return false;
			}
			if (!AdventureState.MapState.AllCompletedQuests.IsNullOrEmpty() || FFSNetwork.IsOnline)
			{
				AdventureState.MapState.MapFTUECompleted = true;
				return false;
			}
			return true;
		}
	}

	protected override void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
			Singleton<UIWindowManager>.Instance.RemoveSkipHideWindows(UIWindowID.PartyAssemblyWindow);
			OnStarted.RemoveAllListeners();
			OnFinished.RemoveAllListeners();
			OnStartedStep.RemoveAllListeners();
			OnFinishedStep.RemoveAllListeners();
			base.OnDestroy();
		}
	}

	public bool Process()
	{
		if (!HasToShowFTUE)
		{
			return false;
		}
		FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
		Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: true, this);
		Singleton<QuestManager>.Instance.HideLogScreen(this);
		Singleton<MapChoreographer>.Instance.ShowQuestLocations(this, show: false);
		NewPartyDisplayUI.PartyDisplay.EnableClickTracker(enable: false, this);
		isPlaying = true;
		OnStarted.Invoke();
		if (AdventureState.MapState.MapParty.CheckCharacters.Count == 0)
		{
			StartStep(initialStep);
		}
		else
		{
			LoadState();
		}
		Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
		return true;
	}

	private void OnSwitchedToMultiplayer()
	{
		Finish();
		UIConfirmationBoxManager.MainMenuInstance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_TURN_OFF_TUTORIAL"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_TURN_OFF_TUTORIAL_MULTIPLAYER"), "GUI_CLOSE", Singleton<UINavigation>.Instance.StateMachine.ToPreviousState);
	}

	private void LoadState()
	{
		List<CMapCharacter> checkCharacters = AdventureState.MapState.MapParty.CheckCharacters;
		if (checkCharacters.Count >= 1)
		{
			stepsCompleted.AddRange(new EMapFTUEStep[6]
			{
				EMapFTUEStep.Initial,
				EMapFTUEStep.CreatedFirstCharacter,
				EMapFTUEStep.CreateNewCharacter,
				EMapFTUEStep.PickCharacterClass,
				EMapFTUEStep.PickPersonalQuest,
				EMapFTUEStep.SelectFirstSlot
			});
		}
		if (checkCharacters.Count > 1 || AdventureState.MapState.MapParty.SelectedCharactersArray[1] != null)
		{
			stepsCompleted.AddRange(new EMapFTUEStep[1] { EMapFTUEStep.SelectSecondSlot });
		}
		if (AdventureState.MapState.MapParty.SelectedCharacters.Count() > 1)
		{
			stepsCompleted.AddRange(new EMapFTUEStep[1] { EMapFTUEStep.CreatedSecondCharacter });
		}
		if (AdventureState.MapState.MapParty.HasIntroduced(EIntroductionConcept.Merchant.ToString()))
		{
			stepsCompleted.AddRange(new EMapFTUEStep[2]
			{
				EMapFTUEStep.VisitMerchant,
				EMapFTUEStep.BuyItem
			});
		}
		if (AdventureState.MapState.MapParty.HasIntroduced(EIntroductionConcept.Quest.ToString()))
		{
			stepsCompleted.AddRange(new EMapFTUEStep[1] { EMapFTUEStep.InteractWithMap });
		}
		EMapFTUEStep eMapFTUEStep = EMapFTUEStep.None;
		foreach (EMapFTUEStep item in stepsCompleted)
		{
			CheckCompletedStep(item);
			if (item > eMapFTUEStep)
			{
				eMapFTUEStep = item;
			}
		}
		Debug.LogGUI(string.Format("Skip steps: {0}. Last step: {1}", string.Join(", ", stepsCompleted), eMapFTUEStep));
		OnFinishedStep.Invoke(eMapFTUEStep);
	}

	public void Finish()
	{
		if (isPlaying)
		{
			isPlaying = false;
			currentStep?.FinishStep();
			FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
			Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: false, this);
			Singleton<UIWindowManager>.Instance.RemoveSkipHideWindows(UIWindowID.PartyAssemblyWindow);
			Singleton<InteractabilityChecker>.Instance.ClearActiveRequests();
			Singleton<QuestManager>.Instance.ShowLogScreen(this);
			Singleton<MapChoreographer>.Instance.ShowQuestLocations(this, show: true);
			NewPartyDisplayUI.PartyDisplay.EnableClickTracker(enable: true, this);
			NewPartyDisplayUI.PartyDisplay.Show(this);
			Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
			AdventureState.MapState.MapFTUECompleted = true;
			SaveData.Instance.SaveCurrentAdventureData();
			Debug.LogGUI("Finished FTUE");
			OnFinished.Invoke();
		}
	}

	public void Skip()
	{
		if (isPlaying)
		{
			Debug.LogGUI("Skip FTUE");
			Finish();
		}
	}

	public bool HasCompletedStep(EMapFTUEStep step)
	{
		if (AdventureState.MapState.MapFTUECompleted)
		{
			return true;
		}
		return stepsCompleted.Contains(step);
	}

	public bool HasProcessedTag(string tag)
	{
		return tags.Contains(tag);
	}

	public void AddTag(string tag)
	{
		Debug.LogGUI("Add ftue tag " + tag);
		tags.Add(tag);
	}

	public void CompleteStep(EMapFTUEStep step)
	{
		if (isPlaying && currentStep != null && currentStep.Step == step)
		{
			currentStep.FinishStep();
		}
		else
		{
			Debug.LogGUI($"Skip complete step {step} (playing: {isPlaying} isCurrent: {currentStep != null && currentStep.Step == step})");
		}
	}

	public ICallbackPromise StartStep(IMapFTUEStep step)
	{
		if (!isPlaying || HasCompletedStep(step.Step))
		{
			Debug.LogGUI($"Skip start step {step.Step} (playing: {isPlaying} isCompleted: {HasCompletedStep(step.Step)})");
			return CallbackPromise.Resolved();
		}
		if (currentStep != null)
		{
			CompleteStep(currentStep.Step);
		}
		currentStep = step;
		ICallbackPromise callbackPromise = step.StartStep();
		CheckStartedStep(step.Step);
		OnStartedStep.Invoke(step.Step);
		return callbackPromise.Then(delegate
		{
			OnCompleteStep(step.Step);
		});
	}

	private void OnCompleteStep(EMapFTUEStep step)
	{
		if (isPlaying && (currentStep == null || currentStep.Step == step))
		{
			Debug.LogGUI($"OnCompleteStep {step}");
			stepsCompleted.Add(step);
			currentStep = null;
			CheckCompletedStep(step);
			OnFinishedStep.Invoke(step);
		}
		else
		{
			Debug.LogGUI($"Skip OnCompleteStep {step} (playing: {isPlaying} current: {currentStep == null || currentStep.Step == step})");
		}
	}

	private void CheckCompletedStep(EMapFTUEStep step)
	{
		switch (step)
		{
		case EMapFTUEStep.CreatedFirstCharacter:
			Singleton<UIWindowManager>.Instance.RemoveSkipHideWindows(UIWindowID.PartyAssemblyWindow);
			NewPartyDisplayUI.PartyDisplay.EnableClickTracker(enable: true, this);
			break;
		case EMapFTUEStep.CreatedSecondCharacter:
			Singleton<QuestManager>.Instance.ShowLogScreen(this);
			Singleton<MapChoreographer>.Instance.ShowQuestLocations(this, show: true);
			NewPartyDisplayUI.PartyDisplay.Hide(this);
			Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
			break;
		case EMapFTUEStep.VisitMerchant:
			NewPartyDisplayUI.PartyDisplay.Show(this);
			break;
		case EMapFTUEStep.InteractWithMap:
			Finish();
			break;
		}
	}

	private void CheckStartedStep(EMapFTUEStep step)
	{
		if (step == EMapFTUEStep.CreateNewCharacter)
		{
			Singleton<UIWindowManager>.Instance.AddSkipHideWindows(UIWindowID.PartyAssemblyWindow);
		}
	}
}
