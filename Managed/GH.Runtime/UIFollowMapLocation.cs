using System;
using UnityEngine;

public class UIFollowMapLocation : MonoBehaviour
{
	protected interface IFollowTarget
	{
		Vector3 GetScreenPoint(Vector3 offset);

		bool CanFollow();
	}

	[Serializable]
	private class FollowTransform : IFollowTarget
	{
		[SerializeField]
		private Transform targetTransform;

		public FollowTransform(Transform target = null)
		{
			targetTransform = target;
		}

		public Vector3 GetScreenPoint(Vector3 offset)
		{
			return ((targetTransform is RectTransform) ? UIManager.Instance.UICamera : CameraController.s_CameraController.m_Camera).WorldToScreenPoint(targetTransform.position + offset);
		}

		public bool CanFollow()
		{
			return targetTransform != null;
		}
	}

	[Serializable]
	private class FollowWorldPosition : IFollowTarget
	{
		[SerializeField]
		private Vector3 worldPosition;

		public FollowWorldPosition(Vector3 target)
		{
			worldPosition = target;
		}

		public Vector3 GetScreenPoint(Vector3 offset)
		{
			return CameraController.s_CameraController.m_Camera.WorldToScreenPoint(worldPosition + offset);
		}

		public bool CanFollow()
		{
			return true;
		}
	}

	[SerializeField]
	protected Vector3 offset;

	[SerializeReference]
	protected IFollowTarget target = new FollowTransform();

	private RectTransform container;

	protected void OnEnable()
	{
		container = base.transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
	}

	public void Track(Transform target, Vector3 offset)
	{
		this.target = new FollowTransform(target);
		this.offset = offset;
	}

	public void Track(Transform target)
	{
		Track(target, Vector3.zero);
	}

	public void Track(Vector3 target)
	{
		this.target = new FollowWorldPosition(target);
		offset = Vector3.zero;
	}

	protected virtual void LateUpdate()
	{
		if (target != null && !(container == null))
		{
			RefreshPosition();
		}
	}

	protected virtual void RefreshPosition()
	{
		CalculatePosition(offset);
	}

	protected void CalculatePosition(Vector3 offset)
	{
		if (target != null && !(container == null) && target.CanFollow())
		{
			Vector3 screenPoint = target.GetScreenPoint(offset);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screenPoint, UIManager.Instance.UICamera, out var localPoint);
			base.transform.localPosition = localPoint;
		}
	}
}
