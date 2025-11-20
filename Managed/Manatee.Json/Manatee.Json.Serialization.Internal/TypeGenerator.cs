using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Manatee.Json.Serialization.Internal;

internal static class TypeGenerator
{
	private const string AssemblyName = "Manatee.Json.DynamicTypes";

	private static readonly AssemblyBuilder _assemblyBuilder;

	private static readonly ModuleBuilder _moduleBuilder;

	private static readonly Dictionary<Type, TypeInfo> _cache;

	static TypeGenerator()
	{
		_assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Manatee.Json.DynamicTypes"), AssemblyBuilderAccess.RunAndCollect);
		_moduleBuilder = _assemblyBuilder.DefineDynamicModule("Manatee.Json.DynamicTypes.dll");
		_cache = new Dictionary<Type, TypeInfo>();
	}

	public static object Generate(Type type)
	{
		if (!_cache.TryGetValue(type, out TypeInfo value))
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			if (!typeInfo.IsInterface)
			{
				throw new ArgumentException($"Type generation only works for interface types. Type '{type}' is not an interface.");
			}
			if (!typeInfo.IsPublic)
			{
				Assembly assembly = typeInfo.Assembly;
				if (!assembly.GetCustomAttributes<InternalsVisibleToAttribute>().Any((InternalsVisibleToAttribute a) => a.AssemblyName == "Manatee.Json.DynamicTypes"))
				{
					throw new ArgumentException($"Type generation only works for accessible interface types. Type '{type}' is not accessible. " + "If possible, make the type public or add '[assembly:InternalsVisibleTo(\"Manatee.Json.DynamicTypes\")] to assembly '" + assembly.FullName + "'.");
				}
			}
			TypeBuilder typeBuilder = _CreateTypeBuilder(type);
			_ImplementProperties(type, typeBuilder);
			_ImplementMethods(type, typeBuilder);
			_ImplementEvents(type, typeBuilder);
			value = typeBuilder.CreateTypeInfo();
			_cache.Add(type, value);
		}
		return _ConstructInstance(value.AsType());
	}

	private static TypeBuilder _CreateTypeBuilder(Type type)
	{
		string expectedTypeName = "Concrete" + type.Name;
		int num = _cache.Values.Count((TypeInfo t) => t.Name == expectedTypeName);
		if (num != 0)
		{
			expectedTypeName += num + 1;
		}
		TypeBuilder typeBuilder = _moduleBuilder.DefineType(expectedTypeName, TypeAttributes.Public);
		typeBuilder.AddInterfaceImplementation(type);
		return typeBuilder;
	}

	private static void _ImplementProperties(Type interfaceType, TypeBuilder builder)
	{
		foreach (PropertyInfo item in _GetAllProperties(interfaceType))
		{
			_ImplementSingleProperty(builder, item);
		}
	}

	private static IEnumerable<PropertyInfo> _GetAllProperties(Type type)
	{
		List<PropertyInfo> list = new List<PropertyInfo>(type.GetTypeInfo().DeclaredProperties.Where((PropertyInfo m) => !m.IsSpecialName));
		IEnumerable<Type> implementedInterfaces = type.GetTypeInfo().ImplementedInterfaces;
		list.AddRange(implementedInterfaces.SelectMany(_GetAllProperties));
		return list;
	}

	private static void _ImplementSingleProperty(TypeBuilder builder, PropertyInfo propertyInfo)
	{
		FieldBuilder field = builder.DefineField("_" + propertyInfo.Name, propertyInfo.PropertyType, FieldAttributes.Private);
		Type[] array = (from p in propertyInfo.GetIndexParameters()
			select p.ParameterType).ToArray();
		PropertyBuilder propertyBuilder = builder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, array);
		if (propertyInfo.CanRead)
		{
			MethodBuilder methodBuilder = builder.DefineMethod(propertyInfo.GetMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName, propertyInfo.PropertyType, array);
			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, field);
			iLGenerator.Emit(OpCodes.Ret);
			propertyBuilder.SetGetMethod(methodBuilder);
			builder.DefineMethodOverride(methodBuilder, propertyInfo.GetMethod);
		}
		if (propertyInfo.CanWrite)
		{
			MethodBuilder methodBuilder = builder.DefineMethod(propertyInfo.SetMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName, null, array.Union(new Type[1] { propertyInfo.PropertyType }).ToArray());
			ILGenerator iLGenerator2 = methodBuilder.GetILGenerator();
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Ldarg_1);
			iLGenerator2.Emit(OpCodes.Stfld, field);
			iLGenerator2.Emit(OpCodes.Ret);
			propertyBuilder.SetSetMethod(methodBuilder);
			builder.DefineMethodOverride(methodBuilder, propertyInfo.SetMethod);
		}
	}

	private static void _ImplementMethods(Type interfaceType, TypeBuilder builder)
	{
		foreach (MethodInfo item in _GetAllMethods(interfaceType))
		{
			_ImplementSingleMethod(builder, item);
		}
	}

	private static IEnumerable<MethodInfo> _GetAllMethods(Type type)
	{
		List<MethodInfo> list = new List<MethodInfo>(type.GetTypeInfo().DeclaredMethods.Where((MethodInfo m) => !m.IsSpecialName));
		IEnumerable<Type> implementedInterfaces = type.GetTypeInfo().ImplementedInterfaces;
		list.AddRange(implementedInterfaces.SelectMany(_GetAllMethods));
		return list;
	}

	private static void _ImplementSingleMethod(TypeBuilder builder, MethodInfo methodInfo)
	{
		Type[] parameterTypes = (from p in methodInfo.GetParameters()
			select p.ParameterType).ToArray();
		MethodBuilder methodBuilder = builder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask, methodInfo.ReturnType, parameterTypes);
		if (methodInfo.ContainsGenericParameters)
		{
			List<Type> source = (from p in methodInfo.GetGenericArguments()
				where p.IsGenericParameter
				select p).ToList();
			string[] names = source.Select((Type p) => p.Name).ToArray();
			GenericTypeParameterBuilder[] array = methodBuilder.DefineGenericParameters(names);
			foreach (GenericTypeParameterBuilder typeParameter in array)
			{
				Type[] genericParameterConstraints = source.Single((Type p) => p.Name == typeParameter.Name).GetTypeInfo().GetGenericParameterConstraints();
				foreach (Type type in genericParameterConstraints)
				{
					if (type.GetTypeInfo().IsInterface)
					{
						typeParameter.SetInterfaceConstraints(type);
					}
					else
					{
						typeParameter.SetBaseTypeConstraint(type);
					}
				}
			}
		}
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		LocalBuilder localBuilder = iLGenerator.DeclareLocal(methodInfo.ReturnType);
		iLGenerator.Emit(OpCodes.Ldloca_S, localBuilder);
		iLGenerator.Emit(OpCodes.Initobj, localBuilder.LocalType);
		iLGenerator.Emit(OpCodes.Ldloc_0);
		iLGenerator.Emit(OpCodes.Stloc_0);
		iLGenerator.Emit(OpCodes.Ldloc_0);
		iLGenerator.Emit(OpCodes.Ret);
	}

	private static void _ImplementEvents(Type interfaceType, TypeBuilder builder)
	{
		foreach (EventInfo item in _GetAllEvents(interfaceType))
		{
			_ImplementSingleEvent(builder, item);
		}
	}

	private static IEnumerable<EventInfo> _GetAllEvents(Type type)
	{
		List<EventInfo> list = new List<EventInfo>(type.GetTypeInfo().DeclaredEvents);
		IEnumerable<Type> implementedInterfaces = type.GetTypeInfo().ImplementedInterfaces;
		list.AddRange(implementedInterfaces.SelectMany(_GetAllEvents));
		return list;
	}

	private static void _ImplementSingleEvent(TypeBuilder builder, EventInfo eventInfo)
	{
		Type eventHandlerType = eventInfo.EventHandlerType;
		FieldBuilder field = builder.DefineField("_" + eventInfo.Name, eventHandlerType, FieldAttributes.Private);
		EventBuilder eventBuilder = builder.DefineEvent(eventInfo.Name, EventAttributes.None, eventHandlerType);
		MethodBuilder methodBuilder = builder.DefineMethod(eventInfo.AddMethod.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.SpecialName, null, new Type[1] { eventHandlerType });
		MethodInfo runtimeMethod = typeof(Delegate).GetRuntimeMethod("Combine", new Type[2]
		{
			typeof(Delegate),
			typeof(Delegate)
		});
		MethodInfo runtimeMethod2 = typeof(Delegate).GetRuntimeMethod("Remove", new Type[2]
		{
			typeof(Delegate),
			typeof(Delegate)
		});
		MethodInfo meth = typeof(Interlocked).GetTypeInfo().DeclaredMethods.Single((MethodInfo m) => m.Name == "CompareExchange" && m.IsGenericMethod).MakeGenericMethod(eventHandlerType);
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		iLGenerator.DeclareLocal(eventHandlerType);
		iLGenerator.DeclareLocal(eventHandlerType);
		iLGenerator.DeclareLocal(eventHandlerType);
		iLGenerator.DeclareLocal(typeof(bool));
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, field);
		iLGenerator.Emit(OpCodes.Stloc_0);
		Label label = iLGenerator.DefineLabel();
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Ldloc_0);
		iLGenerator.Emit(OpCodes.Stloc_1);
		iLGenerator.Emit(OpCodes.Ldloc_1);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Call, runtimeMethod);
		iLGenerator.Emit(OpCodes.Castclass, eventHandlerType);
		iLGenerator.Emit(OpCodes.Stloc_2);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldflda, field);
		iLGenerator.Emit(OpCodes.Ldloc_2);
		iLGenerator.Emit(OpCodes.Ldloc_1);
		iLGenerator.Emit(OpCodes.Call, meth);
		iLGenerator.Emit(OpCodes.Stloc_0);
		iLGenerator.Emit(OpCodes.Ldloc_0);
		iLGenerator.Emit(OpCodes.Ldloc_1);
		iLGenerator.Emit(OpCodes.Ceq);
		iLGenerator.Emit(OpCodes.Ldc_I4_0);
		iLGenerator.Emit(OpCodes.Ceq);
		iLGenerator.Emit(OpCodes.Stloc_3);
		iLGenerator.Emit(OpCodes.Ldloc_3);
		iLGenerator.Emit(OpCodes.Brtrue_S, label);
		iLGenerator.Emit(OpCodes.Ret);
		eventBuilder.SetAddOnMethod(methodBuilder);
		builder.DefineMethodOverride(methodBuilder, eventInfo.AddMethod);
		methodBuilder = builder.DefineMethod(eventInfo.RemoveMethod.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.SpecialName, null, new Type[1] { eventHandlerType });
		ILGenerator iLGenerator2 = methodBuilder.GetILGenerator();
		iLGenerator2.DeclareLocal(eventHandlerType);
		iLGenerator2.DeclareLocal(eventHandlerType);
		iLGenerator2.DeclareLocal(eventHandlerType);
		iLGenerator2.DeclareLocal(typeof(bool));
		iLGenerator2.Emit(OpCodes.Ldarg_0);
		iLGenerator2.Emit(OpCodes.Ldfld, field);
		iLGenerator2.Emit(OpCodes.Stloc_0);
		label = iLGenerator2.DefineLabel();
		iLGenerator2.MarkLabel(label);
		iLGenerator2.Emit(OpCodes.Ldloc_0);
		iLGenerator2.Emit(OpCodes.Stloc_1);
		iLGenerator2.Emit(OpCodes.Ldloc_1);
		iLGenerator2.Emit(OpCodes.Ldarg_1);
		iLGenerator2.Emit(OpCodes.Call, runtimeMethod2);
		iLGenerator2.Emit(OpCodes.Castclass, eventHandlerType);
		iLGenerator2.Emit(OpCodes.Stloc_2);
		iLGenerator2.Emit(OpCodes.Ldarg_0);
		iLGenerator2.Emit(OpCodes.Ldflda, field);
		iLGenerator2.Emit(OpCodes.Ldloc_2);
		iLGenerator2.Emit(OpCodes.Ldloc_1);
		iLGenerator2.Emit(OpCodes.Call, meth);
		iLGenerator2.Emit(OpCodes.Stloc_0);
		iLGenerator2.Emit(OpCodes.Ldloc_0);
		iLGenerator2.Emit(OpCodes.Ldloc_1);
		iLGenerator2.Emit(OpCodes.Ceq);
		iLGenerator2.Emit(OpCodes.Ldc_I4_0);
		iLGenerator2.Emit(OpCodes.Ceq);
		iLGenerator2.Emit(OpCodes.Stloc_3);
		iLGenerator2.Emit(OpCodes.Ldloc_3);
		iLGenerator2.Emit(OpCodes.Brtrue_S, label);
		iLGenerator2.Emit(OpCodes.Ret);
		eventBuilder.SetRemoveOnMethod(methodBuilder);
		builder.DefineMethodOverride(methodBuilder, eventInfo.RemoveMethod);
	}

	private static object _ConstructInstance(Type type)
	{
		return Activator.CreateInstance(type, null);
	}
}
