#define ENABLE_LOGS
using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class WorldspaceDisplayPanelBase : MonoBehaviour
{
	public enum PoinToTrack
	{
		HeadBone,
		Base,
		HeadBoneStatic
	}

	public enum ScaleType
	{
		Scale,
		Size
	}

	[Serializable]
	public class ScaleUpdatedEvent : UnityEvent<float>
	{
	}

	public const float Epsilon = 0.0001f;

	public float m_GUIScreenSpaceOffsetX;

	public float m_GUIScreenSpaceOffsetY;

	[HideInInspector]
	public float m_WorldspaceOffsetY;

	public float SwitchOffUIAtHeight;

	public bool DontScaleUI;

	public ScaleType m_ScaleType;

	public bool m_ScalingWidth = true;

	public bool m_ScalingHeight = true;

	public RectTransform m_ScaleRectTransform;

	public AnimationCurve m_ScalingCurve;

	public AnimationCurve m_TrackingCurve;

	public ScaleUpdatedEvent OnScaleUpdated = new ScaleUpdatedEvent();

	private Transform m_PointToTrack;

	protected Vector2 m_StandardPanelScale;

	protected Vector2 m_BaseActorScale;

	public PoinToTrack m_PointToTrackOnActor;

	protected GameObject m_ObjectToTrack;

	protected Transform m_UIZoomMeasurePoint;

	protected ActorBehaviour m_ActorBehaviour;

	protected CActor m_Actor;

	private Transform m_HeadBonePoint;

	private Transform m_BasePoint;

	private Vector3 m_HeadBaseOffset;

	private RectTransform m_RectTransform;

	private RectTransform m_CanvasRect;

	private void Awake()
	{
		m_BaseActorScale = ((m_ScaleType == ScaleType.Scale) ? new Vector2(m_ScaleRectTransform.localScale.x, m_ScaleRectTransform.localScale.y) : m_ScaleRectTransform.sizeDelta);
	}

	public void Init(GameObject characterToTrack, Transform UIZoomMeasurePoint, CActor actor, ActorBehaviour actorBehaviour, Vector2 increaseScale)
	{
		m_Actor = actor;
		m_ObjectToTrack = characterToTrack;
		m_UIZoomMeasurePoint = UIZoomMeasurePoint;
		m_ActorBehaviour = actorBehaviour;
		if (m_ScaleType == ScaleType.Scale)
		{
			Vector3 vector = (m_ScaleRectTransform.localScale = m_BaseActorScale * increaseScale);
			m_StandardPanelScale = vector;
		}
		else
		{
			Vector2 standardPanelScale = (m_ScaleRectTransform.sizeDelta = m_BaseActorScale * increaseScale);
			m_StandardPanelScale = standardPanelScale;
		}
		m_RectTransform = GetComponent<RectTransform>();
		m_CanvasRect = WorldspaceUITools.Instance.WorldspaceCanvas.GetComponent<RectTransform>();
		if (characterToTrack.FindInChildren("C_headSkel01_JNT") != null)
		{
			m_HeadBonePoint = characterToTrack.gameObject.FindInChildren("C_headSkel01_JNT").transform;
		}
		else if (m_PointToTrackOnActor == PoinToTrack.HeadBone)
		{
			Debug.Log("Unable to find head bone on character " + characterToTrack.name);
			base.gameObject.SetActive(value: false);
		}
		if (characterToTrack.FindInChildren("Base") != null)
		{
			m_BasePoint = characterToTrack.gameObject.FindInChildren("Base").transform;
		}
		else
		{
			Debug.Log("Unable to find base on character " + characterToTrack.name);
			base.gameObject.SetActive(value: false);
		}
		if (m_HeadBonePoint != null && m_BasePoint != null)
		{
			m_HeadBaseOffset = m_HeadBonePoint.position - m_BasePoint.position;
		}
	}

	public void InitTileBehaviour(GameObject objectToTrack, Transform UIZoomMeasurePoint, Vector2 increaseScale)
	{
		m_ObjectToTrack = objectToTrack;
		m_UIZoomMeasurePoint = UIZoomMeasurePoint;
		if (m_ScaleType == ScaleType.Scale)
		{
			Vector3 vector = (m_ScaleRectTransform.localScale = m_BaseActorScale * increaseScale);
			m_StandardPanelScale = vector;
		}
		else
		{
			Vector2 standardPanelScale = (m_ScaleRectTransform.sizeDelta = m_BaseActorScale * increaseScale);
			m_StandardPanelScale = standardPanelScale;
		}
		m_RectTransform = GetComponent<RectTransform>();
		m_CanvasRect = WorldspaceUITools.Instance.WorldspaceCanvas.GetComponent<RectTransform>();
		m_BasePoint = UIZoomMeasurePoint;
	}

	private void LateUpdate()
	{
		if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario || (m_Actor != null && m_Actor.IsDead) || !(m_ObjectToTrack != null) || !(m_BasePoint != null))
		{
			return;
		}
		TrackCharacter();
		if (DontScaleUI)
		{
			return;
		}
		float num = m_ScalingCurve.Evaluate(CalculateViewportZoom());
		Vector2 vector = new Vector2(m_ScalingWidth ? num : 1f, m_ScalingHeight ? num : 1f);
		vector.Scale(m_StandardPanelScale);
		bool flag = false;
		switch (m_ScaleType)
		{
		case ScaleType.Scale:
			if (Mathf.Abs(vector.x - m_ScaleRectTransform.localScale.x) > 0.0001f || Mathf.Abs(vector.y - m_ScaleRectTransform.localScale.y) > 0.0001f)
			{
				m_ScaleRectTransform.localScale = vector;
				flag = true;
			}
			break;
		case ScaleType.Size:
			flag = vector != m_ScaleRectTransform.sizeDelta;
			m_ScaleRectTransform.sizeDelta = vector;
			break;
		}
		if (flag)
		{
			OnScaleUpdated.Invoke(num);
		}
	}

	public void TrackCharacter()
	{
		if (!(WorldspaceUITools.Instance.WorldspaceCamera == null))
		{
			Vector3 position = ((m_PointToTrackOnActor == PoinToTrack.HeadBone) ? m_HeadBonePoint.position : ((m_PointToTrackOnActor != PoinToTrack.Base) ? (m_BasePoint.position + m_HeadBaseOffset) : m_BasePoint.position));
			position.y += m_WorldspaceOffsetY;
			Vector2 screenPoint = WorldspaceUITools.Instance.WorldspaceCamera.WorldToScreenPoint(position);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, screenPoint, WorldspaceUITools.Instance.WorldspaceCanvas.worldCamera, out var localPoint);
			Vector2 vector = new Vector2(m_GUIScreenSpaceOffsetX, m_GUIScreenSpaceOffsetY) * m_TrackingCurve.Evaluate(CalculateViewportZoom());
			Vector3 localPosition = localPoint + vector;
			Vector3 localPosition2 = m_RectTransform.localPosition;
			if (Mathf.Abs(localPosition.x - localPosition2.x) > 0.0001f || Mathf.Abs(localPosition.y - localPosition2.y) > 0.0001f || Mathf.Abs(localPosition.z - localPosition2.z) > 0.0001f)
			{
				m_RectTransform.localPosition = localPosition;
			}
		}
	}

	private float CalculateViewportZoom()
	{
		if (WorldspaceUITools.Instance.WorldspaceCamera == null)
		{
			return 0f;
		}
		Vector2 vector = WorldspaceUITools.Instance.WorldspaceCamera.WorldToViewportPoint(m_UIZoomMeasurePoint.position);
		return Vector3.Distance((Vector2)WorldspaceUITools.Instance.WorldspaceCamera.WorldToViewportPoint(m_ObjectToTrack.transform.position), vector);
	}
}
