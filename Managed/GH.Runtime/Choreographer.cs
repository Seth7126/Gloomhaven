#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AStar;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using Chronos;
using EPOOutline;
using FFSNet;
using GLOOM;
using GLOOM.MainMenu;
using JetBrains.Annotations;
using MEC;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Events;
using Photon.Bolt;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using Script.Controller;
using Script.GUI.GameScreen;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.States.ScenarioStates;
using Script.Misc;
using SharedLibrary;
using SharedLibrary.SimpleLog;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WorldspaceUI;

public class Choreographer : MonoBehaviour
{
	[Serializable]
	public enum ChoreographerStateType
	{
		NA,
		WaitingForMoveAnim,
		WaitingForAttackModifierCards,
		WaitingForAttackAnim,
		WaitingForModifierDrawAnim,
		WaitingForDamageAnim,
		WaitingForGeneralAnim,
		WaitingForItemRefresh,
		WaitingForPlayerIdle,
		WaitingForEndAbilityAnimSync,
		WaitingForEndTurnSync,
		WaitingForEndRoundSync,
		WaitingForProgressChoreographer,
		Play,
		WaitingForCardSelection,
		WaitingForPlayerWaypointSelection,
		WaitingForAreaAttackFocusSelection,
		WaitingForPlayerPushWaypointSelection,
		WaitingForPlayerPullWaypointSelection,
		WaitingForRewardsProcess,
		WaitingInLevelEditor,
		WaitingForExhaustionClear,
		WaitingForAutosave,
		WaitingForTileSelected,
		WaitingForElementPicked,
		WaitingForDelayedDrops,
		WaitingForLoseGoalChestRewardSelection
	}

	public class CWaitState
	{
		public ChoreographerStateType m_State;

		public int m_StateWaitTickFrame;

		public GameObject m_StateWaitActorGO;

		public CActor m_StateWaitActor;
	}

	public delegate void CharacterAddedEventHandler(GameObject character);

	public static ChoreographerStateType[] ChoreographerStateTypes = (ChoreographerStateType[])Enum.GetValues(typeof(ChoreographerStateType));

	private ChoreographerStateType previousState;

	public static Choreographer s_Choreographer;

	[HideInInspector]
	public Vector3 DeadPlayerLocation = new Vector3(0f, 0f, 1000f);

	[HideInInspector]
	public List<GameObject> m_ClientPlayers = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> m_ClientDeadActors = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> m_ClientEnemies = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> m_ClientHeroSummons = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> m_ClientAllyMonsters = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> m_ClientEnemy2Monsters = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> m_ClientNeutralMonsters = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> m_ClientObjects = new List<GameObject>();

	public Dictionary<string, GameObject> m_ClientPlacementPreviews = new Dictionary<string, GameObject>();

	[HideInInspector]
	public List<GameObject> AllActorsInScene = new List<GameObject>();

	[HideInInspector]
	public Thread m_MainThread;

	public TextMeshProUGUI Version;

	public GameObject VersionGO;

	public Text m_Phase;

	public Text m_StateText;

	public Text m_Description;

	public GameObject m_ActorPrefab;

	public CAbilityCard m_InitialActionAbilityCard;

	public ReadyButton readyButton;

	public GameObject m_ConfirmDoorTileSelect;

	[HideInInspector]
	public ActorEvents m_ActorEvents;

	[HideInInspector]
	public CClientTile m_lastSelectedTile;

	[HideInInspector]
	public List<CClientTile> m_LastSelectedTiles;

	[HideInInspector]
	public bool m_SMB_Control_WaitingForAttackAnim;

	[HideInInspector]
	public bool m_SMB_Control_WaitingForActorBeenAttackedAnim;

	[HideInInspector]
	public StateMachineBehaviour m_SMB_Control_ControlledByStateBehaviour;

	[HideInInspector]
	public bool m_TileSelectionDisabled;

	[HideInInspector]
	public CClientTile m_BufferedSelectedTile;

	[HideInInspector]
	public bool m_BufferedTileNetworked;

	[HideInInspector]
	public bool m_BufferedSecondClickToConfirm;

	[HideInInspector]
	public bool m_Wait;

	public UndoButton m_UndoButton;

	public SkipButton m_SkipButton;

	public SelectButton m_selectButton;

	public CanvasGroup m_BottomButtonsCanvasGroup;

	public RoomCameraButton m_RoomCameraButton;

	public Canvas m_WorldSpaceCanvas;

	public Canvas m_OverlayCanvas;

	public GameObject m_GameScenarioScreen;

	public CanvasGroup m_GameGUILevel;

	[HideInInspector]
	public List<CMessageData> m_MessageQueue = new List<CMessageData>();

	[HideInInspector]
	public CActor m_CurrentActor;

	[HideInInspector]
	public CActor m_CurrentOnDeathActor;

	[HideInInspector]
	public CAbility m_CurrentAbility;

	[HideInInspector]
	public CWaitState m_WaitState;

	[HideInInspector]
	public CTile m_ConfirmationDoorTile;

	[SerializeField]
	private FastForwardButton _fastForwardButton;

	private List<GameObject> m_Stars = new List<GameObject>();

	private List<AttackModifierYMLData> m_CurrentActorAttackModifierCards = new List<AttackModifierYMLData>();

	private CActor m_CurrentActorBeingAttacked;

	private CActor m_CurrentActorBeingHealed;

	private List<CTile> m_ConfirmationDoorTileOptionalTileList;

	private bool m_CheckAutotestComplete;

	private bool m_ShouldShowRewards;

	private bool m_QueuedMPStateCompare;

	private bool m_EndOfTurnSyncComplete;

	private bool m_LoggedEndTurnSyncSoftlock;

	private float m_TimeOfLoggedEndOfTurnSoftLock;

	private bool m_EndOfRoundSyncComplete;

	private bool m_LoggedEndRoundSyncSoftlock;

	private float m_TimeOfLoggedEndOfRoundSoftLock;

	private bool m_EndAbilitySyncComplete;

	private bool m_BlockClientMessageProcessing;

	private int _updateBlockers;

	private CActor.EType CurrentPlayerType;

	public const string c_IdleRunStateName = "Idle-Run";

	public const string c_AttackStateName = "Attack";

	public const string c_DamageStateName = "Damage";

	public const string c_HealStateName = "PowerUp";

	public const string c_HitStateName = "Hit";

	public const string c_DeathStateName = "Death";

	public const string c_PowerUpStateName = "PowerUp";

	public const string c_PushPullStateName = "PushPull";

	public const string c_PullStateName = "Pull";

	public const string c_PushStateName = "Push";

	public const string c_UseItemStateName = "UseItem";

	public const string c_SummonedStateName = "Summoned";

	public const string c_TeleportAwayStateName = "TeleportAway";

	public const string c_TeleportBackStateName = "TeleportBack";

	public const string c_LootStateName = "Loot";

	public const string c_SleepIdleStateName = "SleepIdle";

	public const string c_AwakenedStateName = "SleepWakeUp";

	public const string c_SleepHitStateName = "SleepHit";

	public const string c_SleepDeathStateName = "SleepDeath";

	public const string c_IdleAllyCheerName = "CheerAllyIdle";

	public const string c_IdleEnemyCheerName = "CheerEnemyIdle";

	public const long c_MaxMessageUpdateTimeSlice = 8L;

	private bool m_debugMessage;

	private List<GameObject> m_CurrentAttackTargets = new List<GameObject>();

	private List<CClientTile> m_CurrentAttackArea = new List<CClientTile>();

	private List<CActor> m_ActorsBeingTargetedForVFX;

	private List<GameObject> m_ActorObjectsDiedInCurrentRound = new List<GameObject>();

	private List<string> m_AbilitiesUsedThisTurn = new List<string>();

	private List<Tuple<string, CActor.ECauseOfDeath>> m_ActorsKilledThisAbility = new List<Tuple<string, CActor.ECauseOfDeath>>();

	private List<(string actor, string element)> m_ElementsInfusedThisAbility = new List<(string, string)>();

	private CMessageData.MessageType m_PreviousMessageType;

	private bool m_PlayerSelectingToAvoidDamageOrNot;

	private List<Reward> m_RewardsToShowcase = new List<Reward>();

	private List<CMessageData> m_AfterTakeDamageProgressMessages = new List<CMessageData>();

	private Coroutine CheckIfAllControlledPlayersAreDeadCoroutine;

	private List<NetworkPlayer> m_MPEndOfRoundComparisonFinishedList = new List<NetworkPlayer>();

	private List<NetworkPlayer> m_WaitForEndOfAbilityList = new List<NetworkPlayer>();

	private List<NetworkPlayer> m_WaitForEndOfTurnList = new List<NetworkPlayer>();

	private List<NetworkPlayer> m_WaitForEndOfRoundList = new List<NetworkPlayer>();

	private bool m_PlayerToSelectAbilityCardsOrLongRest;

	private CActor m_ActorOpeningDoor;

	private bool m_UsingThreadedEndOfTurnSaves = true;

	public List<ScenarioAchievementProgress> m_InitialProgressAchievements;

	public bool HostReadyToSendCompareState;

	public Action onCharacterAbilityComplete;

	public bool ShowAugmentationBar;

	public bool WaitingForConfirm;

	public const string FONT_ABILITY = "<b>{0}</b>";

	public const string FONT_ATTACKMODSDOUBLE = "<b><color=#D92727>{0}</color></b>";

	public const string FONT_ATTACKMODSPOSITIVE = "<b><color=#FFA414>{0}</color></b>";

	public const string FONT_ATTACKMODSNEGATIVE = "<b><color=#A050E0>{0}</color></b>";

	public const string FONT_CARDNAMES = "<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{0}</color></b></font>";

	public const string FONT_DAMAGE = "<b><color=#f76767>{0}</color></b>";

	public const string FONT_DAMAGE_CALC = "<b><color=#BBBBBB>{0}</color></b>";

	public const string FONT_LOOT = "<color=#EACF8C>{0}</color>";

	public const string FONT_TURNSANDROUNDS = "<b><font=\"MarcellusSC-Regular SDF\">{0}</font></b>";

	public static List<CAbility.EAbilityType> TargetingAbilityTypesToSkip = new List<CAbility.EAbilityType>
	{
		CAbility.EAbilityType.AddActiveBonus,
		CAbility.EAbilityType.AddCondition,
		CAbility.EAbilityType.AddDoom,
		CAbility.EAbilityType.AddDoomSlots,
		CAbility.EAbilityType.ChangeCharacterModel,
		CAbility.EAbilityType.ControlActor,
		CAbility.EAbilityType.DiscardCards,
		CAbility.EAbilityType.ImmunityTo,
		CAbility.EAbilityType.IncreaseCardLimit,
		CAbility.EAbilityType.LoseCards,
		CAbility.EAbilityType.LoseGoalChestReward,
		CAbility.EAbilityType.NullTargeting,
		CAbility.EAbilityType.RemoveActorFromMap,
		CAbility.EAbilityType.RemoveConditions
	};

	public static List<CAbility.EAbilityType> TargetingAbilitySpritesToSkip = new List<CAbility.EAbilityType>
	{
		CAbility.EAbilityType.AddAugment,
		CAbility.EAbilityType.AddSong,
		CAbility.EAbilityType.Advantage,
		CAbility.EAbilityType.ConsumeItemCards,
		CAbility.EAbilityType.ExtraTurn,
		CAbility.EAbilityType.ForgoActionsForCompanion,
		CAbility.EAbilityType.GiveSupplyCard,
		CAbility.EAbilityType.ImmunityTo,
		CAbility.EAbilityType.ImprovedShortRest,
		CAbility.EAbilityType.LoseGoalChestReward,
		CAbility.EAbilityType.OverrideAugmentAttackType,
		CAbility.EAbilityType.PreventDamage,
		CAbility.EAbilityType.RecoverDiscardedCards,
		CAbility.EAbilityType.RecoverLostCards,
		CAbility.EAbilityType.RefreshItemCards,
		CAbility.EAbilityType.RemoveActorFromMap,
		CAbility.EAbilityType.ShuffleModifierDeck,
		CAbility.EAbilityType.TransferDooms
	};

	[HideInInspector]
	public int m_ProcGenSeed;

	[HideInInspector]
	public bool m_FirstLoad;

	[HideInInspector]
	public ScenarioRuleLibrary.ScenarioState m_CurrentState;

	[HideInInspector]
	public List<GameObject> m_AllMaps = new List<GameObject>();

	[HideInInspector]
	public GameObject m_MapSceneRoot;

	[HideInInspector]
	public Scene m_ProcGenScene;

	public const string c_SeedPrefix = "S:";

	public const string c_ProcGenSceneName = "ProcGen";

	private const float c_MinimumRangeOfDoor = 2f;

	private const float c_MinimumRangeOfEntrace = 3f;

	private const float c_MinimumRangeOfEnemyToEntrace = 3f;

	private const float c_MaximumRangeOfDungeonDoor = 5f;

	private GameObject m_PropSceneRoot;

	private int m_MapID;

	private string m_ApparanceStyleOutput;

	private string m_MapAlignmentOutput;

	private SharedLibrary.Random m_Random;

	private List<GameObject> m_DungeonEntranceDoors;

	private List<GameObject> m_DungeonExitDoors;

	private bool m_IsFirstLoad;

	public bool IsRestarting { get; set; }

	public bool IsPlayerTurn => CurrentPlayerType == CActor.EType.Player;

	public List<GameObject> ClientActorObjects => m_ClientPlayers.Concat(m_ClientEnemies).Concat(m_ClientHeroSummons).Concat(m_ClientAllyMonsters)
		.Concat(m_ClientEnemy2Monsters)
		.Concat(m_ClientNeutralMonsters)
		.Concat(m_ClientObjects)
		.ToList();

	public List<GameObject> ClientMonsterObjects => m_ClientEnemies.Concat(m_ClientAllyMonsters).Concat(m_ClientEnemy2Monsters).Concat(m_ClientNeutralMonsters)
		.Concat(m_ClientObjects)
		.ToList();

	public CPlayerActor CurrentPlayerActor
	{
		get
		{
			if (m_CurrentActor is CHeroSummonActor cHeroSummonActor)
			{
				return cHeroSummonActor.Summoner;
			}
			if (m_CurrentActor is CPlayerActor result)
			{
				return result;
			}
			return null;
		}
	}

	public CActor CurrentActor => m_CurrentActor;

	public bool AutoTestIgnoreTileClick { get; set; }

	public bool IsFirstTurnPlaying { get; private set; }

	private List<ActorEvents> m_AllActorEvents
	{
		get
		{
			List<ActorEvents> list = new List<ActorEvents>();
			foreach (GameObject clientActorObject in ClientActorObjects)
			{
				ActorEvents componentInChildren = clientActorObject.GetComponentInChildren<ActorEvents>();
				if (componentInChildren != null)
				{
					list.Add(componentInChildren);
				}
			}
			return list;
		}
	}

	private bool UpdateIsBlocked => _updateBlockers > 0;

	public List<string> IdleStates => new List<string> { "Idle-Run", "SleepIdle", "CheerAllyIdle", "CheerEnemyIdle" };

	public bool PlayersInValidStartingPositions
	{
		get
		{
			if (m_CurrentState.RoundNumber > 1 || PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				return true;
			}
			int num = Mathf.RoundToInt((float)m_ClientPlayers.Count / (float)ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles.Count);
			foreach (List<CClientTile> possibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles)
			{
				int num2 = 0;
				foreach (CClientTile item in possibleStartingTile)
				{
					if (ScenarioManager.Scenario.FindPlayerAt(item.m_Tile.m_ArrayIndex) != null)
					{
						num2++;
					}
				}
				if (num2 > num)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool AnyPlayersInInvalidStartingPositionsForCompanionSummons
	{
		get
		{
			if (m_CurrentState != null && m_CurrentState.RoundNumber < 2 && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				return PlayersInInvalidStartingPositionsForCompanionSummons().Count > 0;
			}
			return false;
		}
	}

	public bool ThisPlayerHasTurnControl
	{
		get
		{
			bool flag = false;
			if (m_CurrentActor != null && m_CurrentActor.OriginalType == CActor.EType.Enemy && m_CurrentActor.MindControlDuration != CAbilityControlActor.EControlDurationType.None && FFSNetwork.IsOnline)
			{
				flag = GameState.IsParentUnderMyControl(m_CurrentActor);
			}
			if (!(!FFSNetwork.IsOnline || m_CurrentActor == null || m_CurrentActor.IsUnderMyControl || flag))
			{
				if (m_CurrentActor is CHeroSummonActor cHeroSummonActor)
				{
					if (cHeroSummonActor.MindControlDuration == CAbilityControlActor.EControlDurationType.None)
					{
						return cHeroSummonActor.Summoner.IsUnderMyControl;
					}
					return false;
				}
				return false;
			}
			return true;
		}
	}

	public bool HaltMultiplayerProgression
	{
		get
		{
			if (!FFSNetwork.IsOnline)
			{
				return false;
			}
			if (!FFSNetwork.IsHost)
			{
				return PlayerRegistry.OtherClientsAreJoining;
			}
			if (PlayerRegistry.AllPlayers.Count != 1 && PlayerRegistry.JoiningPlayers.Count <= 0)
			{
				return PlayerRegistry.ConnectingUsers.Count > 0;
			}
			return true;
		}
	}

	public CMessageData LastMessage { get; set; }

	public List<GameObject> CurrentAttackTargets
	{
		get
		{
			return m_CurrentAttackTargets;
		}
		private set
		{
			m_CurrentAttackTargets = value;
		}
	}

	public List<CClientTile> CurrentAttackArea
	{
		get
		{
			return m_CurrentAttackArea;
		}
		private set
		{
			m_CurrentAttackArea = value;
		}
	}

	public List<CActor> ActorsBeingTargetedForVFX => m_ActorsBeingTargetedForVFX;

	public bool FirstAbility { get; private set; }

	public bool LastAbility { get; private set; }

	public List<CObjectProp> DoorsUnlocking { get; set; }

	public bool PlayerSelectingToAvoidDamageOrNot => m_PlayerSelectingToAvoidDamageOrNot;

	public bool PlayerToSelectAbilityCardsOrLongRest => m_PlayerToSelectAbilityCardsOrLongRest;

	public bool IsShowedHelpBox { get; private set; }

	public bool ForceEnterToCardSelection { get; set; }

	private bool m_FollowCameraDisabled => !CameraController.s_CameraController.CameraFollowOn;

	public event CharacterAddedEventHandler CharacterAdded;

	public event Action<bool> OnAoeTileSelected;

	private void OnCharacterAdded(GameObject character)
	{
		SetAnimSpeed(character);
		this.CharacterAdded?.Invoke(character);
	}

	private void Awake()
	{
		PlatformLayer.Boost?.EnableGpuBoost();
		s_Choreographer = this;
		m_WaitState = null;
		m_SMB_Control_WaitingForAttackAnim = false;
		m_SMB_Control_WaitingForActorBeenAttackedAnim = false;
		VFXShared.RegisteredAuraHexEffects.Clear();
		VFXShared.RegisteredAuraEffectObjects.Clear();
		AutoTestIgnoreTileClick = false;
		ScenarioRuleClient.SetMessageHandler(MessageHandler);
		TileBehaviour.SetCallback(TileHandler);
		SaveData.Instance.Global.GameSpeedChanged += OnGameSpeedChanged;
		IsFirstTurnPlaying = true;
		m_MainThread = Thread.CurrentThread;
		SetChoreographerState(ChoreographerStateType.Play, 0, null);
		if (MF.NeedToShowVersion())
		{
			VersionGO.SetActive(value: true);
			MF.SetVersion(Version);
		}
		else
		{
			VersionGO.SetActive(value: false);
		}
		PlayerRegistry.LoadingInFromJoiningClient = false;
		SwitchSkipAndUndoButtons(SaveData.Instance.Global.SwitchSkipAndUndoButtons);
		SaveData.Instance.Global.SwitchSkipAndUndoButtonsChanged += SwitchSkipAndUndoButtons;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			PlatformLayer.Boost?.DisableGpuBoost();
			Unsubscribe();
			SaveData.Instance.Global.GameSpeedChanged -= OnGameSpeedChanged;
			SaveData.Instance.Global.SwitchSkipAndUndoButtonsChanged -= SwitchSkipAndUndoButtons;
			SceneManager.sceneLoaded -= OnSceneLoadedCallback;
			ObjectPool.RecycleAndDestroyObjects();
			if (ApparanceEngine.Instance != null)
			{
				ApparanceEngine.Instance.RefreshResources();
			}
			FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
			FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
			PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
			PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Remove(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnter));
			PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
			PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
			PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserLeft));
			ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
			if (!CoreApplication.IsQuitting)
			{
				Singleton<UINotificationManager>.Instance.ClearNotifications();
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
				Singleton<MultiplayerImportProgressManager>.Instance?.Cancel();
			}
			s_Choreographer = null;
			TileBehaviour.SetCallback(null);
			ScenarioRuleClient.SetMessageHandler(null);
		}
	}

	private void Unsubscribe()
	{
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Remove(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnter));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserLeft));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
	}

	public void AddUpdateBlocker()
	{
		_updateBlockers++;
	}

	public void RemoveUpdateBlocker()
	{
		_updateBlockers--;
	}

	private string AllMonsterAbilities()
	{
		string text = "";
		List<CMonsterClass> list = new List<CMonsterClass>();
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			CMonsterClass item = (CMonsterClass)allAliveMonster.Class;
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		foreach (CMonsterClass item2 in list)
		{
			text = text + item2.ID + ": " + PrintCardPile(item2.AbilityCards) + "\n\n";
		}
		return text;
	}

	private string PrintCardPile(List<CAbilityCard> cardPile)
	{
		string text = "";
		foreach (CAbilityCard item in cardPile)
		{
			text = text + item.Name + " (" + item.CardInstanceID + "), ";
		}
		return text;
	}

	private string PrintCardPile(List<CMonsterAbilityCard> cardPile)
	{
		string text = "";
		foreach (CMonsterAbilityCard item in cardPile)
		{
			text = text + item.Name + " (" + item.ID + "), ";
		}
		return text;
	}

	private string PrintCardPile(CCardDeck cardPile)
	{
		string text = "";
		foreach (string card in cardPile.Cards)
		{
			text = text + card + ", ";
		}
		return text;
	}

	private string PrintCardPile(List<AttackModifierYMLData> cardPile)
	{
		string text = "";
		foreach (string item in cardPile.Select((AttackModifierYMLData c) => c.Name))
		{
			text = text + item + ", ";
		}
		return text;
	}

	private void ClearStars()
	{
		WorldspaceStarHexDisplay.Instance.ClearStars();
	}

	public GameObject CreateCharacterActor(CClientTile tile, CActor actor, bool isSummoned = false, string skinId = null, bool isExhaustedCharacter = false)
	{
		try
		{
			GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(actor.Type, actor.GetPrefabName(), skinId);
			if (characterPrefabFromBundle == null)
			{
				characterPrefabFromBundle = AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("npc_none", "NULLCharacter", "npcs");
				throw new Exception("Unable to find character with prefab name " + actor.GetPrefabName());
			}
			GameObject gameObject = ObjectPool.Spawn(characterPrefabFromBundle, null, actor.Type != CActor.EType.Enemy);
			CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
			gameObject.name = actor.ActorGuid;
			characterManager.CharacterPrefab = characterPrefabFromBundle;
			characterManager.CharacterActor = actor;
			characterManager.InitialiseCharacter();
			SkinnedMeshRenderer[] componentsInChildren = characterManager.CharacterChildPrefabInstance.Result.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				skinnedMeshRenderer.material = new Material(skinnedMeshRenderer.material);
				if (!(skinnedMeshRenderer.material.shader.name == "Amp_Char_Shader"))
				{
					continue;
				}
				string htmlString = "#FFFFFF";
				float value = 0f;
				float num = 0f;
				if (actor.Type == CActor.EType.Player)
				{
					htmlString = (actor.Class as CCharacterClass).CharacterYML.ColourHTML;
					value = (actor.Class as CCharacterClass).CharacterYML.Fatness;
					num = (actor.Class as CCharacterClass).CharacterYML.VertexAnimIntensity;
				}
				else if (actor.Type == CActor.EType.Enemy || actor.Type == CActor.EType.Neutral || actor.Type == CActor.EType.Enemy2 || actor.Type == CActor.EType.Ally)
				{
					htmlString = (actor.Class as CMonsterClass).MonsterYML.ColourHTML;
					value = (actor.Class as CMonsterClass).MonsterYML.Fatness;
					num = (actor.Class as CMonsterClass).MonsterYML.VertexAnimIntensity;
				}
				else if (actor.Type == CActor.EType.HeroSummon)
				{
					htmlString = (actor.Class as CHeroSummonClass).SummonYML.ColourHTML;
					value = (actor.Class as CHeroSummonClass).SummonYML.Fatness;
					num = (actor.Class as CHeroSummonClass).SummonYML.VertexAnimIntensity;
				}
				if (skinnedMeshRenderer.material.HasProperty("_MOD_TINT") && ColorUtility.TryParseHtmlString(htmlString, out var color))
				{
					skinnedMeshRenderer.material.SetColor("_MOD_TINT", color);
				}
				if (skinnedMeshRenderer.material.HasProperty("_MOD_THICKEN"))
				{
					skinnedMeshRenderer.material.SetFloat("_MOD_THICKEN", value);
				}
				if (!skinnedMeshRenderer.material.HasProperty("_AddVertexAnim"))
				{
					continue;
				}
				if (skinnedMeshRenderer.material.GetFloat("_AddVertexAnim") == 0f)
				{
					if (num != 0f)
					{
						if (skinnedMeshRenderer.material.HasProperty("_NoiseSpeed"))
						{
							skinnedMeshRenderer.material.SetFloat("_NoiseSpeed", 0f);
						}
						if (skinnedMeshRenderer.material.HasProperty("_VertexAnim_Intensity"))
						{
							skinnedMeshRenderer.material.SetFloat("_VertexAnim_Intensity", num);
						}
						skinnedMeshRenderer.material.SetFloat("_AddVertexAnim", 1.1f);
					}
				}
				else if (skinnedMeshRenderer.material.GetFloat("_AddVertexAnim") == 1.1f && num == 0f)
				{
					skinnedMeshRenderer.material.SetFloat("_AddVertexAnim", 0f);
				}
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(m_ActorPrefab);
			Debug.Log("Instantiated actor prefab for actorGuid: " + actor.ActorGuid);
			characterManager.CharacterActorGO = gameObject2;
			Animator animator = gameObject.transform.GetComponentsInChildren<Animator>().FirstOrDefault((Animator x) => x.transform.gameObject.layer == LayerMask.NameToLayer("Hero") || x.transform.gameObject.layer == LayerMask.NameToLayer("Monster"));
			if (animator != null)
			{
				gameObject2.transform.SetParent(animator.transform);
				Debug.Log("Set transform parent for actor prefab for actorGuid: " + actor.ActorGuid + " parent transform instance name: " + animator.transform.name);
				animator.gameObject.AddComponent<ActorEvents>();
				animator.SetInteger("ActorTypeEnum", (int)actor.OriginalType);
			}
			else
			{
				gameObject2.transform.SetParent(gameObject.transform);
				Debug.Log("Set transform parent for actor prefab for actorGuid: " + actor.ActorGuid + " parent transform instance name: " + gameObject.transform.name);
				Debug.LogWarning("Unable to find animator on correct layer for CharacterPrefab:" + actor.GetPrefabName());
			}
			gameObject2.transform.localPosition = Vector3.zero;
			UnityGameEditorObject component = gameObject.GetComponent<UnityGameEditorObject>();
			if (tile != null)
			{
				gameObject.transform.SetParent(ClientScenarioManager.s_ClientScenarioManager.m_Board.transform);
				gameObject.transform.position = tile.m_GameObject.transform.position;
				if (actor.Type == CActor.EType.Enemy)
				{
					Vector3 enemyFocalPoint = GetEnemyFocalPoint(gameObject);
					gameObject.transform.LookAt(enemyFocalPoint);
				}
				else
				{
					GetAveragePositions(out var _, out var averageEnemyPosition, out var _, out var _, out var _);
					gameObject.transform.LookAt(averageEnemyPosition);
				}
				component.m_ShouldSnapToHexSpacing = true;
			}
			else
			{
				component.m_ShouldSnapToHexSpacing = false;
			}
			if (!isSummoned && actor.Type == CActor.EType.Player && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
			{
				m_GameScenarioScreen.SetActive(value: true);
				CardsHandManager.Instance.AddPlayer((CPlayerActor)actor);
				m_GameScenarioScreen.SetActive(value: false);
			}
			if (!isExhaustedCharacter)
			{
				ActorBehaviour.SetActor(gameObject, actor);
				Outlinable outlinable = gameObject.GetComponentInChildren<Outlinable>();
				bool flag = false;
				if (outlinable == null && actor is CObjectActor { AttachedProp: not null } cObjectActor)
				{
					flag = true;
					outlinable = Singleton<ObjectCacheService>.Instance.GetPropObject(cObjectActor.AttachedProp)?.GetComponentInChildren<Outlinable>(includeInactive: true);
				}
				if (outlinable != null)
				{
					outlinable.OutlineParameters.Color = UIInfoTools.GetCharacterOutlineColor(actor.OriginalType);
					WorldspaceUITools.Instance.AddOutlinableToList(outlinable);
					WorldspaceUITools.Instance.RegisterActorOutlinable(actor, new List<Outlinable> { outlinable });
				}
				else if (!flag)
				{
					Debug.LogError("Character instance for prefab: " + actor.GetPrefabName() + " is missing the Outlinable component");
				}
			}
			SummonAppear componentInChildren = gameObject.GetComponentInChildren<SummonAppear>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = isSummoned;
				if (isSummoned)
				{
					MF.GameObjectAnimatorPlay(gameObject, "Summoned");
				}
			}
			AllActorsInScene.Add(gameObject);
			OnCharacterAdded(gameObject);
			if (isExhaustedCharacter)
			{
				gameObject.transform.SetParent(null);
				gameObject.transform.position = DeadPlayerLocation;
				characterManager.DeinitializeCharacter();
			}
			if (actor is CHeroSummonActor { IsCompanionSummon: not false } cHeroSummonActor)
			{
				PreviewEffectInfo previewEffectConfig = UIInfoTools.Instance.GetPreviewEffectConfig(cHeroSummonActor.SummonData.Model);
				string format = ((previewEffectConfig != null && previewEffectConfig.previewEffectText != null) ? LocalizationManager.GetTranslation(previewEffectConfig.previewEffectText) : LocalizationManager.GetTranslation(cHeroSummonActor.HeroSummonClass.DefaultModel + "_TOOLTIP"));
				ReferenceToSprite referenceToSprite = null;
				if (previewEffectConfig?.previewEffectIcon != null)
				{
					referenceToSprite = new ReferenceToSprite(previewEffectConfig?.previewEffectIcon);
				}
				ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI.AddEffect("Companion", referenceToSprite ?? UIInfoTools.Instance.GetCharacterSpriteRef(cHeroSummonActor.Summoner.CharacterClass.CharacterModel, highlight: false, cHeroSummonActor.Summoner.CharacterClass.CharacterYML.CustomCharacterConfig), string.Format("<color=#{1}>{0}", LocalizationManager.GetTranslation(cHeroSummonActor.SummonData.LocKey), UIInfoTools.Instance.GetCharacterColor(cHeroSummonActor.Summoner.CharacterClass.CharacterModel, cHeroSummonActor.Summoner.CharacterClass.CharacterYML.CustomCharacterConfig).ToHex()), string.Format(format, cHeroSummonActor.MaxHealth, cHeroSummonActor.SummonData.Move, cHeroSummonActor.SummonData.Attack, (cHeroSummonActor.SummonData.Range < 2) ? "-" : cHeroSummonActor.SummonData.Range.ToString()));
			}
			if (!isExhaustedCharacter)
			{
				SimpleLog.AddToSimpleLog("Creating character actor gameobject: " + LocalizationManager.GetTranslation(actor.ActorLocKey(), FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true) + GetActorIDForCombatLogIfNeeded(actor) + " at tile: " + tile.m_Tile.m_ArrayIndex.ToString());
			}
			return gameObject;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Choreographer.CreateCharacterActor().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00149", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public IEnumerator CreateCharacterActorCoroutine(CClientTile tile, CActor actor, bool isSummoned = false, string skinId = null, bool isExhaustedCharacter = false)
	{
		GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(actor.Type, actor.GetPrefabName(), skinId);
		if (characterPrefabFromBundle == null)
		{
			AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("npc_none", "NULLCharacter", "npcs");
			throw new Exception("Unable to find character with prefab name " + actor.GetPrefabName());
		}
		GameObject characterInstance = ObjectPool.Spawn(characterPrefabFromBundle, null, actor.Type != CActor.EType.Enemy);
		CharacterManager cm = CharacterManager.GetCharacterManager(characterInstance);
		characterInstance.name = actor.ActorGuid;
		cm.CharacterPrefab = characterPrefabFromBundle;
		cm.CharacterActor = actor;
		cm.InitialiseCharacter();
		SkinnedMeshRenderer[] componentsInChildren = cm.CharacterChildPrefabInstance.Result.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			skinnedMeshRenderer.material = new Material(skinnedMeshRenderer.material);
			if (!(skinnedMeshRenderer.material.shader.name == "Amp_Char_Shader"))
			{
				continue;
			}
			string htmlString = "#FFFFFF";
			float value = 0f;
			float num = 0f;
			if (actor.Type == CActor.EType.Player)
			{
				htmlString = (actor.Class as CCharacterClass).CharacterYML.ColourHTML;
				value = (actor.Class as CCharacterClass).CharacterYML.Fatness;
				num = (actor.Class as CCharacterClass).CharacterYML.VertexAnimIntensity;
			}
			else if (actor.Type == CActor.EType.Enemy || actor.Type == CActor.EType.Neutral || actor.Type == CActor.EType.Enemy2 || actor.Type == CActor.EType.Ally)
			{
				htmlString = (actor.Class as CMonsterClass).MonsterYML.ColourHTML;
				value = (actor.Class as CMonsterClass).MonsterYML.Fatness;
				num = (actor.Class as CMonsterClass).MonsterYML.VertexAnimIntensity;
			}
			else if (actor.Type == CActor.EType.HeroSummon)
			{
				htmlString = (actor.Class as CHeroSummonClass).SummonYML.ColourHTML;
				value = (actor.Class as CHeroSummonClass).SummonYML.Fatness;
				num = (actor.Class as CHeroSummonClass).SummonYML.VertexAnimIntensity;
			}
			if (skinnedMeshRenderer.material.HasProperty("_MOD_TINT") && ColorUtility.TryParseHtmlString(htmlString, out var color))
			{
				skinnedMeshRenderer.material.SetColor("_MOD_TINT", color);
			}
			if (skinnedMeshRenderer.material.HasProperty("_MOD_THICKEN"))
			{
				skinnedMeshRenderer.material.SetFloat("_MOD_THICKEN", value);
			}
			if (!skinnedMeshRenderer.material.HasProperty("_AddVertexAnim"))
			{
				continue;
			}
			if (skinnedMeshRenderer.material.GetFloat("_AddVertexAnim") == 0f)
			{
				if (num != 0f)
				{
					if (skinnedMeshRenderer.material.HasProperty("_NoiseSpeed"))
					{
						skinnedMeshRenderer.material.SetFloat("_NoiseSpeed", 0f);
					}
					if (skinnedMeshRenderer.material.HasProperty("_VertexAnim_Intensity"))
					{
						skinnedMeshRenderer.material.SetFloat("_VertexAnim_Intensity", num);
					}
					skinnedMeshRenderer.material.SetFloat("_AddVertexAnim", 1.1f);
				}
			}
			else if (skinnedMeshRenderer.material.GetFloat("_AddVertexAnim") == 1.1f && num == 0f)
			{
				skinnedMeshRenderer.material.SetFloat("_AddVertexAnim", 0f);
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m_ActorPrefab);
		Debug.Log("Instantiated actor prefab for actorGuid: " + actor.ActorGuid);
		cm.CharacterActorGO = gameObject;
		Animator animator = characterInstance.transform.GetComponentsInChildren<Animator>().FirstOrDefault((Animator x) => x.transform.gameObject.layer == LayerMask.NameToLayer("Hero") || x.transform.gameObject.layer == LayerMask.NameToLayer("Monster"));
		if (animator != null)
		{
			gameObject.transform.SetParent(animator.transform);
			Debug.Log("Set transform parent for actor prefab for actorGuid: " + actor.ActorGuid + " parent transform instance name: " + animator.transform.name);
			animator.gameObject.AddComponent<ActorEvents>();
			animator.SetInteger("ActorTypeEnum", (int)actor.OriginalType);
		}
		else
		{
			gameObject.transform.SetParent(characterInstance.transform);
			Debug.Log("Set transform parent for actor prefab for actorGuid: " + actor.ActorGuid + " parent transform instance name: " + characterInstance.transform.name);
			Debug.LogWarning("Unable to find animator on correct layer for CharacterPrefab:" + actor.GetPrefabName());
		}
		gameObject.transform.localPosition = Vector3.zero;
		UnityGameEditorObject component = characterInstance.GetComponent<UnityGameEditorObject>();
		if (tile != null)
		{
			characterInstance.transform.SetParent(ClientScenarioManager.s_ClientScenarioManager.m_Board.transform);
			characterInstance.transform.position = tile.m_GameObject.transform.position;
			if (actor.Type == CActor.EType.Enemy)
			{
				Vector3 enemyFocalPoint = GetEnemyFocalPoint(characterInstance);
				characterInstance.transform.LookAt(enemyFocalPoint);
			}
			else
			{
				GetAveragePositions(out var _, out var averageEnemyPosition, out var _, out var _, out var _);
				characterInstance.transform.LookAt(averageEnemyPosition);
			}
			component.m_ShouldSnapToHexSpacing = true;
		}
		else
		{
			component.m_ShouldSnapToHexSpacing = false;
		}
		if (!isExhaustedCharacter)
		{
			ActorBehaviour.SetActor(characterInstance, actor);
		}
		if (!isSummoned && actor.Type == CActor.EType.Player && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			yield return null;
			yield return CardsHandManager.Instance.AddPlayerCoroutine((CPlayerActor)actor);
		}
		if (!isExhaustedCharacter)
		{
			Outlinable outlinable = characterInstance.GetComponentInChildren<Outlinable>();
			bool flag = false;
			if (outlinable == null && actor is CObjectActor { AttachedProp: not null } cObjectActor)
			{
				flag = true;
				outlinable = Singleton<ObjectCacheService>.Instance.GetPropObject(cObjectActor.AttachedProp)?.GetComponentInChildren<Outlinable>(includeInactive: true);
			}
			if (outlinable != null)
			{
				outlinable.OutlineParameters.Color = UIInfoTools.GetCharacterOutlineColor(actor.OriginalType);
				WorldspaceUITools.Instance.AddOutlinableToList(outlinable);
				WorldspaceUITools.Instance.RegisterActorOutlinable(actor, new List<Outlinable> { outlinable });
			}
			else if (!flag)
			{
				Debug.LogError("Character instance for prefab: " + actor.GetPrefabName() + " is missing the Outlinable component");
			}
		}
		SummonAppear componentInChildren = characterInstance.GetComponentInChildren<SummonAppear>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = isSummoned;
			if (isSummoned)
			{
				MF.GameObjectAnimatorPlay(characterInstance, "Summoned");
			}
		}
		AllActorsInScene.Add(characterInstance);
		OnCharacterAdded(characterInstance);
		if (isExhaustedCharacter)
		{
			characterInstance.transform.SetParent(null);
			characterInstance.transform.position = DeadPlayerLocation;
			cm.DeinitializeCharacter();
		}
		if (actor is CHeroSummonActor { IsCompanionSummon: not false } cHeroSummonActor)
		{
			PreviewEffectInfo previewEffectConfig = UIInfoTools.Instance.GetPreviewEffectConfig(cHeroSummonActor.SummonData.Model);
			string format = ((previewEffectConfig != null && previewEffectConfig.previewEffectText != null) ? LocalizationManager.GetTranslation(previewEffectConfig.previewEffectText) : LocalizationManager.GetTranslation(cHeroSummonActor.HeroSummonClass.DefaultModel + "_TOOLTIP"));
			ReferenceToSprite referenceToSprite = null;
			if (previewEffectConfig?.previewEffectIcon != null)
			{
				referenceToSprite = new ReferenceToSprite(previewEffectConfig?.previewEffectIcon);
			}
			ActorBehaviour.GetActorBehaviour(characterInstance).m_WorldspacePanelUI.AddEffect("Companion", referenceToSprite ?? UIInfoTools.Instance.GetCharacterSpriteRef(cHeroSummonActor.Summoner.CharacterClass.CharacterModel, highlight: false, cHeroSummonActor.Summoner.CharacterClass.CharacterYML.CustomCharacterConfig), string.Format("<color=#{1}>{0}", LocalizationManager.GetTranslation(cHeroSummonActor.SummonData.LocKey), UIInfoTools.Instance.GetCharacterColor(cHeroSummonActor.Summoner.CharacterClass.CharacterModel, cHeroSummonActor.Summoner.CharacterClass.CharacterYML.CustomCharacterConfig).ToHex()), string.Format(format, cHeroSummonActor.MaxHealth, cHeroSummonActor.SummonData.Move, cHeroSummonActor.SummonData.Attack, (cHeroSummonActor.SummonData.Range < 2) ? "-" : cHeroSummonActor.SummonData.Range.ToString()));
		}
		if (!isExhaustedCharacter)
		{
			SimpleLog.AddToSimpleLog("Creating character actor gameobject: " + LocalizationManager.GetTranslation(actor.ActorLocKey(), FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true) + GetActorIDForCombatLogIfNeeded(actor) + " at tile: " + tile.m_Tile.m_ArrayIndex.ToString());
		}
		yield return characterInstance;
	}

	public void SetDisableVisualState(bool value)
	{
		m_UndoButton.SetDisableVisualState(value);
		m_SkipButton.SetDisableVisualState(value);
		m_selectButton.SetDisableVisualState(value);
		readyButton.SetDisableVisualState(value);
	}

	public GameObject CreatePreviewPlayer(CClientTile tile, CActor actor, string skinId = null)
	{
		return CreatePreviewPlayer(tile, actor.Type, actor.GetPrefabName(), skinId);
	}

	public GameObject CreatePreviewPlayer(CClientTile tile, CActor.EType actorType, string prefabName, string skinId = null)
	{
		try
		{
			GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(actorType, prefabName, skinId);
			if (characterPrefabFromBundle == null)
			{
				return null;
			}
			GameObject gameObject = ObjectPool.Spawn(characterPrefabFromBundle, ClientScenarioManager.s_ClientScenarioManager.m_Board.transform, actorType != CActor.EType.Enemy);
			CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
			characterManager.CharacterPrefab = characterPrefabFromBundle;
			characterManager.InitialiseCharacter(isPreview: true);
			UnityGameEditorObject component = gameObject.GetComponent<UnityGameEditorObject>();
			if (tile != null)
			{
				gameObject.transform.position = tile.m_GameObject.transform.position;
			}
			component.m_ShouldSnapToHexSpacing = true;
			MF.SetLayerRecursively(gameObject.gameObject, LayerMask.NameToLayer("Default"));
			gameObject.gameObject.SetActive(value: false);
			return gameObject;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Choreographer.CreateCharacterActor().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00149", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public void ClearPlayersEnemies()
	{
		foreach (GameObject clientPlayer in m_ClientPlayers)
		{
			CharacterManager characterManager = CharacterManager.GetCharacterManager(clientPlayer);
			if ((bool)characterManager)
			{
				characterManager.DeinitializeCharacter();
				ObjectPool.Recycle(clientPlayer, characterManager.CharacterPrefab);
			}
		}
		m_ClientPlayers.Clear();
		m_ClientDeadActors.Clear();
		foreach (GameObject value in m_ClientPlacementPreviews.Values)
		{
			CharacterManager characterManager2 = CharacterManager.GetCharacterManager(value);
			if ((bool)characterManager2)
			{
				characterManager2.DeinitializeCharacter();
				ObjectPool.Recycle(value, characterManager2.CharacterPrefab);
			}
		}
		m_ClientPlacementPreviews.Clear();
		foreach (GameObject clientEnemy in m_ClientEnemies)
		{
			CharacterManager characterManager3 = CharacterManager.GetCharacterManager(clientEnemy);
			if ((bool)characterManager3)
			{
				characterManager3.DeinitializeCharacter();
				ObjectPool.Recycle(clientEnemy, characterManager3.CharacterPrefab);
			}
		}
		m_ClientEnemies.Clear();
		foreach (GameObject clientHeroSummon in m_ClientHeroSummons)
		{
			CharacterManager characterManager4 = CharacterManager.GetCharacterManager(clientHeroSummon);
			if ((bool)characterManager4)
			{
				characterManager4.DeinitializeCharacter();
				ObjectPool.Recycle(clientHeroSummon, characterManager4.CharacterPrefab);
			}
		}
		m_ClientHeroSummons.Clear();
		foreach (GameObject clientAllyMonster in m_ClientAllyMonsters)
		{
			CharacterManager characterManager5 = CharacterManager.GetCharacterManager(clientAllyMonster);
			if ((bool)characterManager5)
			{
				characterManager5.DeinitializeCharacter();
				ObjectPool.Recycle(clientAllyMonster, characterManager5.CharacterPrefab);
			}
		}
		m_ClientAllyMonsters.Clear();
		foreach (GameObject clientEnemy2Monster in m_ClientEnemy2Monsters)
		{
			CharacterManager characterManager6 = CharacterManager.GetCharacterManager(clientEnemy2Monster);
			if ((bool)characterManager6)
			{
				characterManager6.DeinitializeCharacter();
				ObjectPool.Recycle(clientEnemy2Monster, characterManager6.CharacterPrefab);
			}
		}
		m_ClientEnemy2Monsters.Clear();
		foreach (GameObject clientNeutralMonster in m_ClientNeutralMonsters)
		{
			CharacterManager characterManager7 = CharacterManager.GetCharacterManager(clientNeutralMonster);
			if ((bool)characterManager7)
			{
				characterManager7.DeinitializeCharacter();
				ObjectPool.Recycle(clientNeutralMonster, characterManager7.CharacterPrefab);
			}
		}
		m_ClientNeutralMonsters.Clear();
		foreach (GameObject clientObject in m_ClientObjects)
		{
			CharacterManager characterManager8 = CharacterManager.GetCharacterManager(clientObject);
			if ((bool)characterManager8)
			{
				characterManager8.DeinitializeCharacter();
				ObjectPool.Recycle(clientObject, characterManager8.CharacterPrefab);
			}
		}
		m_ClientObjects.Clear();
	}

	private IEnumerator CreatePlayersEnemies(Action callback)
	{
		m_BlockClientMessageProcessing = true;
		yield return null;
		m_GameScenarioScreen.SetActive(value: true);
		yield return null;
		int x;
		for (x = 0; x < ScenarioManager.Scenario.PlayerActors.Count; x++)
		{
			CClientTile tile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[ScenarioManager.Scenario.PlayerActors[x].StartArrayIndex.X, ScenarioManager.Scenario.PlayerActors[x].StartArrayIndex.Y];
			if (tile != null)
			{
				switch (SaveData.Instance.Global.GameMode)
				{
				case EGameMode.Campaign:
				{
					CMapCharacter characterData2 = SaveData.Instance.Global.CampaignData.AdventureMapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == ScenarioManager.Scenario.PlayerActors[x].CharacterClass.ID);
					string skin = UIInfoTools.Instance.GetSkinForCharacter(characterData2);
					CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(tile, ScenarioManager.Scenario.PlayerActors[x], isSummoned: false, skin));
					yield return coroutineWithData.Coroutine;
					m_ClientPlayers.Add(coroutineWithData.Result);
					yield return null;
					m_ClientPlacementPreviews[ScenarioManager.Scenario.PlayerActors[x].CharacterClass.CharacterID] = CreatePreviewPlayer(tile, ScenarioManager.Scenario.PlayerActors[x], skin);
					break;
				}
				case EGameMode.Guildmaster:
				{
					FFSNet.Console.Log("Creating Player: " + ScenarioManager.Scenario.PlayerActors[x].CharacterClass.ID + "(Actor ID: " + ScenarioManager.Scenario.PlayerActors[x].ID + ")");
					CMapCharacter characterData = SaveData.Instance.Global.AdventureData.AdventureMapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == ScenarioManager.Scenario.PlayerActors[x].CharacterClass.ID);
					string skin = UIInfoTools.Instance.GetSkinForCharacter(characterData);
					CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(tile, ScenarioManager.Scenario.PlayerActors[x], isSummoned: false, skin));
					yield return coroutineWithData.Coroutine;
					m_ClientPlayers.Add(coroutineWithData.Result);
					yield return null;
					m_ClientPlacementPreviews[ScenarioManager.Scenario.PlayerActors[x].CharacterClass.CharacterID] = CreatePreviewPlayer(tile, ScenarioManager.Scenario.PlayerActors[x], skin);
					break;
				}
				case EGameMode.LevelEditor:
				case EGameMode.SingleScenario:
				case EGameMode.Autotest:
				case EGameMode.FrontEndTutorial:
				{
					CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(tile, ScenarioManager.Scenario.PlayerActors[x]));
					yield return coroutineWithData.Coroutine;
					m_ClientPlayers.Add(coroutineWithData.Result);
					yield return null;
					m_ClientPlacementPreviews[ScenarioManager.Scenario.PlayerActors[x].CharacterClass.CharacterID] = CreatePreviewPlayer(tile, ScenarioManager.Scenario.PlayerActors[x]);
					break;
				}
				}
			}
			yield return null;
		}
		int x2;
		for (x2 = 0; x2 < ScenarioManager.Scenario.ExhaustedPlayers.Count; x2++)
		{
			switch (SaveData.Instance.Global.GameMode)
			{
			case EGameMode.Campaign:
			{
				CMapCharacter characterData3 = SaveData.Instance.Global.CampaignData.AdventureMapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == ScenarioManager.Scenario.ExhaustedPlayers[x2].CharacterClass.ID);
				string skinForCharacter = UIInfoTools.Instance.GetSkinForCharacter(characterData3);
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(null, ScenarioManager.Scenario.ExhaustedPlayers[x2], isSummoned: false, skinForCharacter, isExhaustedCharacter: true));
				yield return coroutineWithData.Coroutine;
				m_ClientDeadActors.Add(coroutineWithData.Result);
				break;
			}
			case EGameMode.Guildmaster:
			{
				CMapCharacter characterData4 = SaveData.Instance.Global.AdventureData.AdventureMapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == ScenarioManager.Scenario.ExhaustedPlayers[x2].CharacterClass.ID);
				string skinForCharacter2 = UIInfoTools.Instance.GetSkinForCharacter(characterData4);
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(null, ScenarioManager.Scenario.ExhaustedPlayers[x2], isSummoned: false, skinForCharacter2, isExhaustedCharacter: true));
				yield return coroutineWithData.Coroutine;
				m_ClientDeadActors.Add(coroutineWithData.Result);
				break;
			}
			case EGameMode.LevelEditor:
			case EGameMode.SingleScenario:
			case EGameMode.Autotest:
			case EGameMode.FrontEndTutorial:
			{
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(null, ScenarioManager.Scenario.ExhaustedPlayers[x2], isSummoned: false, null, isExhaustedCharacter: true));
				yield return coroutineWithData.Coroutine;
				m_ClientDeadActors.Add(coroutineWithData.Result);
				break;
			}
			}
			yield return null;
		}
		for (int x3 = 0; x3 < ScenarioManager.Scenario.HeroSummons.Count; x3++)
		{
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[ScenarioManager.Scenario.HeroSummons[x3].StartArrayIndex.X, ScenarioManager.Scenario.HeroSummons[x3].StartArrayIndex.Y];
			if (cClientTile != null)
			{
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(cClientTile, ScenarioManager.Scenario.HeroSummons[x3], isSummoned: true));
				yield return coroutineWithData.Coroutine;
				m_ClientHeroSummons.Add(coroutineWithData.Result);
			}
			yield return null;
		}
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			bool flag = false;
			if (allEnemyState is ObjectState)
			{
				flag = true;
			}
			string prefabName = ((!flag) ? MonsterClassManager.Find(allEnemyState.ClassID).Models[allEnemyState.ChosenModelIndex] : MonsterClassManager.FindObjectClass(allEnemyState.ClassID).Models[allEnemyState.ChosenModelIndex]);
			CharacterManager.WarmUpCharacter(AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Enemy, prefabName));
			yield return null;
		}
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[enemy.StartArrayIndex.X, enemy.StartArrayIndex.Y];
			if (cClientTile2 != null)
			{
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(cClientTile2, enemy));
				yield return coroutineWithData.Coroutine;
				m_ClientEnemies.Add(coroutineWithData.Result);
			}
			yield return null;
		}
		foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
		{
			CClientTile cClientTile3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[allyMonster.StartArrayIndex.X, allyMonster.StartArrayIndex.Y];
			if (cClientTile3 != null)
			{
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(cClientTile3, allyMonster));
				yield return coroutineWithData.Coroutine;
				m_ClientAllyMonsters.Add(coroutineWithData.Result);
			}
			yield return null;
		}
		foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
		{
			CClientTile cClientTile4 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[enemy2Monster.StartArrayIndex.X, enemy2Monster.StartArrayIndex.Y];
			if (cClientTile4 != null)
			{
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(cClientTile4, enemy2Monster));
				yield return coroutineWithData.Coroutine;
				m_ClientEnemy2Monsters.Add(coroutineWithData.Result);
			}
			yield return null;
		}
		foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
		{
			CClientTile cClientTile5 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[neutralMonster.StartArrayIndex.X, neutralMonster.StartArrayIndex.Y];
			if (cClientTile5 != null)
			{
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(cClientTile5, neutralMonster));
				yield return coroutineWithData.Coroutine;
				m_ClientNeutralMonsters.Add(coroutineWithData.Result);
			}
			yield return null;
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			CClientTile cClientTile6 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[@object.StartArrayIndex.X, @object.StartArrayIndex.Y];
			if (cClientTile6 != null)
			{
				CoroutineWithData<GameObject> coroutineWithData = new CoroutineWithData<GameObject>(this, CreateCharacterActorCoroutine(cClientTile6, @object));
				yield return coroutineWithData.Coroutine;
				m_ClientObjects.Add(coroutineWithData.Result);
			}
			yield return null;
		}
		SetCharacterPositions();
		m_BlockClientMessageProcessing = false;
		callback();
	}

	public Vector3 GetAllAveragePositions()
	{
		Vector3 zero = Vector3.zero;
		foreach (GameObject clientPlayer in m_ClientPlayers)
		{
			zero += clientPlayer.transform.position;
		}
		foreach (GameObject clientEnemy in m_ClientEnemies)
		{
			zero += clientEnemy.transform.position;
		}
		foreach (GameObject clientEnemy2Monster in m_ClientEnemy2Monsters)
		{
			zero += clientEnemy2Monster.transform.position;
		}
		return zero / (ScenarioManager.Scenario.Enemies.Count + ScenarioManager.Scenario.PlayerActors.Count + ScenarioManager.Scenario.NeutralMonsters.Count);
	}

	private Vector3 GetEnemyFocalPoint(GameObject enemyGO)
	{
		Vector3 zero = Vector3.zero;
		CMap map = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap s) => s.Monsters.Exists((EnemyState e) => e.Enemy?.ActorGuid == enemyGO.name));
		if (map != null)
		{
			List<GameObject> list = (from w1 in m_ClientPlayers.Concat(m_ClientHeroSummons)
				where (from s in ScenarioManager.Scenario.PlayerActors
					where !s.IsDead && map.MapTiles.Exists((CMapTile e) => e.ArrayIndex.X == s.ArrayIndex.X && e.ArrayIndex.Y == s.ArrayIndex.Y)
					select s.ActorGuid).Contains(w1.name) || (from s in ScenarioManager.Scenario.HeroSummons
					where !s.IsDead && map.MapTiles.Exists((CMapTile e) => e.ArrayIndex.X == s.ArrayIndex.X && e.ArrayIndex.Y == s.ArrayIndex.Y)
					select s.ActorGuid).Contains(w1.name)
				select w1).ToList();
			if (list.Count > 0)
			{
				foreach (GameObject item in list)
				{
					zero += item.transform.position;
				}
				zero /= (float)list.Count;
			}
			else if (map.DoorProps.FirstOrDefault((CObjectProp f) => (f as CObjectDoor).DoorIsOpen) is CObjectDoor prop)
			{
				GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(prop);
				if (propObject != null)
				{
					return propObject.transform.position;
				}
			}
			return zero;
		}
		GetAveragePositions(out var averagePlayerPosition, out var _, out var _, out var _, out var _);
		return averagePlayerPosition;
	}

	private void GetAveragePositions(out Vector3 averagePlayerPosition, out Vector3 averageEnemyPosition, out Vector3 averageAllyMonsterPosition, out Vector3 averageEnemy2Position, out Vector3 averageNeutralMonsterPosition)
	{
		averagePlayerPosition = Vector3.zero;
		List<GameObject> list = m_ClientPlayers.Concat(m_ClientHeroSummons).ToList();
		foreach (GameObject item in list)
		{
			averagePlayerPosition += item.transform.position;
		}
		averagePlayerPosition /= (float)list.Count;
		averageEnemyPosition = Vector3.zero;
		foreach (GameObject clientEnemy in m_ClientEnemies)
		{
			averageEnemyPosition += clientEnemy.transform.position;
		}
		averageEnemyPosition /= (float)m_ClientEnemies.Count;
		averageAllyMonsterPosition = Vector3.zero;
		foreach (GameObject clientAllyMonster in m_ClientAllyMonsters)
		{
			averageAllyMonsterPosition += clientAllyMonster.transform.position;
		}
		averageAllyMonsterPosition /= (float)m_ClientEnemies.Count;
		averageEnemy2Position = Vector3.zero;
		foreach (GameObject clientEnemy2Monster in m_ClientEnemy2Monsters)
		{
			averageEnemy2Position += clientEnemy2Monster.transform.position;
		}
		averageEnemy2Position /= (float)m_ClientEnemy2Monsters.Count;
		averageNeutralMonsterPosition = Vector3.zero;
		foreach (GameObject clientNeutralMonster in m_ClientNeutralMonsters)
		{
			averageNeutralMonsterPosition += clientNeutralMonster.transform.position;
		}
		averageNeutralMonsterPosition /= (float)m_ClientEnemies.Count;
	}

	public void SetCharacterPositions()
	{
		try
		{
			GetAveragePositions(out var averagePlayerPosition, out var averageEnemyPosition, out var _, out var _, out var _);
			if (SaveData.Instance.Global.CurrentlyPlayingCustomLevel && SaveData.Instance.Global.CurrentCustomLevelData.HasFixedPlayerFacingRotation)
			{
				TileIndex tileIndex = SaveData.Instance.Global.CurrentCustomLevelData.FixedFacingDirectionIndices[0];
				TileIndex tileIndex2 = SaveData.Instance.Global.CurrentCustomLevelData.FixedFacingDirectionIndices[1];
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileIndex.X, tileIndex.Y];
				Vector3 forward = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileIndex2.X, tileIndex2.Y].m_GameObject.transform.position - cClientTile.m_GameObject.transform.position;
				foreach (GameObject item in m_ClientPlayers.Concat(m_ClientHeroSummons))
				{
					item.transform.forward = forward;
				}
			}
			else
			{
				GameObject gameObject = ClientScenarioManager.s_ClientScenarioManager.DungeonEntrance;
				Vector3 vector = Vector3.zero;
				bool flag = false;
				if (gameObject == null)
				{
					(GameObject Door, Vector3 DoorNormal) apparanceDungeonEntrance = ClientScenarioManager.s_ClientScenarioManager.ApparanceDungeonEntrance;
					gameObject = apparanceDungeonEntrance.Door;
					vector = apparanceDungeonEntrance.DoorNormal;
					flag = true;
				}
				if (!LevelEditorController.s_Instance.IsEditing && gameObject != null && m_CurrentState.RoundNumber <= 1)
				{
					UnityGameEditorDoorProp component = gameObject.GetComponent<UnityGameEditorDoorProp>();
					Vector3 vector2 = ((component != null) ? component.ForwardVector() : (flag ? vector : gameObject.transform.forward));
					float num = ((!(Vector3.Dot(vector2, averagePlayerPosition - gameObject.transform.position) < 0f)) ? 1 : (-1));
					foreach (GameObject item2 in m_ClientPlayers.Concat(m_ClientHeroSummons))
					{
						item2.transform.forward = num * vector2;
					}
				}
				else
				{
					foreach (GameObject item3 in m_ClientPlayers.Concat(m_ClientHeroSummons))
					{
						item3.transform.LookAt(averageEnemyPosition);
					}
				}
			}
			foreach (GameObject clientEnemy in m_ClientEnemies)
			{
				clientEnemy.transform.LookAt(GetEnemyFocalPoint(clientEnemy));
			}
			foreach (GameObject clientAllyMonster in m_ClientAllyMonsters)
			{
				clientAllyMonster.transform.LookAt(averageEnemyPosition);
			}
			foreach (GameObject clientEnemy2Monster in m_ClientEnemy2Monsters)
			{
				clientEnemy2Monster.transform.LookAt(averageEnemyPosition);
			}
			foreach (GameObject clientNeutralMonster in m_ClientNeutralMonsters)
			{
				clientNeutralMonster.transform.LookAt(averageEnemyPosition);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Choreographer.SetCharacterPositions().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00150", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ClearHilightedActors()
	{
		try
		{
			foreach (GameObject clientActorObject in ClientActorObjects)
			{
				ActorBehaviour.SetHilighted(clientActorObject, hilight: false);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Choreographer.ClearHighlightedActors().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00151", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void MessageHandler(CMessageData message, bool processImmediately)
	{
		if (SceneController.Instance.GlobalErrorMessage.CheckShowingMessageFromThread)
		{
			return;
		}
		if (processImmediately)
		{
			ProcessMessage(message);
			return;
		}
		lock (m_MessageQueue)
		{
			m_MessageQueue.Add(message);
		}
	}

	public void Play(bool restart = false)
	{
		try
		{
			IsRestarting = false;
			try
			{
				if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
				{
					if (restart)
					{
						UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState.InitialState);
					}
					else
					{
						UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState.CurrentState);
					}
				}
				else
				{
					if (SaveData.Instance.Global.GameMode != EGameMode.Guildmaster)
					{
						return;
					}
					if (SaveData.Instance.Global.AdventureData.AdventureMapState.TutorialCompleted)
					{
						if (restart)
						{
							UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState.InitialState);
						}
						else if (SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.Players.Count == 0)
						{
							UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState, SaveData.Instance.Global.AdventureData.AdventureMapState.MapParty);
						}
						else
						{
							UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState);
						}
					}
					else if (restart)
					{
						UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.AdventureData.AdventureMapState.HeadquartersState.CurrentStartingScenario.InitialState);
					}
					else
					{
						UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.AdventureData.AdventureMapState.HeadquartersState.CurrentStartingScenario.CurrentState);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to load scenario.  Returning to main menu.\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00152", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("An exception occurred within the Choreographer.Play().  The scene loading did not complete successfully.\n" + ex2.Message + "\n" + ex2.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00153", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
		}
	}

	public void ScenarioCreateClientBoardAndCharacters(Action callback)
	{
		AllActorsInScene.Clear();
		Camera scenarioCamera = RoomVisibilityManager.s_Instance.ScenarioCamera;
		CameraController.s_CameraController.SetCamera(scenarioCamera.gameObject);
		WorldspaceUITools.Instance.Init(scenarioCamera);
		Transform[] componentsInChildren = RoomVisibilityManager.s_Instance.Maps.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: true);
		}
		ScenarioManager.InitScenario(LevelEditorController.s_Instance.IsEditing);
		s_Choreographer.m_Description.text = ScenarioManager.Scenario.Description;
		ClearPlayersEnemies();
		StartCoroutine(CreatePlayersEnemies(callback));
		m_Wait = true;
	}

	public void ScenarioImportProcessedCallback(bool generatedScenario)
	{
		try
		{
			if (!LevelEditorController.s_Instance.IsEditing)
			{
				SetChoreographerState(ChoreographerStateType.Play, 0, null);
				ScenarioRuleClient.Start();
				if (generatedScenario)
				{
					GetAveragePositions(out var averagePlayerPosition, out var _, out var _, out var _, out var _);
					Transform transform = null;
					GameObject dungeonEntrance = ClientScenarioManager.s_ClientScenarioManager.DungeonEntrance;
					UnityGameEditorDoorProp unityGameEditorDoorProp = null;
					if (dungeonEntrance != null)
					{
						unityGameEditorDoorProp = dungeonEntrance.GetComponent<UnityGameEditorDoorProp>();
						transform = dungeonEntrance.transform;
					}
					else
					{
						transform = ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles[0].m_GameObject.transform;
					}
					bool flag = unityGameEditorDoorProp != null && (unityGameEditorDoorProp.m_DoorType == CObjectDoor.EDoorType.ThinDoor || unityGameEditorDoorProp.m_DoorType == CObjectDoor.EDoorType.ThinNarrowDoor);
					CameraController.s_CameraController.SetCameraDirectionAndFocalPoint(transform, averagePlayerPosition, (!flag) ? 2 : 0);
				}
			}
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the ScenarioImportProcessedCallback.  The scene loading did not complete successfully.\n" + ex.Message + "\n" + ex.StackTrace);
			List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
			list.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_SCENARIO_BUTTON", UnityGameEditorRuntime.ErrorHandlingUnloadSceneResetScenarioStateAndRetryLoad, KeyAction.UI_SUBMIT));
			list.Add(new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, KeyAction.UI_CANCEL));
			SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_CHOREO_00154", ex.StackTrace, list, ex.Message);
		}
	}

	public void Stop()
	{
		try
		{
			ScenarioRuleClient.Stop();
			ClearStars();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Choreographer.Stop().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00155", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Pass()
	{
		try
		{
			m_ConfirmDoorTileSelect.SetActive(value: false);
			Waypoint.s_LockWaypoints = false;
			Waypoint.Clear();
			ScenarioRuleClient.Pass();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Choreographer.Pass().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00156", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void TileHandler(CClientTile clientTile, List<CTile> optionalTileList = null, bool networkActionIfOnline = false, bool isUserClick = false, bool actingPlayerHasSecondClickConfirmationEnabled = false)
	{
		if (LevelEditorController.InterceptTileAction(clientTile) || DebugMenu.InterceptTileAction(clientTile))
		{
			return;
		}
		if (AutoTestController.s_ShouldRecordUIActionsForAutoTest && !AutoTestIgnoreTileClick && isUserClick)
		{
			AutoTestController.s_Instance.LogTileClick(clientTile, optionalTileList, null, Waypoint.s_PlacementTile);
		}
		else if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			Waypoint.s_PlacementTile = CAutoTileClick.TileIndexToClientTile((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoTileClick)?.PlacementTile);
		}
		m_lastSelectedTile = clientTile;
		if (m_WaitState.m_State == ChoreographerStateType.WaitingForCardSelection && CardsHandManager.Instance.IsActive())
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.FindPlayerAt(clientTile.m_Tile.m_ArrayIndex);
			if (cPlayerActor != null && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<ConfirmationBoxState>())
			{
				Debug.Log("Tile  handler select actor");
				if (InitiativeTrack.Instance.Select(cPlayerActor))
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
					CardsHandManager.Instance.SwitchHand(cPlayerActor);
					WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				}
				return;
			}
		}
		if (m_WaitState.m_State == ChoreographerStateType.WaitingForCardSelection && clientTile == Waypoint.s_PlacementTile && InitiativeTrack.Instance.SelectedActor() != null)
		{
			CPlayerActor cPlayerActor2 = (CPlayerActor)InitiativeTrack.Instance.SelectedActor().Actor;
			CTile item = ScenarioManager.Tiles[cPlayerActor2.ArrayIndex.X, cPlayerActor2.ArrayIndex.Y];
			if (ScenarioManager.StartingTiles.Contains(item))
			{
				if (FFSNetwork.IsOnline)
				{
					if (!InitiativeTrack.Instance.SelectedActor().Actor.IsUnderMyControl || Singleton<UIReadyToggle>.Instance.ToggledOn || Singleton<UIReadyToggle>.Instance.IsProgressingBar)
					{
						return;
					}
					if (FFSNetwork.IsHost)
					{
						AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
						PlaceActorAtRoundStart(InitiativeTrack.Instance.SelectedActor().Actor, clientTile);
					}
					int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? cPlayerActor2.CharacterName.GetHashCode() : cPlayerActor2.CharacterClass.ModelInstanceID);
					IProtocolToken supplementaryDataToken = new TileToken(clientTile.m_Tile.m_ArrayIndex);
					Synchronizer.ReplicateControllableStateChange(GameActionType.PlaceCharacter, ActionPhaseType.StartOfRound, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken, null, null, null, validateOnServerBeforeExecuting: true);
				}
				else
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
					PlaceActorAtRoundStart(InitiativeTrack.Instance.SelectedActor().Actor, clientTile);
				}
			}
		}
		if (m_TileSelectionDisabled && isUserClick)
		{
			if (m_BufferedSelectedTile == null)
			{
				FFSNet.Console.Log("TILEHANDLER - TILE SELECTION DISABLED - BUFFERING TILE");
				m_BufferedSelectedTile = clientTile;
				m_BufferedTileNetworked = networkActionIfOnline;
				m_BufferedSecondClickToConfirm = actingPlayerHasSecondClickConfirmationEnabled;
			}
			return;
		}
		if (m_WaitState.m_State == ChoreographerStateType.WaitingForAreaAttackFocusSelection && WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets() > 0)
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
			SetChoreographerState(ChoreographerStateType.Play, 0, null);
		}
		if (m_WaitState.m_State < ChoreographerStateType.Play || (isUserClick && WorldspaceStarHexDisplay.Instance.CurrentDisplayState == WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone) || PhaseManager.Phase == null || !(PhaseManager.Phase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction) || cPhaseAction.CurrentPhaseAbility.m_Ability == null)
		{
			return;
		}
		CActor targetingActor = cPhaseAction.CurrentPhaseAbility.m_Ability.TargetingActor;
		if (targetingActor == null || targetingActor.Type != CActor.EType.Player || !cPhaseAction.CurrentPhaseAbility.m_Ability.CanReceiveTileSelection() || (cPhaseAction.CurrentPhaseAbility.m_Ability.RequiresWaypointSelection() && optionalTileList == null))
		{
			return;
		}
		CAbility cAbility = cPhaseAction.CurrentPhaseAbility.m_Ability;
		if (cAbility is CAbilityMerged cAbilityMerged)
		{
			cAbility = cAbilityMerged.ActiveAbility;
		}
		if (cAbility.RequiresWaypointSelection())
		{
			DisableTileSelection(active: true);
			m_ConfirmDoorTileSelect.SetActive(value: false);
			if (FFSNetwork.IsOnline && !s_Choreographer.ThisPlayerHasTurnControl)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
			}
			ScenarioRuleClient.TileSelected(clientTile.m_Tile, optionalTileList, cAbility.AbilityType == CAbility.EAbilityType.Move);
		}
		else
		{
			if (FFSNetwork.IsOnline && ActionProcessor.CurrentPhase == ActionPhaseType.NONE)
			{
				return;
			}
			DisableTileSelection(active: true);
			m_ConfirmDoorTileSelect.SetActive(value: false);
			FFSNet.Console.Log("TILEHANDLER - ABILITY SECTION");
			if (cAbility.AreaEffect != null)
			{
				FFSNet.Console.Log("TILEHANDLER - AOE ABILITY");
				if (WorldspaceStarHexDisplay.Instance.IsAOELocked())
				{
					FFSNet.Console.Log("TILEHANDLER - AOE LOCKED");
					if (cAbility.IsWaitingForSingleTargetItem() || cAbility.IsWaitingForSingleTargetActiveBonus())
					{
						FFSNet.Console.Log("TILEHANDLER - AOE WAITING FOR SINGLE TARGET ITEM OR ACTIVE BONUS");
						CActor cActor = cAbility.ActorsToTarget.SingleOrDefault((CActor s) => s.ArrayIndex == clientTile.m_Tile.m_ArrayIndex);
						if (cActor != null)
						{
							AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
							ScenarioRuleClient.ApplySingleTarget(cActor);
							readyButton.SetInteractable(interactable: true);
						}
						DisableTileSelection(active: false);
						if (FFSNetwork.IsOnline && m_CurrentActor.IsUnderMyControl)
						{
							NetworkTargetSelection(clientTile, optionalTileList);
						}
						if (ActionProcessor.IsWaitingForTileSelectionFinishedMessage != null)
						{
							ActionProcessor.FinishProcessingAction(ActionProcessor.IsWaitingForTileSelectionFinishedMessage);
							ActionProcessor.IsWaitingForTileSelectionFinishedMessage = null;
						}
					}
					else if (WorldspaceStarHexDisplay.Instance.AlreadySelected(clientTile) && ((cAbility.AbilityType != CAbility.EAbilityType.Push && cAbility.AbilityType != CAbility.EAbilityType.Pull) || (!cAbility.IsInlineSubAbility && !cAbility.IsModifierAbility)))
					{
						FFSNet.Console.Log("TILEHANDLER - AOE SELECTED ALREADY SELECTED TILE");
						if (actingPlayerHasSecondClickConfirmationEnabled)
						{
							FFSNet.Console.Log("TILEHANDLER - AOE - ACTING PLAYER HAS SECOND CLICK CONFIRM ENABLED");
							if (FFSNetwork.IsOnline && m_CurrentActor.IsUnderMyControl)
							{
								NetworkTargetSelection(clientTile, optionalTileList);
							}
							WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
							ScenarioRuleClient.StepComplete();
							m_UndoButton.Toggle(active: false);
							readyButton.Toggle(active: false);
							m_SkipButton.Toggle(active: false);
							SetActiveSelectButton(activate: false);
							if (ActionProcessor.IsWaitingForTileSelectionFinishedMessage != null)
							{
								ActionProcessor.FinishProcessingAction(ActionProcessor.IsWaitingForTileSelectionFinishedMessage);
								ActionProcessor.IsWaitingForTileSelectionFinishedMessage = null;
							}
						}
						else
						{
							FFSNet.Console.Log("TILEHANDLER - AOE - ACTING PLAYER HAS SECOND CLICK CONFIRM DISABLED");
							if (FFSNetwork.IsOnline && m_CurrentActor.IsUnderMyControl)
							{
								NetworkTargetSelection(clientTile, optionalTileList, deselectInstead: true);
							}
							m_UndoButton.SetInteractable(active: false);
							readyButton.SetInteractable(interactable: false);
							m_SkipButton.SetInteractable(active: false);
							SetActiveSelectButton(activate: false);
							if (FFSNetwork.IsOnline && !s_Choreographer.ThisPlayerHasTurnControl)
							{
								ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
							}
							ScenarioRuleClient.TileDeselected(clientTile.m_Tile, optionalTileList);
							this.OnAoeTileSelected?.Invoke(obj: false);
							SetChoreographerState(ChoreographerStateType.WaitingForTileSelected, 0, null);
						}
					}
					else
					{
						FFSNet.Console.Log("TILEHANDLER - AOE - LOCKED AOE + INVALID TILE, DO NOTHING");
						AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
						DisableTileSelection(active: false);
					}
				}
				else
				{
					FFSNet.Console.Log("TILEHANDLER - NO AOE LOCK");
					cAbility.UpdateAreaEffect(clientTile.m_Tile, WorldspaceStarHexDisplay.Instance.AreaEffectAngle);
					if (FFSNetwork.IsOnline && m_CurrentActor.IsUnderMyControl)
					{
						NetworkTargetSelection(clientTile, optionalTileList);
					}
					m_UndoButton.SetInteractable(active: false);
					readyButton.SetInteractable(interactable: false);
					m_SkipButton.SetInteractable(active: false);
					SetActiveSelectButton(activate: false);
					if (FFSNetwork.IsOnline && !s_Choreographer.ThisPlayerHasTurnControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
					}
					ScenarioRuleClient.TileSelected(clientTile.m_Tile, optionalTileList);
					this.OnAoeTileSelected?.Invoke(obj: true);
					SetChoreographerState(ChoreographerStateType.WaitingForTileSelected, 0, null);
				}
			}
			else if (cAbility.IsWaitingForSingleTargetItem() || cAbility.IsWaitingForSingleTargetActiveBonus())
			{
				FFSNet.Console.Log("TILEHANDLER - TARGETED ABILITY WAITING FOR SINGLE TARGET ITEM OR ACTIVE BONUS");
				CActor cActor2 = cAbility.ActorsToTarget.SingleOrDefault((CActor s) => s.ArrayIndex == clientTile.m_Tile.m_ArrayIndex);
				if (cActor2 != null)
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
					ScenarioRuleClient.ApplySingleTarget(cActor2);
					readyButton.SetInteractable(interactable: true);
				}
				DisableTileSelection(active: false);
				if (FFSNetwork.IsOnline && m_CurrentActor.IsUnderMyControl)
				{
					NetworkTargetSelection(clientTile, optionalTileList);
				}
				if (ActionProcessor.IsWaitingForTileSelectionFinishedMessage != null)
				{
					ActionProcessor.FinishProcessingAction(ActionProcessor.IsWaitingForTileSelectionFinishedMessage);
					ActionProcessor.IsWaitingForTileSelectionFinishedMessage = null;
				}
			}
			else if (WorldspaceStarHexDisplay.Instance.AlreadySelected(clientTile) && ((cAbility.AbilityType != CAbility.EAbilityType.Push && cAbility.AbilityType != CAbility.EAbilityType.Pull) || (!cAbility.IsInlineSubAbility && !cAbility.IsModifierAbility)))
			{
				FFSNet.Console.Log("TILEHANDLER - TARGETED ABILITY SELECTED ALREADY SELECTED TILE");
				if (actingPlayerHasSecondClickConfirmationEnabled)
				{
					FFSNet.Console.Log("TILEHANDLER - TARGETED ABILITY - ACTING PLAYER HAS SECOND CLICK CONFIRM ENABLED");
					if (!cAbility.EnoughTargetsSelected())
					{
						InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_NOT_ENOUGH_TARGETS_SELECTED");
						DisableTileSelection(active: false);
						return;
					}
					if (FFSNetwork.IsOnline && m_CurrentActor.IsUnderMyControl)
					{
						NetworkTargetSelection(clientTile, optionalTileList);
					}
					Singleton<UIUseAugmentationsBar>.Instance.Hide();
					Singleton<UIUseItemsBar>.Instance.Hide();
					Singleton<UIActiveBonusBar>.Instance.LockToggledActiveBonuses();
					Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
					ScenarioRuleClient.StepComplete();
					m_UndoButton.Toggle(active: false);
					readyButton.Toggle(active: false);
					m_SkipButton.Toggle(active: false);
					SetActiveSelectButton(activate: false);
					if (ActionProcessor.IsWaitingForTileSelectionFinishedMessage != null)
					{
						ActionProcessor.FinishProcessingAction(ActionProcessor.IsWaitingForTileSelectionFinishedMessage);
						ActionProcessor.IsWaitingForTileSelectionFinishedMessage = null;
					}
				}
				else
				{
					FFSNet.Console.Log("TILEHANDLER - TARGETED ABILITY - ACTING PLAYER HAS SECOND CLICK CONFIRM ENABLED");
					if (FFSNetwork.IsOnline && m_CurrentActor.IsUnderMyControl)
					{
						NetworkTargetSelection(clientTile, optionalTileList, deselectInstead: true);
					}
					m_UndoButton.SetInteractable(active: false);
					readyButton.SetInteractable(interactable: false);
					m_SkipButton.SetInteractable(active: false);
					SetActiveSelectButton(activate: false);
					if (FFSNetwork.IsOnline && !s_Choreographer.ThisPlayerHasTurnControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
					}
					ScenarioRuleClient.TileDeselected(clientTile.m_Tile, optionalTileList);
					SetChoreographerState(ChoreographerStateType.WaitingForTileSelected, 0, null);
				}
			}
			else
			{
				FFSNet.Console.Log("TILEHANDLER - TARGETED ABILITY - TILE NOT YET SELECTED. Current selected targets: " + WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets());
				if (FFSNetwork.IsOnline && m_CurrentActor.IsUnderMyControl)
				{
					NetworkTargetSelection(clientTile, optionalTileList);
				}
				m_UndoButton.SetInteractable(active: false);
				readyButton.SetInteractable(interactable: false);
				m_SkipButton.SetInteractable(active: false);
				SetActiveSelectButton(activate: false);
				if (FFSNetwork.IsOnline && !s_Choreographer.ThisPlayerHasTurnControl)
				{
					ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
				}
				ScenarioRuleClient.TileSelected(clientTile.m_Tile, optionalTileList);
				SetChoreographerState(ChoreographerStateType.WaitingForTileSelected, 0, null);
			}
		}
	}

	private void PlaceActorAtRoundStart(CActor actor, CClientTile clientTile)
	{
		actor.ArrayIndex = clientTile.m_Tile.m_ArrayIndex;
		SimpleLog.AddToSimpleLog(LocalizationManager.GetTranslation(actor.ActorLocKey()) + " swaps starting position to: " + actor.ArrayIndex.X + "," + actor.ArrayIndex.Y);
		ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(actor)).ForceSetLocoIntermediateTarget(clientTile.m_GameObject.transform.position);
		WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
		InitiativeTrack.Instance?.CheckRoundAbilityCardsOrLongRestSelected();
		if (!PlayersInValidStartingPositions)
		{
			Singleton<HelpBox>.Instance.Show("GUI_TOOLTIP_PLAYER_STARTING_PLACEMENT", "GUI_TOOLTIP_TITLE_START_TURN");
			readyButton.ShowWarning(delegate
			{
				m_RoomCameraButton.FocusOnEmptyRoom();
				Singleton<HelpBox>.Instance.HighlightWarning();
			});
			readyButton.SetInteractable(interactable: false);
		}
		else if (AnyPlayersInInvalidStartingPositionsForCompanionSummons)
		{
			ShowWarningWhenAnyPlayersInInvalidStartingPositionsForCompanionSummons();
			readyButton.ShowWarning(delegate
			{
				Singleton<HelpBox>.Instance.HighlightWarning();
			});
		}
		else
		{
			readyButton.HideWarning();
		}
	}

	private void ShowWarningWhenAnyPlayersInInvalidStartingPositionsForCompanionSummons()
	{
		List<CPlayerActor> list = PlayersInInvalidStartingPositionsForCompanionSummons();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			string value = LocalizationManager.GetTranslation(list[i].ActorLocKey()) + ((list.Count > 1 && i >= list.Count - 1) ? ", " : " ");
			stringBuilder.Append(value);
		}
		string text = string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_STARTING_PLACEMENT_COMPANION_SUMMONS"), stringBuilder);
		string translation = LocalizationManager.GetTranslation("GUI_TOOLTIP_TITLE_START_TURN");
		InitiativeTrack.Instance.helpBox.ShowTranslated(text, translation);
		Singleton<HelpBox>.Instance.HighlightWarning();
	}

	public bool CheckForDoors(CClientTile clientTile, List<CTile> optionalTileList = null)
	{
		if (LastMessage.m_Type == CMessageData.MessageType.ActorIsSelectingMoveTile && clientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) != null && !clientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door).Activated)
		{
			m_ConfirmationDoorTile = clientTile.m_Tile;
			m_ConfirmationDoorTileOptionalTileList = optionalTileList;
			return true;
		}
		return false;
	}

	public void ContinueTileSelection()
	{
		try
		{
			m_UndoButton.Toggle(active: false);
			m_SkipButton.Toggle(active: false);
			SetActiveSelectButton(activate: false);
			ScenarioRuleClient.TileSelected(m_ConfirmationDoorTile, m_ConfirmationDoorTileOptionalTileList, processImmediately: true);
			m_ConfirmDoorTileSelect.SetActive(value: false);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Choreographer.ContinueTileSelection().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00159", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void CancelTileSelection()
	{
		try
		{
			m_ConfirmDoorTileSelect.SetActive(value: false);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Choreographer.CancelTileSelection().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00160", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void DisableTileSelection(bool active)
	{
		m_TileSelectionDisabled = active;
		if (!active)
		{
			m_BufferedSelectedTile = null;
			m_BufferedTileNetworked = false;
			m_BufferedSecondClickToConfirm = false;
		}
	}

	public GameObject FindClientEnemy(CEnemyActor enemyActor)
	{
		return ClientMonsterObjects.Find((GameObject x) => ActorBehaviour.GetActor(x)?.ActorGuid == enemyActor?.ActorGuid);
	}

	public GameObject FindClientPlayer(CPlayerActor playerActor)
	{
		return m_ClientPlayers.Find((GameObject x) => ActorBehaviour.GetActor(x)?.ActorGuid == playerActor?.ActorGuid);
	}

	public GameObject FindClientHeroSummon(CHeroSummonActor heroSummonActor)
	{
		return m_ClientHeroSummons.Find((GameObject x) => ActorBehaviour.GetActor(x)?.ActorGuid == heroSummonActor?.ActorGuid);
	}

	public GameObject FindClientPlayer(CActor actor)
	{
		return m_ClientPlayers.Find((GameObject x) => ActorBehaviour.GetActor(x)?.ActorGuid == actor?.ActorGuid);
	}

	public GameObject FindClientActor(CActor actor)
	{
		return ClientActorObjects.Find((GameObject x) => ActorBehaviour.GetActor(x)?.ActorGuid == actor?.ActorGuid);
	}

	public GameObject FindClientObjectActor(CActor actor)
	{
		return m_ClientObjects.Find((GameObject x) => ActorBehaviour.GetActor(x)?.ActorGuid == actor?.ActorGuid);
	}

	public CActor FindPlayerActor(int actorID)
	{
		foreach (GameObject clientPlayer in m_ClientPlayers)
		{
			CActor actor = ActorBehaviour.GetActor(clientPlayer);
			if (actor.ID == actorID)
			{
				return actor;
			}
		}
		return null;
	}

	public GameObject FindClientActorGameObject(CActor actor, bool shouldReturnDummyActorsProp = false)
	{
		if (actor == null)
		{
			return null;
		}
		if (!(actor is CPlayerActor playerActor))
		{
			if (!(actor is CObjectActor cObjectActor))
			{
				if (!(actor is CEnemyActor enemyActor))
				{
					if (actor is CHeroSummonActor heroSummonActor)
					{
						return FindClientHeroSummon(heroSummonActor);
					}
					return null;
				}
				return FindClientEnemy(enemyActor);
			}
			if (shouldReturnDummyActorsProp && cObjectActor.AttachedProp != null)
			{
				return Singleton<ObjectCacheService>.Instance.GetPropObject(cObjectActor.AttachedProp);
			}
			return FindClientObjectActor(cObjectActor);
		}
		return FindClientPlayer(playerActor);
	}

	public void SetChoreographerState(ChoreographerStateType eState, int waitTickFrame, CActor waitActor)
	{
		ChoreographerStateType choreographerStateType = ChoreographerStateType.NA;
		if (m_WaitState != null)
		{
			choreographerStateType = m_WaitState.m_State;
		}
		m_WaitState = new CWaitState();
		m_WaitState.m_State = eState;
		m_WaitState.m_StateWaitTickFrame = Environment.TickCount + waitTickFrame;
		m_WaitState.m_StateWaitActor = waitActor;
		m_WaitState.m_StateWaitActorGO = ((waitActor != null) ? FindClientActorGameObject(waitActor) : null);
		if (m_WaitState.m_StateWaitActor != null && m_WaitState.m_StateWaitActorGO != null)
		{
			ActorEvents.GetActorEvents(m_WaitState.m_StateWaitActorGO).ClearActorEventState();
		}
		InitiativeTrack.Instance?.helpBox?.Hide();
		if (eState == ChoreographerStateType.Play && WorldspaceStarHexDisplay.Instance != null && WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets() <= 0 && choreographerStateType != ChoreographerStateType.WaitingForTileSelected)
		{
			WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
		}
		m_StateText.text = "State : " + m_WaitState.m_State;
		if (eState == ChoreographerStateType.WaitingForEndAbilityAnimSync)
		{
			m_EndAbilitySyncComplete = false;
		}
		if (eState == ChoreographerStateType.WaitingForEndTurnSync)
		{
			m_EndOfTurnSyncComplete = false;
			m_LoggedEndTurnSyncSoftlock = false;
		}
		if (eState == ChoreographerStateType.WaitingForEndRoundSync)
		{
			m_EndOfRoundSyncComplete = false;
			m_LoggedEndRoundSyncSoftlock = false;
		}
	}

	public void ClearAllActorEvents()
	{
		foreach (ActorEvents allActorEvent in m_AllActorEvents)
		{
			allActorEvent.ClearActorEventState();
		}
	}

	public void ClearMessageQueue()
	{
		lock (m_MessageQueue)
		{
			m_MessageQueue.Clear();
		}
	}

	public bool HasMessageOnQueue(CMessageData.MessageType messageType)
	{
		lock (m_MessageQueue)
		{
			return m_MessageQueue.Find((CMessageData x) => x.m_Type == messageType) != null;
		}
	}

	public void SetBlockClientMessageProcessing(bool active)
	{
		m_BlockClientMessageProcessing = active;
	}

	private void Update()
	{
		try
		{
			if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario || SceneController.Instance.GlobalErrorMessage.ShowingMessage || (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer == null) || UpdateIsBlocked)
			{
				return;
			}
			if (!InputManager.GamePadInUse && InputManager.GetWasPressed(KeyAction.SKIP_ATTACK))
			{
				SaveData.Instance.Global.SpeedUpToggle = !SaveData.Instance.Global.SpeedUpToggle;
			}
			if (!m_BlockClientMessageProcessing)
			{
				long num = DateTime.Now.Ticks / 10000;
				while (DateTime.Now.Ticks / 10000 - num < 8)
				{
					if (AutoTestController.s_AutoTestCurrentlyLoaded)
					{
						if (AutoTestController.s_ChoreographerPaused)
						{
							break;
						}
						lock (m_MessageQueue)
						{
							if (m_MessageQueue.Count > 0)
							{
								AutoTestController.ChoreographerMessageProcessed(m_MessageQueue[0].m_Type);
								m_CheckAutotestComplete = AutoTestController.s_AutoLogPlaybackInProgress;
							}
						}
					}
					CMessageData cMessageData = null;
					lock (m_MessageQueue)
					{
						if (m_MessageQueue.Count > 0)
						{
							cMessageData = m_MessageQueue[0];
							m_MessageQueue.RemoveAt(0);
						}
					}
					if (cMessageData != null)
					{
						ProcessMessage(cMessageData);
					}
					if (m_MessageQueue.Count == 0 || m_BlockClientMessageProcessing)
					{
						break;
					}
				}
			}
			if (previousState != m_WaitState.m_State)
			{
				previousState = m_WaitState.m_State;
			}
			switch (m_WaitState.m_State)
			{
			case ChoreographerStateType.WaitingForExhaustionClear:
				try
				{
				}
				catch (Exception ex)
				{
					Debug.LogError("An exception occurred while processing WaitingForExhaustionClear\n" + ex.Message + "\n" + ex.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00161", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
				}
				break;
			case ChoreographerStateType.WaitingForRewardsProcess:
				try
				{
					if (Environment.TickCount >= m_WaitState.m_StateWaitTickFrame && !Singleton<ScenarioRewardManager>.Instance.IsShown && m_ShouldShowRewards)
					{
						Debug.Log("Starting rewards showcase at " + Timekeeper.instance.m_GlobalClock.time);
						m_ShouldShowRewards = false;
						SaveData.Instance.Global.StopSpeedUp();
						Singleton<ScenarioRewardManager>.Instance.Show(CurrentActor, m_RewardsToShowcase, delegate
						{
							Debug.Log("Continuing after rewards showcase at " + Timekeeper.instance.m_GlobalClock.time);
							ScenarioRuleClient.ToggleMessageProcessing(process: true);
							m_BlockClientMessageProcessing = false;
						});
					}
				}
				catch (Exception ex19)
				{
					Debug.LogError("An exception occurred while processing WaitingForRewardsProcess\n" + ex19.Message + "\n" + ex19.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00162", "GUI_ERROR_MAIN_MENU_BUTTON", ex19.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex19.Message);
				}
				break;
			case ChoreographerStateType.WaitingForPlayerPushWaypointSelection:
				try
				{
					SaveData.Instance.Global.StopSpeedUp();
				}
				catch (Exception ex18)
				{
					Debug.LogError("An exception occurred while processing WaitingForPlayerPushWaypointSelection\n" + ex18.Message + "\n" + ex18.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00163", "GUI_ERROR_MAIN_MENU_BUTTON", ex18.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex18.Message);
				}
				break;
			case ChoreographerStateType.WaitingForPlayerPullWaypointSelection:
				try
				{
					SaveData.Instance.Global.StopSpeedUp();
				}
				catch (Exception ex17)
				{
					Debug.LogError("An exception occurred while processing WaitingForPlayerPullWaypointSelection\n" + ex17.Message + "\n" + ex17.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00164", "GUI_ERROR_MAIN_MENU_BUTTON", ex17.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex17.Message);
				}
				break;
			case ChoreographerStateType.WaitingForPlayerWaypointSelection:
				try
				{
					SaveData.Instance.Global.StopSpeedUp();
				}
				catch (Exception ex16)
				{
					Debug.LogError("An exception occurred while processing WaitingForPlayerWaypointSelection\n" + ex16.Message + "\n" + ex16.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00165", "GUI_ERROR_MAIN_MENU_BUTTON", ex16.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex16.Message);
				}
				break;
			case ChoreographerStateType.WaitingForCardSelection:
				try
				{
					SaveData.Instance.Global.StopSpeedUp();
				}
				catch (Exception ex15)
				{
					Debug.LogError("An exception occurred while processing WaitingForCardSelection\n" + ex15.Message + "\n" + ex15.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00166", "GUI_ERROR_MAIN_MENU_BUTTON", ex15.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex15.Message);
				}
				break;
			case ChoreographerStateType.WaitingForMoveAnim:
				try
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Animation);
					SaveData.Instance.Global.StartSpeedUp();
					if (ScenarioManager.Scenario.HasActor(m_WaitState.m_StateWaitActor))
					{
						GameObject gameObject3 = FindClientActorGameObject(m_WaitState.m_StateWaitActor);
						if (m_WaitState.m_StateWaitTickFrame < Environment.TickCount || ActorBehaviour.GetActorBehaviour(gameObject3).AtLocoTarget())
						{
							if (!ActorBehaviour.GetActorBehaviour(gameObject3).AtLocoTarget())
							{
								ActorBehaviour.GetActorBehaviour(gameObject3).TeleportToCurrentLocoTarget();
								SimpleLog.AddToSimpleLog("WaitingForMoveAnim Choreographer state moved because tick time ran out - teleporting to loco target.");
							}
							else
							{
								SimpleLog.AddToSimpleLog("WaitingForMoveAnim Choreographer state moved because actor reached the target position.");
							}
							SetChoreographerState(ChoreographerStateType.Play, 0, null);
							ScenarioRuleClient.StepComplete();
						}
					}
					else
					{
						SetChoreographerState(ChoreographerStateType.Play, 0, null);
						SimpleLog.AddToSimpleLog("WaitingForMoveAnim Choreographer state moved on because actor is dead.");
					}
				}
				catch (Exception ex14)
				{
					Debug.LogError("An exception occurred while processing WaitingForMoveAnim\n" + ex14.Message + "\n" + ex14.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00167", "GUI_ERROR_MAIN_MENU_BUTTON", ex14.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex14.Message);
				}
				break;
			case ChoreographerStateType.WaitingForAreaAttackFocusSelection:
				try
				{
					SaveData.Instance.Global.StopSpeedUp();
				}
				catch (Exception ex13)
				{
					Debug.LogError("An exception occurred while processing WaitingForAreaAttackFocusSelection\n" + ex13.Message + "\n" + ex13.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00168", "GUI_ERROR_MAIN_MENU_BUTTON", ex13.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex13.Message);
				}
				break;
			case ChoreographerStateType.WaitingForAttackModifierCards:
				try
				{
					SaveData.Instance.Global.StopSpeedUp();
					if (m_WaitState.m_StateWaitTickFrame < Environment.TickCount)
					{
						if (m_CurrentActorAttackModifierCards.Count > 0)
						{
							m_WaitState.m_StateWaitTickFrame = Environment.TickCount + 750;
							m_CurrentActorAttackModifierCards.Remove(m_CurrentActorAttackModifierCards[0]);
						}
						else
						{
							GUIInterface.s_GUIInterface.m_AttackModifierCardGUI.SetActive(active: false);
							SetChoreographerState(ChoreographerStateType.Play, 0, null);
							ScenarioRuleClient.StepComplete();
						}
					}
				}
				catch (Exception ex12)
				{
					Debug.LogError("An exception occurred while processing WaitingForAttackModifierCards\n" + ex12.Message + "\n" + ex12.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00169", "GUI_ERROR_MAIN_MENU_BUTTON", ex12.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex12.Message);
				}
				break;
			case ChoreographerStateType.WaitingForAttackAnim:
				try
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Animation);
					SaveData.Instance.Global.StartSpeedUp();
					GameObject gameObject2 = null;
					if (m_CurrentOnDeathActor != null)
					{
						gameObject2 = FindClientActorGameObject(m_CurrentOnDeathActor);
					}
					if (gameObject2 == null)
					{
						m_CurrentOnDeathActor = null;
						gameObject2 = m_WaitState.m_StateWaitActorGO;
					}
					bool flag15 = false;
					ActorEvents actorEvents2 = ActorEvents.GetActorEvents(gameObject2);
					if (m_SMB_Control_WaitingForAttackAnim && actorEvents2 != null && actorEvents2.ReceivedEventThenClear(ActorEvents.ActorEvent.ProgressChoreographer))
					{
						flag15 = true;
					}
					else if ((!m_SMB_Control_WaitingForAttackAnim && m_WaitState.m_StateWaitTickFrame < Environment.TickCount) || (actorEvents2 != null && actorEvents2.ReceivedEventThenClear(ActorEvents.ActorEvent.ProgressChoreographer)))
					{
						flag15 = true;
					}
					if (flag15)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForModifierDrawAnim, 0, null);
					}
				}
				catch (Exception ex11)
				{
					Debug.LogError("An exception occurred while processing WaitingForAttackAnim\n" + ex11.Message + "\n" + ex11.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00170", "GUI_ERROR_MAIN_MENU_BUTTON", ex11.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex11.Message);
				}
				break;
			case ChoreographerStateType.WaitingForModifierDrawAnim:
				try
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Animation);
					SaveData.Instance.Global.StopSpeedUp();
					bool flag14 = true;
					List<GameObject> list = new List<GameObject>();
					foreach (GameObject clientActorObject in ClientActorObjects)
					{
						if (ActorEvents.GetActorEvents(clientActorObject).ReceivedEvent(ActorEvents.ActorEvent.StartDrawingModifiers))
						{
							if (!ActorEvents.GetActorEvents(clientActorObject).ReceivedEvent(ActorEvents.ActorEvent.FinishedDrawingModifiers))
							{
								flag14 = false;
								break;
							}
							list.Add(clientActorObject);
						}
					}
					if (!flag14)
					{
						break;
					}
					SaveData.Instance.Global.StartSpeedUp();
					list.ForEach(delegate(GameObject it)
					{
						if (ActorBehaviour.GetActorBehaviour(it).m_WorldspacePanelUI.isActiveAndEnabled)
						{
							ActorBehaviour.GetActorBehaviour(it).m_WorldspacePanelUI.DisplayAttackModifierDamageFlow();
						}
						else
						{
							Debug.LogWarning("Attempted to display attack modifier damage flow but worldspace panel UI was inactive");
						}
						ActorEvents.GetActorEvents(it).ReceivedEventThenClear(ActorEvents.ActorEvent.StartDrawingModifiers);
						ActorEvents.GetActorEvents(it).ReceivedEventThenClear(ActorEvents.ActorEvent.FinishedDrawingModifiers);
					});
					SetChoreographerState(ChoreographerStateType.Play, 0, null);
					ScenarioRuleClient.StepComplete();
				}
				catch (Exception ex10)
				{
					Debug.LogError("An exception occurred while processing WaitingForAttackAnim\n" + ex10.Message + "\n" + ex10.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00171", "GUI_ERROR_MAIN_MENU_BUTTON", ex10.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex10.Message);
				}
				break;
			case ChoreographerStateType.WaitingForDamageAnim:
				try
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Animation);
					SaveData.Instance.Global.StartSpeedUp();
					GameObject gameObject = FindClientActorGameObject(m_CurrentActor);
					bool flag13 = false;
					if (gameObject == null || !MF.GetGameObjectAnimatorController(gameObject) || m_WaitState.m_StateWaitTickFrame < Environment.TickCount || ActorEvents.GetActorEvents(gameObject).ReceivedEventThenClear(ActorEvents.ActorEvent.ProgressChoreographer))
					{
						flag13 = true;
					}
					if (flag13)
					{
						SetChoreographerState(ChoreographerStateType.Play, 0, null);
						ScenarioRuleClient.StepComplete();
					}
				}
				catch (Exception ex9)
				{
					Debug.LogError("An exception occurred while processing WaitingForDamageAnim\n" + ex9.Message + "\n" + ex9.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00172", "GUI_ERROR_MAIN_MENU_BUTTON", ex9.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex9.Message);
				}
				break;
			case ChoreographerStateType.WaitingForGeneralAnim:
				try
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Animation);
					SaveData.Instance.Global.StartSpeedUp();
					ActorEvents actorEvents = null;
					if (m_WaitState.m_StateWaitActorGO != null)
					{
						actorEvents = ActorEvents.GetActorEvents(m_WaitState.m_StateWaitActorGO);
					}
					if (m_WaitState.m_StateWaitTickFrame < Environment.TickCount || m_WaitState.m_StateWaitActorGO == null || !MF.GetGameObjectAnimatorController(m_WaitState.m_StateWaitActorGO) || (actorEvents != null && actorEvents.ReceivedEventThenClear(ActorEvents.ActorEvent.ProgressChoreographer)))
					{
						SetChoreographerState(ChoreographerStateType.Play, 0, null);
						ScenarioRuleClient.StepComplete();
						if (m_WaitState.m_StateWaitTickFrame < Environment.TickCount)
						{
							Debug.LogWarning("WaitingForGeneralAnim timeout on :" + ((m_WaitState.m_StateWaitActor != null) ? m_WaitState.m_StateWaitActor.Class.ID : "null"));
						}
					}
				}
				catch (Exception ex8)
				{
					Debug.LogError("An exception occurred while processing WaitingForGeneralAnim\n" + ex8.Message + "\n" + ex8.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00173", "GUI_ERROR_MAIN_MENU_BUTTON", ex8.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex8.Message);
				}
				break;
			case ChoreographerStateType.WaitingForElementPicked:
				try
				{
					SaveData.Instance.Global.StopSpeedUp();
					SetChoreographerState(ChoreographerStateType.Play, 0, null);
					ScenarioRuleClient.StepComplete();
					if (m_WaitState.m_StateWaitTickFrame < Environment.TickCount)
					{
						Debug.LogWarning("WaitingForGeneralAnim timeout on :" + ((m_WaitState.m_StateWaitActor != null) ? m_WaitState.m_StateWaitActor.Class.ID : "null"));
					}
				}
				catch (Exception ex7)
				{
					Debug.LogError("An exception occurred while processing WaitingForGeneralAnim\n" + ex7.Message + "\n" + ex7.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00173", "GUI_ERROR_MAIN_MENU_BUTTON", ex7.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex7.Message);
				}
				break;
			case ChoreographerStateType.WaitingForProgressChoreographer:
				try
				{
					SaveData.Instance.Global.StartSpeedUp();
					if (m_WaitState.m_StateWaitTickFrame < Environment.TickCount || m_WaitState.m_StateWaitActorGO == null || ActorEvents.GetActorEvents(m_WaitState.m_StateWaitActorGO) == null || ActorEvents.GetActorEvents(m_WaitState.m_StateWaitActorGO).ReceivedEventThenClear(ActorEvents.ActorEvent.ProgressChoreographer) || IdleStates.Any((string x) => MF.GameObjectAnimatorControllerIsCurrentState(m_WaitState.m_StateWaitActorGO, x)))
					{
						SetChoreographerState(ChoreographerStateType.Play, 0, null);
						ScenarioRuleClient.StepComplete();
						if (m_WaitState.m_StateWaitTickFrame < Environment.TickCount)
						{
							Debug.LogWarning("WaitingForProgressChoreographer timeout on :" + ((m_WaitState.m_StateWaitActor != null) ? m_WaitState.m_StateWaitActor.Class.ID : "null"));
						}
					}
				}
				catch (Exception ex6)
				{
					Debug.LogError("An exception occurred while processing WaitingForProgressChoreographer\n" + ex6.Message + "\n" + ex6.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00174", "GUI_ERROR_MAIN_MENU_BUTTON", ex6.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex6.Message);
				}
				break;
			case ChoreographerStateType.WaitingForPlayerIdle:
				try
				{
					SaveData.Instance.Global.StartSpeedUp();
					if (!ScenarioManager.Scenario.HasActor(m_WaitState.m_StateWaitActor) || (!ActorBehaviour.GetActorBehaviour(m_WaitState.m_StateWaitActorGO).m_WorldspacePanelUI.FlowControlActive() && IdleStates.Any((string x) => MF.GameObjectAnimatorControllerIsCurrentState(m_WaitState.m_StateWaitActorGO, x))))
					{
						SetChoreographerState(ChoreographerStateType.Play, 0, null);
						ScenarioRuleClient.StepComplete();
						SaveData.Instance.Global.StopSpeedUp();
					}
				}
				catch (Exception ex5)
				{
					Debug.LogError("An exception occurred while processing WaitingForPlayerIdle\n" + ex5.Message + "\n" + ex5.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00175", "GUI_ERROR_MAIN_MENU_BUTTON", ex5.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex5.Message);
				}
				break;
			case ChoreographerStateType.WaitingForEndAbilityAnimSync:
				try
				{
					if (m_EndAbilitySyncComplete)
					{
						break;
					}
					SaveData.Instance.Global.StartSpeedUp();
					if (!Singleton<PhaseBannerHandler>.Instance.IsShowingDeath() && (!ScenarioManager.Scenario.HasActor(m_WaitState.m_StateWaitActor) || !MF.GetGameObjectAnimatorController(m_WaitState.m_StateWaitActorGO) || (m_WaitState.m_StateWaitTickFrame < Environment.TickCount && !MF.GameObjectAnimatorInTransition(m_WaitState.m_StateWaitActorGO)) || IdleStates.Any((string x) => MF.GameObjectAnimatorControllerIsCurrentState(m_WaitState.m_StateWaitActorGO, x))) && !ScenarioRuleClient.IsProcessingOrMessagesQueued && m_MessageQueue.Count == 0 && !m_PlayerSelectingToAvoidDamageOrNot)
					{
						m_EndAbilitySyncComplete = true;
						if (FFSNetwork.IsOnline)
						{
							EndOfAbilityReachedLocally();
						}
						else
						{
							AbilityEndSyncedAndReadyToProceed();
						}
					}
				}
				catch (Exception ex4)
				{
					Debug.LogError("An exception occurred while processing WaitingForEndAbilityAnimSync\n" + ex4.Message + "\n" + ex4.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00176", "GUI_ERROR_MAIN_MENU_BUTTON", ex4.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex4.Message);
				}
				break;
			case ChoreographerStateType.WaitingForEndTurnSync:
				try
				{
					if (m_EndOfTurnSyncComplete)
					{
						break;
					}
					bool flag6 = !m_LoggedEndTurnSyncSoftlock && Environment.TickCount > m_WaitState.m_StateWaitTickFrame + 30000;
					SaveData.Instance.Global.StopSpeedUp();
					bool flag7 = false;
					FieldInfo[] fields = GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GetType().GetFields();
					for (int i = 0; i < fields.Length; i++)
					{
						GameObject prefab = fields[i].GetValue(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects) as GameObject;
						if (!ObjectPool.SpawnedContains(prefab))
						{
							continue;
						}
						foreach (GameObject match in ObjectPool.GetMatches(prefab))
						{
							if (match != null && match.activeSelf)
							{
								ParticleSystem component = match.GetComponent<ParticleSystem>();
								if (component != null && component.IsAlive(withChildren: false))
								{
									flag7 = true;
								}
							}
						}
					}
					bool flag8 = false;
					if (DoorsUnlocking != null && DoorsUnlocking.Count > 0)
					{
						flag8 = true;
					}
					bool flag9 = false;
					if (DelayedDropSMB.DelayedDropsAreInProgress())
					{
						flag9 = true;
					}
					bool flag10 = ScenarioRuleClient.MessageQueueLength != 0;
					bool flag11 = m_MessageQueue.Count != 0;
					bool flag12 = true;
					foreach (GameObject actorGO in ClientActorObjects)
					{
						CActor actor = ActorBehaviour.GetActor(actorGO);
						if (ScenarioManager.Scenario.HasActor(actor) && (ActorBehaviour.GetActorBehaviour(actorGO).m_WorldspacePanelUI.FlowControlActive() || !IdleStates.Any((string x) => MF.GameObjectAnimatorControllerIsCurrentState(actorGO, x))))
						{
							flag12 = false;
							break;
						}
					}
					bool isBusy2 = LevelEventsController.s_Instance.IsBusy;
					if (!flag7 && !flag8 && !flag9 && !flag10 && !flag11 && flag12 && !isBusy2)
					{
						m_EndOfTurnSyncComplete = true;
						if (m_LoggedEndTurnSyncSoftlock)
						{
							SimpleLog.AddToSimpleLog($"[END TURN SYNC] UNstuck from WaitingForEndTurnSync after {Timekeeper.instance.m_GlobalClock.unscaledTime - m_TimeOfLoggedEndOfTurnSoftLock:F2}s");
						}
						if (FFSNetwork.IsOnline)
						{
							EndOfTurnReachedLocally();
						}
						else
						{
							TurnEndSyncedAndReadyToProceed();
						}
					}
					else if (flag6)
					{
						m_LoggedEndTurnSyncSoftlock = true;
						m_TimeOfLoggedEndOfTurnSoftLock = Timekeeper.instance.m_GlobalClock.unscaledTime;
						string text2 = "[END TURN SYNC] Stuck at WaitingForEndTurnSync: ";
						if (flag7)
						{
							text2 += "\n[END TURN SYNC] Active bonus buff target effects are still active";
						}
						if (flag8)
						{
							text2 += "\n[END TURN SYNC] Doors are still trying to unlock";
						}
						if (flag9)
						{
							text2 += "\n[END TURN SYNC] Gold is still waiting to be dropped";
						}
						if (flag10)
						{
							text2 += "\n[END TURN SYNC] SRL Message Queue not empty.";
						}
						if (flag11)
						{
							text2 += "\n[END TURN SYNC] Client Message Queue not empty.";
						}
						if (!flag12)
						{
							text2 += "\n[END TURN SYNC] Not all actors are ready to proceed (Flow control is active or not in Idle state).";
						}
						if (isBusy2)
						{
							text2 += "\n[END TURN SYNC] LevelEventsController is still processing messages";
						}
						SimpleLog.AddToSimpleLog(text2);
					}
				}
				catch (Exception ex3)
				{
					Debug.LogError("An exception occurred while processing WaitingForEndTurnSync\n" + ex3.Message + "\n" + ex3.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00177", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex3.Message);
				}
				break;
			case ChoreographerStateType.WaitingForEndRoundSync:
				try
				{
					if (m_EndOfRoundSyncComplete)
					{
						break;
					}
					bool flag = !m_LoggedEndRoundSyncSoftlock && Environment.TickCount > m_WaitState.m_StateWaitTickFrame + 30000;
					bool flag2 = false;
					if (DoorsUnlocking != null && DoorsUnlocking.Count > 0)
					{
						flag2 = true;
					}
					bool flag3 = false;
					if (DelayedDropSMB.DelayedDropsAreInProgress())
					{
						flag3 = true;
					}
					bool flag4 = ScenarioRuleClient.MessageQueueLength != 0;
					bool flag5 = m_MessageQueue.Count != 0;
					bool isBusy = LevelEventsController.s_Instance.IsBusy;
					if (!flag2 && !flag3 && !flag4 && !flag5 && !isBusy)
					{
						m_EndOfRoundSyncComplete = true;
						if (m_LoggedEndRoundSyncSoftlock)
						{
							SimpleLog.AddToSimpleLog($"[END ROUND SYNC] UNstuck from WaitingForEndRoundSync after {Timekeeper.instance.m_GlobalClock.unscaledTime - m_TimeOfLoggedEndOfRoundSoftLock:F2}s");
						}
						if (FFSNetwork.IsOnline)
						{
							EndOfRoundReachedLocally();
						}
						else
						{
							RoundEndSyncedAndReadyToProceed();
						}
					}
					else if (flag)
					{
						m_LoggedEndRoundSyncSoftlock = true;
						m_TimeOfLoggedEndOfRoundSoftLock = Timekeeper.instance.m_GlobalClock.unscaledTime;
						string text = "[END ROUND SYNC] Stuck at WaitingForEndRoundSync: ";
						if (isBusy)
						{
							text += "\n[END ROUND SYNC] LevelEventsController is still processing messages";
						}
						if (flag2)
						{
							text += "\n[END ROUND SYNC] Doors are still trying to unlock";
						}
						if (flag3)
						{
							text += "\n[END ROUND SYNC] Gold is still waiting to be dropped";
						}
						if (flag4)
						{
							text += "\n[END ROUND SYNC] SRL Message Queue not empty.";
						}
						if (flag5)
						{
							text += "\n[END ROUND SYNC] Client Message Queue not empty.";
						}
						SimpleLog.AddToSimpleLog(text);
					}
				}
				catch (Exception ex2)
				{
					Debug.LogError("An exception occurred while processing WaitingForEndRoundSync\n" + ex2.Message + "\n" + ex2.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00177", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
				}
				break;
			case ChoreographerStateType.WaitingForAutosave:
				LogUtils.Log("[Choreographer.cs] Update() AutoSave is in progress...");
				break;
			case ChoreographerStateType.WaitingForTileSelected:
				SaveData.Instance.Global.StopSpeedUp();
				break;
			case ChoreographerStateType.WaitingForDelayedDrops:
				SaveData.Instance.Global.StartSpeedUp();
				if (!DelayedDropSMB.DelayedDropsAreInProgress())
				{
					SetChoreographerState(ChoreographerStateType.Play, 0, null);
					ScenarioRuleClient.StepComplete();
					SaveData.Instance.Global.StopSpeedUp();
				}
				break;
			}
			m_Phase.text = "PHASE : " + PhaseManager.PhaseType;
			if (!FFSNetwork.IsOnline)
			{
				return;
			}
			if (PlayerRegistry.AllPlayers.Find((NetworkPlayer x) => !x.IsParticipant) != null)
			{
				if (Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown)
				{
					Singleton<UIReadyToggle>.Instance?.SetInteractable(interactable: false);
				}
			}
			else if (Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown)
			{
				InitiativeTrack.Instance?.CheckRoundAbilityCardsOrLongRestSelected();
			}
		}
		catch (Exception ex20)
		{
			Debug.LogError("An exception occurred within the Choreographer Update loop!\n" + ex20.Message + "\n" + ex20.StackTrace);
		}
	}

	public void SwitchToSelectActionUiState(CPlayerActor actor)
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectAction);
		if (!s_Choreographer.ThisPlayerHasTurnControl)
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.HexMovement);
		}
	}

	public void CheckWaitingForAutosave(bool hideProgress = true)
	{
		try
		{
			SaveData.Instance.Global.StopSpeedUp();
			if (m_WaitState.m_StateWaitTickFrame >= Environment.TickCount && (SaveData.Instance.IsSavingData || SaveData.Instance.IsSavingThreadActive))
			{
				return;
			}
			if (m_QueuedMPStateCompare)
			{
				SaveData.Instance.IsSavingData = true;
				StartMPEndOfRoundCompare();
				m_QueuedMPStateCompare = false;
				return;
			}
			if (hideProgress && Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
			ChoreographerStateType state = m_WaitState.m_State;
			LogUtils.Log("[CheckWaitingForAutosave] Autosave is in progress... Choreographer is in " + state.ToString() + " state...");
			if (state == ChoreographerStateType.WaitingForAutosave)
			{
				LogUtils.Log("[CheckWaitingForAutosave] AutoSave is in progress... Will try now to resume the game...");
				SetChoreographerState(ChoreographerStateType.Play, 0, null);
				ScenarioRuleClient.NextPhase();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while processing WaitingForAutosave\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00181", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnGameSpeedChanged()
	{
		foreach (GameObject item in AllActorsInScene)
		{
			SetAnimSpeed(item);
		}
	}

	private void SetAnimSpeed(GameObject actor)
	{
		if (!(actor == null))
		{
			Animator animator = actor.GetComponent<Animator>();
			if (animator == null)
			{
				animator = actor.GetComponentInChildren<Animator>();
			}
			if (animator != null)
			{
				animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
			}
		}
	}

	public List<CPlayerActor> PlayersInInvalidStartingPositionsForCompanionSummons()
	{
		List<CPlayerActor> list = new List<CPlayerActor>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (playerActor.CharacterClass.CompanionSummonData == null)
			{
				continue;
			}
			List<CTile> allAdjacentTiles = ScenarioManager.GetAllAdjacentTiles(ScenarioManager.Tiles[playerActor.ArrayIndex.X, playerActor.ArrayIndex.Y]);
			bool flag = false;
			foreach (CTile item in allAdjacentTiles)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				if (cNode.Walkable && !cNode.Blocked && CAbilityFilter.IsValidTile(item, CAbilityFilter.EFilterTile.EmptyHex))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(playerActor);
			}
		}
		return list;
	}

	public bool WillMovingPlayerToTileMakeStartingPositionsValid(CClientTile tileToMoveTo)
	{
		int num = Mathf.RoundToInt((float)m_ClientPlayers.Count / (float)ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles.Count);
		foreach (List<CClientTile> possibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles)
		{
			int num2 = 0;
			foreach (CClientTile item in possibleStartingTile)
			{
				if (item != tileToMoveTo)
				{
					CPlayerActor cPlayerActor = ScenarioManager.Scenario.FindPlayerAt(item.m_Tile.m_ArrayIndex);
					if (cPlayerActor != null && cPlayerActor != InitiativeTrack.Instance.SelectedActor().Actor)
					{
						num2++;
					}
				}
				else
				{
					num2++;
				}
			}
			if (num2 > num)
			{
				return false;
			}
		}
		return true;
	}

	public void SetActiveSelectButton(bool activate)
	{
		if (m_selectButton != null)
		{
			m_selectButton.SetActive(activate);
		}
	}

	public void ShowGameGUI()
	{
		m_GameGUILevel.alpha = 1f;
	}

	public void HideGameGUI()
	{
		m_GameGUILevel.alpha = 0f;
	}

	public void SwitchSkipAndUndoButtons(bool isSwitch)
	{
		RectTransform rectTransform = m_UndoButton.transform as RectTransform;
		RectTransform rectTransform2 = m_SkipButton.transform as RectTransform;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		Vector2 anchoredPosition2 = rectTransform2.anchoredPosition;
		if (isSwitch)
		{
			rectTransform.anchoredPosition = new Vector2(-250f, anchoredPosition.y);
			rectTransform2.anchoredPosition = new Vector2(250f, anchoredPosition2.y);
		}
		else
		{
			rectTransform.anchoredPosition = new Vector2(250f, anchoredPosition.y);
			rectTransform2.anchoredPosition = new Vector2(-250f, anchoredPosition2.y);
		}
	}

	public bool ActorOrHisSummonerIsUnderMyControl(CActor actor)
	{
		if (actor.IsUnderMyControl)
		{
			return true;
		}
		if (actor.Type != actor.OriginalType && GameState.OverridingCurrentActor && GameState.OverridenActionActorStack.Count > 0 && GameState.OverridenActionActorStack.Peek().ControllingActor != null)
		{
			return GameState.OverridenActionActorStack.Peek().ControllingActor.IsUnderMyControl;
		}
		if (actor is CHeroSummonActor cHeroSummonActor)
		{
			return cHeroSummonActor.Summoner.IsUnderMyControl;
		}
		if (Singleton<TakeDamagePanel>.Instance.IsOpen)
		{
			return Singleton<TakeDamagePanel>.Instance.ThisPlayerHasTakeDamageControl;
		}
		return false;
	}

	public void ProxyUndoAction(GameAction action, ref bool executionFinished)
	{
		FFSNet.Console.LogInfo("PROXY: Undoing action.");
		m_UndoButton.OnClickInternal(networkActionIfOnline: false, action);
	}

	public void ProxyConfirmAction()
	{
		FFSNet.Console.LogInfo("PROXY: Confirming action.");
		readyButton.OnClickInternal(networkActionIfOnline: false);
	}

	public void ProxySkipAction()
	{
		FFSNet.Console.LogInfo("PROXY: Skipping action.");
		m_SkipButton.OnClick(networkActionIfOnline: false);
	}

	public void ProxySelectAbilityTarget(GameAction action, bool deselectInstead, ref bool executionFinished)
	{
		executionFinished = false;
		ActionProcessor.IsWaitingForTileSelectionFinishedMessage = action;
		FFSNet.Console.LogInfo("PROXY: " + (deselectInstead ? "Deselecting" : "Selecting") + " an ability target.");
		TargetSelectionToken targetSelectionToken = (TargetSelectionToken)action.SupplementaryDataToken;
		int num = targetSelectionToken.Tiles[0, 0];
		int num2 = targetSelectionToken.Tiles[0, 1];
		FFSNet.Console.LogInfo("X: " + num + ", Y: " + num2);
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[num, num2];
		List<CTile> list = null;
		int length = targetSelectionToken.Tiles.GetLength(0);
		if (length > 1)
		{
			list = new List<CTile>();
			FFSNet.Console.LogInfo("Adding " + (length - 1) + " optional tiles:");
			for (int i = 1; i < targetSelectionToken.Tiles.GetLength(0); i++)
			{
				int num3 = targetSelectionToken.Tiles[i, 0];
				int num4 = targetSelectionToken.Tiles[i, 1];
				list.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[num3, num4].m_Tile);
				FFSNet.Console.LogInfo("X: " + num3 + ", Y: " + num4);
			}
		}
		if (!deselectInstead)
		{
			WorldspaceStarHexDisplay.Instance.ProxyUpdateSelectionHexes(cClientTile, targetSelectionToken.TargetingAngle, targetSelectionToken.AoeLocked);
		}
		TileHandler(cClientTile, list, networkActionIfOnline: false, isUserClick: false, targetSelectionToken.SecondClickToConfirmEnabled);
	}

	public void ServerTryAndPlaceCharacterAtRoundStart(GameAction action, ref bool actionValid, ref bool executionFinished)
	{
		FFSNet.Console.LogInfo("Validating starting tile for character with ControllableID: " + action.ActorID);
		TileToken tileToken = (TileToken)action.SupplementaryDataToken;
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileToken.TileX, tileToken.TileY];
		if (FFSNetwork.IsHost && ScenarioManager.Scenario.FindActorAt(cClientTile.m_Tile.m_ArrayIndex) != null)
		{
			FFSNet.Console.LogWarning("Cannot place character. Tile already occupied.");
			actionValid = false;
			executionFinished = true;
			return;
		}
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			string iD = ((CPlayerActor)cActor).CharacterClass.ID;
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(cActor));
			if (actorBehaviour != null)
			{
				actionValid = true;
				PlaceActorAtRoundStart(cActor, cClientTile);
				if (!actorBehaviour.AtLocoIntermediateTarget())
				{
					executionFinished = false;
					Timing.RunCoroutine(WaitUntilCharacterPlaced(action, actorBehaviour));
				}
				else
				{
					FFSNet.Console.LogInfo(iD + " successfully placed.");
					executionFinished = true;
				}
			}
			else
			{
				actionValid = false;
				executionFinished = true;
				ThrowCharacterPlacementError(iD, "ActorBehaviour returns null.");
			}
		}
		else
		{
			actionValid = false;
			executionFinished = true;
			ThrowCharacterPlacementError("NULL CHARACTER", "CActor returns null (ControllableID: " + action.ActorID + ").");
		}
	}

	private IEnumerator<float> WaitUntilCharacterPlaced(GameAction action, ActorBehaviour actorBehaviour)
	{
		while (!actorBehaviour.AtLocoIntermediateTarget())
		{
			yield return 0f;
		}
		string text = null;
		if (AdventureState.MapState.IsCampaign)
		{
			CMapCharacter mapCharacterWithCharacterNameHash = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(action.ActorID);
			if (mapCharacterWithCharacterNameHash != null)
			{
				text = mapCharacterWithCharacterNameHash.CharacterID;
			}
		}
		else
		{
			text = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
		}
		FFSNet.Console.LogInfo(text + " successfully placed.");
		ActionProcessor.FinishProcessingAction(action);
	}

	private void ThrowCharacterPlacementError(string characterID, string specificErrorText = "")
	{
		if (FFSNetwork.IsClient)
		{
			throw new Exception("Error placing " + characterID + ". " + specificErrorText);
		}
		FFSNet.Console.LogError("ERROR_MULTIPLAYER_00001", "Error placing character " + characterID + ". " + specificErrorText);
	}

	public void ProxyUpdateCharacterStartingTile(IGHControllableState controllableState)
	{
		CharacterManager characterManager = (CharacterManager)(ControllableRegistry.GetControllable(controllableState.ControllableID)?.ControllableObject);
		CActor cActor = null;
		if (characterManager != null)
		{
			cActor = characterManager.CharacterActor;
		}
		if (cActor != null)
		{
			string iD = ((CPlayerActor)cActor).CharacterClass.ID;
			FFSNet.Console.LogInfo("Updating starting tile for " + iD);
			TileToken tileToken = (TileToken)controllableState.StartingTile;
			if (tileToken != null)
			{
				FFSNet.Console.LogInfo("Updating starting tile to X:" + tileToken.TileX + " Y: " + tileToken.TileY);
				if (tileToken.TileX >= ClientScenarioManager.s_ClientScenarioManager.ClientTileArray.GetLength(0))
				{
					Debug.LogError("TileX is greater than array bounds.  TileX: " + tileToken.TileX + " ArrayBounds: " + ClientScenarioManager.s_ClientScenarioManager.ClientTileArray.GetLength(0));
				}
				else if (tileToken.TileY >= ClientScenarioManager.s_ClientScenarioManager.ClientTileArray.GetLength(1))
				{
					Debug.LogError("TileY is greater than array bounds.  TileY: " + tileToken.TileY + " ArrayBounds: " + ClientScenarioManager.s_ClientScenarioManager.ClientTileArray.GetLength(1));
				}
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileToken.TileX, tileToken.TileY];
				if (cClientTile != null)
				{
					PlaceActorAtRoundStart(cActor, cClientTile);
					FFSNet.Console.LogInfo("Starting tile (X: " + tileToken.TileX + ", Y: " + tileToken.TileY + ") chosen for " + iD);
				}
				else
				{
					FFSNet.Console.LogError(string.Empty, "Error placing " + iD + ". ClientTile returns null.");
				}
			}
			else
			{
				FFSNet.Console.LogError(string.Empty, "Error placing " + iD + ". TileToken returns null.");
			}
		}
		else
		{
			ThrowCharacterPlacementError("NULL CHARACTER", "CActor returns null (ControllableID: " + controllableState.ControllableID + ").");
		}
	}

	public void ApplySingleTargetEffect(GameAction action)
	{
		if (m_CurrentAbility != null)
		{
			CActor cActor = m_CurrentAbility.ActorsToTarget.SingleOrDefault((CActor x) => x.Type == (CActor.EType)action.SupplementaryDataIDMin && x.ID == action.ActorID && x.Class.ID == action.ClassID);
			if (cActor != null)
			{
				if (m_CurrentAbility.IsWaitingForSingleTargetItem())
				{
					ScenarioRuleClient.ApplySingleTarget(cActor);
				}
				else if (m_CurrentAbility.IsWaitingForSingleTargetActiveBonus())
				{
					ScenarioRuleClient.ApplySingleTarget(cActor);
				}
				return;
			}
			throw new Exception("Error applying single target item for proxy playerActor. Target actor returns null.");
		}
		throw new Exception("Error applying active ability for proxy playerActor. Current ability returns null.");
	}

	private void NetworkTargetSelection(CClientTile clientTile, List<CTile> optionalTileList, bool deselectInstead = false)
	{
		int actionType = (deselectInstead ? 39 : 38);
		ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
		IProtocolToken supplementaryDataToken = new TargetSelectionToken(clientTile.m_Tile, WorldspaceStarHexDisplay.Instance.AreaEffectAngle, WorldspaceStarHexDisplay.Instance.IsAOELocked(), SaveData.Instance.Global.EnableSecondClickHexToConfirm, optionalTileList);
		Synchronizer.SendGameAction((GameActionType)actionType, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
	}

	private string GetNonOverloadAnim(CAbility ability)
	{
		if (ability == null || !ability.IsItemAbility)
		{
			return "PowerUp";
		}
		return "UseItem";
	}

	public void SendDebugMessage(CMessageData message)
	{
		if (Main.s_DevMode || Main.s_InternalRelease)
		{
			m_debugMessage = true;
			MessageHandler(message, processImmediately: true);
			m_debugMessage = false;
		}
	}

	public void SendMessage(CMessageData message)
	{
		if (Main.s_DevMode || Main.s_InternalRelease)
		{
			MessageHandler(message, processImmediately: true);
		}
	}

	private void ProcessMessage(CMessageData message)
	{
		if (ScenarioRuleClient.s_MainThread != Thread.CurrentThread)
		{
			Debug.LogWarning($"Attempting to process a message not from the main thread. MessageType: {message.m_Type}");
			lock (m_MessageQueue)
			{
				m_MessageQueue.Add(message);
				return;
			}
		}
		if (SceneController.Instance.GlobalErrorMessage.ShowingMessage || PhaseManager.CurrentPhase == null)
		{
			return;
		}
		if (message.m_ActorSpawningMessage != null)
		{
			CurrentPlayerType = message.m_ActorSpawningMessage.Type;
		}
		if (message.m_Type == CMessageData.MessageType.SRLQueueDebugLog)
		{
			CSRLQueueStatusLog_MessageData cSRLQueueStatusLog_MessageData = (CSRLQueueStatusLog_MessageData)message;
			GUIInterface.s_GUIInterface.SetSRLQueueStatusText(cSRLQueueStatusLog_MessageData.m_SRLMessage.m_MessageType.ToString());
		}
		else
		{
			if (!message.m_Type.ToString().Contains("CombatLog"))
			{
				GUIInterface.s_GUIInterface.SetClientQueueStatusText(message.m_Type.ToString());
			}
			_ = LastMessage;
			Debug.Log("[MessageHandler] " + message.m_Type);
			if (IsShowedHelpBox)
			{
				IsShowedHelpBox = false;
				Singleton<HelpBox>.Instance.Hide("GUI_WAIT_PLAYERS_CONFIRM_TIP");
			}
			if (!m_debugMessage)
			{
				LastMessage = message;
			}
		}
		switch (message.m_Type)
		{
		case CMessageData.MessageType.NextRound:
			InputManager.RequestDisableInput(this, EKeyActionTag.All);
			Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.RoundStart);
			CardsHandManager.Instance.ClearStartRoundCardsTokenCache();
			Singleton<PhaseBannerHandler>.Instance.ShowStartRound(m_CurrentState.RoundNumber);
			Singleton<CombatLogHandler>.Instance.AddHighlightedLog(string.Format("<b><font=\"MarcellusSC-Regular SDF\">{0}</font></b>", LocalizationManager.GetTranslation("GUI_COMBATLOG_START_ROUND")) + " " + m_CurrentState.RoundNumber, null);
			UIManager.Instance.BattleGoalContainer.UpdateProgress();
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			break;
		case CMessageData.MessageType.PlayersExhausted:
			if (IsRestarting)
			{
				return;
			}
			try
			{
				CPlayersExhausted_MessageData cPlayersExhausted_MessageData = (CPlayersExhausted_MessageData)message;
				bool flag7 = CardsHandManager.Instance.IsActive();
				StringBuilder stringBuilder5 = new StringBuilder();
				if (cPlayersExhausted_MessageData.m_Players.Count > 0)
				{
					for (int num32 = 0; num32 < cPlayersExhausted_MessageData.m_Players.Count; num32++)
					{
						if (flag7)
						{
							CardsHandManager.Instance.UpdateDeadPlayer(cPlayersExhausted_MessageData.m_Players[num32]);
						}
						string statusText = "Player " + LocalizationManager.GetTranslation(cPlayersExhausted_MessageData.m_Players[num32].ActorLocKey()) + " exhausted";
						GUIInterface.s_GUIInterface.SetStatusText(statusText);
						if (num32 > 0)
						{
							if (num32 == cPlayersExhausted_MessageData.m_Players.Count - 1)
							{
								stringBuilder5.Append(" ");
								stringBuilder5.Append(LocalizationManager.GetTranslation("AND"));
								stringBuilder5.Append(" ");
							}
							else
							{
								stringBuilder5.Append(", ");
							}
						}
						stringBuilder5.Append(LocalizationManager.GetTranslation(cPlayersExhausted_MessageData.m_Players[num32].ActorLocKey()));
						stringBuilder5.Append(" ");
					}
					InitiativeTrack.Instance.helpBox.Show("GUI_CHARACTERS_EXHAUSTED_TIP", "GUI_CHARACTERS_EXHAUSTED_TITLE", stringBuilder5.ToString(), HelpBox.FormatTarget.ALL);
					Singleton<PhaseBannerHandler>.Instance.ShowExhausted(stringBuilder5.ToString());
					if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
					{
						InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
					}
				}
				if (ScenarioManager.Scenario.AllPlayers.All((CPlayerActor p) => p.IsDead) && ScenarioManager.Scenario.AllPlayers.Any((CPlayerActor p) => p.IsDeadForObjectives) && PhaseManager.CurrentPhase is CPhasePlayerExhausted)
				{
					SimpleLog.AddToSimpleLog("Lose triggered at PlayersExhausted message as all player actors are dead.");
					LoseScenario();
				}
				else
				{
					ScenarioRuleClient.StepComplete();
				}
			}
			catch (Exception ex192)
			{
				Debug.LogError("An exception occurred while processing the PlayersExhausted message\n" + ex192.Message + "\n" + ex192.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex192.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex192.Message);
			}
			break;
		case CMessageData.MessageType.ProcessAutosaves:
			if (IsRestarting)
			{
				return;
			}
			try
			{
				if (Singleton<UIResultsManager>.Instance.IsShown)
				{
					return;
				}
				if (SaveData.Instance.Global.CurrentAdventureData == null)
				{
					SimpleLog.AddToSimpleLog("Round Chest Rewards were not applied because CurrentAdventureData is null");
				}
				else if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState == null)
				{
					SimpleLog.AddToSimpleLog("Round Chest Rewards were not applied because AdventureMapState is null");
				}
				else
				{
					SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.ApplyRoundChestRewards();
				}
				m_CurrentState.Update();
				if (SaveData.Instance.Global.GameMode == EGameMode.Autotest || SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial || (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster && SaveData.Instance.Global.AdventureData.AdventureMapState.IsPlayingTutorial))
				{
					SetChoreographerState(ChoreographerStateType.Play, 0, null);
					ScenarioRuleClient.NextPhase();
					break;
				}
				if (GameState.RoundCount > 0)
				{
					SaveData.Instance.IsSavingData = true;
					SetChoreographerState(ChoreographerStateType.WaitingForAutosave, 6000, null);
					ProcessAutosavesThreaded();
					break;
				}
				ScenarioRuleLibrary.ScenarioState currentState = m_CurrentState.DeepCopySerializableObject<ScenarioRuleLibrary.ScenarioState>();
				if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
				{
					SaveData.Instance.IsSavingData = true;
					StartCoroutine(SaveData.Instance.Global.CampaignData.UpdateScenarioCheckpointAsync(currentState));
				}
				else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
				{
					SaveData.Instance.IsSavingData = true;
					StartCoroutine(SaveData.Instance.Global.AdventureData.UpdateScenarioCheckpointAsync(currentState));
				}
				else if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario)
				{
					SaveData.Instance.IsSavingData = true;
					StartCoroutine(SaveData.Instance.Global.SingleScenarioData.UpdateScenarioCheckpointAsync(currentState));
				}
			}
			catch (Exception ex194)
			{
				Debug.LogError("An exception occurred while processing the ProcessAutosaves message\n" + ex194.Message + "\n" + ex194.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00182", "GUI_ERROR_MAIN_MENU_BUTTON", ex194.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex194.Message);
			}
			break;
		case CMessageData.MessageType.PlayerToSelectAbilityCardsOrLongRest:
			if (IsRestarting)
			{
				return;
			}
			try
			{
				if (Singleton<UIResultsManager>.Instance.IsShown)
				{
					return;
				}
				if (m_PlayerToSelectAbilityCardsOrLongRest)
				{
					if (ForceEnterToCardSelection)
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CardSelection);
						ForceEnterToCardSelection = false;
					}
					break;
				}
				m_CurrentActor = null;
				if (m_CurrentState.RoundNumber == 1 && ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles.Count > 1)
				{
					m_RoomCameraButton.Show();
				}
				else
				{
					m_RoomCameraButton.Hide();
				}
				InfusionBoardUI.Instance.UpdateBoard();
				FocusWorld();
				ClearStars();
				CardsHandManager.Instance.EnableCancelActiveAbilities = true;
				CardsHandManager.Instance.HideCharacterPreview();
				CardsHandManager.Instance.ResetShortRestFlags();
				StartCoroutine(CardsHandManager.Instance.ShowCoroutine(CardHandMode.CardsSelection, CardPileType.Any, new List<CardPileType>
				{
					CardPileType.Hand,
					CardPileType.Active,
					CardPileType.Round
				}, 2, fadeUnselectableCards: false, allowFullCardPreview: true, allowFullDeckPreview: true, OnShow));
			}
			catch (Exception ex193)
			{
				Debug.LogError("An exception occurred while processing the PlayerToSelectAbilityCardsOrLongRest message\n" + ex193.Message + "\n" + ex193.StackTrace);
				List<ErrorMessage.LabelAction> list3 = new List<ErrorMessage.LabelAction>();
				list3.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_SCENARIO_BUTTON", UnityGameEditorRuntime.ErrorHandlingUnloadSceneResetScenarioStateAndRetryLoad, KeyAction.UI_SUBMIT));
				list3.Add(new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, KeyAction.UI_CANCEL));
				SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_CHOREO_00002", ex193.StackTrace, list3, ex193.Message);
			}
			break;
		case CMessageData.MessageType.PlayersHaveSelectedAbilityCardsOrLongRest:
			if (IsRestarting)
			{
				return;
			}
			try
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.EnemyAction);
				m_PlayerToSelectAbilityCardsOrLongRest = false;
				if (FFSNetwork.IsOnline)
				{
					InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
					{
						f.Avatar.RefreshActiveInteractable();
					});
				}
				foreach (CPlayerActor playerActor3 in ScenarioManager.Scenario.PlayerActors)
				{
					if (FFSNetwork.IsOnline)
					{
						int controllableID2 = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? playerActor3.CharacterName.GetHashCode() : playerActor3.CharacterClass.ModelInstanceID);
						NetworkPlayer controller = ControllableRegistry.GetController(controllableID2);
						SimpleLog.AddToSimpleLog("---> BACKEND CARD STATE FOR: " + controller.Username + ": " + LocalizationManager.GetTranslation(playerActor3.ActorLocKey()) + "(" + (controller.IsClient ? "Client" : "Host") + ")<---");
					}
					else
					{
						SimpleLog.AddToSimpleLog("---> BACKEND CARD STATE FOR: " + LocalizationManager.GetTranslation(playerActor3.ActorLocKey()) + "<---");
					}
					SimpleLog.AddToSimpleLog("LONG REST SELECTED: " + playerActor3.IsLongRestSelected);
					SimpleLog.AddToSimpleLog("ROUND: " + playerActor3.CharacterClass.RoundAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty());
					SimpleLog.AddToSimpleLog("HAND: " + playerActor3.CharacterClass.HandAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty());
					SimpleLog.AddToSimpleLog("DISCARDED: " + playerActor3.CharacterClass.DiscardedAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty());
					SimpleLog.AddToSimpleLog("BURNED: " + playerActor3.CharacterClass.LostAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty());
					SimpleLog.AddToSimpleLog("ACTIVE: " + playerActor3.CharacterClass.ActivatedAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty());
				}
				foreach (CPlayerActor playerActor4 in ScenarioManager.Scenario.PlayerActors)
				{
					string text30 = null;
					if (FFSNetwork.IsOnline)
					{
						int controllableID3 = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? playerActor4.CharacterName.GetHashCode() : playerActor4.CharacterClass.ModelInstanceID);
						NetworkPlayer controller2 = ControllableRegistry.GetController(controllableID3);
						text30 = controller2.Username + ": " + LocalizationManager.GetTranslation(playerActor4.ActorLocKey()) + "(" + (controller2.IsClient ? "Client" : "Host") + ")Scenario Actor Inventory Items: ";
					}
					else
					{
						text30 = LocalizationManager.GetTranslation(playerActor4.ActorLocKey()) + "Scenario Actor Inventory Items:";
					}
					foreach (CItem allItem in playerActor4.Inventory.AllItems)
					{
						text30 = text30 + "\n" + LocalizationNameConverter.MultiLookupLocalization(allItem.Name, out var _) + " Network ID: " + allItem.NetworkID + " State: " + allItem.SlotState.ToString() + ((AdventureState.MapState?.MapParty != null) ? (" Item Exists on corresponding MapCharacter: " + (AdventureState.MapState.MapParty.IsItemEquippedByParty(allItem) != null)) : "");
					}
					SimpleLog.AddToSimpleLog(text30);
				}
				SimpleLog.AddToSimpleLog("RNG STATES (PlayersHaveSelectedAbilityCardsOrLongRest): \nScenarioRNG:" + ScenarioManager.CurrentScenarioState.PeekScenarioRNG + "\nEnemyIDRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyIDRNG + "\nEnemyAbilityCardRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyAbilityCardRNG + "\nGuidRNG:" + ScenarioManager.CurrentScenarioState.PeekGuidRNG);
				m_RoomCameraButton.Hide();
				CardsHandManager.Instance.Hide();
				SetChoreographerState(ChoreographerStateType.Play, 0, null);
				WorldspaceStarHexDisplay.Instance.ClearStars();
			}
			catch (Exception ex190)
			{
				Debug.LogError("An exception occurred while processing the PlayersHaveSelectedAbilityCardsOrLongRest message\n" + ex190.Message + "\n" + ex190.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex190.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex190.Message);
			}
			break;
		case CMessageData.MessageType.MonsterClassesToSelectAbilityCards:
			if (IsRestarting)
			{
				return;
			}
			try
			{
				UIManager.Instance.BattleGoalContainer.Hide();
				CameraController.s_CameraController.DisableCameraInput(disableInput: true);
				if (ScenarioManager.Scenario.PlayerActors.Count == 0)
				{
					return;
				}
				CMonsterClassesToSelectAbilityCards_MessageData cMonsterClassesToSelectAbilityCards_MessageData = (CMonsterClassesToSelectAbilityCards_MessageData)message;
				foreach (CMonsterClass monsterClass in cMonsterClassesToSelectAbilityCards_MessageData.m_MonsterClasses)
				{
					if (monsterClass.RoundAbilityCard != null)
					{
						SimpleLog.AddToSimpleLog(LocalizationManager.GetTranslation(monsterClass.LocKey) + " selected card " + monsterClass.RoundAbilityCard.Name);
					}
				}
				List<GameObject> clientMonsterObjects = ClientMonsterObjects;
				if (clientMonsterObjects.Count == 0 && !Singleton<UIActiveBonusBar>.Instance.IsShowing)
				{
					Pass();
					break;
				}
				m_CurrentActor = null;
				if (clientMonsterObjects.Count > 0)
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.EnemyAction);
					InitiativeTrack.Instance.ShowMonsterClassesForSelectingRoundAbilityCards(clientMonsterObjects);
					s_Choreographer.m_selectButton?.SetActive(!FFSNetwork.IsClient, LocalizationManager.GetTranslation("GUI_CONTINUE"));
					readyButton.Toggle(!InputManager.GamePadInUse, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONTINUE"), hideOnClick: true, glowingEffect: true, !FFSNetwork.IsClient, disregardTurnControlForInteractability: true);
				}
				else
				{
					readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONPASS, LocalizationManager.GetTranslation("GUI_CONFIRM"), hideOnClick: true, glowingEffect: true, !FFSNetwork.IsClient, disregardTurnControlForInteractability: true);
				}
				if (FFSNetwork.IsOnline)
				{
					Singleton<UIScenarioMultiplayerController>.Instance.HidePlayerInfo();
					if (FFSNetwork.IsClient)
					{
						string translation13 = LocalizationManager.GetTranslation("Consoles/GUI_MULTIPLAYER_TIP_TITLE");
						string translation14 = LocalizationManager.GetTranslation("Consoles/GUI_WAIT_FOR_HOST_TIP");
						InitiativeTrack.Instance.helpBox.ShowTranslated(translation14, translation13);
						ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndHalt, ActionPhaseType.EnemyCardReveal);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.EnemyCardReveal);
					}
				}
				GUIInterface.s_GUIInterface.SetStatusText("Monster Classes selecting ability cards");
			}
			catch (Exception ex196)
			{
				Debug.LogError("An exception occurred while processing the MonsterClassesToSelectAbilityCards message\n" + ex196.Message + "\n" + ex196.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex196.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex196.Message);
			}
			break;
		case CMessageData.MessageType.MonsterClassesHaveSelectedAbilityCards:
			if (IsRestarting)
			{
				return;
			}
			try
			{
				SimpleLog.AddToSimpleLog("RNG STATES (MonsterClassesHaveSelectedAbilityCards): \nScenarioRNG:" + ScenarioManager.CurrentScenarioState.PeekScenarioRNG + "\nEnemyIDRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyIDRNG + "\nEnemyAbilityCardRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyAbilityCardRNG + "\nGuidRNG:" + ScenarioManager.CurrentScenarioState.PeekGuidRNG);
				InitiativeTrack.Instance?.UpdateSelectable(playersSelectable: false, enemiesSelectable: false);
				CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				if (FFSNetwork.IsOnline)
				{
					ControllableRegistry.AllControllables.ForEach(delegate(NetworkControllable x)
					{
						x.ResetState();
					});
				}
			}
			catch (Exception ex191)
			{
				Debug.LogError("An exception occurred while processing the MonsterClassesHaveSelectedAbilityCards message\n" + ex191.Message + "\n" + ex191.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex191.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex191.Message);
			}
			break;
		case CMessageData.MessageType.StartTurn:
			try
			{
				if (IsRestarting)
				{
					return;
				}
				CStartTurn_MessageData messageData10 = (CStartTurn_MessageData)message;
				m_CurrentActor = message.m_ActorSpawningMessage;
				if (CurrentPlayerActor == null)
				{
					UIManager.Instance.BattleGoalContainer.Hide();
				}
				else
				{
					UIManager.Instance.BattleGoalContainer.Show(CurrentPlayerActor);
				}
				if (FFSNetwork.IsOnline && m_CurrentActor is CPlayerActor)
				{
					InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
					{
						f.Avatar.RefreshActiveInteractable();
					});
				}
				if (messageData10.m_ActorSpawningMessage is CPlayerActor)
				{
					SaveData.Instance.Global.StopSpeedUp();
				}
				else
				{
					SaveData.Instance.Global.StartSpeedUp();
				}
				string text31 = "";
				string text32 = "";
				string iconText = null;
				if (FFSNetwork.IsOnline && messageData10.m_ActorSpawningMessage is CPlayerActor cPlayerActor9)
				{
					int controllableID4 = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? cPlayerActor9.CharacterName.GetHashCode() : cPlayerActor9.CharacterClass.ModelInstanceID);
					NetworkPlayer controller3 = ControllableRegistry.GetController(controllableID4);
					text31 = controller3.UserNameWithPlatformIcon() + ": " + LocalizationManager.GetTranslation(cPlayerActor9.ActorLocKey());
					text32 = "  (" + (controller3.IsClient ? "Client" : "Host") + ")";
				}
				else
				{
					text31 = LocalizationManager.GetTranslation(messageData10.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(messageData10.m_ActorSpawningMessage);
				}
				Singleton<CombatLogHandler>.Instance.AddHighlightedLog(string.Format("<b><font=\"MarcellusSC-Regular SDF\">{0}</font></b>", string.Format(LocalizationManager.GetTranslation("GUI_COMBATLOG_START_TURN"), text31) + text32), message.m_ActorSpawningMessage.Type, null, iconText);
				SimpleLog.WriteSimpleLogToFile();
				readyButton.Toggle(active: false);
				readyButton.ClearAlternativeAction();
				s_Choreographer.onCharacterAbilityComplete = null;
				ClearHilightedActors();
				ActorBehaviour.SetHilighted(FindClientActorGameObject(message.m_ActorSpawningMessage), hilight: true);
				m_AbilitiesUsedThisTurn.Clear();
				if (!m_FollowCameraDisabled)
				{
					CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				}
				m_CurrentActor = message.m_ActorSpawningMessage;
				if (FFSNetwork.IsOnline && m_CurrentActor is CPlayerActor)
				{
					InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
					{
						f.Avatar.RefreshActiveInteractable();
					});
				}
				if (CurrentPlayerActor == null)
				{
					UIManager.Instance.BattleGoalContainer.Hide();
				}
				else
				{
					UIManager.Instance.BattleGoalContainer.Show(CurrentPlayerActor);
				}
				CardsHandManager.Instance.EnableCancelActiveAbilities = m_CurrentActor is CPlayerActor;
				ClientScenarioManager.s_ClientScenarioManager.ClearAIPathVisuals(null);
				Singleton<PhaseBannerHandler>.Instance.ResetAndHide();
				if (message.m_ActorSpawningMessage is CPlayerActor cPlayerActor10)
				{
					cPlayerActor10.IsLongRestActionSelected = false;
				}
				if (message.m_ActorSpawningMessage is CEnemyActor enemyActor)
				{
					Singleton<PhaseBannerHandler>.Instance.ShowEnemyTurn(LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey()), delegate
					{
						Debug.Log($"Finished phase banner. Skip next phase? {messageData10.m_SkipNextPhase} (current phase: {PhaseManager.PhaseType})");
						if (PhaseManager.PhaseType == CPhase.PhaseType.StartTurn && !messageData10.m_SkipNextPhase)
						{
							ScenarioRuleClient.NextPhase();
						}
					});
					if (InputManager.GamePadInUse)
					{
						Singleton<EnemyCurrentTurnStatPanel>.Instance.Show(enemyActor);
					}
					break;
				}
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage is CPlayerActor cPlayerActor11)
				{
					int controllableID5 = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? cPlayerActor11.CharacterName.GetHashCode() : cPlayerActor11.CharacterClass.ModelInstanceID);
					NetworkPlayer controller4 = ControllableRegistry.GetController(controllableID5);
					Singleton<UIScenarioMultiplayerController>.Instance.OnStartTurn(cPlayerActor11);
					Singleton<PhaseBannerHandler>.Instance.ShowPlayerTurn(LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey()), string.Format(LocalizationManager.GetTranslation("GUI_COMBATLOG_PLAYER_TURN"), controller4.UserNameWithPlatformIcon()));
				}
				else
				{
					Singleton<PhaseBannerHandler>.Instance.ShowPlayerTurn(LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey()));
				}
				if (!messageData10.m_SkipNextPhase)
				{
					ScenarioRuleClient.NextPhase();
				}
			}
			catch (Exception ex197)
			{
				Debug.LogError("An exception occurred while processing the StartTurn message\n" + ex197.Message + "\n" + ex197.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex197.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex197.Message);
			}
			break;
		case CMessageData.MessageType.ActionSelectionPhaseStart:
		{
			CActionSelectionPhaseStart_MessageData cActionSelectionPhaseStart_MessageData = (CActionSelectionPhaseStart_MessageData)message;
			m_CurrentActor = cActionSelectionPhaseStart_MessageData.m_ActorSpawningMessage;
			if (FFSNetwork.IsOnline && m_CurrentActor is CPlayerActor)
			{
				InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
				{
					f.Avatar.RefreshActiveInteractable();
				});
			}
			if (CurrentPlayerActor == null)
			{
				UIManager.Instance.BattleGoalContainer.Hide();
			}
			else
			{
				UIManager.Instance.BattleGoalContainer.Show(CurrentPlayerActor);
			}
			WorldspaceStarHexDisplay.Instance.Clear();
			Singleton<UIActiveBonusBar>.Instance.Hide();
			m_UndoButton.Toggle(active: false);
			m_SkipButton.Toggle(active: false);
			SetActiveSelectButton(activate: false);
			break;
		}
		case CMessageData.MessageType.ActionSelection:
			if (IsRestarting)
			{
				Debug.Log("Returning out of ActionSelection message as Choreographer.IsRestarting is true");
				return;
			}
			try
			{
				InfusionBoardUI.Instance.UpdateBoard();
				ClearStars();
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				m_UndoButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				m_AbilitiesUsedThisTurn.Clear();
				if (message.m_ActorSpawningMessage.Class is CMonsterClass || message.m_ActorSpawningMessage.Class is CHeroSummonClass || (message.m_ActorSpawningMessage.Class is CCharacterClass && message.m_ActorSpawningMessage.Type != CActor.EType.Player))
				{
					CardsHandManager.Instance.Hide();
					Pass();
					if (message.m_ActorSpawningMessage.Class is CHeroSummonClass && FFSNetwork.IsOnline)
					{
						if (!message.m_ActorSpawningMessage.IsUnderMyControl)
						{
							ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.FirstAction);
						}
						else
						{
							ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.FirstAction);
						}
					}
				}
				else
				{
					CPlayerActor playerActor2 = (CPlayerActor)message.m_ActorSpawningMessage;
					Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(message.m_ActorSpawningMessage);
					CameraController.s_CameraController.SmartFocus(FindClientActorGameObject(playerActor2), pauseDuringTransition: true);
					Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(playerActor2, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility);
					if (playerActor2.CharacterClass.LongRest)
					{
						Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor2);
						CardsHandManager.Instance.ShowLongRestConfirmation(playerActor2, delegate(bool confirmed)
						{
							Singleton<UIUseItemsBar>.Instance.Hide();
							Singleton<UIActiveBonusBar>.Instance.Hide();
							Singleton<UIUseAbilitiesBar>.Instance.Hide();
							Singleton<UIUseAugmentationsBar>.Instance.Hide();
							CardsHandManager.Instance.EnableCancelActiveAbilities = false;
							if (confirmed)
							{
								Singleton<UIUseItemsBar>.Instance.Hide();
							}
							else
							{
								Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor2);
							}
							readyButton.Toggle(confirmed, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
							WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.LongResting;
						});
						readyButton.ClearAlternativeAction();
						readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_PERFORM_LONG_REST"));
						readyButton.QueueAlternativeAction(delegate
						{
							if (playerActor2.CharacterClass.DiscardedAbilityCards.Count == 0)
							{
								CardsHandManager.Instance.ShowLongRested(playerActor2);
								CardsHandManager.Instance.EnableCancelActiveAbilities = !FFSNetwork.IsOnline || playerActor2.IsUnderMyControl;
								GameState.PlayerLongRested(null);
								WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
							}
							else
							{
								InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_LONG_REST", "GUI_LONG_REST");
								Singleton<UIUseItemsBar>.Instance.Hide();
								ActorBehaviour.GetActorBehaviour(FindClientPlayer(playerActor2)).m_WorldspacePanelUI.OnSelectingHealFocus(2);
								CardsHandManager.Instance.Show(playerActor2, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Discarded, 1, fadeUnselectableCards: true, highlightSelectableCards: true);
								Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.LoseCard);
							}
							if (FFSNetwork.IsOnline)
							{
								if (!playerActor2.IsUnderMyControl)
								{
									ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndHalt, ActionPhaseType.LongRest);
								}
								else
								{
									ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.LongRest);
								}
							}
						});
						if (FFSNetwork.IsOnline)
						{
							if (!playerActor2.IsUnderMyControl)
							{
								CardsHandManager.Instance.EnableCancelActiveAbilities = false;
								ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.LongRest);
							}
							else
							{
								ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.LongRest);
							}
						}
					}
					else
					{
						CardsHandManager.Instance.EnableCancelActiveAbilities = !FFSNetwork.IsOnline || playerActor2.IsUnderMyControl;
						if (!playerActor2.CharacterClass.HasLongRested || playerActor2.IsTakingExtraTurn)
						{
							if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.FirstAction)
							{
								CardsHandManager.Instance.Show(playerActor2, CardHandMode.ActionSelection, (!playerActor2.IsTakingExtraTurn) ? CardPileType.Round : CardPileType.ExtraTurn, CardPileType.Any, 0, fadeUnselectableCards: false, highlightSelectableCards: false, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.FORCE_RESET);
								s_Choreographer.SwitchToSelectActionUiState(playerActor2);
								Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor2, (CItem cItem2) => !playerActor2.IsTakingExtraTurn || cItem2.SlotState != CItem.EItemSlotState.Locked || !Singleton<AbilityEffectManager>.Instance.IsItemAffectingActor(playerActor2, cItem2));
								m_InitialActionAbilityCard = null;
								GUIInterface.s_GUIInterface.SetStatusText("Select Ability Card and first action to use");
								if (FFSNetwork.IsOnline)
								{
									if (!playerActor2.IsUnderMyControl)
									{
										ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.FirstAction);
									}
									else
									{
										ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.FirstAction);
									}
								}
							}
							else if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.SecondAction)
							{
								CardsHandManager.Instance.Show(playerActor2, CardHandMode.ActionSelection, (!playerActor2.IsTakingExtraTurn) ? CardPileType.Round : CardPileType.ExtraTurn);
								s_Choreographer.SwitchToSelectActionUiState(playerActor2);
								Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor2, (CItem cItem2) => !playerActor2.IsTakingExtraTurn || cItem2.SlotState != CItem.EItemSlotState.Locked || !Singleton<AbilityEffectManager>.Instance.IsItemAffectingActor(playerActor2, cItem2));
								GUIInterface.s_GUIInterface.SetStatusText("Select second action to use");
								if (FFSNetwork.IsOnline)
								{
									if (!playerActor2.IsUnderMyControl)
									{
										ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.SecondAction);
									}
									else
									{
										ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.SecondAction);
									}
								}
							}
							else
							{
								InitiativeTrack.Instance.helpBox.ClearOverrideController();
								CardsHandManager.Instance.Show(playerActor2, CardHandMode.ActionSelection, (!playerActor2.IsTakingExtraTurn) ? CardPileType.Round : CardPileType.ExtraTurn, CardPileType.Any, 0, fadeUnselectableCards: false, highlightSelectableCards: false, allowFullCardPreview: true, (!CardsActionControlller.s_Instance.HasPlayedCards()) ? CardsHandUI.CardActionsCommand.RESET : CardsHandUI.CardActionsCommand.NONE);
								Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.HexMovement);
								CardsHandManager.Instance.cardsActionController.OnActionFinished();
								CardsHandManager.Instance.UpdateOriginalExtraTurnCards(playerActor2);
								EnableEndTurnButton(playerActor2);
								Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor2, (CItem cItem2) => cItem2.YMLData.Data.CompareAbility == null || cItem2.YMLData.Data.CompareAbility.CompareActor(playerActor2));
								Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(playerActor2, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility);
								if (FFSNetwork.IsOnline)
								{
									if (!playerActor2.IsUnderMyControl)
									{
										ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.EndOfTurn);
									}
									else
									{
										ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.EndOfTurn);
									}
								}
							}
						}
						else
						{
							Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor2, (CItem cItem2) => cItem2.YMLData.Data.CompareAbility == null || cItem2.YMLData.Data.CompareAbility.CompareActor(playerActor2));
							Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.HexMovement);
							EnableEndTurnButton(playerActor2);
							Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor2);
							if (FFSNetwork.IsOnline)
							{
								if (!playerActor2.IsUnderMyControl)
								{
									ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.EndOfTurn);
								}
								else
								{
									ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.EndOfTurn);
								}
							}
						}
					}
				}
				CameraController.s_CameraController.DisableCameraInput(disableInput: false);
			}
			catch (Exception ex195)
			{
				Debug.LogError("An exception occurred while processing the ActionSelection message\n" + ex195.Message + "\n" + ex195.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex195.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex195.Message);
			}
			break;
		case CMessageData.MessageType.PlayerLongRested:
			try
			{
				CPlayerLongRested_MessageData cPlayerLongRested_MessageData = (CPlayerLongRested_MessageData)message;
				ActorBehaviour.UpdateHealth(FindClientPlayer((CPlayerActor)message.m_ActorSpawningMessage), cPlayerLongRested_MessageData.m_ActorOriginalHealth, updateUI: true);
				GameObject gameObject89 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				CPlayerActor playerActor = (CPlayerActor)message.m_ActorSpawningMessage;
				GameObject obj25 = ObjectPool.Spawn(GlobalSettings.Instance.m_GlobalParticles.DefaultHealEffect, gameObject89.transform);
				ObjectPool.Recycle(obj25, VFXShared.GetEffectLifetime(obj25), GlobalSettings.Instance.m_GlobalParticles.DefaultHealEffect);
				message.m_ActorSpawningMessage.ActivatePassiveItems(firstLoad: true);
				Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor, (CItem cItem2) => cItem2.YMLData.Data.CompareAbility == null || cItem2.YMLData.Data.CompareAbility.CompareActor(playerActor));
				string arg43 = LocalizationManager.GetTranslation(cPlayerLongRested_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cPlayerLongRested_MessageData.m_ActorSpawningMessage);
				if (cPlayerLongRested_MessageData.m_AbilityCard != null)
				{
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LONG_REST"), arg43, $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(cPlayerLongRested_MessageData.m_AbilityCard.Name)}</color></b></font>"));
				}
				else
				{
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LONG_REST_NO_CARD"), arg43));
				}
				if (GameState.PendingOnLongRestBonuses.Count == 0)
				{
					EnableEndTurnButton(playerActor);
					Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor);
					Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(playerActor, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility);
				}
				else
				{
					TriggerAnyOnLongRestAddActiveBonuses(playerActor);
				}
				if (FFSNetwork.IsOnline)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.EndOfTurn);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.EndOfTurn);
					}
				}
			}
			catch (Exception ex188)
			{
				Debug.LogError("An exception occurred while processing the PlayerLongRested message\n" + ex188.Message + "\n" + ex188.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex188.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex188.Message);
			}
			break;
		case CMessageData.MessageType.PlayerImprovedShortRested:
			try
			{
				CPlayerImprovedShortRested_MessageData cPlayerImprovedShortRested_MessageData = (CPlayerImprovedShortRested_MessageData)message;
				ActorBehaviour.UpdateHealth(FindClientPlayer((CPlayerActor)message.m_ActorSpawningMessage), cPlayerImprovedShortRested_MessageData.m_ActorOriginalHealth, updateUI: true);
				GameObject gameObject88 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				GameObject obj24 = ObjectPool.Spawn(GlobalSettings.Instance.m_GlobalParticles.DefaultHealEffect, gameObject88.transform);
				ObjectPool.Recycle(obj24, VFXShared.GetEffectLifetime(obj24), GlobalSettings.Instance.m_GlobalParticles.DefaultHealEffect);
				message.m_ActorSpawningMessage.ActivatePassiveItems(firstLoad: true);
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, LocalizationManager.GetTranslation("GUI_END_SELECTION"), hideOnClick: true, glowingEffect: true);
				readyButton.SetInteractable(interactable: false);
				CardsHandManager.Instance.SetControllerIndicatorsActive(active: true);
				ActorBehaviour.GetActorBehaviour(gameObject88).m_WorldspacePanelUI.RemoveEffect("ImprovedShortRest");
				InitiativeTrack.Instance.FindInitiativeTrackActor(cPlayerImprovedShortRested_MessageData.m_ActorSpawningMessage).SetAttributes(cPlayerImprovedShortRested_MessageData.m_ActorSpawningMessage, activePlayerButton: true, changeHilight: false);
				string arg42 = LocalizationManager.GetTranslation(cPlayerImprovedShortRested_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cPlayerImprovedShortRested_MessageData.m_ActorSpawningMessage);
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_SHORT_REST"), arg42, $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(cPlayerImprovedShortRested_MessageData.m_AbilityCard.Name)}</color></b></font>"));
				if (FFSNetwork.IsOnline)
				{
					Singleton<UIReadyToggle>.Instance.ToggleVisibility(visible: true);
				}
			}
			catch (Exception ex187)
			{
				Debug.LogError("An exception occurred while processing the PlayerImprovedShortRested message\n" + ex187.Message + "\n" + ex187.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00194", "GUI_ERROR_MAIN_MENU_BUTTON", ex187.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex187.Message);
			}
			break;
		case CMessageData.MessageType.PlayerShortRested:
			try
			{
				CPlayerShortRested_MessageData cPlayerShortRested_MessageData = (CPlayerShortRested_MessageData)message;
				ActorBehaviour.UpdateHealth(FindClientPlayer(cPlayerShortRested_MessageData.m_Player));
				InitiativeTrack.Instance.FindInitiativeTrackActor(cPlayerShortRested_MessageData.m_Player).SetAttributes(cPlayerShortRested_MessageData.m_Player, activePlayerButton: true, changeHilight: false);
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_SHORT_REST"), LocalizationManager.GetTranslation(cPlayerShortRested_MessageData.m_Player.ActorLocKey()), $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(cPlayerShortRested_MessageData.m_AbilityCard.Name)}</color></b></font>"));
			}
			catch (Exception ex189)
			{
				Debug.LogError("An exception occurred while processing the PlayerShortRested message\n" + ex189.Message + "\n" + ex189.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex189.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex189.Message);
			}
			break;
		case CMessageData.MessageType.StartAbility:
			try
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.UseAction);
				CStartActorAbility_MessageData cStartActorAbility_MessageData = (CStartActorAbility_MessageData)message;
				if (!cStartActorAbility_MessageData.merged)
				{
					Waypoint.s_LockWaypoints = false;
					WorldspaceStarHexDisplay.Instance.LockView = false;
					ClearStars();
				}
				readyButton.ClearAlternativeAction();
				m_UndoButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
				m_UndoButton.SetInteractable(cStartActorAbility_MessageData.m_Ability.CanUndo && cStartActorAbility_MessageData.m_IsFirstAbility);
				SetActiveSelectButton(message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"), hideOnClick: true, cStartActorAbility_MessageData.m_Ability.CanSkip);
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(message.m_ActorSpawningMessage);
				FirstAbility = cStartActorAbility_MessageData.m_IsFirstAbility;
				m_CurrentAbility = cStartActorAbility_MessageData.m_Ability;
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				if (message.m_ActorSpawningMessage is CPlayerActor && cStartActorAbility_MessageData.m_Ability.CanApplyActiveBonusTogglesTo())
				{
					Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(cStartActorAbility_MessageData.m_ActorSpawningMessage, cStartActorAbility_MessageData.m_Ability.AbilityType, CActiveBonus.EActiveBonusBehaviourType.None, null, cStartActorAbility_MessageData.m_Ability.AbilityHasHappened, showSingleTargetBonus: false, cStartActorAbility_MessageData.m_Ability);
				}
				DisableTileSelection(active: false);
				FocusWorld();
			}
			catch (Exception ex185)
			{
				Debug.LogError("An exception occurred while processing the StartAbility message\n" + ex185.Message + "\n" + ex185.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex185.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex185.Message);
			}
			break;
		case CMessageData.MessageType.StartSecondMergedAbility:
			try
			{
				CStartSecondMergedAbility_MessageData cStartSecondMergedAbility_MessageData = (CStartSecondMergedAbility_MessageData)message;
				m_UndoButton.ClearOnClickOverriders();
				m_UndoButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player && FirstAbility, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
				m_UndoButton.SetInteractable(cStartSecondMergedAbility_MessageData.m_Ability.CanUndo);
				CardsActionControlller.s_Instance.RefreshConsumeBar(WaitingForConfirm);
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MergedAbility);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.MergedAbility);
					}
				}
			}
			catch (Exception ex184)
			{
				Debug.LogError("An exception occurred while processing the StartSecondMergedAbility message\n" + ex184.Message + "\n" + ex184.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex184.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex184.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsSelectingMoveTile:
			try
			{
				if (m_PlayerSelectingToAvoidDamageOrNot)
				{
					m_AfterTakeDamageProgressMessages.Add(message);
					break;
				}
				FocusWorld();
				CActorIsSelectingMoveTile_MessageData messageData9 = (CActorIsSelectingMoveTile_MessageData)message;
				Waypoint.s_MovingActor = messageData9.m_MoveAbility.CurrentMovingActor;
				s_Choreographer.m_CurrentActor = messageData9.m_MoveAbility.CurrentMovingActor;
				s_Choreographer.m_CurrentAbility = messageData9.m_MoveAbility;
				bool flag6 = false;
				if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
					foreach (GameObject clientActorObject in ClientActorObjects)
					{
						if (clientActorObject != null)
						{
							ActorBehaviour.GetActorBehaviour(clientActorObject).m_WorldspacePanelUI.ResetPreview();
						}
					}
					if (!m_FollowCameraDisabled)
					{
						CameraController.s_CameraController.DisableCameraInput(disableInput: false);
					}
					if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
					{
						Singleton<UIUseItemsBar>.Instance.ShowUsableItems(message.m_ActorSpawningMessage, (CItem cItem2) => cItem2.YMLData.Data.CompareAbility != null && cItem2.YMLData.Data.CompareAbility.CompareAbility(messageData9.m_MoveAbility), "GUI_USE_ITEM_MOVEMENT_TIP");
						Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(messageData9.m_ActorSpawningMessage, CAbility.EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.None, null, messageData9.m_MoveAbility.HasMoved, showSingleTargetBonus: false, messageData9.m_MoveAbility);
					}
					GameObject gameObject85 = null;
					if (!messageData9.m_MoveAbility.HasMoved)
					{
						WorldspaceStarHexDisplay.Instance.ResetLineDirection();
						Waypoint.RefreshWaypointMovesRemaining(messageData9.m_MoveAbility);
						if (Waypoint.s_Waypoints.Count > 0)
						{
							int num30 = Waypoint.s_Waypoints.Sum((GameObject s) => s.GetComponent<Waypoint>().MoveCost);
							while (num30 > messageData9.m_MoveAbility.MoveCount)
							{
								num30 -= Waypoint.GetLastWaypoint.MoveCost;
								Waypoint.GetLastWaypoint.OnDelete();
							}
						}
					}
					else
					{
						gameObject85 = Waypoint.GetNextWaypoint();
					}
					if (gameObject85 != null)
					{
						Waypoint waypointComponent3 = Waypoint.GetWaypointComponent(gameObject85);
						bool tileSelectionDisabled = m_TileSelectionDisabled;
						DisableTileSelection(active: false);
						s_Choreographer.TileHandler(waypointComponent3.ClientTile, Waypoint.GetCTileList());
						DisableTileSelection(tileSelectionDisabled);
						Waypoint.DestroyWaypoint(gameObject85);
						GUIInterface.s_GUIInterface.SetStatusText("Next stage of path" + message.m_ActorSpawningMessage.ID + " Waypoint Order " + waypointComponent3.Order);
					}
					else
					{
						flag6 = true;
						WorldspaceStarHexDisplay.Instance.LockView = false;
						TileBehaviour.SetCallback(Waypoint.TileHandler);
						SetChoreographerState(ChoreographerStateType.WaitingForPlayerWaypointSelection, 0, null);
						WorldspaceStarHexDisplay.Instance.CurrentMoveDisplayType = WorldspaceStarHexDisplay.EMoveDisplayType.Normal;
						if (WorldspaceStarHexDisplay.Instance.CurrentDisplayState.Equals(WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.MovementSelection))
						{
							WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
						}
						WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.MovementSelection;
						DisableTileSelection(active: false);
						GUIInterface.s_GUIInterface.SetStatusText("Select way points for player path" + message.m_ActorSpawningMessage.ID + " . Number moves " + messageData9.m_MoveAbility.RemainingMoves);
					}
					foreach (CActiveBonus showingActiveBonuse in Singleton<UIActiveBonusBar>.Instance.ShowingActiveBonuses)
					{
						ShowAbilityHelpBoxTooltip(showingActiveBonuse.Ability);
					}
				}
				else
				{
					GUIInterface.s_GUIInterface.SetStatusText("Monster " + LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey()) + "(" + message.m_ActorSpawningMessage.ID + ") moving , moves remaining " + messageData9.m_MoveAbility.RemainingMoves);
				}
				if (!m_FollowCameraDisabled)
				{
					GameObject target4 = FindClientActorGameObject(message.m_ActorSpawningMessage);
					CameraController.s_CameraController.SmartFocus(target4, pauseDuringTransition: true);
				}
				if (m_WaitState.m_State == ChoreographerStateType.WaitingForMoveAnim)
				{
					break;
				}
				m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_MOVEMENT"), hideOnClick: true, messageData9.m_MoveAbility.CanSkip);
				readyButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, (Waypoint.s_Waypoints.Count > 0) ? ReadyButton.EButtonState.EREADYBUTTONCONFIRMMOVEMENT : ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_MOVEMENT"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				if (Waypoint.s_Waypoints.Count <= 0)
				{
					m_UndoButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				}
				m_UndoButton.SetInteractable(messageData9.m_MoveAbility.CanUndo && (FirstAbility || messageData9.m_MoveAbility.IsItemAbility));
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage.Type == CActor.EType.Player && flag6)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MoveTileSelection);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.MoveTileSelection);
					}
				}
			}
			catch (Exception ex183)
			{
				Debug.LogError("An exception occurred while processing the ActorIsSelectingMoveTile message\n" + ex183.Message + "\n" + ex183.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex183.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex183.Message);
			}
			break;
		case CMessageData.MessageType.ActorHasMoved:
			try
			{
				SimpleLog.AddToSimpleLog("ActorHasMoved starting");
				CActorHasMoved_MessageData cActorHasMoved_MessageData = (CActorHasMoved_MessageData)message;
				Waypoint.s_MovingActor = cActorHasMoved_MessageData.m_MovingActor;
				ClearStars();
				Singleton<UIUseItemsBar>.Instance.Hide(resetSlots: false);
				Singleton<UIActiveBonusBar>.Instance.Hide();
				Singleton<UIUseAugmentationsBar>.Instance.Hide();
				TileBehaviour.SetCallback(TileHandler);
				GameObject gameObject86 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				if (gameObject86 != null)
				{
					if (!m_FollowCameraDisabled)
					{
						CameraController.s_CameraController.SetFocalPointGameObject(gameObject86);
						CameraController.s_CameraController.DisableCameraInput(disableInput: true);
					}
					Vector3 position4 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[message.m_ActorSpawningMessage.ArrayIndex.X, message.m_ActorSpawningMessage.ArrayIndex.Y].m_GameObject.transform.position;
					int num31 = 0;
					if (MF.GetGameObjectAnimatorController(gameObject86) != null && MF.GameObjectAnimatorControllerHasState(gameObject86, "Idle-Run"))
					{
						if ((message.m_ActorSpawningMessage.Class is CMonsterClass cMonsterClass && cMonsterClass.CurrentMonsterStat.Move <= 0) || (message.m_ActorSpawningMessage is CHeroSummonActor cHeroSummonActor3 && cHeroSummonActor3.SummonData.Move <= 0))
						{
							if (MF.GetGameObjectAnimatorController(gameObject86) != null)
							{
								num31 = 10000;
								SimpleLog.AddToSimpleLog("ActorHasMoved waitTickFrame = 10000");
								ActorBehaviour.GetActorBehaviour(gameObject86).PushPullToLocation(position4);
							}
							else
							{
								num31 = 400;
								SimpleLog.AddToSimpleLog("ActorHasMoved waitTickFrame = 400");
								gameObject86.transform.position = position4;
							}
						}
						else
						{
							num31 = 10000;
							SimpleLog.AddToSimpleLog("ActorHasMoved waitTickFrame = 10000");
							List<CTile> waypoints = cActorHasMoved_MessageData.m_Waypoints;
							if (cActorHasMoved_MessageData.m_MovingActor.Type != CActor.EType.Player && ((cActorHasMoved_MessageData != null && cActorHasMoved_MessageData.m_MovingActor?.AIMoveFocusPath?.Count == 0) || cActorHasMoved_MessageData?.m_MovingActor?.AIMoveFocusPath == null))
							{
								waypoints = null;
							}
							ActorBehaviour actorBehaviour19 = ActorBehaviour.GetActorBehaviour(gameObject86);
							actorBehaviour19.SetLocoTarget(position4, cActorHasMoved_MessageData.m_Jump, waypoints);
							actorBehaviour19.FadeCharacterOnPosition(message.m_ActorSpawningMessage.ArrayIndex);
						}
					}
					else
					{
						num31 = 400;
						gameObject86.transform.position = position4;
					}
					if (cActorHasMoved_MessageData.m_ActorsToCarry != null)
					{
						foreach (CActor item6 in cActorHasMoved_MessageData.m_ActorsToCarry)
						{
							GameObject gameObject87 = FindClientActorGameObject(item6);
							if (MF.GetGameObjectAnimatorController(gameObject87) != null)
							{
								num31 = 10000;
								SimpleLog.AddToSimpleLog("ActorHasMoved waitTickFrame = 10000");
								ActorBehaviour.GetActorBehaviour(gameObject87).PushPullToLocation(position4);
							}
						}
					}
					SetChoreographerState(ChoreographerStateType.WaitingForMoveAnim, num31, message.m_ActorSpawningMessage);
					SimpleLog.AddToSimpleLog("ActorHasMoved ChoreographerStateType.WaitingForMoveAnim set.");
					if (cActorHasMoved_MessageData.m_Ability.IsMergedAbility)
					{
						AttackModBar.s_AttackModifierBarFlowCanBegin = true;
					}
				}
				else
				{
					SimpleLog.AddToSimpleLog("ActorHasMoved currentActorGameObject is Null! Movement not performed.");
				}
			}
			catch (Exception ex186)
			{
				Debug.LogError("An exception occurred while processing the ActorHasMoved message\n" + ex186.Message + "\n" + ex186.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex186.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex186.Message);
			}
			break;
		case CMessageData.MessageType.PauseLoco:
			try
			{
				CPauseLoco_MessageData cPauseLoco_MessageData = (CPauseLoco_MessageData)message;
				GameObject gameObject14 = FindClientActorGameObject(cPauseLoco_MessageData.m_ActorSpawningMessage);
				if (gameObject14 != null)
				{
					ActorBehaviour.GetActorBehaviour(gameObject14).PauseLoco(cPauseLoco_MessageData.m_Pause);
				}
			}
			catch (Exception ex72)
			{
				Debug.LogError("An exception occurred while processing the StopStartLoco message\n" + ex72.Message + "\n" + ex72.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00014", "GUI_ERROR_MAIN_MENU_BUTTON", ex72.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex72.Message);
			}
			break;
		case CMessageData.MessageType.StopLoco:
			try
			{
				CStopLoco_MessageData cStopLoco_MessageData = (CStopLoco_MessageData)message;
				GameObject gameObject13 = FindClientActorGameObject(cStopLoco_MessageData.m_ActorSpawningMessage);
				if (gameObject13 != null)
				{
					ActorBehaviour.GetActorBehaviour(gameObject13).StopLoco(null);
				}
			}
			catch (Exception ex70)
			{
				Debug.LogError("An exception occurred while processing the StopStartLoco message\n" + ex70.Message + "\n" + ex70.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00014", "GUI_ERROR_MAIN_MENU_BUTTON", ex70.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex70.Message);
			}
			break;
		case CMessageData.MessageType.UpdatePropTransparency:
			try
			{
				CUpdatePropTransparency_MessageData cUpdatePropTransparency_MessageData = (CUpdatePropTransparency_MessageData)message;
				for (int num9 = ScenarioManager.CurrentScenarioState.TransparentProps.Count - 1; num9 >= 0; num9--)
				{
					CObjectObstacle cObjectObstacle = ScenarioManager.CurrentScenarioState.TransparentProps[num9];
					List<TileIndex> pathingBlockers = cObjectObstacle.PathingBlockers;
					CActor cActor = null;
					foreach (TileIndex item7 in pathingBlockers)
					{
						CTile cTile = ScenarioManager.Tiles[item7.X, item7.Y];
						CActor cActor2 = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
						if (cActor2 != null && (cObjectObstacle.RuntimeAttachedActor == null || cActor2.ActorGuid != cObjectObstacle.RuntimeAttachedActor.ActorGuid))
						{
							cActor = cActor2;
						}
					}
					if (cActor == null)
					{
						ScenarioManager.CurrentScenarioState.TransparentProps.Remove(cObjectObstacle);
						GameObject propObject3 = Singleton<ObjectCacheService>.Instance.GetPropObject(cObjectObstacle);
						if (propObject3 != null)
						{
							Debug.Log("Prop found for transparency");
							CustomObjectPositionToChildMaterials componentInChildren = propObject3.GetComponentInChildren<CustomObjectPositionToChildMaterials>();
							if (componentInChildren != null)
							{
								Debug.Log("Script found for transparency");
								componentInChildren.fadeSourceObject = null;
								componentInChildren.enableFade = false;
								Debug.Log("Transparency disabled");
							}
							else
							{
								Debug.LogError("Could not find CustomObjectPositionToChildMaterial.cs script on Prop\n" + cObjectObstacle.PrefabName);
							}
						}
						else
						{
							Debug.Log("Could not find prop for transparency");
						}
					}
				}
				foreach (CObjectObstacle prop in cUpdatePropTransparency_MessageData.m_PropList)
				{
					if (ScenarioManager.CurrentScenarioState.TransparentProps.Contains(prop))
					{
						continue;
					}
					ScenarioManager.CurrentScenarioState.TransparentProps.Add(prop);
					GameObject propObject4 = Singleton<ObjectCacheService>.Instance.GetPropObject(prop);
					if (propObject4 != null)
					{
						CustomObjectPositionToChildMaterials componentInChildren2 = propObject4.GetComponentInChildren<CustomObjectPositionToChildMaterials>();
						if (componentInChildren2 == null)
						{
							Debug.LogError("Could not find CustomObjectPositionToChildMaterial.cs script on Prop\n" + prop.PrefabName);
							continue;
						}
						GameObject fadeSourceObject = FindClientActorGameObject(cUpdatePropTransparency_MessageData.m_ActorSpawningMessage);
						componentInChildren2.fadeSourceObject = fadeSourceObject;
						componentInChildren2.enableFade = true;
					}
					else
					{
						Debug.Log("Could not find prop");
					}
				}
			}
			catch (Exception ex71)
			{
				Debug.LogError("An exception occurred while processing the UpdatePropTransparency message\n" + ex71.Message + "\n" + ex71.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00015", "GUI_ERROR_MAIN_MENU_BUTTON", ex71.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex71.Message);
			}
			break;
		case CMessageData.MessageType.InvalidMove:
			try
			{
				GUIInterface.s_GUIInterface.SetStatusText("Cannot move player " + message.m_ActorSpawningMessage.ID + " there");
			}
			catch (Exception ex67)
			{
				Debug.LogError("An exception occurred while processing the InvalidMove message\n" + ex67.Message + "\n" + ex67.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00016", "GUI_ERROR_MAIN_MENU_BUTTON", ex67.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex67.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsTeleporting:
			try
			{
				CActorIsTeleporting_MessageData cActorIsTeleporting_MessageData = (CActorIsTeleporting_MessageData)message;
				ClearAllActorEvents();
				GameObject target = FindClientActorGameObject(cActorIsTeleporting_MessageData.m_ActorTeleporting);
				if (cActorIsTeleporting_MessageData.m_TeleportAbility != null && !m_FollowCameraDisabled)
				{
					CameraController.s_CameraController.SmartFocus(target, pauseDuringTransition: true);
					CameraController.s_CameraController.DisableCameraInput(disableInput: true);
				}
				bool animationShouldPlay8 = false;
				CActor animatingActorToWaitFor8 = cActorIsTeleporting_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorIsTeleporting_MessageData.m_TeleportAbility, cActorIsTeleporting_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cActorIsTeleporting_MessageData.m_TeleportAbility?.AnimOverload,
					"TeleportAway",
					"PowerUp"
				}, out animationShouldPlay8, out animatingActorToWaitFor8);
				if (animatingActorToWaitFor8 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay8 ? 10000 : 400, animatingActorToWaitFor8);
				}
			}
			catch (Exception ex68)
			{
				Debug.LogError("An exception occurred while processing the ActorIsTeleporting message\n" + ex68.Message + "\n" + ex68.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex68.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex68.Message);
			}
			break;
		case CMessageData.MessageType.ActorHasTeleported:
			try
			{
				CActorHasTeleported_MessageData cActorHasTeleported_MessageData = (CActorHasTeleported_MessageData)message;
				ClearAllActorEvents();
				GameObject gameObject9 = FindClientActorGameObject(cActorHasTeleported_MessageData.m_ActorTeleported);
				GameObject gameObject10 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActorHasTeleported_MessageData.m_EndLocation.X, cActorHasTeleported_MessageData.m_EndLocation.Y].m_GameObject;
				Vector3 position = gameObject10.transform.position;
				CameraController.s_CameraController.SmartFocus(gameObject10);
				ActorBehaviour.GetActorBehaviour(gameObject9).TeleportToLocation(position);
				bool animationShouldPlay7 = false;
				CActor animatingActorToWaitFor7 = cActorHasTeleported_MessageData.m_ActorTeleported;
				ProcessActorAnimation(cActorHasTeleported_MessageData.m_TeleportAbility, cActorHasTeleported_MessageData.m_ActorTeleported, new List<string>
				{
					cActorHasTeleported_MessageData.AnimOverload,
					cActorHasTeleported_MessageData.m_TeleportAbility?.AnimOverload,
					"TeleportBack",
					"PowerUp"
				}, out animationShouldPlay7, out animatingActorToWaitFor7);
				if (animatingActorToWaitFor7 != null && !cActorHasTeleported_MessageData.m_skipAnimationState)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay7 ? 10000 : 400, animatingActorToWaitFor7);
				}
			}
			catch (Exception ex66)
			{
				Debug.LogError("An exception occurred while processing the ActorHasTeleported message\n" + ex66.Message + "\n" + ex66.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00018", "GUI_ERROR_MAIN_MENU_BUTTON", ex66.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex66.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsCastingSwap:
			try
			{
				CActorIsCastingSwap_MessageData cActorIsCastingSwap_MessageData = (CActorIsCastingSwap_MessageData)message;
				ClearAllActorEvents();
				bool animationShouldPlay9 = false;
				CActor animatingActorToWaitFor9 = cActorIsCastingSwap_MessageData.m_ActorCasting;
				ProcessActorAnimation(cActorIsCastingSwap_MessageData.m_SwapAbility, cActorIsCastingSwap_MessageData.m_ActorCasting, new List<string>
				{
					cActorIsCastingSwap_MessageData.m_SwapAbility?.AnimOverload,
					"PowerUp"
				}, out animationShouldPlay9, out animatingActorToWaitFor9);
				GameObject gameObject11 = FindClientActorGameObject(cActorIsCastingSwap_MessageData.m_FirstTarget);
				GameObject gameObject12 = FindClientActorGameObject(cActorIsCastingSwap_MessageData.m_SecondTarget);
				GameObject obj = ObjectPool.Spawn(GlobalSettings.Instance.m_GlobalParticles.DefaultCharacterSwap, gameObject11.transform);
				ObjectPool.Recycle(obj, VFXShared.GetEffectLifetime(obj), GlobalSettings.Instance.m_GlobalParticles.DefaultCharacterSwap);
				GameObject obj2 = ObjectPool.Spawn(GlobalSettings.Instance.m_GlobalParticles.DefaultCharacterSwap, gameObject12.transform);
				ObjectPool.Recycle(obj2, VFXShared.GetEffectLifetime(obj2), GlobalSettings.Instance.m_GlobalParticles.DefaultCharacterSwap);
				if (animatingActorToWaitFor9 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay9 ? 10000 : 400, animatingActorToWaitFor9);
				}
			}
			catch (Exception ex69)
			{
				Debug.LogError("An exception occurred while processing the ActorIsCastingSwap message\n" + ex69.Message + "\n" + ex69.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00197", "GUI_ERROR_MAIN_MENU_BUTTON", ex69.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex69.Message);
			}
			break;
		case CMessageData.MessageType.ActorsAreSwapping:
			try
			{
				CActorsAreSwapping_MessageData cActorsAreSwapping_MessageData = (CActorsAreSwapping_MessageData)message;
				ClearAllActorEvents();
				GameObject gameObject112 = FindClientActorGameObject(cActorsAreSwapping_MessageData.m_FirstTarget);
				GameObject gameObject113 = FindClientActorGameObject(cActorsAreSwapping_MessageData.m_SecondTarget);
				GameObject gameObject114 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActorsAreSwapping_MessageData.m_FirstTarget.ArrayIndex.X, cActorsAreSwapping_MessageData.m_FirstTarget.ArrayIndex.Y].m_GameObject;
				GameObject gameObject115 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActorsAreSwapping_MessageData.m_SecondTarget.ArrayIndex.X, cActorsAreSwapping_MessageData.m_SecondTarget.ArrayIndex.Y].m_GameObject;
				Vector3 position5 = gameObject114.transform.position;
				Vector3 position6 = gameObject115.transform.position;
				ActorBehaviour.GetActorBehaviour(gameObject112).TeleportToLocation(position5);
				ActorBehaviour.GetActorBehaviour(gameObject113).TeleportToLocation(position6);
			}
			catch (Exception ex223)
			{
				Debug.LogError("An exception occurred while processing the ActorsAreSwapping message\n" + ex223.Message + "\n" + ex223.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00198", "GUI_ERROR_MAIN_MENU_BUTTON", ex223.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex223.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsSelectingAttackFocusTargets:
			try
			{
				CActorIsSelectingAttackFocusTargets_MessageData messageData12 = (CActorIsSelectingAttackFocusTargets_MessageData)message;
				bool flag12 = false;
				bool flag13 = false;
				string translation16 = LocalizationManager.GetTranslation("GUI_CONFIRM_ATTACK");
				if (messageData12.m_AttackAbility.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
				{
					translation16 = LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION");
				}
				ClearAllActorEvents();
				if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<SelectItemState>())
				{
					if (Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.EnterState is UseActionScenarioState)
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.UseAction);
					}
					else if (Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.EnterState is SelectTargetState)
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
					}
					else if (Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.EnterState is AbilityActionsScenarioState)
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.AbilityActions);
					}
				}
				FocusWorld();
				if (messageData12.m_AttackingActor.Type == CActor.EType.Player && !messageData12.m_AttackAbility.ProcessIfDead)
				{
					s_Choreographer.m_CurrentAbility = messageData12.m_AttackAbility;
					TileBehaviour.SetCallback(TileHandler);
					foreach (GameObject clientActorObject2 in ClientActorObjects)
					{
						if (clientActorObject2 != null)
						{
							ActorBehaviour.GetActorBehaviour(clientActorObject2).m_WorldspacePanelUI.ResetPreview();
						}
					}
					DisableTileSelection(active: false);
					flag13 = true;
					if (messageData12.m_AttackAbility.AreaEffect != null || messageData12.m_AttackAbility.AreaEffectBackup != null)
					{
						readyButton.SetInteractable(interactable: true);
						if (messageData12.m_AttackAbility.MiscAbilityData != null && messageData12.m_AttackAbility.MiscAbilityData.ExactRange.HasValue && messageData12.m_AttackAbility.MiscAbilityData.ExactRange.Value)
						{
							WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: true);
						}
						SetChoreographerState(ChoreographerStateType.WaitingForAreaAttackFocusSelection, 0, null);
						WorldspaceStarHexDisplay.Instance.SetDisplayAbility(messageData12.m_AttackAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.AreaOfEffect);
						WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
						GUIInterface.s_GUIInterface.SetStatusText("Set Area Effect position/rotation to attack with ability strength = " + messageData12.m_AttackAbility.Strength);
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
					}
					else if (messageData12.m_AttackAbility.AllTargetsOnAttackPath && !messageData12.m_AttackAbility.IsMeleeAttack)
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
						TileBehaviour.SetCallback(Waypoint.TileHandler);
						GUIInterface.s_GUIInterface.SetStatusText("Select way points for attack path" + message.m_ActorSpawningMessage.ID + " . Number moves " + messageData12.m_AttackAbility.Range);
						Waypoint.s_MovingActor = messageData12.m_AttackAbility.TargetingActor;
						SetChoreographerState(ChoreographerStateType.WaitingForAreaAttackFocusSelection, 0, null);
						WorldspaceStarHexDisplay.Instance.SetDisplayAbility(messageData12.m_AttackAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.SelectPath);
						WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
						GUIInterface.s_GUIInterface.SetStatusText("Set Select Path to attack with ability strength = " + messageData12.m_AttackAbility.Strength);
					}
					else
					{
						if (messageData12.m_AttackingActor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction)
						{
							Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
						}
						WorldspaceStarHexDisplay.Instance.SetDisplayAbility(messageData12.m_AttackAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.Normal);
						if (WorldspaceStarHexDisplay.Instance.CurrentDisplayState.Equals(WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection))
						{
							WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
						}
						WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
						GUIInterface.s_GUIInterface.SetStatusText("Select target(s) to attack with ability strength = " + messageData12.m_AttackAbility.Strength);
					}
					if (messageData12.m_AttackingActor.Type == CActor.EType.Player)
					{
						if (messageData12.m_AttackAbility.CanClearTargets() || messageData12.m_AttackAbility.UseSubAbilityTargeting)
						{
							Singleton<UIUseItemsBar>.Instance.ShowUsableItems(messageData12.m_AttackingActor, (CItem cItem2) => cItem2.YMLData.Data.CompareAbility != null && cItem2.YMLData.Data.CompareAbility.CompareAbility(messageData12.m_AttackAbility), (messageData12.m_AttackAbility.ActiveSingleTargetItems.Count == 0) ? "GUI_USE_ITEM_ATTACK_TIP" : "GUI_TOOLTIP_ITEM_SINGLE_TARGET_TIP", (messageData12.m_AttackAbility.ActiveSingleTargetItems.Count == 0) ? "GUI_USE_ITEM_TITLE" : "GUI_TOOLTIP_ITEM_SINGLE_TARGET_TITLE");
							if (messageData12.m_AttackAbility.CanApplyActiveBonusTogglesTo() && messageData12.m_AttackAbility.AbilityType != CAbility.EAbilityType.Attack)
							{
								flag12 = true;
							}
							Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(messageData12.m_AttackingActor, messageData12.m_AttackAbility.AbilityType, CActiveBonus.EActiveBonusBehaviourType.None, null, abilityAlreadyStarted: false, showSingleTargetBonus: false, messageData12.m_AttackAbility);
							if (messageData12.m_AttackSummary.AddTargetActiveBonuses.Count > 0 && !messageData12.m_AttackAbility.MaxTargetsSelected())
							{
								string text35 = "";
								for (int num36 = 0; num36 < messageData12.m_AttackSummary.AddTargetActiveBonuses.Count; num36++)
								{
									CActiveBonus cActiveBonus2 = messageData12.m_AttackSummary.AddTargetActiveBonuses[num36];
									text35 += LocalizationManager.GetTranslation(cActiveBonus2.BaseCard.Name);
									if (num36 < messageData12.m_AttackSummary.AddTargetActiveBonuses.Count - 1)
									{
										text35 += "/";
									}
								}
								InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_ADDITIONAL_TARGET_EFFECT"), text35));
							}
							else
							{
								CActiveBonus cActiveBonus3 = messageData12.m_AttackAbility.ActiveSingleTargetActiveBonuses.FirstOrDefault((CActiveBonus it) => it.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.OverrideAbility || it.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.OverrideMoveAbility);
								if (cActiveBonus3 != null && cActiveBonus3.ToggledBonus && cActiveBonus3.SingleTarget == null)
								{
									Singleton<HelpBox>.Instance.Show("GUI_TOOLTIP_SINGLE_TARGET", cActiveBonus3.BaseCard.Name, null, HelpBox.FormatTarget.NONE, "GUI_TOOLTIP_SINGLE_TARGET");
								}
								else
								{
									Singleton<HelpBox>.Instance.Hide("GUI_TOOLTIP_SINGLE_TARGET");
								}
							}
						}
						else
						{
							Singleton<UIUseItemsBar>.Instance.Hide();
							Singleton<UIActiveBonusBar>.Instance.Hide();
						}
						ShowAbilityHelpBoxTooltip(messageData12.m_AttackAbility);
					}
					if (messageData12.m_AttackAbility.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
					{
						bool flag14 = false;
						IEnumerable<CAttackSummary.TargetSummary> enumerable2;
						if (!messageData12.m_AttackAbility.MaxTargetsSelected())
						{
							IEnumerable<CAttackSummary.TargetSummary> targets = messageData12.m_AttackSummary.Targets;
							enumerable2 = targets;
						}
						else
						{
							enumerable2 = messageData12.m_AttackSummary.Targets.Where((CAttackSummary.TargetSummary it) => messageData12.m_AttackAbility.ActorsToTarget.Contains(it.Actor));
						}
						foreach (CAttackSummary.TargetSummary item8 in enumerable2)
						{
							if (!ScenarioManager.Scenario.HasActor(item8.ActorToAttack))
							{
								continue;
							}
							GameObject gameObject110 = s_Choreographer.FindClientActorGameObject(item8.ActorToAttack);
							if (gameObject110 != null)
							{
								flag14 |= item8.UsedAttackMods != null && item8.UsedAttackMods.Exists((AttackModifierYMLData mod) => mod.AddTarget);
								ActorBehaviour.GetActorBehaviour(gameObject110).m_WorldspacePanelUI.OnSelectingAttackFocus(messageData12.m_AttackAbility, messageData12.m_AttackingActor, item8);
							}
						}
						if (flag14)
						{
							InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_TARGET_MODIFIER_TIP"), string.Format("<color=#{1}>{0}</color>", LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_TARGET_MODIFIER"), UIInfoTools.Instance.regularModifierColor.ToHex()));
						}
					}
				}
				else
				{
					GUIInterface.s_GUIInterface.SetStatusText("Monster [" + messageData12.m_AttackingActor.ID + "] trying to attack (" + (messageData12.m_AttackAbility.NumberTargets - messageData12.m_AttackAbility.NumberTargetsRemaining + 1) + " of " + messageData12.m_AttackAbility.NumberTargets + ") target with base stat + ability strength = " + messageData12.m_AttackAbility.Strength);
					bool flag15 = false;
					IEnumerable<CAttackSummary.TargetSummary> enumerable3;
					if (!messageData12.m_AttackAbility.MaxTargetsSelected())
					{
						IEnumerable<CAttackSummary.TargetSummary> targets = messageData12.m_AttackSummary.Targets;
						enumerable3 = targets;
					}
					else
					{
						enumerable3 = messageData12.m_AttackSummary.Targets.Where((CAttackSummary.TargetSummary it) => messageData12.m_AttackAbility.ActorsToTarget.Contains(it.Actor));
					}
					foreach (CAttackSummary.TargetSummary item9 in enumerable3)
					{
						if (!ScenarioManager.Scenario.HasActor(item9.ActorToAttack))
						{
							continue;
						}
						GameObject gameObject111 = s_Choreographer.FindClientActorGameObject(item9.ActorToAttack);
						if (gameObject111 != null)
						{
							flag15 |= item9.UsedAttackMods != null && item9.UsedAttackMods.Exists((AttackModifierYMLData mod) => mod.AddTarget);
							ActorBehaviour.GetActorBehaviour(gameObject111).m_WorldspacePanelUI.OnSelectingAttackFocus(messageData12.m_AttackAbility, messageData12.m_AttackingActor, item9, isEnemyAttacking: true);
						}
					}
					if (flag15)
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_TARGET_MODIFIER_TIP"), string.Format("<color=#{1}>{0}</color>", LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_TARGET_MODIFIER"), UIInfoTools.Instance.regularModifierColor.ToHex()));
					}
				}
				if (!m_FollowCameraDisabled)
				{
					GameObject target6 = FindClientActorGameObject(messageData12.m_AttackingActor);
					CameraController.s_CameraController.SmartFocus(target6, pauseDuringTransition: true);
					CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				}
				if (!flag12)
				{
					if (messageData12.m_AttackAbility.AreaEffect != null)
					{
						if (!WorldspaceStarHexDisplay.Instance.IsAOELocked())
						{
							AbilityData.MiscAbilityData miscAbilityData = messageData12.m_AttackAbility.MiscAbilityData;
							if ((miscAbilityData == null || miscAbilityData.ExactRange != true) && !messageData12.m_AttackAbility.UseSubAbilityTargeting)
							{
								m_SkipButton.Toggle(messageData12.m_AttackingActor.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_ATTACK"), hideOnClick: true, messageData12.m_AttackAbility.CanSkip);
								readyButton.Toggle(messageData12.m_AttackingActor.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, translation16, hideOnClick: true, glowingEffect: true);
								SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && messageData12.m_AttackingActor.Type == CActor.EType.Player);
								goto IL_4ed0;
							}
						}
						m_UndoButton.Toggle(active: true, UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS, LocalizationManager.GetTranslation("GUI_CLEARTARGETS"));
						if (messageData12.m_AttackAbility.CanSkip)
						{
							m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ATTACK"), hideOnClick: true, messageData12.m_AttackAbility.CanSkip);
						}
						m_SkipButton.SetInteractable(messageData12.m_AttackAbility.CanSkip);
						readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMTARGETS, LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS"), hideOnClick: true, glowingEffect: true);
						readyButton.SetInteractable(messageData12.m_AttackAbility.EnoughTargetsSelected() && !messageData12.m_AttackAbility.IsWaitingForSingleTargetItem() && !messageData12.m_AttackAbility.IsWaitingForSingleTargetActiveBonus());
						SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
					}
					else if (!messageData12.m_AttackAbility.ProcessIfDead)
					{
						if (messageData12.m_AttackAbility.NumberTargets - messageData12.m_AttackAbility.NumberTargetsRemaining > 0)
						{
							m_SkipButton.Toggle(messageData12.m_AttackingActor.Type == CActor.EType.Player, null, hideOnClick: true, messageData12.m_AttackAbility.CanSkip);
							readyButton.Toggle(messageData12.m_AttackingActor.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMTARGETS, string.Format("{0} {1}/{2}", LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS"), messageData12.m_AttackAbility.NumberTargets - messageData12.m_AttackAbility.NumberTargetsRemaining, messageData12.m_AttackAbility.NumberTargets), hideOnClick: true, glowingEffect: true);
							readyButton.SetInteractable(messageData12.m_AttackAbility.EnoughTargetsSelected() && !messageData12.m_AttackAbility.IsWaitingForSingleTargetItem() && !messageData12.m_AttackAbility.IsWaitingForSingleTargetActiveBonus());
							SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && messageData12.m_AttackingActor.Type == CActor.EType.Player);
						}
						else
						{
							m_SkipButton.Toggle(messageData12.m_AttackingActor.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_ATTACK"), hideOnClick: true, messageData12.m_AttackAbility.CanSkip);
							readyButton.Toggle(messageData12.m_AttackingActor.Type == CActor.EType.Player, (messageData12.m_AttackAbility.ActorsToTarget.Count > 0) ? ReadyButton.EButtonState.EREADYBUTTONCONFIRM : ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, translation16, hideOnClick: true, glowingEffect: true);
							readyButton.SetInteractable(messageData12.m_AttackingActor.Type == CActor.EType.Player && messageData12.m_AttackAbility.EnoughTargetsSelected() && !messageData12.m_AttackAbility.IsWaitingForSingleTargetItem() && !messageData12.m_AttackAbility.IsWaitingForSingleTargetActiveBonus());
							SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && messageData12.m_AttackingActor.Type == CActor.EType.Player);
						}
						AbilityData.ActiveBonusData activeBonusData = messageData12.m_AttackAbility.ActiveBonusData;
						if (activeBonusData != null)
						{
							_ = activeBonusData.Behaviour;
							if (true)
							{
								AbilityData.ActiveBonusData activeBonusData2 = messageData12.m_AttackAbility.ActiveBonusData;
								if (activeBonusData2 != null && activeBonusData2.Behaviour == CActiveBonus.EActiveBonusBehaviourType.BuffAttack)
								{
									m_SkipButton.Toggle(messageData12.m_AttackingActor.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"), hideOnClick: true, messageData12.m_AttackAbility.CanSkip);
									readyButton.Toggle(messageData12.m_AttackingActor.Type == CActor.EType.Player, (messageData12.m_AttackAbility.ActorsToTarget.Count > 0) ? ReadyButton.EButtonState.EREADYBUTTONCONFIRM : ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
									readyButton.SetInteractable(messageData12.m_AttackAbility.EnoughTargetsSelected() && !messageData12.m_AttackAbility.IsWaitingForSingleTargetItem() && !messageData12.m_AttackAbility.IsWaitingForSingleTargetActiveBonus());
									SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && messageData12.m_AttackingActor.Type == CActor.EType.Player);
								}
							}
						}
						if (messageData12.m_AttackAbility.ActorsToTarget.Count > 0 && messageData12.m_AttackingActor.Type == CActor.EType.Player && !messageData12.m_AttackAbility.ProcessIfDead)
						{
							s_Choreographer.m_UndoButton.Toggle(FirstAbility && messageData12.m_AttackAbility.CanUndo, (!messageData12.m_AttackAbility.AllTargets) ? UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS : UndoButton.EButtonState.EUNDOBUTTONUNDO, messageData12.m_AttackAbility.AllTargets ? LocalizationManager.GetTranslation("GUI_UNDO") : LocalizationManager.GetTranslation("GUI_CLEARTARGETS"));
						}
						else if (messageData12.m_AttackAbility.ChainAttack && messageData12.m_AttackingActor.Type == CActor.EType.Player)
						{
							s_Choreographer.m_UndoButton.Toggle(FirstAbility && messageData12.m_AttackAbility.CanUndo, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
						}
					}
					goto IL_4ed0;
				}
				goto IL_4efe;
				IL_4ed0:
				if (messageData12.m_AttackingActor.Type != CActor.EType.Player || messageData12.m_AttackAbility.ProcessIfDead)
				{
					ScenarioRuleClient.StepComplete();
				}
				goto IL_4efe;
				IL_4efe:
				if (messageData12.m_AttackingActor.Type == CActor.EType.Player && ShowAugmentationBar)
				{
					CardsActionControlller.s_Instance.RefreshConsumeBar();
				}
				if (FFSNetwork.IsOnline && flag13)
				{
					if (!messageData12.m_AttackingActor.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.TargetSelection);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.TargetSelection);
					}
				}
			}
			catch (Exception ex222)
			{
				Debug.LogError("An exception occurred while processing the ActorIsSelectingAttackFocusTargets message\n" + ex222.Message + "\n" + ex222.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00019", "GUI_ERROR_MAIN_MENU_BUTTON", ex222.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex222.Message);
			}
			break;
		case CMessageData.MessageType.ActorSelectedAttackFocus:
			try
			{
				CActorSelectedAttackFocus_MessageData cActorSelectedAttackFocus_MessageData = (CActorSelectedAttackFocus_MessageData)message;
				if (cActorSelectedAttackFocus_MessageData.m_Ability.AreaEffect != null)
				{
					if (WorldspaceStarHexDisplay.Instance.IsAOELocked())
					{
						if (cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player)
						{
							m_SkipButton.SetInteractable(active: false);
						}
						else
						{
							m_SkipButton.Toggle(active: false);
						}
						readyButton.Toggle(cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMTARGETS, LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS"), hideOnClick: true, glowingEffect: true);
						SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player);
					}
					else
					{
						m_SkipButton.Toggle(cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_ATTACK"), hideOnClick: true, cActorSelectedAttackFocus_MessageData.m_Ability.CanSkip);
						readyButton.Toggle(cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ATTACK"), hideOnClick: true, glowingEffect: true);
						SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player);
					}
				}
				else if (cActorSelectedAttackFocus_MessageData.m_Ability.NumberTargets > 0)
				{
					if (cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player)
					{
						m_SkipButton.SetInteractable(active: false);
					}
					else
					{
						m_SkipButton.Toggle(active: false);
					}
					CAbility ability8 = cActorSelectedAttackFocus_MessageData.m_Ability;
					bool flag9 = ability8.NumberTargetsRemaining == 0;
					bool flag10 = cActorSelectedAttackFocus_MessageData.m_Ability is CAbilityTargeting cAbilityTargeting3 && cAbilityTargeting3.OneTargetAtATime;
					bool flag11 = flag9 || flag10;
					int num33 = ((ability8 is CAbilityAttack cAbilityAttack2) ? cAbilityAttack2.AttackSummary.ActiveBonusAddTargetBuff : 0);
					int num34 = ability8.NumberTargets + num33;
					int value2 = num34 - ability8.NumberTargetsRemaining;
					int num35 = Mathf.Clamp(value2, 0, num34);
					string translation15 = LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS");
					string text33 = $"{translation15} {num35}/{num34}";
					string text34 = (flag11 ? translation15 : text33);
					readyButton.Toggle(cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMTARGETS, text34, hideOnClick: true, glowingEffect: true);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player);
				}
				else
				{
					m_SkipButton.Toggle(cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_ATTACK"), hideOnClick: true, cActorSelectedAttackFocus_MessageData.m_Ability.CanSkip);
					readyButton.Toggle(cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ATTACK"), hideOnClick: true, glowingEffect: true);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && cActorSelectedAttackFocus_MessageData.m_AttackingActor.Type == CActor.EType.Player);
				}
			}
			catch (Exception ex221)
			{
				Debug.LogError("An exception occurred while processing the ActorSelectedAttackFocus message\n" + ex221.Message + "\n" + ex221.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex221.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex221.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsSelectingActiveBonusBuffTarget:
			try
			{
				CActorIsSelectingActiveBonusBuffTarget_MessageData cActorIsSelectingActiveBonusBuffTarget_MessageData = (CActorIsSelectingActiveBonusBuffTarget_MessageData)message;
				InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_ACTIVEBONUS_TARGET_SELECTION_" + cActorIsSelectingActiveBonusBuffTarget_MessageData.m_BaseCardID)), cActorIsSelectingActiveBonusBuffTarget_MessageData.m_ActiveBonusName);
				DisableTileSelection(active: false);
				TileBehaviour.SetCallback(TileHandler);
				WorldspaceStarHexDisplay.Instance.LockView = false;
				WorldspaceStarHexDisplay.Instance.ClearCachedAbilityTiles();
				WorldspaceStarHexDisplay.Instance.SetDisplayAbility(cActorIsSelectingActiveBonusBuffTarget_MessageData.m_AttackAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.Normal);
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
				WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.TargetSelection);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.TargetSelection);
					}
				}
			}
			catch (Exception ex220)
			{
				Debug.LogError("An exception occurred while processing the ActorIsSelectingActiveBonusBuffTarget message\n" + ex220.Message + "\n" + ex220.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex220.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex220.Message);
			}
			break;
		case CMessageData.MessageType.UpdateAttackFocusAfterAttackEffectInlineSubAbility:
			try
			{
				CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = (CUpdateAttackFocusAfterAttackEffectInlineSubAbility)message;
				List<CActor> list4 = new List<CActor>();
				if (cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility.ValidActorsInRange != null)
				{
					list4.AddRange(cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility.ValidActorsInRange);
				}
				if (cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility.ActorsToTarget != null)
				{
					list4.AddRange(cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility.ActorsToTarget);
				}
				list4 = list4.Distinct().ToList();
				int targetIndex5 = 0;
				foreach (CActor item10 in list4)
				{
					CAttackSummary.TargetSummary targetSummary4 = cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary.FindTarget(item10, ref targetIndex5);
					if (targetSummary4 != null && ScenarioManager.Scenario.HasActor(targetSummary4.ActorToAttack))
					{
						GameObject gameObject109 = s_Choreographer.FindClientActorGameObject(targetSummary4.ActorToAttack);
						if (gameObject109 != null)
						{
							ActorBehaviour.GetActorBehaviour(gameObject109).m_WorldspacePanelUI.OnSelectingAttackFocus(cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility, cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor, targetSummary4);
						}
					}
				}
				Singleton<TakeDamagePanel>.Instance.RefreshDamageTooltip();
				if (Singleton<TakeDamagePanel>.Instance.ThisPlayerHasTakeDamageControl)
				{
					Singleton<TakeDamagePanel>.Instance.RefreshDamagePreview();
				}
			}
			catch (Exception ex219)
			{
				Debug.LogError("An exception occurred while processing the UpdateAttackFocusAfterAttackEffectInlineSubAbility message\n" + ex219.Message + "\n" + ex219.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00022", "GUI_ERROR_MAIN_MENU_BUTTON", ex219.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex219.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsAttacking:
			try
			{
				CurrentAttackTargets.Clear();
				CurrentAttackArea.Clear();
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				CActorIsAttacking_MessageData cActorIsAttacking_MessageData = (CActorIsAttacking_MessageData)message;
				m_CurrentAbility = cActorIsAttacking_MessageData.m_AttackAbility;
				if (cActorIsAttacking_MessageData.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					Singleton<UIUseItemsBar>.Instance.Hide();
					Singleton<UIActiveBonusBar>.Instance.Hide();
					Singleton<UIUseAugmentationsBar>.Instance.Hide();
				}
				if (cActorIsAttacking_MessageData.m_AttackAbility.OnDeath)
				{
					m_CurrentOnDeathActor = cActorIsAttacking_MessageData.m_AttackingActor;
				}
				else
				{
					m_CurrentOnDeathActor = null;
				}
				ClearAllActorEvents();
				if (!m_FollowCameraDisabled)
				{
					CameraController.s_CameraController.DisableCameraInput(disableInput: true);
				}
				Vector3 zero7 = Vector3.zero;
				foreach (CActor item11 in cActorIsAttacking_MessageData.m_ActorsAttacking)
				{
					GameObject gameObject107 = FindClientActorGameObject(item11);
					if (gameObject107 != null)
					{
						zero7 += gameObject107.transform.position;
						CurrentAttackTargets.Add(gameObject107);
					}
				}
				zero7 /= (float)cActorIsAttacking_MessageData.m_ActorsAttacking.Count;
				if (FindClientActorGameObject(cActorIsAttacking_MessageData.m_AttackingActor) != null)
				{
					FindClientActorGameObject(cActorIsAttacking_MessageData.m_AttackingActor).transform.LookAt(zero7);
				}
				m_ActorsBeingTargetedForVFX = ((cActorIsAttacking_MessageData.m_AttackAbility.AreaEffectBackup != null) ? cActorIsAttacking_MessageData.m_AttackAbility.ActorsToTarget.ToList() : cActorIsAttacking_MessageData.m_ActorsAttacking);
				WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(m_ActorsBeingTargetedForVFX);
				AttackModBar.s_LongestAttackModSequence = 1;
				if (cActorIsAttacking_MessageData.m_AttackAbility.AreaEffect != null || cActorIsAttacking_MessageData.m_AttackAbility.AreaEffectBackup != null)
				{
					int targetIndex3 = 0;
					foreach (CActor item12 in cActorIsAttacking_MessageData.m_ActorsAttacking)
					{
						CAttackSummary.TargetSummary targetSummary2 = cActorIsAttacking_MessageData.m_AttackSummary.FindTarget(item12, ref targetIndex3);
						if (targetSummary2 != null)
						{
							if (AttackModBar.s_LongestAttackModSequence < targetSummary2.UsedAttackMods.Count)
							{
								AttackModBar.s_LongestAttackModSequence = targetSummary2.UsedAttackMods.Count;
							}
							ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(item12)).m_WorldspacePanelUI.DisplayAttackModifierFlow(targetSummary2, cActorIsAttacking_MessageData.m_AttackAbility);
							if (cActorIsAttacking_MessageData.m_AttackAbility.UseSubAbilityTargeting)
							{
								AttackModBar.s_AttackModifierBarFlowCanBegin = true;
							}
						}
						else
						{
							Debug.LogError("Unable to find Attack Summary for " + item12.GetPrefabName());
						}
					}
					SetChoreographerState(ChoreographerStateType.WaitingForAttackModifierCards, 0, null);
				}
				else
				{
					int targetIndex4 = 0;
					foreach (CActor item13 in cActorIsAttacking_MessageData.m_ActorsAttacking)
					{
						CAttackSummary.TargetSummary targetSummary3 = cActorIsAttacking_MessageData.m_AttackSummary.FindTarget(item13, ref targetIndex4);
						if (targetSummary3 != null)
						{
							GameObject gameObject108 = FindClientActorGameObject(item13);
							if (gameObject108 != null)
							{
								if (AttackModBar.s_LongestAttackModSequence < targetSummary3.UsedAttackMods.Count)
								{
									AttackModBar.s_LongestAttackModSequence = targetSummary3.UsedAttackMods.Count;
								}
								ActorBehaviour.GetActorBehaviour(gameObject108).m_WorldspacePanelUI.DisplayAttackModifierFlow(targetSummary3, cActorIsAttacking_MessageData.m_AttackAbility);
								if (cActorIsAttacking_MessageData.m_AttackAbility.UseSubAbilityTargeting)
								{
									AttackModBar.s_AttackModifierBarFlowCanBegin = true;
								}
							}
						}
						else
						{
							Debug.LogError("Unable to find Attack Summary for " + item13.GetPrefabName());
						}
					}
					SetChoreographerState(ChoreographerStateType.WaitingForAttackModifierCards, 0, null);
				}
				if (!m_FollowCameraDisabled)
				{
					if (FindClientActorGameObject(cActorIsAttacking_MessageData.m_AttackingActor) != null)
					{
						CameraController.s_CameraController.SetOptimalViewPoint(FindClientActorGameObject(cActorIsAttacking_MessageData.m_AttackingActor).transform.position, zero7, cActorIsAttacking_MessageData.m_ActorSpawningMessage.Type);
					}
					if (!cActorIsAttacking_MessageData.m_AttackAbility.IsMeleeAttack)
					{
						if (cActorIsAttacking_MessageData.m_AttackAbility.AreaEffect == null)
						{
							CameraController.s_CameraController.SetFocalPointBetweenTwoObjects(CurrentAttackTargets[0], FindClientActorGameObject(cActorIsAttacking_MessageData.m_AttackingActor));
						}
					}
					else if (CurrentAttackTargets.Count > 0)
					{
						CameraController.s_CameraController.SmartFocus(CurrentAttackTargets[0], pauseDuringTransition: true);
					}
				}
				if (!(FindClientActorGameObject(cActorIsAttacking_MessageData.m_AttackingActor) != null))
				{
					break;
				}
				WeaponCollision[] componentsInChildren3 = FindClientActorGameObject(cActorIsAttacking_MessageData.m_AttackingActor).GetComponentsInChildren<WeaponCollision>();
				if (componentsInChildren3 != null)
				{
					WeaponCollision[] array2 = componentsInChildren3;
					foreach (WeaponCollision weaponCollision2 in array2)
					{
						weaponCollision2.enabled = true;
					}
				}
			}
			catch (Exception ex218)
			{
				Debug.LogError("An exception occurred while processing the ActorIsAttacking message\n" + ex218.Message + "\n" + ex218.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00023", "GUI_ERROR_MAIN_MENU_BUTTON", ex218.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex218.Message);
			}
			break;
		case CMessageData.MessageType.TargetAbilityRange:
			try
			{
				CTargetAbilityRange_MessageData cTargetAbilityRange_MessageData = (CTargetAbilityRange_MessageData)message;
				if (cTargetAbilityRange_MessageData.m_TargetAbilityRange.Count <= 0)
				{
					break;
				}
				foreach (CTile item14 in cTargetAbilityRange_MessageData.m_TargetAbilityRange)
				{
					CurrentAttackArea.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item14.m_ArrayIndex.X, item14.m_ArrayIndex.Y]);
				}
			}
			catch (Exception ex217)
			{
				Debug.LogError("An exception occurred while processing the TargetAbilityRange message\n" + ex217.Message + "\n" + ex217.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00024", "GUI_ERROR_MAIN_MENU_BUTTON", ex217.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex217.Message);
			}
			break;
		case CMessageData.MessageType.ActorHasAttacked:
			try
			{
				CActorHasAttacked_MessageData cActorHasAttacked_MessageData = (CActorHasAttacked_MessageData)message;
				ClearAllActorEvents();
				if (cActorHasAttacked_MessageData.m_AttackAbility.OnDeath)
				{
					m_CurrentOnDeathActor = cActorHasAttacked_MessageData.m_AttackingActor;
				}
				else
				{
					m_CurrentOnDeathActor = null;
				}
				if (m_CurrentOnDeathActor != null)
				{
					SimpleLog.AddToSimpleLog("ActorHasAttacked is processing an OnDeath attack");
				}
				if (ScenarioManager.Scenario.HasActor(cActorHasAttacked_MessageData.m_AttackingActor))
				{
					CurrentAttackArea.Clear();
					if (cActorHasAttacked_MessageData.m_AttackAbility.AreaEffect != null || cActorHasAttacked_MessageData.m_AttackAbility.AreaEffectBackup != null)
					{
						foreach (CTile item15 in cActorHasAttacked_MessageData.m_AttackAbility.ValidTilesInAreaAffected)
						{
							CurrentAttackArea.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item15.m_ArrayIndex.X, item15.m_ArrayIndex.Y]);
						}
						if (cActorHasAttacked_MessageData.m_AttackAbility.AreaEffectBackup != null)
						{
							foreach (CActor item16 in cActorHasAttacked_MessageData.m_AttackAbility.ActorsToTarget)
							{
								CClientTile item5 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item16.ArrayIndex.X, item16.ArrayIndex.Y];
								if (!CurrentAttackArea.Contains(item5))
								{
									CurrentAttackArea.Add(item5);
								}
							}
						}
						if (cActorHasAttacked_MessageData.m_AttackingActor.Type != CActor.EType.Player)
						{
							WorldspaceStarHexDisplay.Instance.SetDisplayAbility(cActorHasAttacked_MessageData.m_AttackAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.EnemyAreaOfEffect);
						}
					}
					else
					{
						foreach (CTile item17 in cActorHasAttacked_MessageData.m_AttackAbility.TilesInRange)
						{
							CurrentAttackArea.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item17.m_ArrayIndex.X, item17.m_ArrayIndex.Y]);
						}
					}
					if (!cActorHasAttacked_MessageData.m_AttackAbility.AllTargetsOnMovePath)
					{
						bool animationShouldPlay47 = false;
						CActor animatingActorToWaitFor47 = cActorHasAttacked_MessageData.m_AttackingActor;
						ProcessActorAnimation(cActorHasAttacked_MessageData.m_AttackAbility, cActorHasAttacked_MessageData.m_AttackingActor, new List<string> { cActorHasAttacked_MessageData.AnimOverload, "Attack" }, out animationShouldPlay47, out animatingActorToWaitFor47);
						if (animatingActorToWaitFor47 != null)
						{
							SetChoreographerState(ChoreographerStateType.WaitingForAttackAnim, animationShouldPlay47 ? 10000 : 400, animatingActorToWaitFor47);
						}
						if (!animationShouldPlay47)
						{
							GameObject gameObject106 = FindClientActorGameObject(animatingActorToWaitFor47);
							if (gameObject106 != null)
							{
								ActorEvents.GetActorEvents(gameObject106).ProgressChoreographer();
							}
						}
					}
					else
					{
						m_SMB_Control_WaitingForAttackAnim = false;
						SetChoreographerState(ChoreographerStateType.WaitingForAttackAnim, (int)GlobalSettings.Instance.m_AttackModifierSettings.SlowMoDuration * 1000, cActorHasAttacked_MessageData.m_AttackingActor);
					}
					LogConsumes(cActorHasAttacked_MessageData.m_AttackingActor, cActorHasAttacked_MessageData.m_AttackingActor.Type, cActorHasAttacked_MessageData.m_AttackAbility, "Attack", "COMBAT_LOG_CONSUME_DAMAGE", "COMBAT_LOG_CONSUME_DAMAGE_NOELEMENT");
				}
				else
				{
					ScenarioRuleClient.StepComplete();
				}
			}
			catch (Exception ex216)
			{
				Debug.LogError("An exception occurred while processing the ActorHasAttacked message\n" + ex216.Message + "\n" + ex216.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00025", "GUI_ERROR_MAIN_MENU_BUTTON", ex216.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex216.Message);
			}
			break;
		case CMessageData.MessageType.ActorBeenAttacked:
			try
			{
				CActorBeenAttacked_MessageData cActorBeenAttacked_MessageData = (CActorBeenAttacked_MessageData)message;
				if (ScenarioManager.Scenario.HasActor(cActorBeenAttacked_MessageData.m_AttackingActor))
				{
					GameObject gameObject104 = FindClientActorGameObject(cActorBeenAttacked_MessageData.m_AttackingActor);
					if (gameObject104 != null)
					{
						WeaponCollision[] componentsInChildren2 = gameObject104.GetComponentsInChildren<WeaponCollision>();
						if (componentsInChildren2 != null)
						{
							WeaponCollision[] array2 = componentsInChildren2;
							foreach (WeaponCollision weaponCollision in array2)
							{
								weaponCollision.enabled = false;
							}
						}
					}
				}
				FFSNet.Console.Log("Actor being attacked: " + cActorBeenAttacked_MessageData.m_ActorBeingAttacked.GetPrefabName());
				GameObject gameObject105 = FindClientActorGameObject(cActorBeenAttacked_MessageData.m_ActorBeingAttacked);
				bool animationShouldPlay46 = false;
				CActor animatingActorToWaitFor46 = cActorBeenAttacked_MessageData.m_ActorBeingAttacked;
				ProcessActorAnimation(cActorBeenAttacked_MessageData.m_AttackAbility, cActorBeenAttacked_MessageData.m_ActorBeingAttacked, new List<string>
				{
					cActorBeenAttacked_MessageData.m_ActorWasAsleep ? "SleepWakeUp" : "Hit",
					"Hit"
				}, out animationShouldPlay46, out animatingActorToWaitFor46);
				if (cActorBeenAttacked_MessageData.m_ActorBeingAttacked.Type != CActor.EType.Player)
				{
					ActorBehaviour.UpdateHealth(gameObject105, cActorBeenAttacked_MessageData.m_ActorOriginalHealth, updateUI: true);
				}
				m_CurrentActorBeingAttacked = cActorBeenAttacked_MessageData.m_ActorBeingAttacked;
				m_CurrentOnDeathActor = null;
				if (DebugMenu.DebugMenuNotNull)
				{
					DebugMenu.Instance.AttackValueOverride = int.MaxValue;
				}
				CurrentAttackArea.Clear();
				SEventAbility sEventAbility17 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Attack, ESESubTypeAbility.None, checkQueue: true);
				SEventAttackModifier sEventAttackModifier2 = SEventLog.FindLastAttackModifierEventWithID(cActorBeenAttacked_MessageData.m_ActorBeingAttacked.ID, cActorBeenAttacked_MessageData.m_ActorBeingAttacked.Class.ID, checkQueue: true, cActorBeenAttacked_MessageData.m_AttackIndex);
				if (sEventAbility17 != null && sEventAttackModifier2 != null)
				{
					SEventAbilityAttack attackAbilityEvent2 = (SEventAbilityAttack)sEventAbility17;
					LogDamageCalculation(attackAbilityEvent2, sEventAttackModifier2, cActorBeenAttacked_MessageData.m_AttackingActor, cActorBeenAttacked_MessageData.m_ActorBeingAttacked, cActorBeenAttacked_MessageData.m_AttackAbility);
				}
			}
			catch (Exception ex215)
			{
				Debug.LogError("An exception occurred while processing the ActorBeenAttacked message\n" + ex215.Message + "\n" + ex215.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00026", "GUI_ERROR_MAIN_MENU_BUTTON", ex215.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex215.Message);
			}
			break;
		case CMessageData.MessageType.ActorBeenAttackedAndKilled:
			try
			{
				CActorBeenAttackedAndKilled_MessageData cActorBeenAttackedAndKilled_MessageData = (CActorBeenAttackedAndKilled_MessageData)message;
				SEventAbility sEventAbility16 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Attack, ESESubTypeAbility.None, checkQueue: true);
				SEventAttackModifier sEventAttackModifier = SEventLog.FindLastAttackModifierEventWithID(cActorBeenAttackedAndKilled_MessageData.m_ActorBeingAttacked.ID, cActorBeenAttackedAndKilled_MessageData.m_ActorBeingAttacked.Class.ID, checkQueue: true, cActorBeenAttackedAndKilled_MessageData.m_AttackIndex);
				m_CurrentOnDeathActor = null;
				if (sEventAbility16 != null && sEventAttackModifier != null)
				{
					SEventAbilityAttack attackAbilityEvent = (SEventAbilityAttack)sEventAbility16;
					LogDamageCalculation(attackAbilityEvent, sEventAttackModifier, cActorBeenAttackedAndKilled_MessageData.m_AttackingActor, cActorBeenAttackedAndKilled_MessageData.m_ActorBeingAttacked, cActorBeenAttackedAndKilled_MessageData.m_AttackAbility);
				}
				GameObject gameObject103 = FindClientActorGameObject(cActorBeenAttackedAndKilled_MessageData.m_ActorBeingAttacked);
				if (gameObject103 != null)
				{
					ActorBehaviour.UpdateHealth(gameObject103);
					ActorBehaviour.GetActorBehaviour(gameObject103).m_WorldspacePanelUI.Hide();
				}
			}
			catch (Exception ex214)
			{
				Debug.LogError("An exception occurred while processing the ActorBeenAttackedAndKilled message\n" + ex214.Message + "\n" + ex214.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00027", "GUI_ERROR_MAIN_MENU_BUTTON", ex214.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex214.Message);
			}
			break;
		case CMessageData.MessageType.InvalidAttack:
			try
			{
				GUIInterface.s_GUIInterface.SetStatusText("Nothing to attack or too far");
				CInvalidAttack_MessageData cInvalidAttack_MessageData = (CInvalidAttack_MessageData)message;
				m_SkipButton.Toggle(cInvalidAttack_MessageData.m_AttackingActor.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_ATTACK"));
				readyButton.Toggle(cInvalidAttack_MessageData.m_AttackingActor.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ATTACK"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && cInvalidAttack_MessageData.m_AttackingActor.Type == CActor.EType.Player);
			}
			catch (Exception ex213)
			{
				Debug.LogError("An exception occurred while processing the InvalidAttack message\n" + ex213.Message + "\n" + ex213.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00028", "GUI_ERROR_MAIN_MENU_BUTTON", ex213.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex213.Message);
			}
			break;
		case CMessageData.MessageType.PlayerWaitForIdle:
			try
			{
				CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData = (CPlayerWaitForIdle_MessageData)message;
				SetChoreographerState(ChoreographerStateType.WaitingForPlayerIdle, 0, cPlayerWaitForIdle_MessageData.m_Actor);
			}
			catch (Exception ex212)
			{
				Debug.LogError("An exception occurred while processing the PlayerWaitForIdle message\n" + ex212.Message + "\n" + ex212.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00029", "GUI_ERROR_MAIN_MENU_BUTTON", ex212.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex212.Message);
			}
			break;
		case CMessageData.MessageType.PlayerSelectingToAvoidDamageOrNot:
			try
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Damage);
				SetActiveAnimationCharacters(isActive: false, onlyForMovedCharacters: true);
				CPlayerSelectingToAvoidDamageOrNot_MessageData cPlayerSelectingToAvoidDamageOrNot_MessageData = (CPlayerSelectingToAvoidDamageOrNot_MessageData)message;
				CActor actorBeingAttacked = cPlayerSelectingToAvoidDamageOrNot_MessageData.m_ActorBeingAttacked;
				CPlayerActor actorToShowCardsFor = cPlayerSelectingToAvoidDamageOrNot_MessageData.m_ActorToShowCardsFor;
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(actorToShowCardsFor);
				GameObject target5 = FindClientActorGameObject(actorBeingAttacked);
				if (InitiativeTrack.Instance.FindInitiativeTrackActor(actorBeingAttacked) is InitiativeTrackPlayerBehaviour initiativeTrackPlayerBehaviour)
				{
					initiativeTrackPlayerBehaviour.ShowWarning(show: true);
				}
				if (!m_FollowCameraDisabled)
				{
					CameraController.s_CameraController.SmartFocus(target5);
				}
				Singleton<UIUseAugmentationsBar>.Instance.Hide();
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				m_UndoButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				m_PlayerSelectingToAvoidDamageOrNot = true;
				if (FFSNetwork.IsOnline)
				{
					Singleton<UIScenarioMultiplayerController>.Instance.OnTakeDamage(actorToShowCardsFor, cPlayerSelectingToAvoidDamageOrNot_MessageData);
					if (!Singleton<TakeDamagePanel>.Instance.ThisPlayerHasTakeDamageControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.TakeDamageConfirmation, savePreviousState: true);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.TakeDamageConfirmation);
					}
				}
				else
				{
					Singleton<TakeDamagePanel>.Instance.Show(actorBeingAttacked, actorToShowCardsFor, cPlayerSelectingToAvoidDamageOrNot_MessageData.m_ModifiedStrength, (cPlayerSelectingToAvoidDamageOrNot_MessageData.m_TargetSummary != null) ? Mathf.Max(0, cPlayerSelectingToAvoidDamageOrNot_MessageData.m_TargetSummary.Pierce - cPlayerSelectingToAvoidDamageOrNot_MessageData.m_TargetSummary.Shield) : 0, cPlayerSelectingToAvoidDamageOrNot_MessageData.m_IsDirectDamage, cPlayerSelectingToAvoidDamageOrNot_MessageData.m_DamagingAbility);
				}
			}
			catch (Exception ex211)
			{
				Debug.LogError("An exception occurred while processing the PlayerSelectingToAvoidDamageOrNot message\n" + ex211.Message + "\n" + ex211.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00030", "GUI_ERROR_MAIN_MENU_BUTTON", ex211.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex211.Message);
			}
			break;
		case CMessageData.MessageType.PlayerSelectedToAvoidDamage:
			try
			{
				CPlayerSelectedToAvoidDamage_MessageData cPlayerSelectedToAvoidDamage_MessageData = (CPlayerSelectedToAvoidDamage_MessageData)message;
				GameObject gameObject102 = FindClientActorGameObject(cPlayerSelectedToAvoidDamage_MessageData.m_ActorBeingAttacked);
				ActorBehaviour.GetActorBehaviour(gameObject102).m_WorldspacePanelUI.FinalizeAttackFlow();
				ActorBehaviour.UpdateHealth(gameObject102);
				string arg47 = LocalizationManager.GetTranslation(cPlayerSelectedToAvoidDamage_MessageData.m_ActorBeingAttacked.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cPlayerSelectedToAvoidDamage_MessageData.m_ActorBeingAttacked);
				switch (cPlayerSelectedToAvoidDamage_MessageData.m_AvoidDamageOption)
				{
				case GameState.EAvoidDamageOption.Lose1HandCard:
				{
					bool isItem6;
					string arg50 = $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationNameConverter.MultiLookupLocalization(cPlayerSelectedToAvoidDamage_MessageData.m_CardsBurnedToAvoidDamage[0].Name, out isItem6)}</color></b></font>";
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_PREVENT_DAMAGE_BURNED_1_HAND"), arg47, arg50));
					break;
				}
				case GameState.EAvoidDamageOption.Lose2DiscardCards:
				{
					bool isItem4;
					string arg48 = $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationNameConverter.MultiLookupLocalization(cPlayerSelectedToAvoidDamage_MessageData.m_CardsBurnedToAvoidDamage[0].Name, out isItem4)}</color></b></font>";
					bool isItem5;
					string arg49 = $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationNameConverter.MultiLookupLocalization(cPlayerSelectedToAvoidDamage_MessageData.m_CardsBurnedToAvoidDamage[1].Name, out isItem5)}</color></b></font>";
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_PREVENT_DAMAGE_BURNED_2_DISCARDED"), arg47, arg48, arg49));
					break;
				}
				}
			}
			catch (Exception ex210)
			{
				Debug.LogError("An exception occurred while processing the PlayerSelectedToAvoidDamage message\n" + ex210.Message + "\n" + ex210.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00031", "GUI_ERROR_MAIN_MENU_BUTTON", ex210.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex210.Message);
			}
			break;
		case CMessageData.MessageType.PlayerSelectedToNotAvoidDamage:
			try
			{
				CPlayerSelectedToNotAvoidDamage_MessageData cPlayerSelectedToNotAvoidDamage_MessageData = (CPlayerSelectedToNotAvoidDamage_MessageData)message;
				GameObject gameObject101 = FindClientActorGameObject(cPlayerSelectedToNotAvoidDamage_MessageData.m_ActorBeingAttacked);
				ActorBehaviour actorBehaviour21 = ActorBehaviour.GetActorBehaviour(gameObject101);
				actorBehaviour21.m_WorldspacePanelUI.FinalizeAttackFlow();
				actorBehaviour21.UpdateHealth(cPlayerSelectedToNotAvoidDamage_MessageData.m_ActorOriginalHealth, updateUI: true);
			}
			catch (Exception ex209)
			{
				Debug.LogError("An exception occurred while processing the PlayerSelectedToNotAvoidDamage message\n" + ex209.Message + "\n" + ex209.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00032", "GUI_ERROR_MAIN_MENU_BUTTON", ex209.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex209.Message);
			}
			break;
		case CMessageData.MessageType.PlayerFinishedSelectingToAvoidDamageOrNot:
			try
			{
				CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData = (CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData)message;
				SetActiveAnimationCharacters(isActive: true);
				if (CurrentActor != null && CurrentActor.Type == CActor.EType.Player && PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction3)
				{
					CAbility ability7 = cPhaseAction3.CurrentPhaseAbility.m_Ability;
					if (ability7.CanReceiveTileSelection())
					{
						m_UndoButton.Toggle(active: true, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
						m_UndoButton.SetInteractable(ability7.CanUndo && FirstAbility);
						m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"), hideOnClick: true, ability7.CanSkip);
						SetActiveSelectButton(activate: true);
						Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(m_CurrentActor);
					}
				}
				readyButton.ClearAlternativeAction();
				Singleton<TakeDamagePanel>.Instance.ClearPreviouslyPreviewed();
				(InitiativeTrack.Instance.FindInitiativeTrackActor(cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData.m_ActorBeingAttacked) as InitiativeTrackPlayerBehaviour)?.ShowWarning(show: false);
				if (m_CurrentActor != null && m_CurrentActor is CPlayerActor cPlayerActor12 && (!m_CurrentActor.m_PlayedThisRound || cPlayerActor12.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction))
				{
					Singleton<UIScenarioMultiplayerController>.Instance.OnFinishTakeDamage(hidePlayerInfo: false);
					CardsHandManager.Instance.Show(cPlayerActor12, CardHandMode.ActionSelection, (!cPlayerActor12.IsTakingExtraTurn) ? CardPileType.Round : CardPileType.ExtraTurn, CardPileType.Any, 0, fadeUnselectableCards: false, highlightSelectableCards: false, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE);
					s_Choreographer.SwitchToSelectActionUiState(cPlayerActor12);
				}
				else
				{
					Singleton<UIScenarioMultiplayerController>.Instance.OnFinishTakeDamage();
				}
				m_PlayerSelectingToAvoidDamageOrNot = false;
				Singleton<TakeDamagePanel>.Instance.ClearTakeDamageInvoked();
				m_AfterTakeDamageProgressMessages.ForEach(ProcessMessage);
				m_AfterTakeDamageProgressMessages.Clear();
			}
			catch (Exception ex208)
			{
				Debug.LogError("An exception occurred while processing the PlayerFinishedSelectingToAvoidDamageOrNot message\n" + ex208.Message + "\n" + ex208.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00033", "GUI_ERROR_MAIN_MENU_BUTTON", ex208.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex208.Message);
			}
			break;
		case CMessageData.MessageType.AttackDone:
			try
			{
				if (m_ActorsBeingTargetedForVFX != null)
				{
					foreach (CActor item18 in m_ActorsBeingTargetedForVFX)
					{
						GameObject gameObject100 = FindClientActorGameObject(item18);
						if (gameObject100 != null)
						{
							ActorBehaviour actorBehaviour20 = ActorBehaviour.GetActorBehaviour(gameObject100);
							if (actorBehaviour20.m_WorldspacePanelUI != null)
							{
								actorBehaviour20.m_WorldspacePanelUI.CancelRetaliateEffect();
							}
						}
					}
					m_ActorsBeingTargetedForVFX.Clear();
				}
				_ = (CAttackDone_MessageData)message;
				if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					WorldspaceStarHexDisplay.Instance.ResetPlayerAttackType();
				}
			}
			catch (Exception ex207)
			{
				Debug.LogError("An exception occurred while processing the AttackDone message\n" + ex207.Message + "\n" + ex207.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00034", "GUI_ERROR_MAIN_MENU_BUTTON", ex207.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex207.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsKilling:
			try
			{
				CActorIsKilling_MessageData cActorIsKilling_MessageData = (CActorIsKilling_MessageData)message;
				ClearAllActorEvents();
				if (cActorIsKilling_MessageData.m_ActorsAppliedTo.Count > 0 && cActorIsKilling_MessageData.m_KillAbility.ValidTilesInAreaAffected != null && cActorIsKilling_MessageData.m_KillAbility.ValidTilesInAreaAffected.Count > 0)
				{
					CurrentAttackArea.Clear();
					foreach (CTile item19 in cActorIsKilling_MessageData.m_KillAbility.ValidTilesInAreaAffected)
					{
						CurrentAttackArea.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item19.m_ArrayIndex.X, item19.m_ArrayIndex.Y]);
					}
				}
				WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(cActorIsKilling_MessageData.m_ActorsAppliedTo);
				GameObject gameObject98 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				Vector3 zero6 = Vector3.zero;
				bool flag8 = false;
				foreach (CActor item20 in cActorIsKilling_MessageData.m_ActorsAppliedTo)
				{
					GameObject gameObject99 = FindClientActorGameObject(item20);
					if (gameObject99 != null)
					{
						zero6 += gameObject99.transform.position;
						flag8 = true;
					}
				}
				if (flag8)
				{
					zero6 /= (float)cActorIsKilling_MessageData.m_ActorsAppliedTo.Count;
					gameObject98.transform.LookAt(zero6);
				}
				bool animationShouldPlay45 = false;
				CActor animatingActorToWaitFor45 = cActorIsKilling_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorIsKilling_MessageData.m_KillAbility, cActorIsKilling_MessageData.m_ActorSpawningMessage, new List<string> { cActorIsKilling_MessageData.AnimOverload, "Attack" }, out animationShouldPlay45, out animatingActorToWaitFor45);
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				Singleton<UIActiveBonusBar>.Instance.Hide();
				if (animatingActorToWaitFor45 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay45 ? 10000 : 400, animatingActorToWaitFor45);
				}
				if (animationShouldPlay45)
				{
					CharacterManager.EnableWeaponColliderForAttack(gameObject98);
				}
				else
				{
					ActorEvents.GetActorEvents(gameObject98).ProgressChoreographer();
				}
			}
			catch (Exception ex206)
			{
				Debug.LogError("An exception occurred while processing the ActorIsKilling message\n" + ex206.Message + "\n" + ex206.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00035", "GUI_ERROR_MAIN_MENU_BUTTON", ex206.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex206.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsHealing:
			try
			{
				CActorIsHealing_MessageData cActorIsHealing_MessageData = (CActorIsHealing_MessageData)message;
				WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(cActorIsHealing_MessageData.m_ActorsHealedAndHealStrength.Keys.ToList());
				string arg45 = LocalizationManager.GetTranslation(cActorIsHealing_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActorIsHealing_MessageData.m_ActorSpawningMessage);
				foreach (KeyValuePair<CActor, int> item21 in cActorIsHealing_MessageData.m_ActorsHealedAndHealStrength)
				{
					string arg46 = LocalizationManager.GetTranslation(item21.Key.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(item21.Key);
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_HEAL"), arg45, arg46, string.Format("<b>{0}</b>", LocalizationManager.GetTranslation("Heal") + " <sprite name=Heal>" + item21.Value)), CombatLogFilter.HEALING);
				}
				ClearAllActorEvents();
				GameObject gameObject96 = ((cActorIsHealing_MessageData.m_ActorsHealedAndHealStrength.Count == 1) ? FindClientActorGameObject(Enumerable.First(cActorIsHealing_MessageData.m_ActorsHealedAndHealStrength).Key) : null);
				GameObject gameObject97 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				if (gameObject96 != null)
				{
					gameObject97.transform.LookAt(gameObject96.transform.position);
				}
				bool animationShouldPlay44 = false;
				CActor animatingActorToWaitFor44 = cActorIsHealing_MessageData.m_ActorSpawningMessage;
				if (!cActorIsHealing_MessageData.m_HealAbility.IsModifierAbility)
				{
					ProcessActorAnimation(cActorIsHealing_MessageData.m_HealAbility, cActorIsHealing_MessageData.m_ActorSpawningMessage, new List<string> { cActorIsHealing_MessageData.AnimOverload, "PowerUp" }, out animationShouldPlay44, out animatingActorToWaitFor44);
				}
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				if (animatingActorToWaitFor44 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay44 ? 10000 : 400, animatingActorToWaitFor44);
				}
				if (!animationShouldPlay44)
				{
					ActorEvents.GetActorEvents(gameObject97).ProgressChoreographer();
				}
				Singleton<UIActiveBonusBar>.Instance.Hide();
			}
			catch (Exception ex205)
			{
				Debug.LogError("An exception occurred while processing the ActorIsHealing message\n" + ex205.Message + "\n" + ex205.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00036", "GUI_ERROR_MAIN_MENU_BUTTON", ex205.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex205.Message);
			}
			break;
		case CMessageData.MessageType.RefreshActorHealth:
			try
			{
				CRefreshActorHealth_MessageData cRefreshActorHealth_MessageData = (CRefreshActorHealth_MessageData)message;
				m_CurrentActorBeingHealed = cRefreshActorHealth_MessageData.m_ActorBeingRefreshed;
				FindClientActorGameObject(message.m_ActorSpawningMessage);
				GameObject gameObject95 = FindClientActorGameObject(m_CurrentActorBeingHealed);
				if (gameObject95 != null)
				{
					ActorBehaviour.UpdateHealth(gameObject95, cRefreshActorHealth_MessageData.m_ActorOriginalHealth, updateUI: true);
				}
			}
			catch (Exception ex204)
			{
				Debug.LogError("An exception occurred while processing the ActorBeenHealed message\n" + ex204.Message + "\n" + ex204.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00037", "GUI_ERROR_MAIN_MENU_BUTTON", ex204.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex204.Message);
			}
			break;
		case CMessageData.MessageType.ActorBeenHealed:
			try
			{
				CActorBeenHealed_MessageData cActorBeenHealed_MessageData = (CActorBeenHealed_MessageData)message;
				m_CurrentActorBeingHealed = cActorBeenHealed_MessageData.m_ActorBeingHealed;
				FindClientActorGameObject(message.m_ActorSpawningMessage);
				GameObject gameObject94 = FindClientActorGameObject(m_CurrentActorBeingHealed);
				GameObject obj28 = ObjectPool.Spawn(GlobalSettings.Instance.m_GlobalParticles.DefaultHealEffect, gameObject94.transform);
				ObjectPool.Recycle(obj28, VFXShared.GetEffectLifetime(obj28), GlobalSettings.Instance.m_GlobalParticles.DefaultHealEffect);
				ActorBehaviour.UpdateHealth(gameObject94, cActorBeenHealed_MessageData.m_ActorOriginalHealth, updateUI: true);
				string arg44 = LocalizationManager.GetTranslation(m_CurrentActorBeingHealed.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(m_CurrentActorBeingHealed);
				StringBuilder stringBuilder6 = new StringBuilder(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_HEALED"), arg44));
				if (cActorBeenHealed_MessageData.m_PoisonTokenRemoved)
				{
					stringBuilder6.AppendFormat(" {0}", LocalizationManager.GetTranslation("COMBAT_LOG_HEALED_POISON") + " <sprite name=Poison>");
				}
				else
				{
					stringBuilder6.AppendFormat(" {0}", string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_HEALED_HEAL"), string.Format("<b>{0}</b>", "<sprite name=Heal>" + cActorBeenHealed_MessageData.m_HealAmount)));
				}
				if (cActorBeenHealed_MessageData.m_WoundTokenRemoved)
				{
					stringBuilder6.AppendFormat(" {0}", LocalizationManager.GetTranslation("COMBAT_LOG_HEALED_WOUND") + " <sprite name=Wound>");
				}
				Singleton<CombatLogHandler>.Instance.AddLog(stringBuilder6.ToString(), CombatLogFilter.HEALING);
			}
			catch (Exception ex203)
			{
				Debug.LogError("An exception occurred while processing the ActorBeenHealed message\n" + ex203.Message + "\n" + ex203.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00037", "GUI_ERROR_MAIN_MENU_BUTTON", ex203.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex203.Message);
			}
			break;
		case CMessageData.MessageType.InvalidHeal:
			try
			{
				GUIInterface.s_GUIInterface.SetStatusText("Nothing to heal or too far");
			}
			catch (Exception ex202)
			{
				Debug.LogError("An exception occurred while processing the InvalidHeal message\n" + ex202.Message + "\n" + ex202.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00038", "GUI_ERROR_MAIN_MENU_BUTTON", ex202.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex202.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsSelectingItemCards:
			try
			{
				CActorIsSelectingItemCards_MessageData cActorIsSelectingItemCards_MessageData = (CActorIsSelectingItemCards_MessageData)message;
				if (cActorIsSelectingItemCards_MessageData.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					WorldspaceStarHexDisplay.Instance.ResetPlayerAttackType();
				}
				ClearAllActorEvents();
				GameObject gameObject92 = ((cActorIsSelectingItemCards_MessageData.m_ActorsRefreshed.Count == 1) ? FindClientActorGameObject(cActorIsSelectingItemCards_MessageData.m_ActorsRefreshed[0]) : null);
				GameObject gameObject93 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				if (gameObject92 != null)
				{
					gameObject93.transform.LookAt(gameObject92.transform.position);
				}
				bool animationShouldPlay43 = false;
				CActor animatingActorToWaitFor43 = cActorIsSelectingItemCards_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorIsSelectingItemCards_MessageData.m_ItemSelectionAbility, cActorIsSelectingItemCards_MessageData.m_ActorSpawningMessage, new List<string> { cActorIsSelectingItemCards_MessageData.AnimOverload, "PowerUp" }, out animationShouldPlay43, out animatingActorToWaitFor43);
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				ClearStars();
				if (animatingActorToWaitFor43 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay43 ? 10000 : 400, animatingActorToWaitFor43);
				}
				if (!animationShouldPlay43)
				{
					ActorEvents.GetActorEvents(gameObject93).ProgressChoreographer();
				}
			}
			catch (Exception ex201)
			{
				Debug.LogError("An exception occurred while processing the ActorIsRefreshingItemCards message\n" + ex201.Message + "\n" + ex201.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00039", "GUI_ERROR_MAIN_MENU_BUTTON", ex201.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex201.Message);
			}
			break;
		case CMessageData.MessageType.ActorHasRefreshedItemCards:
			try
			{
				CActor actorBeenRefreshed2 = ((CActorHasRefreshedItemCards_MessageData)message).m_ActorBeenRefreshed;
				FindClientActorGameObject(message.m_ActorSpawningMessage);
				GameObject gameObject91 = FindClientActorGameObject(actorBeenRefreshed2);
				GameObject obj27 = ObjectPool.Spawn(GlobalSettings.Instance.m_GlobalParticles.DefaultPositiveCondition, gameObject91.transform);
				ObjectPool.Recycle(obj27, VFXShared.GetEffectLifetime(obj27), GlobalSettings.Instance.m_GlobalParticles.DefaultPositiveCondition);
				actorBeenRefreshed2.ActivatePassiveItems(firstLoad: true);
				if (s_Choreographer.m_WaitState.m_State == ChoreographerStateType.WaitingForItemRefresh)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, 10000, message.m_ActorSpawningMessage);
				}
			}
			catch (Exception ex200)
			{
				Debug.LogError("An exception occurred while processing the ActorHasRefreshedItemCards message\n" + ex200.Message + "\n" + ex200.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00040", "GUI_ERROR_MAIN_MENU_BUTTON", ex200.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex200.Message);
			}
			break;
		case CMessageData.MessageType.SelectRefreshOrConsumeItems:
			try
			{
				CSelectRefreshOrConsumeItems_MessageData messageData11 = (CSelectRefreshOrConsumeItems_MessageData)message;
				CActor actorBeenRefreshed = messageData11.m_ActorBeenRefreshed;
				FindClientActorGameObject(message.m_ActorSpawningMessage);
				GameObject gameObject90 = FindClientActorGameObject(actorBeenRefreshed);
				GameObject obj26 = ObjectPool.Spawn(GlobalSettings.Instance.m_GlobalParticles.DefaultPositiveCondition, gameObject90.transform);
				ObjectPool.Recycle(obj26, VFXShared.GetEffectLifetime(obj26), GlobalSettings.Instance.m_GlobalParticles.DefaultPositiveCondition);
				if (messageData11.m_Ability.Strength == CAbilityRefreshItemCards.STRENGTH_ALL)
				{
					IEnumerable<CItem> enumerable = actorBeenRefreshed.Inventory.AllItems.Where((CItem it) => (messageData11.m_SlotStatesToChooseFrom == null || messageData11.m_SlotStatesToChooseFrom.Count == 0 || messageData11.m_SlotStatesToChooseFrom.Contains(it.SlotState)) && (messageData11.m_SlotsToChooseFrom == null || messageData11.m_SlotsToChooseFrom.Count == 0 || messageData11.m_SlotsToChooseFrom.Contains(it.YMLData.Slot)));
					if (messageData11.m_Ability.AbilityType == CAbility.EAbilityType.RefreshItemCards)
					{
						foreach (CItem item22 in enumerable)
						{
							actorBeenRefreshed.Inventory.ReactivateItem(item22, actorBeenRefreshed);
						}
					}
					else
					{
						foreach (CItem item23 in enumerable)
						{
							actorBeenRefreshed.Inventory.UseItem(item23);
						}
					}
				}
				else if (!FFSNetwork.IsOnline || messageData11.m_ActorBeenRefreshed.IsUnderMyControl)
				{
					Singleton<ItemCardRefreshPicker>.Instance.Show(actorBeenRefreshed, messageData11.m_Ability.AbilityType == CAbility.EAbilityType.RefreshItemCards, messageData11.m_SlotStatesToChooseFrom, messageData11.m_SlotsToChooseFrom, messageData11.m_Ability.Strength);
				}
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				m_UndoButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				if (messageData11.m_Ability.Strength == CAbilityRefreshItemCards.STRENGTH_ALL)
				{
					break;
				}
				SetChoreographerState(ChoreographerStateType.WaitingForItemRefresh, 10000000, message.m_ActorSpawningMessage);
				if (FFSNetwork.IsOnline)
				{
					if (!actorBeenRefreshed.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndSwitchBack, (messageData11.m_Ability.AbilityType == CAbility.EAbilityType.RefreshItemCards) ? ActionPhaseType.ItemRefreshing : ActionPhaseType.ItemConsuming);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, (messageData11.m_Ability.AbilityType == CAbility.EAbilityType.RefreshItemCards) ? ActionPhaseType.ItemRefreshing : ActionPhaseType.ItemConsuming);
					}
				}
			}
			catch (Exception ex199)
			{
				Debug.LogError("An exception occurred while processing the SelectRefreshOrConsumeItems message\n" + ex199.Message + "\n" + ex199.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00041", "GUI_ERROR_MAIN_MENU_BUTTON", ex199.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex199.Message);
			}
			break;
		case CMessageData.MessageType.LoseGoalChestRewardChoice:
			try
			{
				CLoseGoalChestRewardChoice_MessageData cLoseGoalChestRewardChoice_MessageData = (CLoseGoalChestRewardChoice_MessageData)message;
				if (ScenarioManager.CurrentScenarioState.GoalChestRewards.Count <= 0 || cLoseGoalChestRewardChoice_MessageData.m_Ability.Strength == CAbilityRefreshItemCards.STRENGTH_ALL)
				{
					break;
				}
				Singleton<ItemRewardLosePicker>.Instance.Show();
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				m_UndoButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				SetChoreographerState(ChoreographerStateType.WaitingForLoseGoalChestRewardSelection, 10000000, message.m_ActorSpawningMessage);
				if (FFSNetwork.IsOnline)
				{
					if (!FFSNetwork.IsHost)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndSwitchBack, ActionPhaseType.LosingItemRewards);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.LosingItemRewards);
					}
				}
			}
			catch (Exception ex198)
			{
				Debug.LogError("An exception occurred while processing the LoseGoalChestRewardChoice message\n" + ex198.Message + "\n" + ex198.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00226", "GUI_ERROR_MAIN_MENU_BUTTON", ex198.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex198.Message);
			}
			break;
		case CMessageData.MessageType.ActorWantsAnActionConfirmation:
		{
			CActorWantsAnActionConfirmation_MessageData cActorWantsAnActionConfirmation_MessageData = (CActorWantsAnActionConfirmation_MessageData)message;
			WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
			WorldspaceStarHexDisplay.Instance.LockView = false;
			WorldspaceStarHexDisplay.Instance.ClearCachedAbilityTiles();
			WorldspaceStarHexDisplay.Instance.ResetPlayerAttackType();
			CardsActionControlller.s_Instance.RefreshConsumeBar();
			m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"), hideOnClick: true, cActorWantsAnActionConfirmation_MessageData.m_Ability.CanSkip);
			m_UndoButton.Toggle(active: true);
			m_UndoButton.SetInteractable(cActorWantsAnActionConfirmation_MessageData.m_Ability.CanUndo && FirstAbility);
			ReadyButton obj23 = readyButton;
			CAbility ability6 = cActorWantsAnActionConfirmation_MessageData.m_Ability;
			obj23.Toggle(active: true, (ability6 != null && ability6.MiscAbilityData?.AllowContinueForNullAbility == true) ? ReadyButton.EButtonState.EREADYBUTTONCONFIRM : ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"));
			SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
			if (FFSNetwork.IsOnline)
			{
				if (!cActorWantsAnActionConfirmation_MessageData.m_ActorSpawningMessage.IsUnderMyControl)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ActionConfirmation);
				}
				else
				{
					ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.ActionConfirmation);
				}
			}
			break;
		}
		case CMessageData.MessageType.Immunity:
		{
			CImmunity_MessageData cImmunity_MessageData = (CImmunity_MessageData)message;
			string arg41 = LocalizationManager.GetTranslation(cImmunity_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cImmunity_MessageData.m_ActorSpawningMessage);
			string text29 = "";
			bool isItem2;
			if (cImmunity_MessageData.negativeCondition != CCondition.ENegativeCondition.NA)
			{
				text29 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_IMMUNE"), LocalizationNameConverter.MultiLookupLocalization(cImmunity_MessageData.m_ImmunityAbility.BaseCard.Name, out isItem2), string.Format("<b>{0}</b>", " " + LocalizationManager.GetTranslation(cImmunity_MessageData.negativeCondition.ToString()) + " <sprite name=" + cImmunity_MessageData.negativeCondition.ToString() + ">"), arg41);
			}
			else if (cImmunity_MessageData.positiveCondition != CCondition.EPositiveCondition.NA)
			{
				text29 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_IMMUNE"), LocalizationNameConverter.MultiLookupLocalization(cImmunity_MessageData.m_ImmunityAbility.BaseCard.Name, out isItem2), string.Format("<b>{0}</b>", " " + LocalizationManager.GetTranslation(cImmunity_MessageData.positiveCondition.ToString()) + " <sprite name=" + cImmunity_MessageData.positiveCondition.ToString() + ">"), arg41);
			}
			Singleton<CombatLogHandler>.Instance.AddLog(text29, CombatLogFilter.CONDITIONS);
			break;
		}
		case CMessageData.MessageType.ActorIsSelectingTargetingFocus:
			try
			{
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = (CActorIsSelectingTargetingFocus_MessageData)message;
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(m_CurrentActor);
				ClearAllActorEvents();
				DisableTileSelection(active: false);
				TileBehaviour.SetCallback(TileHandler);
				m_CurrentActor = cActorIsSelectingTargetingFocus_MessageData.m_ActorSpawningMessage;
				if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (cActorIsSelectingTargetingFocus_MessageData.m_ActorSpawningMessage is CPlayerActor cPlayerActor8)
					{
						bool flag5 = CardsHandManager.Instance.IsShown && !CardsHandManager.Instance.IsShowingPlayerHand(cPlayerActor8);
						if (flag5)
						{
							CardsHandManager.Instance.Show(cPlayerActor8, CardHandMode.ActionSelection, (!cPlayerActor8.IsTakingExtraTurn) ? CardPileType.Round : CardPileType.ExtraTurn, CardPileType.Any, 0, fadeUnselectableCards: false, highlightSelectableCards: false, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE);
						}
						if (flag5 && CurrentActor == cPlayerActor8 && cPlayerActor8.IsTakingExtraTurn)
						{
							s_Choreographer.SwitchToSelectActionUiState(cPlayerActor8);
						}
						else if (InputManager.GamePadInUse)
						{
							Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
							Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
							Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.RefreshAvailable();
						}
					}
					else if (cActorIsSelectingTargetingFocus_MessageData.m_ActorSpawningMessage is CHeroSummonActor)
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
					}
					if (!m_FollowCameraDisabled)
					{
						CameraController.s_CameraController.DisableCameraInput(disableInput: false);
					}
					foreach (GameObject clientActorObject3 in ClientActorObjects)
					{
						if (clientActorObject3 != null)
						{
							ActorBehaviour.GetActorBehaviour(clientActorObject3).m_WorldspacePanelUI.ResetPreview();
						}
					}
					if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AreaEffect != null)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForAreaAttackFocusSelection, 0, null);
						WorldspaceStarHexDisplay.Instance.SetDisplayAbility(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.AreaOfEffect);
						WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
						GUIInterface.s_GUIInterface.SetStatusText("Set Area Effect position/rotation");
					}
					else
					{
						if (cActorIsSelectingTargetingFocus_MessageData.m_ObjectiveRelated)
						{
							WorldspaceStarHexDisplay.Instance.SetDisplayAbility(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.ObjectiveAbility);
						}
						else if (cActorIsSelectingTargetingFocus_MessageData.m_IsPositive)
						{
							WorldspaceStarHexDisplay.Instance.SetDisplayAbility(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.TargetingAbility);
						}
						else
						{
							WorldspaceStarHexDisplay.Instance.SetDisplayAbility(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.NegativeAbility);
						}
						WorldspaceStarHexDisplay.Instance.LockView = false;
						WorldspaceStarHexDisplay.Instance.ClearCachedAbilityTiles();
						WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
						WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
						if (!m_FollowCameraDisabled)
						{
							GameObject target3 = FindClientActorGameObject((cActorIsSelectingTargetingFocus_MessageData.m_CameraFocusActor != null) ? cActorIsSelectingTargetingFocus_MessageData.m_CameraFocusActor : cActorIsSelectingTargetingFocus_MessageData.m_ActorSpawningMessage);
							CameraController.s_CameraController.SmartFocus(target3, pauseDuringTransition: true);
							CameraController.s_CameraController.DisableCameraInput(disableInput: false);
						}
						if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.Heal)
						{
							if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
							{
								GUIInterface.s_GUIInterface.SetStatusText("Select target to heal");
								UpdatePreviewHealForAbility(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility);
								if (!Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<SelectTargetState>())
								{
									Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
								}
							}
							else
							{
								GUIInterface.s_GUIInterface.SetStatusText("Monster trying to heal target with strength = " + cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.ModifiedStrength());
							}
						}
						else if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.Kill)
						{
							foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
							{
								ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(enemy)).m_WorldspacePanelUI.ResetDamagePreview(0);
							}
						}
						else if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.AddDoom)
						{
							Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_DOOM"), string.Format("<color=#{0}>{1}</color>", UIInfoTools.Instance.GetCharacterHexColor(ECharacter.Doomstalker), LocalizationManager.GetTranslation("Doom")));
							foreach (CActor item24 in cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.ValidActorsInRange)
							{
								ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(item24)).m_WorldspacePanelUI.OnSelectingDoomFocus(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility);
							}
						}
						else if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.TransferDooms)
						{
							Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_TRANSFER_DOOM"), $"<color=#{UIInfoTools.Instance.GetCharacterHexColor(ECharacter.Doomstalker)}>{LocalizationManager.GetTranslation(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityBaseCard.Name)}</color>");
							foreach (CActor item25 in cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.ValidActorsInRange)
							{
								ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(item25)).m_WorldspacePanelUI.OnSelectingDoomFocus(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility);
							}
						}
						else if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType != CAbility.EAbilityType.Swap)
						{
							if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.RemoveConditions)
							{
								if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.Strength == -1)
								{
									Singleton<HelpBox>.Instance.Show("GUI_TOOLTIP_REMOVE_CONDITIONS", cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility?.AbilityBaseCard?.Name);
								}
							}
							else if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.Choose)
							{
								foreach (CActor item26 in cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.ValidActorsInRange)
								{
									ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(item26)).m_WorldspacePanelUI.Focus(focus: true);
								}
								Singleton<HelpBox>.Instance.Show($"GUI_CHOOSE_ABILITY_TARGET_{cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityBaseCard.Name}", cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility?.AbilityBaseCard?.Name);
							}
							else if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.Loot)
							{
								if (((CAbilityLoot)cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility).HasPassedState(CAbilityLoot.ELootState.LootTiles))
								{
									Singleton<HelpBox>.Instance.Show("GUI_TOOLTIP_TAKE_QUEST_ITEM", cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility?.AbilityBaseCard?.Name);
								}
							}
							else if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.HelpBoxTooltipLocKey.IsNOTNullOrEmpty())
							{
								ShowAbilityHelpBoxTooltip(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility);
							}
							else
							{
								GUIInterface.s_GUIInterface.SetStatusText("Select target to apply " + cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.GetDescription());
								if (!Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<SelectTargetState>())
								{
									Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
								}
							}
						}
					}
				}
				if (!cActorIsSelectingTargetingFocus_MessageData.m_CanUndo)
				{
					m_UndoButton.Toggle(cActorIsSelectingTargetingFocus_MessageData.m_CanUndo);
				}
				string term = "GUI_SKIP_ABILITY";
				if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.Push)
				{
					term = "GUI_SKIP_PUSH";
				}
				if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.Pull)
				{
					term = "GUI_SKIP_PULL";
				}
				if (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AreaEffect != null)
				{
					m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, LocalizationManager.GetTranslation(term), hideOnClick: true, cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.CanSkip);
					readyButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRM, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true, cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType != CAbility.EAbilityType.Disarm && cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType != CAbility.EAbilityType.ControlActor);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				}
				else
				{
					int num29 = cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.NumberTargets - cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.NumberTargetsRemaining;
					string text28 = ((cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.NumberTargets <= 1 || num29 == 0 || cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility is CAbilityTargeting { OneTargetAtATime: not false }) ? LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS") : string.Format("{0} {1}/{2}", LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS"), num29, cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.NumberTargets));
					m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, LocalizationManager.GetTranslation(term), hideOnClick: true, cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.CanSkip);
					readyButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.ActorsToTarget.Count > 0 && (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.EnoughTargetsSelected() || (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility is CAbilityLoot cAbilityLoot && cAbilityLoot.HasPassedState(CAbilityLoot.ELootState.LootTiles))) && (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType != CAbility.EAbilityType.Push || (cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.AbilityType == CAbility.EAbilityType.Push && cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.ActorsToTarget.Count == 1))) ? ReadyButton.EButtonState.EREADYBUTTONCONFIRM : ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, text28, hideOnClick: true, glowingEffect: true);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				}
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.TargetSelection);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.TargetSelection);
					}
				}
			}
			catch (Exception ex182)
			{
				Debug.LogError("An exception occurred while processing the ActorIsSelectingTargetingFocus message\n" + ex182.Message + "\n" + ex182.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00042", "GUI_ERROR_MAIN_MENU_BUTTON", ex182.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex182.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsSelectingDamageFocus:
			try
			{
				CActorIsSelectingDamageFocusTargets cActorIsSelectingDamageFocusTargets = (CActorIsSelectingDamageFocusTargets)message;
				DisableTileSelection(active: false);
				m_CurrentAbility = cActorIsSelectingDamageFocusTargets.m_DamageAbility;
				if (cActorIsSelectingDamageFocusTargets.m_DamageAbility.ValidActorsInRange == null)
				{
					break;
				}
				TileBehaviour.SetCallback(TileHandler);
				m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player && cActorIsSelectingDamageFocusTargets.m_DamageAbility.CanSkip, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				readyButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, (cActorIsSelectingDamageFocusTargets.m_DamageAbility.ActorsToTarget.Count > 0) ? ReadyButton.EButtonState.EREADYBUTTONCONFIRM : ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				if (cActorIsSelectingDamageFocusTargets.m_DamageAbility.ActorsToTarget.Count > 0 && cActorIsSelectingDamageFocusTargets.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					s_Choreographer.m_UndoButton.Toggle(FirstAbility && cActorIsSelectingDamageFocusTargets.m_DamageAbility.CanUndo, UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS, LocalizationManager.GetTranslation("GUI_CLEARTARGETS"));
				}
				else
				{
					s_Choreographer.m_UndoButton.Toggle(FirstAbility && cActorIsSelectingDamageFocusTargets.m_DamageAbility.CanUndo, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
				}
				if (cActorIsSelectingDamageFocusTargets.m_DamageAbility.AreaEffect != null)
				{
					WorldspaceStarHexDisplay.Instance.SetDisplayAbility(cActorIsSelectingDamageFocusTargets.m_DamageAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.AreaOfEffect);
				}
				else
				{
					WorldspaceStarHexDisplay.Instance.SetDisplayAbility(cActorIsSelectingDamageFocusTargets.m_DamageAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.Normal);
				}
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
				WaitingForConfirm = true;
				int targetIndex2 = 0;
				foreach (CActor item27 in cActorIsSelectingDamageFocusTargets.m_DamageAbility.ValidActorsInRange)
				{
					CAttackSummary.TargetSummary damageSummary = cActorIsSelectingDamageFocusTargets.m_DamageAbility.DamageSummary.FindTarget(item27, ref targetIndex2);
					ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(item27)).m_WorldspacePanelUI.OnSelectingDamageFocus(cActorIsSelectingDamageFocusTargets.m_DamageAbility, cActorIsSelectingDamageFocusTargets.m_ActorSpawningMessage, damageSummary);
				}
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.TargetSelection);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.TargetSelection);
					}
				}
				ShowAbilityHelpBoxTooltip(cActorIsSelectingDamageFocusTargets.m_DamageAbility);
				if (InputManager.GamePadInUse)
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
					Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.RefreshAvailable();
				}
			}
			catch (Exception ex181)
			{
				Debug.LogError("An exception occurred while processing the ActorIsSelectingDamageFocus message\n" + ex181.Message + "\n" + ex181.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00043", "GUI_ERROR_MAIN_MENU_BUTTON", ex181.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex181.Message);
			}
			break;
		case CMessageData.MessageType.ActorHasDamaged:
			try
			{
				CActorHasDamaged_MessageData cActorHasDamaged_MessageData = (CActorHasDamaged_MessageData)message;
				if (cActorHasDamaged_MessageData.AnimOverload.Length == 0 && (cActorHasDamaged_MessageData.m_DamageAbility.UseSubAbilityTargeting || (cActorHasDamaged_MessageData.m_DamageAbility.MiscAbilityData.AutotriggerAbility.HasValue && cActorHasDamaged_MessageData.m_DamageAbility.MiscAbilityData.AutotriggerAbility.Value)))
				{
					return;
				}
				if (cActorHasDamaged_MessageData.m_DamageAbility.ActorsToTarget == null || cActorHasDamaged_MessageData.m_DamageAbility.ActorsToTarget.Count == 0)
				{
					ScenarioRuleClient.StepComplete();
					return;
				}
				Singleton<UIUseAugmentationsBar>.Instance.Hide();
				m_UndoButton.Toggle(active: false);
				ClearAllActorEvents();
				m_ActorsBeingTargetedForVFX = new List<CActor>(cActorHasDamaged_MessageData.m_DamageAbility.ActorsToTarget);
				CurrentAttackArea.Clear();
				foreach (CTile item28 in cActorHasDamaged_MessageData.m_DamageAbility.TilesInRange)
				{
					CurrentAttackArea.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item28.m_ArrayIndex.X, item28.m_ArrayIndex.Y]);
				}
				if (cActorHasDamaged_MessageData.m_DamageAbility.NumberTargets == 1 && cActorHasDamaged_MessageData.m_DamageAbility.ActorsToTarget != null && cActorHasDamaged_MessageData.m_DamageAbility.ActorsToTarget.Count > 0)
				{
					GameObject gameObject83 = FindClientActorGameObject(cActorHasDamaged_MessageData.m_DamageAbility.ActorsToTarget[0]);
					GameObject gameObject84 = FindClientActorGameObject(cActorHasDamaged_MessageData.m_ActorSpawningMessage);
					if (gameObject84 != null && gameObject83 != null)
					{
						gameObject84.transform.LookAt(gameObject83.transform.position);
					}
				}
				bool animationShouldPlay42 = false;
				CActor animatingActorToWaitFor42 = cActorHasDamaged_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorHasDamaged_MessageData.m_DamageAbility, cActorHasDamaged_MessageData.m_ActorSpawningMessage, new List<string> { cActorHasDamaged_MessageData.AnimOverload, "Damage" }, out animationShouldPlay42, out animatingActorToWaitFor42);
				if (animatingActorToWaitFor42 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForDamageAnim, animationShouldPlay42 ? 10000 : 400, animatingActorToWaitFor42);
				}
			}
			catch (Exception ex180)
			{
				Debug.LogError("An exception occurred while processing the ActorHasDamaged message\n" + ex180.Message + "\n" + ex180.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00044", "GUI_ERROR_MAIN_MENU_BUTTON", ex180.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex180.Message);
			}
			break;
		case CMessageData.MessageType.ReturnToSummoner:
			try
			{
				_ = (CReturnToSummoner_MessageData)message;
				CActor animatingActorToWaitFor6 = message.m_ActorSpawningMessage;
				CHeroSummonActor summonActor = animatingActorToWaitFor6 as CHeroSummonActor;
				if (summonActor == null)
				{
					ScenarioRuleClient.StepComplete();
					break;
				}
				Singleton<HelpBox>.Instance.Show("GUI_TOOLTIP_SELECT_SUMMON_MOVEMENT", "GUI_TOOLTIP_SELECT_SUMMON_MOVEMENT_TITLE");
				m_UndoButton.Toggle(active: true, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
				m_UndoButton.SetInteractable(active: false);
				m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				readyButton.ClearAlternativeAction();
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONFIRM_MOVEMENT"), hideOnClick: true, glowingEffect: true);
				summonActor.ReturnToSummoner = false;
				readyButton.QueueAlternativeAction(delegate
				{
					summonActor.ReturnToSummoner = true;
					readyButton.ClearAlternativeAction();
					ScenarioRuleClient.StepComplete();
				});
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				if (FFSNetwork.IsOnline && !message.m_ActorSpawningMessage.IsUnderMyControl)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ReturnToSummonerChoice);
				}
			}
			catch (Exception ex179)
			{
				Debug.LogError("An exception occurred while processing the ReturnToSummoner message\n" + ex179.Message + "\n" + ex179.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00045", "GUI_ERROR_MAIN_MENU_BUTTON", ex179.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex179.Message);
			}
			break;
		case CMessageData.MessageType.ActorBeenDamaged:
			try
			{
				CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = (CActorBeenDamaged_MessageData)message;
				GameObject gameObject82 = FindClientActorGameObject(cActorBeenDamaged_MessageData.m_ActorBeingDamaged);
				if (!(gameObject82 != null))
				{
					break;
				}
				Singleton<TakeDamagePanel>.Instance.RefreshDamagePreview();
				if (!ActorBehaviour.IsPaused(gameObject82))
				{
					bool animationShouldPlay41 = false;
					CActor animatingActorToWaitFor41 = cActorBeenDamaged_MessageData.m_ActorBeingDamaged;
					ProcessActorAnimation(cActorBeenDamaged_MessageData.m_DamageAbility, cActorBeenDamaged_MessageData.m_ActorBeingDamaged, new List<string>
					{
						cActorBeenDamaged_MessageData.m_ActorWasAsleep ? "SleepWakeUp" : "Hit",
						"Hit"
					}, out animationShouldPlay41, out animatingActorToWaitFor41);
					if (cActorBeenDamaged_MessageData.m_DamageAbility != null)
					{
						GUIInterface.s_GUIInterface.SetStatusText("Damaging (" + (cActorBeenDamaged_MessageData.m_DamageAbility.NumberTargets - cActorBeenDamaged_MessageData.m_DamageAbility.NumberTargetsRemaining + 1) + " of " + cActorBeenDamaged_MessageData.m_DamageAbility.NumberTargets + ") target modified strength = " + cActorBeenDamaged_MessageData.m_DamageAbility.ModifiedStrength());
					}
					if (ActorsBeingTargetedForVFX != null)
					{
						ActorsBeingTargetedForVFX.Clear();
					}
				}
				if (cActorBeenDamaged_MessageData.m_DamageAbility != null)
				{
					SEventAbility sEventAbility15 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Damage, ESESubTypeAbility.None, checkQueue: true);
					if (sEventAbility15 != null)
					{
						SEventAbilityDamage sEventAbilityDamage = (SEventAbilityDamage)sEventAbility15;
						string arg39 = LocalizationManager.GetTranslation(cActorBeenDamaged_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActorBeenDamaged_MessageData.m_ActorSpawningMessage);
						string arg40 = LocalizationManager.GetTranslation(cActorBeenDamaged_MessageData.m_ActorBeingDamaged.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActorBeenDamaged_MessageData.m_ActorBeingDamaged);
						int? num27 = (cActorBeenDamaged_MessageData.m_ActualDamage.HasValue ? cActorBeenDamaged_MessageData.m_ActualDamage : new int?(sEventAbilityDamage.Strength));
						CombatLogHandler instance = Singleton<CombatLogHandler>.Instance;
						string translation12 = LocalizationManager.GetTranslation("COMBAT_LOG_CONSUME_DAMAGE_TARGET");
						int? num28 = num27;
						instance.AddLog(string.Format(translation12, arg39, string.Format("<b><color=#f76767>{0}</color></b>", num28 + " " + LocalizationManager.GetTranslation("Damage").ToLower()), arg40), CombatLogFilter.DAMAGE);
					}
				}
			}
			catch (Exception ex178)
			{
				Debug.LogError("An exception occurred while processing the ActorBeenDamaged message\n" + ex178.Message + "\n" + ex178.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00045", "GUI_ERROR_MAIN_MENU_BUTTON", ex178.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex178.Message);
			}
			break;
		case CMessageData.MessageType.Looting:
			try
			{
				CLooting_MessageData cLooting_MessageData = (CLooting_MessageData)message;
				GameObject gameObject81 = FindClientActorGameObject(cLooting_MessageData.m_ActorLooting);
				if (!(gameObject81 != null))
				{
					break;
				}
				bool animationShouldPlay40 = false;
				CActor animatingActorToWaitFor40 = cLooting_MessageData.m_ActorLooting;
				ProcessActorAnimation(cLooting_MessageData.m_LootAbility, cLooting_MessageData.m_ActorLooting, new List<string> { cLooting_MessageData.AnimOverload, "Loot" }, out animationShouldPlay40, out animatingActorToWaitFor40);
				if (animatingActorToWaitFor40 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay40 ? 10000 : 400, animatingActorToWaitFor40);
				}
				List<CollectLootSMB> list2 = MF.GameObjectAnimatorStateBehaviours<CollectLootSMB>(gameObject81);
				for (int num26 = 0; num26 < list2.Count; num26++)
				{
					if (cLooting_MessageData.m_LootAbility != null)
					{
						list2[num26].SetTilesToCheckForDelayedDropSMB(m_ActorObjectsDiedInCurrentRound, cLooting_MessageData.m_ActorLooting);
					}
					list2[num26].SetExtraGoldFromAdditionalEffects(cLooting_MessageData.m_ExtraGoldFromAdditionalEffects);
				}
				if (!animationShouldPlay40)
				{
					ActorEvents.GetActorEvents(gameObject81).ProgressChoreographer();
				}
				if (cLooting_MessageData.m_LootAbility != null)
				{
					GUIInterface.s_GUIInterface.SetStatusText("Looting - Range " + cLooting_MessageData.m_LootAbility.Range);
				}
				if (cLooting_MessageData.m_PropsLooted == null || cLooting_MessageData.m_PropsLooted.Count <= 0)
				{
					break;
				}
				foreach (CObjectProp item29 in cLooting_MessageData.m_PropsLooted)
				{
					string arg38 = LocalizationManager.GetTranslation(cLooting_MessageData.m_ActorLooting.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cLooting_MessageData.m_ActorLooting);
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT"), arg38, LocalizationManager.GetTranslation((item29.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem) ? (item29.PrefabName + "_TOOLTIP") : item29.PrefabName)));
				}
			}
			catch (Exception ex177)
			{
				Debug.LogError("An exception occurred while processing the Looting message\n" + ex177.Message + "\n" + ex177.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00046", "GUI_ERROR_MAIN_MENU_BUTTON", ex177.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex177.Message);
			}
			break;
		case CMessageData.MessageType.PlayerSelectedTile:
			try
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = (CPlayerSelectedTile_MessageData)message;
				WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				if (cPlayerSelectedTile_MessageData.m_Ability.TilesSelected.Count > 0)
				{
					m_SkipButton.SetInteractable(active: false);
					readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMTARGETS, LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS"), hideOnClick: true, glowingEffect: true);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
					if (cPlayerSelectedTile_MessageData.m_Ability.TilesSelected.Count > 0)
					{
						m_LastSelectedTiles = new List<CClientTile>();
						foreach (CTile item30 in cPlayerSelectedTile_MessageData.m_Ability.TilesSelected)
						{
							CClientTile item4 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item30.m_ArrayIndex.X, item30.m_ArrayIndex.Y];
							if (!m_LastSelectedTiles.Contains(item4))
							{
								m_LastSelectedTiles.Add(item4);
							}
						}
					}
				}
				else
				{
					m_SkipButton.Toggle(cPlayerSelectedTile_MessageData.m_Ability.CanSkip, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
					readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS"), hideOnClick: true, glowingEffect: true);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				}
				if (cPlayerSelectedTile_MessageData.m_ActorSpawningMessage.Type == CActor.EType.Player && cPlayerSelectedTile_MessageData.m_Ability is CAbilitySummon cAbilitySummon6)
				{
					string translation11 = LocalizationManager.GetTranslation(cAbilitySummon6.SelectedLocKey);
					Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON_SELECT_HEX"), translation11), string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON"), translation11));
				}
			}
			catch (Exception ex176)
			{
				Debug.LogError("An exception occurred while processing the PlayerSelectedTile message\n" + ex176.Message + "\n" + ex176.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00047", "GUI_ERROR_MAIN_MENU_BUTTON", ex176.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex176.Message);
			}
			break;
		case CMessageData.MessageType.PlacingTrap:
			try
			{
				CPlacingTrap_MessageData cPlacingTrap_MessageData = (CPlacingTrap_MessageData)message;
				ClearAllActorEvents();
				bool animationShouldPlay39 = false;
				CActor animatingActorToWaitFor39 = cPlacingTrap_MessageData.m_ActorPlacingTrap;
				ProcessActorAnimation(cPlacingTrap_MessageData.m_TrapAbility, cPlacingTrap_MessageData.m_ActorPlacingTrap, new List<string>
				{
					cPlacingTrap_MessageData.AnimOverload,
					GetNonOverloadAnim(cPlacingTrap_MessageData.m_TrapAbility)
				}, out animationShouldPlay39, out animatingActorToWaitFor39);
				if (cPlacingTrap_MessageData.m_Tile != null)
				{
					m_lastSelectedTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cPlacingTrap_MessageData.m_Tile.m_ArrayIndex.X, cPlacingTrap_MessageData.m_Tile.m_ArrayIndex.Y];
				}
				GameObject gameObject80 = FindClientActorGameObject(cPlacingTrap_MessageData.m_ActorPlacingTrap);
				gameObject80.transform.LookAt(m_lastSelectedTile.m_GameObject.transform.position);
				if (animatingActorToWaitFor39 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay39 ? 10000 : 400, animatingActorToWaitFor39);
				}
				if (!animationShouldPlay39)
				{
					ActorEvents.GetActorEvents(gameObject80).ProgressChoreographer();
				}
				GUIInterface.s_GUIInterface.SetStatusText("Placing Trap");
			}
			catch (Exception ex175)
			{
				Debug.LogError("An exception occurred while processing the PlacingTrap message\n" + ex175.Message + "\n" + ex175.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00048", "GUI_ERROR_MAIN_MENU_BUTTON", ex175.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex175.Message);
			}
			break;
		case CMessageData.MessageType.DisarmTrap:
			try
			{
				CDisarmTrap_MessageData cDisarmTrap_MessageData = (CDisarmTrap_MessageData)message;
				ClearAllActorEvents();
				bool animationShouldPlay38 = false;
				CActor animatingActorToWaitFor38 = cDisarmTrap_MessageData.m_ActorDisarmingTrap;
				ProcessActorAnimation(cDisarmTrap_MessageData.m_DisarmTrapAbility, cDisarmTrap_MessageData.m_ActorDisarmingTrap, new List<string>
				{
					cDisarmTrap_MessageData.AnimOverload,
					GetNonOverloadAnim(cDisarmTrap_MessageData.m_DisarmTrapAbility)
				}, out animationShouldPlay38, out animatingActorToWaitFor38);
				if (cDisarmTrap_MessageData.m_Tiles != null && cDisarmTrap_MessageData.m_Tiles.Count > 0)
				{
					m_lastSelectedTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[Enumerable.Last(cDisarmTrap_MessageData.m_Tiles).m_ArrayIndex.X, Enumerable.Last(cDisarmTrap_MessageData.m_Tiles).m_ArrayIndex.Y];
					foreach (CTile tile2 in cDisarmTrap_MessageData.m_Tiles)
					{
						m_LastSelectedTiles.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tile2.m_ArrayIndex.X, tile2.m_ArrayIndex.Y]);
					}
				}
				WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(null, cDisarmTrap_MessageData.m_Tiles);
				GameObject gameObject79 = FindClientActorGameObject(cDisarmTrap_MessageData.m_ActorDisarmingTrap);
				if (gameObject79 != null && cDisarmTrap_MessageData.m_Tiles != null)
				{
					Vector3 zero5 = Vector3.zero;
					foreach (CTile tile3 in cDisarmTrap_MessageData.m_Tiles)
					{
						CClientTile cClientTile3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tile3.m_ArrayIndex.X, tile3.m_ArrayIndex.Y];
						zero5 += cClientTile3.m_GameObject.transform.position;
					}
					zero5 /= (float)cDisarmTrap_MessageData.m_Tiles.Count;
					gameObject79.transform.LookAt(zero5);
				}
				foreach (CObjectTrap disarmedTrap in cDisarmTrap_MessageData.m_DisarmedTraps)
				{
					GameObject propObject16 = Singleton<ObjectCacheService>.Instance.GetPropObject(disarmedTrap);
					if (propObject16 != null)
					{
						StartCoroutine(PrimePropActivationAnimation(propObject16, "Trap_Shut"));
						continue;
					}
					throw new Exception("Prop GameObject for instance name: " + disarmedTrap.InstanceName + " not found");
				}
				if (animatingActorToWaitFor38 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay38 ? 10000 : 400, animatingActorToWaitFor38);
				}
				if (!animationShouldPlay38)
				{
					ActorEvents.GetActorEvents(gameObject79).ProgressChoreographer();
				}
				GUIInterface.s_GUIInterface.SetStatusText("Disarming Trap");
			}
			catch (Exception ex174)
			{
				Debug.LogError("An exception occurred while processing the DisarmTrap message\n" + ex174.Message + "\n" + ex174.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00049", "GUI_ERROR_MAIN_MENU_BUTTON", ex174.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex174.Message);
			}
			break;
		case CMessageData.MessageType.ActivateOrDeactivateSpawner:
			try
			{
				CActivateOrDeactivateSpawner_MessageData cActivateOrDeactivateSpawner_MessageData = (CActivateOrDeactivateSpawner_MessageData)message;
				ClearAllActorEvents();
				bool animationShouldPlay37 = false;
				CActor animatingActorToWaitFor37 = cActivateOrDeactivateSpawner_MessageData.m_ActorDeactivatingSpawner;
				ProcessActorAnimation(cActivateOrDeactivateSpawner_MessageData.m_ActivateOrDeactivateSpawnerAbility, cActivateOrDeactivateSpawner_MessageData.m_ActorDeactivatingSpawner, new List<string>
				{
					cActivateOrDeactivateSpawner_MessageData.AnimOverload,
					GetNonOverloadAnim(cActivateOrDeactivateSpawner_MessageData.m_ActivateOrDeactivateSpawnerAbility)
				}, out animationShouldPlay37, out animatingActorToWaitFor37);
				if (cActivateOrDeactivateSpawner_MessageData.m_ActorDeactivatingSpawner.Type != CActor.EType.Player && cActivateOrDeactivateSpawner_MessageData.m_Tiles != null)
				{
					m_lastSelectedTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActivateOrDeactivateSpawner_MessageData.m_Tiles[0].m_ArrayIndex.X, cActivateOrDeactivateSpawner_MessageData.m_Tiles[0].m_ArrayIndex.Y];
				}
				WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(null, cActivateOrDeactivateSpawner_MessageData.m_Tiles);
				GameObject gameObject78 = FindClientActorGameObject(cActivateOrDeactivateSpawner_MessageData.m_ActorDeactivatingSpawner);
				if (gameObject78 != null && cActivateOrDeactivateSpawner_MessageData.m_Tiles != null)
				{
					Vector3 zero4 = Vector3.zero;
					foreach (CTile tile4 in cActivateOrDeactivateSpawner_MessageData.m_Tiles)
					{
						CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tile4.m_ArrayIndex.X, tile4.m_ArrayIndex.Y];
						zero4 += cClientTile2.m_GameObject.transform.position;
					}
					zero4 /= (float)cActivateOrDeactivateSpawner_MessageData.m_Tiles.Count;
					gameObject78.transform.LookAt(zero4);
				}
				if (animatingActorToWaitFor37 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay37 ? 10000 : 400, animatingActorToWaitFor37);
				}
				if (!animationShouldPlay37)
				{
					ActorEvents.GetActorEvents(gameObject78).ProgressChoreographer();
				}
				GUIInterface.s_GUIInterface.SetStatusText("Deactivating Spawner");
			}
			catch (Exception ex173)
			{
				Debug.LogError("An exception occurred while processing the DeactivateSpawner message\n" + ex173.Message + "\n" + ex173.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00219", "GUI_ERROR_MAIN_MENU_BUTTON", ex173.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex173.Message);
			}
			break;
		case CMessageData.MessageType.DestroyProp:
			try
			{
				CDestroyProp_MessageData cDestroyProp_MessageData = (CDestroyProp_MessageData)message;
				StartCoroutine(DestroyProp(cDestroyProp_MessageData.m_DestroyDelay, cDestroyProp_MessageData.m_Prop));
			}
			catch (Exception ex172)
			{
				Debug.LogError("An exception occurred while processing the DestroyObstacle message\n" + ex172.Message + "\n" + ex172.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00050", "GUI_ERROR_MAIN_MENU_BUTTON", ex172.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex172.Message);
			}
			break;
		case CMessageData.MessageType.DestroyObstacle:
			try
			{
				CDestroyObstacle_MessageData cDestroyObstacle_MessageData = (CDestroyObstacle_MessageData)message;
				CActor animatingActorToWaitFor36 = cDestroyObstacle_MessageData.m_ActorDestroyingObstacle;
				bool animationShouldPlay36 = false;
				GameObject gameObject77 = FindClientActorGameObject(cDestroyObstacle_MessageData.m_ActorDestroyingObstacle);
				if (animatingActorToWaitFor36 != null)
				{
					ClearAllActorEvents();
					if ((cDestroyObstacle_MessageData.m_ActorDestroyingObstacle.Type != CActor.EType.Player || cDestroyObstacle_MessageData.m_OverrideSetLastSelectedTile) && cDestroyObstacle_MessageData.m_Tiles != null)
					{
						m_lastSelectedTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cDestroyObstacle_MessageData.m_Tiles[0].m_ArrayIndex.X, cDestroyObstacle_MessageData.m_Tiles[0].m_ArrayIndex.Y];
						if (cDestroyObstacle_MessageData.m_OverrideSetLastSelectedTile)
						{
							m_LastSelectedTiles?.Clear();
							m_LastSelectedTiles?.Add(m_lastSelectedTile);
						}
					}
					WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(null, null, m_LastSelectedTiles);
					ProcessActorAnimation(cDestroyObstacle_MessageData.m_DestroyObstacleAbility, cDestroyObstacle_MessageData.m_ActorDestroyingObstacle, new List<string>
					{
						cDestroyObstacle_MessageData.AnimOverload,
						GetNonOverloadAnim(cDestroyObstacle_MessageData.m_DestroyObstacleAbility)
					}, out animationShouldPlay36, out animatingActorToWaitFor36);
					if (cDestroyObstacle_MessageData.m_FaceObstacle && gameObject77 != null && cDestroyObstacle_MessageData.m_Tiles != null)
					{
						Vector3 zero3 = Vector3.zero;
						foreach (CTile tile5 in cDestroyObstacle_MessageData.m_Tiles)
						{
							zero3 += ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tile5.m_ArrayIndex.X, tile5.m_ArrayIndex.Y].m_GameObject.transform.position;
						}
						zero3 /= (float)cDestroyObstacle_MessageData.m_Tiles.Count;
						gameObject77.transform.LookAt(zero3);
					}
				}
				foreach (CObjectProp destroyedProp in cDestroyObstacle_MessageData.m_DestroyedProps)
				{
					StartCoroutine(DestroyProp(cDestroyObstacle_MessageData.m_DestroyDelay, destroyedProp));
				}
				if (animatingActorToWaitFor36 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay36 ? 10000 : 400, animatingActorToWaitFor36);
				}
				if (!animationShouldPlay36 && gameObject77 != null)
				{
					ActorEvents.GetActorEvents(gameObject77).ProgressChoreographer();
				}
				GUIInterface.s_GUIInterface.SetStatusText("Destroying Obstacle");
			}
			catch (Exception ex171)
			{
				Debug.LogError("An exception occurred while processing the DestroyObstacle message\n" + ex171.Message + "\n" + ex171.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00050", "GUI_ERROR_MAIN_MENU_BUTTON", ex171.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex171.Message);
			}
			break;
		case CMessageData.MessageType.PreventDamage:
			try
			{
				CPreventDamage_MessageData cPreventDamage_MessageData = (CPreventDamage_MessageData)message;
				ClearAllActorEvents();
				GUIInterface.s_GUIInterface.SetStatusText("PreventDamage Active Bonus - Strength " + cPreventDamage_MessageData.m_PreventDamageAbility.Strength);
				LogActiveAbility(cPreventDamage_MessageData.m_ActorSpawningMessage, cPreventDamage_MessageData.m_PreventDamageAbility);
			}
			catch (Exception ex170)
			{
				Debug.LogError("An exception occurred while processing the PreventDamage message\n" + ex170.Message + "\n" + ex170.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00052", "GUI_ERROR_MAIN_MENU_BUTTON", ex170.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex170.Message);
			}
			break;
		case CMessageData.MessageType.PreventDamageTriggered:
			try
			{
				CPreventDamageTriggered_MessageData cPreventDamageTriggered_MessageData = (CPreventDamageTriggered_MessageData)message;
				CBaseCard preventDamageBaseCard = cPreventDamageTriggered_MessageData.m_PreventDamageBaseCard;
				if (preventDamageBaseCard == null)
				{
					break;
				}
				if (preventDamageBaseCard is CItem cItem && cItem.YMLData.Data.ShieldValue > 0 && cItem.YMLData.Data.ShieldValue < int.MaxValue)
				{
					string arg35 = LocalizationManager.GetTranslation(cPreventDamageTriggered_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cPreventDamageTriggered_MessageData.m_ActorSpawningMessage);
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_PREVENT_DAMAGE_AMOUNT"), arg35, cItem.YMLData.Data.ShieldValue, $"<b>{LocalizationManager.GetTranslation(cItem.YMLData.Name)}</b>"), CombatLogFilter.ITEMS);
					break;
				}
				if (preventDamageBaseCard.CardType == CBaseCard.ECardType.ScenarioModifier && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2 && cPhaseAction2.CurrentPhaseAbility.m_Ability != null && cPhaseAction2.CurrentPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.ChangeAllegiance)
				{
					string arg36 = LocalizationManager.GetTranslation(cPreventDamageTriggered_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cPreventDamageTriggered_MessageData.m_ActorSpawningMessage);
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_CHANGE_ALLEGIANCE"), arg36), CombatLogFilter.ABILITIES);
					break;
				}
				string arg37 = LocalizationManager.GetTranslation(cPreventDamageTriggered_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cPreventDamageTriggered_MessageData.m_ActorSpawningMessage);
				string translation10 = LocalizationManager.GetTranslation(preventDamageBaseCard.Name, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
				if (translation10 != null)
				{
					string text27 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_PREVENT_DAMAGE"), arg37, $"<b>{translation10}</b>");
					Singleton<CombatLogHandler>.Instance.AddLog(text27, CombatLogFilter.ABILITIES);
				}
			}
			catch (Exception ex169)
			{
				Debug.LogError("An exception occurred while processing the PreventDamageTriggered message\n" + ex169.Message + "\n" + ex169.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00053", "GUI_ERROR_MAIN_MENU_BUTTON", ex169.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex169.Message);
			}
			break;
		case CMessageData.MessageType.TargetShield:
			try
			{
				CTargetShield_MessageData cTargetShield_MessageData = (CTargetShield_MessageData)message;
				GameObject gameObject75 = FindClientActorGameObject(cTargetShield_MessageData.m_ActorBeingAttacked);
				GameObject gameObject76 = FindClientActorGameObject(cTargetShield_MessageData.m_ActorAttacking);
				Quaternion rotation3 = Quaternion.identity;
				if (gameObject76 != null && gameObject75 != null)
				{
					Vector3 forward = gameObject76.transform.position - gameObject75.transform.position;
					rotation3 = Quaternion.LookRotation(forward, gameObject75.transform.up);
				}
				if (gameObject75 != null)
				{
					GameObject obj22 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.ShieldActiveBonusTargetEffect, null, gameObject75.transform.position, rotation3);
					ObjectPool.Recycle(obj22, VFXShared.GetEffectLifetime(obj22), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.ShieldActiveBonusTargetEffect);
				}
			}
			catch (Exception ex168)
			{
				Debug.LogError("An exception occurred while processing the TargetShield message\n" + ex168.Message + "\n" + ex168.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00054", "GUI_ERROR_MAIN_MENU_BUTTON", ex168.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex168.Message);
			}
			break;
		case CMessageData.MessageType.AttackBuff:
			try
			{
				CAttackBuff_MessageData cAttackBuff_MessageData = (CAttackBuff_MessageData)message;
				ClearAllActorEvents();
				GUIInterface.s_GUIInterface.SetStatusText(cAttackBuff_MessageData.m_AttackAbility.GetDescription());
				s_Choreographer.m_CurrentAbility = cAttackBuff_MessageData.m_AttackAbility;
				if (!(message.m_ActorSpawningMessage is CHeroSummonActor))
				{
					bool animationShouldPlay35 = false;
					CActor animatingActorToWaitFor35 = cAttackBuff_MessageData.m_ActorSpawningMessage;
					ProcessActorAnimation(cAttackBuff_MessageData.m_AttackAbility, cAttackBuff_MessageData.m_ActorSpawningMessage, new List<string>
					{
						cAttackBuff_MessageData.AnimOverload,
						GetNonOverloadAnim(cAttackBuff_MessageData.m_AttackAbility)
					}, out animationShouldPlay35, out animatingActorToWaitFor35);
					if (animatingActorToWaitFor35 != null)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay35 ? 10000 : 400, animatingActorToWaitFor35);
					}
					LogActiveAbility(message.m_ActorSpawningMessage, cAttackBuff_MessageData.m_AttackAbility);
				}
			}
			catch (Exception ex167)
			{
				Debug.LogError("An exception occurred while processing the AttackBuff message\n" + ex167.Message + "\n" + ex167.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00055", "GUI_ERROR_MAIN_MENU_BUTTON", ex167.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex167.Message);
			}
			break;
		case CMessageData.MessageType.MoveBuff:
			try
			{
				CMoveBuff_MessageData cMoveBuff_MessageData = (CMoveBuff_MessageData)message;
				ClearAllActorEvents();
				GUIInterface.s_GUIInterface.SetStatusText(cMoveBuff_MessageData.m_MoveAbility.GetDescription());
				s_Choreographer.m_CurrentAbility = cMoveBuff_MessageData.m_MoveAbility;
				bool animationShouldPlay34 = false;
				CActor animatingActorToWaitFor34 = cMoveBuff_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cMoveBuff_MessageData.m_MoveAbility, cMoveBuff_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cMoveBuff_MessageData.AnimOverload,
					GetNonOverloadAnim(cMoveBuff_MessageData.m_MoveAbility)
				}, out animationShouldPlay34, out animatingActorToWaitFor34);
				if (animatingActorToWaitFor34 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay34 ? 10000 : 400, animatingActorToWaitFor34);
				}
			}
			catch (Exception ex166)
			{
				Debug.LogError("An exception occurred while processing the MoveBuff message\n" + ex166.Message + "\n" + ex166.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00196", "GUI_ERROR_MAIN_MENU_BUTTON", ex166.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex166.Message);
			}
			break;
		case CMessageData.MessageType.DamageBuff:
			try
			{
				CDamageBuff_MessageData cDamageBuff_MessageData = (CDamageBuff_MessageData)message;
				ClearAllActorEvents();
				GUIInterface.s_GUIInterface.SetStatusText(cDamageBuff_MessageData.m_DamageAbility.GetDescription());
				s_Choreographer.m_CurrentAbility = cDamageBuff_MessageData.m_DamageAbility;
				bool animationShouldPlay33 = false;
				CActor animatingActorToWaitFor33 = cDamageBuff_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cDamageBuff_MessageData.m_DamageAbility, cDamageBuff_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cDamageBuff_MessageData.AnimOverload,
					GetNonOverloadAnim(cDamageBuff_MessageData.m_DamageAbility)
				}, out animationShouldPlay33, out animatingActorToWaitFor33);
				if (animatingActorToWaitFor33 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay33 ? 10000 : 400, animatingActorToWaitFor33);
				}
			}
			catch (Exception ex165)
			{
				Debug.LogError("An exception occurred while processing the DamageBuff message\n" + ex165.Message + "\n" + ex165.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00204", "GUI_ERROR_MAIN_MENU_BUTTON", ex165.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex165.Message);
			}
			break;
		case CMessageData.MessageType.TargetAttackBuff:
			try
			{
			}
			catch (Exception ex164)
			{
				Debug.LogError("An exception occurred while processing the TargetAttackBuff message\n" + ex164.Message + "\n" + ex164.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00056", "GUI_ERROR_MAIN_MENU_BUTTON", ex164.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex164.Message);
			}
			break;
		case CMessageData.MessageType.FinishAura:
			try
			{
				CFinishAura_MessageData cFinishAura_MessageData = (CFinishAura_MessageData)message;
				if (cFinishAura_MessageData != null)
				{
					VFXShared.StopRegisteredAuraEffects(cFinishAura_MessageData.m_AuraAbilityID);
				}
			}
			catch (Exception ex163)
			{
				Debug.LogError("An exception occurred while processing the FinishAura message\n" + ex163.Message + "\n" + ex163.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00057", "GUI_ERROR_MAIN_MENU_BUTTON", ex163.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex163.Message);
			}
			break;
		case CMessageData.MessageType.PauseAura:
			try
			{
				CPauseAura_MessageData cPauseAura_MessageData = (CPauseAura_MessageData)message;
				if (cPauseAura_MessageData != null)
				{
					VFXShared.StopRegisteredAuraEffects(cPauseAura_MessageData.m_AuraAbilityID, pause: true);
				}
			}
			catch (Exception ex162)
			{
				Debug.LogError("An exception occurred while processing the PauseAura message\n" + ex162.Message + "\n" + ex162.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00058", "GUI_ERROR_MAIN_MENU_BUTTON", ex162.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex162.Message);
			}
			break;
		case CMessageData.MessageType.UnpauseAura:
			try
			{
				CUnpauseAura_MessageData cUnpauseAura_MessageData = (CUnpauseAura_MessageData)message;
				Animator componentInChildren10 = FindClientActorGameObject(message.m_ActorSpawningMessage).GetComponentInChildren<Animator>();
				if (cUnpauseAura_MessageData != null)
				{
					List<VFXShared.BasicEffectTyped> basicAuraEffects = new List<VFXShared.BasicEffectTyped>();
					VFXShared.HitEffectTargets hitEffectTargets = new VFXShared.HitEffectTargets(componentInChildren10.gameObject);
					CActiveBonus cActiveBonus = cUnpauseAura_MessageData.m_ActorSpawningMessage.FindCard(cUnpauseAura_MessageData.m_AuraBaseCardID, cUnpauseAura_MessageData.m_AuraBaseCardName)?.ActiveBonuses[0];
					if (cActiveBonus != null && VFXShared.RegisteredAuraHexEffects.Count > 0 && VFXShared.RegisteredAuraHexEffects.TryGetValue(cUnpauseAura_MessageData.m_AuraBaseCardID, out var value))
					{
						VFXShared.ProcessAuraHexEffects(cActiveBonus.Ability, value, cUnpauseAura_MessageData.m_ActorSpawningMessage, ref basicAuraEffects, ref hitEffectTargets);
						VFXShared.PlayAuraEffectsFromTimeline(basicAuraEffects, cUnpauseAura_MessageData.m_AuraBaseCardID);
					}
				}
			}
			catch (Exception ex161)
			{
				Debug.LogError("An exception occurred while processing the UnpauseAura message\n" + ex161.Message + "\n" + ex161.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00059", "GUI_ERROR_MAIN_MENU_BUTTON", ex161.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex161.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsApplyingConditionActiveBonus:
			try
			{
				CActorIsApplyingConditionActiveBonus_MessageData cActorIsApplyingConditionActiveBonus_MessageData = (CActorIsApplyingConditionActiveBonus_MessageData)message;
				ClearAllActorEvents();
				if (cActorIsApplyingConditionActiveBonus_MessageData.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					Singleton<UIUseItemsBar>.Instance.Hide();
					Singleton<UIActiveBonusBar>.Instance.Hide();
					Singleton<UIUseAugmentationsBar>.Instance.Hide();
				}
				if (cActorIsApplyingConditionActiveBonus_MessageData.m_ActorsAppliedTo.Count > 0)
				{
					CurrentAttackArea.Clear();
					foreach (CTile item31 in cActorIsApplyingConditionActiveBonus_MessageData.m_Ability.TilesInRange)
					{
						CurrentAttackArea.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item31.m_ArrayIndex.X, item31.m_ArrayIndex.Y]);
					}
					m_ActorsBeingTargetedForVFX = new List<CActor>(cActorIsApplyingConditionActiveBonus_MessageData.m_ActorsAppliedTo);
					WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(cActorIsApplyingConditionActiveBonus_MessageData.m_ActorsAppliedTo);
					GameObject gameObject73 = ((cActorIsApplyingConditionActiveBonus_MessageData.m_ActorsAppliedTo.Count == 1) ? FindClientActorGameObject(cActorIsApplyingConditionActiveBonus_MessageData.m_ActorsAppliedTo[0]) : null);
					GameObject gameObject74 = FindClientActorGameObject(message.m_ActorSpawningMessage);
					if (gameObject73 != null && gameObject74 != null)
					{
						gameObject74.transform.LookAt(gameObject73.transform.position);
					}
					s_Choreographer.m_CurrentAbility = cActorIsApplyingConditionActiveBonus_MessageData.m_Ability;
					bool animationShouldPlay32 = false;
					CActor animatingActorToWaitFor32 = cActorIsApplyingConditionActiveBonus_MessageData.m_ActorSpawningMessage;
					ProcessActorAnimation(cActorIsApplyingConditionActiveBonus_MessageData.m_Ability, cActorIsApplyingConditionActiveBonus_MessageData.m_ActorSpawningMessage, new List<string>
					{
						cActorIsApplyingConditionActiveBonus_MessageData.AnimOverload,
						GetNonOverloadAnim(cActorIsApplyingConditionActiveBonus_MessageData.m_Ability)
					}, out animationShouldPlay32, out animatingActorToWaitFor32);
					readyButton.Toggle(active: false);
					m_SkipButton.Toggle(active: false);
					SetActiveSelectButton(activate: false);
					if (animatingActorToWaitFor32 != null)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay32 ? 10000 : 400, animatingActorToWaitFor32);
					}
					if (!animationShouldPlay32 && !cActorIsApplyingConditionActiveBonus_MessageData.m_Ability.IsModifierAbility && gameObject74 != null)
					{
						ActorEvents.GetActorEvents(gameObject74).ProgressChoreographer();
					}
				}
				else
				{
					ScenarioRuleClient.StepComplete();
				}
			}
			catch (Exception ex160)
			{
				Debug.LogError("An exception occurred while processing the ActorIsApplyingConditionActiveBonus message\n" + ex160.Message + "\n" + ex160.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00060", "GUI_ERROR_MAIN_MENU_BUTTON", ex160.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex160.Message);
			}
			break;
		case CMessageData.MessageType.AbilityTargetingCombatLog:
			try
			{
				CAbilityTargetingCombatLog_MessageData cAbilityTargetingCombatLog_MessageData = (CAbilityTargetingCombatLog_MessageData)message;
				if (cAbilityTargetingCombatLog_MessageData.m_Ability is CAbilityCondition)
				{
					LogConditionApplied(cAbilityTargetingCombatLog_MessageData.m_ActorSpawningMessage, cAbilityTargetingCombatLog_MessageData.m_Ability, cAbilityTargetingCombatLog_MessageData.m_ActorsAppliedTo);
				}
				else
				{
					if (!(cAbilityTargetingCombatLog_MessageData.m_Ability is CAbilityTargeting))
					{
						break;
					}
					SEventAbility sEventAbility14 = SEventLog.FindLastAbilityEventOfAbilityType(cAbilityTargetingCombatLog_MessageData.m_Ability.AbilityType, ESESubTypeAbility.None, checkQueue: true);
					if (sEventAbility14 == null)
					{
						break;
					}
					LogConsumes(cAbilityTargetingCombatLog_MessageData.m_ActorSpawningMessage, cAbilityTargetingCombatLog_MessageData.m_ActorSpawningMessage.Type, cAbilityTargetingCombatLog_MessageData.m_Ability, "Targeting", "COMBAT_LOG_CONSUME_HEAL", "COMBAT_LOG_CONSUME_HEAL_NOELEMENT", "COMBAT_LOG_CONSUME_ABILITY", "COMBAT_LOG_CONSUME_ABILITY_NOELEMENT", "COMBAT_LOG_CONSUME", consumeUnknown: false);
					string arg33 = LocalizationManager.GetTranslation(cAbilityTargetingCombatLog_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cAbilityTargetingCombatLog_MessageData.m_ActorSpawningMessage);
					foreach (CActor item32 in cAbilityTargetingCombatLog_MessageData.m_ActorsAppliedTo)
					{
						if (TargetingAbilityTypesToSkip.Contains(sEventAbility14.AbilityType))
						{
							continue;
						}
						string text20 = LocalizationManager.GetTranslation(item32.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(item32);
						string text21 = string.Empty;
						if (!TargetingAbilitySpritesToSkip.Contains(sEventAbility14.AbilityType))
						{
							text21 = text21 + " <sprite name=" + HelperTools.GetCorrectSprite(sEventAbility14.AbilityType) + ">";
						}
						if (cAbilityTargetingCombatLog_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.ImmunityTo)
						{
							foreach (CCondition.EPositiveCondition key in cAbilityTargetingCombatLog_MessageData.m_Ability.PositiveConditions.Keys)
							{
								text21 = text21 + " <sprite name=" + key.ToString() + ">";
							}
							foreach (CCondition.ENegativeCondition key2 in cAbilityTargetingCombatLog_MessageData.m_Ability.NegativeConditions.Keys)
							{
								text21 = text21 + " <sprite name=" + key2.ToString() + ">";
							}
						}
						switch (cAbilityTargetingCombatLog_MessageData.m_Ability.AbilityType)
						{
						case CAbility.EAbilityType.PlaySong:
							if (!m_AbilitiesUsedThisTurn.Contains(cAbilityTargetingCombatLog_MessageData.m_Ability.AbilityBaseCard.Name))
							{
								m_AbilitiesUsedThisTurn.Add(cAbilityTargetingCombatLog_MessageData.m_Ability.AbilityBaseCard.Name);
								text21 = LocalizationManager.GetTranslation(cAbilityTargetingCombatLog_MessageData.m_Ability.AbilityBaseCard.Name) ?? "";
								string text24 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_PLAYS"), arg33, text21);
								Singleton<CombatLogHandler>.Instance.AddLog(text24, CombatLogFilter.ABILITIES);
							}
							break;
						case CAbility.EAbilityType.ChooseAbility:
						{
							string text25 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_CHOOSE_ABILITY"), arg33);
							Singleton<CombatLogHandler>.Instance.AddLog(text25, CombatLogFilter.ABILITIES);
							break;
						}
						case CAbility.EAbilityType.Kill:
						{
							string text26 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_KILL"), arg33, text20);
							Singleton<CombatLogHandler>.Instance.AddLog(text26, CombatLogFilter.ABILITIES);
							break;
						}
						case CAbility.EAbilityType.GiveSupplyCard:
						{
							List<string> supplyCardNames = CAbilityGiveSupplyCard.GetSupplyCardNames(cAbilityTargetingCombatLog_MessageData.m_Ability as CAbilityGiveSupplyCard);
							string arg34 = string.Join(", ", supplyCardNames.Select((string x) => $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(x)}</color></b></font>"));
							string text23 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_GIVE_CARD"), arg33, arg34, text20);
							Singleton<CombatLogHandler>.Instance.AddLog(text23, CombatLogFilter.ABILITIES);
							break;
						}
						default:
						{
							string text22 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_APPLY_ABILITY"), arg33, $"<b>{LocalizationManager.GetTranslation(sEventAbility14.AbilityType.ToString()) + text21}</b>", text20);
							Singleton<CombatLogHandler>.Instance.AddLog(text22, CombatLogFilter.CONDITIONS);
							break;
						}
						case CAbility.EAbilityType.Choose:
							break;
						}
					}
					break;
				}
			}
			catch (Exception ex159)
			{
				Debug.LogError("An exception occurred while processing the AbilityTargetingCombatLog message\n" + ex159.Message + "\n" + ex159.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00203", "GUI_ERROR_MAIN_MENU_BUTTON", ex159.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex159.Message);
			}
			break;
		case CMessageData.MessageType.Shield:
			try
			{
				CShield_MessageData cShield_MessageData = (CShield_MessageData)message;
				Singleton<UIUseAugmentationsBar>.Instance.Hide();
				GameObject gameObject72 = FindClientActorGameObject(cShield_MessageData.m_ActorAppliedTo);
				if (gameObject72 != null)
				{
					GameObject obj21 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainShield, gameObject72.transform);
					ObjectPool.Recycle(obj21, VFXShared.GetEffectLifetime(obj21), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainShield);
				}
			}
			catch (Exception ex158)
			{
				Debug.LogError("An exception occurred while processing the Shield message\n" + ex158.Message + "\n" + ex158.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00061", "GUI_ERROR_MAIN_MENU_BUTTON", ex158.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex158.Message);
			}
			break;
		case CMessageData.MessageType.AddTarget:
			try
			{
				CAddTarget_MessageData cAddTarget_MessageData = (CAddTarget_MessageData)message;
				GameObject gameObject71 = FindClientActorGameObject(cAddTarget_MessageData.m_ActorAppliedTo);
				if (GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddTarget != null)
				{
					GameObject obj20 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddTarget, gameObject71.transform);
					ObjectPool.Recycle(obj20, VFXShared.GetEffectLifetime(obj20), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddTarget);
				}
			}
			catch (Exception ex157)
			{
				Debug.LogError("An exception occurred while processing the AddTarget message\n" + ex157.Message + "\n" + ex157.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00062", "GUI_ERROR_MAIN_MENU_BUTTON", ex157.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex157.Message);
			}
			break;
		case CMessageData.MessageType.AddHeal:
			try
			{
				CAddHeal_MessageData cAddHeal_MessageData = (CAddHeal_MessageData)message;
				GameObject gameObject70 = FindClientActorGameObject(cAddHeal_MessageData.m_ActorAppliedTo);
				if (GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddHeal != null)
				{
					GameObject obj19 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddHeal, gameObject70.transform);
					ObjectPool.Recycle(obj19, VFXShared.GetEffectLifetime(obj19), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddHeal);
				}
			}
			catch (Exception ex156)
			{
				Debug.LogError("An exception occurred while processing the AddHeal message\n" + ex156.Message + "\n" + ex156.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00063", "GUI_ERROR_MAIN_MENU_BUTTON", ex156.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex156.Message);
			}
			break;
		case CMessageData.MessageType.AddActiveBonus:
			try
			{
				CAddActiveBonus_MessageData cAddActiveBonus_MessageData = (CAddActiveBonus_MessageData)message;
				GameObject gameObject69 = FindClientActorGameObject(cAddActiveBonus_MessageData.m_ActorAppliedTo);
				if (cAddActiveBonus_MessageData.m_AddActiveBonus.AddAbility.AbilityType == CAbility.EAbilityType.Attack)
				{
					if (GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAttackActiveBonus != null)
					{
						GameObject obj17 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAttackActiveBonus, gameObject69.transform);
						ObjectPool.Recycle(obj17, VFXShared.GetEffectLifetime(obj17), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAttackActiveBonus);
					}
				}
				else if (GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainDefault != null)
				{
					GameObject obj18 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainDefault, gameObject69.transform);
					ObjectPool.Recycle(obj18, VFXShared.GetEffectLifetime(obj18), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainDefault);
				}
			}
			catch (Exception ex155)
			{
				Debug.LogError("An exception occurred while processing the AddActiveBonus message\n" + ex155.Message + "\n" + ex155.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00064", "GUI_ERROR_MAIN_MENU_BUTTON", ex155.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex155.Message);
			}
			break;
		case CMessageData.MessageType.AddRange:
			try
			{
				CAddRange_MessageData cAddRange_MessageData = (CAddRange_MessageData)message;
				GameObject gameObject68 = FindClientActorGameObject(cAddRange_MessageData.m_ActorAppliedTo);
				if (GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddRange != null)
				{
					GameObject obj16 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddRange, gameObject68.transform);
					ObjectPool.Recycle(obj16, VFXShared.GetEffectLifetime(obj16), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddRange);
				}
			}
			catch (Exception ex154)
			{
				Debug.LogError("An exception occurred while processing the AddRange message\n" + ex154.Message + "\n" + ex154.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00065", "GUI_ERROR_MAIN_MENU_BUTTON", ex154.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex154.Message);
			}
			break;
		case CMessageData.MessageType.AttackersGainDisadvantage:
			try
			{
				CAttackersGainDisadvantage_MessageData cAttackersGainDisadvantage_MessageData = (CAttackersGainDisadvantage_MessageData)message;
				GameObject gameObject67 = FindClientActorGameObject(cAttackersGainDisadvantage_MessageData.m_ActorAppliedTo);
				if (GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAttackersGainDisadvantage != null)
				{
					GameObject obj15 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAttackersGainDisadvantage, gameObject67.transform);
					ObjectPool.Recycle(obj15, VFXShared.GetEffectLifetime(obj15), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAttackersGainDisadvantage);
				}
			}
			catch (Exception ex153)
			{
				Debug.LogError("An exception occurred while processing the AttackersGainDisadvantage message\n" + ex153.Message + "\n" + ex153.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00066", "GUI_ERROR_MAIN_MENU_BUTTON", ex153.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex153.Message);
			}
			break;
		case CMessageData.MessageType.Retaliate:
			try
			{
				CRetaliate_MessageData cRetaliate_MessageData = (CRetaliate_MessageData)message;
				GameObject gameObject66 = FindClientActorGameObject(cRetaliate_MessageData.m_ActorAppliedTo);
				if (gameObject66 != null)
				{
					GameObject obj14 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainRetaliate, gameObject66.transform);
					ObjectPool.Recycle(obj14, VFXShared.GetEffectLifetime(obj14), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainRetaliate);
				}
			}
			catch (Exception ex152)
			{
				Debug.LogError("An exception occurred while processing the Retaliate message\n" + ex152.Message + "\n" + ex152.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00067", "GUI_ERROR_MAIN_MENU_BUTTON", ex152.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex152.Message);
			}
			break;
		case CMessageData.MessageType.Invisible:
			try
			{
				CInvisible_MessageData cInvisible_MessageData = (CInvisible_MessageData)message;
				if (cInvisible_MessageData.m_ActorToMakeInvisible.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Invisible");
				if (!cInvisible_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject65 = FindClientActorGameObject(cInvisible_MessageData.m_ActorToMakeInvisible);
					if (gameObject65 != null && GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainInvisibility != null)
					{
						GameObject obj13 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainInvisibility, gameObject65.transform);
						ObjectPool.Recycle(obj13, VFXShared.GetEffectLifetime(obj13), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainInvisibility);
					}
				}
				else
				{
					ActorBehaviour actorBehaviour18 = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cInvisible_MessageData.m_ActorToMakeInvisible));
					if (actorBehaviour18.m_WorldspacePanelUI != null)
					{
						actorBehaviour18.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Invisible);
					}
				}
			}
			catch (Exception ex151)
			{
				Debug.LogError("An exception occurred while processing the Invisible message\n" + ex151.Message + "\n" + ex151.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00068", "GUI_ERROR_MAIN_MENU_BUTTON", ex151.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex151.Message);
			}
			break;
		case CMessageData.MessageType.Strengthen:
			try
			{
				CStrengthen_MessageData cStrengthen_MessageData = (CStrengthen_MessageData)message;
				if (cStrengthen_MessageData.m_ActorToStrengthen.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Strengthen");
				if (!cStrengthen_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject63 = FindClientActorGameObject(cStrengthen_MessageData.m_ActorToStrengthen);
					if (gameObject63 != null)
					{
						GameObject obj12 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainStrengthen, gameObject63.transform);
						ObjectPool.Recycle(obj12, VFXShared.GetEffectLifetime(obj12), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainStrengthen);
					}
					break;
				}
				GameObject gameObject64 = s_Choreographer.FindClientActorGameObject(cStrengthen_MessageData.m_ActorToStrengthen);
				if (gameObject64 != null)
				{
					ActorBehaviour actorBehaviour17 = ActorBehaviour.GetActorBehaviour(gameObject64);
					if (actorBehaviour17?.m_WorldspacePanelUI != null)
					{
						actorBehaviour17.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Strengthened);
					}
				}
			}
			catch (Exception ex150)
			{
				Debug.LogError("An exception occurred while processing the Strengthen message\n" + ex150.Message + "\n" + ex150.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00069", "GUI_ERROR_MAIN_MENU_BUTTON", ex150.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex150.Message);
			}
			break;
		case CMessageData.MessageType.Advantage:
			try
			{
				CAdvantage_MessageData cAdvantage_MessageData = (CAdvantage_MessageData)message;
				if (!cAdvantage_MessageData.m_AdvantagedActor.IsDead)
				{
					GUIInterface.s_GUIInterface.SetStatusText(cAdvantage_MessageData.m_AdvantageAbility.GetDescription());
					GameObject gameObject62 = FindClientActorGameObject(cAdvantage_MessageData.m_AdvantagedActor);
					GameObject obj11 = ObjectPool.Spawn(GlobalSettings.Instance.m_GlobalParticles.DefaultPositiveCondition, gameObject62.transform);
					ObjectPool.Recycle(obj11, VFXShared.GetEffectLifetime(obj11), GlobalSettings.Instance.m_GlobalParticles.DefaultPositiveCondition);
					LogActiveAbility(message.m_ActorSpawningMessage, cAdvantage_MessageData.m_AdvantageAbility);
				}
			}
			catch (Exception ex149)
			{
				Debug.LogError("An exception occurred while processing the Advantage message\n" + ex149.Message + "\n" + ex149.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00070", "GUI_ERROR_MAIN_MENU_BUTTON", ex149.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex149.Message);
			}
			break;
		case CMessageData.MessageType.Bless:
			try
			{
				CBless_MessageData cBless_MessageData = (CBless_MessageData)message;
				if (cBless_MessageData.m_ActorToBless.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Bless");
				if (!cBless_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject61 = FindClientActorGameObject(cBless_MessageData.m_ActorToBless);
					if (gameObject61 != null && !cBless_MessageData.m_DifficultyModBless)
					{
						GameObject obj10 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainBless, gameObject61.transform);
						ObjectPool.Recycle(obj10, VFXShared.GetEffectLifetime(obj10), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainBless);
					}
				}
				else
				{
					ActorBehaviour actorBehaviour16 = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cBless_MessageData.m_ActorToBless));
					if (actorBehaviour16.m_WorldspacePanelUI != null)
					{
						actorBehaviour16.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Blessed);
					}
				}
			}
			catch (Exception ex148)
			{
				Debug.LogError("An exception occurred while processing the Bless message\n" + ex148.Message + "\n" + ex148.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00073", "GUI_ERROR_MAIN_MENU_BUTTON", ex148.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex148.Message);
			}
			break;
		case CMessageData.MessageType.Curse:
			try
			{
				CCurse_MessageData cCurse_MessageData = (CCurse_MessageData)message;
				if (cCurse_MessageData.m_ActorToCurse.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Curse");
				if (!cCurse_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject60 = FindClientActorGameObject(cCurse_MessageData.m_ActorToCurse);
					if (gameObject60 != null && !cCurse_MessageData.m_DifficultyModCurse)
					{
						GameObject obj9 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainCurse, gameObject60.transform);
						ObjectPool.Recycle(obj9, VFXShared.GetEffectLifetime(obj9), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainCurse);
					}
				}
				else
				{
					ActorBehaviour actorBehaviour15 = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cCurse_MessageData.m_ActorToCurse));
					if (actorBehaviour15.m_WorldspacePanelUI != null)
					{
						actorBehaviour15.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Cursed);
					}
				}
			}
			catch (Exception ex147)
			{
				Debug.LogError("An exception occurred while processing the Curse message\n" + ex147.Message + "\n" + ex147.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00074", "GUI_ERROR_MAIN_MENU_BUTTON", ex147.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex147.Message);
			}
			break;
		case CMessageData.MessageType.Muddle:
			try
			{
				CMuddle_MessageData cMuddle_MessageData = (CMuddle_MessageData)message;
				if (cMuddle_MessageData.m_ActorToMuddle.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Muddle");
				if (!cMuddle_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject58 = FindClientActorGameObject(cMuddle_MessageData.m_ActorToMuddle);
					if (!(gameObject58 != null))
					{
						break;
					}
					GameObject obj8 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainMuddle, gameObject58.transform);
					ObjectPool.Recycle(obj8, VFXShared.GetEffectLifetime(obj8), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainMuddle);
					ActorBehaviour actorBehaviour13 = ActorBehaviour.GetActorBehaviour(gameObject58);
					if (actorBehaviour13.m_WorldspacePanelUI != null && cMuddle_MessageData.m_ActorToMuddle.Tokens.ModifiedNegativeCondition == CCondition.ENegativeCondition.Muddle && !cMuddle_MessageData.m_ActorToMuddle.Tokens.HasKey(CCondition.ENegativeCondition.Muddle))
					{
						List<CCondition.EPositiveCondition> newConditions = (from it in CActiveBonus.FindApplicableActiveBonuses(cMuddle_MessageData.m_ActorToMuddle, CAbility.EAbilityType.ChangeCondition)
							where it.Ability?.MiscAbilityData?.ReplaceNegativeConditions != null && it.Ability.MiscAbilityData.ReplaceWithPositiveConditions != null && it.Ability.MiscAbilityData.ReplaceNegativeConditions.Contains(CCondition.ENegativeCondition.Muddle)
							select it.Ability.MiscAbilityData.ReplaceWithPositiveConditions[it.Ability.MiscAbilityData.ReplaceNegativeConditions.IndexOf(CCondition.ENegativeCondition.Muddle)]).ToList();
						actorBehaviour13.m_WorldspacePanelUI.ReplaceCondition(EffectsBar.FEffect.Muddled, newConditions);
					}
					break;
				}
				GameObject gameObject59 = FindClientActorGameObject(cMuddle_MessageData.m_ActorToMuddle);
				if (gameObject59 != null)
				{
					ActorBehaviour actorBehaviour14 = ActorBehaviour.GetActorBehaviour(gameObject59);
					if (actorBehaviour14.m_WorldspacePanelUI != null)
					{
						actorBehaviour14.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Muddled);
					}
				}
			}
			catch (Exception ex146)
			{
				Debug.LogError("An exception occurred while processing the Muddle message\n" + ex146.Message + "\n" + ex146.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00071", "GUI_ERROR_MAIN_MENU_BUTTON", ex146.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex146.Message);
			}
			break;
		case CMessageData.MessageType.Poison:
			try
			{
				CPoison_MessageData cPoison_MessageData = (CPoison_MessageData)message;
				if (cPoison_MessageData.m_PoisonedActor.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Poison");
				if (!cPoison_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject56 = FindClientActorGameObject(cPoison_MessageData.m_PoisonedActor);
					if (gameObject56 != null)
					{
						GameObject obj7 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainPoison, gameObject56.transform);
						ObjectPool.Recycle(obj7, VFXShared.GetEffectLifetime(obj7), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainPoison);
					}
					break;
				}
				GameObject gameObject57 = FindClientActorGameObject(cPoison_MessageData.m_PoisonedActor);
				if (gameObject57 != null)
				{
					ActorBehaviour actorBehaviour12 = ActorBehaviour.GetActorBehaviour(gameObject57);
					if (actorBehaviour12.m_WorldspacePanelUI != null)
					{
						actorBehaviour12.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Poisoned);
					}
				}
			}
			catch (Exception ex145)
			{
				Debug.LogError("An exception occurred while processing the Poison message\n" + ex145.Message + "\n" + ex145.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00072", "GUI_ERROR_MAIN_MENU_BUTTON", ex145.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex145.Message);
			}
			break;
		case CMessageData.MessageType.Stun:
			try
			{
				CStun_MessageData cStun_MessageData = (CStun_MessageData)message;
				if (cStun_MessageData.m_ActorToStun.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Stun");
				if (!cStun_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject55 = FindClientActorGameObject(cStun_MessageData.m_ActorToStun);
					if (gameObject55 != null)
					{
						GameObject obj6 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainStun, gameObject55.transform);
						ObjectPool.Recycle(obj6, VFXShared.GetEffectLifetime(obj6), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainStun);
					}
				}
				else
				{
					ActorBehaviour actorBehaviour11 = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cStun_MessageData.m_ActorToStun));
					if (actorBehaviour11.m_WorldspacePanelUI != null)
					{
						actorBehaviour11.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Stunned);
					}
				}
			}
			catch (Exception ex144)
			{
				Debug.LogError("An exception occurred while processing the Stun message\n" + ex144.Message + "\n" + ex144.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00075", "GUI_ERROR_MAIN_MENU_BUTTON", ex144.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex144.Message);
			}
			break;
		case CMessageData.MessageType.Immobilize:
			try
			{
				CImmobilize_MessageData cImmobilize_MessageData = (CImmobilize_MessageData)message;
				if (cImmobilize_MessageData.m_ActorToImmobilize.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Immobilize");
				if (!cImmobilize_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject54 = FindClientActorGameObject(cImmobilize_MessageData.m_ActorToImmobilize);
					if (gameObject54 != null)
					{
						GameObject obj5 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainImmobilize, gameObject54.transform);
						ObjectPool.Recycle(obj5, VFXShared.GetEffectLifetime(obj5), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainImmobilize);
					}
				}
				else if (ScenarioManager.Scenario.HasActor(cImmobilize_MessageData.m_ActorToImmobilize))
				{
					ActorBehaviour actorBehaviour10 = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cImmobilize_MessageData.m_ActorToImmobilize));
					if (actorBehaviour10.m_WorldspacePanelUI != null)
					{
						actorBehaviour10.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Immobilized);
					}
				}
			}
			catch (Exception ex143)
			{
				Debug.LogError("An exception occurred while processing the Immobilize message\n" + ex143.Message + "\n" + ex143.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00077", "GUI_ERROR_MAIN_MENU_BUTTON", ex143.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex143.Message);
			}
			break;
		case CMessageData.MessageType.Wound:
			try
			{
				CWound_MessageData cWound_MessageData = (CWound_MessageData)message;
				if (cWound_MessageData.m_ActorToWound.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Wound");
				if (!cWound_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject52 = FindClientActorGameObject(cWound_MessageData.m_ActorToWound);
					if (gameObject52 != null)
					{
						GameObject obj4 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainWound, gameObject52.transform);
						ObjectPool.Recycle(obj4, VFXShared.GetEffectLifetime(obj4), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainWound);
					}
					break;
				}
				GameObject gameObject53 = FindClientActorGameObject(cWound_MessageData.m_ActorToWound);
				if (gameObject53 != null)
				{
					ActorBehaviour actorBehaviour9 = ActorBehaviour.GetActorBehaviour(gameObject53);
					if (actorBehaviour9.m_WorldspacePanelUI != null)
					{
						actorBehaviour9.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Wounded);
					}
				}
			}
			catch (Exception ex142)
			{
				Debug.LogError("An exception occurred while processing the Wound message\n" + ex142.Message + "\n" + ex142.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00078", "GUI_ERROR_MAIN_MENU_BUTTON", ex142.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex142.Message);
			}
			break;
		case CMessageData.MessageType.Disarm:
			try
			{
				CDisarm_MessageData cDisarm_MessageData = (CDisarm_MessageData)message;
				if (cDisarm_MessageData.m_ActorToDisarm.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Disarm");
				if (!cDisarm_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject50 = FindClientActorGameObject(cDisarm_MessageData.m_ActorToDisarm);
					if (gameObject50 != null)
					{
						GameObject obj3 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainDisarm, gameObject50.transform);
						ObjectPool.Recycle(obj3, VFXShared.GetEffectLifetime(obj3), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainDisarm);
					}
					break;
				}
				GameObject gameObject51 = FindClientActorGameObject(cDisarm_MessageData.m_ActorToDisarm);
				if (gameObject51 != null)
				{
					ActorBehaviour actorBehaviour8 = ActorBehaviour.GetActorBehaviour(gameObject51);
					if (actorBehaviour8.m_WorldspacePanelUI != null)
					{
						actorBehaviour8.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Disarmed);
					}
				}
			}
			catch (Exception ex141)
			{
				Debug.LogError("An exception occurred while processing the Disarm message\n" + ex141.Message + "\n" + ex141.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00080", "GUI_ERROR_MAIN_MENU_BUTTON", ex141.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex141.Message);
			}
			break;
		case CMessageData.MessageType.PoisonTriggered:
			try
			{
				CPoisonTriggered_MessageData cPoisonTriggered_MessageData = (CPoisonTriggered_MessageData)message;
				GUIInterface.s_GUIInterface.SetStatusText("Poison Triggered on " + cPoisonTriggered_MessageData.m_PoisonedActor.GetPrefabName());
				FindClientActorGameObject(cPoisonTriggered_MessageData.m_PoisonedActor);
			}
			catch (Exception ex140)
			{
				Debug.LogError("An exception occurred while processing the Poison Triggered message\n" + ex140.Message + "\n" + ex140.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00076", "GUI_ERROR_MAIN_MENU_BUTTON", ex140.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex140.Message);
			}
			break;
		case CMessageData.MessageType.WoundTriggered:
			try
			{
				CWoundTriggered_MessageData cWoundTriggered_MessageData = (CWoundTriggered_MessageData)message;
				GUIInterface.s_GUIInterface.SetStatusText("Wound Triggered on " + cWoundTriggered_MessageData.m_WoundedActor.GetPrefabName());
				GameObject gameObject48 = FindClientActorGameObject(cWoundTriggered_MessageData.m_WoundedActor);
				if (gameObject48 != null)
				{
					bool animationShouldPlay31 = false;
					CActor animatingActorToWaitFor31 = cWoundTriggered_MessageData.m_WoundedActor;
					ProcessActorAnimation(null, cWoundTriggered_MessageData.m_WoundedActor, new List<string>
					{
						cWoundTriggered_MessageData.m_ActorWasAsleep ? "SleepWakeUp" : "Hit",
						"Hit"
					}, out animationShouldPlay31, out animatingActorToWaitFor31);
					GameObject gameObject49 = ObjectPool.Spawn(GlobalSettings.Instance.m_MagicEffects.WoundDamage, gameObject48.transform);
					ObjectPool.Recycle(gameObject49, VFXShared.GetEffectLifetime(gameObject49), GlobalSettings.Instance.m_MagicEffects.WoundDamage);
					ActorBehaviour.UpdateHealth(gameObject48, cWoundTriggered_MessageData.m_ActorOriginalHealth, updateUI: true);
				}
			}
			catch (Exception ex139)
			{
				Debug.LogError("An exception occurred while processing the Wound Triggered message\n" + ex139.Message + "\n" + ex139.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00079", "GUI_ERROR_MAIN_MENU_BUTTON", ex139.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex139.Message);
			}
			break;
		case CMessageData.MessageType.Sleep:
			try
			{
				CSleep_MessageData cSleep_MessageData = (CSleep_MessageData)message;
				if (cSleep_MessageData.m_ActorToSleep.IsDead)
				{
					break;
				}
				GUIInterface.s_GUIInterface.SetStatusText("Sleeping");
				if (!cSleep_MessageData.m_ConditionAlreadyApplied)
				{
					GameObject gameObject46 = FindClientActorGameObject(cSleep_MessageData.m_ActorToSleep);
					if (!(gameObject46 != null))
					{
						break;
					}
					if (GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainSleep != null)
					{
						GameObject gameObject47 = ObjectPool.Spawn(GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainSleep, gameObject46.transform);
						if (gameObject47 != null)
						{
							ObjectPool.Recycle(gameObject47, VFXShared.GetEffectLifetime(gameObject47), GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainSleep);
						}
					}
					bool animationShouldPlay30 = false;
					ProcessActorAnimation(null, cSleep_MessageData.m_ActorToSleep, new List<string> { cSleep_MessageData.AnimOverload, "SleepIdle" }, out animationShouldPlay30, out var _);
					break;
				}
				ActorBehaviour actorBehaviour7 = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cSleep_MessageData.m_ActorToSleep));
				if (actorBehaviour7.m_WorldspacePanelUI != null)
				{
					actorBehaviour7.m_WorldspacePanelUI.AccentuateEffect(EffectsBar.FEffect.Sleep);
				}
			}
			catch (Exception ex138)
			{
				Debug.LogError("An exception occurred while processing the Sleep message\n" + ex138.Message + "\n" + ex138.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00227", "GUI_ERROR_MAIN_MENU_BUTTON", ex138.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex138.Message);
			}
			break;
		case CMessageData.MessageType.ActorAwakened:
			try
			{
				CActorAwakened_MessageData cActorAwakened_MessageData = (CActorAwakened_MessageData)message;
				bool animationShouldPlay29 = false;
				ProcessActorAnimation(null, cActorAwakened_MessageData.m_ActorAwakened, new List<string> { cActorAwakened_MessageData.AnimOverload, "SleepWakeUp" }, out animationShouldPlay29, out var _);
			}
			catch (Exception ex137)
			{
				Debug.LogError("An exception occurred while processing the ActorAwakened message\n" + ex137.Message + "\n" + ex137.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00228", "GUI_ERROR_MAIN_MENU_BUTTON", ex137.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex137.Message);
			}
			break;
		case CMessageData.MessageType.TargetRetaliate:
			try
			{
				CTargetRetaliate_MessageData cTargetRetaliate_MessageData = (CTargetRetaliate_MessageData)message;
				GameObject gameObject42 = FindClientActorGameObject(cTargetRetaliate_MessageData.m_ActorBeingAttacked);
				GameObject gameObject43 = FindClientActorGameObject(cTargetRetaliate_MessageData.m_ActorAttacking);
				Vector3 normalized = (gameObject43.transform.position - gameObject42.transform.position).normalized;
				Quaternion rotation = Quaternion.LookRotation(normalized, gameObject42.transform.up);
				Vector3 normalized2 = (gameObject42.transform.position - gameObject43.transform.position).normalized;
				Quaternion rotation2 = Quaternion.LookRotation(normalized2, gameObject43.transform.up);
				ActorBehaviour actorBehaviour6 = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cTargetRetaliate_MessageData.m_ActorBeingAttacked));
				if (actorBehaviour6.m_WorldspacePanelUI != null)
				{
					actorBehaviour6.m_WorldspacePanelUI.RetaliateActivatedEffect();
				}
				GameObject gameObject44 = ObjectPool.Spawn(GlobalSettings.Instance.m_MagicEffects.RetaliateHit, null, gameObject42.transform.position, rotation);
				ObjectPool.Recycle(gameObject44, VFXShared.GetEffectLifetime(gameObject44), GlobalSettings.Instance.m_MagicEffects.RetaliateHit);
				GameObject gameObject45 = ObjectPool.Spawn(GlobalSettings.Instance.m_MagicEffects.RetaliateTarget, null, gameObject43.transform.position, rotation2);
				ObjectPool.Recycle(gameObject45, VFXShared.GetEffectLifetime(gameObject45), GlobalSettings.Instance.m_MagicEffects.RetaliateTarget);
				string arg31 = LocalizationManager.GetTranslation(cTargetRetaliate_MessageData.m_ActorBeingAttacked.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cTargetRetaliate_MessageData.m_ActorBeingAttacked);
				string arg32 = LocalizationManager.GetTranslation(cTargetRetaliate_MessageData.m_ActorAttacking.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cTargetRetaliate_MessageData.m_ActorAttacking);
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_RETALIATE"), arg31, arg32, string.Format("<b>{0}</b>", LocalizationManager.GetTranslation("Retaliate") + " <sprite name=Retaliate> " + cTargetRetaliate_MessageData.m_retaliateBuff)), CombatLogFilter.ABILITIES);
			}
			catch (Exception ex136)
			{
				Debug.LogError("An exception occurred while processing the TargetRetaliate message\n" + ex136.Message + "\n" + ex136.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00081", "GUI_ERROR_MAIN_MENU_BUTTON", ex136.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex136.Message);
			}
			break;
		case CMessageData.MessageType.RecoverLostCards:
			try
			{
				CRecoverLostCards_MessageData cRecoverLostCards_MessageData = (CRecoverLostCards_MessageData)message;
				GUIInterface.s_GUIInterface.SetStatusText("Recovering Lost Cards");
				GameObject gameObject40 = FindClientActorGameObject(cRecoverLostCards_MessageData.m_ActorSpawningMessage);
				GameObject gameObject41 = FindClientActorGameObject(cRecoverLostCards_MessageData.m_ActorRecoveringLostCards);
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(cRecoverLostCards_MessageData.m_ActorRecoveringLostCards);
				if (gameObject40 != gameObject41)
				{
					gameObject40.transform.LookAt(gameObject41.transform.position);
				}
				bool animationShouldPlay28 = false;
				CActor animatingActorToWaitFor28 = cRecoverLostCards_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cRecoverLostCards_MessageData.m_Ability, cRecoverLostCards_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cRecoverLostCards_MessageData.AnimOverload,
					GetNonOverloadAnim(cRecoverLostCards_MessageData.m_Ability)
				}, out animationShouldPlay28, out animatingActorToWaitFor28);
				string text19 = LocalizationManager.GetTranslation(cRecoverLostCards_MessageData.m_ActorRecoveringLostCards.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cRecoverLostCards_MessageData.m_ActorRecoveringLostCards);
				Singleton<CombatLogHandler>.Instance.AddLog(text19 + string.Format("<b>{0}</b>", " " + LocalizationManager.GetTranslation("COMBAT_LOG_RECOVER_ALL_LOST_CARDS")), CombatLogFilter.ABILITIES);
			}
			catch (Exception ex135)
			{
				Debug.LogError("An exception occurred while processing the RecoverLostCards message\n" + ex135.Message + "\n" + ex135.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00082", "GUI_ERROR_MAIN_MENU_BUTTON", ex135.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex135.Message);
			}
			break;
		case CMessageData.MessageType.RecoverDiscardedCards:
			try
			{
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(m_CurrentActor);
				CRecoverDiscardedCards_MessageData cRecoverDiscardedCards_MessageData = (CRecoverDiscardedCards_MessageData)message;
				GUIInterface.s_GUIInterface.SetStatusText("Recovering Discarded Cards");
				GameObject gameObject38 = FindClientActorGameObject(cRecoverDiscardedCards_MessageData.m_ActorSpawningMessage);
				GameObject gameObject39 = FindClientActorGameObject(cRecoverDiscardedCards_MessageData.m_ActorRecoveringLostCards);
				m_ActorsBeingTargetedForVFX = new List<CActor> { cRecoverDiscardedCards_MessageData.m_ActorRecoveringLostCards };
				if (gameObject38 != gameObject39)
				{
					gameObject38.transform.LookAt(gameObject39.transform.position);
				}
				bool animationShouldPlay27 = false;
				CActor animatingActorToWaitFor27 = cRecoverDiscardedCards_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cRecoverDiscardedCards_MessageData.m_Ability, cRecoverDiscardedCards_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cRecoverDiscardedCards_MessageData.AnimOverload,
					GetNonOverloadAnim(cRecoverDiscardedCards_MessageData.m_Ability)
				}, out animationShouldPlay27, out animatingActorToWaitFor27);
				string text18 = LocalizationManager.GetTranslation(cRecoverDiscardedCards_MessageData.m_ActorRecoveringLostCards.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cRecoverDiscardedCards_MessageData.m_ActorRecoveringLostCards);
				Singleton<CombatLogHandler>.Instance.AddLog(text18 + string.Format("<b>{0}</b>", " " + LocalizationManager.GetTranslation("COMBAT_LOG_RECOVER_ALL_DISCARDED_CARDS")), CombatLogFilter.ABILITIES);
			}
			catch (Exception ex134)
			{
				Debug.LogError("An exception occurred while processing the RecoverDiscardedCards message\n" + ex134.Message + "\n" + ex134.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00083", "GUI_ERROR_MAIN_MENU_BUTTON", ex134.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex134.Message);
			}
			break;
		case CMessageData.MessageType.SelectRecoverCards:
			try
			{
				CSelectRecoverCards_MessageData cSelectRecoverCards_MessageData = (CSelectRecoverCards_MessageData)message;
				GameObject gameObject36 = FindClientActorGameObject(cSelectRecoverCards_MessageData.m_ActorSpawningMessage);
				GameObject gameObject37 = FindClientActorGameObject(cSelectRecoverCards_MessageData.m_ActorRecoveringLostCards);
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(cSelectRecoverCards_MessageData.m_ActorRecoveringLostCards);
				m_ActorsBeingTargetedForVFX = new List<CActor> { cSelectRecoverCards_MessageData.m_ActorRecoveringLostCards };
				if (gameObject36 != gameObject37)
				{
					gameObject36.transform.LookAt(gameObject37.transform.position);
				}
				bool animationShouldPlay26 = false;
				CActor animatingActorToWaitFor26 = cSelectRecoverCards_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cSelectRecoverCards_MessageData.m_Ability, cSelectRecoverCards_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cSelectRecoverCards_MessageData.AnimOverload,
					GetNonOverloadAnim(cSelectRecoverCards_MessageData.m_Ability)
				}, out animationShouldPlay26, out animatingActorToWaitFor26);
				ClearStars();
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				CPlayerActor cPlayerActor7 = (CPlayerActor)cSelectRecoverCards_MessageData.m_ActorRecoveringLostCards;
				CardHandMode cardHandMode2 = ((cSelectRecoverCards_MessageData.m_Ability is CAbilityRecoverDiscardedCards) ? CardHandMode.RecoverDiscardedCard : CardHandMode.RecoverLostCard);
				CardPileType selectableCardType2 = ((!(cSelectRecoverCards_MessageData.m_Ability is CAbilityRecoverDiscardedCards)) ? CardPileType.Lost : CardPileType.Discarded);
				Func<CAbilityCard, bool> func = null;
				int val = 0;
				switch (cardHandMode2)
				{
				case CardHandMode.RecoverDiscardedCard:
				{
					CAbility ability3 = cSelectRecoverCards_MessageData.m_Ability;
					CAbilityRecoverDiscardedCards discarded = ability3 as CAbilityRecoverDiscardedCards;
					if (discarded != null && !discarded.RecoverCardsWithAbilityOfTypeFilter.IsNullOrEmpty())
					{
						func = (CAbilityCard card) => discarded.RecoverCardsWithAbilityOfTypeFilter.Exists((CAbility.EAbilityType type) => card.HasAbilityOfType(type));
						val = cPlayerActor7.CharacterClass.DiscardedAbilityCards.Count(func);
					}
					else
					{
						val = cPlayerActor7.CharacterClass.DiscardedAbilityCards.Count;
					}
					break;
				}
				case CardHandMode.RecoverLostCard:
				{
					CAbility ability3 = cSelectRecoverCards_MessageData.m_Ability;
					CAbilityRecoverLostCards lost = ability3 as CAbilityRecoverLostCards;
					if (lost != null && !lost.RecoverCardsWithAbilityOfTypeFilter.IsNullOrEmpty())
					{
						func = (CAbilityCard card) => lost.RecoverCardsWithAbilityOfTypeFilter.Exists((CAbility.EAbilityType type) => card.HasAbilityOfType(type));
						val = cPlayerActor7.CharacterClass.LostAbilityCards.Count(func);
					}
					else
					{
						val = cPlayerActor7.CharacterClass.LostAbilityCards.Count;
					}
					break;
				}
				}
				val = Math.Min(val, cSelectRecoverCards_MessageData.m_Ability.Strength);
				CardsHandManager.Instance.Show((CPlayerActor)cSelectRecoverCards_MessageData.m_ActorRecoveringLostCards, cardHandMode2, CardPileType.Any, selectableCardType2, val, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.RESET, forceUseCurrentRoundCards: false, allowFullDeckPreview: true, null, func);
				CoroutineHelper.RunNextFrame(delegate
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CardSelection);
				});
				readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, LocalizationManager.GetTranslation("GUI_END_SELECTION"));
				m_UndoButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
				SetChoreographerState(ChoreographerStateType.WaitingForCardSelection, 0, null);
				switch (cardHandMode2)
				{
				case CardHandMode.RecoverDiscardedCard:
					InitiativeTrack.Instance.helpBox.ShowTranslated((val > 1) ? string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_RECOVER_DISCARDED_CARDS"), cSelectRecoverCards_MessageData.m_Ability.Strength) : LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_RECOVER_DISCARDED_CARD"));
					break;
				case CardHandMode.RecoverLostCard:
					InitiativeTrack.Instance.helpBox.ShowTranslated((val > 1) ? string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_RECOVER_LOST_CARDS"), cSelectRecoverCards_MessageData.m_Ability.Strength) : LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_RECOVER_LOST_CARD"));
					break;
				}
				if (FFSNetwork.IsOnline)
				{
					if (!cPlayerActor7.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.CardRecovery);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.CardRecovery);
					}
				}
			}
			catch (Exception ex133)
			{
				Debug.LogError("An exception occurred while processing the SelectRecoverCards message\n" + ex133.Message + "\n" + ex133.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00084", "GUI_ERROR_MAIN_MENU_BUTTON", ex133.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex133.Message);
			}
			break;
		case CMessageData.MessageType.MoveSelectedCards:
			try
			{
				CMoveSelectedCards cMoveSelectedCards = (CMoveSelectedCards)message;
				string text15 = null;
				string text16 = "";
				for (int num25 = 0; num25 < cMoveSelectedCards.m_Cards.Count; num25++)
				{
					text16 = text16 + ((num25 != 0) ? ", " : "") + $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(cMoveSelectedCards.m_Cards[num25].Name)}</color></b></font>";
				}
				switch (cMoveSelectedCards.m_MoveFromPile)
				{
				case CBaseCard.ECardPile.Discarded:
					text15 = ((cMoveSelectedCards.m_quantity <= 2) ? string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_RECOVER_DISCARDED_CARDS_NAME"), text16) : string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_RECOVER_DISCARDED_CARDS"), cMoveSelectedCards.m_quantity));
					break;
				case CBaseCard.ECardPile.Lost:
					text15 = ((cMoveSelectedCards.m_quantity <= 2) ? string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_RECOVER_LOST_CARDS_NAME"), text16) : string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_RECOVER_LOST_CARDS"), cMoveSelectedCards.m_quantity));
					break;
				case CBaseCard.ECardPile.Hand:
					if (cMoveSelectedCards.m_MoveToPile == CBaseCard.ECardPile.Discarded)
					{
						text15 = ((cMoveSelectedCards.m_quantity <= 2) ? string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DISCARD_CARDS_NAME"), text16) : string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DISCARD_CARDS"), cMoveSelectedCards.m_quantity));
					}
					else if (cMoveSelectedCards.m_MoveToPile == CBaseCard.ECardPile.Lost)
					{
						text15 = ((cMoveSelectedCards.m_quantity <= 2) ? string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOSE_CARDS_NAME"), text16) : string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOSE_CARDS"), cMoveSelectedCards.m_quantity));
					}
					break;
				case CBaseCard.ECardPile.None:
					text15 = ((cMoveSelectedCards.m_quantity <= 2) ? string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_GAIN_CARDS_NAME"), text16) : string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_GAIN_CARDS"), cMoveSelectedCards.m_quantity));
					break;
				}
				string text17 = LocalizationManager.GetTranslation(cMoveSelectedCards.m_ActorRecoveringCards.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cMoveSelectedCards.m_ActorRecoveringCards);
				Singleton<CombatLogHandler>.Instance.AddLog(text17 + string.Format("<b>{0}</b>", " " + text15), CombatLogFilter.ABILITIES);
			}
			catch (Exception ex132)
			{
				Debug.LogError("An exception occurred while processing the RecoveredSelectedCards message\n" + ex132.Message + "\n" + ex132.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00085", "GUI_ERROR_MAIN_MENU_BUTTON", ex132.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex132.Message);
			}
			break;
		case CMessageData.MessageType.SelectLoseCards:
			try
			{
				CSelectLoseCards_MessageData cSelectLoseCards_MessageData = (CSelectLoseCards_MessageData)message;
				s_Choreographer.m_CurrentAbility = cSelectLoseCards_MessageData.m_Ability;
				m_CurrentActor = cSelectLoseCards_MessageData.m_ActorSpawningMessage;
				GameObject gameObject34 = FindClientActorGameObject(cSelectLoseCards_MessageData.m_ActorSpawningMessage);
				GameObject gameObject35 = FindClientActorGameObject(cSelectLoseCards_MessageData.m_ActorLosingCards);
				m_ActorsBeingTargetedForVFX = new List<CActor> { cSelectLoseCards_MessageData.m_ActorLosingCards };
				if (gameObject34 != gameObject35)
				{
					gameObject34.transform.LookAt(gameObject35.transform.position);
				}
				bool animationShouldPlay25 = false;
				CActor animatingActorToWaitFor25 = cSelectLoseCards_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cSelectLoseCards_MessageData.m_Ability, cSelectLoseCards_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cSelectLoseCards_MessageData.AnimOverload,
					GetNonOverloadAnim(cSelectLoseCards_MessageData.m_Ability)
				}, out animationShouldPlay25, out animatingActorToWaitFor25);
				ClearStars();
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				CPlayerActor cPlayerActor6 = (CPlayerActor)cSelectLoseCards_MessageData.m_ActorLosingCards;
				CardHandMode cardHandMode = ((cSelectLoseCards_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.DiscardCards) ? CardHandMode.DiscardCard : CardHandMode.LoseCard);
				CardPileType selectableCardType = CardPileType.Hand;
				int num24 = ((cPlayerActor6.CharacterClass.HandAbilityCards.Count < cSelectLoseCards_MessageData.m_Ability.Strength) ? cPlayerActor6.CharacterClass.HandAbilityCards.Count : cSelectLoseCards_MessageData.m_Ability.Strength);
				CardsHandManager.Instance.Show((CPlayerActor)cSelectLoseCards_MessageData.m_ActorLosingCards, cardHandMode, CardPileType.Any, selectableCardType, num24, fadeUnselectableCards: true, highlightSelectableCards: true);
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.ChooseCards);
				readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, LocalizationManager.GetTranslation("GUI_END_SELECTION"));
				m_UndoButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
				SetChoreographerState(ChoreographerStateType.WaitingForCardSelection, 0, null);
				if (cardHandMode == CardHandMode.LoseCard)
				{
					InitiativeTrack.Instance.helpBox.ShowTranslated((num24 > 1) ? string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_LOSE_CARDS"), cSelectLoseCards_MessageData.m_Ability.Strength) : LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_LOSE_CARD"));
				}
				else
				{
					InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation((num24 > 1) ? "GUI_TOOLTIP_SELECT_DISCARD_CARDS" : "GUI_TOOLTIP_SELECT_DISCARD_CARD"), cSelectLoseCards_MessageData.m_Ability.Strength), string.Format(LocalizationManager.GetTranslation((num24 > 1) ? "GUI_TOOLTIP_SELECT_DISCARD_CARDS_TITLE" : "GUI_TOOLTIP_SELECT_DISCARD_CARD_TITLE"), cSelectLoseCards_MessageData.m_Ability.Strength));
				}
				if (FFSNetwork.IsOnline)
				{
					if (!cPlayerActor6.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.CardRecovery);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.CardRecovery);
					}
				}
			}
			catch (Exception ex131)
			{
				Debug.LogError("An exception occurred while processing the SelectLoseCards message\n" + ex131.Message + "\n" + ex131.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00186", "GUI_ERROR_MAIN_MENU_BUTTON", ex131.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex131.Message);
			}
			break;
		case CMessageData.MessageType.SelectIncreasedCardLimit:
			try
			{
				CSelectIncreasedCardLimit_MessageData cSelectIncreasedCardLimit_MessageData = (CSelectIncreasedCardLimit_MessageData)message;
				GameObject gameObject32 = FindClientActorGameObject(cSelectIncreasedCardLimit_MessageData.m_ActorSpawningMessage);
				GameObject gameObject33 = FindClientActorGameObject(cSelectIncreasedCardLimit_MessageData.m_ActorIncreasingCardLimit);
				m_ActorsBeingTargetedForVFX = new List<CActor> { cSelectIncreasedCardLimit_MessageData.m_ActorIncreasingCardLimit };
				if (gameObject32 != gameObject33)
				{
					gameObject32.transform.LookAt(gameObject33.transform.position);
				}
				bool animationShouldPlay24 = false;
				CActor animatingActorToWaitFor24 = cSelectIncreasedCardLimit_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cSelectIncreasedCardLimit_MessageData.m_Ability, cSelectIncreasedCardLimit_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cSelectIncreasedCardLimit_MessageData.AnimOverload,
					GetNonOverloadAnim(cSelectIncreasedCardLimit_MessageData.m_Ability)
				}, out animationShouldPlay24, out animatingActorToWaitFor24);
				ClearStars();
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				CPlayerActor cPlayerActor5 = (CPlayerActor)cSelectIncreasedCardLimit_MessageData.m_ActorIncreasingCardLimit;
				int num23 = ((cPlayerActor5.CharacterClass.AbilityCardsPool.Count < cSelectIncreasedCardLimit_MessageData.m_Ability.Strength) ? cPlayerActor5.CharacterClass.AbilityCardsPool.Count : cSelectIncreasedCardLimit_MessageData.m_Ability.Strength);
				CardsHandManager.Instance.Show((CPlayerActor)cSelectIncreasedCardLimit_MessageData.m_ActorIncreasingCardLimit, CardHandMode.IncreaseCardLimit, CardPileType.Unselected, CardPileType.Unselected, num23, fadeUnselectableCards: true, highlightSelectableCards: true);
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CardSelection);
				readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, LocalizationManager.GetTranslation("GUI_END_SELECTION"));
				m_UndoButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
				SetChoreographerState(ChoreographerStateType.WaitingForCardSelection, 0, null);
				InitiativeTrack.Instance.helpBox.ShowTranslated((num23 > 1) ? string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_INCREASE_CARDS"), cSelectIncreasedCardLimit_MessageData.m_Ability.Strength) : LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_RECOVER_INCREASE_CARD"));
				if (FFSNetwork.IsOnline)
				{
					if (!cPlayerActor5.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.CardRecovery);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.CardRecovery);
					}
				}
			}
			catch (Exception ex130)
			{
				Debug.LogError("An exception occurred while processing the SelectRecoverCards message\n" + ex130.Message + "\n" + ex130.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00084", "GUI_ERROR_MAIN_MENU_BUTTON", ex130.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex130.Message);
			}
			break;
		case CMessageData.MessageType.SelectExtraTurnCards:
			try
			{
				CSelectExtraTurnCards_MessageData cSelectExtraTurnCards_MessageData = (CSelectExtraTurnCards_MessageData)message;
				GameObject gameObject30 = FindClientActorGameObject(cSelectExtraTurnCards_MessageData.m_ActorSpawningMessage);
				GameObject gameObject31 = FindClientActorGameObject(cSelectExtraTurnCards_MessageData.m_ActorTakingExtraTurn);
				m_ActorsBeingTargetedForVFX = new List<CActor> { cSelectExtraTurnCards_MessageData.m_ActorTakingExtraTurn };
				if (gameObject30 != gameObject31)
				{
					gameObject30.transform.LookAt(gameObject31.transform.position);
				}
				bool animationShouldPlay23 = false;
				CActor animatingActorToWaitFor23 = cSelectExtraTurnCards_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility, cSelectExtraTurnCards_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cSelectExtraTurnCards_MessageData.AnimOverload,
					GetNonOverloadAnim(cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility)
				}, out animationShouldPlay23, out animatingActorToWaitFor23);
				ClearStars();
				CPlayerActor cPlayerActor4 = (CPlayerActor)cSelectExtraTurnCards_MessageData.m_ActorTakingExtraTurn;
				Singleton<AbilityEffectManager>.Instance.ShowExtraTurn(cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility, ActorBehaviour.GetActorBehaviour(gameObject31), cSelectExtraTurnCards_MessageData.m_ActorSpawningMessage as CPlayerActor);
				string title = Singleton<AbilityEffectManager>.Instance.ExtraTurnAbilityEffectTitle(cSelectExtraTurnCards_MessageData.m_ActorTakingExtraTurn);
				if (cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility.ExtraTurnType == CAbilityExtraTurn.EExtraTurnType.BottomAction)
				{
					InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_BOTTOM_ACTION"), title);
				}
				else if (cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility.ExtraTurnType == CAbilityExtraTurn.EExtraTurnType.TopAction)
				{
					InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_TOP_ACTION"), title);
				}
				else if (cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility.ExtraTurnType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
				{
					InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_BOTH_ACTIONS_LATER"), cPlayerActor4.Initiative()), title);
				}
				else
				{
					InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN"), title);
				}
				int maxCardsSelected = ((cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility.ExtraTurnType != CAbilityExtraTurn.EExtraTurnType.BothActions && cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility.ExtraTurnType != CAbilityExtraTurn.EExtraTurnType.BothActionsLater) ? 1 : 2);
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CardSelection);
				CardsHandManager.Instance.Show(cPlayerActor4, CardHandMode.CardsSelection, CardPileType.Any, new List<CardPileType> { CardPileType.Hand }, maxCardsSelected, fadeUnselectableCards: false, highlightSelectableCards: false, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.RESET, forceUseCurrentRoundCards: true);
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, (cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility.AbilityBaseCard is CItem) ? LocalizationManager.GetTranslation("GUI_CONFIRM_USE_ITEM") : LocalizationManager.GetTranslation("GUI_END_SELECTION"));
				readyButton.SetInteractable(interactable: false);
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
				SetChoreographerState(ChoreographerStateType.WaitingForCardSelection, 0, null);
				if (FFSNetwork.IsOnline)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ExtraTurnCardSelection);
				}
			}
			catch (Exception ex129)
			{
				Debug.LogError("An exception occurred while processing the SelectExtraTurnCards message\n" + ex129.Message + "\n" + ex129.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00188", "GUI_ERROR_MAIN_MENU_BUTTON", ex129.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex129.Message);
			}
			break;
		case CMessageData.MessageType.ExtraTurnCardsSelected:
			try
			{
				_ = (CExtraTurnCardsSelected)message;
				CardsHandManager.Instance.cardsActionController.OnActionFinished();
				CardsHandManager.Instance.CacheCardsActionControllerPhase();
				m_UndoButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
			}
			catch (Exception ex128)
			{
				Debug.LogError("An exception occurred while processing the ExtraTurnCardsSelected message\n" + ex128.Message + "\n" + ex128.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00189", "GUI_ERROR_MAIN_MENU_BUTTON", ex128.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex128.Message);
			}
			break;
		case CMessageData.MessageType.EndedExtraTurn:
			try
			{
				CEndedExtraTurn cEndedExtraTurn = (CEndedExtraTurn)message;
				Singleton<AbilityEffectManager>.Instance.RemoveExtraTurnEffect(cEndedExtraTurn.m_ActorTurnEnded);
				CardsHandManager.Instance.RestoreCardsActionControllerPhase();
				m_CurrentActor = cEndedExtraTurn.m_ActorSpawningMessage;
				if (FFSNetwork.IsOnline && m_CurrentActor is CPlayerActor)
				{
					InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
					{
						f.Avatar.RefreshActiveInteractable();
					});
				}
				if (CurrentPlayerActor == null)
				{
					UIManager.Instance.BattleGoalContainer.Hide();
				}
				else
				{
					UIManager.Instance.BattleGoalContainer.Show(CurrentPlayerActor);
				}
				GameState.FinishEndExtraTurn();
				string arg30 = LocalizationManager.GetTranslation(cEndedExtraTurn.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cEndedExtraTurn.m_ActorSpawningMessage);
				Singleton<CombatLogHandler>.Instance.AddHighlightedLog(string.Format("<b><font=\"MarcellusSC-Regular SDF\">{0}</font></b>", string.Format(LocalizationManager.GetTranslation("GUI_COMBATLOG_START_TURN"), arg30)), message.m_ActorSpawningMessage.Type);
			}
			catch (Exception ex127)
			{
				Debug.LogError("An exception occurred while processing the EndedExtraTurn message\n" + ex127.Message + "\n" + ex127.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00190", "GUI_ERROR_MAIN_MENU_BUTTON", ex127.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex127.Message);
			}
			break;
		case CMessageData.MessageType.PlayerIsDisarmed:
			try
			{
				GUIInterface.s_GUIInterface.SetStatusText("You are disarmed and unable to attack");
				m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ATTACK"));
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ATTACK"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(message.m_ActorSpawningMessage)).m_WorldspacePanelUI.DisarmedWarnEffect(active: true);
				InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_DISARMED"), LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey())));
				if (FFSNetwork.IsOnline)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.AbilityUsageBlocked);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.AbilityUsageBlocked);
					}
				}
			}
			catch (Exception ex126)
			{
				Debug.LogError("An exception occurred while processing the PlayerIsDisarmed message\n" + ex126.Message + "\n" + ex126.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00086", "GUI_ERROR_MAIN_MENU_BUTTON", ex126.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex126.Message);
			}
			break;
		case CMessageData.MessageType.PlayerIsStunned:
			try
			{
				_ = (CPlayerIsStunned_MessageData)message;
				GUIInterface.s_GUIInterface.SetStatusText("You are stunned and unable to act");
				m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(message.m_ActorSpawningMessage)).m_WorldspacePanelUI.StunnedWarnEffect(active: true);
				InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_STUNNED"), LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey())));
				if (FFSNetwork.IsOnline)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.AbilityUsageBlocked);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.AbilityUsageBlocked);
					}
				}
			}
			catch (Exception ex125)
			{
				Debug.LogError("An exception occurred while processing the PlayerIsStunned message\n" + ex125.Message + "\n" + ex125.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00087", "GUI_ERROR_MAIN_MENU_BUTTON", ex125.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex125.Message);
			}
			break;
		case CMessageData.MessageType.PlayerIsImmobilized:
			try
			{
				GUIInterface.s_GUIInterface.SetStatusText("You are immobilized and unable to move");
				m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_MOVEMENT"));
				InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_IMMOBILIZED"), LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey())));
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_MOVEMENT"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(message.m_ActorSpawningMessage)).m_WorldspacePanelUI.ImmobilizedWarnEffect(active: true);
				if (FFSNetwork.IsOnline)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.AbilityUsageBlocked);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.AbilityUsageBlocked);
					}
				}
			}
			catch (Exception ex124)
			{
				Debug.LogError("An exception occurred while processing the PlayerIsImmobilized message\n" + ex124.Message + "\n" + ex124.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00088", "GUI_ERROR_MAIN_MENU_BUTTON", ex124.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex124.Message);
			}
			break;
		case CMessageData.MessageType.PlayerIsSleeping:
			try
			{
				_ = (CPlayerIsSleeping_MessageData)message;
				GUIInterface.s_GUIInterface.SetStatusText("You are sleeping and unable to act");
				m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(message.m_ActorSpawningMessage)).m_WorldspacePanelUI.SleepingWarnEffect(active: true);
				InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_SLEEPING"), LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey())));
				if (FFSNetwork.IsOnline)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.AbilityUsageBlocked);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.AbilityUsageBlocked);
					}
				}
			}
			catch (Exception ex123)
			{
				Debug.LogError("An exception occurred while processing the PlayerIsSleeping message\n" + ex123.Message + "\n" + ex123.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00229", "GUI_ERROR_MAIN_MENU_BUTTON", ex123.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex123.Message);
			}
			break;
		case CMessageData.MessageType.PlayerSelectingObjectPosition:
			try
			{
				CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
				m_LastSelectedTiles = null;
				CPlayerSelectingObjectPosition_MessageData messageData8 = (CPlayerSelectingObjectPosition_MessageData)message;
				TileBehaviour.SetCallback(TileHandler);
				DisableTileSelection(active: false);
				if (message.m_ActorSpawningMessage is CPlayerActor)
				{
					Singleton<UIUseItemsBar>.Instance.ShowUsableItems(message.m_ActorSpawningMessage, (CItem cItem2) => cItem2.YMLData.Data.CompareAbility != null && cItem2.YMLData.Data.CompareAbility.CompareAbility(messageData8.m_Ability), "GUI_USE_ITEM_MOVEMENT_TIP");
				}
				if (messageData8.m_Ability is CAbilityDestroyObstacle)
				{
					GUIInterface.s_GUIInterface.SetStatusText(LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_DESTROY_OBSTACLES"));
				}
				else if (messageData8.m_TileFilter.Contains(CAbilityFilter.EFilterTile.EmptyHex))
				{
					GUIInterface.s_GUIInterface.SetStatusText("Select Position to place " + messageData8.m_SpawnType);
				}
				else
				{
					GUIInterface.s_GUIInterface.SetStatusText("Select " + messageData8.m_TileFilter.ToString());
				}
				if (messageData8.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					WorldspaceStarHexDisplay.Instance.ResetPlayerAttackType();
					ShowAbilityHelpBoxTooltip(messageData8.m_Ability);
					if (messageData8.m_SpawnType == ScenarioManager.ObjectImportType.HeroSummons && messageData8.m_Ability is CAbilitySummon cAbilitySummon5)
					{
						string translation9 = LocalizationManager.GetTranslation(cAbilitySummon5.SelectedLocKey);
						Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON_SELECT_HEX"), translation9), string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON"), translation9));
					}
				}
				if (messageData8.m_Ability.AreaEffect != null)
				{
					WorldspaceStarHexDisplay.Instance.SetDisplayAbility(messageData8.m_Ability, WorldspaceStarHexDisplay.EAbilityDisplayType.SelectObjectPositionAreaOfEffect, messageData8.m_SpawnType, messageData8.m_TileFilter);
				}
				else
				{
					WorldspaceStarHexDisplay.Instance.SetDisplayAbility(messageData8.m_Ability, WorldspaceStarHexDisplay.EAbilityDisplayType.SelectObjectPosition, messageData8.m_SpawnType, messageData8.m_TileFilter);
				}
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
				m_UndoButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
				m_UndoButton.SetInteractable(messageData8.m_Ability.CanUndo && FirstAbility);
				m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player && messageData8.m_Ability.CanSkip, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				if (messageData8.m_Ability.TilesSelected.Count > 0)
				{
					readyButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRM, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
					m_LastSelectedTiles = new List<CClientTile>();
					foreach (CTile item33 in messageData8.m_Ability.TilesSelected)
					{
						m_LastSelectedTiles.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item33.m_ArrayIndex.X, item33.m_ArrayIndex.Y]);
					}
				}
				else
				{
					readyButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
				}
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.TargetSelection);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.TargetSelection);
					}
				}
			}
			catch (Exception ex122)
			{
				Debug.LogError("An exception occurred while processing the PlayerSelectingObjectPosition message\n" + ex122.Message + "\n" + ex122.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00089", "GUI_ERROR_MAIN_MENU_BUTTON", ex122.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex122.Message);
			}
			break;
		case CMessageData.MessageType.InvalidSpawnPosition:
			try
			{
				GUIInterface.s_GUIInterface.SetStatusText("Cannot place item there");
			}
			catch (Exception ex121)
			{
				Debug.LogError("An exception occurred while processing the InvalidSpawnPosition message\n" + ex121.Message + "\n" + ex121.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00090", "GUI_ERROR_MAIN_MENU_BUTTON", ex121.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex121.Message);
			}
			break;
		case CMessageData.MessageType.PlacingSpawn:
			try
			{
				CPlacingSpawn_MessageData cPlacingSpawn_MessageData = (CPlacingSpawn_MessageData)message;
				ClearAllActorEvents();
				m_LastSelectedTiles = cPlacingSpawn_MessageData.m_Ability.TilesSelected.Select((CTile t) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[t.m_ArrayIndex.X, t.m_ArrayIndex.Y]).ToList();
				WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(null, null, m_LastSelectedTiles);
				bool animationShouldPlay22 = false;
				CActor animatingActorToWaitFor22 = cPlacingSpawn_MessageData.m_ActorPlacingSpawn;
				ProcessActorAnimation(cPlacingSpawn_MessageData.m_Ability, cPlacingSpawn_MessageData.m_ActorPlacingSpawn, new List<string>
				{
					cPlacingSpawn_MessageData.AnimOverload,
					GetNonOverloadAnim(cPlacingSpawn_MessageData.m_Ability)
				}, out animationShouldPlay22, out animatingActorToWaitFor22);
				if (animatingActorToWaitFor22 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay22 ? 10000 : 400, animatingActorToWaitFor22);
				}
				GameObject gameObject29 = FindClientActorGameObject(cPlacingSpawn_MessageData.m_ActorPlacingSpawn);
				if (!animationShouldPlay22)
				{
					ActorEvents.GetActorEvents(gameObject29).ProgressChoreographer();
				}
				readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, "");
				m_UndoButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				GUIInterface.s_GUIInterface.SetStatusText("Placing Spawn");
			}
			catch (Exception ex120)
			{
				Debug.LogError("An exception occurred while processing the PlacingSpawn message\n" + ex120.Message + "\n" + ex120.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00091", "GUI_ERROR_MAIN_MENU_BUTTON", ex120.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex120.Message);
			}
			break;
		case CMessageData.MessageType.EndAbilityAnimSync:
			try
			{
				CEndActorAbilityAnimSync_MessageData cEndActorAbilityAnimSync_MessageData = (CEndActorAbilityAnimSync_MessageData)message;
				LastAbility = cEndActorAbilityAnimSync_MessageData.m_IsLastAbility;
				CameraController.s_CameraController.ResetOptimalViewPoint();
				SetChoreographerState(ChoreographerStateType.WaitingForEndAbilityAnimSync, 500, message.m_ActorSpawningMessage);
				Singleton<UIScenarioDistributePointsManager>.Instance.Hide();
				if (LastAbility)
				{
					Singleton<UIUseAugmentationsBar>.Instance.Hide();
				}
				TryActivateWaitForConfirmHelpBox();
			}
			catch (Exception ex119)
			{
				Debug.LogError("An exception occurred while processing the EndAbilityAnimSync message\n" + ex119.Message + "\n" + ex119.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00092", "GUI_ERROR_MAIN_MENU_BUTTON", ex119.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex119.Message);
			}
			break;
		case CMessageData.MessageType.ShowElementPicker:
			try
			{
				CShowElementPicker_MessageData cShowElementPicker_MessageData = (CShowElementPicker_MessageData)message;
				int num22 = cShowElementPicker_MessageData.m_InfuseAbility.ElementsToInfuse.Count((ElementInfusionBoardManager.EElement w) => w == ElementInfusionBoardManager.EElement.Any);
				m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				Singleton<HelpBox>.Instance.ShowTranslated((num22 == 1) ? LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENT_INFUSE") : string.Format(LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENTS_INFUSE"), num22));
				CPlayerActor actor = ((!(cShowElementPicker_MessageData.m_ActorSpawningMessage is CPlayerActor)) ? (CurrentPlayerActor ?? (InitiativeTrack.Instance.SelectedActor().Actor as CPlayerActor)) : ((CPlayerActor)cShowElementPicker_MessageData.m_ActorSpawningMessage));
				Singleton<UIUseAbilitiesBar>.Instance.ShowInfuseAbilities(actor, new List<CAbilityInfuse> { cShowElementPicker_MessageData.m_InfuseAbility }, delegate(CAbility ability9)
				{
					Singleton<UIUseAbilitiesBar>.Instance.Remove(ability9);
					if (FFSNetwork.IsOnline)
					{
						ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
					}
					ScenarioRuleClient.StepComplete();
				}, null);
				if (FFSNetwork.IsOnline)
				{
					if (!cShowElementPicker_MessageData.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ElementPicking, savePreviousState: true);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.ElementPicking, savePreviousState: true);
					}
				}
			}
			catch (Exception ex118)
			{
				Debug.LogError("An exception occurred while processing the ShowElementPicker message\n" + ex118.Message + "\n" + ex118.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00093", "GUI_ERROR_MAIN_MENU_BUTTON", ex118.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex118.Message);
			}
			break;
		case CMessageData.MessageType.ShowSelectAbility:
			try
			{
				CShowSelectAbility_MessageData cShowSelectAbility_MessageData = (CShowSelectAbility_MessageData)message;
				CAbilityChooseAbility ability5 = cShowSelectAbility_MessageData.m_ChooseAbility;
				readyButton.ClearAlternativeAction();
				Singleton<UIUseItemsBar>.Instance.Hide(!(message.m_ActorSpawningMessage is CPlayerActor) || !ability5.IsItemAbility);
				if (ability5.ApplicableAbilities.Count > 0)
				{
					if (ability5.Name.Equals("AddActiveBonus"))
					{
						m_UndoButton.Toggle(active: false);
					}
					else
					{
						m_UndoButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
					}
					m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"), hideOnClick: true, ability5.CanSkip);
					readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation(ability5.IsItemAbility ? "GUI_CONFIRM_USE_ITEM" : "GUI_CONFIRM"), hideOnClick: true, glowingEffect: true);
					readyButton.SetInteractable(interactable: false);
					readyButton.QueueAlternativeAction(delegate
					{
						Singleton<UIUseAbilitiesBar>.Instance.Hide();
						readyButton.ClearAlternativeAction();
						ScenarioRuleClient.StepComplete();
					});
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
					Singleton<HelpBox>.Instance.Show($"GUI_CHOOSE_ABILITY_{((ability5.AbilityBaseCard != null) ? ability5.AbilityBaseCard.Name : ability5.ParentAbilityBaseCard.Name)}", (ability5.AbilityBaseCard != null) ? ability5.AbilityBaseCard.Name : ability5.ParentAbilityBaseCard.Name);
					Singleton<UIUseAbilitiesBar>.Instance.ShowChooseAbility(cShowSelectAbility_MessageData.m_ActorSpawningMessage as CPlayerActor, ability5, delegate(CAbility selectedAbility)
					{
						ability5.AbilityChosen(ability5.ApplicableAbilities.IndexOf(selectedAbility));
						readyButton.SetInteractable(interactable: true);
					}, delegate
					{
						ability5.ResetChosenAbility();
						readyButton.SetInteractable(interactable: false);
					});
					if (FFSNetwork.IsOnline && !message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionProcessor.CurrentPhase);
					}
				}
				else
				{
					s_Choreographer.Pass();
				}
			}
			catch (Exception ex117)
			{
				Debug.LogError("An exception occurred while processing the ShowSelectAbility message\n" + ex117.Message + "\n" + ex117.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00093", "GUI_ERROR_MAIN_MENU_BUTTON", ex117.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex117.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsRemovingCondition:
			try
			{
				CActorIsRemovingCondition_MessageData cActorIsRemovingCondition_MessageData = (CActorIsRemovingCondition_MessageData)message;
				string arg28 = LocalizationManager.GetTranslation(cActorIsRemovingCondition_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActorIsRemovingCondition_MessageData.m_ActorSpawningMessage);
				string arg29 = LocalizationManager.GetTranslation(cActorIsRemovingCondition_MessageData.m_ActorAppliedTo.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActorIsRemovingCondition_MessageData.m_ActorAppliedTo);
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_REMOVE_CONDITION"), arg28, string.Format("<b>{0}</b>", LocalizationManager.GetTranslation(cActorIsRemovingCondition_MessageData.m_NegativeCondition.ToString()) + " <sprite name=" + cActorIsRemovingCondition_MessageData.m_NegativeCondition.ToString() + ">"), arg29));
			}
			catch (Exception ex116)
			{
				Debug.LogError("An exception occurred while processing the ActorIsRemovingCondition message\n" + ex116.Message + "\n" + ex116.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00230", "GUI_ERROR_MAIN_MENU_BUTTON", ex116.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex116.Message);
			}
			break;
		case CMessageData.MessageType.EndAbility:
			try
			{
				CEndActorAbility_MessageData cEndActorAbility_MessageData = (CEndActorAbility_MessageData)message;
				WorldspaceStarHexDisplay.Instance.ResetPlayerAttackType();
				if (cEndActorAbility_MessageData.m_ActorSpawningMessage != null && cEndActorAbility_MessageData.m_ActorSpawningMessage.OriginalType == CActor.EType.Player)
				{
					InitiativeTrackActorBehaviour initiativeTrackActorBehaviour = InitiativeTrack.Instance.FindInitiativeTrackActor(cEndActorAbility_MessageData.m_ActorSpawningMessage);
					if (initiativeTrackActorBehaviour != null)
					{
						initiativeTrackActorBehaviour.RefreshAbilities();
					}
				}
				WorldspaceStarHexDisplay.Instance.LockView = false;
				VFXShared.StopPersistentEffects();
				m_UndoButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				TileBehaviour.SetCallback(TileHandler);
				if (cEndActorAbility_MessageData.m_IsLastAbility)
				{
					InfusionBoardUI.Instance.UnreserveElements();
				}
				Singleton<UIUseItemsBar>.Instance.Hide();
				Singleton<UIActiveBonusBar>.Instance.Hide();
				Singleton<UIUseAbilitiesBar>.Instance.Hide();
				Waypoint.s_LockWaypoints = false;
				Waypoint.Clear();
				CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				if (ActorsBeingTargetedForVFX != null)
				{
					ActorsBeingTargetedForVFX.Clear();
				}
				SetChoreographerState(ChoreographerStateType.Play, 0, null);
				Debug.Log("End Ability OnCharacterAbilityComplete Action is " + ((onCharacterAbilityComplete != null) ? "NOT NULL" : "NULL"));
				if (onCharacterAbilityComplete != null && cEndActorAbility_MessageData.m_IsLastAbility)
				{
					Action<bool> onFinish = delegate(bool switchBackToSavedStateIfOnline)
					{
						onCharacterAbilityComplete();
						onCharacterAbilityComplete = null;
						if (FFSNetwork.IsOnline && switchBackToSavedStateIfOnline)
						{
							ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
						}
						ScenarioRuleClient.EndAbilitySynchronise();
					};
					if (cEndActorAbility_MessageData.m_ActorSpawningMessage != null && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
					{
						if (PhaseManager.PhaseType == CPhase.PhaseType.Action && ((CPhaseAction)PhaseManager.Phase).AbilityHappened())
						{
							Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectAbilityInfusion);
							CardsActionControlller.s_Instance.PickAnyInfusionElements(message.m_ActorSpawningMessage, delegate(bool pickingSuccessful)
							{
								onFinish(pickingSuccessful);
							});
						}
						else
						{
							onFinish(obj: false);
						}
					}
					else
					{
						onFinish(obj: false);
					}
				}
				else
				{
					Action onFinish2 = delegate
					{
						ScenarioRuleClient.EndAbilitySynchronise();
					};
					if (cEndActorAbility_MessageData.m_ActorSpawningMessage != null && message.m_ActorSpawningMessage is CHeroSummonActor cHeroSummonActor2)
					{
						CAbility currentAbility = m_CurrentAbility;
						if (currentAbility != null && currentAbility.AbilityType == CAbility.EAbilityType.Attack && cHeroSummonActor2.SummonData.AttackInfuse == ElementInfusionBoardManager.EElement.Any)
						{
							Singleton<UIUseAbilitiesBar>.Instance.ShowGenericInfusion(cHeroSummonActor2.Summoner, delegate
							{
								Singleton<UIUseAbilitiesBar>.Instance.Hide();
								if (FFSNetwork.IsOnline)
								{
									ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
								}
								onFinish2();
							}, m_CurrentAbility, cHeroSummonActor2.BaseCard, LocalizationManager.GetTranslation(cHeroSummonActor2.ActorLocKey()), LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENT_INFUSE"));
							if (FFSNetwork.IsOnline)
							{
								if (!cHeroSummonActor2.Summoner.IsUnderMyControl)
								{
									ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ElementPicking, savePreviousState: true);
								}
								else
								{
									ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.ElementPicking, savePreviousState: true);
								}
							}
							goto IL_115c0;
						}
					}
					ScenarioRuleClient.EndAbilitySynchronise();
				}
				goto IL_115c0;
				IL_115c0:
				m_CurrentAbility = null;
			}
			catch (Exception ex115)
			{
				Debug.LogError("An exception occurred while processing the EndAbility message\n" + ex115.Message + "\n" + ex115.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00094", "GUI_ERROR_MAIN_MENU_BUTTON", ex115.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex115.Message);
			}
			break;
		case CMessageData.MessageType.AbilityStartUpdateCombatLog:
			try
			{
				m_ActorsKilledThisAbility.Clear();
				_ = (CAbilityStartUpdateCombatLog_MessageData)message;
				SEventAction sEventAction13 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionOnFirstAbilityStarted);
				if (sEventAction13 != null && message.m_ActorSpawningMessage is CPlayerActor && !sEventAction13.CurrentPhaseAbilityName.IsNullOrEmpty() && !m_AbilitiesUsedThisTurn.Contains(sEventAction13.CurrentPhaseAbilityName))
				{
					m_AbilitiesUsedThisTurn.Add(sEventAction13.CurrentPhaseAbilityName);
					CombatLogFilter filter = CombatLogFilter.ABILITIES;
					bool isItem;
					string text14 = LocalizationNameConverter.MultiLookupLocalization(sEventAction13.CurrentPhaseAbilityName, out isItem, suppressErrors: true);
					if (sEventAction13.DefaultAbility != "")
					{
						text14 = text14 + " - " + LocalizationManager.GetTranslation(sEventAction13.DefaultAbility);
					}
					if (isItem)
					{
						filter = CombatLogFilter.ITEMS;
					}
					if (text14 != null)
					{
						CActor cActor17 = ScenarioManager.FindActor(sEventAction13.CurrentPhaseActorGuid);
						string arg27 = LocalizationManager.GetTranslation(cActor17.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor17);
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_USES"), arg27, $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{text14}</color></b></font>"), filter);
					}
				}
			}
			catch (Exception ex114)
			{
				Debug.LogError("An exception occurred while processing the AbilityStartUpdateCombatLog message\n" + ex114.Message + "\n" + ex114.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00095", "GUI_ERROR_MAIN_MENU_BUTTON", ex114.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex114.Message);
			}
			break;
		case CMessageData.MessageType.OverrideItemUsedUpdateCombatLog:
			try
			{
				COverrideItemUsedUpdateCombatLog_MessageData cOverrideItemUsedUpdateCombatLog_MessageData = (COverrideItemUsedUpdateCombatLog_MessageData)message;
				SEventAction sEventAction12 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionUpdateItemsUsedInPhase);
				if (sEventAction12 == null || cOverrideItemUsedUpdateCombatLog_MessageData.m_ItemsToUpdate.Count <= 0)
				{
					break;
				}
				CActor cActor16 = ScenarioManager.FindActor(sEventAction12.CurrentPhaseActorGuid);
				foreach (CItem item34 in cOverrideItemUsedUpdateCombatLog_MessageData.m_ItemsToUpdate)
				{
					string arg26 = LocalizationManager.GetTranslation(cActor16.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor16);
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_USES"), arg26, $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(item34.YMLData.Name)}</color></b></font>"), CombatLogFilter.ITEMS);
				}
			}
			catch (Exception ex113)
			{
				Debug.LogError("An exception occurred while processing the AbilityStartUpdateCombatLog message\n" + ex113.Message + "\n" + ex113.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00096", "GUI_ERROR_MAIN_MENU_BUTTON", ex113.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex113.Message);
			}
			break;
		case CMessageData.MessageType.ActiveBonusConsumedElementsCombatLog:
			try
			{
				CActiveBonusConsumedElementsCombatLog_MessageData cActiveBonusConsumedElementsCombatLog_MessageData = (CActiveBonusConsumedElementsCombatLog_MessageData)message;
				if (cActiveBonusConsumedElementsCombatLog_MessageData.m_ElementsConsumed.Count <= 0)
				{
					break;
				}
				CActor actorSpawningMessage12 = cActiveBonusConsumedElementsCombatLog_MessageData.m_ActorSpawningMessage;
				string arg25 = LocalizationManager.GetTranslation(actorSpawningMessage12.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage12);
				string text13 = "";
				for (int num21 = 0; num21 < cActiveBonusConsumedElementsCombatLog_MessageData.m_ElementsConsumed.Count; num21++)
				{
					text13 = text13 + "<sprite name=" + cActiveBonusConsumedElementsCombatLog_MessageData.m_ElementsConsumed[num21].ToString() + ">";
					if (cActiveBonusConsumedElementsCombatLog_MessageData.m_ElementsConsumed.Count > 1)
					{
						text13 += ((cActiveBonusConsumedElementsCombatLog_MessageData.m_ElementsConsumed.Count > 2 && num21 < cActiveBonusConsumedElementsCombatLog_MessageData.m_ElementsConsumed.Count - 1) ? ", " : "and ");
					}
				}
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_CONSUME_ACTIVATE_ACTIVE_BONUS"), arg25, text13, $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(cActiveBonusConsumedElementsCombatLog_MessageData.m_ActiveBonus.BaseCard.Name)}</color></b></font>"), CombatLogFilter.ABILITIES);
			}
			catch (Exception ex112)
			{
				Debug.LogError("An exception occurred while processing the ActiveBonusConsumedElementsCombatLog message\n" + ex112.Message + "\n" + ex112.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00202", "GUI_ERROR_MAIN_MENU_BUTTON", ex112.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex112.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsInfusing:
			try
			{
				CActorIsInfusing_MessageData cActorIsInfusing_MessageData = (CActorIsInfusing_MessageData)message;
				if (cActorIsInfusing_MessageData.m_ActorSpawningMessage != null)
				{
					m_ElementsInfusedThisAbility.Add((cActorIsInfusing_MessageData.m_ActorSpawningMessage.ActorLocKey(), cActorIsInfusing_MessageData.m_Element.ToString()));
					if (m_PreviousMessageType == CMessageData.MessageType.ClearAllActorEvents || m_PreviousMessageType == CMessageData.MessageType.EndAction || m_PreviousMessageType == CMessageData.MessageType.ActorEarnedXP)
					{
						LogInfusedElements();
					}
				}
				else if (Singleton<CombatLogHandler>.Instance != null)
				{
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_INFUSE_ELEMENT"), LocalizationManager.GetTranslation("GUI_ELEMENT_" + cActorIsInfusing_MessageData.m_Element) + " <sprite name=" + cActorIsInfusing_MessageData.m_Element.ToString() + ">"), CombatLogFilter.INFUSIONS);
				}
			}
			catch (Exception ex111)
			{
				Debug.LogError("An exception occurred while processing the ActorIsInfusing message\n" + ex111.Message + "\n" + ex111.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00096", "GUI_ERROR_MAIN_MENU_BUTTON", ex111.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex111.Message);
			}
			break;
		case CMessageData.MessageType.AbilityEndUpdateCombatLog:
			try
			{
				CAbilityEndUpdateCombatLog_MessageData cAbilityEndUpdateCombatLog_MessageData = (CAbilityEndUpdateCombatLog_MessageData)message;
				switch (cAbilityEndUpdateCombatLog_MessageData.m_AbilityType)
				{
				case CAbility.EAbilityType.Move:
				{
					SEventAction sEventAction5 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility7 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Move, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility7 == null)
					{
						break;
					}
					SEventAbilityMove sEventAbilityMove = (SEventAbilityMove)sEventAbility7;
					if (sEventAction5 == null || sEventAbilityMove == null)
					{
						break;
					}
					CActor actorSpawningMessage10 = cAbilityEndUpdateCombatLog_MessageData.m_ActorSpawningMessage;
					string text8 = LocalizationManager.GetTranslation(actorSpawningMessage10.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage10);
					LogConsumes(cAbilityEndUpdateCombatLog_MessageData.m_ActorSpawningMessage, cAbilityEndUpdateCombatLog_MessageData.m_ActorSpawningMessage.Type, cAbilityEndUpdateCombatLog_MessageData.m_Ability, "Move", "COMBAT_LOG_CONSUME_MOVE", "COMBAT_LOG_CONSUME_MOVE_NOELEMENT");
					if (sEventAbilityMove.TilesMoved > 0)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(text8 + string.Format("<b>{0}</b>", " " + LocalizationManager.GetTranslation("COMBAT_LOG_MOVE") + " <sprite name=Move> " + sEventAbilityMove.ActualMoved) + " " + LocalizationManager.GetTranslation("COMBAT_LOG_HEXES"), CombatLogFilter.ABILITIES);
					}
					if (sEventAbility7.AddedNegativeConditionData.Count > 0 && cAbilityEndUpdateCombatLog_MessageData.m_Ability.AllTargetsOnMovePath)
					{
						string text9 = "";
						for (int num19 = 0; num19 < sEventAbility7.AddedNegativeConditionData.Count; num19++)
						{
							SEventAbility.SEventAbilityAddedConditionData sEventAbilityAddedConditionData = sEventAbility7.AddedNegativeConditionData[num19];
							text9 = ((num19 != 0) ? (text9 + " + " + string.Format("<b>{0}</b>", LocalizationManager.GetTranslation(sEventAbilityAddedConditionData.AddedConditionType.ToString()).ToTitleCase() + " <sprite name=" + sEventAbilityAddedConditionData.AddedConditionType.ToString() + ">")) : (text9 + string.Format("<b>{0}</b>", LocalizationManager.GetTranslation(sEventAbilityAddedConditionData.AddedConditionType.ToString()).ToTitleCase() + " <sprite name=" + sEventAbilityAddedConditionData.AddedConditionType.ToString() + ">")));
						}
						if (CAbilityMove.AllTargetActorsOnPath.Count > 0)
						{
							foreach (CActor item35 in CAbilityMove.AllTargetActorsOnPath.Where((CActor w) => w is CEnemyActor).ToList().ToList())
							{
								string arg18 = LocalizationManager.GetTranslation(item35.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(item35);
								Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_APPLY_ABILITY"), text8, text9, arg18), CombatLogFilter.CONDITIONS);
							}
						}
					}
					if (actorSpawningMessage10 != null && (sEventAbilityMove.MovedFromPoint.X != sEventAbilityMove.MovedToPoint.X || sEventAbilityMove.MovedFromPoint.Y != sEventAbilityMove.MovedToPoint.Y))
					{
						SimpleLog.AddToSimpleLog(text8 + " moved from: " + sEventAbilityMove.MovedFromPoint.X + "," + sEventAbilityMove.MovedFromPoint.Y + " to: " + sEventAbilityMove.MovedToPoint.X + "," + sEventAbilityMove.MovedToPoint.Y);
					}
					break;
				}
				case CAbility.EAbilityType.Create:
				{
					SEventAction sEventAction8 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility10 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Create, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility10 == null)
					{
						break;
					}
					SEventAbilityCreate sEventAbilityCreate = (SEventAbilityCreate)sEventAbility10;
					if (sEventAction8 != null && sEventAbilityCreate != null)
					{
						CActor cActor13 = ScenarioManager.FindActor(sEventAction8.CurrentPhaseActorGuid);
						string arg21 = LocalizationManager.GetTranslation(cActor13.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor13);
						if (sEventAbilityCreate.PropsSpawned == 1)
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_CREATE_PROP"), arg21, LocalizationManager.GetTranslation(sEventAbilityCreate.PropName)), CombatLogFilter.ABILITIES);
						}
						else
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_CREATE_PROPS"), arg21, sEventAbilityCreate.PropsSpawned + " " + LocalizationManager.GetTranslation(sEventAbilityCreate.PropName)), CombatLogFilter.ABILITIES);
						}
					}
					break;
				}
				case CAbility.EAbilityType.DestroyObstacle:
				{
					SEventAction sEventAction7 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility9 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.DestroyObstacle, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility9 == null || !(sEventAbility9 is SEventAbilityDestroyObstacle sEventAbilityDestroyObstacle) || sEventAction7 == null || sEventAbilityDestroyObstacle == null)
					{
						break;
					}
					CActor cActor12 = ScenarioManager.FindActor(sEventAction7.CurrentPhaseActorGuid);
					string arg20 = LocalizationManager.GetTranslation(cActor12.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor12);
					if (sEventAbilityDestroyObstacle.DestroyedPropsDictionary != null)
					{
						foreach (string key3 in sEventAbilityDestroyObstacle.DestroyedPropsDictionary.Keys)
						{
							int num20 = sEventAbilityDestroyObstacle.DestroyedPropsDictionary[key3];
							string translation8 = LocalizationManager.GetTranslation(key3, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
							if (translation8 == null)
							{
								LocalizationManager.GetTranslation("Obstacle", FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
							}
							if (translation8 != null)
							{
								if (num20 == 1)
								{
									Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DESTROY_PROP"), arg20, translation8), CombatLogFilter.ABILITIES);
								}
								else
								{
									Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DESTROY_PROPS"), arg20, num20 + " " + translation8), CombatLogFilter.ABILITIES);
								}
							}
						}
					}
					else
					{
						Debug.LogWarning("Obstacles are null for DestroyObstacle at AbilityEndUpdateCombatLog");
					}
					break;
				}
				case CAbility.EAbilityType.DisarmTrap:
				{
					SEventAction sEventAction4 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility6 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.DisarmTrap, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility6 == null)
					{
						break;
					}
					SEventAbilityDisarmTrap sEventAbilityDisarmTrap = (SEventAbilityDisarmTrap)sEventAbility6;
					if (sEventAction4 == null || sEventAbilityDisarmTrap == null)
					{
						break;
					}
					CActor cActor10 = ScenarioManager.FindActor(sEventAction4.CurrentPhaseActorGuid);
					string arg17 = LocalizationManager.GetTranslation(cActor10.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor10);
					foreach (string key4 in sEventAbilityDisarmTrap.TrapsDisarmedDictionary.Keys)
					{
						int num18 = sEventAbilityDisarmTrap.TrapsDisarmedDictionary[key4];
						if (num18 == 1)
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DISARM_PROP"), arg17, LocalizationManager.GetTranslation(key4)), CombatLogFilter.ABILITIES);
						}
						else
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DISARM_PROPS"), arg17, num18 + " " + LocalizationManager.GetTranslation(key4)), CombatLogFilter.ABILITIES);
						}
					}
					break;
				}
				case CAbility.EAbilityType.Infuse:
				{
					SEventAbilityInfuse sEventAbilityInfuse = (SEventAbilityInfuse)SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Infuse, ESESubTypeAbility.AbilityEnded);
					if (sEventAbilityInfuse == null)
					{
						break;
					}
					foreach (ElementInfusionBoardManager.EElement item36 in sEventAbilityInfuse.ElementsInfused)
					{
						if (item36 != ElementInfusionBoardManager.EElement.Any)
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_INFUSE_ELEMENT"), LocalizationManager.GetTranslation("GUI_ELEMENT_" + item36.ToString().ToUpper()) + " <sprite name=" + item36.ToString() + ">"), CombatLogFilter.INFUSIONS);
						}
					}
					break;
				}
				case CAbility.EAbilityType.Attack:
					if (cAbilityEndUpdateCombatLog_MessageData.m_Ability.Augment != null)
					{
						CActor actorSpawningMessage11 = cAbilityEndUpdateCombatLog_MessageData.m_ActorSpawningMessage;
						string arg23 = LocalizationManager.GetTranslation(actorSpawningMessage11.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage11);
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_USES"), arg23, $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(cAbilityEndUpdateCombatLog_MessageData.m_Ability.AbilityBaseCard.Name)}</color></b></font>"), CombatLogFilter.ABILITIES);
					}
					break;
				case CAbility.EAbilityType.Summon:
				{
					SEventAction sEventAction6 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility8 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Summon, ESESubTypeAbility.AbilityEnded);
					CAbilitySummon cAbilitySummon4 = (CAbilitySummon)cAbilityEndUpdateCombatLog_MessageData.m_Ability;
					if (sEventAbility8 == null)
					{
						break;
					}
					SEventAbilitySummon summonEvent = (SEventAbilitySummon)sEventAbility8;
					if (sEventAction6 == null || summonEvent == null)
					{
						break;
					}
					CActor cActor11 = ScenarioManager.FindActor(sEventAction6.CurrentPhaseActorGuid);
					string arg19 = LocalizationManager.GetTranslation(cActor11.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor11);
					if (cAbilitySummon4.SummonedActors != null && cAbilitySummon4.SummonedActors.Count > 0)
					{
						string text10 = LocalizationManager.GetTranslation(cAbilitySummon4.SummonedActors[0].ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cAbilitySummon4.SummonedActors[0]);
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_SUMMON"), arg19, text10), CombatLogFilter.ABILITIES);
						SimpleLog.AddToSimpleLog(text10 + " was summoned at Array Index " + cAbilitySummon4.SummonedActors[0].ArrayIndex.X + ", " + cAbilitySummon4.SummonedActors[0].ArrayIndex.Y);
					}
					else
					{
						if (!(cAbilitySummon4.SelectedSummonID != "Empty"))
						{
							break;
						}
						string text11 = string.Empty;
						HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonEvent.SummonName);
						if (heroSummonYMLData != null)
						{
							text11 = heroSummonYMLData.LocKey;
						}
						else
						{
							MonsterYMLData monsterData = ScenarioRuleClient.SRLYML.GetMonsterData(summonEvent.SummonName);
							if (monsterData != null)
							{
								text11 = monsterData.LocKey;
							}
							else
							{
								Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_FAILED_SUMMON"), arg19, LocalizationManager.GetTranslation(text11)), CombatLogFilter.ABILITIES);
								Debug.LogError("Unable to find summon class for ID " + summonEvent.SummonName);
							}
						}
						if (text11 != string.Empty)
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_SUMMON"), arg19, LocalizationManager.GetTranslation(text11)), CombatLogFilter.ABILITIES);
						}
					}
					break;
				}
				case CAbility.EAbilityType.Trap:
				{
					SEventAction sEventAction = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility3 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Trap, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility3 == null)
					{
						break;
					}
					CActor cActor7 = ScenarioManager.FindActor(sEventAction.CurrentPhaseActorGuid);
					string arg15 = LocalizationManager.GetTranslation(cActor7.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor7);
					SEventAbilityTrap sEventAbilityTrap = (SEventAbilityTrap)sEventAbility3;
					if (sEventAction == null || sEventAbilityTrap == null)
					{
						break;
					}
					if (sEventAbilityTrap.TrapState != CAbilityTrap.TrapState.SelectTrapPosition)
					{
						string text6 = LocalizationManager.GetTranslation(sEventAbilityTrap.TrapName) + ".";
						if (sEventAbilityTrap.Strength > 0)
						{
							text6 = text6 + " " + LocalizationManager.GetTranslation("Damage") + string.Format("<b><color=#f76767>{0}</color></b>", " <sprite name=Attack> " + sEventAbilityTrap.Strength);
						}
						if (sEventAbilityTrap.AddedNegativeConditionData != null && sEventAbilityTrap.AddedNegativeConditionData.Count > 0)
						{
							foreach (SEventAbility.SEventAbilityAddedConditionData addedNegativeConditionDatum in sEventAbilityTrap.AddedNegativeConditionData)
							{
								text6 = text6 + " + " + string.Format("<b>{0}</b>", "<sprite name=" + addedNegativeConditionDatum.AddedConditionType.ToString() + ">");
							}
						}
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_CREATE_TRAP"), arg15, text6), CombatLogFilter.ABILITIES);
					}
					else
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_FAILED_TRAP"), arg15, LocalizationManager.GetTranslation(sEventAbilityTrap.TrapName)), CombatLogFilter.ABILITIES);
					}
					break;
				}
				case CAbility.EAbilityType.PreventDamage:
					if (SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep) == null)
					{
					}
					break;
				case CAbility.EAbilityType.Pull:
				{
					SEventAction sEventAction9 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility11 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Pull, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility11 == null)
					{
						break;
					}
					SEventAbilityPull sEventAbilityPull = (SEventAbilityPull)sEventAbility11;
					if (sEventAction9 == null || sEventAbilityPull == null)
					{
						break;
					}
					CActor cActor14 = ScenarioManager.FindActor(sEventAction9.CurrentPhaseActorGuid);
					string arg22 = LocalizationManager.GetTranslation(cActor14.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor14);
					foreach (CAbilityPull.PulledActorStats item37 in sEventAbilityPull.ActorsDistanceMoved)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format((item37.DistanceMoved > 1) ? LocalizationManager.GetTranslation("COMBAT_LOG_PULL_MULTIPLE") : LocalizationManager.GetTranslation("COMBAT_LOG_PULL"), arg22, item37.Name, item37.DistanceMoved), CombatLogFilter.ABILITIES);
						SimpleLog.AddToSimpleLog(item37.Name + " moved from: " + item37.MovedFromPoint.X + "," + item37.MovedFromPoint.Y + " to: " + item37.MovedToPoint.X + "," + item37.MovedToPoint.Y);
					}
					break;
				}
				case CAbility.EAbilityType.Push:
				{
					SEventAction sEventAction3 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility5 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Push, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility5 == null)
					{
						break;
					}
					SEventAbilityPush sEventAbilityPush = (SEventAbilityPush)sEventAbility5;
					if (sEventAction3 == null || sEventAbilityPush == null)
					{
						break;
					}
					CActor cActor9 = ScenarioManager.FindActor(sEventAction3.CurrentPhaseActorGuid);
					string arg16 = LocalizationManager.GetTranslation(cActor9.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor9);
					foreach (CAbilityPush.PushedActorStats item38 in sEventAbilityPush.ActorsDistanceMoved)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format((item38.DistanceMoved > 1) ? LocalizationManager.GetTranslation("COMBAT_LOG_PUSH_MULTIPLE") : LocalizationManager.GetTranslation("COMBAT_LOG_PUSH"), arg16, item38.Name, item38.DistanceMoved), CombatLogFilter.ABILITIES);
						SimpleLog.AddToSimpleLog(item38.Name + " moved from: " + item38.MovedFromPoint.X + "," + item38.MovedFromPoint.Y + " to: " + item38.MovedToPoint.X + "," + item38.MovedToPoint.Y);
					}
					break;
				}
				case CAbility.EAbilityType.Fear:
				{
					SEventAction sEventAction11 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility13 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Fear, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility13 == null)
					{
						break;
					}
					SEventAbilityFear sEventAbilityFear = (SEventAbilityFear)sEventAbility13;
					if (sEventAction11 == null || sEventAbilityFear == null)
					{
						break;
					}
					CActor cActor15 = ScenarioManager.FindActor(sEventAction11.CurrentPhaseActorGuid);
					string arg24 = LocalizationManager.GetTranslation(cActor15.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor15);
					foreach (CAbilityFear.FearedActorStats item39 in sEventAbilityFear.ActorsDistanceMoved)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format((item39.DistanceMoved > 1) ? LocalizationManager.GetTranslation("COMBAT_LOG_FEAR_MULTIPLE") : LocalizationManager.GetTranslation("COMBAT_LOG_FEAR"), arg24, item39.Name, item39.DistanceMoved), CombatLogFilter.ABILITIES);
						SimpleLog.AddToSimpleLog(item39.Name + " feared from: " + item39.MovedFromPoint.X + "," + item39.MovedFromPoint.Y + " to: " + item39.MovedToPoint.X + "," + item39.MovedToPoint.Y);
					}
					break;
				}
				case CAbility.EAbilityType.Teleport:
				{
					SEventAction sEventAction2 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility4 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Teleport, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility4 != null)
					{
						SEventAbilityTeleport sEventAbilityTeleport = (SEventAbilityTeleport)sEventAbility4;
						if (sEventAction2 != null && sEventAbilityTeleport != null)
						{
							CActor cActor8 = ScenarioManager.FindActor(sEventAction2.CurrentPhaseActorGuid);
							string text7 = LocalizationManager.GetTranslation(cActor8.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor8);
							SimpleLog.AddToSimpleLog(text7 + " moved from: " + ((sEventAbilityTeleport.MovedFromPoint == null) ? "null" : (sEventAbilityTeleport.MovedFromPoint.X + "," + sEventAbilityTeleport.MovedFromPoint.Y)) + " to: " + ((sEventAbilityTeleport.MovedToPoint == null) ? "null" : (sEventAbilityTeleport.MovedToPoint.X + "," + sEventAbilityTeleport.MovedToPoint.Y)));
						}
					}
					break;
				}
				case CAbility.EAbilityType.AddTarget:
				{
					SEventAbility sEventAbility2 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.AddTarget, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility2 != null)
					{
						SEventAbilityTargeting sEventAbilityTargeting = (SEventAbilityTargeting)sEventAbility2;
						if (sEventAbilityTargeting != null)
						{
							CActor cActor6 = ScenarioManager.FindActor(sEventAbilityTargeting.CurrentPhaseActorGuid);
							string text5 = LocalizationManager.GetTranslation(cActor6.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor6);
							Singleton<CombatLogHandler>.Instance.AddLog(text5 + string.Format("<b>{0}</b>", string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ADD_TARGET"), sEventAbilityTargeting.Strength)), CombatLogFilter.ABILITIES);
						}
					}
					break;
				}
				case CAbility.EAbilityType.Swap:
				{
					SEventAction sEventAction10 = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionNextStep);
					SEventAbility sEventAbility12 = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.Swap, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility12 != null)
					{
						SEventAbilitySwap sEventAbilitySwap = (SEventAbilitySwap)sEventAbility12;
						if (sEventAction10 != null && sEventAbilitySwap != null)
						{
							SimpleLog.AddToSimpleLog(sEventAbilitySwap.FirstTargetName + " moved from: " + sEventAbilitySwap.FirstTargetStartLocation.X + "," + sEventAbilitySwap.FirstTargetStartLocation.Y + " to: " + sEventAbilitySwap.SecondTargetStartLocation.X + "," + sEventAbilitySwap.SecondTargetStartLocation.Y);
							SimpleLog.AddToSimpleLog(sEventAbilitySwap.SecondTargetName + " moved from: " + sEventAbilitySwap.SecondTargetStartLocation.X + "," + sEventAbilitySwap.SecondTargetStartLocation.Y + " to: " + sEventAbilitySwap.FirstTargetStartLocation.X + "," + sEventAbilitySwap.FirstTargetStartLocation.Y);
						}
					}
					break;
				}
				case CAbility.EAbilityType.AddModifierToMonsterDeck:
				{
					SEventAbility sEventAbility = SEventLog.FindLastAbilityEventOfAbilityType(CAbility.EAbilityType.AddModifierToMonsterDeck, ESESubTypeAbility.AbilityEnded);
					if (sEventAbility == null)
					{
						break;
					}
					SEventAbilityAddModifierToMonsterDeck sEventAbilityAddModifierToMonsterDeck = (SEventAbilityAddModifierToMonsterDeck)sEventAbility;
					if (sEventAbilityAddModifierToMonsterDeck == null)
					{
						break;
					}
					CActor cActor5 = ScenarioManager.FindActor(sEventAbility.CurrentPhaseActorGuid);
					string arg14 = LocalizationManager.GetTranslation(cActor5.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActor5);
					StringBuilder stringBuilder4 = new StringBuilder();
					for (int num17 = 0; num17 < sEventAbilityAddModifierToMonsterDeck.AddModifierStrings.Count; num17++)
					{
						stringBuilder4.Append(sEventAbilityAddModifierToMonsterDeck.AddModifierStrings[num17]);
						if (num17 != sEventAbilityAddModifierToMonsterDeck.AddModifierStrings.Count - 1)
						{
							stringBuilder4.Append(", ");
						}
					}
					if (sEventAbilityAddModifierToMonsterDeck.AddModifierStrings.Count > 1)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format("<b>{0}</b>", string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ADD_MULTIPLE_MODIFIERS_TO_MONSTER_DECKS"), arg14, stringBuilder4)), CombatLogFilter.ABILITIES);
					}
					else
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format("<b>{0}</b>", string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ADD_MODIFIERS_TO_MONSTER_DECKS"), arg14, stringBuilder4)), CombatLogFilter.ABILITIES);
					}
					bool animationShouldPlay21 = false;
					CActor animatingActorToWaitFor21 = cAbilityEndUpdateCombatLog_MessageData.m_ActorSpawningMessage;
					ProcessActorAnimation(cAbilityEndUpdateCombatLog_MessageData.m_Ability, cAbilityEndUpdateCombatLog_MessageData.m_ActorSpawningMessage, new List<string> { cAbilityEndUpdateCombatLog_MessageData.AnimOverload }, out animationShouldPlay21, out animatingActorToWaitFor21);
					if (animatingActorToWaitFor21 != null)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay21 ? 10000 : 400, animatingActorToWaitFor21);
					}
					break;
				}
				}
				if (cAbilityEndUpdateCombatLog_MessageData.m_Ability.ActiveEnhancements.Count > 0)
				{
					string text12 = " ";
					foreach (CEnhancement activeEnhancement in cAbilityEndUpdateCombatLog_MessageData.m_Ability.ActiveEnhancements)
					{
						text12 = text12 + string.Format("<b>{0}</b>", LocalizationManager.GetTranslation("ENHANCEMENT_" + activeEnhancement.Enhancement)) + ", ";
					}
					text12 = text12.Substring(0, text12.Length - 2);
					Singleton<CombatLogHandler>.Instance.AddLog(LocalizationManager.GetTranslation("COMBAT_LOG_ENHANCEMENTS") + text12, CombatLogFilter.ABILITIES);
				}
				foreach (Tuple<string, CActor.ECauseOfDeath> actorDeadTuple in m_ActorsKilledThisAbility)
				{
					if (actorDeadTuple.Item2 == CActor.ECauseOfDeath.ActorRemovedFromMap)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ACTOR_ESCAPE"), LocalizationManager.GetTranslation(actorDeadTuple.Item1)), CombatLogFilter.DEATHS);
						continue;
					}
					bool flag4 = ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => x.ActorLocKey() == actorDeadTuple.Item1);
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation(flag4 ? "COMBAT_LOG_ACTOR_EXHAUSTED" : "COMBAT_LOG_ACTOR_DIE"), LocalizationManager.GetTranslation(actorDeadTuple.Item1)), CombatLogFilter.DEATHS);
				}
				m_ActorsKilledThisAbility.Clear();
				LogInfusedElements();
			}
			catch (Exception ex110)
			{
				Debug.LogError("An exception occurred while processing the AbilityEndUpdateCombatLog message\n" + ex110.Message + "\n" + ex110.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00097", "GUI_ERROR_MAIN_MENU_BUTTON", ex110.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex110.Message);
			}
			break;
		case CMessageData.MessageType.ActorDead:
			try
			{
				CActorDead_MessageData cActorDead_MessageData = (CActorDead_MessageData)message;
				if (cActorDead_MessageData.m_Actor.CauseOfDeath == CActor.ECauseOfDeath.NoMoreCards)
				{
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ACTOR_EXHAUSTED_LACKOFCARDS"), LocalizationManager.GetTranslation(cActorDead_MessageData.m_Actor.ActorLocKey())), CombatLogFilter.DEATHS);
				}
				else
				{
					m_ActorsKilledThisAbility.Add(new Tuple<string, CActor.ECauseOfDeath>(cActorDead_MessageData.m_Actor.ActorLocKey(), cActorDead_MessageData.m_Actor.CauseOfDeath));
				}
				WorldspaceStarHexDisplay.Instance.UpdateTooltipsForCurrentTile();
				bool flag3 = cActorDead_MessageData.m_Actor.CauseOfDeath == CActor.ECauseOfDeath.ActorRemovedFromMap;
				GameObject gameObject28 = FindClientActorGameObject(cActorDead_MessageData.m_Actor);
				if (gameObject28 != null)
				{
					ActorBehaviour actorBehaviour5 = ActorBehaviour.GetActorBehaviour(gameObject28);
					if (actorBehaviour5.m_WorldspacePanelUI != null)
					{
						actorBehaviour5.m_WorldspacePanelUI.CancelRetaliateEffect();
					}
					m_ActorObjectsDiedInCurrentRound.Add(gameObject28);
					DeathDissolve componentInChildren7 = gameObject28.GetComponentInChildren<DeathDissolve>();
					if (!cActorDead_MessageData.m_OnDeathAbility || !componentInChildren7.PlayedExternally())
					{
						bool animationShouldPlay20 = false;
						CActor animatingActorToWaitFor20 = cActorDead_MessageData.m_Actor;
						ProcessActorAnimation(null, cActorDead_MessageData.m_Actor, new List<string>
						{
							flag3 ? "PowerUp" : (cActorDead_MessageData.m_ActorWasAsleep ? "SleepDeath" : "Death"),
							"Death"
						}, out animationShouldPlay20, out animatingActorToWaitFor20);
					}
					gameObject28.transform.SetParent(null);
					m_ClientDeadActors.Add(gameObject28);
					m_ClientPlayers.Remove(gameObject28);
					m_ClientEnemies.Remove(gameObject28);
					m_ClientHeroSummons.Remove(gameObject28);
					m_ClientAllyMonsters.Remove(gameObject28);
					m_ClientEnemy2Monsters.Remove(gameObject28);
					m_ClientNeutralMonsters.Remove(gameObject28);
					m_ClientObjects.Remove(gameObject28);
					ActorBehaviour.UpdateHealth(gameObject28);
					if (MF.GetGameObjectAnimatorController(gameObject28) == null || cActorDead_MessageData.m_Actor is CObjectActor { IsAttachedToProp: not false })
					{
						ActorBehaviour.GetActorBehaviour(gameObject28)?.m_WorldspacePanelUI.Destroy();
						CharacterManager characterManager3 = CharacterManager.GetCharacterManager(gameObject28);
						characterManager3.DeinitializeCharacter();
						if (characterManager3.CharacterActor.Type == CActor.EType.Player)
						{
							characterManager3.gameObject.transform.position = DeadPlayerLocation;
							if (Singleton<UIReadyTrackerBar>.Instance != null)
							{
								Singleton<UIReadyTrackerBar>.Instance.RemoveTracker((characterManager3.CharacterActor as CPlayerActor).CharacterClass.ID);
							}
						}
						else
						{
							ObjectPool.Recycle(gameObject28, characterManager3.CharacterPrefab);
						}
					}
					CTile propTile = ScenarioManager.Tiles[cActorDead_MessageData.m_Actor.ArrayIndex.X, cActorDead_MessageData.m_Actor.ArrayIndex.Y];
					if (propTile != null)
					{
						CObjectObstacle cObjectObstacle2 = propTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
						if (cObjectObstacle2 == null)
						{
							cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(propTile.m_ArrayIndex, ref propTile);
						}
						if (cObjectObstacle2 != null && ScenarioManager.CurrentScenarioState.TransparentProps.Contains(cObjectObstacle2))
						{
							ScenarioManager.CurrentScenarioState.TransparentProps.Remove(cObjectObstacle2);
							GameObject propObject15 = Singleton<ObjectCacheService>.Instance.GetPropObject(cObjectObstacle2);
							if (propObject15 != null)
							{
								Debug.Log("Prop found for transparency");
								CustomObjectPositionToChildMaterials componentInChildren8 = propObject15.GetComponentInChildren<CustomObjectPositionToChildMaterials>();
								if (componentInChildren8 != null)
								{
									Debug.Log("Script found for transparency");
									componentInChildren8.fadeSourceObject = null;
									componentInChildren8.enableFade = false;
									Debug.Log("Transparency disabled");
								}
								else
								{
									Debug.LogError("Could not find CustomObjectPositionToChildMaterial.cs script on Prop\n" + cObjectObstacle2.PrefabName);
								}
							}
						}
					}
				}
				if (FFSNetwork.IsOnline && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && PlayerRegistry.MyPlayer != null && cActorDead_MessageData.m_Actor.Type == CActor.EType.Player && cActorDead_MessageData.m_Actor.IsUnderMyControl && !PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable x) => x.IsAlive))
				{
					Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
					Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
				}
				Action action = null;
				if (cActorDead_MessageData.m_Actor.OriginalType == CActor.EType.Player)
				{
					CardsHandManager.Instance.UpdateDeadPlayer((CPlayerActor)cActorDead_MessageData.m_Actor);
					if (ScenarioManager.Scenario.PlayerActors.Count == 0)
					{
						readyButton.Toggle(active: false);
						m_SkipButton.Toggle(active: false);
						SetActiveSelectButton(activate: false);
					}
					UnityAction checkActorsDeadAction = delegate
					{
						if (ScenarioManager.Scenario.AllPlayers.All((CPlayerActor p) => p.IsDead) && ScenarioManager.Scenario.AllPlayers.Any((CPlayerActor p) => p.IsDeadForObjectives))
						{
							if (ScenarioManager.CurrentScenarioState.WinObjectives.All((CObjective o) => o.WinsDespiteExhaustion))
							{
								if (ScenarioManager.CurrentScenarioState.CheckObjectivesComplete() == EObjectiveResult.Win)
								{
									SimpleLog.AddToSimpleLog("Win triggered at ActorDead message as all player actors are dead, where all win objectives have WinsDespiteExhaustion == true");
									WinScenario();
								}
								else
								{
									SimpleLog.AddToSimpleLog("Lose triggered at ActorDead message as all player actors are dead.");
									LoseScenario();
								}
							}
							else
							{
								SimpleLog.AddToSimpleLog("Lose triggered at ActorDead message as all player actors are dead.");
								LoseScenario();
							}
						}
					};
					if (cActorDead_MessageData.m_Actor.Health <= 0 && PhaseManager.PhaseType != CPhase.PhaseType.EndTurn)
					{
						action = Singleton<PhaseBannerHandler>.Instance.ShowDie;
						Singleton<PhaseBannerHandler>.Instance.ShowPreDie(cActorDead_MessageData.m_Actor.ActorLocKey(), delegate
						{
							checkActorsDeadAction?.Invoke();
						});
					}
					else
					{
						checkActorsDeadAction?.Invoke();
					}
					if (flag3 && ScenarioManager.CurrentScenarioState.CheckObjectivesComplete() == EObjectiveResult.Win)
					{
						WinScenario(forceUI: false);
					}
				}
				else if (cActorDead_MessageData.m_Actor is CHeroSummonActor cHeroSummonActor)
				{
					CardsHandManager.Instance.RefreshPendingActiveBonus(cHeroSummonActor.Summoner);
				}
				if (gameObject28 != null)
				{
					DeathDissolve componentInChildren9 = gameObject28.GetComponentInChildren<DeathDissolve>();
					if (cActorDead_MessageData.m_OnDeathAbility && componentInChildren9.PlayedExternally())
					{
						action?.Invoke();
						ActorBehaviour.GetActorBehaviour(gameObject28).m_WorldspacePanelUI.Destroy();
						if (componentInChildren9 != null)
						{
							componentInChildren9.ExternalCleanup();
						}
					}
					else
					{
						if (componentInChildren9 != null)
						{
							StartCoroutine(componentInChildren9.Play());
						}
						CameraController.s_CameraController.RemoveFocalPointGameObject(gameObject28);
						Action onDestroy = action;
						ActorBehaviour.GetActorBehaviour(gameObject28)?.m_WorldspacePanelUI.Destroy(onDestroy);
					}
				}
				else
				{
					action?.Invoke();
				}
				UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.AllActorsDeadFiltered));
			}
			catch (Exception ex109)
			{
				Debug.LogError("An exception occurred while processing the ActorDead message\n" + ex109.Message + "\n" + ex109.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00098", "GUI_ERROR_MAIN_MENU_BUTTON", ex109.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex109.Message);
			}
			break;
		case CMessageData.MessageType.UpdateObjectiveProgress:
			try
			{
				if (IsRestarting)
				{
					return;
				}
				if (SaveData.Instance.Global.GameMode == EGameMode.LevelEditor)
				{
					break;
				}
				_ = (CUpdateObjectiveProgress_MessageData)message;
				UIManager.Instance.MissionObjectiveContainer.UpdateProgress();
				UIManager.Instance.BattleGoalContainer.UpdateProgress();
				if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
				{
					if (!PlayersInValidStartingPositions)
					{
						InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_PLAYER_STARTING_PLACEMENT", "GUI_TOOLTIP_TITLE_START_TURN");
						Singleton<HelpBox>.Instance.HighlightWarning();
					}
					else if (AnyPlayersInInvalidStartingPositionsForCompanionSummons)
					{
						ShowWarningWhenAnyPlayersInInvalidStartingPositionsForCompanionSummons();
					}
					else
					{
						InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_START_TURN", "GUI_TOOLTIP_TITLE_START_TURN");
					}
				}
			}
			catch (Exception ex108)
			{
				Debug.LogError("An exception occurred while processing the UpdateObjectiveProgress message\n" + ex108.Message + "\n" + ex108.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00099", "GUI_ERROR_MAIN_MENU_BUTTON", ex108.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex108.Message);
			}
			break;
		case CMessageData.MessageType.ActivateObjective:
			try
			{
				CActivateObjective_MessageData cActivateObjective_MessageData = (CActivateObjective_MessageData)message;
				UIManager.Instance.MissionObjectiveContainer.AddObjective(cActivateObjective_MessageData.m_ActivatedObjective);
			}
			catch (Exception ex107)
			{
				Debug.LogError("An exception occurred while processing the ActivateObjective message\n" + ex107.Message + "\n" + ex107.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00180", "GUI_ERROR_MAIN_MENU_BUTTON", ex107.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex107.Message);
			}
			break;
		case CMessageData.MessageType.DeactivateObjective:
			try
			{
				CDeactivateObjective_MessageData cDeactivateObjective_MessageData = (CDeactivateObjective_MessageData)message;
				UIManager.Instance.MissionObjectiveContainer.RemoveObjective(cDeactivateObjective_MessageData.m_DeactivatedObjective);
			}
			catch (Exception ex106)
			{
				Debug.LogError("An exception occurred while processing the DeactivateObjective message\n" + ex106.Message + "\n" + ex106.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00222", "GUI_ERROR_MAIN_MENU_BUTTON", ex106.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex106.Message);
			}
			break;
		case CMessageData.MessageType.CheckCompleteObjectives:
			try
			{
				_ = (CCheckCompleteObjectives_MessageData)message;
				UIManager.Instance.MissionObjectiveContainer.CheckToRemoveObjectives();
			}
			catch (Exception ex105)
			{
				Debug.LogError("An exception occurred while processing the CheckCompleteObjectives message\n" + ex105.Message + "\n" + ex105.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00180", "GUI_ERROR_MAIN_MENU_BUTTON", ex105.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex105.Message);
			}
			break;
		case CMessageData.MessageType.TriggeredObjectivesEventIdList:
			try
			{
				CTriggeredObjectivesEventIdList_MessageData cTriggeredObjectivesEventIdList_MessageData = (CTriggeredObjectivesEventIdList_MessageData)message;
				if (cTriggeredObjectivesEventIdList_MessageData.m_TriggeredObjectiveEventIdList != null && cTriggeredObjectivesEventIdList_MessageData.m_TriggeredObjectiveEventIdList.Count > 0)
				{
					for (int num16 = 0; num16 < cTriggeredObjectivesEventIdList_MessageData.m_TriggeredObjectiveEventIdList.Count; num16++)
					{
						UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ObjectiveCompleted, null, cTriggeredObjectivesEventIdList_MessageData.m_TriggeredObjectiveEventIdList[num16]));
					}
				}
			}
			catch (Exception ex104)
			{
				Debug.LogError("An exception occurred while processing the ObjectiveTriggeredEventIdList message\n" + ex104.Message + "\n" + ex104.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00206", "GUI_ERROR_MAIN_MENU_BUTTON", ex104.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex104.Message);
			}
			break;
		case CMessageData.MessageType.EndTurn:
			try
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.RoundStart);
				SaveData.Instance.Global.StopSpeedUp();
				InfusionBoardUI.Instance.UnreserveElements();
				CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				if (InitiativeTrack.Instance.SelectedActor().Actor.Class is CCharacterClass)
				{
					CardsHandManager.Instance.Reset((CPlayerActor)InitiativeTrack.Instance.SelectedActor().Actor);
				}
				if (InputManager.GamePadInUse)
				{
					Singleton<EnemyCurrentTurnStatPanel>.Instance.Hide();
				}
				Singleton<UIScenarioMultiplayerController>.Instance.OnEndTurn();
				HideSelectionUI();
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				if (message.m_ActorSpawningMessage is CPlayerActor cPlayerActor3)
				{
					cPlayerActor3.IsLongRestSelected = false;
					cPlayerActor3.IsLongRestActionSelected = false;
					cPlayerActor3.CharacterClass.LongRest = false;
				}
				GameObject gameObject27 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				if (gameObject27 != null)
				{
					ActorBehaviour.GetActorBehaviour(gameObject27).PauseLoco(pause: false);
				}
				if (m_CurrentActor != null)
				{
					UIEventManager.LogActorEndedTurnEvent(m_CurrentActor);
					m_CurrentActor = null;
					if (FFSNetwork.IsOnline)
					{
						InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
						{
							f.Avatar.RefreshActiveInteractable();
						});
					}
					UIManager.Instance.BattleGoalContainer.Hide();
				}
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
				SetChoreographerState(ChoreographerStateType.WaitingForEndTurnSync, 0, null);
			}
			catch (Exception ex103)
			{
				Debug.LogError("An exception occurred while processing the EndTurn message\n" + ex103.Message + "\n" + ex103.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00100", "GUI_ERROR_MAIN_MENU_BUTTON", ex103.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex103.Message);
			}
			break;
		case CMessageData.MessageType.EndRound:
			try
			{
				CardsHandManager.Instance.EnableCancelActiveAbilities = !FFSNetwork.IsOnline;
				CardsHandManager.Instance.Hide();
				ClearStars();
				m_ActorObjectsDiedInCurrentRound.Clear();
				ClearHilightedActors();
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				CameraController.s_CameraController.DisableCameraInput(disableInput: false);
				switch (ScenarioManager.CurrentScenarioState.CheckObjectivesComplete(isEndOfRound: true))
				{
				case EObjectiveResult.Win:
					SimpleLog.AddToSimpleLog("Win triggered at EndRound message after checking objective state.");
					s_Choreographer.WinScenario();
					break;
				case EObjectiveResult.Lose:
					SimpleLog.AddToSimpleLog("Lose triggered at EndRound message after checking objective state.");
					s_Choreographer.LoseScenario();
					break;
				default:
					if (s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.EndAbilityAnimSync)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForEndRoundSync, 0, null);
					}
					break;
				}
				if ((Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown) && (SaveData.Instance.Global.GameMode == EGameMode.Campaign || (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster && !SaveData.Instance.Global.AdventureData.AdventureMapState.IsPlayingTutorial && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.IntroCompleted)))
				{
					SaveData.Instance.Global.m_StatsDataStorage.ScrapeEventLog(EResult.InProgress, endScenario: false, m_CurrentState.RoundNumber);
					SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.InProgressQuestState?.CheckBattleGoals(useCurrentScenarioStats: true);
				}
			}
			catch (Exception ex102)
			{
				Debug.LogError("An exception occurred while processing the EndRound message\n" + ex102.Message + "\n" + ex102.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00101", "GUI_ERROR_MAIN_MENU_BUTTON", ex102.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex102.Message);
			}
			break;
		case CMessageData.MessageType.ActivateProp:
			try
			{
				if (IsRestarting)
				{
					return;
				}
				CActivateProp_MessageData cActivateProp_MessageData = (CActivateProp_MessageData)message;
				Debug.Log("Attempting to activate prop instance name: " + cActivateProp_MessageData.m_Prop.InstanceName);
				if (cActivateProp_MessageData.m_ActorSpawningMessage != null)
				{
					CActor actorSpawningMessage4 = cActivateProp_MessageData.m_ActorSpawningMessage;
					string text4 = LocalizationManager.GetTranslation(actorSpawningMessage4.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage4);
					SimpleLog.AddToSimpleLog(text4 + " activated prop: " + cActivateProp_MessageData.m_Prop.InstanceName + " of type: " + cActivateProp_MessageData.m_Prop.ObjectType.ToString() + " at ArrayIndex: " + cActivateProp_MessageData.m_Prop.ArrayIndex.ToString());
				}
				ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActivateProp_MessageData.m_Prop.ArrayIndex.X, cActivateProp_MessageData.m_Prop.ArrayIndex.Y].m_TileBehaviour.RefreshWorldSpaceIcon();
				switch (cActivateProp_MessageData.m_Prop.ObjectType)
				{
				case ScenarioManager.ObjectImportType.Door:
					CheckOpenDoor(cActivateProp_MessageData.m_Prop, cActivateProp_MessageData.m_ActorSpawningMessage, cActivateProp_MessageData.m_InitialLoad, openDoorOnly: false);
					break;
				case ScenarioManager.ObjectImportType.Chest:
				case ScenarioManager.ObjectImportType.GoalChest:
				{
					m_RewardsToShowcase.Clear();
					CTile cTile2 = ScenarioManager.Tiles[cActivateProp_MessageData.m_Prop.ArrayIndex.X, cActivateProp_MessageData.m_Prop.ArrayIndex.Y];
					CObjectChest cObjectChest = cActivateProp_MessageData.m_Prop as CObjectChest;
					if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
					{
						if (cObjectChest.HasChestPrepopulated)
						{
							CActor cActor3 = ((cActivateProp_MessageData.m_CreditActor == null) ? cActivateProp_MessageData.m_ActorSpawningMessage : cActivateProp_MessageData.m_CreditActor);
							m_RewardsToShowcase = cObjectChest.GetRewardsFromStartingItems();
							List<Reward> rewards = m_RewardsToShowcase.ToList();
							for (int num13 = m_RewardsToShowcase.Count - 1; num13 >= 0; num13--)
							{
								Reward reward = m_RewardsToShowcase[num13];
								if (reward.Item != null && reward.Item.YMLData.Slot == CItem.EItemSlot.QuestItem)
								{
									CItem item = reward.Item.Copy(AdventureState.MapState.GetGUIDBasedOnMapRNGState(), AdventureState.MapState.GetNextItemNetworkID());
									cActor3.Inventory.AddItem(item);
								}
							}
							if (cObjectChest.ObjectType == ScenarioManager.ObjectImportType.GoalChest)
							{
								ScenarioManager.CurrentScenarioState.GoalChestRewards.Add(new Tuple<string, RewardGroup>("party", new RewardGroup(rewards)));
							}
						}
						else
						{
							CMap hexMap = ScenarioManager.Tiles[cActivateProp_MessageData.m_Prop.ArrayIndex.X, cActivateProp_MessageData.m_Prop.ArrayIndex.Y].m_HexMap;
							m_RewardsToShowcase = SaveData.Instance.Global.AdventureData.AdventureMapState.RollTreasureChest(cObjectChest, hexMap.MapGuid, cActivateProp_MessageData.m_ActorSpawningMessage.Class.ID);
						}
					}
					else if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
					{
						CActor cActor4 = ((cActivateProp_MessageData.m_CreditActor == null) ? cActivateProp_MessageData.m_ActorSpawningMessage : cActivateProp_MessageData.m_CreditActor);
						if (cObjectChest.HasChestPrepopulated)
						{
							m_RewardsToShowcase = cObjectChest.GetRewardsFromStartingItems();
							List<Reward> rewards2 = m_RewardsToShowcase.ToList();
							for (int num14 = m_RewardsToShowcase.Count - 1; num14 >= 0; num14--)
							{
								Reward reward2 = m_RewardsToShowcase[num14];
								if (reward2.Item != null && reward2.Item.YMLData.Slot == CItem.EItemSlot.QuestItem)
								{
									CItem item2 = reward2.Item.Copy(AdventureState.MapState.GetGUIDBasedOnMapRNGState(), AdventureState.MapState.GetNextItemNetworkID());
									cActor4.Inventory.AddItem(item2);
								}
							}
							if (cObjectChest.ObjectType == ScenarioManager.ObjectImportType.GoalChest)
							{
								ScenarioManager.CurrentScenarioState.GoalChestRewards.Add(new Tuple<string, RewardGroup>("party", new RewardGroup(rewards2)));
							}
						}
						CMap hexMap2 = ScenarioManager.Tiles[cActivateProp_MessageData.m_Prop.ArrayIndex.X, cActivateProp_MessageData.m_Prop.ArrayIndex.Y].m_HexMap;
						m_RewardsToShowcase.AddRange(SaveData.Instance.Global.CampaignData.AdventureMapState.RollTreasureChest(cObjectChest, hexMap2.MapGuid, cActivateProp_MessageData.m_ActorSpawningMessage.Class.ID));
						if (cActor4 != null)
						{
							foreach (Reward item40 in m_RewardsToShowcase)
							{
								item40.GiveToCharacterType = EGiveToCharacterType.Equip;
								item40.GiveToCharacterID = cActor4.Class.ID;
							}
						}
						if (!cObjectChest.Conditions.IsNullOrEmpty())
						{
							foreach (CCondition.ENegativeCondition condition in cObjectChest.Conditions)
							{
								m_RewardsToShowcase.Add(new Reward(new List<RewardCondition>
								{
									new RewardCondition(RewardCondition.EConditionMapDuration.None, condition, -1)
								}, forEnemy: false));
							}
						}
						if (cObjectChest.DamageValue > 0)
						{
							m_RewardsToShowcase.Add(new Reward(ETreasureType.Damage, cObjectChest.DamageValue, ETreasureDistributionType.None, cActor4.Class.ID));
						}
					}
					else if (cObjectChest.HasChestPrepopulated)
					{
						m_RewardsToShowcase = cObjectChest.GetRewardsFromStartingItems();
						for (int num15 = m_RewardsToShowcase.Count - 1; num15 >= 0; num15--)
						{
							Reward reward3 = m_RewardsToShowcase[num15];
							if (reward3.Item != null && reward3.Item.YMLData.Slot == CItem.EItemSlot.QuestItem)
							{
								CItem item3 = reward3.Item.Copy(AdventureState.MapState.GetGUIDBasedOnMapRNGState(), AdventureState.MapState.GetNextItemNetworkID());
								((cActivateProp_MessageData.m_CreditActor == null) ? cActivateProp_MessageData.m_ActorSpawningMessage : cActivateProp_MessageData.m_CreditActor).Inventory.AddItem(item3);
							}
						}
						if (cObjectChest.ObjectType == ScenarioManager.ObjectImportType.GoalChest)
						{
							ScenarioManager.CurrentScenarioState.GoalChestRewards.Add(new Tuple<string, RewardGroup>("party", new RewardGroup(m_RewardsToShowcase)));
						}
					}
					GameObject propObject13 = Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					if (propObject13 != null)
					{
						GameObject character2 = FindClientActorGameObject(cActivateProp_MessageData.m_ActorSpawningMessage);
						CharacterManager characterManager2 = CharacterManager.GetCharacterManager(character2);
						if (characterManager2 != null)
						{
							characterManager2.ChestsToCollect.Add(propObject13);
						}
						PlayableDirector componentInChildren6 = propObject13.GetComponentInChildren<PlayableDirector>();
						if (componentInChildren6 != null)
						{
							componentInChildren6.Play();
							UnityEngine.Object.Destroy(propObject13, (float)componentInChildren6.duration);
						}
						SEventObjectProp sEventObjectProp2 = SEventLog.FindLastObjectEventOfSubTypeAndObjectType(ESESubTypeObjectProp.Activated, ScenarioManager.ObjectImportType.Chest);
						if (sEventObjectProp2 != null)
						{
							CActor actorSpawningMessage8 = cActivateProp_MessageData.m_ActorSpawningMessage;
							if (actorSpawningMessage8 != null)
							{
								string arg12 = LocalizationManager.GetTranslation(actorSpawningMessage8.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage8);
								SEventObjectPropChest sEventObjectPropChest = (SEventObjectPropChest)sEventObjectProp2;
								Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ACTIVATE_CHESTDAMAGE"), arg12, $"<b>{LocalizationManager.GetTranslation(sEventObjectProp2.ObjectName)}</b>", string.Format("<b><color=#f76767>{0}</color></b>", "<sprite name=Attack> " + sEventObjectPropChest.Damage)), CombatLogFilter.DAMAGE);
							}
						}
						cTile2.m_Props.Remove(cActivateProp_MessageData.m_Prop);
						if (!m_RewardsToShowcase.IsNullOrEmpty())
						{
							m_ShouldShowRewards = true;
							SetChoreographerState(ChoreographerStateType.WaitingForRewardsProcess, 1150, cActivateProp_MessageData.m_ActorSpawningMessage);
							m_BlockClientMessageProcessing = true;
						}
						else
						{
							ScenarioRuleClient.ToggleMessageProcessing(process: true);
						}
						break;
					}
					throw new Exception("Prop GameObject for instance name: " + cActivateProp_MessageData.m_Prop.InstanceName + " not found");
				}
				case ScenarioManager.ObjectImportType.MoneyToken:
				{
					GameObject propObject9 = Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					if (propObject9 != null)
					{
						GameObject character = FindClientActorGameObject(cActivateProp_MessageData.m_ActorSpawningMessage);
						CharacterManager characterManager = CharacterManager.GetCharacterManager(character);
						if (characterManager != null)
						{
							characterManager.GoldPilesToCollect.Add(propObject9);
						}
						else
						{
							Debug.LogError("Failed to pick up gold pile");
						}
						break;
					}
					throw new Exception("Prop GameObject for instance name: " + cActivateProp_MessageData.m_Prop.InstanceName + " not found");
				}
				case ScenarioManager.ObjectImportType.Trap:
				{
					GameObject propObject14 = Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					if (propObject14 != null)
					{
						StartCoroutine(PrimePropActivationAnimation(propObject14, "Trap_Shut"));
						SEventObjectProp sEventObjectProp3 = SEventLog.FindLastObjectEventOfSubTypeAndObjectType(ESESubTypeObjectProp.Activated, ScenarioManager.ObjectImportType.Trap);
						if (sEventObjectProp3 != null)
						{
							CActor actorSpawningMessage9 = cActivateProp_MessageData.m_ActorSpawningMessage;
							if (actorSpawningMessage9 != null)
							{
								string arg13 = LocalizationManager.GetTranslation(actorSpawningMessage9.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage9);
								SEventObjectPropTrap sEventObjectPropTrap2 = (SEventObjectPropTrap)sEventObjectProp3;
								Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ACTIVATE_TRAP"), arg13, $"<b>{LocalizationManager.GetTranslation(sEventObjectProp3.ObjectName)}</b>", string.Format("<b><color=#f76767>{0}</color></b>", "<sprite name=Attack> " + sEventObjectPropTrap2.Damage)), CombatLogFilter.DAMAGE);
							}
						}
						break;
					}
					throw new Exception("Prop GameObject for instance name: " + cActivateProp_MessageData.m_Prop.InstanceName + " not found");
				}
				case ScenarioManager.ObjectImportType.Spawner:
				{
					GameObject propObject10 = Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					if (propObject10 != null)
					{
						UnityEngine.Object.Destroy(propObject10);
						break;
					}
					throw new Exception("Prop GameObject for instance name: " + cActivateProp_MessageData.m_Prop.InstanceName + " not found");
				}
				case ScenarioManager.ObjectImportType.PressurePlate:
				{
					GameObject propObject8 = Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					if (propObject8 != null)
					{
						MF.GameObjectAnimatorPlay(propObject8, "Pressed");
						break;
					}
					throw new Exception("Prop GameObject for instance name: " + cActivateProp_MessageData.m_Prop.InstanceName + " not found");
				}
				case ScenarioManager.ObjectImportType.TerrainHotCoals:
				case ScenarioManager.ObjectImportType.TerrainThorns:
				{
					Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					SEventObjectProp sEventObjectProp = SEventLog.FindLastObjectEventOfSubTypeAndObjectType(ESESubTypeObjectProp.Activated, cActivateProp_MessageData.m_Prop.ObjectType);
					if (sEventObjectProp != null)
					{
						CActor actorSpawningMessage7 = cActivateProp_MessageData.m_ActorSpawningMessage;
						if (actorSpawningMessage7 != null)
						{
							string arg11 = LocalizationManager.GetTranslation(actorSpawningMessage7.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage7);
							SEventObjectPropTrap sEventObjectPropTrap = (SEventObjectPropTrap)sEventObjectProp;
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ACTIVATE_HAZARDOUS_TERRAIN"), arg11, $"<b>{LocalizationManager.GetTranslation(sEventObjectProp.ObjectName)}</b>", string.Format("<b><color=#f76767>{0}</color></b>", "<sprite name=Attack> " + sEventObjectPropTrap.Damage)), CombatLogFilter.DAMAGE);
						}
					}
					break;
				}
				case ScenarioManager.ObjectImportType.Portal:
					RevealRoomCreateCharacterActors();
					break;
				case ScenarioManager.ObjectImportType.CarryableQuestItem:
				{
					GameObject propObject11 = Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					CActor actorSpawningMessage5 = cActivateProp_MessageData.m_ActorSpawningMessage;
					if (actorSpawningMessage5 != null)
					{
						string arg9 = LocalizationManager.GetTranslation(actorSpawningMessage5.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage5);
						string prefabName = cActivateProp_MessageData.m_Prop.PrefabName;
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT"), arg9, prefabName));
						ActorBehaviour.GetActorBehaviour(FindClientActor(actorSpawningMessage5)).m_WorldspacePanelUI.RefreshCarryingQuestItem();
					}
					if (propObject11 != null)
					{
						PlayableDirector componentInChildren4 = propObject11.GetComponentInChildren<PlayableDirector>();
						if (componentInChildren4 != null)
						{
							componentInChildren4.Play();
							UnityEngine.Object.Destroy(propObject11, (float)componentInChildren4.duration);
						}
						else
						{
							UnityEngine.Object.Destroy(propObject11, 1f);
						}
					}
					else if (!cActivateProp_MessageData.m_CarriedByActor)
					{
						throw new Exception("Prop GameObject for instance name: " + cActivateProp_MessageData.m_Prop.InstanceName + " not found");
					}
					break;
				}
				case ScenarioManager.ObjectImportType.Resource:
				{
					GameObject propObject12 = Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					CActor actorSpawningMessage6 = cActivateProp_MessageData.m_ActorSpawningMessage;
					if (actorSpawningMessage6 != null)
					{
						string prefabName2 = cActivateProp_MessageData.m_Prop.PrefabName;
						if (!prefabName2.Contains("Favorite"))
						{
							string arg10 = LocalizationManager.GetTranslation(actorSpawningMessage6.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage6);
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT"), arg10, prefabName2));
						}
					}
					if (propObject12 != null)
					{
						PlayableDirector componentInChildren5 = propObject12.GetComponentInChildren<PlayableDirector>();
						if (componentInChildren5 != null)
						{
							componentInChildren5.Play();
							UnityEngine.Object.Destroy(propObject12, (float)componentInChildren5.duration);
						}
						else
						{
							UnityEngine.Object.Destroy(propObject12, 1f);
						}
					}
					else if (!cActivateProp_MessageData.m_CarriedByActor)
					{
						throw new Exception("Prop GameObject for instance name: " + cActivateProp_MessageData.m_Prop.InstanceName + " not found");
					}
					break;
				}
				case ScenarioManager.ObjectImportType.GenericProp:
				case ScenarioManager.ObjectImportType.MonsterGrave:
				{
					GameObject propObject7 = Singleton<ObjectCacheService>.Instance.GetPropObject(cActivateProp_MessageData.m_Prop);
					if (propObject7 != null)
					{
						PlayableDirector componentInChildren3 = propObject7.GetComponentInChildren<PlayableDirector>();
						if (componentInChildren3 != null)
						{
							componentInChildren3.Play();
							UnityEngine.Object.Destroy(propObject7, (float)componentInChildren3.duration);
						}
						else
						{
							UnityEngine.Object.Destroy(propObject7, 1f);
						}
					}
					else if (!cActivateProp_MessageData.m_CarriedByActor)
					{
						throw new Exception("Prop GameObject for instance name: " + cActivateProp_MessageData.m_Prop.InstanceName + " not found");
					}
					break;
				}
				}
				UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.PropActivated, null, cActivateProp_MessageData.m_Prop.PropGuid, 0, null, cActivateProp_MessageData.m_Prop.ActorActivated));
			}
			catch (Exception ex101)
			{
				Debug.LogError("An exception occurred while processing the ActivateProp message\n" + ex101.Message + "\n" + ex101.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00102", "GUI_ERROR_MAIN_MENU_BUTTON", ex101.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex101.Message);
			}
			break;
		case CMessageData.MessageType.DeactivatePropAnim:
			try
			{
				if (IsRestarting)
				{
					return;
				}
				CDeactivatePropAnim_MessageData cDeactivatePropAnim_MessageData = (CDeactivatePropAnim_MessageData)message;
				GameObject propObject6 = Singleton<ObjectCacheService>.Instance.GetPropObject(cDeactivatePropAnim_MessageData.m_Prop);
				if (propObject6 != null)
				{
					switch (cDeactivatePropAnim_MessageData.m_Prop.ObjectType)
					{
					case ScenarioManager.ObjectImportType.Trap:
						MF.GameObjectAnimatorPlay(propObject6, "Trap_Shut");
						break;
					case ScenarioManager.ObjectImportType.PressurePlate:
						MF.GameObjectAnimatorPlay(propObject6, "UnPressed");
						break;
					case ScenarioManager.ObjectImportType.Spawner:
						MF.GameObjectAnimatorPlay(propObject6, "Deactivate");
						break;
					case ScenarioManager.ObjectImportType.Door:
						MF.GameObjectAnimatorPlay(propObject6, "Idle");
						break;
					}
				}
				else
				{
					Debug.LogWarning("Unable to find game object for prop: " + cDeactivatePropAnim_MessageData.m_Prop.InstanceName);
				}
				UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.PropDeactivated, null, cDeactivatePropAnim_MessageData.m_Prop.PropGuid));
			}
			catch (Exception ex100)
			{
				Debug.LogError("An exception occurred while processing the DeactivateProp message\n" + ex100.Message + "\n" + ex100.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00103", "GUI_ERROR_MAIN_MENU_BUTTON", ex100.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex100.Message);
			}
			break;
		case CMessageData.MessageType.UnlockLockedDoor:
			try
			{
				CUnlockLockedDoor_MessageData cUnlockLockedDoor_MessageData = (CUnlockLockedDoor_MessageData)message;
				if (cUnlockLockedDoor_MessageData.m_ActorSpawningMessage != null)
				{
					CActor actorSpawningMessage3 = cUnlockLockedDoor_MessageData.m_ActorSpawningMessage;
					string text3 = LocalizationManager.GetTranslation(actorSpawningMessage3.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage3);
					SimpleLog.AddToSimpleLog(text3 + " unlocked door: " + cUnlockLockedDoor_MessageData.m_Prop.InstanceName + " at ArrayIndex: " + cUnlockLockedDoor_MessageData.m_Prop.ArrayIndex.ToString());
				}
				if (cUnlockLockedDoor_MessageData.m_Prop.ObjectType == ScenarioManager.ObjectImportType.Door)
				{
					UnlockLockedDoorWithoutOpening(cUnlockLockedDoor_MessageData.m_Prop);
					UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.DoorUnlocked, null, cUnlockLockedDoor_MessageData.m_Prop.PropGuid));
				}
			}
			catch (Exception ex99)
			{
				Debug.LogError("An exception occurred while processing the Unlock Locked Door message\n" + ex99.Message + "\n" + ex99.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00205", "GUI_ERROR_MAIN_MENU_BUTTON", ex99.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex99.Message);
			}
			break;
		case CMessageData.MessageType.ActorSelectedTileToPullTowards:
			try
			{
				CActorSelectedTileToPullTowards cActorSelectedTileToPullTowards = (CActorSelectedTileToPullTowards)message;
				bool animationShouldPlay19 = false;
				CActor animatingActorToWaitFor19 = cActorSelectedTileToPullTowards.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorSelectedTileToPullTowards.m_PullAbility, cActorSelectedTileToPullTowards.m_ActorSpawningMessage, new List<string> { cActorSelectedTileToPullTowards.AnimOverload }, out animationShouldPlay19, out animatingActorToWaitFor19);
				if (animatingActorToWaitFor19 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay19 ? 10000 : 400, animatingActorToWaitFor19);
				}
				GameObject gameObject26 = FindClientActorGameObject(cActorSelectedTileToPullTowards.m_ActorSpawningMessage);
				if (!animationShouldPlay19)
				{
					ActorEvents.GetActorEvents(gameObject26).ProgressChoreographer();
				}
			}
			catch (Exception ex98)
			{
				Debug.LogError("An exception occurred while processing the ActorSelectedTileToPullTowards message\n" + ex98.Message + "\n" + ex98.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00104", "GUI_ERROR_MAIN_MENU_BUTTON", ex98.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex98.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsSelectingPullTile:
			try
			{
				CActorIsSelectingPullTile_MessageData cActorIsSelectingPullTile_MessageData = message as CActorIsSelectingPullTile_MessageData;
				Waypoint.s_MovingActor = cActorIsSelectingPullTile_MessageData.m_PullAbility.CurrentTarget;
				s_Choreographer.m_CurrentActor = cActorIsSelectingPullTile_MessageData.m_PullAbility.TargetingActor;
				s_Choreographer.m_CurrentAbility = cActorIsSelectingPullTile_MessageData.m_PullAbility;
				ClearAllActorEvents();
				if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
					if (!m_FollowCameraDisabled)
					{
						CameraController.s_CameraController.DisableCameraInput(disableInput: false);
					}
					GameObject nextWaypoint2 = Waypoint.GetNextWaypoint();
					if (nextWaypoint2 != null)
					{
						Waypoint waypointComponent2 = Waypoint.GetWaypointComponent(nextWaypoint2);
						s_Choreographer.TileHandler(waypointComponent2.ClientTile, Waypoint.GetCTileList());
						Waypoint.DestroyWaypoint(nextWaypoint2);
						GUIInterface.s_GUIInterface.SetStatusText("Next stage of path" + message.m_ActorSpawningMessage.ID + " Waypoint Order " + waypointComponent2.Order);
					}
					else
					{
						TileBehaviour.SetCallback(Waypoint.TileHandler);
						SetChoreographerState(ChoreographerStateType.WaitingForPlayerPullWaypointSelection, 0, null);
						WorldspaceStarHexDisplay.Instance.CurrentMoveDisplayType = WorldspaceStarHexDisplay.EMoveDisplayType.Pull;
						WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.MovementSelection;
						WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
						DisableTileSelection(active: false);
						GUIInterface.s_GUIInterface.SetStatusText("Select way points for player path" + message.m_ActorSpawningMessage.ID + " . Number moves " + cActorIsSelectingPullTile_MessageData.m_PullAbility.RemainingPulls);
					}
				}
				else
				{
					GUIInterface.s_GUIInterface.SetStatusText("Monster " + LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey()) + "(" + message.m_ActorSpawningMessage.ID + ") moving , moves remaining " + cActorIsSelectingPullTile_MessageData.m_PullAbility.RemainingPulls);
				}
				if (!m_FollowCameraDisabled)
				{
					GameObject target2 = FindClientActorGameObject(cActorIsSelectingPullTile_MessageData.m_PullAbility.CurrentTarget);
					CameraController.s_CameraController.SmartFocus(target2, pauseDuringTransition: true);
				}
				if (m_WaitState.m_State != ChoreographerStateType.WaitingForMoveAnim)
				{
					m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_PULL"), hideOnClick: true, cActorIsSelectingPullTile_MessageData.m_PullAbility.CanSkip);
					readyButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_PULL"), hideOnClick: true, glowingEffect: true);
					m_UndoButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player);
					m_UndoButton.SetInteractable(cActorIsSelectingPullTile_MessageData.m_PullAbility.CanUndo && FirstAbility);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				}
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.PullTileSelection);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.PullTileSelection);
					}
				}
			}
			catch (Exception ex97)
			{
				Debug.LogError("An exception occurred while processing the ActorIsSelectingPullTile message\n" + ex97.Message + "\n" + ex97.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00105", "GUI_ERROR_MAIN_MENU_BUTTON", ex97.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex97.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsPulling:
			try
			{
				CActorIsPulling_MessageData cActorIsPulling_MessageData = (CActorIsPulling_MessageData)message;
				CurrentAttackTargets.Clear();
				CurrentAttackArea.Clear();
				if (cActorIsPulling_MessageData.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
				}
				ClearStars();
				TileBehaviour.SetCallback(TileHandler);
				if (!cActorIsPulling_MessageData.m_PullAbility.UseSubAbilityTargeting)
				{
					GameObject gameObject25 = FindClientActorGameObject(cActorIsPulling_MessageData.m_PullAbility.CurrentTarget);
					FindClientActorGameObject(cActorIsPulling_MessageData.m_ActorSpawningMessage).transform.LookAt(gameObject25.transform.position);
					bool animationShouldPlay18 = false;
					CActor animatingActorToWaitFor18 = cActorIsPulling_MessageData.m_ActorSpawningMessage;
					ProcessActorAnimation(cActorIsPulling_MessageData.m_PullAbility, cActorIsPulling_MessageData.m_ActorSpawningMessage, new List<string> { cActorIsPulling_MessageData.AnimOverload, "Pull" }, out animationShouldPlay18, out animatingActorToWaitFor18);
					if (animatingActorToWaitFor18 != null)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay18 ? 10000 : 400, animatingActorToWaitFor18);
					}
				}
				else
				{
					ScenarioRuleClient.StepComplete();
				}
			}
			catch (Exception ex96)
			{
				Debug.LogError("An exception occurred while processing the ActorIsPulling message\n" + ex96.Message + "\n" + ex96.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00106", "GUI_ERROR_MAIN_MENU_BUTTON", ex96.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex96.Message);
			}
			break;
		case CMessageData.MessageType.ActorHasPulled:
			try
			{
				CActorHasPulled_MessageData cActorHasPulled_MessageData = (CActorHasPulled_MessageData)message;
				GameObject gameObject24 = FindClientActorGameObject(cActorHasPulled_MessageData.m_PullAbility.CurrentTarget);
				if (!m_FollowCameraDisabled)
				{
					CameraController.s_CameraController.SetFocalPointGameObject(gameObject24);
					CameraController.s_CameraController.DisableCameraInput(disableInput: true);
				}
				Vector3 position3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActorHasPulled_MessageData.m_PullAbility.CurrentTarget.ArrayIndex.X, cActorHasPulled_MessageData.m_PullAbility.CurrentTarget.ArrayIndex.Y].m_GameObject.transform.position;
				int num12 = 0;
				if (MF.GetGameObjectAnimatorController(gameObject24) != null)
				{
					num12 = 10000;
					ActorBehaviour.GetActorBehaviour(gameObject24).PushPullToLocation(position3);
				}
				else
				{
					num12 = 400;
					gameObject24.transform.position = position3;
				}
				SetChoreographerState(ChoreographerStateType.WaitingForMoveAnim, num12, cActorHasPulled_MessageData.m_PullAbility.CurrentTarget);
				Waypoint.Clear();
			}
			catch (Exception ex95)
			{
				Debug.LogError("An exception occurred while processing the ActorHasPulled message\n" + ex95.Message + "\n" + ex95.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00107", "GUI_ERROR_MAIN_MENU_BUTTON", ex95.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex95.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsSelectingPushTile:
			try
			{
				CActorIsSelectingPushTile_MessageData cActorIsSelectingPushTile_MessageData = message as CActorIsSelectingPushTile_MessageData;
				Waypoint.s_MovingActor = cActorIsSelectingPushTile_MessageData.m_PushAbility.CurrentTarget;
				s_Choreographer.m_CurrentActor = cActorIsSelectingPushTile_MessageData.m_PushAbility.TargetingActor;
				s_Choreographer.m_CurrentAbility = cActorIsSelectingPushTile_MessageData.m_PushAbility;
				ClearAllActorEvents();
				if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
					if (!m_FollowCameraDisabled)
					{
						CameraController.s_CameraController.DisableCameraInput(disableInput: false);
					}
					WorldspaceStarHexDisplay.Instance.LockView = false;
					GameObject nextWaypoint = Waypoint.GetNextWaypoint();
					if (nextWaypoint != null)
					{
						Waypoint waypointComponent = Waypoint.GetWaypointComponent(nextWaypoint);
						s_Choreographer.TileHandler(waypointComponent.ClientTile, Waypoint.GetCTileList());
						Waypoint.DestroyWaypoint(nextWaypoint);
						GUIInterface.s_GUIInterface.SetStatusText("Next stage of path" + message.m_ActorSpawningMessage.ID + " Waypoint Order " + waypointComponent.Order);
					}
					else
					{
						TileBehaviour.SetCallback(Waypoint.TileHandler);
						SetChoreographerState(ChoreographerStateType.WaitingForPlayerPushWaypointSelection, 0, null);
						WorldspaceStarHexDisplay.Instance.CurrentMoveDisplayType = WorldspaceStarHexDisplay.EMoveDisplayType.Push;
						WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.MovementSelection;
						WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
						DisableTileSelection(active: false);
						GUIInterface.s_GUIInterface.SetStatusText("Select way points for player path" + message.m_ActorSpawningMessage.ID + " . Number moves " + cActorIsSelectingPushTile_MessageData.m_PushAbility.RemainingPushes);
					}
				}
				else
				{
					GUIInterface.s_GUIInterface.SetStatusText("Monster " + LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey()) + "(" + message.m_ActorSpawningMessage.ID + ") moving , moves remaining " + cActorIsSelectingPushTile_MessageData.m_PushAbility.RemainingPushes);
					if (!m_FollowCameraDisabled)
					{
						GameObject focalPointGameObject = FindClientActorGameObject(cActorIsSelectingPushTile_MessageData.m_PushAbility.CurrentTarget);
						CameraController.s_CameraController.SetFocalPointGameObject(focalPointGameObject);
					}
				}
				if (m_WaitState.m_State != ChoreographerStateType.WaitingForMoveAnim)
				{
					m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_PUSH"), hideOnClick: true, cActorIsSelectingPushTile_MessageData.m_PushAbility.CanSkip);
					readyButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_PUSH"), hideOnClick: true, glowingEffect: true);
					m_UndoButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player);
					m_UndoButton.SetInteractable(active: false);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				}
				if (FFSNetwork.IsOnline && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (!message.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.PushTileSelection);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.PushTileSelection);
					}
				}
			}
			catch (Exception ex94)
			{
				Debug.LogError("An exception occurred while processing the ActorIsSelectingPushTile message\n" + ex94.Message + "\n" + ex94.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00108", "GUI_ERROR_MAIN_MENU_BUTTON", ex94.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex94.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsPushing:
			try
			{
				CActorIsPushing_MessageData cActorIsPushing_MessageData = (CActorIsPushing_MessageData)message;
				CurrentAttackTargets.Clear();
				CurrentAttackArea.Clear();
				ClearStars();
				TileBehaviour.SetCallback(TileHandler);
				if (cActorIsPushing_MessageData.m_PushAbility.ValidTilesInAreaAffected != null && cActorIsPushing_MessageData.m_PushAbility.ValidTilesInAreaAffected.Count > 0)
				{
					foreach (CTile item41 in cActorIsPushing_MessageData.m_PushAbility.ValidTilesInAreaAffected)
					{
						CurrentAttackArea.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item41.m_ArrayIndex.X, item41.m_ArrayIndex.Y]);
					}
				}
				if (!cActorIsPushing_MessageData.m_PushAbility.UseSubAbilityTargeting)
				{
					GameObject gameObject23 = FindClientActorGameObject(cActorIsPushing_MessageData.m_PushAbility.CurrentTarget);
					FindClientActorGameObject(cActorIsPushing_MessageData.m_ActorSpawningMessage).transform.LookAt(gameObject23.transform.position);
					bool animationShouldPlay17 = false;
					CActor animatingActorToWaitFor17 = cActorIsPushing_MessageData.m_ActorSpawningMessage;
					ProcessActorAnimation(cActorIsPushing_MessageData.m_PushAbility, cActorIsPushing_MessageData.m_ActorSpawningMessage, new List<string> { cActorIsPushing_MessageData.AnimOverload, "Push" }, out animationShouldPlay17, out animatingActorToWaitFor17);
					if (animatingActorToWaitFor17 != null)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay17 ? 10000 : 400, animatingActorToWaitFor17);
					}
				}
				else
				{
					ScenarioRuleClient.StepComplete();
				}
			}
			catch (Exception ex93)
			{
				Debug.LogError("An exception occurred while processing the ActorIsPushing message\n" + ex93.Message + "\n" + ex93.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00109", "GUI_ERROR_MAIN_MENU_BUTTON", ex93.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex93.Message);
			}
			break;
		case CMessageData.MessageType.ActorHasPushed:
			try
			{
				CActorHasPushed_MessageData cActorHasPushed_MessageData = (CActorHasPushed_MessageData)message;
				GameObject gameObject22 = FindClientActorGameObject(cActorHasPushed_MessageData.m_ActorBeingPushed);
				if (gameObject22 != null)
				{
					if (cActorHasPushed_MessageData.m_PushAbility != null && !m_FollowCameraDisabled)
					{
						CameraController.s_CameraController.SetFocalPointGameObject(gameObject22);
						CameraController.s_CameraController.DisableCameraInput(disableInput: true);
					}
					Vector3 position2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActorHasPushed_MessageData.m_ActorBeingPushed.ArrayIndex.X, cActorHasPushed_MessageData.m_ActorBeingPushed.ArrayIndex.Y].m_GameObject.transform.position;
					int num11 = 0;
					if (MF.GetGameObjectAnimatorController(gameObject22) != null)
					{
						num11 = 10000;
						ActorBehaviour.GetActorBehaviour(gameObject22).PushPullToLocation(position2);
					}
					else
					{
						num11 = 400;
						gameObject22.transform.position = position2;
					}
					SetChoreographerState(ChoreographerStateType.WaitingForMoveAnim, num11, cActorHasPushed_MessageData.m_ActorBeingPushed);
				}
				Waypoint.Clear();
			}
			catch (Exception ex92)
			{
				Debug.LogError("An exception occurred while processing the ActorHasPushed message\n" + ex92.Message + "\n" + ex92.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00110", "GUI_ERROR_MAIN_MENU_BUTTON", ex92.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex92.Message);
			}
			break;
		case CMessageData.MessageType.UpdateAdditionalPushDamagePreview:
			try
			{
				CUpdateAdditionalPushDamagePreview_MessageData cUpdateAdditionalPushDamagePreview_MessageData = (CUpdateAdditionalPushDamagePreview_MessageData)message;
				if (cUpdateAdditionalPushDamagePreview_MessageData.m_PushAbility.ValidActorsInRange == null)
				{
					break;
				}
				if (cUpdateAdditionalPushDamagePreview_MessageData.m_PushAbility.AdditionalPushDamageSummary != null)
				{
					int targetIndex = 0;
					CAttackSummary.TargetSummary targetSummary = cUpdateAdditionalPushDamagePreview_MessageData.m_PushAbility.AdditionalPushDamageSummary.FindTarget(cUpdateAdditionalPushDamagePreview_MessageData.m_PushAbility.CurrentTarget, ref targetIndex);
					if (targetSummary != null)
					{
						ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cUpdateAdditionalPushDamagePreview_MessageData.m_PushAbility.CurrentTarget)).m_WorldspacePanelUI.OnSelectingDamageFocus(cUpdateAdditionalPushDamagePreview_MessageData.m_PushAbility, cUpdateAdditionalPushDamagePreview_MessageData.m_ActorSpawningMessage, targetSummary);
					}
				}
				else
				{
					ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(cUpdateAdditionalPushDamagePreview_MessageData.m_PushAbility.CurrentTarget)).m_WorldspacePanelUI.ResetDamagePreview(0);
				}
			}
			catch (Exception ex91)
			{
				Debug.LogError("An exception occurred while processing the UpdateAdditionalPushDamagePreview message\n" + ex91.Message + "\n" + ex91.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00111", "GUI_ERROR_MAIN_MENU_BUTTON", ex91.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex91.Message);
			}
			break;
		case CMessageData.MessageType.ActionAbilityHasHappened:
			try
			{
				CActionAbilityHasHappened_MessageData cActionAbilityHasHappened_MessageData = (CActionAbilityHasHappened_MessageData)message;
				if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (cActionAbilityHasHappened_MessageData.PendingElementsToInfuse)
					{
						CardsHandManager.Instance.UpdateElements(message.m_ActorSpawningMessage as CPlayerActor);
					}
					if (m_CurrentAbility != null && m_CurrentAbility.AbilityType == CAbility.EAbilityType.ImprovedShortRest)
					{
						InitiativeTrack.Instance.RefreshActorUI(message.m_ActorSpawningMessage);
					}
					if (cActionAbilityHasHappened_MessageData.m_Ability != null)
					{
						Singleton<AbilityEffectManager>.Instance.RemoveEffect(cActionAbilityHasHappened_MessageData.m_Ability);
					}
				}
			}
			catch (Exception ex90)
			{
				Debug.LogError("An exception occurred while processing the ActionAbilityHasHappened message\n" + ex90.Message + "\n" + ex90.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00112", "GUI_ERROR_MAIN_MENU_BUTTON", ex90.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex90.Message);
			}
			break;
		case CMessageData.MessageType.Spawn:
			try
			{
				CSpawn_MessageData messageData7 = (CSpawn_MessageData)message;
				StartCoroutine(SpawnProp(messageData7));
			}
			catch (Exception ex89)
			{
				Debug.LogError("An exception occurred while processing the Spawn message\n" + ex89.Message + "\n" + ex89.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00113", "GUI_ERROR_MAIN_MENU_BUTTON", ex89.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex89.Message);
			}
			break;
		case CMessageData.MessageType.Summon:
			try
			{
				CSummon_MessageData cSummon_MessageData = (CSummon_MessageData)message;
				ClearAllActorEvents();
				s_Choreographer.m_CurrentAbility = cSummon_MessageData.m_SummonAbility;
				GameObject gameObject21 = FindClientActorGameObject(cSummon_MessageData.m_ActorSummoning);
				Vector3 zero2 = Vector3.zero;
				foreach (CTile summonTile in cSummon_MessageData.m_SummonTiles)
				{
					zero2 += ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[summonTile.m_ArrayIndex.X, summonTile.m_ArrayIndex.Y].m_GameObject.transform.position;
				}
				zero2 /= (float)cSummon_MessageData.m_SummonTiles.Count;
				if (gameObject21 != null)
				{
					gameObject21.transform.LookAt(zero2);
				}
				bool animationShouldPlay16 = false;
				CActor animatingActorToWaitFor16 = cSummon_MessageData.m_ActorSummoning;
				ProcessActorAnimation(cSummon_MessageData.m_SummonAbility, cSummon_MessageData.m_ActorSummoning, new List<string>
				{
					cSummon_MessageData.AnimOverload,
					GetNonOverloadAnim(cSummon_MessageData.m_SummonAbility)
				}, out animationShouldPlay16, out animatingActorToWaitFor16);
				if (animatingActorToWaitFor16 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay16 ? 10000 : 400, animatingActorToWaitFor16);
				}
				if (!animationShouldPlay16)
				{
					if (gameObject21 != null)
					{
						ActorEvents.GetActorEvents(gameObject21).ProgressChoreographer();
					}
					SummonSMB.SummonCharacters(cSummon_MessageData.m_SummonedActors);
				}
			}
			catch (Exception ex88)
			{
				Debug.LogError("An exception occurred while processing the Summon message\n" + ex88.Message + "\n" + ex88.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00114", "GUI_ERROR_MAIN_MENU_BUTTON", ex88.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex88.Message);
			}
			break;
		case CMessageData.MessageType.Revive:
			try
			{
				CRevive_MessageData cRevive_MessageData = (CRevive_MessageData)message;
				ClearAllActorEvents();
				s_Choreographer.m_CurrentAbility = cRevive_MessageData.m_ReviveAbility;
				GameObject gameObject20 = FindClientActorGameObject(cRevive_MessageData.m_ActorReviving);
				Vector3 zero = Vector3.zero;
				foreach (CTile reviveTile in cRevive_MessageData.m_ReviveTiles)
				{
					zero += ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[reviveTile.m_ArrayIndex.X, reviveTile.m_ArrayIndex.Y].m_GameObject.transform.position;
				}
				zero /= (float)cRevive_MessageData.m_ReviveTiles.Count;
				gameObject20.transform.LookAt(zero);
				bool animationShouldPlay15 = false;
				CActor animatingActorToWaitFor15 = cRevive_MessageData.m_ActorReviving;
				ProcessActorAnimation(cRevive_MessageData.m_ReviveAbility, cRevive_MessageData.m_ActorReviving, new List<string>
				{
					cRevive_MessageData.AnimOverload,
					GetNonOverloadAnim(cRevive_MessageData.m_ReviveAbility)
				}, out animationShouldPlay15, out animatingActorToWaitFor15);
				if (animatingActorToWaitFor15 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay15 ? 10000 : 400, animatingActorToWaitFor15);
				}
				if (!animationShouldPlay15)
				{
					ActorEvents.GetActorEvents(gameObject20).ProgressChoreographer();
				}
			}
			catch (Exception ex87)
			{
				Debug.LogError("An exception occurred while processing the Revive message\n" + ex87.Message + "\n" + ex87.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00231", "GUI_ERROR_MAIN_MENU_BUTTON", ex87.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex87.Message);
			}
			break;
		case CMessageData.MessageType.RevealProp:
			try
			{
				CRevealDoor_MessageData cRevealDoor_MessageData = (CRevealDoor_MessageData)message;
				GameObject propObject5 = Singleton<ObjectCacheService>.Instance.GetPropObject(cRevealDoor_MessageData.m_Prop);
				if (propObject5 != null && propObject5.activeSelf && cRevealDoor_MessageData.m_Prop.ObjectType == ScenarioManager.ObjectImportType.Door)
				{
					Renderer[] componentsInChildren = propObject5.GetComponentsInChildren<Renderer>();
					Renderer[] array = componentsInChildren;
					for (int num10 = 0; num10 < array.Length; num10++)
					{
						array[num10].enabled = true;
					}
					UnityGameEditorDoorProp component = propObject5.GetComponent<UnityGameEditorDoorProp>();
					if (component != null)
					{
						component.m_InitiallyVisable = true;
					}
					propObject5.FindInChildren("Generated Content", includeInactive: true).SetActive(value: true);
				}
			}
			catch (Exception ex86)
			{
				Debug.LogError("An exception occurred while processing the RevealProp message\n" + ex86.Message + "\n" + ex86.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00115", "GUI_ERROR_MAIN_MENU_BUTTON", ex86.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex86.Message);
			}
			break;
		case CMessageData.MessageType.ElementsInfused:
			try
			{
				CElementsInfused_MessageData cElementsInfused_MessageData = (CElementsInfused_MessageData)message;
				bool flag2 = cElementsInfused_MessageData.m_InfuseAbility != null;
				CAbility cAbility3 = cElementsInfused_MessageData.m_ConsumeAbility;
				if (flag2)
				{
					cAbility3 = cElementsInfused_MessageData.m_InfuseAbility;
				}
				ClearAllActorEvents();
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				bool animationShouldPlay14 = false;
				CActor animatingActorToWaitFor14 = cElementsInfused_MessageData.m_ActorSpawningMessage;
				if (!cAbility3.IsModifierAbility)
				{
					ProcessActorAnimation(cAbility3, cElementsInfused_MessageData.m_ActorSpawningMessage, new List<string>
					{
						cElementsInfused_MessageData.AnimOverload,
						GetNonOverloadAnim(cAbility3)
					}, out animationShouldPlay14, out animatingActorToWaitFor14);
					if (animatingActorToWaitFor14 != null)
					{
						SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay14 ? 10000 : 400, animatingActorToWaitFor14);
					}
				}
				InfusionBoardUI.Instance.UpdateBoard(cElementsInfused_MessageData.m_InfusedElements);
				Singleton<UIUseItemsBar>.Instance.Hide();
				GameObject gameObject19 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				if (!animationShouldPlay14)
				{
					ActorEvents.GetActorEvents(gameObject19).ProgressChoreographer();
				}
			}
			catch (Exception ex85)
			{
				Debug.LogError("An exception occurred while processing the ElementsInfused message\n" + ex85.Message + "\n" + ex85.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00116", "GUI_ERROR_MAIN_MENU_BUTTON", ex85.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex85.Message);
			}
			break;
		case CMessageData.MessageType.WaitForProgressChoreographer:
			try
			{
				if (s_Choreographer.m_WaitState.m_State != ChoreographerStateType.WaitingForItemRefresh)
				{
					CWaitForProgressChoreographer_MessageData cWaitForProgressChoreographer_MessageData = message as CWaitForProgressChoreographer_MessageData;
					if (cWaitForProgressChoreographer_MessageData.ClearEvents)
					{
						ClearAllActorEvents();
					}
					if (s_Choreographer.m_WaitState.m_State == ChoreographerStateType.WaitingForCardSelection)
					{
						Singleton<AbilityEffectManager>.Instance.RemoveExtraTurnEffect(cWaitForProgressChoreographer_MessageData.WaitActor);
					}
					SetChoreographerState(ChoreographerStateType.WaitingForProgressChoreographer, cWaitForProgressChoreographer_MessageData.WaitTickFrame, cWaitForProgressChoreographer_MessageData.WaitActor);
				}
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
					if (cPhaseAction != null && cPhaseAction.CurrentPhaseAbility?.m_Ability != null && cPhaseAction.CurrentPhaseAbility.m_Ability is CAbilityConsumeItemCards cAbilityConsumeItemCards)
					{
						string arg8 = (cAbilityConsumeItemCards.SlotsToConsume.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation($"GUI_ITEM_SLOT_{cAbilityConsumeItemCards.SlotsToConsume[0]}"));
						Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_CONSUME_ITEMS_TIP"), arg8), string.Format(LocalizationManager.GetTranslation("GUI_CONSUME_ITEMS_TITLE"), arg8));
					}
				}
			}
			catch (Exception ex84)
			{
				Debug.LogError("An exception occurred while processing the WaitForProgressChoreographer message\n" + ex84.Message + "\n" + ex84.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00117", "GUI_ERROR_MAIN_MENU_BUTTON", ex84.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex84.Message);
			}
			break;
		case CMessageData.MessageType.ClearWaypointsAndTargets:
			try
			{
				while (Waypoint.s_Waypoints.Count > 0)
				{
					Waypoint.GetLastWaypoint.OnDelete();
				}
			}
			catch (Exception ex83)
			{
				Debug.LogError("An exception occurred while processing the ClearWaypointsAndTargets message\n" + ex83.Message + "\n" + ex83.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00118", "GUI_ERROR_MAIN_MENU_BUTTON", ex83.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex83.Message);
			}
			break;
		case CMessageData.MessageType.ToggledActionAugmentation:
			try
			{
				CToggledActionAugmentation_MessageData cToggledActionAugmentation_MessageData = (CToggledActionAugmentation_MessageData)message;
				CAbility ability4 = cToggledActionAugmentation_MessageData.m_CurrentAbilityAfterToggling;
				if (InputManager.GamePadInUse)
				{
					if (ability4 is CAbilityNull)
					{
						Singleton<UINavigation>.Instance.NavigationManager.DeselectCurrentSelectable();
						Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.Unfocus();
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.UseAction);
					}
					else
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.AbilityActions);
					}
				}
				readyButton.SetInteractable(ability4.EnoughTargetsSelected());
				m_UndoButton.SetInteractable(ability4.CanUndo && FirstAbility);
				m_SkipButton.SetInteractable(ability4.CanSkip);
				SetActiveSelectButton(readyButton.gameObject.activeInHierarchy ? (!readyButton.IsVisibility) : (!readyButton.gameObject.activeInHierarchy));
				while (Waypoint.s_Waypoints.Count > 0)
				{
					Waypoint.GetLastWaypoint.OnDelete();
				}
				Singleton<UIUseAugmentationsBar>.Instance.SetInteractionAvailableSlots(interactable: true);
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: true);
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: true);
				Singleton<UIUseItemsBar>.Instance.ShowUsableItems(ability4.TargetingActor, (CItem cItem2) => cItem2.YMLData.Data.CompareAbility != null && cItem2.YMLData.Data.CompareAbility.CompareAbility(ability4), (ability4.ActiveSingleTargetItems.Count == 0) ? "GUI_USE_ITEM_ATTACK_TIP" : "GUI_TOOLTIP_ITEM_SINGLE_TARGET_TIP", (ability4.ActiveSingleTargetItems.Count == 0) ? "GUI_USE_ITEM_TITLE" : "GUI_TOOLTIP_ITEM_SINGLE_TARGET_TITLE");
				if (FFSNetwork.IsOnline && !ability4.TargetingActor.IsUnderMyControl && ActionProcessor.HasSavedStateInSamePhase)
				{
					if (ActionProcessor.CurrentState.StateType == ActionProcessorStateType.Halted)
					{
						ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
					}
					else
					{
						ActionProcessor.ClearSavedState();
					}
				}
				if (ability4.AbilityType == CAbility.EAbilityType.Heal && message.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					UpdatePreviewHealForAbility(ability4);
				}
			}
			catch (Exception ex82)
			{
				Debug.LogError("An exception occurred while processing the ToggledActionAugmentation message\n" + ex82.Message + "\n" + ex82.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00232", "GUI_ERROR_MAIN_MENU_BUTTON", ex82.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex82.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsApplyingControlActor:
			try
			{
				CActorIsApplyingControlActor cActorIsApplyingControlActor = (CActorIsApplyingControlActor)message;
				ClearAllActorEvents();
				bool animationShouldPlay13 = false;
				CActor animatingActorToWaitFor13 = cActorIsApplyingControlActor.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorIsApplyingControlActor.m_ControlActorAbility, cActorIsApplyingControlActor.m_ActorSpawningMessage, new List<string>
				{
					cActorIsApplyingControlActor.AnimOverload,
					GetNonOverloadAnim(cActorIsApplyingControlActor.m_ControlActorAbility)
				}, out animationShouldPlay13, out animatingActorToWaitFor13);
				SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, 0, animatingActorToWaitFor13);
				DisableTileSelection(active: true);
				GameObject gameObject18 = FindClientActorGameObject(cActorIsApplyingControlActor.m_ActorSpawningMessage);
				if (!animationShouldPlay13)
				{
					ActorEvents.GetActorEvents(gameObject18).ProgressChoreographer();
				}
			}
			catch (Exception ex81)
			{
				Debug.LogError("An exception occurred while processing the ActorIsApplyingControlActor message\n" + ex81.Message + "\n" + ex81.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00120", "GUI_ERROR_MAIN_MENU_BUTTON", ex81.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex81.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsControlled:
			try
			{
				if (((CActorIsControlled_MessageData)message).m_ControlledActor.IsUnderMyControl)
				{
					Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: true);
					Singleton<UIUseAugmentationsBar>.Instance.SetInteractionAvailableSlots(interactable: true);
				}
			}
			catch (Exception ex80)
			{
				Debug.LogError("An exception occurred while processing the ActorIsControlled message\n" + ex80.Message + "\n" + ex80.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00121", "GUI_ERROR_MAIN_MENU_BUTTON", ex80.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex80.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsApplyingAddAugment:
			try
			{
				CActorIsApplyingAddAugment_MessageData cActorIsApplyingAddAugment_MessageData = (CActorIsApplyingAddAugment_MessageData)message;
				ClearAllActorEvents();
				bool animationShouldPlay12 = false;
				CActor animatingActorToWaitFor12 = cActorIsApplyingAddAugment_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorIsApplyingAddAugment_MessageData.m_AddAugmentAbility, cActorIsApplyingAddAugment_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cActorIsApplyingAddAugment_MessageData.AnimOverload,
					GetNonOverloadAnim(cActorIsApplyingAddAugment_MessageData.m_AddAugmentAbility)
				}, out animationShouldPlay12, out animatingActorToWaitFor12);
				if (animatingActorToWaitFor12 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, 0, animatingActorToWaitFor12);
				}
				GameObject gameObject17 = FindClientActorGameObject(cActorIsApplyingAddAugment_MessageData.m_ActorSpawningMessage);
				if (!animationShouldPlay12)
				{
					ActorEvents.GetActorEvents(gameObject17).ProgressChoreographer();
				}
			}
			catch (Exception ex79)
			{
				Debug.LogError("An exception occurred while processing the ActorIsApplyingAddAugment message\n" + ex79.Message + "\n" + ex79.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00122", "GUI_ERROR_MAIN_MENU_BUTTON", ex79.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex79.Message);
			}
			break;
		case CMessageData.MessageType.ApplyToActorAddAugment:
			try
			{
				_ = (CApplyToActorAddAugment_MessageData)message;
			}
			catch (Exception ex78)
			{
				Debug.LogError("An exception occurred while processing the ApplyToActorAddAugment message\n" + ex78.Message + "\n" + ex78.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00123", "GUI_ERROR_MAIN_MENU_BUTTON", ex78.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex78.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsApplyingAddSong:
			try
			{
				CActorIsApplyingAddSong_MessageData cActorIsApplyingAddSong_MessageData = (CActorIsApplyingAddSong_MessageData)message;
				ClearAllActorEvents();
				bool animationShouldPlay11 = false;
				CActor animatingActorToWaitFor11 = cActorIsApplyingAddSong_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorIsApplyingAddSong_MessageData.m_AddSongAbility, cActorIsApplyingAddSong_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cActorIsApplyingAddSong_MessageData.AnimOverload,
					GetNonOverloadAnim(cActorIsApplyingAddSong_MessageData.m_AddSongAbility)
				}, out animationShouldPlay11, out animatingActorToWaitFor11);
				if (animatingActorToWaitFor11 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, 0, animatingActorToWaitFor11);
				}
				GameObject gameObject16 = FindClientActorGameObject(cActorIsApplyingAddSong_MessageData.m_ActorSpawningMessage);
				if (!animationShouldPlay11)
				{
					ActorEvents.GetActorEvents(gameObject16).ProgressChoreographer();
				}
			}
			catch (Exception ex77)
			{
				Debug.LogError("An exception occurred while processing the ActorIsApplyingAddSong message\n" + ex77.Message + "\n" + ex77.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00124", "GUI_ERROR_MAIN_MENU_BUTTON", ex77.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex77.Message);
			}
			break;
		case CMessageData.MessageType.ApplyToActorAddSong:
			try
			{
				_ = (CApplyToActorAddSong_MessageData)message;
			}
			catch (Exception ex76)
			{
				Debug.LogError("An exception occurred while processing the ApplyToActorAddSong message\n" + ex76.Message + "\n" + ex76.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00125", "GUI_ERROR_MAIN_MENU_BUTTON", ex76.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex76.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsApplyingOverrideAugmentAttackType:
			try
			{
				CActorIsApplyingOverrideAugmentAttackType_MessageData cActorIsApplyingOverrideAugmentAttackType_MessageData = (CActorIsApplyingOverrideAugmentAttackType_MessageData)message;
				ClearAllActorEvents();
				bool animationShouldPlay10 = false;
				CActor animatingActorToWaitFor10 = cActorIsApplyingOverrideAugmentAttackType_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorIsApplyingOverrideAugmentAttackType_MessageData.m_OverrideAugmentAttackTypeAbility, cActorIsApplyingOverrideAugmentAttackType_MessageData.m_ActorSpawningMessage, new List<string>
				{
					cActorIsApplyingOverrideAugmentAttackType_MessageData.AnimOverload,
					GetNonOverloadAnim(cActorIsApplyingOverrideAugmentAttackType_MessageData.m_OverrideAugmentAttackTypeAbility)
				}, out animationShouldPlay10, out animatingActorToWaitFor10);
				if (animatingActorToWaitFor10 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, 0, animatingActorToWaitFor10);
				}
				GameObject gameObject15 = FindClientActorGameObject(cActorIsApplyingOverrideAugmentAttackType_MessageData.m_ActorSpawningMessage);
				if (!animationShouldPlay10)
				{
					ActorEvents.GetActorEvents(gameObject15).ProgressChoreographer();
				}
			}
			catch (Exception ex75)
			{
				Debug.LogError("An exception occurred while processing the ActorIsApplyingOverrideAugmentAttackType message\n" + ex75.Message + "\n" + ex75.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00126", "GUI_ERROR_MAIN_MENU_BUTTON", ex75.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex75.Message);
			}
			break;
		case CMessageData.MessageType.ApplyToActorOverrideAugmentAttackType:
			try
			{
				_ = (CApplyToActorOverrideAugmentAttackType_MessageData)message;
			}
			catch (Exception ex74)
			{
				Debug.LogError("An exception occurred while processing the ApplyToActorOverrideAugmentAttackType message\n" + ex74.Message + "\n" + ex74.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00127", "GUI_ERROR_MAIN_MENU_BUTTON", ex74.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex74.Message);
			}
			break;
		case CMessageData.MessageType.UpdateCurrentActor:
			try
			{
				m_CurrentActor = message.m_ActorSpawningMessage;
				if (FFSNetwork.IsOnline && m_CurrentActor is CPlayerActor)
				{
					InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
					{
						f.Avatar.RefreshActiveInteractable();
					});
				}
				if (CurrentPlayerActor == null)
				{
					UIManager.Instance.BattleGoalContainer.Hide();
				}
				else
				{
					UIManager.Instance.BattleGoalContainer.Show(CurrentPlayerActor);
				}
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(m_CurrentActor);
			}
			catch (Exception ex73)
			{
				Debug.LogError("An exception occurred while processing the UpdateCurrentActor message\n" + ex73.Message + "\n" + ex73.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00128", "GUI_ERROR_MAIN_MENU_BUTTON", ex73.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex73.Message);
			}
			break;
		case CMessageData.MessageType.WaitForSingleTargetActiveBonus:
		{
			CWaitForSingleTargetActiveBonus_MessageData cWaitForSingleTargetActiveBonus_MessageData = (CWaitForSingleTargetActiveBonus_MessageData)message;
			InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_SINGLE_TARGET"), LocalizationManager.GetTranslation(cWaitForSingleTargetActiveBonus_MessageData.m_ActiveBonus.BaseCard.Name));
			break;
		}
		case CMessageData.MessageType.ShowSingleTargetActiveBonus:
		{
			CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = (CShowSingleTargetActiveBonus_MessageData)message;
			if (cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus)
			{
				if (cShowSingleTargetActiveBonus_MessageData.m_Ability.CanApplyActiveBonusTogglesTo())
				{
					Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(cShowSingleTargetActiveBonus_MessageData.m_Ability.TargetingActor, cShowSingleTargetActiveBonus_MessageData.m_Ability.AbilityType, CActiveBonus.EActiveBonusBehaviourType.None, null, abilityAlreadyStarted: false, cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus, cShowSingleTargetActiveBonus_MessageData.m_Ability);
					Singleton<UIActiveBonusBar>.Instance.RefreshSingleTargetActiveBonusesFromSRLState();
				}
			}
			else
			{
				Singleton<UIActiveBonusBar>.Instance.HideSingleTargetActiveBonuses();
			}
			if (cShowSingleTargetActiveBonus_MessageData.m_Ability is CAbilitySummon cAbilitySummon3)
			{
				string translation7 = LocalizationManager.GetTranslation(cAbilitySummon3.SelectedLocKey);
				Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON_SELECT_HEX"), translation7), string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON"), translation7));
			}
			else if (cShowSingleTargetActiveBonus_MessageData.m_Ability.AbilityType != CAbility.EAbilityType.Swap)
			{
				ShowAbilityHelpBoxTooltip(cShowSingleTargetActiveBonus_MessageData.m_Ability);
			}
			break;
		}
		case CMessageData.MessageType.OnItemCallback:
			try
			{
				COnItemCallback_MessageData messageData6 = (COnItemCallback_MessageData)message;
				messageData6.m_Callback(messageData6.m_Item, EventArgs.Empty);
				Singleton<UIUseItemsBar>.Instance.RefreshTooltip();
				Singleton<TakeDamagePanel>.Instance.RefreshDamageTooltip();
				if (messageData6.m_CallbackType == ESESubTypeItem.ItemRefreshed)
				{
					if (messageData6.m_ActorSpawningMessage != null)
					{
						CardsHandUI cardsHandUI = CardsHandManager.Instance.cardHandsUI.SingleOrDefault((CardsHandUI x) => x.PlayerActor.CharacterClass.ID == messageData6.m_ActorSpawningMessage.Class.ID);
						if (cardsHandUI != null)
						{
							cardsHandUI.ResetItemElements(messageData6.m_Item);
						}
					}
					else
					{
						CardsHandManager.Instance.CurrentHand.ResetItemElements(messageData6.m_Item);
					}
				}
				CPhase phase = PhaseManager.Phase;
				if (phase.Type == CPhase.PhaseType.Action)
				{
					CAbility cAbility2 = ((CPhaseAction)phase).CurrentPhaseAbility?.m_Ability;
					if (messageData6.m_Item.SlotState == CItem.EItemSlotState.Selected && cAbility2 != null && ((cAbility2.AreaEffect != null && WorldspaceStarHexDisplay.Instance.IsAOELocked()) || (cAbility2.ActorsToTarget != null && cAbility2.ActorsToTarget.Count > 1)) && messageData6.m_Item.SingleTarget == null && cAbility2.ActiveSingleTargetItems.Contains(messageData6.m_Item))
					{
						List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
						if (messageData6.m_Item.YMLData.Data.Overrides != null)
						{
							foreach (CAbilityOverride @override in messageData6.m_Item.YMLData.Data.Overrides)
							{
								if (@override != null && @override.NegativeConditions != null)
								{
									list.AddRange(@override.NegativeConditions);
								}
							}
						}
						if (messageData6.m_Item.YMLData.Data.Abilities != null)
						{
							foreach (CAbility ability9 in messageData6.m_Item.YMLData.Data.Abilities)
							{
								if (ability9 != null && ability9.NegativeConditions != null)
								{
									list.AddRange(ability9.NegativeConditions.Keys);
								}
							}
						}
						if (list.Count > 0)
						{
							StringBuilder stringBuilder3 = new StringBuilder();
							for (int num6 = 0; num6 < list.Count; num6++)
							{
								if (num6 > 0)
								{
									stringBuilder3.Append((num6 == list.Count - 1) ? " and " : ", ");
								}
								stringBuilder3.AppendFormat("{0} <sprite name={1}>", list[num6].ToString().ToUpper(), list[num6]);
							}
							InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SINGLE_TARGET") + " " + LocalizationManager.GetTranslation("to") + " {0}", stringBuilder3), LocalizationManager.GetTranslation(messageData6.m_Item.Name));
						}
						else
						{
							InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_SINGLE_TARGET"), LocalizationManager.GetTranslation(messageData6.m_Item.Name));
						}
						readyButton.SetInteractable(interactable: false);
					}
					if (cAbility2 != null)
					{
						if (cAbility2.AbilityType == CAbility.EAbilityType.LoseCards && m_CurrentActor is CPlayerActor)
						{
							int num7 = ((((CPlayerActor)m_CurrentActor).CharacterClass.HandAbilityCards.Count < cAbility2.Strength) ? ((CPlayerActor)m_CurrentActor).CharacterClass.HandAbilityCards.Count : cAbility2.Strength);
							InitiativeTrack.Instance.helpBox.ShowTranslated((num7 > 1) ? string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_LOSE_CARDS"), cAbility2.Strength) : LocalizationManager.GetTranslation("GUI_TOOLTIP_SELECT_LOSE_CARD"));
						}
						else if (cAbility2.AbilityType == CAbility.EAbilityType.DiscardCards && m_CurrentActor is CPlayerActor)
						{
							int num8 = ((((CPlayerActor)m_CurrentActor).CharacterClass.HandAbilityCards.Count < cAbility2.Strength) ? ((CPlayerActor)m_CurrentActor).CharacterClass.HandAbilityCards.Count : cAbility2.Strength);
							InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation((num8 > 1) ? "GUI_TOOLTIP_SELECT_DISCARD_CARDS" : "GUI_TOOLTIP_SELECT_DISCARD_CARD"), cAbility2.Strength), string.Format(LocalizationManager.GetTranslation((num8 > 1) ? "GUI_TOOLTIP_SELECT_DISCARD_CARDS_TITLE" : "GUI_TOOLTIP_SELECT_DISCARD_CARD_TITLE"), cAbility2.Strength));
						}
					}
				}
				else if (phase.Type == CPhase.PhaseType.ActionSelection && m_CurrentActor is CPlayerActor cPlayerActor2)
				{
					if (cPlayerActor2.CharacterClass.LongRest)
					{
						InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_LONG_REST", "GUI_LONG_REST");
					}
				}
				else if (phase.Type == CPhase.PhaseType.EndTurn)
				{
					CardsHandManager.Instance.EnableAllCardsCombo(value: false);
					CardsHandManager.Instance.DisableInput();
				}
			}
			catch (Exception ex65)
			{
				Debug.LogError("An exception occurred while processing the OnItemCallback message\n" + ex65.Message + "\n" + ex65.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00129", "GUI_ERROR_MAIN_MENU_BUTTON", ex65.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex65.Message);
			}
			break;
		case CMessageData.MessageType.OnItemAbilityUnstacked:
			try
			{
				COnItemAbilityUnstacked_MessageData cOnItemAbilityUnstacked_MessageData = (COnItemAbilityUnstacked_MessageData)message;
				if (cOnItemAbilityUnstacked_MessageData.m_ItemAbility != null)
				{
					foreach (CActor item42 in cOnItemAbilityUnstacked_MessageData.m_ItemAbility.ValidActorsInRange)
					{
						ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(item42)).m_WorldspacePanelUI.PreviewEffects(cOnItemAbilityUnstacked_MessageData.m_ItemAbility, item42, removePreview: true);
					}
				}
				if (PhaseManager.PhaseType != CPhase.PhaseType.Action)
				{
					break;
				}
				CAbility cAbility = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility?.m_Ability;
				if (cAbility == null)
				{
					break;
				}
				foreach (CActor item43 in cAbility.ValidActorsInRange)
				{
					ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(item43)).m_WorldspacePanelUI.PreviewEffects(cAbility, item43);
				}
			}
			catch (Exception ex64)
			{
				Debug.LogError("An exception occurred while processing the OnItemAbilityUnstacked message\n" + ex64.Message + "\n" + ex64.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00130", "GUI_ERROR_MAIN_MENU_BUTTON", ex64.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex64.Message);
			}
			break;
		case CMessageData.MessageType.OnAbilityUpdated:
			try
			{
				((COnAbilityUpdated_MessageData)message).m_ActiveBonus.BespokeBehaviour.OnBehaviourTriggered();
			}
			catch (Exception ex63)
			{
				Debug.LogError("An exception occurred while processing the OnAbilityUpdated message\n" + ex63.Message + "\n" + ex63.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00131", "GUI_ERROR_MAIN_MENU_BUTTON", ex63.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex63.Message);
			}
			break;
		case CMessageData.MessageType.ClearAllActorEvents:
			try
			{
				ClearAllActorEvents();
			}
			catch (Exception ex62)
			{
				Debug.LogError("An exception occurred while processing the ClearAllActorEvents message\n" + ex62.Message + "\n" + ex62.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00132", "GUI_ERROR_MAIN_MENU_BUTTON", ex62.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex62.Message);
			}
			break;
		case CMessageData.MessageType.ActiveBonusTrackerIncremented:
			try
			{
				CActiveBonusTrackerIncremented_MessageData cActiveBonusTrackerIncremented_MessageData = (CActiveBonusTrackerIncremented_MessageData)message;
				InitiativeTrack.Instance?.OnActiveBonusTriggered(cActiveBonusTrackerIncremented_MessageData.m_Actor, cActiveBonusTrackerIncremented_MessageData.m_ActiveBonus);
			}
			catch (Exception ex61)
			{
				Debug.LogError("An exception occurred while processing the ActiveBonusTrackerIncremented message\n" + ex61.Message + "\n" + ex61.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00133", "GUI_ERROR_MAIN_MENU_BUTTON", ex61.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex61.Message);
			}
			break;
		case CMessageData.MessageType.ActiveBonusAugmentAdded:
			try
			{
				CActiveBonusAugmentAdded_MessageData cActiveBonusAugmentAdded_MessageData = (CActiveBonusAugmentAdded_MessageData)message;
				if (!m_AbilitiesUsedThisTurn.Contains(cActiveBonusAugmentAdded_MessageData.m_ActiveBonus.BaseCard.Name))
				{
					m_AbilitiesUsedThisTurn.Add(cActiveBonusAugmentAdded_MessageData.m_ActiveBonus.BaseCard.Name);
				}
			}
			catch (Exception ex60)
			{
				Debug.LogError("An exception occurred while processing the ActiveBonusAugmentAdded message\n" + ex60.Message + "\n" + ex60.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00134", "GUI_ERROR_MAIN_MENU_BUTTON", ex60.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex60.Message);
			}
			break;
		case CMessageData.MessageType.ActiveBonusAugmentSlotChoice:
			try
			{
				CActiveBonusAugmentSlotChoice_MessageData cActiveBonusAugmentSlotChoice_MessageData = (CActiveBonusAugmentSlotChoice_MessageData)message;
				cActiveBonusAugmentSlotChoice_MessageData.m_Actor.ReplaceAugment(cActiveBonusAugmentSlotChoice_MessageData.m_Ability, cActiveBonusAugmentSlotChoice_MessageData.m_Actor.Augments[0], cActiveBonusAugmentSlotChoice_MessageData.m_Actor);
				ScenarioRuleClient.StepComplete();
			}
			catch (Exception ex59)
			{
				Debug.LogError("An exception occurred while processing the ActiveBonusTrackerIncremented message\n" + ex59.Message + "\n" + ex59.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00135", "GUI_ERROR_MAIN_MENU_BUTTON", ex59.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex59.Message);
			}
			break;
		case CMessageData.MessageType.ActiveBonusSongAdded:
			try
			{
				CActiveBonusSongAdded_MessageData cActiveBonusSongAdded_MessageData = (CActiveBonusSongAdded_MessageData)message;
				Debug.Log("Song Added from " + cActiveBonusSongAdded_MessageData.m_ActiveBonus.BaseCard.Name);
			}
			catch (Exception ex58)
			{
				Debug.LogError("An exception occurred while processing the ActiveBonusSongAdded message\n" + ex58.Message + "\n" + ex58.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00136", "GUI_ERROR_MAIN_MENU_BUTTON", ex58.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex58.Message);
			}
			break;
		case CMessageData.MessageType.ActiveBonusSongSlotChoice:
			try
			{
				CActiveBonusSongSlotChoice_MessageData cActiveBonusSongSlotChoice_MessageData = (CActiveBonusSongSlotChoice_MessageData)message;
				cActiveBonusSongSlotChoice_MessageData.m_Actor.ReplaceSong(cActiveBonusSongSlotChoice_MessageData.m_Ability, cActiveBonusSongSlotChoice_MessageData.m_Actor.Songs[0], cActiveBonusSongSlotChoice_MessageData.m_Actor);
				ScenarioRuleClient.StepComplete();
			}
			catch (Exception ex57)
			{
				Debug.LogError("An exception occurred while processing the ActiveBonusSongSlotChoice message\n" + ex57.Message + "\n" + ex57.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00137", "GUI_ERROR_MAIN_MENU_BUTTON", ex57.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex57.Message);
			}
			break;
		case CMessageData.MessageType.ActiveBonusDoomSlotChoice:
			try
			{
				CActiveBonusDoomSlotChoice_MessageData messageData5 = (CActiveBonusDoomSlotChoice_MessageData)message;
				int doomSlots = messageData5.m_ActorSpawningMessage.DoomSlots;
				Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_CHOOSE_DOOM_DISCARD"), doomSlots), $"<color=#{UIInfoTools.Instance.GetCharacterHexColor(ECharacter.Doomstalker)}>{((messageData5.m_ActorSpawningMessage.CachedAddDoomSlotActiveBonuses.Count == 0) ? null : LocalizationManager.GetTranslation(messageData5.m_ActorSpawningMessage.CachedAddDoomSlotActiveBonuses[0].BaseCard.Name))}");
				Action onConfirmed = null;
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONNA, LocalizationManager.GetTranslation("GUI_END_SELECTION"));
				readyButton.AlternativeAction(delegate
				{
					m_UndoButton.ClearOnClickOverriders();
					m_SkipButton.ClearSkipAction();
					onConfirmed?.Invoke();
					Singleton<UIAbilityCardPicker>.Instance.Hide();
					ScenarioRuleClient.StepComplete();
				});
				readyButton.SetInteractable(interactable: false);
				SetActiveSelectButton(activate: true);
				Action undoAction2 = null;
				undoAction2 = delegate
				{
					Singleton<UIAbilityCardPicker>.Instance.ClearSelection();
					m_UndoButton.SetOnClickOverrider(undoAction2, UndoButton.EButtonState.EUNDOBUTTONUNDO);
					m_UndoButton.SetInteractable(active: false);
				};
				m_UndoButton.Toggle(active: true);
				m_UndoButton.SetInteractable(active: false);
				m_UndoButton.SetOnClickOverrider(undoAction2, UndoButton.EButtonState.EUNDOBUTTONUNDO);
				m_UndoButton.SetInteractable(active: false);
				SkipButton skipButton = m_SkipButton;
				Action onSkipAction = Singleton<UIAbilityCardPicker>.Instance.Hide;
				skipButton.Toggle(active: true, null, hideOnClick: true, null, onSkipAction);
				List<DoomPickerChoice> choices = messageData5.m_ActorSpawningMessage.Dooms.Select((CDoom it) => new DoomPickerChoice(it, (CAbilityCard)messageData5.m_ActorSpawningMessage.FindCardWithAbility(it.DoomAbilities[0]))).ToList();
				choices.Add(new DoomPickerChoice(messageData5.m_NewDoom, (CAbilityCard)messageData5.m_ActorSpawningMessage.FindCardWithAbility(messageData5.m_NewDoom.DoomAbilities[0])));
				Singleton<UIAbilityCardPicker>.Instance.Show(messageData5.m_ActorSpawningMessage, choices, doomSlots, delegate(List<IAbilityCardOption> selectedDooms)
				{
					IAbilityCardOption replacedDoom = choices.Except(selectedDooms).SingleOrDefault();
					if (replacedDoom == null)
					{
						onConfirmed = null;
					}
					else
					{
						onConfirmed = delegate
						{
							messageData5.m_ActorSpawningMessage.ReplaceDoom(messageData5.m_NewDoom, ((DoomPickerChoice)replacedDoom).Doom, messageData5.m_DoomTargetActor);
						};
					}
					m_UndoButton.SetInteractable(active: true);
					readyButton.SetInteractable(interactable: true);
					SetActiveSelectButton(activate: false);
				}, delegate
				{
					m_UndoButton.SetInteractable(active: false);
					readyButton.SetInteractable(interactable: false);
					SetActiveSelectButton(activate: true);
				}, delegate
				{
					m_UndoButton.SetInteractable(active: true);
				});
				if (FFSNetwork.IsOnline)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.DoomTransfering);
				}
			}
			catch (Exception ex56)
			{
				Debug.LogError("An exception occurred while processing the ActiveBonusDoomSlotChoice message\n" + ex56.Message + "\n" + ex56.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00233", "GUI_ERROR_MAIN_MENU_BUTTON", ex56.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex56.Message);
			}
			break;
		case CMessageData.MessageType.TransferDoomChoice:
			try
			{
				CTransferDoomChoice_MessageData messageData4 = (CTransferDoomChoice_MessageData)message;
				if (messageData4.m_TransferDoomAbility.Strength >= messageData4.m_ActorSpawningMessage.Dooms.Count)
				{
					for (int num5 = 0; num5 < messageData4.m_ActorSpawningMessage.Dooms.Count; num5++)
					{
						messageData4.m_ActorSpawningMessage.TransferDoom(messageData4.m_ActorSpawningMessage.Dooms[num5], messageData4.m_NewDoomTargetActor);
					}
					ScenarioRuleClient.StepComplete();
					break;
				}
				Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_CHOOSE_DOOM_TRANSFER"), $"<color=#{UIInfoTools.Instance.GetCharacterHexColor(ECharacter.Doomstalker)}>{LocalizationManager.GetTranslation(messageData4.m_TransferDoomAbility.AbilityBaseCard.Name)}");
				List<IAbilityCardOption> selectedChoices = null;
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONNA, LocalizationManager.GetTranslation("GUI_END_SELECTION"));
				readyButton.AlternativeAction(delegate
				{
					m_UndoButton.ClearOnClickOverriders();
					m_SkipButton.ClearSkipAction();
					for (int i = 0; i < selectedChoices.Count; i++)
					{
						messageData4.m_ActorSpawningMessage.ReplaceDoom(((DoomPickerChoice)selectedChoices[i]).Doom, ((DoomPickerChoice)selectedChoices[i]).Doom, messageData4.m_NewDoomTargetActor);
					}
					Singleton<UIAbilityCardPicker>.Instance.Hide();
					ScenarioRuleClient.StepComplete();
				});
				readyButton.SetInteractable(interactable: false);
				m_UndoButton.Toggle(active: true);
				m_UndoButton.SetInteractable(active: false);
				Action undoAction = null;
				undoAction = delegate
				{
					Singleton<UIAbilityCardPicker>.Instance.ClearSelection();
					m_UndoButton.SetOnClickOverrider(undoAction, UndoButton.EButtonState.EUNDOBUTTONUNDO);
					m_UndoButton.SetInteractable(active: false);
				};
				m_UndoButton.SetOnClickOverrider(undoAction, UndoButton.EButtonState.EUNDOBUTTONUNDO);
				m_UndoButton.SetInteractable(active: false);
				m_SkipButton.Toggle(active: true, null, hideOnClick: true, null, delegate
				{
					Singleton<UIAbilityCardPicker>.Instance.Hide();
					for (int i = 0; i < Mathf.Min(messageData4.m_ActorSpawningMessage.Dooms.Count, messageData4.m_TransferDoomAbility.Strength); i++)
					{
						messageData4.m_ActorSpawningMessage.ReplaceDoom(messageData4.m_ActorSpawningMessage.Dooms[i], messageData4.m_ActorSpawningMessage.Dooms[i], messageData4.m_NewDoomTargetActor);
					}
				});
				List<DoomPickerChoice> options = messageData4.m_ActorSpawningMessage.Dooms.Select((CDoom it) => new DoomPickerChoice(it, (CAbilityCard)messageData4.m_ActorSpawningMessage.FindCardWithAbility(it.DoomAbilities[0]))).ToList();
				Singleton<UIAbilityCardPicker>.Instance.Show(messageData4.m_ActorSpawningMessage, options, messageData4.m_TransferDoomAbility.Strength, delegate(List<IAbilityCardOption> selectedDooms)
				{
					selectedChoices = selectedDooms;
					m_UndoButton.SetInteractable(active: true);
					readyButton.SetInteractable(interactable: true);
				}, delegate
				{
					selectedChoices = null;
					m_UndoButton.SetInteractable(active: false);
					readyButton.SetInteractable(interactable: false);
				});
				if (FFSNetwork.IsOnline)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.DoomTransfering);
				}
			}
			catch (Exception ex55)
			{
				Debug.LogError("An exception occurred while processing the TransferDoomChoice message\n" + ex55.Message + "\n" + ex55.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00234", "GUI_ERROR_MAIN_MENU_BUTTON", ex55.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex55.Message);
			}
			break;
		case CMessageData.MessageType.RefreshWorldSpaceStarHexDisplay:
			try
			{
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
				WorldspaceStarHexDisplay.Instance.LockView = false;
				WorldspaceStarHexDisplay.Instance.ClearCachedAbilityTiles();
				readyButton.ClearAlternativeAction();
				foreach (GameObject clientActorObject4 in ClientActorObjects)
				{
					if (clientActorObject4 != null)
					{
						ActorBehaviour.GetActorBehaviour(clientActorObject4).m_WorldspacePanelUI.ResetPreview();
					}
				}
			}
			catch (Exception ex54)
			{
				Debug.LogError("An exception occurred while processing the RefreshWorldSpaceStarHexDisplay message\n" + ex54.Message + "\n" + ex54.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00138", "GUI_ERROR_MAIN_MENU_BUTTON", ex54.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex54.Message);
			}
			break;
		case CMessageData.MessageType.RefreshActiveBonusUI:
			try
			{
				CRefreshActiveBonusUI_MessageData cRefreshActiveBonusUI_MessageData = (CRefreshActiveBonusUI_MessageData)message;
				InitiativeTrack.Instance.RefreshActorUI(cRefreshActiveBonusUI_MessageData.m_Actor);
			}
			catch (Exception ex53)
			{
				Debug.LogError("An exception occurred while processing the RefreshActiveBonusUI message\n" + ex53.Message + "\n" + ex53.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00139", "GUI_ERROR_MAIN_MENU_BUTTON", ex53.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex53.Message);
			}
			break;
		case CMessageData.MessageType.UnlockWaypointsForClearing:
			try
			{
				_ = (CUnlockWaypointsForClearing)message;
				Waypoint.s_LockWaypoints = false;
			}
			catch (Exception ex52)
			{
				Debug.LogError("An exception occurred while processing the UnlockWaypointsForClearing message\n" + ex52.Message + "\n" + ex52.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00195", "GUI_ERROR_MAIN_MENU_BUTTON", ex52.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex52.Message);
			}
			break;
		case CMessageData.MessageType.LockWaypointsFromClearing:
			try
			{
				_ = (CLockWaypointsFromClearing)message;
				Waypoint.s_LockWaypoints = true;
			}
			catch (Exception ex51)
			{
				Debug.LogError("An exception occurred while processing the LockWaypointsFromClearing message\n" + ex51.Message + "\n" + ex51.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00140", "GUI_ERROR_MAIN_MENU_BUTTON", ex51.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex51.Message);
			}
			break;
		case CMessageData.MessageType.CacheWaypoints:
			try
			{
				_ = (CCacheWaypoints)message;
				Waypoint.CacheWaypoints();
			}
			catch (Exception ex50)
			{
				Debug.LogError("An exception occurred while processing the CacheWaypoints message\n" + ex50.Message + "\n" + ex50.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00141", "GUI_ERROR_MAIN_MENU_BUTTON", ex50.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex50.Message);
			}
			break;
		case CMessageData.MessageType.RestoreWaypoints:
			try
			{
				_ = (CRestoreWaypoints)message;
				Waypoint.RestoreWaypoints();
			}
			catch (Exception ex49)
			{
				Debug.LogError("An exception occurred while processing the RestoreWaypoints message\n" + ex49.Message + "\n" + ex49.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00142", "GUI_ERROR_MAIN_MENU_BUTTON", ex49.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex49.Message);
			}
			break;
		case CMessageData.MessageType.ActorEarnedXP:
			try
			{
				CActorEarnedXP_MessageData messageData3 = (CActorEarnedXP_MessageData)message;
				if (messageData3.m_ActorSpawningMessage.Type != CActor.EType.Player)
				{
					break;
				}
				if (ScenarioManager.Scenario.HasActor(messageData3.m_ActorSpawningMessage))
				{
					ActorBehaviour actorBehaviour4 = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(message.m_ActorSpawningMessage));
					if (actorBehaviour4 != null)
					{
						actorBehaviour4.m_WorldspacePanelUI.OnEarnedXP(messageData3.m_xpAmount);
					}
				}
				CActor actorSpawningMessage2 = message.m_ActorSpawningMessage;
				string arg7 = LocalizationManager.GetTranslation(actorSpawningMessage2.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage2);
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_XP"), arg7, messageData3.m_xpAmount), CombatLogFilter.ABILITIES);
				int num = 0;
				int num2 = 0;
				try
				{
					if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
					{
						int? num3 = SaveData.Instance.Global.CampaignData?.AdventureMapState?.MapParty.SelectedCharacters.ToList().SingleOrDefault((CMapCharacter x) => x.CharacterID == messageData3.m_ActorSpawningMessage.Class.ID)?.EXP;
						if (num3.HasValue)
						{
							num2 = num3.Value;
						}
					}
					else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
					{
						int? num4 = SaveData.Instance.Global.AdventureData?.AdventureMapState?.MapParty.SelectedCharacters.ToList().SingleOrDefault((CMapCharacter x) => x.CharacterID == messageData3.m_ActorSpawningMessage.Class.ID)?.EXP;
						if (num4.HasValue)
						{
							num2 = num4.Value;
						}
					}
					num2 += messageData3.m_scenarioXP;
					num = num2 - messageData3.m_xpAmount;
				}
				catch (Exception ex47)
				{
					Debug.LogError("Exception calculating XP.\n" + ex47.Message + "\n" + ex47.StackTrace);
				}
				SimpleLog.AddToSimpleLog($"{arg7} XP Change: {num} -> {num2}");
			}
			catch (Exception ex48)
			{
				Debug.LogError("An exception occurred while processing the ActorEarnedXP message\n" + ex48.Message + "\n" + ex48.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00143", "GUI_ERROR_MAIN_MENU_BUTTON", ex48.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex48.Message);
			}
			break;
		case CMessageData.MessageType.EndAction:
			try
			{
				CEndAction_MessageData cEndAction_MessageData = (CEndAction_MessageData)message;
				if (cEndAction_MessageData.m_ActorSpawningMessage.Type == CActor.EType.Player)
				{
					if (!cEndAction_MessageData.m_ActionHappened)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ACTION_SKIPPED"), LocalizationManager.GetTranslation(message.m_ActorSpawningMessage.ActorLocKey()), $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{LocalizationManager.GetTranslation(cEndAction_MessageData.m_ActionName)}</color></b></font>"), CombatLogFilter.ABILITIES);
					}
					Singleton<UIActiveBonusBar>.Instance.Hide();
				}
			}
			catch (Exception ex46)
			{
				Debug.LogError("An exception occurred while processing the EndAction message\n" + ex46.Message + "\n" + ex46.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00143", "GUI_ERROR_MAIN_MENU_BUTTON", ex46.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex46.Message);
			}
			break;
		case CMessageData.MessageType.SpawnerSpawningUnit:
			try
			{
				CSpawnerSpawningUnit_MessageData cSpawnerSpawningUnit_MessageData = (CSpawnerSpawningUnit_MessageData)message;
				CEnemyActor spawnEnemy = cSpawnerSpawningUnit_MessageData.m_SpawnEnemy;
				string text2 = LocalizationManager.GetTranslation(spawnEnemy.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(spawnEnemy);
				GameObject gameObject8 = null;
				if (!FindClientEnemy(spawnEnemy))
				{
					CClientTile tile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cSpawnerSpawningUnit_MessageData.m_SpawnTile.m_ArrayIndex.X, cSpawnerSpawningUnit_MessageData.m_SpawnTile.m_ArrayIndex.Y];
					gameObject8 = s_Choreographer.CreateCharacterActor(tile, spawnEnemy);
					if (!(gameObject8 != null))
					{
						Debug.LogError("Unable to create character actor object");
						throw new Exception();
					}
					s_Choreographer.m_ClientEnemies.Add(gameObject8);
					s_Choreographer.SetCharacterPositions();
					if (ActorBehaviour.GetActorBehaviour(gameObject8) == null)
					{
						Debug.LogErrorFormat("Unable to find actor behaviour for spawned character actor object {0}", text2);
					}
				}
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_SPAWNER_SPAWNED"), text2), CombatLogFilter.ABILITIES);
			}
			catch (Exception ex45)
			{
				Debug.LogError("An exception occurred while processing the SpawnerSpawningUnit message\n" + ex45.Message + "\n" + ex45.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00179", "GUI_ERROR_MAIN_MENU_BUTTON", ex45.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex45.Message);
			}
			break;
		case CMessageData.MessageType.ShowAugmentationBar:
			try
			{
				CShowAugmentationBar_MessageData cShowAugmentationBar_MessageData = (CShowAugmentationBar_MessageData)message;
				ShowAugmentationBar = true;
				if (cShowAugmentationBar_MessageData.m_Ability.AbilityType != CAbility.EAbilityType.Attack || (cShowAugmentationBar_MessageData.m_Ability.TargetingActor != null && cShowAugmentationBar_MessageData.m_Ability.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm)))
				{
					CardsActionControlller.s_Instance.RefreshConsumeBar(WaitingForConfirm);
					WaitingForConfirm = false;
				}
			}
			catch (Exception ex44)
			{
				Debug.LogError("An exception occurred while processing the ShowAugmentationBar message\n" + ex44.Message + "\n" + ex44.StackTrace);
			}
			break;
		case CMessageData.MessageType.ShowForgoActiveBonus:
			try
			{
				CShowForgoActiveBonusBar_MessageData messageData2 = (CShowForgoActiveBonusBar_MessageData)message;
				m_CurrentActor = messageData2.m_ActorSpawningMessage;
				if (FFSNetwork.IsOnline && m_CurrentActor is CPlayerActor)
				{
					InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
					{
						f.Avatar.RefreshActiveInteractable();
					});
				}
				if (CurrentPlayerActor == null)
				{
					UIManager.Instance.BattleGoalContainer.Hide();
				}
				else
				{
					UIManager.Instance.BattleGoalContainer.Show(CurrentPlayerActor);
				}
				Singleton<HelpBox>.Instance.Show("GUI_TOOLTIP_FORGO_ACTIONS_FOR_COMPANION", messageData2.m_Ability.ActiveBonusYML.CardName);
				Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(messageData2.m_ActorSpawningMessage, CAbility.EAbilityType.ForgoActionsForCompanion, CActiveBonus.EActiveBonusBehaviourType.None, delegate(CActiveBonus bonus, bool selected)
				{
					bool flag19 = s_Choreographer.ActorOrHisSummonerIsUnderMyControl(messageData2.m_ActorSpawningMessage);
					if (FFSNetwork.IsOnline && flag19)
					{
						UIUseActiveBonus slotForActiveBonus = Singleton<UIActiveBonusBar>.Instance.GetSlotForActiveBonus(bonus);
						List<ElementInfusionBoardManager.EElement> list5 = (selected ? slotForActiveBonus.GetSelectedConsumes() : null);
						ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
						int iD = messageData2.m_ActorSpawningMessage.ID;
						IProtocolToken supplementaryDataToken2 = new ActiveBonusToken(bonus, selected, list5.IsNullOrEmpty() ? ((ElementInfusionBoardManager.EElement?)null) : new ElementInfusionBoardManager.EElement?(list5[0]), slotForActiveBonus.SelectedOptions);
						Synchronizer.SendGameAction(GameActionType.ClickActiveBonusSlot, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken2);
					}
					readyButton.AlternativeAction(delegate
					{
						readyButton.ClearAlternativeAction();
						m_UndoButton.ClearOnClickOverriders();
						Singleton<UIActiveBonusBar>.Instance.Hide();
						CardsHandManager.Instance.Hide();
						(bonus as CForgoActionsForCompanionActiveBonus).ApplyToggledActiveBonusesToCompanionSummon();
						ScenarioRuleClient.StepComplete();
					}, ReadyButton.EButtonState.EREADYBUTTONCONFIRM);
					readyButton.SetInteractable(selected);
					m_UndoButton.SetInteractable(selected);
				});
				m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				m_SkipButton.SetInteractable(active: true);
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				Action undo = null;
				undo = delegate
				{
					Singleton<UIActiveBonusBar>.Instance.UndoSelection();
					readyButton.SetInteractable(interactable: false);
					m_UndoButton.SetInteractable(active: false);
					m_UndoButton.SetOnClickOverrider(undo, UndoButton.EButtonState.EUNDOBUTTONUNDO);
					m_UndoButton.SetInteractable(active: false);
				};
				m_UndoButton.Toggle(active: true, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
				m_UndoButton.SetInteractable(active: false);
				m_UndoButton.SetOnClickOverrider(undo, UndoButton.EButtonState.EUNDOBUTTONUNDO);
				m_UndoButton.SetInteractable(active: false);
				if (FFSNetwork.IsOnline)
				{
					if (!m_CurrentActor.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ForgoActionForBoost);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.ForgoActionForBoost);
					}
				}
			}
			catch (Exception ex43)
			{
				Debug.LogError("An exception occurred while processing the ShowForgoActiveBonus message\n" + ex43.Message + "\n" + ex43.StackTrace);
			}
			break;
		case CMessageData.MessageType.FinishedProcessingTileSelected:
			try
			{
				CFinishedProcessingTileSelected_MessageData cFinishedProcessingTileSelected_MessageData = (CFinishedProcessingTileSelected_MessageData)message;
				if (PhaseManager.Phase.Type == CPhase.PhaseType.Action && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
				{
					if (cFinishedProcessingTileSelected_MessageData.m_Ability is CAbilityMove { State: var state } && (state == CAbilityMove.EMoveState.ActorIsMovingB || state == CAbilityMove.EMoveState.ActorIsSelectingMoveTile))
					{
						readyButton.SetInteractable(interactable: false);
						SetActiveSelectButton(activate: false);
					}
					else
					{
						readyButton.SetInteractable(cFinishedProcessingTileSelected_MessageData.m_Ability.RequiresWaypointSelection() ? (Waypoint.s_Waypoints.Count > 0) : cFinishedProcessingTileSelected_MessageData.m_Ability.EnoughTargetsSelected());
						SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
					}
					m_UndoButton.SetInteractable(cFinishedProcessingTileSelected_MessageData.m_Ability.CanUndo && FirstAbility);
					m_SkipButton.SetInteractable(cFinishedProcessingTileSelected_MessageData.m_Ability.CanSkip);
					if (cFinishedProcessingTileSelected_MessageData.m_Ability.AreaEffect != null || cFinishedProcessingTileSelected_MessageData.m_Ability.AreaEffectBackup != null)
					{
						if (!WorldspaceStarHexDisplay.Instance.IsAOELocked() && (cFinishedProcessingTileSelected_MessageData.m_Ability.ValidActorsInRange.Count > 0 || cFinishedProcessingTileSelected_MessageData.m_Ability.TilesSelected.Count > 0 || (cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget != null && cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget.Count > 0)) && cFinishedProcessingTileSelected_MessageData.m_AreaEffectLocked)
						{
							AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
							m_UndoButton.Toggle(active: true, UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS, LocalizationManager.GetTranslation("GUI_CLEARTARGETS"));
							m_SkipButton.SetInteractable(active: false);
							WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: true);
							readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMTARGETS, LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS"), hideOnClick: true, glowingEffect: true);
							SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
							if (cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget != null && cFinishedProcessingTileSelected_MessageData.m_Ability.MaxTargetsSelected())
							{
								foreach (GameObject clientActorObject5 in ClientActorObjects)
								{
									ActorBehaviour actorBehaviour2 = ActorBehaviour.GetActorBehaviour(clientActorObject5);
									if (!cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget.Contains(actorBehaviour2.Actor))
									{
										actorBehaviour2.m_WorldspacePanelUI.ResetPreview();
									}
								}
							}
						}
						else
						{
							AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
						}
					}
					else
					{
						CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cFinishedProcessingTileSelected_MessageData.m_SelectedTile.m_ArrayIndex.X, cFinishedProcessingTileSelected_MessageData.m_SelectedTile.m_ArrayIndex.Y];
						if (!cFinishedProcessingTileSelected_MessageData.m_Ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets() > 0 && (!cFinishedProcessingTileSelected_MessageData.m_Ability.AllTargets || FirstAbility))
						{
							if (!cFinishedProcessingTileSelected_MessageData.m_Ability.IsInlineSubAbility && !cFinishedProcessingTileSelected_MessageData.m_Ability.UseSubAbilityTargeting)
							{
								m_UndoButton.Toggle((FirstAbility || !cFinishedProcessingTileSelected_MessageData.m_Ability.AllTargets) && cFinishedProcessingTileSelected_MessageData.m_Ability.CanUndo, (!cFinishedProcessingTileSelected_MessageData.m_Ability.AllTargets) ? UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS : UndoButton.EButtonState.EUNDOBUTTONUNDO, cFinishedProcessingTileSelected_MessageData.m_Ability.AllTargets ? LocalizationManager.GetTranslation("GUI_UNDO") : LocalizationManager.GetTranslation("GUI_CLEARTARGETS"));
							}
							if (cFinishedProcessingTileSelected_MessageData.m_Ability.MaxTargetsSelected())
							{
								foreach (CActor item44 in cFinishedProcessingTileSelected_MessageData.m_Ability.ValidActorsInRange)
								{
									if (cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget.Contains(item44))
									{
										continue;
									}
									GameObject gameObject7 = FindClientActor(item44);
									if (gameObject7 != null)
									{
										ActorBehaviour actorBehaviour3 = ActorBehaviour.GetActorBehaviour(gameObject7);
										if (actorBehaviour3 != null)
										{
											actorBehaviour3.m_WorldspacePanelUI.ResetPreview();
										}
									}
								}
							}
						}
						if (m_WaitState.m_State == ChoreographerStateType.WaitingForTileSelected)
						{
							if (WorldspaceStarHexDisplay.Instance.AlreadySelected(clientTile))
							{
								AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
							}
							else
							{
								AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
							}
						}
					}
				}
				if (m_WaitState.m_State == ChoreographerStateType.WaitingForTileSelected)
				{
					SetChoreographerState(ChoreographerStateType.Play, 0, null);
					WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				}
				if (m_BufferedSelectedTile != null)
				{
					CClientTile bufferedSelectedTile2 = m_BufferedSelectedTile;
					bool bufferedTileNetworked2 = m_BufferedTileNetworked;
					bool bufferedSecondClickToConfirm2 = m_BufferedSecondClickToConfirm;
					DisableTileSelection(active: false);
					TileHandler(bufferedSelectedTile2, null, bufferedTileNetworked2, isUserClick: false, bufferedSecondClickToConfirm2);
				}
				else
				{
					DisableTileSelection(active: false);
				}
				if (Singleton<UIUseItemsBar>.Instance.IsShown)
				{
					Singleton<UIUseItemsBar>.Instance.RefreshTooltip();
				}
				CAbility ability3 = cFinishedProcessingTileSelected_MessageData.m_Ability;
				CAbilityTargeting targetingAbility = ability3 as CAbilityTargeting;
				if (targetingAbility != null && targetingAbility.TargetingActor != null)
				{
					Singleton<UIUseItemsBar>.Instance.ShowUsableItems(targetingAbility.TargetingActor, (CItem cItem2) => cItem2.YMLData.Data.CompareAbility != null && cItem2.YMLData.Data.CompareAbility.CompareAbility(targetingAbility, null, checkActorsToTarget: true));
				}
				if (cFinishedProcessingTileSelected_MessageData.m_Ability is CAbilityPush cAbilityPush && cAbilityPush.IsAlreadyPushing())
				{
					readyButton.ToggleVisibility(active: false);
				}
				if (cFinishedProcessingTileSelected_MessageData.m_ActorSpawningMessage == null && m_CurrentActor != null)
				{
					if (m_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_STUNNED"), LocalizationManager.GetTranslation(m_CurrentActor.ActorLocKey())));
						ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(m_CurrentActor)).m_WorldspacePanelUI.StunnedWarnEffect(active: true);
					}
					else if (m_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm))
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_DISARMED"), LocalizationManager.GetTranslation(m_CurrentActor.ActorLocKey())));
						ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(m_CurrentActor)).m_WorldspacePanelUI.DisarmedWarnEffect(active: true);
					}
					else if (m_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_SLEEPING"), LocalizationManager.GetTranslation(m_CurrentActor.ActorLocKey())));
						ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(m_CurrentActor)).m_WorldspacePanelUI.SleepingWarnEffect(active: true);
					}
					else if (cFinishedProcessingTileSelected_MessageData.m_Ability is CAbilitySummon cAbilitySummon2)
					{
						string translation6 = LocalizationManager.GetTranslation(cAbilitySummon2.SelectedLocKey);
						Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON_SELECT_HEX"), translation6), string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON"), translation6));
					}
					else if (cFinishedProcessingTileSelected_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.AddDoom)
					{
						if (cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget.Count != 1)
						{
							Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_DOOM"), string.Format("<color=#{0}>{1}</color>", UIInfoTools.Instance.GetCharacterHexColor(ECharacter.Doomstalker), LocalizationManager.GetTranslation("Doom")));
						}
						else
						{
							int count = cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget[0].CachedDoomActiveBonuses.Count;
							List<CActiveBonus> source = CharacterClassManager.FindAllActiveBonuses(CAbility.EAbilityType.AddDoomSlots, m_CurrentActor);
							if (1 + source.Sum((CActiveBonus it) => it.Ability.Strength) <= count)
							{
								Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_DOOM"), string.Format("<color=#{0}>{1}</color>", UIInfoTools.Instance.GetCharacterHexColor(ECharacter.Doomstalker), LocalizationManager.GetTranslation("Doom")));
							}
						}
					}
					else if (cFinishedProcessingTileSelected_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.TransferDooms)
					{
						Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_TRANSFER_DOOM"), $"<color=#{UIInfoTools.Instance.GetCharacterHexColor(ECharacter.Doomstalker)}>{LocalizationManager.GetTranslation(cFinishedProcessingTileSelected_MessageData.m_Ability.AbilityBaseCard.Name)}</color>");
					}
					else if (cFinishedProcessingTileSelected_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.RemoveConditions)
					{
						if (cFinishedProcessingTileSelected_MessageData.m_Ability.Strength == -1)
						{
							Singleton<HelpBox>.Instance.Show("GUI_TOOLTIP_REMOVE_CONDITIONS", cFinishedProcessingTileSelected_MessageData.m_Ability?.AbilityBaseCard?.Name);
						}
					}
					else if (cFinishedProcessingTileSelected_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.Choose)
					{
						foreach (CActor item45 in cFinishedProcessingTileSelected_MessageData.m_Ability.ValidActorsInRange)
						{
							ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(item45)).m_WorldspacePanelUI.Focus(focus: false);
						}
						Singleton<HelpBox>.Instance.Show($"GUI_CHOOSE_ABILITY_TARGET_{cFinishedProcessingTileSelected_MessageData.m_Ability.AbilityBaseCard.Name}", cFinishedProcessingTileSelected_MessageData.m_Ability?.AbilityBaseCard?.Name);
					}
					else
					{
						ShowAbilityHelpBoxTooltip(cFinishedProcessingTileSelected_MessageData.m_Ability);
					}
				}
				if (FFSNetwork.IsOnline && ActionProcessor.HasSavedStateInSamePhase)
				{
					if (ActionProcessor.CurrentState.StateType == ActionProcessorStateType.Halted)
					{
						ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
					}
					else
					{
						ActionProcessor.ClearSavedState();
					}
				}
			}
			catch (Exception ex42)
			{
				Debug.LogError("An exception occurred while processing the FinishedProcessingTileSelected message\n" + ex42.Message + "\n" + ex42.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00183", "GUI_ERROR_MAIN_MENU_BUTTON", ex42.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex42.Message);
			}
			break;
		case CMessageData.MessageType.FinishedProcessingTileDeselected:
			try
			{
				CFinishedProcessingTileDeselected_MessageData cFinishedProcessingTileDeselected_MessageData = (CFinishedProcessingTileDeselected_MessageData)message;
				if (PhaseManager.Phase.Type == CPhase.PhaseType.Action && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
				{
					if (cFinishedProcessingTileDeselected_MessageData.m_Ability.AreaEffect != null)
					{
						AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileCancelClickAudioItem);
						if (cFinishedProcessingTileDeselected_MessageData.m_Ability.MiscAbilityData != null && cFinishedProcessingTileDeselected_MessageData.m_Ability.MiscAbilityData.ExactRange.HasValue && cFinishedProcessingTileDeselected_MessageData.m_Ability.MiscAbilityData.ExactRange.Value)
						{
							readyButton.SetInteractable(cFinishedProcessingTileDeselected_MessageData.m_Ability.EnoughTargetsSelected());
							m_UndoButton.SetInteractable(cFinishedProcessingTileDeselected_MessageData.m_Ability.CanUndo && FirstAbility);
							m_SkipButton.SetInteractable(cFinishedProcessingTileDeselected_MessageData.m_Ability.CanSkip);
							SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
						}
						else if (WorldspaceStarHexDisplay.Instance.IsAOELocked())
						{
							m_UndoButton.Toggle(active: true, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
							m_UndoButton.SetInteractable(cFinishedProcessingTileDeselected_MessageData.m_Ability.CanUndo && FirstAbility);
							m_SkipButton.SetInteractable(cFinishedProcessingTileDeselected_MessageData.m_Ability.CanSkip);
							WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
							WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
							readyButton.SetInteractable(interactable: false);
						}
					}
					else
					{
						if (WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets() <= 0)
						{
							AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileCancelClickAudioItem);
							if (cFinishedProcessingTileDeselected_MessageData.m_Ability.CanUndo && FirstAbility)
							{
								m_UndoButton.Toggle(active: true, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
							}
						}
						readyButton.SetInteractable(cFinishedProcessingTileDeselected_MessageData.m_Ability.EnoughTargetsSelected());
						m_UndoButton.SetInteractable(cFinishedProcessingTileDeselected_MessageData.m_Ability.CanUndo && FirstAbility);
						m_SkipButton.SetInteractable(cFinishedProcessingTileDeselected_MessageData.m_Ability.CanSkip);
						SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
					}
				}
				if (m_WaitState.m_State == ChoreographerStateType.WaitingForTileSelected)
				{
					SetChoreographerState(ChoreographerStateType.Play, 0, null);
					WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				}
				if (m_BufferedSelectedTile != null)
				{
					CClientTile bufferedSelectedTile = m_BufferedSelectedTile;
					bool bufferedTileNetworked = m_BufferedTileNetworked;
					bool bufferedSecondClickToConfirm = m_BufferedSecondClickToConfirm;
					DisableTileSelection(active: false);
					TileHandler(bufferedSelectedTile, null, bufferedTileNetworked, isUserClick: false, bufferedSecondClickToConfirm);
				}
				else
				{
					DisableTileSelection(active: false);
				}
				if (cFinishedProcessingTileDeselected_MessageData.m_ActorSpawningMessage == null && m_CurrentActor != null)
				{
					if (m_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_STUNNED"), LocalizationManager.GetTranslation(m_CurrentActor.ActorLocKey())));
						ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(m_CurrentActor)).m_WorldspacePanelUI.StunnedWarnEffect(active: true);
					}
					else if (m_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm))
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_DISARMED"), LocalizationManager.GetTranslation(m_CurrentActor.ActorLocKey())));
						ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(m_CurrentActor)).m_WorldspacePanelUI.DisarmedWarnEffect(active: true);
					}
					else if (m_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_SLEEPING"), LocalizationManager.GetTranslation(m_CurrentActor.ActorLocKey())));
						ActorBehaviour.GetActorBehaviour(FindClientActorGameObject(m_CurrentActor)).m_WorldspacePanelUI.SleepingWarnEffect(active: true);
					}
					else if (cFinishedProcessingTileDeselected_MessageData.m_Ability is CAbilitySummon cAbilitySummon)
					{
						string translation5 = LocalizationManager.GetTranslation(cAbilitySummon.SelectedLocKey);
						Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON_SELECT_HEX"), translation5), string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_SUMMON"), translation5));
					}
					else if (cFinishedProcessingTileDeselected_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.TransferDooms)
					{
						Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_TRANSFER_DOOM"), $"<color=#{UIInfoTools.Instance.GetCharacterHexColor(ECharacter.Doomstalker)}>{LocalizationManager.GetTranslation(cFinishedProcessingTileDeselected_MessageData.m_Ability.AbilityBaseCard.Name)}</color>");
					}
					else if (cFinishedProcessingTileDeselected_MessageData.m_Ability is CAbilityAttack cAbilityAttack && cAbilityAttack.AttackSummary.Targets.Exists((CAttackSummary.TargetSummary it) => it.UsedAttackMods != null && it.UsedAttackMods.Exists((AttackModifierYMLData mod) => mod.AddTarget)))
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_TARGET_MODIFIER_TIP"), string.Format("<color=#{1}>{0}</color>", LocalizationManager.GetTranslation("GUI_TOOLTIP_ADD_TARGET_MODIFIER"), UIInfoTools.Instance.regularModifierColor.ToHex()));
					}
					else
					{
						ShowAbilityHelpBoxTooltip(cFinishedProcessingTileDeselected_MessageData.m_Ability);
					}
				}
				if (FFSNetwork.IsOnline && ActionProcessor.HasSavedStateInSamePhase)
				{
					if (ActionProcessor.CurrentState.StateType == ActionProcessorStateType.Halted)
					{
						ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
					}
					else
					{
						ActionProcessor.ClearSavedState();
					}
				}
			}
			catch (Exception ex41)
			{
				Debug.LogError("An exception occurred while processing the FinishedProcessingTileDeselected_MessageData message\n" + ex41.Message + "\n" + ex41.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00235", "GUI_ERROR_MAIN_MENU_BUTTON", ex41.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex41.Message);
			}
			break;
		case CMessageData.MessageType.ShowChooseAbilityUI:
			try
			{
				CShowChooseAbilityUI_MessageData cShowChooseAbilityUI_MessageData = (CShowChooseAbilityUI_MessageData)message;
				if (cShowChooseAbilityUI_MessageData.m_ActorSpawningMessage is CPlayerActor)
				{
					m_UndoButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
					m_UndoButton.SetInteractable(cShowChooseAbilityUI_MessageData.m_ChooseAbility.CanUndo && FirstAbility);
					m_SkipButton.Toggle(message.m_ActorSpawningMessage.Type == CActor.EType.Player, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"), hideOnClick: true, cShowChooseAbilityUI_MessageData.m_ChooseAbility.CanSkip);
					SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy && message.m_ActorSpawningMessage.Type == CActor.EType.Player);
				}
			}
			catch (Exception ex40)
			{
				Debug.LogError("An exception occurred while processing the ShowChooseAbilityUI message\n" + ex40.Message + "\n" + ex40.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00184", "GUI_ERROR_MAIN_MENU_BUTTON", ex40.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex40.Message);
			}
			break;
		case CMessageData.MessageType.HideChooseAbilityUI:
			try
			{
				_ = (CHideChooseAbilityUI_MessageData)message;
				Singleton<UIUseAbilitiesBar>.Instance.Hide();
			}
			catch (Exception ex39)
			{
				Debug.LogError("An exception occurred while processing the HideChooseAbilityUI message\n" + ex39.Message + "\n" + ex39.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00185", "GUI_ERROR_MAIN_MENU_BUTTON", ex39.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex39.Message);
			}
			break;
		case CMessageData.MessageType.StartTurnActiveBonusTriggered:
			try
			{
				CardsHandManager.Instance.Hide();
			}
			catch (Exception ex38)
			{
				Debug.LogError("An exception occurred while processing the StartTurnActiveBonusTriggered message\n" + ex38.Message + "\n" + ex38.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00187", "GUI_ERROR_MAIN_MENU_BUTTON", ex38.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex38.Message);
			}
			break;
		case CMessageData.MessageType.CheckForInitiativeAdjustments:
			try
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CheckForInitiativeAdjustments);
				CCheckForInitiativeAdjustments_MessageData cCheckForInitiativeAdjustments_MessageData = (CCheckForInitiativeAdjustments_MessageData)message;
				if (message.m_ActorSpawningMessage is CPlayerActor cPlayerActor)
				{
					m_CurrentActor = message.m_ActorSpawningMessage;
					Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(cCheckForInitiativeAdjustments_MessageData.m_ActorSpawningMessage, CAbility.EAbilityType.AdjustInitiative);
					if (!cPlayerActor.IsDead)
					{
						InitiativeTrack.Instance.Select(cPlayerActor);
					}
				}
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, LocalizationManager.GetTranslation("GUI_CONTINUE"), hideOnClick: true, glowingEffect: false, !FFSNetwork.IsOnline || message.m_ActorSpawningMessage.IsUnderMyControl, disregardTurnControlForInteractability: true);
				if (FFSNetwork.IsOnline)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.InitiativeAdjustments);
				}
			}
			catch (Exception ex37)
			{
				Debug.LogError("An exception occurred while processing the CheckForInitiativeAdjustments message\n" + ex37.Message + "\n" + ex37.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00191", "GUI_ERROR_MAIN_MENU_BUTTON", ex37.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex37.Message);
			}
			break;
		case CMessageData.MessageType.EndInitiativeAdjustments:
			try
			{
				_ = (CEndInitiativeAdjustments_MessageData)message;
				if (message.m_ActorSpawningMessage is CPlayerActor)
				{
					Singleton<UIActiveBonusBar>.Instance.Hide();
				}
			}
			catch (Exception ex36)
			{
				Debug.LogError("An exception occurred while processing the EndInitiativeAdjustments message\n" + ex36.Message + "\n" + ex36.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00192", "GUI_ERROR_MAIN_MENU_BUTTON", ex36.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex36.Message);
			}
			break;
		case CMessageData.MessageType.UpdateInitiativeTrack:
			try
			{
				_ = (CUpdateInitiativeTrack_MessageData)message;
				InitiativeTrack.Instance.UpdateActors();
				readyButton.SetInteractable(interactable: true);
			}
			catch (Exception ex35)
			{
				Debug.LogError("An exception occurred while processing the UpdateInitiativeTrack message\n" + ex35.Message + "\n" + ex35.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00193", "GUI_ERROR_MAIN_MENU_BUTTON", ex35.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex35.Message);
			}
			break;
		case CMessageData.MessageType.ChangeModifier:
			try
			{
				_ = (CChangeModifier_MessageData)message;
			}
			catch (Exception ex34)
			{
				Debug.LogError("An exception occurred while processing the ChangeModifier message\n" + ex34.Message + "\n" + ex34.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00193", "GUI_ERROR_MAIN_MENU_BUTTON", ex34.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex34.Message);
			}
			break;
		case CMessageData.MessageType.StackedAugmentToNullAbility:
			try
			{
				_ = (CStackedAugmentToNullAbility_MessageData)message;
				CardsActionControlller.s_Instance.RefreshConsumeBar();
			}
			catch (Exception ex33)
			{
				Debug.LogError("An exception occurred while processing the UpdateInitiativeTrack message\n" + ex33.Message + "\n" + ex33.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00193", "GUI_ERROR_MAIN_MENU_BUTTON", ex33.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex33.Message);
			}
			break;
		case CMessageData.MessageType.ShowRedistributeDamageUI:
			try
			{
				CShowRedistributeDamageUI_MessageData messageData = (CShowRedistributeDamageUI_MessageData)message;
				Singleton<HelpBox>.Instance.ShowControllerOrKeyboardTip(LocalizationManager.GetTranslation("GUI_TOOLTIP_DISTRIBUTE_HEALTH"), () => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_DISTRIBUTE_HEALTH"), Singleton<InputManager>.Instance.GetGamepadActionIconByName("UIMove")), LocalizationManager.GetTranslation(messageData.m_RedistributeDamageAbility.AbilityBaseCard.Name));
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
				readyButton.SetInteractable(interactable: false);
				m_SkipButton.SetInteractable(messageData.m_RedistributeDamageAbility.ParentAbility == null && messageData.m_RedistributeDamageAbility.CanSkip);
				SetActiveSelectButton(activate: false);
				DistributeDamageService distributeDamage = new DistributeDamageService(messageData.m_ActorSpawningMessage, messageData.m_ActorsToRedistributeBetween, messageData.m_RedistributeDamageAbility);
				WorldspaceStarHexDisplay.Instance.SetDisplayAbility(messageData.m_RedistributeDamageAbility, WorldspaceStarHexDisplay.EAbilityDisplayType.RedistributeDamageAbility);
				WorldspaceStarHexDisplay.Instance.StoreDistributeDamageService(distributeDamage);
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.TargetSelection;
				bool undoButtonToReset = false;
				Singleton<UIScenarioDistributePointsManager>.Instance.ShowAssign(distributeDamage, delegate
				{
					if (distributeDamage.AvailablePoints == 0)
					{
						readyButton.HideWarning();
						readyButton.SetInteractable(interactable: true);
					}
					else
					{
						readyButton.ShowWarning();
						readyButton.SetInteractable(interactable: false);
					}
					if (!undoButtonToReset)
					{
						undoButtonToReset = true;
						m_UndoButton.SetOnClickOverrider(delegate
						{
							distributeDamage.Reset();
							Singleton<UIScenarioDistributePointsManager>.Instance.Refresh();
							readyButton.HideWarning();
							readyButton.SetInteractable(interactable: false);
							undoButtonToReset = false;
							m_UndoButton.ClearOnClickOverriders();
						}, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_RESET"));
					}
					WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				}, delegate
				{
					WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				});
				if (distributeDamage.HasDamageToAssign())
				{
					readyButton.QueueAlternativeAction(delegate
					{
						Singleton<UIScenarioDistributePointsManager>.Instance.Hide();
						messageData.m_RedistributeDamageAbility.StoreHealthChanges(distributeDamage.GetHealthChanges());
						ScenarioRuleClient.StepComplete();
					});
				}
				if (FFSNetwork.IsOnline)
				{
					if (!messageData.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.HealthRedistribution);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.HealthRedistribution);
					}
				}
			}
			catch (Exception ex32)
			{
				Debug.LogError("An exception occurred while processing the ShowRedistributeDamageUI message\n" + ex32.Message + "\n" + ex32.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00199", "GUI_ERROR_MAIN_MENU_BUTTON", ex32.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex32.Message);
			}
			break;
		case CMessageData.MessageType.HideRedistributeDamageUI:
			try
			{
				_ = (CHideRedistributeDamageUI_MessageData)message;
				Singleton<UIScenarioDistributePointsManager>.Instance.Hide();
			}
			catch (Exception ex31)
			{
				Debug.LogError("An exception occurred while processing the HideRedistributeDamageUI message\n" + ex31.Message + "\n" + ex31.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00201", "GUI_ERROR_MAIN_MENU_BUTTON", ex31.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex31.Message);
			}
			break;
		case CMessageData.MessageType.AIFindingPath:
			try
			{
				CAIFindingPath_MessageData cAIFindingPath_MessageData = (CAIFindingPath_MessageData)message;
				CActor actorSpawningMessage = cAIFindingPath_MessageData.m_ActorSpawningMessage;
				CActor targetActor = cAIFindingPath_MessageData.m_TargetActor;
				string arg5 = LocalizationManager.GetTranslation(actorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorSpawningMessage);
				string arg6 = ((targetActor == null) ? LocalizationManager.GetTranslation("Target") : (LocalizationManager.GetTranslation(targetActor.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(targetActor)));
				StringBuilder stringBuilder2 = new StringBuilder(string.Format(LocalizationManager.GetTranslation(cAIFindingPath_MessageData.m_Reason), arg5, arg6));
				Singleton<CombatLogHandler>.Instance.AddLog(stringBuilder2.ToString(), CombatLogFilter.ABILITIES);
			}
			catch (Exception ex30)
			{
				Debug.LogError("An exception occurred while processing the AIFindingPath message\n" + ex30.Message + "\n" + ex30.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00223", "GUI_ERROR_MAIN_MENU_BUTTON", ex30.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex30.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsRedistributingDamage:
			try
			{
				CActorIsRedistributingDamage_MessageData cActorIsRedistributingDamage_MessageData = (CActorIsRedistributingDamage_MessageData)message;
				WorldspaceStarHexDisplay.Instance.ResetPlayerAttackType();
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
				ClearAllActorEvents();
				bool animationShouldPlay5 = false;
				CActor animatingActorToWaitFor5 = cActorIsRedistributingDamage_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorIsRedistributingDamage_MessageData.m_RedistributeDamageAbility, cActorIsRedistributingDamage_MessageData.m_ActorSpawningMessage, new List<string> { cActorIsRedistributingDamage_MessageData.AnimOverload, "PowerUp" }, out animationShouldPlay5, out animatingActorToWaitFor5);
				foreach (CActor item46 in cActorIsRedistributingDamage_MessageData.m_ActorsRedistributingDamageTo)
				{
					string arg4 = LocalizationManager.GetTranslation(item46.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(item46);
					GameObject gameObject4 = FindClientActorGameObject(item46);
					int healthChangeForActor = cActorIsRedistributingDamage_MessageData.m_RedistributeDamageAbility.GetHealthChangeForActor(item46);
					if (healthChangeForActor != item46.Health)
					{
						if (healthChangeForActor > item46.Health)
						{
							StringBuilder stringBuilder = new StringBuilder(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_HEALED"), arg4));
							stringBuilder.AppendFormat(" {0}", string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_HEALED_HEAL"), string.Format("<b>{0}</b>", "<sprite name=Heal>" + (healthChangeForActor - item46.Health))));
							Singleton<CombatLogHandler>.Instance.AddLog(stringBuilder.ToString(), CombatLogFilter.HEALING);
						}
						else
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DAMAGED"), arg4, string.Format("<b>{0}</b>", "<sprite name=Damage>" + Math.Abs(item46.Health - healthChangeForActor))), CombatLogFilter.DAMAGE);
							bool animationShouldPlay6 = false;
							ProcessActorAnimation(cActorIsRedistributingDamage_MessageData.m_RedistributeDamageAbility, item46, new List<string> { "Hit" }, out animationShouldPlay6, out var _);
							animationShouldPlay5 = animationShouldPlay5 || animationShouldPlay6;
						}
						GameObject prefab = ((healthChangeForActor > item46.Health) ? GlobalSettings.Instance.m_GlobalParticles.DefaultHealEffect : GlobalSettings.Instance.m_DefaultHitEffects.DefaultStandardHitEffect);
						GameObject gameObject5 = ObjectPool.Spawn(prefab, gameObject4.transform);
						ObjectPool.Recycle(gameObject5, VFXShared.GetEffectLifetime(gameObject5), prefab);
					}
				}
				readyButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(activate: false);
				if (animatingActorToWaitFor5 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay5 ? 10000 : 400, animatingActorToWaitFor5);
				}
				GameObject gameObject6 = FindClientActorGameObject(message.m_ActorSpawningMessage);
				if (!animationShouldPlay5)
				{
					ActorEvents.GetActorEvents(gameObject6).ProgressChoreographer();
				}
			}
			catch (Exception ex29)
			{
				Debug.LogError("An exception occurred while processing the ActorIsRedistributingDamage message\n" + ex29.Message + "\n" + ex29.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00200", "GUI_ERROR_MAIN_MENU_BUTTON", ex29.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex29.Message);
			}
			break;
		case CMessageData.MessageType.ShowToggleBonusesForAI:
			try
			{
				CShowToggleBonusesForAI_MessageData cShowToggleBonusesForAI_MessageData = (CShowToggleBonusesForAI_MessageData)message;
				readyButton.ClearAlternativeAction();
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONFIRM"), hideOnClick: true, glowingEffect: true);
				if (cShowToggleBonusesForAI_MessageData.m_Ability.CanApplyActiveBonusTogglesTo())
				{
					Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(cShowToggleBonusesForAI_MessageData.m_ActorSpawningMessage, cShowToggleBonusesForAI_MessageData.m_Ability.AbilityType, CActiveBonus.EActiveBonusBehaviourType.None, null, cShowToggleBonusesForAI_MessageData.m_Ability.AbilityHasHappened, showSingleTargetBonus: false, cShowToggleBonusesForAI_MessageData.m_Ability);
				}
				if (FFSNetwork.IsOnline)
				{
					if (!cShowToggleBonusesForAI_MessageData.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.AIBonusToggling);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.AIBonusToggling);
					}
				}
			}
			catch (Exception ex28)
			{
				Debug.LogError("An exception occurred while processing the CShowToggleBonusesForAI_MessageData message\n" + ex28.Message + "\n" + ex28.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00207", "GUI_ERROR_MAIN_MENU_BUTTON", ex28.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex28.Message);
			}
			break;
		case CMessageData.MessageType.ShowEndOfActionToggleBonuses:
			try
			{
				CShowEndOfActionToggleBonuses_MessageData cShowEndOfActionToggleBonuses_MessageData = (CShowEndOfActionToggleBonuses_MessageData)message;
				readyButton.ClearAlternativeAction();
				readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONFIRM"), hideOnClick: true, glowingEffect: true, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
				m_UndoButton.Toggle(active: false);
				m_SkipButton.Toggle(active: true, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(cShowEndOfActionToggleBonuses_MessageData.m_ActorSpawningMessage, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.EndActionAbility, null, abilityAlreadyStarted: true);
				if (FFSNetwork.IsOnline)
				{
					if (!cShowEndOfActionToggleBonuses_MessageData.m_ActorSpawningMessage.IsUnderMyControl)
					{
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.EndOfAction);
					}
					else
					{
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.EndOfAction);
					}
				}
			}
			catch (Exception ex27)
			{
				Debug.LogError("An exception occurred while processing the CShowEndOfActionToggleBonuses_MessageData message\n" + ex27.Message + "\n" + ex27.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00208", "GUI_ERROR_MAIN_MENU_BUTTON", ex27.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex27.Message);
			}
			break;
		case CMessageData.MessageType.RestartRound:
			try
			{
				RestartRound();
			}
			catch (Exception ex26)
			{
				Debug.LogError("An exception occurred while processing the RestartRound message\n" + ex26.Message + "\n" + ex26.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00209", "GUI_ERROR_MAIN_MENU_BUTTON", ex26.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex26.Message);
			}
			break;
		case CMessageData.MessageType.FreezeTime:
			try
			{
				TimeManager.FreezeTime();
			}
			catch (Exception ex25)
			{
				Debug.LogError("An exception occurred while processing the FreezeTime message\n" + ex25.Message + "\n" + ex25.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00210", "GUI_ERROR_MAIN_MENU_BUTTON", ex25.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex25.Message);
			}
			break;
		case CMessageData.MessageType.TileSelectionFinished:
			try
			{
				if (ActionProcessor.IsWaitingForTileSelectionFinishedMessage != null)
				{
					ActionProcessor.FinishProcessingAction(ActionProcessor.IsWaitingForTileSelectionFinishedMessage);
					ActionProcessor.IsWaitingForTileSelectionFinishedMessage = null;
				}
			}
			catch (Exception ex24)
			{
				Debug.LogError("An exception occurred while processing the TileSelectionFinished message\n" + ex24.Message + "\n" + ex24.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00211", "GUI_ERROR_MAIN_MENU_BUTTON", ex24.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex24.Message);
			}
			break;
		case CMessageData.MessageType.WaitForDelayedDrops:
			try
			{
				SetChoreographerState(ChoreographerStateType.WaitingForDelayedDrops, 0, null);
				m_SkipButton.SetInteractable(active: false);
			}
			catch (Exception ex23)
			{
				Debug.LogError("An exception occurred while processing the WaitForDelayedDrops message\n" + ex23.Message + "\n" + ex23.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00212", "GUI_ERROR_MAIN_MENU_BUTTON", ex23.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex23.Message);
			}
			break;
		case CMessageData.MessageType.ActorBeenDoomed:
			try
			{
				CActorBeenDoomed_MessageData cActorBeenDoomed_MessageData = (CActorBeenDoomed_MessageData)message;
				FindClientActorGameObject(cActorBeenDoomed_MessageData.m_ActorBeingDoomed);
				string arg2 = LocalizationManager.GetTranslation(cActorBeenDoomed_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActorBeenDoomed_MessageData.m_ActorSpawningMessage);
				string arg3 = LocalizationManager.GetTranslation(cActorBeenDoomed_MessageData.m_ActorBeingDoomed.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActorBeenDoomed_MessageData.m_ActorBeingDoomed);
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DOOM"), arg2, arg3));
				bool animationShouldPlay4 = false;
				CActor animatingActorToWaitFor4 = cActorBeenDoomed_MessageData.m_ActorSpawningMessage;
				ProcessActorAnimation(cActorBeenDoomed_MessageData.m_DoomAbility, cActorBeenDoomed_MessageData.m_ActorSpawningMessage, new List<string> { cActorBeenDoomed_MessageData.AnimOverload, "Hit" }, out animationShouldPlay4, out animatingActorToWaitFor4);
			}
			catch (Exception ex22)
			{
				Debug.LogError("An exception occurred while processing the ActorBeenDoomed message\n" + ex22.Message + "\n" + ex22.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00214", "GUI_ERROR_MAIN_MENU_BUTTON", ex22.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex22.Message);
			}
			break;
		case CMessageData.MessageType.SRLExceptionMessage:
		{
			CSRLException_MessageData cSRLException_MessageData = (CSRLException_MessageData)message;
			Debug.LogError("An unhandled exception occurred within the SRL\n" + cSRLException_MessageData.m_Message + "\n" + cSRLException_MessageData.m_Stack);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00216", "GUI_ERROR_MAIN_MENU_BUTTON", cSRLException_MessageData.m_Stack, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, cSRLException_MessageData.m_Message);
			break;
		}
		case CMessageData.MessageType.SRLWrongPhaseExceptionMessage:
		{
			CSRLWrongPhaseException_MessageData cSRLWrongPhaseException_MessageData = (CSRLWrongPhaseException_MessageData)message;
			string exceptionMessage = "ScenarioRuleClient was unable to process message of type: " + cSRLWrongPhaseException_MessageData.m_MessageType.ToString() + "during PhaseManager current phase of type: " + cSRLWrongPhaseException_MessageData.m_CurrentPhaseType;
			Debug.LogError("PhaseManager current phase was unable to handle message processed in the SRL\nCurrentPhase:" + cSRLWrongPhaseException_MessageData.m_CurrentPhaseType + "\n" + cSRLWrongPhaseException_MessageData.m_Stack);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00225", "GUI_ERROR_MAIN_MENU_BUTTON", cSRLWrongPhaseException_MessageData.m_Stack, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, exceptionMessage);
			break;
		}
		case CMessageData.MessageType.SupplyCardsGiven:
			try
			{
				CSupplyCardsGiven_MessageData cSupplyCardsGiven_MessageData = (CSupplyCardsGiven_MessageData)message;
				CardsHandManager.Instance.GetHand(cSupplyCardsGiven_MessageData.m_ActorGivenCards).SpawnGivenCardUI(cSupplyCardsGiven_MessageData.m_SupplyCardsGiven);
			}
			catch (Exception ex21)
			{
				Debug.LogError("An exception occurred while processing the SupplyCardsGiven message\n" + ex21.Message + "\n" + ex21.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00220", "GUI_ERROR_MAIN_MENU_BUTTON", ex21.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex21.Message);
			}
			break;
		case CMessageData.MessageType.SupplyCardUsed:
			try
			{
				CSupplyCardUsed_MessageData cSupplyCardUsed_MessageData = (CSupplyCardUsed_MessageData)message;
				CardsHandManager.Instance.GetHand(cSupplyCardUsed_MessageData.m_ActorUsedCard).DestroyCardUI(cSupplyCardUsed_MessageData.m_SupplyCardUsed);
			}
			catch (Exception ex20)
			{
				Debug.LogError("An exception occurred while processing the SupplyCardUsed message\n" + ex20.Message + "\n" + ex20.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00224", "GUI_ERROR_MAIN_MENU_BUTTON", ex20.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex20.Message);
			}
			break;
		case CMessageData.MessageType.ChoosePlayerActorToBurnCardToPreventDamage:
			try
			{
				_ = (CChoosePlayerActorToBurnCardToPreventDamage_MessageData)message;
				List<CPlayerActor> actors = ScenarioManager.Scenario.PlayerActors.Where((CPlayerActor it) => it.CharacterClass.HandAbilityCards.Count > 0 || it.CharacterClass.DiscardedAbilityCards.Count > 1).ToList();
				DistributeSelectPlayerActorService selectPlayerService = new DistributeSelectPlayerActorService(actors, "GUI_CHOOSE_ACTOR_PREVENT_DAMAGE");
				m_SkipButton.Toggle(active: false);
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONFIRM"), hideOnClick: true, glowingEffect: true);
				readyButton.SetInteractable(interactable: false);
				readyButton.QueueAlternativeAction(delegate
				{
					Singleton<UIScenarioDistributePointsManager>.Instance.Hide();
					GameState.SelectedPlayerToAvoidDamage(selectPlayerService.GetSelectedActor() as CPlayerActor);
				});
				Singleton<UIScenarioMultiplayerController>.Instance.ToggleButtonOptions(value: true);
				Singleton<UIScenarioDistributePointsManager>.Instance.ShowSelect(selectPlayerService, delegate
				{
					if (selectPlayerService.GetSelectedActor() == null)
					{
						readyButton.SetInteractable(interactable: false);
					}
					else
					{
						readyButton.SetInteractable(interactable: true, FFSNetwork.IsOnline && FFSNetwork.IsHost);
					}
				});
				if (FFSNetwork.IsOnline)
				{
					if (!FFSNetwork.IsHost)
					{
						string translation3 = LocalizationManager.GetTranslation("Consoles/GUI_MULTIPLAYER_TIP_TITLE");
						string translation4 = LocalizationManager.GetTranslation("Consoles/GUI_WAIT_FOR_HOST_TIP");
						InitiativeTrack.Instance.helpBox.ShowTranslated(translation4, translation3);
						InitiativeTrack.Instance.helpBox.BringToFront();
						ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.HealthRedistribution);
					}
					else
					{
						Singleton<HelpBox>.Instance.Show("GUI_CHOOSE_ACTOR_PREVENT_DAMAGE_HOST_TIP", "GUI_CHOOSE_ACTOR_PREVENT_DAMAGE_TIP_TITLE");
						ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.HealthRedistribution);
					}
				}
			}
			catch (Exception ex19)
			{
				Debug.LogError("An exception occurred while processing the ChoosePlayerActorToBurnCardToPreventDamage message\n" + ex19.Message + "\n" + ex19.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00221", "GUI_ERROR_MAIN_MENU_BUTTON", ex19.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex19.Message);
			}
			break;
		case CMessageData.MessageType.ForgoActionForActiveBonus:
			try
			{
				_ = (CForgoActionForActiveBonus_MessageData)message;
				CardsHandManager.Instance.RefreshCardsActionControllerPhase();
			}
			catch (Exception ex18)
			{
				Debug.LogError("An exception occurred while processing the ForgoActionForActiveBonus message\n" + ex18.Message + "\n" + ex18.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00236", "GUI_ERROR_MAIN_MENU_BUTTON", ex18.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex18.Message);
			}
			break;
		case CMessageData.MessageType.ScenarioModifierHiddenStateUpdated:
			try
			{
				CScenarioModifierHiddenStateUpdated_MessageData cScenarioModifierHiddenStateUpdated_MessageData = (CScenarioModifierHiddenStateUpdated_MessageData)message;
				UIManager.Instance.ScenarioModifierContainer.ModifyUpdatedHiddenOrDeactivatedState(cScenarioModifierHiddenStateUpdated_MessageData.m_UpdatedModifier);
				TryActivateWaitForConfirmHelpBox();
			}
			catch (Exception ex17)
			{
				Debug.LogError("An exception occurred while processing the ScenarioModifierHiddenStateUpdated message\n" + ex17.Message + "\n" + ex17.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00222", "GUI_ERROR_MAIN_MENU_BUTTON", ex17.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex17.Message);
			}
			break;
		case CMessageData.MessageType.RemoveDifficultTerrain:
			try
			{
				CDifficultTerrainRemoved_MessageData cDifficultTerrainRemoved_MessageData = (CDifficultTerrainRemoved_MessageData)message;
				if (cDifficultTerrainRemoved_MessageData.m_DifficultTerrainRemoved != null)
				{
					GameObject propObject2 = Singleton<ObjectCacheService>.Instance.GetPropObject(cDifficultTerrainRemoved_MessageData.m_DifficultTerrainRemoved);
					UnityEngine.Object.DestroyImmediate(propObject2);
					UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.PropDestroyed, null, cDifficultTerrainRemoved_MessageData.m_DifficultTerrainRemoved.PropGuid));
				}
			}
			catch (Exception ex16)
			{
				Debug.LogError("An exception occurred while processing the RemoveDifficultTerrain message\n" + ex16.Message + "\n" + ex16.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00237", "GUI_ERROR_MAIN_MENU_BUTTON", ex16.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex16.Message);
			}
			break;
		case CMessageData.MessageType.FinishedMovingAbilityCard:
			try
			{
				CPlayerFinishedMovingAbilityCard cPlayerFinishedMovingAbilityCard = (CPlayerFinishedMovingAbilityCard)message;
				CardsHandManager.Instance.RefreshPendingActiveBonus((CPlayerActor)cPlayerFinishedMovingAbilityCard.m_ActorSpawningMessage);
				if (cPlayerFinishedMovingAbilityCard.m_SendNetworkSelectedRoundCards)
				{
					CardsHandManager.Instance.GetHand((CPlayerActor)cPlayerFinishedMovingAbilityCard.m_ActorSpawningMessage).NetworkSelectedRoundCards();
				}
			}
			catch (Exception ex15)
			{
				Debug.LogError("An exception occurred while processing the FinishedMovingAbilityCard message\n" + ex15.Message + "\n" + ex15.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00238", "GUI_ERROR_MAIN_MENU_BUTTON", ex15.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex15.Message);
			}
			break;
		case CMessageData.MessageType.ActorIsRecoveringResources:
			try
			{
				CActorIsRecoveringResources_MessageData cActorIsRecoveringResources_MessageData = (CActorIsRecoveringResources_MessageData)message;
				ClearAllActorEvents();
				FindClientActorGameObject(cActorIsRecoveringResources_MessageData.m_ActorRecovering);
				bool animationShouldPlay3 = false;
				CActor animatingActorToWaitFor3 = cActorIsRecoveringResources_MessageData.m_ActorRecovering;
				ProcessActorAnimation(cActorIsRecoveringResources_MessageData.m_RecoverAbility, cActorIsRecoveringResources_MessageData.m_ActorRecovering, new List<string>
				{
					cActorIsRecoveringResources_MessageData.m_RecoverAbility?.AnimOverload,
					"TeleportAway",
					"PowerUp"
				}, out animationShouldPlay3, out animatingActorToWaitFor3);
				if (animatingActorToWaitFor3 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay3 ? 10000 : 400, animatingActorToWaitFor3);
				}
			}
			catch (Exception ex14)
			{
				Debug.LogError("An exception occurred while processing the ActorIsRecoveringResources message\n" + ex14.Message + "\n" + ex14.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00239", "GUI_ERROR_MAIN_MENU_BUTTON", ex14.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex14.Message);
			}
			break;
		case CMessageData.MessageType.ChangeCharacterModel:
			try
			{
				CChangeCharacterModel_MessageData cChangeCharacterModel_MessageData = (CChangeCharacterModel_MessageData)message;
				bool animationShouldPlay2 = false;
				CActor animatingActorToWaitFor2 = cChangeCharacterModel_MessageData.m_ActorToChange;
				ProcessActorAnimation(cChangeCharacterModel_MessageData.m_ChangeCharacterAbility, cChangeCharacterModel_MessageData.m_ActorToChange, new List<string>
				{
					cChangeCharacterModel_MessageData.AnimOverload,
					cChangeCharacterModel_MessageData.m_ChangeCharacterAbility?.AnimOverload,
					"PowerUp"
				}, out animationShouldPlay2, out animatingActorToWaitFor2);
				if (animatingActorToWaitFor2 != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay2 ? 10000 : 400, animatingActorToWaitFor2);
				}
			}
			catch (Exception ex13)
			{
				Debug.LogError("An exception occurred while processing the ChangeCharacterModel message\n" + ex13.Message + "\n" + ex13.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00240", "GUI_ERROR_MAIN_MENU_BUTTON", ex13.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex13.Message);
			}
			break;
		case CMessageData.MessageType.ActorSelectedNullHexes:
			try
			{
				CActorSelectedNullHexes_MessageData cActorSelectedNullHexes_MessageData = (CActorSelectedNullHexes_MessageData)message;
				ClearAllActorEvents();
				m_LastSelectedTiles = cActorSelectedNullHexes_MessageData.m_Ability.TilesSelected.Select((CTile t) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[t.m_ArrayIndex.X, t.m_ArrayIndex.Y]).ToList();
				WorldspaceStarHexDisplay.Instance.ClearNonTargetHexHighlights(null, null, m_LastSelectedTiles);
				bool animationShouldPlay = false;
				CActor animatingActorToWaitFor = cActorSelectedNullHexes_MessageData.m_Ability.TargetingActor;
				ProcessActorAnimation(cActorSelectedNullHexes_MessageData.m_Ability, cActorSelectedNullHexes_MessageData.m_Ability.TargetingActor, new List<string>
				{
					cActorSelectedNullHexes_MessageData.AnimOverload,
					GetNonOverloadAnim(cActorSelectedNullHexes_MessageData.m_Ability)
				}, out animationShouldPlay, out animatingActorToWaitFor);
				if (animatingActorToWaitFor != null)
				{
					SetChoreographerState(ChoreographerStateType.WaitingForGeneralAnim, animationShouldPlay ? 10000 : 400, animatingActorToWaitFor);
				}
				GameObject gameObject3 = FindClientActorGameObject(cActorSelectedNullHexes_MessageData.m_Ability.TargetingActor);
				if (!animationShouldPlay)
				{
					ActorEvents.GetActorEvents(gameObject3).ProgressChoreographer();
				}
				readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, "");
				m_UndoButton.Toggle(active: false);
				m_SkipButton.Toggle(active: false);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
			}
			catch (Exception ex12)
			{
				Debug.LogError("An exception occurred while processing the ActorSelectedNullHexes message\n" + ex12.Message + "\n" + ex12.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00241", "GUI_ERROR_MAIN_MENU_BUTTON", ex12.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex12.Message);
			}
			break;
		case CMessageData.MessageType.AbilityRestarted:
			try
			{
				_ = (CAbilityRestarted_MessageData)message;
				if (WorldspaceStarHexDisplay.Instance.IsAOELocked())
				{
					WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
				}
				if (!Waypoint.s_LockWaypoints)
				{
					Waypoint.Clear();
					WorldspaceStarHexDisplay.Instance.ResetLineDirection();
					WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				}
				readyButton.SetInteractable(interactable: false);
			}
			catch (Exception ex11)
			{
				Debug.LogError("An exception occurred while processing the AbilityRestarted message\n" + ex11.Message + "\n" + ex11.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00242", "GUI_ERROR_MAIN_MENU_BUTTON", ex11.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex11.Message);
			}
			break;
		case CMessageData.MessageType.MoveProp:
			try
			{
				CMoveProp_MessageData cMoveProp_MessageData = (CMoveProp_MessageData)message;
				if (cMoveProp_MessageData.m_MoveSpeed <= 0f)
				{
					GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(cMoveProp_MessageData.m_MoveProp);
					CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cMoveProp_MessageData.m_MoveToTile.m_ArrayIndex.X, cMoveProp_MessageData.m_MoveToTile.m_ArrayIndex.Y];
					propObject.transform.SetParent(cClientTile.m_GameObject.transform.parent);
					propObject.transform.position = cClientTile.m_GameObject.transform.position;
					propObject.transform.eulerAngles = new Vector3(0f, cMoveProp_MessageData.m_MoveProp.Rotation.Y, 0f);
					cMoveProp_MessageData.m_MoveProp.SetLocation(new TileIndex(cClientTile.m_Tile.m_ArrayIndex), GloomUtility.VToCV(propObject.transform.position), GloomUtility.VToCV(propObject.transform.eulerAngles));
					cClientTile.m_TileBehaviour.RefreshWorldSpaceIcon();
				}
				else
				{
					StartCoroutine(MoveProp(cMoveProp_MessageData));
				}
			}
			catch (Exception ex10)
			{
				Debug.LogError("An exception occurred while processing the MoveProp message\n" + ex10.Message + "\n" + ex10.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00243", "GUI_ERROR_MAIN_MENU_BUTTON", ex10.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex10.Message);
			}
			break;
		case CMessageData.MessageType.DestroyRoom:
			try
			{
				CDestroyRoom_MessageData cDestroyRoom_MessageData = (CDestroyRoom_MessageData)message;
				GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(cDestroyRoom_MessageData.m_MapToDestroy);
				if (map != null)
				{
					map.GetComponent<RoomVisibilityTracker>().ShowMaptile(show: false, forceOverride: true);
				}
				RevealRoomCreateCharacterActors();
			}
			catch (Exception ex9)
			{
				Debug.LogError("An exception occurred while processing the DestroyRoom message\n" + ex9.Message + "\n" + ex9.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00244", "GUI_ERROR_MAIN_MENU_BUTTON", ex9.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex9.Message);
			}
			break;
		case CMessageData.MessageType.FinishedProcessingActiveBonusToggle:
			try
			{
				CFinishedProcessingActiveBonusToggle_MessageData cFinishedProcessingActiveBonusToggle_MessageData = (CFinishedProcessingActiveBonusToggle_MessageData)message;
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: true);
				bool flag = !Singleton<TakeDamagePanel>.Instance.IsOpen || !Singleton<TakeDamagePanel>.Instance.ThisPlayerHasTakeDamageControl;
				if (m_CurrentAbility != null)
				{
					if (m_CurrentAbility.AbilityType == CAbility.EAbilityType.Attack && flag)
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectTarget);
						FocusWorld();
						int numberTargets = m_CurrentAbility.NumberTargets;
						if (numberTargets == -1 || numberTargets == m_CurrentAbility.ActorsToTarget.Count)
						{
							string text = ((numberTargets == -1 || numberTargets == 1 || m_CurrentAbility is CAbilityTargeting { OneTargetAtATime: not false }) ? LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS") : string.Format("{0} {1}/{2}", LocalizationManager.GetTranslation("GUI_CONFIRM_TARGETS"), numberTargets - m_CurrentAbility.NumberTargetsRemaining, numberTargets));
							readyButton.Toggle(m_CurrentAbility.FilterActor.Type == CActor.EType.Player, ReadyButton.EButtonState.EREADYBUTTONCONFIRMTARGETS, text, hideOnClick: true, glowingEffect: true);
						}
					}
					else
					{
						readyButton.SetInteractable((!m_CurrentAbility.RequiresWaypointSelection()) ? (m_CurrentAbility.EnoughTargetsSelected() && readyButton.IsInteractablePreviously && !m_CurrentAbility.IsWaitingForSingleTargetItemOrActiveBonus()) : ((bool)Waypoint.GetLastWaypoint && Waypoint.GetLastWaypoint.CanEndMovementHere));
					}
					if (InputManager.GamePadInUse && readyButton.IsInteractable)
					{
						Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.Unfocus();
						readyButton.SetDisableVisualState(value: false);
					}
					SetActiveSelectButton(!readyButton.IsInteractable && flag);
					m_UndoButton.SetInteractable(m_CurrentAbility.CanUndo && FirstAbility);
					m_SkipButton.SetInteractable(m_CurrentAbility.CanSkip);
				}
				if (FFSNetwork.IsOnline && !cFinishedProcessingActiveBonusToggle_MessageData.m_ActorSpawningMessage.IsUnderMyControl && ActionProcessor.HasSavedStateInSamePhase)
				{
					if (ActionProcessor.CurrentState.StateType == ActionProcessorStateType.Halted)
					{
						ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
					}
					else
					{
						ActionProcessor.ClearSavedState();
					}
				}
			}
			catch (Exception ex8)
			{
				Debug.LogError("An exception occurred while processing the FinishedProcessingActiveBonusToggle message\n" + ex8.Message + "\n" + ex8.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00245", "GUI_ERROR_MAIN_MENU_BUTTON", ex8.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex8.Message);
			}
			break;
		case CMessageData.MessageType.FinishedProcessingItemToggle:
			try
			{
				CFinishedProcessingItemToggle_MessageData cFinishedProcessingItemToggle_MessageData = (CFinishedProcessingItemToggle_MessageData)message;
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: true);
				if (cFinishedProcessingItemToggle_MessageData.m_Ability != null)
				{
					if (readyButton.gameObject.activeSelf)
					{
						readyButton.SetInteractable((!cFinishedProcessingItemToggle_MessageData.m_Ability.RequiresWaypointSelection()) ? cFinishedProcessingItemToggle_MessageData.m_Ability.EnoughTargetsSelected() : ((bool)Waypoint.GetLastWaypoint && Waypoint.GetLastWaypoint.CanEndMovementHere));
					}
					m_UndoButton.SetInteractable(cFinishedProcessingItemToggle_MessageData.m_Ability.CanUndo && FirstAbility);
					m_SkipButton.SetInteractable(cFinishedProcessingItemToggle_MessageData.m_Ability.CanSkip);
					SetActiveSelectButton(readyButton.gameObject.activeInHierarchy ? (!readyButton.IsVisibility) : (!readyButton.gameObject.activeInHierarchy));
				}
				if (FFSNetwork.IsOnline && ActionProcessor.HasSavedStateInSamePhase)
				{
					if (ActionProcessor.CurrentState.StateType == ActionProcessorStateType.Halted)
					{
						ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
					}
					else
					{
						ActionProcessor.ClearSavedState();
					}
				}
			}
			catch (Exception ex7)
			{
				Debug.LogError("An exception occurred while processing the FinishedProcessingItemToggle message\n" + ex7.Message + "\n" + ex7.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00248", "GUI_ERROR_MAIN_MENU_BUTTON", ex7.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex7.Message);
			}
			break;
		case CMessageData.MessageType.FinishedProcessingUndo:
			try
			{
				CFinishedProcessingUndo_MessageData cFinishedProcessingUndo_MessageData = (CFinishedProcessingUndo_MessageData)message;
				CAbility ability2 = cFinishedProcessingUndo_MessageData.m_Ability;
				bool isItemAbility = ability2.IsItemAbility;
				CAbility parentAbility = ability2.ParentAbility;
				if (parentAbility == null || parentAbility.AbilityType != CAbility.EAbilityType.ChooseAbility)
				{
					ElementInfusionBoardManager.RestoreState();
					InfusionBoardUI.Instance.RestoreState();
					InfusionBoardUI.Instance.UpdateBoard();
				}
				TileBehaviour.SetCallback(s_Choreographer.TileHandler);
				Waypoint.s_LockWaypoints = false;
				Waypoint.Clear();
				WorldspaceStarHexDisplay.Instance.Clear();
				s_Choreographer.SetChoreographerState(ChoreographerStateType.Play, 0, null);
				if (isItemAbility && s_Choreographer.FirstAbility)
				{
					CardsActionControlller.s_Instance.RefreshConsumeBar();
				}
				s_Choreographer.m_CurrentAbility = null;
			}
			catch (Exception ex6)
			{
				Debug.LogError("An exception occurred while processing the FinishedProcessingClearTargets message\n" + ex6.Message + "\n" + ex6.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00246", "GUI_ERROR_MAIN_MENU_BUTTON", ex6.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex6.Message);
			}
			break;
		case CMessageData.MessageType.FinishedProcessingClearTargets:
			try
			{
				CFinishedProcessingClearTargets_MessageData cFinishedProcessingClearTargets_MessageData = (CFinishedProcessingClearTargets_MessageData)message;
				CAbility ability = cFinishedProcessingClearTargets_MessageData.m_Ability;
				s_Choreographer.m_LastSelectedTiles?.Clear();
				WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
				if (ability.AreaEffect != null || ability.AreaEffectBackup != null || ability.AllTargetsOnAttackPath)
				{
					s_Choreographer.SetChoreographerState(ChoreographerStateType.WaitingForAreaAttackFocusSelection, 0, null);
				}
				m_UndoButton.Toggle((FirstAbility || ability.IsItemAbility) && ability.CanUndo && !ability.IsControlAbility, UndoButton.EButtonState.EUNDOBUTTONUNDO, LocalizationManager.GetTranslation("GUI_UNDO"));
				m_SkipButton.Toggle(ability.CanSkip, LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, LocalizationManager.GetTranslation("GUI_CONFIRM_ACTION"), hideOnClick: true, glowingEffect: true);
				SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
				Singleton<UIUseItemsBar>.Instance.RefreshItems(clear: true);
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: true);
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: true);
				Singleton<UIUseAugmentationsBar>.Instance.SetInteractionAvailableSlots(interactable: true);
			}
			catch (Exception ex5)
			{
				Debug.LogError("An exception occurred while processing the FinishedProcessingClearTargets message\n" + ex5.Message + "\n" + ex5.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00247", "GUI_ERROR_MAIN_MENU_BUTTON", ex5.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex5.Message);
			}
			break;
		case CMessageData.MessageType.UpdateWorldspaceConditionsUI:
			try
			{
				CUpdateWorldspaceConditionsUI_MessageData cUpdateWorldspaceConditionsUI_MessageData = (CUpdateWorldspaceConditionsUI_MessageData)message;
				GameObject gameObject2 = FindClientActorGameObject(cUpdateWorldspaceConditionsUI_MessageData.m_ActorSpawningMessage);
				if (gameObject2 != null)
				{
					ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject2);
					if (actorBehaviour != null)
					{
						actorBehaviour.UpdateWorldspaceConditionsUI();
					}
				}
			}
			catch (Exception ex4)
			{
				Debug.LogError("An exception occurred while processing the UpdateWorldspaceConditionsUI message\n" + ex4.Message + "\n" + ex4.StackTrace);
			}
			break;
		case CMessageData.MessageType.ActorChangedAllegiance:
			try
			{
				CActorChangedAllegiance_MessageData cActorChangedAllegiance_MessageData = (CActorChangedAllegiance_MessageData)message;
				string translation = LocalizationManager.GetTranslation(CAbilityChangeAllegiance.GetAllegianceLoc(cActorChangedAllegiance_MessageData.m_FromType));
				string translation2 = LocalizationManager.GetTranslation(CAbilityChangeAllegiance.GetAllegianceLoc(cActorChangedAllegiance_MessageData.m_ToType));
				GameObject gameObject = FindClientActorGameObject(cActorChangedAllegiance_MessageData.m_ActorSpawningMessage);
				if (gameObject != null)
				{
					Animator gameObjectAnimator = MF.GetGameObjectAnimator(gameObject);
					if (gameObjectAnimator != null)
					{
						gameObjectAnimator.SetInteger("ActorTypeEnum", (int)cActorChangedAllegiance_MessageData.m_ActorSpawningMessage.OriginalType);
					}
				}
				string arg = LocalizationManager.GetTranslation(cActorChangedAllegiance_MessageData.m_ActorSpawningMessage.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(cActorChangedAllegiance_MessageData.m_ActorSpawningMessage);
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_CHANGE_ALLEGIANCE_FROM_X_TO_Y"), arg, translation, translation2), CombatLogFilter.ABILITIES);
			}
			catch (Exception ex3)
			{
				Debug.LogError("An exception occurred while processing the ActorChangedAllegiance message\n" + ex3.Message + "\n" + ex3.StackTrace);
			}
			break;
		case CMessageData.MessageType.FinalizedOverrideAbilityType:
			try
			{
				CFinalizedOverrideAbilityType cFinalizedOverrideAbilityType = (CFinalizedOverrideAbilityType)message;
				readyButton.ClearAlternativeAction();
				if (cFinalizedOverrideAbilityType.m_Ability.CanApplyActiveBonusTogglesTo())
				{
					Singleton<UIActiveBonusBar>.Instance.ShowActiveBonus(cFinalizedOverrideAbilityType.m_ActorSpawningMessage, cFinalizedOverrideAbilityType.m_Ability.AbilityType, CActiveBonus.EActiveBonusBehaviourType.None, null, cFinalizedOverrideAbilityType.m_Ability.AbilityHasHappened, showSingleTargetBonus: false, cFinalizedOverrideAbilityType.m_Ability);
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError("An exception occurred while processing the FinalizedOverrideAbilityType message\n" + ex2.Message + "\n" + ex2.StackTrace);
			}
			break;
		case CMessageData.MessageType.ReplicateStartRoundCardState:
			try
			{
				CReplicateStartRoundCardState_MessageData cReplicateStartRoundCardState_MessageData = (CReplicateStartRoundCardState_MessageData)message;
				StartRoundCardsToken startRoundCardsToken = new StartRoundCardsToken(cReplicateStartRoundCardState_MessageData.m_StartRoundCardState);
				CardsHandManager.Instance.GetHand(cReplicateStartRoundCardState_MessageData.m_PlayerActor).CachedStartRoundToken = startRoundCardsToken;
				if (FFSNetwork.IsOnline)
				{
					int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? cReplicateStartRoundCardState_MessageData.m_PlayerActor.CharacterName.GetHashCode() : cReplicateStartRoundCardState_MessageData.m_PlayerActor.CharacterClass.ModelInstanceID);
					int gameActionID = cReplicateStartRoundCardState_MessageData.m_GameActionID;
					IProtocolToken supplementaryDataToken = startRoundCardsToken;
					Synchronizer.ReplicateControllableStateChange((GameActionType)gameActionID, ActionPhaseType.StartOfRound, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("An exception occurred while processing the ReplicateStartRoundCardState message\n" + ex.Message + "\n" + ex.StackTrace);
			}
			break;
		}
		m_PreviousMessageType = message.m_Type;
		SendMessageToWorldspace(message);
		if (GameState.InitiativeSortedActors.Count > 0 && (message.m_Type == CMessageData.MessageType.StartTurn || message.m_Type == CMessageData.MessageType.ActionSelectionPhaseStart || message.m_Type == CMessageData.MessageType.ActorDead || message.m_Type == CMessageData.MessageType.PlacingSpawn || message.m_Type == CMessageData.MessageType.EndTurn || message.m_Type == CMessageData.MessageType.EndRound || message.m_Type == CMessageData.MessageType.NextRound || message.m_Type == CMessageData.MessageType.Spawn || message.m_Type == CMessageData.MessageType.Summon || message.m_Type == CMessageData.MessageType.ActivateProp))
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				Debug.LogFormat("UpdateInitiativeTrack from SelectAbilityCardsOrLongRest {0}", message.m_Type);
				InitiativeTrack.Instance?.UpdateInitiativeTrack(m_ClientPlayers, ClientMonsterObjects, playersSelectable: true, enemiesSelectable: false);
			}
			else
			{
				bool flag16 = message.m_Type != CMessageData.MessageType.ActivateProp;
				CSummon_MessageData summon = message as CSummon_MessageData;
				bool flag17 = summon != null && !summon.m_SummonAbility.IsMonsterSummon && ScenarioManager.Scenario.HeroSummons.Exists((CHeroSummonActor it) => it.HeroSummonClass.ID == summon.m_SummonAbility.SelectedSummonID && it.IsCompanionSummon);
				bool flag18 = flag16 && !flag17 && (message.m_Type != CMessageData.MessageType.ActorDead || ((CActorDead_MessageData)message).m_Actor is CPlayerActor);
				Debug.LogFormat("UpdateInitiativeTrack from {0} (autoselect {1})", message.m_Type, flag18);
				InitiativeTrack.Instance?.UpdateInitiativeTrack(GameState.InitiativeSortedActors, playersSelectable: false, enemiesSelectable: false, flag18, flag16);
			}
		}
		if (message.m_Type == CMessageData.MessageType.EndAbilityAnimSync)
		{
			Singleton<TakeDamagePanel>.Instance.RefreshDamagePreview();
		}
		void OnShow()
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.RoundStart);
			if (UIScenarioEscMenu.ScenarioInstance.IsVisible)
			{
				UIScenarioEscMenu.ScenarioInstance.Focus();
			}
			if (FFSNetwork.IsOnline)
			{
				Singleton<UIScenarioMultiplayerController>.Instance.InitializeReadyToggleForCardSelection();
				if (FFSNetwork.IsHost)
				{
					Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
					bool num37 = PlayerRegistry.JoiningPlayers.Count > 0 || PlayerRegistry.ConnectingUsers.Count > 0;
					bool flag19 = PlayerRegistry.AllPlayers.Count == 1;
					if (num37 || flag19)
					{
						Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
						PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
						PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
					}
					PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Remove(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnter));
					PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Combine(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnter));
					FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
					FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
				}
				PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserLeft));
				PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserLeft));
				PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
				PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
				PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
				PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
				ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
				ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
				Singleton<UIScenarioMultiplayerController>.Instance.OnCardSelection();
			}
			else
			{
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, LocalizationManager.GetTranslation("GUI_END_SELECTION"), hideOnClick: true, glowingEffect: true);
				readyButton.SetInteractable(interactable: false);
				FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
				FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
			}
			m_UndoButton.Toggle(active: false);
			m_SkipButton.Toggle(active: false);
			SetActiveSelectButton(activate: false);
			InitiativeTrack.Instance.UpdateInitiativeTrack(m_ClientPlayers, ClientMonsterObjects, playersSelectable: true, enemiesSelectable: false, forceSelectFirstActor: true);
			InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
			GUIInterface.s_GUIInterface.SetStatusText("Once all players have round ability cards the end selection button is enabled");
			GUIInterface.s_GUIInterface.SetStatusText("Selecting a 3rd card will clear the set");
			GUIInterface.s_GUIInterface.SetStatusText("or Long Rest, if selecting ability cards then first one is the initiative lead card");
			GUIInterface.s_GUIInterface.SetStatusText("Select each player on the initiative track then select their 2 ability cards");
			m_InitialActionAbilityCard = null;
			InitiativeTrackActorBehaviour initiativeTrackActorBehaviour2 = InitiativeTrack.Instance.SelectedActor();
			if (initiativeTrackActorBehaviour2 != null && initiativeTrackActorBehaviour2.Actor.Type == CActor.EType.Player)
			{
				CardsHandManager.Instance.SwitchHand((CPlayerActor)initiativeTrackActorBehaviour2.Actor);
			}
			SetChoreographerState(ChoreographerStateType.WaitingForCardSelection, 0, null);
			if (AnyPlayersInInvalidStartingPositionsForCompanionSummons)
			{
				ShowWarningWhenAnyPlayersInInvalidStartingPositionsForCompanionSummons();
			}
			if (m_CurrentState.RoundNumber == 1 && !AutoTestController.s_AutoTestCurrentlyLoaded && (!SaveData.Instance.Global.CurrentlyPlayingCustomLevel || SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnSpecificLocations))
			{
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.CharacterPlacement;
			}
			if (FFSNetwork.IsOnline)
			{
				if (!Singleton<StoryController>.Instance.IsVisible)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.StartOfRound);
					if (FFSNetwork.IsClient)
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
					}
				}
				if (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant && !PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable x) => x.IsAlive))
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.StartOfRound);
					Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
				}
			}
			IsFirstTurnPlaying = false;
			if (FFSNetwork.IsClient && PlayerRegistry.MyPlayer != null && !PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment)
			{
				PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment = true;
				Synchronizer.SendSideAction(GameActionType.ReadyForAssignment, null, canBeUnreliable: false, sendToHostOnly: true);
			}
			m_PlayerToSelectAbilityCardsOrLongRest = true;
			if (FFSNetwork.IsOnline)
			{
				InitiativeTrack.Instance.PlayersUI.ForEach(delegate(InitiativeTrackPlayerBehaviour f)
				{
					f.Avatar.RefreshActiveInteractable();
				});
			}
		}
	}

	public void CheckAchievements()
	{
		CMapState mapState = AdventureState.MapState;
		if (mapState != null)
		{
			mapState.CheckNonTrophyAchievements();
			if (mapState.QueuedPlatformAchievementsToUnlock.Count > 0)
			{
				PlatformLayer.Stats.SetAchievementCompleted(mapState.QueuedPlatformAchievementsToUnlock);
			}
		}
	}

	private void SendMessageToWorldspace(CMessageData message)
	{
		try
		{
			foreach (GameObject clientActorObject in ClientActorObjects)
			{
				if (clientActorObject == null)
				{
					Debug.LogErrorFormat("Trying to access a null actor");
				}
				ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(clientActorObject);
				if (actorBehaviour == null)
				{
					Debug.LogErrorFormat("Trying to access a null ActorBehaviour for {0}", clientActorObject.name);
				}
				if (actorBehaviour.m_WorldspacePanelUI == null)
				{
					Debug.LogErrorFormat("Trying to access a null m_WorldspacePanelUI for {0}", actorBehaviour.Actor.ActorLocKey() + GetActorIDForCombatLogIfNeeded(actorBehaviour.Actor));
				}
				actorBehaviour.m_WorldspacePanelUI.OnMessage(message);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while processing SendMessageToWorldspace\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00144", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void EnableEndTurnButton(CPlayerActor currentPlayer)
	{
		try
		{
			readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDTURN, string.Format(LocalizationManager.GetTranslation(currentPlayer.IsTakingExtraTurn ? "GUI_END_EXTRA_TURN" : "GUI_END_TURN"), LocalizationManager.GetTranslation(currentPlayer.ActorLocKey())), hideOnClick: true, glowingEffect: true);
			m_SkipButton.Toggle(active: false);
			SetActiveSelectButton(!readyButton.gameObject.activeInHierarchy);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while processing EnableEndTurnButton\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00145", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ShowAbilityHelpBoxTooltip(CAbility ability)
	{
		if (!ability.HelpBoxTooltipLocKey.IsNOTNullOrEmpty())
		{
			return;
		}
		string text = LocalizationManager.GetTranslation(ability.HelpBoxTooltipLocKey);
		if (ability.NumberTargets > 1)
		{
			text = text.Replace("{CURRENT_TARGETS}", (ability.NumberTargets - ability.NumberTargetsRemaining).ToString()).Replace("{TOTAL_TARGETS}", ability.NumberTargets.ToString());
		}
		if (ability.Strength >= 1)
		{
			text = text.Replace("{STRENGTH}", ability.Strength.ToString());
		}
		if (ability.Range > 1)
		{
			text = text.Replace("{RANGE}", ability.Range.ToString());
		}
		if (ability.AbilityBaseCard != null)
		{
			string text2 = LocalizationManager.GetTranslation(ability.AbilityBaseCard.Name);
			CBaseCard abilityBaseCard = ability.AbilityBaseCard;
			CAbilityCard abilityCard = abilityBaseCard as CAbilityCard;
			if (abilityCard != null)
			{
				ECharacter characterModel = CharacterClassManager.Classes.Single((CCharacterClass s) => s.CharacterID == abilityCard.ClassID).CharacterModel;
				text2 = "<color=#" + UIInfoTools.Instance.GetCharacterColor(characterModel).ToHex() + ">" + text2 + "</color>";
			}
			Singleton<HelpBox>.Instance.ShowTranslated(text, text2);
		}
		else
		{
			Singleton<HelpBox>.Instance.ShowTranslated(text);
		}
	}

	public void LogScenarioResult(SEventActorFinishedScenario.EScenarioResult result)
	{
		if (AdventureState.MapState?.MapParty?.SelectedCharacters == null)
		{
			return;
		}
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			CPlayerActor actor = selectedCharacter.GetActor();
			if (actor != null)
			{
				CActor.EType type = actor.Type;
				string actorGuid = actor.ActorGuid;
				string iD = actor.Class.ID;
				int health = actor.Health;
				int gold = actor.Gold;
				int xP = actor.XP;
				int level = actor.Level;
				List<PositiveConditionPair> checkPositiveTokens = actor.Tokens.CheckPositiveTokens;
				List<NegativeConditionPair> checkNegativeTokens = actor.Tokens.CheckNegativeTokens;
				bool playedThisRound = actor.PlayedThisRound;
				bool isDead = actor.IsDead;
				CActor.ECauseOfDeath causeOfDeath = actor.CauseOfDeath;
				int originalMaxHealth = actor.OriginalMaxHealth;
				SEventLogMessageHandler.AddEventLogMessage(new SEventActorFinishedScenario(result, type, actorGuid, iD, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, IsSummon: false, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnSummon: false, null, null, "", originalMaxHealth));
			}
		}
	}

	public void WinScenario(bool forceUI = true)
	{
		StartCoroutine(WinScenarioCoroutine(forceUI));
	}

	private IEnumerator WinScenarioCoroutine(bool forceUI)
	{
		SimpleLog.AddToSimpleLog("Entered win scenario coroutine");
		ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.None);
		ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.None);
		if (IsRestarting || ScenarioRuleClient.ScenarioRuleClientStopped)
		{
			SimpleLog.AddToSimpleLog("Win scenario coroutine exited early. IsRestarting: " + IsRestarting + ". ScenarioRuleClientStopped: " + ScenarioRuleClient.ScenarioRuleClientStopped);
			yield break;
		}
		CardsHandManager.Instance.DisableHands();
		IsRestarting = true;
		yield return SceneController.Instance.EndScenarioSafely();
		IsRestarting = false;
		try
		{
			if (ScenarioManager.Scenario != null)
			{
				if (!forceUI && ScenarioManager.Scenario.CurrentScenarioResult == SEventActorFinishedScenario.EScenarioResult.Win)
				{
					SimpleLog.AddToSimpleLog("Win scenario coroutine exited early. CurrentScenarioResult != Win");
					yield break;
				}
				ScenarioManager.Scenario.CurrentScenarioResult = SEventActorFinishedScenario.EScenarioResult.Win;
			}
			SimpleLog.AddToSimpleLog("Scenario Won");
			LogScenarioResult(SEventActorFinishedScenario.EScenarioResult.Win);
			CardsHandManager.Instance.IsFullDeckPreviewAllowed = false;
			Singleton<PhaseBannerHandler>.Instance.Disable();
			readyButton.Toggle(active: false);
			m_SkipButton.Toggle(active: false);
			SetActiveSelectButton(activate: false);
			SaveData.Instance.Global.CurrentAdventureData?.AdventureMapState?.ApplyRoundChestRewards();
			Action<UnityAction> onFinishedResultAnimation = delegate(UnityAction endScenarioAction)
			{
				if (LevelEventsController.s_EventsControllerActive)
				{
					Singleton<LevelEventsController>.Instance.CompleteLevel(completionSuccess: true, userQuit: false, endScenarioAction);
				}
				else
				{
					endScenarioAction?.Invoke();
				}
			};
			if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.Autotest)
			{
				Singleton<UIResultsManager>.Instance.Show(1.5f, EndGame, EResult.Win);
			}
			else
			{
				UINewAdventureResultsManager.Implementation.Show(1.5f, EndGame, EResult.Win, UIResultsManager.ERetryOptionType.None, null, m_InitialProgressAchievements, onFinishedResultAnimation);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while processing Scenario Win\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00146", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void LoseScenario(bool forceUI = true)
	{
		StartCoroutine(LoseScenarioCoroutine(forceUI));
	}

	private IEnumerator LoseScenarioCoroutine(bool forceUI)
	{
		SimpleLog.AddToSimpleLog("Entered lose scenario coroutine");
		if (Singleton<TakeDamagePanel>.Instance.IsOpen)
		{
			Singleton<TakeDamagePanel>.Instance.ResetAndHide();
		}
		if (!InputManager.GamePadInUse)
		{
			ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.None);
			ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.None);
		}
		if (IsRestarting || ScenarioRuleClient.ScenarioRuleClientStopped)
		{
			SimpleLog.AddToSimpleLog("Lose scenario coroutine exited early. IsRestarting: " + IsRestarting + ". ScenarioRuleClientStopped: " + ScenarioRuleClient.ScenarioRuleClientStopped);
			yield break;
		}
		CardsHandManager.Instance.DisableHands();
		IsRestarting = true;
		yield return SceneController.Instance.EndScenarioSafely();
		IsRestarting = false;
		try
		{
			if (ScenarioManager.Scenario != null)
			{
				if (!forceUI && ScenarioManager.Scenario.CurrentScenarioResult == SEventActorFinishedScenario.EScenarioResult.Lose)
				{
					SimpleLog.AddToSimpleLog("Lose scenario coroutine exited early. CurrentScenarioResult != Lose");
					yield break;
				}
				ScenarioManager.Scenario.CurrentScenarioResult = SEventActorFinishedScenario.EScenarioResult.Lose;
			}
			SimpleLog.AddToSimpleLog("Scenario Lost");
			LogScenarioResult(SEventActorFinishedScenario.EScenarioResult.Lose);
			CardsHandManager.Instance.IsFullDeckPreviewAllowed = false;
			Singleton<PhaseBannerHandler>.Instance.Disable();
			readyButton.Toggle(active: false);
			m_SkipButton.Toggle(active: false);
			SetActiveSelectButton(activate: false);
			SaveData.Instance.Global.CurrentAdventureData?.AdventureMapState?.ApplyRoundChestRewards();
			Action<UnityAction> onFinishedResultAnimation = delegate(UnityAction endScenarioAction)
			{
				if (LevelEventsController.s_EventsControllerActive)
				{
					Singleton<LevelEventsController>.Instance.CompleteLevel(completionSuccess: false, userQuit: false, endScenarioAction);
				}
				else
				{
					endScenarioAction?.Invoke();
				}
			};
			if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.Autotest)
			{
				Singleton<UIResultsManager>.Instance.Show(1.5f, EndGame, EResult.Lose);
			}
			else if (SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial || SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState.IsTutorialOrIntroScenario == true)
			{
				UINewAdventureResultsManager.Implementation.Show(1.5f, EndGame, EResult.Lose, UIResultsManager.ERetryOptionType.Only, UIManager.RestartScenarioFromInitial, m_InitialProgressAchievements, onFinishedResultAnimation);
			}
			else
			{
				UINewAdventureResultsManager.Implementation.Show(1.5f, EndGame, EResult.Lose, UIResultsManager.ERetryOptionType.Additional, UIManager.RegenerateAndRestartScenarioKeepGoldAndXP, m_InitialProgressAchievements, onFinishedResultAnimation);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while processing Scenario Lose\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00147", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void AbandonScenario()
	{
		StartCoroutine(AbandonScenarioCoroutine());
	}

	private IEnumerator AbandonScenarioCoroutine()
	{
		SimpleLog.AddToSimpleLog("Entered abandon scenario coroutine");
		ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.None);
		ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.None);
		if (IsRestarting || ScenarioRuleClient.ScenarioRuleClientStopped)
		{
			SimpleLog.AddToSimpleLog("Abandon scenario coroutine exited early. IsRestarting: " + IsRestarting + ". ScenarioRuleClientStopped: " + ScenarioRuleClient.ScenarioRuleClientStopped);
			yield break;
		}
		CardsHandManager.Instance.DisableHands();
		IsRestarting = true;
		Singleton<StoryController>.Instance.Clear();
		yield return SceneController.Instance.EndScenarioSafely();
		IsRestarting = false;
		try
		{
			SaveData.Instance.Global.StopSpeedUp();
			SimpleLog.AddToSimpleLog("Scenario Abandoned");
			LogScenarioResult(SEventActorFinishedScenario.EScenarioResult.Resign);
			CardsHandManager.Instance.IsFullDeckPreviewAllowed = false;
			if (LevelEventsController.s_EventsControllerActive)
			{
				Singleton<LevelEventsController>.Instance.CompleteLevel(completionSuccess: false, userQuit: true);
			}
			if (FFSNetwork.IsHost)
			{
				Synchronizer.SendSideAction(GameActionType.AbandonScenario);
			}
			SaveData.Instance.Global.CurrentAdventureData?.AdventureMapState?.ApplyRoundChestRewards();
			if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.Autotest)
			{
				Singleton<UIResultsManager>.Instance.Show(1.5f, EndGame, EResult.Lose);
			}
			else if (SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial || SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState.IsTutorialOrIntroScenario == true)
			{
				UINewAdventureResultsManager.Implementation.Show(1.5f, EndGame, EResult.Lose, UIResultsManager.ERetryOptionType.Only, UIManager.RestartScenarioFromInitial, m_InitialProgressAchievements);
			}
			else
			{
				UINewAdventureResultsManager.Implementation.Show(1.5f, EndGame, EResult.Lose, UIResultsManager.ERetryOptionType.Additional, UIManager.RegenerateAndRestartScenarioKeepGoldAndXP, m_InitialProgressAchievements);
			}
			Singleton<PhaseBannerHandler>.Instance.Disable();
			readyButton.Toggle(active: false);
			m_SkipButton.Toggle(active: false);
			SetActiveSelectButton(activate: false);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while processing Abandon Scenario Lose\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00148", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public async void ProcessAutosavesThreaded()
	{
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		MemoryStream stream = await Task.Run((Func<MemoryStream>)SerializeMapState);
		s_Choreographer.CheckWaitingForAutosave(hideProgress: false);
		Task.Run(delegate
		{
			SaveMapState(stream);
		});
	}

	private MemoryStream SerializeMapState()
	{
		SetSavingProcessFlag(value: true);
		MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Binder = new SerializationBinding();
		binaryFormatter.Serialize(memoryStream, m_CurrentState);
		SetSavingProcessFlag(value: false);
		return memoryStream;
	}

	private void SaveMapState(MemoryStream stream)
	{
		SetSavingProcessFlag(value: true);
		stream.Position = 0L;
		ScenarioRuleLibrary.ScenarioState currentState = (ScenarioRuleLibrary.ScenarioState)new BinaryFormatter
		{
			Binder = new SerializationBinding()
		}.Deserialize(stream);
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			SaveData.Instance.Global.CampaignData.UpdateScenarioCheckpoint(currentState, OnSaveDone);
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
		{
			SaveData.Instance.Global.AdventureData.UpdateScenarioCheckpoint(currentState, OnSaveDone);
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario)
		{
			SaveData.Instance.Global.SingleScenarioData.UpdateScenarioCheckpoint(currentState, OnSaveDone);
		}
	}

	private void OnSaveDone()
	{
		SetSavingProcessFlag(value: false);
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			UnityMainThreadDispatcher.Instance().Enqueue(Singleton<AutoSaveProgress>.Instance.HideProgress);
		}
	}

	private static void SetSavingProcessFlag(bool value)
	{
		SaveData.Instance.IsSavingThreadActive = value;
		SaveData.Instance.IsSavingData = value;
	}

	public void ProcessActorAnimation(CAbility ability, CActor defaultActorToApplyTo, List<string> orderedStatesToTry, out bool animationShouldPlay, out CActor animatingActorToWaitFor)
	{
		if (ability != null && ability is CAbilityTargeting { OriginatesFromAnAura: not false } cAbilityTargeting)
		{
			animatingActorToWaitFor = null;
			bool flag = false;
			if (cAbilityTargeting.AuraTriggerAbilityAnimType == CActiveBonus.EAuraTriggerAbilityAnimType.AnimateAuraOriginator || cAbilityTargeting.AuraTriggerAbilityAnimType == CActiveBonus.EAuraTriggerAbilityAnimType.AnimateBoth)
			{
				List<string> list = orderedStatesToTry?.ToList() ?? new List<string>();
				if (!string.IsNullOrEmpty(cAbilityTargeting.OriginatingAuraAnimOverload))
				{
					list.Insert(0, cAbilityTargeting.OriginatingAuraAnimOverload);
				}
				GameObject gameObject = FindClientActorGameObject(cAbilityTargeting.OriginatingAuraOwner, shouldReturnDummyActorsProp: true);
				if (gameObject != null)
				{
					flag = PlayFirstAvailableAnimationStateOnObject(gameObject, list);
				}
				animatingActorToWaitFor = cAbilityTargeting.OriginatingAuraOwner;
			}
			bool flag2 = false;
			if (animatingActorToWaitFor != defaultActorToApplyTo && (cAbilityTargeting.AuraTriggerAbilityAnimType == CActiveBonus.EAuraTriggerAbilityAnimType.AnimateAuraReceiver || cAbilityTargeting.AuraTriggerAbilityAnimType == CActiveBonus.EAuraTriggerAbilityAnimType.AnimateBoth))
			{
				GameObject gameObject2 = FindClientActorGameObject(defaultActorToApplyTo, shouldReturnDummyActorsProp: true);
				if (gameObject2 != null)
				{
					flag2 = PlayFirstAvailableAnimationStateOnObject(gameObject2, orderedStatesToTry);
				}
				animatingActorToWaitFor = defaultActorToApplyTo;
			}
			animationShouldPlay = flag || flag2;
		}
		else if (ability != null && (ability.SkipAnim || ability.IsModifierAbility))
		{
			animatingActorToWaitFor = defaultActorToApplyTo;
			animationShouldPlay = false;
		}
		else
		{
			GameObject objectToAnimate = FindClientActorGameObject(defaultActorToApplyTo, shouldReturnDummyActorsProp: true);
			animationShouldPlay = PlayFirstAvailableAnimationStateOnObject(objectToAnimate, orderedStatesToTry);
			animatingActorToWaitFor = defaultActorToApplyTo;
		}
	}

	private bool PlayFirstAvailableAnimationStateOnObject(GameObject objectToAnimate, List<string> statesToTry)
	{
		if (objectToAnimate == null || statesToTry == null || statesToTry.Count == 0)
		{
			return false;
		}
		Animator gameObjectAnimator = MF.GetGameObjectAnimator(objectToAnimate);
		if (gameObjectAnimator != null)
		{
			for (int i = 0; i < statesToTry.Count; i++)
			{
				if (MF.AnimatorPlay(gameObjectAnimator, statesToTry[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	private IEnumerator MoveProp(CMoveProp_MessageData messageData)
	{
		GameObject propGO = Singleton<ObjectCacheService>.Instance.GetPropObject(messageData.m_MoveProp);
		CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[messageData.m_MoveToTile.m_ArrayIndex.X, messageData.m_MoveToTile.m_ArrayIndex.Y];
		propGO.transform.SetParent(clientTile.m_GameObject.transform.parent);
		Vector3 startPosition = propGO.transform.position;
		Vector3 endPosition = clientTile.m_GameObject.transform.position;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		float lerpProgress = 0f;
		while (lerpProgress < 1f)
		{
			lerpProgress += (Timekeeper.instance.m_GlobalClock.time - startTime) * messageData.m_MoveSpeed;
			propGO.transform.position = Vector3.Lerp(startPosition, endPosition, lerpProgress);
			yield return null;
		}
		propGO.transform.eulerAngles = new Vector3(0f, messageData.m_MoveProp.Rotation.Y, 0f);
		messageData.m_MoveProp.SetLocation(new TileIndex(clientTile.m_Tile.m_ArrayIndex), GloomUtility.VToCV(propGO.transform.position), GloomUtility.VToCV(propGO.transform.eulerAngles));
		clientTile.m_TileBehaviour.RefreshWorldSpaceIcon();
	}

	private IEnumerator SpawnProp(CSpawn_MessageData messageData)
	{
		if (messageData.m_SpawnDelay > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(messageData.m_SpawnDelay);
		}
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[messageData.m_Prop.ArrayIndex.X, messageData.m_Prop.ArrayIndex.Y];
		GameObject gameObject = GlobalSettings.GetApparancePropPrefab(messageData.m_Prop.PrefabName) ?? GlobalSettings.GetSpecificPropPrefab(messageData.m_Prop.PrefabName);
		if (gameObject == null)
		{
			Debug.LogError("Unable to find prop " + messageData.m_Prop.PrefabName);
			yield break;
		}
		if (Singleton<ObjectCacheService>.Instance.GetPropObject(messageData.m_Prop) != null)
		{
			Debug.LogError("Prop already spawned from prefab: " + messageData.m_Prop.PrefabName);
			yield break;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, cClientTile.m_GameObject.transform.parent);
		UnityGameEditorObject component = gameObject2.GetComponent<UnityGameEditorObject>();
		if (component == null)
		{
			Debug.LogError("No UnityGameEditorObject script found on Trap");
			UnityEngine.Object.Destroy(gameObject2);
			yield break;
		}
		Singleton<ObjectCacheService>.Instance.AddProp(messageData.m_Prop, gameObject2);
		component.PropObject = messageData.m_Prop;
		gameObject2.name = messageData.m_Prop.InstanceName;
		gameObject2.transform.position = cClientTile.m_GameObject.transform.position;
		if (messageData.m_Prop.Rotation != null)
		{
			gameObject2.transform.eulerAngles = new Vector3(0f, messageData.m_Prop.Rotation.Y, 0f);
		}
		messageData.m_Prop.SetLocation(new TileIndex(cClientTile.m_Tile.m_ArrayIndex), GloomUtility.VToCV(gameObject2.transform.position), GloomUtility.VToCV(gameObject2.transform.eulerAngles));
		PropParent component2 = gameObject2.GetComponent<PropParent>();
		if (component2 != null)
		{
			component2.PlacedInScenario = false;
		}
		if (LevelEditorController.s_Instance.IsEditing || LevelEditorController.s_Instance.IsPreviewingLevel || SaveData.Instance.Global.CurrentlyPlayingCustomLevel)
		{
			CCustomLevelData cCustomLevelData = (LevelEditorController.s_Instance.IsEditing ? SaveData.Instance.Global.CurrentEditorLevelData : SaveData.Instance.Global.CurrentCustomLevelData);
			if (cCustomLevelData.HasApparanceOverrides)
			{
				if (messageData.m_Prop is CObjectObstacle cObjectObstacle && !string.IsNullOrEmpty(cObjectObstacle.PropGUIDToCopy))
				{
					CApparanceOverrideDetails apparanceOverrideDetailsForGUID = cCustomLevelData.GetApparanceOverrideDetailsForGUID(cObjectObstacle.PropGUIDToCopy);
					cCustomLevelData.ApparanceOverrideList.Add(new CApparanceOverrideDetails(messageData.m_Prop.PropGuid)
					{
						OverrideBiome = apparanceOverrideDetailsForGUID.OverrideBiome,
						OverrideTheme = apparanceOverrideDetailsForGUID.OverrideTheme,
						OverrideTone = apparanceOverrideDetailsForGUID.OverrideTone,
						OverrideSiblingIndex = apparanceOverrideDetailsForGUID.OverrideSiblingIndex,
						OverrideSubBiome = apparanceOverrideDetailsForGUID.OverrideSubBiome,
						OverrideSubTheme = apparanceOverrideDetailsForGUID.OverrideSubTheme,
						OverrideSeed = apparanceOverrideDetailsForGUID.OverrideSeed
					});
				}
				CApparanceOverrideDetails apparanceOverrideDetailsForGUID2 = cCustomLevelData.GetApparanceOverrideDetailsForGUID(messageData.m_Prop.PropGuid);
				if (apparanceOverrideDetailsForGUID2 != null)
				{
					GameObject gameObject3 = m_MapSceneRoot.FindInChildren(messageData.m_Prop.InstanceName, includeInactive: true);
					if (gameObject3 != null)
					{
						gameObject3.GetComponent<ProceduralStyle>()?.InitialiseWithOverride(apparanceOverrideDetailsForGUID2);
					}
				}
			}
		}
		if (messageData.m_Prop.ObjectType == ScenarioManager.ObjectImportType.PressurePlate && messageData.m_Prop.Activated)
		{
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(messageData.m_Prop);
			StartCoroutine(PrimePropActivationAnimation(propObject, "Pressed"));
		}
		cClientTile.m_TileBehaviour.RefreshWorldSpaceIcon();
	}

	private IEnumerator DestroyProp(float destroyDelay, CObjectProp destroyProp)
	{
		if (destroyDelay > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(destroyDelay);
		}
		GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(destroyProp);
		Singleton<ObjectCacheService>.Instance.RemoveProp(destroyProp);
		propObject.GetComponent<UnityGameEditorObject>().ReleasePathing();
		BreakableObject componentInChildren = propObject.GetComponentInChildren<BreakableObject>();
		if (componentInChildren != null)
		{
			componentInChildren.BreakObject();
		}
		else
		{
			UnityEngine.Object.Destroy(propObject);
		}
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.PropDestroyed, null, destroyProp.PropGuid));
	}

	private IEnumerator PrimePropActivationAnimation(GameObject propGO, string animParam)
	{
		float animatorCheckTimeout = 10f;
		int frameIntervalBetweenChecks = 10;
		float timeAtStart = Timekeeper.instance.m_GlobalClock.time;
		bool timedOut = false;
		int currentFrameInterval = frameIntervalBetweenChecks;
		while (propGO != null)
		{
			if (currentFrameInterval == frameIntervalBetweenChecks)
			{
				if (MF.GameObjectAnimatorPlay(propGO, animParam))
				{
					break;
				}
				currentFrameInterval = 0;
				if (timeAtStart - Timekeeper.instance.m_GlobalClock.time >= animatorCheckTimeout)
				{
					timedOut = true;
					break;
				}
			}
			yield return null;
			currentFrameInterval++;
		}
		if (timedOut)
		{
			Debug.LogWarningFormat("Prop activation animation timed out. Prop:{0} | AnimationParameter:{1}");
		}
	}

	public void CheckOpenDoor(CObjectProp prop, CActor actorOpeningDoor, bool initialLoad, bool openDoorOnly)
	{
		CObjectDoor obj = (CObjectDoor)prop;
		bool flag = true;
		if (obj.LockType != CObjectDoor.ELockType.None)
		{
			List<UnityGameEditorDoorLockProp> list = Singleton<ObjectCacheService>.Instance.GetPropObject(prop).GetComponentsInChildren<UnityGameEditorDoorLockProp>().ToList();
			if (list.Count > 0)
			{
				if (actorOpeningDoor != null)
				{
					m_ActorOpeningDoor = actorOpeningDoor;
				}
				bool flag2 = false;
				foreach (UnityGameEditorDoorLockProp item in list)
				{
					if (!MF.GameObjectAnimatorControllerIsCurrentState(item.gameObject, "Unlock") && !MF.GameObjectAnimatorControllerIsCurrentState(item.gameObject, "Unlocked_Idle"))
					{
						flag2 |= MF.GameObjectAnimatorPlay(item.gameObject, "Unlock");
					}
				}
				if (flag2 && !initialLoad)
				{
					flag = false;
					if (DoorsUnlocking == null)
					{
						DoorsUnlocking = new List<CObjectProp>();
					}
					DoorsUnlocking.Add(prop);
				}
			}
		}
		if (flag)
		{
			if (actorOpeningDoor != null)
			{
				m_ActorOpeningDoor = actorOpeningDoor;
			}
			OpenDoor(prop, initialLoad, openDoorOnly);
		}
	}

	public void OpenDoor(CObjectProp prop, bool initialLoad, bool openDoorOnly)
	{
		GameObject doorGO = Singleton<ObjectCacheService>.Instance.GetPropObject(prop);
		if (doorGO == null)
		{
			Debug.LogWarning("Failed to find door object to open");
			return;
		}
		UnityAction unityAction = delegate
		{
			if (!MF.GameObjectAnimatorControllerIsCurrentState(doorGO, "Open"))
			{
				MF.GameObjectAnimatorPlay(doorGO, "Open");
				if (!string.IsNullOrEmpty(AudioSystem.instance.EnvironmentAudio.DoorOpeningAudioEvent))
				{
					AudioController.Play(AudioSystem.instance.EnvironmentAudio.DoorOpeningAudioEvent, doorGO.transform);
				}
			}
			if (openDoorOnly && m_ActorOpeningDoor != null)
			{
				GameObject gameObject2 = FindClientActorGameObject(m_ActorOpeningDoor);
				if (gameObject2 != null)
				{
					ActorBehaviour.GetActorBehaviour(gameObject2).PauseLoco(pause: false);
				}
				m_ActorOpeningDoor = null;
			}
		};
		if ((bool)MF.GetGameObjectAnimator(doorGO))
		{
			unityAction();
		}
		else
		{
			ProceduralProp component = doorGO.GetComponent<ProceduralProp>();
			if (component != null)
			{
				component.PlacementCompleteAction = unityAction;
			}
		}
		if (!initialLoad)
		{
			RevealRoomCreateCharacterActors();
			WorldspaceStarHexDisplay.Instance.ActorIsSelectingDoor(active: false);
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
		}
		if (m_ActorOpeningDoor != null)
		{
			GameObject gameObject = FindClientActorGameObject(m_ActorOpeningDoor);
			if (gameObject != null)
			{
				ActorBehaviour.GetActorBehaviour(gameObject).PauseLoco(pause: false);
			}
			m_ActorOpeningDoor = null;
		}
	}

	public void UnlockLockedDoorWithoutOpening(CObjectProp prop)
	{
		if (((CObjectDoor)prop).LockType == CObjectDoor.ELockType.None)
		{
			return;
		}
		List<UnityGameEditorDoorLockProp> list = Singleton<ObjectCacheService>.Instance.GetPropObject(prop).GetComponentsInChildren<UnityGameEditorDoorLockProp>().ToList();
		if (list.Count > 0)
		{
			bool flag = false;
			foreach (UnityGameEditorDoorLockProp item in list)
			{
				flag |= MF.GameObjectAnimatorPlay(item.gameObject, "Unlock");
			}
		}
		if (m_CurrentActor != null)
		{
			GameObject gameObject = FindClientActorGameObject(m_CurrentActor);
			if (gameObject != null)
			{
				ActorBehaviour.GetActorBehaviour(gameObject).PauseLoco(pause: false);
			}
		}
	}

	public void RevealRoomCreateCharacterActors()
	{
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			if (!FindClientEnemy(enemy))
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[enemy.StartArrayIndex.X, enemy.StartArrayIndex.Y];
				if (cClientTile != null)
				{
					m_ClientEnemies.Add(CreateCharacterActor(cClientTile, enemy));
				}
			}
		}
		foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
		{
			if (!FindClientEnemy(allyMonster))
			{
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[allyMonster.StartArrayIndex.X, allyMonster.StartArrayIndex.Y];
				if (cClientTile2 != null)
				{
					m_ClientAllyMonsters.Add(CreateCharacterActor(cClientTile2, allyMonster));
				}
			}
		}
		foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
		{
			if (!FindClientEnemy(enemy2Monster))
			{
				CClientTile cClientTile3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[enemy2Monster.StartArrayIndex.X, enemy2Monster.StartArrayIndex.Y];
				if (cClientTile3 != null)
				{
					m_ClientEnemy2Monsters.Add(CreateCharacterActor(cClientTile3, enemy2Monster));
				}
			}
		}
		foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
		{
			if (!FindClientEnemy(neutralMonster))
			{
				CClientTile cClientTile4 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[neutralMonster.StartArrayIndex.X, neutralMonster.StartArrayIndex.Y];
				if (cClientTile4 != null)
				{
					m_ClientNeutralMonsters.Add(CreateCharacterActor(cClientTile4, neutralMonster));
				}
			}
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			if (!FindClientObjectActor(@object))
			{
				CClientTile cClientTile5 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[@object.StartArrayIndex.X, @object.StartArrayIndex.Y];
				if (cClientTile5 != null)
				{
					m_ClientObjects.Add(CreateCharacterActor(cClientTile5, @object));
				}
			}
		}
		TilesOcclusionGenerator.s_Instance?.UpdateAwaitingVolumes();
		RoomVisibilityManager.s_Instance?.EvaluateRoomsVisiblity();
	}

	private void UpdatePreviewHealForAbility(CAbility targetingHealAbility)
	{
		foreach (CActor item in targetingHealAbility.ValidActorsInRange)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(s_Choreographer.FindClientActorGameObject(item));
			int num = targetingHealAbility.ModifiedStrength();
			foreach (CActiveBonus item2 in CActiveBonus.FindApplicableActiveBonuses(item, CAbility.EAbilityType.AddHeal))
			{
				CAbility ability = item2.Ability;
				if (ability != null && ability.ActiveBonusData?.Behaviour == CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal)
				{
					num += item2.ReferenceStrength(new CAbility(), item);
				}
			}
			if (targetingHealAbility is CAbilityHeal cAbilityHeal)
			{
				if (cAbilityHeal.MiscAbilityData != null && cAbilityHeal.MiscAbilityData.HealPercentageOfHealth.HasValue)
				{
					num += (int)Math.Floor((float)item.OriginalMaxHealth * cAbilityHeal.MiscAbilityData.HealPercentageOfHealth.Value);
				}
				else
				{
					foreach (AbilityData.StatIsBasedOnXData item3 in cAbilityHeal.StatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.BasedOn == CAbility.EStatIsBasedOnXType.TargetHPDifference && x.AbilityStatType == CAbility.EAbilityStatType.Strength))
					{
						num += (int)Math.Round((float)Math.Abs(item.OriginalMaxHealth - item.Health) * item3.Multiplier);
					}
					foreach (AbilityData.StatIsBasedOnXData item4 in cAbilityHeal.StatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.BasedOn == CAbility.EStatIsBasedOnXType.TargetDamageTaken && x.AbilityStatType == CAbility.EAbilityStatType.Strength))
					{
						num += (int)Math.Round((float)Math.Abs(item.MaxHealth - item.Health) * item4.Multiplier);
					}
				}
			}
			actorBehaviour.m_WorldspacePanelUI.OnSelectingHealFocus(num);
		}
	}

	private string AttackModifierContainsDamageMod(string modifier)
	{
		if (modifier.Contains("x0"))
		{
			return string.Format("<b><color=#A050E0>{0}</color></b>", "x 0 ");
		}
		if (modifier.Contains("-2"))
		{
			return string.Format("<b><color=#A050E0>{0}</color></b>", "- 2 ");
		}
		if (modifier.Contains("-1"))
		{
			return string.Format("<b><color=#A050E0>{0}</color></b>", "- 1 ");
		}
		if (modifier.Contains("+0"))
		{
			return string.Format("<b><color=#FFA414>{0}</color></b>", "+ 0 ");
		}
		if (modifier.Contains("+1"))
		{
			return string.Format("<b><color=#FFA414>{0}</color></b>", "+ 1 ");
		}
		if (modifier.Contains("+2"))
		{
			return string.Format("<b><color=#FFA414>{0}</color></b>", "+ 2 ");
		}
		if (modifier.Contains("+3"))
		{
			return string.Format("<b><color=#FFA414>{0}</color></b>", "+ 3 ");
		}
		if (modifier.Contains("+4"))
		{
			return string.Format("<b><color=#FFA414>{0}</color></b>", "+ 4 ");
		}
		if (modifier.Contains("x2"))
		{
			return string.Format("<b><color=#D92727>{0}</color></b>", "x 2 ");
		}
		return "";
	}

	public void EndGame(EResult result)
	{
		EndGame(result, null);
	}

	public void EndGame(EResult result, Action onFinish)
	{
		SaveData.Instance.EndScenario(result);
		ScenarioRuleClient.Stop();
		AudioController.StopAmbienceSound();
		SceneController.Instance.ShowLoadingScreen();
		Main.Unpause3DWorld(forceUnpause: true);
		StartCoroutine(EndGameCoroutine(result, onFinish));
	}

	public void EndGameAndSkipTutorial()
	{
		SaveData.Instance.EndScenario(EResult.Win);
		SaveData.Instance.SkipTutorial();
		ScenarioRuleClient.Stop();
		AudioController.StopAmbienceSound();
		SceneController.Instance.ShowLoadingScreen();
		Main.Unpause3DWorld(forceUnpause: true);
		StartCoroutine(EndGameCoroutine(EResult.Win));
	}

	public IEnumerator EndGameCoroutine(EResult result, Action onUnloaded = null)
	{
		SaveData.Instance.Global.StopSpeedUp();
		yield return SaveData.Instance.EndScenarioThreaded(result);
		if (FFSNetwork.IsOnline)
		{
			ControllableRegistry.AllControllables.ForEach(delegate(NetworkControllable x)
			{
				x.ClearScenarioState();
			});
		}
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		yield return UnityGameEditorRuntime.UnloadScenario();
		onUnloaded?.Invoke();
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			PlayerRegistry.StartWaitingForPlayers();
			SceneController.Instance.CampaignResume(SaveData.Instance.Global.CampaignData, isJoiningMPClient: false);
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
		{
			PlayerRegistry.StartWaitingForPlayers();
			SceneController.Instance.GuildmasterResume(SaveData.Instance.Global.AdventureData, isJoiningMPClient: false, regenerateMonsterCards: false);
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial)
		{
			MainMenuUIManager.SetLoadingCompleteCallback(SceneController.Instance.OnFinishedLoadToFrontEndTutorialMenu);
			SceneController.Instance.LoadMainMenu();
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.MainMenu || SaveData.Instance.Global.GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.Autotest)
		{
			SceneController.Instance.LoadMainMenu();
		}
		else
		{
			Debug.LogError("Unsupported Mode set " + SaveData.Instance.Global.GameMode);
			SceneController.Instance.LoadMainMenu();
		}
		if (!string.IsNullOrEmpty(GlobalSettings.Instance.m_MainMenuMusicPlaylist))
		{
			AudioController.PlayMusicPlaylist(GlobalSettings.Instance.m_MainMenuMusicPlaylist);
		}
		else
		{
			AudioController.StopMusic();
		}
	}

	private void LogConditionApplied(CActor applyingActor, CAbility ability, List<CActor> actorsAppliedTo)
	{
		SEventAbilityCondition sEventAbilityCondition = SEventLog.FindLastConditionAbilityEventOfAbilityType(ability.AbilityType, ESESubTypeAbility.None, checkQueue: true);
		if (sEventAbilityCondition == null)
		{
			return;
		}
		string arg = LocalizationManager.GetTranslation(applyingActor.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(applyingActor);
		foreach (CActor item in actorsAppliedTo)
		{
			if (sEventAbilityCondition.AbilityType != CAbility.EAbilityType.AddActiveBonus)
			{
				string text = LocalizationManager.GetTranslation(sEventAbilityCondition.AbilityType.ToString()) + " <sprite name=" + HelperTools.GetCorrectSprite(sEventAbilityCondition.AbilityType) + ">";
				List<CCondition.ENegativeCondition> list = ability.NegativeConditions.Keys.ToList();
				for (int i = 0; i < list.Count; i++)
				{
					text += ((i == list.Count - 1) ? (" " + LocalizationManager.GetTranslation("and") + " ") : ", ");
					text = text + LocalizationManager.GetTranslation(list[i].ToString()) + " <sprite name=" + list[i].ToString() + ">";
				}
				List<CCondition.EPositiveCondition> list2 = ability.PositiveConditions.Keys.ToList();
				for (int j = 0; j < list2.Count; j++)
				{
					text += ((j == list2.Count - 1) ? (" " + LocalizationManager.GetTranslation("and") + " ") : ", ");
					text = text + LocalizationManager.GetTranslation(list2[j].ToString()) + " <sprite name=" + list2[j].ToString() + ">";
				}
				string arg2 = LocalizationManager.GetTranslation(item.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(item);
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_APPLY_ABILITY"), arg, $"<b>{text}</b>", arg2), CombatLogFilter.CONDITIONS);
			}
		}
	}

	private void LogDamageCalculation(SEventAbilityAttack attackAbilityEvent, SEventAttackModifier attackModEvent, CActor attackingActor, CActor actorBeingAttacked, CAbility ability)
	{
		try
		{
			CAttackSummary.TargetSummary targetSummary = null;
			List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
			StringBuilder stringBuilder = new StringBuilder();
			if (ability is CAbilityAttack cAbilityAttack)
			{
				CAttackSummary cAttackSummary = cAbilityAttack.AttackSummary.Copy();
				int targetIndex = 0;
				targetSummary = cAttackSummary.FindTarget(actorBeingAttacked, ref targetIndex);
				if (attackAbilityEvent.AttackEffects != null && attackAbilityEvent.AttackEffects.Count > 0)
				{
					foreach (CAttackEffect attackEffect in attackAbilityEvent.AttackEffects)
					{
						attackEffect.GetBonus(actorBeingAttacked, cAbilityAttack, out var attackBuff, out var _);
						if (attackBuff > 0)
						{
							stringBuilder.Append(" + ");
							stringBuilder.Append(string.Format("<b><color=#f76767>{0}</color></b>", "<sprite name=Attack> " + attackBuff));
						}
					}
				}
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			if (attackAbilityEvent.AddedNegativeConditionData.Count > 0)
			{
				foreach (SEventAbility.SEventAbilityAddedConditionData addedNegativeConditionDatum in attackAbilityEvent.AddedNegativeConditionData)
				{
					bool flag = false;
					if (attackModEvent.ActedOnNegativeConditions.Count() > 0)
					{
						foreach (NegativeConditionPair actedOnNegativeCondition in attackModEvent.ActedOnNegativeConditions)
						{
							CCondition.ENegativeCondition negativeCondition = actedOnNegativeCondition.NegativeCondition;
							if (addedNegativeConditionDatum.AddedConditionType.ToString() == negativeCondition.ToString())
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						stringBuilder2.Append(" + ");
						stringBuilder2.Append(string.Format("<b>{0}</b>", "<sprite name=" + addedNegativeConditionDatum.AddedConditionType.ToString() + ">"));
					}
				}
			}
			if (attackModEvent.NegativeConditionsFromModifiers.Count > 0)
			{
				foreach (CCondition.ENegativeCondition negativeConditionsFromModifier in attackModEvent.NegativeConditionsFromModifiers)
				{
					if (!list.Contains(negativeConditionsFromModifier))
					{
						stringBuilder2.Append(" + ");
						stringBuilder2.Append(string.Format("<b>{0}</b>", "<sprite name=" + negativeConditionsFromModifier.ToString() + "> "));
						list.Add(negativeConditionsFromModifier);
					}
				}
			}
			if (attackModEvent.OverrideBuffs.Count() > 0)
			{
				foreach (SEventAttackModifier.OverrideBuffData overrideBuff2 in attackModEvent.OverrideBuffs)
				{
					if (overrideBuff2.OverrideNegativeConditions.Count() <= 0)
					{
						continue;
					}
					foreach (CCondition.ENegativeCondition overrideNegativeCondition in overrideBuff2.OverrideNegativeConditions)
					{
						if (!list.Contains(overrideNegativeCondition))
						{
							stringBuilder2.Append(" + ");
							stringBuilder2.Append(string.Format("<b>{0}</b>", "<sprite name=" + overrideNegativeCondition.ToString() + ">"));
							list.Add(overrideNegativeCondition);
						}
					}
				}
			}
			if (attackModEvent.TargetActorWasPoisoned)
			{
				stringBuilder.Append(" + ");
				stringBuilder.Append(string.Format("<b><color=#f76767>{0}</color></b>", "<sprite name=Poison> " + 1));
			}
			int num = 0;
			if (attackModEvent.OverrideBuffs.Count > 0)
			{
				foreach (SEventAttackModifier.OverrideBuffData overrideBuff3 in attackModEvent.OverrideBuffs)
				{
					if (overrideBuff3.OverrideStrength != 0)
					{
						num += ((overrideBuff3.OverrideBuffType == SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Item) ? overrideBuff3.OverrideStrength : 0);
						stringBuilder.Append(" " + ((overrideBuff3.OverrideStrength < 0) ? "-" : "+") + " ");
						stringBuilder.Append(string.Format("<b><color=#f76767>{0}</color></b>", "<sprite name=Attack> " + overrideBuff3.OverrideStrength));
					}
				}
			}
			string text = LocalizationManager.GetTranslation(attackingActor.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(attackingActor);
			string text2 = LocalizationManager.GetTranslation(actorBeingAttacked.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actorBeingAttacked);
			for (int i = 0; i < attackModEvent.OverrideBuffs.Count; i++)
			{
				SEventAttackModifier.OverrideBuffData overrideBuff = attackModEvent.OverrideBuffs[i];
				if (!overrideBuff.LogFullName || overrideBuff.OverrideStrength == 0)
				{
					continue;
				}
				string text3 = ((overrideBuff.OverrideStrength > 0) ? "+" : "-");
				string arg = string.Empty;
				if (overrideBuff.OverrideSourceActorClassID != null && ScenarioManager.Scenario.Enemies.Any((CEnemyActor x) => x.Class.ID == overrideBuff.OverrideSourceActorClassID))
				{
					CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.FirstOrDefault((CEnemyActor x) => x.Class.ID == overrideBuff.OverrideSourceActorClassID);
					if (cEnemyActor != null)
					{
						arg = LocalizationManager.GetTranslation(cEnemyActor.ActorLocKey());
					}
				}
				else
				{
					arg = LocalizationManager.GetTranslation(overrideBuff.OverrideBuffName, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true) ?? overrideBuff.OverrideBuffName;
				}
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ATTACK_OVERRIDE_BUFF"), text, string.Format("<b><color=#f76767>{0}</color></b>", text3 + overrideBuff.OverrideStrength + "<sprite name=Attack>"), $"<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{arg}</color></b></font>"));
			}
			Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ATTACK"), text, text2, string.Format("<b>{0}</b>", LocalizationManager.GetTranslation("Attack") + " <sprite name=Attack> " + attackModEvent.AttackAbilityAttackStrength)), CombatLogFilter.ABILITIES);
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			for (int num2 = 0; num2 < attackModEvent.UsedAttackModifierStrings.Count; num2++)
			{
				stringBuilder3.Append(attackModEvent.UsedAttackModifierStrings[num2]);
				if (num2 != attackModEvent.UsedAttackModifierStrings.Count - 1)
				{
					stringBuilder3.Append(", ");
				}
			}
			for (int num3 = 0; num3 < attackModEvent.UnusedAttackModifierStrings.Count; num3++)
			{
				stringBuilder4.Append(attackModEvent.UnusedAttackModifierStrings[num3]);
				if (num3 != attackModEvent.UnusedAttackModifierStrings.Count - 1)
				{
					stringBuilder4.Append(", ");
				}
			}
			if (attackModEvent.UnusedAttackModifierStrings.Count > 0)
			{
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DRAW_ATTACK_MODIFIERS"), text, stringBuilder3.ToString(), stringBuilder4.ToString()), CombatLogFilter.MODIFIERS);
			}
			else
			{
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DRAW_ATTACK_MODIFIER"), text, stringBuilder3.ToString()), CombatLogFilter.MODIFIERS);
			}
			for (int num4 = 0; num4 < attackModEvent.UsedAttackModifierStrings.Count; num4++)
			{
				if (attackModEvent.UsedAttackModifierStringsModified[num4] != "")
				{
					Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_CHANGE_MODIFIER"), attackModEvent.UsedAttackModifierStrings[num4], attackModEvent.UsedAttackModifierStringsModified[num4]), CombatLogFilter.MODIFIERS);
				}
			}
			if (attackModEvent.TargetActorWasInvulnerable)
			{
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_INVULNERABLE"), text2), CombatLogFilter.DAMAGE);
			}
			StringBuilder stringBuilder5 = new StringBuilder();
			stringBuilder5.Append("(");
			if (stringBuilder.Length > 0)
			{
				stringBuilder5.Append("(");
			}
			stringBuilder5.Append("<sprite name=Attack> " + (attackModEvent.AttackAbilityAttackStrength - num));
			if (targetSummary != null && targetSummary.AttackAbilityScalar > 1)
			{
				stringBuilder5.Append(" x " + targetSummary.AttackAbilityScalar);
			}
			stringBuilder5.Append(stringBuilder.ToString());
			if (stringBuilder.Length > 0)
			{
				stringBuilder5.Append(")");
			}
			if (targetSummary != null && targetSummary.AttackScalar > 1)
			{
				stringBuilder5.Append(" x " + targetSummary.AttackScalar);
			}
			stringBuilder5.Append(" ");
			for (int num5 = 0; num5 < attackModEvent.UsedAttackModifierStrings.Count; num5++)
			{
				string value = AttackModifierContainsDamageMod((attackModEvent.UsedAttackModifierStringsModified[num5] == "") ? attackModEvent.UsedAttackModifierStrings[num5] : attackModEvent.UsedAttackModifierStringsModified[num5]);
				stringBuilder5.Append(value);
			}
			if (attackModEvent.FinalPierce > 0)
			{
				if (attackModEvent.FinalPierce >= 99999)
				{
					stringBuilder5.Append(" + <sprite name=IgnoreShield>");
				}
				else
				{
					stringBuilder5.Append(" + <sprite name=Pierce> " + attackModEvent.FinalPierce);
				}
			}
			if (attackModEvent.TargetActorShield > 0 && attackModEvent.FinalPierce < 99999)
			{
				stringBuilder5.Append(" - <sprite name=Shield> " + attackModEvent.TargetActorShield);
			}
			stringBuilder5.Append(" = ");
			stringBuilder5.Append(string.Format("<b><color=#f76767>{0}</color></b>", "<sprite name=Attack> " + Math.Max(0, attackModEvent.FinalAttackStrength)));
			stringBuilder5.Append(")");
			stringBuilder5.Append(stringBuilder2.ToString());
			Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_DAMAGE"), text, string.Format("<b><color=#f76767>{0}</color></b>", "<sprite name=Attack> " + Math.Max(0, attackModEvent.FinalAttackStrength)), $"<b><color=#BBBBBB>{stringBuilder5.ToString()}</color></b>", text2), CombatLogFilter.DAMAGE);
			foreach (ElementInfusionBoardManager.EElement elementsInfusedFromModifier in attackModEvent.ElementsInfusedFromModifiers)
			{
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_INFUSE_ELEMENT"), LocalizationManager.GetTranslation("GUI_ELEMENT_" + elementsInfusedFromModifier.ToString().ToUpper()) + " <sprite name=" + elementsInfusedFromModifier.ToString() + ">"), CombatLogFilter.INFUSIONS);
			}
			StringBuilder stringBuilder6 = new StringBuilder();
			if (attackModEvent.PositiveConditionsFromModifiers.Count <= 0)
			{
				return;
			}
			foreach (CCondition.EPositiveCondition positiveConditionsFromModifier in attackModEvent.PositiveConditionsFromModifiers)
			{
				stringBuilder6.Append("<sprite name=" + positiveConditionsFromModifier.ToString() + "> ");
			}
			Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_APPLY_ABILITY"), text, stringBuilder6, text), CombatLogFilter.CONDITIONS);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while processing LogDamageCalculation\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00215", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void LogActiveAbility(CActor actor, CAbility ability)
	{
		CActiveBonus cActiveBonus = actor.FindCardWithAbility(ability)?.ActiveBonuses.Find((CActiveBonus x) => x.Ability.ID == ability.ID && x.Actor == actor);
		if (cActiveBonus != null && cActiveBonus.Layout != null && cActiveBonus.Layout.ListLayouts.Count > 0)
		{
			string text = CreateLayout.LocaliseText(cActiveBonus.Layout.ListLayouts.FirstOrDefault());
			text = (text.Contains('*'.ToString()) ? CardLayoutRow.ReplaceKey(text, '*', cActiveBonus.Ability.Strength.ToString()) : text);
			if (text != null)
			{
				string arg = LocalizationManager.GetTranslation(actor.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actor);
				string text2 = (cActiveBonus.Layout.CardName.IsNOTNullOrEmpty() ? (LocalizationManager.GetTranslation(cActiveBonus.Layout.CardName) + " - ") : "");
				Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_ACTIVE_ABILITY"), arg, $"<b>{text2 + text}</b>"), CombatLogFilter.ABILITIES);
			}
		}
	}

	public void LogInfusedElements()
	{
		foreach (var item in m_ElementsInfusedThisAbility)
		{
			Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_INFUSING_ELEMENT"), LocalizationManager.GetTranslation(item.actor), LocalizationManager.GetTranslation("GUI_ELEMENT_" + item.element) + " <sprite name=" + item.element + ">"), CombatLogFilter.INFUSIONS);
		}
		m_ElementsInfusedThisAbility.Clear();
	}

	public void LogConsumes(CActor actor, CActor.EType actorType, CAbility ability, string abilityName, string consumed, string noconsume, string consumeability = "COMBAT_LOG_CONSUME_ABILITY", string noconsumeability = "COMBAT_LOG_CONSUME_ABILITY_NOELEMENT", string nostrength = "COMBAT_LOG_CONSUME", bool consumeUnknown = true)
	{
		if (actorType != CActor.EType.Player)
		{
			return;
		}
		string arg = LocalizationManager.GetTranslation(actor.ActorLocKey()) + GetActorIDForCombatLogIfNeeded(actor);
		SEventAction sEventAction = SEventLog.FindLastActionEventOfSubType(ESESubTypeAction.ActionPerform);
		if (sEventAction == null || sEventAction.ActionAugmentationDatas.Count <= 0)
		{
			return;
		}
		foreach (SEventAction.SEventActionAugmentationData actionAugmentationData in sEventAction.ActionAugmentationDatas)
		{
			foreach (SEventAction.SEventActionAugmentationOpData augmentationOpData in actionAugmentationData.AugmentationOpDatas)
			{
				string text = "";
				if (augmentationOpData.AbilityOverrideData.PositiveConditions != null && augmentationOpData.AbilityOverrideData.PositiveConditions.Count > 0)
				{
					foreach (CCondition.EPositiveCondition positiveCondition in augmentationOpData.AbilityOverrideData.PositiveConditions)
					{
						text = text + "<sprite name=" + positiveCondition.ToString() + ">";
					}
				}
				if (augmentationOpData.AbilityOverrideData.NegativeConditions != null && augmentationOpData.AbilityOverrideData.NegativeConditions.Count > 0)
				{
					foreach (CCondition.ENegativeCondition negativeCondition in augmentationOpData.AbilityOverrideData.NegativeConditions)
					{
						text = text + "<sprite name=" + negativeCondition.ToString() + ">";
					}
				}
				if (augmentationOpData.AbilityOverrideData.NumberOfTargets.HasValue && augmentationOpData.AbilityOverrideData.NumberOfTargets > 0)
				{
					text = text + "+" + augmentationOpData.AbilityOverrideData.NumberOfTargets + "<sprite name=Target>";
				}
				bool flag = false;
				int num = (augmentationOpData.AbilityOverrideData.Strength.HasValue ? augmentationOpData.AbilityOverrideData.Strength.Value : 0);
				CAbility.EAbilityType eAbilityType = (augmentationOpData.AbilityOverrideData.AbilityType.HasValue ? augmentationOpData.AbilityOverrideData.AbilityType.Value : CAbility.EAbilityType.None);
				if (!(abilityName == "Heal"))
				{
					if (abilityName == "Targeting")
					{
						if (augmentationOpData.AbilityOverrideData != null && num > 0 && eAbilityType == ability.AbilityType)
						{
							flag = true;
						}
					}
					else if (num > 0)
					{
						flag = true;
					}
				}
				else if (augmentationOpData.AbilityOverrideData != null && num > 0 && eAbilityType == CAbility.EAbilityType.Heal)
				{
					flag = true;
				}
				if (flag)
				{
					string text2 = abilityName;
					if (abilityName == "Targeting")
					{
						text2 = eAbilityType.ToString();
					}
					if (!(text2 != CAbility.EAbilityType.None.ToString()))
					{
						continue;
					}
					if (actionAugmentationData.ElementsConsumed.Count > 0)
					{
						string text3 = string.Empty;
						foreach (ElementInfusionBoardManager.EElement item in actionAugmentationData.ElementsConsumed)
						{
							if (!text3.IsNullOrEmpty())
							{
								text3 = text3 + " " + LocalizationManager.GetTranslation("and") + " ";
							}
							text3 += string.Format("<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{0}</color></b></font>", LocalizationManager.GetTranslation("GUI_ELEMENT_" + item.ToString().ToUpper()));
						}
						if (augmentationOpData.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation(consumed), arg, text3, string.Format("<b>{0}</b>", ((num >= 0) ? "+" : "-") + num + " " + LocalizationManager.GetTranslation(text2) + " <sprite name=" + text2 + "> " + text)), CombatLogFilter.ABILITIES);
						}
						else
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation(consumeability), arg, text3, string.Format("<b>{0}</b>", LocalizationManager.GetTranslation(text2) + " <sprite name=" + text2 + ">" + num)), CombatLogFilter.ABILITIES);
						}
					}
					else if (augmentationOpData.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation(noconsume), arg, string.Format("<b>{0}</b>", ((num >= 0) ? "+" : "-") + num + " " + LocalizationManager.GetTranslation(text2) + " <sprite name=" + text2 + "> " + text)), CombatLogFilter.ABILITIES);
					}
					else
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation(noconsumeability), arg, string.Format("<b>{0}</b>", LocalizationManager.GetTranslation(text2) + " <sprite name=" + text2 + ">" + num)), CombatLogFilter.ABILITIES);
					}
				}
				else
				{
					if (!consumeUnknown)
					{
						continue;
					}
					string text4 = string.Empty;
					if (actionAugmentationData.ElementsConsumed.Count <= 0)
					{
						continue;
					}
					foreach (ElementInfusionBoardManager.EElement item2 in actionAugmentationData.ElementsConsumed)
					{
						if (!text4.IsNullOrEmpty())
						{
							text4 = text4 + " " + LocalizationManager.GetTranslation("and") + " ";
						}
						text4 += string.Format("<font=\"MarcellusSC-Regular SDF\"><b><color=#f3ddab>{0}</color></b></font>", LocalizationManager.GetTranslation("GUI_ELEMENT_" + item2.ToString().ToUpper()));
					}
					bool flag2 = false;
					if (abilityName == "Move")
					{
						SEventAction.SEventAbilityOverrideData abilityOverrideData = augmentationOpData.AbilityOverrideData;
						if (abilityOverrideData != null && abilityOverrideData.Jump == true)
						{
							Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation(consumed), arg, text4, string.Format("<b>{0}</b>", LocalizationManager.GetTranslation("Jump") + " <sprite name=Jump>")), CombatLogFilter.ABILITIES);
							flag2 = true;
						}
					}
					if (!flag2)
					{
						Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation(nostrength), arg, text4), CombatLogFilter.ABILITIES);
					}
				}
			}
		}
	}

	public void OnSwitchHand(CPlayerActor cPlayer)
	{
		foreach (GameObject clientPlayer in m_ClientPlayers)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(clientPlayer);
			actorBehaviour.m_WorldspacePanelUI.Focus(cPlayer == actorBehaviour.Actor);
		}
		Singleton<UIScenarioMultiplayerController>.Instance.ShowPlayerInfo(cPlayer);
	}

	public void OnEnemyHexHighlighted(List<CEnemyActor> enemies)
	{
	}

	public void OnEnemyHexHighlighted(CEnemyActor enemyActor)
	{
	}

	public void OnActorsHexSelected(List<CActor> actors = null)
	{
		foreach (GameObject clientActorObject in ClientActorObjects)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(clientActorObject);
			if (actors != null && actors.Contains(actorBehaviour.Actor))
			{
				actorBehaviour.m_WorldspacePanelUI.HighlightSelected();
			}
			else
			{
				actorBehaviour.m_WorldspacePanelUI.UnhighlightSelected();
			}
		}
	}

	public void OnActorsHexTargeted<T>(List<T> actors, bool isPositive = false, bool untargetOthers = true) where T : CActor
	{
		foreach (GameObject clientActorObject in ClientActorObjects)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(clientActorObject);
			if (actors.Contains(actorBehaviour.Actor))
			{
				actorBehaviour.m_WorldspacePanelUI.HighlightPreview(isPositive);
			}
			else if (untargetOthers)
			{
				actorBehaviour.m_WorldspacePanelUI.UnhighlightPreview();
			}
		}
	}

	public void OnActorsHexTargeted<T>(List<Tuple<T, bool>> actors, bool untargetOthers = true) where T : CActor
	{
		foreach (GameObject clientActorObject in ClientActorObjects)
		{
			ActorBehaviour behavior = ActorBehaviour.GetActorBehaviour(clientActorObject);
			Tuple<T, bool> tuple = actors.FirstOrDefault((Tuple<T, bool> it) => it.Item1 == behavior.Actor);
			if (tuple != null)
			{
				behavior.m_WorldspacePanelUI.HighlightPreview(tuple.Item2);
			}
			else if (untargetOthers)
			{
				behavior.m_WorldspacePanelUI.UnhighlightPreview();
			}
		}
	}

	public void OnActorHexTargeted(CActor enemyActor, bool isPositive = false)
	{
		foreach (GameObject clientActorObject in ClientActorObjects)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(clientActorObject);
			if (enemyActor == actorBehaviour.Actor)
			{
				actorBehaviour.m_WorldspacePanelUI.HighlightPreview(isPositive);
			}
			else
			{
				actorBehaviour.m_WorldspacePanelUI.UnhighlightPreview();
			}
		}
	}

	public static string GetActorIDForCombatLogIfNeeded(CActor actor)
	{
		string result = string.Empty;
		if (!(actor is CHeroSummonActor))
		{
			CEnemyActor enemyActor = actor as CEnemyActor;
			if (enemyActor == null || (enemyActor.MonsterClass.Boss && ScenarioManager.Scenario.AllEnemyMonsters.Count((CEnemyActor x) => x.MonsterClass == enemyActor.MonsterClass) <= 1))
			{
				goto IL_0076;
			}
		}
		result = " (" + actor.ID + ")";
		goto IL_0076;
		IL_0076:
		return result;
	}

	public void HideSelectionUI()
	{
		if (Singleton<UIUseItemsBar>.Instance != null)
		{
			Singleton<UIUseItemsBar>.Instance.Hide();
		}
		if (Singleton<UIActiveBonusBar>.Instance != null)
		{
			Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
		}
		if (Singleton<UIUseAbilitiesBar>.Instance != null)
		{
			Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
		}
		if (Singleton<UIUseAugmentationsBar>.Instance != null)
		{
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
		}
	}

	private void FocusWorld()
	{
		if (!ControllerInputAreaManager.IsFocusedAnyArea(EControllerInputAreaType.ErrorMessage, EControllerInputAreaType.EscMenu, EControllerInputAreaType.GlobalConfirmation, EControllerInputAreaType.Tutorial, EControllerInputAreaType.StoryBox) && !Singleton<ESCMenu>.Instance.IsOpen)
		{
			ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.WorldMap);
		}
	}

	private void TryActivateWaitForConfirmHelpBox()
	{
		if (FFSNetwork.IsOnline && !IsShowedHelpBox && m_CurrentActor is CPlayerActor)
		{
			IsShowedHelpBox = true;
			Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_WAIT_PLAYERS_CONFIRM_TIP"), null, HelpBox.FormatTarget.NONE, "GUI_WAIT_PLAYERS_CONFIRM_TIP");
		}
	}

	private void SetActiveAnimationCharacters(bool isActive, bool onlyForMovedCharacters = false)
	{
		foreach (GameObject clientActorObject in ClientActorObjects)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(clientActorObject);
			bool flag = !onlyForMovedCharacters || actorBehaviour.IsMoving;
			if (actorBehaviour != null && flag)
			{
				actorBehaviour.PauseLoco(!isActive);
			}
		}
	}

	public void ToggleBottomButtons(bool isActive, bool longConfirmSelection)
	{
		m_SkipButton.SetInteractable(isActive);
		m_UndoButton.SetInteractable(isActive);
		if (longConfirmSelection)
		{
			readyButton.SetInteractable(isActive);
		}
		else
		{
			SetActiveSelectButton(isActive);
		}
	}

	public void ToggleSkipButton(bool isActive)
	{
		m_SkipButton.Toggle(isActive);
	}

	public void ToggleUndoButton(bool isActive)
	{
		m_UndoButton.Toggle(isActive);
	}

	public void EndOfTurnReachedLocally()
	{
		if (FFSNetwork.IsClient)
		{
			Synchronizer.SendSideAction(GameActionType.EndOfTurnClientReady, null, canBeUnreliable: false, sendToHostOnly: true);
			SimpleLog.AddToSimpleLog("Player " + PlayerRegistry.MyPlayer.Username + " (ID: " + PlayerRegistry.MyPlayer.PlayerID + ") reached end of turn as a Client, sending EndOfTurnClientReady to host.");
		}
		else if (!m_WaitForEndOfTurnList.Contains(PlayerRegistry.MyPlayer))
		{
			m_WaitForEndOfTurnList.Add(PlayerRegistry.MyPlayer);
			StartCoroutine(WaitForEndOfTurnComplete());
			SimpleLog.AddToSimpleLog("Player " + PlayerRegistry.MyPlayer.Username + " (ID: " + PlayerRegistry.MyPlayer.PlayerID + ") reached end of turn as Host, starting WaitForEndOfTurnComplete coroutine to acknowledge clients.");
		}
		else
		{
			Debug.LogError("Host player is already added to WaitForEndOfTurnList");
		}
	}

	private IEnumerator WaitForEndOfTurnComplete()
	{
		while (PlayerRegistry.Participants.Any((NetworkPlayer x) => m_WaitForEndOfTurnList.Find((NetworkPlayer y) => y.PlayerID == x.PlayerID) == null))
		{
			if (FFSNetwork.IsOnline)
			{
				yield return Timekeeper.instance.WaitForSeconds(0.1f);
				continue;
			}
			m_WaitForEndOfTurnList.Clear();
			TurnEndSyncedAndReadyToProceed();
			yield break;
		}
		Synchronizer.SendSideAction(GameActionType.EndOfTurnAllPlayersReady);
		m_WaitForEndOfTurnList.Clear();
		TurnEndSyncedAndReadyToProceed();
	}

	public void ServerACKClientReachingEndOfTurn(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			if (!m_WaitForEndOfTurnList.Contains(player))
			{
				m_WaitForEndOfTurnList.Add(player);
				SimpleLog.AddToSimpleLog("Host acknowledged client " + player.Username + "( ID: " + player.PlayerID + " ) reached end of turn.");
			}
			else
			{
				Debug.LogError("Player " + player.PlayerID + " is already added to WaitForEndOfTurnList");
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error determining the player who reached end of turn. NetworkPlayer returns null.");
		}
	}

	public void ClientProceedToNextTurn()
	{
		SimpleLog.AddToSimpleLog("End of turn syncing finished. All players ready. Proceeding to the next turn.");
		TurnEndSyncedAndReadyToProceed();
	}

	public void TurnEndSyncedAndReadyToProceed()
	{
		ScenarioRuleClient.EndTurnSynchronise();
		SetChoreographerState(ChoreographerStateType.Play, 0, null);
	}

	private void EndOfRoundReachedLocally()
	{
		if (FFSNetwork.IsClient)
		{
			Synchronizer.SendSideAction(GameActionType.EndOfRoundClientReady, null, canBeUnreliable: false, sendToHostOnly: true);
			SimpleLog.AddToSimpleLog("Player " + PlayerRegistry.MyPlayer.Username + " (ID: " + PlayerRegistry.MyPlayer.PlayerID + ") reached end of Round as a Client, sending EndOfRoundClientReady to host.");
		}
		else if (!m_WaitForEndOfRoundList.Contains(PlayerRegistry.MyPlayer))
		{
			m_WaitForEndOfRoundList.Add(PlayerRegistry.MyPlayer);
			StartCoroutine(WaitForEndOfRoundComplete());
			SimpleLog.AddToSimpleLog("Player " + PlayerRegistry.MyPlayer.Username + " (ID: " + PlayerRegistry.MyPlayer.PlayerID + ") reached end of Round as Host, starting WaitForEndOfRoundComplete coroutine to acknowledge clients.");
		}
		else
		{
			Debug.LogError("Host player is already added to WaitForEndOfRoundList");
		}
	}

	private IEnumerator WaitForEndOfRoundComplete()
	{
		while (PlayerRegistry.Participants.Any((NetworkPlayer x) => m_WaitForEndOfRoundList.Find((NetworkPlayer y) => y.PlayerID == x.PlayerID) == null))
		{
			if (FFSNetwork.IsOnline)
			{
				yield return Timekeeper.instance.WaitForSeconds(0.1f);
				continue;
			}
			m_WaitForEndOfRoundList.Clear();
			RoundEndSyncedAndReadyToProceed();
			yield break;
		}
		Synchronizer.SendSideAction(GameActionType.EndOfRoundAllPlayersReady);
		m_WaitForEndOfRoundList.Clear();
		RoundEndSyncedAndReadyToProceed();
	}

	public void ServerACKClientReachingEndOfRound(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			if (!m_WaitForEndOfRoundList.Contains(player))
			{
				m_WaitForEndOfRoundList.Add(player);
				SimpleLog.AddToSimpleLog("Host acknowledged client " + player.Username + "( ID: " + player.PlayerID + " ) reached end of round.");
			}
			else
			{
				Debug.LogError("Player " + player.PlayerID + " is already added to WaitForEndOfRoundList");
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error determining the player who reached end of Round. NetworkPlayer returns null.");
		}
	}

	public void StartMPEndOfRoundCompare()
	{
		SimpleLog.AddToSimpleLog("Starting End of Round comparison.");
		if (Thread.CurrentThread != SceneController.Instance.MainThread)
		{
			m_QueuedMPStateCompare = true;
			SaveData.Instance.IsSavingData = false;
			return;
		}
		SceneController.Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.MPEndOfTurnStateCompare);
		SceneController.Instance.ShowLoadingScreen();
		if (FFSNetwork.IsClient)
		{
			GHClientCallbacks.CurrentStreamMode = GHClientCallbacks.EStreamMode.ReceivingCompareState;
			Synchronizer.RequestCustomData(DataActionType.CompareScenarioStates);
		}
		else
		{
			m_MPEndOfRoundComparisonFinishedList.Add(PlayerRegistry.MyPlayer);
			StartCoroutine(WaitForEndOfRoundCompareComplete());
		}
	}

	private IEnumerator WaitForEndOfRoundCompareComplete()
	{
		HostReadyToSendCompareState = true;
		while (m_MPEndOfRoundComparisonFinishedList.Count < PlayerRegistry.Participants.Count)
		{
			yield return Timing.WaitForSeconds(0.2f);
		}
		FFSNet.Console.Log("MPEndOfRoundComparisonFinishedList IDs: " + string.Join(",", m_MPEndOfRoundComparisonFinishedList.Select((NetworkPlayer x) => x.PlayerID)));
		FFSNet.Console.Log("PlayerRegistry.Participants IDs: " + string.Join(",", PlayerRegistry.Participants.Select((NetworkPlayer x) => x.PlayerID)));
		HostReadyToSendCompareState = false;
		Synchronizer.SendSideAction(GameActionType.EndOfRoundCompareComplete);
		m_MPEndOfRoundComparisonFinishedList.Clear();
		SceneController.Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
		SceneController.Instance.DisableLoadingScreen();
		SaveData.Instance.IsSavingData = false;
	}

	public void ServerACKClientFinishingEndOfRoundComparison(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			m_MPEndOfRoundComparisonFinishedList.Add(player);
			SimpleLog.AddToSimpleLog("Player " + player.Username + " (ID: " + player.PlayerID + ") finished the end of round state comparison.");
		}
		else
		{
			FFSNet.Console.LogWarning("Error determining the player who finished the end of round state comparison. NetworkPlayer returns null.");
		}
	}

	public void StateCompareFinished()
	{
		SimpleLog.AddToSimpleLog("End of Round state comparison completed. All players ready. Proceeding to next round.");
		SceneController.Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
		SceneController.Instance.DisableLoadingScreen();
		SaveData.Instance.IsSavingData = false;
	}

	public void ClientProceedToNextRound()
	{
		SimpleLog.AddToSimpleLog("End of Round syncing finished. All players ready. Proceeding to the next round.");
		RoundEndSyncedAndReadyToProceed();
	}

	public void RoundEndSyncedAndReadyToProceed()
	{
		ScenarioRuleClient.EndRoundSynchronise();
		SetChoreographerState(ChoreographerStateType.Play, 0, null);
	}

	public void EndOfAbilityReachedLocally()
	{
		if (FFSNetwork.IsClient)
		{
			Synchronizer.SendSideAction(GameActionType.EndOfAbilityClientReady, null, canBeUnreliable: false, sendToHostOnly: true);
			SimpleLog.AddToSimpleLog("Player " + PlayerRegistry.MyPlayer.Username + " (ID: " + PlayerRegistry.MyPlayer.PlayerID + ") reached end of ability as a Client, sending EndOfAbilityClientReady to host.");
		}
		else if (!m_WaitForEndOfAbilityList.Contains(PlayerRegistry.MyPlayer))
		{
			m_WaitForEndOfAbilityList.Add(PlayerRegistry.MyPlayer);
			StartCoroutine(WaitForEndOfAbilityComplete());
			SimpleLog.AddToSimpleLog("Player " + PlayerRegistry.MyPlayer.Username + " (ID: " + PlayerRegistry.MyPlayer.PlayerID + ") reached end of ability as Host, starting WaitForEndOfAbilityComplete coroutine to acknowledge clients.");
		}
		else
		{
			Debug.LogError("Host player is already added to WaitForEndOfAbilityList");
		}
	}

	private IEnumerator WaitForEndOfAbilityComplete()
	{
		while (PlayerRegistry.Participants.Any((NetworkPlayer x) => m_WaitForEndOfAbilityList.Find((NetworkPlayer y) => y.PlayerID == x.PlayerID) == null))
		{
			if (FFSNetwork.IsOnline)
			{
				yield return Timekeeper.instance.WaitForSeconds(0.1f);
				continue;
			}
			m_WaitForEndOfAbilityList.Clear();
			AbilityEndSyncedAndReadyToProceed();
			yield break;
		}
		Synchronizer.SendSideAction(GameActionType.EndOfAbilityAllPlayersReady);
		m_WaitForEndOfAbilityList.Clear();
		AbilityEndSyncedAndReadyToProceed();
	}

	public void ServerACKClientReachingEndOfAbility(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			if (!m_WaitForEndOfAbilityList.Contains(player))
			{
				m_WaitForEndOfAbilityList.Add(player);
				SimpleLog.AddToSimpleLog("Host acknowledged client " + player.Username + "( ID: " + player.PlayerID + " ) reached end of ability.");
			}
			else
			{
				Debug.LogError("Player " + player.PlayerID + " is already added to WaitForEndOfAbilityList");
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error determining the player who reached end of ability. NetworkPlayer returns null.");
		}
	}

	public void ClientProceedToActionSelection()
	{
		SimpleLog.AddToSimpleLog("End of ability syncing finished. All players ready. Proceeding to the next turn.");
		AbilityEndSyncedAndReadyToProceed();
	}

	public void AbilityEndSyncedAndReadyToProceed()
	{
		CEndActorAbility_MessageData cEndActorAbility_MessageData = new CEndActorAbility_MessageData(m_CurrentActor);
		cEndActorAbility_MessageData.m_IsLastAbility = LastAbility;
		ScenarioRuleClient.MessageHandler(cEndActorAbility_MessageData);
		SaveData.Instance.Global.StopSpeedUp();
	}

	private void TriggerAnyOnLongRestAddActiveBonuses(CPlayerActor playerActor)
	{
		StartCoroutine(WaitTriggerAnyOnLongRestAddActiveBonuses(playerActor));
	}

	private IEnumerator WaitTriggerAnyOnLongRestAddActiveBonuses(CPlayerActor playerActor)
	{
		CardsHandUI hand = CardsHandManager.Instance.GetHand(playerActor);
		yield return new WaitWhile(() => hand.AnimatingLostCards);
		GameState.TriggerAnyOnLongRestAddActiveBonuses();
	}

	public void ClientDebugWinScenario()
	{
		FFSNet.Console.LogInfo("HOST DEBUG ACTION: Win Scenario.");
		WinScenario();
	}

	public void ClientDebugLoseScenario()
	{
		FFSNet.Console.LogInfo("HOST DEBUG ACTION: Lose Scenario.");
		LoseScenario();
	}

	public void ClientAbandonScenario()
	{
		FFSNet.Console.LogInfo("HOST: Abandoning Scenario.");
		AbandonScenario();
	}

	private void OnSwitchedToMultiplayer()
	{
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			readyButton.Toggle(active: false);
			Singleton<UIScenarioMultiplayerController>.Instance.InitializeReadyToggleForCardSelection();
			Singleton<UIScenarioMultiplayerController>.Instance.RefreshWaitingNotifications();
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.StartOfRound);
		}
		Singleton<UIScenarioMultiplayerController>.Instance.ShowMultiPlayer();
		Unsubscribe();
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Combine(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnter));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserLeft));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
	}

	private void OnPlayerConnected(BoltConnection connection)
	{
		Debug.Log("OnPlayerConnected");
		if (SaveData.Instance.Global.m_StatsDataStorage.m_LastResult == EResult.None && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			Singleton<UIScenarioMultiplayerController>.Instance.RefreshWaitingNotifications();
			InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
			if (Singleton<UIReadyToggle>.Instance.ToggledOn && PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable x) => x.IsAlive))
			{
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			}
		}
	}

	private void OnUserEnter(UserToken userToken)
	{
		Debug.Log($"OnUserEnter: {PhaseManager.PhaseType}, ActionProcessorPhase: {ActionProcessor.CurrentPhase}");
		if (SaveData.Instance.Global.m_StatsDataStorage.m_LastResult == EResult.None && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
			if (Singleton<UIReadyToggle>.Instance.ToggledOn && PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable x) => x.IsAlive))
			{
				Debug.Log("Cancel ready up since user joined the room");
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			}
		}
	}

	private void OnPlayerJoined(NetworkPlayer player)
	{
		Debug.Log("OnPlayerJoined " + player.Username);
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			if (FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.AllPlayers.Count > 1)
			{
				InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_START_TURN", "GUI_TOOLTIP_TITLE_START_TURN");
			}
			InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
			if (Singleton<UIReadyToggle>.Instance.ToggledOn && PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable x) => x.IsAlive))
			{
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			}
			Singleton<UIScenarioMultiplayerController>.Instance.RefreshWaitingNotifications();
			UIMultiplayerNotifications.ShowPlayerJoined(player);
		}
		Singleton<UIScenarioMultiplayerController>.Instance.RefreshConnectionStatus(player);
	}

	private void OnPlayerLeft(NetworkPlayer player)
	{
		Debug.Log("OnPlayerLeft " + player.Username);
		if ((PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest || ActionProcessor.CurrentPhase == ActionPhaseType.ScenarioEnded) && FFSNetwork.IsHost && Singleton<UIReadyToggle>.Instance.ToggledOn)
		{
			Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
		}
		Singleton<UIMultiplayerEscSubmenu>.Instance.Refresh();
		Singleton<UIScenarioMultiplayerController>.Instance.RefreshWaitingNotifications();
		Singleton<UIScenarioMultiplayerController>.Instance.OnPlayerLeft();
		UIMultiplayerNotifications.ShowPlayerDisconnected(player, PlayerRegistry.AllPlayers.Count == 1);
	}

	private void OnJoiningUserLeft(BoltConnection connection)
	{
		Debug.Log("OnJoiningUserLeft");
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			Singleton<UIScenarioMultiplayerController>.Instance.RefreshWaitingNotifications();
		}
	}

	private void OnControllableOwnershipChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.IsStartingUp)
		{
			return;
		}
		if (Singleton<TakeDamagePanel>.Instance.IsOpen)
		{
			if (!Singleton<TakeDamagePanel>.Instance.ThisPlayerHasTakeDamageControl)
			{
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.TakeDamageConfirmation, savePreviousState: true);
			}
			else
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.TakeDamageConfirmation);
			}
		}
		FFSNet.Console.LogInfo("Scenario: OnControllableOwnershipChanged");
		if (ActionProcessor.CurrentPhase == ActionPhaseType.StartOfRound && PlayerRegistry.MyPlayer != null)
		{
			if (FFSNetwork.IsHost)
			{
				if (PlayerRegistry.MyPlayer != oldController && oldController.IsParticipant && oldController.MyControllables.Any((NetworkControllable x) => x.IsAlive))
				{
					Singleton<UIReadyToggle>.Instance.WaitForPlayerBeforeProceeding(oldController);
				}
				if (PlayerRegistry.MyPlayer != newController && newController.MyControllables.Any((NetworkControllable x) => x.IsAlive))
				{
					Singleton<UIReadyToggle>.Instance.WaitForPlayerBeforeProceeding(newController);
				}
			}
			if (PlayerRegistry.MyPlayer.In(oldController, newController))
			{
				InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
				if (PlayerRegistry.MyPlayer.IsParticipant)
				{
					if (PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable x) => x.IsAlive))
					{
						if (Singleton<UIReadyToggle>.Instance.ToggledOn)
						{
							Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
						}
					}
					else
					{
						Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
					}
				}
			}
		}
		if (m_CurrentActor != null && m_CurrentActor is CPlayerActor cPlayerActor)
		{
			Singleton<UIScenarioMultiplayerController>.Instance.UpdateActorControlButtons(m_CurrentActor);
			if (controllable.ControllableObject is CharacterManager characterManager && characterManager.CharacterActor.Class.ID == cPlayerActor.Class.ID)
			{
				NetworkPlayer controller = ControllableRegistry.GetController((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? cPlayerActor.CharacterName.GetHashCode() : cPlayerActor.CharacterClass.ModelInstanceID);
				if (PlayerRegistry.MyPlayer == oldController && controller == newController)
				{
					Debug.Log("[TEST] Sending SendActionProcessorState.  MyPlayer (old controller) = " + oldController.PlayerID + " Other Player (new controller) = " + newController.PlayerID + " State = " + ActionProcessor.CurrentState.StateType);
					Synchronizer.SendSideAction(GameActionType.SendActionProcessorState, null, canBeUnreliable: false, sendToHostOnly: false, newController.PlayerID, (int)ActionProcessor.CurrentState.StateType);
				}
			}
		}
		Singleton<UIWindowManager>.Instance.HideOrShowWindows(onlyHide: true, forceHideAll: true, popUpsOnly: true);
		if (UIManager.Instance != null && UIManager.Instance.dialogPopup != null && UIManager.Instance.dialogPopup.Window.IsOpen)
		{
			UIManager.Instance.dialogPopup.Cancel();
		}
		if (Singleton<UIUseItemsBar>.Instance != null && Singleton<UIUseItemsBar>.Instance.IsShown)
		{
			Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: true);
		}
		if (Singleton<UIUseAbilitiesBar>.Instance != null && Singleton<UIUseAbilitiesBar>.Instance.IsShown)
		{
			Singleton<UIUseAbilitiesBar>.Instance.SetAbilitiesInteractable(enable: true);
		}
		if (m_UndoButton.gameObject.activeInHierarchy)
		{
			bool interactable = true;
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.CurrentPhaseAbility?.m_Ability != null)
			{
				interactable = FirstAbility && cPhaseAction.CurrentPhaseAbility.m_Ability.CanUndo;
			}
			m_UndoButton.SetInteractable(interactable);
		}
		Singleton<UIScenarioMultiplayerController>.Instance.UpdateCharacterController(controllable);
		string targetCharacterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(controllable.ID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(controllable.ID));
		UIMultiplayerNotifications.ShowPlayerControlsCharacter(newController, LocalizationManager.GetTranslation(CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == targetCharacterID).LocKey));
	}

	public void ProxyRestartRoundMessage()
	{
		s_Choreographer.SetBlockClientMessageProcessing(active: false);
		ScenarioRuleClient.MessageHandler(new CRestartRound_MessageData());
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.CancelProgress();
		}
	}

	public void RestartRound()
	{
		SimpleLog.AddToSimpleLog("User restarted the round");
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.CancelProgress();
			ControllableRegistry.AllControllables.ForEach(delegate(NetworkControllable x)
			{
				x.ResetState();
			});
		}
		SceneController.Instance.RestartScenario();
	}

	private void OnSwitchedToSinglePlayer()
	{
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		if (SaveData.Instance.Global.GameMode != EGameMode.MainMenu)
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				Singleton<UIReadyToggle>.Instance.Reset();
				readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, LocalizationManager.GetTranslation("GUI_END_SELECTION"), hideOnClick: true, glowingEffect: true);
				readyButton.SetInteractable(interactable: false);
				InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_START_TURN", "GUI_TOOLTIP_TITLE_START_TURN");
				InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
				CardsHandManager.Instance.SetHandInteractable(interactable: true);
			}
			Singleton<UIScenarioMultiplayerController>.Instance.HideMultiplayer();
			ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
			FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
		}
	}

	public void LoadProcGenScene(ScenarioRuleLibrary.ScenarioState state, bool isFirstLoad)
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		m_CurrentState = state;
		m_IsFirstLoad = isFirstLoad;
		SceneManager.sceneLoaded += OnSceneLoadedCallback;
		SceneManager.LoadSceneAsync("ProcGen", LoadSceneMode.Additive);
	}

	private void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		m_ProcGenScene = scene;
		SceneManager.sceneLoaded -= OnSceneLoadedCallback;
		StartCoroutine(CheckScenarioAssetBundlesAreLoadedCoroutine());
	}

	private void OnSceneLoadedCallbackContinued()
	{
		if (AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			CCustomLevelData currentCustomLevelData = SaveData.Instance.Global.CurrentCustomLevelData;
			GameState.ShuffleAttackModsEnabledForPlayers = currentCustomLevelData.ShuffleAttackModsEnabledForPlayers;
			GameState.ShuffleAbilityDecksEnabledForMonsters = currentCustomLevelData.ShuffleAbilityDecksEnabledForMonsters;
			GameState.ShuffleAttackModsEnabledForMonsters = currentCustomLevelData.ShuffleAttackModsEnabledForMonsters;
			GameState.RandomiseOnLoad = false;
		}
		else if (SaveData.Instance.Global.CurrentCustomLevelData != null && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			CCustomLevelData currentCustomLevelData2 = SaveData.Instance.Global.CurrentCustomLevelData;
			GameState.ShuffleAttackModsEnabledForPlayers = currentCustomLevelData2.ShuffleAttackModsEnabledForPlayers;
			GameState.ShuffleAbilityDecksEnabledForMonsters = currentCustomLevelData2.ShuffleAbilityDecksEnabledForMonsters;
			GameState.ShuffleAttackModsEnabledForMonsters = currentCustomLevelData2.ShuffleAttackModsEnabledForMonsters;
			GameState.RandomiseOnLoad = currentCustomLevelData2.RandomiseOnLoad;
		}
		else
		{
			GameState.ShuffleAttackModsEnabledForPlayers = true;
			GameState.ShuffleAbilityDecksEnabledForMonsters = true;
			GameState.ShuffleAttackModsEnabledForMonsters = true;
			GameState.RandomiseOnLoad = false;
		}
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		if (AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			AutoTestController.s_Instance.OnProcGenLoadCompleteCallback(SceneController.Instance.GetCurrentScene);
		}
		if (m_CurrentState.ScenarioType == EScenarioType.Custom)
		{
			LevelEditorController.s_Instance.LoadLevelEditorScene(m_CurrentState, m_IsFirstLoad);
			if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor && SaveData.Instance.Global.CurrentlyPlayingCustomLevel && SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnSpecificLocations && m_CurrentState.IsFirstLoad)
			{
				SetPlayerPositionsAroundEntrance(m_CurrentState);
			}
			UnityGameEditorRuntime.OnSceneLoaded(SceneManager.GetSceneByName("ProcGen"), LoadSceneMode.Single);
			if (SaveData.Instance.Global.GameMode == EGameMode.LevelEditor)
			{
				LevelEditorController.s_Instance.OnProcGenLoadCompleteCallback(SceneController.Instance.GetCurrentScene);
			}
		}
		else
		{
			GenerateProcgenLevel(m_CurrentState, m_IsFirstLoad);
			if (SaveData.Instance.Global.CurrentlyPlayingCustomLevel && SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnSpecificLocations && m_CurrentState.IsFirstLoad)
			{
				SetPlayerPositionsAroundEntrance(m_CurrentState);
			}
			UnityGameEditorRuntime.OnSceneLoaded(SceneManager.GetSceneByName("ProcGen"), LoadSceneMode.Single);
		}
	}

	private IEnumerator CheckScenarioAssetBundlesAreLoadedCoroutine()
	{
		while (AssetBundleManager.Instance.ScenarioBundlesLoading)
		{
			yield return null;
		}
		OnSceneLoadedCallbackContinued();
	}

	public List<GameObject> GenerateProcgenLevel(ScenarioRuleLibrary.ScenarioState state, bool isFirstLoad)
	{
		m_ProcGenSeed = state.Seed;
		m_FirstLoad = isFirstLoad;
		m_CurrentState = state;
		Debug.Log("ProcGen generating new level with seed: " + state.Seed);
		m_Random = new SharedLibrary.Random(state.Seed);
		m_ApparanceStyleOutput = "Apparance Style Setting Output:\n";
		m_MapAlignmentOutput = "Map Alignment Output:\n";
		m_MapSceneRoot = RoomVisibilityManager.s_Instance.Maps;
		ProceduralStyle component = m_MapSceneRoot.GetComponent<ProceduralStyle>();
		component.Biome = state.Style.Biome;
		m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Scenario Base Biome is set to " + state.Style.Biome.ToString() + "\n";
		component.SubBiome = state.Style.SubBiome;
		m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Scenario Base SubBiome is set to " + state.Style.SubBiome.ToString() + "\n";
		component.Theme = state.Style.Theme;
		m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Scenario Base Theme is set to " + state.Style.Theme.ToString() + "\n";
		component.SubTheme = state.Style.SubTheme;
		m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Scenario Base SubTheme is set to " + state.Style.SubTheme.ToString() + "\n";
		component.Tone = state.Style.Tone;
		m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Scenario Base Tone is set to " + state.Style.Tone.ToString() + "\n";
		m_PropSceneRoot = RoomVisibilityManager.s_Instance.Props;
		int num = 20;
		if (state.Maps.Count > 0)
		{
			for (int i = 0; i < num; i++)
			{
				m_MapAlignmentOutput = "Map Alignment Attempt: " + (i + 1) + "\n";
				m_MapID = 0;
				m_DungeonEntranceDoors = new List<GameObject>();
				m_DungeonExitDoors = new List<GameObject>();
				LoadMaps(state.Maps[0], state, null, isFirstLoad);
				if (m_AllMaps.Count == state.Maps.Count)
				{
					m_MapAlignmentOutput = m_MapAlignmentOutput + "Map Alignment Attempt: " + (i + 1) + " succeeded!\n";
					Debug.Log(m_MapAlignmentOutput);
					break;
				}
				if (i >= num - 1)
				{
					continue;
				}
				foreach (GameObject allMap in m_AllMaps)
				{
					UnityEngine.Object.DestroyImmediate(allMap);
				}
				m_MapAlignmentOutput += "Destroyed all maps as not all were spawned, trying again\n";
				m_AllMaps.Clear();
				Debug.Log(m_MapAlignmentOutput);
			}
			if (m_AllMaps.Count < state.Maps.Count)
			{
				foreach (CMap item in state.Maps.Where((CMap w) => !m_AllMaps.Any((GameObject a) => a.name == w.MapInstanceName)).ToList())
				{
					state.MapFailedToLoad(item);
				}
			}
			if (m_AllMaps.Count != state.Maps.Count)
			{
				throw new Exception("Invalid map layout.  Map counts not equal.");
			}
			UnityGameEditorRuntime.InitialiseScenario(state);
		}
		ClientScenarioManager.Create(state, isFirstLoad);
		if (isFirstLoad)
		{
			SimpleLog.AddToSimpleLog(m_ApparanceStyleOutput);
			SimpleLog.AddToSimpleLog(m_MapAlignmentOutput);
		}
		if (state.Maps.Count > 0)
		{
			SetupDoors(state, isFirstLoad);
			CalculateLevelFlow(state);
			if (state.ScenarioType == EScenarioType.YML && isFirstLoad)
			{
				SetPlayerPositionsAroundEntrance(state);
				PlaceMonsters(state);
				PlaceRandomProps(state);
				PlaceRandomSpawners(state);
			}
		}
		Debug.Log(m_ApparanceStyleOutput);
		return m_AllMaps;
	}

	public void SetPlayerPositionsAroundEntrance(ScenarioRuleLibrary.ScenarioState state)
	{
		foreach (PlayerState deadPlayer in state.Players.FindAll((PlayerState x) => x.IsDead))
		{
			deadPlayer.Location = new TileIndex(ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles[0][0].m_Tile.m_ArrayIndex);
			CMap cMap = state.Maps.FirstOrDefault((CMap s) => s.MapTiles.Exists((CMapTile e) => e.ArrayIndex.X == deadPlayer.Location.X && e.ArrayIndex.Y == deadPlayer.Location.Y));
			if (cMap != null)
			{
				deadPlayer.StartingMapGuid = cMap.MapGuid;
			}
		}
		List<PlayerState> list = state.Players.FindAll((PlayerState x) => !x.IsDead);
		int num = Mathf.Max(1, Mathf.RoundToInt((float)list.Count / (float)ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles.Count));
		int num2 = 0;
		foreach (List<CClientTile> possibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles)
		{
			for (int num3 = 0; num3 < num; num3++)
			{
				if (num3 < possibleStartingTile.Count)
				{
					PlayerState player = list[num2];
					player.Location = new TileIndex(possibleStartingTile[num3].m_Tile.m_ArrayIndex);
					CMap cMap2 = state.Maps.FirstOrDefault((CMap s) => s.MapTiles.Exists((CMapTile e) => e.ArrayIndex.X == player.Location.X && e.ArrayIndex.Y == player.Location.Y));
					if (cMap2 != null)
					{
						player.StartingMapGuid = cMap2.MapGuid;
					}
					num2++;
				}
				else
				{
					Debug.LogError("Not enough valid starting tiles to place all players");
					state.Players.RemoveAt(num2);
				}
				if (num2 >= list.Count)
				{
					break;
				}
			}
			if (num2 >= list.Count)
			{
				break;
			}
		}
		foreach (PlayerState player2 in list)
		{
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == player2.ClassID);
			SimpleLog.AddToSimpleLog(LocalizationManager.GetTranslation(cCharacterClass.LocKey) + " start position is: " + player2.Location.X + "," + player2.Location.Y);
		}
	}

	private void LoadMaps(CMap map, ScenarioRuleLibrary.ScenarioState state, GameObject connectingDoor, bool generateNewMap)
	{
		GameObject original = null;
		if (state.ScenarioType == EScenarioType.YML)
		{
			original = AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_mapsprocgen", "Map " + map.MapType, "mapsprocgen");
		}
		else if (state.ScenarioType == EScenarioType.Custom)
		{
			original = AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_mapsleveleditor", "Map " + map.MapType, "mapsleveleditor");
		}
		GameObject newMapRootGameObject = UnityEngine.Object.Instantiate(original, m_MapSceneRoot.transform);
		newMapRootGameObject.name = map.MapInstanceName;
		if (Singleton<ObjectCacheService>.Instance.ContainsMap(map))
		{
			Singleton<ObjectCacheService>.Instance.RemoveMap(map);
		}
		Singleton<ObjectCacheService>.Instance.AddMap(map, newMapRootGameObject);
		ApparanceMap mapInfo = newMapRootGameObject.GetComponent<ApparanceMap>();
		mapInfo.MapGuid = map.MapGuid;
		mapInfo.RoomName = map.RoomName;
		mapInfo.Tiles = UnityGameEditorRuntime.FindUnityGameObjects(newMapRootGameObject, ScenarioManager.ObjectImportType.Tile);
		newMapRootGameObject.transform.eulerAngles = Vector3.zero;
		int num = 0;
		Vector3 zero = Vector3.zero;
		foreach (GameObject tile in mapInfo.Tiles)
		{
			zero += tile.transform.position;
			num++;
		}
		zero /= (float)num;
		map.Centre = new CVector3(zero.x, zero.y, zero.z);
		GameObject gameObject = null;
		float num2 = float.MaxValue;
		foreach (GameObject tile2 in mapInfo.Tiles)
		{
			if ((tile2.transform.position - zero).magnitude < num2)
			{
				gameObject = tile2;
				num2 = (tile2.transform.position - zero).magnitude;
			}
		}
		map.ClosestTileIdentityPosition = new CVector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
		List<GameObject> doorsInRoom = UnityGameEditorRuntime.FindUnityGameObjects(newMapRootGameObject, ScenarioManager.ObjectImportType.Door);
		if (doorsInRoom.Count == 0)
		{
			throw new Exception("No doors in room.  Unable to spawn map");
		}
		if (!generateNewMap)
		{
			newMapRootGameObject.transform.position = new Vector3(map.Position.X, map.Position.Y, map.Position.Z);
			newMapRootGameObject.transform.eulerAngles = new Vector3(0f, map.Angle, 0f);
			if (connectingDoor == null && string.IsNullOrEmpty(map.DungeonEntranceDoor) && !string.IsNullOrEmpty(map.EntranceDoor))
			{
				map.DungeonEntranceDoor = map.EntranceDoor;
				map.EntranceDoor = null;
			}
			CObjectDoor doorObject = ((connectingDoor == null) ? (m_CurrentState.Props.Single((CObjectProp s) => s.InstanceName == map.DungeonEntranceDoor) as CObjectDoor) : (m_CurrentState.Props.Single((CObjectProp s) => s.InstanceName == map.EntranceDoor) as CObjectDoor));
			GameObject gameObject2 = null;
			if (connectingDoor == null)
			{
				gameObject2 = doorsInRoom.Single((GameObject s) => Vector3.Distance(GloomUtility.CVToV(doorObject.Position), s.transform.position) < 0.1f);
				doorsInRoom.Remove(gameObject2);
				gameObject2.GetComponent<UnityGameEditorDoorProp>().m_IsDungeonEntrance = true;
				m_DungeonEntranceDoors.Add(gameObject2);
				mapInfo.IsDungeonEntrance = true;
				mapInfo.DungeonEntranceDoor = gameObject2;
			}
			else
			{
				gameObject2 = connectingDoor;
				mapInfo.EntranceDoor = gameObject2;
			}
			InitNewMap(newMapRootGameObject, map);
			gameObject2.transform.SetParent(newMapRootGameObject.transform);
			gameObject2.name = doorObject.InstanceName;
			if (map.IsDungeonExitRoom)
			{
				CObjectDoor exitDoorProp = m_CurrentState.Props.Single((CObjectProp s) => s.InstanceName == map.DungeonExitDoor) as CObjectDoor;
				GameObject gameObject3 = doorsInRoom.Single((GameObject s) => Vector3.Distance(GloomUtility.CVToV(exitDoorProp.Position), s.transform.position) < 0.1f);
				gameObject3.name = exitDoorProp.InstanceName;
				doorsInRoom.Remove(gameObject3);
				gameObject3.GetComponent<UnityGameEditorDoorProp>().m_IsDungeonExit = true;
				m_DungeonExitDoors.Add(gameObject3);
				mapInfo.IsDungeonExit = true;
				mapInfo.DungeonExitDoor = gameObject3;
			}
			if (map.IsAdditionalDungeonEntranceRoom)
			{
				CObjectDoor dungeonEntranceDoorProp = m_CurrentState.Props.Single((CObjectProp s) => s.InstanceName == map.DungeonEntranceDoor) as CObjectDoor;
				GameObject gameObject4 = doorsInRoom.Single((GameObject s) => Vector3.Distance(GloomUtility.CVToV(dungeonEntranceDoorProp.Position), s.transform.position) < 0.1f);
				gameObject4.name = dungeonEntranceDoorProp.InstanceName;
				doorsInRoom.Remove(gameObject4);
				gameObject4.GetComponent<UnityGameEditorDoorProp>().m_IsDungeonEntrance = true;
				m_DungeonEntranceDoors.Add(gameObject4);
				mapInfo.IsDungeonEntrance = true;
				mapInfo.DungeonEntranceDoor = gameObject4;
			}
			if (map.Children != null && map.Children.Count > 0)
			{
				foreach (string procGenMapInputChildID in map.Children.ToList())
				{
					CMap procGenMapInputChild = state.Maps.Single((CMap x) => x.MapGuid == procGenMapInputChildID);
					CObjectDoor exitDoor = state.Props.Single((CObjectProp s) => s.InstanceName == procGenMapInputChild.EntranceDoor) as CObjectDoor;
					GameObject gameObject5 = doorsInRoom.Single((GameObject s) => Vector3.Distance(GloomUtility.CVToV(exitDoor.Position), s.transform.position) < 0.1f);
					doorsInRoom.Remove(gameObject5);
					LoadMaps(procGenMapInputChild, state, gameObject5, generateNewMap);
				}
			}
			{
				foreach (GameObject item in doorsInRoom)
				{
					if (item != null)
					{
						UnityEngine.Object.DestroyImmediate(item);
					}
				}
				return;
			}
		}
		if (FindMapAlignment(connectingDoor, ref doorsInRoom, newMapRootGameObject, map, mapInfo))
		{
			InitNewMap(newMapRootGameObject, map);
			ProceduralMapConfig component = newMapRootGameObject.GetComponent<ProceduralMapConfig>();
			if (map.Children != null && map.Children.Count > 0)
			{
				foreach (string procGenMapInputChildID2 in map.Children.ToList())
				{
					CMap map2 = state.Maps.Single((CMap x) => x.MapGuid == procGenMapInputChildID2);
					float minOptimalDistance = UnityGameEditorRuntime.s_TileSize.x * (float)component.m_OptimalExitToEntranceDistance;
					doorsInRoom = doorsInRoom.OrderByDescending(delegate(GameObject x)
					{
						float magnitude = (x.transform.position - ((mapInfo.DungeonEntranceDoor != null) ? mapInfo.DungeonEntranceDoor.transform.position : mapInfo.EntranceDoor.transform.position)).magnitude;
						return (!(magnitude > minOptimalDistance)) ? ((double)magnitude) : ((double)minOptimalDistance + m_Random.NextDouble());
					}).ToList();
					foreach (GameObject item2 in doorsInRoom.ToList())
					{
						if (!(item2 == null) && !(mapInfo.DungeonEntranceDoor == item2) && !(mapInfo.EntranceDoor == item2))
						{
							int mapID = m_MapID;
							LoadMaps(map2, state, item2, generateNewMap: true);
							if (mapID != m_MapID)
							{
								doorsInRoom.Remove(item2);
								break;
							}
						}
					}
				}
			}
			if (map.IsAdditionalDungeonEntranceRoom && !PlaceAdditionalDungeonEntranceOrExit(connectingDoor, mapInfo, ref doorsInRoom, map, isEntrance: true))
			{
				DestroyMap("Unable to find valid placement for dungeon entrance\n");
				return;
			}
			if (!map.IsDungeonExitRoom || PlaceAdditionalDungeonEntranceOrExit(connectingDoor, mapInfo, ref doorsInRoom, map))
			{
				foreach (GameObject item3 in doorsInRoom)
				{
					if (item3 != null)
					{
						UnityEngine.Object.DestroyImmediate(item3);
					}
				}
				return;
			}
			DestroyMap("Unable to find valid placement for dungeon exit\n");
		}
		else
		{
			DestroyMap("Unable to find valid map alignment for map\n");
		}
		void DestroyMap(string message)
		{
			Singleton<ObjectCacheService>.Instance.RemoveMap(map);
			m_AllMaps.Remove(newMapRootGameObject);
			UnityEngine.Object.DestroyImmediate(newMapRootGameObject);
			m_MapAlignmentOutput += message;
		}
	}

	private bool FindMapAlignment(GameObject connectingDoor, ref List<GameObject> doorsInRoom, GameObject newMapRootGameObject, CMap map, ApparanceMap mapInfo)
	{
		if (connectingDoor == null)
		{
			GameObject gameObject = (mapInfo.DungeonEntranceDoor = doorsInRoom[m_Random.Next(doorsInRoom.Count)]);
			doorsInRoom.Remove(gameObject);
			mapInfo.IsDungeonEntrance = true;
			gameObject.GetComponent<UnityGameEditorDoorProp>().m_IsDungeonEntrance = true;
			m_DungeonEntranceDoors.Add(gameObject);
			map.Angle = newMapRootGameObject.transform.eulerAngles.y;
			return true;
		}
		bool flag = false;
		float num = 0f;
		float num2 = connectingDoor.transform.eulerAngles.y + 180f;
		num2 = ((num2 >= 360f) ? (num2 - 360f) : num2);
		int numberOfDoors = doorsInRoom.Count;
		foreach (int item in from i in Enumerable.Range(0, numberOfDoors)
			select new Tuple<int, int>(m_Random.Next(numberOfDoors), i) into i
			orderby i.Item1
			select i.Item2)
		{
			GameObject gameObject3 = doorsInRoom[item];
			UnityGameEditorDoorProp component = gameObject3.GetComponent<UnityGameEditorDoorProp>();
			UnityGameEditorDoorProp component2 = connectingDoor.GetComponent<UnityGameEditorDoorProp>();
			if (component.m_DoorType != component2.m_DoorType)
			{
				continue;
			}
			for (float num3 = 0f; (int)num3 <= 360; num3 += 60f)
			{
				num = num3;
				newMapRootGameObject.transform.eulerAngles = new Vector3(0f, num3, 0f);
				newMapRootGameObject.transform.position += connectingDoor.transform.position - gameObject3.transform.position;
				Physics.SyncTransforms();
				if (Mathf.Abs(gameObject3.transform.eulerAngles.y - connectingDoor.transform.eulerAngles.y) > 5f && Mathf.Abs(gameObject3.transform.eulerAngles.y - num2) > 5f)
				{
					flag = false;
					m_MapAlignmentOutput = m_MapAlignmentOutput + "ProcGen : Cannot align door " + gameObject3.name + " on map " + map.MapType.ToString() + " to door " + connectingDoor.name + " in parent map " + map.ParentName + " with angle " + num3 + "\n";
					continue;
				}
				bool flag2 = false;
				foreach (GameObject allMap in m_AllMaps)
				{
					ApparanceMap component3 = allMap.GetComponent<ApparanceMap>();
					foreach (GameObject tile in component3.Tiles)
					{
						foreach (GameObject tile2 in mapInfo.Tiles)
						{
							MeshCollider component4 = tile2.GetComponent<MeshCollider>();
							MeshCollider component5 = tile.GetComponent<MeshCollider>();
							if (component4.bounds.Intersects(component5.bounds))
							{
								flag2 = true;
								m_MapAlignmentOutput = m_MapAlignmentOutput + "ProcGen : Tile overlap using door " + gameObject3.name + " on map " + map.MapType.ToString() + " to door " + connectingDoor.name + " in parent map " + map.ParentName + " with angle " + num3 + "\n";
								break;
							}
							if ((tile2.transform.position - tile.transform.position).magnitude < 1f)
							{
								flag2 = true;
								m_MapAlignmentOutput = m_MapAlignmentOutput + "ProcGen : Tile overlap using door " + gameObject3.name + " on map " + map.MapType.ToString() + " to door " + connectingDoor.name + " in parent map " + map.ParentName + " with angle " + num3 + "\n";
								break;
							}
							if (map.ParentName != component3.RoomName && (tile2.transform.position - tile.transform.position).magnitude < 2f)
							{
								flag2 = true;
								m_MapAlignmentOutput = m_MapAlignmentOutput + "ProcGen : Tile+wall padding overlap using door " + gameObject3.name + " on map " + map.MapType.ToString() + " to door " + connectingDoor.name + " in parent map " + map.ParentName + " with angle " + num3 + "\n";
								break;
							}
						}
						if (flag2)
						{
							break;
						}
					}
					if (flag2)
					{
						break;
					}
				}
				if (!flag2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (GameObject dungeonEntranceDoor in m_DungeonEntranceDoors)
			{
				if (doorsInRoom.Any((GameObject a) => Vector3.Distance(a.transform.position, dungeonEntranceDoor.transform.position) < 1f))
				{
					m_MapAlignmentOutput = m_MapAlignmentOutput + "ProcGen : Door overlapping with a dungeon entrace " + gameObject3.name + " on map " + map.MapType.ToString() + " to door " + connectingDoor.name + " in parent map " + map.ParentName + " with angle \n";
					return false;
				}
			}
			newMapRootGameObject.transform.eulerAngles = new Vector3(0f, num, 0f);
			map.Angle = num;
			m_MapAlignmentOutput = m_MapAlignmentOutput + "ProcGen : SUCCESS Aligned door " + gameObject3.name + " on map " + base.name + " to door " + connectingDoor.name + " in parent map " + map.ParentName + " with angle " + num + "\n";
			connectingDoor.transform.SetParent(gameObject3.transform.parent);
			mapInfo.EntranceDoor = connectingDoor;
			map.EntranceDoor = connectingDoor.name;
			return true;
		}
		return false;
	}

	private bool PlaceAdditionalDungeonEntranceOrExit(GameObject connectingDoor, ApparanceMap mapInfo, ref List<GameObject> doorsInRoom, CMap map, bool isEntrance = false)
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(doorsInRoom);
		bool flag = true;
		GameObject gameObject = null;
		while (list.Count > 0)
		{
			gameObject = list[m_Random.Next(list.Count)];
			foreach (GameObject allMap in m_AllMaps)
			{
				flag = true;
				ApparanceMap component = allMap.GetComponent<ApparanceMap>();
				if (!(component != mapInfo))
				{
					continue;
				}
				foreach (GameObject tile in component.Tiles)
				{
					if ((gameObject.transform.position - tile.transform.position).magnitude < 1f)
					{
						flag = false;
						break;
					}
					if (connectingDoor != null && (gameObject.transform.position - connectingDoor.transform.position).magnitude < 1f)
					{
						flag = false;
						break;
					}
					if ((gameObject.transform.position - tile.transform.position).magnitude < 2f)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
			list.Remove(gameObject);
		}
		if (flag)
		{
			if (isEntrance)
			{
				mapInfo.DungeonEntranceDoor = gameObject;
				mapInfo.IsDungeonEntrance = true;
				gameObject.GetComponent<UnityGameEditorDoorProp>().m_IsDungeonEntrance = true;
				map.DungeonEntranceDoor = mapInfo.DungeonEntranceDoor.name;
				m_DungeonEntranceDoors.Add(gameObject);
			}
			else
			{
				mapInfo.DungeonExitDoor = gameObject;
				mapInfo.IsDungeonExit = true;
				gameObject.GetComponent<UnityGameEditorDoorProp>().m_IsDungeonExit = true;
				map.DungeonExitDoor = mapInfo.DungeonExitDoor.name;
				m_DungeonExitDoors.Add(gameObject);
			}
			doorsInRoom.Remove(gameObject);
			return true;
		}
		return false;
	}

	private void InitNewMap(GameObject newMapRootGameObject, CMap map)
	{
		ProceduralStyle component = newMapRootGameObject.GetComponent<ProceduralStyle>();
		if (map.SelectedPossibleRoom != null)
		{
			component.Biome = map.SelectedPossibleRoom.Biome;
			m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Room " + m_MapID + " (Name: " + map.SelectedPossibleRoom.Name + " Map: " + newMapRootGameObject.name + ") has set a Biome override of " + component.Biome.ToString() + "\n";
			component.SubBiome = map.SelectedPossibleRoom.SubBiome;
			m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Room " + m_MapID + " (Name: " + map.SelectedPossibleRoom.Name + " Map: " + newMapRootGameObject.name + ") has set a SubBiome override of " + component.SubBiome.ToString() + "\n";
			component.Theme = map.SelectedPossibleRoom.Theme;
			m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Room " + m_MapID + " (Name: " + map.SelectedPossibleRoom.Name + " Map: " + newMapRootGameObject.name + ") has set a Theme override of " + component.Theme.ToString() + "\n";
			component.SubTheme = map.SelectedPossibleRoom.SubTheme;
			m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Room " + m_MapID + " (Name: " + map.SelectedPossibleRoom.Name + " Map: " + newMapRootGameObject.name + ") has set a SubTheme override of " + component.SubTheme.ToString() + "\n";
			component.Tone = map.SelectedPossibleRoom.Tone;
			m_ApparanceStyleOutput = m_ApparanceStyleOutput + "Room " + m_MapID + " (Name: " + map.SelectedPossibleRoom.Name + " Map: " + newMapRootGameObject.name + ") has set a Tone override of " + component.Tone.ToString() + "\n";
		}
		newMapRootGameObject.SetActive(value: false);
		newMapRootGameObject.SetActive(value: true);
		m_MapID++;
		map.Position = GloomUtility.VToCV(newMapRootGameObject.transform.position);
		map.Rotation = GloomUtility.VToCV(newMapRootGameObject.transform.eulerAngles);
		m_AllMaps.Add(newMapRootGameObject);
	}

	private void SetupDoors(ScenarioRuleLibrary.ScenarioState state, bool firstLoad)
	{
		foreach (GameObject allMap in m_AllMaps)
		{
			foreach (GameObject item in UnityGameEditorRuntime.FindUnityGameObjects(allMap, ScenarioManager.ObjectImportType.Door))
			{
				UnityGameEditorRuntime.MakeDoor(item, state, firstLoad);
			}
		}
		foreach (CMap map in state.Maps)
		{
			foreach (string childMapID in map.Children)
			{
				CMap cMap = state.Maps.Single((CMap s) => s.MapGuid == childMapID);
				if (firstLoad)
				{
					map.ExitDoors.Add(cMap.EntranceDoor);
				}
				if (map.Revealed || map == state.Maps[0])
				{
					Singleton<ObjectCacheService>.Instance.GetPropObject(cMap.EntranceDoor).GetComponent<UnityGameEditorDoorProp>().m_InitiallyVisable = true;
				}
			}
		}
		foreach (CObjectDoor item2 in state.Props.OfType<CObjectDoor>())
		{
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(item2);
			ApparanceLayer.Instance.Create(item2, propObject);
		}
	}

	private bool CheckValidPropLocation(List<GameObject> doors, List<GameObject> tilesInMap, Transform propTransform, bool coverageTransform)
	{
		GameObject propTileGO = tilesInMap.Find((GameObject x) => (x.transform.position - propTransform.position).magnitude < 0.1f);
		if (propTileGO == null)
		{
			return false;
		}
		if (propTileGO.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Count > 0)
		{
			return false;
		}
		if (ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles.Any((CClientTile a) => (a.m_GameObject.transform.position - propTileGO.transform.position).magnitude < 0.1f))
		{
			return false;
		}
		if (ClientScenarioManager.s_ClientScenarioManager.DungeonExitTiles.Any((CClientTile a) => (a.m_GameObject.transform.position - propTileGO.transform.position).magnitude < 0.1f))
		{
			return false;
		}
		foreach (GameObject door in doors)
		{
			float num = (door.GetComponent<UnityGameEditorDoorProp>().m_IsDungeonEntrance ? (3f * UnityGameEditorRuntime.s_TileSize.x) : 2f);
			if ((door.transform.position - propTransform.position).magnitude < num)
			{
				return false;
			}
		}
		if (coverageTransform)
		{
			Vector3 vector = new Vector3(UnityGameEditorRuntime.s_TileSize.x, 0f, 0f);
			for (float num2 = 0f; num2 < 360f; num2 += 60f)
			{
				float closestDistance;
				GameObject gameObject = FindClosestTileTo(tilesInMap, propTransform.position + Quaternion.Euler(0f, num2, 0f) * vector, out closestDistance);
				if (!(gameObject != null))
				{
					continue;
				}
				foreach (ScenarioManager.ObjectImportType item in gameObject.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile)
				{
					if (item == ScenarioManager.ObjectImportType.Coverage)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool CheckValidPropCoverageNonBlocking(GameObject instancedObstacle, List<GameObject> tilesInMap)
	{
		int num = 0;
		int num2 = 0;
		Coverage[] componentsInChildren = instancedObstacle.GetComponentsInChildren<Coverage>();
		foreach (Coverage coverageChild in componentsInChildren)
		{
			num2++;
			if (!(tilesInMap.Find((GameObject x) => (x.transform.position - coverageChild.transform.position).magnitude < 0.1f) != null))
			{
				continue;
			}
			Vector3 vector = new Vector3(UnityGameEditorRuntime.s_TileSize.x, 0f, 0f);
			for (float num3 = 0f; num3 < 360f; num3 += 60f)
			{
				if (FindClosestTileTo(tilesInMap, coverageChild.transform.position + Quaternion.Euler(0f, num3, 0f) * vector, out var closestDistance) != null && closestDistance > 0.5f)
				{
					num++;
					break;
				}
			}
		}
		return num < num2;
	}

	private void PlaceRandomProps(ScenarioRuleLibrary.ScenarioState state)
	{
		foreach (CMap map2 in state.Maps)
		{
			GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(map2);
			List<GameObject> doors = UnityGameEditorRuntime.FindUnityGameObjects(map.gameObject, ScenarioManager.ObjectImportType.Door);
			List<GameObject> list = UnityGameEditorRuntime.FindUnityGameObjects(map.gameObject, ScenarioManager.ObjectImportType.Tile);
			List<CObjectProp> list2 = map2.Props.Where((CObjectProp w) => !(w is CObjectDoor)).ToList();
			List<CObjectDoor> list3 = new List<CObjectDoor>();
			foreach (string exitDoor in map2.ExitDoors)
			{
				CObjectDoor cObjectDoor = (CObjectDoor)state.Props.Find((CObjectProp d) => d.InstanceName == exitDoor);
				if (cObjectDoor != null)
				{
					list3.Add(cObjectDoor);
				}
			}
			list2.Sort((CObjectProp x, CObjectProp cObjectProp2) => CObjectProp.PropImportanceValue(x.PropType).CompareTo(CObjectProp.PropImportanceValue(cObjectProp2.PropType)));
			m_Random.Next(list2.Count);
			for (int num = 0; num < list2.Count; num++)
			{
				CObjectProp cObjectProp = list2[num];
				GameObject propPrefab = GlobalSettings.GetPropPrefab(cObjectProp.PrefabName);
				if (propPrefab == null)
				{
					Debug.LogError("Unable to find asset for prop " + cObjectProp.PrefabName);
					continue;
				}
				float y = propPrefab.transform.eulerAngles.y;
				GameObject gameObject = UnityEngine.Object.Instantiate(propPrefab, Vector3.zero, propPrefab.transform.rotation, map.transform);
				UnityGameEditorObject component = gameObject.GetComponent<UnityGameEditorObject>();
				Singleton<ObjectCacheService>.Instance.AddProp(cObjectProp, gameObject);
				int num2 = m_Random.Next(list.Count);
				bool flag = false;
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					GameObject gameObject2 = list[num2];
					num2 = ((num2 != list.Count - 1) ? (num2 + 1) : 0);
					gameObject.transform.position = gameObject2.transform.position;
					List<Transform> list4 = new List<Transform>();
					foreach (Transform item in gameObject.transform)
					{
						if ((bool)item.GetComponent<Coverage>())
						{
							list4.Add(item);
						}
					}
					bool flag2 = list4.Count > 0;
					bool num4 = list4.Count > 1;
					float num5 = (num4 ? (m_Random.Next(6) * 60) : 0);
					float num6 = (num4 ? (num5 + 360f) : (num5 + 1f));
					for (float num7 = num5; num7 < num6; num7 += 60f)
					{
						gameObject.transform.eulerAngles = new Vector3(0f, y + num7, 0f);
						bool flag3 = true;
						foreach (Transform item2 in list4)
						{
							if (!CheckValidPropLocation(doors, list, item2, coverageTransform: true))
							{
								flag3 = false;
								break;
							}
						}
						if (!flag2)
						{
							flag3 = CheckValidPropLocation(doors, list, gameObject.transform, coverageTransform: false);
						}
						if (flag3 && flag2)
						{
							flag3 = CheckValidPropCoverageNonBlocking(gameObject, list);
						}
						if (!flag3)
						{
							continue;
						}
						gameObject2.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Add(component.m_ObjectType);
						foreach (Transform coverageChildTransform in list4)
						{
							GameObject gameObject3 = list.Find((GameObject x) => (x.transform.position - coverageChildTransform.transform.position).magnitude < 0.1f);
							if (gameObject3 != null)
							{
								gameObject3.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Add(ScenarioManager.ObjectImportType.Coverage);
							}
						}
						flag = true;
						break;
					}
					if (flag)
					{
						break;
					}
				}
				try
				{
					if (flag)
					{
						Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(gameObject.transform.position) + GloomUtility.CVIToVI(m_CurrentState.PositiveSpaceOffset);
						if (cObjectProp is CObjectObstacle)
						{
							List<TileIndex> list5 = new List<TileIndex>();
							Coverage[] componentsInChildren = gameObject.GetComponentsInChildren<Coverage>();
							for (int num8 = 0; num8 < componentsInChildren.Length; num8++)
							{
								Vector3Int vector3Int2 = MF.GetTileIntegerSnapSpace(componentsInChildren[num8].gameObject.transform.position) + GloomUtility.CVIToVI(m_CurrentState.PositiveSpaceOffset);
								list5.Add(new TileIndex(vector3Int2.x, vector3Int2.z));
							}
							(cObjectProp as CObjectObstacle).PathingBlockers = list5;
						}
						else if (cObjectProp is CObjectPressurePlate cObjectPressurePlate)
						{
							if (list3.Count > 0)
							{
								int index = m_Random.Next(list3.Count);
								CObjectDoor doorProp = list3[index];
								cObjectPressurePlate.LinkDoor(doorProp);
							}
							else
							{
								Debug.LogWarning("Procgen could not find a door to link with pressure plate " + gameObject.name);
								state.Props.Remove(cObjectProp);
							}
						}
						cObjectProp.SetLocation(new TileIndex(vector3Int.x, vector3Int.z), GloomUtility.VToCV(gameObject.transform.position), GloomUtility.VToCV(gameObject.transform.eulerAngles));
					}
					else
					{
						Debug.LogWarning("Procgen could not find a valid location for " + gameObject.name);
						state.Props.Remove(cObjectProp);
					}
					Singleton<ObjectCacheService>.Instance.RemoveProp(cObjectProp);
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error destroying obstacle '" + gameObject.name + "' : " + ex.ToString());
				}
			}
		}
	}

	private void PlaceRandomSpawners(ScenarioRuleLibrary.ScenarioState state)
	{
		foreach (CMap map2 in state.Maps)
		{
			if (map2.Spawners.Count <= 0)
			{
				continue;
			}
			GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(map2);
			UnityGameEditorRuntime.FindUnityGameObjects(map.gameObject, ScenarioManager.ObjectImportType.Door);
			List<GameObject> list = UnityGameEditorRuntime.FindUnityGameObjects(map.gameObject, ScenarioManager.ObjectImportType.Tile);
			List<CSpawner> spawners = map2.Spawners;
			List<GameObject> list2 = UnityGameEditorRuntime.FindUnityGameObjects(map.gameObject, ScenarioManager.ObjectImportType.EdgeTile);
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(string.IsNullOrEmpty(map2.DungeonEntranceDoor) ? map2.EntranceDoor : map2.DungeonEntranceDoor);
			float closestDistance;
			GameObject gameObject = FindClosestTileTo(list2, propObject.transform.position, out closestDistance);
			List<GameObject> list3 = new List<GameObject>();
			list3.Add(gameObject);
			list2.Remove(gameObject);
			GameObject gameObject2 = gameObject;
			while (list2.Count > 0)
			{
				float closestDistance2;
				GameObject gameObject3 = FindClosestTileTo(list2, gameObject2.transform.position, out closestDistance2);
				list3.Add(gameObject3);
				list2.Remove(gameObject3);
				gameObject2 = gameObject3;
			}
			foreach (CClientTile allPossibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
			{
				if (list3.Contains(allPossibleStartingTile.m_GameObject))
				{
					list3.Remove(allPossibleStartingTile.m_GameObject);
				}
			}
			int num = Mathf.RoundToInt((float)list3.Count / (float)(spawners.Count + 1));
			int num2 = num;
			int num3 = m_Random.Next(spawners.Count);
			for (int i = 0; i < spawners.Count; i++)
			{
				CSpawner cSpawner = spawners[num3];
				num3 = ((num3 + 1 < spawners.Count) ? (num3 + 1) : 0);
				int num4 = num2;
				for (int j = 0; j < list.Count; j++)
				{
					GameObject gameObject4 = list3[num4];
					GameObject obj = FindClosestTileTo(list, gameObject4.transform.position, out closestDistance);
					bool flag = true;
					if (obj == null)
					{
						flag = false;
					}
					if (obj.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Count > 0)
					{
						flag = false;
					}
					Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(obj.transform.position) + GloomUtility.CVIToVI(m_CurrentState.PositiveSpaceOffset);
					TileIndex tileIndex = new TileIndex(vector3Int.x, vector3Int.z);
					if (spawners.Any((CSpawner s) => s.ArrayIndex != null && s.ArrayIndex == tileIndex))
					{
						flag = false;
					}
					if (flag)
					{
						cSpawner.SetLocation(tileIndex);
						break;
					}
					num4 = ((num4 + 1 <= list3.Count - 1) ? (num4 + 1) : 0);
				}
				num2 = Math.Min(num2 + num, list3.Count - 1);
			}
		}
		for (int num5 = state.Spawners.Count - 1; num5 >= 0; num5--)
		{
			CSpawner cSpawner2 = state.Spawners[num5];
			if (cSpawner2.ArrayIndex == null)
			{
				state.Spawners.Remove(cSpawner2);
				Debug.LogError("Spawner removed as no random location could be found");
			}
		}
	}

	private void CalculateLevelFlow(ScenarioRuleLibrary.ScenarioState state)
	{
		foreach (CMap map2 in state.Maps)
		{
			GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(map2);
			ApparanceMap component = map.GetComponent<ApparanceMap>();
			component.Tiles = UnityGameEditorRuntime.FindUnityGameObjects(map, ScenarioManager.ObjectImportType.Tile);
			int num = 0;
			Vector3 zero = Vector3.zero;
			foreach (GameObject tile in component.Tiles)
			{
				zero += tile.transform.position;
				num++;
			}
			zero /= (float)num;
			map2.Centre = new CVector3(zero.x, zero.y, zero.z);
			Vector3 vector = Vector3.zero;
			List<GameObject> list = new List<GameObject>();
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(map2.DungeonEntranceDoor);
			if (propObject != null)
			{
				list.Add(propObject);
			}
			GameObject propObject2 = Singleton<ObjectCacheService>.Instance.GetPropObject(map2.EntranceDoor);
			if (propObject2 != null)
			{
				list.Add(propObject2);
			}
			foreach (string exitDoor in map2.ExitDoors)
			{
				list.Add(Singleton<ObjectCacheService>.Instance.GetPropObject(exitDoor));
			}
			if (map2.ExitDoors.Count > 0)
			{
				foreach (GameObject item in list)
				{
					vector += item.transform.position;
				}
				vector /= (float)list.Count;
			}
			else
			{
				vector = new Vector3(map2.Centre.X, map2.Centre.Y, map2.Centre.Z);
			}
			Vector3 vector2 = vector - ((component.DungeonEntranceDoor != null) ? component.DungeonEntranceDoor.transform.position : component.EntranceDoor.transform.position);
			vector2.Normalize();
			map2.DefaultLevelFlowNormal = new CVector3(vector2.x, vector2.y, vector2.z);
		}
	}

	private bool ValidEnemyPositionTile(EnemyState procGenMonster, GameObject tileGO, CMap procGenMapInfo, List<GameObject> doors, bool ignoreMonsterAttackType = false, bool ignoreSpawnTileProximity = false, bool ignoreEntranceProximity = false)
	{
		if (tileGO == null)
		{
			return false;
		}
		float num = Vector3.Dot((tileGO.transform.position - GloomUtility.CVToV(procGenMapInfo.Centre)).normalized, GloomUtility.CVToV(procGenMapInfo.DefaultLevelFlowNormal));
		CMonsterClass cMonsterClass = MonsterClassManager.Find(procGenMonster.ClassID);
		if (!ignoreMonsterAttackType && num > float.Epsilon && cMonsterClass.PredominantlyMelee)
		{
			return false;
		}
		if (!ignoreMonsterAttackType && num < -1E-45f && !cMonsterClass.PredominantlyMelee)
		{
			return false;
		}
		if (tileGO.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Count != 0)
		{
			return false;
		}
		foreach (GameObject door in doors)
		{
			float num2 = 0.8f;
			if (door.GetComponent<UnityGameEditorDoorProp>().m_IsDungeonEntrance)
			{
				num2 = (ignoreSpawnTileProximity ? 2f : 3f);
				num2 *= UnityGameEditorRuntime.s_TileSize.x;
			}
			else if (!ignoreEntranceProximity && (procGenMapInfo.EntranceDoor == door.name || procGenMapInfo.DungeonEntranceDoor == door.name))
			{
				num2 = 2f;
			}
			if ((door.transform.position - tileGO.transform.position).magnitude <= num2 + float.Epsilon)
			{
				return false;
			}
		}
		return true;
	}

	private bool ValidAllyMonsterPositionTile(EnemyState procGenMonster, GameObject tileGO, CMap procGenMapInfo, List<GameObject> doors, int maximumRangeFromPlayers, bool ignorePlayerTileProximity = false)
	{
		if (tileGO == null)
		{
			return false;
		}
		if (tileGO.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Count != 0)
		{
			return false;
		}
		foreach (CClientTile allPossibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
		{
			if (allPossibleStartingTile.m_GameObject == tileGO)
			{
				return false;
			}
		}
		foreach (GameObject door in doors)
		{
			float num = 0.8f;
			if ((door.transform.position - tileGO.transform.position).magnitude <= num + float.Epsilon)
			{
				return false;
			}
		}
		if (!ignorePlayerTileProximity)
		{
			bool flag = false;
			foreach (PlayerState player in procGenMapInfo.Players)
			{
				float num2 = Vector3.Distance(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[player.Location.X, player.Location.Y].m_GameObject.transform.position, tileGO.transform.position);
				float num3 = (float)maximumRangeFromPlayers * UnityGameEditorRuntime.s_TileSize.x;
				if (num2 <= num3)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private bool ValidObjectActorPositionTile(ObjectState procGenObject, GameObject tileGO, CMap procGenMapInfo, List<GameObject> doors, bool ignoreLevelFlow = false)
	{
		if (tileGO == null)
		{
			return false;
		}
		if (!ignoreLevelFlow && Vector3.Dot((tileGO.transform.position - GloomUtility.CVToV(procGenMapInfo.Centre)).normalized, GloomUtility.CVToV(procGenMapInfo.DefaultLevelFlowNormal)) < -1E-45f)
		{
			return false;
		}
		if (tileGO.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Count != 0)
		{
			return false;
		}
		if (ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles.Any((CClientTile a) => (a.m_GameObject.transform.position - tileGO.transform.position).magnitude < 0.1f))
		{
			return false;
		}
		foreach (GameObject door in doors)
		{
			float num = 0.8f;
			if ((door.transform.position - tileGO.transform.position).magnitude <= num + float.Epsilon)
			{
				return false;
			}
		}
		return true;
	}

	private GameObject FindEmptyTileInCover(EnemyState procGenMonster, CMap procGenMapInfo, List<GameObject> tilesInMap, List<GameObject> doors)
	{
		foreach (GameObject item in tilesInMap)
		{
			if (!ValidEnemyPositionTile(procGenMonster, item, procGenMapInfo, doors))
			{
				continue;
			}
			Vector3 vector = Singleton<ObjectCacheService>.Instance.GetPropObject(string.IsNullOrEmpty(procGenMapInfo.DungeonEntranceDoor) ? procGenMapInfo.EntranceDoor : procGenMapInfo.DungeonEntranceDoor).transform.position - item.transform.position;
			vector.Normalize();
			float closestDistance;
			GameObject gameObject = FindClosestTileTo(tilesInMap, item.transform.position + vector * UnityGameEditorRuntime.s_TileSize.x, out closestDistance);
			if (!(gameObject != null))
			{
				continue;
			}
			foreach (ScenarioManager.ObjectImportType item2 in gameObject.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile)
			{
				if (item2 != ScenarioManager.ObjectImportType.Coverage)
				{
					return item;
				}
			}
		}
		return null;
	}

	private void PlaceMonsters(ScenarioRuleLibrary.ScenarioState state)
	{
		foreach (CMap map in state.Maps)
		{
			for (int num = map.AllyMonsters.Count - 1; num >= 0; num--)
			{
				EnemyState enemyState = map.AllyMonsters[num];
				if (!PlaceAllyMonster(enemyState, map, m_AllMaps.Single((GameObject s) => s.name == map.MapInstanceName), GloomUtility.CVIToVI(state.PositiveSpaceOffset)))
				{
					ScenarioManager.CurrentScenarioState.AllyMonsters.Remove(enemyState);
				}
			}
		}
		int num2 = 0;
		foreach (CMap map2 in state.Maps)
		{
			bool flag = true;
			foreach (EnemyState monster in map2.Monsters)
			{
				if (!MonsterClassManager.Find(monster.ClassID).PredominantlyMelee)
				{
					flag = false;
					break;
				}
			}
			int num3 = map2.Monsters.Count / 2;
			for (int num4 = map2.Monsters.Count - 1; num4 >= 0; num4--)
			{
				EnemyState enemyState2 = map2.Monsters[num4];
				if (num2 > 0 && !enemyState2.IsElite)
				{
					enemyState2.IncreaseToElite();
					num2--;
					Debug.LogWarning($"ProcGen : Increased enemyActor to elite ({enemyState2.ClassID}) in room {map2.MapInstanceName}. Seed: {m_ProcGenSeed}");
				}
				if (!PlaceEnemy(enemyState2, map2, m_AllMaps.Single((GameObject s) => s.name == map2.MapInstanceName), GloomUtility.CVIToVI(state.PositiveSpaceOffset), flag && num4 <= num3))
				{
					num2 = (enemyState2.IsElite ? (num2 + 2) : (num2 + 1));
					ScenarioManager.CurrentScenarioState.Monsters.Remove(enemyState2);
				}
			}
		}
		foreach (CMap map3 in state.Maps)
		{
			for (int num5 = map3.Enemy2Monsters.Count - 1; num5 >= 0; num5--)
			{
				EnemyState enemyState3 = map3.Enemy2Monsters[num5];
				if (!PlaceEnemy(enemyState3, map3, m_AllMaps.Single((GameObject s) => s.name == map3.MapInstanceName), GloomUtility.CVIToVI(state.PositiveSpaceOffset)))
				{
					ScenarioManager.CurrentScenarioState.Enemy2Monsters.Remove(enemyState3);
				}
			}
		}
		foreach (CMap map4 in state.Maps)
		{
			for (int num6 = map4.NeutralMonsters.Count - 1; num6 >= 0; num6--)
			{
				EnemyState enemyState4 = map4.NeutralMonsters[num6];
				if (!PlaceEnemy(enemyState4, map4, m_AllMaps.Single((GameObject s) => s.name == map4.MapInstanceName), GloomUtility.CVIToVI(state.PositiveSpaceOffset)))
				{
					ScenarioManager.CurrentScenarioState.NeutralMonsters.Remove(enemyState4);
				}
			}
		}
		foreach (CMap map5 in state.Maps)
		{
			for (int num7 = map5.Objects.Count - 1; num7 >= 0; num7--)
			{
				ObjectState objectState = map5.Objects[num7];
				if (!PlaceObjectActor(objectState, map5, m_AllMaps.Single((GameObject s) => s.name == map5.MapInstanceName), GloomUtility.CVIToVI(state.PositiveSpaceOffset)))
				{
					ScenarioManager.CurrentScenarioState.Objects.Remove(objectState);
				}
			}
		}
	}

	private bool PlaceEnemy(EnemyState enemyState, CMap map, GameObject mapGO, Vector3Int positiveSpaceOffset, bool allEnemiesAreMelee = false)
	{
		List<GameObject> doors = UnityGameEditorRuntime.FindUnityGameObjects(mapGO, ScenarioManager.ObjectImportType.Door);
		List<GameObject> list = UnityGameEditorRuntime.FindUnityGameObjects(mapGO, ScenarioManager.ObjectImportType.Tile, findEmptyTiles: true);
		foreach (CClientTile allPossibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
		{
			list.Remove(allPossibleStartingTile.m_GameObject);
		}
		string prefabName = MonsterClassManager.Find(enemyState.ClassID).Models[enemyState.ChosenModelIndex];
		GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Enemy, prefabName);
		bool flag = false;
		int num = m_Random.Next(list.Count);
		int i = 0;
		bool flag2 = true;
		for (int j = 0; j < 2; j++)
		{
			for (; i < list.Count * 4; i++)
			{
				bool flag3 = i >= list.Count || allEnemiesAreMelee;
				bool ignoreSpawnTileProximity = i >= list.Count * 2;
				bool ignoreEntranceProximity = i >= list.Count * 3;
				GameObject gameObject = ((flag2 && !flag3) ? FindEmptyTileInCover(enemyState, map, list, doors) : list[num]);
				gameObject = gameObject ?? list[num];
				if (ValidEnemyPositionTile(enemyState, gameObject, map, doors, flag3, ignoreSpawnTileProximity, ignoreEntranceProximity))
				{
					UnityGameEditorObject component = characterPrefabFromBundle.GetComponent<UnityGameEditorObject>();
					gameObject.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Add(component.m_ObjectType);
					Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(gameObject.transform.position) + positiveSpaceOffset;
					enemyState.Location = new TileIndex(vector3Int.x, vector3Int.z);
					flag = true;
					break;
				}
				num = ((num != list.Count - 1) ? (num + 1) : 0);
			}
			if (flag)
			{
				break;
			}
			flag2 = false;
		}
		if (!flag)
		{
			Debug.LogWarning($"ProcGen : Not able to place enemyActor({enemyState.ClassID}) in room {map.MapInstanceName}. Seed: {m_ProcGenSeed}");
		}
		return flag;
	}

	private bool PlaceAllyMonster(EnemyState allyState, CMap map, GameObject mapGO, Vector3Int positiveSpaceOffset)
	{
		List<GameObject> doors = UnityGameEditorRuntime.FindUnityGameObjects(mapGO, ScenarioManager.ObjectImportType.Door);
		List<GameObject> source = UnityGameEditorRuntime.FindUnityGameObjects(mapGO, ScenarioManager.ObjectImportType.Tile, findEmptyTiles: true);
		source = source.Distinct().ToList();
		foreach (CClientTile allPossibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
		{
			source.Remove(allPossibleStartingTile.m_GameObject);
		}
		string prefabName = MonsterClassManager.Find(allyState.ClassID).Models[allyState.ChosenModelIndex];
		GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Ally, prefabName);
		bool flag = false;
		int num = m_Random.Next(source.Count);
		int num2 = 0;
		for (int i = 0; i < 2; i++)
		{
			while (num2 < source.Count * 4)
			{
				int maximumRangeFromPlayers = ((num2 >= source.Count * 2) ? 3 : ((num2 < source.Count) ? 1 : 2));
				bool ignorePlayerTileProximity = num2 >= source.Count * 3;
				GameObject gameObject = source[num];
				if (ValidAllyMonsterPositionTile(allyState, gameObject, map, doors, maximumRangeFromPlayers, ignorePlayerTileProximity))
				{
					UnityGameEditorObject component = characterPrefabFromBundle.GetComponent<UnityGameEditorObject>();
					UnityGameEditorObject component2 = gameObject.GetComponent<UnityGameEditorObject>();
					Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(gameObject.transform.position) + positiveSpaceOffset;
					TileIndex tileIndex = new TileIndex(vector3Int.x, vector3Int.z);
					bool flag2 = false;
					foreach (CClientTile allPossibleStartingTile2 in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
					{
						if (allPossibleStartingTile2.m_Tile.m_ArrayIndex.X == tileIndex.X && allPossibleStartingTile2.m_Tile.m_ArrayIndex.Y == tileIndex.Y)
						{
							flag2 = true;
						}
					}
					if (!flag2)
					{
						component2.m_ProcGenGameObjectsOnTile.Add(component.m_ObjectType);
						allyState.Location = tileIndex;
						flag = true;
						break;
					}
				}
				else
				{
					num = ((num != source.Count - 1) ? (num + 1) : 0);
					num2++;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (!flag)
		{
			Debug.LogWarning($"ProcGen : Not able to place allyActor({allyState.ClassID}) in room {map.MapInstanceName}. Seed: {m_ProcGenSeed}");
		}
		return flag;
	}

	private bool PlaceObjectActor(ObjectState objectState, CMap map, GameObject mapGO, Vector3Int positiveSpaceOffset)
	{
		List<GameObject> doors = UnityGameEditorRuntime.FindUnityGameObjects(mapGO, ScenarioManager.ObjectImportType.Door);
		List<GameObject> list = UnityGameEditorRuntime.FindUnityGameObjects(mapGO, ScenarioManager.ObjectImportType.Tile, findEmptyTiles: true);
		string prefabName = MonsterClassManager.FindObjectClass(objectState.ClassID).Models[objectState.ChosenModelIndex];
		GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Enemy, prefabName);
		bool flag = false;
		int num = m_Random.Next(list.Count);
		int i = 0;
		for (int j = 0; j < 2; j++)
		{
			for (; i < list.Count * 4; i++)
			{
				bool ignoreLevelFlow = i >= list.Count * 3;
				GameObject gameObject = list[num];
				if (ValidObjectActorPositionTile(objectState, gameObject, map, doors, ignoreLevelFlow))
				{
					UnityGameEditorObject component = characterPrefabFromBundle.GetComponent<UnityGameEditorObject>();
					gameObject.GetComponent<UnityGameEditorObject>().m_ProcGenGameObjectsOnTile.Add(component.m_ObjectType);
					Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(gameObject.transform.position) + positiveSpaceOffset;
					objectState.Location = new TileIndex(vector3Int.x, vector3Int.z);
					flag = true;
					break;
				}
				num = ((num != list.Count - 1) ? (num + 1) : 0);
			}
			if (flag)
			{
				break;
			}
		}
		if (!flag)
		{
			Debug.LogWarning($"ProcGen : Not able to place enemyActor({objectState.ClassID}) in room {map.MapInstanceName}. Seed: {m_ProcGenSeed}");
		}
		return flag;
	}

	private GameObject FindClosestTileTo(List<GameObject> tilesInMap, Vector3 position, out float closestDistance)
	{
		GameObject result = null;
		closestDistance = float.MaxValue;
		foreach (GameObject item in tilesInMap)
		{
			float magnitude = (item.transform.position - position).magnitude;
			if (magnitude < closestDistance)
			{
				closestDistance = magnitude;
				result = item;
			}
		}
		return result;
	}

	public static List<GameObject> FindValidStartingPositions(List<GameObject> tiles, GameObject dungenDoorGO, List<GameObject> players)
	{
		Vector3 doorRightVec = dungenDoorGO.GetComponent<UnityGameEditorDoorProp>().RightVector();
		return tiles.FindAll((GameObject x) => (x.transform.position - dungenDoorGO.transform.position).magnitude < UnityGameEditorRuntime.s_TileSize.x * 3f && (players == null || players.Find((GameObject p) => p.transform.position == x.transform.position) == null)).OrderBy(delegate(GameObject x)
		{
			Vector3 vector = x.transform.position - dungenDoorGO.transform.position;
			float num = 2f + Mathf.Pow(Mathf.Abs(Vector3.Dot(vector.normalized, doorRightVec)), 2f);
			return vector.magnitude * num;
		}).Take(5)
			.ToList();
	}
}
