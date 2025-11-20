using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;

namespace MonoMod.Utils;

public sealed class MMReflectionImporter : IReflectionImporter
{
	private class _Provider : IReflectionImporterProvider
	{
		public bool? UseDefault;

		public IReflectionImporter GetReflectionImporter(ModuleDefinition module)
		{
			MMReflectionImporter mMReflectionImporter = new MMReflectionImporter(module);
			if (UseDefault.HasValue)
			{
				mMReflectionImporter.UseDefault = UseDefault.Value;
			}
			return mMReflectionImporter;
		}
	}

	public static readonly IReflectionImporterProvider Provider = new _Provider();

	public static readonly IReflectionImporterProvider ProviderNoDefault = new _Provider
	{
		UseDefault = false
	};

	private readonly ModuleDefinition Module;

	private readonly DefaultReflectionImporter Default;

	private readonly Dictionary<AssemblyName, AssemblyNameReference> CachedAsms = new Dictionary<AssemblyName, AssemblyNameReference>();

	private readonly Dictionary<Module, TypeReference> CachedModuleTypes = new Dictionary<Module, TypeReference>();

	private readonly Dictionary<Type, TypeReference> CachedTypes = new Dictionary<Type, TypeReference>();

	private readonly Dictionary<FieldInfo, FieldReference> CachedFields = new Dictionary<FieldInfo, FieldReference>();

	private readonly Dictionary<MethodBase, MethodReference> CachedMethods = new Dictionary<MethodBase, MethodReference>();

	public bool UseDefault;

	private readonly Dictionary<Type, TypeReference> ElementTypes;

	public MMReflectionImporter(ModuleDefinition module)
	{
		Module = module;
		Default = new DefaultReflectionImporter(module);
		ElementTypes = new Dictionary<Type, TypeReference>
		{
			{
				typeof(void),
				module.TypeSystem.Void
			},
			{
				typeof(bool),
				module.TypeSystem.Boolean
			},
			{
				typeof(char),
				module.TypeSystem.Char
			},
			{
				typeof(sbyte),
				module.TypeSystem.SByte
			},
			{
				typeof(byte),
				module.TypeSystem.Byte
			},
			{
				typeof(short),
				module.TypeSystem.Int16
			},
			{
				typeof(ushort),
				module.TypeSystem.UInt16
			},
			{
				typeof(int),
				module.TypeSystem.Int32
			},
			{
				typeof(uint),
				module.TypeSystem.UInt32
			},
			{
				typeof(long),
				module.TypeSystem.Int64
			},
			{
				typeof(ulong),
				module.TypeSystem.UInt64
			},
			{
				typeof(float),
				module.TypeSystem.Single
			},
			{
				typeof(double),
				module.TypeSystem.Double
			},
			{
				typeof(string),
				module.TypeSystem.String
			},
			{
				typeof(TypedReference),
				module.TypeSystem.TypedReference
			},
			{
				typeof(IntPtr),
				module.TypeSystem.IntPtr
			},
			{
				typeof(UIntPtr),
				module.TypeSystem.UIntPtr
			},
			{
				typeof(object),
				module.TypeSystem.Object
			}
		};
	}

	public AssemblyNameReference ImportReference(AssemblyName asm)
	{
		if (CachedAsms.TryGetValue(asm, out var value))
		{
			return value;
		}
		return CachedAsms[asm] = Default.ImportReference(asm);
	}

	public TypeReference ImportModuleType(Module module, IGenericParameterProvider context)
	{
		if (CachedModuleTypes.TryGetValue(module, out var value))
		{
			return value;
		}
		return CachedModuleTypes[module] = new TypeReference(string.Empty, "<Module>", Module, ImportReference(module.Assembly.GetName()));
	}

	public TypeReference ImportReference(Type type, IGenericParameterProvider context)
	{
		if (CachedTypes.TryGetValue(type, out var value))
		{
			return value;
		}
		if (UseDefault)
		{
			return CachedTypes[type] = Default.ImportReference(type, context);
		}
		if (type.HasElementType)
		{
			if (type.IsByRef)
			{
				return CachedTypes[type] = new ByReferenceType(ImportReference(type.GetElementType(), context));
			}
			if (type.IsPointer)
			{
				return CachedTypes[type] = new PointerType(ImportReference(type.GetElementType(), context));
			}
			if (type.IsArray)
			{
				ArrayType arrayType = new ArrayType(ImportReference(type.GetElementType(), context), type.GetArrayRank());
				if (type != type.GetElementType().MakeArrayType())
				{
					for (int i = 0; i < arrayType.Rank; i++)
					{
						arrayType.Dimensions[i] = new ArrayDimension(0, null);
					}
				}
				return CachedTypes[type] = arrayType;
			}
		}
		if (type.IsGenericType && !type.IsGenericTypeDefinition)
		{
			GenericInstanceType genericInstanceType = new GenericInstanceType(ImportReference(type.GetGenericTypeDefinition(), context));
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				genericInstanceType.GenericArguments.Add(ImportReference(type2, context));
			}
			return genericInstanceType;
		}
		if (type.IsGenericParameter)
		{
			return CachedTypes[type] = ImportGenericParameter(type, context);
		}
		if (ElementTypes.TryGetValue(type, out value))
		{
			return CachedTypes[type] = value;
		}
		value = new TypeReference(string.Empty, type.Name, Module, ImportReference(type.Assembly.GetName()), type.IsValueType);
		if (type.IsNested)
		{
			value.DeclaringType = ImportReference(type.DeclaringType, context);
		}
		else if (type.Namespace != null)
		{
			value.Namespace = type.Namespace;
		}
		if (type.IsGenericType)
		{
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type3 in genericArguments)
			{
				value.GenericParameters.Add(new GenericParameter(type3.Name, value));
			}
		}
		return CachedTypes[type] = value;
	}

	private static TypeReference ImportGenericParameter(Type type, IGenericParameterProvider context)
	{
		if (context is MethodReference methodReference)
		{
			if (type.DeclaringMethod != null)
			{
				return methodReference.GenericParameters[type.GenericParameterPosition];
			}
			context = methodReference.DeclaringType;
		}
		Type declaringType = type.DeclaringType;
		if (declaringType == null)
		{
			throw new InvalidOperationException();
		}
		TypeReference typeReference = context as TypeReference;
		if (typeReference != null)
		{
			while (typeReference != null)
			{
				TypeReference elementType = typeReference.GetElementType();
				if (elementType.Is(declaringType))
				{
					return elementType.GenericParameters[type.GenericParameterPosition];
				}
				if (typeReference.Is(declaringType))
				{
					return typeReference.GenericParameters[type.GenericParameterPosition];
				}
				typeReference = typeReference.DeclaringType;
			}
		}
		throw new NotSupportedException();
	}

	public FieldReference ImportReference(FieldInfo field, IGenericParameterProvider context)
	{
		if (CachedFields.TryGetValue(field, out var value))
		{
			return value;
		}
		if (UseDefault)
		{
			return CachedFields[field] = Default.ImportReference(field, context);
		}
		Type declaringType = field.DeclaringType;
		TypeReference typeReference = ((declaringType != null) ? ImportReference(declaringType, context) : ImportModuleType(field.Module, context));
		FieldInfo key = field;
		if (declaringType != null && declaringType.IsGenericType)
		{
			field = field.Module.ResolveField(field.MetadataToken);
		}
		return CachedFields[key] = new FieldReference(field.Name, ImportReference(field.FieldType, typeReference), typeReference);
	}

	public MethodReference ImportReference(MethodBase method, IGenericParameterProvider context)
	{
		if (CachedMethods.TryGetValue(method, out var value))
		{
			return value;
		}
		if (method is DynamicMethod dm)
		{
			return new DynamicMethodReference(Module, dm);
		}
		if (UseDefault)
		{
			return CachedMethods[method] = Default.ImportReference(method, context);
		}
		if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
		{
			GenericInstanceMethod genericInstanceMethod = new GenericInstanceMethod(ImportReference((method as MethodInfo).GetGenericMethodDefinition(), context));
			Type[] genericArguments = method.GetGenericArguments();
			foreach (Type type in genericArguments)
			{
				genericInstanceMethod.GenericArguments.Add(ImportReference(type, context));
			}
			return CachedMethods[method] = genericInstanceMethod;
		}
		Type declaringType = method.DeclaringType;
		value = new MethodReference(method.Name, ImportReference(typeof(void), context), (declaringType != null) ? ImportReference(declaringType, context) : ImportModuleType(method.Module, context));
		value.HasThis = (method.CallingConvention & CallingConventions.HasThis) != 0;
		value.ExplicitThis = (method.CallingConvention & CallingConventions.ExplicitThis) != 0;
		if ((method.CallingConvention & CallingConventions.VarArgs) != 0)
		{
			value.CallingConvention = MethodCallingConvention.VarArg;
		}
		MethodBase key = method;
		if (declaringType != null && declaringType.IsGenericType)
		{
			method = method.Module.ResolveMethod(method.MetadataToken);
		}
		if (method.IsGenericMethodDefinition)
		{
			Type[] genericArguments = method.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				value.GenericParameters.Add(new GenericParameter(type2.Name, value));
			}
		}
		value.ReturnType = ImportReference((method as MethodInfo)?.ReturnType ?? typeof(void), value);
		ParameterInfo[] parameters = method.GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			value.Parameters.Add(new ParameterDefinition(parameterInfo.Name, (Mono.Cecil.ParameterAttributes)parameterInfo.Attributes, ImportReference(parameterInfo.ParameterType, value)));
		}
		return CachedMethods[key] = value;
	}
}
