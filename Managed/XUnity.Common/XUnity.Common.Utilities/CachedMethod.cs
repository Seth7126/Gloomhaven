using System.Reflection;

namespace XUnity.Common.Utilities;

public class CachedMethod
{
	private static readonly object[] Args0 = new object[0];

	private static readonly object[] Args1 = new object[1];

	private static readonly object[] Args2 = new object[2];

	private FastReflectionDelegate _invoke;

	internal CachedMethod(MethodInfo method)
	{
		_invoke = method.CreateFastDelegate();
	}

	public object Invoke(object instance, object[] arguments)
	{
		return _invoke(instance, arguments);
	}

	public object Invoke(object instance)
	{
		return _invoke(instance, Args0);
	}

	public object Invoke(object instance, object arg1)
	{
		try
		{
			Args1[0] = arg1;
			return _invoke(instance, Args1);
		}
		finally
		{
			Args1[0] = null;
		}
	}

	public object Invoke(object instance, object arg1, object arg2)
	{
		try
		{
			Args2[0] = arg1;
			Args2[1] = arg2;
			return _invoke(instance, Args2);
		}
		finally
		{
			Args2[0] = null;
			Args2[1] = null;
		}
	}
}
