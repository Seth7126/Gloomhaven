using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FullCardEventPusher : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Flags]
	public enum EventType
	{
		None = 0,
		Highlight = 1,
		Top = 2,
		Bottom = 4,
		Default = 8
	}

	[SerializeField]
	private EventType _eventType;

	private FullAbilityCard _target;

	public EventType Type
	{
		set
		{
			_eventType |= value;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if ((object)_target == null)
		{
			_target = GetComponentInParent<FullAbilityCard>();
		}
		if (!(_target != null))
		{
			return;
		}
		if (_eventType.HasFlag(EventType.Highlight))
		{
			_target.Highlight(active: true);
		}
		bool flag = _eventType.HasFlag(EventType.Top);
		if (flag || _eventType.HasFlag(EventType.Bottom))
		{
			if (!_eventType.HasFlag(EventType.Default))
			{
				_target.OnPointerEnter(flag);
			}
			else
			{
				_target.OnDefaultPointerEnter(flag);
			}
			(flag ? _target.topActionButton : _target.bottomActionButton).ToggleHover(active: true, _eventType.HasFlag(EventType.Default));
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if ((object)_target == null)
		{
			_target = GetComponentInParent<FullAbilityCard>();
		}
		if (!(_target != null))
		{
			return;
		}
		if (_eventType.HasFlag(EventType.Highlight))
		{
			_target.Highlight(active: false);
		}
		bool flag = _eventType.HasFlag(EventType.Top);
		if (flag || _eventType.HasFlag(EventType.Bottom))
		{
			if (!_eventType.HasFlag(EventType.Default))
			{
				_target.OnPointerExit(flag);
			}
			else
			{
				_target.OnDefaultPointerExit(flag);
			}
			(flag ? _target.topActionButton : _target.bottomActionButton).ToggleHover(active: false, _eventType.HasFlag(EventType.Default));
		}
	}
}
