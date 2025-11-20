#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using Script.Controller;
using SharedLibrary.Client;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UnityGameEditorRuntime
{
	public static Vector3 s_TileSize;

	public static bool s_DisplayBasicTilesAtRuntime = false;

	public static string s_ScenarioOrProcGenKey = "1";

	public static bool s_Initialised = false;

	private static Scene currentlyLoadedScene;

	public static void Initialise()
	{
		if (s_Initialised)
		{
			return;
		}
		try
		{
			Vector3 vector = Vector3.zero;
			GameObject gameObject = Resources.Load<GameObject>("Hex");
			if (!(gameObject != null))
			{
				return;
			}
			BoxCollider component = gameObject.GetComponent<BoxCollider>();
			if (component != null)
			{
				vector = component.size;
			}
			if (vector == Vector3.zero)
			{
				Renderer component2 = gameObject.GetComponent<Renderer>();
				if (component2 != null)
				{
					vector = component2.bounds.size;
				}
			}
			if (vector == Vector3.zero)
			{
				throw new Exception("Missing hex reference asset");
			}
			s_TileSize.x = vector.x;
			s_TileSize.z = vector.z * 0.75f;
			CActor.SetLOSTileScalar(s_TileSize.x, s_TileSize.z);
			s_Initialised = true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while initialising UnityGameEditorRuntime\n" + ex.Message + "\n" + ex.StackTrace);
			if (Application.isPlaying)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_RUNTIME_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, SceneController.Instance.ExitApplication, ex.Message);
			}
		}
	}

	public static void LoadScenario(string sceneName)
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
	}

	public static void LoadScenario(ScenarioState state, CMapParty party = null)
	{
		LoadScenario(state, delegate(ScenarioState stateCloneToPrepare)
		{
			if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor && stateCloneToPrepare.Players.Count == 0)
			{
				if (party == null)
				{
					throw new Exception("No players were added to the scenario being loaded!");
				}
				AddPlayersToState(party, stateCloneToPrepare);
			}
		});
	}

	public static void LoadScenario(ScenarioState scenarioState, UnityAction<ScenarioState> preSceneLoadOperation)
	{
		try
		{
			FFSNet.Console.LogInfo("Seed: " + scenarioState.Seed);
			SimpleLog.AddToSimpleLog("RNG STATES (Load Scenario): \nScenarioRNG:" + (scenarioState.ScenarioRNGNotNull ? scenarioState.PeekScenarioRNG.ToString() : "NULL") + "\nEnemyIDRNG:" + ((scenarioState.EnemyIDRNG != null) ? scenarioState.PeekEnemyIDRNG.ToString() : "NULL") + "\nEnemyAbilityCardRNG:" + ((scenarioState.EnemyAbilityCardRNG != null) ? scenarioState.PeekEnemyAbilityCardRNG.ToString() : "NULL") + "\nGuidRNG:" + ((scenarioState.GuidRNG != null) ? scenarioState.PeekGuidRNG.ToString() : "NULL"));
			if (SaveData.Instance.Global.GameMode != EGameMode.None)
			{
				ScenarioState scenarioState2 = scenarioState.DeepCopySerializableObject<ScenarioState>();
				preSceneLoadOperation?.Invoke(scenarioState2);
				Choreographer.s_Choreographer.LoadProcGenScene(scenarioState2, !scenarioState2.IsInitialised);
			}
			else
			{
				Debug.LogError("Game mode has not been set.  Unable to load into level");
				SceneController.Instance.ReloadMainMenu = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while loading a Scenario\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_RUNTIME_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private static void AddPlayersToState(CMapParty party, ScenarioState state)
	{
		foreach (CMapCharacter cData in party.SelectedCharacters)
		{
			CCharacterClass playerClass = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == cData.CharacterID);
			PlayerState item = new PlayerState(cData.CharacterID, 0, null, state.Maps[0].MapGuid, null, cData.MaxHealth, cData.MaxHealth, cData.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: false, CActor.ECauseOfDeath.StillAlive, 1, cData.CharacterName, 0, 0, isLongResting: false, new AbilityDeckState((from c in cData.HandAbilityCards.Select((CAbilityCard c) => new { c.ID, c.CardInstanceID }).AsEnumerable()
				select new Tuple<int, int>(c.ID, c.CardInstanceID)).ToList()), new AttackModifierDeckState(playerClass), cData.CheckEquippedItems)
			{
				IsRevealed = true
			};
			state.Players.Add(item);
		}
	}

	public static string GenerateRoomLog(ScenarioState state)
	{
		string text = string.Empty;
		try
		{
			foreach (CMap mapInput in state.Maps)
			{
				text = text + "Room: " + mapInput.ScenarioRoomName + " (" + mapInput.RoomName + ")\n  Map Type: " + mapInput.MapType.ToString() + "\n  Parent: " + ((mapInput.ParentName == string.Empty) ? "None" : mapInput.ParentName) + "\n";
				foreach (EnemyState monster in from w in state.Monsters.Distinct()
					where w.StartingMapGuid == mapInput.MapGuid
					select w)
				{
					text = text + "    Monster: " + state.Monsters.Where((EnemyState s) => s.StartingMapGuid == mapInput.MapGuid && s.ClassID == monster.ClassID).Count() + " X " + monster.ClassID + "\n";
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while generating debug log\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return text;
	}

	public static IEnumerator UnloadScenario()
	{
		if (currentlyLoadedScene.IsValid() && currentlyLoadedScene.isLoaded)
		{
			InputManager.RequestDisableInput(typeof(UnityGameEditorRuntime), EKeyActionTag.All);
			ObjectPool.RecyclePendingObjects();
			CameraController.s_CameraController.SetCamera(null);
			AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentlyLoadedScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
			WaitForEndOfFrame waitForEndOfFrameDelay = new WaitForEndOfFrame();
			while (!asyncUnload.isDone)
			{
				yield return waitForEndOfFrameDelay;
			}
			InputManager.RequestEnableInput(typeof(UnityGameEditorRuntime), EKeyActionTag.All);
		}
	}

	public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		InputManager.RequestDisableInput(typeof(UnityGameEditorRuntime), EKeyActionTag.All);
		ScenarioManager.CurrentScenarioState.LoadingScenarioState = true;
		SimpleLog.AddToSimpleLog("RNG STATES (start of scenario loaded callback): \nScenarioRNG:" + ScenarioManager.CurrentScenarioState.PeekScenarioRNG + "\nEnemyIDRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyIDRNG + "\nEnemyAbilityCardRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyAbilityCardRNG + "\nGuidRNG:" + ScenarioManager.CurrentScenarioState.PeekGuidRNG);
		currentlyLoadedScene = scene;
		bool generatedScenario = scene.name == "ProcGen";
		SceneManager.SetActiveScene(scene);
		CameraController.s_CameraController.m_Camera.gameObject.GetComponent<CreateLightProbes>().Generate();
		if (!generatedScenario)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
		if (SaveData.Instance.Global.CurrentlyPlayingCustomLevel || AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			if (SaveData.Instance.Global.CurrentlyPlayingCustomLevel && SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType == ELevelPartyChoiceType.LoadAdventureParty && SaveData.Instance.Global.CurrentCustomLevelData.EnemyLevelsScaleToPartyLevel)
			{
				int num = 1;
				if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
				{
					num = SaveData.Instance.Global.CampaignData.AdventureMapState.MapParty.InProgressScenarioLevel;
				}
				else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
				{
					num = SaveData.Instance.Global.AdventureData.AdventureMapState.MapParty.InProgressScenarioLevel;
				}
				else if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial)
				{
					num = SaveData.Instance.Global.SingleScenarioData.AdventureMapState.MapParty.ScenarioLevel;
				}
				else if (SaveData.Instance.Global.GameMode == EGameMode.Autotest)
				{
					num = (int)Math.Ceiling((float)SaveData.Instance.Global.CurrentCustomLevelData.ScenarioState.Players.Sum((PlayerState x) => x.Level) / (2f * (float)SaveData.Instance.Global.CurrentCustomLevelData.ScenarioState.Players.Count));
				}
				ScenarioManager.CurrentScenarioState?.SetScenarioLevel(num);
				MonsterClassManager.SetAllMonsterStatLevels(num, SaveData.Instance.Global.CurrentCustomLevelData.StatBasedOnXOverrideList);
				ScenarioManager.Scenario.EnemyMaxHealthBasedOnPartyLevel = SaveData.Instance.Global.CurrentCustomLevelData.EnemyMaxHealthBasedOnPartyLevel;
				ScenarioManager.Scenario.SetEnemyHealthToMaxOnPlay = SaveData.Instance.Global.CurrentCustomLevelData.SetEnemyHealthToMaxOnPlay;
			}
			else
			{
				ScenarioManager.CurrentScenarioState.UpdateMonsterClassesFromState(SaveData.Instance.Global.CurrentCustomLevelData.ScenarioState.Players.Count, SaveData.Instance.Global.CurrentCustomLevelData.StatBasedOnXOverrideList);
			}
		}
		else
		{
			switch (SaveData.Instance.Global.GameMode)
			{
			case EGameMode.Campaign:
				MonsterClassManager.SetAllMonsterStatLevels(SaveData.Instance.Global.CampaignData.AdventureMapState.MapParty.InProgressScenarioLevel);
				break;
			case EGameMode.Guildmaster:
				MonsterClassManager.SetAllMonsterStatLevels(SaveData.Instance.Global.AdventureData.AdventureMapState.MapParty.InProgressScenarioLevel);
				break;
			case EGameMode.LevelEditor:
				ScenarioManager.CurrentScenarioState.UpdateMonsterClassesFromState();
				break;
			}
		}
		bool applyMapData = AdventureState.MapState != null && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor && (SaveData.Instance.Global.CurrentCustomLevelData == null || (SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnSpecificLocations && SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnAtEntrance));
		if (applyMapData && AdventureState.MapState.MapParty.NextScenarioEffects != null)
		{
			if (AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers != null && AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers.Count() > 0)
			{
				foreach (Tuple<string, Dictionary<string, int>> modifierTuple in AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers)
				{
					if (modifierTuple.Item1 == "party" || modifierTuple.Item1 == "NoneID")
					{
						foreach (CMapCharacter cData in AdventureState.MapState.MapParty.SelectedCharacters)
						{
							CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == cData.CharacterID)?.ApplyEventModifiersToList(modifierTuple.Item2);
						}
						continue;
					}
					CMapCharacter cData2 = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == modifierTuple.Item1);
					if (cData2 != null)
					{
						CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == cData2.CharacterID)?.ApplyEventModifiersToList(modifierTuple.Item2);
					}
				}
				AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers.Clear();
			}
			if (AdventureState.MapState.MapParty.NextScenarioEffects.Discard > 0 && ScenarioManager.CurrentScenarioState.IsFirstLoad)
			{
				ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Discard_Cards_Scenario_Mod", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_DiscardCard_Self", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, AdventureState.MapState.MapParty.NextScenarioEffects.Discard, null, null, null, null, null, isHidden: true));
				AdventureState.MapState.MapParty.NextScenarioEffects.Discard = 0;
			}
			if (AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems != null && AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems.Count() > 0)
			{
				for (int num2 = 0; num2 < AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems.Count; num2++)
				{
					CObjectiveFilter cObjectiveFilter = null;
					string item = AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems[num2].Item1;
					cObjectiveFilter = ((!(item == "party") && !(item == "NoneID")) ? new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.ELootType.None, CAbility.EAbilityType.None, CAbilityFilter.EFilterRevealedType.None, new List<string> { item }) : new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player));
					string consumeSlotString = AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems[num2].Item2;
					if (CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == consumeSlotString) == CItem.EItemSlot.SmallItem)
					{
						ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Consume_Item_Cards_Scenario_Mod_" + num2, ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, cObjectiveFilter, isPositive: true, "ScenarioAbility_ConsumeSmallItem_Self", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, null, null, null, null, null, isHidden: true));
					}
					else
					{
						Debug.LogError("Invalid slot type for consuming items due to start scenario effect after a Road/City Event");
					}
				}
				AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems.Clear();
			}
		}
		PostProcessScenario(Choreographer.s_Choreographer.m_MapSceneRoot, Choreographer.s_Choreographer.m_ProcGenSeed);
		Choreographer.s_Choreographer.ScenarioCreateClientBoardAndCharacters(delegate
		{
			OnScenarioCreateClientBoardAndCharactersDone(applyMapData, generatedScenario);
		});
	}

	private static void OnScenarioCreateClientBoardAndCharactersDone(bool applyMapData, bool generatedScenario)
	{
		if (applyMapData)
		{
			Choreographer.s_Choreographer.m_InitialProgressAchievements = (from it in AdventureState.MapState.MapParty.Achievements
				where it.State == EAchievementState.Unlocked
				select new ScenarioAchievementProgress(it)).ToList();
			foreach (CMapCharacter mapCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
				{
					CharacterClassManager.SetPerks(mapCharacter.Perks.Where((CharacterPerk w) => w.IsActive).ToList());
					CharacterClassManager.SetPerksComplete(mapCharacter.CharacterID);
				}
				else
				{
					CharacterClassManager.AddPerks(mapCharacter.Perks.Where((CharacterPerk w) => w.IsActive).ToList());
				}
				if (SaveData.Instance.Global.GameMode != EGameMode.SingleScenario && SaveData.Instance.Global.GameMode != EGameMode.FrontEndTutorial && ScenarioManager.CurrentScenarioState.IsFirstLoad)
				{
					mapCharacter.ApplyMapConditions();
				}
				mapCharacter.SetActorPersonalQuest();
				if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
				{
					CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == mapCharacter.CharacterID).ResetCardAbilitiesAndEnhancementsOnly();
					mapCharacter.ApplyEnhancements();
				}
				if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
				{
					PlayerState playerState = ScenarioManager.CurrentScenarioState.Players.SingleOrDefault((PlayerState p) => p.ClassID == mapCharacter.CharacterID);
					mapCharacter.ApplyPassiveItems(playerState);
				}
			}
			if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
			{
				while (AdventureState.MapState.MapParty.SelectedCharacters.Any(delegate(CMapCharacter x)
				{
					List<PositiveConditionPair> tempBlessConditions = x.TempBlessConditions;
					return tempBlessConditions != null && tempBlessConditions.Count > 0;
				}))
				{
					foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
					{
						selectedCharacter.ApplyMapBlessCondition();
					}
				}
			}
		}
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				PlayerState playerState2 = ScenarioManager.CurrentScenarioState.Players.SingleOrDefault((PlayerState p) => p.ClassID == playerActor.CharacterClass.ID);
				if (playerState2 != null)
				{
					playerActor.ActivatePassiveItems(firstLoad: true, playerState2);
				}
			}
		}
		Choreographer.s_Choreographer.ScenarioImportProcessedCallback(generatedScenario);
		if (applyMapData && AdventureState.MapState.MapParty.NextScenarioEffects != null)
		{
			foreach (ElementInfusionBoardManager.EElement infusion in AdventureState.MapState.MapParty.NextScenarioEffects.Infusions)
			{
				ElementInfusionBoardManager.Infuse(infusion, null);
				ElementInfusionBoardManager.EndTurn();
			}
			AdventureState.MapState.MapParty.NextScenarioEffects.Infusions.Clear();
			if (AdventureState.MapState.MapParty.NextScenarioEffects.Damage > 0 && ScenarioManager.CurrentScenarioState.IsFirstLoad)
			{
				foreach (CMapCharacter selectedCharacter2 in AdventureState.MapState.MapParty.SelectedCharacters)
				{
					selectedCharacter2.ApplyMapDamage(AdventureState.MapState.MapParty.NextScenarioEffects.Damage);
				}
				AdventureState.MapState.MapParty.NextScenarioEffects.Damage = 0;
			}
			ApplyNextScenarioEnemyConditions(AdventureState.MapState.MapParty.NextScenarioEffects.EnemyNegativeConditions, AdventureState.MapState.MapParty.NextScenarioEffects.EnemyPositiveConditions);
		}
		ScenarioState currentState = Choreographer.s_Choreographer.m_CurrentState;
		List<CBattleGoalState> list = null;
		if (AdventureState.MapState != null && AdventureState.MapState.InProgressQuestState != null)
		{
			list = AdventureState.MapState.InProgressQuestState.ChosenBattleGoalStates.Select((Tuple<string, CBattleGoalState> x) => x.Item2).ToList();
			if (Choreographer.s_Choreographer.m_CurrentState.RoundNumber == 1)
			{
				Debug.Log("[UnityGameEditorRuntime] update battle goals before 1st round to avoid some battle goals incorrect state after retry");
				foreach (CBattleGoalState item in list)
				{
					item.BattleGoalConditionState.ResetProgress();
					item.BattleGoalConditionState.Failed = false;
				}
			}
		}
		UIManager.Instance.InitScenario(currentState.ID, currentState.WinObjectives, currentState.LoseObjectives, currentState.ScenarioModifiers, list);
		TransitionManager.s_Instance?.FadeInLevel();
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: true);
		}
		SimpleLog.AddToSimpleLog("RNG STATES (end of scenario loaded callback): \nScenarioRNG:" + ScenarioManager.CurrentScenarioState.PeekScenarioRNG + "\nEnemyIDRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyIDRNG + "\nEnemyAbilityCardRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyAbilityCardRNG + "\nGuidRNG:" + ScenarioManager.CurrentScenarioState.PeekGuidRNG);
		ScenarioManager.CurrentScenarioState.LoadingScenarioState = false;
		SceneController.Instance.ScenarioIsLoading = true;
		CoroutineHelper.RunCoroutine(WaitForLoad());
	}

	private static void ApplyNextScenarioEnemyConditions(List<NegativeConditionPair> enemyNegativeConditions, List<PositiveConditionPair> enemyPositiveConditions)
	{
		if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario || (enemyNegativeConditions.IsNullOrEmpty() && enemyNegativeConditions.IsNullOrEmpty()))
		{
			return;
		}
		bool flag = true;
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			if (!enemyNegativeConditions.IsNullOrEmpty())
			{
				foreach (NegativeConditionPair enemyNegativeCondition in enemyNegativeConditions)
				{
					if (flag || enemyNegativeCondition.NegativeCondition != CCondition.ENegativeCondition.Curse)
					{
						enemy.ApplyCondition(enemy, enemyNegativeCondition.NegativeCondition, enemyNegativeCondition.RoundDuration, EConditionDecTrigger.Rounds, "", isMapCondition: true);
					}
				}
			}
			if (!enemyPositiveConditions.IsNullOrEmpty())
			{
				foreach (PositiveConditionPair enemyPositiveCondition in enemyPositiveConditions)
				{
					if (flag || enemyPositiveCondition.PositiveCondition != CCondition.EPositiveCondition.Bless)
					{
						enemy.ApplyCondition(enemy, enemyPositiveCondition.PositiveCondition, enemyPositiveCondition.RoundDuration, EConditionDecTrigger.Rounds, "", isMapCondition: true);
					}
				}
			}
			flag = false;
		}
		enemyNegativeConditions?.Clear();
		enemyPositiveConditions?.Clear();
	}

	public static IEnumerator WaitForLoad()
	{
		InputManager.RequestDisableInput(typeof(UnityGameEditorRuntime), EKeyActionTag.All);
		if (FFSNetwork.IsOnline && PlayerRegistry.WaitForOtherPlayers && PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant)
		{
			Synchronizer.SendSideAction(GameActionType.NotifyLoadingFinished);
			PlayerRegistry.NotifyLoadingFinished(PlayerRegistry.MyPlayer);
			if (PlayerRegistry.WaitForOtherPlayers)
			{
				yield return SceneController.Instance.WaitForPlayers();
			}
		}
		yield return null;
		SceneController.Instance.ScenarioIsLoading = false;
		yield return null;
		yield return WaitForProcGen();
		if (FFSNetwork.IsOnline && PlayerRegistry.WaitForOtherPlayersFullLoaded && PlayerRegistry.MyPlayer != null)
		{
			Synchronizer.SendSideAction(GameActionType.NotifyFullLoadingFinished);
			PlayerRegistry.NotifyFullLoadingFinished(PlayerRegistry.MyPlayer);
			if (PlayerRegistry.WaitForOtherPlayersFullLoaded)
			{
				yield return PlayerRegistry.WaitForAllPlayersFullLoaded();
			}
		}
		SceneController.Instance.DisableLoadingScreen();
		InputManager.RequestEnableInput(typeof(UnityGameEditorRuntime), EKeyActionTag.All);
	}

	private static IEnumerator WaitForProcGen()
	{
		ProceduralContentPlatformData proceduralContentPlatformData = PlatformLayer.Setting.ProceduralContentPlatformData;
		if (proceduralContentPlatformData._isWaitForGeneration)
		{
			GameObject maps = RoomVisibilityManager.s_Instance.Maps;
			ProceduralPlacementNotifierHandler component = maps.GetComponent<ProceduralPlacementNotifierHandler>();
			component.UpdateProceduralCache(maps);
			yield return component.WaitForPlacementCompletedAll(proceduralContentPlatformData._maxFramesToWaitContentCompleteness);
		}
	}

	public static void PostProcessScenario(GameObject scenarioRoot, int apparanceSeed)
	{
		foreach (UnityGameEditorObject unityGameEditorObject in Singleton<ObjectCacheService>.Instance.GetUnityGameEditorObjects())
		{
			if (unityGameEditorObject != null && unityGameEditorObject.m_InvisableAtRuntime)
			{
				MeshRenderer component = unityGameEditorObject.GetComponent<MeshRenderer>();
				if (component != null)
				{
					component.enabled = s_DisplayBasicTilesAtRuntime;
				}
			}
		}
		ProceduralScenario component2 = scenarioRoot.GetComponent<ProceduralScenario>();
		component2.ScenarioSeed = apparanceSeed;
		foreach (CObjectDoor item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>())
		{
			UnityGameEditorDoorProp component3 = Singleton<ObjectCacheService>.Instance.GetPropObject(item).GetComponent<UnityGameEditorDoorProp>();
			if (item.LockType == CObjectDoor.ELockType.None || item.DoorIsLocked)
			{
				component3.m_LockType = item.LockType;
			}
		}
		if (component2 != null)
		{
			try
			{
				component2.SetupWalls();
			}
			catch (Exception ex)
			{
				Debug.LogError("An error occurred processing the walls within Apparance.\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_RUNTIME_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		if (!LevelEditorController.s_Instance.IsEditing && !LevelEditorController.s_Instance.IsPreviewingLevel && !SaveData.Instance.Global.CurrentlyPlayingCustomLevel)
		{
			return;
		}
		CCustomLevelData cCustomLevelData = SaveData.Instance.Global.CurrentCustomLevelData ?? SaveData.Instance.Global.CurrentEditorLevelData;
		if (!cCustomLevelData.HasApparanceOverrides)
		{
			return;
		}
		CApparanceOverrideDetails apparanceOverrideDetailsForGUID = cCustomLevelData.GetApparanceOverrideDetailsForGUID("ScenarioApparanceOverrideGUID");
		if (apparanceOverrideDetailsForGUID != null && scenarioRoot != null)
		{
			scenarioRoot.GetComponent<ProceduralStyle>()?.InitialiseWithOverride(apparanceOverrideDetailsForGUID);
		}
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			GameObject gameObject = scenarioRoot.FindInChildren(map.MapInstanceName, includeInactive: true);
			if (!(gameObject != null))
			{
				continue;
			}
			CApparanceOverrideDetails apparanceOverrideDetailsForGUID2 = cCustomLevelData.GetApparanceOverrideDetailsForGUID(map.MapGuid);
			if (apparanceOverrideDetailsForGUID2 != null)
			{
				gameObject.GetComponent<ProceduralStyle>()?.InitialiseWithOverride(apparanceOverrideDetailsForGUID2);
			}
			ProceduralWall[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralWall>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				CApparanceOverrideDetails apparanceOverrideDetailsForGUID3 = cCustomLevelData.GetApparanceOverrideDetailsForGUID(CApparanceOverrideDetails.GetWallGUID(map, i));
				if (apparanceOverrideDetailsForGUID3 != null)
				{
					componentsInChildren[i].GetComponent<ProceduralStyle>()?.InitialiseWithOverride(apparanceOverrideDetailsForGUID3);
				}
			}
		}
		foreach (CObjectDoor item2 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>())
		{
			CApparanceOverrideDetails apparanceOverrideDetailsForGUID4 = cCustomLevelData.GetApparanceOverrideDetailsForGUID(item2.PropGuid);
			if (apparanceOverrideDetailsForGUID4 == null)
			{
				continue;
			}
			GameObject gameObject2 = scenarioRoot.FindInChildren(item2.InstanceName);
			if (gameObject2 != null)
			{
				ProceduralStyle[] componentsInChildren2 = gameObject2.GetComponentsInChildren<ProceduralStyle>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					componentsInChildren2[j]?.InitialiseWithOverride(apparanceOverrideDetailsForGUID4);
				}
			}
			for (int k = 0; k < ApparanceLayer.Instance.DoorInstances.Count; k++)
			{
				if (ApparanceLayer.Instance.DoorInstances[k].name == item2.InstanceName)
				{
					ProceduralStyle[] componentsInChildren3 = ApparanceLayer.Instance.DoorInstances[k].GetComponentsInChildren<ProceduralStyle>();
					for (int l = 0; l < componentsInChildren3.Length; l++)
					{
						componentsInChildren3[k]?.InitialiseWithOverride(apparanceOverrideDetailsForGUID4);
					}
				}
			}
		}
	}

	public static List<GameObject> FindUnityGameObjects(GameObject rootGameObject, ScenarioManager.ObjectImportType objectType, bool findEmptyTiles = false)
	{
		List<GameObject> list = new List<GameObject>();
		if (!findEmptyTiles)
		{
			UnityGameEditorObject[] componentsInChildren = rootGameObject.GetComponentsInChildren<UnityGameEditorObject>(includeInactive: true);
			foreach (UnityGameEditorObject unityGameEditorObject in componentsInChildren)
			{
				if (unityGameEditorObject.m_ObjectType == objectType)
				{
					list.Add(unityGameEditorObject.gameObject);
				}
			}
		}
		else
		{
			foreach (Transform item in rootGameObject.transform)
			{
				UnityGameEditorObject component = item.gameObject.GetComponent<UnityGameEditorObject>();
				if (component != null && component.m_ObjectType == objectType)
				{
					if (component.m_ObjectType == ScenarioManager.ObjectImportType.Tile && component.m_ProcGenGameObjectsOnTile.Count > 0)
					{
						continue;
					}
					list.Add(item.gameObject);
				}
				list.AddRange(FindUnityGameObjects(item.gameObject, objectType));
			}
		}
		return list;
	}

	public static void GetAllChildren(Transform current, List<GameObject> arrayToFill)
	{
		arrayToFill.Add(current.gameObject);
		for (int i = 0; i < current.childCount; i++)
		{
			GetAllChildren(current.GetChild(i), arrayToFill);
		}
	}

	public static List<GameObject> GetAllSceneGameObjects(Scene scene)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			GetAllChildren(rootGameObjects[i].transform, list);
		}
		return list;
	}

	public static void ErrorHandlingUnloadSceneAndLoadMainMenu()
	{
		Debug.LogGUI("Error ErrorHandlingUnloadSceneAndLoadMainMenu");
		SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = true;
		UIManager.LoadMainMenu(delegate
		{
			SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = false;
		});
	}

	public static void ErrorHandlingUnloadSceneResetScenarioStateAndRetryLoad()
	{
		SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = true;
		UIManager.RegenerateAndRestartScenario(delegate
		{
			SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = false;
		});
	}

	public static void ErrorHandlingUnloadSceneResetQuestStateAndReturnToMap()
	{
		SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = true;
		Choreographer.s_Choreographer.EndGame(EResult.Resign, delegate
		{
			SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = false;
		});
	}

	public static void GetObjectsInScenario(Scene scene, ScenarioState scenarioState)
	{
		new List<CObjectProp>();
		foreach (GameObject allSceneGameObject in GetAllSceneGameObjects(scene))
		{
			if (allSceneGameObject.activeInHierarchy)
			{
				UnityGameEditorObject component = allSceneGameObject.GetComponent<UnityGameEditorObject>();
				if (component != null)
				{
					_ = component.m_ExcludeFromExport;
				}
			}
		}
	}

	public static void InitialiseScenario(ScenarioState scenarioState)
	{
		try
		{
			GameObject maps = RoomVisibilityManager.s_Instance.Maps;
			if (scenarioState.IsInitialised)
			{
				foreach (Transform mapTransform in maps.transform)
				{
					mapTransform.GetComponent<ApparanceMap>();
					CMap cMap = scenarioState.Maps.Single((CMap s) => s.MapInstanceName == mapTransform.name);
					foreach (Transform item in mapTransform.transform)
					{
						GameObject tileGO = item.gameObject;
						UnityGameEditorObject component = tileGO.GetComponent<UnityGameEditorObject>();
						if (component != null && (component.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
						{
							Vector3Int intPositiveSpaceSnapSpace = MF.GetTileIntegerSnapSpace(tileGO.transform.position) + new Vector3Int(scenarioState.PositiveSpaceOffset.X, scenarioState.PositiveSpaceOffset.Y, scenarioState.PositiveSpaceOffset.Z);
							try
							{
								CMapTile cMapTile = cMap.MapTiles.Single((CMapTile s) => s.ArrayIndex.X == intPositiveSpaceSnapSpace.x && s.ArrayIndex.Y == intPositiveSpaceSnapSpace.z && Vector3.Distance(GloomUtility.CVToV(s.Position), tileGO.transform.position) < 0.1f);
								tileGO.name = cMapTile.TileGuid;
								Singleton<ObjectCacheService>.Instance.AddTile(cMapTile, item.gameObject);
							}
							catch
							{
								Debug.LogError("Error overlapping tile at: " + intPositiveSpaceSnapSpace.x + " " + intPositiveSpaceSnapSpace.z);
							}
						}
					}
				}
				return;
			}
			Vector3Int vector3Int = new Vector3Int(int.MaxValue, 0, int.MaxValue);
			Vector3Int vector3Int2 = new Vector3Int(int.MinValue, 0, int.MinValue);
			Vector3Int zero = Vector3Int.zero;
			Vector3Int zero2 = Vector3Int.zero;
			bool flag = false;
			foreach (Transform item2 in maps.transform)
			{
				foreach (Transform item3 in item2.transform)
				{
					UnityGameEditorObject component2 = item3.gameObject.GetComponent<UnityGameEditorObject>();
					if (component2 != null && (component2.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component2.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
					{
						Vector3Int tileIntegerSnapSpace = MF.GetTileIntegerSnapSpace(item3.position);
						if (vector3Int.z > tileIntegerSnapSpace.z)
						{
							flag = (Mathf.Abs(tileIntegerSnapSpace.z) & 1) == 1;
						}
						vector3Int.x = ((vector3Int.x > tileIntegerSnapSpace.x) ? tileIntegerSnapSpace.x : vector3Int.x);
						vector3Int.z = ((vector3Int.z > tileIntegerSnapSpace.z) ? tileIntegerSnapSpace.z : vector3Int.z);
						vector3Int2.x = ((vector3Int2.x < tileIntegerSnapSpace.x) ? tileIntegerSnapSpace.x : vector3Int2.x);
						vector3Int2.z = ((vector3Int2.z < tileIntegerSnapSpace.z) ? tileIntegerSnapSpace.z : vector3Int2.z);
					}
				}
			}
			zero2 = new Vector3Int(-vector3Int.x + 1, 0, -vector3Int.z);
			if (flag)
			{
				zero2.z++;
			}
			zero = new Vector3Int(vector3Int2.x - vector3Int.x + 1, 0, vector3Int2.z - vector3Int.z + 1);
			int num = 0;
			foreach (Transform item4 in maps.transform)
			{
				ApparanceMap mapInfo = item4.gameObject.GetComponent<ApparanceMap>();
				CMap cMap2 = scenarioState.Maps.Single((CMap s) => s.MapGuid == mapInfo.MapGuid);
				item4.name = cMap2.MapInstanceName;
				foreach (Transform item5 in item4.transform)
				{
					UnityGameEditorObject component3 = item5.gameObject.GetComponent<UnityGameEditorObject>();
					if (component3 != null && (component3.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component3.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
					{
						Vector3Int vector3Int3 = MF.GetTileIntegerSnapSpace(item5.position) + zero2;
						CMapTile cMapTile2 = new CMapTile(rng: (AdventureState.MapState != null && AdventureState.MapState.MapRNG != null) ? AdventureState.MapState.MapRNG : ((ScenarioManager.CurrentScenarioState == null || ScenarioManager.CurrentScenarioState.GuidRNG == null) ? SharedClient.GlobalRNG : ScenarioManager.CurrentScenarioState.GuidRNG), edgeTile: component3.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile, arrayIndex: new TileIndex(vector3Int3.x, vector3Int3.z), position: GloomUtility.VToCV(item5.position));
						item5.name = cMapTile2.TileGuid;
						Singleton<ObjectCacheService>.Instance.AddTile(cMapTile2, item5.gameObject);
						cMap2.MapTiles.Add(cMapTile2);
					}
				}
				num++;
			}
			SimpleLog.AddToSimpleLog("MapRNG (initialize scenario): " + AdventureState.MapState.PeekMapRNG);
			CVectorInt3 positiveSpaceOffset = new CVectorInt3(zero2.x, zero2.y, zero2.z);
			scenarioState.InitScenarioState(zero.x, zero.z, positiveSpaceOffset);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while Initialising Scenario\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_RUNTIME_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static void GenerateStateFromScene(ScenarioState scenarioState)
	{
	}

	public static void MakeDoor(GameObject doorGO, ScenarioState state, bool firstLoad, bool levelEditorLevel = false, ApparanceMap mapInfo = null)
	{
		UnityGameEditorDoorProp component = doorGO.GetComponent<UnityGameEditorDoorProp>();
		if (mapInfo == null)
		{
			mapInfo = doorGO.GetComponentInParent<ApparanceMap>();
		}
		CMap cMap = state.Maps.Single((CMap s) => s.MapGuid == mapInfo.MapGuid);
		if (firstLoad)
		{
			UnityGameEditorObject component2 = doorGO.GetComponent<UnityGameEditorObject>();
			Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(doorGO.transform.position) + new Vector3Int(state.PositiveSpaceOffset.X, state.PositiveSpaceOffset.Y, state.PositiveSpaceOffset.Z);
			FindUnityGameObjects(Singleton<ObjectCacheService>.Instance.GetMap(cMap), ScenarioManager.ObjectImportType.EdgeTile).Find((GameObject x) => x.transform.position == doorGO.transform.position);
			CObjectDoor cObjectDoor = new CObjectDoor(component2.m_PrefabName, component2.m_ObjectType, new TileIndex(vector3Int.x, vector3Int.z), new CVector3(doorGO.transform.position.x, doorGO.transform.position.y, doorGO.transform.position.z), new CVector3(doorGO.transform.eulerAngles.x, doorGO.transform.eulerAngles.y, doorGO.transform.eulerAngles.z), cMap.MapGuid, component.m_DoorType, component.m_IsDungeonEntrance, component.m_IsDungeonExit);
			doorGO.name = cObjectDoor.InstanceName;
			Singleton<ObjectCacheService>.Instance.AddProp(cObjectDoor, doorGO);
			if (component.m_IsDungeonEntrance)
			{
				cMap.DungeonEntranceDoor = cObjectDoor.InstanceName;
			}
			else if (component.m_IsDungeonExit)
			{
				cMap.DungeonExitDoor = cObjectDoor.InstanceName;
			}
			else if (!levelEditorLevel || cMap.EntranceDoor == null)
			{
				cMap.EntranceDoor = cObjectDoor.InstanceName;
			}
			else
			{
				cMap.ExitDoors.Add(cObjectDoor.InstanceName);
			}
			state.Props.Add(cObjectDoor);
		}
		else
		{
			cMap.Props.Single((CObjectProp s) => s.InstanceName == doorGO.name);
		}
		if (false)
		{
			return;
		}
		if (!component.m_IsDungeonEntrance)
		{
			Renderer[] componentsInChildren = doorGO.GetComponentsInChildren<Renderer>();
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				componentsInChildren[num].enabled = false;
			}
		}
		doorGO.GetComponentInParent<RoomVisibilityTracker>()?.AddProp(doorGO);
	}

	public static CObjectDoor MakeDoorLevelEditor(GameObject doorGO, ScenarioState state)
	{
		UnityGameEditorDoorProp component = doorGO.GetComponent<UnityGameEditorDoorProp>();
		ApparanceMap mapInfo = doorGO.GetComponentInParent<ApparanceMap>();
		CMap cMap = state.Maps.Single((CMap s) => s.MapGuid == mapInfo.MapGuid);
		UnityGameEditorObject component2 = doorGO.GetComponent<UnityGameEditorObject>();
		Vector3Int tileIntegerSnapSpace = MF.GetTileIntegerSnapSpace(doorGO.transform.position);
		FindUnityGameObjects(Singleton<ObjectCacheService>.Instance.GetMap(cMap), ScenarioManager.ObjectImportType.EdgeTile).Find((GameObject x) => x.transform.position == doorGO.transform.position);
		CObjectDoor cObjectDoor = new CObjectDoor(component2.m_PrefabName, component2.m_ObjectType, new TileIndex(tileIntegerSnapSpace.x, tileIntegerSnapSpace.z), new CVector3(doorGO.transform.position.x, doorGO.transform.position.y, doorGO.transform.position.z), new CVector3(doorGO.transform.localEulerAngles.x, doorGO.transform.localEulerAngles.y, doorGO.transform.localEulerAngles.z), cMap.MapGuid, component.m_DoorType, component.m_IsDungeonEntrance, component.m_IsDungeonExit);
		doorGO.name = cObjectDoor.InstanceName;
		Singleton<ObjectCacheService>.Instance.AddProp(cObjectDoor, doorGO);
		state.Props.Add(cObjectDoor);
		return cObjectDoor;
	}
}
