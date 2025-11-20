#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Assets.Script.Misc;
using Chronos;
using FFSNet;
using GLOO.Introduction;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.PhaseManager;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.Events;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.VisibilitySpheres;
using Photon.Bolt;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using SharedLibrary.SimpleLog;
using UnityEngine;
using VolumetricFogAndMist;

public class MapChoreographer : Singleton<MapChoreographer>
{
	public enum ECustomMapAlignment
	{
		None,
		NorthWest,
		NorthEast,
		SouthWest,
		SouthEast
	}

	[SerializeField]
	private GameObject m_ScenariosParent;

	[SerializeField]
	private GameObject m_VillagesParent;

	[SerializeField]
	private PartyToken m_PartyToken;

	[SerializeField]
	private MapLocation mapLocationPrefab;

	[SerializeField]
	private MapMovementFlow movementFlow;

	[Header("Maps")]
	[SerializeField]
	private MapConfig worldMapConfig;

	[SerializeField]
	private GameObject worldMap;

	[SerializeField]
	private MapConfig cityMapConfig;

	[SerializeField]
	private GameObject cityMap;

	[SerializeField]
	private MapTransitioner mapTransition;

	[SerializeField]
	private GameObject _mapCanvas;

	[SerializeField]
	private GameObject _campaignCanvas;

	[SerializeField]
	private GameObject _mapEscMenuCanvas;

	[SerializeField]
	private GameObject _newPartyDisplayUI;

	[SerializeField]
	private GameObject _guildmasterHUD;

	[Header("Fog Reveal")]
	[SerializeField]
	private float m_FogAnimationSpeed = 0.01f;

	[SerializeField]
	private float m_FogAlphaDecrement = 0.01f;

	public static volatile List<CMapClientMessage> m_MessageQueue = new List<CMapClientMessage>();

	public CRoadEvent RoadEventDebugOverride;

	public CRoadEvent CityEventDebugOverride;

	private List<MapLocation> m_Villages = new List<MapLocation>();

	private List<MapLocation> m_Scenarios = new List<MapLocation>();

	private List<MapLocation> m_CityLocations = new List<MapLocation>();

	private List<UILocationMapMarker> m_Streets = new List<UILocationMapMarker>();

	private List<UILocationMapMarker> m_WorldMapLabels = new List<UILocationMapMarker>();

	private List<VisibilitySphereYML.VisibilitySphereDefinition> m_RevealedVisibilitySpheres = new List<VisibilitySphereYML.VisibilitySphereDefinition>();

	private MapLocation m_CurrentLocation;

	private MapLocation m_QuestStartLocation;

	private MapLocation m_QuestEndLocation;

	private MapLocation m_TempCurrentLocation;

	private MapLocation m_QueuedMoveLocation;

	private MapLocation m_LinkedQuestLocation;

	private List<DialogLineDTO> m_IntroGloomhavenLines = new List<DialogLineDTO>();

	private List<DialogLineDTO> m_IntroTravelLines = new List<DialogLineDTO>();

	private List<DialogLineDTO> m_OutroTravelLines = new List<DialogLineDTO>();

	private List<DialogLineDTO> m_OutroGloomhavenLines = new List<DialogLineDTO>();

	private bool m_Initialised;

	private bool m_IsMoving;

	private bool m_LocationSelected;

	private bool m_ShowingQuestRewards;

	private bool m_ShowingQuestMessages;

	private bool m_BlockMapClientMessageProcessing;

	private bool m_ShouldEncounterRoadEvent;

	private bool m_EventComplete;

	private Vector3[] m_WaypointsToEnd;

	private bool m_ResultsProcessFinished;

	private ActionProgressionManager actionProgression;

	private List<NetworkPlayer> m_PlayersFinishedSendingQuestCompletionData = new List<NetworkPlayer>();

	private Dictionary<NetworkPlayer, List<QuestCompletionToken>> m_QuestCompletionsToImport = new Dictionary<NetworkPlayer, List<QuestCompletionToken>>();

	private IEnumerator m_ShowQuestCompletionsToImportCoroutine;

	private bool m_ShowAllScenariosMode;

	private HashSet<string> m_RecentUnlockedQuestLocations = new HashSet<string>();

	private const string Alb = "_Alb";

	private const string MainTex = "_MainTex";

	private static readonly Color32 EnterDungeonTextColor = new Color32(243, 221, 171, byte.MaxValue);

	private const string EnterDungeonKey = "Consoles/GUI_HOTKEY_SELECT_QUEST";

	private const string SelectQuestKey = "Consoles/GUI_HOTKEY_SELECT_QUEST";

	[NonSerialized]
	public bool AreAllCanvasesLoaded = true;

	private float zoomBeforeCityEvent;

	public List<CMapCharacter> QueuedRetirements = new List<CMapCharacter>();

	public MapLocation CurrentLocation => m_CurrentLocation;

	public MapLocation MovingToLocation { get; private set; }

	public bool ShowAllScenariosMode => m_ShowAllScenariosMode;

	public bool PartyAtHQ
	{
		get
		{
			if (CurrentLocation != null && CurrentLocation.MapLocationType == MapLocation.EMapLocationType.Headquarters)
			{
				return !m_IsMoving;
			}
			return false;
		}
	}

	public List<MapLocation> CityQuestLocations => m_CityLocations.FindAll((MapLocation it) => it.LocationQuest != null);

	public MapLocation HeadquartersLocation => m_Villages.SingleOrDefault((MapLocation x) => x.MapLocationType == MapLocation.EMapLocationType.Headquarters);

	public MapChoreographerUIEvents EventBuss { get; private set; }

	public bool ImportingQuestCompletions { get; private set; }

	public bool AutoPlayCityEvent { get; set; }

	protected override void Awake()
	{
		base.Awake();
		MapRuleLibraryClient.Instance.SetMessageHandler(MessageHandler);
		m_BlockMapClientMessageProcessing = false;
		actionProgression = new ActionProgressionManager();
		actionProgression.RequestPauseActions(this);
		EventBuss = new MapChoreographerUIEvents();
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	protected override void OnDestroy()
	{
		StopAllCoroutines();
		EventBuss.ClearEvents();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		if (CameraController.s_CameraController != null)
		{
			CameraController.s_CameraController.SetMapInputSource(value: false);
		}
		Unsubscribe();
		if (!CoreApplication.IsQuitting)
		{
			actionProgression.ClearActions();
			UIMultiplayerNotifications.HideAllMultiplayerNotification(instant: true);
			Singleton<UIPersonalQuestResultManager>.Instance?.HideAllPersonalQuestNotifications(instant: true);
			UIMapNotifications.HideAllNotifications();
			Singleton<MultiplayerImportProgressManager>.Instance?.Cancel();
		}
		m_MessageQueue.Clear();
		base.OnDestroy();
	}

	private void Unsubscribe()
	{
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Remove(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnterRoom));
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningPlayerLeft));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		ControllableRegistry.OnControllableDestroyed = (ControllablesChangedEvent)Delegate.Remove(ControllableRegistry.OnControllableDestroyed, new ControllablesChangedEvent(OnControllableDestroyed));
		ControllableRegistry.OnControllableObjectChanged = (ControllableObjectChangedEvent)Delegate.Remove(ControllableRegistry.OnControllableObjectChanged, new ControllableObjectChangedEvent(OnControllableObjectChanged));
	}

	private IEnumerator Start()
	{
		yield return PartialUILoad();
		m_QuestCompletionsToImport = new Dictionary<NetworkPlayer, List<QuestCompletionToken>>();
		m_PlayersFinishedSendingQuestCompletionData = new List<NetworkPlayer>();
		List<MapConfigYMLData> mapConfigs = ScenarioRuleClient.SRLYML.MapConfigs;
		if (mapConfigs != null && mapConfigs.Count > 0)
		{
			Dictionary<ECustomMapAlignment, Texture2D> moddedMapImages = SceneController.Instance.YML.ModdedMapImages;
			Material[] materials = worldMap.GetComponent<MeshRenderer>().materials;
			foreach (Material material in materials)
			{
				if (material.name.Contains("GH_WorldMap_01_New"))
				{
					material.SetTexture("_Alb", moddedMapImages[ECustomMapAlignment.NorthWest]);
				}
				else if (material.name.Contains("GH_WorldMap_02_New"))
				{
					material.SetTexture("_Alb", moddedMapImages[ECustomMapAlignment.NorthEast]);
				}
				else if (material.name.Contains("GH_WorldMap_03_New"))
				{
					material.SetTexture("_Alb", moddedMapImages[ECustomMapAlignment.SouthWest]);
				}
				else if (material.name.Contains("GH_WorldMap_04_New"))
				{
					material.SetTexture("_Alb", moddedMapImages[ECustomMapAlignment.SouthEast]);
				}
				else if (material.name.Contains("GH_WorldMap_01"))
				{
					material.SetTexture("_MainTex", moddedMapImages[ECustomMapAlignment.NorthWest]);
				}
				else if (material.name.Contains("GH_WorldMap_02"))
				{
					material.SetTexture("_MainTex", moddedMapImages[ECustomMapAlignment.NorthEast]);
				}
				else if (material.name.Contains("GH_WorldMap_03"))
				{
					material.SetTexture("_MainTex", moddedMapImages[ECustomMapAlignment.SouthWest]);
				}
				else if (material.name.Contains("GH_WorldMap_04"))
				{
					material.SetTexture("_MainTex", moddedMapImages[ECustomMapAlignment.SouthEast]);
				}
			}
		}
		SaveData.Instance.Global.StopSpeedUp();
		while (!CameraController.s_CameraController.StartComplete)
		{
			yield return null;
		}
		CameraController.s_CameraController.InitCamera();
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.destination_selection_map);
		while (PlayerRegistry.IsProfanityCheckInProcess)
		{
			yield return null;
		}
		if (!((NewPartyDisplayUI)Singleton<APartyDisplayUI>.Instance).Initialised)
		{
			yield return ((NewPartyDisplayUI)Singleton<APartyDisplayUI>.Instance).InitCoroutine(AdventureState.MapState.MapParty);
		}
		bool hasJustJoined = PlayerRegistry.LoadingInFromJoiningClient || !SceneController.Instance.GameLoadedAndClientReady;
		while (!m_Initialised)
		{
			yield return null;
		}
		yield return MultiplayerStartup();
		yield return Timekeeper.instance.WaitForSeconds(1f);
		if (FFSNetwork.IsOnline && PlayerRegistry.WaitForOtherPlayers && PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant)
		{
			Synchronizer.SendSideAction(GameActionType.NotifyLoadingFinished);
			PlayerRegistry.NotifyLoadingFinished(PlayerRegistry.MyPlayer);
			if (PlayerRegistry.WaitForOtherPlayers)
			{
				yield return SceneController.Instance.WaitForPlayers();
			}
		}
		SceneController.Instance.DisableLoadingScreen();
		Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode((m_CurrentLocation?.LocationQuest == null || m_CurrentLocation.LocationQuest.Quest.Type != EQuestType.City) ? EGuildmasterMode.WorldMap : EGuildmasterMode.City);
		if (!FFSNetwork.IsOnline && AdventureState.MapState.MapParty.CheckCharacters.Count < 2)
		{
			Singleton<UIGuildmasterHUD>.Instance.Hide(EGuildmasterOptionsLock.Enough_Characters);
		}
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		if (Singleton<MapFTUEManager>.Instance != null && Singleton<MapFTUEManager>.Instance.Process())
		{
			Singleton<QuestManager>.Instance.Show();
			Singleton<MapFTUEManager>.Instance.OnFinished.AddListener(delegate
			{
				MapViewInit();
			});
		}
		else if (!AdventureState.MapState.IsCampaign || AdventureState.MapState.JustCompletedLocationState == null)
		{
			SceneController.Instance.BusyProcessingResults = true;
			m_RecentUnlockedQuestLocations = new HashSet<string>(AdventureState.MapState.QueuedUnlockedQuestIDs);
			Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: false);
			NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true);
			StartCoroutine(ShowQueuedContent(saveAfterShow: true, delegate
			{
				MapViewInit(FFSNetwork.IsHost || !hasJustJoined);
			}, !AdventureState.MapState.IsCampaign));
		}
		else
		{
			m_RecentUnlockedQuestLocations = new HashSet<string>(AdventureState.MapState.QueuedUnlockedQuestIDs);
			MapViewInit(FFSNetwork.IsHost || !hasJustJoined);
		}
	}

	private IEnumerator PartialUILoad()
	{
		AreAllCanvasesLoaded = false;
		yield return null;
		_campaignCanvas.SetActive(value: true);
		yield return null;
		_newPartyDisplayUI.SetActive(value: true);
		_mapEscMenuCanvas.SetActive(value: true);
		yield return null;
		_mapCanvas.SetActive(value: true);
		yield return null;
		_guildmasterHUD.SetActive(value: true);
		yield return null;
		Singleton<UINewEnhancementShopInventory>.Instance.Preload();
		yield return null;
		Singleton<UIGuildmasterHUD>.Instance.UIAchievementInventory.Preload();
		AreAllCanvasesLoaded = true;
	}

	private void MapViewInit(bool readyRewards = true)
	{
		try
		{
			Singleton<QuestManager>.Instance.Show();
			SceneController.Instance.BusyProcessingResults = true;
			if (AdventureState.MapState.JustCompletedLocationState != null)
			{
				CheckForIntroMessages(m_CurrentLocation);
				m_Villages.SingleOrDefault((MapLocation x) => x.Location == AdventureState.MapState.HeadquartersState).RefreshLocationLine(m_CurrentLocation, forceRefresh: true);
				if (AdventureState.MapState.CurrentMapPhaseType != EMapPhaseType.AtLinkedScenario)
				{
					Singleton<UIGuildmasterHUD>.Instance.Show(this);
					Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: true);
					StartMove(AdventureState.MapState.HeadquartersState);
					AdventureState.MapState.JustCompletedLocationState = null;
					return;
				}
				NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true);
				Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: false);
				StartCoroutine(ShowQueuedContent(saveAfterShow: false, delegate
				{
					AdventureState.MapState.QueuedUnlockedQuestIDs.Remove(m_CurrentLocation.LocationQuest.LinkedQuestState.ID);
					List<Reward> personalQuestRewards = AdventureState.MapState.MapParty.SelectedCharacters.Where((CMapCharacter it) => it.PersonalQuest != null && it.PersonalQuest.State == EPersonalQuestState.Completed).SelectMany((CMapCharacter it) => it.PersonalQuest.CurrentPersonalQuestStepData.IsPersonalQuestStep ? it.PersonalQuest.RewardsByStep[it.PersonalQuest.CurrentPersonalQuestStep].SelectMany((RewardGroup rewardGroup) => rewardGroup.Rewards) : it.PersonalQuest.FinalRewards.SelectMany((RewardGroup rewardGroup) => rewardGroup.Rewards)).ToList();
					List<Reward> rewards = AdventureState.MapState.QueuedCompletionRewards.FindAll((Reward it) => !personalQuestRewards.Contains(it));
					AdventureState.MapState.QueuedCompletionRewards.RemoveAll((Reward it) => rewards.Contains(it));
					Singleton<UIDistributeRewardManager>.Instance.Process(rewards, delegate
					{
						Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_LINKED_QUEST_CHOOSE_TOOLTIP"), LocalizationManager.GetTranslation(m_CurrentLocation.LocationQuest.LinkedQuestState.Quest.LocalisedNameKey)));
						SceneController.Instance.BusyProcessingResults = false;
						NewPartyDisplayUI.PartyDisplay.Show(this);
						NewPartyDisplayUI.PartyDisplay.EnableCharacterSelection(enable: false, this);
						if (!AdventureState.MapState.MapParty.HasIntroduced(EIntroductionConcept.LinkedQuest.ToString()))
						{
							Singleton<UIIntroductionManager>.Instance.Show(EIntroductionConcept.LinkedQuest);
							AdventureState.MapState.MapParty.MarkIntroDone(EIntroductionConcept.LinkedQuest.ToString());
						}
					}, null, readyRewards);
				}, clearQueuedRewards: false));
				CurrentLocation.HideQuestMapMarker(this);
			}
			else
			{
				AdventureState.MapState.QueuedUnlockedQuestIDs.Clear();
				Singleton<UIGuildmasterHUD>.Instance.Show(this);
				Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: true);
				NewPartyDisplayUI.PartyDisplay.Show(this);
				Singleton<UIDistributeRewardManager>.Instance.Process(AdventureState.MapState.QueuedCompletionRewards, delegate
				{
					AdventureState.MapState.QueuedCompletionRewards.Clear();
					SaveData.Instance.SaveCurrentAdventureData();
					OnFinishedProcessingResults();
				}, null, readyRewards);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the MapViewInit function\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnFinishedProcessingResults()
	{
		actionProgression.RequestResumeActions(this, "RetireCharacter");
		StartCoroutine(WaitAllRetired(delegate
		{
			m_ResultsProcessFinished = true;
			ICallbackPromise callbackPromise = CallbackPromise.Resolved();
			Debug.LogGUI("OnFinishedProcessingResults");
			if (FFSNetwork.IsOnline)
			{
				foreach (CMapCharacter item in AdventureState.MapState.MapParty.SelectedCharacters.Where((CMapCharacter it) => it.IsUnderMyControl && it.PersonalQuest != null && it.PersonalQuest.State == EPersonalQuestState.Completed && it.PersonalQuest.CurrentPersonalQuestStep >= it.PersonalQuest.PersonalQuestSteps - 1).ToList())
				{
					AdventureState.MapState.MapParty.RetireCharacter(item.CharacterID);
				}
				foreach (CMapCharacter character in AdventureState.MapState.MapParty.SelectedCharacters.Where((CMapCharacter it) => it.IsUnderMyControl && it.PersonalQuest != null && it.PersonalQuest.State == EPersonalQuestState.Completed && it.PersonalQuest.CurrentPersonalQuestStep < it.PersonalQuest.PersonalQuestSteps - 1).ToList())
				{
					PersonalQuestDTO personalQuest = new PersonalQuestDTO(character.PersonalQuest);
					character.PersonalQuest.NextPersonalQuestStep();
					callbackPromise = callbackPromise.Then(() => Singleton<UIPersonalQuestResultManager>.Instance.CreateOtherPlayerProgressedPersonalQuest(character, personalQuest));
				}
			}
			callbackPromise.Done(delegate
			{
				actionProgression.RequestResumeActions(this);
				SceneController.Instance.BusyProcessingResults = false;
				if (FFSNetwork.IsHost)
				{
					Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
				}
				if (AutoPlayCityEvent && (!FFSNetwork.IsOnline || FFSNetwork.IsHost))
				{
					Singleton<UIGuildmasterHUD>.Instance.OpenCityEvent();
				}
				AutoPlayCityEvent = false;
				if (FFSNetwork.IsOnline && FFSNetwork.IsClient)
				{
					PartyAdventureData partyAdventureData = null;
					if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
					{
						partyAdventureData = SaveData.Instance.Global.AllCampaigns.SingleOrDefault((PartyAdventureData s) => s.PartyName == SaveData.Instance.Global.CurrentAdventureData.PartyName && ((!s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID)));
						if (partyAdventureData != null && partyAdventureData.AdventureMapState == null)
						{
							partyAdventureData.Load(EGameMode.Campaign, isJoiningMPClient: true);
						}
					}
					else
					{
						partyAdventureData = SaveData.Instance.Global.AllAdventures.SingleOrDefault((PartyAdventureData s) => s.PartyName == SaveData.Instance.Global.CurrentAdventureData.PartyName && ((!s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID)));
						if (partyAdventureData != null && partyAdventureData.AdventureMapState == null)
						{
							partyAdventureData.Load(EGameMode.Guildmaster, isJoiningMPClient: true);
						}
					}
					if (partyAdventureData != null)
					{
						List<CMapCharacter> checkCharacters = AdventureState.MapState.MapParty.CheckCharacters;
						foreach (CMapCharacter localCharacter in partyAdventureData.AdventureMapState.MapParty.CheckCharacters)
						{
							CMapCharacter cMapCharacter = checkCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == localCharacter.CharacterID && x.CharacterName == localCharacter.CharacterName);
							if (cMapCharacter != null && localCharacter.CompletedSoloQuestData.Count > 0)
							{
								IProtocolToken supplementaryDataToken = new QuestCompletionToken(localCharacter, cMapCharacter);
								Synchronizer.SendGameAction(GameActionType.SendLocalSaveFileQuestCompletion, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: true, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
							}
						}
					}
					Synchronizer.SendGameAction(GameActionType.FinishedSendingLocalSaveFileQuestCompletion, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: true);
				}
			});
		}));
	}

	private void OnGUI()
	{
	}

	private string PrintAchievements(EAchievementType type, EAchievementState state)
	{
		string text = "";
		if (AdventureState.MapState.MapParty.Achievements != null)
		{
			foreach (CPartyAchievement achievement in AdventureState.MapState.MapParty.Achievements)
			{
				if (achievement.State == state && achievement.Achievement.AchievementType == type)
				{
					text = text + achievement.ID + ", ";
				}
			}
		}
		if (text.IsNullOrEmpty())
		{
			text = "None";
		}
		return text;
	}

	private void Update()
	{
		if (m_BlockMapClientMessageProcessing)
		{
			return;
		}
		int num = 25;
		while (num-- > 0)
		{
			CMapClientMessage cMapClientMessage = null;
			lock (m_MessageQueue)
			{
				if (m_MessageQueue.Count > 0)
				{
					cMapClientMessage = m_MessageQueue[0];
					m_MessageQueue.RemoveAt(0);
				}
			}
			if (cMapClientMessage != null)
			{
				ProcessMessage(cMapClientMessage);
			}
			if (m_MessageQueue.Count == 0 || m_BlockMapClientMessageProcessing)
			{
				break;
			}
		}
	}

	public IEnumerator InitMap()
	{
		while (!AreAllCanvasesLoaded)
		{
			yield return null;
		}
		List<VisibilitySphereYML.VisibilitySphereDefinition> spheresToReveal = new List<VisibilitySphereYML.VisibilitySphereDefinition>();
		m_RevealedVisibilitySpheres.Clear();
		foreach (MapLocation village in m_Villages)
		{
			UnityEngine.Object.Destroy(village.gameObject);
		}
		m_Villages.Clear();
		foreach (MapLocation scenario in m_Scenarios)
		{
			UnityEngine.Object.Destroy(scenario.gameObject);
		}
		m_Scenarios.Clear();
		m_CityLocations.Clear();
		foreach (CLocationState item in AdventureState.MapState.AllVillages.Where((CLocationState w) => w.LocationState != ELocationState.Locked))
		{
			MapLocation mapLocation = UnityEngine.Object.Instantiate(mapLocationPrefab, m_VillagesParent.transform);
			mapLocation.transform.position = new Vector3(item.Location.MapLocation.X, item.Location.MapLocation.Y, item.Location.MapLocation.Z);
			mapLocation.Init(item, OnMapLocationSelect, OnMapLocationHighlight);
			m_Villages.Add(mapLocation);
			yield return null;
		}
		if (AdventureState.MapState.IsCampaign)
		{
			foreach (CStoreLocationState storeLocationState in AdventureState.MapState.HeadquartersState.StoreLocationStates)
			{
				MapLocation mapLocation2 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_VillagesParent.transform);
				mapLocation2.transform.position = new Vector3(storeLocationState.Location.MapLocation.X, storeLocationState.Location.MapLocation.Y, storeLocationState.Location.MapLocation.Z);
				mapLocation2.Init(storeLocationState, OnStoreMapLocationSelect, (MapLocation location, bool active) => true);
				m_CityLocations.Add(mapLocation2);
				yield return null;
			}
			foreach (Tuple<string, CVector3> streetMapLocation in AdventureState.MapState.HeadquartersState.Headquarters.StreetMapLocations)
			{
				m_Streets.Add(Singleton<MapMarkersManager>.Instance.SpawnInformationMarker(streetMapLocation.Item1, new Vector3(streetMapLocation.Item2.X, streetMapLocation.Item2.Y, streetMapLocation.Item2.Z), () => Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City));
			}
		}
		foreach (Tuple<string, CVector3> worldMapLabelLocation in AdventureState.MapState.HeadquartersState.Headquarters.WorldMapLabelLocations)
		{
			m_WorldMapLabels.Add(Singleton<MapMarkersManager>.Instance.SpawnInformationMarker(worldMapLabelLocation.Item1, new Vector3(worldMapLabelLocation.Item2.X, worldMapLabelLocation.Item2.Y, worldMapLabelLocation.Item2.Z), () => Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.City));
		}
		foreach (CQuestState item2 in AdventureState.MapState.AllQuests.Where((CQuestState w) => w.Quest.Type != EQuestType.Travel && (w.Quest.Type != EQuestType.Job || (AdventureState.MapState.TutorialCompleted && AdventureState.MapState.IntroCompleted)) && ((w.IsIntroQuest && w.QuestState == CQuestState.EQuestState.Unlocked) || (!w.IsIntroQuest && w.QuestState != CQuestState.EQuestState.Locked && w.QuestState != CQuestState.EQuestState.Blocked))))
		{
			if (!AdventureState.MapState.QueuedUnlockedQuestIDs.Contains(item2.ID))
			{
				spheresToReveal.AddRange(item2.Quest.UnlockSphereDefinitions);
			}
			if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
			{
				CMapScenarioState scenarioState = item2.ScenarioState;
				MapLocation mapLocation3 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_ScenariosParent.transform);
				if (item2 is CJobQuestState cJobQuestState)
				{
					mapLocation3.transform.position = new Vector3(cJobQuestState.JobMapLocation.X, cJobQuestState.JobMapLocation.Y, cJobQuestState.JobMapLocation.Z);
				}
				else
				{
					mapLocation3.transform.position = new Vector3(scenarioState.Location.MapLocation.X, scenarioState.Location.MapLocation.Y, scenarioState.Location.MapLocation.Z);
				}
				mapLocation3.Init(scenarioState, OnMapLocationSelect, OnMapLocationHighlight, item2);
				m_Scenarios.Add(mapLocation3);
				if (item2.InProgress)
				{
					Debug.LogWarning("Quest is in progress while we are on map screen");
				}
				if (item2.Quest.Type == EQuestType.City)
				{
					m_CityLocations.Add(mapLocation3);
				}
				yield return null;
			}
			else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
			{
				CMapScenarioState scenarioState2 = item2.ScenarioState;
				MapLocation mapLocation4 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_ScenariosParent.transform);
				if (item2 is CJobQuestState cJobQuestState2)
				{
					mapLocation4.transform.position = new Vector3(cJobQuestState2.JobMapLocation.X, cJobQuestState2.JobMapLocation.Y, cJobQuestState2.JobMapLocation.Z);
				}
				else
				{
					mapLocation4.transform.position = new Vector3(scenarioState2.Location.MapLocation.X, scenarioState2.Location.MapLocation.Y, scenarioState2.Location.MapLocation.Z);
				}
				mapLocation4.Init(scenarioState2, OnMapLocationSelect, OnMapLocationHighlight, item2);
				m_Scenarios.Add(mapLocation4);
				if (item2.InProgress)
				{
					Debug.LogWarning("Quest is in progress while we are on map screen");
				}
			}
		}
		foreach (CQuestState quest in AdventureState.MapState.AllTravelQuests.Where((CQuestState w) => (w.IsIntroQuest && w.QuestState == CQuestState.EQuestState.Unlocked) || (!w.IsIntroQuest && w.QuestState != CQuestState.EQuestState.Locked && w.QuestState != CQuestState.EQuestState.Blocked)))
		{
			if (!AdventureState.MapState.QueuedUnlockedQuestIDs.Contains(quest.ID))
			{
				spheresToReveal.AddRange(quest.Quest.UnlockSphereDefinitions);
			}
			CLocationState endingVillage = AdventureState.MapState.AllVillages.SingleOrDefault((CLocationState v) => v.ID == quest.Quest.EndingVillage);
			MapLocation mapLocation5;
			if (endingVillage.LocationState == ELocationState.Locked)
			{
				mapLocation5 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_VillagesParent.transform);
				mapLocation5.transform.position = new Vector3(endingVillage.Location.MapLocation.X, endingVillage.Location.MapLocation.Y, endingVillage.Location.MapLocation.Z);
				mapLocation5.Init(endingVillage, OnMapLocationSelect, OnMapLocationHighlight, quest);
				m_Villages.Add(mapLocation5);
				if (quest.Quest.Type == EQuestType.City)
				{
					m_CityLocations.Add(mapLocation5);
				}
			}
			else
			{
				mapLocation5 = m_Villages.Find((MapLocation x) => x.Location == endingVillage);
				mapLocation5.Init(endingVillage, OnMapLocationSelect, OnMapLocationHighlight, quest);
			}
			CMapScenarioState scenarioState3 = quest.ScenarioState;
			MapLocation mapLocation6 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_ScenariosParent.transform);
			mapLocation6.transform.position = new Vector3(scenarioState3.Location.MapLocation.X, scenarioState3.Location.MapLocation.Y, scenarioState3.Location.MapLocation.Z);
			mapLocation6.Init(scenarioState3, null, null);
			m_Scenarios.Add(mapLocation5);
			mapLocation6.gameObject.SetActive(value: false);
			mapLocation5.AddScenarioLocation(mapLocation6);
			yield return null;
		}
		ConnectUnlockedVillages();
		foreach (MapLocation village2 in m_Villages)
		{
			spheresToReveal.AddRange(village2.GetVillageRoadPositions());
		}
		foreach (CQuestState allCompletedQuest in AdventureState.MapState.AllCompletedQuests)
		{
			if (!AdventureState.MapState.QueuedCompletedQuestIDs.Contains(allCompletedQuest.ID))
			{
				spheresToReveal.AddRange(allCompletedQuest.Quest.CompleteSphereDefinitions);
			}
		}
		foreach (CVisibilitySphereState item3 in AdventureState.MapState.VisibilitySphereStates.Where((CVisibilitySphereState w) => w.VisibilitySphereState == CVisibilitySphereState.EVisibilitySphereState.Unlocked || w.UnlockConditionState.IsUnlocked()))
		{
			if (item3.VisibilitySphereState == CVisibilitySphereState.EVisibilitySphereState.Locked)
			{
				item3.UnlockVisibilitySphere();
			}
			spheresToReveal.AddRange(item3.VisibilitySphere.SphereDefinitions);
		}
		RevealVisibilitySpheres(spheresToReveal, revealInstantly: true);
		if (AdventureState.MapState.JustCompletedLocationState != null)
		{
			MapLocation startingMapLocation = m_Scenarios.SingleOrDefault((MapLocation x) => x.Location == AdventureState.MapState.JustCompletedLocationState);
			if (startingMapLocation == null)
			{
				Debug.LogErrorFormat("No found village location for the completed quest: " + AdventureState.MapState.JustCompletedLocationState.Location.LocalisedName + ". Redirecting to headquarters location");
				startingMapLocation = m_Villages.SingleOrDefault((MapLocation x) => x.Location == AdventureState.MapState.HeadquartersState);
				AdventureState.MapState.JustCompletedLocationState = null;
			}
			m_CurrentLocation = startingMapLocation;
			m_PartyToken.transform.position = startingMapLocation.CenterPosition;
			if (startingMapLocation?.LocationQuest?.LinkedQuestState != null)
			{
				m_LinkedQuestLocation = m_Scenarios.SingleOrDefault((MapLocation x) => x.LocationQuest.ID == startingMapLocation.LocationQuest.LinkedQuestState.ID);
			}
		}
		else
		{
			MapLocation mapLocation7 = (m_CurrentLocation = m_Villages.SingleOrDefault((MapLocation x) => x.Location == AdventureState.MapState.HeadquartersState));
			m_PartyToken.transform.position = mapLocation7.CenterPosition;
		}
		RefreshLocationLines((m_TempCurrentLocation != null) ? m_TempCurrentLocation : m_CurrentLocation);
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			SaveDataShared.ApplyEnhancementIcons(selectedCharacter.Enhancements, selectedCharacter.CharacterID);
		}
		Singleton<QuestManager>.Instance.SortQuestLogAlphabetically();
		OnMapLocationsChanged();
		m_Initialised = true;
	}

	public static void MessageHandler(object message, bool processImmediately)
	{
		CMapClientMessage cMapClientMessage = (CMapClientMessage)message;
		if (processImmediately)
		{
			Singleton<MapChoreographer>.Instance.ProcessMessage(cMapClientMessage);
			return;
		}
		lock (m_MessageQueue)
		{
			m_MessageQueue.Add(cMapClientMessage);
		}
	}

	private void ProcessMessage(CMapClientMessage message)
	{
		try
		{
			if (!AreAllCanvasesLoaded)
			{
				return;
			}
			Debug.Log(message.m_MessageType);
			switch (message.m_MessageType)
			{
			case EMapClientMessageType.StartMoving:
			{
				CStartMoving_MapClientMessage cStartMoving_MapClientMessage = (CStartMoving_MapClientMessage)message;
				StartMove(cStartMoving_MapClientMessage.m_EndLocation);
				break;
			}
			case EMapClientMessageType.EnterScenario:
			{
				CEnterScenario_MapClientMessage cEnterScenario_MapClientMessage = (CEnterScenario_MapClientMessage)message;
				EnterLoadout(cEnterScenario_MapClientMessage.m_ScenarioLocation);
				break;
			}
			case EMapClientMessageType.ShowRoadEvent:
				_ = (CShowRoadEvent_MapClientMessage)message;
				ShowEncounteredEvent(AdventureState.MapState.MapParty.RoadEventDeck, "RoadEvent");
				break;
			case EMapClientMessageType.ShowMapMessages:
			{
				CShowMapMessages_MapClientMessage cShowMapMessages_MapClientMessage = (CShowMapMessages_MapClientMessage)message;
				ShowMapMessages(cShowMapMessages_MapClientMessage.m_messageTrigger, cShowMapMessages_MapClientMessage.m_MapMessages);
				break;
			}
			case EMapClientMessageType.ContentUnlocked:
				if (((CContentUnlocked_MapClientMessage)message).m_ContentType == "guild_character")
				{
					Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
				}
				break;
			case EMapClientMessageType.AchievementCompleted:
			{
				CAchievementCompleted_MapClientMessage messageData = (CAchievementCompleted_MapClientMessage)message;
				Debug.LogGUI("AchievementCompleted " + messageData.m_AchievementID);
				if (!AdventureState.MapState.IsCampaign || m_IsMoving || !(MovingToLocation == null))
				{
					break;
				}
				CPartyAchievement achievement = AdventureState.MapState.MapParty.Achievements.FirstOrDefault((CPartyAchievement it) => it.ID == messageData.m_AchievementID);
				if (achievement == null || achievement.Achievement.AchievementType != EAchievementType.Campaign)
				{
					break;
				}
				List<Reward> rewards = achievement.Rewards.SelectMany((RewardGroup it) => it.Rewards).ToList();
				if (rewards.Count <= 0)
				{
					break;
				}
				AdventureState.MapState.QueuedCompletionRewards.RemoveAll((Reward it) => rewards.Contains(it));
				rewards.RemoveAll((Reward x) => x.Type == ETreasureType.UnlockCharacter && AdventureState.MapState.MapParty.UnlockedCharacterIDs.Contains(x.CharacterID));
				StartCoroutine(WaitToShowAchievements(delegate
				{
					Debug.LogGUI("Queue achievement " + achievement.ID);
					actionProgression.AddAction("AchievementCompleted", delegate
					{
						Debug.LogGUI("Process achievement rewards " + achievement.ID);
						CallbackPromise promise = new CallbackPromise(delegate
						{
							SaveData.Instance.SaveCurrentAdventureData();
						});
						if (!LocalizationManager.TryGetTranslation(achievement.Achievement.LocalizedRewardsTitle, out var Translation))
						{
							Translation = LocalizationManager.GetTranslation("GUI_ACHIEVEMENT_REWARDS");
						}
						Singleton<UIAdventureRewardsManager>.Instance.ShowRewards(rewards.FindAll((Reward it) => it.IsVisibleInUI()), Translation, "GUI_ACHIEVEMENT_REWARDS_CLOSE", delegate
						{
							Singleton<UIDistributeRewardManager>.Instance.Process(rewards, promise.Resolve, null, m_ResultsProcessFinished);
						});
						if (LocalizationManager.TryGetTranslation(achievement.Achievement.LocalizedRewardsStory, out var _))
						{
							string text = rewards.FirstOrDefault((Reward it) => it.Type == ETreasureType.UnlockCharacter)?.CharacterID;
							Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.AchievementClaimed, new MapStoryController.MapDialogInfo(new DialogLineDTO(achievement.Achievement.LocalizedRewardsStory, text ?? "Narrator"), null, hideOtherUI: false));
						}
						return promise;
					});
				}));
				break;
			}
			case EMapClientMessageType.InHQ:
				SceneController.Instance.BusyProcessingResults = true;
				SaveData.Instance.SaveCurrentAdventureData();
				Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: false, this);
				if (NewPartyDisplayUI.PartyDisplay.Initialised)
				{
					NewPartyDisplayUI.PartyDisplay.Show(this);
					NewPartyDisplayUI.PartyDisplay.EnableCharacterSelection(enable: true, this);
					Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.WorldMap);
				}
				Singleton<UIGuildmasterHUD>.Instance.Show(this);
				Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: true);
				Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
				Singleton<QuestManager>.Instance.RefreshLockedQuests();
				Singleton<QuestManager>.Instance.Show();
				OnMapLocationSelect(m_CurrentLocation, active: false);
				Singleton<MapMarkersManager>.Instance.ShowMarkers();
				CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				CameraController.s_CameraController.CancelZoom();
				CameraController.s_CameraController.m_ExtraMinimumFOV = 0f;
				RefreshLocationLines((m_TempCurrentLocation != null) ? m_TempCurrentLocation : m_CurrentLocation);
				m_Villages.SingleOrDefault((MapLocation x) => x.Location == AdventureState.MapState.HeadquartersState).HideLocationLine();
				AdventureState.MapState.QueuedUnlockedQuestIDs.Clear();
				if (AdventureState.MapState.QueuedCompletionRewards.Count > 0)
				{
					StartCoroutine(CoroutineHelper.DelayedStartCoroutine(0.5f, delegate
					{
						Singleton<UIDistributeRewardManager>.Instance.Process(AdventureState.MapState.QueuedCompletionRewards, delegate
						{
							AdventureState.MapState.QueuedCompletionRewards.Clear();
							SaveData.Instance.SaveCurrentAdventureData();
							OnFinishedProcessingResults();
						});
					}));
				}
				else if (FFSNetwork.IsOnline)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
					Singleton<UIMapMultiplayerController>.Instance.ShowRewardsMultiplayer(OnFinishedProcessingResults);
				}
				else
				{
					OnFinishedProcessingResults();
				}
				break;
			case EMapClientMessageType.CharacterRetired:
			{
				CCharacterRetired_MapClientMessage cCharacterRetired_MapClientMessage = (CCharacterRetired_MapClientMessage)message;
				NewPartyDisplayUI.PartyDisplay.RemoveCharacterFromSlot(cCharacterRetired_MapClientMessage.m_CharacterID, m_ResultsProcessFinished);
				break;
			}
			case EMapClientMessageType.PostTrophyAchievement:
			{
				CPostTrophyAchievement_MapClientMessage cPostTrophyAchievement_MapClientMessage = (CPostTrophyAchievement_MapClientMessage)message;
				PlatformLayer.Stats.SetAchievementCompleted(cPostTrophyAchievement_MapClientMessage.m_Achievements);
				break;
			}
			case EMapClientMessageType.PersonalQuestCompleted:
			{
				CPersonalQuestCompleted_MapClientMessage messageData2 = (CPersonalQuestCompleted_MapClientMessage)message;
				if (!m_ResultsProcessFinished || m_IsMoving)
				{
					break;
				}
				CMapCharacter character = AdventureState.MapState.MapParty.SelectedCharacters.Single((CMapCharacter it) => it.CharacterID == messageData2.m_CharacterID);
				bool refreshMap = false;
				foreach (Reward item in character.PersonalQuest.CurrentRewards.SelectMany((RewardGroup it) => it.Rewards))
				{
					refreshMap |= item.Type == ETreasureType.UnlockQuest;
					AdventureState.MapState.QueuedCompletionRewards.Remove(item);
				}
				Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
				if (FFSNetwork.IsOnline && character.IsUnderMyControl)
				{
					SceneController.Instance.RetiringCharacter = true;
				}
				Singleton<UIDistributeRewardManager>.Instance.Process(character.PersonalQuest.CurrentRewards.SelectMany((RewardGroup it) => it.Rewards).ToList(), delegate
				{
					if (refreshMap)
					{
						RefreshAllMapLocations();
					}
					if (!FFSNetwork.IsOnline || character.IsUnderMyControl)
					{
						PersonalQuestDTO personalQuestDto = new PersonalQuestDTO(character.PersonalQuest);
						character.PersonalQuest.NextPersonalQuestStep();
						actionProgression.AddAction("PersonalQuestCompleted", () => Singleton<UIPersonalQuestResultManager>.Instance.ShowPersonalQuestCompleted(character, personalQuestDto).Then(delegate
						{
							if (!AdventureState.MapState.MapParty.ExistsCharacterToRetire())
							{
								SaveData.Instance.SaveCurrentAdventureData();
								if (FFSNetwork.IsOnline && SceneController.Instance.RetiringCharacter)
								{
									SceneController.Instance.RetiringCharacter = false;
									Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
								}
								AdventureState.MapState.CheckNonTrophyAchievements();
							}
						}));
					}
				}, null, readyAllPlayers: false);
				break;
			}
			case EMapClientMessageType.UnlockCharacter:
			{
				CUnlockCharacter_MapClientMessage messageData4 = (CUnlockCharacter_MapClientMessage)message;
				if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster && AdventureState.MapState.HeadquartersState.MultiplayerUnlocked && !NewPartyDisplayUI.PartyDisplay.CharacterAlreadyBenched(messageData4.m_UnlockedCharacter) && AdventureState.MapState.MapParty.SelectedCharacters.All((CMapCharacter it) => it.CharacterID != messageData4.m_UnlockedCharacter.CharacterID) && !ControllableRegistry.AllControllables.Exists((NetworkControllable x) => x.ID == CharacterClassManager.GetModelInstanceIDFromCharacterID(messageData4.m_UnlockedCharacter.CharacterID)))
				{
					NewPartyDisplayUI.PartyDisplay.AddBenchedCharacter(messageData4.m_UnlockedCharacter);
				}
				NewPartyDisplayUI.PartyDisplay.RefreshNewClassesNotification();
				break;
			}
			case EMapClientMessageType.RegenerateAllMapScenarios:
				RequestRegenerateAllMapScenarios();
				break;
			case EMapClientMessageType.TempleDevotionLevelUp:
			{
				CTempleDevotionLevelUp_MapClientMessage messageData3 = (CTempleDevotionLevelUp_MapClientMessage)message;
				actionProgression.AddAction("TempleDevotionLevelUp", delegate
				{
					CallbackPromise callbackPromise = CallbackPromise.Resolved();
					if (!messageData3.m_StoryLocText.IsNullOrEmpty())
					{
						callbackPromise = new CallbackPromise();
						Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.Temple, new MapStoryController.MapDialogInfo(new DialogLineDTO(messageData3.m_StoryLocText, "Priestess", EExpression.Default, null, null, messageData3.m_StoryAudioId), callbackPromise.Resolve, hideOtherUI: false));
					}
					return callbackPromise.Then(delegate
					{
						CallbackPromise callbackPromise2 = new CallbackPromise();
						Singleton<UIAdventureRewardsManager>.Instance.ShowRewards(messageData3.m_Rewards.SelectMany((RewardGroup it) => it.Rewards).ToList(), string.Format(LocalizationManager.GetTranslation("GUI_DEVOTION_LEVEL"), messageData3.m_NewLevel + 1), "GUI_QUEST_COMPLETED_REWARDS_CLOSE", callbackPromise2.Resolve);
						return callbackPromise2;
					});
				});
				break;
			}
			case EMapClientMessageType.GoldChanged:
			{
				CGoldChanged_MapClientMessage cGoldChanged_MapClientMessage = (CGoldChanged_MapClientMessage)message;
				if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
				{
					EventBuss.OnPartyGoldChanged(cGoldChanged_MapClientMessage.m_NewGold);
				}
				else
				{
					EventBuss.OnCharacterGoldChanged(cGoldChanged_MapClientMessage.m_NewGold, cGoldChanged_MapClientMessage.m_CharacterId, cGoldChanged_MapClientMessage.m_CharacterName);
				}
				break;
			}
			case EMapClientMessageType.CharacterXpChanged:
			{
				CXpChanged_MapClientMessage cXpChanged_MapClientMessage = (CXpChanged_MapClientMessage)message;
				EventBuss.OnCharacterXpChanged(cXpChanged_MapClientMessage.m_CharacterId, cXpChanged_MapClientMessage.m_CharacterName);
				break;
			}
			case EMapClientMessageType.CharacterLevelUpAvailable:
			{
				CCharacterLevelupAvailable_MapClientMessage cCharacterLevelupAvailable_MapClientMessage = (CCharacterLevelupAvailable_MapClientMessage)message;
				EventBuss.OnCharacterLevelupUnlocked(cCharacterLevelupAvailable_MapClientMessage.m_CharacterId, cCharacterLevelupAvailable_MapClientMessage.m_CharacterName);
				break;
			}
			case EMapClientMessageType.ProsperityChanged:
			{
				CProsperityChanged_MapClientMessage cProsperityChanged_MapClientMessage = (CProsperityChanged_MapClientMessage)message;
				EventBuss.OnProsperityChanged(cProsperityChanged_MapClientMessage.m_NewProsperityXP, cProsperityChanged_MapClientMessage.m_PreviousProsperityXP, cProsperityChanged_MapClientMessage.m_NewProsperityLevel, cProsperityChanged_MapClientMessage.m_PreviousProsperityLevel);
				if (cProsperityChanged_MapClientMessage.m_NewProsperityLevel != cProsperityChanged_MapClientMessage.m_PreviousProsperityLevel)
				{
					Singleton<UIGuildmasterHUD>.Instance.RefreshNotifications(EGuildmasterMode.Merchant);
				}
				break;
			}
			case EMapClientMessageType.ReputationChanged:
			{
				CReputationChanged_MapClientMessage cReputationChanged_MapClientMessage = (CReputationChanged_MapClientMessage)message;
				EventBuss.OnReputationChanged(cReputationChanged_MapClientMessage.m_Reputation);
				break;
			}
			case EMapClientMessageType.CharacterCreated:
			{
				CCharacterCreated_MapClientMessage cCharacterCreated_MapClientMessage = (CCharacterCreated_MapClientMessage)message;
				EventBuss.OnCharacterCreated(cCharacterCreated_MapClientMessage.m_Character);
				break;
			}
			case EMapClientMessageType.CharacterAbilityCardGained:
			{
				CCharacterAbilityCardGained_MapClientMessage cCharacterAbilityCardGained_MapClientMessage = (CCharacterAbilityCardGained_MapClientMessage)message;
				NewPartyDisplayUI.PartyDisplay.ShowNewAbilityCardWon(cCharacterAbilityCardGained_MapClientMessage.m_CharacterId, cCharacterAbilityCardGained_MapClientMessage.m_CharacterName, cCharacterAbilityCardGained_MapClientMessage.m_AbilityCard);
				break;
			}
			case EMapClientMessageType.CharacterPerkPointsChanged:
			{
				CCharacterPerkPointsChanged_MapClientMessage cCharacterPerkPointsChanged_MapClientMessage = (CCharacterPerkPointsChanged_MapClientMessage)message;
				EventBuss.OnCharacterPerkPointsChanged(cCharacterPerkPointsChanged_MapClientMessage.m_CharacterId, cCharacterPerkPointsChanged_MapClientMessage.m_CharacterName, cCharacterPerkPointsChanged_MapClientMessage.m_PerkPoints);
				break;
			}
			case EMapClientMessageType.CharacterConditionGained:
			{
				CCharacterCondition_MapClientMessage cCharacterCondition_MapClientMessage = (CCharacterCondition_MapClientMessage)message;
				EventBuss.OnCharacterConditionGained(cCharacterCondition_MapClientMessage.m_CharacterId, cCharacterCondition_MapClientMessage.m_CharacterName);
				break;
			}
			case EMapClientMessageType.CharacterItemBound:
			{
				CCharacterItemBound_MapClientMessage cCharacterItemBound_MapClientMessage = (CCharacterItemBound_MapClientMessage)message;
				EventBuss.OnCharacterItemBound(cCharacterItemBound_MapClientMessage.m_Character, cCharacterItemBound_MapClientMessage.m_Item);
				break;
			}
			case EMapClientMessageType.CharacterItemUnequipped:
			{
				CCharacterItemUnequipped_MapClientMessage cCharacterItemUnequipped_MapClientMessage = (CCharacterItemUnequipped_MapClientMessage)message;
				EventBuss.OnCharacterItemUnequipped(cCharacterItemUnequipped_MapClientMessage.m_Character, cCharacterItemUnequipped_MapClientMessage.m_Items);
				break;
			}
			case EMapClientMessageType.CharacterItemUnbound:
			{
				CCharacterItemUnbound_MapClientMessage cCharacterItemUnbound_MapClientMessage = (CCharacterItemUnbound_MapClientMessage)message;
				EventBuss.OnCharacterItemUnbound(cCharacterItemUnbound_MapClientMessage.m_Character, cCharacterItemUnbound_MapClientMessage.m_Item);
				break;
			}
			case EMapClientMessageType.CharacterItemEquipped:
			{
				CCharacterItemEquipped_MapClientMessage cCharacterItemEquipped_MapClientMessage = (CCharacterItemEquipped_MapClientMessage)message;
				EventBuss.OnCharacterItemEquipped(cCharacterItemEquipped_MapClientMessage.m_Character, cCharacterItemEquipped_MapClientMessage.m_Item);
				break;
			}
			case EMapClientMessageType.ItemAddedToParty:
			{
				CItemAddedToParty_MapClientMessage cItemAddedToParty_MapClientMessage = (CItemAddedToParty_MapClientMessage)message;
				EventBuss.OnItemAdded(cItemAddedToParty_MapClientMessage.m_Item);
				break;
			}
			case EMapClientMessageType.ItemRemovedFromParty:
			{
				CItemRemovedFromParty_MapClientMessage cItemRemovedFromParty_MapClientMessage = (CItemRemovedFromParty_MapClientMessage)message;
				EventBuss.OnItemRemoved(cItemRemovedFromParty_MapClientMessage.m_Item, cItemRemovedFromParty_MapClientMessage.m_SlotIndex);
				break;
			}
			case EMapClientMessageType.NewUnlockedClassesChanged:
				NewPartyDisplayUI.PartyDisplay.RefreshNewClassesNotification();
				break;
			case EMapClientMessageType.FinishCheckLockedContent:
				if (SceneController.Instance.CheckingLockedContent)
				{
					SceneController.Instance.CheckingLockedContent = false;
					if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
					{
						Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
					}
				}
				break;
			case EMapClientMessageType.CharacterLevelledUp:
				if (AdventureState.MapState.QueuedUnlockedQuestIDs.Count <= 0)
				{
					break;
				}
				actionProgression.AddAction("CharacterLevelledUp", delegate
				{
					if (AdventureState.MapState.QueuedUnlockedQuestIDs.Count == 0)
					{
						return CallbackPromise.Resolved();
					}
					foreach (string queuedUnlockedQuestID in AdventureState.MapState.QueuedUnlockedQuestIDs)
					{
						m_RecentUnlockedQuestLocations.Add(queuedUnlockedQuestID);
					}
					Singleton<QuestManager>.Instance.HideLogScreen(message);
					NewPartyDisplayUI.PartyDisplay.Hide(message, instant: true);
					if (NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.IsOpen)
					{
						NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.Hide();
					}
					CallbackPromise promise = new CallbackPromise();
					RefreshAllMapLocations(delegate
					{
						Singleton<QuestManager>.Instance.ShowLogScreen(message);
						NewPartyDisplayUI.PartyDisplay.Show(message);
						promise.Resolve();
					}, save: false);
					return promise;
				});
				break;
			case EMapClientMessageType.Save:
				SaveData.Instance.SaveCurrentAdventureData();
				break;
			case EMapClientMessageType.FinishedSoloScenarioImport:
				RefreshAllMapLocations(null, save: false);
				SaveData.Instance.SaveCurrentAdventureData();
				if (!FFSNetwork.IsOnline || !FFSNetwork.IsHost)
				{
					break;
				}
				{
					foreach (CMapCharacter mapCharacter in AdventureState.MapState.MapParty.CheckCharacters)
					{
						if (!AdventureState.MapState.IsCampaign || (mapCharacter.PersonalQuest != null && !PlayerRegistry.AllPlayers.Exists((NetworkPlayer it) => it.CreatingCharacter != null && it.CreatingCharacter.CharacterName == mapCharacter.CharacterName)))
						{
							int controllableID = (AdventureState.MapState.IsCampaign ? mapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(mapCharacter.CharacterID));
							ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
							IProtocolToken supplementaryDataToken = new ItemInventoryToken(mapCharacter, AdventureState.MapState.MapParty, AdventureState.MapState.GoldMode);
							Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
						}
					}
					break;
				}
			case EMapClientMessageType.Count:
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing MapClientMessage.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private IEnumerator WaitToShowAchievements(Action onFinished)
	{
		yield return WaitAllRetired(null);
		if (Singleton<UIDistributeRewardManager>.Instance.IsDistributing)
		{
			yield return WaitDistributionEnds(null);
		}
		onFinished?.Invoke();
	}

	public bool OnStoreMapLocationSelect(MapLocation mapLocation, bool active)
	{
		try
		{
			if (!m_Initialised || m_IsMoving || !active || !(mapLocation.Location is CStoreLocationState cStoreLocationState))
			{
				return false;
			}
			switch (cStoreLocationState.StoreLocation.StoreType)
			{
			case EHQStores.Enhancer:
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.Enchantress);
				break;
			case EHQStores.Merchant:
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.Merchant);
				break;
			case EHQStores.Temple:
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.Temple);
				break;
			case EHQStores.Trainer:
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.Trainer);
				break;
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing OnStoreMapLocationSelect.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return false;
	}

	public bool OnMapLocationSelect(MapLocation mapLocation, bool active)
	{
		try
		{
			if (m_Initialised && !m_IsMoving && MovingToLocation == null)
			{
				ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.WorldMap);
				bool flag = false;
				if (active && mapLocation.LocationQuest != null)
				{
					if (!IsVisibleInMap(mapLocation))
					{
						if (mapLocation.LocationQuest.Quest.Type == EQuestType.City)
						{
							OpenCityMap(transition: false);
						}
						else
						{
							OpenWorldMap(transition: false);
						}
						OnMapLocationHighlight(mapLocation, active);
						flag = true;
					}
					RequirementCheckResult requirementCheckResult = mapLocation.LocationQuest.CheckRequirements();
					if (!requirementCheckResult.IsUnlocked() && !requirementCheckResult.IsOnlyMissingCharacters() && !ShowAllScenariosMode)
					{
						Singleton<AdventureMapUIManager>.Instance.DeselectCurrentMapLocation();
						Singleton<AdventureMapUIManager>.Instance.ShowWarning(requirementCheckResult, autoHide: true);
						return false;
					}
				}
				RefreshLocationLines((m_TempCurrentLocation != null) ? m_TempCurrentLocation : m_CurrentLocation);
				if (active)
				{
					switch (mapLocation.MapLocationType)
					{
					case MapLocation.EMapLocationType.Boss:
					case MapLocation.EMapLocationType.Scenario:
						Singleton<AdventureMapUIManager>.Instance.OnSelectedMapLocation(mapLocation, OnMoveClick);
						break;
					case MapLocation.EMapLocationType.Village:
						Singleton<AdventureMapUIManager>.Instance.OnSelectedMapLocation(mapLocation, OnMoveClick);
						break;
					case MapLocation.EMapLocationType.Headquarters:
						if (!AdventureState.MapState.IsCampaign || Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.WorldMap)
						{
							return false;
						}
						if (IsChoosingLinkedQuestOption())
						{
							Singleton<AdventureMapUIManager>.Instance.OnSelectedMapLocation(mapLocation, OnMoveClick);
							break;
						}
						OpenCityMap();
						return false;
					}
					FocusOnLocation(mapLocation);
				}
				else
				{
					Singleton<AdventureMapUIManager>.Instance.OnDeselectedMapLocation(mapLocation);
					ResetFocusOnLocation();
					if (IsChoosingLinkedQuestOption())
					{
						Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_LINKED_QUEST_CHOOSE_TOOLTIP"), LocalizationManager.GetTranslation(m_CurrentLocation.LocationQuest.LinkedQuestState.Quest.LocalisedNameKey)));
					}
				}
				m_LocationSelected = active;
				Singleton<QuestManager>.Instance.OnMapLocationQuestSelected(mapLocation.LocationQuest, active && mapLocation.LocationQuest != null);
				if (flag)
				{
					CameraController.s_CameraController.ResetPositionToFocusOnTargetPoint();
				}
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing OnMapLocationSelect.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return false;
	}

	public bool OnMapLocationHighlight(MapLocation mapLocation, bool active)
	{
		try
		{
			if (m_Initialised && !m_IsMoving && MovingToLocation == null)
			{
				if (IsVisibleInMap(mapLocation))
				{
					if (active)
					{
						bool flag = true;
						bool flag2 = !FFSNetwork.IsOnline || FFSNetwork.IsHost || mapLocation.IsSelected;
						switch (mapLocation.MapLocationType)
						{
						case MapLocation.EMapLocationType.Boss:
						case MapLocation.EMapLocationType.Scenario:
						{
							if (!(mapLocation != m_CurrentLocation) || IsChoosingLinkedQuestOption())
							{
								break;
							}
							CQuestState cQuestState = null;
							foreach (CQuestState item in AdventureState.MapState.AllQuests.Where((CQuestState w) => w.QuestState >= CQuestState.EQuestState.Unlocked))
							{
								if (item.ScenarioState == mapLocation.Location)
								{
									cQuestState = item;
									break;
								}
							}
							if (cQuestState == null)
							{
								break;
							}
							RequirementCheckResult requirementCheckResult2 = cQuestState.CheckRequirements();
							flag = requirementCheckResult2.IsUnlocked() || requirementCheckResult2.IsOnlyMissingCharacters() || ShowAllScenariosMode;
							if (!flag)
							{
								break;
							}
							if (cQuestState is CJobQuestState cJobQuestState)
							{
								string startingVillage2 = cJobQuestState.JobVillageID;
								m_TempCurrentLocation = m_Villages.SingleOrDefault((MapLocation x) => x.Location.ID == startingVillage2);
							}
							else
							{
								string startingVillage3 = cQuestState.Quest.StartingVillage;
								MapLocation mapLocation3 = m_Villages.SingleOrDefault((MapLocation x) => x.Location.ID == startingVillage3);
								if (mapLocation3 != null && mapLocation3.Location.LocationState >= ELocationState.Unlocked)
								{
									m_TempCurrentLocation = mapLocation3;
								}
							}
							if (m_TempCurrentLocation != null && flag2)
							{
								m_PartyToken.PartyInstantMove(m_TempCurrentLocation.CenterPosition);
							}
							break;
						}
						case MapLocation.EMapLocationType.Village:
						{
							if (!(mapLocation != m_CurrentLocation) || mapLocation.LocationQuest == null)
							{
								break;
							}
							RequirementCheckResult requirementCheckResult = mapLocation.LocationQuest.CheckRequirements();
							flag = (requirementCheckResult.IsUnlocked() || requirementCheckResult.IsOnlyMissingCharacters()) && !ShowAllScenariosMode;
							if (!flag)
							{
								break;
							}
							string startingVillage = mapLocation.LocationQuest.Quest.StartingVillage;
							MapLocation mapLocation2 = m_Villages.Single((MapLocation x) => x.Location.ID == startingVillage);
							if (mapLocation2 != m_CurrentLocation)
							{
								m_TempCurrentLocation = mapLocation2;
								if (m_TempCurrentLocation != null && flag2)
								{
									m_PartyToken.PartyInstantMove(m_TempCurrentLocation.CenterPosition);
								}
							}
							break;
						}
						case MapLocation.EMapLocationType.Headquarters:
							if (!AdventureState.MapState.IsCampaign || Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.WorldMap)
							{
								return false;
							}
							flag = IsChoosingLinkedQuestOption();
							break;
						}
						if (flag && flag2)
						{
							RefreshLocationLines((m_TempCurrentLocation != null) ? m_TempCurrentLocation : m_CurrentLocation);
						}
					}
					else if (m_TempCurrentLocation != null && !m_LocationSelected)
					{
						m_TempCurrentLocation = null;
						m_PartyToken.PartyInstantMove(m_CurrentLocation.CenterPosition);
						mapLocation.HideLocationLine();
					}
					else
					{
						RefreshLocationLines((m_TempCurrentLocation != null) ? m_TempCurrentLocation : m_CurrentLocation);
					}
					if (AdventureState.MapState.IsCampaign && mapLocation.MapLocationType == MapLocation.EMapLocationType.Headquarters && CityQuestLocations.Count > 0)
					{
						Singleton<QuestManager>.Instance.OnHeadquartersMapLocationQuestHighlighted(active);
					}
				}
				if (mapLocation.LocationQuest != null)
				{
					Singleton<QuestManager>.Instance.OnMapLocationQuestHighlighted(mapLocation.LocationQuest, active);
				}
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing OnMapLocationHighlight.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return false;
	}

	private void OnMoveClick(MapLocation moveToLocation)
	{
		try
		{
			NewPartyDisplayUI.PartyDisplay.TabInput.UnRegister();
			FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToMultiplayer);
			FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
			if (FFSNetwork.IsHost)
			{
				IProtocolToken supplementaryDataToken = new LocationToken(moveToLocation.Location.ID);
				Synchronizer.SendGameAction(GameActionType.MoveToNewNode, ActionPhaseType.MapHQ, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			Singleton<UIReadyToggle>.Instance.Reset();
			if (FFSNetwork.IsOnline)
			{
				Singleton<UIMapMultiplayerController>.Instance.ResetUI();
			}
			Singleton<AdventureMapUIManager>.Instance.HideTravelOption();
			Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: true, this);
			Singleton<QuestManager>.Instance.OnPartyMove();
			Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: false);
			Singleton<UIGuildmasterHUD>.Instance.HideBanner(this, hide: true);
			NewPartyDisplayUI.PartyDisplay.Hide(this);
			if (ShowAllScenariosMode)
			{
				if (moveToLocation.LocationQuest != null)
				{
					moveToLocation.LocationQuest.ResetQuest();
					moveToLocation.LocationQuest.SetInProgressQuest();
				}
				moveToLocation.RefreshLocationLine(m_CurrentLocation, forceRefresh: true);
			}
			if (!m_IsMoving)
			{
				if (IsChoosingLinkedQuestOption() && HeadquartersLocation == moveToLocation)
				{
					StartMove(AdventureState.MapState.HeadquartersState);
				}
				else if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
				{
					Singleton<UIPersonalQuestResultManager>.Instance.HideAllPersonalQuestNotifications();
					CheckCampaignIntro(moveToLocation);
				}
				else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
				{
					MapRuleLibraryClient.Instance.AddQueueMessage(new CMove_MapDLLMessage(m_CurrentLocation.Location.ID, moveToLocation.Location.ID), processImmediately: false);
				}
			}
			SimpleLog.AddToSimpleLog("Started moving to location: " + moveToLocation.Location.ID);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing OnMoveClick.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void CheckCampaignIntro(MapLocation moveToLocation, bool first = true)
	{
		CheckForIntroMessages(moveToLocation);
		if (m_IntroGloomhavenLines.Count > 0 && (!moveToLocation.Location.ID.ToUpper().Contains("JOTL") || !first))
		{
			m_QueuedMoveLocation = moveToLocation;
			MapStoryController.MapDialogInfo message = new MapStoryController.MapDialogInfo(m_IntroGloomhavenLines, null);
			Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.IntroGloomhaven, message, FinishedShowingIntroGloomhavenMessages);
			if (FFSNetwork.IsOnline)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted);
			}
		}
		else
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CMove_MapDLLMessage(m_CurrentLocation.Location.ID, moveToLocation.Location.ID), processImmediately: false);
		}
	}

	private void CheckForIntroMessages(MapLocation moveToLocation)
	{
		m_IntroGloomhavenLines = new List<DialogLineDTO>();
		m_IntroTravelLines = new List<DialogLineDTO>();
		m_OutroTravelLines = new List<DialogLineDTO>();
		m_OutroGloomhavenLines = new List<DialogLineDTO>();
		bool flag = false;
		int num = 0;
		while (!flag)
		{
			num++;
			string key = string.Format(moveToLocation.LocationQuest.Quest.LocalisedIntroGloomhavenKey, num);
			if (LocalizationManager.TryGetTranslation(key, out var _))
			{
				string narrativeImageId = "TO_IN_01";
				string narrativeAudioId = key;
				Tuple<string, string> tuple = null;
				if (moveToLocation.LocationQuest.Quest.NarrativeTextImageOverride != null)
				{
					tuple = moveToLocation.LocationQuest.Quest.NarrativeTextImageOverride.Find((Tuple<string, string> x) => x.Item1 == key);
				}
				if (tuple != null)
				{
					narrativeImageId = tuple.Item2;
				}
				if (moveToLocation.LocationQuest.Quest.NarrativeTextAudioOverride != null)
				{
					narrativeAudioId = moveToLocation.LocationQuest.Quest.NarrativeTextAudioOverride.Find((Tuple<string, string> x) => x.Item1 == key)?.Item2;
				}
				m_IntroGloomhavenLines.Add(new DialogLineDTO(key, "Narrator", EExpression.Default, narrativeImageId, AdventureState.MapState.IsCampaign ? LocalizationManager.GetTranslation(moveToLocation.LocationQuest.Quest.LocalisedNameKey) : null, narrativeAudioId));
			}
			else
			{
				flag = true;
			}
		}
		flag = false;
		num = 0;
		while (!flag)
		{
			num++;
			string key2 = string.Format(moveToLocation.LocationQuest.Quest.LocalisedIntroTravelKey, num);
			if (LocalizationManager.TryGetTranslation(key2, out var _))
			{
				string narrativeImageId2 = null;
				string narrativeAudioId2 = key2;
				Tuple<string, string> tuple2 = null;
				if (moveToLocation.LocationQuest.Quest.NarrativeTextImageOverride != null)
				{
					tuple2 = moveToLocation.LocationQuest.Quest.NarrativeTextImageOverride.Find((Tuple<string, string> x) => x.Item1 == key2);
				}
				if (tuple2 != null)
				{
					narrativeImageId2 = tuple2.Item2;
				}
				if (moveToLocation.LocationQuest.Quest.NarrativeTextAudioOverride != null)
				{
					narrativeAudioId2 = moveToLocation.LocationQuest.Quest.NarrativeTextAudioOverride.Find((Tuple<string, string> x) => x.Item1 == key2)?.Item2;
				}
				m_IntroTravelLines.Add(new DialogLineDTO(key2, "Narrator", EExpression.Default, narrativeImageId2, AdventureState.MapState.IsCampaign ? LocalizationManager.GetTranslation(moveToLocation.LocationQuest.Quest.LocalisedNameKey) : null, narrativeAudioId2));
			}
			else
			{
				flag = true;
			}
		}
		flag = false;
		num = 0;
		while (!flag)
		{
			num++;
			string key3 = string.Format(moveToLocation.LocationQuest.Quest.LocalisedOutroTravelKey, num);
			if (LocalizationManager.TryGetTranslation(key3, out var _))
			{
				string narrativeImageId3 = null;
				string narrativeAudioId3 = key3;
				Tuple<string, string> tuple3 = null;
				if (moveToLocation.LocationQuest.Quest.NarrativeTextImageOverride != null)
				{
					tuple3 = moveToLocation.LocationQuest.Quest.NarrativeTextImageOverride.Find((Tuple<string, string> x) => x.Item1 == key3);
				}
				if (tuple3 != null)
				{
					narrativeImageId3 = tuple3.Item2;
				}
				if (moveToLocation.LocationQuest.Quest.NarrativeTextAudioOverride != null)
				{
					narrativeAudioId3 = moveToLocation.LocationQuest.Quest.NarrativeTextAudioOverride.Find((Tuple<string, string> x) => x.Item1 == key3)?.Item2;
				}
				m_OutroTravelLines.Add(new DialogLineDTO(key3, "Narrator", EExpression.Default, narrativeImageId3, AdventureState.MapState.IsCampaign ? LocalizationManager.GetTranslation(moveToLocation.LocationQuest.Quest.LocalisedNameKey) : null, narrativeAudioId3));
			}
			else
			{
				flag = true;
			}
		}
		flag = false;
		num = 0;
		while (!flag)
		{
			num++;
			string key4 = string.Format(moveToLocation.LocationQuest.Quest.LocalisedOutroGloomhavenKey, num);
			if (LocalizationManager.TryGetTranslation(key4, out var _))
			{
				string narrativeImageId4 = "TO_IN_01";
				string narrativeAudioId4 = key4;
				Tuple<string, string> tuple4 = null;
				if (moveToLocation.LocationQuest.Quest.NarrativeTextImageOverride != null)
				{
					tuple4 = moveToLocation.LocationQuest.Quest.NarrativeTextImageOverride.Find((Tuple<string, string> x) => x.Item1 == key4);
				}
				if (tuple4 != null)
				{
					narrativeImageId4 = tuple4.Item2;
				}
				if (moveToLocation.LocationQuest.Quest.NarrativeTextAudioOverride != null)
				{
					narrativeAudioId4 = moveToLocation.LocationQuest.Quest.NarrativeTextAudioOverride.Find((Tuple<string, string> x) => x.Item1 == key4)?.Item2;
				}
				m_OutroGloomhavenLines.Add(new DialogLineDTO(key4, "Narrator", EExpression.Default, narrativeImageId4, AdventureState.MapState.IsCampaign ? LocalizationManager.GetTranslation(moveToLocation.LocationQuest.Quest.LocalisedNameKey) : null, narrativeAudioId4));
			}
			else
			{
				flag = true;
			}
		}
	}

	private void FinishedShowingIntroGloomhavenMessages(EMapMessageTrigger mapMessageTrigger)
	{
		MapRuleLibraryClient.Instance.AddQueueMessage(new CMove_MapDLLMessage(m_CurrentLocation.Location.ID, m_QueuedMoveLocation.Location.ID), processImmediately: false);
		m_QueuedMoveLocation = null;
	}

	private void StartMove(CLocationState moveToLocation)
	{
		try
		{
			if (m_IsMoving)
			{
				return;
			}
			SimpleLog.AddToSimpleLog("MoveToLocation: " + moveToLocation.Location.LocalisedName);
			SimpleLog.AddToSimpleLog("MapRNG (Started Move): " + AdventureState.MapState.PeekMapRNG);
			if (FFSNetwork.IsOnline)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted);
			}
			Singleton<MapMarkersManager>.Instance.HideMarkers();
			Singleton<QuestManager>.Instance.OnPartyMove();
			Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: false);
			Singleton<UIGuildmasterHUD>.Instance.HideBanner(this, hide: true);
			if (Singleton<UILevelUpWindow>.Instance.IsLevelingUp())
			{
				NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.CloseLevelUpWindow();
				NewPartyDisplayUI.PartyDisplay.EscapeCurrentCharacter();
			}
			NewPartyDisplayUI.PartyDisplay.Hide(this);
			MovingToLocation = null;
			m_WaypointsToEnd = new Vector3[0];
			m_IsMoving = true;
			if (moveToLocation is CMapScenarioState)
			{
				MovingToLocation = m_Scenarios.SingleOrDefault((MapLocation x) => x.Location == moveToLocation);
			}
			else if (moveToLocation is CVillageState || moveToLocation is CHeadquartersState)
			{
				MovingToLocation = m_Villages.SingleOrDefault((MapLocation x) => x.Location == moveToLocation);
			}
			if (MovingToLocation == null)
			{
				m_IsMoving = false;
				return;
			}
			MovingToLocation.RevealPathFog();
			switch (MovingToLocation.MapLocationType)
			{
			case MapLocation.EMapLocationType.Boss:
			case MapLocation.EMapLocationType.Scenario:
			case MapLocation.EMapLocationType.Headquarters:
				m_WaypointsToEnd = MovingToLocation.PathToThisNode;
				break;
			case MapLocation.EMapLocationType.Village:
				m_WaypointsToEnd = MovingToLocation.PathToNextScenario;
				break;
			}
			m_ShouldEncounterRoadEvent = false;
			bool flag = false;
			if (MovingToLocation.MapLocationType != MapLocation.EMapLocationType.Headquarters && !IsChoosingLinkedQuestOption() && !m_EventComplete)
			{
				m_ShouldEncounterRoadEvent = (MovingToLocation.LocationQuest.ScenarioState.ShouldEncounterRoadEvent(MovingToLocation.Location.ID) && (MovingToLocation.Location.ID.ToUpper().Contains("JOTL") || (MovingToLocation.LocationQuest.Quest.Type != EQuestType.City && MovingToLocation.LocationQuest.Quest.Type != EQuestType.CityAdjacent))) || (!MovingToLocation.Location.ID.ToUpper().Contains("JOTL") && RoadEventDebugOverride != null);
				if (MovingToLocation.LocationQuest != null)
				{
					MapLocation mapLocation = null;
					CQuestState locationQuest = MovingToLocation.LocationQuest;
					CJobQuestState jobQuestState = locationQuest as CJobQuestState;
					mapLocation = ((jobQuestState == null) ? m_Villages.SingleOrDefault((MapLocation x) => x.Location.ID == MovingToLocation.LocationQuest.Quest.StartingVillage) : m_Villages.SingleOrDefault((MapLocation x) => x.Location.ID == jobQuestState.JobVillageID));
					m_PartyToken.PartyInstantMove(mapLocation.CenterPosition);
				}
				if (AdventureState.MapState.IsCampaign && (MovingToLocation.Location.ID.ToUpper().Contains("JOTL") || (MovingToLocation.LocationQuest.Quest.Type != EQuestType.City && MovingToLocation.LocationQuest.Quest.Type != EQuestType.CityAdjacent)))
				{
					CCardDeck roadEventDeck = AdventureState.MapState.MapParty.RoadEventDeck;
					if (roadEventDeck != null && roadEventDeck.CardCount() > 0)
					{
						flag = true;
					}
				}
			}
			bool flag2 = IsChoosingLinkedQuestOption();
			AdventureState.MapState.JustCompletedLocationState = null;
			m_LinkedQuestLocation = null;
			if (flag2)
			{
				CurrentLocation.ShowQuestMapMarker(instant: true, this);
			}
			ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.None);
			if (MovingToLocation.LocationQuest != null && m_ShouldEncounterRoadEvent && (!string.IsNullOrEmpty(MovingToLocation.LocationQuest.ScenarioState.RoadEventID) || (!MovingToLocation.Location.ID.ToUpper().Contains("JOTL") && RoadEventDebugOverride != null) || flag))
			{
				if (MovingToLocation.Location.ID.ToUpper().Contains("JOTL"))
				{
					movementFlow.Prepare(m_PartyToken, ref m_WaypointsToEnd);
					movementFlow.MovePartyToJOTLEncounter(m_PartyToken, OnEventTrigger);
				}
				else
				{
					movementFlow.PrepareTravelTo(m_PartyToken, ref m_WaypointsToEnd, MovingToLocation);
					CoroutineHelper.RunDelayedAction(movementFlow.DelayToStartMoving, delegate
					{
						movementFlow.MovePartyToEncounter(m_PartyToken, OnEventTrigger);
					});
				}
			}
			else if (MovingToLocation.MapLocationType == MapLocation.EMapLocationType.Headquarters && m_OutroTravelLines.Count > 0)
			{
				if (!m_EventComplete)
				{
					movementFlow.PrepareTravelTo(m_PartyToken, ref m_WaypointsToEnd, MovingToLocation);
				}
				movementFlow.IncreaseZoomToParty(m_PartyToken);
				CoroutineHelper.RunDelayedAction(movementFlow.DelayToStartMoving, OnOutroTravelMessagesTrigger);
			}
			else if (MovingToLocation.MapLocationType == MapLocation.EMapLocationType.Headquarters && m_OutroGloomhavenLines.Count > 0)
			{
				if (!m_EventComplete)
				{
					movementFlow.PrepareTravelTo(m_PartyToken, ref m_WaypointsToEnd, MovingToLocation);
				}
				movementFlow.IncreaseZoomToParty(m_PartyToken);
				CoroutineHelper.RunDelayedAction(movementFlow.DelayToStartMoving, delegate
				{
					movementFlow.MovePartyFromOriginToNarrativeDestination(m_PartyToken, OnOutroGloomhavenMessagesTrigger);
				});
			}
			else if (MovingToLocation.MapLocationType != MapLocation.EMapLocationType.Headquarters && m_IntroTravelLines.Count > 0)
			{
				if (!m_EventComplete)
				{
					movementFlow.PrepareTravelTo(m_PartyToken, ref m_WaypointsToEnd, MovingToLocation);
				}
				else
				{
					movementFlow.IncreaseZoomToParty(m_PartyToken, movementFlow.DelayToStartMoving);
				}
				CoroutineHelper.RunDelayedAction(movementFlow.DelayToStartMoving, delegate
				{
					movementFlow.MovePartyFromOriginToNarrativeDestination(m_PartyToken, OnIntroTravelMessagesTrigger);
				});
			}
			else if (MovingToLocation.MapLocationType == MapLocation.EMapLocationType.Headquarters && !flag2)
			{
				movementFlow.TeleportToDestination(m_PartyToken, MovingToLocation);
				CompleteMoveCallback();
			}
			else
			{
				if (!m_EventComplete)
				{
					movementFlow.PrepareTravelTo(m_PartyToken, ref m_WaypointsToEnd, MovingToLocation);
				}
				else
				{
					movementFlow.IncreaseZoomToParty(m_PartyToken, movementFlow.DelayToStartMoving);
				}
				CoroutineHelper.RunDelayedAction(movementFlow.DelayToStartMoving, delegate
				{
					movementFlow.MovePartyFromOriginToDestination(m_PartyToken, CompleteMoveCallback);
				});
			}
			m_EventComplete = false;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing StartMove.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnIntroTravelMessagesTrigger()
	{
		try
		{
			MapStoryController.MapDialogInfo message = new MapStoryController.MapDialogInfo(m_IntroTravelLines, null);
			Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.IntroTravel, message, FinishedShowingIntroTravelMessages);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing OnIntroTravelMessagesTrigger.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00014", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnOutroTravelMessagesTrigger()
	{
		try
		{
			MapStoryController.MapDialogInfo message = new MapStoryController.MapDialogInfo(m_OutroTravelLines, null);
			Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.OutroTravel, message, FinishedShowingOutroTravelMessages);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing OnOutroTravelMessagesTrigger.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00015", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnOutroGloomhavenMessagesTrigger()
	{
		try
		{
			MapStoryController.MapDialogInfo message = new MapStoryController.MapDialogInfo(m_OutroGloomhavenLines, null);
			Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.OutroGloomhaven, message, FinishedShowingOutroGloomhavenMessages);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing OnOutroGloomhavenMessagesTrigger.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00016", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void FinishedShowingIntroTravelMessages(EMapMessageTrigger mapMessageTrigger)
	{
		try
		{
			if (!m_PartyToken.IsMoving)
			{
				CompleteMoveCallback();
			}
			else
			{
				m_PartyToken.SetOnArriveCallback(CompleteMoveCallback);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing FinishedShowingIntroTravelMessages.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void FinishedShowingOutroTravelMessages(EMapMessageTrigger mapMessageTrigger)
	{
		try
		{
			if (m_OutroGloomhavenLines.Count > 0)
			{
				movementFlow.MovePartyFromNarrativeToDestination(m_PartyToken, OnOutroGloomhavenMessagesTrigger);
			}
			else
			{
				movementFlow.MovePartyFromNarrativeToDestination(m_PartyToken, CompleteMoveCallback);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing FinishedShowingOutroTravelMessages.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00018", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void FinishedShowingOutroGloomhavenMessages(EMapMessageTrigger mapMessageTrigger)
	{
		try
		{
			if (m_PartyToken.IsMoving)
			{
				m_PartyToken.SetOnArriveCallback(CompleteMoveCallback);
			}
			else
			{
				CompleteMoveCallback();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing FinishedShowingOutroGloomhavenMessages.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00019", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnEventTrigger()
	{
		try
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CRoadEvent_MapDLLMessage(), processImmediately: false);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing OnEventTrigger.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ShowEncounteredEvent(CCardDeck eventDeck, string eventType)
	{
		try
		{
			CRoadEvent cRoadEvent = null;
			string roadEvent = null;
			bool flag = false;
			if (eventType == "RoadEvent" && RoadEventDebugOverride != null)
			{
				cRoadEvent = RoadEventDebugOverride;
				RoadEventDebugOverride = null;
				flag = true;
			}
			else if (eventType == "CityEvent" && CityEventDebugOverride != null)
			{
				cRoadEvent = CityEventDebugOverride;
				CityEventDebugOverride = null;
				flag = true;
			}
			else if (AdventureState.MapState.IsCampaign)
			{
				roadEvent = eventDeck.DrawCard(CCardDeck.EShuffle.None, CCardDeck.EDiscard.None);
			}
			else
			{
				roadEvent = MovingToLocation.LocationQuest.ScenarioState.RoadEventID;
			}
			if (roadEvent.IsNOTNullOrEmpty())
			{
				cRoadEvent = MapRuleLibraryClient.MRLYML.RoadEvents.SingleOrDefault((CRoadEvent e) => e.ID == roadEvent);
				if (cRoadEvent == null)
				{
					cRoadEvent = MapRuleLibraryClient.MRLYML.CityEvents.SingleOrDefault((CRoadEvent e) => e.ID == roadEvent);
				}
			}
			CheckExpansionLoaded(eventDeck, eventType);
			bool flag2 = true;
			if (AdventureState.MapState.IsCampaign && eventType == "RoadEvent")
			{
				CRoadEvent cRoadEvent2 = null;
				string iD = ((CMapPhaseRoadEvent)AdventureState.MapState.CurrentMapPhase).EndLocation.ID;
				while (flag2)
				{
					if (cRoadEvent == cRoadEvent2)
					{
						flag2 = false;
						cRoadEvent = null;
					}
					else
					{
						if (cRoadEvent2 == null)
						{
							cRoadEvent2 = cRoadEvent;
						}
						if ((iD.ToUpper().Contains("JOTL") && cRoadEvent.Expansion == "") || !iD.ToUpper().Contains(cRoadEvent.Expansion.ToUpper()))
						{
							cRoadEvent = GetNewEvent(eventDeck, flag ? null : cRoadEvent);
						}
						else if (cRoadEvent.RequiredClass != null && cRoadEvent.RequiredClass.Count > 0 && !cRoadEvent.RequiredClass.Contains("None"))
						{
							bool flag3 = false;
							foreach (string rclass in cRoadEvent.RequiredClass)
							{
								if (AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == rclass) != null)
								{
									flag3 = true;
								}
							}
							if (flag3)
							{
								flag2 = false;
							}
							else
							{
								cRoadEvent = GetNewEvent(eventDeck, flag ? null : cRoadEvent);
							}
						}
						else
						{
							flag2 = false;
						}
					}
					flag = false;
				}
			}
			if (cRoadEvent == null || cRoadEvent.EventType == "RoadEvent")
			{
				Singleton<UIEventPanel>.Instance.StartEvent(cRoadEvent, ContinueMoveAfterEvent);
			}
			else
			{
				actionProgression.RequestPauseActions(this);
				Singleton<UIEventPanel>.Instance.StartEvent(cRoadEvent, FinishCityEvent);
			}
			movementFlow.IncreaseZoomToParty(m_PartyToken, UIInfoTools.Instance.eventZoomDuration);
			SimpleLog.AddToSimpleLog("Encountered Road Event: " + ((cRoadEvent == null) ? "None" : cRoadEvent.ID));
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing ShowEncounteredEvent.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private CRoadEvent GetNewEvent(CCardDeck eventDeck, CRoadEvent rolledEvent)
	{
		if (rolledEvent != null)
		{
			eventDeck.AddCard(rolledEvent.ID, CCardDeck.EAddCard.Bottom, allowDups: false);
		}
		string roadEvent = eventDeck.DrawCard(CCardDeck.EShuffle.None, CCardDeck.EDiscard.None);
		rolledEvent = MapRuleLibraryClient.MRLYML.RoadEvents.SingleOrDefault((CRoadEvent e) => e.ID == roadEvent);
		if (rolledEvent == null)
		{
			rolledEvent = MapRuleLibraryClient.MRLYML.CityEvents.SingleOrDefault((CRoadEvent e) => e.ID == roadEvent);
		}
		return rolledEvent;
	}

	public void CheckExpansionLoaded(CCardDeck eventDeck, string eventType)
	{
		if (eventType == "RoadEvent" && !AdventureState.MapState.MapParty.JOTLEventsLoaded && MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents != null && MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents.Count > 0)
		{
			AdventureState.MapState.MapParty.JOTLEventsLoaded = true;
			if (!eventDeck.Cards.Any((string x) => x.ToUpper().Contains("JOTL")))
			{
				CCardDeck cCardDeck = new CCardDeck(MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents);
				cCardDeck.Shuffle();
				eventDeck.ExpandDeck(cCardDeck.Cards);
			}
		}
	}

	public void OpenCityEvent()
	{
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIMapMultiplayerController>.Instance.OnSelectedCityEvent();
			if (FFSNetwork.IsHost)
			{
				DetermineHostToggleInteractability();
			}
			else
			{
				DetermineClientToggleInteractability();
			}
		}
		else
		{
			OpenCityEventInternal();
		}
	}

	public void ProxyOpenCityEvent(GameAction action)
	{
		OpenCityEventInternal();
	}

	private void OpenCityEventInternal()
	{
		NewPartyDisplayUI.PartyDisplay.Hide(this);
		Singleton<QuestManager>.Instance.HideLogScreen(this, instant: false);
		Singleton<UIGuildmasterHUD>.Instance.Hide(this, instant: false);
		zoomBeforeCityEvent = CameraController.s_CameraController.Zoom;
		ShowEncounteredEvent(AdventureState.MapState.MapParty.CityEventDeck, "CityEvent");
		AdventureState.MapState.CanDrawCityEvent = false;
	}

	private void FinishCityEvent()
	{
		Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.Reset();
			Singleton<UIMapMultiplayerController>.Instance.ResetUI();
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
			Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
		}
		AdventureState.MapState.CheckNonTrophyAchievements();
		AdventureState.MapState.CheckPersonalQuests();
		SaveData.Instance.Global.CurrentAdventureData.Save();
		if (FFSNetwork.IsHost)
		{
			Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
		}
		if (AdventureState.MapState.QueuedUnlockedQuestIDs.Count > 0)
		{
			foreach (string queuedUnlockedQuestID in AdventureState.MapState.QueuedUnlockedQuestIDs)
			{
				m_RecentUnlockedQuestLocations.Add(queuedUnlockedQuestID);
			}
			RefreshAllMapLocations(FinishCityEvent);
		}
		else
		{
			CameraController.s_CameraController.ZoomToFOV(zoomBeforeCityEvent, UIInfoTools.Instance.eventZoomDuration);
			Singleton<UIGuildmasterHUD>.Instance.Show(this, instant: false);
			Singleton<QuestManager>.Instance.ShowLogScreen(this, instant: false);
			NewPartyDisplayUI.PartyDisplay.Show(this);
			actionProgression.RequestResumeActions(this);
		}
	}

	private void ContinueMoveAfterEvent()
	{
		try
		{
			if (MovingToLocation.MapLocationType != MapLocation.EMapLocationType.Headquarters)
			{
				if (MovingToLocation.Location.ID.ToUpper().Contains("JOTL"))
				{
					m_IsMoving = false;
					m_EventComplete = true;
					movementFlow.MovePartyFromJOTLEncounter(m_PartyToken, delegate
					{
						CheckCampaignIntro(MovingToLocation, first: false);
					});
				}
				else if (m_IntroTravelLines.Count > 0)
				{
					movementFlow.MovePartyFromEncounterToDestination(m_PartyToken, OnIntroTravelMessagesTrigger);
				}
				else
				{
					movementFlow.MovePartyFromEncounterToDestination(m_PartyToken, CompleteMoveCallback);
				}
			}
			else
			{
				movementFlow.MovePartyFromEncounterToDestination(m_PartyToken, CompleteMoveCallback);
			}
			ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.None);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing ContinueMoveAfterEvent.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void CompleteMoveCallback()
	{
		try
		{
			m_IsMoving = false;
			switch (MovingToLocation.MapLocationType)
			{
			case MapLocation.EMapLocationType.Boss:
			case MapLocation.EMapLocationType.Scenario:
			case MapLocation.EMapLocationType.Headquarters:
				m_CurrentLocation = MovingToLocation;
				break;
			case MapLocation.EMapLocationType.Village:
				Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: false, this);
				m_CurrentLocation = MovingToLocation.GetCurrentScenarioLocation();
				break;
			}
			MovingToLocation = null;
			if (AdventureState.MapState.IsCampaign && m_CurrentLocation.MapLocationType == MapLocation.EMapLocationType.Headquarters)
			{
				StartCoroutine(ShowQueuedContent(saveAfterShow: false, delegate
				{
					MapRuleLibraryClient.Instance.AddQueueMessage(new CMoveComplete_MapDLLMessage(m_CurrentLocation.Location), processImmediately: false);
				}, clearQueuedRewards: false));
			}
			else
			{
				MapRuleLibraryClient.Instance.AddQueueMessage(new CMoveComplete_MapDLLMessage(m_CurrentLocation.Location), processImmediately: false);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing CompleteMoveCallback.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void EnterLoadout(CMapScenarioState location)
	{
		Singleton<UILoadoutManager>.Instance.EnterLoadout(AdventureState.MapState.MapParty, location.QuestState, EnterScenario);
		NewPartyDisplayUI.PartyDisplay.Show(this);
	}

	private void PreloadAbilityCardsPools()
	{
		CMapCharacter[] selectedCharactersArray = AdventureState.MapState.MapParty.SelectedCharactersArray;
		if (selectedCharactersArray == null)
		{
			return;
		}
		CMapCharacter[] array = selectedCharactersArray;
		foreach (CMapCharacter cMapCharacter in array)
		{
			if (cMapCharacter == null)
			{
				continue;
			}
			foreach (CAbilityCard ownedAbilityCard in cMapCharacter.GetOwnedAbilityCards())
			{
				if (ownedAbilityCard.GetAbilityCardYML == null)
				{
					Debug.LogError("Ability card data is null! Check YML file for " + ownedAbilityCard.Name);
				}
				else
				{
					ObjectPool.CreatePooledAbilityCard(ownedAbilityCard.ID, 2);
				}
			}
		}
	}

	private void EnterScenario()
	{
		try
		{
			PreloadAbilityCardsPools();
			ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.None);
			NewPartyDisplayUI.PartyDisplay.DestroyBenchedCharacters();
			PlayerRegistry.StartWaitingForPlayers();
			if (FFSNetwork.IsOnline)
			{
				if (FFSNetwork.IsHost)
				{
					Synchronizer.SendGameAction(GameActionType.EnterScenario, ActionPhaseType.MapLoadoutScreen);
				}
				ActionProcessor.SetState(ActionProcessorStateType.Halted);
			}
			MapRuleLibraryClient.Instance.AddQueueMessage(new CMapDLLMessage(EMapDLLMessageType.EnteredScenario), processImmediately: false);
			AdventureState.MapState.EnterScenario();
			if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
			{
				SceneController.Instance.CampaignScenarioStart();
			}
			else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
			{
				SceneController.Instance.NewAdventureScenarioStart();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while processing EnterScenario.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void RefreshLocationLines(MapLocation currentLocation)
	{
		try
		{
			if (!(currentLocation != null))
			{
				return;
			}
			foreach (MapLocation item in m_Villages.Where((MapLocation x) => x != currentLocation))
			{
				item.RefreshLocationLine(currentLocation);
			}
			foreach (MapLocation item2 in m_Scenarios.Where((MapLocation x) => x != currentLocation))
			{
				item2.RefreshLocationLine(currentLocation, IsLinkedQuestLocation(item2) && !m_LocationSelected && !HeadquartersLocation.IsHighlighted);
			}
			if (IsChoosingLinkedQuestOption())
			{
				HeadquartersLocation.RefreshLocationLine(currentLocation, !m_LocationSelected && !m_LinkedQuestLocation.IsHighlighted);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the MapChoreographer.RefreshLocationLines().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ConnectUnlockedVillages()
	{
		try
		{
			foreach (MapLocation village in m_Villages)
			{
				if (!(village.Location is CVillageState cVillageState))
				{
					continue;
				}
				foreach (string connectedVillage in cVillageState.ConnectedVillageIDs)
				{
					MapLocation mapLocation = m_Villages.SingleOrDefault((MapLocation v) => v.Location.ID == connectedVillage);
					if (mapLocation != null)
					{
						village.AddVillageRoad(mapLocation);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the MapChoreographer.ConnectUnlockedVillages().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private IEnumerator ShowQueuedContent(bool saveAfterShow, Action callback = null, bool clearQueuedRewards = true)
	{
		ShowQueuedQuestRewards(saveAfterShow, clearQueuedRewards);
		while (m_ShowingQuestRewards)
		{
			yield return null;
		}
		ShowQueuedQuestMessages();
		while (m_ShowingQuestMessages)
		{
			yield return null;
		}
		callback?.Invoke();
	}

	public void ShowQueuedQuestRewards(bool save, bool clearQueuedRewards = true)
	{
		List<Reward> list;
		if (AdventureState.MapState.IsCampaign)
		{
			bool unlocksEnchantress = AdventureState.MapState.QueuedCompletionRewards.Exists((Reward it) => it.Type == ETreasureType.UnlockEnhancer);
			list = AdventureState.MapState.QueuedCompletionRewards.FindAll((Reward it) => it.Type == ETreasureType.UnlockQuest || (it.IsVisibleInUI() && (!unlocksEnchantress || it.Type != ETreasureType.Enhancement)));
		}
		else
		{
			list = AdventureState.MapState.QueuedCompletionRewards.FindAll((Reward it) => it.IsVisibleInUI());
		}
		if (clearQueuedRewards)
		{
			AdventureState.MapState.QueuedCompletionRewards.Clear();
		}
		List<Reward> personalQuestRewards = AdventureState.MapState.MapParty.SelectedCharacters.Where((CMapCharacter it) => it.PersonalQuest != null && it.PersonalQuest.State == EPersonalQuestState.Completed).SelectMany((CMapCharacter it) => it.PersonalQuest.CurrentPersonalQuestStepData.IsPersonalQuestStep ? it.PersonalQuest.RewardsByStep[it.PersonalQuest.CurrentPersonalQuestStep].SelectMany((RewardGroup rewardGroup) => rewardGroup.Rewards) : it.PersonalQuest.FinalRewards.SelectMany((RewardGroup rewardGroup) => rewardGroup.Rewards)).ToList();
		for (int num = 0; num < personalQuestRewards.Count; num++)
		{
			if (personalQuestRewards[num].Type == ETreasureType.UnlockQuest)
			{
				m_RecentUnlockedQuestLocations.Remove(personalQuestRewards[num].UnlockName);
			}
		}
		if (list.Count > 0)
		{
			m_ShowingQuestRewards = true;
			float zoomBefore = CameraController.s_CameraController.Zoom;
			movementFlow.IncreaseZoomToParty(m_PartyToken, UIInfoTools.Instance.eventZoomDuration, delegate
			{
				if (!m_ShowingQuestRewards)
				{
					CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				}
			});
			List<Reward> list2 = list.FindAll((Reward it) => !personalQuestRewards.Contains(it));
			string term = "GUI_QUEST_COMPLETED_REWARDS";
			if (AdventureState.MapState.IsCampaign && list2.Count > 0)
			{
				List<CPartyAchievement> source = AdventureState.MapState.MapParty.Achievements.FindAll((CPartyAchievement it) => it.State == EAchievementState.Completed && it.Achievement.AchievementType == EAchievementType.Campaign);
				int num2 = 0;
				CPartyAchievement cPartyAchievement = null;
				for (int num3 = 0; num3 < list2.Count; num3++)
				{
					Reward reward = list2[num3];
					CPartyAchievement cPartyAchievement2 = source.FirstOrDefault((CPartyAchievement it) => it.Rewards.Exists((RewardGroup group) => group.Rewards.Contains(reward)));
					if (cPartyAchievement2 != null)
					{
						num2++;
						if (reward.Type == ETreasureType.UnlockCharacter && LocalizationManager.TryGetTranslation(cPartyAchievement2.Achievement.LocalizedRewardsStory, out var _))
						{
							cPartyAchievement = cPartyAchievement2;
						}
					}
				}
				if (cPartyAchievement != null)
				{
					string text = list.FirstOrDefault((Reward it) => it.Type == ETreasureType.UnlockCharacter)?.CharacterID;
					Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.AchievementClaimed, new MapStoryController.MapDialogInfo(new DialogLineDTO(cPartyAchievement.Achievement.LocalizedRewardsStory, text ?? "Narrator"), null, hideOtherUI: false));
					if (list2.Count == num2)
					{
						term = cPartyAchievement.Achievement.LocalizedRewardsTitle;
					}
				}
			}
			Singleton<UIAdventureRewardsManager>.Instance.ShowRewards(list2, LocalizationManager.GetTranslation(term), "GUI_QUEST_COMPLETED_REWARDS_CLOSE", delegate
			{
				ShowUnlockedQuests(zoomBefore).Then((Func<ICallbackPromise>)ShowPersonalQuestsProgress).Done(delegate
				{
					CameraController.s_CameraController.DisableCameraInput(disableInput: false);
					m_ShowingQuestRewards = false;
					if (save)
					{
						SaveData.Instance.SaveCurrentAdventureData();
					}
				});
			});
		}
		else if (AdventureState.MapState.IsCampaign)
		{
			m_ShowingQuestRewards = true;
			ShowUnlockedQuests().Then((Func<ICallbackPromise>)ShowPersonalQuestsProgress).Done(delegate
			{
				m_ShowingQuestRewards = false;
			});
		}
	}

	private ICallbackPromise ShowPersonalQuestsProgress()
	{
		try
		{
			if (AdventureState.MapState.IsCampaign && !IsChoosingLinkedQuestOption() && !m_ResultsProcessFinished)
			{
				Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: true, this);
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
				return Singleton<UIPersonalQuestResultManager>.Instance.ShowPersonalQuestsProgress(AdventureState.MapState.MapParty.SelectedCharacters.ToArray()).Then(delegate
				{
					if (FFSNetwork.IsOnline && AdventureState.MapState.MapParty.ExistsCharacterToRetire())
					{
						Singleton<UIMapMultiplayerController>.Instance.ShowRetirementMultiplayer();
					}
					actionProgression.RequestResumeActions(this, "RetireCharacter");
					CallbackPromise callbackPromise = new CallbackPromise();
					if (base.gameObject.activeInHierarchy)
					{
						StartCoroutine(WaitAllRetired(callbackPromise.Resolve));
					}
					return callbackPromise;
				}).Then(delegate
				{
					Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: false, this);
				});
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the ShowPersonalQuestsProgress function\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return CallbackPromise.Resolved();
	}

	private IEnumerator WaitAllRetired(Action callback)
	{
		Debug.LogGUI("WaitAllRetired");
		NewPartyDisplayUI.PartyDisplay.EnableAssignPlayer(enable: false, this);
		yield return new WaitUntil(() => !AdventureState.MapState.MapParty.ExistsCharacterToRetire() && QueuedRetirements.Count == 0);
		NewPartyDisplayUI.PartyDisplay.EnableAssignPlayer(enable: true, this);
		Debug.LogGUI("Finish WaitAllRetired");
		callback?.Invoke();
	}

	private IEnumerator WaitDistributionEnds(Action callback)
	{
		Debug.LogGUI("Wait distribution finished");
		yield return new WaitUntil(() => !Singleton<UIDistributeRewardManager>.Instance.IsDistributing);
		Debug.LogGUI("Finish wait distribution");
		callback?.Invoke();
	}

	public void ShowQueuedQuestMessages()
	{
		m_ShowingQuestMessages = true;
		List<MapStoryController.MapDialogInfo> list = new List<MapStoryController.MapDialogInfo>();
		List<Tuple<Vector3, float>> list2 = new List<Tuple<Vector3, float>>();
		foreach (string questID in AdventureState.MapState.QueuedCompletedQuestIDs)
		{
			CQuest cQuest = MapRuleLibraryClient.MRLYML.Quests.SingleOrDefault((CQuest q) => q.ID == questID);
			if (cQuest == null)
			{
				continue;
			}
			if (cQuest.CompleteDialogueLines.Count > 0)
			{
				List<DialogLineDTO> list3 = new List<DialogLineDTO>();
				foreach (MapDialogueLine completeDialogueLine in cQuest.CompleteDialogueLines)
				{
					list3.Add(new DialogLineDTO(completeDialogueLine));
				}
				list.Add(new MapStoryController.MapDialogInfo(list3, null));
			}
			foreach (VisibilitySphereYML.VisibilitySphereDefinition completeSphereDefinition in cQuest.CompleteSphereDefinitions)
			{
				Vector3 item = new Vector3(completeSphereDefinition.MapLocation.X, completeSphereDefinition.MapLocation.Y, completeSphereDefinition.MapLocation.Z);
				list2.Add(new Tuple<Vector3, float>(item, completeSphereDefinition.Radius));
			}
		}
		foreach (string questID2 in AdventureState.MapState.QueuedUnlockedQuestIDs)
		{
			CQuest cQuest2 = MapRuleLibraryClient.MRLYML.Quests.SingleOrDefault((CQuest q) => q.ID == questID2);
			if (cQuest2 == null || cQuest2.UnlockDialogueLines.Count <= 0)
			{
				continue;
			}
			List<DialogLineDTO> list4 = new List<DialogLineDTO>();
			foreach (MapDialogueLine unlockDialogueLine in cQuest2.UnlockDialogueLines)
			{
				list4.Add(new DialogLineDTO(unlockDialogueLine));
			}
			list.Add(new MapStoryController.MapDialogInfo(list4, null));
			foreach (VisibilitySphereYML.VisibilitySphereDefinition unlockSphereDefinition in cQuest2.UnlockSphereDefinitions)
			{
				Vector3 item2 = new Vector3(unlockSphereDefinition.MapLocation.X, unlockSphereDefinition.MapLocation.Y, unlockSphereDefinition.MapLocation.Z);
				list2.Add(new Tuple<Vector3, float>(item2, unlockSphereDefinition.Radius));
			}
		}
		if (list2.Count > 0)
		{
			StartCoroutine(RevealFog(list2));
		}
		if (list.Count > 0)
		{
			if (FFSNetwork.IsOnline && FFSNetwork.IsClient && !m_ResultsProcessFinished)
			{
				Singleton<MapStoryController>.Instance.Clear();
			}
			Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.QuestUnlockedOrCompleted, list, FinishedShowingQuestMessages);
			return;
		}
		if (AdventureState.MapState.IsCampaign && AdventureState.MapState.QueuedUnlockedQuestIDs.Count > 0)
		{
			AdventureState.MapState.QueuedUnlockedQuestIDs.Clear();
			SaveData.Instance.SaveCurrentAdventureData();
		}
		GetUnlockedMessagesForTrigger(EMapMessageTrigger.Map);
	}

	private void FinishedShowingQuestMessages(EMapMessageTrigger mapMessageTrigger)
	{
		AdventureState.MapState.QueuedUnlockedQuestIDs.Clear();
		AdventureState.MapState.QueuedCompletedQuestIDs.Clear();
		SaveData.Instance.SaveCurrentAdventureData();
		m_ShowingQuestMessages = false;
		GetUnlockedMessagesForTrigger(EMapMessageTrigger.Map);
	}

	public void ShowAchievementCompletionMessage(CPartyAchievement achievement)
	{
		List<MapStoryController.MapDialogInfo> list = new List<MapStoryController.MapDialogInfo>();
		List<DialogLineDTO> list2 = new List<DialogLineDTO>();
		foreach (MapDialogueLine completeDialogueLine in achievement.Achievement.CompleteDialogueLines)
		{
			list2.Add(new DialogLineDTO(completeDialogueLine));
		}
		list.Add(new MapStoryController.MapDialogInfo(list2, null));
		if (list.Count > 0)
		{
			Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.AchievementClaimed, list, FinishedShowingAchievementCompletionMessage);
		}
		else
		{
			FinishedShowingAchievementCompletionMessage(EMapMessageTrigger.AchievementClaimed);
		}
	}

	public void FinishedShowingAchievementCompletionMessage(EMapMessageTrigger mapMessageTrigger)
	{
		if (AdventureState.MapState.QueuedUnlockedQuestIDs.Count > 0)
		{
			RefreshAllMapLocations();
		}
	}

	public void GetUnlockedMessagesForTrigger(EMapMessageTrigger messageTrigger, CQuestState quest = null)
	{
		MapRuleLibraryClient.Instance.AddQueueMessage(new CGetMapMessagesForTrigger_MapDLLMessage(messageTrigger), processImmediately: false);
	}

	private void ShowMapMessages(EMapMessageTrigger messageTrigger, List<CMapMessageState> mapMessages)
	{
		if (mapMessages.Count > 0)
		{
			Singleton<MapStoryController>.Instance.Show(messageTrigger, mapMessages, null, OnAllMapMessagesShown);
		}
		else
		{
			OnAllMapMessagesShown(messageTrigger);
		}
	}

	public void OnMapMessageShown(CMapMessageState mapMessageState)
	{
		mapMessageState.MapMessageShown();
		CameraController.s_CameraController.m_TargetFocalPoint = m_PartyToken.transform.position;
	}

	public void OnAllMapMessagesShown(EMapMessageTrigger messageTrigger)
	{
		m_ShowingQuestMessages = false;
		if (m_QueuedMoveLocation != null)
		{
			OnMoveClick(m_QueuedMoveLocation);
		}
	}

	public void RequestRegenerateAllMapScenarios()
	{
		if (FFSNetwork.IsOnline)
		{
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.RegenerateAllScenarios);
		}
		else
		{
			RegenerateAllMapScenarios();
		}
	}

	public void ProxyRequestRegenerateAllMapScenarios()
	{
		RegenerateAllMapScenarios();
		Debug.Log("[IsSwitchingCharacter] attempting to set at ProxyRequestRegenerateAllMapScenarios");
		if (PlayerRegistry.IsSwitchingCharacter)
		{
			Debug.Log("[IsSwitchingCharacter] False - ProxyRequestRegenerateAllMapScenarios");
			PlayerRegistry.IsSwitchingCharacter = false;
		}
	}

	public void RegenerateAllMapScenarios(bool rerollQuestRewards = false)
	{
		if (AdventureState.MapState.MapParty.SelectedCharacters.Count() != 0)
		{
			AdventureState.MapState.RegenerateAllMapScenarios(excludeInProgressQuest: false, rerollQuestRewards);
			Singleton<QuestManager>.Instance.RefreshLockedQuests();
			Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
		}
		else
		{
			Debug.LogWarning("Regenerate Scenarios was not called as there are no selected characters");
		}
	}

	public void RefreshAllMapLocations(Action onFinish = null, bool save = true)
	{
		List<VisibilitySphereYML.VisibilitySphereDefinition> list = new List<VisibilitySphereYML.VisibilitySphereDefinition>();
		foreach (CLocationState item in AdventureState.MapState.AllVillages.Where((CLocationState w) => m_Villages.SingleOrDefault((MapLocation v) => v.Location == w) == null && w.LocationState != ELocationState.Locked))
		{
			MapLocation mapLocation = UnityEngine.Object.Instantiate(mapLocationPrefab, m_VillagesParent.transform);
			mapLocation.transform.position = new Vector3(item.Location.MapLocation.X, item.Location.MapLocation.Y, item.Location.MapLocation.Z);
			mapLocation.Init(item, OnMapLocationSelect, OnMapLocationHighlight);
			m_Villages.Add(mapLocation);
		}
		foreach (CQuestState item2 in AdventureState.MapState.AllQuests.Where((CQuestState w) => m_Scenarios.SingleOrDefault((MapLocation v) => v.LocationQuest == w) == null && w.Quest.Type != EQuestType.Travel && (w.Quest.Type != EQuestType.Job || (AdventureState.MapState.TutorialCompleted && AdventureState.MapState.IntroCompleted)) && ((w.IsIntroQuest && w.QuestState == CQuestState.EQuestState.Unlocked) || (!w.IsIntroQuest && w.QuestState != CQuestState.EQuestState.Locked && w.QuestState != CQuestState.EQuestState.Blocked))))
		{
			if (!AdventureState.MapState.QueuedUnlockedQuestIDs.Contains(item2.ID))
			{
				list.AddRange(item2.Quest.UnlockSphereDefinitions);
			}
			CMapScenarioState scenarioState = item2.ScenarioState;
			MapLocation mapLocation2 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_ScenariosParent.transform);
			if (item2 is CJobQuestState cJobQuestState)
			{
				mapLocation2.transform.position = new Vector3(cJobQuestState.JobMapLocation.X, cJobQuestState.JobMapLocation.Y, cJobQuestState.JobMapLocation.Z);
			}
			else
			{
				mapLocation2.transform.position = new Vector3(scenarioState.Location.MapLocation.X, scenarioState.Location.MapLocation.Y, scenarioState.Location.MapLocation.Z);
			}
			mapLocation2.Init(scenarioState, OnMapLocationSelect, OnMapLocationHighlight, item2);
			m_Scenarios.Add(mapLocation2);
			if (item2.Quest.Type == EQuestType.City)
			{
				m_CityLocations.Add(mapLocation2);
			}
		}
		foreach (CQuestState quest in AdventureState.MapState.AllTravelQuests.Where((CQuestState w) => m_Villages.SingleOrDefault((MapLocation v) => v.LocationQuest == w) == null && ((w.IsIntroQuest && w.QuestState == CQuestState.EQuestState.Unlocked) || (!w.IsIntroQuest && w.QuestState != CQuestState.EQuestState.Locked && w.QuestState != CQuestState.EQuestState.Blocked))))
		{
			if (!AdventureState.MapState.QueuedUnlockedQuestIDs.Contains(quest.ID))
			{
				foreach (VisibilitySphereYML.VisibilitySphereDefinition unlockSphereDefinition in quest.Quest.UnlockSphereDefinitions)
				{
					_ = unlockSphereDefinition;
					list.AddRange(quest.Quest.UnlockSphereDefinitions);
				}
			}
			CLocationState endingVillage = AdventureState.MapState.AllVillages.SingleOrDefault((CLocationState v) => v.ID == quest.Quest.EndingVillage);
			MapLocation mapLocation3 = null;
			if (endingVillage.LocationState == ELocationState.Locked)
			{
				mapLocation3 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_VillagesParent.transform);
				mapLocation3.transform.position = new Vector3(endingVillage.Location.MapLocation.X, endingVillage.Location.MapLocation.Y, endingVillage.Location.MapLocation.Z);
				mapLocation3.Init(endingVillage, OnMapLocationSelect, OnMapLocationHighlight, quest);
				m_Villages.Add(mapLocation3);
				if (quest.Quest.Type == EQuestType.City)
				{
					m_CityLocations.Add(mapLocation3);
				}
			}
			else
			{
				mapLocation3 = m_Villages.Find((MapLocation x) => x.Location == endingVillage);
				mapLocation3.Init(endingVillage, OnMapLocationSelect, OnMapLocationHighlight, quest);
			}
			CMapScenarioState scenarioState2 = quest.ScenarioState;
			MapLocation mapLocation4 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_ScenariosParent.transform);
			mapLocation4.transform.position = new Vector3(scenarioState2.Location.MapLocation.X, scenarioState2.Location.MapLocation.Y, scenarioState2.Location.MapLocation.Z);
			mapLocation4.Init(scenarioState2, null, null);
			m_Scenarios.Add(mapLocation3);
			mapLocation4.gameObject.SetActive(value: false);
			mapLocation3.AddScenarioLocation(mapLocation4);
		}
		ConnectUnlockedVillages();
		foreach (MapLocation village in m_Villages)
		{
			list.AddRange(village.GetVillageRoadPositions());
		}
		foreach (CQuestState allCompletedQuest in AdventureState.MapState.AllCompletedQuests)
		{
			if (AdventureState.MapState.QueuedCompletedQuestIDs.Contains(allCompletedQuest.ID))
			{
				continue;
			}
			foreach (VisibilitySphereYML.VisibilitySphereDefinition completeSphereDefinition in allCompletedQuest.Quest.CompleteSphereDefinitions)
			{
				_ = completeSphereDefinition;
				list.AddRange(allCompletedQuest.Quest.CompleteSphereDefinitions);
			}
		}
		foreach (CVisibilitySphereState item3 in AdventureState.MapState.VisibilitySphereStates.Where((CVisibilitySphereState w) => w.VisibilitySphereState == CVisibilitySphereState.EVisibilitySphereState.Unlocked))
		{
			list.AddRange(item3.VisibilitySphere.SphereDefinitions);
		}
		RevealVisibilitySpheres(list);
		MapLocation mapLocation5 = (m_CurrentLocation = m_Villages.SingleOrDefault((MapLocation x) => x.Location == AdventureState.MapState.HeadquartersState));
		m_PartyToken.transform.position = mapLocation5.CenterPosition;
		RefreshLocationLines((m_TempCurrentLocation != null) ? m_TempCurrentLocation : m_CurrentLocation);
		if (!((NewPartyDisplayUI)Singleton<APartyDisplayUI>.Instance).Initialised)
		{
			((NewPartyDisplayUI)Singleton<APartyDisplayUI>.Instance).Init(AdventureState.MapState.MapParty);
		}
		Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
		OnMapLocationsChanged();
		if (m_ResultsProcessFinished)
		{
			StartCoroutine(ShowQueuedContent(saveAfterShow: false, onFinish));
			onFinish = null;
		}
		GetUnlockedMessagesForTrigger(EMapMessageTrigger.Map);
		if (save)
		{
			SaveData.Instance.SaveCurrentAdventureData();
			SaveData.Instance.Global.CurrentAdventureData.EnqueueSaveCheckpoint(onFinish);
		}
	}

	private MapLocation GetMoveTargetLocation(string locationID)
	{
		if (HeadquartersLocation.Location.ID == locationID)
		{
			return HeadquartersLocation;
		}
		return m_Scenarios.SingleOrDefault((MapLocation x) => x.Location.ID == locationID);
	}

	public void RevealVisibilitySpheres(List<VisibilitySphereYML.VisibilitySphereDefinition> sphereDefinitions, bool revealInstantly = false)
	{
		List<VisibilitySphereYML.VisibilitySphereDefinition> list = new List<VisibilitySphereYML.VisibilitySphereDefinition>();
		foreach (VisibilitySphereYML.VisibilitySphereDefinition sphere in sphereDefinitions)
		{
			if (!m_RevealedVisibilitySpheres.Any((VisibilitySphereYML.VisibilitySphereDefinition x) => x.Equals(sphere)))
			{
				list.Add(sphere);
			}
		}
		List<Tuple<Vector3, float>> list2 = new List<Tuple<Vector3, float>>();
		foreach (VisibilitySphereYML.VisibilitySphereDefinition item2 in list)
		{
			Vector3 item = new Vector3(item2.MapLocation.X, item2.MapLocation.Y, item2.MapLocation.Z);
			list2.Add(new Tuple<Vector3, float>(item, item2.Radius));
		}
		if (list2.Count > 0)
		{
			if (revealInstantly)
			{
				RevealFogInstantly(list2);
			}
			else
			{
				StartCoroutine(RevealFog(list2));
			}
		}
		m_RevealedVisibilitySpheres.AddRange(list);
	}

	public void ResetVisibilitySpheres()
	{
		m_RevealedVisibilitySpheres.Clear();
		VolumetricFog.instance.ResetFogOfWar();
	}

	public IEnumerator RevealFog(List<Tuple<Vector3, float>> positions)
	{
		yield return new WaitForEndOfFrame();
		float alphaValue = 1f;
		while (alphaValue > 0f)
		{
			alphaValue = Math.Max(0f, alphaValue - m_FogAlphaDecrement);
			foreach (Tuple<Vector3, float> position in positions)
			{
				VolumetricFog.instance.SetFogOfWarAlpha(position.Item1, position.Item2, alphaValue, blendAlpha: true, 0f, 0f, 0f, 0f);
			}
			yield return Timekeeper.instance.WaitForSeconds(m_FogAnimationSpeed);
		}
	}

	public void RevealFogInstantly(List<Tuple<Vector3, float>> positions)
	{
		foreach (Tuple<Vector3, float> position in positions)
		{
			VolumetricFog.instance.SetFogOfWarAlpha(position.Item1, position.Item2, 0f, blendAlpha: true, 0f, 0f, 0f, 0f);
		}
	}

	public void RevealNewFog(List<Tuple<Vector3, float>> positions)
	{
		foreach (Tuple<Vector3, float> position in positions)
		{
			VolumetricFog.instance.SetFogOfWarAlpha(position.Item1, position.Item2, 0f, blendAlpha: true, 1f, 0f, 0f, 0f);
		}
	}

	public void DebugApplyAutoCompleteYML()
	{
		AdventureState.MapState.CheckAutoCompleteQuestsAndAchievements();
		SaveData.Instance.Global.CurrentAdventureData.Save();
		foreach (MapLocation village in m_Villages)
		{
			UnityEngine.Object.Destroy(village.gameObject);
		}
		m_Villages.Clear();
		foreach (MapLocation scenario in m_Scenarios)
		{
			UnityEngine.Object.Destroy(scenario.gameObject);
		}
		m_Scenarios.Clear();
		m_CityLocations.RemoveAll((MapLocation it) => it.MapLocationType != MapLocation.EMapLocationType.Store);
		RefreshAllMapLocations();
	}

	public void DebugShowAllScenarios()
	{
		try
		{
			if (MapFTUEManager.IsPlaying)
			{
				Singleton<MapFTUEManager>.Instance.Finish();
			}
			StartCoroutine(DebugShowAllScenariosCoroutine());
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the MapChoreographer.DebugShowAllScenarios().\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private bool ValidateShowAllScenarios()
	{
		return AdventureState.MapState.MapParty.SelectedCharacters.Any();
	}

	public IEnumerator DebugShowAllScenariosCoroutine()
	{
		if (!ValidateShowAllScenarios())
		{
			Debug.LogWarning("Can't unlock all scenarios on current game state!");
			yield break;
		}
		m_ShowAllScenariosMode = true;
		foreach (MapLocation village in m_Villages)
		{
			UnityEngine.Object.Destroy(village.gameObject);
		}
		foreach (MapLocation scenario in m_Scenarios)
		{
			UnityEngine.Object.Destroy(scenario.gameObject);
		}
		yield return new WaitForEndOfFrame();
		m_Villages.Clear();
		m_Scenarios.Clear();
		m_CityLocations.Clear();
		MapLocation mapLocation = UnityEngine.Object.Instantiate(mapLocationPrefab, m_VillagesParent.transform);
		mapLocation.transform.position = new Vector3(AdventureState.MapState.HeadquartersState.Location.MapLocation.X, AdventureState.MapState.HeadquartersState.Location.MapLocation.Y, AdventureState.MapState.HeadquartersState.Location.MapLocation.Z);
		mapLocation.Init(AdventureState.MapState.HeadquartersState, OnMapLocationSelect, OnMapLocationHighlight);
		m_Villages.Add(mapLocation);
		m_CurrentLocation = mapLocation;
		foreach (CQuestState quest in AdventureState.MapState.AllQuests.Where((CQuestState q) => q.Quest.Type != EQuestType.Job))
		{
			quest.UnlockQuest();
			if (quest.Quest.Type != EQuestType.Travel)
			{
				CMapScenarioState scenarioState = quest.ScenarioState;
				MapLocation mapLocation2 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_ScenariosParent.transform);
				mapLocation2.transform.position = new Vector3(scenarioState.Location.MapLocation.X, scenarioState.Location.MapLocation.Y, scenarioState.Location.MapLocation.Z);
				mapLocation2.Init(scenarioState, OnMapLocationSelect, OnMapLocationHighlight, quest);
				m_Scenarios.Add(mapLocation2);
				if (quest.Quest.Type == EQuestType.City)
				{
					m_CityLocations.Add(mapLocation2);
				}
				continue;
			}
			CLocationState cLocationState = AdventureState.MapState.AllVillages.SingleOrDefault((CLocationState v) => v.ID == quest.Quest.EndingVillage);
			MapLocation mapLocation3 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_VillagesParent.transform);
			mapLocation3.transform.position = new Vector3(cLocationState.Location.MapLocation.X, cLocationState.Location.MapLocation.Y, cLocationState.Location.MapLocation.Z);
			mapLocation3.Init(cLocationState, OnMapLocationSelect, OnMapLocationHighlight, quest);
			m_Villages.Add(mapLocation3);
			CMapScenarioState scenarioState2 = quest.ScenarioState;
			MapLocation mapLocation4 = UnityEngine.Object.Instantiate(mapLocationPrefab, m_ScenariosParent.transform);
			mapLocation4.transform.position = new Vector3(scenarioState2.Location.MapLocation.X, scenarioState2.Location.MapLocation.Y, scenarioState2.Location.MapLocation.Z);
			mapLocation4.Init(scenarioState2, null, null);
			m_Scenarios.Add(mapLocation3);
			mapLocation4.gameObject.SetActive(value: false);
			mapLocation3.AddScenarioLocation(mapLocation4);
		}
		VolumetricFog.instance.SetFogOfWarAlpha(new Vector3(0f, 0f, 0f), 1000f, 0f, blendAlpha: true, 0f, 0f, 0f, 0f);
		RefreshLocationLines((m_TempCurrentLocation != null) ? m_TempCurrentLocation : m_CurrentLocation);
		if (!((NewPartyDisplayUI)Singleton<APartyDisplayUI>.Instance).Initialised)
		{
			((NewPartyDisplayUI)Singleton<APartyDisplayUI>.Instance).Init(AdventureState.MapState.MapParty);
		}
		Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
		OnMapLocationsChanged();
	}

	private void OnMapLocationsChanged()
	{
		Singleton<QuestManager>.Instance.RefreshLockedQuests();
		RefreshShownLocationsByMap();
	}

	public void DebugAddJobQuestState(string jobID)
	{
		AdventureState.MapState.DebugAddJobQuest(jobID);
		SaveData.Instance.Global.CurrentAdventureData.Save();
		foreach (MapLocation village in m_Villages)
		{
			UnityEngine.Object.Destroy(village.gameObject);
		}
		m_Villages.Clear();
		foreach (MapLocation scenario in m_Scenarios)
		{
			UnityEngine.Object.Destroy(scenario.gameObject);
		}
		m_Scenarios.Clear();
		m_CityLocations.Clear();
		RefreshAllMapLocations();
	}

	public void ClientMoveToNode(GameAction action)
	{
		MapLocation moveTargetLocation = GetMoveTargetLocation(((LocationToken)action.SupplementaryDataToken).ID);
		Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode((moveTargetLocation.LocationQuest == null || moveTargetLocation.LocationQuest.Quest.Type != EQuestType.City) ? EGuildmasterMode.WorldMap : EGuildmasterMode.City);
		if (moveTargetLocation != null)
		{
			OnMoveClick(moveTargetLocation);
			return;
		}
		throw new Exception("Error moving to node. No target location found with ID: " + ((LocationToken)action.SupplementaryDataToken).ID);
	}

	public void ClientEnterScenario()
	{
		FFSNet.Console.LogInfo("Entering scenario.");
		Singleton<UILoadoutManager>.Instance.HideConfirmWarning();
		EnterScenario();
	}

	private IEnumerator MultiplayerStartup()
	{
		try
		{
			FFSNet.Console.LogInfo("Map multiplayer initialization.");
			Unsubscribe();
			if (FFSNetwork.IsOnline)
			{
				InitializeSelectQuestReadyUp();
				if (FFSNetwork.IsHost)
				{
					FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
				}
				ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
				ControllableRegistry.OnControllableDestroyed = (ControllablesChangedEvent)Delegate.Combine(ControllableRegistry.OnControllableDestroyed, new ControllablesChangedEvent(OnControllableDestroyed));
				ControllableRegistry.OnControllableObjectChanged = (ControllableObjectChangedEvent)Delegate.Combine(ControllableRegistry.OnControllableObjectChanged, new ControllableObjectChangedEvent(OnControllableObjectChanged));
				PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Combine(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnterRoom));
				PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
				PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
				PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
				PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningPlayerLeft));
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
				if (AdventureState.MapState.CurrentMapPhaseType == EMapPhaseType.AtLinkedScenario)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapAtLinkedScenario);
				}
				else
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
				}
				Singleton<UIMapMultiplayerController>.Instance.ShowMapMultiplayer();
			}
			else
			{
				FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
				FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the MultiplayerStartup function\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		if (!FFSNetwork.IsOnline)
		{
			yield break;
		}
		if (FFSNetwork.IsHost)
		{
			Synchronizer.SendSideAction(GameActionType.GameLoadedAndHostReady);
			SceneController.Instance.GameLoadedAndHostReady = true;
			yield break;
		}
		float time = Timekeeper.instance.m_GlobalClock.time;
		while (PlayerRegistry.MyPlayer == null)
		{
			if (Timekeeper.instance.m_GlobalClock.time - time > 60f || !FFSNetwork.IsOnline)
			{
				if (FFSNetwork.IsOnline)
				{
					string text = "Timed out waiting for MyPlayer to be assigned.";
					Debug.LogError(text);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00001", "GUI_ERROR_MAIN_MENU_BUTTON", "", UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, text);
				}
				yield break;
			}
			yield return null;
		}
		if (!PlayerRegistry.LoadingInFromJoiningClient)
		{
			Debug.Log("Client waiting for Host to finish loading map scene before continuing MultiplayerStartup");
			while (!SceneController.Instance.GameLoadedAndHostReady)
			{
				yield return null;
			}
		}
		if (ControllableRegistry.AllControllables.Any((NetworkControllable a) => a.NetworkEntity == null))
		{
			time = Timekeeper.instance.m_GlobalClock.time;
			while (ControllableRegistry.AllControllables.Any((NetworkControllable a) => a.NetworkEntity == null))
			{
				if (Timekeeper.instance.m_GlobalClock.time - time > 120f)
				{
					string text2 = "Timed out waiting for Network Entity to sync.\nNull Entities:\n" + string.Join("\n", from s in ControllableRegistry.AllControllables
						where s.NetworkEntity == null
						select s.ID);
					Debug.LogError(text2);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00001", "GUI_ERROR_MAIN_MENU_BUTTON", "", UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, text2);
					yield break;
				}
				Debug.Log("Attempting to sync Network Entities and found a null entity - retrying");
				yield return null;
			}
		}
		try
		{
			ControllableRegistry.AllControllables.ForEach(delegate(NetworkControllable x)
			{
				x.ApplyState();
			});
			if (!SceneController.Instance.GameLoadedAndClientReady)
			{
				Synchronizer.SendSideAction(GameActionType.GameLoadedAndClientReady, null, canBeUnreliable: false, sendToHostOnly: true);
				SceneController.Instance.GameLoadedAndClientReady = true;
			}
			if (FFSNetwork.IsClient && !PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment)
			{
				PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment = true;
				Synchronizer.SendSideAction(GameActionType.ReadyForAssignment, null, canBeUnreliable: false, sendToHostOnly: true);
			}
			PlayerRegistry.LoadingInFromJoiningClient = false;
		}
		catch (Exception ex2)
		{
			Debug.LogError("An exception occurred within the MultiplayerStartup function\n" + ex2.Message + "\n" + ex2.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
		}
	}

	public static void InitReadyToggle(EReadyUpToggleStates toggleState)
	{
		switch (toggleState)
		{
		case EReadyUpToggleStates.CityEvents:
			Singleton<UIReadyToggle>.Instance.InitializeLabelGamepad(LocalizationManager.GetTranslation("Consoles/GUI_HOTKEY_SELECT_QUEST"), EnterDungeonTextColor);
			Singleton<UIReadyToggle>.Instance.Initialize(show: false, delegate
			{
				Singleton<UIMapMultiplayerController>.Instance.ConfirmCityEvent();
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			}, delegate
			{
				Singleton<UIMapMultiplayerController>.Instance.OnReady(ready: false);
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			}, delegate
			{
				Singleton<UIReadyToggle>.Instance.Reset();
				Singleton<UIMapMultiplayerController>.Instance.ToggleReadyUpUI(show: false, EReadyUpToggleStates.CityEvents);
				Singleton<MapChoreographer>.Instance.OpenCityEventInternal();
			}, delegate(NetworkPlayer player, bool isReady)
			{
				Singleton<UIMapMultiplayerController>.Instance.UpdateReadyPlayer(player, isReady);
			}, null, null, "GUI_DRAW_CITY_EVENT", "GUI_CITY_EVENT_CANCEL", "PlaySound_UIMultiQuestSelected", bringToFront: false, delegate
			{
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			}, delegate
			{
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			}, validateReadyUpOnPlayerLeft: false, UIReadyToggle.EReadyUpType.Participant, toggleState);
			break;
		case EReadyUpToggleStates.Quests:
			Singleton<UIReadyToggle>.Instance.InitializeLabelGamepad(LocalizationManager.GetTranslation("Consoles/GUI_HOTKEY_SELECT_QUEST"), EnterDungeonTextColor);
			Singleton<UIReadyToggle>.Instance.Initialize(show: false, delegate
			{
				Singleton<UIMapMultiplayerController>.Instance.ConfirmSelectedLocation();
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			}, delegate
			{
				Singleton<UIMapMultiplayerController>.Instance.OnReady(ready: false);
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			}, delegate
			{
				Singleton<UIMapMultiplayerController>.Instance.ToggleReadyUpUI(show: false, EReadyUpToggleStates.Quests);
				Singleton<UIReadyToggle>.Instance.Reset();
				Singleton<AdventureMapUIManager>.Instance.ConfirmTravel();
			}, delegate(NetworkPlayer player, bool isReady)
			{
				Singleton<UIMapMultiplayerController>.Instance.UpdateReadyPlayer(player, isReady);
			}, null, Singleton<AdventureMapUIManager>.Instance.CheckTravel, "GUI_SELECT_QUEST", "GUI_CANCEL", "PlaySound_UIMultiQuestSelected", bringToFront: false, delegate
			{
				Singleton<APartyDisplayUI>.Instance.CloseWindows();
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			}, delegate
			{
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			}, validateReadyUpOnPlayerLeft: false, UIReadyToggle.EReadyUpType.Participant, toggleState);
			break;
		default:
		{
			string text = "Invalid state sent to InitReadyToggle";
			Debug.LogError(text);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00016", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, text);
			break;
		}
		}
	}

	private void OnSwitchedToMultiplayer()
	{
		FFSNet.Console.LogInfo("MAP: OnSwitchedToMultiplayer");
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		InitReadyToggle(EReadyUpToggleStates.Quests);
		Singleton<UIMapMultiplayerController>.Instance.ShowMapMultiplayer();
		NewPartyDisplayUI.PartyDisplay.UpdateConnectionStatus();
		if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.City && Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.WorldMap)
		{
			if (InputManager.GamePadInUse)
			{
				Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
			}
			else
			{
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
			}
		}
		PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Combine(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnterRoom));
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningPlayerLeft));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		ControllableRegistry.OnControllableDestroyed = (ControllablesChangedEvent)Delegate.Combine(ControllableRegistry.OnControllableDestroyed, new ControllablesChangedEvent(OnControllableDestroyed));
		ControllableRegistry.OnControllableObjectChanged = (ControllableObjectChangedEvent)Delegate.Combine(ControllableRegistry.OnControllableObjectChanged, new ControllableObjectChangedEvent(OnControllableObjectChanged));
		FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
	}

	private void OnUserEnterRoom(UserToken userToken)
	{
		FFSNet.Console.LogInfo("MAP: OnUserEnterRoom");
		if (ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ && FFSNetwork.IsHost)
		{
			DetermineHostToggleInteractability();
		}
		Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
	}

	private void OnPlayerConnected(BoltConnection connection)
	{
		FFSNet.Console.LogInfo("MAP: OnPlayerConnected");
		if (ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ && FFSNetwork.IsHost)
		{
			DetermineHostToggleInteractability();
		}
		Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
	}

	private void OnPlayerJoined(NetworkPlayer player)
	{
		FFSNet.Console.LogInfo("MAP: OnPlayerJoined");
		if (FFSNetwork.IsHost)
		{
			DetermineHostToggleInteractability();
		}
		Singleton<UIMapMultiplayerController>.Instance.OnPlayerJoined(player);
	}

	private void OnPlayerLeft(NetworkPlayer player)
	{
		FFSNet.Console.LogInfo("MAP: OnPlayerLeft");
		if (FFSNetwork.IsHost && ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ)
		{
			DetermineHostToggleInteractability();
		}
		Singleton<UIMapMultiplayerController>.Instance.OnPlayerLeft(player);
	}

	private void OnJoiningPlayerLeft(BoltConnection connection)
	{
		FFSNet.Console.LogInfo("MAP: OnPlayerJoiningPlayerLeft");
		Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
	}

	private void OnControllableDestroyed(NetworkControllable controllable)
	{
		if (!FFSNetwork.IsStartingUp)
		{
			FFSNet.Console.LogInfo("MAP: OnControllableDestroyed");
			Singleton<UIMapMultiplayerController>.Instance.RemoveControllable(controllable);
		}
	}

	private void OnControllableObjectChanged(NetworkControllable controllable, IControllable oldControllableObject, IControllable newControllableObject)
	{
		if (FFSNetwork.IsStartingUp)
		{
			return;
		}
		FFSNet.Console.LogInfo("MAP: OnControllableObjectChanged");
		if (ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ)
		{
			if (FFSNetwork.IsHost)
			{
				DetermineHostToggleInteractability();
			}
			else if (FFSNetwork.IsClient)
			{
				DetermineClientToggleInteractability();
			}
			Singleton<UIMapMultiplayerController>.Instance.UpdateCharacterController(controllable, controllable.Controller);
		}
	}

	private void OnControllableOwnershipChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		if (FFSNetwork.IsStartingUp)
		{
			return;
		}
		FFSNet.Console.LogInfo("MAP: OnControllableOwnershipChanged");
		if (ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ)
		{
			if (FFSNetwork.IsHost)
			{
				DetermineHostToggleInteractability();
			}
			else if (FFSNetwork.IsClient)
			{
				DetermineClientToggleInteractability();
			}
		}
		Singleton<UIMapMultiplayerController>.Instance.UpdateCharacterController(controllable, newController);
	}

	private void OnSwitchedToSinglePlayer()
	{
		FFSNet.Console.LogInfo("MAP: OnSwitchedToSinglePlayer");
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		if (SaveData.Instance.Global.GameMode != EGameMode.MainMenu)
		{
			Singleton<HelpBox>.Instance?.Hide();
			Singleton<UIMapMultiplayerController>.Instance.HideMultiplayer();
			Singleton<UIReadyToggle>.Instance.Reset();
			Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
			ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
			ControllableRegistry.OnControllableDestroyed = (ControllablesChangedEvent)Delegate.Remove(ControllableRegistry.OnControllableDestroyed, new ControllablesChangedEvent(OnControllableDestroyed));
			ControllableRegistry.OnControllableObjectChanged = (ControllableObjectChangedEvent)Delegate.Remove(ControllableRegistry.OnControllableObjectChanged, new ControllableObjectChangedEvent(OnControllableObjectChanged));
			FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
			NewPartyDisplayUI.PartyDisplay.UpdateConnectionStatus();
			if (AdventureState.MapState.MapParty.CheckCharacters.Count < 2)
			{
				Singleton<UIGuildmasterHUD>.Instance.Hide(EGuildmasterOptionsLock.Enough_Characters);
			}
		}
	}

	public void DetermineHostToggleInteractability()
	{
		FFSNet.Console.LogInfo("Determining host toggle lock");
		if (!FFSNetwork.IsHost)
		{
			return;
		}
		bool flag = (PlayerRegistry.AllPlayers.Count > 1 || Singleton<UIReadyToggle>.Instance.ReadyUpType == UIReadyToggle.EReadyUpType.Player || ActionProcessor.CurrentPhase.In(ActionPhaseType.MapAtLinkedScenario)) && ((PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.ConnectingUsers.Count == 0) || ActionProcessor.CurrentPhase.In(ActionPhaseType.MapAtLinkedScenario)) && (Singleton<UIReadyToggle>.Instance.ReadyUpType != UIReadyToggle.EReadyUpType.Participant || PlayerRegistry.AllPlayers.All((NetworkPlayer x) => x.IsParticipant));
		if (flag && Singleton<UIReadyToggle>.Instance.ToggledOn && (Singleton<UIMapMultiplayerController>.Instance.IsReadyTownRecords || Singleton<UIMapMultiplayerController>.Instance.IsReadyRewards))
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
		}
		else
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(flag);
			if (!flag && Singleton<UIReadyToggle>.Instance.ToggledOn)
			{
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			}
		}
		Singleton<UIMapMultiplayerController>.Instance.RefreshHostOptions();
	}

	public void DeterminePlayerToggleInteractability()
	{
		if (FFSNetwork.IsHost)
		{
			DetermineHostToggleInteractability();
		}
		else
		{
			DetermineClientToggleInteractability();
		}
	}

	private void DetermineClientToggleInteractability()
	{
		if (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant)
		{
			bool interactable = !Singleton<UIMapMultiplayerController>.Instance.IsReadyRewards || !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer);
			FFSNet.Console.LogInfo("Determining client toggle interactability " + interactable + " " + PlayerRegistry.MyPlayer.Username);
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable);
		}
		else if (Singleton<UIReadyToggle>.Instance.ReadyUpType == UIReadyToggle.EReadyUpType.Participant)
		{
			FFSNet.Console.LogInfo("Determining client toggle interactability false ");
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
			if (Singleton<UIReadyToggle>.Instance.ToggledOn)
			{
				Debug.LogGUI("Cancel readied");
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			}
		}
	}

	public void ProxySelectedLocation(GameAction action)
	{
		try
		{
			string locationId = ((LocationToken)action.SupplementaryDataToken).ID;
			FFSNet.Console.LogInfo("MAP: Host Selected Location " + locationId);
			MapLocation mapLocation = m_Scenarios.SingleOrDefault((MapLocation x) => x.Location.ID == locationId);
			if (mapLocation == null)
			{
				mapLocation = m_Villages.SingleOrDefault((MapLocation x) => x.Location.ID == locationId);
			}
			if (mapLocation == null)
			{
				Debug.LogError("Failed to find " + locationId);
			}
			InitializeSelectQuestReadyUp();
			Singleton<UIMapMultiplayerController>.Instance.ProxyHostSelectedLocation(mapLocation);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not process ProxySelectedLocation.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ProxySelectedCityEvent(GameAction action)
	{
		try
		{
			InitReadyToggle(EReadyUpToggleStates.CityEvents);
			Singleton<UIMapMultiplayerController>.Instance.ProxyHostSelectedCityEvent();
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not process ProxySelectedCityEvent.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ProxySelectedTownRecords(GameAction action)
	{
		try
		{
			Singleton<UIMapMultiplayerController>.Instance.ProxyHostSelectedTownRecords();
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not process ProxySelectedTownRecords.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void QueueQuestCompletionsToImport(NetworkPlayer player, QuestCompletionToken token)
	{
		if (m_QuestCompletionsToImport.TryGetValue(player, out var value))
		{
			value.Add(token);
		}
		else
		{
			m_QuestCompletionsToImport.Add(player, new List<QuestCompletionToken> { token });
		}
		if (FFSNetwork.IsHost && m_ShowQuestCompletionsToImportCoroutine == null)
		{
			m_ShowQuestCompletionsToImportCoroutine = ShowQuestCompletionsToImportCoroutine();
			StartCoroutine(m_ShowQuestCompletionsToImportCoroutine);
		}
	}

	public void PlayerFinishedSendingQuestCompletionData(NetworkPlayer player)
	{
		m_PlayersFinishedSendingQuestCompletionData.Add(player);
	}

	private IEnumerator ShowQuestCompletionsToImportCoroutine()
	{
		while (PlayerRegistry.JoiningPlayers.Count > 0 || PlayerRegistry.ConnectingUsers.Count > 0 || PlayerRegistry.OtherClientsAreJoining)
		{
			yield return null;
		}
		ImportingQuestCompletions = true;
		while (PlayerRegistry.AllPlayers.Any((NetworkPlayer x) => x.IsClient && !m_PlayersFinishedSendingQuestCompletionData.Contains(x)))
		{
			yield return null;
		}
		foreach (NetworkPlayer key in m_QuestCompletionsToImport.Keys)
		{
			foreach (QuestCompletionToken item in m_QuestCompletionsToImport[key])
			{
				IProtocolToken supplementaryDataToken = item;
				Synchronizer.SendGameAction(GameActionType.SendLocalSaveFileQuestCompletion, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, key.PlayerID, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
		}
		Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.ShowQuestCompletionImport);
	}

	public void ShowQuestCompletionsToImport()
	{
		try
		{
			ImportingQuestCompletions = true;
			foreach (NetworkPlayer networkPlayer in m_QuestCompletionsToImport.Keys)
			{
				actionProgression.RequestPauseActions(networkPlayer);
				Singleton<MultiplayerImportProgressManager>.Instance.Import(networkPlayer, m_QuestCompletionsToImport[networkPlayer].ToList()).Done(delegate
				{
					actionProgression.RequestResumeActions(networkPlayer);
					SaveData.Instance.SaveCurrentAdventureData();
					ImportingQuestCompletions = false;
					m_ShowQuestCompletionsToImportCoroutine = null;
					if (FFSNetwork.IsHost)
					{
						Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
					}
				});
			}
			m_QuestCompletionsToImport.Clear();
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not process ShowQuestCompletionsToImport.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void ProxyRetireCharacter(GameAction action, ref bool actionValid)
	{
		string characterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(action.ActorID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID));
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		FFSNet.Console.LogInfo("MAP: RetireCharacter " + characterID);
		CMapCharacter character = AdventureState.MapState.MapParty.SelectedCharacters.First((CMapCharacter it) => it.CharacterID == characterID);
		if (character == null)
		{
			actionValid = false;
			throw new Exception("Error character to retire is not selected (" + characterID + ").");
		}
		bool haveCharacterToRetire = AdventureState.MapState.MapParty.SelectedCharacters.Any((CMapCharacter it) => it.IsUnderMyControl && it.PersonalQuest.IsFinished);
		if (m_ResultsProcessFinished)
		{
			SceneController.Instance.RetiringCharacter = true;
			QueueConfirmRetirement(character, () => Singleton<UIMapMultiplayerController>.Instance.ConfirmRetirement(character, player, m_ResultsProcessFinished && !haveCharacterToRetire && Singleton<UIGuildmasterHUD>.Instance.AreOptionsAvailable).Then(delegate
			{
				if (!AdventureState.MapState.MapParty.ExistsCharacterToRetire())
				{
					SaveData.Instance.SaveCurrentAdventureData();
					SceneController.Instance.RetiringCharacter = false;
					AdventureState.MapState.CheckNonTrophyAchievements();
					if (FFSNetwork.IsHost)
					{
						Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
					}
				}
			}));
		}
		else
		{
			QueueConfirmRetirement(character, () => Singleton<UIMapMultiplayerController>.Instance.ConfirmRetirement(character, player, m_ResultsProcessFinished && !haveCharacterToRetire && Singleton<UIGuildmasterHUD>.Instance.AreOptionsAvailable));
		}
		actionValid = true;
	}

	public void QueueConfirmRetirement(CMapCharacter character, Func<ICallbackPromise> retireProcess)
	{
		AddQueuedRetirement(character);
		actionProgression.AddAction("RetireCharacter", () => retireProcess().Then(delegate
		{
			RemoveQueuedRetirement(character);
		}), 10);
	}

	public void RemoveQueuedRetirement(CMapCharacter character)
	{
		QueuedRetirements.Remove(character);
		Debug.LogGUI("RemoveQueuedRetirement " + character.CharacterID + " " + QueuedRetirements.Count);
		if (QueuedRetirements.Count == 0)
		{
			Singleton<UIGuildmasterHUD>.Instance.EnableCityEncounter(this, enable: true);
		}
		Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
	}

	public void AddQueuedRetirement(CMapCharacter character)
	{
		QueuedRetirements.Add(character);
		Debug.LogGUI("AddQueuedRetirement " + character.CharacterID + " " + QueuedRetirements.Count);
		Singleton<UIGuildmasterHUD>.Instance.DisableCityEncounter(this, LocalizationManager.GetTranslation("GUI_CITY_EVENT_BLOCKED"));
		Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
	}

	public void ProxyProgressPersonalQuest(GameAction action)
	{
		string characterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(action.ActorID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID));
		FFSNet.Console.LogInfo("MAP: Progress Personal Quest " + characterID);
		CMapCharacter character = AdventureState.MapState.MapParty.SelectedCharacters.First((CMapCharacter it) => it.CharacterID == characterID);
		if (character == null)
		{
			throw new Exception("Error character to progress is not selected (" + characterID + ").");
		}
		PersonalQuestDTO personalQuest = new PersonalQuestDTO(character.PersonalQuest);
		character.PersonalQuest.NextPersonalQuestStep();
		actionProgression.AddAction("ProxyProgressPersonalQuest", () => Singleton<UIPersonalQuestResultManager>.Instance.CreateOtherPlayerProgressedPersonalQuest(character, personalQuest), 10);
	}

	private void InitializeSelectQuestReadyUp()
	{
		if (FFSNetwork.IsHost)
		{
			Singleton<UIReadyToggle>.Instance.Initialize(show: false, delegate
			{
				Singleton<UIMapMultiplayerController>.Instance.ConfirmSelectedLocation();
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			}, delegate
			{
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
				Singleton<UIMapMultiplayerController>.Instance.OnReady(ready: false);
			}, delegate
			{
				Singleton<UIReadyToggle>.Instance.Reset();
				Singleton<UIMapMultiplayerController>.Instance.ToggleReadyUpUI(show: false, EReadyUpToggleStates.Quests);
				Singleton<AdventureMapUIManager>.Instance.ConfirmTravel();
			}, delegate(NetworkPlayer player, bool isReady)
			{
				Singleton<UIMapMultiplayerController>.Instance.UpdateReadyPlayer(player, isReady);
			}, null, Singleton<AdventureMapUIManager>.Instance.CheckTravel, "GUI_SELECT_QUEST", "GUI_CANCEL", "PlaySound_UIMultiQuestSelected", bringToFront: false, delegate
			{
				Singleton<APartyDisplayUI>.Instance.CloseWindows();
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			}, delegate
			{
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			}, validateReadyUpOnPlayerLeft: false, UIReadyToggle.EReadyUpType.Participant, EReadyUpToggleStates.Quests);
			DetermineHostToggleInteractability();
			return;
		}
		Singleton<UIReadyToggle>.Instance.Initialize(show: false, delegate
		{
			Singleton<UIMapMultiplayerController>.Instance.OnReady(ready: true);
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
		}, delegate
		{
			Singleton<UIMapMultiplayerController>.Instance.OnReady(ready: false);
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			DetermineClientToggleInteractability();
		}, delegate
		{
			Singleton<UIReadyToggle>.Instance.Reset();
			Singleton<UIMapMultiplayerController>.Instance.ToggleReadyUpUI(show: false, EReadyUpToggleStates.Quests);
			Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(Singleton<UIMapMultiplayerController>.Instance, enableOptions: false);
		}, delegate(NetworkPlayer player, bool isReady)
		{
			Singleton<UIMapMultiplayerController>.Instance.UpdateReadyPlayer(player, isReady);
		}, null, null, "GUI_ACCEPT_QUEST", "GUI_CANCEL", "PlaySound_UIMultiPlayerReady", bringToFront: false, delegate
		{
			Singleton<APartyDisplayUI>.Instance.CloseWindows();
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
		}, delegate
		{
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
		}, validateReadyUpOnPlayerLeft: false, UIReadyToggle.EReadyUpType.Participant, EReadyUpToggleStates.Quests);
		DetermineClientToggleInteractability();
	}

	private void FocusOnLocation(MapLocation location)
	{
		foreach (MapLocation village in m_Villages)
		{
			village.SetFocused(location == village);
		}
		foreach (MapLocation scenario in m_Scenarios)
		{
			scenario.SetFocused(location == scenario || (IsChoosingLinkedQuestOption() && AdventureState.MapState.JustCompletedLocationState == scenario.Location));
		}
		MapConfig mapConfig = ((Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City) ? cityMapConfig : worldMapConfig);
		CameraController.s_CameraController.SetOverriddenBehavior(new MapFocusCameraBehavior(mapConfig.FocusLocationZoom, location.CenterPosition, mapConfig.FocusLocationTime, delegate
		{
			CameraController.s_CameraController.ClearOverriddenBehavior();
			CameraController.s_CameraController.DisableCameraInput(disableInput: true);
		}));
	}

	private void ResetFocusOnLocation()
	{
		foreach (MapLocation village in m_Villages)
		{
			village.SetFocused(focused: true);
		}
		foreach (MapLocation scenario in m_Scenarios)
		{
			scenario.SetFocused(focused: true);
		}
		CameraController.s_CameraController.ClearOverriddenBehavior("MAP_FOCUS");
		CameraController.s_CameraController.DisableCameraInput(disableInput: false);
	}

	public void SelectLocation(MapLocation location)
	{
		if (IsVisibleInMap(location))
		{
			location.Select();
		}
		else if (IsChoosingLinkedQuestOption())
		{
			RequirementCheckResult requirementCheckResult = location.LocationQuest.CheckRequirements();
			if (!requirementCheckResult.IsValidLinkedQuestChoice())
			{
				Singleton<AdventureMapUIManager>.Instance.ShowWarning(requirementCheckResult, autoHide: true);
			}
		}
		else if (location.LocationQuest.Quest.Type == EQuestType.City)
		{
			OpenCityMap(transition: true, delegate
			{
				location.Select();
			});
		}
		else
		{
			OpenWorldMap(transition: true, delegate
			{
				location.Select();
			});
		}
	}

	private void SetMapConfig(MapConfig mapConfig)
	{
		CameraController.s_CameraController.m_CameraMoveSpeed = mapConfig.CameraMoveSpeed;
		CameraController.s_CameraController.m_ZoomOutExtraHeight = mapConfig.ZoomOutExtraHeight;
		CameraController.s_CameraController.m_DefaultFOV = mapConfig.MaxFOV;
		CameraController.s_CameraController.m_MinimumFOV = mapConfig.MinimumFOV;
		CameraController.s_CameraController.SetFocalBoundsForScenario(mapConfig.CameraBounds);
		if (m_CurrentLocation != null)
		{
			m_PartyToken.PartyInstantMove(m_CurrentLocation.CenterPosition);
		}
		CameraController.s_CameraController.m_TargetFocalPoint = m_PartyToken.transform.position;
		CameraController.s_CameraController.m_ZoomWheelScalar = mapConfig.ZoomWheelSpeed;
		CameraController.s_CameraController.ResetZoomTo(mapConfig.DefaultFOV);
		CameraController.s_CameraController.ResetPositionToFocusOnTargetPoint();
		Singleton<MapMarkersManager>.Instance.SetMaxZoomLocationsVisible(mapConfig.VisibleMapMarkersFOV);
	}

	public void OpenWorldMap(bool transition = true, Action onSwitched = null)
	{
		CameraController.s_CameraController.SetMapInputSource(value: true);
		if (AdventureState.MapState.IsCampaign && transition && mapTransition != null)
		{
			mapTransition.TransitionTo(cityMapConfig.ZoomAmountExit, worldMapConfig.ZoomAmountEnter, delegate
			{
				OpenWorldMap(transition: false);
				CameraController.s_CameraController.ResetZoomTo(worldMapConfig.DefaultFOV - worldMapConfig.ZoomAmountEnter);
			}, onSwitched);
			return;
		}
		SetMapConfig(worldMapConfig);
		worldMap?.SetActive(value: true);
		if (cityMap != null)
		{
			cityMap?.SetActive(value: false);
		}
		Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
		RefreshShownLocationsByMap();
		onSwitched?.Invoke();
	}

	public void OpenCityMap(bool transition = true, Action onSwitched = null)
	{
		if (!AdventureState.MapState.IsCampaign)
		{
			return;
		}
		CameraController.s_CameraController.SetMapInputSource(value: true);
		if (transition && mapTransition != null)
		{
			mapTransition.TransitionTo(worldMapConfig.ZoomAmountExit, cityMapConfig.ZoomAmountEnter, delegate
			{
				OpenCityMap(transition: false);
				CameraController.s_CameraController.ResetZoomTo(cityMapConfig.DefaultFOV - cityMapConfig.ZoomAmountEnter);
			}, onSwitched);
			return;
		}
		SetMapConfig(cityMapConfig);
		worldMap.SetActive(value: false);
		cityMap.SetActive(value: true);
		Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.City);
		RefreshShownLocationsByMap();
		onSwitched?.Invoke();
	}

	private void RefreshShownLocationsByMap()
	{
		if (!AdventureState.MapState.IsCampaign || (Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.City && Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.WorldMap))
		{
			return;
		}
		foreach (MapLocation item in m_Scenarios.Where((MapLocation it) => it.LocationQuest != null))
		{
			if (IsVisibleInMap(item))
			{
				item.ShowLocation(this);
			}
			else
			{
				item.HideLocation(this);
			}
		}
		if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City)
		{
			foreach (MapLocation cityLocation in m_CityLocations)
			{
				if (cityLocation.MapLocationType != MapLocation.EMapLocationType.Store)
				{
					cityLocation.ShowLocation(this);
					continue;
				}
				switch ((cityLocation.Location.Location as CStoreLocation).StoreType)
				{
				case EHQStores.Enhancer:
					if (AdventureState.MapState.HeadquartersState.EnhancerUnlocked)
					{
						cityLocation.ShowLocation(this);
					}
					else
					{
						cityLocation.HideLocation(this);
					}
					break;
				case EHQStores.Merchant:
					if (AdventureState.MapState.HeadquartersState.MerchantUnlocked)
					{
						cityLocation.ShowLocation(this);
					}
					else
					{
						cityLocation.HideLocation(this);
					}
					break;
				case EHQStores.Temple:
					if (AdventureState.MapState.HeadquartersState.TempleUnlocked)
					{
						cityLocation.ShowLocation(this);
					}
					else
					{
						cityLocation.HideLocation(this);
					}
					break;
				case EHQStores.Trainer:
					if (AdventureState.MapState.HeadquartersState.TrainerUnlocked)
					{
						cityLocation.ShowLocation(this);
					}
					else
					{
						cityLocation.HideLocation(this);
					}
					break;
				}
			}
			foreach (UILocationMapMarker street in m_Streets)
			{
				Singleton<MapMarkersManager>.Instance.RefreshVisibilityLocationMarker(street);
			}
			foreach (UILocationMapMarker worldMapLabel in m_WorldMapLabels)
			{
				worldMapLabel.Hide();
			}
			HeadquartersLocation.HideLocation(this);
			return;
		}
		foreach (MapLocation cityLocation2 in m_CityLocations)
		{
			cityLocation2.HideLocation(this);
		}
		foreach (UILocationMapMarker street2 in m_Streets)
		{
			street2.Hide();
		}
		foreach (UILocationMapMarker worldMapLabel2 in m_WorldMapLabels)
		{
			Singleton<MapMarkersManager>.Instance.RefreshVisibilityLocationMarker(worldMapLabel2);
		}
		HeadquartersLocation.ShowLocation(this);
	}

	public bool IsVisibleInMap(MapLocation mapLocation)
	{
		if (!AdventureState.MapState.IsCampaign)
		{
			return true;
		}
		if (mapLocation.MapLocationType == MapLocation.EMapLocationType.Headquarters)
		{
			return true;
		}
		if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City)
		{
			if (mapLocation.LocationQuest == null)
			{
				return m_CityLocations.Contains(mapLocation);
			}
			return mapLocation.LocationQuest.Quest.Type == EQuestType.City;
		}
		if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.WorldMap)
		{
			if (mapLocation.LocationQuest == null)
			{
				return !m_CityLocations.Contains(mapLocation);
			}
			return mapLocation.LocationQuest.Quest.Type != EQuestType.City;
		}
		return true;
	}

	public bool IsChoosingLinkedQuestOption()
	{
		return m_LinkedQuestLocation != null;
	}

	public bool IsLinkedQuest(CQuestState quest)
	{
		if (IsChoosingLinkedQuestOption())
		{
			return m_LinkedQuestLocation.LocationQuest == quest;
		}
		return false;
	}

	public bool IsLinkedQuestLocation(MapLocation location)
	{
		if (IsChoosingLinkedQuestOption())
		{
			return m_LinkedQuestLocation == location;
		}
		return false;
	}

	public MapLocation GetMapLocationByQuest(string questID)
	{
		return m_Scenarios.First((MapLocation it) => it.LocationQuest != null && it.LocationQuest.ID == questID);
	}

	public void ShowUnlockedQuests(ICollection<string> questsIds, Action onFinish, float? returnZoom = null)
	{
		if (questsIds.Count == 0)
		{
			onFinish?.Invoke();
			return;
		}
		Singleton<MapMarkersManager>.Instance.ShowMarkers();
		List<MapLocation> locations = questsIds.Select(GetMapLocationByQuest).ToList();
		if (Singleton<UIUnlockLocationFlowManager>.Instance == null)
		{
			onFinish?.Invoke();
		}
		else
		{
			Singleton<UIUnlockLocationFlowManager>.Instance.ShowUnlockedLocations(locations, onFinish, returnZoom);
		}
	}

	private ICallbackPromise ShowUnlockedQuests(float? returnZoom = null)
	{
		CallbackPromise callbackPromise = new CallbackPromise();
		ShowUnlockedQuests(m_RecentUnlockedQuestLocations, callbackPromise.Resolve, returnZoom);
		return callbackPromise.Then(delegate
		{
			m_RecentUnlockedQuestLocations.Clear();
		});
	}

	public void ShowQuestLocations(Component request, bool show)
	{
		foreach (MapLocation item in m_Scenarios.Where((MapLocation it) => it.LocationQuest != null))
		{
			if (show)
			{
				item.ShowLocation(request);
			}
			else
			{
				item.HideLocation(request);
			}
		}
	}

	public void RequestPauseActionProgression(object request)
	{
		actionProgression.RequestPauseActions(request);
	}

	public void RequestResumeActionProgression(object request)
	{
		actionProgression.RequestResumeActions(request);
	}
}
