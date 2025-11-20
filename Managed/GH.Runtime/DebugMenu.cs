#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using BeautifyEffect;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.BattleGoals;
using MapRuleLibrary.YML.Events;
using MapRuleLibrary.YML.PersonalQuests;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class DebugMenu : MonoBehaviour
{
	public enum CaptureTileFunctions
	{
		None,
		SpawnMonster,
		SelectCharacter,
		SpawnProp,
		SpawnSpawner,
		SpawnObject,
		LOSSourceTile,
		LOSTargetTile,
		SetPosition
	}

	private class DisplayText
	{
		private float startTime;

		private Rect area;

		public bool Show { get; set; }

		public float ElapsedTime => Time.realtimeSinceStartup - startTime;

		public int ID { get; private set; }

		public int Position { get; private set; }

		public string Text { get; private set; }

		public Rect Area => area;

		public DisplayText(int id, string text)
		{
			ID = id;
			Text = text;
			Show = true;
			startTime = Time.realtimeSinceStartup;
			area = new Rect(Screen.width - 400, 20f, 300f, 20f);
		}

		public void UpdateText(string text)
		{
			Text = text;
			startTime = Time.realtimeSinceStartup;
		}

		public void UpdatePosition(int position)
		{
			Position = position;
			area.y = position * 20;
		}
	}

	public class DisplayTextList
	{
		private List<DisplayText> displayTexts;

		public float DisplayTextTime { get; set; }

		public DisplayTextList()
		{
			displayTexts = new List<DisplayText>();
			DisplayTextTime = 3f;
		}

		public void Add(int id, string text)
		{
			DisplayText displayText = displayTexts.SingleOrDefault((DisplayText x) => x.ID == id);
			if (displayText == null)
			{
				DisplayText displayText2 = new DisplayText(id, text);
				displayTexts.Add(displayText2);
				displayText2.UpdatePosition(displayTexts.Count);
			}
			else
			{
				displayText.UpdateText(text);
			}
		}

		public void Update()
		{
			if (displayTexts.Count <= 0)
			{
				return;
			}
			bool flag = false;
			foreach (DisplayText displayText in displayTexts)
			{
				GUILayout.BeginArea(displayText.Area);
				GUILayout.Label(new GUIContent(displayText.Text));
				GUILayout.EndArea();
				if (displayText.ElapsedTime > DisplayTextTime)
				{
					displayText.Show = false;
					flag = true;
				}
			}
			if (flag)
			{
				displayTexts.RemoveAll((DisplayText x) => !x.Show);
				for (int num = 0; num < displayTexts.Count; num++)
				{
					displayTexts[num].UpdatePosition(num + 1);
				}
			}
		}
	}

	[Header("Defaults")]
	public bool FollowCameraDisabled;

	[Header("Internals")]
	public TextMeshProUGUI MenuTitle;

	public GameObject Frame;

	public GameObject ScenarioOptions;

	public GameObject NewAdventureMapOptions;

	public GameObject DebugKeys;

	public Toggle FollowCamToggle;

	public TMP_InputField SetCameraHeightText;

	public AutoCompleteComboBox SpawnMonsterDropdown;

	public TMP_Dropdown SpawnMonsterLevelDropdown;

	public Toggle SpawnMonsterAllyToggle;

	public Toggle SpawnMonsterNeutralToggle;

	public Toggle SpawnMonsterEnemy2Toggle;

	public TMP_InputField SetAttackStrengthText;

	public TMP_Dropdown InfuseElementDropdown;

	public TextMeshProUGUI SelectedCharacterText;

	public TMP_Dropdown PostiveConditionDropdown;

	public TMP_Dropdown NegativeConditionDropdown;

	public Button EditInventoryButton;

	public TMP_Dropdown EditCardsDropdown;

	public Button SetAttackModButton;

	public Button AddPositiveConditionButton;

	public Button AddNegativeConditionButton;

	public Button KillButton;

	public Button KillAllEnemiesButton;

	public Button ShowDebugKeysButton;

	public TextMeshProUGUI ShowDebugKeysText;

	public DebugEditInventory DebugEditInventory;

	public TMP_InputField GainXPText;

	public TMP_InputField GainMoneyTokenText;

	public TMP_InputField SetHealthText;

	public TMP_Dropdown SpawnPropDropDown;

	public Button SpawnPropButton;

	public GameObject UpdateCharacter;

	public TMP_Dropdown SetBiomeDropDown;

	public Button SetBiomeButton;

	public TMP_Dropdown SetSubBiomeDropDown;

	public Button SetSubBiomeButton;

	public TMP_Dropdown SetThemeDropDown;

	public Button SetThemeButton;

	public TMP_Dropdown SetSubThemeDropDown;

	public Button SetSubThemeButton;

	public TMP_Dropdown SetToneDropDown;

	public Button SetToneButton;

	public TMP_InputField SaveCustomStateInputField;

	public Toggle Vignette;

	public GameObject RestartButton;

	public TMP_Dropdown MoveCardsDropdown;

	public Button MoveAvailableButton;

	public Button MoveDiscardButton;

	public Button MoveBurnButton;

	public Toggle DisplayScenarioRNG;

	public Toggle DisplayMapRNG;

	public AutoCompleteComboBox SpawnObjectDropdown;

	public TMP_Dropdown SpawnObjectLevelDropdown;

	public TMP_Dropdown GuildmasterAddItemDropDown;

	public Button GuildmasterAddItemButton;

	public TMP_Dropdown GuildmasterAddJobDropDown;

	public Button GuildmasterAddJobButton;

	public Button SendErrorReport;

	public Button QuickSpawnButton;

	public TMP_Dropdown SwapCardsUnselectedDropdown;

	public TMP_Dropdown SwapCardsCurrentDropdown;

	public Button SwapUnselectedCardsButton;

	public TMP_Dropdown AddAttackModifierCardDropdown;

	public Button AddAttackModifierButton;

	public Toggle MPEndOfTurnCompare;

	public Button ForceDesyncButton;

	public Toggle[] ControllerModeToggles;

	public TMP_InputField QueueActionDelayInput;

	[Header("ClosedBeta Options")]
	public GameObject[] PanelsToDisable;

	[Header("Map")]
	public Button ShowAllLocationsButton;

	public Button ChangeDifficultyButton;

	public GameObject NewAdventureDifficultyPrefab;

	public TMP_Dropdown EventDropDown;

	public TMP_Dropdown BattleGoalDropDown;

	public TMP_Dropdown CityEventDropDown;

	public TMP_Dropdown PQDropDown;

	public Button SetEventButton;

	public Button SetCityEventButton;

	public Button SetBattleGoalButton;

	public Toggle DisplayTrophiesButton;

	public GameObject CampaignOptions;

	public TMP_InputField ReputationInput;

	public TMP_InputField ProsperityInput;

	public TMP_InputField XPInput;

	public TMP_InputField PerkChecksInput;

	public TMP_InputField GoldInput;

	public Toggle FHLoSToggle;

	public Toggle FHSummonFocusToggle;

	public Toggle ReducedRandomnessToggle;

	public Toggle FHRollingModifiersToggle;

	[Header("Other Options")]
	public bool isSlowmoDisabled;

	[NonSerialized]
	public bool ShowTileIndex;

	private const string m_MenuTitle = "Debug Menu";

	private CActor selectedCharacter;

	private bool isDebugKeysShowing;

	private bool isGUIVisible;

	private bool isPaused;

	public DisplayTextList displayTextList;

	private bool isWallFadeDisabled;

	private CursorLockMode _lockMode;

	private CClientTile LOSSourceTile;

	private CClientTile LOSTargetTile;

	public UnityEvent OnDebugMenuShownEvent = new UnityEvent();

	public UnityEvent OnDebugMenuHiddenEvent = new UnityEvent();

	public static bool displayCardsId;

	public static bool displayScenarioRNG;

	public static bool displayMapRNG;

	public static bool displayCardPile;

	public static bool displayTrophies;

	public static bool disableInputOnAppUnfocus;

	private bool quickspawning;

	private string quickSpawnMonsterName = "BanditArcherID";

	private int quickSpawnMonsterLevel = 1;

	public static DebugMenu Instance => Singleton<DebugMenuProvider>.Instance.DebugMenuInstance;

	public static bool DebugMenuNotNull => Singleton<DebugMenuProvider>.Instance != null;

	public int AttackValueOverride { get; set; }

	public bool CaptureTile { get; private set; }

	public bool IsMenuOpen { get; private set; }

	public bool IsMapTeleporting { get; set; }

	public bool ShowSpeedUp { get; set; }

	public CaptureTileFunctions CaptureTileFunction { get; private set; }

	private void Awake()
	{
		CaptureTile = false;
		IsMenuOpen = false;
		isDebugKeysShowing = false;
		isGUIVisible = true;
		isPaused = false;
		displayTextList = new DisplayTextList();
		isWallFadeDisabled = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		AttackValueOverride = int.MaxValue;
		FollowCameraDisabled = false;
		FollowCamToggle.isOn = !FollowCameraDisabled;
		ToggleSelectedCharacterMenus(toggle: false);
	}

	private void OnEnable()
	{
		Init();
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map && Singleton<MapChoreographer>.Instance != null)
		{
			NewAdventureMapOptions.SetActive(value: true);
			ScenarioOptions.SetActive(value: false);
			DebugKeys.SetActive(value: false);
			CampaignOptions.SetActive(SaveData.Instance.Global.GameMode == EGameMode.Campaign);
		}
		else
		{
			ScenarioOptions.SetActive(value: true);
			NewAdventureMapOptions.SetActive(value: false);
			DebugKeys.SetActive(value: false);
		}
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign && SaveData.Instance.Global.CampaignData.AdventureMapState.IsInScenarioPhase)
		{
			RestartButton.SetActive(value: true);
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster && SaveData.Instance.Global.AdventureData.AdventureMapState.IsInScenarioPhase)
		{
			RestartButton.SetActive(value: true);
		}
		else
		{
			RestartButton.SetActive(value: false);
		}
		ShowDebugKeysText.text = "Show Debug Keys";
		(UpdateCharacter.transform as RectTransform).sizeDelta = new Vector2(880f, 400f);
		OnDebugMenuShownEvent?.Invoke();
	}

	private void OnDisable()
	{
		OnDebugMenuHiddenEvent?.Invoke();
	}

	public void Init()
	{
		try
		{
			List<string> list = MonsterClassManager.Classes.Select((CMonsterClass x) => x.ID).ToList();
			list.Sort();
			SpawnMonsterDropdown.AvailableOptions = list;
			SpawnObjectDropdown.AvailableOptions = MonsterClassManager.ObjectClasses.Select((CObjectClass x) => x.ID).ToList();
			SpawnMonsterLevelDropdown.options.Clear();
			SpawnMonsterLevelDropdown.AddOptions(new List<string> { "0", "1", "2", "3", "4", "5", "6", "7" });
			SpawnObjectLevelDropdown.options.Clear();
			SpawnObjectLevelDropdown.AddOptions(new List<string> { "0", "1", "2", "3", "4", "5", "6", "7" });
			InfuseElementDropdown.options.Clear();
			InfuseElementDropdown.AddOptions((from x in (ElementInfusionBoardManager.EElement[])Enum.GetValues(typeof(ElementInfusionBoardManager.EElement))
				where x != ElementInfusionBoardManager.EElement.Any
				select x.ToString()).ToList());
			NegativeConditionDropdown.options.Clear();
			NegativeConditionDropdown.AddOptions((from x in (CCondition.ENegativeCondition[])Enum.GetValues(typeof(CCondition.ENegativeCondition))
				where x != CCondition.ENegativeCondition.NA
				select x.ToString()).ToList());
			PostiveConditionDropdown.options.Clear();
			PostiveConditionDropdown.AddOptions((from x in (CCondition.EPositiveCondition[])Enum.GetValues(typeof(CCondition.EPositiveCondition))
				where x != CCondition.EPositiveCondition.NA
				select x.ToString()).ToList());
			SpawnPropDropDown.options.Clear();
			SpawnPropDropDown.AddOptions((from s in CObjectProp.PropTypes
				where s != EPropType.None
				select s.ToString()).ToList());
			GuildmasterAddItemDropDown.options.Clear();
			GuildmasterAddItemDropDown.AddOptions(ScenarioRuleClient.SRLYML.ItemCards.Select((ItemCardYMLData s) => s.Name).ToList());
			GuildmasterAddJobDropDown.options.Clear();
			GuildmasterAddJobDropDown.AddOptions((from q in MapRuleLibraryClient.MRLYML.Quests.FindAll((CQuest q) => q.Type == EQuestType.Job)
				select q.ID).ToList());
			SetBiomeDropDown.options.Clear();
			SetBiomeDropDown.AddOptions((from s in (ScenarioPossibleRoom.EBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.EBiome))
				where s != ScenarioPossibleRoom.EBiome.Inherit
				select s.ToString()).ToList());
			SetSubBiomeDropDown.options.Clear();
			SetSubBiomeDropDown.AddOptions((from s in (ScenarioPossibleRoom.ESubBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubBiome))
				where s != ScenarioPossibleRoom.ESubBiome.Inherit
				select s.ToString()).ToList());
			SetThemeDropDown.options.Clear();
			SetThemeDropDown.AddOptions((from s in (ScenarioPossibleRoom.ETheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETheme))
				where s != ScenarioPossibleRoom.ETheme.Inherit
				select s.ToString()).ToList());
			SetSubThemeDropDown.options.Clear();
			SetSubThemeDropDown.AddOptions((from s in (ScenarioPossibleRoom.ESubTheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubTheme))
				where s != ScenarioPossibleRoom.ESubTheme.Inherit
				select s.ToString()).ToList());
			SetToneDropDown.options.Clear();
			SetToneDropDown.AddOptions((from s in (ScenarioPossibleRoom.ETone[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETone))
				where s != ScenarioPossibleRoom.ETone.Inherit
				select s.ToString()).ToList());
			EventDropDown.options.Clear();
			EventDropDown.AddOptions(MapRuleLibraryClient.MRLYML.RoadEvents.Select((CRoadEvent s) => s.ID).ToList());
			BattleGoalDropDown.options.Clear();
			BattleGoalDropDown.AddOptions(MapRuleLibraryClient.MRLYML.BattleGoals.Select((BattleGoalYMLData s) => s.ID).ToList());
			CityEventDropDown.options.Clear();
			CityEventDropDown.AddOptions(MapRuleLibraryClient.MRLYML.CityEvents.Select((CRoadEvent s) => s.ID).ToList());
			PQDropDown.options.Clear();
			PQDropDown.AddOptions(MapRuleLibraryClient.MRLYML.PersonalQuests.Select((PersonalQuestYMLData s) => s.ID).ToList());
			MoveCardsDropdown.options.Clear();
			if (Beautify.instance != null)
			{
				Vignette.SetValue(Beautify.instance.vignetting);
			}
			DisplayMapRNG.SetValue(displayMapRNG);
			DisplayScenarioRNG.SetValue(displayScenarioRNG);
			AddAttackModifierCardDropdown.options.Clear();
			AddAttackModifierCardDropdown.AddOptions(ScenarioRuleClient.SRLYML.AttackModifiers.Select((AttackModifierYMLData x) => x.Name).ToList());
			EditCardsDropdown.options.Clear();
			EditCardsDropdown.onValueChanged.AddListener(delegate
			{
				EditCards();
			});
			SpawnMonsterNeutralToggle.onValueChanged.AddListener(delegate
			{
				SpawnMonsterAllyToggle.SetValue(value: false);
				SpawnMonsterEnemy2Toggle.SetValue(value: false);
			});
			SpawnMonsterAllyToggle.onValueChanged.AddListener(delegate
			{
				SpawnMonsterNeutralToggle.SetValue(value: false);
				SpawnMonsterEnemy2Toggle.SetValue(value: false);
			});
			SpawnMonsterEnemy2Toggle.onValueChanged.AddListener(delegate
			{
				SpawnMonsterAllyToggle.SetValue(value: false);
				SpawnMonsterNeutralToggle.SetValue(value: false);
			});
			SwapCardsUnselectedDropdown.ClearOptions();
			SwapCardsCurrentDropdown.ClearOptions();
			SetCameraHeightText.text = ((int)CameraController.s_CameraController.m_ZoomOutExtraHeight).ToString();
			FHLoSToggle.SetValue(AdventureState.MapState != null && AdventureState.MapState.HouseRulesSetting.HasFlag(StateShared.EHouseRulesFlag.FrosthavenLOS));
			FHSummonFocusToggle.SetValue(AdventureState.MapState != null && AdventureState.MapState.HouseRulesSetting.HasFlag(StateShared.EHouseRulesFlag.FrosthavenSummonFocus));
			ReducedRandomnessToggle.SetValue(AdventureState.MapState != null && AdventureState.MapState.HouseRulesSetting.HasFlag(StateShared.EHouseRulesFlag.ReducedRandomness));
			FHRollingModifiersToggle.SetValue(AdventureState.MapState != null && AdventureState.MapState.HouseRulesSetting.HasFlag(StateShared.EHouseRulesFlag.FrosthavenRollingAttackModifiers));
			if (QueueActionDelayInput != null)
			{
				QueueActionDelayInput.text = ActionProcessor.QueueDelay.ToString();
			}
			for (int num = 0; num < ControllerModeToggles.Length; num++)
			{
				ControllerModeToggles[num].onValueChanged.AddListener(ToggleControllerMode);
				ControllerModeToggles[num].SetIsOnWithoutNotify(InputManager.IsControllerModeDisabled);
			}
			MPEndOfTurnCompare.SetValue(SaveData.Instance.Global.MPEndOfTurnCompare);
		}
		catch (Exception ex)
		{
			Debug.LogError("An error occurred initialising the Debug menu\n" + ex.StackTrace);
		}
	}

	public void ToggleMenu()
	{
		IsMenuOpen = !IsMenuOpen;
		base.gameObject.SetActive(IsMenuOpen);
		InputManager.UpdateMouseInputEnabled(IsMenuOpen);
		if (IsMenuOpen)
		{
			_lockMode = Cursor.lockState;
			Cursor.lockState = CursorLockMode.None;
		}
		else
		{
			Cursor.lockState = _lockMode;
		}
	}

	public static bool InterceptTileAction(CClientTile clientTile)
	{
		if (DebugMenuNotNull && Instance.CaptureTile)
		{
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SpawnMonster)
			{
				Instance.SpawnMonsterAtLocation(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SelectCharacter)
			{
				Instance.CharacterSelected(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SpawnProp)
			{
				Instance.SpawnPropAtLocation(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SpawnSpawner)
			{
				Instance.SpawnSpawnerAtLocation(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SpawnObject)
			{
				Instance.SpawnObjectAtLocation(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.LOSSourceTile)
			{
				Instance.SetLOSSourceTile(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.LOSTargetTile)
			{
				Instance.SetLOSTargetTile(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SetPosition)
			{
				Instance.SetActorPosition(clientTile);
				return true;
			}
		}
		return false;
	}

	public void ToggleControllerMode(bool toggled)
	{
		InputManager.IsControllerModeDisabled = toggled;
		for (int i = 0; i < ControllerModeToggles.Length; i++)
		{
			ControllerModeToggles[i].SetIsOnWithoutNotify(toggled);
		}
	}

	public void StallMainThread(int seconds)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		while (realtimeSinceStartup + (float)seconds > Time.realtimeSinceStartup)
		{
			Thread.Sleep(100);
		}
	}

	public void SetQueueActionDelay()
	{
		ActionProcessor.QueueDelay = float.Parse(QueueActionDelayInput.text);
		Debug.Log("Set Queue Action Delay to " + ActionProcessor.QueueDelay);
	}

	public static void ToggleDisableInputOnAppUnfocus()
	{
		disableInputOnAppUnfocus = !disableInputOnAppUnfocus;
	}

	public void ToggleDisplayCardsId(bool active)
	{
		displayCardsId = active;
	}

	public void ToggleVignette(bool active)
	{
		Beautify.instance.vignetting = active;
	}

	public void ToggleScenarioRNG(bool active)
	{
		displayScenarioRNG = active;
	}

	public void ToggleMapRNG(bool active)
	{
		displayMapRNG = active;
	}

	public void ToggleDisplayCardPile(bool active)
	{
		displayCardPile = active;
	}

	public void ToggleDisplayTrophies(bool active)
	{
		displayTrophies = active;
	}

	public void ToggleMPEndOfTurnCompare(bool active)
	{
		SaveData.Instance.Global.MPEndOfTurnCompare = active;
		SaveData.Instance.Global.Save();
	}

	public void ToggleShowAStar(bool value)
	{
		ClientScenarioManager.s_ClientScenarioManager.ToggleAStarDisplay();
		Debug.Log("Show AStar has been " + (value ? "enabled" : "disabled"));
	}

	public void ToggleShowAIPath(bool value)
	{
		ClientScenarioManager.s_ClientScenarioManager.ToggleAIPathDisplay();
		Debug.Log("Show AI Path has been " + (value ? "enabled" : "disabled"));
	}

	public void ToggleStatusLog(bool value)
	{
		GUIInterface.s_GUIInterface.ToggleStatusWindow(value);
	}

	public void ToggleDebugSlowMo(bool value)
	{
		if (TimeManager.IsSlowMo)
		{
			TimeManager.UnslowTime();
		}
		else
		{
			TimeManager.SlowTime(0.25f);
		}
	}

	public void ToggleTileIndex(bool value)
	{
		ShowTileIndex = value;
		if (ClientScenarioManager.s_ClientScenarioManager != null)
		{
			ClientScenarioManager.s_ClientScenarioManager.CreateTilesUI();
		}
	}

	public void ToggleShowLevelFlow(bool value)
	{
		ClientScenarioManager.s_ClientScenarioManager.ToggleLevelFlowDisplay();
		Debug.Log("Show Level Flow " + (value ? "enabled" : "disabled"));
	}

	public void ToggleShowLOS(bool value)
	{
		ClientScenarioManager.s_ClientScenarioManager.ToggleLOSDisplay(value);
		Debug.Log("Show LOS " + (value ? "enabled" : "disabled"));
	}

	public void ToggleSelectLOSSourceTile()
	{
		MenuTitle.text = "Select an LOS Source Tile";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.LOSSourceTile;
		IsMenuOpen = false;
	}

	private void ToggleSelectLOSTargetTile()
	{
		MenuTitle.text = "Select an LOS Target Tile";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.LOSTargetTile;
		IsMenuOpen = false;
	}

	public void ToggleAdvantageOutputText(bool value)
	{
		GameState.OutputAdvantageInfo = value;
		Debug.Log("Advantage Debug Output has been " + (value ? "enabled" : "disabled"));
	}

	public void ToggleCameraFollow(bool value)
	{
		FollowCameraDisabled = !value;
		if (FollowCameraDisabled)
		{
			CameraController.s_CameraController.DisableCameraInput(disableInput: false);
		}
		Debug.Log("Camera Follow has been " + (value ? "enabled" : "disabled"));
	}

	public void ToggleDisableSlowmo(bool value)
	{
		isSlowmoDisabled = value;
		Debug.Log("Disable slowmo " + (value ? "enabled" : "disabled"));
	}

	public void ToggleFHLoS(bool value)
	{
		StateShared.EHouseRulesFlag houseRulesSetting = StateShared.ToggleHouseRuleFlag(AdventureState.MapState.HouseRulesSetting, StateShared.EHouseRulesFlag.FrosthavenLOS);
		AdventureState.MapState.ChangeHouseRules(houseRulesSetting);
		Debug.Log("Alternate LoS rule " + (value ? "enabled" : "disabled"));
	}

	public void ToggleFHSummon(bool value)
	{
		StateShared.EHouseRulesFlag houseRulesSetting = StateShared.ToggleHouseRuleFlag(AdventureState.MapState.HouseRulesSetting, StateShared.EHouseRulesFlag.FrosthavenSummonFocus);
		AdventureState.MapState.ChangeHouseRules(houseRulesSetting);
		Debug.Log("Alternate Summon " + (value ? "enabled" : "disabled"));
	}

	public void ToggleReducedRNG(bool value)
	{
		StateShared.EHouseRulesFlag houseRulesSetting = StateShared.ToggleHouseRuleFlag(AdventureState.MapState.HouseRulesSetting, StateShared.EHouseRulesFlag.ReducedRandomness);
		AdventureState.MapState.ChangeHouseRules(houseRulesSetting);
		Debug.Log("Reduced Randomness rule " + (value ? "enabled" : "disabled"));
	}

	public void ToggleRolling(bool value)
	{
		StateShared.EHouseRulesFlag houseRulesSetting = StateShared.ToggleHouseRuleFlag(AdventureState.MapState.HouseRulesSetting, StateShared.EHouseRulesFlag.FrosthavenRollingAttackModifiers);
		AdventureState.MapState.ChangeHouseRules(houseRulesSetting);
		Debug.Log("Frosthaven Rolling rule " + (value ? "enabled" : "disabled"));
	}

	public void SetCameraAngle()
	{
		if (int.TryParse(SetCameraHeightText.text, out var result))
		{
			CameraController.s_CameraController.m_ZoomOutExtraHeight = result;
			Debug.Log("Camera height set to " + result);
		}
	}

	public void SpawnSelectLocation()
	{
		MenuTitle.text = "Select an empty tile";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SpawnMonster;
		IsMenuOpen = false;
	}

	public void SpawnMonsterAtLocation(CClientTile tile)
	{
		string monsterID = (quickspawning ? quickSpawnMonsterName : SpawnMonsterDropdown.Text);
		int result;
		if (!quickspawning)
		{
			int.TryParse(SpawnMonsterLevelDropdown.options[SpawnMonsterLevelDropdown.value].text, out result);
		}
		else
		{
			result = quickSpawnMonsterLevel;
		}
		try
		{
			if (quickspawning || SpawnMonsterDropdown.IsSelectionValid)
			{
				if (LevelEditorController.SpawnMonsterAtLocation(type: (!quickspawning && SpawnMonsterAllyToggle.isOn) ? CActor.EType.Ally : ((!quickspawning && SpawnMonsterNeutralToggle.isOn) ? CActor.EType.Neutral : ((quickspawning || !SpawnMonsterEnemy2Toggle.isOn) ? CActor.EType.Enemy : CActor.EType.Enemy2)), tile: tile, monsterID: monsterID, level: result))
				{
					CaptureTile = false;
					CaptureTileFunction = CaptureTileFunctions.None;
					IsMenuOpen = true;
					MenuTitle.text = "Debug Menu";
					Frame.SetActive(value: true);
					ToggleMenu();
				}
				quickspawning = false;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
			SpawnSelectLocation();
		}
	}

	public void QuickSpawnMonster()
	{
		quickspawning = true;
		SpawnSelectLocation();
	}

	public void SpawnSelectLocationObject()
	{
		MenuTitle.text = "Select an empty tile";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SpawnObject;
		IsMenuOpen = false;
	}

	public void SpawnObjectAtLocation(CClientTile tile)
	{
		string text = SpawnObjectDropdown.Text;
		if (SpawnObjectDropdown.IsSelectionValid && int.TryParse(SpawnObjectLevelDropdown.options[SpawnObjectLevelDropdown.value].text, out var result) && LevelEditorController.SpawnObjectAtLocation(tile, text, result))
		{
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			ToggleMenu();
		}
	}

	public void SpawnPropAtLocation(CClientTile tile)
	{
		string text = SpawnPropDropDown.options[SpawnPropDropDown.value].text;
		if (LevelEditorController.SpawnPropAtLocation(new List<CClientTile> { tile }, text))
		{
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			ToggleMenu();
		}
	}

	public void SetLOSSourceTile(CClientTile tile)
	{
		CaptureTile = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		LOSSourceTile = tile;
		ToggleSelectLOSTargetTile();
	}

	public void SetLOSTargetTile(CClientTile tile)
	{
		CaptureTile = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		LOSTargetTile = tile;
		IsMenuOpen = true;
		MenuTitle.text = "Debug Menu";
		Frame.SetActive(value: true);
		ToggleMenu();
		ClientScenarioManager.s_ClientScenarioManager.SetSpecificSourceAndTargetTile(LOSSourceTile, LOSTargetTile);
	}

	public void SelectActorPosition()
	{
		if (selectedCharacter != null)
		{
			MenuTitle.text = "Select an empty tile";
			Frame.SetActive(value: false);
			CaptureTile = true;
			CaptureTileFunction = CaptureTileFunctions.SetPosition;
			IsMenuOpen = false;
		}
	}

	public void SetActorPosition(CClientTile clientTile)
	{
		if (selectedCharacter != null && clientTile != null && WorldspaceStarHexDisplay.Instance.IsMoveTileValid(clientTile, ignoreBlocked: false, selectedCharacter, selectedCharacter.Type))
		{
			selectedCharacter.ArrayIndex = clientTile.m_Tile.m_ArrayIndex;
			GameObject obj = Choreographer.s_Choreographer.FindClientActorGameObject(selectedCharacter);
			Vector3 position = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[clientTile.m_Tile.m_ArrayIndex.X, clientTile.m_Tile.m_ArrayIndex.Y].m_GameObject.transform.position;
			ActorBehaviour.GetActorBehaviour(obj).TeleportToLocation(position);
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			ToggleMenu();
		}
	}

	public void RegenerateApparance()
	{
		LevelEditorController.RegenerateApparance();
	}

	public void SetBiome()
	{
		LevelEditorController.SetBiome(((ScenarioPossibleRoom.EBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.EBiome))).Single((ScenarioPossibleRoom.EBiome s) => s.ToString() == SetBiomeDropDown.options[SetBiomeDropDown.value].text));
	}

	public void SetSubBiome()
	{
		LevelEditorController.SetSubBiome(((ScenarioPossibleRoom.ESubBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubBiome))).Single((ScenarioPossibleRoom.ESubBiome s) => s.ToString() == SetSubBiomeDropDown.options[SetSubBiomeDropDown.value].text));
	}

	public void SetTheme()
	{
		LevelEditorController.SetTheme(((ScenarioPossibleRoom.ETheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETheme))).Single((ScenarioPossibleRoom.ETheme s) => s.ToString() == SetThemeDropDown.options[SetThemeDropDown.value].text));
	}

	public void SetSubTheme()
	{
		LevelEditorController.SetSubTheme(((ScenarioPossibleRoom.ESubTheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubTheme))).Single((ScenarioPossibleRoom.ESubTheme s) => s.ToString() == SetSubThemeDropDown.options[SetSubThemeDropDown.value].text));
	}

	public void SetTone()
	{
		LevelEditorController.SetTone(((ScenarioPossibleRoom.ETone[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETone))).Single((ScenarioPossibleRoom.ETone s) => s.ToString() == SetToneDropDown.options[SetToneDropDown.value].text));
	}

	public void SetAttackStrength()
	{
		if (int.TryParse(SetAttackStrengthText.text, out var result))
		{
			ScenarioRuleClient.SetNextAttackValueOverride(result);
			AttackValueOverride = result;
			Debug.Log("The next attack will have " + result + " set as the strength");
		}
	}

	public void AddElement()
	{
		try
		{
			ElementInfusionBoardManager.EElement element = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == InfuseElementDropdown.options[InfuseElementDropdown.value].text);
			ElementInfusionBoardManager.Infuse(element, null);
			ElementInfusionBoardManager.EndTurn();
			Debug.Log("Infused " + element);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to infuse element.  An exception occurred.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void InfuseAll()
	{
		foreach (ElementInfusionBoardManager.EElement item in ElementInfusionBoardManager.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
		{
			try
			{
				ElementInfusionBoardManager.Infuse(item, null);
				Debug.Log("Infused " + item);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to infuse element.  An exception occurred.\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		ElementInfusionBoardManager.EndTurn();
	}

	public void SelectCharacter()
	{
		MenuTitle.text = "Select a character";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SelectCharacter;
		IsMenuOpen = false;
	}

	public void SelectPropLocation()
	{
		MenuTitle.text = "Select a tile to spawn the prop";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SpawnProp;
		IsMenuOpen = false;
	}

	public void SelectSpawnerLocation()
	{
		MenuTitle.text = "Select a tile to spawn the Spawner";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SpawnSpawner;
		IsMenuOpen = false;
	}

	public void CharacterSelected(CClientTile tile)
	{
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex);
		if (cActor != null)
		{
			selectedCharacter = cActor;
			SelectedCharacterText.text = cActor.GetPrefabName();
			ToggleSelectedCharacterMenus(toggle: true);
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			if (cActor is CPlayerActor cPlayerActor)
			{
				RefreshPlayerCardDropdowns(cPlayerActor.CharacterClass);
			}
			CMonsterClass cMonsterClass = cActor.Class as CMonsterClass;
			if (cMonsterClass != null)
			{
				if (cMonsterClass.NonEliteVariant != null)
				{
					cMonsterClass = cMonsterClass.NonEliteVariant;
				}
				EditCardsDropdown.ClearOptions();
				EditCardsDropdown.AddOptions(cMonsterClass.AbilityCardsPool.Select((CMonsterAbilityCard x) => string.Format("{0} (Init:{1} - {2})", x.ID, x.Initiative, string.Join(", ", x.Action.Abilities.Select((CAbility y) => y.AbilityType.ToString() + " " + y.Strength)))).ToList());
			}
			Debug.Log("Selected " + cActor.GetPrefabName());
		}
		else
		{
			Debug.LogError("Unable to find target at selected location");
			SelectCharacter();
		}
	}

	private void RefreshPlayerCardDropdowns(CCharacterClass characterClass)
	{
		MoveCardsDropdown.ClearOptions();
		MoveCardsDropdown.AddOptions(characterClass.HandAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
		MoveCardsDropdown.AddOptions(characterClass.DiscardedAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
		MoveCardsDropdown.AddOptions(characterClass.LostAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
		MoveCardsDropdown.AddOptions(characterClass.PermanentlyLostAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
		SwapCardsUnselectedDropdown.ClearOptions();
		SwapCardsUnselectedDropdown.AddOptions(characterClass.UnselectedAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
		SwapCardsCurrentDropdown.ClearOptions();
		SwapCardsCurrentDropdown.AddOptions(characterClass.HandAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
		SwapCardsCurrentDropdown.AddOptions(characterClass.DiscardedAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
		SwapCardsCurrentDropdown.AddOptions(characterClass.LostAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
		SwapCardsCurrentDropdown.AddOptions(characterClass.PermanentlyLostAbilityCards.Select((CAbilityCard x) => string.Format("{0} - {1} ({2})", x.ID, x.Name.Replace("ABILITY_CARD_", ""), x.Initiative)).ToList());
	}

	public void SwapInUnselectedCard()
	{
		if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !SwapCardsUnselectedDropdown.options[SwapCardsUnselectedDropdown.value].text.IsNullOrEmpty() && !SwapCardsCurrentDropdown.options[SwapCardsCurrentDropdown.value].text.IsNullOrEmpty() && selectedCharacter is CPlayerActor cPlayerActor && CardsHandManager.Instance.IsActive())
		{
			MoveAbilityCardToPile(cPlayerActor.CharacterClass, "ABILITY_CARD_" + Regex.Replace(Regex.Replace(SwapCardsCurrentDropdown.options[SwapCardsCurrentDropdown.value].text, " \\(.*?\\)", ""), "^.*? - ", ""), "Unselected", updateUI: false);
			MoveAbilityCardToPile(cPlayerActor.CharacterClass, "ABILITY_CARD_" + Regex.Replace(Regex.Replace(SwapCardsUnselectedDropdown.options[SwapCardsUnselectedDropdown.value].text, " \\(.*?\\)", ""), "^.*? - ", ""), "Available");
			RefreshPlayerCardDropdowns(cPlayerActor.CharacterClass);
		}
	}

	public void MoveAbilityCard(string destinationPile)
	{
		if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !MoveCardsDropdown.options[MoveCardsDropdown.value].text.IsNullOrEmpty() && selectedCharacter is CPlayerActor cPlayerActor && CardsHandManager.Instance.IsActive())
		{
			MoveAbilityCardToPile(cPlayerActor.CharacterClass, "ABILITY_CARD_" + Regex.Replace(Regex.Replace(MoveCardsDropdown.options[MoveCardsDropdown.value].text, " \\(.*?\\)", ""), "^.*? - ", ""), destinationPile);
			RefreshPlayerCardDropdowns(cPlayerActor.CharacterClass);
		}
	}

	private static void MoveAbilityCardToPile(CCharacterClass characterClass, string cardName, string destinationPile, bool updateUI = true)
	{
		CAbilityCard cAbilityCard = characterClass.HandAbilityCards.SingleOrDefault((CAbilityCard x) => x.Name == cardName);
		List<CAbilityCard> fromAbilityCardList = characterClass.HandAbilityCards;
		string fromCardPileName = "HandAbilityCards";
		if (cAbilityCard == null)
		{
			fromAbilityCardList = characterClass.DiscardedAbilityCards;
			cAbilityCard = characterClass.DiscardedAbilityCards.SingleOrDefault((CAbilityCard x) => x.Name == cardName);
			fromCardPileName = "DiscardedAbilityCards";
		}
		if (cAbilityCard == null)
		{
			fromAbilityCardList = characterClass.LostAbilityCards;
			cAbilityCard = characterClass.LostAbilityCards.SingleOrDefault((CAbilityCard x) => x.Name == cardName);
			fromCardPileName = "LostAbilityCards";
		}
		if (cAbilityCard == null)
		{
			fromAbilityCardList = characterClass.PermanentlyLostAbilityCards;
			cAbilityCard = characterClass.PermanentlyLostAbilityCards.SingleOrDefault((CAbilityCard x) => x.Name == cardName);
			fromCardPileName = "PermanentlyLostAbilityCards";
		}
		if (cAbilityCard == null)
		{
			fromAbilityCardList = characterClass.UnselectedAbilityCards;
			cAbilityCard = characterClass.UnselectedAbilityCards.SingleOrDefault((CAbilityCard x) => x.Name == cardName);
			fromCardPileName = "UnselectedAbilityCards";
		}
		if (cAbilityCard != null)
		{
			switch (destinationPile)
			{
			case "Available":
				characterClass.MoveAbilityCard(cAbilityCard, fromAbilityCardList, characterClass.HandAbilityCards, fromCardPileName, "HandAbilityCards");
				break;
			case "Discard":
				characterClass.MoveAbilityCard(cAbilityCard, fromAbilityCardList, characterClass.DiscardedAbilityCards, fromCardPileName, "DiscardedAbilityCards");
				break;
			case "Lost":
				characterClass.MoveAbilityCard(cAbilityCard, fromAbilityCardList, characterClass.LostAbilityCards, fromCardPileName, "LostAbilityCards");
				break;
			case "Unselected":
				characterClass.MoveAbilityCard(cAbilityCard, fromAbilityCardList, characterClass.UnselectedAbilityCards, fromCardPileName, "UnselectedAbilityCards");
				break;
			}
			if (updateUI)
			{
				CardsHandManager.Instance.Show(CardHandMode.CardsSelection, CardPileType.Any, new List<CardPileType>
				{
					CardPileType.Hand,
					CardPileType.Active,
					CardPileType.Round
				}, 2);
			}
		}
	}

	public void GuildmasterAddItem()
	{
		string itemName = GuildmasterAddItemDropDown.options[GuildmasterAddItemDropDown.value].text;
		CItem cItem = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.Name == itemName).GetItem.Copy(SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GetGUIDBasedOnMapRNGState(), SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GetNextItemNetworkID());
		SimpleLog.AddToSimpleLog("(DebugMenu) Adding new item " + cItem.Name + " with NetworkID " + cItem.NetworkID);
		AdventureState.MapState.MapParty.AddItem(cItem);
		Debug.Log("Gained item: " + itemName);
	}

	public static void GetAllItems()
	{
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				CItem cItem = itemCard.GetItem.Copy(SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GetGUIDBasedOnMapRNGState(), SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GetNextItemNetworkID());
				SimpleLog.AddToSimpleLog("(DebugMenu) Adding new item " + cItem.Name + " with NetworkID " + cItem.NetworkID);
				selectedCharacter.AddItemToBoundItems(cItem);
				Debug.Log("Gained item: " + cItem.Name + " for " + selectedCharacter.CharacterName);
			}
		}
	}

	public void GuildmasterAddJob()
	{
		string text = GuildmasterAddJobDropDown.options[GuildmasterAddJobDropDown.value].text;
		Singleton<MapChoreographer>.Instance.DebugAddJobQuestState(text);
	}

	public void SetReputation()
	{
		SetReputationValue(int.Parse(ReputationInput.text));
	}

	public static void ChangeReputation(int value)
	{
		SetReputationValue(AdventureState.MapState.MapParty.Reputation + value);
	}

	private static void SetReputationValue(int value)
	{
		AdventureState.MapState.MapParty.UpdateReputation(value);
		Debug.Log("Updated Reputation " + value);
	}

	public void SetProsperityXP()
	{
		SetProsperityXP(int.Parse(ProsperityInput.text));
	}

	public static void SetProsperityXP(int value)
	{
		AdventureState.MapState.MapParty.UpdateProsperityXP(value);
		Debug.Log("Updated Prosperity XP " + value);
		AdventureState.MapState.CheckLockedContent();
	}

	public void SetPerkChecks()
	{
		SetPerkChecks(int.Parse(PerkChecksInput.text));
	}

	public static void SetPerkChecks(int checks)
	{
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			selectedCharacter.UpdatePerkChecks(checks);
			Debug.Log(selectedCharacter.CharacterYMLData.Model.ToString() + " Updated Perk checks " + checks);
		}
	}

	public void GainSelectedCharactersGold()
	{
		GainSelectedCharactersGold(int.Parse(GoldInput.text));
	}

	public static void GainSelectedCharactersGold(int gold)
	{
		if (AdventureState.MapState.IsCampaign)
		{
			foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				selectedCharacter.ModifyGold(gold, useGoldModifier: false);
				Debug.Log(selectedCharacter.CharacterYMLData.Model.ToString() + " Updated gold " + gold);
			}
			return;
		}
		AdventureState.MapState.MapParty.ModifyPartyGold(gold, useGoldModifier: false);
		Debug.Log("Updated gold " + gold);
	}

	public static void AddEnhancmentPoints(int points)
	{
		(Singleton<MapChoreographer>.Instance.HeadquartersLocation.Location as CHeadquartersState).EnhancementSlots += points;
		Debug.Log("Updated enhancment points " + points);
	}

	public void GainSelectedCharactersXP()
	{
		GainSelectedCharactersXP(int.Parse(XPInput.text));
	}

	public static void GainSelectedCharactersXP(int xp)
	{
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			selectedCharacter.GainEXP(xp, 1f);
			Debug.Log(selectedCharacter.CharacterYMLData.Model.ToString() + " Updated XP " + xp);
		}
	}

	public void ResetIntroductions()
	{
		AdventureState.MapState.MapParty.IntroductionDoneIds.Clear();
		Debug.Log("Reset intros");
	}

	private void ToggleSelectedCharacterMenus(bool toggle)
	{
		PostiveConditionDropdown.enabled = toggle;
		NegativeConditionDropdown.enabled = toggle;
		EditInventoryButton.enabled = toggle;
		EditCardsDropdown.enabled = toggle;
		SetAttackModButton.enabled = toggle;
		AddPositiveConditionButton.enabled = toggle;
		AddNegativeConditionButton.enabled = toggle;
		KillButton.enabled = toggle;
		AddAttackModifierCardDropdown.enabled = toggle;
		AddAttackModifierButton.enabled = toggle;
	}

	public void Kill()
	{
		if (selectedCharacter == null)
		{
			return;
		}
		try
		{
			CMessageData lastMessage = Choreographer.s_Choreographer.LastMessage;
			GameObject obj = Choreographer.s_Choreographer.FindClientActorGameObject(selectedCharacter);
			selectedCharacter.Health = 0;
			ActorBehaviour.UpdateHealth(obj);
			GameState.KillActor(selectedCharacter, selectedCharacter, CActor.ECauseOfDeath.DebugMenu, out var _);
			if (selectedCharacter.Type == CActor.EType.Player)
			{
				(selectedCharacter.Class as CCharacterClass).CheckForFinishedActiveBonuses(selectedCharacter);
			}
			else if (selectedCharacter.Type == CActor.EType.HeroSummon)
			{
				(selectedCharacter as CHeroSummonActor).Summoner.CharacterClass.CheckForFinishedActiveBonuses((selectedCharacter as CHeroSummonActor).Summoner);
			}
			Choreographer.s_Choreographer.LastMessage = lastMessage;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
		Debug.Log("Character " + selectedCharacter.GetPrefabName() + " has been killed");
		selectedCharacter = null;
		SelectedCharacterText.text = "No Character Selected";
		MoveCardsDropdown.options.Clear();
		ToggleSelectedCharacterMenus(toggle: false);
		ToggleMenu();
	}

	public static void KillAllEnemies()
	{
		foreach (CEnemyActor allEnemyMonster in ScenarioManager.Scenario.AllEnemyMonsters)
		{
			if (!allEnemyMonster.MonsterClass.Boss)
			{
				try
				{
					CMessageData lastMessage = Choreographer.s_Choreographer.LastMessage;
					GameObject obj = Choreographer.s_Choreographer.FindClientActorGameObject(allEnemyMonster);
					allEnemyMonster.Health = 0;
					ActorBehaviour.UpdateHealth(obj);
					GameState.KillActor(allEnemyMonster, allEnemyMonster, CActor.ECauseOfDeath.DebugMenu, out var _);
					Choreographer.s_Choreographer.LastMessage = lastMessage;
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.Message + "\n" + ex.StackTrace);
				}
				Debug.Log("Character " + allEnemyMonster.GetPrefabName() + " has been killed");
			}
		}
	}

	public static void RestoreCards()
	{
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			foreach (CAbilityCard item in new List<CAbilityCard>(allPlayer.CharacterClass.DiscardedAbilityCards))
			{
				MoveAbilityCardToPile(allPlayer.CharacterClass, item.Name, "Available");
			}
			foreach (CAbilityCard item2 in new List<CAbilityCard>(allPlayer.CharacterClass.LostAbilityCards))
			{
				MoveAbilityCardToPile(allPlayer.CharacterClass, item2.Name, "Available");
			}
			foreach (CAbilityCard item3 in new List<CAbilityCard>(allPlayer.CharacterClass.PermanentlyLostAbilityCards))
			{
				MoveAbilityCardToPile(allPlayer.CharacterClass, item3.Name, "Available");
			}
		}
	}

	public static void RevealAllRooms()
	{
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			map.Reveal(initial: false);
		}
		foreach (CObjectDoor item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>())
		{
			item.ForceActivate(null);
		}
	}

	public static void ChangeMeshesByCube()
	{
		GeneratedMeshInfoCalculator.ChangeMeshesByCube();
	}

	public static void SimplifyMeshes(float quality)
	{
		GeneratedMeshInfoCalculator.SimplifyModelToLimit(quality);
	}

	public void ShowAllScenarios()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.DebugShowAllScenarios();
			ToggleMenu();
		}
	}

	public static void UnlockEnchantress()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			(Singleton<MapChoreographer>.Instance.HeadquartersLocation.Location as CHeadquartersState).UnlockEnhancer();
			List<Reward> rewards = (from reward in AdventureState.MapState.AllQuests.SelectMany((CQuestState quest) => quest.QuestCompletionRewardGroup.Rewards)
				where reward.Type == ETreasureType.Enhancement
				select reward).ToList();
			Singleton<UIDistributeRewardManager>.Instance.Process(rewards, delegate
			{
				SaveData.Instance.SaveCurrentAdventureData();
			});
		}
	}

	public static void ShowAllScenariosNoToggle()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.DebugShowAllScenarios();
		}
	}

	public void DrawCityEvent()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.OpenCityEvent();
			ToggleMenu();
		}
	}

	public void ApplyAutoCompleteYML()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.DebugApplyAutoCompleteYML();
			ToggleMenu();
		}
	}

	public void ChangeDifficulty()
	{
	}

	public static void UnlockAllCharacters()
	{
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			RewardGroup item = new RewardGroup(new List<Reward>
			{
				new Reward(@class.CharacterID)
			});
			AdventureState.MapState.ApplyRewards(new List<RewardGroup> { item });
		}
	}

	public void EditInventory()
	{
		DebugEditInventory.ShowEditInventory(selectedCharacter.Inventory);
	}

	public void EditCards()
	{
		if (selectedCharacter == null)
		{
			return;
		}
		try
		{
			if (PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest)
			{
				if (selectedCharacter.Class is CMonsterClass && !EditCardsDropdown.options[EditCardsDropdown.value].text.IsNullOrEmpty())
				{
					CMonsterClass cMonsterClass = (CMonsterClass)selectedCharacter.Class;
					if (cMonsterClass.NonEliteVariant != null)
					{
						cMonsterClass = cMonsterClass.NonEliteVariant;
					}
					cMonsterClass.ResetAbilityDeck();
					string cardname = Regex.Replace(EditCardsDropdown.options[EditCardsDropdown.value].text, " \\(.*?\\)", "");
					CMonsterAbilityCard cMonsterAbilityCard = cMonsterClass.AbilityCards.SingleOrDefault((CMonsterAbilityCard x) => x.ID.ToString() == cardname);
					if (cMonsterAbilityCard != null)
					{
						cMonsterClass.AbilityCards.Remove(cMonsterAbilityCard);
						cMonsterClass.AbilityCards.Shuffle();
						cMonsterClass.AbilityCards.Insert(0, cMonsterAbilityCard);
					}
					DisplayDebugText(2, "Next monster card ID: " + cMonsterClass.AbilityCards[0].ID + " (Init: " + cMonsterClass.AbilityCards[0].Initiative + ")");
				}
				else
				{
					Debug.Log("Debug card selection is only available for Monsters");
				}
			}
			else
			{
				Debug.Log("Invalid phase for monster card editing");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void SetAttackMod()
	{
		if (selectedCharacter == null)
		{
			return;
		}
		try
		{
			if (PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest)
			{
				if (selectedCharacter.Class is CMonsterClass)
				{
					if (selectedCharacter is CEnemyActor enemyActor)
					{
						DisplayDebugText(2, MonsterClassManager.DebugSetAttackMod(enemyActor));
					}
					return;
				}
				CCharacterClass cCharacterClass = null;
				cCharacterClass = ((!(selectedCharacter is CHeroSummonActor cHeroSummonActor)) ? (selectedCharacter.Class as CCharacterClass) : (cHeroSummonActor.Summoner.Class as CCharacterClass));
				if (cCharacterClass.DiscardedAttackModifierCards.Count > 0)
				{
					cCharacterClass.ResetAttackModifierDeck();
				}
				AttackModifierYMLData item = cCharacterClass.AttackModifierCards[0];
				cCharacterClass.AttackModifierCards.Remove(item);
				cCharacterClass.AttackModifierCards.Add(item);
				DisplayDebugText(2, "Next: " + cCharacterClass.AttackModifierCards[0].Name);
			}
			else
			{
				Debug.Log("Invalid phase for monster card editing");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void AddAttackModifier()
	{
		if (selectedCharacter == null)
		{
			return;
		}
		try
		{
			if (PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest)
			{
				if (!(selectedCharacter is CEnemyActor enemyActor))
				{
					CCharacterClass cCharacterClass = null;
					cCharacterClass = ((!(selectedCharacter is CHeroSummonActor cHeroSummonActor)) ? (selectedCharacter.Class as CCharacterClass) : (cHeroSummonActor.Summoner.Class as CCharacterClass));
					List<string> cardNames = new List<string> { AddAttackModifierCardDropdown.options[AddAttackModifierCardDropdown.value].text };
					cCharacterClass.AddAdditionalModifierCards(cardNames);
					DisplayDebugText(2, "Added attack modifier card : " + AddAttackModifierCardDropdown.options[AddAttackModifierCardDropdown.value].text);
				}
				else
				{
					List<string> cardNames2 = new List<string> { AddAttackModifierCardDropdown.options[AddAttackModifierCardDropdown.value].text };
					MonsterClassManager.AddAdditionalModifierCards(enemyActor, cardNames2);
					DisplayDebugText(2, "Added attack modifier card : " + AddAttackModifierCardDropdown.options[AddAttackModifierCardDropdown.value].text);
				}
			}
			else
			{
				Debug.Log("Invalid phase for attack modifier card editing");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void AddPositiveCondition()
	{
		if (selectedCharacter != null)
		{
			CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.Single((CCondition.EPositiveCondition x) => x.ToString() == PostiveConditionDropdown.options[PostiveConditionDropdown.value].text);
			switch (ePositiveCondition)
			{
			case CCondition.EPositiveCondition.Bless:
				(CAbility.CreateAbility(CAbility.EAbilityType.Bless, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityBless).ApplyToActor(selectedCharacter);
				Debug.Log("Bless applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.EPositiveCondition.Strengthen:
				(CAbility.CreateAbility(CAbility.EAbilityType.Strengthen, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityStrengthen).ApplyToActor(selectedCharacter);
				Debug.Log("Strengthen applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.EPositiveCondition.Invisible:
				(CAbility.CreateAbility(CAbility.EAbilityType.Invisible, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityInvisible).ApplyToActor(selectedCharacter);
				Debug.Log("Invisible applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.EPositiveCondition.Immovable:
				(CAbility.CreateAbility(CAbility.EAbilityType.Immovable, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityImmovable).ApplyToActor(selectedCharacter);
				Debug.Log("Immovable applied to " + selectedCharacter.GetPrefabName());
				break;
			default:
				Debug.Log("Condition " + ePositiveCondition.ToString() + " is not supported yet");
				break;
			}
		}
	}

	public void AddNegativeCondition()
	{
		if (selectedCharacter != null)
		{
			CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.Single((CCondition.ENegativeCondition x) => x.ToString() == NegativeConditionDropdown.options[NegativeConditionDropdown.value].text);
			switch (eNegativeCondition)
			{
			case CCondition.ENegativeCondition.Muddle:
				(CAbility.CreateAbility(CAbility.EAbilityType.Muddle, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityMuddle).ApplyToActor(selectedCharacter);
				Debug.Log("Muddle applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.ENegativeCondition.Poison:
				(CAbility.CreateAbility(CAbility.EAbilityType.Poison, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityPoison).ApplyToActor(selectedCharacter);
				Debug.Log("Poison applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.ENegativeCondition.Curse:
				(CAbility.CreateAbility(CAbility.EAbilityType.Curse, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityCurse).ApplyToActor(selectedCharacter);
				Debug.Log("Curse applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.ENegativeCondition.Immobilize:
				(CAbility.CreateAbility(CAbility.EAbilityType.Immobilize, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityImmobilize).ApplyToActor(selectedCharacter);
				Debug.Log("Immobilize applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.ENegativeCondition.Disarm:
				(CAbility.CreateAbility(CAbility.EAbilityType.Disarm, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityDisarm).ApplyToActor(selectedCharacter);
				Debug.Log("Disarm applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.ENegativeCondition.Wound:
				(CAbility.CreateAbility(CAbility.EAbilityType.Wound, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityWound).ApplyToActor(selectedCharacter);
				Debug.Log("Wound applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.ENegativeCondition.Stun:
				(CAbility.CreateAbility(CAbility.EAbilityType.Stun, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityStun).ApplyToActor(selectedCharacter);
				Debug.Log("Stun applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.ENegativeCondition.StopFlying:
				(CAbility.CreateAbility(CAbility.EAbilityType.StopFlying, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilityStopFlying).ApplyToActor(selectedCharacter);
				Debug.Log("Stop flying applied to " + selectedCharacter.GetPrefabName());
				break;
			case CCondition.ENegativeCondition.Sleep:
				(CAbility.CreateAbility(CAbility.EAbilityType.Sleep, CAbilityFilterContainer.CreateDefaultFilter(), selectedCharacter.Type == CActor.EType.Enemy, isTargetedAbility: true) as CAbilitySleep).ApplyToActor(selectedCharacter);
				Debug.Log("Sleep applied to " + selectedCharacter.GetPrefabName());
				break;
			default:
				Debug.Log("Condition " + eNegativeCondition.ToString() + " is not supported yet");
				break;
			}
		}
	}

	public void SetNewAdvModeRoadEvent()
	{
		SetRoadEvent(EventDropDown.options[EventDropDown.value].text);
	}

	public static void SetRoadEvent(string roadEvent)
	{
		CRoadEvent cRoadEvent = MapRuleLibraryClient.MRLYML.RoadEvents.SingleOrDefault((CRoadEvent s) => s.ID == roadEvent);
		if (cRoadEvent != null)
		{
			Singleton<MapChoreographer>.Instance.RoadEventDebugOverride = cRoadEvent;
			Debug.Log("Event " + cRoadEvent.ID + " has been set as the next road event");
		}
	}

	public static void SetCityEvent(string cityEvent)
	{
		CRoadEvent cRoadEvent = MapRuleLibraryClient.MRLYML.CityEvents.SingleOrDefault((CRoadEvent s) => s.ID == cityEvent);
		if (cRoadEvent != null)
		{
			Singleton<MapChoreographer>.Instance.CityEventDebugOverride = cRoadEvent;
			Debug.Log("Event " + cRoadEvent.ID + " has been set as the next city event");
		}
	}

	public void SetNewAdvModeCityEvent()
	{
		CRoadEvent cRoadEvent = MapRuleLibraryClient.MRLYML.CityEvents.SingleOrDefault((CRoadEvent s) => s.ID == CityEventDropDown.options[CityEventDropDown.value].text);
		if (cRoadEvent != null)
		{
			Singleton<MapChoreographer>.Instance.CityEventDebugOverride = cRoadEvent;
			Debug.Log("Event " + cRoadEvent.ID + " has been set as the next city event");
		}
	}

	public void SetNextBattleGoal()
	{
		BattleGoalYMLData battleGoalYMLData = MapRuleLibraryClient.MRLYML.BattleGoals.SingleOrDefault((BattleGoalYMLData s) => s.ID == BattleGoalDropDown.options[BattleGoalDropDown.value].text);
		if (battleGoalYMLData != null)
		{
			AdventureState.MapState.DebugNextBattleGoals.Add(battleGoalYMLData);
			Debug.Log("Battle Goal " + battleGoalYMLData.ID + " has been set as one of the next Battle Goals");
		}
	}

	public void ShowTrophies()
	{
		BattleGoalYMLData battleGoalYMLData = MapRuleLibraryClient.MRLYML.BattleGoals.SingleOrDefault((BattleGoalYMLData s) => s.ID == BattleGoalDropDown.options[BattleGoalDropDown.value].text);
		if (battleGoalYMLData != null)
		{
			AdventureState.MapState.DebugNextBattleGoals.Add(battleGoalYMLData);
			Debug.Log("Battle Goal " + battleGoalYMLData.ID + " has been set as one of the next Battle Goals");
		}
	}

	public void SetNewAdvModePersonalQuest()
	{
		PersonalQuestYMLData personalQuestYMLData = MapRuleLibraryClient.MRLYML.PersonalQuests.SingleOrDefault((PersonalQuestYMLData s) => s.ID == PQDropDown.options[PQDropDown.value].text);
		if (personalQuestYMLData != null)
		{
			AdventureState.MapState.DebugNextPersonalQuests.Add(personalQuestYMLData);
			Debug.Log("Personal Quest " + personalQuestYMLData.ID + " has been set as one of the next Personal Quests");
		}
	}

	public static void AutoCompleteAllPersonalQuests()
	{
		if (AdventureState.MapState.IsCampaign)
		{
			foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				AutoCompleteCharacterPersonalQuest(selectedCharacter);
			}
		}
		Debug.Log("Complete all Personal Quests");
	}

	public static void AutoCompleteMinePersonalQuest()
	{
		if (AdventureState.MapState.IsCampaign)
		{
			AutoCompleteCharacterPersonalQuest(AdventureState.MapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter x) => !FFSNetwork.IsOnline || x.IsUnderMyControl));
		}
		Debug.Log("Complete mine Personal Quest");
	}

	private static void AutoCompleteCharacterPersonalQuest(CMapCharacter character)
	{
		CPersonalQuestState personalQuest;
		do
		{
			personalQuest = character.PersonalQuest;
		}
		while (personalQuest != null && personalQuest.NextPersonalQuestStep());
		character.PersonalQuest?.CompletePersonalQuest(character.CharacterID);
	}

	public void Win()
	{
		WinNoToggle();
		ToggleMenu();
	}

	public static void WinNoToggle()
	{
		if (FFSNetwork.IsHost)
		{
			Synchronizer.SendGameAction(GameActionType.DebugWinScenario);
		}
		Choreographer.s_Choreographer.WinScenario();
	}

	public void Lose()
	{
		LoseNoToggle();
		ToggleMenu();
	}

	public static void LoseNoToggle()
	{
		if (FFSNetwork.IsHost)
		{
			Synchronizer.SendGameAction(GameActionType.DebugLoseScenario);
		}
		Choreographer.s_Choreographer.LoseScenario();
	}

	public void Restart()
	{
		ToggleMenu();
		(Singleton<ESCMenu>.Instance as UIScenarioEscMenu).RestartScenario();
	}

	public void GainXP()
	{
		if (selectedCharacter != null && int.TryParse(GainXPText.text, out var result))
		{
			selectedCharacter.GainXP(result);
		}
	}

	public void SpeedUp()
	{
		SaveData.Instance.Global.DebugSpeedMode = true;
		if (TimeManager.DefaultTimeScale < 10f)
		{
			TimeManager.DefaultTimeScale += 0.02f;
		}
		else
		{
			TimeManager.DefaultTimeScale = 10f;
		}
		DisplayTimeScaleText();
	}

	public void SlowDown()
	{
		SaveData.Instance.Global.DebugSpeedMode = true;
		if (TimeManager.DefaultTimeScale >= 0.02f)
		{
			TimeManager.DefaultTimeScale -= 0.02f;
		}
		else
		{
			TimeManager.DefaultTimeScale = 0f;
		}
		DisplayTimeScaleText();
	}

	public void ForceDesync()
	{
		if (FFSNetwork.IsOnline)
		{
			FFSNetwork.HandleDesync(new Exception("Desync triggered by debug menu"));
		}
	}

	public void CompareScenarioStates()
	{
		if (FFSNetwork.IsOnline)
		{
			Choreographer.s_Choreographer.StartMPEndOfRoundCompare();
		}
	}

	public void GainMoneyTokens()
	{
		if (selectedCharacter != null && int.TryParse(GainMoneyTokenText.text, out var result))
		{
			selectedCharacter.AddGold(result);
			if (UIManager.Instance != null)
			{
				UIManager.Instance.OnGoldValueChanged();
			}
		}
	}

	public void SetHealth()
	{
		if (selectedCharacter != null && int.TryParse(SetHealthText.text, out var result))
		{
			if (result < selectedCharacter.Health && result > 0)
			{
				selectedCharacter.Damaged(selectedCharacter.Health - result, fromAttackAbility: false, null, null);
			}
			else if (result > selectedCharacter.Health && result <= selectedCharacter.MaxHealth)
			{
				selectedCharacter.Healed(result - selectedCharacter.Health);
			}
		}
	}

	public void OnSendErrorReport()
	{
		SceneController.Instance.GlobalErrorMessage.ShowSendErrorReport("Debug Error Report");
		ToggleMenu();
	}

	public void ToggleDebugKeys()
	{
		isDebugKeysShowing = !isDebugKeysShowing;
		if (isDebugKeysShowing)
		{
			ScenarioOptions.SetActive(value: false);
			DebugKeys.SetActive(value: true);
			ShowDebugKeysText.text = "Show Debug Menu";
		}
		else if (SaveData.Instance.Global.CurrentGameState == EGameState.Map && Singleton<MapChoreographer>.Instance != null)
		{
			ScenarioOptions.SetActive(value: false);
			NewAdventureMapOptions.SetActive(value: true);
			DebugKeys.SetActive(value: false);
			ShowDebugKeysText.text = "Show Debug Keys";
		}
		else
		{
			ScenarioOptions.SetActive(value: true);
			NewAdventureMapOptions.SetActive(value: false);
			DebugKeys.SetActive(value: false);
			ShowDebugKeysText.text = "Show Debug Keys";
		}
	}

	public void TogglePause()
	{
		if (!FFSNetwork.IsOnline)
		{
			if (TimeManager.IsPaused)
			{
				TimeManager.UnpauseTime();
				Time.timeScale = 1f;
			}
			else
			{
				TimeManager.PauseTime();
				Time.timeScale = 0f;
			}
			DisplayTimeScaleText();
		}
		else
		{
			FFSNet.Console.LogWarning("Tried using debug menu pause time. Feature disabled in multiplayer.");
		}
	}

	public void ToggleGameSpeedText(bool value)
	{
		ShowSpeedUp = value;
	}

	public void ToggleGameSpeedUp()
	{
		SaveData.Instance.Global.SpeedUpToggle = !SaveData.Instance.Global.SpeedUpToggle;
	}

	private void DisplayTimeScaleText()
	{
		displayTextList.Add(1, "Time Scale: " + TimeManager.TimeScale.ToString("0.00"));
	}

	public void ToggleWallFade()
	{
		if (isWallFadeDisabled)
		{
			Shader.SetGlobalInt("ToggleWallFade", 1);
			isWallFadeDisabled = false;
		}
		else
		{
			Shader.SetGlobalInt("ToggleWallFade", 0);
			isWallFadeDisabled = true;
		}
	}

	public void DisplayDebugText(int id, string text)
	{
		displayTextList.Add(id, text);
	}

	public void SpawnSpawnerAtLocation(CClientTile tile)
	{
		List<CTile> allAdjacentTiles = ScenarioManager.GetAllAdjacentTiles(tile.m_Tile);
		allAdjacentTiles.Add(tile.m_Tile);
		List<CClientTile> list = new List<CClientTile>();
		foreach (CTile item in allAdjacentTiles)
		{
			list.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y]);
		}
		if (LevelEditorController.SpawnSpawnerAtLocation(list))
		{
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			ToggleMenu();
		}
	}

	public void ClearAllAchievements()
	{
		PlatformLayer.Stats.ClearAllAchievements();
	}

	public void ProxyDebugAddItem(GameAction action)
	{
		CItem cItem = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData x) => x.ID == action.SupplementaryDataIDMax)?.GetItem;
		if (cItem != null)
		{
			CInventory cInventory = Choreographer.s_Choreographer.FindPlayerActor(action.ActorID)?.Inventory;
			if (cInventory != null)
			{
				FFSNet.Console.LogInfo("PROXY: Adding item (ID: " + cItem.ID + ", Name: " + cItem.Name + ")");
				cInventory.AddItem(cItem, overrideExisting: true);
				return;
			}
			throw new Exception("Error adding item (ID: " + cItem.ID + ", Name: " + cItem.Name + "). Cannot find Actor with ID: " + action.ActorID);
		}
		throw new Exception("Error adding item. Cannot find item with ID: " + action.SupplementaryDataIDMax);
	}
}
