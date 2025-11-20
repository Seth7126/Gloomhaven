using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using UnityEngine;

public class MapTimedMovementFlow : MapMovementFlow<MapTimedFlowConfig>
{
	public override void PrepareTravelTo(PartyToken partyToken, ref Vector3[] waypoints, MapLocation destination)
	{
		base.PrepareTravelTo(partyToken, ref waypoints, destination);
		if (destination.MapLocationType != MapLocation.EMapLocationType.Headquarters)
		{
			TeleportPartyToWayPoint(partyToken, CalculatePositionToStartMovement(partyToken), resetCamera: false);
		}
		IncreaseZoomToParty(partyToken, DelayToStartMoving);
	}

	protected void TeleportPartyToWayPoint(PartyToken partyoken, int point, bool resetCamera = true)
	{
		FFSNet.Console.LogInfo("WaypointsToEnd: " + m_WaypointsToEnd.ToStringPretty());
		FFSNet.Console.LogInfo("Skip to: " + point);
		m_WaypointsToEnd = m_WaypointsToEnd.Skip(point).Take(m_WaypointsToEnd.Length - point).ToArray();
		FFSNet.Console.LogInfo("WaypointsToEnd after teleport: " + m_WaypointsToEnd.ToStringPretty());
		if (m_WaypointsToEnd.Length != 0)
		{
			partyoken.PartyInstantMove(m_WaypointsToEnd[0]);
			if (resetCamera)
			{
				CameraController.s_CameraController.m_TargetFocalPoint = partyoken.transform.position;
				CameraController.s_CameraController.ResetPositionToFocusOnTargetPoint();
			}
		}
	}

	private void MovePartyFromOriginToDestination(PartyToken partyToken, Action onArrive, Action onFaded)
	{
		AudioControllerUtils.PlaySeqSounds(mapConfig.StartMovingAudioItem, mapConfig.MovingAudioItem);
		MovePartyToFadeIn(partyToken, mapConfig.DelayToFadeInBlack, delegate
		{
			onFaded?.Invoke();
			MovePartyToFadeOutDestination(partyToken, delegate
			{
				if (AudioControllerUtils.IsPlaying(mapConfig.MovingAudioItem))
				{
					AudioControllerUtils.StopSound(mapConfig.MovingAudioItem, mapConfig.FadeOutMovingAudioItemDuration);
					AudioControllerUtils.PlaySound(mapConfig.ReachDestinationAudioItem);
				}
				onArrive?.Invoke();
			});
		});
	}

	private void MovePartyToFadeIn(PartyToken partyToken, float duration, Action onFaded = null)
	{
		partyToken.PartyMoveTo(m_WaypointsToEnd, duration, delegate(List<Vector3> visitedPositions)
		{
			m_WaypointsToEnd = m_WaypointsToEnd.Skip(visitedPositions.Count).Take(m_WaypointsToEnd.Length - visitedPositions.Count).ToArray();
			onFaded?.Invoke();
		}, delegate(float progress)
		{
			TransitionManager.s_Instance.SetFade((progress < mapConfig.PartyPercentMoveToStartFadeInBlack) ? 0f : ((progress - mapConfig.PartyPercentMoveToStartFadeInBlack) / (1f - mapConfig.PartyPercentMoveToStartFadeInBlack)));
		});
	}

	private void MovePartyToFadeOutDestination(PartyToken partyToken, Action onFinished = null)
	{
		TeleportPartyToWayPoint(partyToken, CalculatePositionToSkipNearDestination(partyToken));
		Singleton<MapChoreographer>.Instance.StartCoroutine(CoroutineHelper.DelayedStartCoroutine(mapConfig.PartyTravelFadedBlackDuration, delegate
		{
			float num = CalculateDistanceToDest(partyToken) / partyToken.MoveSpeed;
			float delay = ((num < mapConfig.PlayReachDestinationSFXTimeOffset) ? num : (num - mapConfig.PlayReachDestinationSFXTimeOffset));
			Singleton<MapChoreographer>.Instance.StartCoroutine(CoroutineHelper.DelayedStartCoroutine(delay, delegate
			{
				if (AudioControllerUtils.IsPlaying(mapConfig.MovingAudioItem))
				{
					AudioControllerUtils.StopSound(mapConfig.MovingAudioItem, mapConfig.FadeOutMovingAudioItemDuration);
					AudioControllerUtils.PlaySound(mapConfig.ReachDestinationAudioItem);
				}
			}));
			partyToken.PartyMoveTo(m_WaypointsToEnd, onFinished, delegate(float progress)
			{
				TransitionManager.s_Instance.SetFade((progress > mapConfig.PartyPercentMoveToFinishFadeOutBlack) ? 0f : (1f - progress / mapConfig.PartyPercentMoveToFinishFadeOutBlack));
			});
		}));
	}

	private int CalculatePositionToSkipNearDestination(PartyToken partyToken)
	{
		float num = mapConfig.DelayToArriveDestination * partyToken.MoveSpeed;
		if (num == 0f)
		{
			return m_WaypointsToEnd.Length;
		}
		int num2 = m_WaypointsToEnd.Length - 1;
		float num3 = 0f;
		for (int num4 = m_WaypointsToEnd.Length - 1; num4 > 0; num4--)
		{
			num3 += Vector3.Distance(m_WaypointsToEnd[num4], m_WaypointsToEnd[num4 - 1]);
			num2--;
			if (num3 >= num)
			{
				break;
			}
		}
		return Math.Max(num2, 0);
	}

	private int CalculatePositionToStartMovement(PartyToken partyToken)
	{
		if (mapConfig.PartyMinStartTravelDistance == 0f)
		{
			return 0;
		}
		int num = 1;
		float num2 = ((m_WaypointsToEnd.Length == 0) ? 0f : Vector3.Distance(partyToken.transform.position, m_WaypointsToEnd[0]));
		if (num2 >= mapConfig.PartyMinStartTravelDistance)
		{
			return num;
		}
		for (int i = 0; i < m_WaypointsToEnd.Length - 1; i++)
		{
			float num3 = num2;
			num2 += Vector3.Distance(m_WaypointsToEnd[i], m_WaypointsToEnd[i + 1]);
			if (num2 >= mapConfig.PartyMinStartTravelDistance)
			{
				if (Math.Abs(num2 - mapConfig.PartyMinStartTravelDistance) <= Math.Abs(num3 - mapConfig.PartyMinStartTravelDistance))
				{
					num++;
				}
				break;
			}
			num++;
		}
		return num;
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

	public override void MovePartyFromOriginToNarrativeDestination(PartyToken partyToken, Action onArrived)
	{
		MovePartyFromOriginToDestination(partyToken, onArrived, delegate
		{
			FocusOnPartyMove(partyToken, 0f);
		});
	}

	public override void MovePartyFromNarrativeToDestination(PartyToken partyToken, Action onArrived)
	{
		MovePartyFromOriginToNarrativeDestination(partyToken, onArrived);
	}

	public override void MovePartyFromOriginToDestination(PartyToken partyToken, Action onArrived)
	{
		MovePartyFromOriginToDestination(partyToken, onArrived, delegate
		{
			IncreaseZoomToParty(partyToken);
		});
	}

	private void OnDestroy()
	{
		if (AudioControllerUtils.IsPlaying(mapConfig.MovingAudioItem))
		{
			AudioControllerUtils.StopSound(mapConfig.MovingAudioItem);
		}
	}

	public override void MovePartyFromEncounterToDestination(PartyToken partyToken, Action onArrived)
	{
		AudioControllerUtils.PlaySound(mapConfig.MovingAudioItem);
		IncreaseZoomToParty(partyToken);
		MovePartyToFadeOutDestination(partyToken, delegate
		{
			if (AudioControllerUtils.IsPlaying(mapConfig.MovingAudioItem))
			{
				AudioControllerUtils.StopSound(mapConfig.MovingAudioItem, mapConfig.FadeOutMovingAudioItemDuration);
				AudioControllerUtils.PlaySound(mapConfig.ReachDestinationAudioItem);
			}
			onArrived?.Invoke();
		});
	}

	public override void MovePartyToEncounter(PartyToken partyToken, Action onEventTrigger)
	{
		AudioControllerUtils.PlaySeqSounds(mapConfig.StartMovingAudioItem, mapConfig.MovingAudioItem);
		MovePartyToFadeIn(partyToken, mapConfig.DelayToEncounter, onEventTrigger);
	}

	public override void MovePartyToJOTLEncounter(PartyToken partyToken, Action onEventTrigger)
	{
		float duration = mapConfig.DelayToEncounter * (1f - mapConfig.PartyPercentMoveToStartFadeInBlack);
		TransitionManager.s_Instance.Fade(onEventTrigger, duration, 0f, 1f);
	}

	public override void MovePartyFromJOTLEncounter(PartyToken partyToken, Action action)
	{
		TeleportPartyToWayPoint(partyToken, CalculatePositionToStartMovement(partyToken));
		FocusOnPartyMove(partyToken, 0f);
		TransitionManager.s_Instance.Fade(action, mapConfig.DelayToEncounter * (1f - mapConfig.PartyPercentMoveToStartFadeInBlack), 1f, 0f);
	}
}
