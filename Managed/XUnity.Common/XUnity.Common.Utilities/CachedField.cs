using System;
using System.Reflection;

namespace XUnity.Common.Utilities;

public class CachedField
{
	private Func<object, object> _get;

	private Action<object, object> _set;

	public Type FieldType { get; }

	internal CachedField(FieldInfo fieldInfo)
	{
		_get = CustomFastReflectionHelper.CreateFastFieldGetter<object, object>(fieldInfo);
		_set = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>(fieldInfo);
		FieldType = fieldInfo.FieldType;
	}

	public void Set(object instance, object value)
	{
		if (_set != null)
		{
			_set(instance, value);
		}
	}

	public object Get(object instance)
	{
		if (_get == null)
		{
			return null;
		}
		return _get(instance);
	}
}
