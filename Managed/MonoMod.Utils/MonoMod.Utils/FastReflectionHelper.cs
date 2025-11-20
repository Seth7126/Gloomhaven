using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MonoMod.Utils;

public static class FastReflectionHelper
{
	private static readonly Type[] _DynamicMethodDelegateArgs = new Type[2]
	{
		typeof(object),
		typeof(object[])
	};

	private static readonly Dictionary<MethodInfo, FastReflectionDelegate> _MethodCache = new Dictionary<MethodInfo, FastReflectionDelegate>();

	private static FastReflectionDelegate _CreateFastDelegate(MethodBase method, bool directBoxValueAccess = true)
	{
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("FastReflection<" + method.GetID(null, null, withType: true, proxyMethod: false, simple: true) + ">", typeof(object), _DynamicMethodDelegateArgs);
		ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
		ParameterInfo[] parameters = method.GetParameters();
		bool flag = true;
		if (!method.IsStatic)
		{
			iLProcessor.Emit(OpCodes.Ldarg_0);
			if (method.DeclaringType.IsValueType)
			{
				iLProcessor.Emit(OpCodes.Unbox_Any, method.DeclaringType);
			}
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			Type type = parameters[i].ParameterType;
			bool isByRef = type.IsByRef;
			if (isByRef)
			{
				type = type.GetElementType();
			}
			bool isValueType = type.IsValueType;
			if (isByRef && isValueType && !directBoxValueAccess)
			{
				iLProcessor.Emit(OpCodes.Ldarg_1);
				iLProcessor.Emit(OpCodes.Ldc_I4, i);
			}
			iLProcessor.Emit(OpCodes.Ldarg_1);
			iLProcessor.Emit(OpCodes.Ldc_I4, i);
			if (isByRef && !isValueType)
			{
				iLProcessor.Emit(OpCodes.Ldelema, typeof(object));
				continue;
			}
			iLProcessor.Emit(OpCodes.Ldelem_Ref);
			if (!isValueType)
			{
				continue;
			}
			if (!isByRef || !directBoxValueAccess)
			{
				iLProcessor.Emit(OpCodes.Unbox_Any, type);
				if (isByRef)
				{
					iLProcessor.Emit(OpCodes.Box, type);
					iLProcessor.Emit(OpCodes.Dup);
					iLProcessor.Emit(OpCodes.Unbox, type);
					if (flag)
					{
						flag = false;
						dynamicMethodDefinition.Definition.Body.Variables.Add(new VariableDefinition(new PinnedType(new PointerType(dynamicMethodDefinition.Definition.Module.TypeSystem.Void))));
					}
					iLProcessor.Emit(OpCodes.Stloc_0);
					iLProcessor.Emit(OpCodes.Stelem_Ref);
					iLProcessor.Emit(OpCodes.Ldloc_0);
				}
			}
			else
			{
				iLProcessor.Emit(OpCodes.Unbox, type);
			}
		}
		if (method.IsConstructor)
		{
			iLProcessor.Emit(OpCodes.Newobj, method as ConstructorInfo);
		}
		else if (method.IsFinal || !method.IsVirtual)
		{
			iLProcessor.Emit(OpCodes.Call, method as MethodInfo);
		}
		else
		{
			iLProcessor.Emit(OpCodes.Callvirt, method as MethodInfo);
		}
		Type type2 = (method.IsConstructor ? method.DeclaringType : (method as MethodInfo).ReturnType);
		if (type2 != typeof(void))
		{
			if (type2.IsValueType)
			{
				iLProcessor.Emit(OpCodes.Box, type2);
			}
		}
		else
		{
			iLProcessor.Emit(OpCodes.Ldnull);
		}
		iLProcessor.Emit(OpCodes.Ret);
		return (FastReflectionDelegate)dynamicMethodDefinition.Generate().CreateDelegate(typeof(FastReflectionDelegate));
	}

	public static FastReflectionDelegate CreateFastDelegate(this MethodInfo method, bool directBoxValueAccess = true)
	{
		return method.GetFastDelegate(directBoxValueAccess);
	}

	public static FastReflectionDelegate GetFastDelegate(this MethodInfo method, bool directBoxValueAccess = true)
	{
		if (_MethodCache.TryGetValue(method, out var value))
		{
			return value;
		}
		value = _CreateFastDelegate(method, directBoxValueAccess);
		_MethodCache.Add(method, value);
		return value;
	}
}
