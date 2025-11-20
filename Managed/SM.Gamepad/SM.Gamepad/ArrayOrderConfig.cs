using System.Collections.Generic;
using UnityEngine;

namespace SM.Gamepad;

public class ArrayOrderConfig<T> : ScriptableObject
{
	[SerializeField]
	private bool _missingElementInTheEnd = true;

	[SerializeField]
	private T[] _elements;

	public IComparer<T> GetComparer()
	{
		return new ArrayComparer<T>(_elements, _missingElementInTheEnd);
	}
}
