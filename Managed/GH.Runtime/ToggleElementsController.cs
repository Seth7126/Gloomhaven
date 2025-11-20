using System.Collections.Generic;
using UnityEngine;

public class ToggleElementsController : MonoBehaviour
{
	private List<IElementTogglable> _currentElements;

	private void Awake()
	{
		_currentElements = new List<IElementTogglable>();
		GetComponentsInChildren(includeInactive: true, _currentElements);
	}

	public void ToggleElements(bool isActive)
	{
		foreach (IElementTogglable currentElement in _currentElements)
		{
			currentElement.Toggle(isActive);
		}
	}
}
