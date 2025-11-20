using System.Collections.Generic;
using UnityEngine;

namespace XUnity.ResourceRedirector;

internal struct BackingFieldOrArray
{
	private Object _field;

	private Object[] _array;

	private BackingSource _source;

	public Object Field
	{
		get
		{
			if (_source == BackingSource.None)
			{
				return null;
			}
			if (_source == BackingSource.SingleField)
			{
				return _field;
			}
			if (_array == null || _array.Length == 0)
			{
				return null;
			}
			return _array[0];
		}
		set
		{
			_field = value;
			_array = null;
			_source = BackingSource.SingleField;
		}
	}

	public Object[] Array
	{
		get
		{
			if (_source == BackingSource.Array)
			{
				return _array;
			}
			if (_field == (Object)null)
			{
				Array = (Object[])(object)new Object[0];
			}
			else
			{
				Array = (Object[])(object)new Object[1] { _field };
			}
			return _array;
		}
		set
		{
			_field = null;
			_array = value;
			_source = BackingSource.Array;
		}
	}

	public BackingFieldOrArray(Object field)
	{
		_field = field;
		_array = null;
		_source = BackingSource.SingleField;
	}

	public BackingFieldOrArray(Object[] array)
	{
		_field = null;
		_array = array;
		_source = BackingSource.Array;
	}

	public IEnumerable<Object> IterateObjects()
	{
		if (_array != null)
		{
			Object[] array = _array;
			for (int i = 0; i < array.Length; i++)
			{
				yield return array[i];
			}
		}
		else if (_field != (Object)null)
		{
			yield return _field;
		}
	}
}
