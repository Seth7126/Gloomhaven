using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace MonoMod.RuntimeDetour.Platforms;

public sealed class DetourRuntimeNETPlatform : DetourRuntimeILPlatform
{
	private static readonly object[] _NoArgs = new object[0];

	private static readonly FieldInfo _DynamicMethod_m_method = typeof(DynamicMethod).GetField("m_method", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo _DynamicMethod_GetMethodDescriptor = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo _RuntimeMethodHandle_m_value = typeof(RuntimeMethodHandle).GetField("m_value", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo _RuntimeHelpers__CompileMethod = typeof(RuntimeHelpers).GetMethod("_CompileMethod", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly bool _RuntimeHelpers__CompileMethod_TakesIntPtr = _RuntimeHelpers__CompileMethod != null && _RuntimeHelpers__CompileMethod.GetParameters()[0].ParameterType.FullName == "System.IntPtr";

	private static readonly bool _RuntimeHelpers__CompileMethod_TakesIRuntimeMethodInfo = _RuntimeHelpers__CompileMethod != null && _RuntimeHelpers__CompileMethod.GetParameters()[0].ParameterType.FullName == "System.IRuntimeMethodInfo";

	protected override RuntimeMethodHandle GetMethodHandle(MethodBase method)
	{
		if (method is DynamicMethod dynamicMethod)
		{
			if (_RuntimeHelpers__CompileMethod_TakesIntPtr)
			{
				_RuntimeHelpers__CompileMethod.Invoke(null, new object[1] { ((RuntimeMethodHandle)_DynamicMethod_GetMethodDescriptor.Invoke(dynamicMethod, _NoArgs)).Value });
			}
			else if (_RuntimeHelpers__CompileMethod_TakesIRuntimeMethodInfo)
			{
				_RuntimeHelpers__CompileMethod.Invoke(null, new object[1] { _RuntimeMethodHandle_m_value.GetValue((RuntimeMethodHandle)_DynamicMethod_GetMethodDescriptor.Invoke(dynamicMethod, _NoArgs)) });
			}
			else
			{
				try
				{
					dynamicMethod.CreateDelegate(typeof(MulticastDelegate));
				}
				catch
				{
				}
			}
			if (_DynamicMethod_m_method != null)
			{
				return (RuntimeMethodHandle)_DynamicMethod_m_method.GetValue(method);
			}
			if (_DynamicMethod_GetMethodDescriptor != null)
			{
				return (RuntimeMethodHandle)_DynamicMethod_GetMethodDescriptor.Invoke(method, _NoArgs);
			}
		}
		return method.MethodHandle;
	}
}
