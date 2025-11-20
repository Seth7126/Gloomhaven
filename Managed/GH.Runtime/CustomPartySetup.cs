#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLOOM;
using GLOOM.MainMenu;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using SharedLibrary;
using SharedLibrary.Client;
using TMPro;
using UnityEngine;

public class CustomPartySetup : MonoBehaviour
{
	public static CustomPartySetup Instance;

	[SerializeField]
	private NewPartyDisplayUI NewPartyDisplayUI;

	[SerializeField]
	private GameObject LevelEditorMenu;

	[SerializeField]
	private CanvasGroup warningPanel;

	[SerializeField]
	private TextMeshProUGUI warningText;

	[SerializeField]
	private ExtendedButton startButton;

	[SerializeField]
	private ExtendedButton backButton;

	[NonSerialized]
	private CCustomLevelData m_CurrentCustomLevel;

	[NonSerialized]
	private PartyAdventureData m_CurrentCustomParty;

	public const string SandBoxScenarioID = "SandboxScenario";

	private bool showingForSandbox;

	private bool showingForCreateNew;

	private bool showingForCustomCreated;

	private ScenarioDefinition scenarioDefinition;

	private SkipFrameKeyActionHandlerBlocker skipFrameBlocker;

	public PartyAdventureData CurrentCustomParty => m_CurrentCustomParty;

	private void Awake()
	{
		Instance = this;
		ActivateKeyActionHandlers();
	}

	private void OnDestroy()
	{
		Instance = null;
		DeactivateKeyActionHandlers();
		NewPartyDisplayUI.OnOpenedPanel -= OnOpenedPanel;
	}

	private void Start()
	{
		NewPartyDisplayUI.OnOpenedPanel += OnOpenedPanel;
	}

	private void ActivateKeyActionHandlers()
	{
		skipFrameBlocker = new SkipFrameKeyActionHandlerBlocker(this);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnStartLevelPressed).AddBlocker(new ExtendedButtonActiveKeyActionHandlerBlocker(startButton)).AddBlocker(new ExtendedButtonInteractableKeyActionHandlerBlocker(startButton)));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnBackButtonPressed).AddBlocker(new ExtendedButtonActiveKeyActionHandlerBlocker(backButton)).AddBlocker(skipFrameBlocker));
	}

	private void DeactivateKeyActionHandlers()
	{
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnStartLevelPressed);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnBackButtonPressed);
	}

	private void OnOpenedPanel(bool selected, NewPartyDisplayUI.DisplayType display)
	{
		if (selected)
		{
			startButton.gameObject.SetActive(value: false);
			backButton.gameObject.SetActive(value: false);
		}
		else
		{
			startButton.gameObject.SetActive(value: true);
			backButton.gameObject.SetActive(value: true);
			skipFrameBlocker.Run();
		}
	}

	private void Update()
	{
		if (m_CurrentCustomParty.AdventureMapState.MapParty.SelectedCharacters.Count() < m_CurrentCustomParty.AdventureMapState.MinRequiredCharacters)
		{
			warningText.text = string.Format(LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_MIN_HEROES"), AdventureState.MapState.MinRequiredCharacters);
			warningPanel.alpha = 1f;
			startButton.interactable = false;
		}
		else
		{
			startButton.interactable = true;
			warningPanel.alpha = 0f;
		}
	}

	public void ShowForSandbox()
	{
		showingForSandbox = true;
		PartyAdventureData partyAdventureData = CreateAllUnlockedParty("SandboxScenario", EAdventureDifficulty.Normal, StateShared.EHouseRulesFlag.None, EGoldMode.PartyGold, EEnhancementMode.CharacterPersistent, EGameMode.SingleScenario);
		partyAdventureData.AdventureMapState.MinRequiredCharacters = 1;
		ShowForYML("SandboxScenario", partyAdventureData);
		SaveData.Instance.Global.ResumeSingleScenarioName = string.Empty;
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.CustomPartySetup);
		skipFrameBlocker.Run();
	}

	public void ShowForYML(string scenarioID, PartyAdventureData partyData)
	{
		scenarioDefinition = ScenarioRuleClient.SRLYML.GetScenarioDefinition(scenarioID);
		if (scenarioDefinition != null)
		{
			partyData.AdventureMapState.HeadquartersState.UnlockPartyUI(usingMap: false);
			partyData.AdventureMapState.TutorialCompleted = true;
			AdventureState.UpdateMapState(partyData.AdventureMapState);
			m_CurrentCustomParty = partyData;
			base.gameObject.SetActive(value: true);
			NewPartyDisplayUI.Init(partyData.AdventureMapState.MapParty);
		}
		else
		{
			Debug.LogError("Unable to find Scenario with ID " + scenarioID);
		}
	}

	public void ShowForCustomLevel(CCustomLevelData customLevel)
	{
		if (customLevel.ScenarioState == null)
		{
			scenarioDefinition = ScenarioRuleClient.SRLYML.GetScenarioDefinition(Path.GetFileNameWithoutExtension(customLevel.YMLFile));
		}
		showingForCreateNew = true;
		m_CurrentCustomLevel = customLevel;
		m_CurrentCustomParty = CreateEmptyParty();
		m_CurrentCustomParty.AdventureMapState.HeadquartersState.UnlockPartyUI(usingMap: false);
		m_CurrentCustomParty.AdventureMapState.TutorialCompleted = true;
		AdventureState.UpdateMapState(m_CurrentCustomParty.AdventureMapState);
		base.gameObject.SetActive(value: true);
		NewPartyDisplayUI.Init(m_CurrentCustomParty.AdventureMapState.MapParty);
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.CustomPartySetup);
	}

	public void ShowForCustomLevel(CCustomLevelData customLevel, PartyAdventureData partyData)
	{
		if (customLevel.ScenarioState == null)
		{
			scenarioDefinition = ScenarioRuleClient.SRLYML.GetScenarioDefinition(Path.GetFileNameWithoutExtension(customLevel.YMLFile));
		}
		showingForCustomCreated = true;
		m_CurrentCustomLevel = customLevel.DeepCopySerializableObject<CCustomLevelData>();
		m_CurrentCustomParty = partyData;
		partyData.Load(partyData.GameMode, isJoiningMPClient: false);
		AdventureState.UpdateMapState(partyData.AdventureMapState);
		base.gameObject.SetActive(value: true);
		NewPartyDisplayUI.Init(partyData.AdventureMapState.MapParty);
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.CustomPartySetup);
	}

	private PartyAdventureData CreateEmptyParty()
	{
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), "Custom Party", EAdventureDifficulty.Normal, StateShared.EHouseRulesFlag.None, new SaveOwner(), EGoldMode.PartyGold, DLCRegistry.EDLCKey.None, EEnhancementMode.CharacterPersistent, EGameMode.SingleScenario, SaveData.Instance.Global?.CurrentModdedRuleset);
		AdventureState.UpdateMapState(partyAdventureData.AdventureMapState);
		partyAdventureData.AdventureMapState.MapParty.ModifyPartyGold(1000, useGoldModifier: false);
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			if (GlobalData.CompletedCharacters.Contains(@class.CharacterID))
			{
				partyAdventureData.AdventureMapState.MapParty.UnlockedCharacterIDs.Add(@class.CharacterID);
				partyAdventureData.AdventureMapState.MapParty.AddCharacterToCharactersList(new CMapCharacter(@class.CharacterID, string.Empty, 0));
			}
		}
		foreach (CMapCharacter character in partyAdventureData.AdventureMapState.MapParty.CheckCharacters)
		{
			character.MaxLevel();
			character.OwnedAbilityCardIDs.AddRange(from s in CharacterClassManager.AllAbilityCards
				where s.ClassID == character.CharacterID && !character.OwnedAbilityCardIDs.Contains(s.ID)
				select s.ID);
		}
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			partyAdventureData.AdventureMapState.MapParty.AddItemToUnboundItems(itemCard.GetItem);
		}
		return partyAdventureData;
	}

	private PartyAdventureData CreateAllUnlockedParty(string partyName, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSetting, EGoldMode goldMode, EEnhancementMode enhancementMode, EGameMode gameMode)
	{
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), partyName, difficulty, houseRulesSetting, new SaveOwner(), goldMode, DLCRegistry.EDLCKey.None, enhancementMode, gameMode, SaveData.Instance.Global?.CurrentModdedRuleset);
		AdventureState.UpdateMapState(partyAdventureData.AdventureMapState);
		partyAdventureData.AdventureMapState.MapParty.ModifyPartyGold(10000, useGoldModifier: false);
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			partyAdventureData.AdventureMapState.MapParty.UnlockedCharacterIDs.Add(@class.CharacterID);
			partyAdventureData.AdventureMapState.MapParty.AddCharacterToCharactersList(new CMapCharacter(@class.CharacterID, string.Empty, 0));
		}
		foreach (CMapCharacter character in partyAdventureData.AdventureMapState.MapParty.CheckCharacters)
		{
			character.MaxLevel();
			character.OwnedAbilityCardIDs.AddRange(from s in CharacterClassManager.AllAbilityCards
				where s.ClassID == character.CharacterID && !character.OwnedAbilityCardIDs.Contains(s.ID)
				select s.ID);
		}
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			partyAdventureData.AdventureMapState.MapParty.AddItemToUnboundItems(itemCard.GetItem);
		}
		return partyAdventureData;
	}

	public void OnStartLevelPressed()
	{
		if (m_CurrentCustomParty.AdventureMapState.MapParty.SelectedCharacters.Count() < m_CurrentCustomParty.AdventureMapState.MinRequiredCharacters)
		{
			return;
		}
		if (showingForCustomCreated)
		{
			LevelEditorController.s_Instance.SetLevelEditorState(LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty);
		}
		if (scenarioDefinition != null)
		{
			List<Tuple<ScenarioMessage, string>> roomRevealedMessageToMapGuid;
			if (m_CurrentCustomLevel == null)
			{
				m_CurrentCustomLevel = new CCustomLevelData(scenarioDefinition.ID, CMapScenarioState.CreateNewScenario(scenarioDefinition, SharedClient.GlobalRNG.Next(), m_CurrentCustomParty.AdventureMapState.MapParty.PartyLevel, scenarioDefinition.FileName, scenarioDefinition.ID, (scenarioDefinition.Description != string.Empty) ? LocalizationManager.GetTranslation(scenarioDefinition.Description) : string.Empty, 4, new SharedLibrary.Random(), out var debugOutput, out roomRevealedMessageToMapGuid, m_CurrentCustomParty.AdventureMapState.MapParty.ThreatLevel));
				Debug.Log("Generating Custom YML scenario:\n" + debugOutput);
				scenarioDefinition = null;
			}
			else if (m_CurrentCustomLevel.ScenarioState == null)
			{
				m_CurrentCustomLevel.ScenarioState = CMapScenarioState.CreateNewScenario(scenarioDefinition, SharedClient.GlobalRNG.Next(), m_CurrentCustomParty.AdventureMapState.MapParty.PartyLevel, scenarioDefinition.FileName, LocalizationManager.GetTranslation(scenarioDefinition.ID), (scenarioDefinition.Description != string.Empty) ? LocalizationManager.GetTranslation(scenarioDefinition.Description) : string.Empty, 4, new SharedLibrary.Random(), out var _, out roomRevealedMessageToMapGuid, m_CurrentCustomParty.AdventureMapState.MapParty.ThreatLevel);
			}
		}
		SaveData.Instance.StartNewCustomLevel(m_CurrentCustomLevel, m_CurrentCustomParty);
		SceneController.Instance.LoadSingleScenario(m_CurrentCustomParty);
	}

	public void OnBackButtonPressed()
	{
		if (showingForSandbox)
		{
			base.gameObject.SetActive(value: false);
			showingForSandbox = false;
			NewPartyDisplayUI.Hide(NewPartyDisplayUI, instant: false, delegate
			{
				SceneController.Instance.YML.ReturnToMainMenu();
				MainMenuUIManager.Instance.mainMenu.Show();
				Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MainOptions);
			});
		}
		else if (showingForCreateNew)
		{
			showingForCreateNew = false;
			NewPartyDisplayUI.Hide(NewPartyDisplayUI, instant: false, delegate
			{
				base.gameObject.SetActive(value: false);
				LevelEditorMenu.SetActive(value: true);
			});
		}
		else
		{
			if (showingForCustomCreated)
			{
				showingForCustomCreated = false;
			}
			NewPartyDisplayUI.Hide(NewPartyDisplayUI, instant: false, delegate
			{
				base.gameObject.SetActive(value: false);
			});
		}
	}
}
