using System;
using System.Linq;
using UnityEngine;

public class MapPointsMovementFlow : MapMovementFlow<MapPointsFlowConfig>
{
	public override void MovePartyFromOriginToNarrativeDestination(PartyToken partyToken, Action onArrived)
	{
		MovePartyFromOriginToDestination(partyToken, onArrived);
	}

	public override void MovePartyFromNarrativeToDestination(PartyToken partyToken, Action onArrived)
	{
		MovePartyFromOriginToDestination(partyToken, onArrived);
	}

	public override void MovePartyFromEncounterToDestination(PartyToken partyToken, Action onArrived)
	{
		int num = (int)((float)m_WaypointsToEnd.Length * mapConfig.PercentMovedToEncounter);
		m_WaypointsToEnd = m_WaypointsToEnd.Skip(num).Take(m_WaypointsToEnd.Length - num).ToArray();
		float num2 = CalculateDistanceToDest(partyToken) / partyToken.MoveSpeed;
		float delay = ((num2 < mapConfig.PlayReachDestinationSFXTimeOffset) ? num2 : (num2 - mapConfig.PlayReachDestinationSFXTimeOffset));
		Singleton<MapChoreographer>.Instance.StartCoroutine(CoroutineHelper.DelayedStartCoroutine(delay, delegate
		{
			AudioControllerUtils.StopSound(mapConfig.MovingAudioItem, mapConfig.FadeOutMovingAudioItemDuration);
			AudioControllerUtils.PlaySound(mapConfig.ReachDestinationAudioItem);
		}));
		AudioControllerUtils.PlaySound(mapConfig.MovingAudioItem);
		partyToken.PartyMoveTo(m_WaypointsToEnd, onArrived);
	}

	public override void MovePartyToEncounter(PartyToken partyToken, Action onEventTrigger)
	{
		AudioControllerUtils.PlaySound(mapConfig.MovingAudioItem);
		Vector3[] positions = m_WaypointsToEnd.Take((int)((float)m_WaypointsToEnd.Length * mapConfig.PercentMovedToEncounter)).ToArray();
		partyToken.PartyMoveTo(positions, onEventTrigger);
	}

	public override void MovePartyFromOriginToDestination(PartyToken partyToken, Action onArrived)
	{
		float num = CalculateDistanceToDest(partyToken) / partyToken.MoveSpeed;
		float delay = ((num < mapConfig.PlayReachDestinationSFXTimeOffset) ? num : (num - mapConfig.PlayReachDestinationSFXTimeOffset));
		Singleton<MapChoreographer>.Instance.StartCoroutine(CoroutineHelper.DelayedStartCoroutine(delay, delegate
		{
			AudioControllerUtils.StopSound(mapConfig.MovingAudioItem, mapConfig.FadeOutMovingAudioItemDuration);
			AudioControllerUtils.PlaySound(mapConfig.ReachDestinationAudioItem);
		}));
		AudioControllerUtils.PlaySound(mapConfig.MovingAudioItem);
		partyToken.PartyMoveTo(m_WaypointsToEnd, onArrived);
	}

	public override void MovePartyToJOTLEncounter(PartyToken partyToken, Action onEventTrigger)
	{
		onEventTrigger?.Invoke();
	}

	public override void MovePartyFromJOTLEncounter(PartyToken partyToken, Action action)
	{
		action?.Invoke();
	}

	private float CalculateDistanceToDest(PartyToken partyToken)
	{
		float num = ((m_WaypointsToEnd.Length == 0) ? 0f : Vector3.Distance(partyToken.transform.position, m_WaypointsToEnd[0]));
		for (int i = 0; i < m_WaypointsToEnd.Length - 1; i++)
		{
			num += Vector3.Distance(m_WaypointsToEnd[i], m_WaypointsToEnd[i + 1]);
		}
		return num;
	}

	private void OnDestroy()
	{
		if (AudioControllerUtils.IsPlaying(mapConfig.MovingAudioItem))
		{
			AudioControllerUtils.StopSound(mapConfig.MovingAudioItem);
		}
	}
}
