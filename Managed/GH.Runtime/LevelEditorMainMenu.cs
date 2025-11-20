using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLOOM.MainMenu;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorMainMenu : MonoBehaviour
{
	[Header("File List")]
	public GameObject FileListItemPrefab;

	public RectTransform FileListItemParent;

	[Space]
	public TMP_InputField LevelFolderPathInput;

	public ExtendedToggle UseDefaultPathInputToggle;

	public GameObject SetLevelFolderContainer;

	[Header("File Details Panel")]
	public TextMeshProUGUI LevelNameLabel;

	public TextMeshProUGUI LevelVersionLabel;

	public GameObject LevelDLCLabelWrapper;

	public TextMeshProUGUI LevelDLCLabel;

	public TextMeshProUGUI LevelTypeLabel;

	public TextMeshProUGUI LevelDateLabel;

	public TextMeshProUGUI LevelSpawnTypeLabel;

	public GameObject LevelVersionLabelGroup;

	public GameObject EditSelectedFileButton;

	public GameObject EditSelectedFileLockedButton;

	public GameObject SteamWorkshopGroup;

	public GameObject SteamWorkshopGroupLocked;

	public GameObject UploadToSteamButton;

	public GameObject UpdateFromSteamButton;

	public LevelEditorMainMenuModUpload LevelModUploadScreen;

	public GameObject PlayButtonGO;

	public GameObject PlayButtonLockedGO;

	public GameObject FileNeedsSelectingObject;

	[Header("Party Selection Option Panel")]
	public GameObject PartySelectionOptionPanel;

	public TextMeshProUGUI PartySelectionOptionText;

	public LayoutElement LoadExistingPartyElement;

	public GameObject LoadExistingPartyGO;

	public GameObject LoadExistingPartyLockedGO;

	public CustomPartySetup PartySetup;

	[Header("Buttons")]
	public GameObject CreateNewLevelButton;

	public GameObject CreateNewLevelLockedButton;

	public GameObject BulkProcessLevelsInLevelEditorButton;

	[Header("Dev-Only")]
	public Transform[] ComponentsToBeHiddenForNonDev;

	[Header("AutoTest")]
	public TMP_InputField AutotestFolderPathInput;

	public TMP_Dropdown AutotestLoadDropdown;

	[Space]
	public TextMeshProUGUI StatusText;

	private bool m_CachedSortDirection;

	private List<LevelEditorMainMenuFileListItem> m_FileListItems;

	private LevelEditorMainMenuFileListItem m_CurrentlySelectedFileItem;

	private bool m_OverrideModdingEnabled;

	private const bool c_ShouldDefaultAllowBulkLevelProcessing = false;

	public CCustomLevelData CurrentLevelData { get; private set; }

	private bool m_ShouldShowModdingControls
	{
		get
		{
			if (!m_OverrideModdingEnabled)
			{
				return SaveData.Instance.IsModdingEnabled;
			}
			return true;
		}
	}

	private void Awake()
	{
		m_FileListItems = new List<LevelEditorMainMenuFileListItem>();
		RefreshUIFromDataManagers();
	}

	private void Start()
	{
		Transform[] componentsToBeHiddenForNonDev = ComponentsToBeHiddenForNonDev;
		for (int i = 0; i < componentsToBeHiddenForNonDev.Length; i++)
		{
			componentsToBeHiddenForNonDev[i].gameObject.SetActive(value: false);
		}
		CreateNewLevelButton.SetActive(m_ShouldShowModdingControls);
		EditSelectedFileButton.SetActive(m_ShouldShowModdingControls);
		SteamWorkshopGroup.SetActive(m_ShouldShowModdingControls);
		GameObject bulkProcessLevelsInLevelEditorButton = BulkProcessLevelsInLevelEditorButton;
		if ((object)bulkProcessLevelsInLevelEditorButton != null)
		{
			_ = m_ShouldShowModdingControls;
			bulkProcessLevelsInLevelEditorButton.SetActive(value: false);
		}
		CreateNewLevelLockedButton.SetActive(!m_ShouldShowModdingControls);
		EditSelectedFileLockedButton.SetActive(!m_ShouldShowModdingControls);
		SteamWorkshopGroupLocked.SetActive(!m_ShouldShowModdingControls);
		UseDefaultPathInputToggle.SetIsOnWithoutNotify(!SaveData.Instance.Global.UseCustomLevelDataFolder);
		SetLevelFolderContainer.SetActive(!UseDefaultPathInputToggle.isOn);
		LevelFolderPathInput.text = SaveData.Instance.Global.CustomLevelDataFolder;
	}

	private void OnEnable()
	{
		GameObject bulkProcessLevelsInLevelEditorButton = BulkProcessLevelsInLevelEditorButton;
		if ((object)bulkProcessLevelsInLevelEditorButton != null)
		{
			_ = m_ShouldShowModdingControls;
			bulkProcessLevelsInLevelEditorButton.SetActive(value: false);
		}
		SaveData.Instance.AutoTestDataManager.CurrentlyRunningAutotestFile = null;
		if (PlatformLayer.FileSystem.ExistsDirectory(SaveData.Instance.Global.LastAutotestFolder))
		{
			SaveData.Instance.AutoTestDataManager.CurrentAutotestPath = SaveData.Instance.Global.LastAutotestFolder;
			AutotestFolderPathInput.text = SaveData.Instance.Global.LastAutotestFolder;
		}
		RefreshUIFromDataManagers();
	}

	public void ShowLevelEditorScreen()
	{
		LevelEditorController.s_Instance.SetLevelEditorState(LevelEditorController.ELevelEditorState.Idle);
		base.gameObject.SetActive(value: true);
	}

	private void ClearUI()
	{
		m_CurrentlySelectedFileItem = null;
		FileNeedsSelectingObject.SetActive(m_CurrentlySelectedFileItem == null);
		foreach (LevelEditorMainMenuFileListItem fileListItem in m_FileListItems)
		{
			Object.Destroy(fileListItem.gameObject);
		}
		m_FileListItems.Clear();
	}

	public void RefreshUIFromDataManagers(bool fromSort = false)
	{
		ClearUI();
		string text = string.Empty;
		bool useModdedFolder = SceneController.Instance.Modding?.LevelEditorRuleset != null;
		if (!fromSort)
		{
			SaveData.Instance.LevelEditorDataManager.DetermineAvailableFilesFromLoadFolder(SaveData.Instance.Global.UseCustomLevelDataFolder, useModdedFolder);
		}
		for (int i = 0; i < SaveData.Instance.LevelEditorDataManager.AvailableFiles.Count; i++)
		{
			FileInfo fileInfo = SaveData.Instance.LevelEditorDataManager.AvailableFiles[i];
			LevelEditorMainMenuFileListItem component = Object.Instantiate(FileListItemPrefab, FileListItemParent).GetComponent<LevelEditorMainMenuFileListItem>();
			m_FileListItems.Add(component);
			component.InitForFileInfo(fileInfo, i, UpdateDetailsUIForSelectedListItem);
		}
		if (PlatformLayer.FileSystem.ExistsDirectory(SaveData.Instance.AutoTestDataManager.CurrentAutotestPath))
		{
			if (!fromSort)
			{
				SaveData.Instance.AutoTestDataManager.DetermineAvailableFilesFromLoadFolder();
			}
			AutotestLoadDropdown.options.Clear();
			AutotestLoadDropdown.AddOptions(SaveData.Instance.AutoTestDataManager.AutoTestFiles.Select((FileInfo f) => Path.GetFileName(f.Name)).ToList());
			AutotestFolderPathInput.text = SaveData.Instance.AutoTestDataManager.CurrentAutotestPath;
		}
		else
		{
			text += "Invalid Autotest folder path.\n";
		}
		StatusText.text = text;
	}

	public void UpdateDetailsUIForSelectedListItem(LevelEditorMainMenuFileListItem pressedItem)
	{
		m_CurrentlySelectedFileItem = pressedItem;
		StatusText.text = string.Empty;
		if (pressedItem?.LevelData != null)
		{
			LevelVersionLabelGroup.SetActive(value: true);
			CurrentLevelData = pressedItem.LevelData;
			if (SceneController.Instance.Modding?.LevelEditorRuleset != null && pressedItem.LevelData.PartySpawnType == ELevelPartyChoiceType.LoadAdventureParty)
			{
				PlayButtonGO.SetActive(value: false);
				PlayButtonLockedGO.SetActive(value: true);
			}
			else
			{
				PlayButtonGO.SetActive(m_ShouldShowModdingControls);
				PlayButtonLockedGO.SetActive(!m_ShouldShowModdingControls);
			}
			LevelNameLabel.text = pressedItem.LevelData.Name;
			LevelDateLabel.text = pressedItem.DataFileInfo.LastWriteTime.ToString("dd/MM/yyyy HH:mm");
			LevelSpawnTypeLabel.text = pressedItem.LevelData.PartySpawnType.ToString();
			LevelDLCLabelWrapper.SetActive(value: false);
			ModMetadata modMetadataForCustomLevel = SaveData.Instance.LevelEditorDataManager.GetModMetadataForCustomLevel(pressedItem.LevelData);
			if (modMetadataForCustomLevel != null)
			{
				LevelVersionLabel.text = modMetadataForCustomLevel.Version.ToString();
				SteamWorkshopGroup.SetActive(m_ShouldShowModdingControls);
				SteamWorkshopGroupLocked.SetActive(!m_ShouldShowModdingControls);
				if (SaveData.Instance.LevelEditorDataManager.CheckIfLevelIsFromWorkshop(pressedItem.LevelData, pressedItem.DataFileInfo))
				{
					UploadToSteamButton.SetActive(value: false);
					if (SaveData.Instance.LevelEditorDataManager.CheckIfWorkshopLevelNeedUpdate(pressedItem.LevelData))
					{
						UpdateFromSteamButton.SetActive(value: true);
						LevelTypeLabel.text = "Workshop Level *UPDATE AVAILABLE*";
					}
					else
					{
						SteamWorkshopGroup.SetActive(value: false);
						SteamWorkshopGroupLocked.SetActive(value: false);
						UpdateFromSteamButton.SetActive(value: false);
						LevelTypeLabel.text = "Workshop Level";
					}
				}
				else
				{
					LevelTypeLabel.text = "Local Custom Level";
					UploadToSteamButton.SetActive(!SaveData.Instance.Global.UseCustomLevelDataFolder);
					UpdateFromSteamButton.SetActive(value: false);
				}
			}
			else
			{
				LevelTypeLabel.text = "Custom Level";
				LevelVersionLabelGroup.SetActive(value: false);
				SteamWorkshopGroup.SetActive(value: false);
				SteamWorkshopGroupLocked.SetActive(value: false);
			}
		}
		else
		{
			Debug.LogError("Error parsing Data from file");
			m_CurrentlySelectedFileItem = null;
		}
		FileNeedsSelectingObject.SetActive(m_CurrentlySelectedFileItem == null);
	}

	public void OnSortListByFilenamePressed()
	{
		m_CachedSortDirection = !m_CachedSortDirection;
		SaveData.Instance.LevelEditorDataManager.SortAvailableFiles(LevelEditorDataManager.ELevelFileSortType.Filename, m_CachedSortDirection);
		RefreshUIFromDataManagers(fromSort: true);
	}

	public void OnSortListByDatePressed()
	{
		m_CachedSortDirection = !m_CachedSortDirection;
		SaveData.Instance.LevelEditorDataManager.SortAvailableFiles(LevelEditorDataManager.ELevelFileSortType.DateModified, m_CachedSortDirection);
		RefreshUIFromDataManagers(fromSort: true);
	}

	public void OnSortListByTypePressed()
	{
		m_CachedSortDirection = !m_CachedSortDirection;
		SaveData.Instance.LevelEditorDataManager.SortAvailableFiles(LevelEditorDataManager.ELevelFileSortType.Extension, m_CachedSortDirection);
		RefreshUIFromDataManagers(fromSort: true);
	}

	public void OnPlaySelectedFilePressed()
	{
		if (m_CurrentlySelectedFileItem == null)
		{
			return;
		}
		if (m_CurrentlySelectedFileItem.LevelData != null)
		{
			if (!PlatformLayer.DLC.CanPlayCustomLevel(m_CurrentlySelectedFileItem.LevelData))
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation("TEMP NEEDS LOCALISATION : DLC Needed", "TEMP NEEDS LOCALISATION : You need dlc to play this level");
			}
			else if (m_CurrentlySelectedFileItem.LevelData.PartySpawnType == ELevelPartyChoiceType.LoadAdventureParty)
			{
				OnPartySelectionLoadExistingPressed();
			}
			else if (m_CurrentlySelectedFileItem.LevelData.PartySpawnType == ELevelPartyChoiceType.ChooseOwnParty || m_CurrentlySelectedFileItem.LevelData.PartySpawnType == ELevelPartyChoiceType.ChooseOwnPartyRestricted)
			{
				PartySelectionOptionPanel.SetActive(value: true);
				PartySelectionOptionText.text = ((m_CurrentlySelectedFileItem.LevelData.PartySpawnType == ELevelPartyChoiceType.ChooseOwnParty) ? "Playing a level of this type requires that you create a party or choose one from an existing save game" : "Playing a level of this type requires that you create a party");
				if (SaveData.Instance.Global.AllAdventures.Length == 0)
				{
					LoadExistingPartyGO.SetActive(value: false);
					LoadExistingPartyLockedGO.SetActive(value: true);
				}
				else
				{
					LoadExistingPartyGO.SetActive(value: true);
					LoadExistingPartyLockedGO.SetActive(value: false);
				}
			}
			else
			{
				LevelEditorController.s_Instance.SetLevelEditorState(LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel);
				SaveData.Instance.LoadCustomLevelFromData(m_CurrentlySelectedFileItem.LevelData, LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel);
				SceneController.Instance.LoadCustomLevel(SaveData.Instance.Global.CurrentCustomLevelData);
			}
		}
		else
		{
			Debug.LogError("No LevelData was found");
		}
	}

	public void OnEditSelectedFilePressed()
	{
		if (!(m_CurrentlySelectedFileItem == null))
		{
			if (m_CurrentlySelectedFileItem.LevelData != null)
			{
				SaveData.Instance.LoadCustomLevelFromData(m_CurrentlySelectedFileItem.LevelData, LevelEditorController.ELevelEditorState.Editing);
				SceneController.Instance.LevelEditorStart();
			}
			else
			{
				Debug.LogError("No LevelData was found");
			}
		}
	}

	public void OnCreateNewLevelPressed()
	{
		SaveData.Instance.Global.CurrentEditorLevelData = new CCustomLevelData();
		if (SaveData.Instance.Global.CurrentEditorLevelData == null)
		{
			Debug.LogError("Failed to create party data for level");
			return;
		}
		List<CMap> list = new List<CMap>();
		CMap.SetChildren(list);
		foreach (CMap item in list)
		{
			item.Revealed = true;
		}
		ApparanceStyle style = new ApparanceStyle(ScenarioPossibleRoom.EBiome.Crypt);
		ScenarioState scenarioState = new ScenarioState("Name", "Description", "ID", SharedClient.GlobalRNG.Next(), 1, EScenarioType.Custom, "FileName", new List<CObjective>(), new List<CObjective>(), new List<CScenarioModifier>(), style, new List<string>(), new List<string>())
		{
			Maps = list
		};
		SceneController.Instance.LevelEditorStart(scenarioState);
	}

	public void OpenLoadPartyDialogForLevelData(CCustomLevelData levelData)
	{
		base.gameObject.SetActive(value: false);
		Singleton<UILoadGameWindow>.Instance.Show(new LoadCustomLevelDataService(levelData), delegate
		{
			ShowLevelEditorScreen();
			LevelEditorController.s_Instance?.QuitLevelEditor();
		});
	}

	public void OnUseDefaultLevelFolderToggled(bool value)
	{
		SaveData.Instance.Global.UseCustomLevelDataFolder = !value;
		SetLevelFolderContainer.SetActive(!value);
		RefreshUIFromDataManagers();
	}

	public void SetCustomLevelDataFolderPressed()
	{
		if (SaveData.Instance.LevelEditorDataManager.TrySetCustomLevelDataPath(LevelFolderPathInput.text, out var errorLog))
		{
			RefreshUIFromDataManagers();
			return;
		}
		ClearUI();
		StatusText.text = errorLog;
	}

	public void OnBulkProcessLevelsInLevelEditorPressed()
	{
	}

	public void OnPartySelectionCreateNew()
	{
		PartySelectionOptionPanel.SetActive(value: false);
		base.gameObject.SetActive(value: false);
		PartySetup.ShowForCustomLevel(m_CurrentlySelectedFileItem.LevelData);
		m_CurrentlySelectedFileItem = null;
		FileNeedsSelectingObject.SetActive(value: false);
	}

	public void OnPartySelectionLoadExistingPressed()
	{
		PartySelectionOptionPanel.SetActive(value: false);
		OpenLoadPartyDialogForLevelData(m_CurrentlySelectedFileItem.LevelData);
		m_CurrentlySelectedFileItem = null;
		FileNeedsSelectingObject.SetActive(value: false);
	}

	public void OnPartySelectionCancelPressed()
	{
		PartySelectionOptionPanel.SetActive(value: false);
	}

	public void SetCustomAutotestFolder()
	{
		SaveData.Instance.AutoTestDataManager.CurrentAutotestPath = AutotestFolderPathInput.text;
		RefreshUIFromDataManagers();
	}

	public void ResetAutotestFolderToDefault()
	{
		SaveData.Instance.AutoTestDataManager.CurrentAutotestPath = RootSaveData.AutoTestPath;
		RefreshUIFromDataManagers();
	}

	public void EditAutotest()
	{
		SaveData.Instance.LoadAutoTestFromData(SaveData.Instance.AutoTestDataManager.GetAutotestDataFromFile(SaveData.Instance.AutoTestDataManager.AutoTestFiles[AutotestLoadDropdown.value]), SaveData.Instance.AutoTestDataManager.AutoTestFiles[AutotestLoadDropdown.value], loadIntoEditor: true);
		SceneController.Instance.LevelEditorStart();
	}

	public void PlayAutotest()
	{
		SaveData.Instance.LoadAutoTestFromData(SaveData.Instance.AutoTestDataManager.GetAutotestDataFromFile(SaveData.Instance.AutoTestDataManager.AutoTestFiles[AutotestLoadDropdown.value]), SaveData.Instance.AutoTestDataManager.AutoTestFiles[AutotestLoadDropdown.value]);
		SceneController.Instance.AutoTestStart(SaveData.Instance.Global.CurrentAutoTestDataCopy, EAutoTestControllerState.PlayFromMenu);
	}

	public void BulkRunAllAutoTests()
	{
		SceneController.Instance.ShowLoadingScreen();
		AutoTestController.s_Instance.EvaluateAllAvailableTestsAndReportString(null);
	}

	public void OnUploadToSteamPressed()
	{
		LevelModUploadScreen.SetUploadingNewLevel(CurrentLevelData);
	}

	public void OnUpdateFromSteamPressed()
	{
		if (!(m_CurrentlySelectedFileItem == null))
		{
			SaveData.Instance.LevelEditorDataManager.UpdateWorkshopLevel(m_CurrentlySelectedFileItem.LevelData);
			RefreshUIFromDataManagers();
		}
	}
}
