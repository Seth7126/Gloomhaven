using System;
using System.Reflection;

namespace XUnity.Common.Utilities;

public class CachedProperty
{
	private static readonly object[] Args0 = new object[0];

	private static readonly object[] Args1 = new object[1];

	private FastReflectionDelegate _set;

	private FastReflectionDelegate _get;

	public Type PropertyType { get; }

	internal CachedProperty(PropertyInfo propertyInfo)
	{
		if (propertyInfo.CanRead)
		{
			_get = propertyInfo.GetGetMethod(nonPublic: true).CreateFastDelegate();
		}
		if (propertyInfo.CanWrite)
		{
			_set = propertyInfo.GetSetMethod(nonPublic: true).CreateFastDelegate();
		}
		PropertyType = propertyInfo.PropertyType;
	}

	public void Set(object instance, object[] arguments)
	{
		if (_set != null)
		{
			_set(instance, arguments);
		}
	}

	public void Set(object instance, object arg1)
	{
		if (_set == null)
		{
			return;
		}
		try
		{
			Args1[0] = arg1;
			_set(instance, Args1);
		}
		finally
		{
			Args1[0] = null;
		}
	}

	public object Get(object instance, object[] arguments)
	{
		if (_get == null)
		{
			return null;
		}
		return _get(instance, arguments);
	}

	public object Get(object instance)
	{
		if (_get == null)
		{
			return null;
		}
		return _get(instance, Args0);
	}
}
