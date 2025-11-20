using System;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;

namespace MonoMod.Utils;

public sealed class DMDEmitDynamicMethodGenerator : DMDGenerator<DMDEmitDynamicMethodGenerator>
{
	protected override MethodInfo _Generate(DynamicMethodDefinition dmd, object context)
	{
		MethodBase originalMethod = dmd.OriginalMethod;
		MethodDefinition definition = dmd.Definition;
		Type[] array;
		if (originalMethod != null)
		{
			ParameterInfo[] parameters = originalMethod.GetParameters();
			int num = 0;
			if (!originalMethod.IsStatic)
			{
				num++;
				array = new Type[parameters.Length + 1];
				array[0] = originalMethod.GetThisParamType();
			}
			else
			{
				array = new Type[parameters.Length];
			}
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i + num] = parameters[i].ParameterType;
			}
		}
		else
		{
			int num2 = 0;
			if (definition.HasThis)
			{
				num2++;
				array = new Type[definition.Parameters.Count + 1];
				Type type = definition.DeclaringType.ResolveReflection();
				if (type.IsValueType)
				{
					type = type.MakeByRefType();
				}
				array[0] = type;
			}
			else
			{
				array = new Type[definition.Parameters.Count];
			}
			for (int j = 0; j < definition.Parameters.Count; j++)
			{
				array[j + num2] = definition.Parameters[j].ParameterType.ResolveReflection();
			}
		}
		DynamicMethod dynamicMethod = new DynamicMethod("DMD<" + (originalMethod?.GetID(null, null, withType: true, proxyMethod: false, simple: true) ?? definition.GetID(null, null, withType: true, simple: true)) + ">", (originalMethod as MethodInfo)?.ReturnType ?? definition.ReturnType?.ResolveReflection() ?? typeof(void), array, originalMethod?.DeclaringType ?? typeof(DynamicMethodDefinition), skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		_DMDEmit.Generate(dmd, dynamicMethod, iLGenerator);
		return dynamicMethod;
	}
}
