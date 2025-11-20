using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[AddComponentMenu("Event/MouseOverUIElement")]
public sealed class MouseOverUIElement : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[Serializable]
	public class MouseEnterEvent : UnityEvent
	{
	}

	[Serializable]
	public class MouseExitEvent : UnityEvent
	{
	}

	public MouseEnterEvent onMouseEnter;

	public MouseExitEvent onMouseExit;

	public MouseExitEvent onSelected;

	public MouseExitEvent onDeselected;

	public string audioItemMouseEnter;

	public string audioItemMouseExit;

	public void OnPointerEnter(PointerEventData eventData)
	{
		AudioControllerUtils.PlaySound(audioItemMouseEnter);
		onMouseEnter?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		AudioControllerUtils.PlaySound(audioItemMouseExit);
		onMouseExit?.Invoke();
	}

	public void OnSelect(BaseEventData eventData)
	{
		AudioControllerUtils.PlaySound(audioItemMouseEnter);
		onMouseEnter?.Invoke();
		onSelected?.Invoke();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		AudioControllerUtils.PlaySound(audioItemMouseExit);
		onMouseExit?.Invoke();
		onDeselected?.Invoke();
	}
}
