#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace UnityEngine.UIElements;

internal class UIElementsUtility : IUIElementsUtility
{
	private static Stack<IMGUIContainer> s_ContainerStack = new Stack<IMGUIContainer>();

	private static Dictionary<int, Panel> s_UIElementsCache = new Dictionary<int, Panel>();

	private static Event s_EventInstance = new Event();

	internal static Color editorPlayModeTintColor = Color.white;

	internal static float singleLineHeight = 18f;

	private static UIElementsUtility s_Instance = new UIElementsUtility();

	internal static List<Panel> s_PanelsIterationList = new List<Panel>();

	internal static readonly string s_RepaintProfilerMarkerName = "UIElementsUtility.DoDispatch(Repaint Event)";

	internal static readonly string s_EventProfilerMarkerName = "UIElementsUtility.DoDispatch(Non Repaint Event)";

	private static readonly ProfilerMarker s_RepaintProfilerMarker = new ProfilerMarker(s_RepaintProfilerMarkerName);

	private static readonly ProfilerMarker s_EventProfilerMarker = new ProfilerMarker(s_EventProfilerMarkerName);

	private UIElementsUtility()
	{
		UIEventRegistration.RegisterUIElementSystem(this);
	}

	internal static IMGUIContainer GetCurrentIMGUIContainer()
	{
		if (s_ContainerStack.Count > 0)
		{
			return s_ContainerStack.Peek();
		}
		return null;
	}

	bool IUIElementsUtility.MakeCurrentIMGUIContainerDirty()
	{
		if (s_ContainerStack.Count > 0)
		{
			s_ContainerStack.Peek().MarkDirtyLayout();
			return true;
		}
		return false;
	}

	bool IUIElementsUtility.TakeCapture()
	{
		if (s_ContainerStack.Count > 0)
		{
			IMGUIContainer iMGUIContainer = s_ContainerStack.Peek();
			IEventHandler capturingElement = iMGUIContainer.panel.GetCapturingElement(PointerId.mousePointerId);
			if (capturingElement != null && capturingElement != iMGUIContainer)
			{
				Debug.Log("Should not grab hot control with an active capture");
			}
			iMGUIContainer.CaptureMouse();
			return true;
		}
		return false;
	}

	bool IUIElementsUtility.ReleaseCapture()
	{
		return false;
	}

	bool IUIElementsUtility.ProcessEvent(int instanceID, IntPtr nativeEventPtr, ref bool eventHandled)
	{
		if (nativeEventPtr != IntPtr.Zero && s_UIElementsCache.TryGetValue(instanceID, out var value))
		{
			if (value.contextType == ContextType.Editor)
			{
				s_EventInstance.CopyFromPtr(nativeEventPtr);
				eventHandled = DoDispatch(value);
			}
			return true;
		}
		return false;
	}

	bool IUIElementsUtility.CleanupRoots()
	{
		s_EventInstance = null;
		s_UIElementsCache = null;
		s_ContainerStack = null;
		return false;
	}

	bool IUIElementsUtility.EndContainerGUIFromException(Exception exception)
	{
		if (s_ContainerStack.Count > 0)
		{
			GUIUtility.EndContainer();
			s_ContainerStack.Pop();
		}
		return false;
	}

	void IUIElementsUtility.UpdateSchedulers()
	{
		s_PanelsIterationList.Clear();
		GetAllPanels(s_PanelsIterationList, ContextType.Editor);
		foreach (Panel s_PanelsIteration in s_PanelsIterationList)
		{
			s_PanelsIteration.timerEventScheduler.UpdateScheduledEvents();
			s_PanelsIteration.UpdateAnimations();
			s_PanelsIteration.UpdateBindings();
		}
	}

	void IUIElementsUtility.RequestRepaintForPanels(Action<ScriptableObject> repaintCallback)
	{
		Dictionary<int, Panel>.Enumerator panelsIterator = GetPanelsIterator();
		while (panelsIterator.MoveNext())
		{
			Panel value = panelsIterator.Current.Value;
			if (value.contextType == ContextType.Editor && value.isDirty)
			{
				repaintCallback(value.ownerObject);
			}
		}
	}

	public static void RegisterCachedPanel(int instanceID, Panel panel)
	{
		s_UIElementsCache.Add(instanceID, panel);
	}

	public static void RemoveCachedPanel(int instanceID)
	{
		s_UIElementsCache.Remove(instanceID);
	}

	public static bool TryGetPanel(int instanceID, out Panel panel)
	{
		return s_UIElementsCache.TryGetValue(instanceID, out panel);
	}

	internal static void BeginContainerGUI(GUILayoutUtility.LayoutCache cache, Event evt, IMGUIContainer container)
	{
		if (container.useOwnerObjectGUIState)
		{
			GUIUtility.BeginContainerFromOwner(container.elementPanel.ownerObject);
		}
		else
		{
			GUIUtility.BeginContainer(container.guiState);
		}
		s_ContainerStack.Push(container);
		GUIUtility.s_SkinMode = (int)container.contextType;
		GUIUtility.s_OriginalID = container.elementPanel.ownerObject.GetInstanceID();
		if (Event.current == null)
		{
			Event.current = evt;
		}
		else
		{
			Event.current.CopyFrom(evt);
		}
		GUI.enabled = container.enabledInHierarchy;
		GUILayoutUtility.BeginContainer(cache);
		GUIUtility.ResetGlobalState();
	}

	internal static void EndContainerGUI(Event evt, Rect layoutSize)
	{
		if (Event.current.type == EventType.Layout && s_ContainerStack.Count > 0)
		{
			GUILayoutUtility.LayoutFromContainer(layoutSize.width, layoutSize.height);
		}
		GUILayoutUtility.SelectIDList(GUIUtility.s_OriginalID, isWindow: false);
		GUIContent.ClearStaticCache();
		if (s_ContainerStack.Count > 0)
		{
		}
		evt.CopyFrom(Event.current);
		if (s_ContainerStack.Count > 0)
		{
			GUIUtility.EndContainer();
			s_ContainerStack.Pop();
		}
	}

	internal static EventBase CreateEvent(Event systemEvent)
	{
		return CreateEvent(systemEvent, systemEvent.rawType);
	}

	internal static EventBase CreateEvent(Event systemEvent, EventType eventType)
	{
		switch (eventType)
		{
		case EventType.MouseMove:
			return PointerEventBase<PointerMoveEvent>.GetPooled(systemEvent);
		case EventType.MouseDrag:
			return PointerEventBase<PointerMoveEvent>.GetPooled(systemEvent);
		case EventType.MouseDown:
			if (PointerDeviceState.HasAdditionalPressedButtons(PointerId.mousePointerId, systemEvent.button))
			{
				return PointerEventBase<PointerMoveEvent>.GetPooled(systemEvent);
			}
			return PointerEventBase<PointerDownEvent>.GetPooled(systemEvent);
		case EventType.MouseUp:
			if (PointerDeviceState.HasAdditionalPressedButtons(PointerId.mousePointerId, systemEvent.button))
			{
				return PointerEventBase<PointerMoveEvent>.GetPooled(systemEvent);
			}
			return PointerEventBase<PointerUpEvent>.GetPooled(systemEvent);
		case EventType.ContextClick:
			return MouseEventBase<ContextClickEvent>.GetPooled(systemEvent);
		case EventType.MouseEnterWindow:
			return MouseEventBase<MouseEnterWindowEvent>.GetPooled(systemEvent);
		case EventType.MouseLeaveWindow:
			return MouseLeaveWindowEvent.GetPooled(systemEvent);
		case EventType.ScrollWheel:
			return WheelEvent.GetPooled(systemEvent);
		case EventType.KeyDown:
			return KeyboardEventBase<KeyDownEvent>.GetPooled(systemEvent);
		case EventType.KeyUp:
			return KeyboardEventBase<KeyUpEvent>.GetPooled(systemEvent);
		case EventType.ValidateCommand:
			return CommandEventBase<ValidateCommandEvent>.GetPooled(systemEvent);
		case EventType.ExecuteCommand:
			return CommandEventBase<ExecuteCommandEvent>.GetPooled(systemEvent);
		default:
			return IMGUIEvent.GetPooled(systemEvent);
		}
	}

	private static bool DoDispatch(BaseVisualElementPanel panel)
	{
		bool result = false;
		if (s_EventInstance.type == EventType.Repaint)
		{
			using (s_RepaintProfilerMarker.Auto())
			{
				panel.Repaint(s_EventInstance);
			}
			result = panel.IMGUIContainersCount > 0;
		}
		else
		{
			panel.ValidateLayout();
			using EventBase eventBase = CreateEvent(s_EventInstance);
			bool flag = s_EventInstance.type == EventType.Used || s_EventInstance.type == EventType.Layout || s_EventInstance.type == EventType.ExecuteCommand || s_EventInstance.type == EventType.ValidateCommand;
			using (s_EventProfilerMarker.Auto())
			{
				panel.SendEvent(eventBase, (!flag) ? DispatchMode.Default : DispatchMode.Immediate);
			}
			if (eventBase.isPropagationStopped)
			{
				panel.visualTree.IncrementVersion(VersionChangeType.Repaint);
				result = true;
			}
		}
		return result;
	}

	internal static void GetAllPanels(List<Panel> panels, ContextType contextType)
	{
		Dictionary<int, Panel>.Enumerator panelsIterator = GetPanelsIterator();
		while (panelsIterator.MoveNext())
		{
			if (panelsIterator.Current.Value.contextType == contextType)
			{
				panels.Add(panelsIterator.Current.Value);
			}
		}
	}

	internal static Dictionary<int, Panel>.Enumerator GetPanelsIterator()
	{
		return s_UIElementsCache.GetEnumerator();
	}

	internal static Panel FindOrCreateEditorPanel(ScriptableObject ownerObject)
	{
		if (!s_UIElementsCache.TryGetValue(ownerObject.GetInstanceID(), out var value))
		{
			value = Panel.CreateEditorPanel(ownerObject);
			RegisterCachedPanel(ownerObject.GetInstanceID(), value);
		}
		else
		{
			Debug.Assert(ContextType.Editor == value.contextType, "Panel is not an editor panel.");
		}
		return value;
	}

	internal static float PixelsPerUnitScaleForElement(VisualElement ve, Sprite sprite)
	{
		if (ve == null || sprite == null)
		{
			return 1f;
		}
		float pixelsPerUnit = sprite.pixelsPerUnit;
		pixelsPerUnit = Mathf.Max(0.01f, pixelsPerUnit);
		return 100f / pixelsPerUnit;
	}
}
