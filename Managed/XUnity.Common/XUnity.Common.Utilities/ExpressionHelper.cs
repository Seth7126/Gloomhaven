using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace XUnity.Common.Utilities;

public static class ExpressionHelper
{
	public static Delegate CreateTypedFastInvoke(MethodBase method)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		return CreateTypedFastInvokeUnchecked(method);
	}

	public static Delegate CreateTypedFastInvokeUnchecked(MethodBase method)
	{
		if (method == null)
		{
			return null;
		}
		if (method.IsGenericMethod)
		{
			throw new ArgumentException("The provided method must not be generic.", "method");
		}
		if (method is MethodInfo methodInfo)
		{
			Expression[] arguments;
			if (method.IsStatic)
			{
				ParameterExpression[] array = (from p in methodInfo.GetParameters()
					select Expression.Parameter(p.ParameterType, p.Name)).ToArray();
				arguments = array;
				return Expression.Lambda(Expression.Call(null, methodInfo, arguments), array).Compile();
			}
			List<ParameterExpression> list = (from p in methodInfo.GetParameters()
				select Expression.Parameter(p.ParameterType, p.Name)).ToList();
			list.Insert(0, Expression.Parameter(methodInfo.DeclaringType, "instance"));
			ParameterExpression instance = list[0];
			arguments = list.Skip(1).ToArray();
			return Expression.Lambda(Expression.Call(instance, methodInfo, arguments), list.ToArray()).Compile();
		}
		if (method is ConstructorInfo constructorInfo)
		{
			ParameterExpression[] array2 = (from p in constructorInfo.GetParameters()
				select Expression.Parameter(p.ParameterType, p.Name)).ToArray();
			Expression[] arguments = array2;
			return Expression.Lambda(Expression.New(constructorInfo, arguments), array2).Compile();
		}
		throw new ArgumentException("method", "This method only supports MethodInfo and ConstructorInfo.");
	}
}
