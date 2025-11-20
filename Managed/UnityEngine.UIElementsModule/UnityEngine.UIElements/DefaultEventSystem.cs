using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class DefaultEventSystem
{
	public enum UpdateMode
	{
		Always,
		IgnoreIfAppNotFocused
	}

	internal interface IInput
	{
		int touchCount { get; }

		bool mousePresent { get; }

		bool GetButtonDown(string button);

		float GetAxisRaw(string axis);

		Touch GetTouch(int index);
	}

	internal class Input : IInput
	{
		public int touchCount => UnityEngine.Input.touchCount;

		public bool mousePresent => UnityEngine.Input.mousePresent;

		public bool GetButtonDown(string button)
		{
			return UnityEngine.Input.GetButtonDown(button);
		}

		public float GetAxisRaw(string axis)
		{
			return UnityEngine.Input.GetAxis(axis);
		}

		public Touch GetTouch(int index)
		{
			return UnityEngine.Input.GetTouch(index);
		}
	}

	internal static Func<bool> IsEditorRemoteConnected = () => false;

	private IInput m_Input;

	private readonly string m_HorizontalAxis = "Horizontal";

	private readonly string m_VerticalAxis = "Vertical";

	private readonly string m_SubmitButton = "Submit";

	private readonly string m_CancelButton = "Cancel";

	private readonly float m_InputActionsPerSecond = 10f;

	private readonly float m_RepeatDelay = 0.5f;

	private bool m_SendingTouchEvents;

	private Event m_Event = new Event();

	private BaseRuntimePanel m_FocusedPanel;

	private int m_ConsecutiveMoveCount;

	private Vector2 m_LastMoveVector;

	private float m_PrevActionTime;

	private bool isAppFocused => Application.isFocused;

	internal IInput input
	{
		get
		{
			return m_Input ?? (m_Input = new Input());
		}
		set
		{
			m_Input = value;
		}
	}

	public BaseRuntimePanel focusedPanel
	{
		get
		{
			return m_FocusedPanel;
		}
		set
		{
			if (m_FocusedPanel != value)
			{
				m_FocusedPanel?.Blur();
				m_FocusedPanel = value;
				m_FocusedPanel?.Focus();
			}
		}
	}

	private bool ShouldIgnoreEventsOnAppNotFocused()
	{
		OperatingSystemFamily operatingSystemFamily = SystemInfo.operatingSystemFamily;
		OperatingSystemFamily operatingSystemFamily2 = operatingSystemFamily;
		if ((uint)(operatingSystemFamily2 - 1) <= 2u)
		{
			return true;
		}
		return false;
	}

	public void Update(UpdateMode updateMode = UpdateMode.Always)
	{
		if (isAppFocused || !ShouldIgnoreEventsOnAppNotFocused() || updateMode != UpdateMode.IgnoreIfAppNotFocused)
		{
			m_SendingTouchEvents = ProcessTouchEvents();
			SendIMGUIEvents();
			SendInputEvents();
		}
	}

	private void SendIMGUIEvents()
	{
		while (Event.PopEvent(m_Event))
		{
			if (m_Event.type == EventType.Repaint)
			{
				continue;
			}
			if (m_Event.type == EventType.KeyUp || m_Event.type == EventType.KeyDown)
			{
				SendFocusBasedEvent((DefaultEventSystem self) => UIElementsRuntimeUtility.CreateEvent(self.m_Event), this);
			}
			else
			{
				if (m_SendingTouchEvents || !input.mousePresent)
				{
					continue;
				}
				int? targetDisplay;
				Vector2 localScreenPosition = GetLocalScreenPosition(m_Event, out targetDisplay);
				if (m_Event.type == EventType.ScrollWheel)
				{
					SendPositionBasedEvent(localScreenPosition, m_Event.delta, PointerId.mousePointerId, targetDisplay, delegate(Vector3 panelPosition, Vector3 panelDelta, DefaultEventSystem self)
					{
						self.m_Event.mousePosition = panelPosition;
						return UIElementsRuntimeUtility.CreateEvent(self.m_Event);
					}, this);
					continue;
				}
				SendPositionBasedEvent(localScreenPosition, m_Event.delta, PointerId.mousePointerId, targetDisplay, delegate(Vector3 panelPosition, Vector3 panelDelta, DefaultEventSystem self)
				{
					self.m_Event.mousePosition = panelPosition;
					self.m_Event.delta = panelDelta;
					return UIElementsRuntimeUtility.CreateEvent(self.m_Event);
				}, this, m_Event.type == EventType.MouseDown);
			}
		}
	}

	private void SendInputEvents()
	{
		if (ShouldSendMoveFromInput())
		{
			SendFocusBasedEvent((DefaultEventSystem self) => NavigationMoveEvent.GetPooled(self.GetRawMoveVector()), this);
		}
		if (input.GetButtonDown(m_SubmitButton))
		{
			SendFocusBasedEvent((DefaultEventSystem self) => EventBase<NavigationSubmitEvent>.GetPooled(), this);
		}
		if (input.GetButtonDown(m_CancelButton))
		{
			SendFocusBasedEvent((DefaultEventSystem self) => EventBase<NavigationCancelEvent>.GetPooled(), this);
		}
	}

	internal void SendFocusBasedEvent<TArg>(Func<TArg, EventBase> evtFactory, TArg arg)
	{
		if (focusedPanel != null)
		{
			using (EventBase e = evtFactory(arg))
			{
				focusedPanel.visualTree.SendEvent(e);
				UpdateFocusedPanel(focusedPanel);
				return;
			}
		}
		List<Panel> sortedPlayerPanels = UIElementsRuntimeUtility.GetSortedPlayerPanels();
		for (int num = sortedPlayerPanels.Count - 1; num >= 0; num--)
		{
			Panel panel = sortedPlayerPanels[num];
			if (panel is BaseRuntimePanel baseRuntimePanel)
			{
				using EventBase eventBase = evtFactory(arg);
				baseRuntimePanel.visualTree.SendEvent(eventBase);
				if (eventBase.processedByFocusController)
				{
					UpdateFocusedPanel(baseRuntimePanel);
				}
				if (eventBase.isPropagationStopped)
				{
					break;
				}
			}
		}
	}

	internal void SendPositionBasedEvent<TArg>(Vector3 mousePosition, Vector3 delta, int pointerId, Func<Vector3, Vector3, TArg, EventBase> evtFactory, TArg arg, bool deselectIfNoTarget = false)
	{
		SendPositionBasedEvent(mousePosition, delta, pointerId, null, evtFactory, arg, deselectIfNoTarget);
	}

	private void SendPositionBasedEvent<TArg>(Vector3 mousePosition, Vector3 delta, int pointerId, int? targetDisplay, Func<Vector3, Vector3, TArg, EventBase> evtFactory, TArg arg, bool deselectIfNoTarget = false)
	{
		if (focusedPanel != null)
		{
			UpdateFocusedPanel(focusedPanel);
		}
		IPanel panel = PointerDeviceState.GetPlayerPanelWithSoftPointerCapture(pointerId);
		IEventHandler capturingElement = RuntimePanel.s_EventDispatcher.pointerState.GetCapturingElement(pointerId);
		if (capturingElement is VisualElement visualElement)
		{
			panel = visualElement.panel;
		}
		BaseRuntimePanel baseRuntimePanel = null;
		Vector2 panelPosition = Vector2.zero;
		Vector2 panelDelta = Vector2.zero;
		if (panel is BaseRuntimePanel baseRuntimePanel2)
		{
			baseRuntimePanel = baseRuntimePanel2;
			baseRuntimePanel.ScreenToPanel(mousePosition, delta, out panelPosition, out panelDelta);
		}
		else
		{
			List<Panel> sortedPlayerPanels = UIElementsRuntimeUtility.GetSortedPlayerPanels();
			for (int num = sortedPlayerPanels.Count - 1; num >= 0; num--)
			{
				if (sortedPlayerPanels[num] is BaseRuntimePanel baseRuntimePanel3 && (!targetDisplay.HasValue || baseRuntimePanel3.targetDisplay == targetDisplay) && baseRuntimePanel3.ScreenToPanel(mousePosition, delta, out panelPosition, out panelDelta) && baseRuntimePanel3.Pick(panelPosition) != null)
				{
					baseRuntimePanel = baseRuntimePanel3;
					break;
				}
			}
		}
		BaseRuntimePanel baseRuntimePanel4 = PointerDeviceState.GetPanel(pointerId, ContextType.Player) as BaseRuntimePanel;
		if (baseRuntimePanel4 != baseRuntimePanel)
		{
			baseRuntimePanel4?.PointerLeavesPanel(pointerId, baseRuntimePanel4.ScreenToPanel(mousePosition));
			baseRuntimePanel?.PointerEntersPanel(pointerId, panelPosition);
		}
		if (baseRuntimePanel != null)
		{
			using (EventBase eventBase = evtFactory(panelPosition, panelDelta, arg))
			{
				baseRuntimePanel.visualTree.SendEvent(eventBase);
				if (eventBase.processedByFocusController)
				{
					UpdateFocusedPanel(baseRuntimePanel);
				}
				if (eventBase.eventTypeId == EventBase<PointerDownEvent>.TypeId())
				{
					PointerDeviceState.SetPlayerPanelWithSoftPointerCapture(pointerId, baseRuntimePanel);
				}
				else if (eventBase.eventTypeId == EventBase<PointerUpEvent>.TypeId() && ((PointerUpEvent)eventBase).pressedButtons == 0)
				{
					PointerDeviceState.SetPlayerPanelWithSoftPointerCapture(pointerId, null);
				}
				return;
			}
		}
		if (deselectIfNoTarget)
		{
			focusedPanel = null;
		}
	}

	private void UpdateFocusedPanel(BaseRuntimePanel runtimePanel)
	{
		if (runtimePanel.focusController.focusedElement != null)
		{
			focusedPanel = runtimePanel;
		}
		else if (focusedPanel == runtimePanel)
		{
			focusedPanel = null;
		}
	}

	private static EventBase MakeTouchEvent(Touch touch, EventModifiers modifiers)
	{
		return touch.phase switch
		{
			TouchPhase.Began => PointerEventBase<PointerDownEvent>.GetPooled(touch, modifiers), 
			TouchPhase.Moved => PointerEventBase<PointerMoveEvent>.GetPooled(touch, modifiers), 
			TouchPhase.Stationary => PointerEventBase<PointerStationaryEvent>.GetPooled(touch, modifiers), 
			TouchPhase.Ended => PointerEventBase<PointerUpEvent>.GetPooled(touch, modifiers), 
			TouchPhase.Canceled => PointerEventBase<PointerCancelEvent>.GetPooled(touch, modifiers), 
			_ => null, 
		};
	}

	private bool ProcessTouchEvents()
	{
		for (int i = 0; i < input.touchCount; i++)
		{
			Touch touch = input.GetTouch(i);
			if (touch.type != TouchType.Indirect)
			{
				touch.position = UIElementsRuntimeUtility.MultiDisplayBottomLeftToPanelPosition(touch.position, out var targetDisplay);
				touch.rawPosition = UIElementsRuntimeUtility.MultiDisplayBottomLeftToPanelPosition(touch.rawPosition, out var _);
				touch.deltaPosition = UIElementsRuntimeUtility.ScreenBottomLeftToPanelDelta(touch.deltaPosition);
				SendPositionBasedEvent(touch.position, touch.deltaPosition, PointerId.touchPointerIdBase + touch.fingerId, targetDisplay, delegate(Vector3 panelPosition, Vector3 panelDelta, Touch _touch)
				{
					_touch.position = panelPosition;
					_touch.deltaPosition = panelDelta;
					return MakeTouchEvent(_touch, EventModifiers.None);
				}, touch);
			}
		}
		return input.touchCount > 0;
	}

	private Vector2 GetRawMoveVector()
	{
		Vector2 zero = Vector2.zero;
		zero.x = input.GetAxisRaw(m_HorizontalAxis);
		zero.y = input.GetAxisRaw(m_VerticalAxis);
		if (input.GetButtonDown(m_HorizontalAxis))
		{
			if (zero.x < 0f)
			{
				zero.x = -1f;
			}
			if (zero.x > 0f)
			{
				zero.x = 1f;
			}
		}
		if (input.GetButtonDown(m_VerticalAxis))
		{
			if (zero.y < 0f)
			{
				zero.y = -1f;
			}
			if (zero.y > 0f)
			{
				zero.y = 1f;
			}
		}
		return zero;
	}

	private bool ShouldSendMoveFromInput()
	{
		float unscaledTime = Time.unscaledTime;
		Vector2 rawMoveVector = GetRawMoveVector();
		if (Mathf.Approximately(rawMoveVector.x, 0f) && Mathf.Approximately(rawMoveVector.y, 0f))
		{
			m_ConsecutiveMoveCount = 0;
			return false;
		}
		bool flag = input.GetButtonDown(m_HorizontalAxis) || input.GetButtonDown(m_VerticalAxis);
		bool flag2 = Vector2.Dot(rawMoveVector, m_LastMoveVector) > 0f;
		if (!flag)
		{
			flag = ((!flag2 || m_ConsecutiveMoveCount != 1) ? (unscaledTime > m_PrevActionTime + 1f / m_InputActionsPerSecond) : (unscaledTime > m_PrevActionTime + m_RepeatDelay));
		}
		if (!flag)
		{
			return false;
		}
		NavigationMoveEvent.Direction direction = NavigationMoveEvent.DetermineMoveDirection(rawMoveVector.x, rawMoveVector.y);
		if (direction != NavigationMoveEvent.Direction.None)
		{
			if (!flag2)
			{
				m_ConsecutiveMoveCount = 0;
			}
			m_ConsecutiveMoveCount++;
			m_PrevActionTime = unscaledTime;
			m_LastMoveVector = rawMoveVector;
		}
		else
		{
			m_ConsecutiveMoveCount = 0;
		}
		return direction != NavigationMoveEvent.Direction.None;
	}

	private static Vector2 GetLocalScreenPosition(Event evt, out int? targetDisplay)
	{
		targetDisplay = null;
		return evt.mousePosition;
	}
}
