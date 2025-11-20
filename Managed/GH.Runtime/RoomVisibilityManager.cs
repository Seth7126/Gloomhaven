using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class RoomVisibilityManager : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private GameObject _maps;

	[SerializeField]
	[UsedImplicitly]
	private GameObject _props;

	[SerializeField]
	[UsedImplicitly]
	private Camera _scenarioCamera;

	public static RoomVisibilityManager s_Instance;

	private List<RoomVisibilityTracker> m_RoomTrackers = new List<RoomVisibilityTracker>();

	public GameObject Maps => _maps;

	public Camera ScenarioCamera => _scenarioCamera;

	public GameObject Props => _props;

	[UsedImplicitly]
	private void Awake()
	{
		s_Instance = this;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		s_Instance = null;
	}

	public void AddRoom(RoomVisibilityTracker roomTracker)
	{
		m_RoomTrackers.Add(roomTracker);
	}

	public void EvaluateRoomsVisiblity()
	{
		for (int num = m_RoomTrackers.Count - 1; num >= 0; num--)
		{
			if (m_RoomTrackers[num].IsVisible())
			{
				m_RoomTrackers[num].EnableAssets();
				m_RoomTrackers.RemoveAt(num);
			}
		}
	}

	public void ReloadCamera()
	{
		CoroutineHelper.RunCoroutine(ReloadCameraCoroutine());
	}

	private IEnumerator ReloadCameraCoroutine()
	{
		yield return null;
		_scenarioCamera.enabled = false;
		yield return null;
		_scenarioCamera.enabled = true;
		yield return null;
		_scenarioCamera.enabled = false;
		yield return null;
		_scenarioCamera.enabled = true;
	}
}
