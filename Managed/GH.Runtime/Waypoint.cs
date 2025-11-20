using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FFSNet;
using GLOOM;
using Photon.Bolt;
using ScenarioRuleLibrary;
using SharedLibrary.SimpleLog;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public enum EWaypointType
	{
		None,
		Move,
		Push,
		Pull,
		Attack
	}

	public static CActor s_MovingActor;

	public bool IsTheLastWaypoint;

	public bool CanEndMovementHere = true;

	public bool WaypointIsADoor;

	[HideInInspector]
	public bool DoorIsOpen;

	public GameObject ParentWaypointObject;

	public GameObject EndMovementButton;

	public GameObject DoorYesButton;

	public GameObject DeleteButton;

	public static List<GameObject> s_Waypoints = new List<GameObject>();

	public static List<GameObject> s_CachedWaypoints = new List<GameObject>();

	private static List<Point> s_ClearValidSelectionTiles = new List<Point>();

	public static CClientTile s_PlacementTile;

	public static bool s_LockWaypoints = false;

	public static bool s_CachedLockWaypoints = false;

	private CClientTile m_ClientTile;

	private int m_Order;

	private int m_MovesRemaining;

	private int m_MoveCost;

	private int m_MovesAvailable;

	private EWaypointType m_WaypointType;

	private bool m_TileIsBlocked;

	private bool m_ActorFlying;

	public static bool IsLastWaypointADoor
	{
		get
		{
			if (s_Waypoints.Count == 0)
			{
				return false;
			}
			return s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>().WaypointIsADoor;
		}
	}

	public static Waypoint GetLastWaypoint
	{
		get
		{
			if (s_Waypoints.Count == 0)
			{
				return null;
			}
			return s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>();
		}
	}

	public static bool SingleAction
	{
		get
		{
			if (s_Waypoints.Count == 0)
			{
				return false;
			}
			Waypoint getLastWaypoint = GetLastWaypoint;
			if (getLastWaypoint.m_WaypointType != EWaypointType.Push && getLastWaypoint.m_WaypointType != EWaypointType.Pull)
			{
				return getLastWaypoint.m_WaypointType == EWaypointType.Attack;
			}
			return true;
		}
	}

	public CClientTile ClientTile => m_ClientTile;

	public int MovesRemaining => m_MovesRemaining;

	public int MoveCost => m_MoveCost;

	public int Order => m_Order;

	public static void RefreshWaypointMovesRemaining(CAbilityMove moveAbility)
	{
		int num = moveAbility.MoveCount;
		foreach (GameObject s_Waypoint in s_Waypoints)
		{
			Waypoint component = s_Waypoint.GetComponent<Waypoint>();
			component.m_MovesRemaining = num - component.MoveCost;
			num -= component.MoveCost;
		}
	}

	private void OnEnable()
	{
		EndMovementButton.SetActive(value: false);
		DoorYesButton.SetActive(value: false);
		DeleteButton.SetActive(value: false);
	}

	private void OnDestroy()
	{
		s_Waypoints.Remove(base.gameObject);
	}

	public void SetOrder(int order, List<Point> path = null)
	{
		try
		{
			m_Order = order;
			CheckIfPlayerCanEndMovementHere(path);
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
			CheckForDoors();
			SetupWaypoint();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.SetOrder().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void CheckIfPlayerCanEndMovementHere(List<Point> path = null)
	{
		try
		{
			CanEndMovementHere = true;
			if (m_WaypointType == EWaypointType.Attack)
			{
				return;
			}
			if (m_WaypointType != EWaypointType.Move && m_MovesAvailable > 0 && !m_TileIsBlocked)
			{
				CanEndMovementHere = false;
				return;
			}
			bool flag = false;
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { CarryOtherActorsOnHex: not false } cAbilityMove)
			{
				if (cAbilityMove.ActorsToCarry.Count > 0)
				{
					flag = true;
				}
				else if (path != null && path.Count > 1)
				{
					for (int i = 0; i < path.Count - 1; i++)
					{
						List<CActor> actorsOnTile = GameState.GetActorsOnTile(ScenarioManager.Tiles[path[i].X, path[i].Y], Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor, CAbilityFilterContainer.CreateDefaultFilter(), null, isTargetedAbility: false, false);
						if (actorsOnTile != null && actorsOnTile.Count > 0)
						{
							flag = true;
						}
					}
				}
			}
			if (!Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor.IgnoreActorCollision || flag)
			{
				foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
				{
					if (!allAliveMonster.PhasedOut && !allAliveMonster.IgnoreActorCollision)
					{
						Point arrayIndex = allAliveMonster.ArrayIndex;
						if ((s_MovingActor != allAliveMonster || s_Waypoints.Count <= 0) && arrayIndex == ClientTile.m_Tile.m_ArrayIndex)
						{
							CanEndMovementHere = false;
							return;
						}
					}
				}
				foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
				{
					if (!heroSummon.PhasedOut && !heroSummon.IgnoreActorCollision)
					{
						Point arrayIndex2 = heroSummon.ArrayIndex;
						if ((s_MovingActor != heroSummon || s_Waypoints.Count <= 0) && arrayIndex2 == ClientTile.m_Tile.m_ArrayIndex)
						{
							CanEndMovementHere = false;
							return;
						}
					}
				}
				foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
				{
					if (!@object.PhasedOut)
					{
						Point arrayIndex3 = @object.ArrayIndex;
						if ((s_MovingActor != @object || s_Waypoints.Count <= 0) && arrayIndex3 == ClientTile.m_Tile.m_ArrayIndex)
						{
							CanEndMovementHere = false;
							return;
						}
					}
				}
				foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
				{
					if (!playerActor.PhasedOut && !playerActor.IgnoreActorCollision)
					{
						Point arrayIndex4 = playerActor.ArrayIndex;
						if ((s_MovingActor != playerActor || s_Waypoints.Count <= 0) && arrayIndex4 == ClientTile.m_Tile.m_ArrayIndex)
						{
							CanEndMovementHere = false;
							return;
						}
					}
				}
			}
			if (!m_ActorFlying || (m_ActorFlying && Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor.IgnoreActorCollision))
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[ClientTile.m_Tile.m_ArrayIndex.X, ClientTile.m_Tile.m_ArrayIndex.Y];
				CanEndMovementHere = cNode != null && cNode.Walkable && !cNode.Blocked && CanEndMovementHere;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.CheckIfPlayerCanEndMovementHere().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void CheckForDoors()
	{
		try
		{
			WaypointIsADoor = Choreographer.s_Choreographer.CheckForDoors(ClientTile);
			if (m_WaypointType == EWaypointType.Move)
			{
				if (WaypointIsADoor && !DoorIsOpen)
				{
					SetupWaypointForDoor(active: true);
				}
				else
				{
					SetupWaypointForDoor(active: false);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.CheckForDoors().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void EndMovement()
	{
		try
		{
			if (CanEndMovementHere)
			{
				EndMovementButton.SetActive(value: false);
				DeleteButton.SetActive(value: false);
				WorldspaceStarHexDisplay.Instance.ActorIsOpeningDoor(active: false);
				OnPress(networkActionIfOnline: true);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.EndMovement().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void OpenDoor()
	{
		try
		{
			DoorYesButton.SetActive(value: false);
			Choreographer.s_Choreographer.DisableTileSelection(active: false);
			DoorIsOpen = true;
			WorldspaceStarHexDisplay.Instance.ActorIsOpeningDoor(active: true);
			OnPress();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.OpenDoor().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void LateUpdate()
	{
		if (m_ClientTile == null || !(m_ClientTile.m_GameObject != null) || !(WorldspaceUITools.Instance != null) || !(WorldspaceUITools.Instance.WorldspaceCanvas != null) || !(WorldspaceUITools.Instance.WorldspaceCamera != null))
		{
			return;
		}
		RectTransform component = WorldspaceUITools.Instance.WorldspaceCanvas.GetComponent<RectTransform>();
		if (component != null)
		{
			Vector2 b = new Vector2(component.rect.width / (float)Screen.width, component.rect.height / (float)Screen.height);
			RectTransform component2 = GetComponent<RectTransform>();
			if (component2 != null)
			{
				component2.anchoredPosition = Vector2.Scale(WorldspaceUITools.Instance.WorldspaceCamera.WorldToScreenPoint(m_ClientTile.m_GameObject.transform.position), b);
			}
		}
	}

	public void OnPress(bool networkActionIfOnline = false)
	{
		try
		{
			GameObject obj = (SingleAction ? s_Waypoints[s_Waypoints.Count - 1] : s_Waypoints[0]);
			CClientTile clientTile = GetWaypointComponent(obj).ClientTile;
			if (FFSNetwork.IsOnline && Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl && networkActionIfOnline)
			{
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new TileToken(clientTile.m_Tile.m_ArrayIndex);
				Synchronizer.SendGameAction(GameActionType.PressWaypoint, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			if (SingleAction)
			{
				Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
			}
			Choreographer.s_Choreographer.AutoTestIgnoreTileClick = true;
			Choreographer.s_Choreographer.TileHandler(clientTile, GetCTileList());
			Choreographer.s_Choreographer.AutoTestIgnoreTileClick = false;
			if (WaypointIsADoor)
			{
				Choreographer.s_Choreographer.ContinueTileSelection();
			}
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
			Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
			Choreographer.s_Choreographer.SetActiveSelectButton(activate: false);
			DestroyWaypoint(obj);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.OnPress().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void OnDelete()
	{
		try
		{
			WorldspaceStarHexDisplay.Instance.ActorIsSelectingDoor(active: false);
			s_Waypoints.Remove(ParentWaypointObject);
			Choreographer.s_Choreographer.DisableTileSelection(active: false);
			if (s_Waypoints.Count == 0)
			{
				switch (m_WaypointType)
				{
				case EWaypointType.Move:
				{
					bool active = !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove cAbilityMove) || cAbilityMove.CanSkip;
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active, LocalizationManager.GetTranslation("GUI_SKIP_MOVEMENT"));
					Choreographer.s_Choreographer.readyButton.ResetAlternativeAction(LocalizationManager.GetTranslation("GUI_CONFIRM_MOVEMENT"), ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED);
					Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy);
					break;
				}
				case EWaypointType.Push:
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: true);
					Choreographer.s_Choreographer.readyButton.ResetAlternativeAction(LocalizationManager.GetTranslation("GUI_END_PUSH"), ReadyButton.EButtonState.EREADYBUTTONCONTINUE);
					if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush { AdditionalPushEffect: var additionalPushEffect } cAbilityPush && additionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked))
					{
						cAbilityPush.UpdateAdditionalPushDamageSummary(adjacentToBlockedTile: false, null);
					}
					Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy);
					break;
				case EWaypointType.Pull:
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: true);
					Choreographer.s_Choreographer.readyButton.ResetAlternativeAction(LocalizationManager.GetTranslation("GUI_END_PULL"), ReadyButton.EButtonState.EREADYBUTTONCONTINUE);
					Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy);
					break;
				case EWaypointType.Attack:
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: true);
					Choreographer.s_Choreographer.readyButton.ResetAlternativeAction(LocalizationManager.GetTranslation("GUI_CONFIRM_ATTACK"), ReadyButton.EButtonState.EREADYBUTTONCONTINUE);
					if (PhaseManager.Phase is CPhaseAction)
					{
						ScenarioRuleClient.ClearTargets();
					}
					Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy);
					break;
				}
				if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { CarryOtherActorsOnHex: not false })
				{
					ScenarioRuleClient.UpdateMoveCarry(GetCTileList(), removeWaypoint: true);
				}
			}
			else
			{
				if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush { AdditionalPushEffect: var additionalPushEffect2 } cAbilityPush2 && additionalPushEffect2.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked))
				{
					bool adjacentToBlockedTile = false;
					if (CAbilityPush.IsPushTileAdjacentToBlockedPushTile(GetLastWaypoint.ClientTile.m_Tile, cAbilityPush2.CurrentTarget, cAbilityPush2.CurrentTarget.Type, GetLastWaypoint.ClientTile.m_Tile.m_ArrayIndex, cAbilityPush2.PushFromPoint, cAbilityPush2.Strength, intoBlocked: true))
					{
						adjacentToBlockedTile = true;
					}
					cAbilityPush2.UpdateAdditionalPushDamageSummary(adjacentToBlockedTile, GetLastWaypoint.ClientTile.m_Tile);
				}
				if (m_WaypointType == EWaypointType.Attack)
				{
					GetLastWaypoint.SetupWaypoint();
				}
				else
				{
					Choreographer.s_Choreographer.readyButton.SetInteractable(GetLastWaypoint.CanEndMovementHere);
					Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy);
				}
			}
			UnityEngine.Object.Destroy(ParentWaypointObject);
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { MoveRestrictionType: var moveRestrictionType } cAbilityMove3 && moveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && !cAbilityMove3.HasMoved && s_Waypoints.Count <= 0)
			{
				WorldspaceStarHexDisplay.Instance.ResetLineDirection();
			}
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.OnDelete().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static GameObject GetNextWaypoint()
	{
		try
		{
			if (s_Waypoints.Count == 0)
			{
				return null;
			}
			return s_Waypoints[0];
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.GetNextWaypoint().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public static List<CTile> GetCTileList()
	{
		try
		{
			List<CTile> list = new List<CTile>();
			foreach (GameObject s_Waypoint in s_Waypoints)
			{
				list.Add(GetWaypointComponent(s_Waypoint).m_ClientTile.m_Tile);
			}
			return list;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.GetCTileList().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public static void CacheWaypoints()
	{
		s_CachedWaypoints.Clear();
		s_CachedWaypoints.AddRange(s_Waypoints);
		s_CachedLockWaypoints = s_LockWaypoints;
	}

	public static void RestoreWaypoints()
	{
		if (s_CachedWaypoints.Count > 0)
		{
			s_Waypoints.Clear();
			s_Waypoints.AddRange(s_CachedWaypoints);
			s_CachedWaypoints.Clear();
			s_LockWaypoints = s_CachedLockWaypoints;
			s_CachedLockWaypoints = false;
		}
	}

	private void SetupWaypoint()
	{
		bool flag = true;
		try
		{
			IsTheLastWaypoint = s_Waypoints.FindIndex((GameObject x) => x == base.gameObject) == s_Waypoints.Count - 1;
			if (WaypointIsADoor)
			{
				Choreographer.s_Choreographer.readyButton.AlternativeAction(delegate
				{
					GetLastWaypoint.OpenDoor();
				}, ReadyButton.EButtonState.EREADYBUTTONOPENDOOR, LocalizationManager.GetTranslation("GUI_OPEN_DOOR"));
			}
			else
			{
				string text = string.Empty;
				switch (m_WaypointType)
				{
				case EWaypointType.Move:
					text = LocalizationManager.GetTranslation("GUI_CONFIRM_MOVEMENT");
					break;
				case EWaypointType.Pull:
					text = LocalizationManager.GetTranslation("GUI_CONFIRM_PULL");
					break;
				case EWaypointType.Push:
					text = LocalizationManager.GetTranslation("GUI_CONFIRM_PUSH");
					break;
				case EWaypointType.Attack:
					if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack)
					{
						ScenarioRuleClient.UpdateAttackpath(GetCTileList(), processImmediately: true);
					}
					if (WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets(GetCTileList()) > 0)
					{
						text = LocalizationManager.GetTranslation("GUI_CONFIRM_ATTACK");
					}
					else
					{
						flag = false;
					}
					break;
				}
				if (text != string.Empty)
				{
					Choreographer.s_Choreographer.readyButton.AlternativeAction(delegate
					{
						GetLastWaypoint.OnPress();
					}, ReadyButton.EButtonState.EREADYBUTTONCONFIRMMOVEMENT, text);
				}
			}
			Choreographer.s_Choreographer.readyButton.SetInteractable(CanEndMovementHere && flag && (Choreographer.s_Choreographer.ThisPlayerHasTurnControl || FFSNetwork.IsHost));
			Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.SetupWaypoint().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static void Create(CClientTile clientTile, int movesRemaining, int moveCost, int movesAvailable, EWaypointType waypointType, bool actorFlying, bool tileIsBlocked, List<Point> path = null)
	{
		try
		{
			SimpleLog.AddToSimpleLog("Creating Waypoint at " + clientTile.m_Tile.m_ArrayIndex.ToString());
			GameObject gameObject = UnityEngine.Object.Instantiate(GlobalSettings.Instance.m_WaypointPrefab);
			s_Waypoints.Add(gameObject);
			Waypoint component = gameObject.GetComponent<Waypoint>();
			component.m_ClientTile = clientTile;
			component.m_MovesRemaining = movesRemaining;
			component.m_MoveCost = moveCost;
			component.m_MovesAvailable = movesAvailable;
			component.m_WaypointType = waypointType;
			component.m_ActorFlying = actorFlying;
			component.m_TileIsBlocked = tileIsBlocked;
			component.SetOrder(GetHighestOrder() + 1, path);
			gameObject.transform.SetParent(WorldspaceUITools.Instance.WorldspaceCanvas.transform, worldPositionStays: false);
			gameObject.transform.localScale = Vector3.one;
			Choreographer.s_Choreographer.m_UndoButton.SetOnClickOverrider(delegate
			{
				if (s_Waypoints.Count != 0)
				{
					GetLastWaypoint.OnDelete();
					if (s_Waypoints.Count == 0 && waypointType == EWaypointType.Move)
					{
						Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, null, hideOnClick: true, glowingEffect: true);
						Choreographer.s_Choreographer.SetActiveSelectButton(!Choreographer.s_Choreographer.readyButton.gameObject.activeInHierarchy);
					}
				}
			}, UndoButton.EButtonState.EUNDOBUTTONUNDOWAYPOINT, LocalizationManager.GetTranslation("GUI_UNDO_WAYPOINT"));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.Create().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static Waypoint GetWaypointComponent(GameObject waypointGO)
	{
		return waypointGO.GetComponent<Waypoint>();
	}

	private static int GetHighestOrder()
	{
		int num = 0;
		foreach (GameObject s_Waypoint in s_Waypoints)
		{
			Waypoint component = s_Waypoint.GetComponent<Waypoint>();
			if (component.m_Order > num)
			{
				num = component.m_Order;
			}
		}
		return num;
	}

	public static void AddValidSelectionTiles(Point point)
	{
		s_ClearValidSelectionTiles.Add(point);
	}

	public static void ClearValidSelectionTiles()
	{
		s_ClearValidSelectionTiles.Clear();
	}

	public static void DestroyWaypoint(GameObject waypoint)
	{
		Waypoint component = waypoint.GetComponent<Waypoint>();
		SimpleLog.AddToSimpleLog("Destroying Waypoint at " + component.m_ClientTile.m_Tile.m_ArrayIndex.ToString());
		UnityEngine.Object.Destroy(waypoint);
		s_Waypoints.Remove(waypoint);
	}

	public static void Clear()
	{
		if (!s_LockWaypoints)
		{
			for (int num = s_Waypoints.Count - 1; num >= 0; num--)
			{
				if (!s_CachedWaypoints.Contains(s_Waypoints[num]))
				{
					DestroyWaypoint(s_Waypoints[num]);
				}
			}
		}
		ClearValidSelectionTiles();
	}

	private void SetupWaypointForDoor(bool active)
	{
		WorldspaceStarHexDisplay.Instance.ActorIsSelectingDoor(active);
	}

	private static bool IsLastWaypointClicked(CClientTile clientTile, bool actingPlayerHasSecondClickConfirmationEnabled = false)
	{
		if (s_Waypoints.Count <= 0)
		{
			return false;
		}
		Waypoint component = s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>();
		if (clientTile != component.ClientTile)
		{
			return false;
		}
		if (actingPlayerHasSecondClickConfirmationEnabled)
		{
			if (!Choreographer.s_Choreographer.readyButton.IsInteractable)
			{
				return false;
			}
			if (component.m_WaypointType != EWaypointType.Attack && !component.CanEndMovementHere)
			{
				return false;
			}
			if (Choreographer.s_Choreographer.readyButton.ActionsQueue.Count == 0)
			{
				Choreographer.s_Choreographer.readyButton.ActionsQueue.Add(delegate
				{
					GetLastWaypoint.OnPress();
				});
			}
			Choreographer.s_Choreographer.readyButton.OnClick();
		}
		else
		{
			Choreographer.s_Choreographer.m_UndoButton.OnClick();
		}
		return true;
	}

	private static void DeleteLastWaypoint()
	{
		GetLastWaypoint?.OnDelete();
		if (s_Waypoints.Count <= 0)
		{
			Choreographer.s_Choreographer.m_UndoButton.ClearOnClickOverriders();
		}
	}

	public static void TileHandler(CClientTile clientTile, List<CTile> optionalTileList, bool networkActionIfOnline = true, bool isUserClick = false, bool actingPlayerHasSecondClickConfirmationEnabled = false)
	{
		try
		{
			if (LevelEditorController.InterceptTileAction(clientTile) || DebugMenu.InterceptTileAction(clientTile))
			{
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Exiting early, action was intercepted");
				return;
			}
			if (Choreographer.s_Choreographer.m_TileSelectionDisabled && isUserClick)
			{
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Exiting early, tile selection is disabled");
				return;
			}
			if (AutoTestController.s_ShouldRecordUIActionsForAutoTest && isUserClick)
			{
				AutoTestController.s_Instance.LogTileClick(clientTile, optionalTileList, s_ClearValidSelectionTiles, null);
			}
			CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
			if (currentAbility != null && (currentAbility.IsWaitingForSingleTargetItem() || currentAbility.IsWaitingForSingleTargetActiveBonus()))
			{
				CActor cActor = currentAbility.ActorsToTarget.SingleOrDefault((CActor s) => s.ArrayIndex == clientTile.m_Tile.m_ArrayIndex);
				if (cActor != null)
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
					ScenarioRuleClient.ApplySingleTarget(cActor);
					if (FFSNetwork.IsOnline && networkActionIfOnline)
					{
						Synchronizer.SendGameActionClassID(GameActionType.ApplySingleTargetEffect, ActionProcessor.CurrentPhase, cActor.ID, (int)cActor.Type, supplementaryDataBool: true, cActor.Class.ID);
					}
				}
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Exiting early, single target item/active bonus was active with no target selected");
				return;
			}
			if (IsLastWaypointClicked(clientTile, actingPlayerHasSecondClickConfirmationEnabled))
			{
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Clicked previous waypoint twice. SecondClickConfirmationEnabled: " + actingPlayerHasSecondClickConfirmationEnabled);
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileCancelClickAudioItem);
				return;
			}
			if (IsLastWaypointADoor)
			{
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Exiting early, last waypoint is a door");
				return;
			}
			if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				s_ClearValidSelectionTiles = CAutoTileClick.TileIndexToAStarList((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoTileClick)?.Waypoints);
				if (s_ClearValidSelectionTiles == null)
				{
					Debug.LogError("Invalid tile selection.  No valid waypoints were found.");
					AutoTestController.s_Instance.CurrentAutoLogPlayback.TestEnded = true;
					return;
				}
			}
			if (Choreographer.s_Choreographer.m_CurrentAbility != null && (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull || Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush))
			{
				foreach (GameObject s_Waypoint in s_Waypoints)
				{
					if (s_Waypoint.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex == clientTile.m_Tile.m_ArrayIndex)
					{
						SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Exiting early, push/pull cannot select same waypoint twice");
						return;
					}
				}
			}
			if (s_ClearValidSelectionTiles.Any((Point it) => it.Equals(clientTile.m_Tile.m_ArrayIndex)))
			{
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Tile (" + clientTile.m_Tile.m_ArrayIndex.ToString() + ") is in valid selection tiles");
				Point startLocation = ((s_Waypoints.Count == 0) ? s_MovingActor.ArrayIndex : s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex);
				bool fly = false;
				bool jump = false;
				bool ignoreDifficultTerrain = false;
				bool ignoreHazardousTerrain = false;
				int num = 0;
				bool ignoreMoveCost = true;
				bool ignoreBlockedTileMoveCost = false;
				EWaypointType eWaypointType = EWaypointType.None;
				bool carryOtherActors = false;
				CActor.EType carryType = CActor.EType.Unknown;
				CAbilityMove cAbilityMove = null;
				if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove)
				{
					SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Current ability is a move ability");
					cAbilityMove = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityMove;
					fly = cAbilityMove.Fly;
					jump = cAbilityMove.Jump;
					ignoreDifficultTerrain = cAbilityMove.IgnoreDifficultTerrain;
					ignoreHazardousTerrain = cAbilityMove.IgnoreHazardousTerrain;
					ignoreBlockedTileMoveCost = cAbilityMove.IgnoreBlockedTileMoveCost;
					num = cAbilityMove.RemainingMoves;
					eWaypointType = EWaypointType.Move;
					ignoreMoveCost = false;
					carryOtherActors = cAbilityMove.CarryOtherActorsOnHex;
					if (cAbilityMove.ActorsToCarry.Count > 0)
					{
						int strength = 0;
						carryType = cAbilityMove.ActorsToCarry[0].Type;
						CAbilityMove.GetMoveBonuses(cAbilityMove.ActorsToCarry[0], out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, ref strength);
					}
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull cAbilityPull)
				{
					SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Current ability is a pull ability");
					num = cAbilityPull.RemainingPulls;
					eWaypointType = EWaypointType.Pull;
					if (cAbilityPull.CurrentTarget is CEnemyActor cEnemyActor)
					{
						fly = cEnemyActor.CachedFlying;
					}
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush cAbilityPush)
				{
					SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Current ability is a push ability");
					num = cAbilityPush.RemainingPushes;
					eWaypointType = EWaypointType.Push;
					if (cAbilityPush.CurrentTarget is CEnemyActor { CachedFlying: var cachedFlying })
					{
						fly = cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles) || cachedFlying;
					}
				}
				else if (WorldspaceStarHexDisplay.Instance.CurrentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.SelectPath && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack cAbilityAttack)
				{
					SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Current ability is an attack ability with path selection");
					num = cAbilityAttack.Range;
					eWaypointType = EWaypointType.Attack;
					fly = true;
				}
				bool avoidTraps = clientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap) == null;
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Checking paths to new potential waypoint (" + clientTile.m_Tile.m_ArrayIndex.ToString() + ")");
				bool foundPath;
				List<Point> path = CActor.FindCharacterPath(s_MovingActor, startLocation, clientTile.m_Tile.m_ArrayIndex, jump || fly, ignoreMoveCost, out foundPath, avoidTraps, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors, carryType);
				int num2 = ((s_Waypoints.Count > 0) ? s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>().MovesRemaining : num);
				if (!foundPath || CAbilityMove.CalculateMoveCost(path, !fly, !jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost) > num2)
				{
					SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: No path found avoiding traps, trying to find a path including traps");
					path = CActor.FindCharacterPath(s_MovingActor, startLocation, clientTile.m_Tile.m_ArrayIndex, jump || fly, ignoreMoveCost, out foundPath, avoidTraps: false, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors, carryType);
				}
				if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush { AdditionalPushEffect: var additionalPushEffect } cAbilityPush2 && additionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked) && clientTile.m_Tile.m_ArrayIndex == cAbilityPush2.CurrentTarget.ArrayIndex)
				{
					foundPath = true;
				}
				int num3 = CAbilityMove.CalculateMoveCost(path, !fly, !jump, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost, num2 < 2);
				int num4 = Mathf.Max(num2 - num3, 0);
				int num5 = 0;
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Found path: " + foundPath + ", AbilityMovesRemaining: " + num2 + ", MoveCost: " + num3);
				if (foundPath && num2 >= num3)
				{
					SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Found path and moves remaining is >= to move cost so we can create the waypoint");
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
					bool flag = false;
					if (eWaypointType == EWaypointType.Pull || eWaypointType == EWaypointType.Push)
					{
						if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull cAbilityPull2)
						{
							List<CTile> pullTiles = CAbilityPull.GetPullTiles(cAbilityPull2.CurrentTarget, cAbilityPull2.CurrentTarget.Type, clientTile.m_Tile.m_ArrayIndex, cAbilityPull2.PullToPoint, cAbilityPull2.PullType);
							if (pullTiles != null && pullTiles.Count > 0)
							{
								foreach (CTile item in pullTiles)
								{
									bool foundPath2;
									List<Point> list = CActor.FindCharacterPath(cAbilityPull2.CurrentTarget, cAbilityPull2.CurrentTarget.ArrayIndex, item.m_ArrayIndex, cAbilityPull2.CurrentTarget is CEnemyActor cEnemyActor3 && cEnemyActor3.CachedFlying, ignoreMoveCost, out foundPath2);
									if (foundPath2 && list.Count <= cAbilityPull2.RemainingPulls)
									{
										num5++;
									}
								}
							}
						}
						else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush cAbilityPush3)
						{
							num5 = CAbilityPush.GetPushTiles(cAbilityPush3.CurrentTarget, cAbilityPush3.CurrentTarget.Type, clientTile.m_Tile.m_ArrayIndex, cAbilityPush3.PushFromPoint, num4, cAbilityPush3.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles)).Count;
							if (cAbilityPush3.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked) && num5 > 0)
							{
								if (CAbilityPush.IsPushTileAdjacentToBlockedPushTile(clientTile.m_Tile, cAbilityPush3.CurrentTarget, cAbilityPush3.CurrentTarget.Type, clientTile.m_Tile.m_ArrayIndex, cAbilityPush3.PushFromPoint, num4, intoBlocked: true))
								{
									flag = true;
								}
								cAbilityPush3.UpdateAdditionalPushDamageSummary(flag, clientTile.m_Tile);
							}
						}
					}
					if (FFSNetwork.IsOnline && networkActionIfOnline && Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl)
					{
						ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
						bool enableSecondClickHexToConfirm = SaveData.Instance.Global.EnableSecondClickHexToConfirm;
						IProtocolToken supplementaryDataToken = new TilesToken(s_ClearValidSelectionTiles);
						Synchronizer.SendGameAction(GameActionType.CreateWaypoint, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, enableSecondClickHexToConfirm, default(Guid), supplementaryDataToken);
					}
					Create(clientTile, num4, num3, num5, eWaypointType, fly, flag, path);
					if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack)
					{
						ScenarioRuleClient.UpdateAttackpath(GetCTileList());
					}
					if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { CarryOtherActorsOnHex: not false })
					{
						ScenarioRuleClient.UpdateMoveCarry(GetCTileList());
					}
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush cAbilityPush4)
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
					cAbilityPush4.UpdateAdditionalPushDamageSummary(adjacentToBlockedTile: false, null);
				}
				else
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
				}
			}
			else
			{
				SimpleLog.AddToSimpleLog("[WAYPOINT TILE HANDLER]: Selected tile " + clientTile.m_Tile.m_ArrayIndex.ToString() + " was not found in the valid selection tile list");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the Waypoint.TileHandler().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static void ProxySelectTileToCreateMovementPath(GameAction action)
	{
		FFSNet.Console.LogInfo("PROXY: Creating movement path.", customFlag: true);
		TilesToken tilesToken = (TilesToken)action.SupplementaryDataToken;
		ClearValidSelectionTiles();
		for (int i = 0; i < tilesToken.Tiles.GetLength(0); i++)
		{
			int x = tilesToken.Tiles[i, 0];
			int y = tilesToken.Tiles[i, 1];
			AddValidSelectionTiles(new Point(x, y));
			FFSNet.Console.LogInfo("X: " + x + ", Y: " + y, customFlag: true);
		}
		int num = tilesToken.Tiles.GetLength(0) - 1;
		int num2 = tilesToken.Tiles[num, 0];
		int num3 = tilesToken.Tiles[num, 1];
		TileHandler(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[num2, num3], null, networkActionIfOnline: false, isUserClick: false, action.SupplementaryDataBoolean);
	}

	public static void ProxyDeleteLastWaypoint()
	{
		FFSNet.Console.LogInfo("PROXY: About to delete the last waypoint.");
		DeleteLastWaypoint();
	}

	public static void ProxyPressWaypoint(GameAction action)
	{
		FFSNet.Console.LogInfo("PROXY: About to press a waypoint.");
		TileToken tileToken = (TileToken)action.SupplementaryDataToken;
		foreach (GameObject s_Waypoint in s_Waypoints)
		{
			Waypoint waypointComponent = GetWaypointComponent(s_Waypoint);
			if (waypointComponent != null)
			{
				if (waypointComponent.m_ClientTile.m_Tile.m_ArrayIndex.X == tileToken.TileX && waypointComponent.m_ClientTile.m_Tile.m_ArrayIndex.Y == tileToken.TileY)
				{
					FFSNet.Console.LogInfo("PROXY: Waypoint found. Pressing waypoint.");
					waypointComponent.OnPress();
					return;
				}
				continue;
			}
			throw new Exception("Error pressing waypoint for proxy playerActor. Waypoint returns null.");
		}
		throw new Exception("Error selecting waypoint for proxy playerActor. No such waypoint exists (X = " + tileToken.TileX + ", Y = " + tileToken.TileY + ")");
	}
}
