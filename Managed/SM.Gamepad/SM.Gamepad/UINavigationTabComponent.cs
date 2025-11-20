using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SM.Gamepad;

public class UINavigationTabComponent : MonoBehaviour
{
	[SerializeField]
	private bool _isCycled;

	[SerializeField]
	private bool _autoInitialise;

	[SerializeField]
	private Transform _target;

	[SerializeField]
	public string Name;

	private List<UINavigationTabElement> _elements;

	private UINavigationTabElement _current;

	public bool ContainsElements
	{
		get
		{
			if (_elements != null)
			{
				return _elements.Count > 0;
			}
			return false;
		}
	}

	public int ElementCount => _elements.Count;

	public int? CurrentIndex
	{
		get
		{
			if (!(_current != null))
			{
				return null;
			}
			return _elements.IndexOf(_current);
		}
	}

	private void Awake()
	{
		if (_autoInitialise)
		{
			Initialize();
		}
	}

	public void Initialize()
	{
		if (_target == null)
		{
			_target = base.transform;
		}
		_elements = new List<UINavigationTabElement>();
		for (int i = 0; i < _target.childCount; i++)
		{
			Transform child = _target.GetChild(i);
			if (child.gameObject.activeSelf)
			{
				UINavigationTabElement component = child.GetComponent<UINavigationTabElement>();
				if (!(component == null) && component.IsInteractable)
				{
					_elements.Add(component);
				}
			}
		}
	}

	public void CurrentOrFirst()
	{
		if (_current == null)
		{
			First();
			return;
		}
		DeselectCurrent();
		SelectCurrent();
	}

	public void SetCurrentElement(int index)
	{
		_current = _elements[index];
	}

	public void Deinitialize()
	{
		_elements.Clear();
	}

	private void SelectFirstElement()
	{
		_current = _elements.First();
		SelectCurrent();
	}

	private void DeselectAndSelectNew(UINavigationTabElement target)
	{
		DeselectCurrent();
		_current = target;
		SelectCurrent();
	}

	public void First()
	{
		if (ContainsElements)
		{
			DeselectCurrent();
			SelectFirstElement();
		}
	}

	public void DeselectCurrent()
	{
		if (_current != null)
		{
			_current.Element.Deselect();
		}
	}

	public void SelectCurrent()
	{
		if (_current != null)
		{
			_current.Element.Select();
		}
	}

	public void RemoveCurrent(bool deselectFirst = true)
	{
		if (deselectFirst)
		{
			DeselectCurrent();
		}
		_current = null;
	}

	public void Next()
	{
		if (!ContainsElements)
		{
			return;
		}
		if (CurrentIndex.HasValue)
		{
			UINavigationTabElement next = GetNext(CurrentIndex.Value);
			if (next != null)
			{
				DeselectAndSelectNew(next);
			}
		}
		else
		{
			SelectFirstElement();
		}
	}

	public void Previous()
	{
		if (!ContainsElements)
		{
			return;
		}
		if (CurrentIndex.HasValue)
		{
			UINavigationTabElement previous = GetPrevious(CurrentIndex.Value);
			if (previous != null)
			{
				DeselectAndSelectNew(previous);
			}
		}
		else
		{
			SelectFirstElement();
		}
	}

	private UINavigationTabElement GetNext(int currentIndex)
	{
		if (_current == _elements.Last())
		{
			if (_isCycled)
			{
				return _elements.First();
			}
			return null;
		}
		return _elements[currentIndex + 1];
	}

	private UINavigationTabElement GetPrevious(int currentIndex)
	{
		if (_current == _elements.First())
		{
			if (_isCycled)
			{
				return _elements.Last();
			}
			return null;
		}
		return _elements[currentIndex - 1];
	}

	public void Select(UINavigationTabElement tabElement)
	{
		UINavigationTabElement uINavigationTabElement = _elements.Find((UINavigationTabElement x) => x == tabElement);
		if (uINavigationTabElement != null)
		{
			DeselectAndSelectNew(uINavigationTabElement);
		}
	}

	public void Select(int index)
	{
		if (ContainsElements && index >= 0 && index < _elements.Count)
		{
			DeselectAndSelectNew(_elements[index]);
		}
	}
}
