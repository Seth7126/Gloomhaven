using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ListSelector<T> : MonoBehaviour
{
	[SerializeField]
	private bool _isCycled;

	[SerializeField]
	private UnityEvent<T> _onSelectedUnityEvent;

	[SerializeField]
	private UnityEvent<T> _onDeselectedUnityEvent;

	[SerializeField]
	private AudioButtonProfile audioProfile;

	private LinkedList<T> _elements = new LinkedList<T>();

	private LinkedListNode<T> _current;

	public bool ContainsElements => _elements.Count > 0;

	public bool HasCurrent => _current != null;

	public T SelectedElement => _current.Value;

	public event Action<T> OnSelectedChanged;

	public void SetElements(List<T> elements)
	{
		ClearElements();
		foreach (T element in elements)
		{
			AddElement(element);
		}
	}

	public virtual void AddElement(T element)
	{
		_elements.AddLast(element);
	}

	public virtual void ClearElements()
	{
		if (_current != null)
		{
			OnDeselected(_current.Value);
		}
		_elements.Clear();
	}

	public virtual void RemoveElement(T element)
	{
		if (_current != null && _current.Value.Equals(element))
		{
			Next();
		}
		_elements.Remove(element);
	}

	public void SelectWithoutNotify(T element)
	{
		Select(element, notify: false);
	}

	private void Select(LinkedListNode<T> element, bool notify = true)
	{
		Select(element.Value, notify);
		if (audioProfile != null)
		{
			PlaySound(audioProfile.mouseEnterAudioItem);
		}
	}

	protected virtual void Select(T element, bool notify = true)
	{
		if (_current != null)
		{
			OnDeselected(_current.Value);
		}
		_current = _elements.Find(element);
		OnSelected(element);
		if (notify)
		{
			this.OnSelectedChanged?.Invoke(element);
		}
	}

	protected virtual void OnSelected(T element)
	{
		_onSelectedUnityEvent?.Invoke(element);
	}

	protected virtual void OnDeselected(T element)
	{
		_onDeselectedUnityEvent?.Invoke(element);
	}

	public void Next()
	{
		if (!ContainsElements)
		{
			return;
		}
		if (_current == _elements.Last)
		{
			if (_isCycled)
			{
				Select(_elements.First);
			}
		}
		else
		{
			Select(_current.Next);
		}
	}

	public void Previous()
	{
		if (!ContainsElements)
		{
			return;
		}
		if (_current == _elements.First)
		{
			if (_isCycled)
			{
				Select(_elements.Last);
			}
		}
		else
		{
			Select(_current.Previous);
		}
	}

	private void PlaySound(string audioItem)
	{
		if (!string.IsNullOrEmpty(audioItem) && AudioController.GetAudioItem(audioItem) != null)
		{
			AudioControllerUtils.PlaySound(audioItem);
		}
	}
}
