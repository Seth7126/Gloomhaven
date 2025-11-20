#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.SpecialStates;
using UnityEngine;

public class PartyToken : MonoBehaviour
{
	public float MoveSpeed;

	private Coroutine stopCoroutine;

	private Action onArrivedCallback;

	public bool IsMoving => stopCoroutine != null;

	public void PartyLookAt(Vector3 position)
	{
		base.transform.LookAt(position);
	}

	public void PartyMoveTo(Vector3[] positions, Action callback = null, Action<float> onProgress = null)
	{
		SetOnArriveCallback(callback);
		stopCoroutine = StartCoroutine(PartyMoveCoroutine(positions, onProgress));
	}

	private IEnumerator PartyMoveCoroutine(Vector3[] positions, Action<float> onProgress = null)
	{
		BeginMoving();
		float totalDistance = ((positions.Length != 0) ? Vector3.Distance(base.transform.position, positions[0]) : 0f);
		float traveledDistance = 0f;
		for (int i = 0; i < positions.Length - 1; i++)
		{
			totalDistance += Vector3.Distance(positions[i], positions[i + 1]);
		}
		foreach (Vector3 pos in positions)
		{
			while (base.transform.position != pos)
			{
				Vector3 vector = Vector3.MoveTowards(base.transform.position, pos, MoveSpeed * Timekeeper.instance.m_GlobalClock.deltaTime);
				traveledDistance += Vector3.Distance(vector, base.transform.position);
				base.transform.position = vector;
				CameraController.s_CameraController.m_TargetFocalPoint = base.transform.position;
				base.transform.LookAt(pos);
				onProgress?.Invoke(traveledDistance / totalDistance);
				yield return new WaitForEndOfFrame();
			}
		}
		onProgress?.Invoke(1f);
		CompleteMoving();
	}

	public void SetOnArriveCallback(Action callback)
	{
		onArrivedCallback = callback;
	}

	public void PartyInstantMove(Vector3 position)
	{
		StopMoving();
		base.transform.position = position;
	}

	public void StopMoving()
	{
		if (stopCoroutine != null)
		{
			StopCoroutine(stopCoroutine);
		}
		stopCoroutine = null;
	}

	public void PartyMoveTo(Vector3[] positions, float duration, Action<List<Vector3>> callback = null, Action<float> onProgress = null)
	{
		List<Vector3> visitedPoints = new List<Vector3>();
		SetOnArriveCallback(delegate
		{
			callback?.Invoke(visitedPoints);
		});
		stopCoroutine = StartCoroutine(PartyMoveCoroutine(positions, duration, visitedPoints, onProgress));
	}

	private IEnumerator PartyMoveCoroutine(Vector3[] positions, float duration, List<Vector3> visitedPositions, Action<float> onProgress = null)
	{
		BeginMoving();
		float timeSpent = 0f;
		visitedPositions.Clear();
		float totalDistance = ((positions.Length != 0) ? Vector3.Distance(base.transform.position, positions[0]) : 0f);
		float traveledDistance = 0f;
		for (int i = 0; i < positions.Length - 1; i++)
		{
			totalDistance += Vector3.Distance(positions[i], positions[i + 1]);
		}
		foreach (Vector3 pos in positions)
		{
			while (base.transform.position != pos)
			{
				Vector3 vector = Vector3.MoveTowards(base.transform.position, pos, MoveSpeed * Timekeeper.instance.m_GlobalClock.deltaTime);
				traveledDistance += Vector3.Distance(vector, base.transform.position);
				base.transform.position = vector;
				CameraController.s_CameraController.m_TargetFocalPoint = base.transform.position;
				if (CameraController.s_CameraController.m_IsCameraCodeControlDisabled)
				{
					Debug.LogWarning("Trying to move camera to token but m_IsCameraCodeControlDisabled is disabled");
				}
				if (CameraController.s_CameraController.OverriddenBehavior != null)
				{
					Debug.LogWarning("Trying to move camera to token but there is an OverriddenBehavior " + CameraController.s_CameraController.OverriddenBehavior.ID + " enabled " + CameraController.s_CameraController.OverriddenBehavior.IsEnabled);
				}
				if (Main.s_Pause3DWorld)
				{
					Debug.LogWarning("Trying to move camera to token but world is paused");
				}
				base.transform.LookAt(pos);
				yield return new WaitForEndOfFrame();
				timeSpent += Timekeeper.instance.m_GlobalClock.deltaTime;
				onProgress?.Invoke(Mathf.Max(Mathf.Clamp01(timeSpent / duration), traveledDistance / totalDistance));
				if (timeSpent > duration)
				{
					CompleteMoving();
					yield break;
				}
			}
			visitedPositions.Add(pos);
		}
		CompleteMoving();
	}

	private void BeginMoving()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(SpecialStateTag.Lock);
	}

	private void CompleteMoving()
	{
		stopCoroutine = null;
		onArrivedCallback?.Invoke();
	}
}
