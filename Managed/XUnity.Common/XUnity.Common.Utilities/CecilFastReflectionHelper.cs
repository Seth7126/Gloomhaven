using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace XUnity.Common.Utilities;

internal static class CecilFastReflectionHelper
{
	private static readonly Type[] DynamicMethodDelegateArgs = new Type[2]
	{
		typeof(object),
		typeof(object[])
	};

	public static FastReflectionDelegate CreateFastDelegate(MethodBase method, bool directBoxValueAccess, bool forceNonVirtcall)
	{
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("FastReflection<" + method.DeclaringType.FullName + "." + method.Name + ">", typeof(object), DynamicMethodDelegateArgs);
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
		else if (method.IsFinal || !method.IsVirtual || forceNonVirtcall)
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

	public static Func<T, F> CreateFastFieldGetter<T, F>(FieldInfo fieldInfo)
	{
		if (fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		if (!typeof(F).IsAssignableFrom(fieldInfo.FieldType))
		{
			throw new ArgumentException("FieldInfo type does not match return type.");
		}
		if (typeof(T) != typeof(object) && (fieldInfo.DeclaringType == null || !fieldInfo.DeclaringType.IsAssignableFrom(typeof(T))))
		{
			throw new MissingFieldException(typeof(T).Name, fieldInfo.Name);
		}
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("FastReflection<" + typeof(T).FullName + ".Get_" + fieldInfo.Name + ">", typeof(F), new Type[1] { typeof(T) });
		ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
		if (!fieldInfo.IsStatic)
		{
			iLProcessor.Emit(OpCodes.Ldarg_0);
			iLProcessor.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
		}
		iLProcessor.Emit(fieldInfo.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldInfo);
		if (fieldInfo.FieldType.IsValueType != typeof(F).IsValueType)
		{
			iLProcessor.Emit(OpCodes.Box, fieldInfo.FieldType);
		}
		iLProcessor.Emit(OpCodes.Ret);
		return (Func<T, F>)dynamicMethodDefinition.Generate().CreateDelegate(typeof(Func<T, F>));
	}

	public static Action<T, F> CreateFastFieldSetter<T, F>(FieldInfo fieldInfo)
	{
		if (fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		if (!typeof(F).IsAssignableFrom(fieldInfo.FieldType))
		{
			throw new ArgumentException("FieldInfo type does not match argument type.");
		}
		if (typeof(T) != typeof(object) && (fieldInfo.DeclaringType == null || !fieldInfo.DeclaringType.IsAssignableFrom(typeof(T))))
		{
			throw new MissingFieldException(typeof(T).Name, fieldInfo.Name);
		}
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("FastReflection<" + typeof(T).FullName + ".Set_" + fieldInfo.Name + ">", null, new Type[2]
		{
			typeof(T),
			typeof(F)
		});
		ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
		if (!fieldInfo.IsStatic)
		{
			iLProcessor.Emit(OpCodes.Ldarg_0);
			iLProcessor.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
		}
		iLProcessor.Emit(OpCodes.Ldarg_1);
		if (fieldInfo.FieldType != typeof(F))
		{
			if (fieldInfo.FieldType.IsValueType != typeof(F).IsValueType)
			{
				if (fieldInfo.FieldType.IsValueType)
				{
					iLProcessor.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
				}
				else
				{
					iLProcessor.Emit(OpCodes.Box, fieldInfo.FieldType);
				}
			}
			else
			{
				iLProcessor.Emit(OpCodes.Castclass, fieldInfo.FieldType);
			}
		}
		iLProcessor.Emit(fieldInfo.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldInfo);
		iLProcessor.Emit(OpCodes.Ret);
		return (Action<T, F>)dynamicMethodDefinition.Generate().CreateDelegate(typeof(Action<T, F>));
	}
}
