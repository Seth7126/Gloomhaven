using System;
using UnityEngine;

public abstract class MapMovementFlow : MonoBehaviour
{
	protected Vector3[] m_WaypointsToEnd;

	public abstract float DelayToStartMoving { get; }

	public virtual void PrepareTravelTo(PartyToken partyToken, ref Vector3[] waypoints, MapLocation destination)
	{
		Prepare(partyToken, ref waypoints);
		FocusOnPartyMove(partyToken, DelayToStartMoving);
	}

	public virtual void Prepare(PartyToken partyToken, ref Vector3[] waypoints)
	{
		m_WaypointsToEnd = waypoints;
	}

	public abstract void MovePartyFromOriginToNarrativeDestination(PartyToken partyToken, Action onArrived);

	public abstract void MovePartyFromNarrativeToDestination(PartyToken partyToken, Action onArrived);

	public abstract void MovePartyFromEncounterToDestination(PartyToken partyToken, Action onArrived);

	public abstract void MovePartyToEncounter(PartyToken partyToken, Action onArrived);

	public abstract void MovePartyFromOriginToDestination(PartyToken partyToken, Action onArrived);

	public abstract void IncreaseZoomToParty(PartyToken partyToken, float duration = 0f, Action onFinished = null);

	public abstract void FocusOnPartyMove(PartyToken partyToken, float transitionTime);

	public abstract void MovePartyToJOTLEncounter(PartyToken partyToken, Action onEventTrigger);

	public abstract void MovePartyFromJOTLEncounter(PartyToken partyToken, Action action);

	public void TeleportToDestination(PartyToken mPartyToken, MapLocation movingToLocation)
	{
		mPartyToken.PartyInstantMove(movingToLocation.CenterPosition);
		FocusOnPartyMove(mPartyToken, 0f);
		CameraController.s_CameraController.ResetPositionToFocusOnTargetPoint();
	}
}
public abstract class MapMovementFlow<T> : MapMovementFlow where T : IMapFlowConfig
{
	[SerializeField]
	protected T mapConfig;

	public override float DelayToStartMoving => mapConfig.DelayToStartMoving;

	public override void Prepare(PartyToken partyToken, ref Vector3[] waypoints)
	{
		partyToken.MoveSpeed = mapConfig.PartyMovementSpeed;
		base.Prepare(partyToken, ref waypoints);
	}

	public override void IncreaseZoomToParty(PartyToken partyToken, float zoomTransitionDuration = 0f, Action onFinished = null)
	{
		if (mapConfig.IncreaseZoomToPartyToken < CameraController.s_CameraController.m_MinimumFOV)
		{
			CameraController.s_CameraController.m_ExtraMinimumFOV = mapConfig.IncreaseZoomToPartyToken - CameraController.s_CameraController.m_MinimumFOV;
		}
		else
		{
			CameraController.s_CameraController.m_ExtraMinimumFOV = 0f;
		}
		FocusOn(partyToken, mapConfig.IncreaseZoomToPartyToken, zoomTransitionDuration, onFinished);
	}

	public override void FocusOnPartyMove(PartyToken partyToken, float transitionTime)
	{
		float zoom = CameraController.s_CameraController.CalculateZoomByPercent(mapConfig.ZoomInPercentOnPartyMove);
		FocusOn(partyToken, zoom, transitionTime);
	}

	private void FocusOn(PartyToken partyToken, float zoom, float transitionTime, Action onFinished = null)
	{
		CameraController.s_CameraController.SetOverriddenBehavior(new MapFocusCameraBehavior(zoom, partyToken.transform.position, transitionTime, delegate
		{
			CameraController.s_CameraController.ClearOverriddenBehavior();
			onFinished?.Invoke();
		}));
	}
}
