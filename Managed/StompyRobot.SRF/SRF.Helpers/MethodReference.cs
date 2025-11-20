using System;
using System.Reflection;

namespace SRF.Helpers;

public sealed class MethodReference
{
	private readonly Func<object[], object> _method;

	public MethodReference(object target, MethodInfo method)
	{
		SRDebugUtil.AssertNotNull(target);
		_method = (object[] o) => method.Invoke(target, o);
	}

	public MethodReference(Func<object[], object> method)
	{
		_method = method;
	}

	public object Invoke(object[] parameters)
	{
		return _method(parameters);
	}

	public static implicit operator MethodReference(Action action)
	{
		return new MethodReference(delegate
		{
			action();
			return (object)null;
		});
	}
}
