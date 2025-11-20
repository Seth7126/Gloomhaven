using System;
using System.Reflection;
using System.Reflection.Emit;

namespace MonoMod.RuntimeDetour.Platforms;

public sealed class DetourRuntimeMonoPlatform : DetourRuntimeILPlatform
{
	private static readonly object[] _NoArgs = new object[0];

	private static readonly MethodInfo _DynamicMethod_CreateDynMethod = typeof(DynamicMethod).GetMethod("CreateDynMethod", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo _DynamicMethod_mhandle = typeof(DynamicMethod).GetField("mhandle", BindingFlags.Instance | BindingFlags.NonPublic);

	protected override RuntimeMethodHandle GetMethodHandle(MethodBase method)
	{
		if (method is DynamicMethod)
		{
			_DynamicMethod_CreateDynMethod?.Invoke(method, _NoArgs);
			if (_DynamicMethod_mhandle != null)
			{
				return (RuntimeMethodHandle)_DynamicMethod_mhandle.GetValue(method);
			}
		}
		return method.MethodHandle;
	}
}
