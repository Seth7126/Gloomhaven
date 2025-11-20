#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using AsmodeeNet.Utils.Extensions;
using Chronos;
using EPOOutline;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.Controller;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldspaceStarHexDisplay : MonoBehaviour
{
	public class TileTemplate
	{
		public HexSelect_Control.HexType m_Type;

		public HexSelect_Control.HexMode m_Mode;

		public bool m_Hover;

		public TileTemplate(HexSelect_Control.HexType type, HexSelect_Control.HexMode mode, bool hover)
		{
			m_Type = type;
			m_Mode = mode;
			m_Hover = hover;
		}
	}

	public enum EAbilityDisplayType
	{
		None,
		Normal,
		AreaOfEffect,
		EnemyAreaOfEffect,
		SelectObjectPosition,
		TargetingAbility,
		NegativeAbility,
		SelectPath,
		SelectObjectPositionAreaOfEffect,
		RedistributeDamageAbility,
		ObjectiveAbility
	}

	public enum EMoveDisplayType
	{
		None,
		Normal,
		Pull,
		Push
	}

	public enum WorldSpaceStarDisplayState
	{
		ShowNone,
		CharacterPlacement,
		MovementSelection,
		TargetSelection,
		LongResting,
		LevelEditorSpawning
	}

	private class PropInfo
	{
		public string Title { get; set; }

		public string Description { get; set; }

		public string LootedBy { get; set; }

		public int Gold { get; set; }

		public ScenarioManager.ObjectImportType ImportType { get; set; } = ScenarioManager.ObjectImportType.None;

		public (string title, string description) Get()
		{
			if (Gold > 0)
			{
				if (ImportType != ScenarioManager.ObjectImportType.None)
				{
					Title = Title + "\n&\n" + Gold + " " + LocalizationManager.GetTranslation("Gold");
				}
				else
				{
					Title = Gold + " " + LocalizationManager.GetTranslation("Gold");
				}
			}
			if (!LootedBy.IsNullOrEmpty())
			{
				if (Description.IsNullOrEmpty())
				{
					Description = LootedBy;
				}
				else
				{
					Description = Description + "\n" + LootedBy;
				}
			}
			return (title: Title, description: Description);
		}
	}

	public static WorldspaceStarHexDisplay Instance;

	private Dictionary<CClientTile, HexSelect_Control> s_PossibleMoveStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_PossibleMoveStarsRemainingInDirection = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_PathMoveStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_PossibleAttackPathStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_PathAttackStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_AttackStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_AbilityStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_PlacementStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_DungeonExitStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, HexSelect_Control> s_ImpossibleMoveChestDoorStars = new Dictionary<CClientTile, HexSelect_Control>();

	private CClientTile s_SelectedPlacementStars;

	private CClientTile s_CursorHighlightedTile;

	private HexSelect_Control s_CursorHighlightedStar;

	private Dictionary<CClientTile, HexSelect_Control> s_HighlightAllStars = new Dictionary<CClientTile, HexSelect_Control>();

	private Dictionary<CClientTile, Dictionary<CClientTile, HexSelect_Control>> s_CurrentlyActiveStars = new Dictionary<CClientTile, Dictionary<CClientTile, HexSelect_Control>>();

	private List<CClientTile> s_TilesExpectedToDisplay = new List<CClientTile>();

	private List<CClientTile> s_CachedMoveStars = new List<CClientTile>();

	private List<CClientTile> s_CachedPushStars = new List<CClientTile>();

	private List<CClientTile> s_CachedPullStars = new List<CClientTile>();

	private int m_AreaEffectAngle;

	private int m_LastAreaEffectAngle;

	private float m_LastRecievedRotateDirection;

	private bool m_TurningRight;

	private CInteractable m_LastPointedAtInteractable;

	private bool m_NewTilePointedAtThisFrame;

	private List<CClientTile> s_CachedAbilityRangeTiles = new List<CClientTile>();

	private List<CClientTile> s_ChestTiles = new List<CClientTile>();

	private List<CClientTile> s_DoorTiles = new List<CClientTile>();

	private List<CTile> s_LeftLineTiles = new List<CTile>();

	private List<CTile> s_RightLineTiles = new List<CTile>();

	private List<CTile> s_TopLeftLineTiles = new List<CTile>();

	private List<CTile> s_TopRightLineTiles = new List<CTile>();

	private List<CTile> s_BottomLeftLineTiles = new List<CTile>();

	private List<CTile> s_BottomRightLineTiles = new List<CTile>();

	private List<CTile> s_AllLineTiles = new List<CTile>();

	private bool m_EndingMovement;

	private bool m_OpeningDoor;

	private bool m_SelectingADoor;

	private bool m_AOELocked;

	private CAbility m_SavedAbility;

	private ScenarioManager.ObjectImportType m_ObjectSpawnType = ScenarioManager.ObjectImportType.None;

	private List<CAbilityFilter.EFilterTile> m_TileFilters;

	private CShowRedistributeDamageUI_MessageData m_SavedRedistributeDamageUIMessage;

	private DistributeDamageService m_SavedDistributeDamageService;

	private bool m_RefreshedCurrentState;

	private bool m_AllHexesHighlighted;

	private bool m_FoundAllChestAndDoorTiles;

	private List<ActorBehaviour> s_EnemiesToRetaliate = new List<ActorBehaviour>();

	private GameObject m_TargetTooltip;

	private Action m_OnRemoveTargetTooltip;

	public TileTemplate s_MovePossiblityTemplate = new TileTemplate(HexSelect_Control.HexType.Move, HexSelect_Control.HexMode.Reach, hover: false);

	public TileTemplate s_MoveHazardPossibilityTemplate = new TileTemplate(HexSelect_Control.HexType.NegativeEffect, HexSelect_Control.HexMode.PossibleTarget, hover: false);

	public TileTemplate s_MoveChestDoorPossibilityTemplate = new TileTemplate(HexSelect_Control.HexType.PositiveEffect, HexSelect_Control.HexMode.PossibleTarget, hover: false);

	public TileTemplate s_DifficultTerrainPossibilityTemplate = new TileTemplate(HexSelect_Control.HexType.DifficultTerrain, HexSelect_Control.HexMode.PossibleTarget, hover: false);

	public EMoveDisplayType CurrentMoveDisplayType;

	private WorldSpaceStarDisplayState m_currentDisplayState;

	private WorldSpaceStarDisplayState m_savedDisplayState;

	private bool m_HexDisplayToggledOff;

	private CActor _shownActorPanel;

	public ScenarioManager.EAdjacentPosition CurrentLineDirection;

	private List<GameObject> previewedCharacterPlacements = new List<GameObject>();

	private const int WAITING_FOR_SECOND_THREAD_TIMEOUT = 20;

	private bool _doShow = true;

	private RequestCounter _hideCounter;

	public int PossibleMoveStarsCount => s_PossibleMoveStars.Count;

	public int AreaEffectAngle => m_AreaEffectAngle;

	public int AbilityRange
	{
		get
		{
			if (m_SavedAbility != null)
			{
				return m_SavedAbility.Range;
			}
			return 0;
		}
	}

	public bool LockView { get; set; }

	public EAbilityDisplayType CurrentAbilityDisplayType { get; private set; }

	public WorldSpaceStarDisplayState CurrentDisplayState
	{
		get
		{
			return m_currentDisplayState;
		}
		set
		{
			if (m_currentDisplayState != value)
			{
				m_currentDisplayState = value;
				m_savedDisplayState = value;
				OnStarDisplayStateChanged();
			}
		}
	}

	public bool MaxTargetsSelected
	{
		get
		{
			if (m_SavedAbility != null)
			{
				return m_SavedAbility.MaxTargetsSelected();
			}
			return false;
		}
	}

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		m_currentDisplayState = WorldSpaceStarDisplayState.ShowNone;
		s_CachedAbilityRangeTiles.Clear();
		_hideCounter = new RequestCounter(Hide, Show);
		s_CursorHighlightedStar = null;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	public void ToggleHexDisplay(bool enabled)
	{
		if (enabled)
		{
			m_currentDisplayState = m_savedDisplayState;
			m_savedDisplayState = WorldSpaceStarDisplayState.ShowNone;
			m_HexDisplayToggledOff = false;
		}
		else
		{
			m_savedDisplayState = CurrentDisplayState;
			m_currentDisplayState = WorldSpaceStarDisplayState.ShowNone;
			m_HexDisplayToggledOff = true;
			if (s_CursorHighlightedStar != null)
			{
				ObjectPool.Recycle(s_CursorHighlightedStar.gameObject, GlobalSettings.Instance.m_GenericHexStar);
			}
			s_CursorHighlightedStar = null;
		}
		OnStarDisplayStateChanged();
	}

	private void OnStarDisplayStateChanged()
	{
		UpdateRetaliateDamage(0, forceUpdate: true);
		SetInitialState();
		RefreshCurrentState();
	}

	public void SetDisplayAbility(CAbility displayAbility, EAbilityDisplayType attackType = EAbilityDisplayType.None, ScenarioManager.ObjectImportType objectSpawnType = ScenarioManager.ObjectImportType.None, List<CAbilityFilter.EFilterTile> tileFilters = null)
	{
		m_SavedAbility = displayAbility;
		CurrentAbilityDisplayType = attackType;
		m_ObjectSpawnType = objectSpawnType;
		m_TileFilters = tileFilters?.ToList();
		s_CachedAbilityRangeTiles.Clear();
	}

	private void SetInitialState()
	{
		if (Choreographer.s_Choreographer != null && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && m_currentDisplayState == WorldSpaceStarDisplayState.TargetSelection && CurrentAbilityDisplayType == EAbilityDisplayType.RedistributeDamageAbility)
		{
			SetDisplayRedistributeDamageInitialState();
		}
	}

	public void RefreshCurrentState()
	{
		if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForTileSelected)
		{
			return;
		}
		RefreshPreviewedCharacterPlacements();
		switch (m_currentDisplayState)
		{
		case WorldSpaceStarDisplayState.CharacterPlacement:
			ShowPossiblePlacementHexes();
			break;
		case WorldSpaceStarDisplayState.MovementSelection:
			ShowPossibleMovementSelectionHexes();
			break;
		case WorldSpaceStarDisplayState.TargetSelection:
			if (CurrentAbilityDisplayType == EAbilityDisplayType.SelectPath)
			{
				ShowPossibleAttackPathStars();
			}
			break;
		case WorldSpaceStarDisplayState.LongResting:
			ShowLongRestHex();
			break;
		case WorldSpaceStarDisplayState.ShowNone:
			if (ControllerInputPointer.CursorType > ECursorType.Default)
			{
				ControllerInputPointer.CursorType = ECursorType.Default;
			}
			ClearStars();
			break;
		}
		ShowPossibleExitHexes();
		m_RefreshedCurrentState = true;
	}

	private void RefreshPreviewedCharacterPlacements()
	{
		if (m_currentDisplayState != WorldSpaceStarDisplayState.CharacterPlacement)
		{
			HidePreviewedCharacterPlacements();
		}
	}

	private void HidePreviewedCharacterPlacements()
	{
		foreach (GameObject previewedCharacterPlacement in previewedCharacterPlacements)
		{
			if (previewedCharacterPlacement != null)
			{
				previewedCharacterPlacement.SetActive(value: false);
			}
		}
		previewedCharacterPlacements.Clear();
	}

	public void Update()
	{
		if (Choreographer.s_Choreographer != null && !m_HexDisplayToggledOff)
		{
			if (SaveData.Instance.Global.GameMode == EGameMode.LevelEditor)
			{
				m_NewTilePointedAtThisFrame = PointingAtANewTile();
				if (m_NewTilePointedAtThisFrame || m_RefreshedCurrentState)
				{
					if (m_currentDisplayState == WorldSpaceStarDisplayState.LevelEditorSpawning)
					{
						ListenForTargetingInputEvents();
						DisplayLevelEditorObstaclePlacementStars();
					}
					ShowLevelEditorStartingTiles();
				}
			}
			else if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && !TimeManager.IsPaused && Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForTileSelected)
			{
				m_NewTilePointedAtThisFrame = PointingAtANewTile();
				if (!m_AllHexesHighlighted && !LockView)
				{
					if (m_NewTilePointedAtThisFrame || m_RefreshedCurrentState)
					{
						ClearTargetTooltip();
						switch (m_currentDisplayState)
						{
						case WorldSpaceStarDisplayState.CharacterPlacement:
							HighlightSelectedPlacementHex();
							break;
						case WorldSpaceStarDisplayState.MovementSelection:
							ShowMovePathStars();
							break;
						case WorldSpaceStarDisplayState.TargetSelection:
							if (Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
							{
								if (ListenForTargetingInputEvents())
								{
									switch (CurrentAbilityDisplayType)
									{
									case EAbilityDisplayType.AreaOfEffect:
										DisplayAOEStars();
										break;
									case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
										DisplaySelectObjectPositionAOEStars();
										break;
									}
								}
								else
								{
									ShowPossibleTargetSelectionHexes();
								}
							}
							else
							{
								ShowPossibleTargetSelectionHexes();
							}
							break;
						}
						m_RefreshedCurrentState = false;
					}
					else if (m_currentDisplayState == WorldSpaceStarDisplayState.TargetSelection && Choreographer.s_Choreographer.ThisPlayerHasTurnControl && ListenForTargetingInputEvents())
					{
						switch (CurrentAbilityDisplayType)
						{
						case EAbilityDisplayType.AreaOfEffect:
							DisplayAOEStars();
							break;
						case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
							DisplaySelectObjectPositionAOEStars();
							break;
						}
					}
				}
				RefreshHighlightAllHexes();
				ControlPlayerEndMovement();
				DisplayCursorHoverStar();
			}
		}
		if ((bool)s_CursorHighlightedStar)
		{
			s_CursorHighlightedStar?.gameObject.SetActive(_doShow);
		}
	}

	public void ShowPossiblePlacementHexes()
	{
		ClearStars();
		ControllerInputPointer.CursorType = ((InteractableUnderMouse()?.GetComponent<TileBehaviour>()?.m_ClientTile != null) ? ECursorType.Default : ECursorType.Invalid);
		foreach (CClientTile allPossibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
		{
			if (!s_PlacementStars.ContainsKey(allPossibleStartingTile) && allPossibleStartingTile.m_Tile.m_HexMap != null && allPossibleStartingTile.m_Tile.m_HexMap.Revealed)
			{
				if (!Choreographer.s_Choreographer.PlayersInValidStartingPositions || Choreographer.s_Choreographer.AnyPlayersInInvalidStartingPositionsForCompanionSummons)
				{
					ModifyStar(allPossibleStartingTile, HexSelect_Control.HexType.InvalidPlacement, HexSelect_Control.HexMode.Selected, hover: false, s_PlacementStars);
				}
				else
				{
					ModifyStar(allPossibleStartingTile, HexSelect_Control.HexType.Move, HexSelect_Control.HexMode.Reach, hover: false, s_PlacementStars);
				}
			}
		}
		SetHexBorders(s_PlacementStars);
	}

	public void ShowPossibleExitHexes()
	{
		ClearStars(possibleMove: false, pathMove: false, attack: false, attackPath: false, possibleAttackPath: false, ability: false, placement: false, placementSelection: false, allHighlightedTiles: false, impossibleMoveChestDoor: false, possibleMoveDirection: false, cursorHighlightedTiles: false, exitDungeon: true);
		foreach (CClientTile tile in ClientScenarioManager.s_ClientScenarioManager.DungeonExitTiles)
		{
			bool flag = false;
			foreach (CObjective winObjective in ScenarioManager.CurrentScenarioState.WinObjectives)
			{
				if (!winObjective.IsActive)
				{
					continue;
				}
				if (winObjective is CObjective_ActorReachPosition cObjective_ActorReachPosition)
				{
					if (cObjective_ActorReachPosition.ActorTargetPositions.SingleOrDefault((TileIndex t) => t.X == tile.m_Tile.m_ArrayIndex.X && t.Y == tile.m_Tile.m_ArrayIndex.Y) != null)
					{
						flag = true;
						break;
					}
				}
				else if (winObjective is CObjective_ActorsEscaped cObjective_ActorsEscaped && cObjective_ActorsEscaped.EscapePositions.SingleOrDefault((TileIndex t) => t.X == tile.m_Tile.m_ArrayIndex.X && t.Y == tile.m_Tile.m_ArrayIndex.Y) != null)
				{
					flag = true;
					break;
				}
			}
			if (flag && !s_CurrentlyActiveStars.ContainsKey(tile) && !s_DungeonExitStars.ContainsKey(tile) && tile.m_Tile.m_HexMap != null)
			{
				bool flag2 = (tile.m_Tile.m_HexMap != null && tile.m_Tile.m_HexMap.Revealed) || (tile.m_Tile.m_Hex2Map != null && tile.m_Tile.m_Hex2Map.Revealed);
				ModifyStar(tile, flag2 ? HexSelect_Control.HexType.ExitDungeonRevealed : HexSelect_Control.HexType.ExitDungeonUnrevealed, HexSelect_Control.HexMode.Reach, hover: false, s_DungeonExitStars);
			}
		}
		SetHexBorders(s_DungeonExitStars);
	}

	public void HighlightSelectedPlacementHex()
	{
		Waypoint.s_PlacementTile = null;
		ClearStars(possibleMove: true, pathMove: true, attack: true, attackPath: true, possibleAttackPath: true, ability: true, placement: false);
		CInteractable cInteractable = Interactable();
		if (cInteractable == null)
		{
			HidePreviewedCharacterPlacements();
			return;
		}
		TileBehaviour component = cInteractable.GetComponent<TileBehaviour>();
		if (component == null || component.m_ClientTile == null)
		{
			HidePreviewedCharacterPlacements();
			return;
		}
		CClientTile clientTile = component.m_ClientTile;
		foreach (CClientTile allPossibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
		{
			if (allPossibleStartingTile != clientTile && allPossibleStartingTile.m_Tile.m_HexMap != null && allPossibleStartingTile.m_Tile.m_HexMap.Revealed)
			{
				if (!Choreographer.s_Choreographer.PlayersInValidStartingPositions || Choreographer.s_Choreographer.AnyPlayersInInvalidStartingPositionsForCompanionSummons)
				{
					ModifyStar(allPossibleStartingTile, HexSelect_Control.HexType.InvalidPlacement, HexSelect_Control.HexMode.Reach, hover: false, s_PlacementStars);
				}
				else
				{
					ModifyStar(allPossibleStartingTile, HexSelect_Control.HexType.Move, HexSelect_Control.HexMode.Reach, hover: false, s_PlacementStars);
				}
				s_PlacementStars[allPossibleStartingTile].RefreshHexUI();
			}
		}
		SetHexBorders(s_PlacementStars);
		if (!s_PlacementStars.ContainsKey(clientTile))
		{
			ControllerInputPointer.CursorType = ECursorType.Default;
			HidePreviewedCharacterPlacements();
			return;
		}
		if (ScenarioManager.Scenario.FindActorAt(clientTile.m_Tile.m_ArrayIndex) != null)
		{
			ControllerInputPointer.CursorType = ECursorType.Default;
			HidePreviewedCharacterPlacements();
			return;
		}
		if (s_PlacementStars.ContainsKey(clientTile))
		{
			CActor cActor = InitiativeTrack.Instance.SelectedActor()?.Actor;
			if (cActor is CPlayerActor && cActor.IsUnderMyControl)
			{
				PreviewPlacement(cActor.Class.ID, clientTile, Choreographer.s_Choreographer.FindClientPlayer(cActor).transform.rotation);
			}
			else
			{
				HidePreviewedCharacterPlacements();
			}
			ControllerInputPointer.CursorType = ECursorType.Targeted;
			s_PlacementStars[clientTile].SetHover(hover: true);
			s_SelectedPlacementStars = clientTile;
			bool flag = false;
			if (Choreographer.s_Choreographer.WillMovingPlayerToTileMakeStartingPositionsValid(clientTile))
			{
				flag = true;
			}
			if (flag)
			{
				ModifyStar(clientTile, HexSelect_Control.HexType.Move, HexSelect_Control.HexMode.Reach, hover: true, s_PlacementStars);
			}
			else
			{
				ModifyStar(clientTile, HexSelect_Control.HexType.InvalidPlacement, HexSelect_Control.HexMode.Reach, hover: true, s_PlacementStars);
			}
			s_PlacementStars[clientTile].RefreshHexUI();
		}
		else
		{
			ControllerInputPointer.CursorType = ECursorType.Default;
		}
		Waypoint.s_PlacementTile = clientTile;
	}

	public void ShowLongRestHex()
	{
		ClearStars();
		if (Choreographer.s_Choreographer.m_CurrentActor != null)
		{
			CActor currentActor = Choreographer.s_Choreographer.m_CurrentActor;
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[currentActor.ArrayIndex.X, currentActor.ArrayIndex.Y];
			if (!s_PlacementStars.ContainsKey(cClientTile) && cClientTile.m_Tile.m_HexMap != null && cClientTile.m_Tile.m_HexMap.Revealed)
			{
				ModifyStar(cClientTile, HexSelect_Control.HexType.PositiveEffect, HexSelect_Control.HexMode.Selected, hover: true, s_PlacementStars);
				SetHexBorders(s_PlacementStars);
			}
		}
	}

	private void ShowPossibleMovementSelectionHexes()
	{
		switch (CurrentMoveDisplayType)
		{
		case EMoveDisplayType.Normal:
			ShowPossibleMoveStars();
			break;
		case EMoveDisplayType.Push:
			ShowPossiblePushStars();
			break;
		case EMoveDisplayType.Pull:
			ShowPossiblePullStars();
			break;
		}
	}

	private void ShowPossibleMoveStars()
	{
		try
		{
			ClearStars();
			Waypoint.ClearValidSelectionTiles();
			if (Choreographer.s_Choreographer.m_CurrentAbility == null || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove) || m_SelectingADoor)
			{
				return;
			}
			CAbilityMove cAbilityMove = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityMove;
			bool ignoreBlocked = cAbilityMove.Jump || cAbilityMove.Fly;
			bool ignoreDifficultTerrain = cAbilityMove.IgnoreDifficultTerrain;
			CActor s_MovingActor = Waypoint.s_MovingActor;
			int remainingMoves = ((Waypoint.s_Waypoints.Count > 0) ? Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().MovesRemaining : cAbilityMove.RemainingMoves);
			Point startingPoint = ((Waypoint.s_Waypoints.Count == 0) ? Waypoint.s_MovingActor.ArrayIndex : Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex);
			CActor.EType type = Waypoint.s_MovingActor.Type;
			if (!m_FoundAllChestAndDoorTiles)
			{
				FindAllChestAndDoorTiles();
			}
			List<CClientTile> list = new List<CClientTile>();
			if (cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
			{
				if (Waypoint.s_Waypoints.Count > 0)
				{
					list = ((CurrentLineDirection != ScenarioManager.EAdjacentPosition.ECenter) ? GetRemainingTilesInDirection(s_MovingActor, remainingMoves, CurrentLineDirection) : FindStraightLineDirection(s_MovingActor, remainingMoves));
					list.AddRange(s_DoorTiles);
					PossibleMoveTileStarCreation(list, ignoreBlocked, s_MovingActor, remainingMoves, startingPoint, type, s_PossibleMoveStarsRemainingInDirection, cAbilityMove.RestrictRange, cAbilityMove.RestrictPoint);
					SetHexBorders(s_PossibleMoveStarsRemainingInDirection);
				}
				else
				{
					list = ((CurrentLineDirection != ScenarioManager.EAdjacentPosition.ECenter) ? GetRemainingTilesInDirection(s_MovingActor, remainingMoves, CurrentLineDirection) : FindAllStraightLineTiles(s_MovingActor, remainingMoves));
					list.AddRange(s_DoorTiles);
					PossibleMoveTileStarCreation(list, ignoreBlocked, s_MovingActor, remainingMoves, startingPoint, type, s_PossibleMoveStars, cAbilityMove.RestrictRange, cAbilityMove.RestrictPoint);
					SetHexBorders(s_PossibleMoveStars);
				}
			}
			else
			{
				if (cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.MustEndNextToObstacle))
				{
					foreach (CTile mustEndNextToObstacleTile in CAbilityMove.GetMustEndNextToObstacleTiles(s_MovingActor, remainingMoves, (Waypoint.s_Waypoints.Count == 0) ? Waypoint.s_MovingActor.ArrayIndex : Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex, ignoreBlocked, ignoreDifficultTerrain))
					{
						CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[mustEndNextToObstacleTile.m_ArrayIndex.X, mustEndNextToObstacleTile.m_ArrayIndex.Y];
						if (cClientTile != null)
						{
							list.Add(cClientTile);
						}
					}
				}
				else
				{
					for (int i = 0; i < ScenarioManager.Height; i++)
					{
						for (int j = 0; j < ScenarioManager.Width; j++)
						{
							CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[j, i];
							if (cClientTile2 != null)
							{
								list.Add(cClientTile2);
							}
						}
					}
				}
				PossibleMoveTileStarCreation(list, ignoreBlocked, s_MovingActor, remainingMoves, startingPoint, type, s_PossibleMoveStars, cAbilityMove.RestrictRange, cAbilityMove.RestrictPoint);
				SetHexBorders(s_PossibleMoveStars);
			}
			SetHexBorders(s_ImpossibleMoveChestDoorStars);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.PossibleMoveStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ShowPossiblePushStars()
	{
		try
		{
			if (!(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush))
			{
				return;
			}
			Waypoint.ClearValidSelectionTiles();
			ClearStars();
			CAbilityPush cAbilityPush = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityPush;
			CEnemyActor cEnemyActor = null;
			if (cAbilityPush.CurrentTarget is CEnemyActor)
			{
				cEnemyActor = cAbilityPush.CurrentTarget as CEnemyActor;
			}
			bool ignoreBlocked = false;
			if (cEnemyActor != null)
			{
				ignoreBlocked = cEnemyActor.CachedFlying;
			}
			if (cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles))
			{
				ignoreBlocked = true;
			}
			if (cAbilityPush.CurrentTarget == null)
			{
				return;
			}
			CActor currentTarget = cAbilityPush.CurrentTarget;
			int num = ((Waypoint.s_Waypoints.Count > 0) ? Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().MovesRemaining : cAbilityPush.RemainingPushes);
			Point point = ((Waypoint.s_Waypoints.Count == 0) ? cAbilityPush.CurrentTarget.ArrayIndex : Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex);
			CActor.EType type = cAbilityPush.CurrentTarget.Type;
			List<CClientTile> list = new List<CClientTile>();
			foreach (CTile pushTile in CAbilityPush.GetPushTiles(cAbilityPush.CurrentTarget, type, point, cAbilityPush.PushFromPoint, num, cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles)))
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[pushTile.m_ArrayIndex.X, pushTile.m_ArrayIndex.Y];
				if (cClientTile != null)
				{
					list.Add(cClientTile);
				}
			}
			bool flag = false;
			if (cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked))
			{
				CTile cTile = ScenarioManager.Tiles[cAbilityPush.CurrentTarget.ArrayIndex.X, cAbilityPush.CurrentTarget.ArrayIndex.Y];
				if (CAbilityPush.IsPushTileAdjacentToBlockedPushTile(cTile, cAbilityPush.CurrentTarget, type, point, cAbilityPush.PushFromPoint, cAbilityPush.RemainingPushes, intoBlocked: true))
				{
					list.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y]);
					flag = true;
				}
			}
			PossibleMoveTileStarCreation(list, ignoreBlocked, currentTarget, num, point, type, s_PossibleMoveStars);
			SetHexBorders(s_PossibleMoveStars);
			SetHexBorders(s_ImpossibleMoveChestDoorStars);
			s_CachedPushStars.Clear();
			s_CachedPushStars.AddRange(s_PossibleMoveStars.Keys);
			bool flag2 = s_PossibleMoveStars.Count == 0;
			Choreographer.s_Choreographer.readyButton.SetInteractable(flag2 || flag);
			Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy && Choreographer.s_Choreographer.IsPlayerTurn);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.PossiblePushStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ShowPossiblePullStars()
	{
		try
		{
			if (Choreographer.s_Choreographer.m_CurrentAbility == null || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull))
			{
				return;
			}
			CAbilityPull cAbilityPull = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityPull;
			Waypoint.ClearValidSelectionTiles();
			ClearStars();
			bool ignoreBlocked = false;
			CEnemyActor cEnemyActor = null;
			if (cAbilityPull.CurrentTarget is CEnemyActor)
			{
				cEnemyActor = cAbilityPull.CurrentTarget as CEnemyActor;
			}
			if (cEnemyActor != null)
			{
				ignoreBlocked = cEnemyActor.CachedFlying;
			}
			CActor currentTarget = cAbilityPull.CurrentTarget;
			int remainingMoves = ((Waypoint.s_Waypoints.Count > 0) ? Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().MovesRemaining : cAbilityPull.RemainingPulls);
			Point point = ((Waypoint.s_Waypoints.Count == 0) ? cAbilityPull.CurrentTarget.ArrayIndex : Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex);
			CActor.EType type = cAbilityPull.CurrentTarget.Type;
			List<CClientTile> list = new List<CClientTile>();
			foreach (CTile pullTile in CAbilityPull.GetPullTiles(cAbilityPull.CurrentTarget, type, point, cAbilityPull.PullToPoint, cAbilityPull.PullType))
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[pullTile.m_ArrayIndex.X, pullTile.m_ArrayIndex.Y];
				if (cClientTile != null)
				{
					list.Add(cClientTile);
				}
			}
			PossibleMoveTileStarCreation(list, ignoreBlocked, currentTarget, remainingMoves, point, type, s_PossibleMoveStars);
			SetHexBorders(s_PossibleMoveStars);
			SetHexBorders(s_ImpossibleMoveChestDoorStars);
			s_CachedPullStars.Clear();
			s_CachedPullStars.AddRange(s_PossibleMoveStars.Keys);
			bool interactable = s_PossibleMoveStars.Count == 0;
			Choreographer.s_Choreographer.readyButton.SetInteractable(interactable);
			Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy && Choreographer.s_Choreographer.IsPlayerTurn);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.PossiblePullStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void PossibleMoveTileStarCreation(List<CClientTile> tiles, bool ignoreBlocked, CActor movingActor, int remainingMoves, Point startingPoint, CActor.EType actorType, Dictionary<CClientTile, HexSelect_Control> storeTiles, int? restrictRange = null, Point? restrictPoint = null, bool LOS = false)
	{
		bool jump = ignoreBlocked;
		bool fly = ignoreBlocked;
		bool ignoreDifficultTerrain = false;
		bool ignoreHazardousTerrain = false;
		bool carryOtherActors = false;
		CActor.EType eType = CActor.EType.Unknown;
		CAbilityMove cAbilityMove = null;
		if (Choreographer.s_Choreographer.m_CurrentAbility != null)
		{
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove)
			{
				cAbilityMove = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityMove;
				jump = cAbilityMove.Jump;
				fly = cAbilityMove.Fly;
				ignoreDifficultTerrain = cAbilityMove.IgnoreDifficultTerrain;
				ignoreHazardousTerrain = cAbilityMove.IgnoreHazardousTerrain;
				carryOtherActors = cAbilityMove.CarryOtherActorsOnHex;
				if (cAbilityMove.ActorsToCarry.Count > 0)
				{
					int strength = 0;
					eType = cAbilityMove.ActorsToCarry[0].Type;
					actorType = eType;
					CAbilityMove.GetMoveBonuses(cAbilityMove.ActorsToCarry[0], out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, ref strength);
				}
				ignoreBlocked = jump || fly;
			}
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull || Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush)
			{
				ignoreDifficultTerrain = true;
			}
		}
		List<CObjectTrap> source = (from w in ScenarioManager.CurrentScenarioState.Maps.SelectMany((CMap sm) => sm.Props).OfType<CObjectTrap>()
			where !w.Activated
			select w).ToList();
		CActor.EType movingType = actorType;
		if (actorType == CActor.EType.Player && GameState.OverridingCurrentActor && GameState.OverridenActionActorStack.Peek().OriginalType == CActor.EType.Enemy)
		{
			movingType = CActor.EType.Enemy;
		}
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(movingActor, CAbility.EAbilityType.AddCondition).ToList();
		foreach (CClientTile tile in tiles)
		{
			if (!IsMoveTileValid(tile, ignoreBlocked, movingActor, movingType))
			{
				continue;
			}
			CTile targetTile = ScenarioManager.Tiles[movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y];
			if (LOS && !CActor.HaveLOS(tile.m_Tile, targetTile))
			{
				continue;
			}
			if (!(tile.m_Tile.m_ArrayIndex == startingPoint))
			{
				bool foundPath;
				int num = CAbilityMove.CalculateMoveCost(CActor.FindCharacterPath(movingActor, startingPoint, tile.m_Tile.m_ArrayIndex, ignoreBlocked, CurrentMoveDisplayType != EMoveDisplayType.Normal, out foundPath, avoidTraps: false, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors, eType), !fly, CurrentMoveDisplayType != EMoveDisplayType.Normal || !jump, ignoreMoveCost: false, ignoreDifficultTerrain);
				if (!foundPath || num > remainingMoves)
				{
					ProcessPossibleMoveTileAsDoorOrChest(tile, s_ImpossibleMoveChestDoorStars);
					continue;
				}
				if (restrictRange.HasValue && restrictPoint.HasValue)
				{
					bool foundPath2;
					List<Point> path = CActor.FindCharacterPath(movingActor, restrictPoint.Value, tile.m_Tile.m_ArrayIndex, ignoreBlocked, CurrentMoveDisplayType != EMoveDisplayType.Normal, out foundPath2, avoidTraps: false, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors);
					if (!foundPath2 || CAbilityMove.CalculateMoveCost(path, !ignoreBlocked, CurrentMoveDisplayType == EMoveDisplayType.Normal, ignoreMoveCost: false, ignoreDifficultTerrain) > restrictRange.Value)
					{
						continue;
					}
				}
				else if (((actorType == CActor.EType.Player && GameState.OverridingCurrentActor && (GameState.OverridenActionActorStack.Peek().PreviousActor.Type != CActor.EType.Player || movingActor.OriginalType != CActor.EType.Player)) || (movingActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)) && ProcessClosedDoorsForControlledAI(tile, s_ImpossibleMoveChestDoorStars))
				{
					continue;
				}
			}
			if (ProcessPossibleMoveTileAsDoorOrChest(tile, storeTiles))
			{
				continue;
			}
			bool flag = false;
			CObjectProp cObjectProp = tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap);
			if (cObjectProp != null && source.Contains(cObjectProp))
			{
				flag = true;
			}
			if (!flag && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null))
			{
				flag = true;
			}
			bool flag2 = false;
			if (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainRubble) != null || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainWater) != null)
			{
				flag2 = true;
			}
			bool flag3 = false;
			foreach (CActiveBonus item in list)
			{
				if (item.ActiveBonusIsActivatedByTile(tile.m_Tile))
				{
					flag3 = true;
				}
			}
			if (flag3)
			{
				ModifyStar(tile, HexSelect_Control.HexType.ActiveBonusCondition, HexSelect_Control.HexMode.PossibleTarget, hover: false, storeTiles);
			}
			else if (flag2)
			{
				ModifyStar(tile, s_DifficultTerrainPossibilityTemplate.m_Type, s_DifficultTerrainPossibilityTemplate.m_Mode, s_DifficultTerrainPossibilityTemplate.m_Hover, storeTiles);
			}
			else if (flag)
			{
				ModifyStar(tile, s_MoveHazardPossibilityTemplate.m_Type, s_MoveHazardPossibilityTemplate.m_Mode, s_MoveHazardPossibilityTemplate.m_Hover, storeTiles);
			}
			else
			{
				ModifyStar(tile, s_MovePossiblityTemplate.m_Type, s_MovePossiblityTemplate.m_Mode, s_MovePossiblityTemplate.m_Hover, storeTiles);
			}
		}
	}

	private bool ProcessPossibleMoveTileAsDoorOrChest(CClientTile tile, Dictionary<CClientTile, HexSelect_Control> tileStore, bool checkChests = true, bool checkDoors = true)
	{
		if (checkChests && s_ChestTiles.Contains(tile))
		{
			if (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null)
			{
				s_ChestTiles.Remove(tile);
				return false;
			}
			ModifyStar(tile, s_MoveChestDoorPossibilityTemplate.m_Type, s_MoveChestDoorPossibilityTemplate.m_Mode, s_MoveChestDoorPossibilityTemplate.m_Hover, tileStore);
			return true;
		}
		if (checkDoors && s_DoorTiles.Contains(tile))
		{
			CObjectDoor cObjectDoor = (CObjectDoor)tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
			if (cObjectDoor == null || cObjectDoor.IsDungeonEntrance || cObjectDoor.IsDungeonExit || cObjectDoor.DoorIsOpen)
			{
				s_DoorTiles.Remove(tile);
				return false;
			}
			ModifyStar(tile, s_MoveChestDoorPossibilityTemplate.m_Type, s_MoveChestDoorPossibilityTemplate.m_Mode, s_MoveChestDoorPossibilityTemplate.m_Hover, tileStore);
			return true;
		}
		return false;
	}

	private bool ProcessClosedDoorsForControlledAI(CClientTile tile, Dictionary<CClientTile, HexSelect_Control> tileStore)
	{
		if (s_DoorTiles.Contains(tile))
		{
			CObjectDoor cObjectDoor = (CObjectDoor)tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
			if (cObjectDoor == null || cObjectDoor.IsDungeonEntrance || cObjectDoor.IsDungeonExit || cObjectDoor.DoorIsOpen)
			{
				s_DoorTiles.Remove(tile);
				return false;
			}
			ModifyStar(tile, s_MoveChestDoorPossibilityTemplate.m_Type, s_MoveChestDoorPossibilityTemplate.m_Mode, s_MoveChestDoorPossibilityTemplate.m_Hover, tileStore);
			return true;
		}
		return false;
	}

	public bool IsMoveTileValid(CClientTile tile, bool ignoreBlocked, CActor movingActor, CActor.EType movingType)
	{
		try
		{
			if (tile == null)
			{
				return false;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y];
			if (cNode == null)
			{
				return false;
			}
			if (tile.m_Tile.m_HexMap != null && tile.m_Tile.m_Hex2Map != null)
			{
				if (!tile.m_Tile.m_HexMap.Revealed && !tile.m_Tile.m_Hex2Map.Revealed)
				{
					return false;
				}
			}
			else
			{
				if (tile.m_Tile.m_HexMap != null && !tile.m_Tile.m_HexMap.Revealed)
				{
					return false;
				}
				if (tile.m_Tile.m_Hex2Map != null && !tile.m_Tile.m_Hex2Map.Revealed)
				{
					return false;
				}
			}
			if (!cNode.Walkable || (cNode.Blocked && !ignoreBlocked))
			{
				return false;
			}
			CObjectDoor cObjectDoor = (CObjectDoor)tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
			if (cObjectDoor != null && (cObjectDoor.DoorIsLocked || (cObjectDoor.PropHealthDetails != null && cObjectDoor.PropHealthDetails.HasHealth && cObjectDoor.PropHealthDetails.CurrentHealth > 0)))
			{
				return false;
			}
			foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(tile.m_Tile.m_ArrayIndex))
			{
				if (item != null && !ignoreBlocked && !(item is CHeroSummonActor { DoesNotBlock: not false }) && !CActor.AreActorsAllied(item.Type, movingType) && !item.Deactivated)
				{
					return false;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.IsMoveTileValid().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return false;
		}
	}

	private void ShowMovePathStars()
	{
		try
		{
			s_TilesExpectedToDisplay.Clear();
			s_TilesExpectedToDisplay.AddRange(s_DungeonExitStars.Keys);
			s_TilesExpectedToDisplay.AddRange(s_PossibleMoveStars.Keys);
			s_TilesExpectedToDisplay.AddRange(s_PossibleMoveStarsRemainingInDirection.Keys);
			s_TilesExpectedToDisplay.AddRange(s_ImpossibleMoveChestDoorStars.Keys);
			s_CachedMoveStars.Clear();
			s_CachedMoveStars.AddRange(s_PathMoveStars.Keys);
			s_PathMoveStars.Clear();
			while (WaypointLine.s_Instance.m_Transform.childCount > 0)
			{
				ObjectPool.Recycle(WaypointLine.s_Instance.m_Transform.GetChild(0).gameObject, GlobalSettings.Instance.m_WaypointHolder);
			}
			WaypointLine.s_Instance.m_UpdateFlag = true;
			Waypoint.ClearValidSelectionTiles();
			CActor cActor = Waypoint.s_MovingActor;
			if (cActor == null)
			{
				return;
			}
			bool jump = false;
			bool fly = false;
			bool ignoreDifficultTerrain = false;
			bool ignoreHazardousTerrain = false;
			bool ignoreBlockedTileMoveCost = false;
			CAbilityMove cAbilityMove = null;
			CAbilityPull cAbilityPull = null;
			CAbilityPush cAbilityPush = null;
			bool carryOtherActors = false;
			CActor.EType carryType = CActor.EType.Unknown;
			if (Choreographer.s_Choreographer.m_CurrentAbility != null)
			{
				if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove)
				{
					cAbilityMove = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityMove;
					jump = cAbilityMove.Jump;
					fly = cAbilityMove.Fly;
					ignoreDifficultTerrain = cAbilityMove.IgnoreDifficultTerrain;
					ignoreHazardousTerrain = cAbilityMove.IgnoreHazardousTerrain;
					ignoreBlockedTileMoveCost = cAbilityMove.IgnoreBlockedTileMoveCost;
					carryOtherActors = cAbilityMove.CarryOtherActorsOnHex;
					if (cAbilityMove.ActorsToCarry.Count > 0)
					{
						int strength = 0;
						carryType = cAbilityMove.ActorsToCarry[0].Type;
						CAbilityMove.GetMoveBonuses(cAbilityMove.ActorsToCarry[0], out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, ref strength);
					}
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull)
				{
					cAbilityPull = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityPull;
					cActor = cAbilityPull.CurrentTarget;
					if (cActor is CEnemyActor)
					{
						fly = (cActor as CEnemyActor).CachedFlying;
					}
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush)
				{
					cAbilityPush = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityPush;
					cActor = cAbilityPush.CurrentTarget;
					if (cActor is CEnemyActor)
					{
						fly = (cActor as CEnemyActor).CachedFlying;
						fly = cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles) || fly;
					}
				}
			}
			if (cActor == null || !ScenarioManager.Scenario.HasActor(cActor))
			{
				return;
			}
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(cActor);
			if (gameObject == null)
			{
				return;
			}
			CClientTile cClientTile = Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile;
			ControllerInputPointer.CursorType = ((InteractableUnderMouse()?.GetComponent<TileBehaviour>()?.m_ClientTile != null) ? ECursorType.Default : ECursorType.Invalid);
			List<Point> list = new List<Point>();
			list.Add(cActor.ArrayIndex);
			if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush && cAbilityPush != null && cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked))
			{
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
				if ((cClientTile == null || !s_PossibleMoveStars.ContainsKey(cClientTile) || cClientTile == cClientTile2) && CAbilityPush.IsPushTileAdjacentToBlockedPushTile(cClientTile2.m_Tile, cAbilityPush.CurrentTarget, cAbilityPush.CurrentTarget.Type, cClientTile2.m_Tile.m_ArrayIndex, cAbilityPush.PushFromPoint, cAbilityPush.Strength, intoBlocked: true))
				{
					Waypoint.AddValidSelectionTiles(cActor.ArrayIndex);
					if (s_PossibleMoveStars.TryGetValue(cClientTile2, out var value))
					{
						value = s_PossibleMoveStars[cClientTile2];
						value.Type = HexSelect_Control.HexType.NegativeEffect;
						value.Mode = HexSelect_Control.HexMode.Selected;
						value.Hover = cClientTile2 == cClientTile;
						value.RefreshHexUI();
						if (!s_PathMoveStars.ContainsKey(cClientTile2))
						{
							s_PathMoveStars.Add(cClientTile2, value);
						}
					}
				}
			}
			list.AddRange(Waypoint.s_Waypoints.Select((GameObject w) => w.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex).ToList());
			if (Waypoint.GetLastWaypoint == null || Waypoint.GetLastWaypoint.MovesRemaining > 0)
			{
				if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
				{
					if (list.Count < 2)
					{
						if (cClientTile != null && s_PossibleMoveStars.ContainsKey(cClientTile) && !m_SelectingADoor)
						{
							list.Add(cClientTile.m_Tile.m_ArrayIndex);
						}
					}
					else if (cClientTile != null && s_PossibleMoveStarsRemainingInDirection.ContainsKey(cClientTile) && !m_SelectingADoor)
					{
						list.Add(cClientTile.m_Tile.m_ArrayIndex);
					}
				}
				else if (cAbilityPush != null)
				{
					if (cClientTile != null && s_PossibleMoveStars.ContainsKey(cClientTile) && s_CachedPushStars.Contains(cClientTile) && !m_SelectingADoor)
					{
						list.Add(cClientTile.m_Tile.m_ArrayIndex);
					}
				}
				else if (cAbilityPull != null)
				{
					if (cClientTile != null && s_PossibleMoveStars.ContainsKey(cClientTile) && s_CachedPullStars.Contains(cClientTile) && !m_SelectingADoor)
					{
						list.Add(cClientTile.m_Tile.m_ArrayIndex);
					}
				}
				else if (cClientTile != null && s_PossibleMoveStars.ContainsKey(cClientTile))
				{
					list.Add(cClientTile.m_Tile.m_ArrayIndex);
				}
			}
			if (list.Count < 2)
			{
				foreach (CClientTile s_CachedMoveStar in s_CachedMoveStars)
				{
					if (s_PossibleMoveStars.ContainsKey(s_CachedMoveStar))
					{
						ResetTileStarFromPathMoveToPossiblePath(s_CachedMoveStar, s_PossibleMoveStars[s_CachedMoveStar]);
					}
					else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && s_PossibleMoveStarsRemainingInDirection.ContainsKey(s_CachedMoveStar))
					{
						ResetTileStarFromPathMoveToPossiblePath(s_CachedMoveStar, s_PossibleMoveStarsRemainingInDirection[s_CachedMoveStar]);
					}
				}
				return;
			}
			List<Point> list2 = new List<Point>();
			bool foundPath;
			if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
			{
				list2.AddRange(CActor.FindStraightPath(list[0], list[list.Count - 1], jump || fly, out foundPath));
			}
			else if (cAbilityMove != null)
			{
				bool flag = false;
				if (Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor.OriginalType == CActor.EType.Enemy && Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor.MindControlDuration != CAbilityControlActor.EControlDurationType.None)
				{
					flag = GameState.IsParentUnderMyControl(Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor);
				}
				if (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor.IsUnderMyControl || flag)
				{
					bool flag2 = false;
					for (int num = 0; num < list.Count - 1; num++)
					{
						list2.AddRange(CActor.FindCharacterPath(cActor, list[num], list[num + 1], jump || fly, ignoreMoveCost: false, out foundPath, !(jump || fly), ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors, carryType));
						if (!foundPath)
						{
							flag2 = true;
						}
					}
					if (flag2 || CAbilityMove.CalculateMoveCost(list2, !fly, !jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost) > cAbilityMove.RemainingMoves)
					{
						list2.Clear();
						for (int num2 = 0; num2 < list.Count - 1; num2++)
						{
							list2.AddRange(CActor.FindCharacterPath(cActor, list[num2], list[num2 + 1], jump || fly, ignoreMoveCost: false, out foundPath, avoidTraps: false, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors, carryType));
						}
					}
				}
			}
			else
			{
				for (int num3 = 0; num3 < list.Count - 1; num3++)
				{
					list2.AddRange(CActor.FindCharacterPath(cActor, list[num3], list[num3 + 1], jump || fly, ignoreMoveCost: true, out foundPath, avoidTraps: false, ignoreDifficultTerrain));
				}
			}
			Vector3 position = gameObject.transform.position;
			ObjectPool.Spawn(GlobalSettings.Instance.m_WaypointHolder, WaypointLine.s_Instance.m_Transform, position + WaypointHolder.YOffset, Quaternion.identity).GetComponent<WaypointHolder>()?.EnableType(WaypointHolder.EWaypointType.Start);
			IEnumerable<CObjectTrap> source = from w in ScenarioManager.CurrentScenarioState.Maps.SelectMany((CMap sm) => sm.Props).OfType<CObjectTrap>()
				where !w.Activated
				select w;
			foreach (Point item in list2)
			{
				CClientTile cClientTile3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.X, item.Y];
				bool flag3 = false;
				if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
				{
					if (!s_PossibleMoveStars.ContainsKey(cClientTile3) && !s_PossibleMoveStarsRemainingInDirection.ContainsKey(cClientTile3))
					{
						if (!s_AllLineTiles.Contains(cClientTile3.m_Tile))
						{
							{
								foreach (CClientTile s_CachedMoveStar2 in s_CachedMoveStars)
								{
									if (s_PossibleMoveStars.ContainsKey(s_CachedMoveStar2))
									{
										ResetTileStarFromPathMoveToPossiblePath(s_CachedMoveStar2, s_PossibleMoveStars[s_CachedMoveStar2]);
									}
									else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && s_PossibleMoveStarsRemainingInDirection.ContainsKey(s_CachedMoveStar2))
									{
										ResetTileStarFromPathMoveToPossiblePath(s_CachedMoveStar2, s_PossibleMoveStarsRemainingInDirection[s_CachedMoveStar2]);
									}
								}
								return;
							}
						}
						flag3 = true;
					}
				}
				else if (!s_PossibleMoveStars.ContainsKey(cClientTile3))
				{
					flag3 = true;
				}
				if (flag3)
				{
					if (s_ChestTiles.Contains(cClientTile3) || s_DoorTiles.Contains(cClientTile3))
					{
						if (s_ChestTiles.Contains(cClientTile3) && cClientTile3.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null)
						{
							s_ChestTiles.Remove(cClientTile3);
						}
						ModifyStar(cClientTile3, s_MoveChestDoorPossibilityTemplate.m_Type, HexSelect_Control.HexMode.Reach, s_MoveChestDoorPossibilityTemplate.m_Hover, s_PossibleMoveStars);
						s_TilesExpectedToDisplay.Add(cClientTile3);
						s_PossibleMoveStars[cClientTile3].RefreshHexUI();
					}
					else
					{
						ModifyStar(cClientTile3, HexSelect_Control.HexType.Move, HexSelect_Control.HexMode.Reach, hover: false, s_PossibleMoveStars);
						s_TilesExpectedToDisplay.Add(cClientTile3);
						s_PossibleMoveStars[cClientTile3].RefreshHexUI();
					}
				}
				List<CActiveBonus> list3 = CActiveBonus.FindApplicableActiveBonuses(cActor, CAbility.EAbilityType.AddCondition).ToList();
				bool flag4 = false;
				foreach (CActiveBonus item2 in list3)
				{
					if (item2.ActiveBonusIsActivatedByTile(cClientTile3.m_Tile))
					{
						flag4 = true;
						if (item2.BaseCard is CItem cItem)
						{
							ShowHoveredTileTooltip(cClientTile3, LocalizationManager.GetTranslation(cItem.Name), LocalizationManager.GetTranslation("Item_" + cItem.YMLData.Art.RemoveSpaces()));
						}
					}
				}
				bool flag5 = false;
				bool flag6 = false;
				bool flag7 = Waypoint.GetLastWaypoint != null && Waypoint.GetLastWaypoint.ClientTile.m_Tile.m_ArrayIndex == item;
				CObjectProp cObjectProp = cClientTile3.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap);
				if (cObjectProp != null && source.Contains(cObjectProp))
				{
					flag5 = true;
				}
				if (!flag5)
				{
					if (cClientTile3.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null || cClientTile3.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null)
					{
						flag5 = true;
					}
					else if (cClientTile3.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainRubble) != null || cClientTile3.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainWater) != null)
					{
						flag6 = true;
					}
				}
				if (cClientTile != null && Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush { AdditionalPushEffect: var additionalPushEffect } cAbilityPush2 && additionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked) && item == list2.Last())
				{
					int tileDistance = ScenarioManager.GetTileDistance(cAbilityPush2.CurrentTarget.ArrayIndex.X, cAbilityPush2.CurrentTarget.ArrayIndex.Y, cClientTile.m_Tile.m_ArrayIndex.X, cClientTile.m_Tile.m_ArrayIndex.Y);
					int num4 = cAbilityPush2.Strength - tileDistance;
					if (num4 > 0 && CAbilityPush.IsPushTileAdjacentToBlockedPushTile(cClientTile.m_Tile, cAbilityPush2.CurrentTarget, cAbilityPush2.CurrentTarget.Type, cClientTile.m_Tile.m_ArrayIndex, cAbilityPush2.PushFromPoint, num4, intoBlocked: true))
					{
						flag5 = true;
					}
				}
				HexSelect_Control hexSelect_Control = null;
				if (s_PossibleMoveStars.ContainsKey(cClientTile3))
				{
					hexSelect_Control = s_PossibleMoveStars[cClientTile3];
				}
				else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && s_PossibleMoveStarsRemainingInDirection.ContainsKey(cClientTile3))
				{
					hexSelect_Control = s_PossibleMoveStarsRemainingInDirection[cClientTile3];
				}
				if (hexSelect_Control == null)
				{
					return;
				}
				if (s_ChestTiles.Contains(cClientTile3) || s_DoorTiles.Contains(cClientTile3))
				{
					hexSelect_Control.Type = s_MoveChestDoorPossibilityTemplate.m_Type;
					hexSelect_Control.Mode = HexSelect_Control.HexMode.Selected;
				}
				else if (flag4)
				{
					hexSelect_Control.Type = HexSelect_Control.HexType.ActiveBonusCondition;
					hexSelect_Control.Mode = HexSelect_Control.HexMode.PossibleTarget;
				}
				else if (flag5)
				{
					hexSelect_Control.Type = s_MoveHazardPossibilityTemplate.m_Type;
					hexSelect_Control.Mode = ((!flag7) ? HexSelect_Control.HexMode.PossibleTarget : HexSelect_Control.HexMode.Selected);
				}
				else if (flag6)
				{
					hexSelect_Control.Type = s_DifficultTerrainPossibilityTemplate.m_Type;
					hexSelect_Control.Mode = ((!flag7) ? HexSelect_Control.HexMode.PossibleTarget : HexSelect_Control.HexMode.Selected);
				}
				else
				{
					hexSelect_Control.Type = s_MovePossiblityTemplate.m_Type;
					hexSelect_Control.Mode = ((!flag7) ? HexSelect_Control.HexMode.PossibleTarget : HexSelect_Control.HexMode.Selected);
				}
				hexSelect_Control.Hover = cClientTile3 == cClientTile;
				hexSelect_Control.RefreshHexUI();
				if (hexSelect_Control.Hover)
				{
					ControllerInputPointer.CursorType = ECursorType.Targeted;
				}
				if (!s_PathMoveStars.ContainsKey(cClientTile3))
				{
					s_PathMoveStars.Add(cClientTile3, hexSelect_Control);
				}
				WaypointHolder component = ObjectPool.Spawn(GlobalSettings.Instance.m_WaypointHolder, WaypointLine.s_Instance.m_Transform, cClientTile3.m_GameObject.transform.position + WaypointHolder.YOffset, Quaternion.identity).GetComponent<WaypointHolder>();
				if (item == cActor.ArrayIndex)
				{
					component?.EnableType(WaypointHolder.EWaypointType.Start);
				}
				else if (list.Contains(item))
				{
					component?.EnableType(WaypointHolder.EWaypointType.Node);
				}
				else
				{
					component?.EnableType(WaypointHolder.EWaypointType.Small);
				}
				if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
				{
					if (Waypoint.s_Waypoints.Count > 0)
					{
						if (s_PossibleMoveStarsRemainingInDirection.ContainsKey(cClientTile3))
						{
							Waypoint.AddValidSelectionTiles(cClientTile3.m_Tile.m_ArrayIndex);
						}
					}
					else
					{
						Waypoint.AddValidSelectionTiles(cClientTile3.m_Tile.m_ArrayIndex);
					}
				}
				else if (cAbilityPush != null)
				{
					if (s_CachedPushStars.Contains(cClientTile3))
					{
						Waypoint.AddValidSelectionTiles(cClientTile3.m_Tile.m_ArrayIndex);
					}
				}
				else if (cAbilityPull != null)
				{
					if (s_CachedPullStars.Contains(cClientTile3))
					{
						Waypoint.AddValidSelectionTiles(cClientTile3.m_Tile.m_ArrayIndex);
					}
				}
				else
				{
					Waypoint.AddValidSelectionTiles(cClientTile3.m_Tile.m_ArrayIndex);
				}
			}
			foreach (CClientTile s_CachedMoveStar3 in s_CachedMoveStars)
			{
				if (!s_PathMoveStars.ContainsKey(s_CachedMoveStar3))
				{
					if (s_PossibleMoveStars.ContainsKey(s_CachedMoveStar3))
					{
						ResetTileStarFromPathMoveToPossiblePath(s_CachedMoveStar3, s_PossibleMoveStars[s_CachedMoveStar3]);
					}
					else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && s_PossibleMoveStarsRemainingInDirection.ContainsKey(s_CachedMoveStar3))
					{
						ResetTileStarFromPathMoveToPossiblePath(s_CachedMoveStar3, s_PossibleMoveStarsRemainingInDirection[s_CachedMoveStar3]);
					}
				}
			}
			ClearUnusedStars(s_TilesExpectedToDisplay);
			WaypointLine.s_Instance.m_UpdateFlag = true;
			if (Waypoint.GetLastWaypoint?.ClientTile == cClientTile)
			{
				Choreographer.s_Choreographer.readyButton.ButtonComponent.Select();
			}
			else if (EventSystem.current.currentSelectedGameObject == Choreographer.s_Choreographer.readyButton.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.MovePathStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ResetTileStarFromPathMoveToPossiblePath(CClientTile tile, HexSelect_Control hexControl)
	{
		if (!s_PossibleMoveStars.ContainsKey(tile) && !s_PossibleMoveStarsRemainingInDirection.ContainsKey(tile))
		{
			return;
		}
		hexControl.Hover = false;
		if (s_ChestTiles.Contains(tile) || s_DoorTiles.Contains(tile))
		{
			if (s_ChestTiles.Contains(tile) && tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null)
			{
				s_ChestTiles.Remove(tile);
				bool flag = hexControl.Type == HexSelect_Control.HexType.NegativeEffect;
				hexControl.Mode = (flag ? HexSelect_Control.HexMode.PossibleTarget : HexSelect_Control.HexMode.Reach);
			}
			hexControl.Mode = s_MoveChestDoorPossibilityTemplate.m_Mode;
		}
		else
		{
			bool flag2 = false;
			bool flag3 = false;
			IEnumerable<CObjectTrap> source = from w in ScenarioManager.CurrentScenarioState.Maps.SelectMany((CMap sm) => sm.Props).OfType<CObjectTrap>()
				where !w.Activated
				select w;
			CObjectProp cObjectProp = tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap);
			if (cObjectProp != null && source.Contains(cObjectProp))
			{
				flag2 = true;
			}
			if (!flag2)
			{
				if (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null)
				{
					flag2 = true;
				}
				else if (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainRubble) != null || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainWater) != null)
				{
					flag3 = true;
				}
			}
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(Waypoint.s_MovingActor, CAbility.EAbilityType.AddCondition).ToList();
			bool flag4 = false;
			foreach (CActiveBonus item in list)
			{
				if (item.ActiveBonusIsActivatedByTile(tile.m_Tile))
				{
					flag4 = true;
				}
			}
			if (flag4)
			{
				hexControl.Mode = HexSelect_Control.HexMode.PossibleTarget;
				hexControl.Type = HexSelect_Control.HexType.ActiveBonusCondition;
			}
			else if (flag2)
			{
				hexControl.Mode = s_MoveHazardPossibilityTemplate.m_Mode;
				hexControl.Type = s_MoveHazardPossibilityTemplate.m_Type;
			}
			else if (flag3)
			{
				hexControl.Mode = s_DifficultTerrainPossibilityTemplate.m_Mode;
				hexControl.Type = s_DifficultTerrainPossibilityTemplate.m_Type;
			}
			else
			{
				hexControl.Mode = s_MovePossiblityTemplate.m_Mode;
				hexControl.Type = s_MovePossiblityTemplate.m_Type;
			}
		}
		hexControl.HexUpdate();
	}

	public void ResetTileStarFromAttackPathToPossibleAttackPath(CClientTile tile, HexSelect_Control hexControl)
	{
		if (s_PossibleAttackPathStars.ContainsKey(tile))
		{
			hexControl.Hover = false;
			hexControl.Type = HexSelect_Control.HexType.Move;
			hexControl.Mode = HexSelect_Control.HexMode.Reach;
			hexControl.HexUpdate();
		}
	}

	private void ShowPossibleTargetSelectionHexes(CClientTile originTile = null)
	{
		switch (CurrentAbilityDisplayType)
		{
		case EAbilityDisplayType.Normal:
			DisplayStandardAttackStars(originTile);
			break;
		case EAbilityDisplayType.AreaOfEffect:
			DisplayAOEStars(originTile);
			break;
		case EAbilityDisplayType.EnemyAreaOfEffect:
			DisplayEnemyAOEStars();
			break;
		case EAbilityDisplayType.ObjectiveAbility:
			DisplayTargetingAbilityStars(originTile, isObjective: true);
			break;
		case EAbilityDisplayType.TargetingAbility:
			DisplayTargetingAbilityStars(originTile);
			break;
		case EAbilityDisplayType.NegativeAbility:
			DisplayTargetingAbilityStars(originTile, isObjective: false, isPositive: false);
			break;
		case EAbilityDisplayType.SelectObjectPosition:
			DisplaySelectObjectPositionStars(originTile);
			break;
		case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
			DisplaySelectObjectPositionAOEStars(originTile);
			break;
		case EAbilityDisplayType.SelectPath:
			DisplayAttackPathStars(originTile);
			break;
		case EAbilityDisplayType.RedistributeDamageAbility:
			DisplayRedistributeDamageStars();
			break;
		}
	}

	private void GetAbilityValues(CAbility ability, ref int range, ref bool rangedAttack, ref bool includeCasterTile)
	{
		try
		{
			if (ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
			{
				includeCasterTile = true;
			}
			else
			{
				includeCasterTile = false;
			}
			rangedAttack = ability.Range > 1;
			if (ability is CAbilityAttack)
			{
				rangedAttack = !(ability as CAbilityAttack).IsMeleeAttack;
			}
			else if (CurrentAbilityDisplayType == EAbilityDisplayType.AreaOfEffect && ability.AreaEffect != null && ability.AreaEffect.Melee)
			{
				rangedAttack = false;
			}
			range = ((ability.Range <= 1) ? 1 : ability.Range);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.GetAbilityValues().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public bool AlreadySelected(CClientTile clientTile)
	{
		switch (CurrentAbilityDisplayType)
		{
		case EAbilityDisplayType.AreaOfEffect:
		case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
			if (m_SavedAbility != null)
			{
				CActor item = ScenarioManager.Scenario.FindActorAt(clientTile.m_Tile.m_ArrayIndex);
				if (!m_SavedAbility.ValidTilesInAreaEffectedIncludingBlocked.Contains(clientTile.m_Tile))
				{
					return m_SavedAbility.ActorsToTarget.Contains(item);
				}
				return true;
			}
			break;
		case EAbilityDisplayType.Normal:
		case EAbilityDisplayType.TargetingAbility:
		case EAbilityDisplayType.NegativeAbility:
		case EAbilityDisplayType.ObjectiveAbility:
			if (m_SavedAbility != null)
			{
				CActor item2 = ScenarioManager.Scenario.FindActorAt(clientTile.m_Tile.m_ArrayIndex);
				return m_SavedAbility.ActorsToTarget.Contains(item2);
			}
			break;
		case EAbilityDisplayType.SelectObjectPosition:
			if (m_SavedAbility != null)
			{
				return m_SavedAbility.TilesSelected.Contains(clientTile.m_Tile);
			}
			break;
		}
		return false;
	}

	public int CurrentNumberSelectedTargets(List<CTile> optionalTileList = null)
	{
		switch (CurrentAbilityDisplayType)
		{
		case EAbilityDisplayType.Normal:
		case EAbilityDisplayType.AreaOfEffect:
		case EAbilityDisplayType.TargetingAbility:
		case EAbilityDisplayType.NegativeAbility:
		case EAbilityDisplayType.SelectPath:
		case EAbilityDisplayType.ObjectiveAbility:
			if (m_SavedAbility != null)
			{
				return m_SavedAbility.ActorsToTarget.Count;
			}
			break;
		case EAbilityDisplayType.SelectObjectPosition:
		case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
			if (m_SavedAbility != null)
			{
				return m_SavedAbility.TilesSelected.Count;
			}
			break;
		}
		return 0;
	}

	private void DisplayStandardAttackStars(CClientTile originTile = null)
	{
		try
		{
			s_TilesExpectedToDisplay.Clear();
			s_TilesExpectedToDisplay.AddRange(s_DungeonExitStars.Keys);
			for (int i = 0; i < s_EnemiesToRetaliate.Count; i++)
			{
				s_EnemiesToRetaliate[i].m_WorldspacePanelUI.RetaliateWarnEffect(active: false);
			}
			s_EnemiesToRetaliate.Clear();
			UpdateRetaliateDamage(0);
			if (Choreographer.s_Choreographer.m_CurrentActor == null)
			{
				return;
			}
			CAbility savedAbility = m_SavedAbility;
			bool flag = m_SavedAbility.AbilityType == CAbility.EAbilityType.Damage;
			bool flag2 = m_SavedAbility.AbilityType == CAbility.EAbilityType.Attack;
			if (savedAbility == null || savedAbility.TargetingActor.Type == CActor.EType.Enemy || savedAbility.TargetingActor.Type == CActor.EType.HeroSummon)
			{
				return;
			}
			int range = 1;
			bool includeCasterTile = false;
			bool rangedAttack = false;
			GetAbilityValues(savedAbility, ref range, ref rangedAttack, ref includeCasterTile);
			List<CClientTile> clientTiles;
			if (savedAbility.AllTargetsOnMovePath)
			{
				clientTiles = savedAbility.ValidActorsInRange.Select((CActor s) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s.ArrayIndex.X, s.ArrayIndex.Y]).ToList();
			}
			else if (savedAbility.AreaEffectBackup != null)
			{
				clientTiles = savedAbility.ActorsToTarget.Select((CActor s) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s.ArrayIndex.X, s.ArrayIndex.Y]).ToList();
				clientTiles.AddRange(from w in CurrentAbilityRangeTiles(savedAbility, range, includeCasterTile, rangedAttack)
					where !clientTiles.Contains(w)
					select w);
			}
			else
			{
				AbilityData.MiscAbilityData miscAbilityData = savedAbility.MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.AllTargetsAdjacentToParentTargets.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData2 = savedAbility.MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.Value)
					{
						clientTiles = savedAbility.ValidActorsInRange.Select((CActor s) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s.ArrayIndex.X, s.ArrayIndex.Y]).ToList();
						clientTiles.AddRange(savedAbility.TilesInRange.Select((CTile s) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s.m_ArrayIndex.X, s.m_ArrayIndex.Y]).ToList());
						goto IL_024e;
					}
				}
				clientTiles = CurrentAbilityRangeTiles(savedAbility, range, includeCasterTile, rangedAttack).ToList();
			}
			goto IL_024e;
			IL_024e:
			List<Tuple<CActor, bool>> list = new List<Tuple<CActor, bool>>();
			foreach (CClientTile tile in clientTiles)
			{
				CActor cActor = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor e) => e.ArrayIndex == tile.m_Tile.m_ArrayIndex);
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) => e.ArrayIndex == tile.m_Tile.m_ArrayIndex);
				bool flag3 = savedAbility.ActorsToTarget.Contains(cEnemyActor) || savedAbility.ActorsToTarget.Contains(cActor);
				bool flag4 = tile == originTile;
				if (savedAbility.NumberTargets != 0 && savedAbility.MaxTargetsSelected() && !flag3 && tile != originTile)
				{
					continue;
				}
				HexSelect_Control.HexMode mode;
				HexSelect_Control.HexType type;
				if (savedAbility.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && !flag)
				{
					mode = HexSelect_Control.HexMode.PossibleTarget;
					type = HexSelect_Control.HexType.PositiveEffect;
					if (flag3)
					{
						mode = HexSelect_Control.HexMode.Selected;
						if (cEnemyActor != null)
						{
							list.Add(new Tuple<CActor, bool>(cEnemyActor, item2: true));
						}
						if (cActor != null)
						{
							list.Add(new Tuple<CActor, bool>(cActor, item2: true));
						}
					}
				}
				else if (cEnemyActor != null || cActor != null)
				{
					mode = HexSelect_Control.HexMode.PossibleTarget;
					type = HexSelect_Control.HexType.NegativeEffect;
					if (flag3)
					{
						mode = HexSelect_Control.HexMode.Selected;
						if (cEnemyActor != null)
						{
							list.Add(new Tuple<CActor, bool>(cEnemyActor, item2: false));
						}
						if (cActor != null)
						{
							list.Add(new Tuple<CActor, bool>(cActor, item2: false));
						}
					}
				}
				else if (savedAbility.MaxTargetsSelected() && flag4)
				{
					mode = HexSelect_Control.HexMode.Cursor;
					type = HexSelect_Control.HexType.Move;
				}
				else
				{
					mode = HexSelect_Control.HexMode.Reach;
					type = HexSelect_Control.HexType.NegativeEffect;
				}
				s_TilesExpectedToDisplay.Add(tile);
				ModifyStar(tile, type, mode, flag4, s_AttackStars);
			}
			Choreographer.s_Choreographer.OnActorHexTargeted(null);
			DisplayStandardAttackHoverStars(out var totalRetaliate, originTile, flag);
			Choreographer.s_Choreographer.OnActorsHexTargeted(list, untargetOthers: false);
			Choreographer.s_Choreographer.OnActorsHexSelected(list.Select((Tuple<CActor, bool> it) => it.Item1).ToList());
			if (flag2)
			{
				for (int num = 0; num < list.Count; num++)
				{
					CActor actor = list[num].Item1;
					if (s_EnemiesToRetaliate.Exists((ActorBehaviour it) => it.Actor == actor))
					{
						continue;
					}
					bool foundPath;
					List<Point> list2 = ScenarioManager.PathFinder.FindPath(Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex, actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
					if (!foundPath)
					{
						continue;
					}
					int num2 = actor.CalculateRetaliate(savedAbility, list2.Count);
					if (num2 > 0)
					{
						ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(actor));
						if (actorBehaviour != null && actorBehaviour.m_WorldspacePanelUI != null)
						{
							s_EnemiesToRetaliate.Add(actorBehaviour);
							actorBehaviour.m_WorldspacePanelUI.RetaliateWarnEffect(active: true);
						}
						totalRetaliate += num2;
					}
				}
			}
			if (flag2 && totalRetaliate > 0)
			{
				UpdateRetaliateDamage(totalRetaliate);
			}
			ClearUnusedStars(s_TilesExpectedToDisplay);
			SetHexBorders(s_AttackStars);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayStandardAttackStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void DisplayStandardAttackHoverStars(out int totalRetaliate, CClientTile originTile = null, bool damageAbility = false)
	{
		totalRetaliate = 0;
		try
		{
			for (int i = 0; i < s_EnemiesToRetaliate.Count; i++)
			{
				s_EnemiesToRetaliate[i].m_WorldspacePanelUI.RetaliateWarnEffect(active: false);
			}
			s_EnemiesToRetaliate.Clear();
			CAbility savedAbility = m_SavedAbility;
			bool flag = savedAbility != null && savedAbility.AbilityType == CAbility.EAbilityType.Attack;
			TileBehaviour tileBehaviour = Interactable()?.GetComponent<TileBehaviour>();
			ControllerInputPointer.CursorType = ((!(InteractableUnderMouse()?.GetComponent<TileBehaviour>() == null)) ? ECursorType.Default : ECursorType.Invalid);
			CClientTile tile;
			if (FFSNetwork.IsClient)
			{
				if (originTile == null)
				{
					return;
				}
				tile = originTile;
			}
			else
			{
				if (tileBehaviour == null)
				{
					return;
				}
				tile = tileBehaviour.m_ClientTile;
			}
			if (tile == null || !s_AttackStars.ContainsKey(tile))
			{
				return;
			}
			HexSelect_Control hexSelect_Control = s_AttackStars[tile];
			hexSelect_Control.Hover = true;
			CAbility savedAbility2 = m_SavedAbility;
			if (savedAbility2 == null)
			{
				return;
			}
			if (!savedAbility2.AllTargetsOnAttackPath)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) => e.ArrayIndex == tile.m_Tile.m_ArrayIndex);
				Choreographer.s_Choreographer.OnEnemyHexHighlighted(cEnemyActor);
				Choreographer.s_Choreographer.OnActorHexTargeted(cEnemyActor, hexSelect_Control.Type == HexSelect_Control.HexType.PositiveEffect);
				if (cEnemyActor == null)
				{
					return;
				}
				bool foundPath;
				List<Point> list = ScenarioManager.PathFinder.FindPath(Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex, cEnemyActor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
				if (!foundPath)
				{
					return;
				}
				ControllerInputPointer.CursorType = ECursorType.Targeted;
				int num = cEnemyActor.CalculateRetaliate(savedAbility2, list.Count);
				if (flag && num > 0)
				{
					ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(cEnemyActor));
					if (actorBehaviour != null && actorBehaviour.m_WorldspacePanelUI != null)
					{
						s_EnemiesToRetaliate.Add(actorBehaviour);
						actorBehaviour.m_WorldspacePanelUI.RetaliateWarnEffect(active: true);
					}
					UpdateRetaliateDamage(num);
				}
				totalRetaliate += num;
				if (m_SavedAbility is CAbilityAttack cAbilityAttack && cAbilityAttack.IsTargetOutsideOfInitialRange(cEnemyActor) && cAbilityAttack.AttackSummary.AddRangeActiveBonuses.FirstOrDefault((CActiveBonus it) => it.Ability.IsItemAbility)?.BaseCard is CItem cItem)
				{
					ShowHoveredActorTooltip(cEnemyActor, LocalizationManager.GetTranslation(cItem.Name), LocalizationManager.GetTranslation("Item_" + cItem.YMLData.Art.RemoveSpaces()));
				}
				return;
			}
			CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) => e.ArrayIndex == tile.m_Tile.m_ArrayIndex);
			if (cEnemyActor2 == null)
			{
				Choreographer.s_Choreographer.OnEnemyHexHighlighted(cEnemyActor2);
				Choreographer.s_Choreographer.OnActorHexTargeted(cEnemyActor2);
				return;
			}
			bool foundPath2;
			List<Point> list2 = ScenarioManager.PathFinder.FindPath(Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex, cEnemyActor2.ArrayIndex, !damageAbility && ((CAbilityAttack)m_SavedAbility).IsMeleeAttack, ignoreMoveCost: true, out foundPath2);
			if (!foundPath2)
			{
				return;
			}
			ControllerInputPointer.CursorType = ECursorType.Targeted;
			int num2 = 1;
			List<CEnemyActor> list3 = new List<CEnemyActor>();
			foreach (Point point in list2)
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[point.X, point.Y];
				if (cClientTile == null)
				{
					return;
				}
				CEnemyActor cEnemyActor3 = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor x) => x.ArrayIndex == point);
				s_TilesExpectedToDisplay.Add(cClientTile);
				ModifyStar(cClientTile, HexSelect_Control.HexType.NegativeEffect, (cEnemyActor3 != null) ? HexSelect_Control.HexMode.Selected : HexSelect_Control.HexMode.Reach, hover: false, s_AttackStars);
				if (cEnemyActor3 != null)
				{
					list3.Add(cEnemyActor2);
					int num3 = cEnemyActor3.CalculateRetaliate(m_SavedAbility, num2);
					if (flag && num3 > 0)
					{
						ActorBehaviour actorBehaviour2 = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(cEnemyActor3));
						if (actorBehaviour2 != null && actorBehaviour2.m_WorldspacePanelUI != null)
						{
							s_EnemiesToRetaliate.Add(actorBehaviour2);
							actorBehaviour2.m_WorldspacePanelUI.RetaliateWarnEffect(active: true);
						}
					}
					totalRetaliate += num3;
				}
				num2++;
			}
			Choreographer.s_Choreographer.OnEnemyHexHighlighted(list3);
			Choreographer.s_Choreographer.OnActorsHexTargeted(list3);
			if (flag && totalRetaliate > 0)
			{
				UpdateRetaliateDamage(totalRetaliate);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayStandardAttackHoverStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void SetTargetTooltip(GameObject obj, Action onRemovedTarget = null)
	{
		if (m_TargetTooltip != obj)
		{
			m_OnRemoveTargetTooltip?.Invoke();
		}
		m_TargetTooltip = obj;
		m_OnRemoveTargetTooltip = onRemovedTarget;
	}

	private void ClearTargetTooltip()
	{
		SetTargetTooltip(null);
	}

	private void ShowHoveredActorTooltip(CActor actor, string tooltipTitle, string tooltipText)
	{
		GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(actor);
		if (m_TargetTooltip != gameObject)
		{
			ClearTargetTooltip();
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
		if (actorBehaviour != null && actorBehaviour.m_WorldspacePanelUI != null)
		{
			actorBehaviour.m_WorldspacePanelUI.ShowInfoTooltip(tooltipTitle, tooltipText);
			SetTargetTooltip(gameObject, actorBehaviour.m_WorldspacePanelUI.HideInfoTooltip);
		}
	}

	private void ShowHoveredTileTooltip(CClientTile tile, string tooltipTitle, string tooltipText)
	{
		if (m_TargetTooltip != tile.m_TileBehaviour.gameObject)
		{
			ClearTargetTooltip();
		}
		tile.m_TileBehaviour.ShowTooltip(tooltipTitle, tooltipText);
		SetTargetTooltip(tile.m_TileBehaviour.gameObject, tile.m_TileBehaviour.HideTooltip);
	}

	public void SetAOELocked(bool locked)
	{
		m_AOELocked = locked;
		FFSNet.Console.LogInfo("AOE LOCK: " + locked);
	}

	public bool IsAOELocked()
	{
		return m_AOELocked;
	}

	private void DisplayEnemyAOEStars()
	{
		try
		{
			if (m_SavedAbility == null)
			{
				return;
			}
			ClearStars();
			foreach (CTile item in m_SavedAbility.ValidTilesInAreaAffected)
			{
				CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				if (clientTile != null && !(clientTile.m_Tile.m_ArrayIndex == Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex))
				{
					bool flag = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor e) => e.ArrayIndex == clientTile.m_Tile.m_ArrayIndex) != null;
					CreateStar(clientTile, HexSelect_Control.HexType.NegativeEffect, (!flag) ? HexSelect_Control.HexMode.PossibleTarget : HexSelect_Control.HexMode.Selected, hover: false, s_AttackStars);
				}
			}
			SetHexBorders(s_AttackStars);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayEnemyAOEStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void DisplayAOEStars(CClientTile originTile = null)
	{
		try
		{
			if (m_SavedAbility == null)
			{
				return;
			}
			s_TilesExpectedToDisplay.Clear();
			s_TilesExpectedToDisplay.AddRange(s_DungeonExitStars.Keys);
			int range = 1;
			bool includeCasterTile = false;
			bool rangedAttack = false;
			GetAbilityValues(m_SavedAbility, ref range, ref rangedAttack, ref includeCasterTile);
			List<CClientTile> list = new List<CClientTile>();
			if (!m_SavedAbility.MaxTargetsSelected())
			{
				list.AddRange(CurrentAbilityRangeTiles(m_SavedAbility, range, rangedAttack, includeCasterTile));
				foreach (CActor item4 in m_SavedAbility.ValidActorsInRange.ToList())
				{
					CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item4.ArrayIndex.X, item4.ArrayIndex.Y];
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			for (int i = 0; i < s_EnemiesToRetaliate.Count; i++)
			{
				s_EnemiesToRetaliate[i].m_WorldspacePanelUI.RetaliateWarnEffect(active: false);
			}
			s_EnemiesToRetaliate.Clear();
			UpdateRetaliateDamage(0);
			CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				if (AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent is CAutoAOETileHover)
				{
					cClientTile = CAutoTileClick.TileIndexToClientTile((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoAOETileHover).PlacementTile);
				}
			}
			else if (AutoTestController.s_ShouldRecordUIActionsForAutoTest)
			{
				AutoTestController.s_Instance.LogAOETileHover(cClientTile);
			}
			if (m_SavedAbility.AreaEffect != null && !m_AOELocked)
			{
				m_SavedAbility.UpdateAreaEffect(cClientTile?.m_Tile, m_AreaEffectAngle);
			}
			int num = 0;
			List<CEnemyActor> list2 = new List<CEnemyActor>();
			List<CTile> list3 = new List<CTile>();
			list3.AddRange(m_SavedAbility.ValidTilesInAreaEffectedIncludingBlocked);
			if (m_SavedAbility.ActorsToTarget != null)
			{
				foreach (CActor item5 in m_SavedAbility.ActorsToTarget.ToList())
				{
					CTile item2 = ScenarioManager.Tiles[item5.ArrayIndex.X, item5.ArrayIndex.Y];
					if (!list3.Contains(item2))
					{
						list3.Add(item2);
					}
				}
			}
			foreach (CTile item6 in list3)
			{
				CClientTile item3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item6.m_ArrayIndex.X, item6.m_ArrayIndex.Y];
				if (!list.Contains(item3))
				{
					list.Add(item3);
				}
			}
			List<CActor> list4 = new List<CActor>();
			List<CActor> list5 = new List<CActor>();
			foreach (CClientTile clientTile in list)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) => e.ArrayIndex == clientTile.m_Tile.m_ArrayIndex);
				bool num2 = cEnemyActor != null;
				s_TilesExpectedToDisplay.Add(clientTile);
				bool flag = (!m_AOELocked && list3.Contains(clientTile.m_Tile)) || (m_AOELocked && clientTile == cClientTile);
				HexSelect_Control.HexType type = HexSelect_Control.HexType.NegativeEffect;
				HexSelect_Control.HexMode mode;
				if (num2)
				{
					list2.Add(cEnemyActor);
					mode = HexSelect_Control.HexMode.PossibleTarget;
					if (m_AOELocked && list3.Contains(clientTile.m_Tile) && m_SavedAbility.ActorsToTarget.Contains(cEnemyActor))
					{
						mode = HexSelect_Control.HexMode.Selected;
						list4.Add(cEnemyActor);
						list5.Add(cEnemyActor);
					}
					else if (flag)
					{
						list4.Add(cEnemyActor);
					}
				}
				else
				{
					mode = HexSelect_Control.HexMode.Reach;
					if (m_AOELocked)
					{
						if (list3.Contains(clientTile.m_Tile))
						{
							mode = HexSelect_Control.HexMode.SelectedAOEWithoutTarget;
						}
						else if (clientTile == cClientTile)
						{
							type = HexSelect_Control.HexType.Move;
							mode = HexSelect_Control.HexMode.Cursor;
						}
					}
				}
				ModifyStar(clientTile, type, mode, flag, s_AttackStars);
				if (!num2)
				{
					continue;
				}
				bool foundPath;
				List<Point> list6 = ScenarioManager.PathFinder.FindPath(Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex, cEnemyActor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
				if (foundPath)
				{
					int num3 = cEnemyActor.CalculateRetaliate(m_SavedAbility, list6.Count);
					if (num3 > 0)
					{
						ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(cEnemyActor));
						if (actorBehaviour != null && actorBehaviour.m_WorldspacePanelUI != null)
						{
							s_EnemiesToRetaliate.Add(actorBehaviour);
							actorBehaviour.m_WorldspacePanelUI.RetaliateWarnEffect(active: true);
						}
					}
					num += num3;
				}
				if (num > 0)
				{
					UpdateRetaliateDamage(num);
				}
			}
			Choreographer.s_Choreographer.OnEnemyHexHighlighted(list2);
			Choreographer.s_Choreographer.OnActorsHexTargeted(list4);
			Choreographer.s_Choreographer.OnActorsHexSelected(list5);
			ClearUnusedStars(s_TilesExpectedToDisplay);
			List<CClientTile> hoveredTiles = (from s in s_AttackStars
				where s.Value.Hover
				select s.Key).ToList();
			SetHexBorders(s_AttackStars, hoveredTiles);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayAOEStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void DisplaySelectObjectPositionAOEStars(CClientTile originTile = null)
	{
		try
		{
			if (m_SavedAbility == null)
			{
				return;
			}
			s_TilesExpectedToDisplay.Clear();
			s_TilesExpectedToDisplay.AddRange(s_DungeonExitStars.Keys);
			int range = 1;
			bool includeCasterTile = false;
			bool rangedAttack = false;
			GetAbilityValues(m_SavedAbility, ref range, ref rangedAttack, ref includeCasterTile);
			List<CClientTile> list = CurrentAbilityRangeTiles(m_SavedAbility, range, rangedAttack, includeCasterTile).ToList();
			for (int i = 0; i < s_EnemiesToRetaliate.Count; i++)
			{
				s_EnemiesToRetaliate[i].m_WorldspacePanelUI.RetaliateWarnEffect(active: false);
			}
			s_EnemiesToRetaliate.Clear();
			UpdateRetaliateDamage(0);
			CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				if (AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent is CAutoAOETileHover)
				{
					cClientTile = CAutoTileClick.TileIndexToClientTile((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoAOETileHover).PlacementTile);
				}
			}
			else if (AutoTestController.s_ShouldRecordUIActionsForAutoTest)
			{
				AutoTestController.s_Instance.LogAOETileHover(cClientTile);
			}
			if (m_SavedAbility.AreaEffect != null && !m_AOELocked)
			{
				m_SavedAbility.UpdateAreaEffect(cClientTile?.m_Tile, m_AreaEffectAngle);
			}
			List<CTile> list2 = new List<CTile>();
			HexSelect_Control.HexMode mode = ((!m_AOELocked) ? HexSelect_Control.HexMode.PossibleTarget : HexSelect_Control.HexMode.Selected);
			foreach (CTile item in m_SavedAbility.ValidTilesInAreaAffected)
			{
				if (m_SavedAbility.SubAbilities.Count > 0)
				{
					foreach (CAbility subAbility in m_SavedAbility.SubAbilities)
					{
						if (subAbility is CAbilityAttack cAbilityAttack)
						{
							list2.AddRange(GameState.GetTilesInRange(item.m_ArrayIndex, cAbilityAttack.Range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false));
						}
					}
				}
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				if (cClientTile2 != null)
				{
					s_TilesExpectedToDisplay.Add(cClientTile2);
					ModifyStar(cClientTile2, HexSelect_Control.HexType.PositiveEffect, mode, hover: true, s_AttackStars);
				}
			}
			foreach (CClientTile item2 in list)
			{
				if (!s_TilesExpectedToDisplay.Contains(item2) && !m_SavedAbility.ValidTilesInAreaAffected.Contains(item2.m_Tile))
				{
					s_TilesExpectedToDisplay.Add(item2);
					bool flag = false;
					if (list2.Count > 0 && list2.Contains(item2.m_Tile) && ScenarioManager.Scenario.FindActorAt(item2.m_Tile.m_ArrayIndex) != null)
					{
						ModifyStar(item2, HexSelect_Control.HexType.NegativeEffect, mode, hover: false, s_AttackStars);
						flag = true;
					}
					if (!flag)
					{
						ModifyStar(item2, HexSelect_Control.HexType.PositiveEffect, HexSelect_Control.HexMode.Reach, hover: false, s_AttackStars);
					}
				}
			}
			ClearUnusedStars(s_TilesExpectedToDisplay);
			SetHexBorders(s_AttackStars);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayAOEStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private bool ListenForTargetingInputEvents()
	{
		bool result = false;
		EAbilityDisplayType currentAbilityDisplayType = CurrentAbilityDisplayType;
		if ((currentAbilityDisplayType == EAbilityDisplayType.AreaOfEffect || currentAbilityDisplayType == EAbilityDisplayType.SelectObjectPositionAreaOfEffect) && m_SavedAbility != null)
		{
			result = ((m_SavedAbility.Range <= 1) ? RotateAOEWithMouse() : RotateAOEWithKeyboard());
		}
		if (CurrentDisplayState == WorldSpaceStarDisplayState.LevelEditorSpawning)
		{
			result = RotateAOEWithKeyboard();
		}
		return result;
	}

	private bool RotateAOEWithMouse()
	{
		try
		{
			if (!m_NewTilePointedAtThisFrame && !AutoTestController.s_AutoLogPlaybackInProgress)
			{
				return false;
			}
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.X, Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.Y];
			CClientTile cClientTile2 = Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile;
			if (AutoTestController.s_AutoLogPlaybackInProgress && AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent is CAutoAOETileHover)
			{
				cClientTile2 = CAutoTileClick.TileIndexToClientTile((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoAOETileHover).PlacementTile);
			}
			if (cClientTile2 == null)
			{
				return false;
			}
			Vector3 forward = cClientTile2.m_GameObject.transform.position - cClientTile.m_GameObject.transform.position;
			forward.y = 0f;
			m_AreaEffectAngle = (int)Quaternion.LookRotation(forward).eulerAngles.y;
			m_AreaEffectAngle = 360 - m_AreaEffectAngle - 270;
			if (m_AreaEffectAngle < 0)
			{
				m_AreaEffectAngle = 360 + m_AreaEffectAngle;
			}
			m_AreaEffectAngle = m_AreaEffectAngle / 60 * 60;
			if (m_AreaEffectAngle != m_LastAreaEffectAngle)
			{
				Debug.Log("New area Effect Angle:" + m_AreaEffectAngle);
				m_LastAreaEffectAngle = m_AreaEffectAngle;
				return true;
			}
			Debug.Log("Same area Effect Angle:" + m_AreaEffectAngle);
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.RotateAOEWithMouse().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return false;
		}
	}

	private bool RotateAOEWithKeyboard()
	{
		bool result = false;
		if (InputManager.GetWasPressed(KeyAction.ROTATE_TARGET))
		{
			RotateAOEClockwise(turnRight: true);
			result = true;
		}
		if (InputManager.GetWasPressed(KeyAction.ROTATE_TARGET_BUTTON))
		{
			RotateAOEClockwise(turnRight: true);
			result = true;
		}
		return result;
	}

	public void RotateAOEClockwise(bool turnRight)
	{
		try
		{
			if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				if (AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent is CAutoAOERotate cAutoAOERotate)
				{
					turnRight = cAutoAOERotate.IsClockWise;
				}
			}
			else if (AutoTestController.s_ShouldRecordUIActionsForAutoTest)
			{
				AutoTestController.s_Instance.LogAOERotate(turnRight);
			}
			if (m_LastRecievedRotateDirection + 0.3f < Timekeeper.instance.m_GlobalClock.time)
			{
				m_TurningRight = turnRight;
				m_LastRecievedRotateDirection = Timekeeper.instance.m_GlobalClock.time;
			}
			if (m_TurningRight)
			{
				m_AreaEffectAngle -= 60;
				m_AreaEffectAngle = ((m_AreaEffectAngle < 0) ? (m_AreaEffectAngle + 360) : m_AreaEffectAngle);
			}
			else
			{
				m_AreaEffectAngle += 60;
				m_AreaEffectAngle = ((m_AreaEffectAngle >= 360) ? (m_AreaEffectAngle - 360) : m_AreaEffectAngle);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.RotateAOEClockwise().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Clear()
	{
		try
		{
			ClearStars();
			ResetPlayerAttackType();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.Clear().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void DisplayTargetingAbilityStars(CClientTile originTile = null, bool isObjective = false, bool isPositive = true)
	{
		try
		{
			ClearStars();
			while (m_SavedAbility.IsUpdating)
			{
				Debug.LogWarning($"[WorldspaceStarHexDisplay] waiting for the ScenarioRuleClient work thread for {20} ms");
				Thread.Sleep(20);
			}
			CAbility savedAbility = m_SavedAbility;
			if (savedAbility == null)
			{
				return;
			}
			List<CActor> list = new List<CActor>();
			List<CActor> list2 = new List<CActor>();
			CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			List<CActor> list3 = new List<CActor>();
			if (!(savedAbility is CAbilityTargeting { OneTargetAtATime: not false }) || savedAbility.ActorsToTarget.Count <= 0)
			{
				list3.AddRange(savedAbility.ValidActorsInRange);
			}
			list3.AddRange(savedAbility.ActorsToTarget);
			list3 = list3.Distinct().ToList();
			List<CActor> list4 = new List<CActor>();
			list4.AddRange(savedAbility.ActorsToTarget);
			ControllerInputPointer.CursorType = ECursorType.Default;
			foreach (CActor item in list3)
			{
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.ArrayIndex.X, item.ArrayIndex.Y];
				bool flag = list4.Contains(item);
				bool flag2 = cClientTile != null && cClientTile2 == cClientTile;
				ModifyStar(cClientTile2, isObjective ? HexSelect_Control.HexType.Objective : (isPositive ? HexSelect_Control.HexType.PositiveEffect : HexSelect_Control.HexType.NegativeEffect), (!flag) ? HexSelect_Control.HexMode.PossibleTarget : HexSelect_Control.HexMode.Selected, flag2, s_AbilityStars);
				if (flag2 || flag)
				{
					list2.Add(item);
				}
				if (flag)
				{
					list.Add(item);
				}
				if (flag2 && savedAbility is CAbilityAddDoom && item is CEnemyActor)
				{
					int count = item.CachedDoomActiveBonuses.Count;
					List<CActiveBonus> cachedAddDoomSlotActiveBonuses = Choreographer.s_Choreographer.m_CurrentActor.CachedAddDoomSlotActiveBonuses;
					if (Choreographer.s_Choreographer.m_CurrentActor.DoomSlots <= count)
					{
						string tooltipTitle = string.Join("\n\n", cachedAddDoomSlotActiveBonuses.Select((CActiveBonus it) => "<color=#" + UIInfoTools.Instance.mainColor.ToHex() + ">" + LocalizationManager.GetTranslation(it.BaseCard.Name) + "</color>\n" + ((it.Layout == null) ? string.Empty : ActivePropertyLookup(it.Ability, LocalisationAndPropertyLookup(it.Ability, it.Layout.ListLayouts.FirstOrDefault())))));
						ShowHoveredActorTooltip(item, tooltipTitle, null);
					}
				}
				if (flag2)
				{
					ControllerInputPointer.CursorType = ECursorType.Targeted;
				}
			}
			if (!savedAbility.MaxTargetsSelected())
			{
				foreach (CClientTile item2 in CurrentAbilityRangeTiles(savedAbility, (savedAbility.Targeting == CAbility.EAbilityTargeting.Range) ? savedAbility.Range : 0, includeCasterTile: true, savedAbility.Targeting == CAbility.EAbilityTargeting.Range && savedAbility.Range > 1).ToList())
				{
					if (!s_AbilityStars.ContainsKey(item2))
					{
						bool hover = cClientTile != null && item2 == cClientTile;
						ModifyStar(item2, isObjective ? HexSelect_Control.HexType.Objective : (isPositive ? HexSelect_Control.HexType.PositiveEffect : HexSelect_Control.HexType.NegativeEffect), HexSelect_Control.HexMode.Reach, hover, s_AbilityStars);
					}
				}
			}
			Choreographer.s_Choreographer.OnActorsHexTargeted(list2, isPositive);
			Choreographer.s_Choreographer.OnActorsHexSelected(list);
			SetHexBorders(s_AbilityStars);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayTargetingAbilityStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00014", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private string ActivePropertyLookup(CAbility ability, string text)
	{
		while (text.Contains('^'.ToString()))
		{
			string text2 = CardLayoutRow.GetKey(text, '^').Replace(" ", "");
			int num = 0;
			if (text2 == ability.Name)
			{
				num += ability.Strength;
			}
			text = CardLayoutRow.ReplaceKey(text, '^', num.ToString());
		}
		return text;
	}

	private string LocalisationAndPropertyLookup(CAbility ability, string desc)
	{
		if (desc == null)
		{
			desc = string.Empty;
		}
		desc = CreateLayout.LocaliseText(desc);
		desc = (desc.Contains('*'.ToString()) ? CardLayoutRow.ReplaceKey(desc, '*', ability.Strength.ToString()) : desc);
		return desc;
	}

	private void DisplaySelectObjectPositionStars(CClientTile originTile = null)
	{
		try
		{
			if (m_SavedAbility == null)
			{
				return;
			}
			ClearStars();
			if (s_CachedAbilityRangeTiles.Count <= 0)
			{
				CurrentAbilityRangeTiles(m_SavedAbility, m_SavedAbility.Range, includeCasterTile: false, rangedAttack: false, m_SavedAbility.TilesSelected);
			}
			CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			List<CClientTile> list = new List<CClientTile>();
			foreach (CClientTile s_CachedAbilityRangeTile in s_CachedAbilityRangeTiles)
			{
				if (m_TileFilters != null)
				{
					bool flag = false;
					foreach (CAbilityFilter.EFilterTile tileFilter in m_TileFilters)
					{
						if (CAbilityFilter.IsValidTile(s_CachedAbilityRangeTile.m_Tile, tileFilter))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
				CObjectProp cObjectProp = s_CachedAbilityRangeTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
				if (cObjectProp != null && cObjectProp is CObjectDoor && (cObjectProp as CObjectDoor).IsDungeonEntrance)
				{
					continue;
				}
				CObjectProp cObjectProp2 = s_CachedAbilityRangeTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap);
				if (cObjectProp2 != null && cObjectProp2 is CObjectTrap && (cObjectProp2 as CObjectTrap).Activated)
				{
					continue;
				}
				bool flag2 = m_SavedAbility.TilesSelected?.Contains(s_CachedAbilityRangeTile.m_Tile) ?? false;
				bool flag3 = cClientTile != null && cClientTile == s_CachedAbilityRangeTile;
				if (flag3)
				{
					ControllerInputPointer.CursorType = ECursorType.Targeted;
				}
				if (!m_SavedAbility.AllTargets || flag2)
				{
					if (flag2)
					{
						list.Add(s_CachedAbilityRangeTile);
					}
					ModifyStar(s_CachedAbilityRangeTile, HexSelect_Control.HexType.PositiveEffect, flag2 ? HexSelect_Control.HexMode.Selected : HexSelect_Control.HexMode.Reach, flag3, s_AbilityStars);
				}
			}
			if (m_ObjectSpawnType == ScenarioManager.ObjectImportType.HeroSummons && m_SavedAbility is CAbilitySummon { SelectedSummonYMLData: not null } cAbilitySummon && ((cClientTile != null && s_CurrentlyActiveStars.ContainsKey(cClientTile)) || !list.IsNullOrEmpty()))
			{
				if (cClientTile != null && s_CurrentlyActiveStars.ContainsKey(cClientTile) && list.Count < cAbilitySummon.NumberTargets)
				{
					list.Add(cClientTile);
				}
				int num = 0;
				foreach (CClientTile item in list)
				{
					string text = cAbilitySummon.SelectedSummonID + "_" + num;
					num++;
					if (!Choreographer.s_Choreographer.m_ClientPlacementPreviews.ContainsKey(text))
					{
						Choreographer.s_Choreographer.m_ClientPlacementPreviews[text] = Choreographer.s_Choreographer.CreatePreviewPlayer(item, CActor.EType.HeroSummon, cAbilitySummon.SelectedSummonYMLData.Model);
					}
					PreviewPlacement(text, item, Quaternion.identity);
				}
			}
			else
			{
				HidePreviewedCharacterPlacements();
			}
			SetHexBorders(s_AbilityStars);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplaySelectObjectPositionStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00015", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void PreviewPlacement(string actor, CClientTile tile, Quaternion rotation)
	{
		GameObject gameObject = null;
		if (actor != null)
		{
			gameObject = Choreographer.s_Choreographer.m_ClientPlacementPreviews[actor];
		}
		if (gameObject != null)
		{
			gameObject.transform.position = tile.m_GameObject.transform.position;
			gameObject.transform.rotation = rotation;
			gameObject.SetActive(value: true);
			previewedCharacterPlacements.Add(gameObject);
		}
	}

	private void ShowPossibleAttackPathStars()
	{
		try
		{
			ClearStars();
			Waypoint.ClearValidSelectionTiles();
			if (Choreographer.s_Choreographer.m_CurrentAbility == null || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack))
			{
				return;
			}
			CAbilityAttack cAbilityAttack = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityAttack;
			CActor currentActor = Choreographer.s_Choreographer.m_CurrentActor;
			int remainingMoves = ((Waypoint.s_Waypoints.Count > 0) ? Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().MovesRemaining : cAbilityAttack.Range);
			Point startingPoint = ((Waypoint.s_Waypoints.Count == 0) ? Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex : Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex);
			CActor.EType type = Choreographer.s_Choreographer.m_CurrentActor.Type;
			List<CClientTile> list = new List<CClientTile>();
			for (int i = 0; i < ScenarioManager.Height; i++)
			{
				for (int j = 0; j < ScenarioManager.Width; j++)
				{
					CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[j, i];
					if (cClientTile != null)
					{
						list.Add(cClientTile);
					}
				}
			}
			PossibleMoveTileStarCreation(list, ignoreBlocked: true, currentActor, remainingMoves, startingPoint, type, s_PossibleAttackPathStars, null, null, LOS: true);
			SetHexBorders(s_PossibleAttackPathStars);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.ShowPossibleAttackPathStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00016", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void DisplayAttackPathStars(CClientTile originTile = null)
	{
		try
		{
			if (!(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack cAbilityAttack))
			{
				return;
			}
			s_TilesExpectedToDisplay.Clear();
			s_TilesExpectedToDisplay.AddRange(s_DungeonExitStars.Keys);
			s_TilesExpectedToDisplay.AddRange(s_PossibleAttackPathStars.Keys);
			s_CachedMoveStars.Clear();
			s_CachedMoveStars.AddRange(s_PathAttackStars.Keys);
			s_PathAttackStars.Clear();
			while (WaypointLine.s_Instance.m_Transform.childCount > 0)
			{
				ObjectPool.Recycle(WaypointLine.s_Instance.m_Transform.GetChild(0).gameObject, GlobalSettings.Instance.m_WaypointHolder);
			}
			WaypointLine.s_Instance.m_UpdateFlag = true;
			Waypoint.ClearValidSelectionTiles();
			CActor currentActor = Choreographer.s_Choreographer.m_CurrentActor;
			if (Choreographer.s_Choreographer.FindClientActorGameObject(currentActor) == null)
			{
				return;
			}
			CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			List<Point> list = new List<Point>();
			list.Add(currentActor.ArrayIndex);
			list.AddRange(Waypoint.s_Waypoints.Select((GameObject w) => w.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex).ToList());
			int num = ((Waypoint.s_Waypoints.Count > 0) ? Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().MovesRemaining : cAbilityAttack.Range);
			if (cClientTile != null && s_PossibleAttackPathStars.ContainsKey(cClientTile) && num > 0)
			{
				list.Add(cClientTile.m_Tile.m_ArrayIndex);
			}
			if (list.Count < 2)
			{
				foreach (CClientTile s_CachedMoveStar in s_CachedMoveStars)
				{
					if (s_PossibleAttackPathStars.ContainsKey(s_CachedMoveStar))
					{
						ResetTileStarFromAttackPathToPossibleAttackPath(s_CachedMoveStar, s_PossibleAttackPathStars[s_CachedMoveStar]);
					}
				}
				return;
			}
			List<Point> list2 = new List<Point>();
			for (int num2 = 0; num2 < list.Count - 1; num2++)
			{
				list2.AddRange(CActor.FindCharacterPath(currentActor, list[num2], list[num2 + 1], ignoreBlocked: true, ignoreMoveCost: true, out var _));
			}
			Vector3 position = Choreographer.s_Choreographer.FindClientActorGameObject(currentActor).transform.position;
			ObjectPool.Spawn(GlobalSettings.Instance.m_WaypointHolder, WaypointLine.s_Instance.m_Transform, position + WaypointHolder.YOffset, Quaternion.identity).GetComponent<WaypointHolder>()?.EnableType(WaypointHolder.EWaypointType.Start);
			foreach (Point item in list2)
			{
				CClientTile waypointTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.X, item.Y];
				bool flag = false;
				if (!s_PossibleAttackPathStars.ContainsKey(waypointTile))
				{
					flag = true;
				}
				if (flag)
				{
					if (s_ChestTiles.Contains(waypointTile) || s_DoorTiles.Contains(waypointTile))
					{
						if (s_ChestTiles.Contains(waypointTile) && waypointTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null)
						{
							s_ChestTiles.Remove(waypointTile);
						}
						ModifyStar(waypointTile, s_MoveChestDoorPossibilityTemplate.m_Type, HexSelect_Control.HexMode.Reach, s_MoveChestDoorPossibilityTemplate.m_Hover, s_PossibleAttackPathStars);
						s_TilesExpectedToDisplay.Add(waypointTile);
						s_PossibleAttackPathStars[waypointTile].RefreshHexUI();
					}
					else
					{
						ModifyStar(waypointTile, HexSelect_Control.HexType.Move, HexSelect_Control.HexMode.Reach, hover: false, s_PossibleAttackPathStars);
						s_TilesExpectedToDisplay.Add(waypointTile);
						s_PossibleAttackPathStars[waypointTile].RefreshHexUI();
					}
				}
				HexSelect_Control hexSelect_Control = null;
				if (s_PossibleAttackPathStars.ContainsKey(waypointTile))
				{
					hexSelect_Control = s_PossibleAttackPathStars[waypointTile];
				}
				if (hexSelect_Control == null)
				{
					return;
				}
				bool flag2 = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) => e.ArrayIndex == waypointTile.m_Tile.m_ArrayIndex) != null;
				hexSelect_Control.Type = (flag2 ? HexSelect_Control.HexType.NegativeEffect : HexSelect_Control.HexType.NegativeEffect);
				hexSelect_Control.Mode = ((!flag2) ? HexSelect_Control.HexMode.PossibleTarget : HexSelect_Control.HexMode.Selected);
				hexSelect_Control.Hover = waypointTile == cClientTile;
				hexSelect_Control.RefreshHexUI();
				if (!s_PathAttackStars.ContainsKey(waypointTile))
				{
					s_PathAttackStars.Add(waypointTile, hexSelect_Control);
				}
				WaypointHolder component = ObjectPool.Spawn(GlobalSettings.Instance.m_WaypointHolder, WaypointLine.s_Instance.m_Transform, waypointTile.m_GameObject.transform.position + WaypointHolder.YOffset, Quaternion.identity).GetComponent<WaypointHolder>();
				if (item == currentActor.ArrayIndex)
				{
					component?.EnableType(WaypointHolder.EWaypointType.Start);
				}
				else if (list.Contains(item))
				{
					component?.EnableType(WaypointHolder.EWaypointType.Node);
				}
				else
				{
					component?.EnableType(WaypointHolder.EWaypointType.Small);
				}
				Waypoint.AddValidSelectionTiles(waypointTile.m_Tile.m_ArrayIndex);
			}
			foreach (CClientTile s_CachedMoveStar2 in s_CachedMoveStars)
			{
				if (!s_PathAttackStars.ContainsKey(s_CachedMoveStar2) && s_PossibleAttackPathStars.ContainsKey(s_CachedMoveStar2))
				{
					ResetTileStarFromAttackPathToPossibleAttackPath(s_CachedMoveStar2, s_PossibleAttackPathStars[s_CachedMoveStar2]);
				}
			}
			ClearUnusedStars(s_TilesExpectedToDisplay);
			WaypointLine.s_Instance.m_UpdateFlag = true;
			if (Waypoint.GetLastWaypoint?.ClientTile == cClientTile)
			{
				Choreographer.s_Choreographer.readyButton.ButtonComponent.Select();
			}
			else if (EventSystem.current.currentSelectedGameObject == Choreographer.s_Choreographer.readyButton.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayAttackPathStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void StoreDistributeDamageService(DistributeDamageService distributeDamageService)
	{
		m_SavedDistributeDamageService = distributeDamageService;
	}

	private void SetDisplayRedistributeDamageInitialState()
	{
		CShowRedistributeDamageUI_MessageData cShowRedistributeDamageUI_MessageData = (Choreographer.s_Choreographer.LastMessage as CShowRedistributeDamageUI_MessageData) ?? m_SavedRedistributeDamageUIMessage;
		if (cShowRedistributeDamageUI_MessageData != null)
		{
			m_SavedRedistributeDamageUIMessage = cShowRedistributeDamageUI_MessageData;
		}
	}

	private void DisplayRedistributeDamageStars()
	{
		try
		{
			s_TilesExpectedToDisplay.Clear();
			s_TilesExpectedToDisplay.AddRange(s_DungeonExitStars.Keys);
			if (Choreographer.s_Choreographer.m_CurrentActor == null)
			{
				return;
			}
			SetDisplayRedistributeDamageInitialState();
			CAbility redistributeDamageAbility = m_SavedRedistributeDamageUIMessage.m_RedistributeDamageAbility;
			if (redistributeDamageAbility == null || redistributeDamageAbility.TargetingActor.Type == CActor.EType.Enemy || redistributeDamageAbility.TargetingActor.Type == CActor.EType.HeroSummon)
			{
				return;
			}
			List<Tuple<CActor, bool>> list = new List<Tuple<CActor, bool>>();
			foreach (CActor item in m_SavedRedistributeDamageUIMessage.m_ActorsToRedistributeBetween)
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.ArrayIndex.X, item.ArrayIndex.Y];
				HexSelect_Control.HexMode mode = HexSelect_Control.HexMode.PossibleTarget;
				HexSelect_Control.HexType type = HexSelect_Control.HexType.Move;
				int assignedPoints = m_SavedDistributeDamageService.GetAssignedPoints(item);
				if (assignedPoints < 0)
				{
					mode = HexSelect_Control.HexMode.Selected;
					type = HexSelect_Control.HexType.NegativeEffect;
					list.Add(new Tuple<CActor, bool>(item, item2: false));
				}
				else if (assignedPoints > 0)
				{
					mode = HexSelect_Control.HexMode.Selected;
					type = HexSelect_Control.HexType.PositiveEffect;
					list.Add(new Tuple<CActor, bool>(item, item2: true));
				}
				s_TilesExpectedToDisplay.Add(cClientTile);
				ModifyStar(cClientTile, type, mode, hover: false, s_AttackStars);
			}
			ClearUnusedStars(s_TilesExpectedToDisplay);
			SetHexBorders(s_AttackStars);
			Choreographer.s_Choreographer.OnActorsHexTargeted(list, untargetOthers: true);
			Choreographer.s_Choreographer.OnActorsHexSelected(list.Select((Tuple<CActor, bool> it) => it.Item1).ToList());
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayStandardAttackStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private GameObject CreateStar(CClientTile tile, HexSelect_Control.HexType type, HexSelect_Control.HexMode mode, bool hover, Dictionary<CClientTile, HexSelect_Control> container)
	{
		try
		{
			GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.m_GenericHexStar, tile.m_GameObject.transform);
			if (gameObject != null)
			{
				HexSelect_Control component = gameObject.GetComponent<HexSelect_Control>();
				component.Type = type;
				component.Mode = mode;
				component.Hover = hover;
				if (!container.ContainsKey(tile))
				{
					container.Add(tile, component);
				}
				container[tile] = component;
				if (!s_CurrentlyActiveStars.ContainsKey(tile))
				{
					s_CurrentlyActiveStars.Add(tile, container);
				}
				SetStarPos(gameObject, tile);
				return gameObject;
			}
			throw new Exception("Generic hex star was not spawned");
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.CreateStar().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00018", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return null;
	}

	private GameObject ModifyStar(CClientTile tile, HexSelect_Control.HexType type, HexSelect_Control.HexMode mode, bool hover, Dictionary<CClientTile, HexSelect_Control> container)
	{
		try
		{
			if (s_CurrentlyActiveStars.ContainsKey(tile))
			{
				Dictionary<CClientTile, HexSelect_Control> dictionary = s_CurrentlyActiveStars[tile];
				if (dictionary != null && dictionary == container)
				{
					HexSelect_Control hexSelect_Control = container[tile];
					hexSelect_Control.Type = type;
					hexSelect_Control.Mode = mode;
					hexSelect_Control.Hover = hover;
					return hexSelect_Control.gameObject;
				}
				GameObject objInstance = dictionary[tile].gameObject;
				dictionary.Remove(tile);
				s_CurrentlyActiveStars.Remove(tile);
				ObjectPool.Recycle(objInstance, GlobalSettings.Instance.m_GenericHexStar);
				return ModifyStar(tile, type, mode, hover, container);
			}
			return CreateStar(tile, type, mode, hover, container);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.ModifyStar().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00019", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return null;
	}

	private void SetHexBorders(Dictionary<CClientTile, HexSelect_Control> tileStars, List<CClientTile> hoveredTiles = null)
	{
		try
		{
			HashSet<CClientTile> activeTiles = new HashSet<CClientTile>(tileStars.Keys);
			foreach (KeyValuePair<CClientTile, HexSelect_Control> tileStar in tileStars)
			{
				HexSelect_Control value = tileStar.Value;
				value.SetNeighbors(tileStar.Key, activeTiles, hoveredTiles);
				value.RefreshHexUI(forceUpdate: false);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.SetHexBorders().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void RefreshHexesWithAllBorders(Dictionary<CClientTile, HexSelect_Control> tileStars)
	{
		try
		{
			new HashSet<CClientTile>();
			foreach (KeyValuePair<CClientTile, HexSelect_Control> tileStar in tileStars)
			{
				HexSelect_Control value = tileStar.Value;
				value.SetAllNeighborsOn();
				value.RefreshHexUI(forceUpdate: false);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.RefreshHexesWithAllBorders().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void SetStarPos(GameObject starGameObject, CClientTile tile)
	{
		starGameObject.transform.SetParent(ClientScenarioManager.s_ClientScenarioManager.m_Board.transform);
		Vector3 position = new Vector3(tile.m_GameObject.transform.position.x, tile.m_GameObject.transform.position.y, tile.m_GameObject.transform.position.z);
		starGameObject.transform.position = position;
	}

	public void ClearStars(bool possibleMove = true, bool pathMove = true, bool attack = true, bool attackPath = true, bool possibleAttackPath = true, bool ability = true, bool placement = true, bool placementSelection = true, bool allHighlightedTiles = true, bool impossibleMoveChestDoor = true, bool possibleMoveDirection = true, bool cursorHighlightedTiles = true, bool exitDungeon = false)
	{
		try
		{
			Choreographer.s_Choreographer.OnActorsHexSelected();
			Choreographer.s_Choreographer.OnActorHexTargeted(null);
			if (pathMove)
			{
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_PathMoveStar in s_PathMoveStars)
				{
					if (!(s_PathMoveStar.Value == null))
					{
						ResetTileStarFromPathMoveToPossiblePath(s_PathMoveStar.Key, s_PathMoveStar.Value);
					}
				}
				s_PathMoveStars.Clear();
				while (WaypointLine.s_Instance.m_Transform.childCount > 0)
				{
					ObjectPool.Recycle(WaypointLine.s_Instance.m_Transform.GetChild(0).gameObject, GlobalSettings.Instance.m_WaypointHolder);
				}
				WaypointLine.s_Instance.m_UpdateFlag = true;
			}
			if (possibleMove)
			{
				RemoveSubsetFromAllActiveList(s_PossibleMoveStars);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_PossibleMoveStar in s_PossibleMoveStars)
				{
					if (!(s_PossibleMoveStar.Value == null))
					{
						ObjectPool.Recycle(s_PossibleMoveStar.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				s_PossibleMoveStars.Clear();
			}
			if (possibleMoveDirection)
			{
				RemoveSubsetFromAllActiveList(s_PossibleMoveStarsRemainingInDirection);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> item in s_PossibleMoveStarsRemainingInDirection)
				{
					if (!(item.Value == null))
					{
						ObjectPool.Recycle(item.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				s_PossibleMoveStarsRemainingInDirection.Clear();
			}
			if (attack)
			{
				RemoveSubsetFromAllActiveList(s_AttackStars);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_AttackStar in s_AttackStars)
				{
					if (!(s_AttackStar.Value == null))
					{
						ObjectPool.Recycle(s_AttackStar.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				s_AttackStars.Clear();
			}
			if (attackPath)
			{
				RemoveSubsetFromAllActiveList(s_PathAttackStars);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_PathAttackStar in s_PathAttackStars)
				{
					if (!(s_PathAttackStar.Value == null))
					{
						ResetTileStarFromAttackPathToPossibleAttackPath(s_PathAttackStar.Key, s_PathAttackStar.Value);
					}
				}
				s_PathAttackStars.Clear();
				while (WaypointLine.s_Instance.m_Transform.childCount > 0)
				{
					ObjectPool.Recycle(WaypointLine.s_Instance.m_Transform.GetChild(0).gameObject, GlobalSettings.Instance.m_WaypointHolder);
				}
				WaypointLine.s_Instance.m_UpdateFlag = true;
			}
			if (possibleAttackPath)
			{
				RemoveSubsetFromAllActiveList(s_PossibleAttackPathStars);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_PossibleAttackPathStar in s_PossibleAttackPathStars)
				{
					if (!(s_PossibleAttackPathStar.Value == null))
					{
						ObjectPool.Recycle(s_PossibleAttackPathStar.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				s_PossibleAttackPathStars.Clear();
			}
			if (ability)
			{
				RemoveSubsetFromAllActiveList(s_AbilityStars);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_AbilityStar in s_AbilityStars)
				{
					if (!(s_AbilityStar.Value == null))
					{
						ObjectPool.Recycle(s_AbilityStar.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				s_AbilityStars.Clear();
			}
			if (placement)
			{
				RemoveSubsetFromAllActiveList(s_PlacementStars);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_PlacementStar in s_PlacementStars)
				{
					if (!(s_PlacementStar.Value == null))
					{
						ObjectPool.Recycle(s_PlacementStar.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				s_PlacementStars.Clear();
			}
			if (placementSelection)
			{
				if (s_SelectedPlacementStars != null && s_PlacementStars.ContainsKey(s_SelectedPlacementStars))
				{
					s_PlacementStars[s_SelectedPlacementStars].SetHover(hover: false);
				}
				s_SelectedPlacementStars = null;
			}
			if (allHighlightedTiles)
			{
				RemoveSubsetFromAllActiveList(s_HighlightAllStars);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_HighlightAllStar in s_HighlightAllStars)
				{
					if (!(s_HighlightAllStar.Value == null))
					{
						ObjectPool.Recycle(s_HighlightAllStar.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				s_HighlightAllStars.Clear();
			}
			if (impossibleMoveChestDoor)
			{
				RemoveSubsetFromAllActiveList(s_ImpossibleMoveChestDoorStars);
				foreach (KeyValuePair<CClientTile, HexSelect_Control> s_ImpossibleMoveChestDoorStar in s_ImpossibleMoveChestDoorStars)
				{
					if (!(s_ImpossibleMoveChestDoorStar.Value == null))
					{
						ObjectPool.Recycle(s_ImpossibleMoveChestDoorStar.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				s_ImpossibleMoveChestDoorStars.Clear();
			}
			if (!exitDungeon)
			{
				return;
			}
			RemoveSubsetFromAllActiveList(s_DungeonExitStars);
			foreach (KeyValuePair<CClientTile, HexSelect_Control> s_DungeonExitStar in s_DungeonExitStars)
			{
				if (!(s_DungeonExitStar.Value == null))
				{
					ObjectPool.Recycle(s_DungeonExitStar.Value.gameObject, GlobalSettings.Instance.m_GenericHexStar);
				}
			}
			s_DungeonExitStars.Clear();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.ClearStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RemoveSubsetFromAllActiveList(Dictionary<CClientTile, HexSelect_Control> dictionaryToRemove)
	{
		foreach (CClientTile key in dictionaryToRemove.Keys)
		{
			s_CurrentlyActiveStars.Remove(key);
		}
	}

	public void ClearUnusedStars(List<CClientTile> tilesThatShouldRemain)
	{
		List<CClientTile> list = new List<CClientTile>();
		foreach (CClientTile key2 in s_CurrentlyActiveStars.Keys)
		{
			if (!tilesThatShouldRemain.Contains(key2))
			{
				list.Add(key2);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			CClientTile key = list[i];
			Dictionary<CClientTile, HexSelect_Control> dictionary = s_CurrentlyActiveStars[key];
			GameObject objInstance = dictionary[key].gameObject;
			dictionary.Remove(key);
			s_CurrentlyActiveStars.Remove(key);
			ObjectPool.Recycle(objInstance, GlobalSettings.Instance.m_GenericHexStar);
		}
	}

	public void DisplayCursorHoverStar()
	{
		CInteractable cInteractable = Interactable();
		TileBehaviour tileBehaviour = null;
		if (cInteractable != null)
		{
			tileBehaviour = cInteractable.GetComponent<TileBehaviour>();
		}
		if (tileBehaviour != null && tileBehaviour.m_ClientTile == null)
		{
			ControllerInputPointer.CursorType = ECursorType.Invalid;
		}
		if (tileBehaviour == null || tileBehaviour.m_ClientTile == null)
		{
			ShowActorStatPanelForTile(null);
			s_CursorHighlightedTile = null;
			WorldspaceUITools.Instance.DisableAllHoveredOutlines();
			return;
		}
		CClientTile clientTile = tileBehaviour.m_ClientTile;
		if (clientTile.m_Tile.m_HexMap != null && !clientTile.m_Tile.m_HexMap.Revealed && (clientTile.m_Tile.m_Hex2Map == null || !clientTile.m_Tile.m_Hex2Map.Revealed))
		{
			ControllerInputPointer.CursorType = ECursorType.Invalid;
			if (!CanCursorHighlightUnrevealedTile(clientTile))
			{
				s_CursorHighlightedTile = null;
				ShowActorStatPanelForTile(null);
				Singleton<UITextInfoPanel>.Instance?.Hide();
				if (s_CursorHighlightedStar != null)
				{
					s_CursorHighlightedStar.gameObject.SetActive(value: false);
				}
			}
			else if (s_CursorHighlightedTile != clientTile)
			{
				s_CursorHighlightedTile = clientTile;
				ShowTooltipForTile(clientTile);
				ShowActorStatPanelForTile(clientTile);
			}
			return;
		}
		bool active = CanCursorHighlightTile(clientTile);
		if (s_CursorHighlightedStar == null)
		{
			HexSelect_Control component = ObjectPool.Spawn(GlobalSettings.Instance.m_GenericHexStar, clientTile.m_GameObject.transform).GetComponent<HexSelect_Control>();
			component.Type = HexSelect_Control.HexType.Move;
			component.Mode = HexSelect_Control.HexMode.Cursor;
			component.RefreshHexUI();
			component.SetHover(hover: false);
			s_CursorHighlightedStar = component;
		}
		if (s_CursorHighlightedTile != clientTile)
		{
			SetStarPos(s_CursorHighlightedStar.gameObject, clientTile);
			s_CursorHighlightedStar.Type = HexSelect_Control.HexType.Move;
			s_CursorHighlightedStar.Mode = HexSelect_Control.HexMode.Cursor;
			s_CursorHighlightedStar.RefreshHexUI();
			s_CursorHighlightedStar.SetHover(hover: false);
			s_CursorHighlightedTile = clientTile;
			s_CursorHighlightedStar.gameObject.SetActive(active);
			ShowTooltipForTile(clientTile);
			ShowActorStatPanelForTile(clientTile);
		}
		if (ControllerInputPointer.CursorType == ECursorType.Invalid)
		{
			ControllerInputPointer.CursorType = ECursorType.Default;
		}
	}

	public void UpdateTooltipsForCurrentTile()
	{
		CInteractable cInteractable = Interactable();
		TileBehaviour tileBehaviour = null;
		if (cInteractable != null)
		{
			tileBehaviour = cInteractable.GetComponent<TileBehaviour>();
		}
		if (tileBehaviour != null && tileBehaviour.m_ClientTile == null)
		{
			ControllerInputPointer.CursorType = ECursorType.Invalid;
		}
		if (tileBehaviour == null || tileBehaviour.m_ClientTile == null)
		{
			WorldspaceUITools.Instance.DisableAllHoveredOutlines();
			return;
		}
		CClientTile clientTile = tileBehaviour.m_ClientTile;
		ShowTooltipForTile(clientTile);
		ShowActorStatPanelForTile(clientTile);
	}

	public void Hide(object requester)
	{
		_hideCounter.Add(requester);
	}

	public void CancelHide(object requester)
	{
		_hideCounter.Remove(requester);
	}

	private void Show()
	{
		_doShow = true;
	}

	private void Hide()
	{
		_doShow = false;
		HideActorStatPanel();
	}

	private bool CanCursorHighlightTile(CClientTile tile)
	{
		if (s_CurrentlyActiveStars.ContainsKey(tile))
		{
			return false;
		}
		CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y];
		if (cNode == null)
		{
			return false;
		}
		if (!cNode.Walkable)
		{
			return false;
		}
		return true;
	}

	private bool CanCursorHighlightUnrevealedTile(CClientTile tile)
	{
		return ClientScenarioManager.s_ClientScenarioManager.DungeonExitTiles.Contains(tile);
	}

	private void HideActorStatPanel()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor && _shownActorPanel != null)
		{
			Singleton<ActorStatPanel>.Instance.HideForActor(_shownActorPanel);
		}
	}

	private void ShowActorStatPanelForTile(CClientTile tile)
	{
		if (tile == null)
		{
			HideActorStatPanel();
			_shownActorPanel = null;
		}
		else if (_doShow && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex);
			if (_shownActorPanel != null)
			{
				HideActorStatPanel();
			}
			if (cActor != null)
			{
				_shownActorPanel = cActor;
				Singleton<ActorStatPanel>.Instance.Show(_shownActorPanel);
			}
			else
			{
				HideActorStatPanel();
				_shownActorPanel = null;
			}
		}
	}

	private void ShowTooltipForTile(CClientTile tile)
	{
		if (!_doShow)
		{
			return;
		}
		WorldspaceUITools.Instance.DisableAllHoveredOutlines();
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex);
		if (cActor != null)
		{
			List<Outlinable> actorOutlinable = WorldspaceUITools.Instance.GetActorOutlinable(cActor);
			if (actorOutlinable != null && actorOutlinable.Count > 0)
			{
				foreach (Outlinable item in actorOutlinable)
				{
					if (item != null)
					{
						WorldspaceUITools.Instance.EnableHoveredOutline(item);
					}
				}
			}
			else
			{
				Debug.LogError("No outlines found for actor: " + LocalizationManager.GetTranslation(cActor.ActorLocKey()) + Choreographer.GetActorIDForCombatLogIfNeeded(cActor));
			}
		}
		List<PropInfo> list = new List<PropInfo>();
		List<CObjectProp> props = tile.m_Tile.m_Props;
		bool flag = false;
		for (int i = 0; i < props.Count; i++)
		{
			CObjectProp cObjectProp = props[i];
			PropInfo info = new PropInfo();
			if (cObjectProp.CanLootLocKey.IsNOTNullOrEmpty())
			{
				info.LootedBy = string.Format(LocalizationManager.GetTranslation("GUI_PROP_CAN_BE_LOOTED_BY"), LocalizationManager.GetTranslation(cObjectProp.CanLootLocKey));
			}
			PropHealthDetails propHealthDetails = cObjectProp.PropHealthDetails;
			if (propHealthDetails != null && propHealthDetails.CurrentHealth > 0 && cObjectProp.PropActorHasBeenAssigned)
			{
				flag = true;
				break;
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.MoneyToken)
			{
				PropInfo propInfo = list.FirstOrDefault((PropInfo x) => x.ImportType == ScenarioManager.ObjectImportType.MoneyToken || (x.ImportType == ScenarioManager.ObjectImportType.None && x.Gold > 0));
				if (propInfo != null)
				{
					info = propInfo;
				}
				info.Gold += ((ScenarioManager.Scenario.SLTE == null) ? 1 : ScenarioManager.Scenario.SLTE.GoldConversion);
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Chest)
			{
				info.Title = LocalizationManager.GetTranslation("TREASURE_CHEST_TOOLTIP");
				info.ImportType = cObjectProp.ObjectType;
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.GoalChest)
			{
				info.Title = LocalizationManager.GetTranslation("GOAL_CHEST_TOOLTIP");
				info.ImportType = cObjectProp.ObjectType;
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Obstacle)
			{
				info.Title = LocalizationManager.GetTranslation("OBSTACLE_TOOLTIP");
				info.Description = (cObjectProp.OverrideDisallowDestroyAndMove ? LocalizationManager.GetTranslation("IMMOVABLE_OBSTACLE_DESCR_TOOLTIP") : null);
				info.ImportType = cObjectProp.ObjectType;
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Door)
			{
				CObjectDoor cObjectDoor = (CObjectDoor)cObjectProp;
				info.Title = (cObjectDoor.DoorIsOpen ? LocalizationManager.GetTranslation("DOORWAY_TOOLTIP") : (cObjectDoor.DoorIsLocked ? LocalizationManager.GetTranslation("LOCKED_DOOR_TOOLTIP") : LocalizationManager.GetTranslation("CLOSED_DOOR_TOOLTIP")));
				info.ImportType = cObjectProp.ObjectType;
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.PressurePlate)
			{
				info.ImportType = cObjectProp.ObjectType;
				info.Title = LocalizationManager.GetTranslation("PRESSURE_PLATE_TOOLTIP");
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Portal)
			{
				info.ImportType = cObjectProp.ObjectType;
				info.Title = LocalizationManager.GetTranslation("PORTAL_TOOLTIP");
				info.Description = LocalizationManager.GetTranslation("PORTAL_DESCR_TOOLTIP");
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
			{
				info.ImportType = cObjectProp.ObjectType;
				info.Title = LocalizationManager.GetTranslation(cObjectProp.PrefabName + "_TOOLTIP");
				info.Description = LocalizationManager.GetTranslation(cObjectProp.PrefabName + "_DESCR_TOOLTIP");
			}
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Resource)
			{
				CObjectResource cObjectResource = (CObjectResource)cObjectProp;
				info.ImportType = cObjectProp.ObjectType;
				info.Title = LocalizationManager.GetTranslation(cObjectResource.ResourceData.ID);
				info.Description = LocalizationManager.GetTranslation(cObjectResource.ResourceData.ID + "_DESCR_TOOLTIP");
			}
			if (!list.Exists((PropInfo x) => x.ImportType == info.ImportType))
			{
				list.Add(info);
			}
		}
		if (list.Count <= 0 && ClientScenarioManager.s_ClientScenarioManager.DungeonExitTiles.Contains(tile))
		{
			list.Add(new PropInfo
			{
				Title = LocalizationManager.GetTranslation("DUNGEON_EXIT_TOOLTIP"),
				Description = LocalizationManager.GetTranslation("DUNGEON_EXIT_DESCR_TOOLTIP"),
				ImportType = ScenarioManager.ObjectImportType.Door
			});
		}
		List<CObjective> list2 = ScenarioManager.CurrentScenarioState.AllObjectives.Where((CObjective x) => x.CustomTileHoverLocKey.IsNOTNullOrEmpty() && (x.ObjectiveType == EObjectiveType.ActorReachPosition || x.ObjectiveType == EObjectiveType.ActorsEscaped || x.ObjectiveType == EObjectiveType.AnyActorReachPosition)).ToList();
		if (list2.Count > 0)
		{
			foreach (CObjective item2 in list2)
			{
				if (!item2.CustomTileHoverLocKey.IsNullOrEmpty())
				{
					List<TileIndex> list3 = null;
					if (item2 is CObjective_ActorReachPosition cObjective_ActorReachPosition)
					{
						list3 = cObjective_ActorReachPosition.ActorTargetPositions;
					}
					if (item2 is CObjective_ActorsEscaped cObjective_ActorsEscaped)
					{
						list3 = cObjective_ActorsEscaped.EscapePositions;
					}
					if (item2 is CObjective_AnyActorReachPosition cObjective_AnyActorReachPosition)
					{
						list3 = cObjective_AnyActorReachPosition.ActorTargetPositions;
					}
					if (list3?.FirstOrDefault((TileIndex x) => x.X == tile.m_Tile.m_ArrayIndex.X && x.Y == tile.m_Tile.m_ArrayIndex.Y) != null)
					{
						list.Add(new PropInfo
						{
							Title = LocalizationManager.GetTranslation("DUNGEON_EXIT_TOOLTIP"),
							Description = LocalizationManager.GetTranslation(item2.CustomTileHoverLocKey),
							ImportType = ScenarioManager.ObjectImportType.Door
						});
					}
				}
			}
		}
		if (list.Count <= 0)
		{
			CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y];
			if (cNode != null && cNode.Blocked)
			{
				List<CTile> allAdjacentTiles = ScenarioManager.GetAllAdjacentTiles(tile.m_Tile);
				for (int num = 0; num < allAdjacentTiles.Count; num++)
				{
					if (allAdjacentTiles[num].m_Props.Count > 0 && allAdjacentTiles[num].FindProp(ScenarioManager.ObjectImportType.Obstacle) != null)
					{
						list.Add(new PropInfo
						{
							Title = LocalizationManager.GetTranslation("OBSTACLE_TOOLTIP"),
							ImportType = ScenarioManager.ObjectImportType.Obstacle
						});
					}
				}
			}
			foreach (CSpawner spawner in tile.m_Tile.m_Spawners)
			{
				if (!spawner.SpawnerData.SpawnerHoverNameLoc.IsNOTNullOrEmpty())
				{
					continue;
				}
				string text = spawner.SpawnerData.SpawnerHoverNameLoc;
				string text2 = string.Empty;
				if (text.Contains('|'))
				{
					string[] array = text.Split('|');
					if (array.Length == 2)
					{
						text = array[0];
						text2 = array[1];
					}
				}
				PropInfo propInfo2 = new PropInfo();
				if (text.IsNOTNullOrEmpty())
				{
					propInfo2.Title = LocalizationManager.GetTranslation(text);
				}
				if (text2.IsNOTNullOrEmpty())
				{
					propInfo2.Description = LocalizationManager.GetTranslation(text2);
				}
				if (spawner.Prop != null)
				{
					propInfo2.ImportType = spawner.Prop.ObjectType;
				}
				else
				{
					propInfo2.ImportType = ScenarioManager.ObjectImportType.Spawner;
				}
				list.Add(propInfo2);
			}
		}
		SaveData instance = SaveData.Instance;
		if ((object)instance != null && instance.Global?.GameMode == EGameMode.LevelEditor)
		{
			return;
		}
		Singleton<UITextInfoPanel>.Instance?.Hide();
		Singleton<UIPropInfoPanel>.Instance?.Hide(UIPropInfoPanel.EPropType.QuestItem);
		if (flag)
		{
			return;
		}
		foreach (CObjectProp item3 in props)
		{
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(item3);
			if (!(propObject != null))
			{
				continue;
			}
			List<Outlinable> list4 = propObject.GetComponentsInChildren<Outlinable>().ToList();
			if (list4 == null)
			{
				continue;
			}
			foreach (Outlinable item4 in list4)
			{
				WorldspaceUITools.Instance.EnableHoveredOutline(item4);
			}
		}
		if (list.Count > 0)
		{
			if (list.Any((PropInfo x) => x.ImportType == ScenarioManager.ObjectImportType.CarryableQuestItem))
			{
				var (title, description) = list.First((PropInfo x) => x.ImportType == ScenarioManager.ObjectImportType.CarryableQuestItem).Get();
				Singleton<UIPropInfoPanel>.Instance?.ShowQuestItem(title, description);
			}
			else
			{
				Debug.Log("Show text info panel\n" + string.Join(',', list.Select((PropInfo x) => x.Title)));
				Singleton<UITextInfoPanel>.Instance?.Show(list.Select((PropInfo x) => x.Get()).ToHashSet().ToArray());
			}
		}
		else
		{
			Singleton<UITextInfoPanel>.Instance?.TryReset();
		}
	}

	public void ActorIsSelectingDoor(bool active)
	{
		m_SelectingADoor = active;
	}

	public void ActorIsOpeningDoor(bool active)
	{
		m_OpeningDoor = active;
	}

	public void ClearNonTargetHexHighlights(List<CActor> actors = null, List<CTile> tiles = null, List<CClientTile> clientTiles = null, bool removeHover = true)
	{
		List<CClientTile> list = null;
		if (list == null && actors != null)
		{
			list = actors.Select((CActor a) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[a.ArrayIndex.X, a.ArrayIndex.Y]).ToList();
		}
		if (list == null && tiles != null)
		{
			list = tiles.Select((CTile t) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[t.m_ArrayIndex.X, t.m_ArrayIndex.Y]).ToList();
		}
		list = list ?? clientTiles;
		if (list == null)
		{
			return;
		}
		LockView = true;
		list.AddRange(s_DungeonExitStars.Keys);
		ClearUnusedStars(list);
		foreach (KeyValuePair<CClientTile, Dictionary<CClientTile, HexSelect_Control>> s_CurrentlyActiveStar in s_CurrentlyActiveStars)
		{
			HexSelect_Control hexSelect_Control = s_CurrentlyActiveStar.Value[s_CurrentlyActiveStar.Key];
			hexSelect_Control.Hover = false;
			hexSelect_Control.HexUpdate();
		}
	}

	public void ResetPlayerAttackType()
	{
		if (Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.InvalidAttack && Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.ActorSelectedAttackFocus && Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.PlayerSelectedTile && CurrentAbilityDisplayType != EAbilityDisplayType.None)
		{
			CurrentAbilityDisplayType = EAbilityDisplayType.None;
			LockView = false;
			m_SavedAbility = null;
			s_CachedAbilityRangeTiles.Clear();
			m_SavedRedistributeDamageUIMessage = null;
			m_SavedDistributeDamageService = null;
		}
	}

	private void ControlPlayerEndMovement()
	{
		if (Choreographer.s_Choreographer.m_CurrentActor != null && Choreographer.s_Choreographer.m_CurrentActor.Type == CActor.EType.Player)
		{
			if (Choreographer.s_Choreographer.LastMessage.m_Type == CMessageData.MessageType.ActorHasMoved)
			{
				m_EndingMovement = true;
				m_SelectingADoor = false;
			}
			if (Choreographer.s_Choreographer.LastMessage.m_Type == CMessageData.MessageType.ActionSelection)
			{
				m_EndingMovement = false;
				m_OpeningDoor = false;
				m_SelectingADoor = false;
			}
			if (m_EndingMovement && !m_OpeningDoor && Choreographer.s_Choreographer.LastMessage.m_Type == CMessageData.MessageType.ActorIsSelectingMoveTile)
			{
				m_EndingMovement = false;
				m_OpeningDoor = false;
			}
		}
	}

	private List<CClientTile> CurrentAbilityRangeTiles(CAbility ability, int range = 1, bool includeCasterTile = false, bool rangedAttack = false, List<CTile> selectedTiles = null)
	{
		try
		{
			if (s_CachedAbilityRangeTiles.Count > 0)
			{
				return s_CachedAbilityRangeTiles;
			}
			s_CachedAbilityRangeTiles.Clear();
			if (ability.IsCurrentlyTargetingActors())
			{
				if (ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
				{
					s_CachedAbilityRangeTiles.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.X, Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.Y]);
					return s_CachedAbilityRangeTiles;
				}
				if (ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion, exclusive: true) && Choreographer.s_Choreographer.m_CurrentActor is CPlayerActor { CompanionSummon: not null } cPlayerActor)
				{
					s_CachedAbilityRangeTiles.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cPlayerActor.CompanionSummon.ArrayIndex.X, cPlayerActor.CompanionSummon.ArrayIndex.Y]);
					return s_CachedAbilityRangeTiles;
				}
			}
			CTile cTile = ScenarioManager.Tiles[Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.X, Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.Y];
			List<CTile> list = new List<CTile>();
			if (ability.AllTargetsOnMovePath)
			{
				list.AddRange(ability.TilesSelected);
			}
			else if (!(ability is CAbilityTargeting { OneTargetAtATime: not false, AreaEffectSelected: not false }))
			{
				if (ability.TilesInRange != null)
				{
					list.AddRange(ability.TilesInRange);
				}
				else
				{
					Debug.LogError("Unable to find CurrentAbilityRangeTiles to display");
				}
			}
			if (ability.ValidActorsInRange != null && ability.ValidActorsInRange.Count > 0)
			{
				foreach (CObjectActor objectActor in ability.ValidActorsInRange.Where((CActor x) => x is CObjectActor))
				{
					if (!list.Any((CTile x) => x.m_ArrayIndex == objectActor.ArrayIndex) && objectActor.IsAttachedToProp && objectActor.AttachedProp.ObjectType == ScenarioManager.ObjectImportType.Door && objectActor.AttachedProp.PropHealthDetails != null && objectActor.AttachedProp.PropHealthDetails.HasHealth && objectActor.AttachedProp.PropHealthDetails.CurrentHealth > 0)
					{
						CTile adjacentTile = ScenarioManager.GetAdjacentTile(objectActor.ArrayIndex.X, objectActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
						if (adjacentTile != null)
						{
							list.Add(adjacentTile);
						}
					}
				}
			}
			if (ability is CAbilityDestroyObstacle && ability.AllTargets)
			{
				foreach (CTile item2 in ability.TilesSelected)
				{
					if (!list.Contains(item2))
					{
						list.Add(item2);
					}
				}
			}
			foreach (CTile item3 in list)
			{
				if (includeCasterTile || item3 != cTile)
				{
					bool flag = true;
					CActor cActor = ScenarioManager.Scenario.FindActorAt(item3.m_ArrayIndex);
					if (cActor != null)
					{
						flag = ability.AbilityFilter.IsValidTarget(cActor, ability.FilterActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible);
					}
					if (flag)
					{
						CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item3.m_ArrayIndex.X, item3.m_ArrayIndex.Y];
						s_CachedAbilityRangeTiles.Add(item);
					}
				}
			}
			return s_CachedAbilityRangeTiles;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.CurrentAbilityRangeTiles().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00022", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return null;
	}

	private int DistanceBetweenPoints(Point pointA, Point pointB, bool jump)
	{
		bool foundPath;
		return ScenarioManager.PathFinder.FindPath(pointA, pointB, jump, ignoreMoveCost: true, out foundPath).Count;
	}

	private bool PointingAtANewTile()
	{
		CInteractable cInteractable = Interactable();
		if (cInteractable != m_LastPointedAtInteractable)
		{
			m_LastPointedAtInteractable = cInteractable;
			return true;
		}
		return false;
	}

	private void UpdateRetaliateDamage(int retaliateDamage, bool forceUpdate = false)
	{
		if (Choreographer.s_Choreographer.m_CurrentActor != null)
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(Choreographer.s_Choreographer.m_CurrentActor);
			if (gameObject != null)
			{
				ActorBehaviour.GetActorBehaviour(gameObject).UpdateRetaliateDamage(retaliateDamage, forceUpdate);
			}
		}
	}

	private CInteractable InteractableNearestToPosition(Vector3 position)
	{
		return MF.FindNearestInteractableToPosition(ignoreinteractableviaguiflag: false, Controller.Instance.m_HexSelectionRaycastLayer, position);
	}

	private CInteractable Interactable()
	{
		if (AutoTestController.s_AutoLogPlaybackInProgress || UIManager.IsPointerOverUI)
		{
			return null;
		}
		return InteractableUnderMouse();
	}

	private CInteractable InteractableUnderMouse()
	{
		if (!(Controller.Instance != null))
		{
			return null;
		}
		return MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, Controller.Instance.m_HexSelectionRaycastLayer);
	}

	private Vector3 GetCentralPoint(List<CClientTile> validAOETiles)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (CClientTile validAOETile in validAOETiles)
		{
			foreach (CClientTile validAOETile2 in validAOETiles)
			{
				if (validAOETile != validAOETile2)
				{
					float x = (validAOETile.m_GameObject.transform.position.x + validAOETile2.m_GameObject.transform.position.x) / 2f;
					float y = (validAOETile.m_GameObject.transform.position.y + validAOETile2.m_GameObject.transform.position.y) / 2f;
					float z = (validAOETile.m_GameObject.transform.position.z + validAOETile2.m_GameObject.transform.position.z) / 2f;
					list.Add(new Vector3(x, y, z));
				}
			}
		}
		return GetMeanVector(list);
	}

	private Vector3 GetMeanVector(List<Vector3> positions)
	{
		if (positions.Count == 0)
		{
			return Vector3.zero;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		foreach (Vector3 position in positions)
		{
			num += position.x;
			num2 += position.y;
			num3 += position.z;
		}
		return new Vector3(num / (float)positions.Count, num2 / (float)positions.Count, num3 / (float)positions.Count);
	}

	public void DisplayLevelEditorObstaclePlacementStars(CClientTile originTile = null)
	{
		try
		{
			s_TilesExpectedToDisplay.Clear();
			CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			if (cClientTile == null)
			{
				return;
			}
			List<CTile> list = new List<CTile>();
			int length = ScenarioManager.Tiles.GetLength(1);
			int length2 = ScenarioManager.Tiles.GetLength(0);
			for (int i = 0; i < length2; i++)
			{
				for (int j = 0; j < length; j++)
				{
					list.Add(ScenarioManager.Tiles[i, j]);
				}
			}
			List<CTile> obj = new List<CTile> { cClientTile.m_Tile };
			List<CTile> validTilesIncludingBlockedOut = null;
			obj.AddRange(CAreaEffect.GetValidTiles(cClientTile.m_Tile, cClientTile.m_Tile, LevelEditorController.s_Instance.CurrentAreaEffectSelection, m_AreaEffectAngle, list, getBlocked: false, ref validTilesIncludingBlockedOut));
			foreach (CTile item in obj)
			{
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				s_TilesExpectedToDisplay.Add(cClientTile2);
				HexSelect_Control.HexMode mode = HexSelect_Control.HexMode.PossibleTarget;
				HexSelect_Control.HexType type = HexSelect_Control.HexType.PositiveEffect;
				ModifyStar(cClientTile2, type, mode, hover: true, s_AttackStars);
			}
			ClearUnusedStars(s_TilesExpectedToDisplay);
			List<CClientTile> hoveredTiles = (from s in s_AttackStars
				where s.Value.Hover
				select s.Key).ToList();
			SetHexBorders(s_AttackStars, hoveredTiles);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.DisplayAOEStars().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ShowLevelEditorStartingTiles()
	{
		s_TilesExpectedToDisplay.Clear();
		if (SaveData.Instance.Global.CurrentEditorLevelData != null && SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes.Count > 0)
		{
			foreach (TileIndex startingTileIndex in SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes)
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[startingTileIndex.X, startingTileIndex.Y];
				if (cClientTile != null && !s_PlacementStars.ContainsKey(cClientTile) && cClientTile.m_Tile.m_HexMap != null && cClientTile.m_Tile.m_HexMap.Revealed)
				{
					ModifyStar(cClientTile, HexSelect_Control.HexType.Move, HexSelect_Control.HexMode.Reach, hover: false, s_PlacementStars);
				}
			}
		}
		SetHexBorders(s_PlacementStars);
	}

	private void RefreshHighlightAllHexes()
	{
		if (!m_AllHexesHighlighted && InputManager.GetIsPressed(KeyAction.HIGHLIGHT))
		{
			m_AllHexesHighlighted = true;
			ClearStars();
			List<CClientTile> list = new List<CClientTile>();
			for (int i = 0; i < ScenarioManager.Height; i++)
			{
				for (int j = 0; j < ScenarioManager.Width; j++)
				{
					CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[j, i];
					if (cClientTile != null)
					{
						list.Add(cClientTile);
					}
				}
			}
			IEnumerable<CObjectTrap> source = from w in ScenarioManager.CurrentScenarioState.Maps.SelectMany((CMap sm) => sm.Props).OfType<CObjectTrap>()
				where !w.Activated
				select w;
			foreach (CClientTile item in list)
			{
				if (!IsHighlightTileValid(item))
				{
					continue;
				}
				TileTemplate tileTemplate = s_MovePossiblityTemplate;
				CObjectProp cObjectProp = item.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap);
				if (cObjectProp != null)
				{
					if (source.Contains(cObjectProp))
					{
						tileTemplate = s_MoveHazardPossibilityTemplate;
					}
				}
				else if (item.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null || item.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null)
				{
					tileTemplate = s_MoveHazardPossibilityTemplate;
				}
				else if (item.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainRubble) != null || item.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainWater) != null)
				{
					tileTemplate = s_DifficultTerrainPossibilityTemplate;
				}
				else
				{
					CObjectProp cObjectProp2 = item.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest);
					CObjectProp cObjectProp3 = item.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
					if (cObjectProp2 != null || (cObjectProp3 != null && !(cObjectProp3 as CObjectDoor).IsDungeonEntrance))
					{
						tileTemplate = s_MoveChestDoorPossibilityTemplate;
					}
				}
				ModifyStar(item, tileTemplate.m_Type, tileTemplate.m_Mode, tileTemplate.m_Hover, s_HighlightAllStars);
			}
			RefreshHexesWithAllBorders(s_HighlightAllStars);
		}
		if (m_AllHexesHighlighted && (!InputManager.GetIsPressed(KeyAction.HIGHLIGHT) || !Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(KeyAction.HIGHLIGHT).Enabled))
		{
			m_AllHexesHighlighted = false;
			ClearStars();
			RefreshCurrentState();
		}
	}

	private bool IsHighlightTileValid(CClientTile tile)
	{
		try
		{
			if (tile == null)
			{
				return false;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y];
			if (cNode == null)
			{
				return false;
			}
			if (tile.m_Tile.m_HexMap != null && tile.m_Tile.m_Hex2Map != null)
			{
				if (!tile.m_Tile.m_HexMap.Revealed && !tile.m_Tile.m_Hex2Map.Revealed)
				{
					return false;
				}
			}
			else
			{
				if (tile.m_Tile.m_HexMap != null && !tile.m_Tile.m_HexMap.Revealed)
				{
					return false;
				}
				if (tile.m_Tile.m_Hex2Map != null && !tile.m_Tile.m_Hex2Map.Revealed)
				{
					return false;
				}
			}
			if (!cNode.Walkable)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the WorldspaceStarHexDisplay.IsHighlightTileValid().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00023", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return false;
		}
	}

	private void FindAllChestAndDoorTiles()
	{
		s_ChestTiles.Clear();
		s_DoorTiles.Clear();
		new List<CClientTile>();
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int j = 0; j < ScenarioManager.Width; j++)
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[j, i];
				if (cClientTile == null)
				{
					continue;
				}
				if (cClientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) != null)
				{
					s_ChestTiles.Add(cClientTile);
					continue;
				}
				CObjectDoor cObjectDoor = (CObjectDoor)cClientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
				if (cObjectDoor != null && cClientTile.m_Tile.m_HexMap != null && cClientTile.m_Tile.m_Hex2Map != null && !cObjectDoor.DoorIsOpen)
				{
					s_DoorTiles.Add(cClientTile);
				}
			}
		}
		m_FoundAllChestAndDoorTiles = true;
	}

	public void ClearCachedAbilityTiles()
	{
		s_CachedAbilityRangeTiles.Clear();
	}

	private List<CClientTile> FindStraightLineDirection(CActor movingActor, int remainingMoves)
	{
		List<CClientTile> result = new List<CClientTile>();
		if (Waypoint.s_Waypoints.Count > 0)
		{
			Waypoint waypointComponent = Waypoint.GetWaypointComponent(Waypoint.s_Waypoints.Last());
			if (waypointComponent != null && waypointComponent.ClientTile.m_Tile != null)
			{
				if (s_LeftLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.ELeft);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.ELeft;
				}
				else if (s_RightLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.ERight);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.ERight;
				}
				else if (s_TopLeftLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.ETopLeft);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.ETopLeft;
				}
				else if (s_TopRightLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.ETopRight);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.ETopRight;
				}
				else if (s_BottomLeftLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.EBottomLeft);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.EBottomLeft;
				}
				else if (s_BottomRightLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.EBottomRight);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.EBottomRight;
				}
			}
		}
		return result;
	}

	private List<CClientTile> GetRemainingTilesInDirection(CActor movingActor, int remainingMoves, ScenarioManager.EAdjacentPosition position)
	{
		List<CClientTile> list = new List<CClientTile>();
		List<CTile> list2 = null;
		switch (position)
		{
		case ScenarioManager.EAdjacentPosition.ELeft:
			list2 = s_LeftLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.ERight:
			list2 = s_RightLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.ETopLeft:
			list2 = s_TopLeftLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.ETopRight:
			list2 = s_TopRightLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.EBottomLeft:
			list2 = s_BottomLeftLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.EBottomRight:
			list2 = s_BottomRightLineTiles;
			break;
		}
		if (Waypoint.s_Waypoints.Count > 0)
		{
			Waypoint waypointComponent = Waypoint.GetWaypointComponent(Waypoint.s_Waypoints.Last());
			if (list2.Contains(waypointComponent.ClientTile.m_Tile))
			{
				for (int i = list2.IndexOf(waypointComponent.ClientTile.m_Tile) + 1; i < list2.Count; i++)
				{
					CTile cTile = list2[i];
					CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y];
					list.Add(item);
				}
			}
		}
		else
		{
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y];
			if (cClientTile != null && list2.Contains(cClientTile.m_Tile))
			{
				for (int j = list2.IndexOf(cClientTile.m_Tile) + 1; j < list2.Count; j++)
				{
					CTile cTile2 = list2[j];
					CClientTile item2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y];
					list.Add(item2);
				}
			}
		}
		return list;
	}

	private List<CClientTile> FindAllStraightLineTiles(CActor movingActor, int remainingMoves)
	{
		List<CClientTile> list = new List<CClientTile>();
		s_LeftLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.ELeft);
		s_RightLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.ERight);
		s_TopLeftLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.ETopLeft);
		s_TopRightLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.ETopRight);
		s_BottomLeftLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.EBottomLeft);
		s_BottomRightLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.EBottomRight);
		s_AllLineTiles.Clear();
		s_AllLineTiles.AddRange(s_LeftLineTiles);
		s_AllLineTiles.AddRange(s_RightLineTiles);
		s_AllLineTiles.AddRange(s_TopLeftLineTiles);
		s_AllLineTiles.AddRange(s_TopRightLineTiles);
		s_AllLineTiles.AddRange(s_BottomLeftLineTiles);
		s_AllLineTiles.AddRange(s_BottomRightLineTiles);
		foreach (CTile s_LeftLineTile in s_LeftLineTiles)
		{
			CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s_LeftLineTile.m_ArrayIndex.X, s_LeftLineTile.m_ArrayIndex.Y];
			list.Add(item);
		}
		foreach (CTile s_RightLineTile in s_RightLineTiles)
		{
			CClientTile item2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s_RightLineTile.m_ArrayIndex.X, s_RightLineTile.m_ArrayIndex.Y];
			list.Add(item2);
		}
		foreach (CTile s_TopLeftLineTile in s_TopLeftLineTiles)
		{
			CClientTile item3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s_TopLeftLineTile.m_ArrayIndex.X, s_TopLeftLineTile.m_ArrayIndex.Y];
			list.Add(item3);
		}
		foreach (CTile s_TopRightLineTile in s_TopRightLineTiles)
		{
			CClientTile item4 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s_TopRightLineTile.m_ArrayIndex.X, s_TopRightLineTile.m_ArrayIndex.Y];
			list.Add(item4);
		}
		foreach (CTile s_BottomLeftLineTile in s_BottomLeftLineTiles)
		{
			CClientTile item5 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s_BottomLeftLineTile.m_ArrayIndex.X, s_BottomLeftLineTile.m_ArrayIndex.Y];
			list.Add(item5);
		}
		foreach (CTile s_BottomRightLineTile in s_BottomRightLineTiles)
		{
			CClientTile item6 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s_BottomRightLineTile.m_ArrayIndex.X, s_BottomRightLineTile.m_ArrayIndex.Y];
			list.Add(item6);
		}
		return list;
	}

	public void ResetLineDirection()
	{
		CurrentLineDirection = ScenarioManager.EAdjacentPosition.ECenter;
	}

	public void ProxyUpdateSelectionHexes(CClientTile originTile, int targetingAngle, bool aoeLocked)
	{
		FFSNet.Console.LogInfo("PROXY: Updating ability selection hexes. Targeting angle: " + targetingAngle);
		m_AreaEffectAngle = targetingAngle;
		SetAOELocked(aoeLocked);
		ShowPossibleTargetSelectionHexes(originTile);
	}
}
