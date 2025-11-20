using UnityEngine.EventSystems;

namespace UnityEngine.UIElements;

[AddComponentMenu("UI Toolkit/Panel Event Handler (UI Toolkit)")]
public class PanelEventHandler : UIBehaviour, IPointerMoveHandler, IEventSystemHandler, IPointerUpHandler, IPointerDownHandler, ISubmitHandler, ICancelHandler, IMoveHandler, IScrollHandler, ISelectHandler, IDeselectHandler, IPointerExitHandler, IPointerEnterHandler, IRuntimePanelComponent
{
	private enum PointerEventType
	{
		Default,
		Down,
		Up
	}

	private class PointerEvent : IPointerEvent
	{
		public int pointerId { get; private set; }

		public string pointerType { get; private set; }

		public bool isPrimary { get; private set; }

		public int button { get; private set; }

		public int pressedButtons { get; private set; }

		public Vector3 position { get; private set; }

		public Vector3 localPosition { get; private set; }

		public Vector3 deltaPosition { get; private set; }

		public float deltaTime { get; private set; }

		public int clickCount { get; private set; }

		public float pressure { get; private set; }

		public float tangentialPressure { get; private set; }

		public float altitudeAngle { get; private set; }

		public float azimuthAngle { get; private set; }

		public float twist { get; private set; }

		public Vector2 radius { get; private set; }

		public Vector2 radiusVariance { get; private set; }

		public EventModifiers modifiers { get; private set; }

		public bool shiftKey => (modifiers & EventModifiers.Shift) != 0;

		public bool ctrlKey => (modifiers & EventModifiers.Control) != 0;

		public bool commandKey => (modifiers & EventModifiers.Command) != 0;

		public bool altKey => (modifiers & EventModifiers.Alt) != 0;

		public bool actionKey
		{
			get
			{
				if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.OSXPlayer)
				{
					return ctrlKey;
				}
				return commandKey;
			}
		}

		public void Read(PanelEventHandler self, PointerEventData eventData, PointerEventType eventType)
		{
			pointerId = self.eventSystem.currentInputModule.ConvertUIToolkitPointerId(eventData);
			pointerType = (InRange(pointerId, PointerId.touchPointerIdBase, PointerId.touchPointerCount) ? PointerType.touch : (InRange(pointerId, PointerId.penPointerIdBase, PointerId.penPointerCount) ? PointerType.pen : PointerType.mouse));
			isPrimary = pointerId == PointerId.mousePointerId || pointerId == PointerId.touchPointerIdBase || pointerId == PointerId.penPointerIdBase;
			button = (int)eventData.button;
			clickCount = eventData.clickCount;
			int num = Screen.height;
			Vector3 vector = Display.RelativeMouseAt(eventData.position);
			if (vector != Vector3.zero)
			{
				int num2 = (int)vector.z;
				if (num2 > 0 && num2 < Display.displays.Length)
				{
					num = Display.displays[num2].systemHeight;
				}
			}
			else
			{
				vector = eventData.position;
			}
			Vector2 delta = eventData.delta;
			vector.y = (float)num - vector.y;
			delta.y = 0f - delta.y;
			Vector3 vector2 = (position = vector);
			localPosition = vector2;
			deltaPosition = delta;
			deltaTime = 0f;
			pressure = eventData.pressure;
			tangentialPressure = eventData.tangentialPressure;
			altitudeAngle = eventData.altitudeAngle;
			azimuthAngle = eventData.azimuthAngle;
			twist = eventData.twist;
			radius = eventData.radius;
			radiusVariance = eventData.radiusVariance;
			modifiers = s_Modifiers;
			if (eventType == PointerEventType.Default)
			{
				button = -1;
				clickCount = 0;
			}
			else
			{
				button = ((button >= 0) ? button : 0);
				clickCount = Mathf.Max(1, clickCount);
				switch (eventType)
				{
				case PointerEventType.Down:
					PointerDeviceState.PressButton(pointerId, button);
					break;
				case PointerEventType.Up:
					PointerDeviceState.ReleaseButton(pointerId, button);
					break;
				}
			}
			pressedButtons = PointerDeviceState.GetPressedButtons(pointerId);
			static bool InRange(int i, int start, int count)
			{
				if (i >= start)
				{
					return i < start + count;
				}
				return false;
			}
		}

		public void SetPosition(Vector3 positionOverride, Vector3 deltaOverride)
		{
			Vector3 vector = (position = positionOverride);
			localPosition = vector;
			deltaPosition = deltaOverride;
		}
	}

	private BaseRuntimePanel m_Panel;

	private readonly PointerEvent m_PointerEvent = new PointerEvent();

	private bool m_Selecting;

	private Event m_Event = new Event();

	private static EventModifiers s_Modifiers;

	public IPanel panel
	{
		get
		{
			return m_Panel;
		}
		set
		{
			BaseRuntimePanel baseRuntimePanel = (BaseRuntimePanel)value;
			if (m_Panel != baseRuntimePanel)
			{
				UnregisterCallbacks();
				m_Panel = baseRuntimePanel;
				RegisterCallbacks();
			}
		}
	}

	private GameObject selectableGameObject => m_Panel?.selectableGameObject;

	private EventSystem eventSystem => UIElementsRuntimeUtility.activeEventSystem as EventSystem;

	protected override void OnEnable()
	{
		base.OnEnable();
		RegisterCallbacks();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UnregisterCallbacks();
	}

	private void RegisterCallbacks()
	{
		if (m_Panel != null)
		{
			m_Panel.destroyed += OnPanelDestroyed;
			m_Panel.visualTree.RegisterCallback<FocusEvent>(OnElementFocus, TrickleDown.TrickleDown);
			m_Panel.visualTree.RegisterCallback<BlurEvent>(OnElementBlur, TrickleDown.TrickleDown);
		}
	}

	private void UnregisterCallbacks()
	{
		if (m_Panel != null)
		{
			m_Panel.destroyed -= OnPanelDestroyed;
			m_Panel.visualTree.UnregisterCallback<FocusEvent>(OnElementFocus, TrickleDown.TrickleDown);
			m_Panel.visualTree.UnregisterCallback<BlurEvent>(OnElementBlur, TrickleDown.TrickleDown);
		}
	}

	private void OnPanelDestroyed()
	{
		panel = null;
	}

	private void OnElementFocus(FocusEvent e)
	{
		if (!m_Selecting && eventSystem != null)
		{
			eventSystem.SetSelectedGameObject(selectableGameObject);
		}
	}

	private void OnElementBlur(BlurEvent e)
	{
	}

	public void OnSelect(BaseEventData eventData)
	{
		m_Selecting = true;
		try
		{
			m_Panel?.Focus();
		}
		finally
		{
			m_Selecting = false;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		m_Panel?.Blur();
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData))
		{
			return;
		}
		using PointerMoveEvent e = PointerEventBase<PointerMoveEvent>.GetPooled(m_PointerEvent);
		SendEvent(e, eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData, PointerEventType.Up))
		{
			return;
		}
		using PointerUpEvent pointerUpEvent = PointerEventBase<PointerUpEvent>.GetPooled(m_PointerEvent);
		SendEvent(pointerUpEvent, eventData);
		if (pointerUpEvent.pressedButtons == 0)
		{
			PointerDeviceState.SetPlayerPanelWithSoftPointerCapture(pointerUpEvent.pointerId, null);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData, PointerEventType.Down))
		{
			return;
		}
		if (eventSystem != null)
		{
			eventSystem.SetSelectedGameObject(selectableGameObject);
		}
		using PointerDownEvent pointerDownEvent = PointerEventBase<PointerDownEvent>.GetPooled(m_PointerEvent);
		SendEvent(pointerDownEvent, eventData);
		PointerDeviceState.SetPlayerPanelWithSoftPointerCapture(pointerDownEvent.pointerId, m_Panel);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData))
		{
			return;
		}
		if (eventData.pointerCurrentRaycast.gameObject == base.gameObject && eventData.pointerPressRaycast.gameObject != base.gameObject && m_PointerEvent.pointerId != PointerId.mousePointerId)
		{
			using PointerCancelEvent e = PointerEventBase<PointerCancelEvent>.GetPooled(m_PointerEvent);
			SendEvent(e, eventData);
		}
		m_Panel.PointerLeavesPanel(m_PointerEvent.pointerId, m_PointerEvent.position);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_Panel != null && ReadPointerData(m_PointerEvent, eventData))
		{
			m_Panel.PointerEntersPanel(m_PointerEvent.pointerId, m_PointerEvent.position);
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (m_Panel == null)
		{
			return;
		}
		using NavigationSubmitEvent e = EventBase<NavigationSubmitEvent>.GetPooled();
		SendEvent(e, eventData);
	}

	public void OnCancel(BaseEventData eventData)
	{
		if (m_Panel == null)
		{
			return;
		}
		using NavigationCancelEvent e = EventBase<NavigationCancelEvent>.GetPooled();
		SendEvent(e, eventData);
	}

	public void OnMove(AxisEventData eventData)
	{
		if (m_Panel == null)
		{
			return;
		}
		using NavigationMoveEvent e = NavigationMoveEvent.GetPooled(eventData.moveVector);
		SendEvent(e, eventData);
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData))
		{
			return;
		}
		Vector2 scrollDelta = eventData.scrollDelta;
		scrollDelta.y = 0f - scrollDelta.y;
		scrollDelta /= 20f;
		using WheelEvent e = WheelEvent.GetPooled(scrollDelta, m_PointerEvent);
		SendEvent(e, eventData);
	}

	private void SendEvent(EventBase e, BaseEventData sourceEventData)
	{
		m_Panel.SendEvent(e);
		if (e.isPropagationStopped)
		{
			sourceEventData.Use();
		}
	}

	private void SendEvent(EventBase e, Event sourceEvent)
	{
		m_Panel.SendEvent(e);
		if (e.isPropagationStopped)
		{
			sourceEvent.Use();
		}
	}

	private void Update()
	{
		if (m_Panel != null && eventSystem != null && eventSystem.currentSelectedGameObject == selectableGameObject)
		{
			ProcessImguiEvents(isSelected: true);
		}
	}

	private void LateUpdate()
	{
		ProcessImguiEvents(isSelected: false);
	}

	private void ProcessImguiEvents(bool isSelected)
	{
		bool flag = true;
		while (Event.PopEvent(m_Event))
		{
			if (m_Event.type == EventType.Ignore || m_Event.type == EventType.Repaint || m_Event.type == EventType.Layout)
			{
				continue;
			}
			s_Modifiers = (flag ? m_Event.modifiers : (s_Modifiers | m_Event.modifiers));
			flag = false;
			if (isSelected)
			{
				ProcessKeyboardEvent(m_Event);
				if (m_Event.type != EventType.Used)
				{
					ProcessTabEvent(m_Event);
				}
			}
		}
	}

	private void ProcessKeyboardEvent(Event e)
	{
		if (e.type == EventType.KeyUp)
		{
			if (e.character == '\0')
			{
				SendKeyUpEvent(e, e.keyCode, e.modifiers);
			}
		}
		else if (e.type == EventType.KeyDown)
		{
			if (e.character == '\0')
			{
				SendKeyDownEvent(e, e.keyCode, e.modifiers);
			}
			else
			{
				SendTextEvent(e, e.character, e.modifiers);
			}
		}
	}

	private void ProcessTabEvent(Event e)
	{
		if (e.type == EventType.KeyDown && e.character == '\t')
		{
			SendTabEvent(e, (!e.shift) ? 1 : (-1));
		}
	}

	private void SendTabEvent(Event e, int direction)
	{
		using NavigationTabEvent e2 = NavigationTabEvent.GetPooled(direction);
		SendEvent(e2, e);
	}

	private void SendKeyUpEvent(Event e, KeyCode keyCode, EventModifiers modifiers)
	{
		using KeyUpEvent e2 = KeyboardEventBase<KeyUpEvent>.GetPooled('\0', keyCode, modifiers);
		SendEvent(e2, e);
	}

	private void SendKeyDownEvent(Event e, KeyCode keyCode, EventModifiers modifiers)
	{
		using KeyDownEvent e2 = KeyboardEventBase<KeyDownEvent>.GetPooled('\0', keyCode, modifiers);
		SendEvent(e2, e);
	}

	private void SendTextEvent(Event e, char c, EventModifiers modifiers)
	{
		using KeyDownEvent e2 = KeyboardEventBase<KeyDownEvent>.GetPooled(c, KeyCode.None, modifiers);
		SendEvent(e2, e);
	}

	private bool ReadPointerData(PointerEvent pe, PointerEventData eventData, PointerEventType eventType = PointerEventType.Default)
	{
		if (eventSystem == null || eventSystem.currentInputModule == null)
		{
			return false;
		}
		pe.Read(this, eventData, eventType);
		m_Panel.ScreenToPanel(pe.position, pe.deltaPosition, out var panelPosition, out var panelDelta, allowOutside: true);
		pe.SetPosition(panelPosition, panelDelta);
		return true;
	}
}
