using UnityEngine;

public class CameraTargetFocalFollowController
{
	private readonly CameraController _cameraController;

	private GameObject _targetGameObject;

	private CInteractable _targetInteractable;

	private bool _isFollowingTarget;

	private bool _pauseDuringTransition;

	public bool IsFollowingTarget => _isFollowingTarget;

	public CameraTargetFocalFollowController(CameraController cameraController)
	{
		_cameraController = cameraController;
	}

	public void TryUpdatePoint()
	{
		if ((bool)_targetInteractable)
		{
			_cameraController.m_TargetFocalPoint = _targetInteractable.transform.position;
		}
		else if ((bool)_targetGameObject)
		{
			_cameraController.m_TargetFocalPoint = _targetGameObject.transform.position;
		}
	}

	public void TryRemovePoint(GameObject point)
	{
		if (_targetGameObject == point)
		{
			SetPoint(null);
		}
	}

	public void ResetPoint()
	{
		SetPoint(null);
	}

	public void SetPoint(GameObject gameobject)
	{
		_targetGameObject = gameobject;
		_targetInteractable = (gameobject ? gameobject.GetComponent<CInteractable>() : null);
	}

	public void SetPoint(Vector3 position, bool pauseDuringTransition = false)
	{
		_cameraController.ResetFocalPointGameObject();
		if (pauseDuringTransition && !TimeManager.IsPaused)
		{
			_pauseDuringTransition = pauseDuringTransition;
			TimeManager.PauseTime();
		}
		_isFollowingTarget = true;
		_cameraController.m_TargetFocalPoint = position;
	}

	public void OnArrivedToPoint()
	{
		_isFollowingTarget = false;
		if (_pauseDuringTransition)
		{
			TimeManager.UnpauseTime();
			_pauseDuringTransition = false;
		}
	}
}
