using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using FFSNet;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UICharacterCreatorWindow : MonoBehaviour
{
	private class CharacterViewInfo
	{
		public CMapCharacter Character;

		public ICharacterCreatorClass CharacterClass { get; private set; }

		public CharacterViewInfo(ICharacterCreatorClass characterClass)
		{
			CharacterClass = characterClass;
			Character = null;
		}

		public bool IsValid()
		{
			if (CharacterClass != null)
			{
				return Character?.PersonalQuest != null;
			}
			return false;
		}
	}

	private UIWindow window;

	[SerializeField]
	private UICharacterCreatorClassStep classStep;

	[SerializeField]
	private UICharacterCreatorPersonalQuestStep personalQuestStep;

	[SerializeField]
	private UICharacterCreatorNameStep nameStep;

	[SerializeField]
	private GUIAnimator transitionFromClassToName;

	[SerializeField]
	private GUIAnimator transitionFromNameToClass;

	[SerializeField]
	private GUIAnimator transitionFromNameToPersonalQuest;

	[SerializeField]
	private GUIAnimator transitionFromPersonalQuestToName;

	private CharacterViewInfo characterViewInfo;

	private CallbackPromise<CMapCharacter> promise;

	private ICharacterCreatorService creationService;

	private bool isHidding;

	public bool IsShown
	{
		get
		{
			if (!window.IsOpen)
			{
				return isHidding;
			}
			return true;
		}
	}

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState stat)
		{
			isHidding = false;
			switch (stat)
			{
			case UIWindow.VisualState.Shown:
				OpenClass();
				break;
			case UIWindow.VisualState.Hidden:
				OnHidden();
				break;
			}
		});
		window.onTransitionBegin.AddListener(delegate(UIWindow window, UIWindow.VisualState stat, bool _)
		{
			isHidding = stat == UIWindow.VisualState.Hidden;
		});
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		classStep = null;
		personalQuestStep = null;
		nameStep = null;
	}

	public ICallbackPromise<CMapCharacter> Build(List<ICharacterCreatorClass> classes, ICharacterCreatorService creationService, Action<ICharacterCreatorClass> onHovered = null)
	{
		SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.IsCreatingCharacter = true;
		transitionFromClassToName.Stop();
		if (transitionFromNameToClass != null)
		{
			transitionFromNameToClass.Stop();
		}
		if (transitionFromNameToPersonalQuest != null)
		{
			transitionFromNameToPersonalQuest.Stop();
		}
		transitionFromPersonalQuestToName.Stop();
		promise = new CallbackPromise<CMapCharacter>();
		this.creationService = creationService;
		characterViewInfo = new CharacterViewInfo(classes[0]);
		nameStep.Hide(instant: true);
		personalQuestStep.Hide(instant: true);
		classStep.Setup(classes, delegate(ICharacterCreatorClass classPreview, bool preview)
		{
			if (preview)
			{
				Singleton<Character3DDisplayManager>.Instance.Display(this, classPreview.Data.Model);
			}
			else
			{
				Singleton<Character3DDisplayManager>.Instance.Hide(this);
			}
		}, onHovered);
		isHidding = false;
		window.Show();
		return promise;
	}

	private void OpenClass()
	{
		classStep.Show(characterViewInfo.CharacterClass).Done(OnSelectedClass, Cancel);
	}

	private void OnSelectedClass(ICharacterCreatorClass character)
	{
		if (characterViewInfo?.CharacterClass.ID != character.ID)
		{
			characterViewInfo = new CharacterViewInfo(character);
		}
		classStep.Hide();
		transitionFromClassToName.Play(fromStart: false);
		NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true, null, deselectCurrentCharacter: false);
		OpenName();
	}

	private void OpenPersonalQuest()
	{
		if (transitionFromNameToPersonalQuest != null)
		{
			transitionFromNameToPersonalQuest.Play(fromStart: false);
		}
		if (transitionFromClassToName.IsPlaying)
		{
			transitionFromClassToName.Stop();
		}
		if (transitionFromPersonalQuestToName.IsPlaying)
		{
			transitionFromPersonalQuestToName.Stop();
		}
		personalQuestStep.Show(characterViewInfo.Character.PossiblePersonalQuests).Done(OnSelectedPersonalQuest, GoBackToName);
	}

	private void OnSelectedPersonalQuest(CPersonalQuestState personalQuest)
	{
		creationService.AssignPersonalQuest(characterViewInfo.Character, personalQuest);
		AdventureState.MapState.MapParty.AddCharacterToCharactersList(characterViewInfo.Character);
		personalQuestStep.Hide(instant: true);
		window.Hide(instant: true);
		SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.IsCreatingCharacter = false;
	}

	private void GoBackToClass()
	{
		transitionFromClassToName.Stop();
		transitionFromClassToName.GoInitState();
		NewPartyDisplayUI.PartyDisplay.Show(this);
		nameStep.Hide();
		if (transitionFromNameToClass != null)
		{
			transitionFromNameToClass.Play(fromStart: false);
		}
		OpenClass();
	}

	private void OpenName()
	{
		SceneController.Instance.SelectingPersonalQuest = true;
		nameStep.Show(characterViewInfo.CharacterClass).Done(OnSelectedName, GoBackToClass);
	}

	private void GoBackToName()
	{
		transitionFromPersonalQuestToName.Stop();
		personalQuestStep.Hide(instant: true);
		transitionFromPersonalQuestToName.Play(fromStart: false);
		OpenName();
	}

	private void OnSelectedName(string characterName)
	{
		if (FFSNetwork.IsOnline)
		{
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			int playerID = PlayerRegistry.MyPlayer.PlayerID;
			IProtocolToken supplementaryDataToken = new CampaignCharacterData(characterViewInfo.CharacterClass.ID, characterName);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.CampaignCreateCharacter, currentPhase, disableAutoReplication: false, 0, 0, 0, playerID, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		else
		{
			nameStep.Hide();
			characterViewInfo.Character = creationService.Create(characterViewInfo.CharacterClass.ID, characterName);
			OpenPersonalQuest();
		}
	}

	public void ProxyCreateCharacter(GameAction action)
	{
		CampaignCharacterData campaignCharacterData = action.SupplementaryDataToken as CampaignCharacterData;
		NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.Single((NetworkPlayer s) => s.PlayerID == action.SupplementaryDataIDMax);
		networkPlayer.CreatingCharacter = new CreatingCharacter(networkPlayer.PlayerID, campaignCharacterData.CharacterName);
		if (PlayerRegistry.MyPlayer.PlayerID == action.SupplementaryDataIDMax)
		{
			if (!nameStep.IsNameRepeated(campaignCharacterData.CharacterName))
			{
				nameStep.Hide();
				characterViewInfo.Character = creationService.Create(campaignCharacterData.CharacterID, campaignCharacterData.CharacterName);
				OpenPersonalQuest();
			}
			else
			{
				nameStep.Hide();
				nameStep.Show(characterViewInfo.CharacterClass).Done(OnSelectedName, GoBackToClass);
				nameStep.IsValid();
			}
		}
		else if (!nameStep.IsNameRepeated(campaignCharacterData.CharacterName))
		{
			CMapCharacter addCharacter = CharacterCreatorService.CreateCharacter(campaignCharacterData.CharacterID, campaignCharacterData.CharacterName);
			AdventureState.MapState.MapParty.AddCharacterToCharactersList(addCharacter);
		}
	}

	private void OnHidden()
	{
		if (promise != null)
		{
			Singleton<Character3DDisplayManager>.Instance.Hide(this);
			if ((bool)transitionFromNameToPersonalQuest)
			{
				transitionFromNameToPersonalQuest.Stop();
			}
			transitionFromClassToName.Stop();
			if (transitionFromNameToClass != null)
			{
				transitionFromNameToClass.Stop();
			}
			transitionFromPersonalQuestToName.Play();
			if (characterViewInfo == null || !characterViewInfo.IsValid())
			{
				promise.Cancel();
				promise = null;
				return;
			}
			promise.Resolve(characterViewInfo.Character);
			characterViewInfo = null;
			promise = null;
			SceneController.Instance.SelectingPersonalQuest = false;
			Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
		}
	}

	public void ResetUI()
	{
		NewPartyDisplayUI.PartyDisplay.Show(this);
		transitionFromClassToName.GoInitState();
		SceneController.Instance.SelectingPersonalQuest = false;
		Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
	}

	public void Cancel()
	{
		if (IsShown)
		{
			characterViewInfo = null;
			window.Hide();
			SceneController.Instance.SelectingPersonalQuest = false;
			Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
			SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.IsCreatingCharacter = false;
		}
	}

	public void TogglePerksTooltip()
	{
		classStep.TogglePerksTooltip();
	}
}
