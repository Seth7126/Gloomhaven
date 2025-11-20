using System;
using AStar;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIEventManager : MonoBehaviour
{
	public static event Action<UIEvent> OnEventLogged;

	public static void LogTileSelectEvent(Point tilePoint)
	{
		if (PhaseManager.Phase != null && PhaseManager.Phase is CPhaseAction && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
		{
			LogUIEvent(new UIEvent(UIEvent.EUIEventType.TileSelectedForAbility, null, tile: new TileIndex(tilePoint), contextId: ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility?.m_BaseCard?.Name));
		}
	}

	public static void LogActorEndedTurnEvent(CActor actorEndedTurn)
	{
		LogUIEvent(new UIEvent(UIEvent.EUIEventType.ActorEndedTurnOnTile, tile: new TileIndex(actorEndedTurn.ArrayIndex), contextId: actorEndedTurn.ActorGuid, actorPrefabName: actorEndedTurn.GetPrefabName()));
	}

	public static void LogUIEvent(UIEvent loggedEvent)
	{
		UIEventManager.OnEventLogged?.Invoke(loggedEvent);
	}
}
