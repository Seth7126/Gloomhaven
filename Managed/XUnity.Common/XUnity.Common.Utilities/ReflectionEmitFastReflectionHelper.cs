using System;
using System.Reflection;
using System.Reflection.Emit;

namespace XUnity.Common.Utilities;

internal static class ReflectionEmitFastReflectionHelper
{
	private static readonly Type[] DynamicMethodDelegateArgs = new Type[2]
	{
		typeof(object),
		typeof(object[])
	};

	public static FastReflectionDelegate CreateFastDelegate(MethodBase method, bool directBoxValueAccess, bool forceNonVirtcall)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("FastReflection<" + method.DeclaringType.FullName + "." + method.Name + ">", typeof(object), DynamicMethodDelegateArgs, method.DeclaringType.Module, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		ParameterInfo[] parameters = method.GetParameters();
		bool flag = true;
		if (!method.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			if (method.DeclaringType.IsValueType)
			{
				iLGenerator.Emit(OpCodes.Unbox_Any, method.DeclaringType);
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
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Ldc_I4, i);
			}
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Ldc_I4, i);
			if (isByRef && !isValueType)
			{
				iLGenerator.Emit(OpCodes.Ldelema, typeof(object));
				continue;
			}
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			if (!isValueType)
			{
				continue;
			}
			if (!isByRef || !directBoxValueAccess)
			{
				iLGenerator.Emit(OpCodes.Unbox_Any, type);
				if (isByRef)
				{
					iLGenerator.Emit(OpCodes.Box, type);
					iLGenerator.Emit(OpCodes.Dup);
					iLGenerator.Emit(OpCodes.Unbox, type);
					if (flag)
					{
						flag = false;
						throw new NotImplementedException("No idea how to implement this...");
					}
					iLGenerator.Emit(OpCodes.Stloc_0);
					iLGenerator.Emit(OpCodes.Stelem_Ref);
					iLGenerator.Emit(OpCodes.Ldloc_0);
				}
			}
			else
			{
				iLGenerator.Emit(OpCodes.Unbox, type);
			}
		}
		if (method.IsConstructor)
		{
			iLGenerator.Emit(OpCodes.Newobj, method as ConstructorInfo);
		}
		else if (method.IsFinal || !method.IsVirtual || forceNonVirtcall)
		{
			iLGenerator.Emit(OpCodes.Call, method as MethodInfo);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Callvirt, method as MethodInfo);
		}
		Type type2 = (method.IsConstructor ? method.DeclaringType : (method as MethodInfo).ReturnType);
		if (type2 != typeof(void))
		{
			if (type2.IsValueType)
			{
				iLGenerator.Emit(OpCodes.Box, type2);
			}
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldnull);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (FastReflectionDelegate)dynamicMethod.CreateDelegate(typeof(FastReflectionDelegate));
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
		DynamicMethod dynamicMethod = new DynamicMethod("FastReflection<" + typeof(T).FullName + ".Get_" + fieldInfo.Name + ">", typeof(F), new Type[1] { typeof(T) }, fieldInfo.DeclaringType.Module, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (!fieldInfo.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
		}
		iLGenerator.Emit(fieldInfo.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldInfo);
		if (fieldInfo.FieldType.IsValueType != typeof(F).IsValueType)
		{
			iLGenerator.Emit(OpCodes.Box, fieldInfo.FieldType);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (Func<T, F>)dynamicMethod.CreateDelegate(typeof(Func<T, F>));
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
		DynamicMethod dynamicMethod = new DynamicMethod("FastReflection<" + typeof(T).FullName + ".Set_" + fieldInfo.Name + ">", null, new Type[2]
		{
			typeof(T),
			typeof(F)
		}, fieldInfo.DeclaringType.Module, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (!fieldInfo.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
		}
		iLGenerator.Emit(OpCodes.Ldarg_1);
		if (fieldInfo.FieldType != typeof(F))
		{
			if (fieldInfo.FieldType.IsValueType != typeof(F).IsValueType)
			{
				if (fieldInfo.FieldType.IsValueType)
				{
					iLGenerator.Emit(OpCodes.Unbox, fieldInfo.FieldType);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Box, fieldInfo.FieldType);
				}
			}
			else
			{
				iLGenerator.Emit(OpCodes.Castclass, fieldInfo.FieldType);
			}
		}
		iLGenerator.Emit(fieldInfo.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldInfo);
		iLGenerator.Emit(OpCodes.Ret);
		return (Action<T, F>)dynamicMethod.CreateDelegate(typeof(Action<T, F>));
	}
}
