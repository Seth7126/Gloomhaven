using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ClickTrackerExtended : MonoBehaviour
{
	[Serializable]
	private class Reference
	{
		public UIWindow windowControl;

		public RectTransform area;
	}

	[Serializable]
	public class ClickButtonEvent : UnityEvent
	{
	}

	[SerializeField]
	private string id;

	public List<RectTransform> area;

	public List<UIWindowID> areaIds;

	[SerializeField]
	private List<Reference> areaReferences;

	public ClickButtonEvent onClick;

	public MouseClickType clickType;

	public TrackingArea trackingArea;

	public bool useButtonDown = true;

	public string audioItemClick;

	public bool skipOutsideScreen = true;

	[SerializeField]
	private KeyAction keyAction = KeyAction.None;

	[ConditionalField("keyAction", KeyAction.None, false)]
	[SerializeField]
	private EControllerInputAreaType requiredControllerArea;

	[ConditionalField("keyAction", KeyAction.None, false)]
	[SerializeField]
	[Tooltip("If it's enabled, the key action will only be used when a controller is attached. If it's false, it'll listen for the key action in keyboard too")]
	private bool isControllerKeyAction = true;

	private RectTransform screenArea;

	public bool SkipNextClick { get; set; }

	private void Awake()
	{
		if (skipOutsideScreen)
		{
			screenArea = base.transform.root as RectTransform;
		}
		foreach (UIWindowID areaId in areaIds)
		{
			UIWindow window = UIWindow.GetWindow(areaId);
			if (window != null)
			{
				area.Add(window.GetComponent<RectTransform>());
			}
		}
	}

	private void OnDestroy()
	{
		if (onClick != null)
		{
			onClick.RemoveAllListeners();
		}
	}

	private void Update()
	{
		bool flag = false;
		switch (clickType)
		{
		case MouseClickType.LeftClick:
			flag = ((!useButtonDown) ? Singleton<InputManager>.Instance.PlayerControl.MouseClickLeft.WasReleased : Singleton<InputManager>.Instance.PlayerControl.MouseClickLeft.WasPressed);
			break;
		case MouseClickType.RightClick:
			flag = ((!useButtonDown) ? Singleton<InputManager>.Instance.PlayerControl.MouseClickRight.WasReleased : Singleton<InputManager>.Instance.PlayerControl.MouseClickRight.WasPressed);
			break;
		case MouseClickType.MiddleClick:
			flag = ((!useButtonDown) ? Singleton<InputManager>.Instance.PlayerControl.MouseClickMiddle.WasReleased : Singleton<InputManager>.Instance.PlayerControl.MouseClickMiddle.WasPressed);
			break;
		}
		if (flag)
		{
			if ((!(screenArea != null) || RectTransformUtility.RectangleContainsScreenPoint(screenArea, InputManager.CursorPosition, UIManager.Instance.UICamera)) && ((trackingArea == TrackingArea.WithinThisArea && InArea()) || (trackingArea == TrackingArea.OutsideThisArea && !InArea()) || trackingArea == TrackingArea.Anywhere))
			{
				ProcessClick();
			}
		}
		else if (keyAction != KeyAction.None && (!isControllerKeyAction || InputManager.GamePadInUse) && (requiredControllerArea == EControllerInputAreaType.None || ControllerInputAreaManager.IsFocusedArea(requiredControllerArea)) && (useButtonDown ? InputManager.GetWasPressed(keyAction) : InputManager.GetWasReleased(keyAction)))
		{
			ProcessClick();
		}
	}

	private void ProcessClick()
	{
		if (SkipNextClick)
		{
			SkipNextClick = false;
		}
		else if (onClick != null)
		{
			AudioControllerUtils.PlaySound(audioItemClick);
			onClick.Invoke();
		}
		else
		{
			Debug.LogError("ClickTrackerExtended: Trying to invoke but the event is null.");
		}
	}

	public void AddArea(UIWindowID windowId)
	{
		UIWindow window = UIWindow.GetWindow(windowId);
		if (window != null)
		{
			area.Add(window.GetComponent<RectTransform>());
		}
	}

	public void RemoveArea(UIWindowID windowId)
	{
		UIWindow window = UIWindow.GetWindow(windowId);
		if (window != null)
		{
			area.Remove(window.GetComponent<RectTransform>());
		}
	}

	private bool InArea()
	{
		if (ClickedUI())
		{
			return true;
		}
		if (area.Exists((RectTransform it) => IsVisible(it) && RectTransformUtility.RectangleContainsScreenPoint(it, InputManager.CursorPosition, UIManager.Instance.UICamera)))
		{
			return true;
		}
		return areaReferences.Exists((Reference it) => IsVisible(it.windowControl) && RectTransformUtility.RectangleContainsScreenPoint(it.area, InputManager.CursorPosition, UIManager.Instance.UICamera));
	}

	private bool IsVisible(RectTransform rect)
	{
		UIWindow component = rect.GetComponent<UIWindow>();
		if (component == null)
		{
			return rect.gameObject.activeInHierarchy;
		}
		return IsVisible(component);
	}

	private bool ClickedUI()
	{
		if (EventSystem.current.IsPointerOverGameObject())
		{
			GameObject gameObject = UIManager.GameObjectUnderPointer();
			if (gameObject == null)
			{
				return false;
			}
			ClickTrackerElement component = gameObject.GetComponent<ClickTrackerElement>();
			if (component == null)
			{
				return false;
			}
			if (!component.clickTrackerID.IsNullOrEmpty())
			{
				return component.clickTrackerID == id;
			}
			return true;
		}
		return false;
	}

	private bool IsVisible(UIWindow rect)
	{
		if (rect.gameObject.activeInHierarchy)
		{
			return rect.IsOpen;
		}
		return false;
	}
}
