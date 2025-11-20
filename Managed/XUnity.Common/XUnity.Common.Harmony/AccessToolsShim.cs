using System;
using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.Common.Harmony;

public static class AccessToolsShim
{
	private static readonly BindingFlags All;

	private static readonly Func<Type, string, Type[], Type[], MethodInfo> AccessTools_Method;

	private static readonly Func<Type, string, PropertyInfo> AccessTools_Property;

	static AccessToolsShim()
	{
		All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		MethodInfo method = ClrTypes.AccessTools.GetMethod("Method", All, null, new Type[4]
		{
			typeof(Type),
			typeof(string),
			typeof(Type[]),
			typeof(Type[])
		}, null);
		MethodInfo method2 = ClrTypes.AccessTools.GetMethod("Property", All, null, new Type[2]
		{
			typeof(Type),
			typeof(string)
		}, null);
		AccessTools_Method = (Func<Type, string, Type[], Type[], MethodInfo>)ExpressionHelper.CreateTypedFastInvoke(method);
		AccessTools_Property = (Func<Type, string, PropertyInfo>)ExpressionHelper.CreateTypedFastInvoke(method2);
	}

	public static MethodInfo Method(Type type, string name, params Type[] parameters)
	{
		return AccessTools_Method(type, name, parameters, null);
	}

	public static PropertyInfo Property(Type type, string name)
	{
		return AccessTools_Property(type, name);
	}
}
