using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ClickTrackerMap : MonoBehaviour
{
	[Serializable]
	public class ClickButtonEvent : UnityEvent
	{
	}

	public List<RectTransform> ignoreAreas;

	public List<UIWindowID> ignoreAreaIds;

	public ClickButtonEvent onClick;

	public MouseClickType clickType;

	public bool useButtonDown = true;

	public string audioItemClick;

	private void Awake()
	{
		foreach (UIWindowID ignoreAreaId in ignoreAreaIds)
		{
			UIWindow window = UIWindow.GetWindow(ignoreAreaId);
			if (window != null)
			{
				ignoreAreas.Add(window.GetComponent<RectTransform>());
			}
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
		if (flag && IsClickingValid(clickType))
		{
			if (onClick != null)
			{
				AudioControllerUtils.PlaySound(audioItemClick);
				onClick.Invoke();
			}
			else
			{
				Debug.LogError("ClickTrackerExtended: Trying to invoke but the event is null.");
			}
		}
	}

	private bool IsClickingValid(MouseClickType clickType)
	{
		if (EventSystem.current.IsPointerOverGameObject())
		{
			GameObject gameObject = UIManager.GameObjectUnderPointer(0 - clickType - 1);
			if (gameObject != null && gameObject.transform is RectTransform && !ignoreAreas.Contains((RectTransform)gameObject.transform))
			{
				return false;
			}
		}
		if (clickType == MouseClickType.LeftClick && Physics.Raycast(CameraController.s_CameraController.m_Camera.ScreenPointToRay(InputManager.CursorPosition), out var hitInfo) && hitInfo.collider.GetComponent<MapLocation>() != null)
		{
			return false;
		}
		return true;
	}
}
