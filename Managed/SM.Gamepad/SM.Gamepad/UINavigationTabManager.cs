using System;
using System.Collections.Generic;
using UnityEngine;

namespace SM.Gamepad;

public class UINavigationTabManager
{
	private List<UINavigationTabComponent> _elements;

	public void Initialize()
	{
		_elements = new List<UINavigationTabComponent>();
		UINavigationTabComponent[] array = UnityEngine.Object.FindObjectsOfType<UINavigationTabComponent>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			_elements.Add(array[i]);
		}
	}

	public void Deinitialize()
	{
		foreach (UINavigationTabComponent element in _elements)
		{
			element.Deinitialize();
		}
		_elements.Clear();
	}

	public UINavigationTabComponent GetTab(string name)
	{
		foreach (UINavigationTabComponent element in _elements)
		{
			if (element.Name == name)
			{
				return element;
			}
		}
		throw new Exception("UINavigationTabComponent with name " + name + " is not found");
	}
}
