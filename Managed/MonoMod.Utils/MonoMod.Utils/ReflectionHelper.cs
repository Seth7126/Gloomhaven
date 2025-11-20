using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Mono.Cecil;

namespace MonoMod.Utils;

public static class ReflectionHelper
{
	private static readonly Dictionary<string, Assembly> AssemblyCache = new Dictionary<string, Assembly>();

	private static readonly Dictionary<string, MemberInfo> ResolveReflectionCache = new Dictionary<string, MemberInfo>();

	private const BindingFlags _BindingFlagsAll = (BindingFlags)(-1);

	private static MemberInfo _Cache(MemberReference key, MemberInfo value)
	{
		if (key != null && value != null)
		{
			lock (ResolveReflectionCache)
			{
				ResolveReflectionCache[key.FullName] = value;
			}
		}
		return value;
	}

	public static Assembly Load(ModuleDefinition module)
	{
		using MemoryStream memoryStream = new MemoryStream();
		module.Write(memoryStream);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return Load(memoryStream);
	}

	public static Assembly Load(Stream stream)
	{
		Assembly asm;
		if (stream is MemoryStream memoryStream)
		{
			asm = Assembly.Load(memoryStream.GetBuffer());
		}
		else
		{
			using MemoryStream memoryStream2 = new MemoryStream();
			byte[] array = new byte[4096];
			int count;
			while (0 < (count = stream.Read(array, 0, array.Length)))
			{
				memoryStream2.Write(array, 0, count);
			}
			memoryStream2.Seek(0L, SeekOrigin.Begin);
			asm = Assembly.Load(memoryStream2.GetBuffer());
		}
		AppDomain.CurrentDomain.AssemblyResolve += (object s, ResolveEventArgs e) => (!(e.Name == asm.FullName)) ? null : asm;
		return asm;
	}

	public static Type GetType(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		Type type = Type.GetType(name);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType(name);
			if (type != null)
			{
				return type;
			}
		}
		return null;
	}

	public static Type ResolveReflection(this TypeReference mref)
	{
		return _ResolveReflection(mref, null) as Type;
	}

	public static MethodBase ResolveReflection(this MethodReference mref)
	{
		return _ResolveReflection(mref, null) as MethodBase;
	}

	public static FieldInfo ResolveReflection(this FieldReference mref)
	{
		return _ResolveReflection(mref, null) as FieldInfo;
	}

	public static PropertyInfo ResolveReflection(this PropertyReference mref)
	{
		return _ResolveReflection(mref, null) as PropertyInfo;
	}

	public static EventInfo ResolveReflection(this EventReference mref)
	{
		return _ResolveReflection(mref, null) as EventInfo;
	}

	public static MemberInfo ResolveReflection(this MemberReference mref)
	{
		return _ResolveReflection(mref, null);
	}

	private static MemberInfo _ResolveReflection(MemberReference mref, Module[] modules)
	{
		if (mref == null)
		{
			return null;
		}
		lock (ResolveReflectionCache)
		{
			if (ResolveReflectionCache.TryGetValue(mref.FullName, out var value) && value != null)
			{
				return value;
			}
		}
		if (mref is GenericParameter)
		{
			throw new NotSupportedException("ResolveReflection on GenericParameter currently not supported");
		}
		if (mref is MethodReference method && mref.DeclaringType is ArrayType)
		{
			Type type = _ResolveReflection(mref.DeclaringType, modules) as Type;
			string methodID = method.GetID(null, null, withType: false);
			MethodBase methodBase = type.GetMethods((BindingFlags)(-1)).Cast<MethodBase>().Concat(type.GetConstructors((BindingFlags)(-1)))
				.FirstOrDefault((MethodBase m) => m.GetID(null, null, withType: false) == methodID);
			if (methodBase != null)
			{
				return _Cache(mref, methodBase);
			}
		}
		TypeReference typeReference = mref.DeclaringType ?? (mref as TypeReference) ?? throw new ArgumentException("MemberReference hasn't got a DeclaringType / isn't a TypeReference in itself");
		if (modules == null)
		{
			IMetadataScope scope = typeReference.Scope;
			string asmName;
			string text;
			if (!(scope is AssemblyNameReference assemblyNameReference))
			{
				if (!(scope is ModuleDefinition moduleDefinition))
				{
					if (!(scope is ModuleReference))
					{
						throw new NotSupportedException("Unsupported scope type " + typeReference.Scope.GetType().FullName);
					}
					asmName = typeReference.Module.Assembly.FullName;
					text = typeReference.Module.Name;
				}
				else
				{
					asmName = moduleDefinition.Assembly.FullName;
					text = moduleDefinition.Name;
				}
			}
			else
			{
				asmName = assemblyNameReference.FullName;
				text = null;
			}
			Assembly value2 = null;
			lock (AssemblyCache)
			{
				if (!AssemblyCache.TryGetValue(asmName, out value2))
				{
					value2 = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(delegate(Assembly other)
					{
						AssemblyName name = other.GetName();
						return name.Name == asmName || name.FullName == asmName;
					});
					if (value2 == null)
					{
						value2 = Assembly.Load(new AssemblyName(asmName));
					}
					AssemblyCache[asmName] = value2;
				}
			}
			modules = (string.IsNullOrEmpty(text) ? value2.GetModules() : new Module[1] { value2.GetModule(text) });
		}
		if (mref is TypeReference typeReference2)
		{
			if (typeReference2.FullName == "<Module>")
			{
				throw new ArgumentException("Type <Module> cannot be resolved to a runtime reflection type");
			}
			Type type;
			if (mref is TypeSpecification typeSpecification)
			{
				type = _ResolveReflection(typeSpecification.ElementType, null) as Type;
				if (type == null)
				{
					return null;
				}
				if (typeSpecification.IsByReference)
				{
					return ResolveReflectionCache[mref.FullName] = type.MakeByRefType();
				}
				if (typeSpecification.IsPointer)
				{
					return ResolveReflectionCache[mref.FullName] = type.MakePointerType();
				}
				if (typeSpecification.IsArray)
				{
					return ResolveReflectionCache[mref.FullName] = ((typeSpecification as ArrayType).IsVector ? type.MakeArrayType() : type.MakeArrayType((typeSpecification as ArrayType).Dimensions.Count));
				}
				if (typeSpecification.IsGenericInstance)
				{
					return ResolveReflectionCache[mref.FullName] = type.MakeGenericType((typeSpecification as GenericInstanceType).GenericArguments.Select((TypeReference arg) => _ResolveReflection(arg, null) as Type).ToArray());
				}
			}
			else
			{
				type = modules.Select((Module module) => module.GetType(mref.FullName.Replace("/", "+"), throwOnError: false, ignoreCase: false)).FirstOrDefault((Type m) => m != null);
				if (type == null)
				{
					type = modules.Select((Module module) => module.GetTypes().FirstOrDefault((Type m) => mref.Is(m))).FirstOrDefault((Type m) => m != null);
				}
			}
			return _Cache(mref, type);
		}
		bool flag = mref.DeclaringType.FullName == "<Module>";
		MemberInfo memberInfo5;
		if (mref is GenericInstanceMethod genericInstanceMethod)
		{
			memberInfo5 = _ResolveReflection(genericInstanceMethod.ElementMethod, modules);
			memberInfo5 = (memberInfo5 as MethodInfo)?.MakeGenericMethod(genericInstanceMethod.GenericArguments.Select((TypeReference arg) => _ResolveReflection(arg, null) as Type).ToArray());
		}
		else if (flag)
		{
			if (mref is MethodReference)
			{
				memberInfo5 = modules.Select((Module module) => module.GetMethods((BindingFlags)(-1)).FirstOrDefault((MethodInfo m) => mref.Is(m))).FirstOrDefault((MethodInfo m) => m != null);
			}
			else
			{
				if (!(mref is FieldReference))
				{
					throw new NotSupportedException("Unsupported <Module> member type " + mref.GetType().FullName);
				}
				memberInfo5 = modules.Select((Module module) => module.GetFields((BindingFlags)(-1)).FirstOrDefault((FieldInfo m) => mref.Is(m))).FirstOrDefault((FieldInfo m) => m != null);
			}
		}
		else
		{
			Type type2 = _ResolveReflection(mref.DeclaringType, modules) as Type;
			memberInfo5 = ((mref is MethodReference) ? type2.GetMethods((BindingFlags)(-1)).Cast<MethodBase>().Concat(type2.GetConstructors((BindingFlags)(-1)))
				.FirstOrDefault((MethodBase m) => mref.Is(m)) : ((!(mref is FieldReference)) ? type2.GetMembers((BindingFlags)(-1)).FirstOrDefault((MemberInfo m) => mref.Is(m)) : type2.GetFields((BindingFlags)(-1)).FirstOrDefault((FieldInfo m) => mref.Is(m))));
		}
		return _Cache(mref, memberInfo5);
	}

	public static SignatureHelper ResolveReflection(this CallSite csite, Module context)
	{
		return csite.ResolveReflectionSignature(context);
	}

	public static SignatureHelper ResolveReflectionSignature(this IMethodSignature csite, Module context)
	{
		SignatureHelper signatureHelper = csite.CallingConvention switch
		{
			MethodCallingConvention.C => SignatureHelper.GetMethodSigHelper(context, CallingConvention.Cdecl, csite.ReturnType.ResolveReflection()), 
			MethodCallingConvention.StdCall => SignatureHelper.GetMethodSigHelper(context, CallingConvention.StdCall, csite.ReturnType.ResolveReflection()), 
			MethodCallingConvention.ThisCall => SignatureHelper.GetMethodSigHelper(context, CallingConvention.ThisCall, csite.ReturnType.ResolveReflection()), 
			MethodCallingConvention.FastCall => SignatureHelper.GetMethodSigHelper(context, CallingConvention.FastCall, csite.ReturnType.ResolveReflection()), 
			MethodCallingConvention.VarArg => SignatureHelper.GetMethodSigHelper(context, CallingConventions.VarArgs, csite.ReturnType.ResolveReflection()), 
			_ => (!csite.ExplicitThis) ? SignatureHelper.GetMethodSigHelper(context, CallingConventions.Standard, csite.ReturnType.ResolveReflection()) : SignatureHelper.GetMethodSigHelper(context, CallingConventions.ExplicitThis, csite.ReturnType.ResolveReflection()), 
		};
		if (context != null)
		{
			List<Type> list = new List<Type>();
			List<Type> list2 = new List<Type>();
			foreach (ParameterDefinition parameter in csite.Parameters)
			{
				if (parameter.ParameterType.IsSentinel)
				{
					signatureHelper.AddSentinel();
				}
				if (parameter.ParameterType.IsPinned)
				{
					signatureHelper.AddArgument(parameter.ParameterType.ResolveReflection(), pinned: true);
					continue;
				}
				list2.Clear();
				list.Clear();
				for (TypeReference typeReference = parameter.ParameterType; typeReference is TypeSpecification typeSpecification; typeReference = typeSpecification.ElementType)
				{
					if (!(typeReference is RequiredModifierType requiredModifierType))
					{
						if (typeReference is OptionalModifierType optionalModifierType)
						{
							list2.Add(optionalModifierType.ModifierType.ResolveReflection());
						}
					}
					else
					{
						list.Add(requiredModifierType.ModifierType.ResolveReflection());
					}
				}
				signatureHelper.AddArgument(parameter.ParameterType.ResolveReflection(), list.ToArray(), list2.ToArray());
			}
		}
		else
		{
			foreach (ParameterDefinition parameter2 in csite.Parameters)
			{
				signatureHelper.AddArgument(parameter2.ParameterType.ResolveReflection());
			}
		}
		return signatureHelper;
	}
}
