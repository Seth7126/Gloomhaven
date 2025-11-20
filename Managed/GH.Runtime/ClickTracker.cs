#define ENABLE_LOGS
using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[AddComponentMenu("Event/ClickTracker")]
public sealed class ClickTracker : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Serializable]
	public class ClickButtonEvent : UnityEvent
	{
	}

	public ClickButtonEvent onClick;

	public MouseClickType clickType;

	public TrackingArea trackingArea;

	public bool useButtonDown = true;

	public string audioItemClick;

	private bool isHovering;

	public bool SkipNextClick { get; set; }

	[UsedImplicitly]
	private void OnDestroy()
	{
		onClick.RemoveAllListeners();
	}

	private void OnEnable()
	{
		isHovering = false;
	}

	private void Update()
	{
		invokeClick();
	}

	public void invokeClick()
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
		if (flag && ((trackingArea == TrackingArea.WithinThisArea && isHovering) || (trackingArea == TrackingArea.OutsideThisArea && !isHovering) || trackingArea == TrackingArea.Anywhere))
		{
			if (SkipNextClick)
			{
				SkipNextClick = false;
			}
			else if (onClick != null)
			{
				Debug.Log("INVOKING");
				AudioControllerUtils.PlaySound(audioItemClick);
				onClick.Invoke();
			}
			else
			{
				Debug.LogError("ClickTracker: Trying to invoke but the event is null.");
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isHovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHovering = false;
	}

	private void LogHoverDebugText(PointerEventData eventData, bool enteredArea)
	{
		string debugText = (enteredArea ? "Entered. " : "Exited. ") + "Hovering: ";
		eventData.hovered?.ForEach(delegate(GameObject x)
		{
			debugText = debugText + x.gameObject.name + ". ";
		});
		Debug.Log(debugText);
	}
}
