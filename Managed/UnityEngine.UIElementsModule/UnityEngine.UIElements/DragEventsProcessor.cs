#define UNITY_ASSERTIONS
using UnityEngine.Assertions;

namespace UnityEngine.UIElements;

internal abstract class DragEventsProcessor
{
	private enum DragState
	{
		None,
		CanStartDrag,
		Dragging
	}

	private bool m_IsRegistered;

	private DragState m_DragState;

	private Vector3 m_Start;

	internal readonly VisualElement m_Target;

	private const int k_DistanceToActivation = 5;

	internal DefaultDragAndDropClient dragAndDropClient;

	internal bool isRegistered => m_IsRegistered;

	internal virtual bool supportsDragEvents => true;

	internal bool useDragEvents => isEditorContext && supportsDragEvents;

	private bool isEditorContext
	{
		get
		{
			Assert.IsNotNull(m_Target);
			Assert.IsNotNull(m_Target.parent);
			return m_Target.panel.contextType == ContextType.Editor;
		}
	}

	internal DragEventsProcessor(VisualElement target)
	{
		m_Target = target;
		m_Target.RegisterCallback<AttachToPanelEvent>(RegisterCallbacksFromTarget);
		m_Target.RegisterCallback<DetachFromPanelEvent>(UnregisterCallbacksFromTarget);
		RegisterCallbacksFromTarget();
	}

	private void RegisterCallbacksFromTarget(AttachToPanelEvent evt)
	{
		RegisterCallbacksFromTarget();
	}

	private void RegisterCallbacksFromTarget()
	{
		if (!m_IsRegistered)
		{
			m_IsRegistered = true;
			m_Target.RegisterCallback<PointerDownEvent>(OnPointerDownEvent, TrickleDown.TrickleDown);
			m_Target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent, TrickleDown.TrickleDown);
			m_Target.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
			m_Target.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
			m_Target.RegisterCallback<PointerCancelEvent>(OnPointerCancelEvent);
		}
	}

	private void UnregisterCallbacksFromTarget(DetachFromPanelEvent evt)
	{
		UnregisterCallbacksFromTarget();
	}

	internal void UnregisterCallbacksFromTarget(bool unregisterPanelEvents = false)
	{
		m_IsRegistered = false;
		m_Target.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent, TrickleDown.TrickleDown);
		m_Target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent, TrickleDown.TrickleDown);
		m_Target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
		m_Target.UnregisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
		m_Target.UnregisterCallback<PointerCancelEvent>(OnPointerCancelEvent);
		if (unregisterPanelEvents)
		{
			m_Target.UnregisterCallback<AttachToPanelEvent>(RegisterCallbacksFromTarget);
			m_Target.UnregisterCallback<DetachFromPanelEvent>(UnregisterCallbacksFromTarget);
		}
	}

	protected abstract bool CanStartDrag(Vector3 pointerPosition);

	protected abstract StartDragArgs StartDrag(Vector3 pointerPosition);

	protected abstract DragVisualMode UpdateDrag(Vector3 pointerPosition);

	protected abstract void OnDrop(Vector3 pointerPosition);

	protected abstract void ClearDragAndDropUI();

	private void OnPointerDownEvent(PointerDownEvent evt)
	{
		if (evt.button != 0)
		{
			m_DragState = DragState.None;
		}
		else if (CanStartDrag(evt.position))
		{
			m_DragState = DragState.CanStartDrag;
			m_Start = evt.position;
		}
	}

	internal void OnPointerUpEvent(PointerUpEvent evt)
	{
		if (!useDragEvents && m_DragState == DragState.Dragging)
		{
			m_Target.ReleasePointer(evt.pointerId);
			OnDrop(evt.position);
			ClearDragAndDropUI();
			evt.StopPropagation();
		}
		m_DragState = DragState.None;
	}

	private void OnPointerLeaveEvent(PointerLeaveEvent evt)
	{
		if (evt.target == m_Target)
		{
			ClearDragAndDropUI();
		}
	}

	private void OnPointerCancelEvent(PointerCancelEvent evt)
	{
		if (!useDragEvents)
		{
			ClearDragAndDropUI();
		}
	}

	private void OnPointerMoveEvent(PointerMoveEvent evt)
	{
		if (useDragEvents)
		{
			if (m_DragState != DragState.CanStartDrag)
			{
				return;
			}
		}
		else
		{
			if (m_DragState == DragState.Dragging)
			{
				UpdateDrag(evt.position);
				return;
			}
			if (m_DragState != DragState.CanStartDrag)
			{
				return;
			}
		}
		if (!(Mathf.Abs(m_Start.x - evt.position.x) > 5f) && !(Mathf.Abs(m_Start.y - evt.position.y) > 5f))
		{
			return;
		}
		StartDragArgs args = StartDrag(m_Start);
		if (useDragEvents)
		{
			if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.MouseDrag)
			{
				return;
			}
			DragAndDropUtility.dragAndDrop.StartDrag(args);
		}
		else
		{
			m_Target.CapturePointer(evt.pointerId);
			evt.StopPropagation();
			dragAndDropClient = new DefaultDragAndDropClient();
			dragAndDropClient.StartDrag(args);
		}
		m_DragState = DragState.Dragging;
	}
}
