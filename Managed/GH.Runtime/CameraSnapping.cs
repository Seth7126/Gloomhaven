using Code.State;
using InControl;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.ScenarioStates;
using UnityEngine;

public class CameraSnapping : MonoBehaviour
{
	[SerializeField]
	private float _maxDistance = 1000f;

	[SerializeField]
	private LayerMask _layerMask = 1048576;

	[SerializeField]
	private float _snappingBreakThreshold = 0.65f;

	[SerializeField]
	private int _updateFrequency = 15;

	private readonly Vector3 CenterOfScreen = new Vector3(0.5f, 0.5f, 0f);

	private readonly RaycastHit[] ResultHits = new RaycastHit[1];

	private CameraController _cameraController;

	private Camera _camera;

	private PlayerTwoAxisAction _input;

	private Vector3 _closestPosition;

	private bool _alreadySnapped;

	private int _frequencyCount;

	private void Awake()
	{
		_cameraController = CameraController.s_CameraController;
		_camera = GetComponent<Camera>();
	}

	private void Update()
	{
		IState currentState = Singleton<UINavigation>.Instance.StateMachine.CurrentState;
		if (!(currentState is ScenarioState))
		{
			if (!(currentState is WorldMapState))
			{
				return;
			}
			_input = Singleton<InputManager>.Instance.PlayerControl.PanCameraMap;
		}
		else
		{
			_input = Singleton<InputManager>.Instance.PlayerControl.PanCamera;
		}
		if (_input.Vector.sqrMagnitude > _snappingBreakThreshold || !_cameraController.SnappingEnabled || (_input.Vector.sqrMagnitude > 0f && (_cameraController.m_TargetFocalPoint - _closestPosition).sqrMagnitude > 1f))
		{
			_alreadySnapped = false;
			return;
		}
		if (_alreadySnapped)
		{
			UpdateCameraPosition();
			return;
		}
		if (_frequencyCount < _updateFrequency)
		{
			_frequencyCount++;
			return;
		}
		_frequencyCount = 0;
		if (GetClosestLocation(out _closestPosition))
		{
			_alreadySnapped = true;
			UpdateCameraPosition();
		}
	}

	private void UpdateCameraPosition()
	{
		_cameraController.m_TargetFocalPoint = _closestPosition;
	}

	private bool GetClosestLocation(out Vector3 closestPosition)
	{
		closestPosition = Vector3.zero;
		if (Physics.RaycastNonAlloc(_camera.ViewportPointToRay(CenterOfScreen), ResultHits, _maxDistance, _layerMask) <= 0)
		{
			return false;
		}
		closestPosition = ResultHits[0].transform.position;
		return true;
	}
}
